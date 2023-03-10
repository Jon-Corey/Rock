namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 199, "1.14.1" )]
    public class AddObsidianGroupScheduler : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddObsidianGroupScheduler_AddPublicBlockTypes();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            ShortTermServingProjects_DeletePublicBlockTypes();
        }

        /// <summary>
        /// JPH: Add public block types needed for the Obsidian Group Scheduler.
        /// </summary>
        private void AddObsidianGroupScheduler_AddPublicBlockTypes()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Group.Scheduling.GroupScheduler", "Group Scheduler", "Rock.Blocks.Group.Scheduling.GroupScheduler, Rock.Blocks, Version=1.15.0.13, Culture=neutral, PublicKeyToken=null", false, false, "7ADCE833-A785-4A54-9805-7335809C5367" );
            RockMigrationHelper.UpdateMobileBlockType( "Group Scheduler", "Allows group schedules for groups and locations to be managed by a scheduler.", "Rock.Blocks.Group.Scheduling.GroupScheduler", "Group Scheduling", "511D8E2E-4AF3-48D8-88EF-2AB311CD47E0" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "511D8E2E-4AF3-48D8-88EF-2AB311CD47E0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Alternate Group Individual Selection", "EnableAlternateGroupIndividualSelection", "Enable Alternate Group Individual Selection", "Determines if individuals may be selected from alternate groups.", 0, "False", "BE4DAA0D-95AB-40A8-826B-9391691C068D" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "511D8E2E-4AF3-48D8-88EF-2AB311CD47E0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Parent Group Individual Selection", "EnableParentGroupIndividualSelection", "Enable Parent Group Individual Selection", "Determines if individuals may be selected from parent groups.", 1, "False", "131BEF8F-55CE-4D6B-8EB2-B18449975B2C" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "511D8E2E-4AF3-48D8-88EF-2AB311CD47E0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Data View Individual Selection", "EnableDataViewIndividualSelection", "Enable Data View Individual Selection", "Determines if individuals may be selected from data views.", 2, "False", "6CA4FDDC-9B2F-406F-A7A6-583E25B0B942" );
        }

        /// <summary>
        /// /// JPH: Delete public block types added for the Obsidian Group Scheduler.
        /// </summary>
        private void ShortTermServingProjects_DeletePublicBlockTypes()
        {
            RockMigrationHelper.DeleteBlockAttribute( "6CA4FDDC-9B2F-406F-A7A6-583E25B0B942" );
            RockMigrationHelper.DeleteBlockAttribute( "131BEF8F-55CE-4D6B-8EB2-B18449975B2C" );
            RockMigrationHelper.DeleteBlockAttribute( "BE4DAA0D-95AB-40A8-826B-9391691C068D" );
            RockMigrationHelper.DeleteBlockType( "511D8E2E-4AF3-48D8-88EF-2AB311CD47E0" );
            RockMigrationHelper.DeleteEntityType( "7ADCE833-A785-4A54-9805-7335809C5367" );
        }
    }
}
