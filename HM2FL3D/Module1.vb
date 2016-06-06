Imports System.Text.RegularExpressions
Imports System.IO
Module Hm2Flac3D
    Dim sr_inp As StreamReader
    Dim sw_Zone As StreamWriter
    Dim sw_Sel As StreamWriter
    ''' <summary>
    ''' 是否是按编辑模式进行数据提取。
    ''' 在编辑模式下，文件的位置，Abaqus中的单元类型与Flac3d中的单元类型的对应关系等都是可以人工指定的。
    ''' </summary>
    ''' <remarks></remarks>
    Private blnEditMode As Boolean
    ''' <summary>
    ''' 每一组Structural Element 的id号，此id号为全局的，各种不同类型的结构单元之间的id号也没有相同的。
    ''' </summary>
    ''' <remarks></remarks>
    Private SelId As Long = 1

    ''' <summary>
    ''' 一个全局的节点集合，其中包含了所有Liner单元中的节点，而且其中的节点编号没有重复。
    ''' </summary>
    ''' <remarks></remarks>
    Private listLinerNode As New SortedSet(Of Long)

    Sub Main()
        Dim path_inp As String
        Dim path_flacStructure As String
        Dim path_flacZone As String
        Console.WriteLine("******** CONVERT INP CODE(IMPORTED FROM HYPERMESH) TO FLAC3D ********")
        blnEditMode = getReply("Run this application in edit mode.")

        ' 确定文件路径
        If blnEditMode AndAlso (Not getReply("Use the defalt file path.")) Then
            Console.WriteLine("Input the file path of the inp file: ")
            path_inp = Console.ReadLine()
            If Not File.Exists(path_inp) Then
                Console.WriteLine("Error : Specified inp file not detected!")
                Console.Read()
                Exit Sub
            End If
            Dim d As String = Path.GetDirectoryName(path_inp)
            path_flacStructure = Path.Combine(d, "structures.dat")
            path_flacZone = Path.Combine(d, "Zones.Flac3D")
        Else                                                                     '默认模式
            Dim d As String = My.Application.Info.DirectoryPath
            path_inp = Path.Combine(d, "Hm2Flac3D.inp")
            If Not File.Exists(path_inp) Then
                Console.WriteLine("Error : Specified inp file not detected!")
                Console.Read()
                Exit Sub
            End If
            path_flacStructure = Path.Combine(d, "structures.dat")
            path_flacZone = Path.Combine(d, "Zones.Flac3D")
        End If

        ' 打开文件
        Dim Fileinp As FileStream = File.Open(path_inp, FileMode.Open)
        Dim Fileflc_Sel As FileStream = File.Create(path_flacStructure)
        Dim Fileflc_Zone As FileStream = File.Create(path_flacZone)
        sr_inp = New StreamReader(Fileinp)
        sw_Sel = New StreamWriter(Fileflc_Sel)
        sw_Zone = New StreamWriter(Fileflc_Zone)

        '写入头文件信息
        ' 用im zone.dat来在Flac3d中导入网格时，在zone.dat中可以先写入文件头信息，而structures.dat文件中，不能用“*”来写注释
        Dim strHeading As String
        strHeading = " * --------------------------------------------------" & vbCrLf &
            " *  INP (exported from Hypermesh) to FLAC3D " & vbCrLf &
            " *  Coded by Zengfy" & vbCrLf &
            " *  Latest update time: 2015/12/12 " & vbCrLf &
            " * --------------------------------------------------" & vbCrLf &
            "* Generated time: " & DateTime.Today.ToString("yyyy/MM/dd") & "   " & DateTime.Now.ToShortTimeString & vbCrLf &
            vbCrLf & "* GRIDPOINTS"
        sw_Zone.WriteLine(strHeading)

        ' --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        Dim blnSuccedd As Boolean = ReadFile(sr_inp, sw_Sel, sw_Zone)    ' 读取数据
        ' --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        '操作完成后关闭资源
        sr_inp.Close()
        sw_Sel.Close()
        sw_Zone.Close()
        Fileinp.Close()
        Fileflc_Sel.Close()
        Fileflc_Zone.Close()
        Console.WriteLine(vbCrLf & "******** 数据提取完成 ********")
        Console.Read()
    End Sub

    ''' <summary>
    ''' 读取数据,并返回在读取数据的过程中是否出错。
    ''' </summary>
    ''' <param name="sr_inp"></param>
    ''' <param name="sw_Sel"></param>
    ''' <param name="sw_Zone"></param>
    ''' <returns>如果数据提取成功，则返回True，否则返回False</returns>
    ''' <remarks>在读取数据时，每一个生成单元的函数中，都会返回最后跳出循环的那个字符串，如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。</remarks>
    Private Function ReadFile(sr_inp As StreamReader, sw_Sel As StreamWriter, sw_Zone As StreamWriter) As Boolean
        Dim pattern_ElementType As String = "\*ELEMENT,TYPE=(.+),ELSET=(.+)"  '大致结构为： *ELEMENT,TYPE=B31,ELSET=columns-c1
        Dim strLine As String = sr_inp.ReadLine()

        '第一步：输出节点
        Try
            While Not strLine.StartsWith("*NODE")
                strLine = sr_inp.ReadLine
            End While
        Catch ex As Exception
            Console.WriteLine("No Node is detected in the specified inp file!")
            Return False
        End Try
        strLine = Gen_Node(sr_inp, sw_Sel, sw_Zone)
        '第二步：输出单元
        Do
            '在Hypermesh导出的inp文件中，可以有很多个 *ELEMENT,TYPE=B31,ELSET=columns 这样的语句，它们是按hypermesh中的Component来进行分组的。
            If Regex.Match(strLine, pattern_ElementType).Success Then
                '如果
                Dim m As Match = Regex.Match(strLine, pattern_ElementType)
                Dim strEleType As String = m.Groups(1).Value
                Dim strComponentName As String = m.Groups(2).Value
                Dim tp As ElementType = GetElementType(strEleType, strComponentName)
                Select Case tp

                    ' 结构单元
                    Case ElementType.BEAM
                        strLine = Gen_Beam(sr_inp, sw_Sel, strComponentName)
                    Case ElementType.PILE
                        strLine = Gen_Pile(sr_inp, sw_Sel, strComponentName)
                    Case ElementType.SHELL3
                        strLine = Gen_Shell3(sr_inp, sw_Sel, strComponentName)
                    Case ElementType.Liner3
                        strLine = Gen_Liner3(sr_inp, sw_Sel, strComponentName)

                        ' Zone单元
                    Case ElementType.ZONE_B8
                        strLine = Gen_Zone_B8(sr_inp, sw_Zone, strComponentName)
                    Case ElementType.ZONE_W6
                        strLine = Gen_Zone_W6(sr_inp, sw_Zone, strComponentName)
                    Case ElementType.ZONE_T4
                        strLine = Gen_Zone_T4(sr_inp, sw_Zone, strComponentName)

                    Case Else 'Hypermesh中的类型在Flac3d中没有设置对应的类型
                        Dim strAsk As String = String.Format("Warning : Can not match element type "" {0} ""(in component {1}) with a corresponding type in Flac3D,", strEleType, strComponentName)
                        Console.WriteLine(strAsk)
                        '如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。
                        strLine = sr_inp.ReadLine()
                End Select
            Else
                '如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。
                strLine = sr_inp.ReadLine()
            End If
        Loop While (strLine IsNot Nothing) AndAlso (Not strLine.StartsWith("**HMASSEM"))

        ' -------------------------------------------------- 后续补充操作 -----------------------------------------------

        'sw_Sel.WriteLine("SEL NODE INIT XPOS ADD 0.0")   '将所有结构单元与土相连的节点位置创建接触

        ''当Liner单元用SelLiner创建时，它只与其side1方向的那一个Zone之间建立了link，此时要手动地为每一个linerNode与其side2方向的Zone之间建立Link。
        'For Each linerNode As Long In listLinerNode
        '    sw_Sel.WriteLine("SEL LINK id {0} {1} side2", linerNode + 10000000000, linerNode)
        'Next
        '' ---------------------------------------------------------------------------------------------------------------
        Return True
    End Function

    ''' <summary>
    ''' 创建节点
    ''' </summary>
    ''' <param name="sr">用来提取数据的inp文件</param>
    ''' <param name="sw_Sel">用来写入Structural element的节点的那个文本。</param>
    ''' <param name="sw_zone">用来写入zone单元的节点的那个文本。</param>
    ''' <remarks></remarks>
    Private Function Gen_Node(sr As StreamReader, sw_Sel As StreamWriter, sw_zone As StreamWriter) As String
        Dim strLine As String
        strLine = sr.ReadLine()  ' 大致的结构为： "        16,  10.0           ,  6.6666666666667,  0.0            "
        Do Until strLine.StartsWith("*")
            sw_Sel.WriteLine("SEL NODE cid  {0}", strLine) ' 大致的结构为： ' SEL NODE cid  1440016  0.216969418565193E+02 -0.531659539393860E+02 -0.161000000000000E+02
            sw_zone.WriteLine("G  {0}", strLine)
            strLine = sr.ReadLine()  ' 大致的结构为： "        16,  10.0           ,  6.6666666666667,  0.0            "
        Loop
        Return strLine
    End Function

#Region "  ---  生成不同类型的单元"

    ''' <summary>
    ''' 生成桩单元，并返回跳出循环的字符串
    ''' </summary>
    ''' <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
    ''' <remarks></remarks>
    Private Function Gen_Pile(sr As StreamReader, sw As StreamWriter, component As String) As String
        Dim pattern As String = "\s*(\d*),\s*(\d*),\s*(\d*)"
        Dim strLine As String
        Dim eleId As Long
        Dim node1 As Long, node2 As Long
        '
        strLine = sr.ReadLine()  ' 大致的结构为： "        16,  10.0           ,  6.6666666666667,  0.0            "
        Dim match As Match, groups As GroupCollection
        match = Regex.Match(strLine, pattern)
        Do While match.Success
            groups = match.Groups
            eleId = groups(1).Value
            node1 = groups(2).Value
            node2 = groups(3).Value
            If node1 <> node2 Then
                sw.WriteLine("SEL PILESEL  cid   {0} id   {1} nodes  {2} {3}", eleId, SelId, node1, node2) ' 大致的结构为：SEL PILESEL  cid   109200 id   109200 nodes  1770004 1769720
            Else
                Console.WriteLine("Warning : Pile element {0} have two nodes with the same id: {1} .", eleId, node1)
            End If
            '读取下一个节点
            strLine = sr.ReadLine
            match = Regex.Match(strLine, pattern)
        Loop
        sw.WriteLine("sel group  {0} range id {1}", component, SelId) ' 大致的结构为：SEL PILESEL  cid   109200 id   109200 nodes  1770004 1769720
        SelId += 1
        Return strLine
    End Function

    ''' <summary>
    ''' 生成梁单元，并返回跳出循环的字符串
    ''' </summary>
    ''' <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
    ''' <remarks></remarks>
    Private Function Gen_Beam(sr As StreamReader, sw As StreamWriter, component As String) As String
        Dim pattern As String = "\s*(\d*),\s*(\d*),\s*(\d*)"
        Dim strLine As String
        Dim eleId As Long
        Dim node1 As Long, node2 As Long
        '
        strLine = sr.ReadLine()  ' 大致的结构为： "        16,  10.0           ,  6.6666666666667,  0.0            "
        Dim match As Match, groups As GroupCollection
        match = Regex.Match(strLine, pattern)
        Do While match.Success
            groups = match.Groups
            eleId = groups(1).Value
            node1 = groups(2).Value
            node2 = groups(3).Value
            If node1 <> node2 Then
                sw.WriteLine("SEL BEAMSEL  cid   {0} id   {1} nodes  {2} {3}", eleId, SelId, node1, node2) ' 大致的结构为：SEL PILESEL  cid   109200 id   109200 nodes  1770004 1769720
            Else
                Console.WriteLine("Warning : Beam element {0} have two nodes with the same id: {1} .", eleId, node1)
            End If
            '读取下一个节点
            strLine = sr.ReadLine
            match = Regex.Match(strLine, pattern)
        Loop
        sw.WriteLine("sel group  {0} range id {1}", component, SelId) ' 大致的结构为：SEL PILESEL  cid   109200 id   109200 nodes  1770004 1769720
        SelId += 1
        Return strLine
    End Function

    ''' <summary>
    ''' 生成三角形壳单元，并返回跳出循环的字符串
    ''' </summary>
    ''' <param name="sr"></param>
    ''' <param name="sw"></param>
    ''' <param name="Component"></param>
    ''' <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
    ''' <remarks></remarks>
    Private Function Gen_Shell3(sr As StreamReader, sw As StreamWriter, ByVal Component As String) As String
        Dim pattern As String = "\s*(\d*),\s*(\d*),\s*(\d*),\s*(\d*)"
        Dim strLine As String
        Dim eleId As Long
        Dim node1 As Long, node2 As Long, node3 As Long
        '
        strLine = sr.ReadLine()  ' 大致的结构为： "        16,  10.0           ,  6.6666666666667,  0.0            "
        Dim match As Match, groups As GroupCollection
        match = Regex.Match(strLine, pattern)
        Do While match.Success
            groups = match.Groups
            eleId = groups(1).Value
            node1 = groups(2).Value
            node2 = groups(3).Value
            node3 = groups(4).Value
            '下面这一条语句所创建的Shell，它并不会与其周围的Zone之间建立 Node-to-Zone links.
            sw.WriteLine("SEL SHELLSEL cid   {0} id   {1} ele DKT_CST  nodes  {2} {3} {4}", eleId, SelId, node1, node2, node3) ' 大致的结构为：SEL SHELLSEL cid    68341 id    68341 ele DKT_CST  nodes  1 2 3
            '读取下一个节点
            strLine = sr.ReadLine
            match = Regex.Match(strLine, pattern)
        Loop
        sw.WriteLine("sel group  {0} range id {1}", Component, SelId) ' 大致的结构为：SEL PILESEL  cid   109200 id   109200 nodes  1770004 1769720
        SelId += 1
        Return strLine
    End Function

    ''' <summary>
    ''' 生成三角形壳单元，并返回跳出循环的字符串
    ''' </summary>
    ''' <param name="sr"></param>
    ''' <param name="sw"></param>
    ''' <param name="Component"></param>
    ''' <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
    ''' <remarks></remarks>
    Private Function Gen_Liner3(sr As StreamReader, sw As StreamWriter, ByVal Component As String) As String
        Dim pattern As String = "\s*(\d*),\s*(\d*),\s*(\d*),\s*(\d*)"
        Dim strLine As String
        Dim eleId As Long
        Dim node1 As Long, node2 As Long, node3 As Long
        '
        strLine = sr.ReadLine()  ' 大致的结构为： "        16,  10.0           ,  6.6666666666667,  0.0            "
        Dim match As Match, groups As GroupCollection
        match = Regex.Match(strLine, pattern)
        Do While match.Success
            groups = match.Groups
            eleId = groups(1).Value
            node1 = groups(2).Value
            node2 = groups(3).Value
            node3 = groups(4).Value
            '下面这一条语句所创建的Liner，它并不会与其周围的Zone之间建立 Node-to-Zone links.
            sw.WriteLine("SEL LINERSEL cid   {0} id   {1}  ele DKT_CST  nodes  {2} {3} {4}", eleId, SelId, node1, node2, node3) ' 大致的结构为：SEL SHELLSEL cid    68341 id    68341 ele DKT_CST  nodes  1 2 3
            listLinerNode.Add(node1)
            listLinerNode.Add(node2)
            listLinerNode.Add(node3)
            '读取下一个节点
            strLine = sr.ReadLine
            match = Regex.Match(strLine, pattern)
        Loop

        sw.WriteLine("sel group  {0} range id {1}", Component, SelId) ' 大致的结构为：SEL PILESEL  cid   109200 id   109200 nodes  1770004 1769720
        SelId += 1
        Return strLine
    End Function

    ''' <summary>
    ''' 生成六面体八节点单元，并返回跳出循环的字符串
    ''' </summary>
    ''' <param name="sr"></param>
    ''' <param name="sw_zone"></param>
    ''' <param name="Component"></param>
    ''' <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
    ''' <remarks>由于在inp文件中，此类单元的节点是分在两行中的，所以不能用“match.Success”作为循环终止的判断</remarks>
    Private Function Gen_Zone_B8(sr As StreamReader, sw_zone As StreamWriter, ByVal Component As String) As String

        Dim pattern As String = "\s*(\d+),\s*(\d+),\s*(\d+),\s*(\d+),\s*(\d+),\s*(\d+),\s*(\d+),\s*(\d+),\s*(\d+)"
        Dim eleId As Long
        Dim node1 As Long, node2 As Long, node3 As Long, node4 As Long, node5 As Long, node6 As Long, node7 As Long, node8 As Long
        Dim listEleId As New List(Of Long)

        '先写入标志语句
        sw_zone.WriteLine("* ZONES")
        Dim ptNum As String = "^\s*(\d+)"
        '
        Dim match As Match, groups As GroupCollection
        Dim strLine As String
        ' 在inp文件中的大致的结构为：      87482,     49066,     49224,     49040,     49065,     37816,     37974,     37790, 换行 37815
        strLine = sr.ReadLine()
        Do
            strLine = strLine & sr.ReadLine()   ' 将两行的内容连接成一行
            match = Regex.Match(strLine, pattern)
            groups = match.Groups
            eleId = groups(1).Value
            node1 = groups(2).Value
            node2 = groups(3).Value
            node3 = groups(4).Value
            node4 = groups(5).Value
            node5 = groups(6).Value
            node6 = groups(7).Value
            node7 = groups(8).Value
            node8 = groups(9).Value
            '在Flac3d中的大致的结构为： Z B8   65268  47853  47854  47852  43871  47851  43901  43847  43898
            sw_zone.WriteLine("Z B8  {0}   {1}  {2}  {3}  {4}  {5}  {6}  {7}  {8}", eleId, node6, node5, node7, node2, node8, node3, node1, node4)
            listEleId.Add(eleId)
            '读取下一个节点
            strLine = sr.ReadLine
        Loop While Regex.Match(strLine, ptNum).Success

        '将此Component中的所有单元写入Flac3d中的一个组中
        sw_zone.WriteLine("* GROUPS")
        sw_zone.WriteLine("ZGROUP " & Component)
        Dim num As Long
        For num = 0 To listEleId.Count - 1
            sw_zone.Write("    " & listEleId(num))
            ' 写5个即换行
            If (num + 1) Mod 5 = 0 Then
                sw_zone.Write(vbCrLf)  '换行不能用Chr(13)，否则在Flac3d中用im zones.flac3d时，会出现不能识别的报错。
            End If
        Next
        sw_zone.WriteLine()
        Return strLine
    End Function

    ''' <summary>
    ''' 生成三棱柱（五面体、六节点）单元，并返回跳出循环的字符串
    ''' </summary>
    ''' <param name="sr"></param>
    ''' <param name="sw_zone"></param>
    ''' <param name="Component"></param>
    ''' <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
    ''' <remarks></remarks>
    Private Function Gen_Zone_W6(sr As StreamReader, sw_zone As StreamWriter, ByVal Component As String) As String

        Dim pattern As String = "\s*(\d+),\s*(\d+),\s*(\d+),\s*(\d+),\s*(\d+),\s*(\d+),\s*(\d+)"
        Dim eleId As Long
        Dim node1 As Long, node2 As Long, node3 As Long, node4 As Long, node5 As Long, node6 As Long
        Dim listEleId As New List(Of Long)

        '先写入标志语句
        sw_zone.WriteLine("* ZONES")
        Dim ptNum As String = "^\s*(\d+)"
        '
        Dim match As Match, groups As GroupCollection
        Dim strLine As String
        strLine = sr.ReadLine()  ' 大致的结构为：  单元id, 节点1 2 3 4 5 6
        match = Regex.Match(strLine, pattern)
        Do While match.Success
            groups = match.Groups
            eleId = groups(1).Value
            node1 = groups(2).Value
            node2 = groups(3).Value
            node3 = groups(4).Value
            node4 = groups(5).Value
            node5 = groups(6).Value
            node6 = groups(7).Value
            sw_zone.WriteLine("Z W6  {0}   {1}  {2}  {3}  {4}  {5}  {6}", eleId, node1, node3, node4, node2, node6, node5)
            listEleId.Add(eleId)
            '读取下一个节点
            strLine = sr.ReadLine
            match = Regex.Match(strLine, pattern)
        Loop

        '将此Component中的所有单元写入Flac3d中的一个组中
        sw_zone.WriteLine("* GROUPS")
        sw_zone.WriteLine("ZGROUP " & Component)
        Dim num As Long
        For num = 0 To listEleId.Count - 1
            sw_zone.Write("    " & listEleId(num))
            ' 写5个即换行
            If (num + 1) Mod 5 = 0 Then
                sw_zone.Write(vbCrLf)  '换行不能用Chr(13)，否则在Flac3d中用im zones.flac3d时，会出现不能识别的报错。
            End If
        Next
        sw_zone.WriteLine()
        Return strLine
    End Function

    ''' <summary>
    ''' 生成四面体（金字塔形、四节点）单元，并返回跳出循环的字符串
    ''' </summary>
    ''' <param name="sr"></param>
    ''' <param name="sw_zone"></param>
    ''' <param name="Component"></param>
    ''' <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
    ''' <remarks></remarks>
    Private Function Gen_Zone_T4(sr As StreamReader, sw_zone As StreamWriter, ByVal Component As String) As String

        Dim pattern As String = "\s*(\d+),\s*(\d+),\s*(\d+),\s*(\d+),\s*(\d+)"
        Dim eleId As Long
        Dim node1 As Long, node2 As Long, node3 As Long, node4 As Long
        Dim listEleId As New List(Of Long)

        '先写入标志语句
        sw_zone.WriteLine("* ZONES")
        Dim ptNum As String = "^\s*(\d+)"
        '
        Dim match As Match, groups As GroupCollection
        Dim strLine As String
        strLine = sr.ReadLine()  ' 大致的结构为：  单元id, 节点1 2 3 4 5 6
        match = Regex.Match(strLine, pattern)
        Do While match.Success
            groups = match.Groups
            eleId = groups(1).Value
            node1 = groups(2).Value
            node2 = groups(3).Value
            node3 = groups(4).Value
            node4 = groups(5).Value
            sw_zone.WriteLine("Z T4  {0}   {1}  {2}  {3}  {4}", eleId, node1, node2, node3, node4)
            listEleId.Add(eleId)
            '读取下一个节点
            strLine = sr.ReadLine
            match = Regex.Match(strLine, pattern)
        Loop
        '将此Component中的所有单元写入Flac3d中的一个组中
        sw_zone.WriteLine("* GROUPS")
        sw_zone.WriteLine("ZGROUP " & Component)
        Dim num As Long
        For num = 0 To listEleId.Count - 1
            sw_zone.Write("    " & listEleId(num))
            ' 写5个即换行
            If (num + 1) Mod 5 = 0 Then
                sw_zone.Write(vbCrLf)  '换行不能用Chr(13)，否则在Flac3d中用im zones.flac3d时，会出现不能识别的报错。
            End If
        Next
        sw_zone.WriteLine()
        Return strLine
    End Function

#End Region

    ''' <summary>
    ''' 在Console中进行手动判断，如果用户输入Yes或者Y，则返回布尔值True，否则返回False
    ''' </summary>
    ''' <param name="question">要询问用户的问题</param>
    ''' <returns></returns>
    ''' <remarks>如果用户输入Yes或者Y，则返回布尔值True，否则返回False</remarks>
    Private Function getReply(ByVal question As String) As Boolean
        Console.WriteLine(question & "Yes[Y] or No[N] ?")
        Dim ans As String = Console.ReadLine
        If String.Compare(ans, "y", True) = 0 OrElse String.Compare(ans, "yes", True) = 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' 根据inp文件中对于单元类型以及Component命名的描述，确定此组是属于什么单元
    ''' </summary>
    ''' <param name="TypeName"></param>
    ''' <param name="componentName"></param>
    ''' <returns>返回与inp文件中的单元类型所对应的Flac3d中的单元类型</returns>
    ''' <remarks></remarks>
    Private Function GetElementType(TypeName As String, ByVal componentName As String) As ElementType
        Dim eleType As ElementType = ElementType.Others
        If (TypeName.IndexOf("B31", System.StringComparison.OrdinalIgnoreCase) >= 0 OrElse TypeName.IndexOf("B21", System.StringComparison.OrdinalIgnoreCase) >= 0) _
            AndAlso componentName.IndexOf("beam", System.StringComparison.OrdinalIgnoreCase) >= 0 Then
            eleType = ElementType.BEAM
        End If
        If (TypeName.IndexOf("B31", System.StringComparison.OrdinalIgnoreCase) >= 0 OrElse TypeName.IndexOf("B21", System.StringComparison.OrdinalIgnoreCase) >= 0) _
            AndAlso componentName.IndexOf("pile", System.StringComparison.OrdinalIgnoreCase) >= 0 Then
            eleType = ElementType.PILE
        End If
        If (TypeName.IndexOf("S3", System.StringComparison.OrdinalIgnoreCase) >= 0) AndAlso componentName.IndexOf("Shell", System.StringComparison.OrdinalIgnoreCase) >= 0 Then
            eleType = ElementType.SHELL3
        End If
        'If (TypeName.IndexOf("S3", System.StringComparison.OrdinalIgnoreCase) >= 0) AndAlso componentName.IndexOf("Liner", System.StringComparison.OrdinalIgnoreCase) >= 0 Then
        '    eleType = ElementType.Liner3
        'End If
        If TypeName.IndexOf("C3D8", System.StringComparison.OrdinalIgnoreCase) >= 0 Then
            eleType = ElementType.ZONE_B8
        End If
        If TypeName.IndexOf("C3D6", System.StringComparison.OrdinalIgnoreCase) >= 0 Then
            eleType = ElementType.ZONE_W6
        End If
        If TypeName.IndexOf("C3D4", System.StringComparison.OrdinalIgnoreCase) >= 0 Then
            eleType = ElementType.ZONE_T4
        End If
        Return eleType
    End Function

End Module

''' <summary>
''' 在Flac3D中的单元类型，每一种类型代表了一种单元的编码形式
''' </summary>
''' <remarks></remarks>
Public Enum ElementType
    ''' <summary>
    ''' 梁单元，其在Flac3d中的导入格式为：SEL BEAMSEL  cid   107629 id   107629 nodes  1768565 1757075
    ''' </summary>
    ''' <remarks>在结构单元导入文件Structures.dat的结尾要加上一句：SEL NODE INIT XPOS ADD 0.0 ，
    ''' 以将结构单元之间进行耦合。</remarks>
    BEAM
    ''' <summary>
    ''' 桩单元，其在Flac3d中的导入格式为：SEL PILESEL  cid   110190 id   110190 nodes  1770162 1769878
    ''' </summary>
    ''' <remarks>在结构单元导入文件Structures.dat的结尾要加上一句：SEL NODE INIT XPOS ADD 0.0 ，
    ''' 以将结构单元之间进行耦合。</remarks>
    PILE
    ''' <summary>
    ''' 三角形壳单元，其在Flac3d中的导入格式为：SEL SHELLSEL cid    99440 id    99440 ele DKT_CST  nodes  1759355 1758957 1758956
    ''' </summary>
    ''' <remarks>在结构单元导入文件Structures.dat的结尾要加上一句：SEL NODE INIT XPOS ADD 0.0 ，
    ''' 以将结构单元之间进行耦合。</remarks>
    SHELL3
    ''' <summary>
    ''' 三角形Liner单元，其在Flac3d中的导入格式为：SEL SHELLSEL cid    68341 id    68341 ele DKT_CST  nodes  1564835 1579841 1564832
    ''' </summary>
    ''' <remarks></remarks>
    Liner3
    ''' <summary>
    ''' 六面体八节点单元，其在Flac3d中的导入格式为：Z B8   90098  95441  95242  93742  95005  93417  95617  93041  95485
    ''' </summary>
    ''' <remarks></remarks>
    ZONE_B8
    ''' <summary>
    ''' 三棱柱（五面体、六节点）单元，其在Flac3d中的导入格式为：Z W6   90083  93007  93014  93016  94941  94674  92956
    ''' </summary>
    ''' <remarks></remarks>
    ZONE_W6
    ''' <summary>
    ''' 四面体（金字塔形、四节点）单元，其在Flac3d中的导入格式为：Z T4   90111  92891  92874  92868  92954
    ''' </summary>
    ''' <remarks></remarks>
    ZONE_T4
    ''' <summary>
    ''' 其他未识别的单元类型
    ''' </summary>
    ''' <remarks></remarks>
    Others
End Enum