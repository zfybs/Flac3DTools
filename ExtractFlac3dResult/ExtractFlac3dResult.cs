using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;

namespace ExtractFlac3dResult
{
    sealed class ExtractFlac3dResult
    {
        #region    ---   Declarations

        /// <summary>
        /// 进行数据提取的那此.dat文本
        /// </summary>
        /// <remarks></remarks>
        static StreamReader sr_dat;

        /// <summary>
        /// 是否是按编辑模式进行数据提取。
        /// 在编辑模式下，文件的位置，Abaqus中的单元类型与Flac3d中的单元类型的对应关系等都是可以人工指定的。
        /// </summary>
        /// <remarks></remarks>
        private static bool blnEditMode;

        private static Application ExcelApp;
        private static Workbook wkbk;

        /// <summary>
        /// 第一条计算结果数据（数值）所在的行号
        /// </summary>
        /// <remarks></remarks>
        const byte cstRow_FirstData = 2;

        /// <summary>
        /// 指定文件夹中所有可以用来进行数据提取的文档
        /// </summary>
        /// <remarks></remarks>
        private static List<string> Path_Data = new List<string>();

        /// <summary>
        /// 当前所读取到的行号
        /// </summary>
        /// <remarks></remarks>
        private static UInt32 LineNum = 0;

        #endregion

        public static void Main()
        {
            string path_Wkbk = "";
            Console.WriteLine("******** Extract data from FLAC3D result file. (Zengfy 2015-12-18) ********" + "\r\n" +
                              "");
            // 确定文件路径
            //默认模式
            Console.WriteLine(@"Choose the way you want to start the extraction: ");
            Console.WriteLine(@"1. Input the file path of the dat file;");
            Console.WriteLine(@"2. Type the file name if this file is in the application folder;");
            Console.WriteLine(@"3. Press ""Enter"" and search all the .dat files in the application folder.");
            string dirApp = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;

            string reply = Console.ReadLine();
            if (reply.Trim() == "")
            {
                //直接搜索程序所在文件夹中的第一个.dat文件
                string[] files = Directory.GetFiles(dirApp);
                foreach (string Fl in files)
                {
                    if (String.Compare(Path.GetExtension(Fl), @".dat", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Path_Data.Add(Fl);
                    }
                }
            }
            else
            {
                if (Path.GetDirectoryName(reply) != "") //说明输入的是文件的绝对路径
                {
                    if (File.Exists(reply))
                    {
                        Path_Data.Add(reply);
                    }
                    else
                    {
                        Console.WriteLine("Can not find the specified file.");
                        Console.ReadLine();
                        return;
                    }
                }
                else // 说明输入的是文件的相对路径，即相对于当前这个.exe程序所在的文件夹
                {
                    var CombinedFile = Path.Combine(dirApp, reply);
                    if (File.Exists(CombinedFile.ToString()))
                    {
                        Path_Data.Add(CombinedFile);
                    }
                    else
                    {
                        Console.WriteLine("Can not find the specified file.");
                        Console.ReadLine();
                        return;
                    }
                }
            }
            //保存数据的Excel文档
            if (Path_Data.Count > 0)
            {
                if (File.Exists(Path_Data[0]))
                {
                    string dirData = Path.GetDirectoryName(Path_Data[0]);
                    path_Wkbk = Path.Combine(dirData, "Flac3D-ResultData.xlsx");
                }
                else
                {
                    Console.WriteLine("Can not find the specified file.");
                    Console.ReadLine();
                    return;
                }
            }
            else
            {
                Console.WriteLine("Can not find the specified file.");
                Console.ReadLine();
                return;
            }

            // ----------------------------------  开始提取数据 -------------------------------------------------------------------
            try
            {
                for (short DataFileNum = 0; DataFileNum <= Path_Data.Count - 1; DataFileNum++)
                {
                    string path_Datafile = Path_Data[DataFileNum]; // 进行数据提取的这个.dat文件的绝对路径
                    Console.WriteLine("Extracting data from ： {0}", path_Datafile);
                    // 打开文件
                    FileStream datFile = File.Open(path_Datafile, FileMode.Open);
                    sr_dat = new StreamReader(datFile);
                    // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    List<DataResultList> AllData = new List<DataResultList>(); //某一个结果文本中的所有数据，在List中的第一项用来记录此类
                    try
                    {
                        AllData = ReadFile(sr_dat); // 读取数据
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error : Data in the specified file \"{0}\"can not be extracted correctly.",
                            path_Datafile);
                        Console.ReadLine();
                        continue;
                    }
                    // ------------------- 将数据写入Excel工作簿中 -------------------------------------------------------------------------------------------------------------------------
                    if (AllData.Count > 0)
                    {
                        //打开工作簿
                        if (wkbk == null)
                        {
                            if (File.Exists(path_Wkbk))
                            {
                                // wkbk = Interaction.GetObject(path_Wkbk, null) as Workbook;
                                wkbk = Marshal.BindToMoniker(path_Wkbk) as Workbook;
                                ExcelApp = wkbk.Application;
                            }
                            else
                            {
                                if (ExcelApp == null)
                                {
                                    ExcelApp = new Application();
                                }
                                wkbk = ExcelApp.Workbooks.Add();
                                wkbk.SaveAs(path_Wkbk);
                            }
                        }
                        else
                        {
                            ExcelApp = wkbk.Application;
                        }
                        string filename = Path.GetFileNameWithoutExtension(path_Datafile);

                        // ------------------------- 指定要将数据写入哪一个工作表 ---------------------------
                        Worksheet sht = null;

                        foreach (Worksheet testSht in wkbk.Worksheets)
                        {
                            if (string.Compare(testSht.Name, filename, true) == 0)
                            {
                                sht = testSht;
                                break;
                            }
                        }
                        if (sht == null)
                        {
                            sht = wkbk.Worksheets.Add();
                            sht.Name = filename;
                        }
                        ExcelApp.ScreenUpdating = false;
                        WriteDataToExcel(AllData, sht); // 将数据写入Excel工作簿中
                        ExcelApp.ScreenUpdating = true;
                        // ---------------------- 设置此Worksheet的界面效果 ----------------------
                        sht.Activate();
                        Window with_1 = ExcelApp.Windows.Item[wkbk.Name];
                        with_1.Activate();
                        // 窗口的拆分与冻结
                        with_1.SplitRow = 1;
                        with_1.SplitColumn = 4;
                        with_1.FreezePanes = true;
                    }
                    //' --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    //操作完成后关闭资源
                    sr_dat.Close();
                    datFile.Close();
                }
            }
            finally
            {
                if (wkbk != null)
                {
                    wkbk.Save();
                }
                if (ExcelApp != null)
                {
                    ExcelApp.ScreenUpdating = true;
                }
            }

            Console.WriteLine("\r\n" + "******** 数据提取完成 ********");

            if (ExcelApp.Visible == true)
            {
                if (ExcelApp.WindowState == XlWindowState.xlMinimized)
                {
                    ExcelApp.WindowState = XlWindowState.xlMaximized;
                }
            }
            else
            {
                wkbk.Save();
                wkbk.Close();
            }
            Console.ReadLine();
            // 界面UI
        }

        /// <summary>
        /// 读取数据,并返回在读取数据的过程中是否出错。
        /// </summary>
        /// <param name="Reader"></param>
        /// <returns>如果数据提取成功，则返回True，否则返回False</returns>
        /// <remarks>在读取数据时，每一个生成单元的函数中，都会返回最后跳出循环的那个字符串，如果某行字符串没有进行任何的数据提取，或者进行单元类型的判断，则继续读取下一行字符。</remarks>
        private static List<DataResultList> ReadFile(StreamReader Reader)
        {
            string strLine = "";
            DataResultList blockData = new DataResultList();
            List<DataResultList> AllData = new List<DataResultList>();
            //
            strLine = Reader.ReadLine();
            LineNum++;
            while (strLine != null)
            {
                if (IsData(strLine))
                {
                    blockData = GetData(Reader, strLine);
                    AllData.Add(blockData);
                }
                strLine = Reader.ReadLine();
                LineNum++;
            }

            return AllData;
        }

        /// <summary>
        /// 提取一个数据块中的所有数据
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="strLine">这一行字符必须要是一个数据块中的第一行数据</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private static DataResultList GetData(StreamReader sr, string strLine)
        {
            List<long> nodeId = new List<long>();
            List<double> x = new List<double>();
            List<double> y = new List<double>();
            List<double> z = new List<double>();
            string[] strData = null;
            strData = strLine.Split(new[] { '(', ',', ',', ')' });

            do
            {
                nodeId.Add(long.Parse(strData[0]));
                x.Add(double.Parse(strData[1]));
                y.Add(double.Parse(strData[2]));
                z.Add(double.Parse(strData[3]));
                //
                strLine = sr.ReadLine();
                LineNum++;
                if (strLine == null)
                {
                    // 如果 strLine 的值为 Nothing，说明已经读到了文本文件的结尾了。
                    // Console.WriteLine("Nothing 所在的行号为： " & LineNum)
                    break;
                }
                strData = strLine.Split(new[] { '(', ',', ',', ')' });
            } while (IsData(strLine));

            // 此时的strLine为不满足数据格式的一行字符串
            DataResultList result = new DataResultList();
            result.NodeId = nodeId;
            result.X = x;
            result.Y = y;
            result.Z = z;
            result.strEscape = strLine;
            return result;
        }

        /// <summary>
        /// 精确判断某一行字符是否是一行数据
        /// </summary>
        /// <param name="strLine"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private static bool IsData(string strLine)
        {
            bool res = true;
            string[] Data = strLine.Split(new[] { '(', ',', ',', ')' });
            if (Data.Length >= 4)
            {
                //第一个数值是否是一个整数
                long numLong;
                if (!long.TryParse(Data[0], out numLong))
                {
                    res = false;
                    return res;
                }
                //后面三个数值是否是一个
                double numDouble;
                if (!double.TryParse(Data[1], out numDouble))
                {
                    res = false;
                    return res;
                }
                if (!double.TryParse(Data[2], out numDouble))
                {
                    res = false;
                    return res;
                }
                if (!double.TryParse(Data[3], out numDouble))
                {
                    res = false;
                    return res;
                }
            }
            else
            {
                res = false;
                return res;
            }
            return res;
        }


        /// <summary>
        /// 将一个dat文本中的所有数据写入对应的Worksheet中。
        /// </summary>
        /// <param name="AllData">一个dat文本中的所有数据。注意：在一个文本中，
        /// 记录节点位置的坐标点的数量与记录节点位移的坐标点的数量并不一定是相同的。</param>
        /// <param name="wkSheet">要进行写入的工作表</param>
        /// <remarks></remarks>
        private static void WriteDataToExcel(List<DataResultList> AllData, Worksheet wkSheet)
        {
            Application app = wkSheet.Application;
            int BlockCount = AllData.Count;
            uint RowsCount = 0; // 每一个数据块的数据行数
            DataResultList results = new DataResultList();
            // 先写入测点的位置信息
            results = AllData[0];
            RowsCount = (uint)results.NodeId.Count;
            wkSheet.Range["A1:D1"].Value = new[] { "ID", "X", "Y", "Z" };
            wkSheet.Range[wkSheet.Cells[cstRow_FirstData, 1], wkSheet.Cells[cstRow_FirstData + RowsCount - 1, 1]].Value
                = app.WorksheetFunction.Transpose(results.NodeId.ToArray());
            wkSheet.Range[wkSheet.Cells[cstRow_FirstData, 2], wkSheet.Cells[cstRow_FirstData + RowsCount - 1, 2]].Value
                = app.WorksheetFunction.Transpose(results.X.ToArray());
            wkSheet.Range[wkSheet.Cells[cstRow_FirstData, 3], wkSheet.Cells[cstRow_FirstData + RowsCount - 1, 3]].Value
                = app.WorksheetFunction.Transpose(results.Y.ToArray());
            wkSheet.Range[wkSheet.Cells[cstRow_FirstData, 4], wkSheet.Cells[cstRow_FirstData + RowsCount - 1, 4]].Value
                = app.WorksheetFunction.Transpose(results.Z.ToArray());
            //写入X数据
            var startColumn = 5;
            for (int BlockNum = 1; BlockNum <= BlockCount - 1; BlockNum++)
            {
                startColumn++;
                results = AllData[BlockNum];
                // 将
                DataResultArray SortedRes = SortResult(results, AllData[0].NodeId);
                RowsCount = (uint)SortedRes.Count;
                wkSheet.Range[
                    wkSheet.Cells[cstRow_FirstData, startColumn],
                    wkSheet.Cells[cstRow_FirstData + RowsCount - 1, startColumn]].Value = SortedRes.X;
                // app.WorksheetFunction.Transpose(results.X.ToArray)
                wkSheet.Range[
                    wkSheet.Cells[cstRow_FirstData, startColumn + BlockCount],
                    wkSheet.Cells[cstRow_FirstData + RowsCount - 1, startColumn + BlockCount]].Value = SortedRes.Y;
                //  app.WorksheetFunction.Transpose(results.Y.ToArray)
                wkSheet.Range[
                    wkSheet.Cells[cstRow_FirstData, startColumn + BlockCount * 2],
                    wkSheet.Cells[cstRow_FirstData + RowsCount - 1, startColumn + BlockCount * 2]].Value = SortedRes.Z;
                //  app.WorksheetFunction.Transpose(results.Z.ToArray)
            }
            try
            {
                wkSheet.Activate();
                wkSheet.Range[wkSheet.Cells[1, 1], wkSheet.Cells[1, startColumn + BlockCount * 2]].Select();
                if (wkSheet.AutoFilterMode)
                {
                    app.Selection.AutoFilter();
                    app.Selection.AutoFilter();
                }
                else
                {
                    app.Selection.AutoFilter();
                }
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(
                    "Warnning : Can not set the autofilter for the worksheet: {0}, but it will not affect the extraction of data.",
                    wkSheet.Name);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        /// <summary>
        /// 对源数据进行排版，来让其中的Id集体与指定的目标Id集合相对应。
        /// </summary>
        /// <param name="SourceResult">要进行排版的数据源</param>
        /// <param name="DestinationIdList">要匹配到的目标Id集合</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private static DataResultArray SortResult(DataResultList SourceResult, List<long> DestinationIdList)
        {
            int NodeCount = DestinationIdList.Count;
            object[,] NodeId = new object[NodeCount - 1 + 1, 1];
            object[,] X = new object[NodeCount - 1 + 1, 1];
            object[,] Y = new object[NodeCount - 1 + 1, 1];
            object[,] Z = new object[NodeCount - 1 + 1, 1];

            //---------------------- 开始进行数据排版 ------------------------------
            int SourceIndex = 0;
            int DestiIndex = 0;
            //
            DataResultList with_1 = SourceResult;
            List<long> SourceNode = with_1.NodeId;
            for (SourceIndex = 0; SourceIndex <= SourceNode.Count - 1; SourceIndex++)
            {
                DestiIndex = DestinationIdList.IndexOf(SourceNode[SourceIndex], DestiIndex);
                if (DestiIndex < 0)
                {
                    // 搜索原理：
                    // 这里有一个前提假设：在SourceNode与DestinationIdList中，节点的Id都是从小到大排列的。
                    // 所以，一旦SourceNode中的节点在DestinationIdList集体中找不到对应的匹配节点号，则SourceNode中后面的节点都不会找到匹配的节点号了。
                    break;
                }
                else
                {
                    // 将数据放置在对应的位置上
                    NodeId[DestiIndex, 0] = with_1.NodeId[SourceIndex];
                    X[DestiIndex, 0] = with_1.X[SourceIndex];
                    Y[DestiIndex, 0] = with_1.Y[SourceIndex];
                    Z[DestiIndex, 0] = with_1.Z[SourceIndex];
                }
            }

            //---------------------------------------------------------------------
            DataResultArray Res = new DataResultArray();
            Res.NodeId = NodeId;
            Res.X = X;
            Res.Y = Y;
            Res.Z = Z;
            return Res;
        }

        private static void wkbk_BeforeClose(ref bool Cancel)
        {
            wkbk = null;
        }

        /// <summary>
        /// 一个数据块中所有的数据，以及最后退出数据块的那一行字符串
        /// </summary>
        /// <remarks>此类中的NodeId、X、Y、Z这四个集合中的元素个数肯定是相同的。</remarks>
        private struct DataResultList
        {
            /// <summary>
            /// 在一个数据块中，所有节点的Id号（从Flac3d中输出后，这些节点的Id号都是按从小到大的顺序排列的）。
            /// </summary>
            /// <remarks></remarks>
            public List<long> NodeId;

            public List<double> X;
            public List<double> Y;
            public List<double> Z;
            public string strEscape;
        }

        /// <summary>
        /// 一个数据块中所有的数据，以及最后退出数据块的那一行字符串.
        /// 此类中的NodeId、X、Y、Z这四个二维数组必须都是列向量，不能是行向量。
        /// </summary>
        /// <remarks>此类中的NodeId、X、Y、Z这四个集合中的元素个数肯定是相同的。</remarks>
        private struct DataResultArray
        {
            /// <summary>
            /// 所有元素的个数
            /// </summary>
            /// <remarks></remarks>
            public int Count
            {
                get { return NodeId.Length; }
            }

            /// <summary>
            /// 此列向量中，可以有空数据，所以其数据类型为Object，而不是Long。
            /// </summary>
            /// <remarks></remarks>
            public object[,] NodeId;

            public object[,] X;
            public object[,] Y;
            public object[,] Z;
        }
    }
}