using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;


namespace Hm2Flac3D
{
    public class Hm2Zone : InpReader
    {
        #region ---   Fields

        /// <summary> 用来写入 Zone 单元的节点的那个文本。 </summary>
        private StreamWriter sw_Zone;

        /// <summary>
        ///  将用来创建 Liner 的 group 放在 Flac 中 编号为2及以上的 slot 中，以避免与其他的group产生干扰。
        /// </summary>
        /// <remarks> 如果不显式指定 slot，则 group 默认是放在 slot 1 中的。 </remarks>
        private int linerGroupSlot = 2;

        /// <summary>
        /// 一个全局的节点集合，其中包含了所有Zone单元中的GridPoint，而且其中的节点编号没有重复。
        /// </summary>
        /// <remarks></remarks>
        private Dictionary<int, XYZ> _allGridPoints = new Dictionary<int, XYZ>();

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="inpReader"></param>
        /// <param name="message">用于输出的消息说明</param>
        public Hm2Zone(StreamReader inpReader, StringBuilder message)
            : base(inpReader, message)
        {
            //
            Flac3dCommandWriters fcw = Flac3dCommandWriters.GetUniqueInstance();
            StreamWriter swZone = fcw.GetWriter(ElementType.GridPoint, Flac3dCommandWriters.FileZone);
            //
            sw_Zone = swZone;
            _message = message;

            //写入头文件信息
            // 用im zone.dat来在Flac3d中导入网格时，在zone.dat中可以先写入文件头信息，而structures.dat文件中，不能用“*”来写注释
            string strHeading =
                       "* --------------------------------------------------" + "\r\n" +
                       "*  INP (exported from Hypermesh) to FLAC3D " + "\r\n" +
                       "*  Coded by Zengfy. Contact the developer with 619086871@qq.com." + "\r\n" +
                       "*  Latest update date: 2016/11/23 " + "\r\n" +
                       "* --------------------------------------------------" + "\r\n" +
                       "* Generated time: " + DateTime.Today.ToString("yyyy/MM/dd") + "   " +
                       DateTime.Now.ToShortTimeString() + "\r\n" + "\r\n" + "* GRIDPOINTS";

            sw_Zone.WriteLine(strHeading);
        }

        /// <summary>
        /// 读取数据，并返回跳出循环的字符串
        /// </summary>
        /// <returns>如果数据提取成功，则返回True，否则返回False</returns>
        /// <remarks>在读取数据时，每一个生成单元的函数中，都会返回最后跳出循环的那个字符串，如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。</remarks>
        public override string ReadFile()
        {
            //第一步：输出节点
            // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            string strLine = ReadNodesOrGridpoint(); // 读取数据
            // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            //第二步：输出单元
            strLine = ReadElements(sr_inp, strLine); // 读取数据

            return strLine;
        }

        /// <summary> 创建土体的 GridPoint </summary>
        /// <param name="sr"></param>
        protected override string Gen_Node(StreamReader sr)
        {
            string strLine = "";
            strLine = sr.ReadLine(); // 大致的结构为： "        16,  10.0           ,  6.6666666666667,  0.0            "
            while (!(strLine.StartsWith("*")))
            {
                // 写入flac文件
                sw_Zone.WriteLine("G  {0}", strLine);
                // 大致的结构为： ' G           1,  147.5          ,  0.0            ,  -9.35          

                //保存节点信息，以供后面创建其他单元之用
                string[] comps = strLine.Split(',');

                _allGridPoints.Add(int.Parse(comps[0]), new XYZ(
                    double.Parse(comps[1]),
                    double.Parse(comps[2]),
                    double.Parse(comps[3])));


                // 读取下一个节点
                strLine = sr.ReadLine(); // 大致的结构为： "        16,  10.0           ,  6.6666666666667,  0.0            

            }
            return strLine;
        }

        /// <summary>
        /// 读取数据，并返回跳出循环的字符串
        /// </summary>
        /// <param name="sr_inp"></param>
        /// <param name="stringline"> 当前要读取 inp 文件中的那一行数据 </param>
        /// <returns>如果数据提取成功，则返回True，否则返回False</returns>
        /// <remarks>在读取数据时，每一个生成单元的函数中，都会返回最后跳出循环的那个字符串，如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。</remarks>
        protected override string ReadElements(StreamReader sr_inp, string stringline)
        {
            const string pattern_ElementType = @"\*ELEMENT,TYPE=(.+),ELSET=(.+)"; //大致结构为： *ELEMENT,TYPE=B31,ELSET=columns-c1
            const string pattern_ElementSet_LinerGroup = @"\*ELSET, ELSET=(LG.*)";   // 大致结构为：*ELSET, ELSET=LG_C2Wall
            const string pattern_NodeSet_Merge = @"\*NSET, NSET=(LM.*)";      // 大致结构为：*NSET, NSET=LM_WallBottom

            //表示 Hypermesh 中的 SetBrowser 里面的 Element 组，用来作为 在Flac3D 中创建 Liner 时的 group 区间。

            string strLine = stringline;
            Match match;
            do
            {
                //在Hypermesh导出的inp文件中，可以有很多个 *ELEMENT,TYPE=B31,ELSET=columns 这样的语句，它们是按hypermesh中的Component来进行分组的。
                match = Regex.Match(strLine, pattern_ElementType);
                if (match.Success)
                {
                    //
                    string strEleType = match.Groups[1].Value;

                    string strComponentName = match.Groups[2].Value;
                    // 当前读取到Inp中的那一个 Component（即 Hypermesh 中的 Component）

                    ElementType tp = Hm2Flac3DHandler.GetElementType(strEleType, strComponentName);

                    // 创建 Flac3D 单元
                    strLine = GenerateElement(tp, sr_inp, strComponentName);
                }
                else if ((match = Regex.Match(strLine, pattern_ElementSet_LinerGroup)).Success)
                {
                    string groupName = match.Groups[1].Value;

                    if (groupName.Contains("-")) // set 的名称中不能包含“-”
                    {
                        _message.AppendLine($"Warning : Can not export element set : \" {groupName} \", make sure the set name starts with \"LG\"(case ignored) and excludes \"-\" if you want to bind the set to the creation of Liner elements.");
                    }
                    else
                    {
                        // 创建 Flac3D 单元
                        strLine = Gen_LinerGroup(sr_inp, groupName);
                    }
                }
                else if ((match = Regex.Match(strLine, pattern_NodeSet_Merge)).Success)
                {
                    string groupName = match.Groups[1].Value;

                    if (groupName.Contains("-")) // set 的名称中不能包含“-”
                    {
                        _message.AppendLine($"Warning : Can not export element set : \" {groupName} \", make sure the set name starts with \"LM\"(case ignored).");
                    }
                    else
                    {

                        //
                        Flac3dCommandWriters fcw = Flac3dCommandWriters.GetUniqueInstance();
                        StreamWriter swMergeGp = fcw.GetWriter(ElementType.GridPoint, groupName,fileSuffix: Flac3dCommandWriters.FileSuffixSel);
                        //
                        // 创建 Flac3D 单元
                        strLine = Gen_GpMerge(sr_inp, swMergeGp, groupName);
                    }
                }
                else
                {
                    //如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。
                    strLine = sr_inp.ReadLine();
                }
            } while (strLine != null);

            return strLine;
        }

        /// <summary>
        /// 生成Flac3D中的Zone单元，并返回跳出循环的字符串
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sr"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        protected override string GenerateElement(ElementType type, StreamReader sr, string component)
        {
            string strLine = "";
            switch (type)
            {
                // Zone单元
                case ElementType.ZONE_B8:
                    strLine = Gen_Zone_B8(sr_inp, sw_Zone, component);
                    break;
                case ElementType.ZONE_W6:
                    strLine = Gen_Zone_W6(sr_inp, sw_Zone, component);
                    break;
                case ElementType.ZONE_T4:
                    strLine = Gen_Zone_T4(sr_inp, sw_Zone, component);
                    break;

                default: //Hypermesh中的类型在Flac3d中没有设置对应的类型
                    _message.AppendLine(
                        string.Format(
                            "Warning : Can not match element type \" {0} \"(in component {1}) with a corresponding type in Flac3D,",
                            type.ToString(), component));

                    //如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。
                    strLine = sr_inp.ReadLine();
                    break;
            }
            return strLine;
        }

        /// <summary> 在zones.flac3d 文件中写入用来绑定Liner的Group的单元集合 </summary>
        /// <param name="sr"></param>
        /// <param name="groupName"> group的名称必须以LG开头，而且不包含“-” </param>
        /// <returns></returns>
        private string Gen_LinerGroup(StreamReader sr, string groupName)
        {
            string pattern = "\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+)";
            // set 集合在inp文件中的格式为： 132,       133,       134,       135,       136,       137,       138,       139,

            List<long> listEleId = new List<long>();

            //先写入标志语句
            sw_Zone.WriteLine("* ZONES");
            string ptNum = "^\\s*(\\d+)";
            //
            Match match = default(Match);
            GroupCollection groups;
            string strLine = "";
            strLine = sr.ReadLine(); // 大致的结构为：  单元id, 节点1 2 3 4 5 6
            match = Regex.Match(strLine, pattern);
            while (match.Success)
            {
                //将此行中所有的单元id添加到集合中
                groups = match.Groups;

                listEleId.AddRange(new long[]
                {
                    long.Parse(groups[1].Value),
                    long.Parse(groups[2].Value),
                    long.Parse(groups[3].Value),
                    long.Parse(groups[4].Value),
                    long.Parse(groups[5].Value),
                    long.Parse(groups[6].Value),
                    long.Parse(groups[7].Value),
                    long.Parse(groups[8].Value)
                });

                //读取下一个节点
                strLine = sr.ReadLine();
                match = Regex.Match(strLine, pattern);
            }
            //将此Component中的所有单元写入Flac3d中的一个组中
            sw_Zone.WriteLine("* GROUPS");
            // 将用来创建 Liner 的 group 放在 Flac 中 编号为2及以上的 slot 中，以避免与其他的group产生干扰。
            sw_Zone.WriteLine("ZGROUP " + groupName + " SLOT " + linerGroupSlot);
            linerGroupSlot++;
            int num = 0;
            for (num = 0; num <= listEleId.Count - 1; num++)
            {
                sw_Zone.Write("    " + Convert.ToString(listEleId[num]));
                // 写5个即换行
                if (Convert.ToInt32(num + 1) % 5 == 0)
                {
                    sw_Zone.Write("\r\n"); //换行不能用Chr(13)，否则在Flac3d中用im zones.flac3d时，会出现不能识别的报错。
                }
            }
            sw_Zone.WriteLine();
            return strLine;
        }

        /// <summary> 从NSet（Node Set）中提取出节点编号，并写入一个单独的文本，用来执行Merge操作</summary>
        /// <param name="sr"></param>
        /// <param name="groupName"> group的名称必须以LM开头 </param>
        /// <param name="sw_gpMerge"> 要写入哪一个文本 </param>
        /// <returns></returns>
        private string Gen_GpMerge(StreamReader sr, StreamWriter sw_gpMerge, string groupName)
        {
            string pattern = "\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+)";
            // set 集合在inp文件中的格式为： 132,       133,       134,       135,       136,       137,       138,       139,
            // 这八个节点中每一个都对应了一条语句，即 generate merge 0.001 Range x= (23.73, 23.78) y =( -0.01, 0.01)  z= ( 19.65, 19.67) 
            List<int> listNdId = new List<int>();  // Node Set 中所有的节点编号

            //先写入标志语句
            string ptNum = "^\\s*(\\d+)";
            //
            Match match = default(Match);
            GroupCollection groups;
            string strLine = "";
            strLine = sr.ReadLine(); // 大致的结构为：  单元id, 节点1 2 3 4 5 6
            match = Regex.Match(strLine, pattern);
            while (match.Success)
            {
                //将此行中所有的单元id添加到集合中
                groups = match.Groups;

                listNdId.AddRange(new int[]
                {
                    int.Parse(groups[1].Value),
                    int.Parse(groups[2].Value),
                    int.Parse(groups[3].Value),
                    int.Parse(groups[4].Value),
                    int.Parse(groups[5].Value),
                    int.Parse(groups[6].Value),
                    int.Parse(groups[7].Value),
                    int.Parse(groups[8].Value)
                });

                //读取下一个节点
                strLine = sr.ReadLine();
                match = Regex.Match(strLine, pattern);
            }

            //将此Node Set中的每一个节点都生成一条 Generate Merge 命令
            XYZ node;
            string mergeCommand = "Generate Merge " + Hm2Flac3DHandler.CubeRangePrecision * 2 + "  ";
            foreach (int nodeId in listNdId)
            {
                node = _allGridPoints[nodeId];
                string range = Hm2Flac3DHandler.ExtendCentroid(node);

                sw_gpMerge.WriteLine(mergeCommand + range);
            }

            //
            return strLine;
        }

        #region   ---  生成不同类型的 Zone 单元

        /// <summary>
        /// 生成六面体八节点单元，并返回跳出循环的字符串
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="sw_zone"></param>
        /// <param name="Component"> 当前读取到Inp中的那一个 Component（即 Hypermesh 中的 Component） </param>
        /// <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
        /// <remarks>由于在inp文件中，此类单元的节点是分在两行中的，所以不能用“match.Success”作为循环终止的判断</remarks>
        private string Gen_Zone_B8(StreamReader sr, StreamWriter sw_zone, string Component)
        {
            string pattern =
                "\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+)";
            long eleId = 0;
            long node1 = 0;
            long node2 = 0;
            long node3 = 0;
            long node4 = 0;
            long node5 = 0;
            long node6 = 0;
            long node7 = 0;
            long node8 = 0;
            List<long> listEleId = new List<long>();

            //先写入标志语句
            sw_zone.WriteLine("* ZONES");
            string ptNum = "^\\s*(\\d+)";
            //
            Match match = default(Match);
            GroupCollection groups = default(GroupCollection);
            string strLine = "";
            // 在inp文件中的大致的结构为：      87482,     49066,     49224,     49040,     49065,     37816,     37974,     37790, 换行 37815
            strLine = sr.ReadLine();
            do
            {
                strLine = strLine + sr.ReadLine(); // 将两行的内容连接成一行
                match = Regex.Match(strLine, pattern);
                groups = match.Groups;
                eleId = Convert.ToInt64(groups[1].Value);
                node1 = Convert.ToInt64(groups[2].Value);
                node2 = Convert.ToInt64(groups[3].Value);
                node3 = Convert.ToInt64(groups[4].Value);
                node4 = Convert.ToInt64(groups[5].Value);
                node5 = Convert.ToInt64(groups[6].Value);
                node6 = Convert.ToInt64(groups[7].Value);
                node7 = Convert.ToInt64(groups[8].Value);
                node8 = Convert.ToInt64(groups[9].Value);
                //在Flac3d中的大致的结构为： Z B8   65268  47853  47854  47852  43871  47851  43901  43847  43898
                sw_zone.WriteLine("Z B8  {0}   {1}  {2}  {3}  {4}  {5}  {6}  {7}  {8}", eleId, node6, node5, node7,
                    node2, node8, node3, node1, node4);
                listEleId.Add(eleId);
                //读取下一个节点
                strLine = sr.ReadLine();
            } while (Regex.Match(strLine, ptNum).Success);

            //将此Component中的所有单元写入Flac3d中的一个组中
            sw_zone.WriteLine("* GROUPS");
            sw_zone.WriteLine("ZGROUP " + Component);
            int num = 0;
            for (num = 0; num <= listEleId.Count - 1; num++)
            {
                sw_zone.Write("    " + Convert.ToString(listEleId[num]));
                // 写5个即换行
                if (Convert.ToInt32(num + 1) % 5 == 0)
                {
                    sw_zone.Write("\r\n"); //换行不能用Chr(13)，否则在Flac3d中用im zones.flac3d时，会出现不能识别的报错。
                }
            }
            sw_zone.WriteLine();
            return strLine;
        }

        /// <summary>
        /// 生成三棱柱（五面体、六节点）单元，并返回跳出循环的字符串
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="sw_zone"></param>
        /// <param name="Component"> 当前读取到Inp中的那一个 Component（即 Hypermesh 中的 Component） </param>
        /// <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
        /// <remarks></remarks>
        private string Gen_Zone_W6(StreamReader sr, StreamWriter sw_zone, string Component)
        {
            string pattern = "\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+)";
            long eleId = 0;
            long node1 = 0;
            long node2 = 0;
            long node3 = 0;
            long node4 = 0;
            long node5 = 0;
            long node6 = 0;
            List<long> listEleId = new List<long>();

            //先写入标志语句
            sw_zone.WriteLine("* ZONES");
            string ptNum = "^\\s*(\\d+)";
            //
            Match match = default(Match);
            GroupCollection groups = default(GroupCollection);
            string strLine = "";
            strLine = sr.ReadLine(); // 大致的结构为：  单元id, 节点1 2 3 4 5 6
            match = Regex.Match(strLine, pattern);
            while (match.Success)
            {
                groups = match.Groups;
                eleId = Convert.ToInt64(groups[1].Value);
                node1 = Convert.ToInt64(groups[2].Value);
                node2 = Convert.ToInt64(groups[3].Value);
                node3 = Convert.ToInt64(groups[4].Value);
                node4 = Convert.ToInt64(groups[5].Value);
                node5 = Convert.ToInt64(groups[6].Value);
                node6 = Convert.ToInt64(groups[7].Value);
                sw_zone.WriteLine("Z W6  {0}   {1}  {2}  {3}  {4}  {5}  {6}", eleId, node1, node3, node4, node2, node6,
                    node5);
                listEleId.Add(eleId);
                //读取下一个节点
                strLine = sr.ReadLine();
                match = Regex.Match(strLine, pattern);
            }

            //将此Component中的所有单元写入Flac3d中的一个组中
            sw_zone.WriteLine("* GROUPS");
            sw_zone.WriteLine("ZGROUP " + Component);
            int num = 0;
            for (num = 0; num <= listEleId.Count - 1; num++)
            {
                sw_zone.Write("    " + Convert.ToString(listEleId[num]));
                // 写5个即换行
                if (Convert.ToInt32(num + 1) % 5 == 0)
                {
                    sw_zone.Write("\r\n"); //换行不能用Chr(13)，否则在Flac3d中用im zones.flac3d时，会出现不能识别的报错。
                }
            }
            sw_zone.WriteLine();
            return strLine;
        }

        /// <summary>
        /// 生成四面体（金字塔形、四节点）单元，并返回跳出循环的字符串
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="sw_zone"></param>
        /// <param name="Component"> 当前读取到Inp中的那一个 Component（即 Hypermesh 中的 Component） </param>
        /// <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
        /// <remarks></remarks>
        private string Gen_Zone_T4(StreamReader sr, StreamWriter sw_zone, string Component)
        {
            string pattern = "\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+)";
            long eleId = 0;
            long node1 = 0;
            long node2 = 0;
            long node3 = 0;
            long node4 = 0;
            List<long> listEleId = new List<long>();

            //先写入标志语句
            sw_zone.WriteLine("* ZONES");
            string ptNum = "^\\s*(\\d+)";
            //
            Match match = default(Match);
            GroupCollection groups = default(GroupCollection);
            string strLine = "";
            strLine = sr.ReadLine(); // 大致的结构为：  单元id, 节点1 2 3 4 5 6
            match = Regex.Match(strLine, pattern);
            while (match.Success)
            {
                groups = match.Groups;
                eleId = Convert.ToInt64(groups[1].Value);
                node1 = Convert.ToInt64(groups[2].Value);
                node2 = Convert.ToInt64(groups[3].Value);
                node3 = Convert.ToInt64(groups[4].Value);
                node4 = Convert.ToInt64(groups[5].Value);
                sw_zone.WriteLine("Z T4  {0}   {1}  {2}  {3}  {4}", eleId, node1, node2, node3, node4);
                listEleId.Add(eleId);
                //读取下一个节点
                strLine = sr.ReadLine();
                match = Regex.Match(strLine, pattern);
            }
            //将此Component中的所有单元写入Flac3d中的一个组中
            sw_zone.WriteLine("* GROUPS");
            sw_zone.WriteLine("ZGROUP " + Component);
            int num = 0;
            for (num = 0; num <= listEleId.Count - 1; num++)
            {
                sw_zone.Write("    " + Convert.ToString(listEleId[num]));
                // 写5个即换行
                if (Convert.ToInt32(num + 1) % 5 == 0)
                {
                    sw_zone.Write("\r\n"); //换行不能用Chr(13)，否则在Flac3d中用im zones.flac3d时，会出现不能识别的报错。
                }
            }
            sw_zone.WriteLine();
            return strLine;
        }

        #endregion
    }
}