using System.Windows.Forms;

namespace Hm2Flac3D.Enhanced
{
    public partial class form_Hm2Flac3D : System.Windows.Forms.Form
    {

        //Form overrides dispose to clean up the component list.
        [System.Diagnostics.DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components != null)
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        //NOTE: The following procedure is required by the Windows Form Designer
        //It can be modified using the Windows Form Designer.
        //Do not modify it using the code editor.
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(form_Hm2Flac3D));
            this.buttonTransForm = new System.Windows.Forms.Button();
            this.TextBox_zonesInp = new System.Windows.Forms.TextBox();
            this.TextBox_structuresInp = new System.Windows.Forms.TextBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.Label2 = new System.Windows.Forms.Label();
            this.ButtonChooseZones = new System.Windows.Forms.Button();
            this.ButtonChooseStructures = new System.Windows.Forms.Button();
            this.LabelHello = new System.Windows.Forms.TextBox();
            this.ProgressBar1 = new System.Windows.Forms.ProgressBar();
            this.ButtonNoLiner = new System.Windows.Forms.Button();
            this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.button_OpenWorkDirectory = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.ToolStripMenuItem_Setting = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_ClearText = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_Directory = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_Help = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_HelpDocument = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_About = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonTransForm
            // 
            this.buttonTransForm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTransForm.Location = new System.Drawing.Point(397, 347);
            this.buttonTransForm.Name = "buttonTransForm";
            this.buttonTransForm.Size = new System.Drawing.Size(75, 23);
            this.buttonTransForm.TabIndex = 0;
            this.buttonTransForm.Text = "转换";
            this.buttonTransForm.UseVisualStyleBackColor = true;
            this.buttonTransForm.Click += new System.EventHandler(this.buttonTransForm_Click);
            // 
            // TextBox_zonesInp
            // 
            this.TextBox_zonesInp.AllowDrop = true;
            this.TextBox_zonesInp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextBox_zonesInp.Location = new System.Drawing.Point(11, 266);
            this.TextBox_zonesInp.Name = "TextBox_zonesInp";
            this.TextBox_zonesInp.Size = new System.Drawing.Size(379, 21);
            this.TextBox_zonesInp.TabIndex = 1;
            this.TextBox_zonesInp.DragDrop += new System.Windows.Forms.DragEventHandler(this.TextBox_zonesInp_DragDrop);
            this.TextBox_zonesInp.DragEnter += new System.Windows.Forms.DragEventHandler(this.TextBox_zonesInp_DragEnter);
            // 
            // TextBox_structuresInp
            // 
            this.TextBox_structuresInp.AllowDrop = true;
            this.TextBox_structuresInp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextBox_structuresInp.Location = new System.Drawing.Point(12, 314);
            this.TextBox_structuresInp.Name = "TextBox_structuresInp";
            this.TextBox_structuresInp.Size = new System.Drawing.Size(379, 21);
            this.TextBox_structuresInp.TabIndex = 2;
            this.TextBox_structuresInp.DragDrop += new System.Windows.Forms.DragEventHandler(this.TextBox_zonesInp_DragDrop);
            this.TextBox_structuresInp.DragEnter += new System.Windows.Forms.DragEventHandler(this.TextBox_zonesInp_DragEnter);
            // 
            // Label1
            // 
            this.Label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(12, 247);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(59, 12);
            this.Label1.TabIndex = 3;
            this.Label1.Text = "zones.inp";
            // 
            // Label2
            // 
            this.Label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Label2.AutoSize = true;
            this.Label2.Location = new System.Drawing.Point(12, 296);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(89, 12);
            this.Label2.TabIndex = 3;
            this.Label2.Text = "structures.inp";
            // 
            // ButtonChooseZones
            // 
            this.ButtonChooseZones.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonChooseZones.Location = new System.Drawing.Point(396, 267);
            this.ButtonChooseZones.Name = "ButtonChooseZones";
            this.ButtonChooseZones.Size = new System.Drawing.Size(75, 23);
            this.ButtonChooseZones.TabIndex = 1;
            this.ButtonChooseZones.Text = "选择";
            this.ButtonChooseZones.UseVisualStyleBackColor = true;
            this.ButtonChooseZones.Click += new System.EventHandler(this.ButtonChooseZones_Click);
            // 
            // ButtonChooseStructures
            // 
            this.ButtonChooseStructures.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonChooseStructures.Location = new System.Drawing.Point(396, 312);
            this.ButtonChooseStructures.Name = "ButtonChooseStructures";
            this.ButtonChooseStructures.Size = new System.Drawing.Size(75, 23);
            this.ButtonChooseStructures.TabIndex = 2;
            this.ButtonChooseStructures.Text = "选择";
            this.ButtonChooseStructures.UseVisualStyleBackColor = true;
            this.ButtonChooseStructures.Click += new System.EventHandler(this.ButtonChooseZones_Click);
            // 
            // LabelHello
            // 
            this.LabelHello.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LabelHello.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.LabelHello.Cursor = System.Windows.Forms.Cursors.Default;
            this.LabelHello.Location = new System.Drawing.Point(9, 27);
            this.LabelHello.Multiline = true;
            this.LabelHello.Name = "LabelHello";
            this.LabelHello.ReadOnly = true;
            this.LabelHello.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.LabelHello.ShortcutsEnabled = false;
            this.LabelHello.Size = new System.Drawing.Size(462, 209);
            this.LabelHello.TabIndex = 3;
            this.LabelHello.TabStop = false;
            this.LabelHello.Text = "******** CONVERT INP CODE(IMPORTED FROM HYPERMESH) TO FLAC3D ********";
            this.LabelHello.Click += new System.EventHandler(this.LabelHello_Click);
            // 
            // ProgressBar1
            // 
            this.ProgressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ProgressBar1.Location = new System.Drawing.Point(0, 378);
            this.ProgressBar1.Name = "ProgressBar1";
            this.ProgressBar1.Size = new System.Drawing.Size(484, 10);
            this.ProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.ProgressBar1.TabIndex = 4;
            // 
            // ButtonNoLiner
            // 
            this.ButtonNoLiner.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ButtonNoLiner.Location = new System.Drawing.Point(11, 346);
            this.ButtonNoLiner.Name = "ButtonNoLiner";
            this.ButtonNoLiner.Size = new System.Drawing.Size(75, 23);
            this.ButtonNoLiner.TabIndex = 6;
            this.ButtonNoLiner.Text = "No Liner";
            this.ToolTip1.SetToolTip(this.ButtonNoLiner, "不导出 Liner 单元");
            this.ButtonNoLiner.UseVisualStyleBackColor = true;
            this.ButtonNoLiner.Click += new System.EventHandler(this.ButtonNoLiner_Click);
            // 
            // button_OpenWorkDirectory
            // 
            this.button_OpenWorkDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OpenWorkDirectory.Location = new System.Drawing.Point(316, 347);
            this.button_OpenWorkDirectory.Name = "button_OpenWorkDirectory";
            this.button_OpenWorkDirectory.Size = new System.Drawing.Size(75, 23);
            this.button_OpenWorkDirectory.TabIndex = 8;
            this.button_OpenWorkDirectory.Text = "打开文件夹";
            this.ToolTip1.SetToolTip(this.button_OpenWorkDirectory, "打开工作路径文件夹");
            this.button_OpenWorkDirectory.UseVisualStyleBackColor = true;
            this.button_OpenWorkDirectory.Click += new System.EventHandler(this.button_OpenWorkDirectory_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_Setting,
            this.ToolStripMenuItem_Help});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(484, 24);
            this.menuStrip1.TabIndex = 7;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // ToolStripMenuItem_Setting
            // 
            this.ToolStripMenuItem_Setting.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_ClearText,
            this.ToolStripMenuItem_Directory});
            this.ToolStripMenuItem_Setting.Name = "ToolStripMenuItem_Setting";
            this.ToolStripMenuItem_Setting.Size = new System.Drawing.Size(45, 20);
            this.ToolStripMenuItem_Setting.Text = "设置";
            // 
            // ToolStripMenuItem_ClearText
            // 
            this.ToolStripMenuItem_ClearText.Name = "ToolStripMenuItem_ClearText";
            this.ToolStripMenuItem_ClearText.Size = new System.Drawing.Size(126, 22);
            this.ToolStripMenuItem_ClearText.Text = "清除文本";
            this.ToolStripMenuItem_ClearText.Click += new System.EventHandler(this.ToolStripMenuItem_ClearText_Click);
            // 
            // ToolStripMenuItem_Directory
            // 
            this.ToolStripMenuItem_Directory.Name = "ToolStripMenuItem_Directory";
            this.ToolStripMenuItem_Directory.Size = new System.Drawing.Size(126, 22);
            this.ToolStripMenuItem_Directory.Text = "工作路径";
            this.ToolStripMenuItem_Directory.ToolTipText = "命令文本的输入文件夹";
            this.ToolStripMenuItem_Directory.Click += new System.EventHandler(this.ToolStripMenuItem_Directory_Click);
            // 
            // ToolStripMenuItem_Help
            // 
            this.ToolStripMenuItem_Help.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_HelpDocument,
            this.ToolStripMenuItem_About});
            this.ToolStripMenuItem_Help.Name = "ToolStripMenuItem_Help";
            this.ToolStripMenuItem_Help.Size = new System.Drawing.Size(45, 20);
            this.ToolStripMenuItem_Help.Text = "帮助";
            // 
            // ToolStripMenuItem_HelpDocument
            // 
            this.ToolStripMenuItem_HelpDocument.Name = "ToolStripMenuItem_HelpDocument";
            this.ToolStripMenuItem_HelpDocument.Size = new System.Drawing.Size(100, 22);
            this.ToolStripMenuItem_HelpDocument.Text = "文档";
            // 
            // ToolStripMenuItem_About
            // 
            this.ToolStripMenuItem_About.Name = "ToolStripMenuItem_About";
            this.ToolStripMenuItem_About.Size = new System.Drawing.Size(100, 22);
            this.ToolStripMenuItem_About.Text = "关于";
            this.ToolStripMenuItem_About.Click += new System.EventHandler(this.ToolStripMenuItem_About_Click);
            // 
            // form_Hm2Flac3D
            // 
            this.AcceptButton = this.buttonTransForm;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 388);
            this.Controls.Add(this.button_OpenWorkDirectory);
            this.Controls.Add(this.ButtonNoLiner);
            this.Controls.Add(this.ProgressBar1);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.LabelHello);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.TextBox_structuresInp);
            this.Controls.Add(this.TextBox_zonesInp);
            this.Controls.Add(this.ButtonChooseStructures);
            this.Controls.Add(this.ButtonChooseZones);
            this.Controls.Add(this.buttonTransForm);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(500, 300);
            this.Name = "form_Hm2Flac3D";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Hm2Flac3D";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.form_Hm2Flac3D_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.form_Hm2Flac3D_FormClosed);
            this.Load += new System.EventHandler(this.form_Hm2Flac3D_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        internal System.Windows.Forms.Button buttonTransForm;
        internal System.Windows.Forms.TextBox TextBox_zonesInp;
        internal System.Windows.Forms.TextBox TextBox_structuresInp;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.Button ButtonChooseZones;
        internal System.Windows.Forms.Button ButtonChooseStructures;
        internal System.Windows.Forms.TextBox LabelHello;
        internal System.Windows.Forms.ProgressBar ProgressBar1;
        internal System.Windows.Forms.Button ButtonNoLiner;
        internal System.Windows.Forms.ToolTip ToolTip1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem ToolStripMenuItem_Setting;
        private ToolStripMenuItem ToolStripMenuItem_Directory;
        private ToolStripMenuItem ToolStripMenuItem_Help;
        private ToolStripMenuItem ToolStripMenuItem_About;
        private System.ComponentModel.IContainer components;
        private Button button_OpenWorkDirectory;
        private ToolStripMenuItem ToolStripMenuItem_ClearText;
        private ToolStripMenuItem ToolStripMenuItem_HelpDocument;
    }
}
