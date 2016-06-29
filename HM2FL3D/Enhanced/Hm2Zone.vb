Imports System.Text.RegularExpressions
Imports System.IO
Imports System.Text

Public Class Hm2Zone
    Inherits InpReader

#Region "---   Fields"

    ''' <summary> 用来写入 Zone 单元的节点的那个文本。 </summary>
    Private sw_Zone As StreamWriter

    ''' <summary>
    '''  将用来创建 Liner 的 group 放在 Flac 中 编号为2及以上的 slot 中，以避免与其他的group产生干扰。
    ''' </summary>
    ''' <remarks> 如果不显式指定 slot，则 group 默认是放在 slot 1 中的。 </remarks>
    Private linerGroupSlot As Integer = 2

#End Region

    ''' <summary>
    ''' 构造函数
    ''' </summary>
    ''' <param name="inpReader"></param>
    ''' <param name="zoneWriter"></param>
    ''' <param name="message">用于输出的消息说明</param>
    Public Sub New(inpReader As StreamReader, zoneWriter As StreamWriter, ByRef message As StringBuilder)

        MyBase.New(inpReader, message)
        sw_Zone = zoneWriter
        _message = message

        '写入头文件信息
        ' 用im zone.dat来在Flac3d中导入网格时，在zone.dat中可以先写入文件头信息，而structures.dat文件中，不能用“*”来写注释
        Dim strHeading As String
        strHeading = " * --------------------------------------------------" & vbCrLf &
            " *  INP (exported from Hypermesh) to FLAC3D " & vbCrLf &
            " *  Coded by Zeng Fanyun. Email: 619086871@qq.com" & vbCrLf &
            " *  Latest update time: 2016/6/28 " & vbCrLf &
            " * --------------------------------------------------" & vbCrLf &
            "* Generated time: " & DateTime.Today.ToString("yyyy/MM/dd") & "   " & DateTime.Now.ToShortTimeString & vbCrLf &
            vbCrLf & "* GRIDPOINTS"
        sw_Zone.WriteLine(strHeading)

    End Sub

    ''' <summary>
    ''' 读取数据，并返回跳出循环的字符串
    ''' </summary>
    ''' <returns>如果数据提取成功，则返回True，否则返回False</returns>
    ''' <remarks>在读取数据时，每一个生成单元的函数中，都会返回最后跳出循环的那个字符串，如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。</remarks>
    Public Overrides Function ReadFile() As String

        '第一步：输出节点
        ' --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        Dim strLine As String = MyBase.ReadNodesOrGridpoint()    ' 读取数据
        ' --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        '第二步：输出单元
        strLine = ReadElements(sr_inp, strLine)    ' 读取数据

        Return strLine
    End Function

    Protected Overrides Function Gen_Node(sr As StreamReader) As String
        Dim strLine As String
        strLine = sr.ReadLine()  ' 大致的结构为： "        16,  10.0           ,  6.6666666666667,  0.0            "
        Do Until strLine.StartsWith("*")
            sw_Zone.WriteLine("G  {0}", strLine)
            strLine = sr.ReadLine()  ' 大致的结构为： "        16,  10.0           ,  6.6666666666667,  0.0            "
        Loop
        Return strLine
    End Function

    ''' <summary>
    ''' 读取数据，并返回跳出循环的字符串
    ''' </summary>
    ''' <param name="sr_inp"></param>
    ''' <param name="stringline"> 当前要读取 inp 文件中的那一行数据 </param>
    ''' <returns>如果数据提取成功，则返回True，否则返回False</returns>
    ''' <remarks>在读取数据时，每一个生成单元的函数中，都会返回最后跳出循环的那个字符串，如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。</remarks>
    Protected Overrides Function ReadElements(sr_inp As StreamReader, stringline As String) As String
        Dim pattern_ElementType As String = "\*ELEMENT,TYPE=(.+),ELSET=(.+)"  '大致结构为： *ELEMENT,TYPE=B31,ELSET=columns-c1
        Dim pattern_Group As String = "\*ELSET, ELSET=(GLiner.*)"  '表示 Hypermesh 中的 SetBrowser 里面的 Element 组，用来作为 在Flac3D 中创建 Liner 时的 group 区间。

        Dim strLine As String = stringline
        Dim match As Match
        Do
            '在Hypermesh导出的inp文件中，可以有很多个 *ELEMENT,TYPE=B31,ELSET=columns 这样的语句，它们是按hypermesh中的Component来进行分组的。
            match = Regex.Match(strLine, pattern_ElementType)
            If match.Success Then
                '
                Dim strEleType As String = match.Groups(1).Value

                Dim strComponentName As String = match.Groups(2).Value  ' 当前读取到Inp中的那一个 Component（即 Hypermesh 中的 Component）

                Dim tp As ElementType = Hm2Flac3DHandler.GetElementType(strEleType, strComponentName)

                ' 创建 Flac3D 单元
                strLine = GenarateElement(tp, sr_inp, strComponentName)

            ElseIf Regex.Match(strLine, pattern_Group).Success Then

                match = Regex.Match(strLine, pattern_Group)

                Dim groupName As String = match.Groups(1).Value

                If groupName.Contains("-") Then  ' set 的名称中不能包含“-”
                    _message.AppendLine(String.Format("Warning : Can not export element set : "" {0} "", make sure the set name starts with ""GLiner""(case ignored) and excludes ""-"" if you want to bind the set to the creation of Liner elements.", groupName))
                Else
                    ' 创建 Flac3D 单元
                    strLine = Gen_Group(sr_inp, groupName)
                End If
            Else
                '如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。
                strLine = sr_inp.ReadLine()
            End If
        Loop While (strLine IsNot Nothing)

        Return strLine
    End Function

    ''' <summary>
    ''' 生成Flac3D中的Zone单元，并返回跳出循环的字符串
    ''' </summary>
    ''' <param name="type"></param>
    ''' <param name="sr"></param>
    ''' <param name="component"></param>
    ''' <returns></returns>
    Protected Overrides Function GenarateElement(type As ElementType, sr As StreamReader, component As String) As String
        Dim strLine As String
        Select Case type

            ' Zone单元
            Case ElementType.ZONE_B8
                strLine = Gen_Zone_B8(sr_inp, sw_Zone, component)
            Case ElementType.ZONE_W6
                strLine = Gen_Zone_W6(sr_inp, sw_Zone, component)
            Case ElementType.ZONE_T4
                strLine = Gen_Zone_T4(sr_inp, sw_Zone, component)

            Case Else 'Hypermesh中的类型在Flac3d中没有设置对应的类型
                _message.AppendLine(String.Format("Warning : Can not match element type "" {0} ""(in component {1}) with a corresponding type in Flac3D,", type.ToString(), component))

                '如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。
                strLine = sr_inp.ReadLine()
        End Select
        Return strLine
    End Function

    ''' <summary> 在zones.flac3d 文件中写入用来绑定Liner的Group的单元集合 </summary>
    ''' <param name="sr"></param>
    ''' <param name="groupName"> group的名称必须以GLiner开头，而且不包含“-” </param>
    ''' <returns></returns>
    Private Function Gen_Group(sr As StreamReader, groupName As String) As String

        Dim pattern As String = "\s*(\d+),\s*(\d+),\s*(\d+),\s*(\d+),\s*(\d+),\s*(\d+),\s*(\d+),\s*(\d+)"  ' set 集合在inp文件中的格式为： 132,       133,       134,       135,       136,       137,       138,       139,

        Dim listEleId As New List(Of Long)

        '先写入标志语句
        sw_Zone.WriteLine("* ZONES")
        Dim ptNum As String = "^\s*(\d+)"
        '
        Dim match As Match, groups As GroupCollection
        Dim strLine As String
        strLine = sr.ReadLine()  ' 大致的结构为：  单元id, 节点1 2 3 4 5 6
        match = Regex.Match(strLine, pattern)
        Do While match.Success
            ’ 将此行中所有的单元id添加到集合中
            groups = match.Groups

            listEleId.AddRange({groups(1).Value, groups(2).Value, groups(3).Value, groups(4).Value,
                               groups(5).Value, groups(6).Value, groups(7).Value, groups(8).Value})
            
            '读取下一个节点
            strLine = sr.ReadLine
            match = Regex.Match(strLine, pattern)
        Loop
        '将此Component中的所有单元写入Flac3d中的一个组中
        sw_Zone.WriteLine("* GROUPS")
        ' 将用来创建 Liner 的 group 放在 Flac 中 编号为2及以上的 slot 中，以避免与其他的group产生干扰。
        sw_Zone.WriteLine("ZGROUP " & groupName & " SLOT " & linerGroupSlot)
        linerGroupSlot += 1
        Dim num As Long
        For num = 0 To listEleId.Count - 1
            sw_Zone.Write("    " & listEleId(num))
            ' 写5个即换行
            If (num + 1) Mod 5 = 0 Then
                sw_Zone.Write(vbCrLf)  '换行不能用Chr(13)，否则在Flac3d中用im zones.flac3d时，会出现不能识别的报错。
            End If
        Next
        sw_Zone.WriteLine()
        Return strLine

    End Function

#Region "  ---  生成不同类型的 Zone 单元"

    ''' <summary>
    ''' 生成六面体八节点单元，并返回跳出循环的字符串
    ''' </summary>
    ''' <param name="sr"></param>
    ''' <param name="sw_zone"></param>
    ''' <param name="Component"> 当前读取到Inp中的那一个 Component（即 Hypermesh 中的 Component） </param>
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
    ''' <param name="Component"> 当前读取到Inp中的那一个 Component（即 Hypermesh 中的 Component） </param>
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
    ''' <param name="Component"> 当前读取到Inp中的那一个 Component（即 Hypermesh 中的 Component） </param>
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

End Class
