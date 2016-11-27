using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hm2Flac3D.Utility;

namespace Hm2Flac3D.Enhanced
{
    /// <summary> 用将管理不同类型的Zone单元或者结构单元，将其写入到不同的文本文件中。 </summary>
    internal class Flac3dCommandWriters
    {
        /// <summary>
        /// 已经打开的文本
        /// </summary>
        private Dictionary<string, StreamWriter> _openedWriters;

        #region ---   构造函数

        private static Flac3dCommandWriters _uniqueInstance;

        public static Flac3dCommandWriters GetUniqueInstance()
        {
            if (_uniqueInstance == null)
            {
                _uniqueInstance = new Flac3dCommandWriters();
            }
            return _uniqueInstance;
        }

        private Flac3dCommandWriters()
        {
            _openedWriters = new Dictionary<string, StreamWriter>();
        }

        #endregion

        #region ---   公共函数

        /// <summary>
        /// 根据要导出的单元类型，返回一个 StreamWriter，以存储用于在Flac3D中创建此类结构的命令语句
        /// </summary>
        /// <param name="elementType">单元类型，可以是结构单元、土体单元等任意类型的单元</param>
        /// <param name="writerName">写入的文本文件的名称，不包括后缀名 </param>
        /// <param name="fileSuffix"> 用户强行指定的文本后缀，如果不指定，则由<paramref name="elementType"/>来确定文件后缀。 </param>
        /// <returns></returns>
        public StreamWriter GetWriter(ElementType elementType, string writerName, string fileSuffix = null)
        {
            // 确定文件名
            string fileName = writerName + (fileSuffix ?? GetFileSuffix(elementType));

            if (_openedWriters.ContainsKey(fileName))
            {
                return _openedWriters[fileName];
            }
            else // 说明此文本还未创建
            {
                string filePath = Path.Combine(GetCommandDirectory(), fileName);
                //
                FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                //
                _openedWriters.Add(fileName, sw);
                return sw;
            }
        }

        /// <summary> 将所有的 StreamWriter 文本文件关闭 </summary>
        /// <param name="writeMain">是否要新增一个命令文本，用来统领整个建模过程</param>
        public void CloseAllWriters(bool writeMain)
        {
            if (_openedWriters != null)
            {
                foreach (StreamWriter sw in _openedWriters.Values)
                {
                    if (sw != null)
                    {
                        sw.Close();
                        sw.Dispose();
                    }
                }
                //
                if (writeMain)
                {
                    WriteMain();
                }
                //
                _openedWriters.Clear();
                _openedWriters = null;
                _uniqueInstance = null;
            }
        }

        #endregion

        #region ---   文件或文件夹的路径

        public static DateTime? WorkingStartTime;

        /// <summary>
        /// 所有的命令文件所在的文件夹的绝对路径
        /// </summary>
        public static string GetCommandDirectory()
        {
            if (WorkingStartTime == null)
            {
                WorkingStartTime = DateTime.Now;
            }
            // 设置命令文本所在的文件夹的绝对路径
            string commandDirePath = Path.Combine(GetWorkDirectory(),
                "Hm2Fl-" + WorkingStartTime.Value.ToString("yyyyMMdd-HHmmss"));

            if (!Directory.Exists(commandDirePath))
            {
                Directory.CreateDirectory(commandDirePath);
            }
            return commandDirePath;
        }

        /// <summary> Hm2Flad3d 的工作文件夹 </summary>
        public static string GetWorkDirectory()
        {
            Hm2Fl3dSetting hs = new Hm2Fl3dSetting();
            string p = hs.WorkDirectory;
            if (Directory.Exists(p))
            {
                return p;
            }
            else
            {
                return Directory.GetCurrentDirectory();
            }
        }

        public const string FileSuffixZone = ".flac3d";
        public const string FileSuffixSel = ".dat";

        /// <summary> 用来用来统领整个建模过程的命令文本的文件名 </summary>
        public const string FileMain = "0_Main";

        /// <summary> 用来创建Zone单元的命令文本的文件名 </summary>
        public const string FileZone = "1_Zones";

        /// <summary> 用来创建结构单元节点的命令文本的文件名 </summary>
        public const string FileSelNode = "2_SelNode";

        #endregion

        /// <summary> 根据不同的单元类型返回对应文本的后缀名 </summary>
        /// <param name="elementType"></param>
        /// <returns></returns>
        private string GetFileSuffix(ElementType elementType)
        {
            ElementType structureType = ElementType.SelNode | ElementType.BEAM | ElementType.PILE | ElementType.SHELL3 |
                                        ElementType.Liner3 | ElementType.Liner4;
            ElementType zoneType = ElementType.GridPoint | ElementType.ZONE_T4 | ElementType.ZONE_B8 |
                                   ElementType.ZONE_W6;

            if ((elementType & structureType) > 0)
            {
                // 结构单元文本文件的后缀
                return FileSuffixSel;
            }
            else if ((elementType & zoneType) > 0)
            {
                // 土体单元文本文件的后缀 
                return FileSuffixZone;
            }
            else
            {
                // 默认后缀 
                return FileSuffixSel;
            }
        }

        /// <summary>
        /// 新增一个命令文本，用来统领整个建模过程
        /// </summary>
        private void WriteMain()
        {
            var names = _openedWriters.Keys.ToList();
            if (names.Count > 0)
            {
                string filePath = Path.Combine(GetCommandDirectory(), FileMain + FileSuffixSel);
                FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                //
                sw.WriteLine(
                    @"new
set pr 30   ; 使用的cpu核数
set fish safe_conversion   off
set nmd on  ; When nmd is on, any tetrahedral zones will use the nmd algorithm during the stress calculations.
set notice on ; controls whether informational messages generated by the program during command processing will be sent to the screen and the log file
set warning off ; controls whether warning messages generated by the program during command processing will be sent to the screen and the log file 
set echo off;
");

                // 因为土体单元的文件后缀不同，所以进行特殊处理
                const string zoneCommandFile = FileZone + FileSuffixZone;
                if (names.Contains(zoneCommandFile, StringComparer.OrdinalIgnoreCase))
                {
                    sw.WriteLine($"ImpGrid {zoneCommandFile}");
                    names.Remove(zoneCommandFile);
                }
                // 其他结构单元
                foreach (var name in names)
                {
                    sw.WriteLine($"Call {name}");
                }
                //
                sw.Close();
                sw.Dispose();
            }
        }
    }
}