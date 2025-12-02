<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Label1 = New Label()
        Check_cam = New CheckBox()
        Button_OK = New Button()
        Check_usb = New CheckBox()
        Button_config = New Button()
        SuspendLayout()
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(12, 9)
        Label1.Name = "Label1"
        Label1.Size = New Size(132, 15)
        Label1.TabIndex = 0
        Label1.Text = "Monitoring USB drive..."
        ' 
        ' Check_cam
        ' 
        Check_cam.AutoSize = True
        Check_cam.Checked = True
        Check_cam.CheckState = CheckState.Checked
        Check_cam.Enabled = False
        Check_cam.Location = New Point(216, 5)
        Check_cam.Name = "Check_cam"
        Check_cam.Size = New Size(75, 19)
        Check_cam.TabIndex = 1
        Check_cam.Text = "Use cam."
        Check_cam.UseVisualStyleBackColor = True
        ' 
        ' Button_OK
        ' 
        Button_OK.Location = New Point(109, 30)
        Button_OK.Name = "Button_OK"
        Button_OK.Size = New Size(80, 29)
        Button_OK.TabIndex = 2
        Button_OK.Text = "PW Test"
        Button_OK.UseVisualStyleBackColor = True
        Button_OK.Visible = False
        ' 
        ' Check_usb
        ' 
        Check_usb.AutoSize = True
        Check_usb.Checked = True
        Check_usb.CheckState = CheckState.Checked
        Check_usb.Location = New Point(216, 30)
        Check_usb.Name = "Check_usb"
        Check_usb.Size = New Size(90, 19)
        Check_usb.TabIndex = 3
        Check_usb.Text = "Format USB"
        Check_usb.UseVisualStyleBackColor = True
        ' 
        ' Button_config
        ' 
        Button_config.Location = New Point(12, 30)
        Button_config.Name = "Button_config"
        Button_config.Size = New Size(80, 29)
        Button_config.TabIndex = 4
        Button_config.Text = "Config"
        Button_config.UseVisualStyleBackColor = True
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(312, 66)
        Controls.Add(Button_config)
        Controls.Add(Check_usb)
        Controls.Add(Button_OK)
        Controls.Add(Check_cam)
        Controls.Add(Label1)
        FormBorderStyle = FormBorderStyle.None
        Name = "Form1"
        ShowIcon = False
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterScreen
        Text = "chk USB"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents Check_cam As CheckBox
    Friend WithEvents Button_OK As Button
    Friend WithEvents Check_usb As CheckBox
    Friend WithEvents Button_config As Button

End Class
