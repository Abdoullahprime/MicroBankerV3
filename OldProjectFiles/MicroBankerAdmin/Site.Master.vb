Public Class SiteMaster
    Inherits MasterPage
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

    End Sub
    Protected Sub lnk_Click(ByVal sender As Object, ByVal e As EventArgs)
        'Dim Url As String = "http://192.168.88.210/Reports/"
        'Dim Url As String = "http://18.156.69.72/MicroBankerReportsDemo/"
        Dim Url As String = "http://localhost/MicroBankerReportsDemo/"
        Response.Write("<script language='javascript'>window.open('" & Url & "','_blank','');")
        Response.Write("</script>")
    End Sub
End Class