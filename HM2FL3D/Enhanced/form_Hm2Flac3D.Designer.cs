using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Data;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using System.Collections;
using System.Linq;
using System.Windows.Forms;

namespace Hm2Flac3D
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

        //Required by the Windows Form Designer
        private System.ComponentModel.Container components = null;

        //NOTE: The following procedure is required by the Windows Form Designer
        //It can be modified using the Windows Form Designer.
        //Do not modify it using the code editor.
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            base.Load += new System.EventHandler(form_Hm2Flac3D_Load);
            base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(form_Hm2Flac3D_FormClosing);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(form_Hm2Flac3D));
            this.buttonTransForm = new System.Windows.Forms.Button();
            this.buttonTransForm.Click += new System.EventHandler(this.buttonTransForm_Click);
            this.TextBox_zonesInp = new System.Windows.Forms.TextBox();
            this.TextBox_zonesInp.DragEnter += new DragEventHandler(this.TextBox_zonesInp_DragEnter);
            this.TextBox_zonesInp.DragDrop += new DragEventHandler(this.TextBox_zonesInp_DragDrop);
            this.TextBox_zonesInp.MouseEnter += new System.EventHandler(this.TextBox_zonesInp_Enter);
            this.TextBox_structuresInp = new System.Windows.Forms.TextBox();
            this.TextBox_structuresInp.DragEnter += new DragEventHandler(this.TextBox_zonesInp_DragEnter);
            this.TextBox_structuresInp.DragDrop += new DragEventHandler(this.TextBox_zonesInp_DragDrop);
            this.TextBox_structuresInp.MouseEnter += new System.EventHandler(this.TextBox_zonesInp_Enter);
            this.Label1 = new System.Windows.Forms.Label();
            this.Label2 = new System.Windows.Forms.Label();
            this.ButtonChooseZones = new System.Windows.Forms.Button();
            this.ButtonChooseZones.Click += new System.EventHandler(this.ButtonChooseZones_Click);
            this.ButtonChooseStructures = new System.Windows.Forms.Button();
            this.ButtonChooseStructures.Click += new System.EventHandler(this.ButtonChooseZones_Click);
            this.LabelHello = new System.Windows.Forms.TextBox();
            this.LabelHello.Click += new System.EventHandler(this.LabelHello_Click);
            this.ProgressBar1 = new System.Windows.Forms.ProgressBar();
            this.ButtonClose = new System.Windows.Forms.Button();
            this.ButtonClose.Click += new System.EventHandler(this.ButtonClose_Click);
            this.ButtonNoLiner = new System.Windows.Forms.Button();
            this.ButtonNoLiner.Click += new System.EventHandler(this.ButtonNoLiner_Click);
            this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            //
            //buttonTransForm
            //
            this.buttonTransForm.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
            this.buttonTransForm.Location = new System.Drawing.Point(390, 270);
            this.buttonTransForm.Name = "buttonTransForm";
            this.buttonTransForm.Size = new System.Drawing.Size(75, 23);
            this.buttonTransForm.TabIndex = 0;
            this.buttonTransForm.Text = "转换";
            this.buttonTransForm.UseVisualStyleBackColor = true;
            //
            //TextBox_zonesInp
            //
            this.TextBox_zonesInp.AllowDrop = true;
            this.TextBox_zonesInp.Anchor = (System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right);
            this.TextBox_zonesInp.Location = new System.Drawing.Point(11, 189);
            this.TextBox_zonesInp.Name = "TextBox_zonesInp";
            this.TextBox_zonesInp.Size = new System.Drawing.Size(372, 21);
            this.TextBox_zonesInp.TabIndex = 1;
            //
            //TextBox_structuresInp
            //
            this.TextBox_structuresInp.AllowDrop = true;
            this.TextBox_structuresInp.Anchor = (System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right);
            this.TextBox_structuresInp.Location = new System.Drawing.Point(12, 237);
            this.TextBox_structuresInp.Name = "TextBox_structuresInp";
            this.TextBox_structuresInp.Size = new System.Drawing.Size(372, 21);
            this.TextBox_structuresInp.TabIndex = 2;
            //
            //Label1
            //
            this.Label1.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(12, 170);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(59, 12);
            this.Label1.TabIndex = 3;
            this.Label1.Text = "zones.inp";
            //
            //Label2
            //
            this.Label2.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
            this.Label2.AutoSize = true;
            this.Label2.Location = new System.Drawing.Point(12, 219);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(89, 12);
            this.Label2.TabIndex = 3;
            this.Label2.Text = "structures.inp";
            //
            //ButtonChooseZones
            //
            this.ButtonChooseZones.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
            this.ButtonChooseZones.Location = new System.Drawing.Point(389, 190);
            this.ButtonChooseZones.Name = "ButtonChooseZones";
            this.ButtonChooseZones.Size = new System.Drawing.Size(75, 23);
            this.ButtonChooseZones.TabIndex = 1;
            this.ButtonChooseZones.Text = "选择";
            this.ButtonChooseZones.UseVisualStyleBackColor = true;
            //
            //ButtonChooseStructures
            //
            this.ButtonChooseStructures.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
            this.ButtonChooseStructures.Location = new System.Drawing.Point(389, 235);
            this.ButtonChooseStructures.Name = "ButtonChooseStructures";
            this.ButtonChooseStructures.Size = new System.Drawing.Size(75, 23);
            this.ButtonChooseStructures.TabIndex = 2;
            this.ButtonChooseStructures.Text = "选择";
            this.ButtonChooseStructures.UseVisualStyleBackColor = true;
            //
            //LabelHello
            //
            this.LabelHello.Anchor = (System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right);
            this.LabelHello.BackColor = System.Drawing.Color.FromArgb(System.Convert.ToInt32(System.Convert.ToByte(224)), System.Convert.ToInt32(System.Convert.ToByte(224)), System.Convert.ToInt32(System.Convert.ToByte(224)));
            this.LabelHello.Cursor = System.Windows.Forms.Cursors.Default;
            this.LabelHello.Location = new System.Drawing.Point(9, 9);
            this.LabelHello.Multiline = true;
            this.LabelHello.Name = "LabelHello";
            this.LabelHello.ReadOnly = true;
            this.LabelHello.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.LabelHello.ShortcutsEnabled = false;
            this.LabelHello.Size = new System.Drawing.Size(455, 150);
            this.LabelHello.TabIndex = 3;
            this.LabelHello.TabStop = false;
            this.LabelHello.Text = "******** CONVERT INP CODE(IMPORTED FROM HYPERMESH) TO FLAC3D ********";
            //
            //ProgressBar1
            //
            this.ProgressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ProgressBar1.Location = new System.Drawing.Point(0, 301);
            this.ProgressBar1.Name = "ProgressBar1";
            this.ProgressBar1.Size = new System.Drawing.Size(477, 10);
            this.ProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.ProgressBar1.TabIndex = 4;
            //
            //ButtonClose
            //
            this.ButtonClose.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
            this.ButtonClose.Location = new System.Drawing.Point(308, 270);
            this.ButtonClose.Name = "ButtonClose";
            this.ButtonClose.Size = new System.Drawing.Size(75, 23);
            this.ButtonClose.TabIndex = 5;
            this.ButtonClose.Text = "关闭";
            this.ButtonClose.UseVisualStyleBackColor = true;
            //
            //ButtonNoLiner
            //
            this.ButtonNoLiner.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
            this.ButtonNoLiner.Location = new System.Drawing.Point(11, 269);
            this.ButtonNoLiner.Name = "ButtonNoLiner";
            this.ButtonNoLiner.Size = new System.Drawing.Size(75, 23);
            this.ButtonNoLiner.TabIndex = 6;
            this.ButtonNoLiner.Text = "No Liner";
            this.ToolTip1.SetToolTip(this.ButtonNoLiner, "不导出 Liner 单元");
            this.ButtonNoLiner.UseVisualStyleBackColor = true;
            //
            //form_Hm2Flac3D
            //
            this.AcceptButton = this.buttonTransForm;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6, 12);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(477, 311);
            this.Controls.Add(this.ButtonNoLiner);
            this.Controls.Add(this.ProgressBar1);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.LabelHello);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.TextBox_structuresInp);
            this.Controls.Add(this.TextBox_zonesInp);
            this.Controls.Add(this.ButtonChooseStructures);
            this.Controls.Add(this.ButtonChooseZones);
            this.Controls.Add(this.ButtonClose);
            this.Controls.Add(this.buttonTransForm);
            this.Icon = (System.Drawing.Icon)(resources.GetObject("$this.Icon"));
            this.MinimumSize = new System.Drawing.Size(459, 300);
            this.Name = "form_Hm2Flac3D";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "form_Hm2Flac3D";
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
        internal System.Windows.Forms.Button ButtonClose;
        internal System.Windows.Forms.Button ButtonNoLiner;
        internal System.Windows.Forms.ToolTip ToolTip1;
    }
}
