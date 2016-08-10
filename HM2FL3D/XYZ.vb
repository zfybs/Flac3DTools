''' <summary>
''' 一个空间的坐标点或者空间的矢量
''' </summary>
Public Class XYZ

    Public X As Double
    Public Y As Double
    Public Z As Double

    Public Sub New(x As Double, y As Double, z As Double)
        Me.X = x
        Me.Y = y
        Me.Z = z
    End Sub

    Public Overrides Function ToString() As String
        Return "( " & X.ToString() & "," & vbTab & Y.ToString() & "," & vbTab & Z.ToString() & " )"
    End Function

#Region "---   空间点的方法"

    ''' <summary> 计算空间两个点的距离 </summary>
    ''' <returns></returns>
    Public Shared Function Distance(node1 As XYZ, node2 As XYZ) As Double
        Return node1.DistanceTo(node2)
    End Function

    ''' <summary> 计算空间两个点的距离 </summary>
    ''' <returns></returns>
    Public Function DistanceTo(node2 As XYZ) As Double
        Return Math.Sqrt((X - node2.X) ^ 2 + (Y - node2.Y) ^ 2 + (Z - node2.Z) ^ 2)
    End Function

    ''' <summary> 一个空间点沿空间的位移矢量移动后的新位置 </summary>
    Public Function Move(vector As XYZ) As XYZ
        Return New XYZ(X + vector.X, Y + vector.Y, Z + vector.Z)
    End Function

    ''' <summary> 从本坐标点指向输入的 node2 的位移矢量 </summary>
    ''' <param name="node2"> 矢量的终点 </param>
    ''' <returns> 一个空间矢量，起始点为 node1，终点为 node2 </returns>
    Public Function VectorTo(node2 As XYZ) As XYZ
        Return New XYZ(node2.X - X, node2.Y - Y, node2.Z - Z)
    End Function

#End Region

#Region "---   空间矢量的方法"


    ''' <summary>
    ''' 将一个空间矢量缩放到指定的长度（方向不变）
    ''' </summary>
    ''' <param name="newLength">缩放后的长度</param>
    ''' <returns> 缩放后的新矢量 </returns>
    Public Function SetLength(newLength As Double) As XYZ
        Dim ratio As Double = newLength / Me.Length()
        Return New XYZ(X * ratio, Y * ratio, Z * ratio)
    End Function

    ''' <summary> 空间矢量的长度 </summary>
    Public Function Length() As Double
        Return Math.Sqrt(X ^ 2 + Y ^ 2 + Z ^ 2)
    End Function


#End Region


End Class