Imports System.Text.RegularExpressions
Imports System.IO
Imports Microsoft.Office.Interop
Imports Microsoft.Office.Interop.Excel

Module ExtractFlac3dResult

#Region "   ---   Declarations"

    ''' <summary>
    ''' 进行数据提取的那此.dat文本
    ''' </summary>
    ''' <remarks></remarks>
    Dim sr_dat As StreamReader
    ''' <summary>
    ''' 是否是按编辑模式进行数据提取。
    ''' 在编辑模式下，文件的位置，Abaqus中的单元类型与Flac3d中的单元类型的对应关系等都是可以人工指定的。
    ''' </summary>
    ''' <remarks></remarks>
    Private blnEditMode As Boolean
    Private ExcelApp As Excel.Application
    Private WithEvents wkbk As Excel.Workbook
    ''' <summary>
    ''' 第一条计算结果数据（数值）所在的行号
    ''' </summary>
    ''' <remarks></remarks>
    Const cstRow_FirstData As Byte = 2

    ''' <summary>
    ''' 指定文件夹中所有可以用来进行数据提取的文档
    ''' </summary>
    ''' <remarks></remarks>
    Private Path_Data As New List(Of String)

    ''' <summary>
    ''' 当前所读取到的行号
    ''' </summary>
    ''' <remarks></remarks>
    Private LineNum As UInteger = 0
#End Region

    Sub Main()
        Dim path_Wkbk As String
        Console.WriteLine("******** Extract data from FLAC3D result file. (Zengfy 2015-12-18) ********" & vbCrLf &
                          "")
        ' 确定文件路径
        '默认模式
        Console.WriteLine("Choose the way you want to start the extraction: ")
        Console.WriteLine("1. Input the file path of the dat file;")
        Console.WriteLine("2. Type the file name if this file is in the application folder;")
        Console.WriteLine("3. Press ""Enter"" and search all the .dat files in the application folder.")
        Dim dirApp As String = My.Application.Info.DirectoryPath

        Dim reply As String = Console.ReadLine
        If reply.Trim = "" Then
            '直接搜索程序所在文件夹中的第一个.dat文件
            Dim files As String() = Directory.GetFiles(dirApp)
            For Each Fl As String In files
                If String.Compare(Path.GetExtension(Fl), ".dat", True) = 0 Then
                    Path_Data.Add(Fl)
                End If
            Next
        Else
            If Path.GetDirectoryName(reply) <> "" Then '说明输入的是文件的绝对路径
                If File.Exists(reply) Then
                    Path_Data.Add(reply)
                Else
                    Console.WriteLine("Can not find the specified file.")
                    Console.ReadLine()
                    Exit Sub
                End If
            Else ' 说明输入的是文件的相对路径，即相对于当前这个.exe程序所在的文件夹
                Dim CombinedFile = Path.Combine(dirApp, reply)
                If File.Exists(CombinedFile) Then
                    Path_Data.Add(CombinedFile)
                Else
                    Console.WriteLine("Can not find the specified file.")
                    Console.ReadLine()
                    Exit Sub
                End If
            End If
        End If
        '保存数据的Excel文档    
        If Path_Data.Count > 0 Then
            If File.Exists(Path_Data.Item(0)) Then
                Dim dirData As String = Path.GetDirectoryName(Path_Data.Item(0))
                path_Wkbk = Path.Combine(dirData, "Flac3D-ResultData.xlsx")
            Else
                Console.WriteLine("Can not find the specified file.")
                Console.ReadLine()
                Exit Sub
            End If
        Else
            Console.WriteLine("Can not find the specified file.")
            Console.ReadLine()
            Exit Sub
        End If

        ' ----------------------------------  开始提取数据 -------------------------------------------------------------------
        Try
            For DataFileNum As Short = 0 To Path_Data.Count - 1
                Dim path_Datafile As String = Path_Data.Item(DataFileNum)  ' 进行数据提取的这个.dat文件的绝对路径
                Console.WriteLine("Extracting data from ： {0}", path_Datafile)
                ' 打开文件
                Dim datFile As FileStream = File.Open(path_Datafile, FileMode.Open)
                sr_dat = New StreamReader(datFile)
                ' --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                Dim AllData As New List(Of DataResultList)  '某一个结果文本中的所有数据，在List中的第一项用来记录此类
                Try
                    AllData = ReadFile(sr_dat)    ' 读取数据
                Catch ex As Exception
                    Console.WriteLine("Error : Data in the specified file ""{0}""can not be extracted correctly.", path_Datafile)
                    Console.ReadLine()
                    Continue For
                End Try
                ' ------------------- 将数据写入Excel工作簿中 -------------------------------------------------------------------------------------------------------------------------
                If AllData.Count > 0 Then
                    '打开工作簿
                    If wkbk Is Nothing Then
                        If File.Exists(path_Wkbk) Then
                            wkbk = GetObject(path_Wkbk)
                            ExcelApp = wkbk.Application
                        Else
                            If ExcelApp Is Nothing Then
                                ExcelApp = New Application
                            End If
                            wkbk = ExcelApp.Workbooks.Add()
                            wkbk.SaveAs(path_Wkbk)
                        End If
                    Else
                        ExcelApp = wkbk.Application
                    End If
                    Dim filename As String = Path.GetFileNameWithoutExtension(path_Datafile)

                    ' ------------------------- 指定要将数据写入哪一个工作表 ---------------------------
                    Dim sht As Excel.Worksheet = Nothing

                    For Each testSht As Worksheet In wkbk.Worksheets
                        If String.Compare(testSht.Name, filename, True) = 0 Then
                            sht = testSht
                            Exit For
                        End If
                    Next
                    If sht Is Nothing Then
                        sht = wkbk.Worksheets.Add()
                        sht.Name = filename
                    End If
                    ExcelApp.ScreenUpdating = False
                    WriteDataToExcel(AllData, sht)    ' 将数据写入Excel工作簿中
                    ExcelApp.ScreenUpdating = True
                    ' ---------------------- 设置此Worksheet的界面效果 ----------------------
                    sht.Activate()
                    With ExcelApp.Windows.Item(wkbk.Name)
                        .Activate()
                        ' 窗口的拆分与冻结
                        .SplitRow = 1
                        .SplitColumn = 4
                        .FreezePanes = True
                    End With
                End If
                '' --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                '操作完成后关闭资源
                sr_dat.Close()
                datFile.Close()
            Next
        Finally
            If wkbk IsNot Nothing Then
                wkbk.Save()
            End If
            If ExcelApp IsNot Nothing Then
                ExcelApp.ScreenUpdating = True
            End If
        End Try

        Console.WriteLine(vbCrLf & "******** 数据提取完成 ********")

        With ExcelApp
            If .Visible = True Then
                If .WindowState = XlWindowState.xlMinimized Then
                    .WindowState = XlWindowState.xlMaximized
                End If
            Else
                wkbk.Save()
                wkbk.Close()
            End If
        End With
        Console.ReadLine()
        ' 界面UI
    End Sub

    ''' <summary>
    ''' 读取数据,并返回在读取数据的过程中是否出错。
    ''' </summary>
    ''' <param name="Reader"></param>
    ''' <returns>如果数据提取成功，则返回True，否则返回False</returns>
    ''' <remarks>在读取数据时，每一个生成单元的函数中，都会返回最后跳出循环的那个字符串，如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。</remarks>
    Private Function ReadFile(Reader As StreamReader) As List(Of DataResultList)
        Dim strLine As String
        Dim blockData As DataResultList
        Dim AllData As New List(Of DataResultList)
        '
        strLine = Reader.ReadLine()
        LineNum += 1
        While strLine IsNot Nothing
            If IsData(strLine) Then
                blockData = GetData(Reader, strLine)
                AllData.Add(blockData)
            End If
            strLine = Reader.ReadLine()
            LineNum += 1

        End While

        Return AllData
    End Function

    ''' <summary>
    ''' 提取一个数据块中的所有数据
    ''' </summary>
    ''' <param name="sr"></param>
    ''' <param name="strLine">这一行字符必须要是一个数据块中的第一行数据</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetData(sr As StreamReader, strLine As String) As DataResultList
        Dim nodeId As New List(Of Long)
        Dim x As New List(Of Double)
        Dim y As New List(Of Double)
        Dim z As New List(Of Double)
        Dim strData As String()
        strData = strLine.Split({"("c, ","c, ","c, ")"c})

        Do
            nodeId.Add(Long.Parse(strData(0)))
            x.Add(Double.Parse(strData(1)))
            y.Add(Double.Parse(strData(2)))
            z.Add(Double.Parse(strData(3)))
            '
            strLine = sr.ReadLine()
            LineNum += 1
            If strLine Is Nothing Then
                ' 如果 strLine 的值为 Nothing，说明已经读到了文本文件的结尾了。
                ' Console.WriteLine("Nothing 所在的行号为： " & LineNum)
                Exit Do
            End If
            strData = strLine.Split({"("c, ","c, ","c, ")"c})
        Loop While IsData(strLine)

        ' 此时的strLine为不满足数据格式的一行字符串
        Dim result As DataResultList
        With result
            .NodeId = nodeId
            .X = x
            .Y = y
            .Z = z
            .strEscape = strLine
        End With
        Return result
    End Function

    ''' <summary>
    ''' 精确判断某一行字符是否是一行数据
    ''' </summary>
    ''' <param name="strLine"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function IsData(strLine As String) As Boolean
        Dim res As Boolean = True
        Dim Data As String() = strLine.Split({"("c, ","c, ","c, ")"c})
        If Data.Length >= 4 Then
            '第一个数值是否是一个整数
            Dim numLong As Long
            If Not Long.TryParse(Data(0), numLong) Then
                res = False
                Return res
            End If
            '后面三个数值是否是一个
            Dim numDouble As Double
            If Not Double.TryParse(Data(1), numDouble) Then
                res = False
                Return res
            End If
            If Not Double.TryParse(Data(2), numDouble) Then
                res = False
                Return res
            End If
            If Not Double.TryParse(Data(3), numDouble) Then
                res = False
                Return res
            End If
        Else
            res = False
            Return res
        End If
        Return res
    End Function


    ''' <summary>
    ''' 将一个dat文本中的所有数据写入对应的Worksheet中。
    ''' </summary>
    ''' <param name="AllData">一个dat文本中的所有数据。注意：在一个文本中，
    ''' 记录节点位置的坐标点的数量与记录节点位移的坐标点的数量并不一定是相同的。</param>
    ''' <param name="wkSheet">要进行写入的工作表</param>
    ''' <remarks></remarks>
    Private Sub WriteDataToExcel(AllData As List(Of DataResultList), wkSheet As Excel.Worksheet)
        Dim app As Excel.Application = wkSheet.Application
        Dim BlockCount As Integer = AllData.Count
        Dim RowsCount As Integer ' 每一个数据块的数据行数
        Dim results As DataResultList
        With wkSheet
            ' 先写入测点的位置信息
            results = AllData.Item(0)
            RowsCount = results.NodeId.Count
            .Range("A1:D1").Value = {"ID", "X", "Y", "Z"}
            .Range(.Cells(cstRow_FirstData, 1), .Cells(cstRow_FirstData + RowsCount - 1, 1)).Value = app.WorksheetFunction.Transpose(results.NodeId.ToArray)
            .Range(.Cells(cstRow_FirstData, 2), .Cells(cstRow_FirstData + RowsCount - 1, 2)).Value = app.WorksheetFunction.Transpose(results.X.ToArray)
            .Range(.Cells(cstRow_FirstData, 3), .Cells(cstRow_FirstData + RowsCount - 1, 3)).Value = app.WorksheetFunction.Transpose(results.Y.ToArray)
            .Range(.Cells(cstRow_FirstData, 4), .Cells(cstRow_FirstData + RowsCount - 1, 4)).Value = app.WorksheetFunction.Transpose(results.Z.ToArray)
            '写入X数据
            Dim startColumn = 5
            For BlockNum As Integer = 1 To BlockCount - 1
                startColumn += 1
                results = AllData.Item(BlockNum)
                ' 将
                Dim SortedRes As DataResultArray = SortResult(results, AllData.Item(0).NodeId)
                RowsCount = SortedRes.Count
                .Range(.Cells(cstRow_FirstData, startColumn), .Cells(cstRow_FirstData + RowsCount - 1, startColumn)).Value = SortedRes.X ' app.WorksheetFunction.Transpose(results.X.ToArray)
                .Range(.Cells(cstRow_FirstData, startColumn + BlockCount), .Cells(cstRow_FirstData + RowsCount - 1, startColumn + BlockCount)).Value = SortedRes.Y '  app.WorksheetFunction.Transpose(results.Y.ToArray)
                .Range(.Cells(cstRow_FirstData, startColumn + BlockCount * 2), .Cells(cstRow_FirstData + RowsCount - 1, startColumn + BlockCount * 2)).Value = SortedRes.Z '  app.WorksheetFunction.Transpose(results.Z.ToArray)
            Next
            Try
                .Activate()
                .Range(.Cells(1, 1), .Cells(1, startColumn + BlockCount * 2)).Select()
                If .AutoFilterMode Then
                    app.Selection.AutoFilter()
                    app.Selection.AutoFilter()
                Else
                    app.Selection.AutoFilter()
                End If
            Catch ex As Exception
                Console.ForegroundColor = ConsoleColor.Yellow
                Console.WriteLine("Warnning : Can not set the autofilter for the worksheet: {0}, but it will not affect the extraction of data.", wkSheet.Name)
                Console.ForegroundColor = ConsoleColor.Gray
            End Try

        End With
    End Sub

    ''' <summary>
    ''' 对源数据进行排版，来让其中的Id集体与指定的目标Id集合相对应。
    ''' </summary>
    ''' <param name="SourceResult">要进行排版的数据源</param>
    ''' <param name="DestinationIdList">要匹配到的目标Id集合</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function SortResult(ByVal SourceResult As DataResultList, ByVal DestinationIdList As List(Of Long)) As DataResultArray
        Dim NodeCount As UInteger = DestinationIdList.Count
        Dim NodeId(0 To NodeCount - 1, 0 To 0) As Object
        Dim X(0 To NodeCount - 1, 0 To 0) As Object
        Dim Y(0 To NodeCount - 1, 0 To 0) As Object
        Dim Z(0 To NodeCount - 1, 0 To 0) As Object

        '---------------------- 开始进行数据排版 ------------------------------
        Dim SourceIndex As UInteger
        Dim DestiIndex As UInteger
        '
        With SourceResult
            Dim SourceNode As List(Of Long) = .NodeId
            For SourceIndex = 0 To SourceNode.Count - 1
                DestiIndex = DestinationIdList.IndexOf(SourceNode.Item(SourceIndex), DestiIndex)
                If DestiIndex < 0 Then
                    ' 搜索原理：
                    ' 这里有一个前提假设：在SourceNode与DestinationIdList中，节点的Id都是从小到大排列的。
                    ' 所以，一旦SourceNode中的节点在DestinationIdList集体中找不到对应的匹配节点号，则SourceNode中后面的节点都不会找到匹配的节点号了。
                    Exit For
                Else
                    ' 将数据放置在对应的位置上
                    NodeId(DestiIndex, 0) = .NodeId.Item(SourceIndex)
                    X(DestiIndex, 0) = .X.Item(SourceIndex)
                    Y(DestiIndex, 0) = .Y.Item(SourceIndex)
                    Z(DestiIndex, 0) = .Z.Item(SourceIndex)
                End If
            Next
        End With

        '---------------------------------------------------------------------
        Dim Res As New DataResultArray
        With Res
            .NodeId = NodeId
            .X = X
            .Y = Y
            .Z = Z
        End With
        Return Res
    End Function

    Private Sub wkbk_BeforeClose(ByRef Cancel As Boolean) Handles wkbk.BeforeClose
        wkbk = Nothing
    End Sub

    ''' <summary>
    ''' 一个数据块中所有的数据，以及最后退出数据块的那一行字符串
    ''' </summary>
    ''' <remarks>此类中的NodeId、X、Y、Z这四个集合中的元素个数肯定是相同的。</remarks>
    Private Structure DataResultList
        ''' <summary>
        ''' 在一个数据块中，所有节点的Id号（从Flac3d中输出后，这些节点的Id号都是按从小到大的顺序排列的）。
        ''' </summary>
        ''' <remarks></remarks>
        Public NodeId As List(Of Long)
        Public X As List(Of Double)
        Public Y As List(Of Double)
        Public Z As List(Of Double)
        Public strEscape As String
    End Structure

    ''' <summary>
    ''' 一个数据块中所有的数据，以及最后退出数据块的那一行字符串.
    ''' 此类中的NodeId、X、Y、Z这四个二维数组必须都是列向量，不能是行向量。
    ''' </summary>
    ''' <remarks>此类中的NodeId、X、Y、Z这四个集合中的元素个数肯定是相同的。</remarks>
    Private Structure DataResultArray

        ''' <summary>
        ''' 所有元素的个数
        ''' </summary>
        ''' <remarks></remarks>
        Public ReadOnly Property Count As UInteger
            Get
                Return NodeId.Length
            End Get
        End Property

        ''' <summary>
        ''' 此列向量中，可以有空数据，所以其数据类型为Object，而不是Long。
        ''' </summary>
        ''' <remarks></remarks>
        Public NodeId(,) As Object
        Public X(,) As Object
        Public Y(,) As Object
        Public Z(,) As Object
    End Structure


End Module
