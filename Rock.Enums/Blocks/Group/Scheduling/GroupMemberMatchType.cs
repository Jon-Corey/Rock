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

namespace Rock.Enums.Blocks.Group.Scheduling
{
    /// <summary>
    /// The group member match type options available in the Group Scheduler block.
    /// </summary>
    public enum GroupMemberMatchType
    {
        /// <summary>
        /// Show all members of the selected group.
        /// </summary>
        AllGroupMembers = 0,

        /// <summary>
        /// Show all members of the selected group that have a scheduling preference set for the selected week.
        /// </summary>
        MatchingWeek = 1,

        /// <summary>
        /// Show all members of the selected group that have a scheduling preference set for the selected week AND whose assignment (location/schedule) matches the selected location and schedule OR they have no assignment.
        /// </summary>
        MatchingAssignment = 2
    }
}
