using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Web.UI;

namespace com.shepherdchurch.CubeDown.Web.UI.Controls
{
    public class KioskPersonSearch : WebControl
    {
        #region Events

        public event EventHandler SelectedPerson;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int? PersonId
        {
            get
            {
                EnsureChildControls();

                return _hfSelectedPersonId.Value.AsIntegerOrNull();
            }
        }

        #endregion

        #region Child Controls

        protected HiddenField _hfSelectedPersonId;

        protected LinkButton _lbSelectPerson;

        protected TextBox _tbSearch;

        protected ScreenKeyboard _kbSearch;

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            EnsureChildControls();

            if ( Page is RockPage rockPage )
            {
                rockPage.AddScriptLink( "~/Plugins/com_shepherdchurch/CubeDown/Scripts/kiosk-person-search.js" );
                rockPage.AddCSSLink( "~/Plugins/com_shepherdchurch/CubeDown/Styles/kiosk-person-search.css" );
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _hfSelectedPersonId = new HiddenField
            {
                ID = ID + "_hfSelectedPersonId"
            };
            Controls.Add( _hfSelectedPersonId );

            _lbSelectPerson = new LinkButton
            {
                ID = ID + "_lbSelectPerson",
                CssClass = "hidden js-click"
            };
            _lbSelectPerson.Click += lbSelectPerson_Click;
            Controls.Add( _lbSelectPerson );

            _tbSearch = new Rock.Web.UI.Controls.RockTextBox
            {
                ID = ID + "_tbSearch",
                CssClass = "margin-b-lg js-text-field"
            };
            Controls.Add( _tbSearch );

            _kbSearch = new ScreenKeyboard
            {
                ID = ID + "_kbSearch",
                KeyboardType = ScreenKeyboardType.Qwerty,
                ControlToTarget = _tbSearch.ID
            };
            Controls.Add( _kbSearch );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );
            
            string script = $"Rock.controls.sc_kioskPersonSearch.initialize({{ id: '{ ClientID }' }});";

            ScriptManager.RegisterStartupScript( this, GetType(), $"kiosk-person-search-init-{ ClientID }", script, true );

        }

        /// <summary>
        /// Renders the control to the specified HTML writer.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Id, ClientID );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, $"kiosk-person-search { CssClass }" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            {
                _hfSelectedPersonId.RenderControl( writer );
                _lbSelectPerson.RenderControl( writer );
                _tbSearch.RenderControl( writer );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "keyboard-container" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                {
                    _kbSearch.RenderControl( writer );
                }
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.H2 );
                {
                    writer.WriteEncodedText( "Results" );
                }
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "person-search-results row" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.RenderEndTag();
            }
            writer.RenderEndTag();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the lbSelectPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void lbSelectPerson_Click( object sender, EventArgs e )
        {
            _tbSearch.Text = string.Empty;
            SelectedPerson?.Invoke( this, new EventArgs() );
        }

        #endregion
    }
}
