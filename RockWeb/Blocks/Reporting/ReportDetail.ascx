﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReportDetail.ascx.cs" Inherits="RockWeb.Blocks.Reporting.ReportDetail" %>
<asp:UpdatePanel ID="upReport" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <asp:HiddenField ID="hfReportId" runat="server" />

                <div id="pnlEditDetails" runat="server">
                    <div class="panel panel-block">
                        <div class="panel-heading">
                            <h1 class="panel-title"><i class="fa fa-list-alt"></i> <asp:Literal ID="lActionTitle" runat="server" /></h1>
                        </div>
                        <div class="panel-body">

                            <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                            <asp:CustomValidator ID="cvSecurityError" runat="server" Display="None"></asp:CustomValidator>

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Report, Rock" PropertyName="Name" Label="Name" />
                                </div>
                                <div class="col-md-6">
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Report, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:CategoryPicker ID="cpCategory" runat="server" Required="true" EntityTypeName="Rock.Model.Report" Label="Category" />
                                    <Rock:EntityTypePicker ID="etpEntityType" runat="server" Label="Applies To" Required="true" AutoPostBack="true" OnSelectedIndexChanged="etpEntityType_SelectedIndexChanged" EnhanceForLongLists="true" />
                                    <Rock:DataViewItemPicker ID="dvpDataView" runat="server" Label="Data View" Required="false" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:KeyValueList ID="kvSortFields" runat="server" Label="Sorting" />
                                </div>
                            </div>

                            <Rock:PanelWidget runat="server" ID="pwAdvanced" Title="Advanced Settings">
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:NumberBox ID="nbFetchTop" runat="server" NumberType="Integer" Required="false" Label="Resulting Row Limit" MinimumValue="0" MaxLength="9"
                                        Help="Limits the number of rows returned in the report. Leave blank to show all rows." />
                                        <Rock:RockTextBox ID="tbQueryHint" runat="server" Label="Query Hint" Help="The Query Hint to apply to the query that is executed on the database server. These can sometimes improve the performance of the report, but could also make it worse. Examples are: <code>OPTIMIZE FOR UNKNOWN</code> and <code>RECOMPILE</code>." />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:ValueList ID="vMergeFields" runat="server" Label="Communication Merge Fields"
                                            Help="When creating a new communication from the report, fields from the report can be used as merge fields on the communication. Select any of the fields that you'd like to be available for the communication. If the same recipient has multiple results in this report, each result will be included in an 'AdditionalFields' list. These can be accessed using Lava in the communication. For example: {% for field in AdditionalFields %}{{ field.FieldName }}{% endfor %}" />
                                        <Rock:ValueList ID="vRecipientFields" runat="server" Label="Communication Recipient Fields"
                                            Help="Fields from the report that should be used to determine the recipient for a communication. Note that only fields that can be used as a recipient field can be selected. If left blank, and this is a Person report, it will assume the 'Id' contains the recipient's person Id."/>
                                    </div>
                                </div>
                            </Rock:PanelWidget>

                            <section class="panel panel-widget">
                                <header class="panel-heading clearfix">
                                    <h3 class="panel-title pull-left">Fields</h3>
                                    <div class="pull-right">
                                        <asp:LinkButton runat="server" ID="btnAddField" CssClass="btn btn-xs btn-action" CausesValidation="false" OnClick="btnAddField_Click"><i class="fa fa-plus"></i> Add Field</asp:LinkButton>
                                    </div>
                                </header>
                                <div class="panel-body panel-widget-sort-container">
                                    <asp:PlaceHolder runat="server" ID="phReportFields" ViewStateMode="Disabled" />
                                </div>
                            </section>

                            <div class="actions">
                                <asp:LinkButton ID="btnSave" runat="server" Text="Save" data-shortcut-key="s" ToolTip="Alt+s" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" data-shortcut-key="c" ToolTip="Alt+c" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                            </div>
                        </div>
                    </div>
                </div>

                <div id="pnlViewDetails" runat="server">
                    <div class="panel panel-block">
                        <div class="panel-heading">
                            <h1 class="panel-title"><i class="fa fa-list-alt"></i> <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
                        </div>
                        <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
                        <div class="panel-body">
                            <div class="clearfix">
                                <div class="pull-right ml-1 mb-1">
                                    <Rock:HighlightLabel runat="server" ID="hlTimeToRun" />
                                    <Rock:HighlightLabel runat="server" ID="hlRunSince" />
                                    <Rock:HighlightLabel runat="server" ID="hlLastRun" />
                                </div>
                                <div class="description">
                                    <asp:Literal ID="lReportDescription" runat="server"></asp:Literal>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <asp:Literal ID="lCategory" runat="server" />
                                </div>
                                <div class="col-md-6">
                                    <asp:Literal ID="lDataView" runat="server" />
                                </div>
                            </div>

                            <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                            <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" Visible="false" />

                            <div class="actions">
                                <asp:LinkButton ID="btnEdit" runat="server" data-shortcut-key="e" AccessKey="m" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                                <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                                <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" />
                                <div class="pull-right">
                                    <asp:LinkButton ID="btnCopy" runat="server" Tooltip="Copy Report" CssClass="btn btn-default btn-sm btn-square" Text="<i class='fa fa-clone'></i>" OnClick="btnCopy_Click" />
                                    <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-square btn-security" />
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="panel panel-block">
                        <div class="panel-heading">
                            <h1 class="panel-title"><i class="fa fa-table"></i> Report Data</h1>

                            <div class="panel-labels">
                                <asp:LinkButton ID="btnToggleResults" runat="server" CssClass="btn btn-default btn-xs" OnClick="btnToggleResults_Click" />
                            </div>
                        </div>

                        <asp:Panel ID="pnlResultsGrid" runat="server">
                            <div class="panel-body">
                                <div class="grid grid-panel">
                                    <Rock:Grid ID="gReport" runat="server" AllowSorting="true" EmptyDataText="No Results" />
                                </div>
                            </div>
                        </asp:Panel>
                    </div>
                 </div>

        </asp:Panel>
        <script>

            Sys.Application.add_load(function () {
                var fixHelper = function (e, ui) {
                    ui.children().each(function () {
                        $(this).width($(this).width());
                    });
                    return ui;
                };

                // javascript to make the Reorder buttons work on the panel-widget controls
                $('.panel-widget-sort-container').sortable({
                    helper: fixHelper,
                    handle: '.panel-widget-reorder',
                    containment: 'parent',
                    tolerance: 'pointer',
                    start: function (event, ui) {
                        {
                            var start_pos = ui.item.index();
                            ui.item.data('start_pos', start_pos);
                        }
                    },
                    update: function (event, ui) {
                        {
                            $('#' + '<%=btnSave.ClientID %>').addClass('disabled');
                            var newItemIndex = $(ui.item).prevAll('.panel-widget').length;
                            var postbackArg = 're-order-panel-widget:' + ui.item.attr('id') + ';' + newItemIndex;
                            window.location = "javascript:__doPostBack('<%=upReport.ClientID %>', '" +  postbackArg + "')";
                        }
                    }
                });

                function htmlEncode(value) {
                    // from http://stackoverflow.com/questions/1219860/html-encoding-in-javascript-jquery
                    // create a in-memory div, set it's inner text(which jQuery automatically encodes)
                    // then grab the encoded contents back out.  The div never exists on the page.
                    return $('<div/>').text(value).html();
                }


                // javascript to set the widget panel title based on the defined column header text when collapsed
                $('.panel-widget .panel-heading').on('click', function (e, data) {
                    if ($(this).find('.fa-chevron-down').length) {
                        var title = $(this).closest('section').find('.js-column-header-textbox').val();
                        var reportFieldGuid = $(this).closest('section').find('.js-report-field-guid').val();

                        // set hidden value of title
                        $(this).find('.js-header-title-hidden').val(htmlEncode(title));

                        // set displayed text of title
                        $(this).find('.js-header-title').text(title);

                        // update displayed sorting field names to match updated title
                        var $kvSortFields = $('#<%=kvSortFields.ClientID %>');
                        var $vMergeFields = $('#<%=vMergeFields.ClientID %>');
                        var $vRecipientFields = $('#<%=vRecipientFields.ClientID %>');

                        $kvSortFields.find('.key-value-key').find('option[value="' + reportFieldGuid + '"]').text(title);
                        $vMergeFields.find('.js-value-list-input').find('option[value="' + reportFieldGuid + '"]').text(title);
                        $vRecipientFields.find('.js-value-list-input').find('option[value="' + reportFieldGuid + '"]').text(title);

                        // update the HTML for when the next sorting field is added
                        var valueHtml = $kvSortFields.find('.js-value-html').val();
                        var $fakeDiv = $('<div/>').append(valueHtml);
                        $fakeDiv.find('.key-value-key').find('option[value="' + reportFieldGuid + '"]').text(title);
                        var updatedValueHtml = $fakeDiv.html();
                        $kvSortFields.find('.js-value-html').val(updatedValueHtml);

                        var valueHtml2 = $vMergeFields.find('.js-value-list-html').val();
                        var $fakeDiv2 = $('<div/>').append(valueHtml2);
                        $fakeDiv2.find('.js-value-list-input').find('option[value="' + reportFieldGuid + '"]').text(title);
                        var updatedValueHtml2 = $fakeDiv2.html();
                        $vMergeFields.find('.js-value-list-html').val(updatedValueHtml2);

                        var valueHtml3 = $vRecipientFields.find('.js-value-list-html').val();
                        var $fakeDiv3 = $('<div/>').append(valueHtml3);
                        $fakeDiv3.find('.js-value-list-input').find('option[value="' + reportFieldGuid + '"]').text(title);
                        var updatedValueHtml3 = $fakeDiv3.html();
                        $vRecipientFields.find('.js-value-list-html').val(updatedValueHtml3);
                    }
                })

            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
