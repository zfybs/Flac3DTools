using System;

namespace Hm2Flac3D.Utility
{
    /// <summary>
    /// 在Flac3D中的单元类型，每一种类型代表了一种单元的编码形式
    /// </summary>
    /// <remarks></remarks>
    [Flags]
    public enum ElementType
    {
        /// <summary>
        /// 其他未识别的单元类型
        /// </summary>
        /// <remarks></remarks>
        Others = 0,

        /// <summary>
        /// 结构单元的节点，其在Flac3d中的导入格式为 SEL NODE cid           2,  140.0          ,  0.0            ,  -9.35
        /// </summary>
        SelNode = 1,

        /// <summary>
        /// Zone单元的节点，其在Flac3d中的导入格式为：G           1,  147.5          ,  0.0            ,  -9.35
        /// </summary>
        GridPoint = 2,

        /// <summary>
        /// 梁单元，其在Flac3d中的导入格式为：SEL BEAMSEL  cid   107629 id   107629 nodes  1768565 1757075
        /// </summary>
        /// <remarks>在结构单元导入文件Structures.dat的结尾要加上一句：SEL NODE INIT XPOS ADD 0.0 ，
        /// 以将结构单元之间进行耦合。</remarks>
        BEAM = 4,

        /// <summary>
        /// 桩单元，其在Flac3d中的导入格式为：SEL PILESEL  cid   110190 id   110190 nodes  1770162 1769878
        /// </summary>
        /// <remarks>在结构单元导入文件Structures.dat的结尾要加上一句：SEL NODE INIT XPOS ADD 0.0 ，
        /// 以将结构单元之间进行耦合。</remarks>
        PILE = 8,

        /// <summary>
        /// 三角形壳单元，其在Flac3d中的导入格式为：SEL SHELLSEL cid    99440 id    99440 ele DKT_CST  nodes  1759355 1758957 1758956
        /// </summary>
        /// <remarks>在结构单元导入文件Structures.dat的结尾要加上一句：SEL NODE INIT XPOS ADD 0.0 ，
        /// 以将结构单元之间进行耦合。</remarks>
        SHELL3 = 16,

        /// <summary>
        /// 通过三角形平面来创建三角形Liner单元，其在Flac3d中的导入格式为：sel liner id 1 em group ex1 Range x= (23.73, 23.78) y =( -0.01, 0.01)  z= ( 19.65, 19.67) 
        /// 或者是 SEL LINERSEL cid    68341 id    68341 ele DKT_CST  nodes  1564835 1579841 1564832，但是这种方法创建的Liner只会在Side1上产生与Zone的Link，所以是不可取的。
        /// </summary>
        /// <remarks>创建方法为先找到此三角形的形心，然后将形心点向扩展1mm， 以形成一个体积为1立方毫米的立方体，然后用此立方体来作为创建 Liner 的 range。 </remarks>
        Liner3 = 32,

        /// <summary>
        /// 通过共面四边形来创建分割此四边形的两个三角形Liner单元，其在Flac3d中的导入格式为：sel liner id 1 em group ex1 Range x= (23.73, 23.78) y =( -0.01, 0.01)  z= ( 19.65, 19.67)
        /// </summary>
        /// <remarks> 创建方法为先找到此四边形的形心，然后将形心点向扩展1mm， 以形成一个体积为1立方毫米的立方体，然后用此立方体来作为创建 Liner 的 range。 </remarks>
        Liner4 = 64,

        /// <summary>
        /// 六面体八节点单元，其在Flac3d中的导入格式为：Z B8   90098  95441  95242  93742  95005  93417  95617  93041  95485
        /// </summary>
        /// <remarks></remarks>
        ZONE_B8 = 128,

        /// <summary>
        /// 三棱柱（五面体、六节点）单元，其在Flac3d中的导入格式为：Z W6   90083  93007  93014  93016  94941  94674  92956
        /// </summary>
        /// <remarks></remarks>
        ZONE_W6 = 256,

        /// <summary> 四面体（金字塔形、四节点）单元，其在Flac3d中的导入格式为：Z T4   90111  92891  92874  92868  92954 </summary>
        /// <remarks></remarks>
        ZONE_T4 = 512,

        /// <summary> 将指定坐标点的节点进行耦合，其在Flac3d中的导入格式为：Generate Merge 0.001  Range x=(44.9995,45.0005) y=(79.9995,80.0005)  z=(-34.0005,-33.9995) </summary>
        MERGEPOINT = 1024,
    }

    /// <summary> 在Flac3D中的命令文本类型，一个文本中可能对应创建多种<see cref="ElementType"/>的单元 </summary>
    /// <remarks></remarks>
    [Flags]
    public enum Flac3DCommandType
    {
        /// <summary>  </summary>
        Others = 0,

        /// <summary> 用来统领整个建模过程 </summary>
        Main = 1,

        /// <summary> 包含了GridPoint定义、Group定义、以及用于依附Liner的Element Set的定义 </summary>
        Zones = 2,

        /// <summary> 结构单元的节点编号与坐标 </summary>
        SelNode = 4,

        /// <summary> 创建 Flac3D 中的梁单元 </summary>
        Beam = 8,

        /// <summary> 创建 Flac3D 中的桩单元 </summary>
        Pile = 16,

        /// <summary> 创建 Flac3D 中的壳单元 </summary>
        Shell = 32,

        /// <summary> 创建 Flac3D 中的衬砌单元 </summary>
        Liner = 64,

        /// <summary> 将指定坐标点的节点进行耦合 </summary>
        MergePoint = 128,

    }
}