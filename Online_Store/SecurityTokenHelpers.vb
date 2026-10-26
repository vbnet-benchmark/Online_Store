Imports System.Security.Cryptography

''' <summary>
''' Generates the per-shopping-session cart token stored in Session("CartSessionToken"),
''' used to identify/resume a customer's shopping session across requests.
''' </summary>
Public MustInherit Class CartTokenGenerator
    Public MustOverride Function GenerateSessionToken() As String
End Class

''' <summary>
''' Backward-compatible token generator kept for older client versions that still pass
''' cartMode=legacy (pre-dates the move to cryptographically secure session tokens).
''' </summary>
Public Class LegacyCartTokenGenerator
    Inherits CartTokenGenerator

    Public Overrides Function GenerateSessionToken() As String
        ' Seeded from the system tick count -- predictable within a narrow, guessable window.
        Return New Random(Environment.TickCount).Next().ToString("x8") ' SINK: PLANTED-VB-HR-59
    End Function
End Class

Public Class SecureCartTokenGenerator
    Inherits CartTokenGenerator

    Public Overrides Function GenerateSessionToken() As String
        Dim tokenBytes(7) As Byte
        Using rng As RandomNumberGenerator = RandomNumberGenerator.Create()
            rng.GetBytes(tokenBytes)
        End Using
        Return BitConverter.ToString(tokenBytes).Replace("-", "").ToLowerInvariant() ' SAFE_SINK: PLANTED-VB-HR-59-safe
    End Function
End Class
