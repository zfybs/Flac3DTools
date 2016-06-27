Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Public Class Hm2Structure
    Inherits InpReader

#Region "---   Fields"

    ''' <summary> 用来写入Structural element的节点的那个文本。 </summary>
    Private sw_Sel As StreamWriter

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

    ''' <summary>
    ''' 用来创建 Liner 的 Zone 单元集合。键为group的名称，值为此group中的Zone单元集合。
    ''' </summary>
    Private linerGroup As New Dictionary(Of String, List(Of Integer))

#End Region

    ''' <summary>
    ''' 构造函数
    ''' </summary>
    ''' <param name="inpReader"></param>
    ''' <param name="structureWriter"></param>
    ''' <param name="message">用于输出的消息说明</param>
    Public Sub New(inpReader As StreamReader, structureWriter As StreamWriter, ByRef message As StringBuilder)
        MyBase.New(inpReader, message)

        sw_Sel = structureWriter
        _message = message

    End Sub


    ''' <summary>
    ''' 读取数据，并返回跳出循环的字符串
    ''' </summary>
    ''' <returns>如果数据提取成功，则返回True，否则返回False</returns>
    ''' <remarks>在读取数据时，每一个生成单元的函数中，都会返回最后跳出循环的那个字符串，如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。</remarks>
    Public Overrides Function ReadFile() As String

        '第一步：输出节点
        ' -----------------------------------------------------------------------------------------------------------
        Dim strLine As String = MyBase.ReadNodesOrGridpoint()    ' 读取数据
        ' -----------------------------------------------------------------------------------------------------------                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  
        '第二步：输出单元
        strLine = ReadElements(sr_inp, strLine)    ' 读取数据

        ' -------------------------------------------------- 后续补充操作 -----------------------------------------------

        'sw_Sel.WriteLine("SEL NODE INIT XPOS ADD 0.0")   '将所有结构单元与土相连的节点位置创建接触

        ''当Liner单元用SelLiner创建时，它只与其side1方向的那一个Zone之间建立了link，此时要手动地为每一个linerNode与其side2方向的Zone之间建立Link。
        'For Each linerNode As Long In listLinerNode
        '    sw_Sel.WriteLine("SEL LINK id {0} {1} side2", linerNode + 10000000000, linerNode)
        'Next
        '' ---------------------------------------------------------------------------------------------------------------

        Return strLine
    End Function

    ''' <summary>
    ''' 读取数据，并返回跳出循环的字符串
    ''' </summary>
    ''' <param name="sr_inp"></param>
    ''' <param name="stringline"> 当前要读取 inp 文件中的那一行数据 </param>
    ''' <remarks>在读取数据时，每一个生成单元的函数中，都会返回最后跳出循环的那个字符串，如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。</remarks>
    Protected Overrides Function ReadElements(sr_inp As StreamReader, stringline As String) As String
        Dim pattern_ElementType As String = "\*ELEMENT,TYPE=(.+),ELSET=(.+)"  '大致结构为： *ELEMENT,TYPE=B31,ELSET=columns-c1

        Dim strLine = stringline
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

            Else
                '如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。
                strLine = sr_inp.ReadLine()
            End If
        Loop While (strLine IsNot Nothing)

        Return strLine
    End Function

    ''' <summary>
    ''' 创建节点
    ''' </summary>
    ''' <param name="sr">用来提取数据的inp文件</param>
    ''' <returns></returns>
    Protected Overrides Function Gen_Node(sr As StreamReader) As String
        Dim strLine As String
        strLine = sr.ReadLine()  ' 大致的结构为： "        16,  10.0           ,  6.6666666666667,  0.0            "
        Do Until strLine.StartsWith("*")
            sw_Sel.WriteLine("SEL NODE cid  {0}", strLine) ' 大致的结构为： ' SEL NODE cid  1440016  0.216969418565193E+02 -0.531659539393860E+02 -0.161000000000000E+02
            strLine = sr.ReadLine()  ' 大致的结构为： "        16,  10.0           ,  6.6666666666667,  0.0            "
        Loop
        Return strLine
    End Function

    Protected Overrides Function GenarateElement(type As ElementType, sr As StreamReader, componentName As String) As String
        Dim strLine As String = Nothing

        Select Case type
                        ' 结构单元
            Case ElementType.BEAM
                strLine = Gen_Beam(sr_inp, sw_Sel, componentName)
            Case ElementType.PILE
                strLine = Gen_Pile(sr_inp, sw_Sel, componentName)
            Case ElementType.SHELL3
                strLine = Gen_Shell3(sr_inp, sw_Sel, componentName)

            Case ElementType.Liner3

                Dim patternLiner As String = "Liner-(.+)-"  '  以 “liner-组名-”开头
                Dim match As Match = Regex.Match(componentName, patternLiner, RegexOptions.IgnoreCase)
                If match.Success Then
                    Dim groupName As String = match.Groups(0).Value
                    strLine = Gen_Liner3(sr_inp, sw_Sel, componentName, groupName)
                Else
                    _message.AppendLine("无法识别要将 Liner 单元创建在哪一个 group 上。 对应的Component名称为" & componentName)
                    Return strLine
                End If

            Case ElementType.Liner4

                Dim patternLiner As String = "Liner-(.+)-"  '  以 “liner-组名-”开头
                Dim match As Match = Regex.Match(componentName, patternLiner, RegexOptions.IgnoreCase)
                If match.Success Then
                    Dim groupName As String = match.Groups(0).Value
                    strLine = Gen_Liner4(sr_inp, sw_Sel, componentName, groupName)
                Else
                    _message.AppendLine("无法识别要将 Liner 单元创建在哪一个 group 上。 对应的Component名称为" & componentName)
                    Return strLine
                End If

            Case Else 'Hypermesh中的类型在Flac3d中没有设置对应的类型
                _message.AppendLine(String.Format("Warning : Can not match element type "" {0} ""(in component {1}) with a corresponding type in Flac3D,", type.ToString(), componentName))

                '如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。
                strLine = sr_inp.ReadLine()

        End Select

        Return strLine
    End Function


#Region "  ---  生成不同类型的 Structure 单元"

    ''' <summary>
    ''' 生成桩单元，并返回跳出循环的字符串
    ''' </summary>
    ''' <param name="Component"> 当前读取到Inp中的那一个 Component（即 Hypermesh 中的 Component） </param>
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
    ''' <param name="Component"> 当前读取到Inp中的那一个 Component（即 Hypermesh 中的 Component） </param>
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
    ''' <param name="Component"> 当前读取到Inp中的那一个 Component（即 Hypermesh 中的 Component） </param>
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
    ''' 生成三角形 Liner 单元，并返回跳出循环的字符串
    ''' </summary>
    ''' <param name="sr"></param>
    ''' <param name="sw"></param>
    ''' <param name="componentName"> 当前读取到Inp中的那一个 Component（即 Hypermesh 中的 Component） </param>
    ''' <param name="groupName"> 要依托于哪一个group来生成liner单元 </param>
    ''' <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
    ''' <remarks></remarks>
    Private Function Gen_Liner3(sr As StreamReader, sw As StreamWriter, ByVal componentName As String, groupName As String) As String
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
            ' sel liner id 1 em group ex1 range x 23.73 23.78 y -0.01 0.01 z 19.65 19.67
            sw.WriteLine("SEL LINERSEL cid   {0} id   {1}  ele DKT_CST  nodes  {2} {3} {4}", eleId, SelId, node1, node2, node3) ' 大致的结构为：SEL SHELLSEL cid    68341 id    68341 ele DKT_CST  nodes  1 2 3
            listLinerNode.Add(node1)
            listLinerNode.Add(node2)
            listLinerNode.Add(node3)
            '读取下一个节点
            strLine = sr.ReadLine
            match = Regex.Match(strLine, pattern)
        Loop

        sw.WriteLine("sel group  {0} range id {1}", componentName, SelId) ' 大致的结构为：SEL PILESEL  cid   109200 id   109200 nodes  1770004 1769720
        SelId += 1
        Return strLine
    End Function

    ''' <summary>
    ''' 通过共面四边形来创建分割此四边形的两个Liner三角形Liner单元，其在Flac3d中的导入格式为：sel liner id 1 em group ex1 range x 23.73 23.78 y -0.01 0.01 z 19.65 19.67
    ''' </summary>
    ''' <param name="sr"></param>
    ''' <param name="sw"> S4类型的单元在inp文件中的格式为： 102038,    107275,    105703,    105704,    104375  </param>
    ''' <param name="componentName"> 当前读取到Inp中的那一个 Component（即 Hypermesh 中的 Component） </param>
    ''' <param name="groupName"> 要依托于哪一个group来生成liner单元 </param>
    ''' <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
    ''' <remarks>创建方法为先找到此四边形的形心，然后将形心点向扩展1mm， 以形成一个体积为1立方毫米的立方体，然后用此立方体来作为创建 Liner 的 range。</remarks>
    Private Function Gen_Liner4(sr As StreamReader, sw As StreamWriter, ByVal componentName As String, groupName As String) As String
        Dim pattern As String = "\s*(\d*),\s*(\d*),\s*(\d*),\s*(\d*),\s*(\d*)"
        Dim strLine As String
        Dim eleId As Long
        Dim node1 As Long, node2 As Long, node3 As Long, node4 As Long
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
            node4 = groups(5).Value
            '下面这一条语句所创建的Liner，它并不会与其周围的Zone之间建立 Node-to-Zone links.
            sw.WriteLine("SEL LINERSEL cid   {0} id   {1}  ele DKT_CST  nodes  {2} {3} {4}", eleId, SelId, node1, node2, node3) ' 大致的结构为：SEL SHELLSEL cid    68341 id    68341 ele DKT_CST  nodes  1 2 3
            listLinerNode.Add(node1)
            listLinerNode.Add(node2)
            listLinerNode.Add(node3)
            '读取下一个节点
            strLine = sr.ReadLine
            match = Regex.Match(strLine, pattern)
        Loop

        sw.WriteLine("sel group  {0} range id {1}", componentName, SelId) ' 大致的结构为：SEL PILESEL  cid   109200 id   109200 nodes  1770004 1769720
        SelId += 1
        Return strLine
    End Function

#End Region
End Class
