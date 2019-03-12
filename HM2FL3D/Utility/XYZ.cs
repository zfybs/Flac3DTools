using System;
using System.Diagnostics;

namespace Hm2Flac3D.Utility
{
    /// <summary>
    /// 一个空间的坐标点或者空间的矢量
    /// </summary>
    public class XYZ
    {
        public double X;
        public double Y;
        public double Z;

        public XYZ(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return "( " + X.ToString() + "," + "\t" + Y.ToString() + "," + "\t" + Z.ToString() + " )";
        }

        #region ---   空间点的方法

        /// <summary> 计算空间两个点的距离 </summary>
        /// <returns></returns>
        public static double Distance(XYZ node1, XYZ node2)
        {
            return node1.DistanceTo(node2);
        }

        /// <summary> 计算空间两个点的距离 </summary>
        /// <returns></returns>
        public double DistanceTo(XYZ node2)
        {
            var a = Math.Sqrt(Math.Pow((this.X - node2.X), 2) + Math.Pow((this.Y - node2.Y), 2) + Math.Pow((this.Z - node2.Z), 2));
            var b = a;
            return a;
        }

        /// <summary> 一个空间点沿空间的位移矢量移动后的新位置 </summary>
        public XYZ Move(XYZ vector)
        {
            return new XYZ(X + vector.X, Y + vector.Y, Z + vector.Z);
        }

        /// <summary> 从本坐标点指向输入的 node2 的位移矢量 </summary>
        /// <param name="node2"> 矢量的终点 </param>
        /// <returns> 一个空间矢量，起始点为 node1，终点为 node2 </returns>
        public XYZ VectorTo(XYZ node2)
        {
            return new XYZ(node2.X - X, node2.Y - Y, node2.Z - Z);
        }

        #endregion

        #region ---   空间矢量的方法

        /// <summary>
        /// 将一个空间矢量缩放到指定的长度（方向不变）
        /// </summary>
        /// <param name="newLength">缩放后的长度</param>
        /// <returns> 缩放后的新矢量 </returns>
        public XYZ SetLength(double newLength)
        {
            double ratio = newLength / Length();
            return new XYZ(X * ratio, Y * ratio, Z * ratio);
        }

        /// <summary> 空间矢量的长度 </summary>
        public double Length()
        {
            return Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2));
        }

        /// <summary> 对矢量进行缩放 </summary>
        /// <param name="ratio">缩放比例</param>
        public XYZ Scale(double ratio)
        {
            return new XYZ(X * ratio, Y * ratio, Z * ratio);
        }

        #endregion

        /// <summary>
        /// 寻找三个点所形成的空间三角形的形心
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <param name="node3"></param>
        /// <returns></returns>
        public static XYZ FindCentroid(XYZ node1, XYZ node2, XYZ node3)
        {
            return new XYZ(
                (node1.X + node2.X + node3.X) / 3,
                (node1.Y + node2.Y + node3.Y) / 3,
                (node1.Z + node2.Z + node3.Z) / 3);
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

            // 返回形心点的坐标
            return c1.Move(c1.VectorTo(c2).SetLength(x));
            //return c1.Move(c1.VectorTo(c2).Scale(x / centDis));
        }

        /// <summary> 计算空间三角形的面积  </summary>
        /// <returns></returns>
        private static double Area(XYZ node1, XYZ node2, XYZ node3)
        {
            // 三条边长
            double a = XYZ.Distance(node1, node2);
            double b = XYZ.Distance(node2, node3);
            double c = XYZ.Distance(node3, node1);

            // double p = (a + b + c) / 2;
            //return Math.Sqrt(p * (p - a) * (p - b) * (p - c)); // 海伦公式

            return Math.Sqrt((a + b + c) * (b + c - a) * (a + c - b) * (a + b - c) / 16);
        }
    }
}