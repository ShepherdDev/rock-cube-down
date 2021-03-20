using System;
using System.ComponentModel;
using System.Linq;

using com.shepherdchurch.CubeDown;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_shepherdchurch.CubeDown
{
    [DisplayName( "View Receipt Lava" )]
    [Category( "Shepherd Church > Cube Down" )]
    [Description( "Shows a user their receipt from a previous purchase." )]

    [IntegerField( "Max Receipt Age", "The number of days after a receipt is issued that it can be viewed from this block. Can be set to 0 for no limit.", false, 90, order: 0 )]
    [CodeEditorField( "Receipt Template", "The lava to use when rendering the receipt.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, true, "{% include '~/Plugins/com_shepherdchurch/CubeDown/Assets/Receipt.lava' %}", order: 1 )]
    public partial class ViewReceiptLava : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                ShowReceipt( PageParameter( "ReceiptCode" ) );
            }
        }

        /// <summary>
        /// Shows the receipt.
        /// </summary>
        /// <param name="receiptCode">The receipt code.</param>
        protected void ShowReceipt( string receiptCode )
        {
            var rockContext = new RockContext();
            ShoppingCart cart = null;
            Person customer = null;
            int? receiptId = null;
            int entityTypeId = EntityTypeCache.Get( typeof( InteractionComponent ) ).Id;
            int maxAgeInDays = GetAttributeValue( "MaxReceiptAge" ).AsInteger();

            nbNotFound.Visible = true;
            lReceipt.Text = string.Empty;

            if ( string.IsNullOrWhiteSpace( receiptCode ) )
            {
                return;
            }

            //
            // Attempt to decode the receipt code.
            //
            ushort? receiptSecret;
            receiptId = ReceiptHelper.DecodeReceiptCode( receiptCode, out receiptSecret );

            if ( !receiptId.HasValue )
            {
                return;
            }

            //
            // Load the receipt and validate the code.
            //
            var receipt = new InteractionComponentService( rockContext ).Get( receiptId.Value );
            if ( receipt == null || !ReceiptHelper.ValidateReceiptCode( receipt, receiptCode ) )
            {
                return;
            }

            //
            // Verify the receipt is not past the age limit.
            //
            if ( maxAgeInDays > 0 && RockDateTime.Now.Subtract( receipt.CreatedDateTime.Value).TotalDays >= maxAgeInDays)
            {
                return;
            }

            //
            // Load the cart and the customer if we have one.
            //
            cart = receipt.ComponentData.FromJsonOrNull<ShoppingCart>();
            if ( cart == null )
            {
                return;
            }

            if ( receipt.EntityId.HasValue )
            {
                customer = new PersonAliasService( rockContext ).GetPerson( receipt.EntityId.Value );
            }

            //
            // Load any related payments.
            //
            var payments = new FinancialTransactionService( rockContext ).Queryable()
                .Where( t => t.TransactionDetails.Any( d => d.EntityTypeId == entityTypeId && d.EntityId == receiptId ) );

            //
            // Setup the lava merge and display the receipt.
            //
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );
            mergeFields.Add( "Cart", cart );
            mergeFields.Add( "Person", customer );
            mergeFields.Add( "Payments", payments );

            nbNotFound.Visible = false;
            lReceipt.Text = GetAttributeValue( "ReceiptTemplate" ).ResolveMergeFields( mergeFields, CurrentPerson );
        }
    }
}
