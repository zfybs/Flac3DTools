using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Hm2Flac3D
{
    public partial class form_Hm2Flac3D
    {
        #region Default Instance

        private static form_Hm2Flac3D defaultInstance;

        /// <summary>
        /// Added by the VB.Net to C# Converter to support default instance behavour in C#
        /// </summary>
        public static form_Hm2Flac3D Default
        {
            get
            {
                if (defaultInstance == null)
                {
                    defaultInstance = new form_Hm2Flac3D();
                    defaultInstance.FormClosed += new FormClosedEventHandler(defaultInstance_FormClosed);
                }

                return defaultInstance;
            }
            set { defaultInstance = value; }
        }

        static void defaultInstance_FormClosed(object sender, FormClosedEventArgs e)
        {
            defaultInstance = null;
        }

        #endregion

        private StringBuilder _message;
        private BackgroundWorker worker;

        #region ---   窗口的加载与关闭

        public form_Hm2Flac3D()
        {
            // This call is required by the designer.
            InitializeComponent();

            //Added to support default instance behavour in C#
            if (defaultInstance == null)
                defaultInstance = this;

            // Add any initialization after the InitializeComponent() call.

            worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            //
            ProgressBar1.MarqueeAnimationSpeed = 10;
            ProgressBar1.Visible = false;
            //
        }

        public void form_Hm2Flac3D_Load(object sender, EventArgs e)
        {
            _message = new StringBuilder();
            _message.AppendLine("******** CONVERT INP CODE(EXPORTED FROM HYPERMESH) TO FLAC3D ********");
            LabelHello.Text = _message.ToString();

            // 设置初始的 inp 文件位置
            string exeDire = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
            TextBox_zonesInp.Text = Path.Combine(exeDire, "zones.inp");
            TextBox_structuresInp.Text = Path.Combine(exeDire, "structures.inp");
        }

        public void ButtonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        public void form_Hm2Flac3D_FormClosing(object sender, FormClosingEventArgs e)
        {
            _message = null;
            //
            worker.CancelAsync();
            worker.Dispose();
            worker = null;
        }

        #endregion

        #region ---   界面事件处理

        private void DoseOff()
        {
            ButtonClose.Enabled = false;
            ButtonChooseZones.Enabled = false;
            ButtonChooseStructures.Enabled = false;
            ButtonNoLiner.Enabled = false;
            TextBox_structuresInp.Enabled = false;
            TextBox_zonesInp.Enabled = false;
        }

        private void WarmUp()
        {
            ButtonClose.Enabled = true;
            ButtonChooseZones.Enabled = true;
            ButtonChooseStructures.Enabled = true;
            ButtonNoLiner.Enabled = true;
            TextBox_structuresInp.Enabled = true;
            TextBox_zonesInp.Enabled = true;
        }

        // 文件路径的拖拽
        public void TextBox_zonesInp_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // There is text. Allow copy.
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                // There is no text. Prohibit drop.
                e.Effect = DragDropEffects.None;
            }
        }

        public void TextBox_zonesInp_DragDrop(object sender, DragEventArgs e)
        {
            TextBox txt = (TextBox)sender;
            string[] FileDrop = e.Data.GetData(DataFormats.FileDrop) as string[];
            // DoSomething with the Files or Directories that are droped in.
            string filepath = FileDrop[0];
            if (string.Compare(strA: Path.GetExtension(filepath), strB: ".inp", ignoreCase: true) == 0)
            {
                txt.Text = filepath;
            }
            else
            {
                txt.Text = "请确保文件后缀名为\".inp\"";
            }
        }

        public void ButtonChooseZones_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string path = ChooseInpFile("选择对应的 inp 文件");

            // 写入数据
            if (!string.IsNullOrEmpty(path))
            {
                if (btn.Name == ButtonChooseZones.Name)
                {
                    TextBox_zonesInp.Text = path;
                }
                if (btn.Name == ButtonChooseStructures.Name)
                {
                    TextBox_structuresInp.Text = path;
                }
            }
        }

        /// <summary> 通过选择文件对话框选择要进行数据提取的Excel文件 </summary>
        /// <returns> 要进行数据提取的Excel文件的绝对路径 </returns>
        public static string ChooseInpFile(string title)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = title;
            ofd.CheckFileExists = true;
            ofd.AddExtension = true;
            ofd.Filter = "inp文件(*.inp)| *.inp";
            ofd.FilterIndex = 2;
            ofd.Multiselect = false;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                return Convert.ToString(ofd.FileName.Length > 0 ? ofd.FileName : "");
            }
            return "";
        }

        public void TextBox_zonesInp_Enter(object sender, EventArgs e)
        {
            var txt = (TextBox)sender;
            txt.SelectAll();
            txt.Focus();
        }

        #endregion

        #region ---    Liner 模式

        public void buttonTransForm_Click(object sender, EventArgs e)
        {
            _message.AppendLine();
            if (!worker.IsBusy)
            {
                DoseOff();

                ProgressBar1.Visible = true;

                worker.RunWorkerAsync();
            }
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // 设置命令文本所在的文件夹的绝对路径
            string globalDirePath = Path.Combine(Directory.GetCurrentDirectory(), "Hm2Fl-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"));

            if (!Directory.Exists(globalDirePath))
            {
                Directory.CreateDirectory(globalDirePath);
            }
            Flac3dCommandWriters.CommandDirectory = globalDirePath;

            //
            Thread thdZone = default(Thread);
            Thread thdSel = default(Thread);

            // 先生成土体的flac文件
            if (CheckZone())
            {
                string pathZ = TextBox_zonesInp.Text;
                thdZone = new Thread(new ParameterizedThreadStart(Export2Zone));
                thdZone.Start(pathZ);
            }
            else
            {
                _message.AppendLine("******** 土体网格数据提取失败 ********");
            }

            // 再生成结构的flac文件
            if (CheckSel())
            {
                string pathS = TextBox_structuresInp.Text;
                thdSel = new Thread(new ParameterizedThreadStart(Export2Sel));
                thdSel.Start(pathS);
            }
            else
            {
                _message.AppendLine("******** 结构网格数据提取失败 ********");
            }

            //
            if (thdZone != null)
            {
                thdZone.Join();
            }
            if (thdSel != null)
            {
                thdSel.Join();
            }
        }

        private bool CheckZone()
        {
            string pathZ = Convert.ToString(TextBox_zonesInp.Text);

            if (string.IsNullOrEmpty(pathZ))
            {
                return false;
            }

            if (!File.Exists(pathZ))
            {
                _message.AppendLine("指定位置的土体网格文件不存在");
                return false;
            }

            // 先生成土体的flac文件
            if (string.Compare(strA: Path.GetExtension(pathZ), strB: ".inp", ignoreCase: true) != 0)
            {
                _message.AppendLine("土体网格文件格式不对");
                return false;
            }


            return true;
        }

        private bool CheckSel()
        {
            string pathS = Convert.ToString(TextBox_structuresInp.Text);

            if (string.IsNullOrEmpty(pathS))
            {
                return false;
            }

            if (!File.Exists(pathS))
            {
                _message.AppendLine("指定位置的结构网格文件不存在");
                return false;
            }

            if (string.Compare(strA: Path.GetExtension(pathS), strB: ".inp", ignoreCase: true) != 0)
            {
                _message.AppendLine("结构网格文件格式不对");
                return false;
            }


            return true;
        }

        /// <summary> 将Hypermesh中的实体单元导出到一个 flac3d 文本中。其名称为 0zones.flac3d。 </summary>
        /// <param name="path_ZoneFile"></param>
        private void Export2Zone(object path_ZoneFile)
        {
            string pathZ = path_ZoneFile.ToString();

            // 打开文本
            FileStream Fileinp = default(FileStream);
            StreamReader sr_inp = default(StreamReader);

            try
            {
                // 打开文本
                Fileinp = File.Open(pathZ, FileMode.Open);
                sr_inp = new StreamReader(Fileinp);
                //
                var hmZone = new Hm2Zone(sr_inp, _message);
                hmZone.ReadFile();
                //
                _message.AppendLine(Convert.ToString("******** 土体网格数据提取完成 ********" + "\r\n"));
            }
            catch (Exception ex)
            {
                _message.AppendLine(ex.Message);
                _message.AppendLine(Convert.ToString("******** 土体网格数据提取失败 ********" + "\r\n"));
            }
          }

        /// <summary> 将不同类型的结构单元导出到一个或者多个文本中 </summary>
        /// <param name="path_StructFile">记录结构单元的inp文件的绝对路径</param>
        private void Export2Sel(object path_StructFile)
        {
            string pathS = path_StructFile.ToString();

            // 打开文本

            FileStream Fileinp = default(FileStream);
            StreamReader sr_inp = default(StreamReader);

            try
            {
                Fileinp = File.Open(pathS, FileMode.Open);
                sr_inp = new StreamReader(Fileinp);
                //'
                var hmSel = new Hm2Structure(sr_inp, _message);
                hmSel.ReadFile();
                //
                _message.AppendLine(Convert.ToString("******** 结构网格数据提取完成 ********" + "\r\n"));
            }
            catch (Exception ex)
            {
                _message.AppendLine(ex.Message);
                _message.AppendLine(Convert.ToString("******** 结构网格数据提取失败 ********" + "\r\n"));
            }
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //
            _message.AppendLine();
            _message.AppendLine("转换完成");
            LabelHello.Text = _message.ToString();

            // 关闭所有打开的文本
            Flac3dCommandWriters fcw = Flac3dCommandWriters.GetUniqueInstance();
            fcw.CloseAllWriters(true);

            //
            ProgressBar1.Visible = false;
            WarmUp();
        }

        #endregion

        #region ---   通过 Console 运行无 Liner 的转换程序

        public void ButtonNoLiner_Click(object sender, EventArgs e)
        {
            Hide();

            Hm2Flac3DHandler.AllocConsole();

            Hm2Flac3D_V1.Main();
            // 一般情况下，当控制台被手动点击关闭（而不是通过 FreeConsole 来释放控制台）后，整个程序的进程就结束了，这里为了保险起见，再强制关闭一下。
            Close();
        }

        public void LabelHello_Click(object sender, EventArgs e)
        {
        }

        #endregion
    }
}