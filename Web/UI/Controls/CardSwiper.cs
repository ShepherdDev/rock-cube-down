using System;
using System.Web.UI;
using Rock.Financial;
using Rock.Web.UI;

namespace com.shepherdchurch.CubeDown.Web.UI.Controls
{
    public class CardSwiper : Control, IPostBackEventHandler
    {
        #region Events

        /// <summary>
        /// Occurs when a swipe has occurred.
        /// </summary>
        public event EventHandler<SwipeEventArgs> Swipe;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the CSS class.
        /// </summary>
        /// <value>
        /// The CSS class.
        /// </value>
        public string CssClass
        {
            get
            {
                return ( string ) ViewState["CssClass"] ?? string.Empty;
            }
            set
            {
                ViewState["CssClass"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the card icons to be displayed.
        /// </summary>
        /// <value>
        /// The card icons to be displayed.
        /// </value>
        public SupportedCardIcons CardIcons
        {
            get
            {
                return ( ( SupportedCardIcons? ) ViewState["CardIcons"] ) ?? SupportedCardIcons.All;
            }
            set
            {
                ViewState["CardIcons"] = value;
            }
        }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( Page is RockPage rockPage )
            {
                rockPage.AddScriptLink( "~/Plugins/com_shepherdchurch/CubeDown/Scripts/card-swiper.js" );
                rockPage.AddCSSLink( "~/Plugins/com_shepherdchurch/CubeDown/Styles/card-swiper.css" );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            string script = $"Rock.controls.sc_cardSwiper.initialize({{ id: '{ ClientID }', postback: '{ UniqueID }' }});";

            ScriptManager.RegisterStartupScript( this, GetType(), $"card-swiper-init-{ ClientID }", script, true );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Id, ClientID );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, $"card-swiper { CssClass }" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "supported-cards" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                {
                    if ( CardIcons.HasFlag( SupportedCardIcons.Visa ) )
                    {
                        RenderCreditCard( writer, "fa-cc-visa" );
                    }

                    if ( CardIcons.HasFlag( SupportedCardIcons.Mastercard ) )
                    {
                        RenderCreditCard( writer, "fa-cc-mastercard" );
                    }

                    if ( CardIcons.HasFlag( SupportedCardIcons.AmericanExpress ) )
                    {
                        RenderCreditCard( writer, "fa-cc-amex" );
                    }

                    if ( CardIcons.HasFlag( SupportedCardIcons.Discover ) )
                    {
                        RenderCreditCard( writer, "fa-cc-discover" );
                    }
                }
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Src, ResolveClientUrl( "~/Assets/Images/Kiosk/card_swipe.png" ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Img );
                writer.RenderEndTag();

            }
            writer.RenderEndTag();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Renders the credit card icon.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="cssClass">The CSS class.</param>
        protected void RenderCreditCard( HtmlTextWriter writer, string cssClass )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Class, $"fa { cssClass } fa-2x" );
            writer.RenderBeginTag( HtmlTextWriterTag.I );
            writer.RenderEndTag();
        }

        #endregion

        #region IPostBackEventHandler

        /// <summary>
        /// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">A <see cref="T:System.String" /> that represents an optional event argument to be passed to the event handler.</param>
        public void RaisePostBackEvent( string eventArgument )
        {
            var swipeInfo = new SwipePaymentInfo( eventArgument );

            Swipe?.Invoke( this, new SwipeEventArgs( swipeInfo ) );
        }

        #endregion
    }

    /// <summary>
    /// The supported card icons to display in the Card Swiper control.
    /// </summary>
    [Flags]
    public enum SupportedCardIcons
    {
        None = 0,
        Visa = 1,
        Mastercard = 2,
        AmericanExpress = 4,
        Discover = 8,
        All = Visa | Mastercard | AmericanExpress | Discover
    }

    public class SwipeEventArgs : EventArgs
    {
        public SwipePaymentInfo PaymentInfo { get; private set; }

        public SwipeEventArgs( SwipePaymentInfo paymentInfo )
        {
            PaymentInfo = paymentInfo;
        }
    }
}
