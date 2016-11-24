using System;
using System.Runtime.InteropServices;

namespace Hm2Flac3D
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
        /// 寻找三个点所形成的空间三角形的形心
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <param name="node3"></param>
        /// <returns></returns>
        public static XYZ FindCentroid(XYZ node1, XYZ node2, XYZ node3)
        {
            return new XYZ(Convert.ToDouble((node1.X + node2.X + node3.X) / 3), Convert.ToDouble(
                (node1.Y + node2.Y + node3.Y) / 3), Convert.ToDouble(
                    (node1.Z + node2.Z + node3.Z) / 3));
        }

        /// <summary>
        /// 寻找空间四个点所形成的共面的空间四边形角形的形心，如果四个点不共面，则会报错
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <param name="node3"></param>
        /// <param name="node4"></param>
        /// <returns> 四边形的形心点的坐标 </returns>
        /// <remarks>在inp文件中，输入的四个节点的顺序一定是可以形成一个边界环路的，即使此S4单元的网格形状为有凹角的异型错误网格。</remarks>
        public static XYZ FindCentroid(XYZ node1, XYZ node2, XYZ node3, XYZ node4)
        {
            // 以两个对角点中距离较短的那个作为两个三角形的分割边
            XYZ[] nodes = null;
            if (node1.DistanceTo(node3) < node2.DistanceTo(node4))
            {
                nodes = new[] { node2, node1, node3, node4 };
            }
            else
            {
                nodes = new[] { node1, node2, node4, node3 };
            }

            // 先计算第一个三角形的形心位置与面积
            XYZ c1 = FindCentroid(nodes[0], nodes[1], nodes[2]);
            double area1 = Area(nodes[0], nodes[1], nodes[2]);

            // 再计算第二个三角形的形心位置与面积
            XYZ c2 = FindCentroid(nodes[1], nodes[2], nodes[3]);
            double area2 = Area(nodes[1], nodes[2], nodes[3]);

            double centDis = c1.DistanceTo(c2);

            // 利用杠杆原理计算两个三角形的组合四边形形心位置：area1 * x=area2 * (centDis-x)
            double x = centDis / (area1 / area2 + 1); // 四边形的形心点处在两个三角形的形心连线上，x 为四边形的形心点到第1个三角形形心的距离。

            return c1.Move(c1.VectorTo(c2).SetLength(x));
        }

        /// <summary> 计算空间三角形的面积  </summary>
        /// <returns></returns>
        private static double Area(XYZ node1, XYZ node2, XYZ node3)
        {
            // 三条边长
            double a = XYZ.Distance(node1, node2);
            double b = XYZ.Distance(node2, node3);
            double c = XYZ.Distance(node3, node1);

            double p = (a + b + c) / 2;
            return p * (p - a) * (p - b) * (p - c); // 海伦公式
        }

        /// <summary>
        /// 用来框选住 Liner 的形心的一个立方体区域，其单位为m。也就是说，Hypermesh中要尽量保证模型的网格尺寸不要小于这个值。
        /// </summary>
        /// <remarks></remarks>
        public const double CubeRangePrecision = 0.0005;

        /// <summary>
        /// 将 形心点 扩展到一个立方体区域
        /// </summary>
        /// <param name="centroid"></param>
        /// <returns>返回的格式为： "Range x= (23.73, 23.78) y =( -0.01, 0.01)  z= ( 19.65, 19.67)"</returns>
        /// <remarks></remarks>
        public static string ExtendCentroid(XYZ centroid)
        {
            // 左右各扩展0.5mm，以形成一个1立方米的区域
            return string.Format("Range x=({0},{1}) y=({2},{3})  z=({4},{5})",
                (centroid.X - CubeRangePrecision), (centroid.X + CubeRangePrecision),
                (centroid.Y - CubeRangePrecision), (centroid.Y + CubeRangePrecision),
                (centroid.Z - CubeRangePrecision), (centroid.Z + CubeRangePrecision));
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