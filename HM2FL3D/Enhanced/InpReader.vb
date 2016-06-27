Imports System.Text.RegularExpressions
Imports System.IO
Imports System.Text

Public MustInherit Class InpReader

    ''' <summary> 用来读取网格的 inp 文件 </summary>
    Protected sr_inp As StreamReader

        ''' <summary> 用于输出的消息说明 </summary>
    Protected _message As StringBuilder

    Protected Sub New(inpReader As StreamReader,message As StringBuilder)
        sr_inp = inpReader
        _message=message
    End Sub


    ''' <summary>
    ''' 读取数据,并返回在读取数据的过程中是否出错。
    ''' </summary>
    ''' <returns>如果数据提取成功，则返回True，否则返回False</returns>
    ''' <remarks>在读取数据时，每一个生成单元的函数中，都会返回最后跳出循环的那个字符串，如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。</remarks>
    Public MustOverride Function ReadFile() As string
    
    ''' <summary>
    ''' 读取网格节点数据,并返回在读取数据的过程中是否出错。
    ''' </summary>
    ''' <returns>如果数据提取成功，则返回True，否则返回False</returns>
    ''' <remarks>在读取数据时，每一个生成单元的函数中，都会返回最后跳出循环的那个字符串，如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。</remarks>
    Protected Function ReadNodesOrGridpoint() As string
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
        strLine = Gen_Node(sr_inp)
        Return strLine
    End Function

    ''' <summary>
    ''' 创建节点，并返回跳出循环的字符串
    ''' </summary>
    ''' <param name="sr"> 用来提取数据的inp文件 </param>
    ''' <returns></returns>
    Protected MustOverride Function Gen_Node(sr As StreamReader) As String

        ''' <summary>
    ''' 读取数据,并返回在读取数据的过程中是否出错。
    ''' </summary>
    ''' <param name="sr_inp"></param>
    ''' <param name="stringline"> 当前要读取 inp 文件中的那一行数据 </param>
    ''' <returns>如果数据提取成功，则返回True，否则返回False</returns>
    ''' <remarks>在读取数据时，每一个生成单元的函数中，都会返回最后跳出循环的那个字符串，如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。</remarks>
    Protected MustOverride Function ReadElements(sr_inp As StreamReader, stringline As String) As string

    ''' <summary>
    ''' 生成Flac3D中的单元，并返回跳出循环的字符串
    ''' </summary>
    ''' <param name="type"> 此单元的类型 </param>
    ''' <param name="sr"> 用来提取数据的inp文件 </param>
    ''' <param name="Component"> 当前读取到Inp中的那一个 Component（即 Hypermesh 中的 Component） </param>
    ''' <returns></returns>
    Protected MustOverride Function GenarateElement(type As ElementType, sr As StreamReader, component As String) As String

End Class
