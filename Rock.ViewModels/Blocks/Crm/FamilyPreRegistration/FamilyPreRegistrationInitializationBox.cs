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

namespace Rock.ViewModels.Blocks.Crm.FamilyPreRegistration
{
    /// <summary>
    /// The box that contains all the initialization information for the Family Pre-Registration block.
    /// </summary>
    public class FamilyPreRegistrationInitializationBox : BlockBox
    {
        /// <summary>
        /// Indicates whether the campus field is hidden.
        /// <para>If there is only one active campus then the campus field will not show.</para>
        /// </summary>
        public bool IsCampusHidden { get; set; }

        /// <summary>
        /// Indicates whether the campus field is optional.
        /// <para>If the campus field is required and there is only one active campus, then the campus field will not show and the single campus will be selected.</para>
        /// </summary>
        public bool IsCampusOptional { get; set; }

        /// <summary>
        /// The campus unique identifier to use by default when adding a new family.
        /// </summary>
        public Guid? DefaultCampusGuid { get; set; }

        /// <summary>
        /// Filters the campus field by campus types.
        /// </summary>
        public List<Guid> CampusTypesFilter { get; set; }

        /// <summary>
        /// Filters the campus field by campus statuses.
        /// </summary>
        public List<Guid> CampusStatusesFilter { get; set; }
    }
}
