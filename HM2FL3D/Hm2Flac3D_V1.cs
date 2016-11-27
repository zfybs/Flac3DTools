using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Hm2Flac3D.Enhanced;
using Hm2Flac3D.Utility;

namespace Hm2Flac3D
{
    /// <summary>
    /// Hm2Flac3D 1.0 版，一个 Console Application，并且其中不包含创建 Liner 单元的功能
    /// </summary>
    sealed class Hm2Flac3D_V1
    {
        static StreamReader sr_inp;
        static StreamWriter sw_Zone;
        static StreamWriter sw_Sel;

        /// <summary>
        /// 是否是按编辑模式进行数据提取。
        /// 在编辑模式下，文件的位置，Abaqus中的单元类型与Flac3d中的单元类型的对应关系等都是可以人工指定的。
        /// </summary>
        /// <remarks></remarks>
        private static bool blnEditMode;

        /// <summary>
        /// 每一组Structural Element 的id号，此id号为全局的，各种不同类型的结构单元之间的id号也没有相同的。
        /// </summary>
        /// <remarks></remarks>
        private static long SelId = 1;

        /// <summary>
        /// 一个全局的节点集合，其中包含了所有Liner单元中的节点，而且其中的节点编号没有重复。
        /// </summary>
        /// <remarks></remarks>
        private static SortedSet<long> listLinerNode = new SortedSet<long>();

        public static void Main()
        {
            string path_inp = "";
            string path_flacStructure = "";
            string path_flacZone = "";
            Console.WriteLine(@"******** CONVERT INP CODE(EXPORTED FROM HYPERMESH) TO FLAC3D ********");
            blnEditMode = getReply(@"Run this application in edit mode.
Or press Enter to extract data from the Hm2Flac3D.inp in the same folder as this executing file.
");

            // 确定文件路径
            if (blnEditMode && (!getReply("Use the default file path.")))
            {
                Console.WriteLine(@"Input the file path of the inp file: ");
                path_inp = Console.ReadLine();
                if (!File.Exists(path_inp))
                {
                    Console.WriteLine(@"Error : Specified inp file not detected!");
                    Console.Read();
                    return;
                }
                string d = Path.GetDirectoryName(path_inp);
                path_flacStructure = Path.Combine(d, "structures.dat");
                path_flacZone = Path.Combine(d, "Zones.Flac3D");
            }
            else //默认模式
            {
                string d = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;

                path_inp = Path.Combine(d, "Hm2Flac3D.inp");
                if (!File.Exists(path_inp))
                {
                    Console.WriteLine(@"Error : Specified inp file not detected!");
                    Console.Read();
                    return;
                }
                path_flacStructure = Path.Combine(d, "structures.dat");
                path_flacZone = Path.Combine(d, "Zones.Flac3D");
            }

            // 打开文件
            FileStream Fileinp = File.Open(path_inp, FileMode.Open);
            FileStream Fileflc_Sel = File.Create(path_flacStructure);
            FileStream Fileflc_Zone = File.Create(path_flacZone);
            sr_inp = new StreamReader(Fileinp);
            sw_Sel = new StreamWriter(Fileflc_Sel);
            sw_Zone = new StreamWriter(Fileflc_Zone);

            //写入头文件信息

            // 用im zone.dat来在Flac3d中导入网格时，在zone.dat中可以先写入文件头信息，而structures.dat文件中，不能用“*”来写注释
            string strHeading =
                         "* --------------------------------------------------" + "\r\n" +
                         "*  INP (exported from Hypermesh) to FLAC3D " + "\r\n" +
                         "*  Coded by Zengfy. Contact the developer with 619086871@qq.com." + "\r\n" +
                         "*  Latest update time: 2016/11/23 " + "\r\n" +
                         "* --------------------------------------------------" + "\r\n" +
                         "* Generated time: " + DateTime.Today.ToString("yyyy/MM/dd") + "   " +
                         DateTime.Now.ToShortTimeString() + "\r\n" + "\r\n" + "* GRIDPOINTS";

            sw_Zone.WriteLine(strHeading);

            // -----------------------------------------------------------------------------------------------------------
            bool blnSuccedd = ReadFile(sr_inp, sw_Sel, sw_Zone); // 读取数据
            // -----------------------------------------------------------------------------------------------------------                   

            //操作完成后关闭资源
            sr_inp.Close();
            sw_Sel.Close();
            sw_Zone.Close();
            Fileinp.Close();
            Fileflc_Sel.Close();
            Fileflc_Zone.Close();
            Console.WriteLine("\r\n" + @"******** 数据提取完成 ********");
            Console.Read();
        }

        /// <summary>
        /// 读取数据,并返回在读取数据的过程中是否出错。
        /// </summary>
        /// <param name="sr_inp"></param>
        /// <param name="sw_Sel"></param>
        /// <param name="sw_Zone"></param>
        /// <returns>如果数据提取成功，则返回True，否则返回False</returns>
        /// <remarks>在读取数据时，每一个生成单元的函数中，都会返回最后跳出循环的那个字符串，如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。</remarks>
        private static bool ReadFile(StreamReader sr_inp, StreamWriter sw_Sel, StreamWriter sw_Zone)
        {
            string pattern_ElementType = "\\*ELEMENT,TYPE=(.+),ELSET=(.+)"; //大致结构为： *ELEMENT,TYPE=B31,ELSET=columns-c1
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
                return false;
            }
            strLine = Gen_Node(sr_inp, sw_Sel, sw_Zone);
            //第二步：输出单元
            Match match = default(Match);
            do
            {
                //在Hypermesh导出的inp文件中，可以有很多个 *ELEMENT,TYPE=B31,ELSET=columns 这样的语句，它们是按hypermesh中的Component来进行分组的。
                match = Regex.Match(strLine, pattern_ElementType);
                if (match.Success)
                {
                    //如果
                    Match m = Regex.Match(strLine, pattern_ElementType);
                    string strEleType = m.Groups[1].Value;
                    string strComponentName = m.Groups[2].Value;
                    ElementType tp = Hm2Flac3DHandler.GetElementType(strEleType, strComponentName);
                    switch (tp)
                    {
                        // 结构单元
                        case ElementType.BEAM:
                            strLine = Gen_Beam(sr_inp, sw_Sel, strComponentName);
                            break;
                        case ElementType.PILE:
                            strLine = Gen_Pile(sr_inp, sw_Sel, strComponentName);
                            break;
                        case ElementType.SHELL3:
                            strLine = Gen_Shell3(sr_inp, sw_Sel, strComponentName);
                            break;
                        //Case ElementType.Liner3
                        //    strLine = Gen_Liner3(sr_inp, sw_Sel, strComponentName)


                        // Zone单元
                        case ElementType.ZONE_B8:
                            strLine = Gen_Zone_B8(sr_inp, sw_Zone, strComponentName);
                            break;
                        case ElementType.ZONE_W6:
                            strLine = Gen_Zone_W6(sr_inp, sw_Zone, strComponentName);
                            break;
                        case ElementType.ZONE_T4:
                            strLine = Gen_Zone_T4(sr_inp, sw_Zone, strComponentName);
                            break;

                        default: //Hypermesh中的类型在Flac3d中没有设置对应的类型
                            string strAsk =
                                string.Format(
                                    "Warning : Can not match element type \" {0} \"(in component {1}) with a corresponding type in Flac3D,",
                                    strEleType, strComponentName);
                            Console.WriteLine(strAsk);
                            //如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。
                            strLine = sr_inp.ReadLine();
                            break;
                    }
                }
                else
                {
                    //如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。
                    strLine = sr_inp.ReadLine();
                }
            } while ((strLine != null) && (!strLine.StartsWith("**HMASSEM")));

            // -------------------------------------------------- 后续补充操作 -----------------------------------------------

            //sw_Sel.WriteLine("SEL NODE INIT XPOS ADD 0.0")   '将所有结构单元与土相连的节点位置创建接触

            //'当Liner单元用SelLiner创建时，它只与其side1方向的那一个Zone之间建立了link，此时要手动地为每一个linerNode与其side2方向的Zone之间建立Link。
            //For Each linerNode As Long In listLinerNode
            //    sw_Sel.WriteLine("SEL LINK id {0} {1} side2", linerNode + 10000000000, linerNode)
            //Next
            //' ---------------------------------------------------------------------------------------------------------------
            return true;
        }

        /// <summary>
        /// 创建节点
        /// </summary>
        /// <param name="sr">用来提取数据的inp文件</param>
        /// <param name="sw_Sel">用来写入Structural element的节点的那个文本。</param>
        /// <param name="sw_zone">用来写入zone单元的节点的那个文本。</param>
        /// <remarks></remarks>
        private static string Gen_Node(StreamReader sr, StreamWriter sw_Sel, StreamWriter sw_zone)
        {
            string strLine = "";
            strLine = sr.ReadLine(); // 大致的结构为： "        16,  10.0           ,  6.6666666666667,  0.0            "
            while (!(strLine.StartsWith("*")))
            {
                sw_Sel.WriteLine("SEL NODE cid  {0}", strLine);
                // 大致的结构为： ' SEL NODE cid  1440016  0.216969418565193E+02 -0.531659539393860E+02 -0.161000000000000E+02
                sw_zone.WriteLine("G  {0}", strLine);
                strLine = sr.ReadLine(); // 大致的结构为： "        16,  10.0           ,  6.6666666666667,  0.0            "
            }
            return strLine;
        }

        #region   ---  生成不同类型的单元

        /// <summary>
        /// 生成桩单元，并返回跳出循环的字符串
        /// </summary>
        /// <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
        /// <remarks></remarks>
        private static string Gen_Pile(StreamReader sr, StreamWriter sw, string component)
        {
            string pattern = "\\s*(\\d*),\\s*(\\d*),\\s*(\\d*)";
            string strLine = "";
            long eleId = 0;
            long node1 = 0;
            long node2 = 0;
            //
            strLine = sr.ReadLine(); // 大致的结构为： "        16,  10.0           ,  6.6666666666667,  0.0            "
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
                    sw.WriteLine("SEL PILESEL  cid   {0} id   {1} nodes  {2} {3}", eleId, SelId, node1, node2);
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
            sw.WriteLine("sel group  {0} range id {1}", component, SelId);
            // 大致的结构为：SEL PILESEL  cid   109200 id   109200 nodes  1770004 1769720
            SelId++;
            return strLine;
        }

        /// <summary>
        /// 生成梁单元，并返回跳出循环的字符串
        /// </summary>
        /// <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
        /// <remarks></remarks>
        private static string Gen_Beam(StreamReader sr, StreamWriter sw, string component)
        {
            string pattern = "\\s*(\\d*),\\s*(\\d*),\\s*(\\d*)";
            string strLine = "";
            long eleId = 0;
            long node1 = 0;
            long node2 = 0;
            //
            strLine = sr.ReadLine(); // 大致的结构为： "        16,  10.0           ,  6.6666666666667,  0.0            "
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
                    sw.WriteLine("SEL BEAMSEL  cid   {0} id   {1} nodes  {2} {3}", eleId, SelId, node1, node2);
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
            sw.WriteLine("sel group  {0} range id {1}", component, SelId);
            // 大致的结构为：SEL PILESEL  cid   109200 id   109200 nodes  1770004 1769720
            SelId++;
            return strLine;
        }

        /// <summary>
        /// 生成三角形壳单元，并返回跳出循环的字符串
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="sw"></param>
        /// <param name="Component"></param>
        /// <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
        /// <remarks></remarks>
        private static string Gen_Shell3(StreamReader sr, StreamWriter sw, string Component)
        {
            string pattern = "\\s*(\\d*),\\s*(\\d*),\\s*(\\d*),\\s*(\\d*)";
            string strLine = "";
            long eleId = 0;
            long node1 = 0;
            long node2 = 0;
            long node3 = 0;
            //
            strLine = sr.ReadLine(); // 大致的结构为： "        16,  10.0           ,  6.6666666666667,  0.0            "
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
                sw.WriteLine("SEL SHELLSEL cid   {0} id   {1} ele DKT_CST  nodes  {2} {3} {4}", eleId, SelId, node1,
                    node2, node3); // 大致的结构为：SEL SHELLSEL cid    68341 id    68341 ele DKT_CST  nodes  1 2 3
                //读取下一个节点
                strLine = sr.ReadLine();
                match = Regex.Match(strLine, pattern);
            }
            sw.WriteLine("sel group  {0} range id {1}", Component, SelId);
            // 大致的结构为：SEL PILESEL  cid   109200 id   109200 nodes  1770004 1769720
            SelId++;
            return strLine;
        }

        /// <summary>
        /// 生成三角形壳单元，并返回跳出循环的字符串
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="sw"></param>
        /// <param name="Component"></param>
        /// <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
        /// <remarks></remarks>
        private static string Gen_Liner3(StreamReader sr, StreamWriter sw, string Component)
        {
            string pattern = "\\s*(\\d*),\\s*(\\d*),\\s*(\\d*),\\s*(\\d*)";
            string strLine = "";
            long eleId = 0;
            long node1 = 0;
            long node2 = 0;
            long node3 = 0;
            //
            strLine = sr.ReadLine(); // 大致的结构为： "        16,  10.0           ,  6.6666666666667,  0.0            "
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
                //下面这一条语句所创建的Liner，它并不会与其周围的Zone之间建立 Node-to-Zone links.
                sw.WriteLine("SEL LINERSEL cid   {0} id   {1}  ele DKT_CST  nodes  {2} {3} {4}", eleId, SelId, node1,
                    node2, node3); // 大致的结构为：SEL SHELLSEL cid    68341 id    68341 ele DKT_CST  nodes  1 2 3
                listLinerNode.Add(node1);
                listLinerNode.Add(node2);
                listLinerNode.Add(node3);
                //读取下一个节点
                strLine = sr.ReadLine();
                match = Regex.Match(strLine, pattern);
            }

            sw.WriteLine("sel group  {0} range id {1}", Component, SelId);
            // 大致的结构为：SEL PILESEL  cid   109200 id   109200 nodes  1770004 1769720
            SelId++;
            return strLine;
        }

        /// <summary>
        /// 生成六面体八节点单元，并返回跳出循环的字符串
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="sw_zone"></param>
        /// <param name="Component"></param>
        /// <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
        /// <remarks>由于在inp文件中，此类单元的节点是分在两行中的，所以不能用“match.Success”作为循环终止的判断</remarks>
        private static string Gen_Zone_B8(StreamReader sr, StreamWriter sw_zone, string Component)
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
        /// <param name="Component"></param>
        /// <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
        /// <remarks></remarks>
        private static string Gen_Zone_W6(StreamReader sr, StreamWriter sw_zone, string Component)
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
        /// <param name="Component"></param>
        /// <returns>在这一区块中，最后一次读取的字符，即跳出循环的字符，比如：**HWCOLOR COMP 57   60 或者 **HMASSEM  2   6 A_M</returns>
        /// <remarks></remarks>
        private static string Gen_Zone_T4(StreamReader sr, StreamWriter sw_zone, string Component)
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

        /// <summary>
        /// 在Console中进行手动判断，如果用户输入Yes或者Y，则返回布尔值True，否则返回False
        /// </summary>
        /// <param name="question">要询问用户的问题</param>
        /// <returns></returns>
        /// <remarks>如果用户输入Yes或者Y，则返回布尔值True，否则返回False</remarks>
        private static bool getReply(string question)
        {
            Console.WriteLine(question + "Yes[Y] or No[N] ?");
            string ans = Console.ReadLine();
            if (string.Compare(ans, "y", true) == 0 || string.Compare(ans, "yes", true) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}