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
using Rock.Enums.Blocks.Group.Scheduling;

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduler
{
    /// <summary>
    /// The resource settings to indicate how individuals should be selected for assignment.
    /// </summary>
    public class GroupSchedulerResourceSettingsBag
    {
        /// <summary>
        /// Gets or sets the enabled <see cref="ResourceListSourceType"/>s, from which individuals may be scheduled.
        /// </summary>
        /// <value>
        /// The enabled <see cref="ResourceListSourceType"/>s, from which individuals may be scheduled.
        /// </value>
        public List<ResourceListSourceType> EnabledResourceListSourceTypes { get; set; }

        /// <summary>
        /// Gets or sets the selected <see cref="ResourceListSourceType"/>.
        /// </summary>
        /// <value>
        /// The selected <see cref="ResourceListSourceType"/>.
        /// </value>
        public ResourceListSourceType SourceType { get; set; }

        /// <summary>
        /// Gets or sets the selected <see cref="GroupMemberMatchType"/>.
        /// </summary>
        /// <value>
        /// The selected <see cref="GroupMemberMatchType"/>.
        /// </value>
        public GroupMemberMatchType MatchType { get; set; }
    }
}
