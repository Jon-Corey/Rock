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

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduler
{
    /// <summary>
    /// The information needed to identify a specific occurrence to be scheduled within the Group Scheduler.
    /// </summary>
    public class GroupSchedulerOccurrenceBag
    {
        /// <summary>
        /// Gets or sets the group ID for this occurrence.
        /// </summary>
        /// <value>
        /// The group ID for this occurrence.
        /// </value>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the parent group ID (if any) for this occurrence.
        /// </summary>
        /// <value>
        /// The parent group ID (if any) for this occurrence.
        /// </value>
        public int? ParentGroupId { get; set; }

        /// <summary>
        /// Gets or sets the location ID for this occurrence.
        /// </summary>
        /// <value>
        /// The location ID for this occurrence.
        /// </value>
        public int LocationId { get; set; }

        /// <summary>
        /// Gets or sets the schedule ID for this occurrence.
        /// </summary>
        /// <value>
        /// The schedule ID for this occurrence.
        /// </value>
        public int ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the ISO 8601 Sunday date for this occurrence.
        /// </summary>
        /// <value>
        /// The ISO 8601 Sunday date for this occurrence.
        /// </value>
        public string SundayDate { get; set; }
    }
}
