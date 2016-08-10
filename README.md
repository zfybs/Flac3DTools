# HM2FL3D   ![image](https://github.com/zfybs/HM2FL3D/blob/master/HM2FL3D/Pictures/Hm2Flac3D_64.png)
将Hypermesh风格输出的inp文件转换为Flac3D的风格。 CONVERT INP CODE(EXPORTED FROM HYPERMESH) TO FLAC3D

****************

#一、 Hypermesh to Flac3d 转换规则
 __注意：一个Component中可以有多种要输出的单元类型，在Hypermesh转换为Inp格式时，Hypermesh会自动将同一个Component中的不同单元类型进行分组。__

![image](https://github.com/zfybs/HM2FL3D/blob/master/HM2FL3D/Pictures/HM2Flac3D转换规则.png) 


#二、 Liner 单元的转换说明
##2.1 配合 Set Browser 使用
在Flac3D中，Liner单元如果要在两个侧面都与Zone之间创建柔性接触，只能通过
Sel Liner id 1 em Group ex1 Range x= (23.73, 23.78) y =( -0.01, 0.01)  z= ( 19.65, 19.67)
所以，如果要创建此类型的Liner，就必须要先有一个用于附着的Group对象，然后Flac3D会通过搜索此Group中所有边界zone的边界Face，只要此Face的形心位于上面语句中的Range所指定范围中，Flac3D就会根据此Face创建出一个或者多个三角形的 Liner 单元。
此Group对象是通过Hypermesh中的Set来实现的。即将要用来进行附着的Zone组合为多个Element Set，然后通过 Liner 的 Componet 的名称来指定要附着到哪一个 Element Set中。在将Element Set从inp文件中转换到Flac3D文件中去时，会将其分配一个单独的Group中，而且此Group所在的Slot编号会大于1，因为Hypermesh中的Component中的Zone单元也会被放置到Group中，而在默认情况下，Group如果不显式指定Slot的话，会自动分配到Slot 1中。

##2.2 Element Set 与对应的 Liner Component的命名规范
Element Set 与对应的 Liner Component的命名规范如下：
-	Element Set必须以“GLiner”开头，而且名称中不能包含“-”。比如“GLiner”、“GLiner_Zone”都是可以的；
-	Liner Component的名称必须以“Liner-附着组名”开头，更多的名称信息可以在组名后用“-”进行分隔。比如当其要附着到组GLiner中时，“Liner-GLinerLeft”、“Liner-GLiner-Left”都是可以的，但是“Liner-GLinerLeft”会将此Liner单元附着到组“GLinerLeft”中，但是如果Flac3D中并没有创建一个组“GLinerLeft”的话，自然是会出现异常的。
   ![image](https://github.com/zfybs/HM2FL3D/blob/master/HM2FL3D/Pictures/LinerSet.png) ![image](https://github.com/zfybs/HM2FL3D/blob/master/HM2FL3D/Pictures/LinerComponent.png) 
图1 Hypermesh中与Liner 的创建相关的Set 与 Component 的命名示例


##2.3 inp 导出 S4 的规则
Hypermesh中S3或者S4单元的输出到inp文件中的节点顺序（通过 Hypermesh 中的 2D面板 > Edit element > Create来进行创建单元的测试）：
1. 绕边界环线进行编号；
2. 如果创建S4单元时是按边界环线点击节点，则inp中的节点顺序与创建时点击的节点顺序一致；
3. 如果创建S4单元时并不是按边界环线点击节点，在Hypermesh中自动将第3、4个节点顺序进行调整，以将其重排为边界环路的顺序，而不改变前面的两个节点顺序。
4. 对于异型的S4单元（比如箭头形这种有凹角的），不论节点点击顺序如何，在Hypermesh中都会确保其在inp中的顺序形成一个边界环路,但是要注意，这种四边形网格是有错误的，在计算时肯定会出现异常。

#三、 使用说明
##3.1 使用流程

1.	在Hypermesh中绘制好土体与结构（桩、支撑、地下连续墙）的网格。注意在有地下连续墙的位置，一定要将墙体左右的Zone单元用Hypermesh中Detach分隔开来；
2.	先将三维土体单元导出为Abaqus的inp文件；再将一维与二维的结构单元导出为Abaqus的inp文件。注意导出时只将要导出的单元显示出来，然后选择“Displayed模式” ![image](https://github.com/zfybs/HM2FL3D/blob/master/HM2FL3D/Pictures/Export.png)  ；
3.	分别将上面两个inp文件用本Hm2Flac3D程序将网格转换为Zones.Flac3D的土体网格与structures.dat的结构单元网格，生成的文本文件会保存在与对应的inp文件相同的文件夹内。
4.	在Flac3d中，先用“import zones.Flac3D”导入土体网格，再用“Call structures.dat”导入结构单元，注意导入的先后顺序；
5.	开始在Flac3D中进行设置与计算。Enjoy it!

##3.2 注意事项
###3.2.1 将zones单元与结构单元分开导出
在Hypermesh中进行网格导出时，建议将Zone单元与结构单元分开来导出和转换。如果将土体单元与结构单元同时导出并转换时，本程序并不会报错，导出的网格也不会有问题，__但是，zones.flac3d文件中的grid数目可能会多于所有的zone单元所用到的grid，同样地，structures.dat文件中的node的数目可能会多于所有的结构单元所用到的node。另外，如果将土体单元与结构单元同时导出，在将模型导入到Flac中时也看不出很明显的问题，但是在此模型中创建Liner单元时，可能会很慢，最终计算结果也会有问题__。
###3.2.2 在Hypermesh中调整节点的编号（推理未测试）
在通过如下语句生成Liner时，Flac3D会自动为生成出来的Liner单元的每一个Node分配节点编号。为了避免Node的编号的冲突，在分配节点时会对当前模型中已经存在的nodes集合的编号进行搜索（猜测会在内存中保存一个SortedSet<UInt64> 的集合）。
Sel Liner id 1 em Group ex1 Range x= (23.73, 23.78) y =( -0.01, 0.01)  z= ( 19.65, 19.67)
比如在生成Liner之前，Flac3D模型中的节点（不论结构单元还是实体单元）已经占据了编号1~5000，则在“Sel Liner …”时，就必须要从1开始搜索可用的节点编号，这样的遍历与Contains()的判断会降低一定的计算效率，而且已有的节点越多，这种搜索就越费时。
最后的结论就是：建议用户自行在Hypermesh中去调整网格节点的编号，使Hypermesh中的 nodes.MinmumId > nodes.Count。

###3.2.3 C3D8类型的单元
对于六面体八节点单元，其在inp文件中的最后一个节点是写在第二行的。如果从hypermesh中导出的inp文件中。C3D8类型的最后一个节点不是写在第二行，则此程序不会导出这些单元。
![image](https://github.com/zfybs/HM2FL3D/blob/master/HM2FL3D/Pictures/inp文件中C3D8类型节点格式.png) 

###3.2.4 单元法向
对于三维网格单元，其在Hypermesh中的法向（Normal）并不要求一致。
###3.2.5 Merge的问题
对于 Hm2Flac3D 或者 Ansys 导出的网格，虽然在Hypermesh中已经用Equivilance进行了节点合并，但是在Flac3D中进行Solve时，还是有极小的可能会报出“Zero stiffness in grid-point 18545” 这种错误。此时可能并不是土体单元未被赋上材料属性，而是需要对初始网格在Flac3D中再进行一次合并（用 gen merge 0.1），然后就可以正常计算了。