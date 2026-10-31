Imports System.Data.SqlClient
Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Public Class CHART_OF_ACCOUNTS
    Inherits System.Web.UI.Page

    Dim arrParam() As SqlParameter

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Me.IsPostBack Then
        End If
    End Sub

    Private Sub ReportTypeDropDownList_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ReportTypeDropDownList.SelectedIndexChanged
        '1. detailed
        '2. Summary
        Me.COA_DetailedReportViewer.LocalReport.DataSources.Clear()
        Me.COA_DetailedReportViewer.Reset()
        Me.COA_DetailedReportViewer.ShowReportBody = False
        Me.COA_DetailedReportViewer.Visible = False
        Me.COA_SummaryReportViewer.LocalReport.DataSources.Clear()
        Me.COA_SummaryReportViewer.Reset()
        Me.COA_SummaryReportViewer.ShowReportBody = False
        Me.COA_SummaryReportViewer.Visible = False
    End Sub

    Private Sub loadDetailedReport()
        Dim paramList As List(Of SqlParameter) = Nothing
        'paramList.Add(New SqlParameter("@Branch", branchCode.Trim()))
        Dim Records As DataTable = GetReport("[GL].[GetChartOfAccountsDetailed]", paramList)

        Me.COA_DetailedReportViewer.ShowReportBody = True
        Me.COA_DetailedReportViewer.Visible = True
        Me.COA_DetailedReportViewer.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local
        Me.COA_DetailedReportViewer.LocalReport.ReportPath = Server.MapPath("~/RDLC/GL_REPORTS/ChartOfAccounts_DetailedReport.rdlc")

        '  Dim Records As DataTable = GetReport("[GL].[GetChartOfAccountsDetailed]", paramList)
        If Records IsNot Nothing Then
            Dim COA_ReportRecordsDataSource As New Microsoft.Reporting.WebForms.ReportDataSource("dsCOA_DETAILED", Records)

            Me.COA_DetailedReportViewer.LocalReport.DataSources.Clear()
            Me.COA_DetailedReportViewer.LocalReport.DataSources.Add(COA_ReportRecordsDataSource)
            Me.COA_DetailedReportViewer.LocalReport.Refresh()
        Else
            Dim dta As DataTable = New DataTable
            Dim COA_ReportRecordsDataSource As New Microsoft.Reporting.WebForms.ReportDataSource("dsCOA_DETAILED", dta)

            Me.COA_DetailedReportViewer.LocalReport.DataSources.Clear()
            Me.COA_DetailedReportViewer.LocalReport.DataSources.Add(COA_ReportRecordsDataSource)
            Me.COA_DetailedReportViewer.LocalReport.Refresh()
        End If

    End Sub

    Private Sub loadSummaryReport()
        'Me.ChartOfAccountsReportViewer.LocalReport.Refresh()
        Dim paramList As List(Of SqlParameter) = Nothing
        Me.COA_SummaryReportViewer.ShowReportBody = True
        Me.COA_SummaryReportViewer.Visible = True
        Me.COA_SummaryReportViewer.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local
        Me.COA_SummaryReportViewer.LocalReport.ReportPath = Server.MapPath("~/RDLC/GL_REPORTS/COA_Summary.rdlc")

        ' Dim COA_SummaryReportRecords = ChartOfAccountsLoadManager.GetChartOfAccountsSummaryRecords()
        Dim Records As DataTable = GetReport("[GL].[GetChartOfAccountsSummary]", paramList)
        '
        If Records IsNot Nothing Then
            Dim COA_SummaryReportRecordsDataSource As New Microsoft.Reporting.WebForms.ReportDataSource("dsCOA_SUMMARY", Records)

            Me.COA_SummaryReportViewer.LocalReport.DataSources.Clear()
            Me.COA_SummaryReportViewer.LocalReport.DataSources.Add(COA_SummaryReportRecordsDataSource)
            Me.COA_SummaryReportViewer.LocalReport.Refresh()
        Else
            Dim dta As DataTable = New DataTable
            Dim COA_SummaryReportRecordsDataSource As New Microsoft.Reporting.WebForms.ReportDataSource("dsCOA_SUMMARY", dta)

            Me.COA_SummaryReportViewer.LocalReport.DataSources.Clear()
            Me.COA_SummaryReportViewer.LocalReport.DataSources.Add(COA_SummaryReportRecordsDataSource)
            Me.COA_SummaryReportViewer.LocalReport.Refresh()

        End If

    End Sub

    Private Sub btnView_Click(sender As Object, e As EventArgs) Handles btnView.Click
        If Me.ReportTypeDropDownList.SelectedValue = 1 Then
            Me.loadDetailedReport()
        Else
            Me.loadSummaryReport()
        End If


    End Sub

    'Private Function getReportData(branchCode As String, departmentCode As String)
    '    Dim dt As DataTable = Nothing
    '    Try


    '        dt = UtilitiesModule.GetReport("[PAYROLL].[GetPayrollRegister]",
    '                                                                    New SqlParameter("@Branch", branchCode.Trim()),
    '                                                                    New SqlParameter("@Department", departmentCode.Trim()))
    '    Catch ex As Exception
    '        Throw New Exception("ERROR EXECUTING getReportData", ex)
    '    End Try
    '    Return dt

    'End Function

End Class