using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

using com.shepherdchurch.CubeDown;
using com.shepherdchurch.CubeDown.Web.UI.Controls;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_shepherdchurch.CubeDown
{
    [DisplayName( "Terminal" )]
    [Category( "Shepherd Church > Cube Down" )]
    [Description( "Acts as a POS terminal in Rock for selling items." )]
    [CustomDropdownListField( "Content Channel", "The content channel that contains the items to be made available for purchase.", "SELECT CC.[Name] AS [Text], CC.[Guid] AS [Value] FROM [ContentChannel] AS CC INNER JOIN [ContentChannelType] AS CCT ON CCT.[Id] = CC.[ContentChannelTypeId] WHERE CCT.[Guid] = '1B6F0B28-91F8-45B7-8CE5-412E661AFEB3'", true, "", "", order: 0 )]
    [PersonField( "Guest Customer", "The person to associate anonymous transactions with.", true, order: 1 )]
    [CodeEditorField( "Items Template", "The lava template to use when formatting the items for sale.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, true, "{% include '~/Plugins/com_shepherdchurch/CubeDown/Assets/Terminal.lava' %}", "", order: 2 )]

    [FinancialGatewayField( "Credit Card Gateway", "The payment gateway to use for Credit Card transactions.", true, "", "Financial", order: 0 )]
    [AccountField( "Default Account", "If not account is specified on any of the purchased items, this account will be used.", true, "", "Financial", order: 1 )]
    [TextField( "Batch Name Prefix", "The prefix to add to the financial batch.", true, "Terminal Sale", "Financial", order: 2 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE, "Transaction Type", "The Financial Transaction Type to use when creating transactions.", true, false, com.shepherdchurch.CubeDown.SystemGuid.DefinedValue.PURCHASE, "Financial", order: 3 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE, "Source", "The Financial Source Type to use when creating transactions", true, false, Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_KIOSK, "Financial", order: 4 )]
    [DecimalField( "Tax Rate", "The tax rate to be applied to taxable items. If your tax rate is 8.25% then enter 8.25", false, 0, "Financial", order: 5 )]
    [BooleanField( "Allow Custom Items", "Allows user to enter their own line items via a direct dollar amount.", false, "Financial", order: 6 )]
    [TextField( "Custom Item Name", "The default name to use for custom items, can be overridden by user.", true, "Generic Sale", "Financial", order: 7 )]

    [CodeEditorField( "Receipt Template", "The content to display in the on-screen receipt text.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, true, @"<p>Receipt #{{ Cart.ReceiptCode }}</p>
<p>{{ 'Global' | Attribute:'PublicApplicationRoot' }}ViewReceipt/{{ Cart.ReceiptCode }}</p>
<div class='text-center'>
  <span class='qrcode' style='display: inline-block' data-text='{{ 'Global' | Attribute:'PublicApplicationRoot' }}ViewReceipt/{{ Cart.ReceiptCode }}'></span>
</div>", "Receipts", order: 0 )]
    [SystemEmailField( "Receipt Email", "", false, "", "Receipts", order: 1 )]
    [CodeEditorField( "SMS Receipt Template", "The text to send when an SMS receipt it selected.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, true, "To view your receipt for a purchase of {{ Cart.Total | FormatAsCurrency }} visit {{ 'Global' | Attribute:'PublicApplicationRoot' }}ViewReceipt/{{ Cart.ReceiptCode }}", "Receipts", order: 2 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM, "Receipt SMS Number", "", false, false, "", "Receipts", order: 3 )]
    public partial class Terminal : RockBlock
    {
        #region Properties

        /// <summary>
        /// Gets or sets the cart.
        /// </summary>
        /// <value>
        /// The cart.
        /// </value>
        protected ShoppingCart Cart { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        /// <value>
        /// The customer identifier.
        /// </value>
        protected int? CustomerId
        {
            get
            {
                return ( int? ) ViewState["CustomerId"];
            }
            set
            {
                ViewState["CustomerId"] = value;
                _customer = null;
            }
        }

        /// <summary>
        /// Gets or sets the customer.
        /// </summary>
        /// <value>
        /// The customer.
        /// </value>
        protected Person Customer
        {
            get
            {
                if ( _customer == null && CustomerId.HasValue )
                {
                    _customer = new PersonService( new RockContext() ).Get( CustomerId.Value );
                }

                return _customer;
            }
        }
        private Person _customer;

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += Terminal_BlockUpdated;

            RockPage.AddCSSLink( "~/Plugins/com_shepherdchurch/CubeDown/Styles/terminal.css" );
            RockPage.AddScriptLink( "~/Plugins/com_shepherdchurch/CubeDown/Scripts/qrcode.min.js" );
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            Cart = ( ShoppingCart ) ViewState["Cart"];
        }

        /// <summary>
        /// Raises the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                ResetTerminal();
            }
            else
            {
                string[] eventArgs = ( Request.Form["__EVENTARGUMENT"] ?? string.Empty ).Split( '^' );

                if ( eventArgs.Length == 2 )
                {
                    switch ( eventArgs[0] )
                    {
                        case "AddToCart":
                            AddToCart( eventArgs[1].AsInteger() );
                            break;

                        case "AddCustomItem":
                            ShowAddCustomItem();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["Cart"] = Cart;

            return base.SaveViewState();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the item list.
        /// </summary>
        protected void ShowItemList()
        {
            var contentChannel = new ContentChannelService( new RockContext() ).Get( GetAttributeValue( "ContentChannel" ).AsGuid() );
            if ( contentChannel != null )
            {
                var items = contentChannel.Items.AsQueryable();

                DateTime now = RockDateTime.Now;

                //
                // Filter the items based on start and expire dates.
                //
                if ( contentChannel.ContentChannelType.DateRangeType == ContentChannelDateType.SingleDate || contentChannel.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange )
                {
                    items = items.Where( i => i.StartDateTime <= now );
                }

                if ( contentChannel.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange )
                {
                    items = items.Where( i => !i.ExpireDateTime.HasValue || i.ExpireDateTime.Value > now );
                }

                //
                // Filter items based on approval.
                //
                if ( contentChannel.RequiresApproval )
                {
                    items = items.Where( i => i.ApprovedDateTime.HasValue );
                }

                //
                // Sort the items.
                //
                if ( contentChannel.ItemsManuallyOrdered )
                {
                    items = items.OrderBy( i => i.Order );
                }
                else
                {
                    items = items.OrderBy( i => i.Title );
                }

                //
                // Display the items.
                //
                var itemList = items.ToList();
                if ( itemList.Count == 0 )
                {
                    lItemsItemList.Text = "<div class='alert alert-warning'>No items available for sale.</div>";
                }
                else
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );
                    mergeFields.Add( "Items", itemList );
                    mergeFields.Add( "AllowCustomItems", GetAttributeValue( "AllowCustomItems" ).AsBoolean() );
                    mergeFields.Add( "CustomItemName", GetAttributeValue( "CustomItemName" ) );

                    string template = GetAttributeValue( "ItemsTemplate" );
                    lItemsItemList.Text = template.ResolveMergeFields( mergeFields, CurrentPerson );
                }
            }
            else
            {
                lItemsItemList.Text = "<div class='alert alert-danger'>Missing configuration, please check block settings.</div>";
            }
        }

        /// <summary>
        /// Adds an item to the cart.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        protected void AddToCart( int itemId )
        {
            using ( var rockContext = new RockContext() )
            {
                var item = new ContentChannelItemService( rockContext ).Get( itemId );

                if ( item != null )
                {
                    var existingItem = Cart.Items.FirstOrDefault( i => i.ItemId == item.Id );

                    if ( existingItem != null )
                    {
                        existingItem.Quantity += 1;
                    }
                    else
                    {
                        Cart.Items.Add( new ShoppingCartItem( item ) );
                    }

                    Cart.Calculate();
                }
            }
        }

        /// <summary>
        /// Shows the add custom item.
        /// </summary>
        protected void ShowAddCustomItem()
        {
            tbCustomItemAmount.Text = string.Empty;
            cbCustomItemTaxable.Checked = false;
            tbCustomItemName.Text = string.Empty;
            tbCustomItemName.Placeholder = GetAttributeValue( "CustomItemName" );

            mdlCustomItem.Show();
        }

        /// <summary>
        /// Resets the terminal.
        /// </summary>
        protected void ResetTerminal()
        {
            CustomerId = null;
            Cart = new ShoppingCart
            {
                TaxRate = GetAttributeValue( "TaxRate" ).AsDecimal()
            };

            pnlCart.Visible = false;
            pnlPay.Visible = false;
            pnlReceipt.Visible = false;

            pnlItems.Visible = true;

            ShowItemList();
        }

        /// <summary>
        /// Gets the transaction details.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        protected List<FinancialTransactionDetail> GetTransactionDetails( RockContext rockContext )
        {
            var details = new List<FinancialTransactionDetail>();
            var financialAccountService = new FinancialAccountService( rockContext );
            int defaultAccountId = financialAccountService.Get( GetAttributeValue( "DefaultAccount" ).AsGuid() ).Id;

            foreach ( var item in Cart.Items )
            {
                int financialAccountId = item.AccountId ?? defaultAccountId;

                var transactionDetail = details.FirstOrDefault( d => d.AccountId == financialAccountId );

                if ( transactionDetail == null )
                {
                    transactionDetail = new FinancialTransactionDetail
                    {
                        Amount = 0,
                        AccountId = financialAccountId
                    };
                    details.Add( transactionDetail );
                }

                transactionDetail.Amount += item.ExtendedPrice;
            }

            return details;
        }

        /// <summary>
        /// Generates the receipt.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        protected InteractionComponent GenerateReceipt( RockContext rockContext )
        {
            var interactionService = new InteractionService( rockContext );
            var receipt = new InteractionComponent();

            new InteractionComponentService( rockContext ).Add( receipt );

            receipt.ChannelId = InteractionChannelCache.Get( com.shepherdchurch.CubeDown.SystemGuid.InteractionChannel.CUBE_DOWN_RECEIPTS.AsGuid() ).Id;
            receipt.Name = "Temporary Receipt";
            receipt.ComponentSummary = string.Format( "Total {0:c}", Cart.Total );

            if ( Customer != null )
            {
                receipt.EntityId = Customer.PrimaryAliasId;
            }

            foreach ( var item in Cart.Items )
            {
                var interaction = new Interaction
                {
                    EntityId = null,
                    Operation = "Buy",
                    InteractionDateTime = RockDateTime.Now,
                    InteractionData = item.ToJson()
                };

                if ( item.Quantity == 1 )
                {
                    interaction.InteractionSummary = item.Name;
                }
                else
                {
                    interaction.InteractionSummary = string.Format( "{0} (qty {1})", item.Name, item.Quantity );
                }

                interactionService.Add( interaction );
            }

            rockContext.SaveChanges();

            receipt = new InteractionComponentService( rockContext ).Get( receipt.Id );

            var receiptCode = ReceiptHelper.EncodeReceiptCode( receipt );
            Cart.ReceiptCode = receiptCode;

            receipt.Name = string.Format( "Receipt #{0}", receiptCode );
            receipt.ComponentData = Cart.ToJson();

            rockContext.SaveChanges();


            return receipt;
        }

        /// <summary>
        /// Shows the receipt panel.
        /// </summary>
        protected void ShowReceiptPanel()
        {
            var items = new List<KeyValuePair<string, string>>();

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );
            mergeFields.AddOrReplace( "Person", Customer );
            mergeFields.AddOrReplace( "Cart", Cart );

            lReceiptSummary.Text = GetAttributeValue( "ReceiptTemplate" ).ResolveMergeFields( mergeFields, CurrentPerson );

            if ( Customer != null )
            {
                if ( !string.IsNullOrWhiteSpace( Customer.Email ) && Customer.IsEmailActive && !string.IsNullOrWhiteSpace( GetAttributeValue( "ReceiptEmail" ) ) )
                {
                    items.Add( new KeyValuePair<string, string>( string.Format( "Email {0}", Customer.Email ), string.Format( "email:{0}", Customer.Email ) ) );
                }

                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "ReceiptSMSNumber" ) ) )
                {
                    foreach ( var sms in Customer.PhoneNumbers.Where( n => n.IsMessagingEnabled ) )
                    {
                        string smsNumber = sms.Number;
                        if ( !string.IsNullOrWhiteSpace( sms.CountryCode ) )
                        {
                            smsNumber = "+" + sms.CountryCode + sms.Number;
                        }

                        items.Add( new KeyValuePair<string, string>( string.Format( "SMS {0}", sms.NumberFormatted ), string.Format( "sms:{0}", smsNumber ) ) );
                    }
                }
            }

            items.Add( new KeyValuePair<string, string>( "No Receipt", string.Empty ) );

            rptrReceiptOptions.DataSource = items.Select( kvp => new
            {
                Title = kvp.Key,
                kvp.Value
            } );
            rptrReceiptOptions.DataBind();

            pnlPay.Visible = false;
            pnlReceipt.Visible = true;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the BlockUpdated event of the Terminal control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Terminal_BlockUpdated( object sender, EventArgs e )
        {
            ResetTerminal();
        }

        /// <summary>
        /// Handles the Click event of the lbResetCart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbResetCart_Click( object sender, EventArgs e )
        {
            ResetTerminal();
        }

        /// <summary>
        /// Handles the Click event of the lbViewCart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbViewCart_Click( object sender, EventArgs e )
        {
            pnlItems.Visible = false;
            pnlCart.Visible = true;

            lbPayNow.Enabled = Cart.Items.Any();

            rptrCartItems.DataSource = Cart.Items;
            rptrCartItems.DataBind();
        }

        /// <summary>
        /// Handles the Click event of the lbBackToItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbBackToItems_Click( object sender, EventArgs e )
        {
            pnlCart.Visible = false;
            pnlItems.Visible = true;
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptrCartItems control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptrCartItems_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var itemGuid = e.CommandArgument.ToString().AsGuid();
            var item = Cart.Items.FirstOrDefault( i => i.Guid == itemGuid );

            if ( item == null )
            {
                return;
            }

            switch ( e.CommandName )
            {
                case "IncreaseQuantity":
                    item.Quantity += 1;
                    break;

                case "DecreaseQuantity":
                    item.Quantity -= 1;
                    break;

                case "Remove":
                    Cart.Items.Remove( item );
                    break;
            }

            Cart.Calculate();
            lbPayNow.Enabled = Cart.Items.Any();

            rptrCartItems.DataSource = Cart.Items;
            rptrCartItems.DataBind();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptrCartItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptrCartItems_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var lbDecreaseQuantity = e.Item.FindControl( "lbDecreaseQuantity" ) as LinkButton;

            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var item = ( ShoppingCartItem ) e.Item.DataItem;

                if ( item.Quantity > 1 )
                {
                    lbDecreaseQuantity.RemoveCssClass( "disabled" );
                }
                else
                {
                    lbDecreaseQuantity.AddCssClass( "disabled" );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSelectCustomer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSelectCustomer_Click( object sender, EventArgs e )
        {
            mdlSelectCustomer.Show();
        }

        /// <summary>
        /// Handles the Click event of the lbClearCustomer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbClearCustomer_Click( object sender, EventArgs e )
        {
            CustomerId = null;
        }

        /// <summary>
        /// Handles the SelectedPerson event of the psCustomer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void psCustomer_SelectedPerson( object sender, EventArgs e )
        {
            CustomerId = psCustomer.PersonId;
            mdlSelectCustomer.Hide();
        }

        /// <summary>
        /// Handles the Click event of the lbPayNow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbPayNow_Click( object sender, EventArgs e )
        {
            pnlCart.Visible = false;
            pnlPay.Visible = true;
            nbSwipeErrors.Text = string.Empty;

            //
            // If they are buying a "free" cart, just save it and jump to the receipt.
            //
            if ( Cart.Total == 0 )
            {
                //
                // Generate the receipt.
                //
                using ( var rockContext = new RockContext() )
                {
                    var receipt = GenerateReceipt( rockContext );
                }

                ShowReceiptPanel();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbBackToCart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbBackToCart_Click( object sender, EventArgs e )
        {
            pnlPay.Visible = false;
            pnlCart.Visible = true;
        }

        /// <summary>
        /// Handles the Swipe event of the csPayWithCard control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SwipeEventArgs"/> instance containing the event data.</param>
        protected void csPayWithCard_Swipe( object sender, SwipeEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                try
                {
                    var swipeInfo = e.PaymentInfo;
                    var person = Customer;

                    if ( person == null )
                    {
                        person = new PersonAliasService( rockContext ).GetPerson( GetAttributeValue( "GuestCustomer" ).AsGuid() );

                        if ( person == null )
                        {
                            nbSwipeErrors.Text = "No guest customer configured. Transaction not processed.";
                            return;
                        }
                    }

                    //
                    // Get the gateway to use.
                    //
                    FinancialGateway financialGateway = null;
                    GatewayComponent gateway = null;
                    Guid? gatewayGuid = GetAttributeValue( "CreditCardGateway" ).AsGuidOrNull();
                    if ( gatewayGuid.HasValue )
                    {
                        financialGateway = new FinancialGatewayService( rockContext ).Get( gatewayGuid.Value );

                        if ( financialGateway != null )
                        {
                            financialGateway.LoadAttributes( rockContext );
                        }

                        gateway = financialGateway.GetGatewayComponent();
                    }

                    if ( gateway == null )
                    {
                        nbSwipeErrors.Text = "Invalid gateway provided. Please provide a gateway. Transaction not processed.";
                        return;
                    }

                    swipeInfo.Amount = Cart.Total;

                    //
                    // Process the transaction.
                    //
                    string errorMessage = string.Empty;
                    var transaction = gateway.Charge( financialGateway, swipeInfo, out errorMessage );

                    if ( transaction == null )
                    {
                        nbSwipeErrors.Text = String.Format( "An error occurred while process this transaction. Message: {0}", errorMessage );
                        return;
                    }

                    //
                    // Set some common information about the transaction.
                    //
                    transaction.AuthorizedPersonAliasId = person.PrimaryAliasId;
                    transaction.TransactionDateTime = RockDateTime.Now;
                    transaction.FinancialGatewayId = financialGateway.Id;
                    transaction.TransactionTypeValueId = DefinedValueCache.Get( GetAttributeValue( "TransactionType" ) ).Id;
                    transaction.SourceTypeValueId = DefinedValueCache.Get( GetAttributeValue( "Source" ) ).Id;
                    transaction.Summary = swipeInfo.Comment1;

                    //
                    // Ensure we have payment details.
                    //
                    if ( transaction.FinancialPaymentDetail == null )
                    {
                        transaction.FinancialPaymentDetail = new FinancialPaymentDetail();
                    }
                    transaction.FinancialPaymentDetail.SetFromPaymentInfo( swipeInfo, gateway, rockContext );

                    //
                    // Setup the transaction details to credit the correct account.
                    //
                    GetTransactionDetails( rockContext ).ForEach( d => transaction.TransactionDetails.Add( d ) );

                    var batchService = new FinancialBatchService( rockContext );

                    //
                    // Get the batch 
                    //
                    var batch = batchService.Get(
                        GetAttributeValue( "BatchNamePrefix" ),
                        swipeInfo.CurrencyTypeValue,
                        swipeInfo.CreditCardTypeValue,
                        transaction.TransactionDateTime.Value,
                        financialGateway.GetBatchTimeOffset() );

                    var batchChanges = new History.HistoryChangeList();

                    if ( batch.Id == 0 )
                    {
                        batchChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Batch" );
                        History.EvaluateChange( batchChanges, "Batch Name", string.Empty, batch.Name );
                        History.EvaluateChange( batchChanges, "Status", null, batch.Status );
                        History.EvaluateChange( batchChanges, "Start Date/Time", null, batch.BatchStartDateTime );
                        History.EvaluateChange( batchChanges, "End Date/Time", null, batch.BatchEndDateTime );
                    }

                    //
                    // Update the control amount.
                    //
                    decimal newControlAmount = batch.ControlAmount + transaction.TotalAmount;
                    History.EvaluateChange( batchChanges, "Control Amount", batch.ControlAmount.FormatAsCurrency(), newControlAmount.FormatAsCurrency() );
                    batch.ControlAmount = newControlAmount;

                    //
                    // Add the transaction to the batch.
                    //
                    transaction.BatchId = batch.Id;
                    batch.Transactions.Add( transaction );

                    //
                    // Generate the receipt.
                    //
                    int receiptId;
                    using ( var rockContext2 = new RockContext() )
                    {
                        var receipt = GenerateReceipt( rockContext2 );
                        receiptId = receipt.Id;
                    }

                    //
                    // Update each transaction detail to reference the receipt.
                    //
                    foreach ( var transactionDetail in transaction.TransactionDetails )
                    {
                        transactionDetail.EntityTypeId = EntityTypeCache.Get( typeof( InteractionComponent ) ).Id;
                        transactionDetail.EntityId = receiptId;
                    }

                    rockContext.WrapTransaction( () =>
                    {
                        rockContext.SaveChanges();

                        HistoryService.SaveChanges(
                            rockContext,
                            typeof( FinancialBatch ),
                            Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                            batch.Id,
                            batchChanges
                        );
                    } );

                    ShowReceiptPanel();
                }
                catch ( Exception ex )
                {
                    nbSwipeErrors.Text = String.Format( "An error occurred while process this transaction. Message: {0}", ex.Message );
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptrReceiptOptions control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptrReceiptOptions_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var destination = e.CommandArgument.ToStringSafe();
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );

            mergeFields.AddOrReplace( "Person", Customer );
            mergeFields.AddOrReplace( "Cart", Cart );

            if ( destination.StartsWith( "email:" ) )
            {
                var emailMessage = new RockEmailMessage( GetAttributeValue( "ReceiptEmail" ).AsGuid() )
                {
                    AppRoot = GlobalAttributesCache.Get().GetValue( "InternalApplicationRoot" ),
                    CreateCommunicationRecord = false
                };

                emailMessage.AddRecipient( new RecipientData( destination.Substring( 6 ), mergeFields ) );

                emailMessage.Send();
            }
            else if ( destination.StartsWith( "sms:" ) )
            {
                var smsMessage = new RockSMSMessage
                {
                    FromNumber = DefinedValueCache.Get( GetAttributeValue( "ReceiptSMSNumber" ).AsGuid() ),
                    Message = GetAttributeValue( "SMSReceiptTemplate" ),
                    AppRoot = GlobalAttributesCache.Get().GetValue( "InternalApplicationRoot" )
                };

                smsMessage.AddRecipient( new RecipientData( destination.Substring( 4 ), mergeFields ) );

                List<string> errorMessages;
                bool result = smsMessage.Send( out errorMessages );
            }

            ResetTerminal();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdlCustomItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdlCustomItem_SaveClick( object sender, EventArgs e )
        {
            mdlCustomItem.Hide();
            mdlCustomItemName.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdlCustomItemName control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdlCustomItemName_SaveClick( object sender, EventArgs e )
        {
            var name = !string.IsNullOrWhiteSpace( tbCustomItemName.Text ) ? tbCustomItemName.Text : tbCustomItemName.Placeholder;

            Cart.Items.Add( new ShoppingCartItem
            {
                Name = name,
                Quantity = 1,
                Price = tbCustomItemAmount.Text.AsDecimal(),
                IsTaxable = cbCustomItemTaxable.Checked
            } );

            Cart.Calculate();

            mdlCustomItemName.Hide();
        }

        #endregion
    }
}
