using System;
using System.IO;
using System.Text;

namespace Hm2Flac3D
{

    public abstract class InpReader
{
    /// <summary> 用来读取网格的 inp 文件 </summary>
    protected StreamReader sr_inp;

    /// <summary> 用于输出的消息说明 </summary>
    protected StringBuilder _message;

    protected InpReader(StreamReader inpReader, StringBuilder message)
    {
        sr_inp = inpReader;
        _message = message;
    }


    /// <summary>
    /// 读取数据,并返回在读取数据的过程中是否出错。
    /// </summary>
    /// <returns>如果数据提取成功，则返回True，否则返回False</returns>
    /// <remarks>在读取数据时，每一个生成单元的函数中，都会返回最后跳出循环的那个字符串，如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。</remarks>
    public abstract string ReadFile();

    /// <summary>
    /// 读取网格节点数据,并返回在读取数据的过程中是否出错。
    /// </summary>
    /// <returns>如果数据提取成功，则返回True，否则返回False</returns>
    /// <remarks>在读取数据时，每一个生成单元的函数中，都会返回最后跳出循环的那个字符串，如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。</remarks>
    protected string ReadNodesOrGridpoint()
    {
        string strLine = sr_inp.ReadLine();

        //第一步：输出节点
        try
        {
            while (!strLine.StartsWith("*NODE"))
            {
                strLine = sr_inp.ReadLine();
            }
        }
        catch (Exception)
        {
            Console.WriteLine("No Node is detected in the specified inp file!");
            return Convert.ToString(false);
        }
        strLine = Gen_Node(sr_inp);
        return strLine;
    }

    /// <summary>
    /// 创建节点，并返回跳出循环的字符串
    /// </summary>
    /// <param name="sr"> 用来提取数据的inp文件 </param>
    /// <returns></returns>
    protected abstract string Gen_Node(StreamReader sr);

    /// <summary>
    /// 读取数据,并返回在读取数据的过程中是否出错。
    /// </summary>
    /// <param name="sr_inp"></param>
    /// <param name="stringline"> 当前要读取 inp 文件中的那一行数据 </param>
    /// <returns>如果数据提取成功，则返回True，否则返回False</returns>
    /// <remarks>在读取数据时，每一个生成单元的函数中，都会返回最后跳出循环的那个字符串，如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。</remarks>
    protected abstract string ReadElements(StreamReader sr_inp, string stringline);

    /// <summary>
    /// 生成Flac3D中的单元，并返回跳出循环的字符串
    /// </summary>
    /// <param name="type"> 此单元的类型 </param>
    /// <param name="sr"> 用来提取数据的inp文件 </param>
    /// <param name="Component"> 当前读取到Inp中的那一个 Component（即 Hypermesh 中的 Component） </param>
    /// <returns></returns>
    protected abstract string GenerateElement(ElementType type, StreamReader sr, string component);
}
}