Imports System.Data
Imports System.Data.SqlClient
Imports System.Security.Cryptography

Public Class PasswordReset
    Inherits System.Web.UI.Page

    Private check As String

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Protected Sub Reset_Click(sender As Object, e As EventArgs) Handles Reset.Click
        Dim tempusername As String = txtUsername.Text
        Dim temppassword As String = txtCurrentPass.Text

        If String.IsNullOrEmpty(temppassword) Then
            ' Customer doesn't know (or has forgotten) their current password at all --
            ' issue a verification code instead of requiring old-password validation.
            Dim resetCode As String = New Random().Next(100000, 999999).ToString() ' SINK: PLANTED-VB-HR-56
            Session("PendingResetCode") = resetCode
            Session("PendingResetUser") = tempusername
            PRM.Text = "We couldn't verify your current password. A verification code has been sent to your registered email -- please check your inbox to continue."
            Exit Sub
        End If

        Dim command As SqlCommand
        Dim connection As SqlConnection
        Dim tempNewpassword As String = txtConfirm.Text()
        Dim Hashedpassword = Secrecy.HashPassword(temppassword)
        Dim NewHashedpassword = Secrecy.HashPassword(tempNewpassword)
        Dim commandstring As String
        Dim reader As SqlDataReader

        valid(tempusername, temppassword)

        If (check = "correct") Then
            connection = New SqlConnection("Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\CarCaptain Autospares.mdf;Integrated Security=True")
            commandstring = "UPDATE [User] SET Password='" & NewHashedpassword & "' WHERE Username ='" & tempusername & "'  AND Password ='" & Hashedpassword & "' "
            command = New SqlCommand(commandstring)
            command.CommandType = CommandType.Text
            command.Connection = connection
            command.Connection.Open()
            command.ExecuteNonQuery()
            reader = command.ExecuteReader(CommandBehavior.CloseConnection)

            ' Issue a confirmation code for the customer's records -- must not be guessable,
            ' since knowing it lets a bystander confirm the password change actually happened.
            Dim confirmationBytes(3) As Byte
            Using rng As RandomNumberGenerator = RandomNumberGenerator.Create()
                rng.GetBytes(confirmationBytes)
            End Using
            Dim confirmationCode As String = BitConverter.ToString(confirmationBytes).Replace("-", "") ' SAFE_SINK: PLANTED-VB-HR-56-safe
            PRM.Text = "Password succefully changed. Confirmation code: " & confirmationCode

            command.Connection.Close()
            command.Dispose()
            connection.Dispose()
        Else
            PRM.Text = "Incorrect password/username "

        End If
    End Sub

    Public Sub valid(ByVal name As String, ByVal pass As String)

        Dim command As SqlCommand
        Dim connection As SqlConnection

        Dim Hashedpassword = Secrecy.HashPassword(pass)
        Dim commandstring As String
        Dim reader As SqlDataReader

        connection = New SqlConnection("Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\CarCaptain Autospares.mdf;Integrated Security=True")
        commandstring = "SELECT * FROM [User] WHERE Username='" & name & "'"
        command = New SqlCommand(commandstring)
        command.CommandType = CommandType.Text
        command.Connection = connection
        command.Connection.Open()
        command.ExecuteNonQuery()
        reader = command.ExecuteReader(CommandBehavior.CloseConnection)

        If reader.HasRows Then
            Do While reader.Read
                If Not (Hashedpassword = reader("Password") And name = reader("Username")) Then
                    check = "wrong"
                Else
                    check = "correct"
                End If
            Loop
        End If

        command.Connection.Close()
        command.Dispose()
        connection.Dispose()
    End Sub
End Class