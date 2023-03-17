// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Rock.Attribute;
using Rock.Data;
using Rock.Logging;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to makes sure that persisted dataviews are updated based on their schedule interval.
    /// </summary>
    [DisplayName( "Update Persisted DataViews" )]
    [Description( "Job to makes sure that persisted dataviews are updated based on their schedule interval." )]

    [IntegerField( "SQL Command Timeout", "Maximum amount of time (in seconds) to wait for each SQL command to complete. Leave blank to use the default for this job (300 seconds). ", false, 5 * 60, "General", 1, TIMEOUT_KEY )]
    public class UpdatePersistedDataviews : RockJob
    {
        private const string TIMEOUT_KEY = "SqlCommandTimeout";

        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public UpdatePersistedDataviews()
        {
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            int sqlCommandTimeout = GetAttributeValue( TIMEOUT_KEY ).AsIntegerOrNull() ?? 300;
            StringBuilder results = new StringBuilder();
            int updatedDataViewCount = 0;
            var errors = new List<string>();
            List<Exception> exceptions = new List<Exception>();

            var log = new RockProcessLogger
            {
                LogDomain = RockLogDomains.Jobs,
                DefaultTopic = "UpdatePersistedDataViews"
            };

            using ( var rockContextList = new RockContext() )
            {
                log.Write( $"Job started.", logLevel: RockLogLevel.Info );

                var currentDateTime = RockDateTime.Now;

                // get a list of all the data views that need to be refreshed
                var expiredPersistedDataViewIds = new DataViewService( rockContextList ).Queryable()
                    .Where( a => a.PersistedScheduleIntervalMinutes.HasValue )
                        .Where( a =>
                            ( a.PersistedLastRefreshDateTime == null )
                            || ( System.Data.Entity.SqlServer.SqlFunctions.DateAdd( "mi", a.PersistedScheduleIntervalMinutes.Value, a.PersistedLastRefreshDateTime.Value ) < currentDateTime ) )
                        .Select( a => a.Id );

                var expiredPersistedDataViewsIdsList = expiredPersistedDataViewIds.ToList();

                // Collect the DataViews with persisted schedules that do not have a last refresh date or need to be refreshed. 
                var persistedScheduleDataViewIds = new DataViewService( rockContextList ).Queryable()
                    .Where( a => a.PersistedScheduleId.HasValue && ( a.PersistedLastRefreshDateTime == null || a.PersistedLastRefreshDateTime.Value <= currentDateTime ) );

                // For DataViews with schedules that have not been persisted,
                // check to see if they are due to be persisted, and add them to the expired dataview list if they are.
                foreach ( var dataview in persistedScheduleDataViewIds )
                {
                    var nextDates = dataview.PersistedSchedule
                        .GetScheduledStartTimes( dataview.PersistedLastRefreshDateTime ?? dataview.PersistedSchedule.GetFirstStartDateTime().Value, currentDateTime );
                    if ( nextDates.Count > 0 )
                    {
                        if ( nextDates.First() < currentDateTime )
                        {
                            expiredPersistedDataViewsIdsList.Add( dataview.Id );
                        }
                    }
                }

                var totalItemCount = expiredPersistedDataViewsIdsList.Count;
                var currentItemCount = 0;

                log.Write( $"Processing expired Data Views... [DataViewCount={totalItemCount}, Timeout={sqlCommandTimeout}s]" );

                foreach ( var dataViewId in expiredPersistedDataViewsIdsList )
                {
                    currentItemCount++;
                    using ( var persistContext = new RockContext() )
                    {
                        var dataView = new DataViewService( persistContext ).Get( dataViewId );
                        var name = dataView.Name;
                        try
                        {
                            log.DefaultTopic = $"UpdatePersistedDataViews:{name}";
                            log.Write( $"Processing... [{currentItemCount} of {totalItemCount}]" );

                            this.UpdateLastStatusMessage( $"{name} Updating..." );
                            dataView.PersistResult( sqlCommandTimeout );

                            log.Write( "Saving..." );
                            persistContext.SaveChanges();

                            log.Write( $"Data View updated. [ElapsedTime={dataView.PersistedLastRunDurationMilliseconds ?? 0 }ms]" );

                            updatedDataViewCount++;
                        }
                        catch ( Exception ex )
                        {
                            // Capture and log the exception because we're not going to fail this job
                            // unless all the data views fail.
                            var errorMessage = $"An error occurred while trying to update persisted data view '{name}' so it was skipped. Error: {ex.Message}";
                            errors.Add( errorMessage );
                            var ex2 = new Exception( errorMessage, ex );
                            exceptions.Add( ex2 );
                            ExceptionLogService.LogException( ex2, null );

                            log.Write( $"Data View update failed. [Exception={ex2.Message}]" );
                            continue;
                        }
                    }
                }

                log.DefaultTopic = "UpdatePersistedDataViews";
                log.Write( $"Job completed. [Processed={totalItemCount}, Updated={updatedDataViewCount}]", logLevel: RockLogLevel.Info );
            }

            // Format the result message
            results.AppendLine( $"Updated {updatedDataViewCount} {"dataview".PluralizeIf( updatedDataViewCount != 1 )}" );
            this.Result = results.ToString();

            if ( errors.Any() )
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.Append( "Errors: " );
                errors.ForEach( e => { sb.AppendLine(); sb.Append( e ); } );
                string errorMessage = sb.ToString();
                this.Result += errorMessage;

                // We're not going to throw an aggregate exception unless there were no successes.
                // Otherwise the status message does not show any of the success messages in
                // the last status message.
                if ( updatedDataViewCount == 0 )
                {
                    throw new AggregateException( exceptions.ToArray() );
                }
            }
        }

        /// <summary>
        /// Simplifies writing entries to the RockLog for a specific process.
        /// </summary>
        private class RockProcessLogger
        {
            public string LogDomain { get; set; }
            public string DefaultTopic { get; set; }
            public RockLogLevel DefaultLogLevel { get; set; } = RockLogLevel.Debug;

            public void Write( string message, string topic = null, RockLogLevel? logLevel = null )
            {
                var msg = $"({ topic ?? DefaultTopic }) { message }";
                RockLogger.Log.WriteToLog( logLevel ?? DefaultLogLevel, domain: LogDomain, messageTemplate: msg );
            }
        }
    }
}