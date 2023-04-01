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
using System.Data.Entity;
using System.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Blocks.Group.Scheduling;
using Rock.Enums.Controls;
using Rock.Field.Types;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduler;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Group.Scheduling
{
    /// <summary>
    /// Allows group schedules for groups and locations to be managed by a scheduler.
    /// </summary>

    [DisplayName( "Group Scheduler" )]
    [Category( "Group Scheduling" )]
    [Description( "Allows group schedules for groups and locations to be managed by a scheduler." )]
    [IconCssClass( "fa fa-calendar-alt" )]

    #region Block Attributes

    [BooleanField( "Enable Alternate Group Individual Selection",
        Key = AttributeKey.EnableAlternateGroupIndividualSelection,
        Description = "Determines if individuals may be selected from alternate groups.",
        ControlType = BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = false,
        Order = 0,
        IsRequired = false )]

    [BooleanField( "Enable Parent Group Individual Selection",
        Key = AttributeKey.EnableParentGroupIndividualSelection,
        Description = "Determines if individuals may be selected from parent groups.",
        ControlType = BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = false,
        Order = 1,
        IsRequired = false )]

    [BooleanField( "Enable Data View Individual Selection",
        Key = AttributeKey.EnableDataViewIndividualSelection,
        Description = "Determines if individuals may be selected from data views.",
        ControlType = BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = false,
        Order = 2,
        IsRequired = false )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "7ADCE833-A785-4A54-9805-7335809C5367" )]
    [Rock.SystemGuid.BlockTypeGuid( "511D8E2E-4AF3-48D8-88EF-2AB311CD47E0" )]
    public class GroupScheduler : RockObsidianBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string EnableAlternateGroupIndividualSelection = "EnableAlternateGroupIndividualSelection";
            public const string EnableParentGroupIndividualSelection = "EnableParentGroupIndividualSelection";
            public const string EnableDataViewIndividualSelection = "EnableDataViewIndividualSelection";
        }

        #endregion

        #region Fields

        private List<int> _groupIds;
        private List<GroupLocationSchedule> _groupLocationSchedules;

        private DateTime _actualStartDate = RockDateTime.Today;
        private DateTime _actualEndDate = RockDateTime.Today.AddDays( 42 );

        #endregion

        #region Properties

        public override string BlockFileUrl => $"{base.BlockFileUrl}.obs";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new GroupSchedulerInitializationBox();

                rockContext.SqlLogging( true );

                SetBoxInitialState( box, rockContext );

                rockContext.SqlLogging( false );

                return box;
            }
        }

        /// <summary>
        /// Sets the initial state of the box.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialState( GroupSchedulerInitializationBox box, RockContext rockContext )
        {
            var block = new BlockService( rockContext ).Get( this.BlockId );
            block.LoadAttributes( rockContext );

            box.Filters = GetFilters( rockContext );
            box.ScheduleOccurrences = GetScheduleOccurrences( rockContext );
            box.ResourceSettings = GetResourceSettings();
            box.CloneSettings = GetCloneSettings();
            box.SecurityGrantToken = GetSecurityGrantToken();
        }

        /// <summary>
        /// Gets the filters, overriding any defaults with user preferences.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The filters.</returns>
        private GroupSchedulerFiltersBag GetFilters( RockContext rockContext )
        {
            var filters = new GroupSchedulerFiltersBag();

            // TODO (JPH): Hook into user preferences to override defaults, once supported in Obsidian blocks.

            filters.Groups = new List<ListItemBag>
            {
                new ListItemBag { Value = "e82bd99e-7dc1-483c-a4f2-09017088c55e", Text = "Children's" },
                new ListItemBag { Value = "0ba93d66-21b1-4229-979d-f76ceb57666d", Text = "A/V Team" }
            };

            RefineFilters( rockContext, filters );

            return filters;
        }

        /// <summary>
        /// Refines the filters, overriding any selections if necessary, as some filter values are dependent on the values of other filters
        /// and current user authorization.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filters">The filters that should be refined.</param>
        private void RefineFilters( RockContext rockContext, GroupSchedulerFiltersBag filters )
        {
            ValidateDateRange( filters );
            GetAuthorizedGroups( rockContext, filters );
            GetLocationsAndSchedules( rockContext, filters );
        }

        /// <summary>
        /// Validates the date range and attempts to set the first and last "end of week" dates (as well as the friendly date range) on the provided filters object.
        /// </summary>
        /// <param name="filters">The filters whose date range should be validated.</param>
        private void ValidateDateRange( GroupSchedulerFiltersBag filters )
        {
            if ( filters.DateRange == null )
            {
                // Default to the next 6 weeks.
                filters.DateRange = new SlidingDateRangeBag
                {
                    RangeType = SlidingDateRangeType.Next,
                    TimeUnit = TimeUnitType.Week,
                    TimeValue = 6
                };
            }

            var lowerDate = filters.DateRange.LowerDate;
            var upperDate = filters.DateRange.UpperDate;

            // Make sure we have a date range that makes sense.
            if ( lowerDate.HasValue && upperDate.HasValue && lowerDate > upperDate )
            {
                upperDate = lowerDate;
            }

            /*
             * Use the non-Obsidian Sliding Date Range Picker control (for now) to calculate the selected start and end dates,
             * as it has quite a bit of built-in logic.
             */
            var picker = new SlidingDateRangePicker
            {
                SlidingDateRangeMode = ( SlidingDateRangePicker.SlidingDateRangeType ) ( int ) filters.DateRange.RangeType,
                TimeUnit = ( SlidingDateRangePicker.TimeUnitType ) ( int ) ( filters.DateRange.TimeUnit ?? 0 ),
                NumberOfTimeUnits = filters.DateRange.TimeValue ?? 1,
                DateRangeModeStart = lowerDate?.DateTime,
                DateRangeModeEnd = upperDate?.DateTime
            };

            var dateRange = picker.SelectedDateRange;
            var startDate = dateRange.Start;
            var endDate = dateRange.End;

            // Reset the Obsidian picker control to match any changes made above.
            filters.DateRange = new SlidingDateRangeBag
            {
                RangeType = ( SlidingDateRangeType ) ( int ) picker.SlidingDateRangeMode,
                TimeUnit = ( TimeUnitType ) ( int ) picker.TimeUnit,
                TimeValue = picker.NumberOfTimeUnits,
                LowerDate = picker.DateRangeModeStart,
                UpperDate = picker.DateRangeModeEnd
            };

            DateTime? firstEndOfWeekDate = null;
            if ( startDate.HasValue )
            {
                firstEndOfWeekDate = startDate.Value.EndOfWeek( RockDateTime.FirstDayOfWeek );
            }

            DateTime? lastEndOfWeekDate = null;
            if ( endDate.HasValue )
            {
                lastEndOfWeekDate = endDate.Value.EndOfWeek( RockDateTime.FirstDayOfWeek );
            }

            var format = "M/d";
            string friendlyDateRange = null;

            if ( firstEndOfWeekDate.HasValue && lastEndOfWeekDate.HasValue )
            {
                // This doesn't need to be precise; just need to determine if we should try to list all "end of week" dates or just a range.
                var numberOfWeeks = ( lastEndOfWeekDate.Value - firstEndOfWeekDate.Value ).TotalDays / 7;
                if ( numberOfWeeks > 4 )
                {
                    friendlyDateRange = $"{firstEndOfWeekDate.Value.ToString( format )} - {lastEndOfWeekDate.Value.ToString( format )}";
                }
                else
                {
                    var endOfWeekDates = new List<DateTime>();
                    var endOfWeekDate = firstEndOfWeekDate.Value;
                    while ( endOfWeekDate <= lastEndOfWeekDate.Value )
                    {
                        endOfWeekDates.Add( endOfWeekDate );
                        endOfWeekDate = endOfWeekDate.AddDays( 7 );
                    }

                    friendlyDateRange = string.Join( ", ", endOfWeekDates.Select( d => d.ToString( format ) ) );
                }
            }
            else if ( firstEndOfWeekDate.HasValue )
            {
                friendlyDateRange = $"From {firstEndOfWeekDate.Value.ToString( format )}";
            }
            else if ( lastEndOfWeekDate.HasValue )
            {
                friendlyDateRange = $"Through {lastEndOfWeekDate.Value.ToString( format )}";
            }

            filters.FirstEndOfWeekDate = firstEndOfWeekDate;
            filters.LastEndOfWeekDate = lastEndOfWeekDate;
            filters.FriendlyDateRange = friendlyDateRange;
        }

        /// <summary>
        /// Gets the authorized groups from those selected within the filters, ensuring the current person has EDIT or SCHEDULE permission.
        /// <para>
        /// The groups will be updated on the filters object to include only those that are authorized.
        /// </para>
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filters">The filters whose groups should be loaded and validated.</param>
        private void GetAuthorizedGroups( RockContext rockContext, GroupSchedulerFiltersBag filters )
        {
            if ( filters.Groups?.Any() != true )
            {
                return;
            }

            var groupGuids = filters.Groups
                .Select( g => g.Value.AsGuidOrNull() )
                .Where( g => g.HasValue )
                .Select( g => g.Value )
                .ToList();

            if ( !groupGuids.Any() )
            {
                filters.Groups = null;
                return;
            }

            // Get the selected groups and preload ParentGroup, as it's needed for a proper Authorization check.
            var groups = new GroupService( rockContext )
                .GetByGuids( groupGuids )
                .Include( g => g.ParentGroup )
                .AsNoTracking()
                .Where( g =>
                    g.IsActive
                    && !g.IsArchived
                    && g.GroupType.IsSchedulingEnabled
                    && !g.DisableScheduling
                )
                .ToList();

            // Ensure the current user has the correct permission(s) to schedule the selected groups and update the filters if necessary.
            groups = groups
                .Where( g =>
                    g.IsAuthorized( Authorization.EDIT, this.RequestContext.CurrentPerson )
                    || g.IsAuthorized( Authorization.SCHEDULE, this.RequestContext.CurrentPerson )
                )
                .ToList();

            filters.Groups = groups
                .Select( g => new ListItemBag
                {
                    Value = g.Guid.ToString(),
                    Text = g.Name
                } )
                .ToList();

            // Set aside the final list of group IDs for later use when selecting locations, schedules and occurrences to be scheduled.
            _groupIds = groups
                .Select( g => g.Id )
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Gets the available and selected locations and schedules, based on the combined, currently-applied filters.
        /// <para>
        /// The locations and schedules will be updated on the filters object.
        /// </para>
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filters">The filters whose locations and schedules should be loaded.</param>
        private void GetLocationsAndSchedules( RockContext rockContext, GroupSchedulerFiltersBag filters )
        {
            if ( _groupIds?.Any() != true )
            {
                _groupLocationSchedules = null;
                filters.Locations = null;
                filters.Schedules = null;
                return;
            }

            /*
             * Get any already-existing attendance occurrence records tied to the [group, location, schedule, occurrence date] occurrences
             * we're retrieving. We'll need these IDs to facilitate scheduling within the Obsidian JavaScript block. Note that we'll create
             * any missing attendance occurrence records below, before sending the final, filtered collection of occurrences back to the client.
             */
            var attendanceOccurrencesQry = new AttendanceOccurrenceService( rockContext )
                .Queryable()
                .AsNoTracking();

            /*
             * Get all locations and schedules tied to the selected group(s) initially, so we can properly load the "available" lists.
             * 
             * Go ahead and materialize the list so we can:
             *  1) Perform additional, in-memory filtering.
             *  2) Set the scheduled start date/time(s) on the returned instances; these represent the "occurrences" that may be scheduled.
             */
            var groupLocationSchedules = new GroupLocationService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gl =>
                    _groupIds.Contains( gl.GroupId )
                    && gl.Location.IsActive
                )
                .SelectMany( gl => gl.Schedules, ( gl, s ) => new
                {
                    gl.Group,
                    gl.Location,
                    Schedule = s,
                    Config = gl.GroupLocationScheduleConfigs.FirstOrDefault( c => c.ScheduleId == s.Id )
                } )
                .Where( gls =>
                    gls.Schedule.IsActive
                    /*
                     * Limit to those schedules that fall within the specified date range. Due to the design of recurring schedules,
                     * we can only do loose date comparisons at the query level. We'll potentially pull back more records than we'll
                     * actually display (for now), and further refine once the schedule objects are materialized below.
                     * 
                     * Get schedules whose:
                     * 
                     *  1) EffectiveStartDate < end date (so we don't get any Schedules that start AFTER the specified date range), AND
                     *  2) EffectiveEndDate is null OR >= start date (so we don't get any Schedules that have already ended BEFORE the specified date range).
                     * 
                     * Keep in mind that schedules with a null EffectiveEndDate represent recurring schedules that have no end date.
                     */
                    && gls.Schedule.EffectiveStartDate.HasValue
                    && gls.Schedule.EffectiveStartDate.Value < _actualEndDate
                    && (
                        !gls.Schedule.EffectiveEndDate.HasValue
                        || gls.Schedule.EffectiveEndDate.Value >= _actualStartDate
                    )
                )
                .Select( gls => new GroupLocationSchedule
                {
                    Group = gls.Group,
                    Location = gls.Location,
                    Schedule = gls.Schedule,
                    Config = gls.Config,
                    AttendanceOccurrences = attendanceOccurrencesQry.Where( ao =>
                        ao.GroupId == gls.Group.Id
                        && ao.LocationId == gls.Location.Id
                        && ao.ScheduleId == gls.Schedule.Id
                    )
                    .ToList()
                } )
                .ToList();

            // Remove any schedules that don't actually have start any date/time(s) within the specified date range, and set the start date/time(s) on those that remain.
            if ( groupLocationSchedules.Any() )
            {
                for ( int i = groupLocationSchedules.Count - 1; i >= 0; i-- )
                {
                    var groupLocationSchedule = groupLocationSchedules.ElementAt( i );

                    var startDateTimes = groupLocationSchedule.Schedule.GetScheduledStartTimes( _actualStartDate, _actualEndDate );
                    if ( startDateTimes?.Any() != true )
                    {
                        groupLocationSchedules.Remove( groupLocationSchedule );
                        continue;
                    }

                    groupLocationSchedule.StartDateTimes.AddRange( startDateTimes );
                }
            }

            // Refine the complete list of GroupLocationSchedules by the selected locations.
            var selectedLocationGuids = ( filters.Locations?.SelectedLocations ?? new List<ListItemBag>() )
                .Select( l => l.Value?.AsGuidOrNull() )
                .Where( g => g.HasValue )
                .Select( g => g.Value )
                .ToList();

            var glsMatchingLocations = groupLocationSchedules
                .Where( gls => !selectedLocationGuids.Any() || selectedLocationGuids.Contains( gls.Location.Guid ) )
                .ToList();

            // Refine the complete list of GroupLocationSchedules by the selected schedules.
            var selectedScheduleGuids = ( filters.Schedules?.SelectedSchedules ?? new List<ListItemBag>() )
                .Select( s => s.Value?.AsGuidOrNull() )
                .Where( g => g.HasValue )
                .Select( g => g.Value )
                .ToList();

            var glsMatchingSchedules = groupLocationSchedules
                .Where( gls => !selectedScheduleGuids.Any() || selectedScheduleGuids.Contains( gls.Schedule.Guid ) )
                .ToList();

            /*
             * Refine down to the intersect of the above two collections.
             * This is the list of GroupLocationSchedules that match all currently-applied filters.
             */
            _groupLocationSchedules = glsMatchingLocations
                .Intersect( glsMatchingSchedules )
                .ToList();

            // Determine the new list of available (and selected) locations based on the currently-selected schedules.
            var availableLocations = glsMatchingSchedules
                .GroupBy( gls => gls.Location.Id )
                .Select( grouping => new ListItemBag
                {
                    Value = grouping.FirstOrDefault()?.Location?.Guid.ToString(),
                    Text = grouping.FirstOrDefault()?.Location.ToString( true )
                } )
                .ToList();

            var selectedLocations = availableLocations
                .Where( l => selectedLocationGuids.Any( selected => selected.ToString() == l.Value ) )
                .ToList();

            // Determine the new list of available (and selected) schedules based on the currently-selected locations.
            var availableSchedules = glsMatchingLocations
                .GroupBy( gls => gls.Schedule.Id )
                .Select( grouping => new ListItemBag
                {
                    Value = grouping.FirstOrDefault()?.Schedule?.Guid.ToString(),
                    Text = grouping.FirstOrDefault()?.Schedule?.ToString()
                } )
                .ToList();

            var selectedSchedules = availableSchedules
                .Where( s => selectedScheduleGuids.Any( selected => selected.ToString() == s.Value ) )
                .ToList();

            // Update the filters object to reflect the results.
            filters.Locations = new GroupSchedulerLocationsBag
            {
                AvailableLocations = availableLocations,
                SelectedLocations = selectedLocations
            };

            filters.Schedules = new GroupSchedulerSchedulesBag
            {
                AvailableSchedules = availableSchedules,
                SelectedSchedules = selectedSchedules
            };
        }

        /// <summary>
        /// Gets the list of [group, location, schedule, occurrence date] occurrences, based on the currently-applied filters.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The list of [group, location, schedule, occurrence date] occurrences.</returns>
        private List<GroupSchedulerOccurrenceBag> GetScheduleOccurrences( RockContext rockContext )
        {
            if ( _groupLocationSchedules?.Any() != true )
            {
                return null;
            }

            EnsureAttendanceOccurrencesExist( rockContext );

            return _groupLocationSchedules
                .SelectMany( gls => gls.StartDateTimes, ( gls, startDateTime ) =>
                {
                    var attendanceOccurrenceId = gls.AttendanceOccurrences
                        .FirstOrDefault( ao =>
                            ao.GroupId == gls.Group.Id
                            && ao.LocationId == gls.Location.Id
                            && ao.ScheduleId == gls.Schedule.Id
                            && ao.OccurrenceDate == startDateTime.Date
                        )?.Id ?? 0;

                    return new GroupSchedulerOccurrenceBag
                    {
                        AttendanceOccurrenceId = attendanceOccurrenceId,
                        GroupId = gls.Group.Id,
                        GroupName = gls.Group.Name,
                        ParentGroupId = gls.Group.ParentGroupId,
                        LocationId = gls.Location.Id,
                        LocationName = gls.Location.ToString( true ),
                        ScheduleId = gls.Schedule.Id,
                        ScheduleName = gls.Schedule.ToString(),
                        ScheduleOrder = gls.Schedule.Order,
                        OccurrenceDate = startDateTime.Date.ToISO8601DateString(),
                        FriendlyOccurrenceDate = startDateTime.Date.ToString( "dddd, MMM d" ),
                        OccurrenceDateTime = startDateTime,
                        SundayDate = RockDateTime.GetSundayDate( startDateTime ).ToISO8601DateString(),
                        MinimumCapacity = gls.Config?.MinimumCapacity,
                        DesiredCapacity = gls.Config?.DesiredCapacity,
                        MaximumCapacity = gls.Config?.MaximumCapacity
                    };
                } )
                .Where( o => o.AttendanceOccurrenceId > 0 )
                .OrderBy( o => o.OccurrenceDate )
                .ThenBy( o => o.ScheduleOrder )
                .ThenBy( o => o.OccurrenceDateTime )
                .ToList();
        }

        /// <summary>
        /// Ensures attendance occurrence records exists for all [group, location, schedule, occurrence date] occurrences, based on the currently-applied filters.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void EnsureAttendanceOccurrencesExist( RockContext rockContext )
        {
            if ( _groupLocationSchedules?.Any() != true )
            {
                return;
            }

            var newAttendanceOccurrences = new List<AttendanceOccurrence>();

            foreach ( var gls in _groupLocationSchedules )
            {
                foreach ( var startDateTime in gls.StartDateTimes )
                {
                    var occurrenceDate = startDateTime.Date;
                    if ( gls.AttendanceOccurrences.Any( ao => ao.OccurrenceDate == occurrenceDate ) )
                    {
                        continue;
                    }

                    var attendanceOccurrence = new AttendanceOccurrence
                    {
                        GroupId = gls.Group.Id,
                        LocationId = gls.Location.Id,
                        ScheduleId = gls.Schedule.Id,
                        OccurrenceDate = occurrenceDate
                    };

                    gls.AttendanceOccurrences.Add( attendanceOccurrence );
                    newAttendanceOccurrences.Add( attendanceOccurrence );
                }
            }

            if ( !newAttendanceOccurrences.Any() )
            {
                return;
            }

            var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
            attendanceOccurrenceService.AddRange( newAttendanceOccurrences );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Gets the resource settings, overriding any defaults with user preferences.
        /// </summary>
        /// <returns>The resource settings.</returns>
        private GroupSchedulerResourceSettingsBag GetResourceSettings()
        {
            var enabledResourceListSourceTypes = GetEnabledResourceListSourceTypes();

            // TODO (JPH): Hook into user preferences to override defaults, once supported in Obsidian blocks.

            return new GroupSchedulerResourceSettingsBag
            {
                EnabledResourceListSourceTypes = enabledResourceListSourceTypes,
                ResourceListSourceType = enabledResourceListSourceTypes.FirstOrDefault(),
                ResourceGroupMemberFilterType = default
            };
        }

        /// <summary>
        /// Gets the enabled resource list source types from which individuals may be scheduled.
        /// </summary>
        /// <returns>The enabled resource list source types.</returns>
        private List<ResourceListSourceType> GetEnabledResourceListSourceTypes()
        {
            var enabledTypes = new List<ResourceListSourceType> {
                ResourceListSourceType.GroupMembers,
                ResourceListSourceType.GroupMatchingPreference,
                ResourceListSourceType.GroupMatchingAssignment
            };

            if ( GetAttributeValue( AttributeKey.EnableAlternateGroupIndividualSelection ).AsBoolean() )
            {
                enabledTypes.Add( ResourceListSourceType.AlternateGroup );
            }

            if ( GetAttributeValue( AttributeKey.EnableParentGroupIndividualSelection ).AsBoolean() )
            {
                enabledTypes.Add( ResourceListSourceType.ParentGroup );
            }

            if ( GetAttributeValue( AttributeKey.EnableDataViewIndividualSelection ).AsBoolean() )
            {
                enabledTypes.Add( ResourceListSourceType.DataView );
            }

            return enabledTypes;
        }

        /// <summary>
        /// Gets the clone settings, overriding any defaults with user preferences.
        /// </summary>
        /// <returns>The clone settings.</returns>
        private GroupSchedulerCloneSettingsBag GetCloneSettings()
        {
            // TODO (JPH): Hook into user preferences to override defaults, once supported in Obsidian blocks.

            return new GroupSchedulerCloneSettingsBag();
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</returns>
        private string GetSecurityGrantToken()
        {
            return new Rock.Security.SecurityGrant().ToToken();
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Validates and applies the provided filters, then returns the new list of [group, location, schedule, occurrence date] occurrences,
        /// based on the applied filters.
        /// </summary>
        /// <param name="bag">The filters to apply.</param>
        /// <returns>An object containing the validated filters and new list of filtered [group, location, schedule, occurrence date] occurrences.</returns>
        [BlockAction]
        public BlockActionResult ApplyFilters( GroupSchedulerFiltersBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.SqlLogging( true );

                RefineFilters( rockContext, bag );

                var results = new GroupSchedulerAppliedFiltersBag
                {
                    filters = bag,
                    ScheduleOccurrences = GetScheduleOccurrences( rockContext )
                };

                rockContext.SqlLogging( false );

                return ActionOk( results );
            }
        }

        #endregion

        #region Supporting Classes

        private class GroupLocationSchedule
        {
            private readonly List<DateTime> _startDateTimes = new List<DateTime>();

            public Rock.Model.Group Group { get; set; }

            public Location Location { get; set; }

            public Schedule Schedule { get; set; }

            public GroupLocationScheduleConfig Config { get; set; }

            public List<AttendanceOccurrence> AttendanceOccurrences { get; set; }

            public List<DateTime> StartDateTimes => _startDateTimes;
        }

        #endregion
    }
}
