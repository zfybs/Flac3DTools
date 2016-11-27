using System;
using System.IO;
using System.Windows.Forms;

namespace Hm2Flac3D.Utility
{
    /// <summary>
    /// 帮助文档的位置
    /// </summary>
    public partial class FormSettings
    {
        public FormSettings()
        {
            InitializeComponent();
        }

        private Hm2Fl3dSetting settings1 = new Hm2Fl3dSetting();

        public void HelpLocation_Load(object sender, EventArgs e)
        {
            this.TextBox_WorkDirectory.Text = settings1.WorkDirectory;
            toolTip1.SetToolTip(checkBox_ExecutingDirectory, Directory.GetCurrentDirectory());
        }

        public void Form1_FormClosing_1(object sender,
            FormClosingEventArgs e)
        {
            // Save settings manually.
            settings1.Save();
        }

        public void btnOk_Click(object sender, EventArgs e)
        {
            if (_useExecutingDirectory)
            {
                settings1.WorkDirectory = Directory.GetCurrentDirectory();
                Close();
                return;
            }
            // 必须使用用户输入的路径
            string userPath = TextBox_WorkDirectory.Text;
            if (Directory.Exists(userPath))
            {
                settings1.WorkDirectory = userPath;
                Close();
            }
            else
            {
                var res = MessageBox.Show(@"指定的文件夹不存在，点击是以创建，点击否进行重新设置。", "错误", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (res == DialogResult.Yes)
                {
                    // 尝试创建一个文件夹
                    try
                    {
                        Directory.CreateDirectory(userPath);
                        settings1.WorkDirectory = userPath;
                        Close();
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(@"文件夹创建失败。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    // 直接返回
                    return;
                }
            }
        }

        private bool _useExecutingDirectory;
        private void checkBox_ExecutingDirectory_CheckedChanged(object sender, EventArgs e)
        {
            _useExecutingDirectory = checkBox_ExecutingDirectory.Checked;
            TextBox_WorkDirectory.Enabled = !_useExecutingDirectory;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog()
            {
                Description = @"选择工作文件夹，转换后的文件会放置在此文件夹中。",
            };
            var res = fbd.ShowDialog();
            if (res == DialogResult.OK)
            {
                TextBox_WorkDirectory.Text = fbd.SelectedPath;
            }
        }
    }
}