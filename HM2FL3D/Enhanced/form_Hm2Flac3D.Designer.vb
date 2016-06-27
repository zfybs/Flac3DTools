<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class form_Hm2Flac3D
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(form_Hm2Flac3D))
        Me.buttonTransForm = New System.Windows.Forms.Button()
        Me.TextBox_zonesInp = New System.Windows.Forms.TextBox()
        Me.TextBox_structuresInp = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.ButtonChooseZones = New System.Windows.Forms.Button()
        Me.ButtonChooseStructures = New System.Windows.Forms.Button()
        Me.LabelHello = New System.Windows.Forms.Label()
        Me.SuspendLayout
        '
        'buttonTransForm
        '
        Me.buttonTransForm.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.buttonTransForm.Location = New System.Drawing.Point(356, 261)
        Me.buttonTransForm.Name = "buttonTransForm"
        Me.buttonTransForm.Size = New System.Drawing.Size(75, 23)
        Me.buttonTransForm.TabIndex = 0
        Me.buttonTransForm.Text = "转换"
        Me.buttonTransForm.UseVisualStyleBackColor = true
        '
        'TextBox_zonesInp
        '
        Me.TextBox_zonesInp.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left)  _
            Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.TextBox_zonesInp.Location = New System.Drawing.Point(11, 184)
        Me.TextBox_zonesInp.Name = "TextBox_zonesInp"
        Me.TextBox_zonesInp.Size = New System.Drawing.Size(338, 21)
        Me.TextBox_zonesInp.TabIndex = 1
        Me.TextBox_zonesInp.Text = "F:\ProgrammingCases\GitHubProjects\HM2FL3D\HM2FL3D\bin\Debug\zones.inp"
        '
        'TextBox_structuresInp
        '
        Me.TextBox_structuresInp.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left)  _
            Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.TextBox_structuresInp.Location = New System.Drawing.Point(12, 232)
        Me.TextBox_structuresInp.Name = "TextBox_structuresInp"
        Me.TextBox_structuresInp.Size = New System.Drawing.Size(338, 21)
        Me.TextBox_structuresInp.TabIndex = 2
        Me.TextBox_structuresInp.Text = "F:\ProgrammingCases\GitHubProjects\HM2FL3D\HM2FL3D\bin\Debug\structures.inp"
        '
        'Label1
        '
        Me.Label1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left),System.Windows.Forms.AnchorStyles)
        Me.Label1.AutoSize = true
        Me.Label1.Location = New System.Drawing.Point(12, 165)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(59, 12)
        Me.Label1.TabIndex = 3
        Me.Label1.Text = "zones.inp"
        '
        'Label2
        '
        Me.Label2.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left),System.Windows.Forms.AnchorStyles)
        Me.Label2.AutoSize = true
        Me.Label2.Location = New System.Drawing.Point(12, 214)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(89, 12)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "structures.inp"
        '
        'ButtonChooseZones
        '
        Me.ButtonChooseZones.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.ButtonChooseZones.Location = New System.Drawing.Point(355, 185)
        Me.ButtonChooseZones.Name = "ButtonChooseZones"
        Me.ButtonChooseZones.Size = New System.Drawing.Size(75, 23)
        Me.ButtonChooseZones.TabIndex = 0
        Me.ButtonChooseZones.Text = "选择"
        Me.ButtonChooseZones.UseVisualStyleBackColor = true
        '
        'ButtonChooseStructures
        '
        Me.ButtonChooseStructures.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.ButtonChooseStructures.Location = New System.Drawing.Point(355, 230)
        Me.ButtonChooseStructures.Name = "ButtonChooseStructures"
        Me.ButtonChooseStructures.Size = New System.Drawing.Size(75, 23)
        Me.ButtonChooseStructures.TabIndex = 0
        Me.ButtonChooseStructures.Text = "选择"
        Me.ButtonChooseStructures.UseVisualStyleBackColor = true
        '
        'LabelHello
        '
        Me.LabelHello.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom)  _
            Or System.Windows.Forms.AnchorStyles.Left)  _
            Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.LabelHello.Location = New System.Drawing.Point(9, 9)
        Me.LabelHello.Name = "LabelHello"
        Me.LabelHello.Size = New System.Drawing.Size(421, 149)
        Me.LabelHello.TabIndex = 3
        Me.LabelHello.Text = "******** CONVERT INP CODE(IMPORTED FROM HYPERMESH) TO FLAC3D ********"
        '
        'form_Hm2Flac3D
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6!, 12!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(443, 296)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.LabelHello)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.TextBox_structuresInp)
        Me.Controls.Add(Me.TextBox_zonesInp)
        Me.Controls.Add(Me.ButtonChooseStructures)
        Me.Controls.Add(Me.ButtonChooseZones)
        Me.Controls.Add(Me.buttonTransForm)
        Me.Icon = CType(resources.GetObject("$this.Icon"),System.Drawing.Icon)
        Me.MinimumSize = New System.Drawing.Size(459, 213)
        Me.Name = "form_Hm2Flac3D"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "form_Hm2Flac3D"
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub

    Friend WithEvents buttonTransForm As Windows.Forms.Button
    Friend WithEvents TextBox_zonesInp As Windows.Forms.TextBox
    Friend WithEvents TextBox_structuresInp As Windows.Forms.TextBox
    Friend WithEvents Label1 As Windows.Forms.Label
    Friend WithEvents Label2 As Windows.Forms.Label
    Friend WithEvents ButtonChooseZones As Windows.Forms.Button
    Friend WithEvents ButtonChooseStructures As Windows.Forms.Button
    Friend WithEvents LabelHello As Windows.Forms.Label
End Class
