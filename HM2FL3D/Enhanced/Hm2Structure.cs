using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Hm2Flac3D.Utility;

namespace Hm2Flac3D.Enhanced
{
    public class Hm2Structure : InpReader
    {
        #region ---   Fields

        /// <summary>
        /// 每一组Structural Element 的id号，此id号为全局的，各种不同类型的结构单元之间的id号也没有相同的。
        /// </summary>
        /// <remarks></remarks>
        private int _globalSelId = 1;

        /// <summary>
        /// 一个全局的节点集合，其中包含了所有结构单元中的节点，而且其中的节点编号没有重复。
        /// </summary>
        /// <remarks></remarks>
        private Dictionary<int, XYZ> _allNodes = new Dictionary<int, XYZ>();

        /// <summary>
        /// Liner单元是附着于Hm中Element Set集合中的3D单元的表面创建出来的，当同一个Element Set中，3D单元的表面同时存在S3或S4的二维单元时，应将这些单元放置到相同的sel liner id中。
        /// </summary>
        /// <remarks>
        /// 避免出现如下情况：
        /// Sel Liner id 2 em group LG_CircularCube Range x=(6917.23818,6917.24318) y=(4023.51887,4023.52387) z=(1965.85547,1965.86047)
        /// sel group  Liner-LG_CircularCube range id 2
        /// Sel Liner id 6 em group LG_CircularCube Range x=(6912.63975,6912.64475) y=(4029.43823,4029.44323) z=(1986.85124,1986.85624)
        /// sel group  Liner-LG_CircularCube range id 6
        /// </remarks>
        private Dictionary<string, int> _LinerId_Group = new Dictionary<string, int>();

        /// <summary>
        /// 参考 <seealso cref="_LinerId_Group"/> 的说明
        /// </summary>
        private Dictionary<string, int> _ShellId_Group = new Dictionary<string, int>();

        /*
        /// <summary>
        /// Liner单元是附着于Hm中Element Set集合中的3D单元的表面创建出来的，当同一个Element Set中，3D单元的表面同时存在S3或S4的二维单元时，应将这些单元放置到相同的sel liner id中。
        /// </summary>
        /// <remarks>
        /// 避免出现如下情况：
        /// Sel Liner id 2 em group LG_CircularCube Range x=(6917.23818,6917.24318) y=(4023.51887,4023.52387) z=(1965.85547,1965.86047)
        /// sel group  Liner-LG_CircularCube range id 2
        /// Sel Liner id 6 em group LG_CircularCube Range x=(6912.63975,6912.64475) y=(4029.43823,4029.44323) z=(1986.85124,1986.85624)
        /// sel group  Liner-LG_CircularCube range id 6
        /// </remarks>
        private class Attached2DSel
        {
            public string ElementSetName;
            public int SelId;
            public StreamWriter txtWriter;
        }

        private List<Attached2DSel> Attached2DSels = new List<Attached2DSel>();
        */
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="inpReader"></param>
        /// <param name="structureWriter"></param>
        /// <param name="message">用于输出的消息说明</param>
        public Hm2Structure(StreamReader inpReader, StringBuilder message)
            : base(inpReader, message)
        {
            _message = message;
        }

        /// <summary>
        /// 读取数据，并返回跳出循环的字符串
        /// </summary>
        /// <returns>如果数据提取成功，则返回True，否则返回False</returns>
        /// <remarks>在读取数据时，每一个生成单元的函数中，都会返回最后跳出循环的那个字符串，如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。</remarks>
        public override string ReadFile()
        {
            //第一步：输出节点
            // -----------------------------------------------------------------------------------------------------------
            string strLine = base.ReadNodesOrGridpoint(); // 读取数据

            // -----------------------------------------------------------------------------------------------------------
            //第二步：输出单元
            strLine = ReadElements(sr_inp, strLine); // 读取数据

            // -------------------------------------------------- 后续补充操作 -----------------------------------------------

            //sw_Sel.WriteLine("SEL NODE INIT XPOS ADD 0.0")   '将所有结构单元与土相连的节点位置创建接触

            //'当Liner单元用SelLiner创建时，它只与其side1方向的那一个Zone之间建立了link，此时要手动地为每一个linerNode与其side2方向的Zone之间建立Link。
            //For Each linerNode As Long In listLinerNode
            //    sw_Sel.WriteLine("SEL LINK id {0} {1} side2", linerNode + 10000000000, linerNode)
            //Next
            //' ---------------------------------------------------------------------------------------------------------------

            return strLine;
        }

        /// <summary>
        /// 读取数据，并返回跳出循环的字符串
        /// </summary>
        /// <param name="sr_inp"></param>
        /// <param name="stringline"> 当前要读取 inp 文件中的那一行数据 </param>
        /// <remarks>在读取数据时，每一个生成单元的函数中，都会返回最后跳出循环的那个字符串，如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。</remarks>
        protected override string ReadElements(StreamReader sr_inp, string stringline)
        {
            string pattern_ElementType = @"\*ELEMENT,TYPE=(.+),ELSET=(.+)"; //大致结构为： *ELEMENT,TYPE=B31,ELSET=columns-c1

            var strLine = stringline;
            Match match = default(Match);
            do
            {
                //在Hypermesh导出的inp文件中，可以有很多个 *ELEMENT,TYPE=B31,ELSET=columns 这样的语句，它们是按hypermesh中的Component来进行分组的。
                match = Regex.Match(strLine, pattern_ElementType);
                if (match.Success)
                {
                    //
                    string strEleType = match.Groups[1].Value;

                    string strComponentName = match.Groups[2].Value; // 当前读取到Inp中的那一个 Component（即 Hypermesh 中的 Component）

                    ElementType tp = Hm2Flac3DHandler.GetElementType(strEleType, strComponentName);

                    // 创建 Flac3D 单元
                    strLine = GenerateElement(tp, sr_inp, strComponentName);
                }
                else
                {
                    //如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。
                    strLine = sr_inp.ReadLine();
                }
            } while (strLine != null);
            return strLine;
        }

        protected override string GenerateElement(ElementType type, StreamReader sr_inp, string componentName)
        {
            //
            Flac3dCommandWriters fcw = Flac3dCommandWriters.GetUniqueInstance();
            Flac3DCommandType fct = Hm2Flac3DHandler.GetCommandType(type);
            StreamWriter swSel = fcw.GetWriter(fct, componentName, _globalSelId);
            //
            // 结构单元
            string strLine;
            int selId = 1;
            if (type == ElementType.BEAM)
            {
                strLine = Gen_Beam(sr_inp, swSel, componentName, _globalSelId);
                _globalSelId++;
            }
            else if (type == ElementType.PILE)
            {
                strLine = Gen_Pile(sr_inp, swSel, componentName, _globalSelId);
                _globalSelId++;
            }
            else if (type == ElementType.SHELL3)
            {
                if (_ShellId_Group.ContainsKey(componentName))
                {
                    selId = _ShellId_Group[componentName];
                    strLine = Gen_Shell3(sr_inp, swSel, componentName, selId);
                }
                else
                {
                    _ShellId_Group.Add(componentName, _globalSelId);
                    selId = _globalSelId;
                    strLine = Gen_Shell3(sr_inp, swSel, componentName, selId);
                    _globalSelId++;
                }
            }
            else if (type == (ElementType.Liner3))
            {
                if (_LinerId_Group.ContainsKey(componentName))
                {
                    selId = _LinerId_Group[componentName];
                    strLine = Gen_Liner(sr_inp, swSel, componentName, selId, true);
                }
                else
                {
                    _LinerId_Group.Add(componentName, _globalSelId);
                    selId = _globalSelId;
                    strLine = Gen_Liner(sr_inp, swSel, componentName, selId, true);
                    _globalSelId++;
                }
            }
            else if (type == ElementType.Liner4)
            {
                if (_LinerId_Group.ContainsKey(componentName))
                {
                    selId = _LinerId_Group[componentName];
                    strLine = Gen_Liner(sr_inp, swSel, componentName, selId, false);
                }
                else
                {
                    _LinerId_Group.Add(componentName, _globalSelId);
                    selId = _globalSelId;
                    strLine = Gen_Liner(sr_inp, swSel, componentName, selId, false);
                    _globalSelId++;
                }
            } //Hypermesh中的类型在Flac3d中没有设置对应的类型
            else
            {
                _message.AppendLine($"Warning : Can not match element type \" {type} \"(in component {componentName}) with a corresponding type in Flac3D.");

                //如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。
                strLine = sr_inp.ReadLine();
            }
            return strLine;
        }

        /// <summary>
        /// 创建节点
        /// </summary>
        /// <param name="sr">用来提取数据的inp文件</param>
        /// <returns></returns>
        protected override string Gen_Node(StreamReader sr)
        {
            string strLine = "";
            //
            Flac3dCommandWriters fcw = Flac3dCommandWriters.GetUniqueInstance();
            Flac3DCommandType fct = Hm2Flac3DHandler.GetCommandType(ElementType.SelNode);
            StreamWriter swNode = fcw.GetWriter(fct, Flac3dCommandWriters.FileSelNode, null);
            //
            strLine = sr.ReadLine(); // 节点信息在inp中的大致的结构为： "   16,  10.0     ,  6.6666666666667,  0.0     "
            while (!(strLine.StartsWith("*")))
            {
                // 写入flac文件
                swNode.WriteLine("SEL NODE cid  {0}", strLine);
                // 大致的结构为： ' SEL NODE cid  1440016  0.216969418565193E+02 -0.531659539393860E+02 -0.161000000000000E+02

                //保存节点信息，以供后面创建Liner之用
                string[] comps = strLine.Split(',');

                _allNodes.Add(int.Parse(comps[0]), new XYZ(
                    double.Parse(comps[1]),
                    double.Parse(comps[2]),
                    double.Parse(comps[3])));


                // 读取下一个节点
                strLine = sr.ReadLine(); // 大致的结构为： "        16,  10.0           ,  6.6666666666667,  0.0            "
            }
            return strLine;
        }

        #region   ---  生成不同类型的 Structure 单元

        /// <summary>
        /// 生成桩单元，并返回跳出循环的字符串
        /// </summary>
        /// <param name="component"> 当前读取到Inp中的那一个 Component（即 Hypermesh 中的 Component） </param>
        /// <param name="selId"> 结构单元所属集合的Id </param>
        /// <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
        /// <remarks></remarks>
        private string Gen_Pile(StreamReader sr, StreamWriter sw, string component, int selId)
        {
            string pattern = @"^\s*(\d*),\s*(\d*),\s*(\d*)";
            long eleId = 0;
            long node1 = 0;
            long node2 = 0;
            //
            string strLine = GetFirstDataString(sr); // 大致的结构为：  单元id, 节点1 2 3 4 5 6
            Match match = default(Match);
            GroupCollection groups = default(GroupCollection);
            match = Regex.Match(strLine, pattern);
            while (match.Success)
            {
                groups = match.Groups;
                eleId = Convert.ToInt64(groups[1].Value);
                node1 = Convert.ToInt64(groups[2].Value);
                node2 = Convert.ToInt64(groups[3].Value);
                if (node1 != node2)
                {
                    sw.WriteLine("SEL PILESEL  cid   {0} id   {1} nodes  {2} {3}", eleId, selId, node1, node2);
                    // 大致的结构为：SEL PILESEL  cid   109200 id   109200 nodes  1770004 1769720
                }
                else
                {
                    Console.WriteLine("Warning : Pile element {0} have two nodes with the same id: {1} .", eleId, node1);
                }
                //读取下一个节点
                strLine = sr.ReadLine();
                match = Regex.Match(strLine, pattern);
            }
            sw.WriteLine("sel group  {0} range id {1}", component, selId);
            // 大致的结构为：SEL PILESEL  cid   109200 id   109200 nodes  1770004 1769720
            return strLine;
        }

        /// <summary>
        /// 生成梁单元，并返回跳出循环的字符串
        /// </summary>
        /// <param name="component"> 当前读取到Inp中的那一个 Component（即 Hypermesh 中的 Component） </param>
        /// <param name="selId"> 结构单元所属集合的Id </param>
        /// <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
        /// <remarks></remarks>
        private string Gen_Beam(StreamReader sr, StreamWriter sw, string component, int selId)
        {
            string pattern = @"^\s*(\d*),\s*(\d*),\s*(\d*)";
            long eleId = 0;
            long node1 = 0;
            long node2 = 0;
            //
            string strLine = GetFirstDataString(sr);  // 大致的结构为： "        16,  10.0           ,  6.6666666666667,  0.0            "
            Match match = default(Match);
            GroupCollection groups = default(GroupCollection);
            match = Regex.Match(strLine, pattern);
            while (match.Success)
            {
                groups = match.Groups;
                eleId = Convert.ToInt64(groups[1].Value);
                node1 = Convert.ToInt64(groups[2].Value);
                node2 = Convert.ToInt64(groups[3].Value);
                if (node1 != node2)
                {
                    sw.WriteLine("SEL BEAMSEL  cid   {0} id   {1} nodes  {2} {3}", eleId, selId, node1, node2);
                    // 大致的结构为：SEL PILESEL  cid   109200 id   109200 nodes  1770004 1769720
                }
                else
                {
                    Console.WriteLine("Warning : Beam element {0} have two nodes with the same id: {1} .", eleId, node1);
                }
                //读取下一个节点
                strLine = sr.ReadLine();
                match = Regex.Match(strLine, pattern);
            }
            sw.WriteLine("sel group  {0} range id {1}", component, selId);
            // 大致的结构为：SEL PILESEL  cid   109200 id   109200 nodes  1770004 1769720
            return strLine;
        }

        /// <summary>
        /// 生成三角形壳单元，并返回跳出循环的字符串
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="sw"></param>
        /// <param name="component"> 当前读取到Inp中的那一个 Component（即 Hypermesh 中的 Component） </param>
        /// <param name="selId"> 结构单元所属集合的Id </param>
        /// <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
        /// <remarks></remarks>
        private string Gen_Shell3(StreamReader sr, StreamWriter sw, string component, int selId)
        {
            string pattern = @"^\s*(\d*),\s*(\d*),\s*(\d*),\s*(\d*)";
            long eleId = 0;
            long node1 = 0;
            long node2 = 0;
            long node3 = 0;
            //
            string strLine = GetFirstDataString(sr);  // 大致的结构为： "        16,  10.0           ,  6.6666666666667,  0.0            "
            Match match = default(Match);
            GroupCollection groups = default(GroupCollection);
            match = Regex.Match(strLine, pattern);
            while (match.Success)
            {
                groups = match.Groups;
                eleId = Convert.ToInt64(groups[1].Value);
                node1 = Convert.ToInt64(groups[2].Value);
                node2 = Convert.ToInt64(groups[3].Value);
                node3 = Convert.ToInt64(groups[4].Value);
                //下面这一条语句所创建的Shell，它并不会与其周围的Zone之间建立 Node-to-Zone links.
                sw.WriteLine("SEL SHELLSEL cid   {0} id   {1} ele DKT_CST  nodes  {2} {3} {4}", eleId, selId, node1, node2,
                    node3); // 大致的结构为：SEL SHELLSEL cid    68341 id    68341 ele DKT_CST  nodes  1 2 3
                            //读取下一个节点
                strLine = sr.ReadLine();
                match = Regex.Match(strLine, pattern);
            }
            sw.WriteLine("sel group  {0} range id {1}", component, selId);
            // 大致的结构为：SEL PILESEL  cid   109200 id   109200 nodes  1770004 1769720
            return strLine;
        }

        /// <summary>
        /// 根据共面的三个节点或者四个节点来生成三角形 Liner 单元，并返回跳出循环的字符串
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="sw"></param>
        /// <param name="componentName"> 当前读取到Inp中的那一个 Component（即 Hypermesh 中的 Component） </param>
        /// <param name="threeNodes"> true 表示通过 S3 单元来创建Liner，False 表示通过 S4 来创建 Liner </param>
        /// <param name="selId"> 结构单元所属集合的Id </param>
        /// <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符</returns>
        /// <remarks> Element Set 与对应的 Liner Component的命名规范如下：
        /// 1.Element Set必须以“LG”开头，而且名称中不能包含“-”。比如“LG”、“LG_Zone”都是可以的；
        /// 2.Liner Component的名称必须以“Liner-附着组名”开头。比如当其要附着到组LG中时，“Liner-LGLeft”、“Liner-LG-Left”都是可以的，
        ///         但是“Liner-LGLeft”会将此Liner单元附着到组“LGLeft”中，但是如果Flac3D中并没有创建一个组“LGLeft”的话，自然是会出现异常的。</remarks>
        private string Gen_Liner(StreamReader sr, StreamWriter sw, string componentName, int selId, bool threeNodes)
        {
            string strLine = null;

            string patternLiner = "Liner-(.+)"; //  以 “liner-组名-”开头
            Match match = Regex.Match(componentName, patternLiner, RegexOptions.IgnoreCase);
            if (match.Success) // 说明 Liner的Component命名有效
            {
                // 1、确定附着的组的名称

                // 当 liner 要附着到组 LG 中时，“Liner-LGLeft”、“Liner-LG-Left”都是可以的，
                // 但是“Liner-LGLeft”会将此Liner单元附着到组“LGLeft”中，
                string groupName = match.Groups[1].Value;
                if (groupName.Contains("-"))
                {
                    groupName = groupName.Split('-')[0];
                }

                // 2、进行转换
                if (threeNodes)
                {
                    strLine = Gen_Liner3(sr_inp, sw, componentName, groupName, selId);
                }
                else
                {
                    strLine = Gen_Liner4(sr_inp, sw, componentName, groupName, selId);
                }
            }
            else
            {
                _message.AppendLine("无法识别要将 Liner 单元创建在哪一个 group 上。 对应的Component名称为" + componentName);
            }
            return strLine;
        }

        /// <summary>
        /// 在某Zone的三角形 Face 表面生成一个三角形 Liner 单元，并返回跳出循环的字符串
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="sw"></param>
        /// <param name="componentName"> 当前读取到Inp中的那一个 Component（即 Hypermesh 中的 Component） </param>
        /// <param name="groupName"> 要依托于哪一个group来生成liner单元 </param>
        /// <param name="selId"> 结构单元所属集合的Id </param>
        /// <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
        /// <remarks></remarks>
        private string Gen_Liner3(StreamReader sr, StreamWriter sw, string componentName, string groupName, int selId)
        {
            string pattern = @"^\s*(\d*),\s*(\d*),\s*(\d*),\s*(\d*)";
            int eleId;
            int node1 = 0;
            int node2 = 0;
            int node3 = 0;
            //
            string strLine = GetFirstDataString(sr); // S3类型的信息在inp中，大致的结构为： " 102038,    107275,    105703,    105704"
            Match match;
            GroupCollection groups;
            match = Regex.Match(strLine, pattern);
            while (match.Success)
            {
                groups = match.Groups;
                eleId = Convert.ToInt32(groups[1].Value);
                // 在 inp 中 S3 单元的id号，但是这个单元并不用来创建Flac中的 liner 单元，而是利用其三个节点来创建liner 单元。
                node1 = Convert.ToInt32(groups[2].Value);
                node2 = Convert.ToInt32(groups[3].Value);
                node3 = Convert.ToInt32(groups[4].Value);

                // 获取此三个节点所形成的三角形的形心点
                XYZ centroid = XYZ.FindCentroid(_allNodes[node1], _allNodes[node2], _allNodes[node3]);

                string range = Hm2Flac3DHandler.ExtendCentroid(centroid);

                // 创建与两面都有与 zone 的接触的 Liner 单元
                // Sel Liner  id 1 em group ex1 range x 23.73 23.78 y -0.01 0.01 z 19.65 19.67
                sw.WriteLine("Sel Liner id {0} em group {1} {2}", selId, groupName, range);

                //下面这一条语句所创建的Liner，它并不会与其周围的Zone之间建立 Node-to-Zone links.
                //而且即使用"SEL NODE INIT XPOS ADD 0.0"来创建Node-Zone link，此单元上也只有一面会与Zone之间有link
                //sw.WriteLine("SEL LINERSEL cid   {0} id   {1}  ele DKT_CST  nodes  {2} {3} {4}", eleId, SelId, node1, node2, node3) ' 大致的结构为：SEL SHELLSEL cid    68341 id    68341 ele DKT_CST  nodes  1 2 3

                //读取下一个节点
                strLine = sr.ReadLine();
                match = Regex.Match(strLine, pattern);
            }

            sw.WriteLine("sel group  {0} range id {1}", componentName, selId);
            return strLine;
        }

        /// <summary>
        /// 在某Zone的四边形 Face 表面创建分割此四边形的两个三角形 Liner 单元，
        /// 其在Flac3d中的导入格式为：sel liner id 1 em group ex1 range x 23.73 23.78 y -0.01 0.01 z 19.65 19.67
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="sw"> S4类型的单元在inp文件中的格式为： 102038,    107275,    105703,    105704,    104375  </param>
        /// <param name="componentName"> 当前读取到Inp中的那一个 Component（即 Hypermesh 中的 Component） </param>
        /// <param name="groupName"> 要依托于哪一个group来生成liner单元 </param>
        /// <param name="selId"> 结构单元所属集合的Id </param>
        /// <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
        /// <remarks>创建方法为先找到此四边形的形心，然后将形心点向扩展1mm， 以形成一个体积为1立方毫米的立方体，然后用此立方体来作为创建 Liner 的 range。</remarks>
        private string Gen_Liner4(StreamReader sr, StreamWriter sw, string componentName, string groupName, int selId)
        {
            string pattern = @"^\s*(\d*),\s*(\d*),\s*(\d*),\s*(\d*),\s*(\d*)";
            int eleId;
            int node1 = 0;
            int node2 = 0;
            int node3 = 0;
            int node4 = 0;
            //
            string strLine = GetFirstDataString(sr);// S4 类型的信息在inp中，大致的结构为： " 102038,    107275,    105703,    105704,    104375 "
            Match match = default(Match);
            GroupCollection groups = default(GroupCollection);
            match = Regex.Match(strLine, pattern);
            while (match.Success)
            {
                groups = match.Groups;
                eleId = Convert.ToInt32(groups[1].Value);
                // 在 inp 中 S4 单元的id号，但是这个单元并不用来创建Flac中的 liner 单元，而是利用其四个节点来创建liner 单元。
                node1 = Convert.ToInt32(groups[2].Value);
                node2 = Convert.ToInt32(groups[3].Value);
                node3 = Convert.ToInt32(groups[4].Value);
                node4 = Convert.ToInt32(groups[5].Value);

                // 获取此四个节点所形成的共面四边形的形心点
                XYZ centroid = XYZ.FindCentroid(_allNodes[node1], _allNodes[node2], _allNodes[node3],
                    _allNodes[node4]);

                string range = Hm2Flac3DHandler.ExtendCentroid(centroid);

                // 创建与两面都有与 zone 的接触的 Liner 单元
                // Sel Liner  id 1 em group ex1 range x 23.73 23.78 y -0.01 0.01 z 19.65 19.67
                sw.WriteLine("Sel Liner id {0} em group {1} {2}", selId, groupName, range);

                //下面这一条语句所创建的Liner，它并不会与其周围的Zone之间建立 Node-to-Zone links.
                //而且即使用"SEL NODE INIT XPOS ADD 0.0"来创建Node-Zone link，此单元上也只有一面会与Zone之间有link
                //sw.WriteLine("SEL LINERSEL cid   {0} id   {1}  ele DKT_CST  nodes  {2} {3} {4}", eleId, SelId, node1, node2, node3) ' 大致的结构为：SEL SHELLSEL cid    68341 id    68341 ele DKT_CST  nodes  1 2 3

                //Debug.Print(_allNodes(node1).ToString() & _allNodes(node2).ToString() &
                //            _allNodes(node3).ToString() & _allNodes(node4).ToString() &
                //            centroid.ToString())

                //读取下一个节点
                strLine = sr.ReadLine();
                match = Regex.Match(strLine, pattern);
            }

            sw.WriteLine("sel group  {0} range id {1}", componentName, selId);
            // 大致的结构为：SEL PILESEL  cid   109200 id   109200 nodes  1770004 1769720
            return strLine;
        }

        #endregion
    }
}