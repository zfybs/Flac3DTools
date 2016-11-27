using System.Windows.Forms;

namespace Hm2Flac3D.Utility
{
    partial class FormSettings : System.Windows.Forms.Form
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
            this.Label1 = new System.Windows.Forms.Label();
            this.TextBox_WorkDirectory = new System.Windows.Forms.TextBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.checkBox_ExecutingDirectory = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(13, 13);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(125, 12);
            this.Label1.TabIndex = 0;
            this.Label1.Text = "Hm2Flac3d 工作文件夹";
            // 
            // TextBox_WorkDirectory
            // 
            this.TextBox_WorkDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextBox_WorkDirectory.Location = new System.Drawing.Point(24, 29);
            this.TextBox_WorkDirectory.Name = "TextBox_WorkDirectory";
            this.TextBox_WorkDirectory.Size = new System.Drawing.Size(277, 21);
            this.TextBox_WorkDirectory.TabIndex = 1;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(307, 58);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "确定";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // checkBox_ExecutingDirectory
            // 
            this.checkBox_ExecutingDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox_ExecutingDirectory.AutoSize = true;
            this.checkBox_ExecutingDirectory.Location = new System.Drawing.Point(298, 7);
            this.checkBox_ExecutingDirectory.Name = "checkBox_ExecutingDirectory";
            this.checkBox_ExecutingDirectory.Size = new System.Drawing.Size(84, 16);
            this.checkBox_ExecutingDirectory.TabIndex = 3;
            this.checkBox_ExecutingDirectory.Text = "程序文件夹";
            this.checkBox_ExecutingDirectory.UseVisualStyleBackColor = true;
            this.checkBox_ExecutingDirectory.CheckedChanged += new System.EventHandler(this.checkBox_ExecutingDirectory_CheckedChanged);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(307, 27);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "选择";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FormSettings
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(394, 93);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.checkBox_ExecutingDirectory);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.TextBox_WorkDirectory);
            this.Controls.Add(this.Label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Options";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing_1);
            this.Load += new System.EventHandler(this.HelpLocation_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        internal Label Label1;
        internal TextBox TextBox_WorkDirectory;
        internal Button btnOk;
        private CheckBox checkBox_ExecutingDirectory;
        private ToolTip toolTip1;
        private System.ComponentModel.IContainer components;
        private Button button1;
    }

}
