Imports System.Net
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Threading
 
Module Module1
    Dim WithEvents oReadSiteHTML As New ReadSiteHTMLClass
    Sub Main()
        Dim URLlist() As String = System.IO.File.ReadAllLines("sitelist.txt")
 
        'Multi-Threaded
        Dim ThreadList As New ArrayList
 
        Dim stopper As New Stopwatch
        stopper.Start()
 
        For Each tempurl In URLlist
            Dim t As New Thread(AddressOf oReadSiteHTML.GetHTML)
            ThreadList.Add(t)
            t.Start(tempurl)
        Next
 
        For Each t In ThreadList
            t.join()
        Next
 
        stopper.Stop()
        Dim timetook As String = stopper.ElapsedMilliseconds.ToString()
 
        'single threaded
 
        Dim stopper2 As New Stopwatch
        stopper2.Start()
 
        For Each tempurl In URLlist
            oReadSiteHTML.GetHTML(tempurl)
        Next
 
        stopper2.Stop()
        Dim timetook2 As String = stopper2.ElapsedMilliseconds.ToString()
 
    End Sub
 
    Private Function GenerateHash(ByVal SourceText As String) As String
        Dim Uni As New UnicodeEncoding()
        Dim ByteSourceText() As Byte = Uni.GetBytes(SourceText)
        Dim Md5 As New MD5CryptoServiceProvider()
        Dim ByteHash() As Byte = Md5.ComputeHash(ByteSourceText)
        Return Convert.ToBase64String(ByteHash)
    End Function
 
    Public Class ReadSiteHTMLClass
        Public SiteHtmlHash As String
        Public Event ThreadHash(ByVal SiteHtmlHash2 As String)
        Public Sub GetHTML(ByVal URL As String)
            Dim request As HttpWebRequest = WebRequest.Create(URL)
            Dim response As HttpWebResponse = request.GetResponse()
            Dim reader As StreamReader = New StreamReader(response.GetResponseStream())
            Dim str As String = reader.ReadLine()
            Dim sitefulltext As String = ""
            Do While (Not str Is Nothing)
                sitefulltext = String.Concat(sitefulltext, str)
                str = reader.ReadLine()
                If str Is Nothing Then
                    Exit Do
                End If
            Loop
            reader.Close()
            SiteHtmlHash = GenerateHash(sitefulltext)
 
            RaiseEvent ThreadHash(SiteHtmlHash &amp; ":" &amp; URL)
        End Sub
    End Class
 
    Sub ThreadHash(ByVal SiteHtmlHash3 As String) Handles oReadSiteHTML.ThreadHash
        SyncLock GetType(ReadSiteHTMLClass)
            File.AppendAllText("hashes.txt", SiteHtmlHash3 &amp; System.Environment.NewLine)
        End SyncLock
    End Sub
 
End Module
