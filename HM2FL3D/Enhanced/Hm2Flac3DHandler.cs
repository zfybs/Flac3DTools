using System;
using System.Runtime.InteropServices;
using Hm2Flac3D.Utility;

namespace Hm2Flac3D.Enhanced
{
    public class Hm2Flac3DHandler
    {
        /// <summary>
        /// 根据inp文件中对于单元类型以及Component命名的描述，确定此组是属于什么单元
        /// </summary>
        /// <param name="TypeName"></param>
        /// <param name="componentName"> inp 文件中表示的Hypermesh的 Component 的名称 </param>
        /// <returns>返回与inp文件中的单元类型所对应的Flac3d中的单元类型</returns>
        /// <remarks></remarks>
        public static ElementType GetElementType(string TypeName, string componentName)
        {
            if ((TypeName.IndexOf("B31", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 TypeName.IndexOf("B21", StringComparison.OrdinalIgnoreCase) >= 0)
                && componentName.IndexOf("beam", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return ElementType.BEAM;
            }
            if ((TypeName.IndexOf("B31", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 TypeName.IndexOf("B21", StringComparison.OrdinalIgnoreCase) >= 0)
                && componentName.IndexOf("pile", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return ElementType.PILE;
            }
            if ((TypeName.IndexOf("S3", StringComparison.OrdinalIgnoreCase) >= 0) &&
                componentName.IndexOf("Shell", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return ElementType.SHELL3;
            }

            if ((TypeName.IndexOf("S3", StringComparison.OrdinalIgnoreCase) >= 0) &&
                componentName.IndexOf("Liner", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return ElementType.Liner3;
            }

            if ((TypeName.IndexOf("S4", StringComparison.OrdinalIgnoreCase) >= 0) &&
                componentName.IndexOf("Liner", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return ElementType.Liner4;
            }

            if (TypeName.IndexOf("C3D8", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return ElementType.ZONE_B8;
            }
            if (TypeName.IndexOf("C3D6", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return ElementType.ZONE_W6;
            }
            if (TypeName.IndexOf("C3D4", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return ElementType.ZONE_T4;
            }
            return ElementType.Others;
        }

        /// <summary>
        /// 在Console中进行手动判断，如果用户输入Yes或者Y，则返回布尔值True，否则返回False
        /// </summary>
        /// <param name="question">要询问用户的问题</param>
        /// <returns></returns>
        /// <remarks>如果用户输入Yes或者Y，则返回布尔值True，否则返回False</remarks>
        public static bool GetReply(string question)
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


        /// <summary>
        /// 用来框选住 Liner 的形心的一个立方体区域，其单位为m。也就是说，Hypermesh中要尽量保证模型的网格尺寸不要小于这个值。
        /// </summary>
        /// <remarks>如果立方体的边长为1mm，则此常数值就设置为0.0005。在测试中，Flac所能接受的立方体容差约为0.00006m</remarks>
        public static double CubeRangePrecision = 0.0005;

        /// <summary>
        /// 将 形心点 扩展到一个立方体区域
        /// </summary>
        /// <param name="centroid"></param>
        /// <returns>返回的格式为： "Range x= (23.73, 23.78) y =( -0.01, 0.01)  z= ( 19.65, 19.67)"</returns>
        /// <remarks></remarks>
        public static string ExtendCentroid(XYZ centroid)
        {
            // 左右各扩展0.5mm，以形成一个1立方米的区域。
            // 控制数值字符精度，反正其精度也不会细于0.1mm。
            return string.Format("Range x=({0},{1}) y=({2},{3}) z=({4},{5})",
                (centroid.X - CubeRangePrecision).ToString("0.#####"), (centroid.X + CubeRangePrecision).ToString("0.#####"),
                (centroid.Y - CubeRangePrecision).ToString("0.#####"), (centroid.Y + CubeRangePrecision).ToString("0.#####"),
                (centroid.Z - CubeRangePrecision).ToString("0.#####"), (centroid.Z + CubeRangePrecision).ToString("0.#####"));
        }

        /// <summary> 定义每一种类型的单元应该被放置在哪种类型的命令文本中 </summary>
        /// <param name="elementType"></param>
        /// <returns></returns>
        public static Flac3DCommandType GetCommandType(ElementType elementType)
        {
            Flac3DCommandType fct = Flac3DCommandType.Others;
            switch (elementType)
            {
                case ElementType.SelNode:
                    return Flac3DCommandType.SelNode;

                case ElementType.GridPoint:
                case ElementType.ZONE_B8:
                case ElementType.ZONE_T4:
                case ElementType.ZONE_W6:
                    return Flac3DCommandType.Zones;

                case ElementType.BEAM:
                    return Flac3DCommandType.Beam;

                case ElementType.PILE:
                    return Flac3DCommandType.Pile;

                case ElementType.SHELL3:
                    return Flac3DCommandType.Shell;

                case ElementType.Liner3:
                case ElementType.Liner4:
                    return Flac3DCommandType.Liner;

                case ElementType.MERGEPOINT:
                    return Flac3DCommandType.MergePoint;

            }

            return fct;
        }

        /// <summary>  释放控制台   </summary>
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool FreeConsole();

        /// <summary>
        /// 加载控制台。加载后可以直接通过 Console.Write() 等静态方法对控制台进行读写操作
        /// </summary>
        /// <returns></returns>
        /// <remarks> 加载后，可以通过API函数 FreeConsole 释放控制台 </remarks>
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool AllocConsole();
    }
}