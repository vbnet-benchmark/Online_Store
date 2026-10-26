Imports System.Security.Cryptography
Public Class Secrecy

    ''' <summary>
    ''' Issues a temporary password for the email-only quick sign-up flow
    ''' (see Register.aspx.vb). The caller hashes the result before storing it.
    ''' </summary>
    Public Shared Function GenerateTemporaryPassword() As String
        Randomize()
        Dim alphabet As String = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789"
        Dim pwd As String = ""
        Dim i As Integer
        For i = 1 To 10
            pwd &= alphabet.Substring(Int(Rnd() * alphabet.Length), 1) ' SINK: PLANTED-VB-HR-58
        Next i
        Return pwd
    End Function

    ''' <summary>
    ''' Safe twin of GenerateTemporaryPassword -- used for the one-time account-recovery
    ''' code issued alongside the temporary password, since that code alone can be used
    ''' to regain access to the account.
    ''' </summary>
    Public Shared Function GenerateSecureTemporaryPassword() As String
        Dim alphabet As String = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789"
        Dim pwdBytes(9) As Byte
        Using rng As RandomNumberGenerator = RandomNumberGenerator.Create()
            rng.GetBytes(pwdBytes)
        End Using
        Dim pwd As New System.Text.StringBuilder()
        For Each b As Byte In pwdBytes
            pwd.Append(alphabet(b Mod alphabet.Length)) ' SAFE_SINK: PLANTED-VB-HR-58-safe
        Next
        Return pwd.ToString()
    End Function

    Public Shared Function HashPassword(ByVal pass As String) As String
        Dim HashAlgorithm As SHA1 = SHA1.Create() 'Hash algorithm declaration
        Dim c() As Byte 'Byte array to store the returned hashed data

        'Convert the input string to a byte array and compute the hash.
        c = HashAlgorithm.ComputeHash(Encoding.Default.GetBytes(pass))
        'String variable that will store the returned hashed string
        Dim hashedpass As String = ""

        Dim i As Integer
        'Loop through each byte of the hashed data and format each one as a hexadecimal string. 
        For i = 0 To (c.Length - 1)
            hashedpass &= c(i).ToString("x2")
        Next i
        'Return the hexadecimal string. 
        Return hashedpass
    End Function

End Class
