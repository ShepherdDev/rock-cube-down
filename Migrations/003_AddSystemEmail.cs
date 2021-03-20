using Rock.Plugin;

namespace com.shepherdchurch.CubeDown.Migrations
{
    [MigrationNumber( 3, "1.8.3" )]
    public class AddSystemEmail : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.UpdateSystemEmail( "Plugins",
                "Purchase Receipt Email",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                "Receipt #{{ Cart.ReceiptCode }}",
                @"{{ 'Global' | Attribute:'EmailHeader' }}

<p>Hello {{ Person.NickName }},</p>

<p>
    Thank you for your purchase of {{ Cart.Total | FormatAsCurrency }}.
    You can view your receipt by clicking <a href=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}ViewReceipt/{{ Cart.ReceiptCode }}"">this link</a>.
</p>

{{ 'Global' | Attribute:'EmailFooter' }}",
                SystemGuid.SystemEmail.PURCHASE_RECEIPT_EMAIL );


            //
            // Add the "Active" attribute to the content channel type.
            //
            int contentChannelTypeId = (int)SqlScalar( $"SELECT [Id] FROM [ContentChannelType] WHERE [Guid] = '{ SystemGuid.ContentChannelType.CUBE_DOWN_PRODUCTS }'" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ContentChannelItem",
                Rock.SystemGuid.FieldType.FINANCIAL_ACCOUNT,
                "ContentChannelTypeId",
                contentChannelTypeId.ToString(),
                "Account",
                string.Empty,
                "Specifies the financial account that will be used for this item.",
                4,
                "",
                "33E6026C-9D3C-4732-9AED-1954525EFBAB",
                "CubeDown.Account" );
        }

        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "33E6026C-9D3C-4732-9AED-1954525EFBAB" );
            RockMigrationHelper.DeleteSystemEmail( SystemGuid.SystemEmail.PURCHASE_RECEIPT_EMAIL );
        }
    }
}
