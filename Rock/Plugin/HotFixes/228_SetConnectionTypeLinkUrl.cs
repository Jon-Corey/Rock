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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 228, "1.16.7" )]
    public class SetConnectionTypeLinkUrl : Migration
    {
        /// <inheritdoc/>
        public override void Up()
        {
            //----------------------------------------------------------------------------------
            // <auto-generated>
            //     This Up() migration method was generated by the Rock.CodeGeneration project.
            //     The purpose is to prevent hotfix migrations from running when they are not
            //     needed. The migrations in this file are run by an EF migration instead.
            // </auto-generated>
            //----------------------------------------------------------------------------------
        }

        private void OldUp()
        {
            Sql( $@"
                UPDATE  [EntityType]
                SET     [LinkUrlLavaTemplate] = '~/people/connections/types/{{{{ Entity.Id }}}}'
                WHERE   [Guid] = '{Rock.SystemGuid.EntityType.CONNECTION_TYPE}'"
            );
        }

        /// <inheritdoc/>
        public override void Down()
        {
        }
    }
}
