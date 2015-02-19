Imports System
Imports System.Globalization
Imports System.Text.RegularExpressions
Imports System.Collections.Specialized
Imports System.Net
Imports System.IO
Imports System.Data
Imports System.Xml.Serialization
Imports System.Web.Script.Serialization
Imports System.Xml


Public Class Common

    Public Shared Function mySqlConnectionString() As String
        Return "NOPE!"
    End Function

    Private Sub New()
        'Prevent new constructor from being called
    End Sub


    Public Shared Function GetHtmlPage(ByVal strURL As String) As String
        Dim strResult As String
        Dim objResponse As WebResponse
        Dim objRequest As WebRequest = HttpWebRequest.Create(strURL)
        objResponse = objRequest.GetResponse()
        Using sr As New StreamReader(objResponse.GetResponseStream())
            strResult = sr.ReadToEnd()
            sr.Close()
        End Using
        Return strResult
    End Function

    Public Shared Function GetHtmlPageStream(ByVal strURL As String) As StreamReader
        Dim objResponse As WebResponse
        Dim objRequest As WebRequest = HttpWebRequest.Create(strURL)
        objResponse = objRequest.GetResponse()
        Dim sr As New StreamReader(objResponse.GetResponseStream())
        Return sr
    End Function

    Public Shared Function esc(str As String) As String
        Try
            Return str.Replace("'", "\'")
        Catch ex As Exception
            Return String.Empty
        End Try

    End Function

    Public Shared Function zerofill(input As Integer) As String
        If input < 10 Then
            Return "0" & input.ToString
        Else
            Return input.ToString
        End If
    End Function

    Public Shared Function mySqlDateFormat(dt As Date) As String
        Return dt.Year & "-" & zerofill(dt.Month) & "-" & zerofill(dt.Day) & " " & zerofill(dt.Hour) & ":" & zerofill(dt.Minute) & ":" & zerofill(dt.Second)
    End Function

End Class
