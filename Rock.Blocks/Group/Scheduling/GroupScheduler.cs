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
        private List<int> _locationIds;
        private List<int> _scheduleIds;

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
                filters.Locations = null;
                filters.Schedules = null;
                return;
            }

            // Get all locations and schedules tied to the selected group(s) initially, so we can properly load the "available" lists.
            var groupLocationSchedulesQuery = new GroupLocationService( rockContext )
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
                    Schedule = s
                } )
                .Where( gls =>
                    gls.Schedule.IsActive
                );

            /*
             * Limit to those schedules that fall within the specified date range. Due to the design of recurring schedules,
             * we can only do loose date comparisons at the query level. We'll potentially pull back more records than we'll
             * actually display (for now), and further refine the schedules in a later step.
             */
            DateTime? startDate;
            DateTime? endDate;

            if ( filters.FirstEndOfWeekDate.HasValue )
            {
                /*
                 * Subtract 6 days from the first "end of week" date specified; this will be our starting date to schedule.
                 * This will limit to schedules that haven't already ended before the first "start of week" date.
                 * Keep in mind that schedules with a null EffectiveEndDate represent recurring schedules that have no end date.
                 */
                startDate = filters.FirstEndOfWeekDate.Value.AddDays( -6 );
                groupLocationSchedulesQuery = groupLocationSchedulesQuery
                    .Where( gls =>
                        !gls.Schedule.EffectiveEndDate.HasValue
                        || gls.Schedule.EffectiveEndDate.Value >= startDate
                    );
            }

            if ( filters.LastEndOfWeekDate.HasValue )
            {
                /*
                 * Limit to schedules that have already started on or before the last "end of week" date.
                 * We'll add a day to the filters value, since what we have so far is the selected "end of week" date @ 11:59.999PM.
                 * This way, we can use "less than" in our filtering, to follow Rock's rule: let your start be "inclusive" and your end be "exclusive".
                 */
                endDate = filters.LastEndOfWeekDate.Value.AddDays( 1 ).StartOfDay();
                groupLocationSchedulesQuery = groupLocationSchedulesQuery
                    .Where( gls =>
                        gls.Schedule.EffectiveStartDate.HasValue
                        && gls.Schedule.EffectiveStartDate < endDate
                    );
            }

            // Materialize the list of GroupLocationSchedules so we can perform additional, in-memory filtering.
            var groupLocationSchedules = groupLocationSchedulesQuery.ToList();

            // TODO: complete final date filtering here.

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
            var glsMatchingFilters = glsMatchingLocations
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
        /// Validates and applies the provided filters, then returns the new list of [group, location, schedule] occurrences, based on the applied filters.
        /// </summary>
        /// <param name="bag">The filters to apply.</param>
        /// <returns>An object containing the validated filters and new list of filtered [group, location, schedule] occurrences.</returns>
        [BlockAction]
        public BlockActionResult ApplyFilters( GroupSchedulerFiltersBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                RefineFilters( rockContext, bag );

                var results = new GroupSchedulerAppliedFiltersBag
                {
                    filters = bag,
                    ScheduleOccurrences = GetScheduleOccurrences( rockContext, bag )
                };

                return ActionOk( results );
            }
        }

        #endregion
    }
}
