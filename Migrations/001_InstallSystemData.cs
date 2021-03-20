using Rock.Plugin;

namespace com.shepherdchurch.CubeDown.Migrations
{
    [MigrationNumber( 1, "1.8.3" )]
    public class InstallSystemData : Migration
    {
        #region Lava

        static readonly string ComponentDetailTemplate = @"<div class=""row"">
    <div class=""col-md-6"">
        <dl><dt>Name</dt><dd>{{ InteractionComponent.Name }}<dd/></dl>
    </div>
    <div class=""col-md-6"">
        <dl>
            <dt>Customer</dt>
            <dd>
                {% assign customer = InteractionComponent.EntityId | PersonByAliasId %}
                {% if customer != null %}
                    <a href=""/Person/{{ customer.Id }}"">{{ customer.FullName }}</a>
                {% endif %}
            <dd/>
        </dl>
    </div>
</div>

{% assign cart = InteractionComponent.ComponentData | FromJSON %}
<div class=""row"">
    <div class=""col-md-6"">
        <dl>
            <dt>
                <div class=""row"">
                    <div class=""col-sm-6"">Item</div>
                    <div class=""col-sm-2"">Qty</div>
                    <div class=""col-sm-2"">Price</div>
                    <div class=""col-sm-2"">Ext Price</div>
                </div>
            </dt>
            <dd>
                {% for item in cart.Items %}
                    <div class=""row"">
                        <div class=""col-sm-6"">{{ item.Name }}</div>
                        <div class=""col-sm-2"">{{ item.Quantity }}</div>
                        <div class=""col-sm-2"">{{ item.Price | FormatAsCurrency }}</div>
                        <div class=""col-sm-2"">
                            {{ item.ExtendedPrice | FormatAsCurrency }}
                            {% if item.IsTaxable %}<em>T</em>{% endif %}
                        </div>
                    </div>
                {% endfor %}
            </dd>
        </dl>
    </div>
    
    <div class=""col-md-6"">
        <dl>
            <div class=""row"">
                <div class=""col-sm-4"">
                    <dt>Subtotal</dt>
                    <dd>{{ cart.Subtotal | FormatAsCurrency }}</dd>
                </div>
                <div class=""col-sm-4"">
                    <dt>Tax</dt>
                    <dd>{{ cart.Tax | FormatAsCurrency }} <em>({{ cart.TaxRate }}%)</em></dd>
                </div>
                <div class=""col-sm-4"">
                    <dt>Total</dt>
                    <dd>{{ cart.Total | FormatAsCurrency }}</dd>
                </div>
            </div>
        </dl>
    </div>
</div>
";

        #endregion

        public override void Up()
        {
            //
            // Add the Interaction Channel Medium type.
            //
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.INTERACTION_CHANNEL_MEDIUM,
                "Receipt",
                "Used for tracking Cube Down terminal receipts.",
                SystemGuid.DefinedValue.RECEIPT,
                true );

            //
            // Add the Financial Transaction Type.
            //
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE,
                "Purchase",
                "A purchase at a Cube Down terminal.",
                SystemGuid.DefinedValue.PURCHASE,
                true );

            //
            // Add the interaction channel.
            //
            Sql( $@"
DECLARE @ComponentEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '{ Rock.SystemGuid.EntityType.PERSON_ALIAS }')
DECLARE @ChannelTypeMediumValueId INT = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{ SystemGuid.DefinedValue.RECEIPT }')

INSERT INTO [InteractionChannel]
	([Name], [ComponentEntityTypeId], [ChannelTypeMediumValueId], [UsesSession], [ComponentDetailTemplate], [IsActive], [Guid])
	VALUES (
		'Cube Down Receipts',
		@ComponentEntityTypeId,
		@ChannelTypeMediumValueId,
		0,
		'{ ComponentDetailTemplate.Replace( "'", "''" ) }',
		1,
		'{ SystemGuid.InteractionChannel.CUBE_DOWN_RECEIPTS }')" );

            //
            // Add the Content Channel Type.
            //
            Sql( $@"
INSERT INTO [ContentChannelType]
	([Name], [DateRangeType], [DisablePriority], [IncludeTime], [DisableContentField], [DisableStatus], [IsSystem], [Guid])
VALUES
	('Cube Down Products', 3, 1, 1, 0, 1, 1, '{ SystemGuid.ContentChannelType.CUBE_DOWN_PRODUCTS }')
" );

            int contentChannelTypeId = (int)SqlScalar( $"SELECT [Id] FROM [ContentChannelType] WHERE [Guid] = '{ SystemGuid.ContentChannelType.CUBE_DOWN_PRODUCTS }'" );

            //
            // Add the "Active" attribute to the content channel type.
            //
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ContentChannelItem",
                Rock.SystemGuid.FieldType.BOOLEAN,
                "ContentChannelTypeId",
                contentChannelTypeId.ToString(),
                "Active",
                string.Empty,
                string.Empty,
                0,
                "True",
                "BEB84411-960E-41A8-B3B1-A849B16BF434",
                "CubeDown.Active" );
            Sql( $"UPDATE [Attribute] SET [IsRequired] = 1 WHERE [Guid] = 'BEB84411-960E-41A8-B3B1-A849B16BF434'" );

            //
            // Add the "Image" attribute to the content channel type.
            //
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ContentChannelItem",
                Rock.SystemGuid.FieldType.IMAGE,
                "ContentChannelTypeId",
                contentChannelTypeId.ToString(),
                "Image",
                string.Empty,
                string.Empty,
                1,
                "",
                "7C78DCE8-3D72-4878-8A75-55137DF8C87A",
                "CubeDown.Image" );
            RockMigrationHelper.AddAttributeQualifier( "7C78DCE8-3D72-4878-8A75-55137DF8C87A",
                "binaryFileType",
                Rock.SystemGuid.BinaryFiletype.CONTENT_CHANNEL_ITEM_IMAGE,
                "5863EF24-4DF2-45CE-9F65-A8941586DE02" );

            //
            // Add the "Price" attribute to the content channel type.
            //
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ContentChannelItem",
                Rock.SystemGuid.FieldType.CURRENCY,
                "ContentChannelTypeId",
                contentChannelTypeId.ToString(),
                "Price",
                string.Empty,
                string.Empty,
                2,
                "",
                "DA635766-B4AD-48CA-BE5A-0BA6C3B9726B",
                "CubeDown.Price" );
            Sql( $"UPDATE [Attribute] SET [IsRequired] = 1, [IsGridColumn] = 1 WHERE [Guid] = 'DA635766-B4AD-48CA-BE5A-0BA6C3B9726B'" );

            //
            // Add the "Taxable" attribute to the content channel type.
            //
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ContentChannelItem",
                Rock.SystemGuid.FieldType.BOOLEAN,
                "ContentChannelTypeId",
                contentChannelTypeId.ToString(),
                "Taxable",
                string.Empty,
                string.Empty,
                3,
                "False",
                "8E2FDB90-F276-4CAA-80E2-C7F1E67C8C14",
                "CubeDown.Taxable" );


        }

        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "8E2FDB90-F276-4CAA-80E2-C7F1E67C8C14" );
            RockMigrationHelper.DeleteAttribute( "DA635766-B4AD-48CA-BE5A-0BA6C3B9726B" );
            RockMigrationHelper.DeleteAttribute( "7C78DCE8-3D72-4878-8A75-55137DF8C87A" );
            RockMigrationHelper.DeleteAttribute( "BEB84411-960E-41A8-B3B1-A849B16BF434" );
            RockMigrationHelper.DeleteByGuid( SystemGuid.InteractionChannel.CUBE_DOWN_RECEIPTS, "InteractionChannel" );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.PURCHASE );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.RECEIPT );
        }
    }
}
