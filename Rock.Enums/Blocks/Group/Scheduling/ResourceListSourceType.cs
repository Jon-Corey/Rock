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
    /// The resource list source type options available in the Group Scheduler block.
    /// </summary>
    public enum ResourceListSourceType
    {
        /// <summary>
        /// The members of the selected group.
        /// </summary>
        Group = 0,

        /// <summary>
        /// The members of another group.
        /// </summary>
        AlternateGroup = 1,

        /// <summary>
        /// The members of the parent group of the selected group.
        /// </summary>
        ParentGroup = 2,

        /// <summary>
        /// The people that exist in a selected data view.
        /// </summary>
        DataView = 3
    }
}
