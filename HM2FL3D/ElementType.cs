namespace Hm2Flac3D
{
    /// <summary>
    /// 在Flac3D中的单元类型，每一种类型代表了一种单元的编码形式
    /// </summary>
    /// <remarks></remarks>
    public enum ElementType
    {
        /// <summary>
        /// 梁单元，其在Flac3d中的导入格式为：SEL BEAMSEL  cid   107629 id   107629 nodes  1768565 1757075
        /// </summary>
        /// <remarks>在结构单元导入文件Structures.dat的结尾要加上一句：SEL NODE INIT XPOS ADD 0.0 ，
        /// 以将结构单元之间进行耦合。</remarks>
        BEAM,

        /// <summary>
        /// 桩单元，其在Flac3d中的导入格式为：SEL PILESEL  cid   110190 id   110190 nodes  1770162 1769878
        /// </summary>
        /// <remarks>在结构单元导入文件Structures.dat的结尾要加上一句：SEL NODE INIT XPOS ADD 0.0 ，
        /// 以将结构单元之间进行耦合。</remarks>
        PILE,

        /// <summary>
        /// 三角形壳单元，其在Flac3d中的导入格式为：SEL SHELLSEL cid    99440 id    99440 ele DKT_CST  nodes  1759355 1758957 1758956
        /// </summary>
        /// <remarks>在结构单元导入文件Structures.dat的结尾要加上一句：SEL NODE INIT XPOS ADD 0.0 ，
        /// 以将结构单元之间进行耦合。</remarks>
        SHELL3,

        /// <summary>
        /// 通过三角形平面来创建三角形Liner单元，其在Flac3d中的导入格式为：sel liner id 1 em group ex1 Range x= (23.73, 23.78) y =( -0.01, 0.01)  z= ( 19.65, 19.67) 
        /// 或者是 SEL LINERSEL cid    68341 id    68341 ele DKT_CST  nodes  1564835 1579841 1564832，但是这种方法创建的Liner只会在Side1上产生与Zone的Link，所以是不可取的。
        /// </summary>
        /// <remarks>创建方法为先找到此三角形的形心，然后将形心点向扩展1mm， 以形成一个体积为1立方毫米的立方体，然后用此立方体来作为创建 Liner 的 range。 </remarks>
        Liner3,

        /// <summary>
        /// 通过共面四边形来创建分割此四边形的两个三角形Liner单元，其在Flac3d中的导入格式为：sel liner id 1 em group ex1 Range x= (23.73, 23.78) y =( -0.01, 0.01)  z= ( 19.65, 19.67)
        /// </summary>
        /// <remarks> 创建方法为先找到此四边形的形心，然后将形心点向扩展1mm， 以形成一个体积为1立方毫米的立方体，然后用此立方体来作为创建 Liner 的 range。 </remarks>
        Liner4,

        /// <summary>
        /// 六面体八节点单元，其在Flac3d中的导入格式为：Z B8   90098  95441  95242  93742  95005  93417  95617  93041  95485
        /// </summary>
        /// <remarks></remarks>
        ZONE_B8,

        /// <summary>
        /// 三棱柱（五面体、六节点）单元，其在Flac3d中的导入格式为：Z W6   90083  93007  93014  93016  94941  94674  92956
        /// </summary>
        /// <remarks></remarks>
        ZONE_W6,

        /// <summary>
        /// 四面体（金字塔形、四节点）单元，其在Flac3d中的导入格式为：Z T4   90111  92891  92874  92868  92954
        /// </summary>
        /// <remarks></remarks>
        ZONE_T4,

        /// <summary>
        /// 其他未识别的单元类型
        /// </summary>
        /// <remarks></remarks>
        Others
    }
}