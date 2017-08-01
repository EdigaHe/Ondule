namespace PluginBar.UI
{
    partial class PropertyWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_OK = new System.Windows.Forms.Button();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.lb_coilNum = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cb_distribution = new System.Windows.Forms.CheckBox();
            this.cb_clockwise = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lb_layerNum = new System.Windows.Forms.Label();
            this.trackBar2 = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_OK
            // 
            this.btn_OK.BackColor = System.Drawing.SystemColors.Highlight;
            this.btn_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_OK.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_OK.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.btn_OK.Location = new System.Drawing.Point(474, 364);
            this.btn_OK.Margin = new System.Windows.Forms.Padding(6);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(210, 81);
            this.btn_OK.TabIndex = 0;
            this.btn_OK.Text = "OK";
            this.btn_OK.UseVisualStyleBackColor = false;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 
            // trackBar1
            // 
            this.trackBar1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.trackBar1.Location = new System.Drawing.Point(54, 126);
            this.trackBar1.Margin = new System.Windows.Forms.Padding(6);
            this.trackBar1.Maximum = 30;
            this.trackBar1.Minimum = 1;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(548, 90);
            this.trackBar1.TabIndex = 1;
            this.trackBar1.Value = 1;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            this.trackBar1.ValueChanged += new System.EventHandler(this.trackBar1_ValueChanged);
            // 
            // lb_coilNum
            // 
            this.lb_coilNum.AutoSize = true;
            this.lb_coilNum.Font = new System.Drawing.Font("Segoe UI Semibold", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_coilNum.Location = new System.Drawing.Point(634, 77);
            this.lb_coilNum.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lb_coilNum.Name = "lb_coilNum";
            this.lb_coilNum.Size = new System.Drawing.Size(52, 72);
            this.lb_coilNum.TabIndex = 2;
            this.lb_coilNum.Text = "1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.label2.Location = new System.Drawing.Point(45, 66);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(209, 54);
            this.label2.TabIndex = 3;
            this.label2.Text = "Some Text";
            // 
            // cb_distribution
            // 
            this.cb_distribution.AutoSize = true;
            this.cb_distribution.Enabled = false;
            this.cb_distribution.Font = new System.Drawing.Font("Segoe UI Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_distribution.Location = new System.Drawing.Point(72, 360);
            this.cb_distribution.Margin = new System.Windows.Forms.Padding(6);
            this.cb_distribution.Name = "cb_distribution";
            this.cb_distribution.Size = new System.Drawing.Size(305, 49);
            this.cb_distribution.TabIndex = 4;
            this.cb_distribution.Text = "Equal Distribution?";
            this.cb_distribution.UseVisualStyleBackColor = true;
            this.cb_distribution.Visible = false;
            // 
            // cb_clockwise
            // 
            this.cb_clockwise.AutoSize = true;
            this.cb_clockwise.Enabled = false;
            this.cb_clockwise.Font = new System.Drawing.Font("Segoe UI Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_clockwise.Location = new System.Drawing.Point(72, 419);
            this.cb_clockwise.Margin = new System.Windows.Forms.Padding(6);
            this.cb_clockwise.Name = "cb_clockwise";
            this.cb_clockwise.Size = new System.Drawing.Size(196, 49);
            this.cb_clockwise.TabIndex = 5;
            this.cb_clockwise.Text = "Clockwise?";
            this.cb_clockwise.UseVisualStyleBackColor = true;
            this.cb_clockwise.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(36, 202);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(209, 54);
            this.label1.TabIndex = 6;
            this.label1.Text = "Some Text";
            // 
            // lb_layerNum
            // 
            this.lb_layerNum.AutoSize = true;
            this.lb_layerNum.Font = new System.Drawing.Font("Segoe UI Semibold", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_layerNum.Location = new System.Drawing.Point(634, 256);
            this.lb_layerNum.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lb_layerNum.Name = "lb_layerNum";
            this.lb_layerNum.Size = new System.Drawing.Size(52, 72);
            this.lb_layerNum.TabIndex = 7;
            this.lb_layerNum.Text = "1";
            // 
            // trackBar2
            // 
            this.trackBar2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.trackBar2.Location = new System.Drawing.Point(54, 262);
            this.trackBar2.Margin = new System.Windows.Forms.Padding(6);
            this.trackBar2.Minimum = 1;
            this.trackBar2.Name = "trackBar2";
            this.trackBar2.Size = new System.Drawing.Size(548, 90);
            this.trackBar2.TabIndex = 8;
            this.trackBar2.Value = 1;
            this.trackBar2.Scroll += new System.EventHandler(this.trackBar2_Scroll);
            // 
            // PropertyWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = MetroFramework.Drawing.MetroBorderStyle.FixedSingle;
            this.ClientSize = new System.Drawing.Size(747, 482);
            this.ControlBox = false;
            this.Controls.Add(this.trackBar2);
            this.Controls.Add(this.lb_layerNum);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cb_clockwise);
            this.Controls.Add(this.cb_distribution);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lb_coilNum);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.btn_OK);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "PropertyWindow";
            this.Text = "Coil Property";
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Label lb_coilNum;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cb_distribution;
        private System.Windows.Forms.CheckBox cb_clockwise;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lb_layerNum;
        private System.Windows.Forms.TrackBar trackBar2;
    }
}