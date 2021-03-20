<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ViewReceiptLava.ascx.cs" Inherits="RockWeb.Plugins.com_shepherdchurch.CubeDown.ViewReceiptLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbNotFound" runat="server" NotificationBoxType="Warning" Visible="false">
            We couldn't find that receipt or it may have expired.
        </Rock:NotificationBox>

        <asp:Literal ID="lReceipt" runat="server" />
    </ContentTemplate>
</asp:UpdatePanel>
