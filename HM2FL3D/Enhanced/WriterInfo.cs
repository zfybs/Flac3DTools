using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hm2Flac3D.Utility;

namespace Enhanced
{
    /// <summary>
    /// 每一个Flac3D命令文件的信息
    /// </summary>
    public class WriterInfo
    {
        public Flac3DCommandType CommandType { get; set; }

        /// <summary> 文件名，包含后缀 </summary>
        public string FileName { get; set; }

        /// <summary> 结构单元所属的集合 Id，null表示无 Id </summary>
        public int? Id { get; set; }

        public StreamWriter Writer { get; set; }

        public WriterInfo(Flac3DCommandType commandType, string fileName, StreamWriter writer)
        {
            CommandType = commandType;
            FileName = fileName;
            Writer = writer;
        }
    }
}
