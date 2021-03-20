using Rock.Plugin;

namespace com.shepherdchurch.CubeDown.Migrations
{
    [MigrationNumber( 2, "1.8.3" )]
    public class SetupPages : Migration
    {
        public override void Up()
        {
            #region Block Type Terminal

            // BlockType: Terminal
            RockMigrationHelper.AddBlockType( "Terminal",
                "Acts as a POS terminal in Rock for selling items.",
                "~/Plugins/com_shepherdchurch/CubeDown/Terminal.ascx",
                "Shepherd Church > Cube Down",
                SystemGuid.BlockType.TERMINAL );

            #endregion

            #region Block Type View Receipt Lava

            // BlockType: View Receipt Lava
            RockMigrationHelper.AddBlockType( "View Receipt Lava",
                "Shows a user their receipt from a previous purchase.",
                "~/Plugins/com_shepherdchurch/CubeDown/ViewReceiptLava.ascx",
                "Shepherd Church > Cube Down",
                SystemGuid.BlockType.VIEW_RECEIPT_LAVA );

            #endregion

            #region Page Terminal

            // Page: Terminal
            RockMigrationHelper.AddPage( "5b6dbc42-8b03-4d15-8d92-aafa28fd8616",
                "2e169330-d7d7-4eca-b417-72c64be150f0", // Blank
                "Terminal",
                @"",
                SystemGuid.Page.TERMINAL,
                "fa fa-hand-holding-usd" );

            // Block for Page Terminal: Terminal
            RockMigrationHelper.AddBlock( SystemGuid.Page.TERMINAL,
                "",
                SystemGuid.BlockType.TERMINAL,
                "Terminal",
                "Main",
                @"",
                @"",
                0,
                SystemGuid.Block.TERMINAL__TERMINAL );

            #endregion

            #region Page Terminal > View Receipt

            // Page: View Receipt
            RockMigrationHelper.AddPage( SystemGuid.Page.TERMINAL,
                "2e169330-d7d7-4eca-b417-72c64be150f0", // Blank
                "View Receipt",
                @"",
                SystemGuid.Page.VIEW_RECEIPT,
                "" );

            RockMigrationHelper.AddPageRoute( SystemGuid.Page.VIEW_RECEIPT, "ViewReceipt/{ReceiptCode}" );

            // Block for Page View Receipt: View Receipt Lava
            RockMigrationHelper.AddBlock( SystemGuid.Page.VIEW_RECEIPT,
                "",
                SystemGuid.BlockType.VIEW_RECEIPT_LAVA,
                "View Receipt Lava",
                "Main",
                @"",
                @"",
                0,
                SystemGuid.Block.VIEW_RECEIPT__VIEW_RECEIPT_LAVA );

            RockMigrationHelper.AddSecurityAuthForPage( SystemGuid.Page.VIEW_RECEIPT,
                0,
                Rock.Security.Authorization.VIEW,
                true,
                string.Empty,
                (int)Rock.Model.SpecialRole.AllUsers,
                "EEFAFCD6-13F4-4918-BB68-4633FA48A922" );

            #endregion
        }

        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( SystemGuid.Block.VIEW_RECEIPT__VIEW_RECEIPT_LAVA );
            RockMigrationHelper.DeletePage( SystemGuid.Page.VIEW_RECEIPT );

            RockMigrationHelper.DeleteBlock( SystemGuid.Block.TERMINAL__TERMINAL );
            RockMigrationHelper.DeletePage( SystemGuid.Page.TERMINAL );

            RockMigrationHelper.DeleteBlockType( SystemGuid.BlockType.VIEW_RECEIPT_LAVA );
            RockMigrationHelper.DeleteBlockType( SystemGuid.BlockType.TERMINAL );
        }
    }
}
