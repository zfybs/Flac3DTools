Public Class Hm2Flac3DHandler

    ''' <summary>
    ''' 根据inp文件中对于单元类型以及Component命名的描述，确定此组是属于什么单元
    ''' </summary>
    ''' <param name="TypeName"></param>
    ''' <param name="componentName"> inp 文件中表示的Hypermesh的 Component 的名称 </param>
    ''' <returns>返回与inp文件中的单元类型所对应的Flac3d中的单元类型</returns>
    ''' <remarks></remarks>
    Public Shared Function GetElementType(TypeName As String, ByVal componentName As String) As ElementType

        If (TypeName.IndexOf("B31", System.StringComparison.OrdinalIgnoreCase) >= 0 OrElse TypeName.IndexOf("B21", System.StringComparison.OrdinalIgnoreCase) >= 0) _
            AndAlso componentName.IndexOf("beam", System.StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return ElementType.BEAM
        End If
        If (TypeName.IndexOf("B31", System.StringComparison.OrdinalIgnoreCase) >= 0 OrElse TypeName.IndexOf("B21", System.StringComparison.OrdinalIgnoreCase) >= 0) _
            AndAlso componentName.IndexOf("pile", System.StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return ElementType.PILE
        End If
        If (TypeName.IndexOf("S3", System.StringComparison.OrdinalIgnoreCase) >= 0) AndAlso componentName.IndexOf("Shell", System.StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return ElementType.SHELL3
        End If

        If (TypeName.IndexOf("S3", System.StringComparison.OrdinalIgnoreCase) >= 0) AndAlso componentName.IndexOf("Liner", System.StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return ElementType.Liner3
        End If

        If (TypeName.IndexOf("S4", System.StringComparison.OrdinalIgnoreCase) >= 0) AndAlso componentName.IndexOf("Liner", System.StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return ElementType.Liner4
        End If

        If TypeName.IndexOf("C3D8", System.StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return ElementType.ZONE_B8
        End If
        If TypeName.IndexOf("C3D6", System.StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return ElementType.ZONE_W6
        End If
        If TypeName.IndexOf("C3D4", System.StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return ElementType.ZONE_T4
        End If
        Return ElementType.Others
    End Function

    ''' <summary>
    ''' 在Console中进行手动判断，如果用户输入Yes或者Y，则返回布尔值True，否则返回False
    ''' </summary>
    ''' <param name="question">要询问用户的问题</param>
    ''' <returns></returns>
    ''' <remarks>如果用户输入Yes或者Y，则返回布尔值True，否则返回False</remarks>
    Public Shared Function GetReply(ByVal question As String) As Boolean
        Console.WriteLine(question & "Yes[Y] or No[N] ?")
        Dim ans As String = Console.ReadLine
        If String.Compare(ans, "y", True) = 0 OrElse String.Compare(ans, "yes", True) = 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' 寻找三个点所形成的空间三角形的形心
    ''' </summary>
    ''' <param name="node1"></param>
    ''' <param name="node2"></param>
    ''' <param name="node3"></param>
    ''' <returns></returns>
         
    Public Shared Function FindCentroid(node1 As XYZ, node2 As XYZ, node3 As XYZ) As XYZ
        Return New XYZ()
    End Function

    ''' <summary>
    ''' 寻找空间四个点所形成的共面的空间四边形角形的形心，如果四个点不共面，则会报错 
    ''' </summary>
    ''' <param name="node1"></param>
    ''' <param name="node2"></param>
    ''' <param name="node3"></param>
    ''' <param name="node4"></param>
    ''' <returns></returns>
    Public Shared Function FindCentroid(node1 As XYZ, node2 As XYZ, node3 As XYZ, node4 As XYZ) As XYZ
        Return New XYZ()
    End Function

End Class

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
    ''' 通过三角形平面来创建三角形Liner单元，其在Flac3d中的导入格式为：SEL LINERSEL cid    68341 id    68341 ele DKT_CST  nodes  1564835 1579841 1564832
    ''' </summary>
    ''' <remarks></remarks>
    Liner3

    ''' <summary>
    ''' 通过共面四边形来创建分割此四边形的两个Liner三角形Liner单元，其在Flac3d中的导入格式为：sel liner id 1 em group ex1 range x 23.73 23.78 y -0.01 0.01 z 19.65 19.67
    ''' </summary>
    ''' <remarks> 创建方法为先找到此四边形的形心，然后将形心点向扩展1mm， 以形成一个体积为1立方毫米的立方体，然后用此立方体来作为创建 Liner 的 range。 </remarks>
    Liner4

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

''' <summary>
''' 一个空间的坐标点
''' </summary>
Public Structure XYZ
    Public X As Double
    Public Y As Double
    Public Z As Double

    Public Sub New(x As Double, y As Double, z As Double)
        Me.X = x
        Me.Y = y
        Me.Z = z
    End Sub
End Structure