Imports System.Runtime.InteropServices

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
        Return New XYZ((node1.X + node2.X + node3.X) / 3,
                       (node1.Y + node2.Y + node3.Y) / 3,
                       (node1.Z + node2.Z + node3.Z) / 3)
    End Function

    ''' <summary>
    ''' 寻找空间四个点所形成的共面的空间四边形角形的形心，如果四个点不共面，则会报错 
    ''' </summary>
    ''' <param name="node1"></param>
    ''' <param name="node2"></param>
    ''' <param name="node3"></param>
    ''' <param name="node4"></param>
    ''' <returns> 四边形的形心点的坐标 </returns>
    ''' <remarks>在inp文件中，输入的四个节点的顺序一定是可以形成一个边界环路的，即使此S4单元的网格形状为有凹角的异型错误网格。</remarks>
    Public Shared Function FindCentroid(node1 As XYZ, node2 As XYZ, node3 As XYZ, node4 As XYZ) As XYZ

        ' 以两个对角点中距离较短的那个作为两个三角形的分割边
        Dim nodes As XYZ()
        If node1.DistanceTo(node3) < node2.DistanceTo(node4) Then
            nodes = {node2, node1, node3, node4}
        Else
            nodes = {node1, node2, node4, node3}
        End If

        ' 先计算第一个三角形的形心位置与面积
        Dim c1 As XYZ = FindCentroid(nodes(0), nodes(1), nodes(2))
        Dim area1 As Double = Area(nodes(0), nodes(1), nodes(2))

        ' 再计算第二个三角形的形心位置与面积
        Dim c2 As XYZ = FindCentroid(nodes(1), nodes(2), nodes(3))
        Dim area2 As Double = Area(nodes(1), nodes(2), nodes(3))

        Dim centDis As Double = c1.DistanceTo(c2)

        ' 利用杠杆原理计算两个三角形的组合四边形形心位置：area1 * x=area2 * (centDis-x)
        Dim x As Double = centDis / (area1 / area2 + 1)  ' 四边形的形心点处在两个三角形的形心连线上，x 为四边形的形心点到第1个三角形形心的距离。

        Return c1.Move(c1.VectorTo(c2).Scale(x))

    End Function

    ''' <summary> 计算空间三角形的面积  </summary>
    ''' <returns></returns>
    Private Shared Function Area(node1 As XYZ, node2 As XYZ, node3 As XYZ) As Double
        ' 三条边长
        Dim a As Double = XYZ.Distance(node1, node2)
        Dim b As Double = XYZ.Distance(node2, node3)
        Dim c As Double = XYZ.Distance(node3, node1)

        Dim p As Double = (a + b + c) / 2
        Return p * (p - a) * (p - b) * (p - c)  ' 海伦公式
    End Function

    ''' <summary>
    ''' 用来框选住 Liner 的形心的一个立方体区域，其单位为m。也就是说，Hypermesh中要尽量保证模型的网格尺寸不要小于这个值。
    ''' </summary>
    ''' <remarks></remarks>
    Private Const rangePrecision As Double = 0.001

    ''' <summary>
    ''' 将 形心点 扩展到一个立方体区域
    ''' </summary>
    ''' <param name="centroid"></param>
    ''' <returns>返回的格式为： "Range x= (23.73, 23.78) y =( -0.01, 0.01)  z= ( 19.65, 19.67)"</returns>
    ''' <remarks></remarks>
    Public Shared Function ExtendCentroid(centroid As XYZ) As String

        Return String.Format("Range x=({0},{1}) y=({2},{3})  z=({4},{5})",
                             (centroid.X - rangePrecision).ToString, (centroid.X + rangePrecision).ToString,
                              (centroid.Y - rangePrecision).ToString, (centroid.Y + rangePrecision).ToString,
                               (centroid.Z - rangePrecision).ToString, (centroid.Z + rangePrecision).ToString)

    End Function

    ''' <summary>  释放控制台   </summary>
    <DllImport("kernel32.dll", SetLastError:=True, ExactSpelling:=True)> _
    Public Shared Function FreeConsole() As Boolean
    End Function

    ''' <summary>
    ''' 加载控制台。加载后可以直接通过 Console.Write() 等静态方法对控制台进行读写操作
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks> 加载后，可以通过API函数 FreeConsole 释放控制台 </remarks>
    <DllImport("kernel32.dll", SetLastError:=True, ExactSpelling:=True)> _
    Public Shared Function AllocConsole() As Boolean
    End Function

End Class
