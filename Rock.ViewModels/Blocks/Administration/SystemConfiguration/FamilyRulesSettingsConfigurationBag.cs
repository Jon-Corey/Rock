﻿// <copyright>
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

namespace Rock.ViewModels.Blocks.Administration.SystemConfiguration
{
    /// <summary>
    /// Contains the Family rules settings configuration details.
    /// </summary>
    public class FamilyRulesSettingsConfigurationBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether [enable bible strict spouse].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable bible strict spouse]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableBibleStrictSpouse { get; set; }
    }
}
