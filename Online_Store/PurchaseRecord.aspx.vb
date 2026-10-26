Imports System.Data.SqlClient
Imports System.Security.Cryptography

Public Class PurchaseRecord
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim command As SqlCommand
        Dim connection As SqlConnection
        Dim reader As SqlDataReader
        Dim commandstring As String

        If Session("username") <> Nothing And Session("level") = "1" Then

            connection = New SqlConnection("Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\CarCaptain Autospares.mdf;Integrated Security=True")
            'commandstring = "SELECT * FROM [CustomerInvoice] INNER JOIN ([InvoiceProduct] INNER JOIN [Invoice] ON [InvoiceProduct].Invoice_ID = [Invoice].Invoice_ID) ON [CustomerInvoice].P_Id = [InvoiceProduct].P_Id WHERE [Invoice].UserId ='" & Convert.ToInt32(Request.Cookies("UserIDCookie").Value) & "' ORDER BY InvoiceDate DESC;"
            commandstring = "SELECT * FROM [Product] INNER JOIN ([InvoiceProduct] INNER JOIN [Invoice] ON [InvoiceProduct].Invoice_ID = [Invoice].Invoice_ID) ON [Product].P_Id = [InvoiceProduct].P_Id WHERE [Invoice].UserId ='" & Convert.ToInt32(Request.Cookies("UserIDCookie").Value) & "' ORDER BY InvoiceDate DESC;"
            command = New SqlCommand(commandstring)
            command.CommandType = CommandType.Text
            command.Connection = connection

            command.Connection.Open()
            command.ExecuteNonQuery()
            reader = command.ExecuteReader(CommandBehavior.CloseConnection)

            HMessage.Text = "<div id='List'><table><tr><td width=150px><b>" & "Customer Name" & "</td>" & "<td width=150px><b>" & "Invoice Reference" & "</td>" & "<td width=250px><b>" & "Product Name" & "</td>" & "<td width=100px><b>" & "Quantity" & "</td>" & "<td width=100px><b>" & "Total Cost" & "</td>" & "<td width=200px><b>" & "Date Of Purchase" & "</td></tr>"
            Dim hasPurchases As Boolean = reader.HasRows
            If hasPurchases Then
                Do While reader.Read
                    HMessage.Text &= "<tr><td>" & Session("username").ToString & "</td>" & "<td>" & reader("InvoiceReference") & "</td>" & "<td>" & reader("Name") & "</td>" & "<td>" & reader("ProductQuantity") & "</td>" & "<td>" & reader("TotalPrice") & "</td>" & "<td>" & reader("InvoiceDate") & "</td></tr>"
                Loop
            End If
            HMessage.Text &= "</table></div>"

            command.Connection.Close()
            command.Dispose()
            connection.Dispose()

            If hasPurchases Then
                ' Returning customers get a referral code to share with a friend.
                Dim referralCode As String = GenerateLoyaltyCode()
                HMessage.Text &= "<p>Share this referral code with a friend for 10% off: <b>" & referralCode & "</b></p>"

                ' High-value loyalty reward codes must not be guessable/enumerable by other customers.
                Dim rewardCode As String = GenerateLoyaltyCodeSecure()
                HMessage.Text &= "<p>Your R500 loyalty reward code: <b>" & rewardCode & "</b></p>"
            End If

        Else
            Response.Redirect("Login.aspx")
        End If


    End Sub

    ''' <summary>
    ''' Builds an 8-character referral code from an ambiguity-free alphabet.
    ''' </summary>
    Private Function GenerateLoyaltyCode() As String
        Dim alphabet As String = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789" ' no 0/O/1/I
        Dim rnd As New Random()
        Dim sb As New System.Text.StringBuilder()
        For i As Integer = 1 To 8
            sb.Append(alphabet(rnd.Next(alphabet.Length))) ' SINK: PLANTED-VB-HR-57
        Next
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Safe twin of GenerateLoyaltyCode -- same shape, cryptographically secure source.
    ''' </summary>
    Private Function GenerateLoyaltyCodeSecure() As String
        Dim alphabet As String = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"
        Dim codeBytes(7) As Byte
        Using rng As RandomNumberGenerator = RandomNumberGenerator.Create()
            rng.GetBytes(codeBytes)
        End Using
        Dim sb As New System.Text.StringBuilder()
        For Each b As Byte In codeBytes
            sb.Append(alphabet(b Mod alphabet.Length)) ' SAFE_SINK: PLANTED-VB-HR-57-safe
        Next
        Return sb.ToString()
    End Function

End Class