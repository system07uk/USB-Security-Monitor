Imports System.Management
Imports System.Timers
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Net
Imports Emgu.CV
Imports Emgu.CV.BitmapExtension
Imports System.Windows.Forms
Imports System.Net.Http
Imports Newtonsoft.Json
Imports System.Text
Imports System.Threading.Tasks
Imports System.Linq
Imports System.Diagnostics

Public Class Form1
    ' Modified: Support for %APPDATA%
    Private ReadOnly CONFIG_PATH As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HUK_CHK.NFO")
    Private Const FORMAT_ARGS As String = "/FS:NTFS /Q /Y"
    Private Const DISCORD_COLOR_WARNING As Integer = 16711680 ' Red: For changes
    Private Const DISCORD_COLOR_NORMAL As Integer = 65280 ' Green: For normal
    Private WithEvents ShutdownTimer As New System.Timers.Timer(5000)
    Private shutdownInitiated As Boolean = False
    Private WithEvents Watcher As ManagementEventWatcher
    Dim normalPassword As String, emergencyPassword As String
    Dim webhookUrl As String
    Private lastTaskbarTitles As New List(Of String)
    Public header As String
    Private cam_capture As VideoCapture
    Private webcamImage As Bitmap

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim credentials = ReadCredentials(CONFIG_PATH)
        normalPassword = credentials.Item1
        emergencyPassword = credentials.Item2
        webhookUrl = credentials.Item3
        If String.IsNullOrEmpty(normalPassword) AndAlso String.IsNullOrEmpty(emergencyPassword) AndAlso String.IsNullOrEmpty(webhookUrl) Then
            ' No file or empty data: Show settings dialog
            Button_config_Click(Nothing, EventArgs.Empty)
        End If
        ShutdownTimer.Start()
        Dim query As New WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2")
        Watcher = New ManagementEventWatcher(query)
        AddHandler Watcher.EventArrived, AddressOf USBInserted
        Watcher.Start()
    End Sub

    Private Sub ShutdownTimer_Elapsed(sender As Object, e As ElapsedEventArgs) Handles ShutdownTimer.Elapsed
        ShutdownTimer.Stop()
        If Me.InvokeRequired Then Me.Invoke(Sub() ShowPasswordDialog()) Else ShowPasswordDialog()
    End Sub

    Private Async Sub ShowPasswordDialog()
        Dim password = ShowPasswordInputBox("Enter password:", "Password Verification", 60)
        If String.IsNullOrEmpty(password) OrElse password <> normalPassword Then
            Await Task.Run(Sub() Me.Invoke(Sub() MessageBox.Show("Unauthorized user. Shutting down the system.")))
            shutdownInitiated = True
            InitiateShutdown(120)
            CaptureAndSendEmail()
            Dim emergencyInput = ShowPasswordInputBox("===Enter emergency password===", "Emergency Password Verification", 60)
            If emergencyInput = emergencyPassword Then
                shutdownInitiated = False
                InitiateShutdown(0, True)
                MessageBox.Show("System shutdown canceled.")
            End If
        Else
            shutdownInitiated = False
            MessageBox.Show("System normal")
        End If
    End Sub

    Private Function ShowPasswordInputBox(prompt As String, title As String, Optional timeoutSeconds As Integer = 60) As String
        Dim inputForm As New Form With {
            .Width = 400, .Height = 180, .Text = title, .FormBorderStyle = FormBorderStyle.FixedDialog,
            .StartPosition = FormStartPosition.CenterScreen, .MinimizeBox = False, .MaximizeBox = False
        }
        Dim label As New Label With {.Left = 10, .Top = 20, .Text = prompt, .AutoSize = True}
        Dim textBox As New TextBox With {.Left = 10, .Top = 50, .Width = 360, .PasswordChar = "*"c}
        Dim okButton As New Button With {.Text = "OK", .Left = 210, .Top = 100, .DialogResult = DialogResult.OK}
        Dim cancelButton As New Button With {.Text = "Cancel", .Left = 290, .Top = 100, .DialogResult = DialogResult.Cancel}
        Dim countdownLabel As New Label With {.Left = 10, .Top = 130, .AutoSize = True}
        inputForm.Controls.AddRange({label, textBox, okButton, cancelButton, countdownLabel})
        inputForm.AcceptButton = okButton
        inputForm.CancelButton = cancelButton
        Dim remainingTime = timeoutSeconds
        Dim countdownTimer As New System.Windows.Forms.Timer With {.Interval = 1000}
        AddHandler countdownTimer.Tick, Sub()
                                            remainingTime -= 1
                                            If remainingTime <= 0 Then
                                                countdownTimer.Stop()
                                                inputForm.DialogResult = DialogResult.Cancel
                                                inputForm.Close()
                                            Else
                                                countdownLabel.Text = $"Remaining time: {remainingTime} seconds"
                                            End If
                                        End Sub
        countdownTimer.Start()
        Dim result = If(inputForm.ShowDialog() = DialogResult.OK, textBox.Text, String.Empty)
        countdownTimer.Stop()
        Return result
    End Function

    Private Function ReadCredentials(filePath As String) As Tuple(Of String, String, String)
        Try
            Dim lines = File.ReadAllLines(filePath)
            If lines.Length >= 3 Then
                Return Tuple.Create(lines(0), lines(1), lines(2))
            Else
                Throw New Exception("Insufficient data in file.")
            End If
        Catch ex As Exception
            MessageBox.Show("No config file, proceeding to password setup.")
            ShowConfigDialog()
            Return Tuple.Create(String.Empty, String.Empty, String.Empty)
        End Try
    End Function

    Private Sub CaptureAndSendEmail()
        Try
            InitializeWebcam()
            ProcessFrame()
            Dim myip = IPtest()
            header = $"Event rec: {myip}: {Environ$("computername")}: {Environ$("username")}"
            InitializeTaskbarMonitoring()
            SendToDiscord(lastTaskbarTitles, New List(Of String), lastTaskbarTitles)
        Catch ex As Exception
            MessageBox.Show("An error occurred: " & ex.Message)
        Finally
            ReleaseWebcam()
        End Try
    End Sub

    Private Sub InitializeWebcam()
        Try
            cam_capture = New VideoCapture()
            If cam_capture Is Nothing OrElse cam_capture.Ptr = IntPtr.Zero Then
                If Check_cam IsNot Nothing Then Check_cam.Checked = False
                Return
            End If
            If Check_cam IsNot Nothing Then Check_cam.Checked = True
        Catch ex As Exception
            If Check_cam IsNot Nothing Then Check_cam.Checked = False
        End Try
    End Sub

    Private Sub ReleaseWebcam()
        cam_capture?.Dispose()
        cam_capture = Nothing
    End Sub

    Private Sub USBInserted(sender As Object, e As EventArrivedEventArgs)
        Dim driveLetter = e.NewEvent.Properties("DriveName").Value.ToString()
        If Check_usb IsNot Nothing AndAlso Check_usb.Checked = True Then
            FormatDrive(driveLetter)
            MessageBox.Show("Unauthorized device connected and removed: " & driveLetter)
        End If
    End Sub

    Private Sub FormatDrive(driveLetter As String)
        Try
            Dim psi As New ProcessStartInfo("cmd.exe", $"/c format {driveLetter} {FORMAT_ARGS}") With {
                .WindowStyle = ProcessWindowStyle.Hidden, .CreateNoWindow = True
            }
            Process.Start(psi)
            ShutdownTimer.Start()
        Catch ex As Exception
            MessageBox.Show("Error during formatting: " & ex.Message)
        End Try
    End Sub

    Private Sub SetCheckCamChecked(value As Boolean)
        If Check_cam IsNot Nothing Then
            If Check_cam.InvokeRequired Then
                Check_cam.Invoke(New Action(Of Boolean)(AddressOf SetCheckCamChecked), value)
            Else
                Check_cam.Checked = value
            End If
        End If
    End Sub

    Private Sub ProcessFrame()
        If cam_capture IsNot Nothing AndAlso cam_capture.Ptr <> IntPtr.Zero Then
            Using frame As Mat = cam_capture.QueryFrame()
                If frame IsNot Nothing AndAlso Not frame.IsEmpty Then webcamImage = frame.ToBitmap()
            End Using
        End If
    End Sub

    Public Function IPtest() As String
        Try
            Dim host = Dns.GetHostName()
            Dim ipAddr = Dns.GetHostEntry(host).AddressList.FirstOrDefault(Function(addr) addr.AddressFamily = Net.Sockets.AddressFamily.InterNetwork)
            Return If(ipAddr IsNot Nothing, ipAddr.ToString(), "0.0.0.0")
        Catch
            Return "0.0.0.0"
        End Try
    End Function

    Private Sub InitializeTaskbarMonitoring()
        SendInitialTaskbarTitles()
    End Sub

    Private Sub SendInitialTaskbarTitles()
        lastTaskbarTitles = GetTaskbarTitles()
        SendToDiscord(lastTaskbarTitles, New List(Of String), lastTaskbarTitles)
    End Sub

    Private Function GetTaskbarTitles() As List(Of String)
        Return Process.GetProcesses().
            Where(Function(p) Not String.IsNullOrEmpty(p.MainWindowTitle)).
            Select(Function(p) p.MainWindowTitle).
            ToList()
    End Function

    Private Async Sub SendToDiscord(newTitles As List(Of String), removedTitles As List(Of String), currentTitles As List(Of String))
        If String.IsNullOrEmpty(webhookUrl) Then Return
        Try
            Using client As New HttpClient()
                Dim desc = $"**Added**{If(newTitles.Any, vbCrLf & "• " & String.Join(vbCrLf & "• ", newTitles), ": None")}" & vbCrLf &
                           $"**Removed**{If(removedTitles.Any, vbCrLf & "• " & String.Join(vbCrLf & "• ", removedTitles), ": None")}" & vbCrLf & vbCrLf &
                           $"**Currently Running**{If(currentTitles.Any, vbCrLf & "• " & String.Join(vbCrLf & "• ", currentTitles.Take(15)), ": None")}"
                Dim payload = New With {
                    .username = "Security Alert Bot",
                    .content = $"**{header}** | {DateTime.Now:HH:mm:ss}",
                    .embeds = {New With {
                        .description = desc,
                        .color = If(newTitles.Any Or removedTitles.Any, DISCORD_COLOR_WARNING, DISCORD_COLOR_NORMAL),
                        .timestamp = DateTime.UtcNow.ToString("o")
                    }}
                }
                Dim multipart = New MultipartFormDataContent()
                multipart.Add(New StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"), "payload_json")
                Dim screenFilePath = CaptureScreen()
                If Not String.IsNullOrEmpty(screenFilePath) Then
                    multipart.Add(New ByteArrayContent(File.ReadAllBytes(screenFilePath)), "file", "screen.png")
                    File.Delete(screenFilePath)
                End If
                If Check_cam IsNot Nothing AndAlso Check_cam.Checked Then
                    Dim webcamFilePath = CaptureWebcam()
                    If Not String.IsNullOrEmpty(webcamFilePath) Then
                        multipart.Add(New ByteArrayContent(File.ReadAllBytes(webcamFilePath)), "file2", "webcam.jpeg")
                        File.Delete(webcamFilePath)
                    End If
                End If
                Dim resp = Await client.PostAsync(webhookUrl, multipart)
                If resp.IsSuccessStatusCode Then
                    Console.WriteLine("Discord webhook sent successfully: Status " & resp.StatusCode)
                Else
                    Console.WriteLine("Discord webhook failed: Status " & resp.StatusCode & " | Error: " & Await resp.Content.ReadAsStringAsync())
                End If
            End Using
        Catch ex As Exception
            Console.WriteLine("Error sending Discord webhook: " & ex.Message & " | StackTrace: " & ex.StackTrace)
        End Try
    End Sub

    Private Function CaptureScreen() As String
        Try
            Dim bounds = Screen.PrimaryScreen.Bounds
            Using bitmap As New Bitmap(bounds.Width, bounds.Height)
                Using g = Graphics.FromImage(bitmap)
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size)
                End Using
                Dim timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss")
                Dim filePath = Path.Combine(Path.GetTempPath(), $"screenshot_{timestamp}.png")
                bitmap.Save(filePath, ImageFormat.Png)
                Return filePath
            End Using
        Catch ex As Exception
            Console.WriteLine("Error during screen capture: " & ex.Message)
            Return String.Empty
        End Try
    End Function

    Private Function CaptureWebcam() As String
        Try
            Dim timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss")
            Dim filePath = Path.Combine(Path.GetTempPath(), $"webcam_{timestamp}.jpeg")
            webcamImage.Save(filePath, ImageFormat.Jpeg)
            Return filePath
        Catch ex As Exception
            Console.WriteLine("Error during webcam capture: " & ex.Message)
            Return String.Empty
        End Try
    End Function

    Private Sub Button_OK_Click(sender As Object, e As EventArgs) Handles Button_OK.Click
        shutdownInitiated = False
        InitiateShutdown(0, True)
        MessageBox.Show("System shutdown canceled.")
    End Sub

    Private Sub InitiateShutdown(delaySeconds As Integer, Optional isAbort As Boolean = False)
        Try
            Dim args = If(isAbort, "/a", $"/s /t {delaySeconds}")
            Dim psi As New ProcessStartInfo("shutdown", args) With {
                .WindowStyle = ProcessWindowStyle.Hidden, .CreateNoWindow = True, .UseShellExecute = True
            }
            Process.Start(psi)
            Console.WriteLine(If(isAbort, "Shutdown canceled", $"Shutdown initiated: {delaySeconds} seconds later"))
        Catch ex As Exception
            MessageBox.Show($"Shutdown execution failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Console.WriteLine($"Shutdown error: {ex.Message}")
        End Try
    End Sub

    Private Sub Button_config_Click(sender As Object, e As EventArgs) Handles Button_config.Click
        If Not File.Exists(CONFIG_PATH) Then
            ' No file: Show input dialog directly
            ShowConfigDialog()
        Else
            ' File exists: Verify normal password then show input dialog
            Dim verifyPassword = ShowPasswordInputBox("Enter normal password to change settings:", "Password Verification", 60)
            If verifyPassword = normalPassword Then
                ShowConfigDialog()
            Else
                MessageBox.Show("Password mismatch. Settings change canceled.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        End If
    End Sub

    Private Sub ShowConfigDialog()
        Dim configForm As New Form With {
        .Width = 500, .Height = 400, .Text = "Change Settings", .FormBorderStyle = FormBorderStyle.FixedDialog,
        .StartPosition = FormStartPosition.CenterScreen, .MinimizeBox = False, .MaximizeBox = False
    }
        Dim lblNormal As New Label With {.Left = 20, .Top = 20, .Text = "Normal Password:", .AutoSize = True}
        Dim txtNormal As New TextBox With {.Left = 20, .Top = 45, .Width = 440, .PasswordChar = "*"c}
        If File.Exists(CONFIG_PATH) Then txtNormal.Text = normalPassword ' Show existing value (masked)
        Dim lblEmergency As New Label With {.Left = 20, .Top = 80, .Text = "Emergency Password:", .AutoSize = True}
        Dim txtEmergency As New TextBox With {.Left = 20, .Top = 105, .Width = 440, .PasswordChar = "*"c}
        If File.Exists(CONFIG_PATH) Then txtEmergency.Text = emergencyPassword
        Dim lblWebhook As New Label With {.Left = 20, .Top = 150, .Text = "Discord Webhook URL:", .AutoSize = True}
        Dim txtWebhook As New TextBox With {.Left = 20, .Top = 175, .Width = 440, .Multiline = True, .Height = 60}
        If File.Exists(CONFIG_PATH) Then txtWebhook.Text = webhookUrl
        Dim btnSave As New Button With {.Text = "Save", .Left = 300, .Top = 250, .DialogResult = DialogResult.OK}
        Dim btnCancel As New Button With {.Text = "Cancel", .Left = 400, .Top = 250, .DialogResult = DialogResult.Cancel}
        configForm.Controls.AddRange({lblNormal, txtNormal, lblEmergency, txtEmergency, lblWebhook, txtWebhook, btnSave, btnCancel})
        configForm.AcceptButton = btnSave
        configForm.CancelButton = btnCancel
        If configForm.ShowDialog() = DialogResult.OK Then
            Try
                Dim content = $"{txtNormal.Text.Trim()}{vbCrLf}{txtEmergency.Text.Trim()}{vbCrLf}{txtWebhook.Text.Trim()}"
                File.WriteAllText(CONFIG_PATH, content)
                ' Reload
                Dim credentials = ReadCredentials(CONFIG_PATH)
                normalPassword = credentials.Item1
                emergencyPassword = credentials.Item2
                webhookUrl = credentials.Item3
                MessageBox.Show("Settings saved. Restarting the program.", "Completed", MessageBoxButtons.OK, MessageBoxIcon.Information)
                ' Program restart logic
                Process.Start(Application.ExecutablePath)
                Application.Exit()
            Catch ex As Exception
                MessageBox.Show("Error saving settings: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub
End Class