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
