Imports System.IO
Imports System.Text
Imports System.Windows.Forms
Imports System.Threading

Public Class form_Hm2Flac3D

    Private _message As StringBuilder

    Private Sub form_Hm2Flac3D_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _message = New StringBuilder
        _message.AppendLine("******** CONVERT INP CODE(IMPORTED FROM HYPERMESH) TO FLAC3D ********")
        LabelHello.Text = _message.ToString()
        _message.AppendLine()
    End Sub


    Private Sub buttonTransForm_Click(sender As Object, e As EventArgs) Handles buttonTransForm.Click

        Dim pathZ As String = TextBox_zonesInp.Text
        Dim pathS As String = TextBox_structuresInp.Text

        Dim thdZone As Thread
        Dim thdSel As Thread

        ' 先生成土体的flac文件
        If Not String.IsNullOrEmpty(pathZ) Then
            If String.Compare(Path.GetExtension(pathZ), ".inp", ignoreCase:=True) <> 0 Then
                MessageBox.Show("土体网格文件格式不对")
                Return
            End If

            If Not File.Exists(pathZ) <> 0 Then
                MessageBox.Show("指定位置的土体网格文件不存在")
                Return
            End If

            thdZone = New Thread(AddressOf Export2Zone)
            thdZone.Start(pathZ)

        End If

        ' 再生成结构的flac文件
        If Not String.IsNullOrEmpty(pathS) Then
            If String.Compare(Path.GetExtension(pathS), ".inp", ignoreCase:=True) <> 0 Then
                MessageBox.Show("结构网格文件格式不对")
                Return
            End If

            If Not File.Exists(pathS) <> 0 Then
                MessageBox.Show("指定位置的结构网格文件不存在")
                Return
            End If

            thdSel = New Thread(AddressOf Export2Sel)
            thdSel.Start(pathS)

        End If

        '
        If thdZone IsNot Nothing Then thdZone.Join()
        If thdSel IsNot Nothing Then thdSel.Join()

        _message.AppendLine()
        _message.AppendLine("转换完成")

        LabelHello.Text = _message.ToString()
    End Sub


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
        Dim hmSel = New Hm2Structure(sr_inp, sw_Sel,_message)
        hmSel.ReadFile()
        ' --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        '操作完成后关闭资源
        sr_inp.Close()
        sw_Sel.Close()
        Fileinp.Close()
        Fileflc_Sel.Close()
        _message.AppendLine("******** 结构网格数据提取完成 ********")

    End Sub

    Private Sub form_Hm2Flac3D_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        _message = Nothing
    End Sub
End Class