<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Terminal.ascx.cs" Inherits="RockWeb.Plugins.com_shepherdchurch.CubeDown.Terminal" %>
<%@ Register Namespace="com.shepherdchurch.CubeDown.Web.UI.Controls" Assembly="com.shepherdchurch.CubeDown" TagPrefix="CD" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbBlockConfigErrors" runat="server" NotificationBoxType="Danger" />

        <asp:Panel ID="pnlItems" runat="server" CssClass="pos pos-items">
            <div class="pos-header clearfix">
                <asp:LinkButton ID="lbResetCart" runat="server" CssClass="btn btn-warning btn-lg reset-cart" OnClick="lbResetCart_Click">
                    <i class="fa fa-times"></i> Clear Cart
                </asp:LinkButton>
                <asp:LinkButton ID="lbViewCart" runat="server" CssClass="btn btn-success btn-lg view-cart" OnClick="lbViewCart_Click">
                    <span class="amount"><%= Cart.Subtotal.ToString( "c" ) %></span>
                    <i class="fa fa-shopping-cart"></i>
                </asp:LinkButton>
            </div>

            <div class="pos-body">
                <asp:Literal ID="lItemsItemList" runat="server" />
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlCart" runat="server" CssClass="pos pos-cart" Visible="false">
            <div class="pos-header clearfix">
                <asp:LinkButton ID="lbBackToItems" runat="server" CssClass="btn btn-info btn-lg back-to-items" OnClick="lbBackToItems_Click">
                    <i class="fa fa-arrow-left"></i> Back To Items
                </asp:LinkButton>
                <div class="cart-total">
                    Subtotal <span class="amount"><%= Cart.Subtotal.ToString( "c" ) %></span>
                </div>
            </div>

            <div class="pos-body">
                <div class="pos-body-grid">
                    <asp:Repeater ID="rptrCartItems" runat="server" OnItemCommand="rptrCartItems_ItemCommand" OnItemDataBound="rptrCartItems_ItemDataBound">
                        <HeaderTemplate>
                            <table class="table table-bordered pos-table-items">
                                <thead>
                                    <tr>
                                        <th class="col-title col-sm-6">Name</th>
                                        <th class="col-quantity col-sm-3">Quantity</th>
                                        <th class="col-price col-sm-1">Price</th>
                                        <th class="col-extended-price col-sm-1">Extended<br />Price</th>
                                        <th class="col-remove col-sm-1">Remove</th>
                                    </tr>
                                </thead>
                            <tbody>
                        </HeaderTemplate>

                        <ItemTemplate>
                            <tr>
                                <td class="col-name"><%# Eval( "Name" ) %></td>
                                <td class="col-quantity">
                                    <span class="group">
                                        <asp:LinkButton ID="lbDecreaseQuantity" runat="server" CssClass="btn btn-sm" CommandName="DecreaseQuantity" CommandArgument='<%# Eval("Guid") %>'>
                                            <i class="fa fa-arrow-down"></i>
                                        </asp:LinkButton>
                                        <span class="btn-sm"><%# Eval( "Quantity" ) %></span>
                                        <asp:LinkButton ID="lbIncreaseQuantity" runat="server" CssClass="btn btn-sm" CommandName="IncreaseQuantity" CommandArgument='<%# Eval("Guid") %>'>
                                            <i class="fa fa-arrow-up"></i>
                                        </asp:LinkButton>
                                    </span>
                                </td>
                                <td class="col-price"><%# Eval( "Price", "{0:c}" ) %></td>
                                <td class="col-extended-price"><%# Eval( "ExtendedPrice", "{0:c}" ) %></td>
                                <td class="col-remove">
                                    <asp:LinkButton ID="lbDelete" runat="server" CssClass="btn btn-danger btn-sm" CommandName="Remove" CommandArgument='<%# Eval("Guid") %>'>
                                        <i class="fa fa-times"></i>
                                    </asp:LinkButton>
                                </td>
                            </tr>
                        </ItemTemplate>

                        <FooterTemplate>
                                </tbody>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                </div>

                <div class="customer-container">
                    <strong>Customer</strong>: <span class="name"><%= Customer != null ? Customer.FullName : "Guest" %></span>

                    <div class="btn-group margin-l-md">
                        <asp:LinkButton ID="lbSelectCustomer" runat="server" CssClass="btn btn-default" OnClick="lbSelectCustomer_Click">
                            <i class="fa fa-fw fa-pencil"></i>
                        </asp:LinkButton>
                        <asp:LinkButton ID="lbClearCustomer" runat="server" CssClass="btn btn-default" OnClick="lbClearCustomer_Click">
                            <i class="fa fa-fw fa-times"></i>
                        </asp:LinkButton>
                    </div>
                </div>

                <div class="pay-container">
                    <asp:LinkButton ID="lbPayNow" runat="server" CssClass="btn btn-primary" Text="Pay Now" OnClick="lbPayNow_Click" />
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlPay" runat="server" CssClass="pos pos-pay" Visible="false">
            <div class="pos-header clearfix">
                <asp:LinkButton ID="lbBackToCart" runat="server" CssClass="btn btn-info btn-lg back-to-cart" OnClick="lbBackToCart_Click">
                    <i class="fa fa-arrow-left"></i> Back To Cart
                </asp:LinkButton>

                <div class="cart-total">
                    Total <span class="amount"><%= Cart.Total.ToString( "c" ) %></span>
                </div>
            </div>

            <div class="pos-body">
                <Rock:NotificationBox ID="nbSwipeErrors" runat="server" NotificationBoxType="Danger" />

                <div class="amounts margin-b-lg">
                    <table>
                        <tbody>
                            <tr>
                                <td>Subtotal:</td>
                                <td><%= Cart.Subtotal.ToString( "c" ) %></td>
                            </tr>
                            <tr>
                                <td>Tax:</td>
                                <td><%= Cart.Tax.ToString( "c" ) %></td>
                            </tr>
                            <tr>
                                <td>Total:</td>
                                <td><%= Cart.Total.ToString( "c" ) %></td>
                            </tr>
                        </tbody>
                    </table>
                </div>

                <CD:CardSwiper ID="csPayWithCard" runat="server" CssClass="margin-t-md" OnSwipe="csPayWithCard_Swipe" />
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlReceipt" runat="server" CssClass="pos pos-receipt" Visible="false">
            <div class="pos-header clearfix">
                <a href="#" class="btn btn-lg btn-link invisible">&nbsp;</a>
            </div>

            <div class="pos-body">
                <div class="pos-receipt-summary">
                    <asp:Literal ID="lReceiptSummary" runat="server" />
                </div>

                <div class="pos-receipt-buttons">
                    <asp:Repeater ID="rptrReceiptOptions" runat="server" OnItemCommand="rptrReceiptOptions_ItemCommand">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbReceipt" runat="server" CssClass="btn btn-primary" CommandName="Receipt" CommandArgument='<%# Eval("Value") %>'><%# Eval("Title") %></asp:LinkButton>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </asp:Panel>

        <Rock:ModalDialog ID="mdlCustomItem" runat="server" SaveButtonText="Next" Title="Add Custom Item" ValidationGroup="AddCustomItem" OnSaveClick="mdlCustomItem_SaveClick">
            <Content>
                <asp:ValidationSummary ID="vsAddCustomItem" runat="server" ValidationGroup="AddCustomItem" CssClass="alert alert-warning" />
                <div class="row">
                    <div class="col-sm-6">
                        <Rock:RockTextBox ID="tbCustomItemAmount" runat="server" Required="true" Label="Amount" PrependText="$" ValidationGroup="AddCustomItem"/>
                        <asp:RegularExpressionValidator ID="reAmountValidator" runat="server" ControlToValidate="tbCustomItemAmount" ValidationExpression="^[0-9]+(.[0-9]{1,2}){0,1}$" Display="Dynamic" CssClass="validation-error help-inline" ValidationGroup="AddCustomItem" ErrorMessage="Please specify a valid amount." />

                        <Rock:RockCheckBox ID="cbCustomItemTaxable" runat="server" Label="Taxable" SelectedIconCssClass="far fa-2x fa-check-square" UnSelectedIconCssClass="far fa-2x fa-square" />
                    </div>
                    <div class="col-sm-6">
                        <div class="text-center">
                            <CD:ScreenKeyboard ID="skCustomItemAmount" runat="server" ControlToTarget="tbCustomItemAmount" KeyboardType="NumberPad" />
                        </div>
                    </div>
                </div>

            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="mdlCustomItemName" runat="server" SaveButtonText="Add" Title="Add Custom Item" ValidationGroup="AddCustomItemName" OnSaveClick="mdlCustomItemName_SaveClick">
            <Content>
                <Rock:RockTextBox ID="tbCustomItemName" runat="server" Required="false" Label="Title" Placeholder="Generic Sale" ValidationGroup="AddCustomItemName"/>
                <div class="text-center">
                    <CD:ScreenKeyboard ID="skCustomItemName" runat="server" ControlToTarget="tbCustomItemName" KeyboardType="Qwerty" />
                </div>
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="mdlSelectCustomer" runat="server">
            <Content>
                <CD:KioskPersonSearch ID="psCustomer" runat="server" OnSelectedPerson="psCustomer_SelectedPerson" />
            </Content>
        </Rock:ModalDialog>


<script>
    Sys.Application.add_load(function () {
        var $qrcode = $('.qrcode');

        $qrcode.each(function () {
            var width = $(this).data('width') || 128;
            var height = $(this).data('height') || 128;
            var dark = $(this).data('dark') || '#000000';
            var light = $(this).data('light') || '#ffffff';
            var text = $(this).data('text') || '';

            if (text !== '') {
                new QRCode($(this).get(0), {
                    text: text,
                    width: width,
                    height: height,
                    colorDark: dark,
                    colorLight: light,
                    correctLevel: QRCode.CorrectLevel.M
                });
            }
        });
    });
</script>

    </ContentTemplate>
</asp:UpdatePanel>
