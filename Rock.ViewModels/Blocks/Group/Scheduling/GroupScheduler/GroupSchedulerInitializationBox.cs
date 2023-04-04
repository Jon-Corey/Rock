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

using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduler
{
    /// <summary>
    /// The box that contains all the initialization information for the Group Scheduler block.
    /// </summary>
    public class GroupSchedulerInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the filters to limit what is shown on the Group scheduler.
        /// </summary>
        /// <value>
        /// The filters to limit what is shown on the Group scheduler.
        /// </value>
        public GroupSchedulerFiltersBag Filters { get; set; }

        /// <summary>
        /// Gets or sets the [group, location, schedule] occurrences to be scheduled.
        /// </summary>
        /// <value>
        /// The [group, location, schedule] occurrences to be scheduled.
        ///</value>
        public List<GroupSchedulerOccurrenceBag> ScheduleOccurrences { get; set; }

        /// <summary>
        /// Gets or sets the clone settings to indicate how schedules should be cloned.
        /// </summary>
        /// <value>
        /// The clone settings to indicate how schedules should be cloned.
        /// </value>
        public GroupSchedulerCloneSettingsBag CloneSettings { get; set; }
    }
}
