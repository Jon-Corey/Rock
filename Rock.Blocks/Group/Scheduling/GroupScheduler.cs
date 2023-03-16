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

        private List<Rock.Model.Group> _groups;
        private List<Rock.Model.GroupLocation> _groupLocations;
        private List<Rock.Model.Schedule> _schedules;

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

                SetBoxInitialState( box, rockContext );

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

            var filters = GetFilters( rockContext );

            box.Filters = filters;
            box.ScheduleOccurrences = GetScheduleOccurrences( rockContext, filters );
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

            ValidateFilters( rockContext, filters );

            return filters;
        }

        /// <summary>
        /// Validates the filters, overriding any selections if necessary.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filters">The filters that should be validated.</param>
        private void ValidateFilters( RockContext rockContext, GroupSchedulerFiltersBag filters )
        {
            ValidateDateRange( filters );
            GetAuthorizedGroups( rockContext, filters );
            UpdateLocations( rockContext, filters );
            UpdateSchedules( rockContext, filters );
        }

        /// <summary>
        /// Validates the date range and sets the first and last "end of week" dates (as well as the friendly date range) on the provided filters.
        /// <para>
        /// If the date range is invalid or in the past, the current "end of week" date will be used to set all date values.
        /// </para>
        /// </summary>
        /// <param name="filters">The filters whose date range should be validated.</param>
        private void ValidateDateRange( GroupSchedulerFiltersBag filters )
        {
            var thisEndOfWeekDate = RockDateTime.Now.EndOfWeek( RockDateTime.FirstDayOfWeek ).Date;
            var adjustPicker = false;

            DateTime? firstEndOfWeekDate = null;
            var startDate = filters.DateRange?.LowerDate?.Date;
            if ( startDate.HasValue )
            {
                firstEndOfWeekDate = startDate.Value.EndOfWeek( RockDateTime.FirstDayOfWeek );
            }
            else
            {
                firstEndOfWeekDate = thisEndOfWeekDate;
                adjustPicker = true;
            }

            DateTime? lastEndOfWeekDate = null;
            var endDate = filters.DateRange?.UpperDate?.Date;
            if ( endDate.HasValue )
            {
                lastEndOfWeekDate = endDate.Value.EndOfWeek( RockDateTime.FirstDayOfWeek );
            }
            else
            {
                lastEndOfWeekDate = thisEndOfWeekDate;
                adjustPicker = true;
            }

            // Make sure we have a date range that makes sense.
            if ( lastEndOfWeekDate < firstEndOfWeekDate )
            {
                lastEndOfWeekDate = firstEndOfWeekDate;
                adjustPicker = true;
            }

            // Default to the current week if either a start or end date weren't provided or are in the past.
            if ( firstEndOfWeekDate.Value < thisEndOfWeekDate || lastEndOfWeekDate.Value < thisEndOfWeekDate )
            {
                firstEndOfWeekDate = thisEndOfWeekDate;
                lastEndOfWeekDate = thisEndOfWeekDate;

                adjustPicker = true;
            }

            var format = "M/d";
            string friendlyDateRange = null;

            // This doesn't need to be precise; just need to determine if we should try to list all "end of week" dates or just a range.
            var numberOfWeeks = ( lastEndOfWeekDate.Value - firstEndOfWeekDate.Value ).TotalDays / 7;
            if ( numberOfWeeks > 7 )
            {
                friendlyDateRange = $"{firstEndOfWeekDate.Value.ToString( format )} - {lastEndOfWeekDate.Value.ToString( format )}";
            }
            else
            {
                var endOfWeekDate = firstEndOfWeekDate.Value;
                var endOfWeekDates = new List<DateTime>();
                while ( endOfWeekDate <= lastEndOfWeekDate.Value )
                {
                    endOfWeekDates.Add( endOfWeekDate );

                    endOfWeekDate = endOfWeekDate.AddDays( 7 );
                }

                friendlyDateRange = string.Join( ", ", endOfWeekDates.Select( d => d.ToString( format ) ) );
            }

            filters.FirstEndOfWeekDate = firstEndOfWeekDate;
            filters.LastEndOfWeekDate = lastEndOfWeekDate;
            filters.FriendlyDateRange = friendlyDateRange;

            if ( adjustPicker )
            {
                // If we made any adjustments above, adjust the UI's sliding date range picker to match.
                filters.DateRange = new SlidingDateRangeBag
                {
                    LowerDate = firstEndOfWeekDate.Value.AddDays( -6 ),
                    UpperDate = lastEndOfWeekDate.Value,
                    RangeType = SlidingDateRangeType.DateRange
                };
            }
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
            var currentPerson = RequestContext.CurrentPerson;
            _groups = new GroupService( rockContext )
                .GetByGuids( groupGuids )
                .Include( g => g.ParentGroup )
                .AsNoTracking()
                .ToList();

            // Ensure the current user has the correct permission(s) to schedule the selected groups and update the filters if necessary.
            _groups = _groups
                .Where( g =>
                    g.IsAuthorized( Authorization.EDIT, this.RequestContext.CurrentPerson )
                    || g.IsAuthorized( Authorization.SCHEDULE, this.RequestContext.CurrentPerson )
                )
                .ToList();

            filters.Groups = _groups
                .Select( g => new ListItemBag
                {
                    Value = g.Guid.ToString(),
                    Text = g.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Updates the locations on the filters object to reflect the groups selected within the filters.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filters">The filters whose locations should be updated.</param>
        private void UpdateLocations( RockContext rockContext, GroupSchedulerFiltersBag filters )
        {
            if ( _groups?.Any() != true )
            {
                filters.Locations = null;
                return;
            }
        }

        /// <summary>
        /// Updates the schedules on the filters object to reflect the groups, locations and date range selected within the filters.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filters">The filters whose schedules should be updated.</param>
        private void UpdateSchedules( RockContext rockContext, GroupSchedulerFiltersBag filters )
        {
            if ( _groupLocations?.Any() != true )
            {
                filters.Schedules = null;
                return;
            }
        }

        /// <summary>
        /// Gets the list of [group, location, schedule] occurrences, based on the currently-applied filters.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filters">The currently-applied filters.</param>
        /// <returns>The list of [group, location, schedule] occurrences.</returns>
        private List<GroupSchedulerOccurrenceBag> GetScheduleOccurrences( RockContext rockContext, GroupSchedulerFiltersBag filters )
        {
            return null;
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
                SourceType = enabledResourceListSourceTypes.FirstOrDefault(),
                MatchType = default
            };
        }

        /// <summary>
        /// Gets the enabled resource list source types from which individuals may be scheduled.
        /// </summary>
        /// <returns>The enabled resource list source types.</returns>
        private List<ResourceListSourceType> GetEnabledResourceListSourceTypes()
        {
            var enabledTypes = new List<ResourceListSourceType> { ResourceListSourceType.Group };

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
        /// 
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult UpdateFilters( GroupSchedulerFiltersBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                ValidateFilters( rockContext, bag );

                return ActionOk( bag );
            }
        }

        /// <summary>
        /// Validates and applies the provided filters, then returns the new list of [group, location, schedule] occurrences, based on the applied filters.
        /// </summary>
        /// <param name="bag">The filters to apply.</param>
        /// <returns>An object containing the validated filters and new list of filtered [group, location, schedule] occurrences.</returns>
        [BlockAction]
        public BlockActionResult ApplyFilters( GroupSchedulerFiltersBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                ValidateFilters( rockContext, bag );

                var results = new GroupSchedulerAppliedFiltersBag
                {
                    AppliedFilters = bag,
                    ScheduleOccurrences = GetScheduleOccurrences( rockContext, bag )
                };

                return ActionOk( results );
            }
        }

        #endregion
    }
}
