# HM2FL3D   ![image](https://github.com/zfybs/HM2FL3D/blob/master/HM2FL3D/Pictures/Hm2Flac3D_64.png)
将Hypermesh风格输出的inp文件转换为Flac3D的风格。 CONVERT INP CODE(IMPORTED FROM HYPERMESH) TO FLAC3D

 __注意：一个Component中可以有多种要输出的单元类型，在Hypermesh转换为Inp格式时，Hypermesh会自动将同一个Component中的不同单元类型进行分组。__

![image](https://github.com/zfybs/HM2FL3D/blob/master/HM2FL3D/Pictures/Hypermesh to Flac3d 转换规则.png)
 
###使用说明
1. 在Hypermesh中绘制好土体与结构的网格，不用绘制地下连续墙；
2. 先将三维土体单元导出为Abaqus的inp文件；再将一维与二维的结构单元导出为Abaqus的inp文件；
3. 分别将上面两个inp文件用本程序将网格转换为Zones.Flac3D的土体网格与structures.dat的结构单元网格。注意，在进行网格转换时，要先将此对应的inp文件重命名为“Hm2Flac3d.inp”，并将其与本程序放置在同一个文件夹内。；
2. 在Flac3d中，先用“import zones.Flac3D”导入土体网格，再用“Call structures.dat”导入结构单元，注意导入的先后顺序；
5. 开始在Flac3D中进行设置与计算。Enjoy it!
 
 
 
###注意事项
 
1. 将zones单元与结构单元分开导出
在Hypermesh中进行网格导出时，建议将Zone单元与结构单元分开来导出和转换。如果将土体单元与结构单元同时导出并转换时，本程序并不会报错，导出的网格也不会有问题，__但是，zones.flac3d文件中的grid数目可能会多于所有的zone单元所用到的grid，同样地，structures.dat文件中的node的数目可能会多于所有的结构单元所用到的node。另外，如果将土体单元与结构单元同时导出，在将模型导入到Flac中时也看不出很明显的问题，但是在此模型中创建Liner单元时，可能会很慢，最终计算结果也会有问题__。
2. C3D8类型的单元
对于六面体八节点单元，其在inp文件中的最后一个节点是写在第二行的。如果从hypermesh中导出的inp文件中。C3D8类型的最后一个节点不是写在第二行，则此程序不会导出这些单元。

![image](https://github.com/zfybs/HM2FL3D/blob/master/HM2FL3D/Pictures/inp文件中C3D8类型节点格式.png)

3. 对于三维网格单元，其在Hypermesh中的法向（Normal）并不要求全部一致。 
4. 对于 Hm2Flac3D 或者 Ansys 导出的网格，虽然在Hypermesh中已经用Equivilance进行了节点合并，但是在Flac3D中进行Solve时，还是有极小的可能会报出“Zero stiffness in grid-point 18545” 这种错误。此时可能并不是土体单元未被赋上材料属性，而是需要对初始网格在Flac3D中再进行一次合并（用 gen merge 0.1），然后就可以正常计算了。
