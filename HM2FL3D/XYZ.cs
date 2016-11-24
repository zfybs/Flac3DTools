using System;
namespace Hm2Flac3D
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
            return Math.Sqrt(Math.Pow((X - node2.X), 2) + Math.Pow((Y - node2.Y), 2) + Math.Pow((Z - node2.Z), 2));
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

        #endregion
    }
}