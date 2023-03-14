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
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduler
{
    /// <summary>
    /// The currently-applied filters, to limit what is shown on the Group scheduler.
    /// </summary>
    public class GroupSchedulerFiltersBag
    {
        /// <summary>
        /// Gets or sets the selected groups.
        /// </summary>
        /// <value>
        /// The selected groups.
        /// </value>
        public List<ListItemBag> Groups { get; set; }

        /// <summary>
        /// Gets or sets the selected locations.
        /// </summary>
        /// <value>
        /// The selected locations.
        /// </value>
        public List<ListItemBag> Locations { get; set; }

        /// <summary>
        /// Gest or sets the selected schedules.
        /// </summary>
        /// <value>
        /// The selected schedules.
        /// </value>
        public List<ListItemBag> Schedules { get; set; }

        /// <summary>
        /// Gets or sets the selected date range.
        /// </summary>
        /// <value>
        /// The selected date range.
        /// </value>
        public SlidingDateRangeBag DateRange { get; set; }

        /// <summary>
        /// Gets or sets the end of week dates, based on the selected date range.
        /// </summary>
        /// <value>
        /// The end of week dates, based on the selected date range.
        /// </value>
        public List<DateTime> EndOfWeekDates { get; set; }
    }
}
