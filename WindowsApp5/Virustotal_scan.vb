Imports System.IO
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Security.Cryptography
Imports System.Net
Imports System.Text.RegularExpressions
Public Class Virustotal_scan

    Public Function CalculateFileHash(filePath As String) As String
        Dim hashString As String = ""

        Try
            Using fileStream As FileStream = New FileStream(filePath, FileMode.Open, FileAccess.Read)
                Using sha256 As SHA256 = SHA256.Create()
                    Dim hashBytes As Byte() = sha256.ComputeHash(fileStream)
                    hashString = BitConverter.ToString(hashBytes).Replace("-", String.Empty)
                End Using
            End Using
        Catch ex As Exception
        End Try

        Return hashString
    End Function

    Public Async Sub GetFileDetails()
        Dim client As HttpClient = New HttpClient()
        Dim request As HttpRequestMessage = New HttpRequestMessage() With {
        .Method = HttpMethod.Get,
        .RequestUri = New Uri("https://www.virustotal.com/api/v3/files/" + TextBox1.Text)
    }
        request.Headers.Accept.Add(New MediaTypeWithQualityHeaderValue("application/json"))
        request.Headers.Add("x-apikey", "478a62faa7503392d6dd4ffde5144178fbfd704aee911fbe8e811553ac4372af")

        Try
            Using response As HttpResponseMessage = Await client.SendAsync(request)
                response.EnsureSuccessStatusCode()
                Dim body As String = Await response.Content.ReadAsStringAsync()
                RichTextBox1.Text = (body)
                result()
                prog()
            End Using
        Catch ex As Exception
        End Try
    End Sub
    Sub result()
        Dim regex As New Regex("""([^""]+)"":\s*{[^}]+,\s*""result"":\s*""([^""]+)""")
        Dim matches As MatchCollection = regex.Matches(RichTextBox1.Text)
        Dim results As New List(Of String)()
        For Each match As Match In matches
            Dim engineName As String = match.Groups(1).Value
            Dim result As String = match.Groups(2).Value
            results.Add(engineName & " ====>> " & result)
        Next
        RichTextBox2.Text = String.Join(Environment.NewLine, results)
        Dim engineNameCount As Integer = matches.Count
        Label1.Text = " The virus has been detected by " + engineNameCount.ToString() + " protection(s)."
    End Sub
    Public Structure VirusTotalScanResult
        Public Property Vendor As String
        Public Property Result As String
    End Structure
    Dim filePath As String
    <STAThread> ' إضافة السمة STAThread
    Sub filees()
        Me.Invoke(Sub()
                      Dim openFileDialog As New OpenFileDialog()
                      openFileDialog.InitialDirectory = "C:\"
                      openFileDialog.Filter = "Files (*.*)|*.*"
                      If openFileDialog.ShowDialog() = DialogResult.OK Then
                          filePath = openFileDialog.FileName
                      Else
                          filePath = String.Empty
                      End If
                      openFileDialog.Dispose()
                  End Sub)
    End Sub
    Sub prog()
        Dim labelText As String = Label1.Text
        Dim index As Integer = labelText.IndexOf(":") + 1
        Dim numberText As String = labelText.Substring(index).Trim()
        Dim targetValue As Integer = Integer.Parse(numberText)
        For i As Integer = 2 To targetValue
            Application.DoEvents()
        Next
    End Sub
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click

        Dim thread As New Threading.Thread(AddressOf third_filees)

        thread.Start()
    End Sub
    Sub third_filees()
        '
        filees()

        Try
            Dim client As New HttpClient()
            ' Create a new MultipartFormDataContent object.
            Dim content As New MultipartFormDataContent()
            ' Add the file as a StreamContent parameter.
            Dim fileStream As Stream = File.OpenRead(filePath)
            Dim fileContent As New StreamContent(fileStream)
            content.Add(fileContent, "file", Path.GetFileName(filePath))
            ' Add the required headers.
            Label1.Text = "Uploading"
            client.DefaultRequestHeaders.Add("accept", "application/json")
            client.DefaultRequestHeaders.Add("x-apikey", "478a62faa7503392d6dd4ffde5144178fbfd704aee911fbe8e811553ac4372af")
            ' Send the request and get the response.
            Dim response As HttpResponseMessage = client.PostAsync("https://www.virustotal.com/api/v3/files", content).Result
            ' Check the response status code.
            If response.StatusCode = HttpStatusCode.OK Then
                ' Get the response body.
                Dim responseBody As String = response.Content.ReadAsStringAsync().Result
                ' Display the response in RichTextBox2.
                RichTextBox2.Invoke(Sub() RichTextBox2.Text = responseBody)
            Else
                ' Print the error message.
                RichTextBox2.Invoke(Sub() RichTextBox2.Text = "Error: " & response.ReasonPhrase)
            End If
            Dim hash As String = CalculateFileHash(filePath)
            TextBox1.Invoke(Sub() TextBox1.Text = hash)
            GetFileDetails()
        Catch ex As Exception
        Finally
            TextBox1.Text = Nothing
            Me.Invoke(Sub() GetFileDetails())
        End Try
    End Sub

    Private Sub Virustotal_scan_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class