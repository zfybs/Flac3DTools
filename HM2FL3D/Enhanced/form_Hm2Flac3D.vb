Imports System.IO
Imports System.Text
Imports System.Windows.Forms
Imports System.Threading
Imports System.ComponentModel
Imports System.Runtime.InteropServices

Public Class form_Hm2Flac3D

    Private _message As StringBuilder

    Private WithEvents worker As BackgroundWorker

#Region "---   窗口的加载与关闭"

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        worker = New BackgroundWorker
        worker.WorkerSupportsCancellation = True
        worker.WorkerReportsProgress = True
        '
        ProgressBar1.MarqueeAnimationSpeed = 10
        ProgressBar1.Visible = False
        '

    End Sub

    Private Sub form_Hm2Flac3D_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _message = New StringBuilder
        _message.AppendLine("******** CONVERT INP CODE(EXPORTED FROM HYPERMESH) TO FLAC3D ********")
        LabelHello.Text = _message.ToString()

        ' 设置初始的 inp 文件位置
        Dim exeDire As String = My.Application.Info.DirectoryPath
        TextBox_zonesInp.Text = Path.Combine(exeDire, "zones.inp")
        TextBox_structuresInp.Text = Path.Combine(exeDire, "structures.inp")
    End Sub

    Private Sub ButtonClose_Click(sender As Object, e As EventArgs) Handles ButtonClose.Click
        Me.Close()
    End Sub

    Private Sub form_Hm2Flac3D_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        _message = Nothing
        '
        worker.CancelAsync()
        worker.Dispose()
        worker = Nothing
    End Sub

#End Region

#Region "---   界面事件处理"

    Private Sub DoseOff()
        ButtonClose.Enabled = False
        ButtonChooseZones.Enabled = False
        ButtonChooseStructures.Enabled = False
        ButtonNoLiner.Enabled = False
        TextBox_structuresInp.Enabled = False
        TextBox_zonesInp.Enabled = False
    End Sub

    Private Sub WarmUp()
        ButtonClose.Enabled = True
        ButtonChooseZones.Enabled = True
        ButtonChooseStructures.Enabled = True
        ButtonNoLiner.Enabled = True
        TextBox_structuresInp.Enabled = True
        TextBox_zonesInp.Enabled = True
    End Sub

    ' 文件路径的拖拽
    Private Sub TextBox_zonesInp_DragEnter(sender As Object, e As DragEventArgs) Handles TextBox_zonesInp.DragEnter, TextBox_structuresInp.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            ' There is text. Allow copy.
            e.Effect = DragDropEffects.Copy
        Else
            ' There is no text. Prohibit drop.
            e.Effect = DragDropEffects.None
        End If

    End Sub

    Private Sub TextBox_zonesInp_DragDrop(sender As Object, e As DragEventArgs) Handles TextBox_zonesInp.DragDrop, TextBox_structuresInp.DragDrop
        Dim txt As TextBox = DirectCast(sender, TextBox)
        Dim FileDrop As String() = e.Data.GetData(DataFormats.FileDrop)
        ' DoSomething with the Files or Directories that are droped in.
        Dim filepath As String = FileDrop(0)
        If String.Compare(Path.GetExtension(filepath), ".inp", ignoreCase:=True) = 0 Then
            txt.Text = filepath
        Else
            txt.Text = "请确保文件后缀名为"".inp"""
        End If

    End Sub

    Private Sub ButtonChooseZones_Click(sender As Object, e As EventArgs) Handles ButtonChooseZones.Click, ButtonChooseStructures.Click
        Dim btn As Button = DirectCast(sender, Button)
        Dim path As String = ChooseInpFile("选择对应的 inp 文件")

        ' 写入数据
        If Not String.IsNullOrEmpty(path) Then
            If btn.Name = ButtonChooseZones.Name Then
                TextBox_zonesInp.Text = path
            End If
            If btn.Name = ButtonChooseStructures.Name Then
                TextBox_structuresInp.Text = path
            End If
        End If
    End Sub

    ''' <summary> 通过选择文件对话框选择要进行数据提取的Excel文件 </summary>
    ''' <returns> 要进行数据提取的Excel文件的绝对路径 </returns>
    Public Shared Function ChooseInpFile(title As String) As String

        Dim ofd As New OpenFileDialog()
        With ofd
            .Title = title
            .CheckFileExists = True
            .AddExtension = True
            .Filter = "inp文件(*.inp)| *.inp"
            .FilterIndex = 2
            .Multiselect = False
        End With

        If ofd.ShowDialog() = DialogResult.OK Then
            Return If(ofd.FileName.Length > 0, ofd.FileName, "")
        End If
        Return ""
    End Function

    Private Sub TextBox_zonesInp_Enter(sender As Object, e As EventArgs) Handles TextBox_zonesInp.MouseEnter, TextBox_structuresInp.MouseEnter
        Dim txt = DirectCast(sender, TextBox)
        txt.SelectAll()
        txt.Focus()
    End Sub

#End Region

#Region "---    Liner 模式"

    Private Sub buttonTransForm_Click(sender As Object, e As EventArgs) Handles buttonTransForm.Click
        _message.AppendLine()
        If Not worker.IsBusy Then

            Call DoseOff()

            ProgressBar1.Visible = True

            worker.RunWorkerAsync()
        End If
    End Sub

    Private Sub worker_DoWork(sender As Object, e As DoWorkEventArgs) Handles worker.DoWork
        Dim thdZone As Thread
        Dim thdSel As Thread

        ' 先生成土体的flac文件
        If CheckZone() Then
            Dim pathZ As String = TextBox_zonesInp.Text
            thdZone = New Thread(AddressOf Export2Zone)
            thdZone.Start(pathZ)
        Else
            _message.AppendLine("******** 土体网格数据提取失败 ********")
        End If

        ' 再生成结构的flac文件
        If CheckSel() Then
            Dim pathS As String = TextBox_structuresInp.Text
            thdSel = New Thread(AddressOf Export2Sel)
            thdSel.Start(pathS)
        Else
            _message.AppendLine("******** 结构网格数据提取失败 ********")
        End If

        '
        If thdZone IsNot Nothing Then thdZone.Join()
        If thdSel IsNot Nothing Then thdSel.Join()
    End Sub

    Private Function CheckZone() As Boolean
        Dim pathZ As String = TextBox_zonesInp.Text

        If String.IsNullOrEmpty(pathZ) Then
            Return False
        End If

        If Not File.Exists(pathZ) <> 0 Then
            _message.AppendLine("指定位置的土体网格文件不存在")
            Return False
        End If

        ' 先生成土体的flac文件
        If String.Compare(Path.GetExtension(pathZ), ".inp", ignoreCase:=True) <> 0 Then
            _message.AppendLine("土体网格文件格式不对")
            Return False
        End If


        Return True
    End Function
    Private Function CheckSel() As Boolean
        Dim pathS As String = TextBox_structuresInp.Text

        If String.IsNullOrEmpty(pathS) Then
            Return False
        End If

        If Not File.Exists(pathS) <> 0 Then
            _message.AppendLine("指定位置的结构网格文件不存在")
            Return False
        End If

        If String.Compare(Path.GetExtension(pathS), ".inp", ignoreCase:=True) <> 0 Then
            _message.AppendLine("结构网格文件格式不对")
            Return False
        End If


        Return True
    End Function

    Private Sub Export2Zone(pathZ As String)

        Dim zoneTxt As String = Path.Combine((New FileInfo(pathZ)).DirectoryName, "zones.flac3d")

        ' 打开文本
        Dim Fileinp As FileStream = File.Open(pathZ, FileMode.Open)
        Dim Fileflc_Zone As FileStream = File.Create(zoneTxt)
        Dim sr_inp = New StreamReader(Fileinp)
        Dim sw_Zone = New StreamWriter(Fileflc_Zone)


        ' --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        Dim hmZone = New Hm2Zone(sr_inp, sw_Zone, _message)
        hmZone.ReadFile()
        ' --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        '操作完成后关闭资源
        sr_inp.Close()
        sw_Zone.Close()
        Fileinp.Close()
        Fileflc_Zone.Close()
        _message.AppendLine("******** 土体网格数据提取完成 ********")

    End Sub

    Private Sub Export2Sel(pathS As String)
        Dim selTxt As String = Path.Combine((New FileInfo(pathS)).DirectoryName, "structures.dat")

        ' 打开文本
        Dim Fileinp As FileStream = File.Open(pathS, FileMode.Open)
        Dim Fileflc_Sel As FileStream = File.Create(selTxt)
        Dim sr_inp = New StreamReader(Fileinp)
        Dim sw_Sel = New StreamWriter(Fileflc_Sel)


        ' --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        Dim hmSel = New Hm2Structure(sr_inp, sw_Sel, _message)
        hmSel.ReadFile()
        ' --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        '操作完成后关闭资源
        sr_inp.Close()
        sw_Sel.Close()
        Fileinp.Close()
        Fileflc_Sel.Close()
        _message.AppendLine("******** 结构网格数据提取完成 ********")

    End Sub

    Private Sub worker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles worker.RunWorkerCompleted

        _message.AppendLine()
        _message.AppendLine("转换完成")

        LabelHello.Text = _message.ToString()
        ProgressBar1.Visible = False
        Call WarmUp()
    End Sub

#End Region

#Region "---   通过 Console 运行无 Liner 的转换程序"

    Private Sub ButtonNoLiner_Click(sender As Object, e As EventArgs) Handles ButtonNoLiner.Click

        Me.Hide()

        Call Hm2Flac3DHandler.AllocConsole()

        Hm2Flac3D.Main()

        ' 一般情况下，当控制台被手动点击关闭（而不是通过 FreeConsole 来释放控制台）后，整个程序的进程就结束了，这里为了保险起见，再强制关闭一下。
        Me.Close()
    End Sub

#End Region

End Class