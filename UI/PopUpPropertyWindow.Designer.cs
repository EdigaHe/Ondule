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
            this.cb_distribution = new System.Windows.Forms.CheckBox();
            this.cb_clockwise = new System.Windows.Forms.CheckBox();

            // Coil Diameter
            this.label2 = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.lb_coilDiameter = new System.Windows.Forms.Label();

            // Pitch
            this.label1 = new System.Windows.Forms.Label();
            this.trackBar2 = new System.Windows.Forms.TrackBar();
            this.lb_pitchValue = new System.Windows.Forms.Label();
            
            // Length
            this.label4 = new System.Windows.Forms.Label();
            this.trackBar3 = new System.Windows.Forms.TrackBar();
            this.lb_length = new System.Windows.Forms.Label();
            
            // Spring Diameter
            this.label5 = new System.Windows.Forms.Label();
            this.trackBar4 = new System.Windows.Forms.TrackBar();
            this.lb_springDiameter = new System.Windows.Forms.Label();

            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar4)).BeginInit();
            this.SuspendLayout();


            // 
            // btn_OK
            // 
            this.btn_OK.BackColor = System.Drawing.SystemColors.Highlight;
            this.btn_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_OK.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_OK.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.btn_OK.Location = new System.Drawing.Point(225, 348);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(105, 42);
            this.btn_OK.TabIndex = 0;
            this.btn_OK.Text = "OK";
            this.btn_OK.UseVisualStyleBackColor = false;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 
            // trackBar1
            // 
            this.trackBar1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.trackBar1.Location = new System.Drawing.Point(27, 40);
            this.trackBar1.Maximum = 30;
            this.trackBar1.Minimum = 1;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(274, 45);
            this.trackBar1.TabIndex = 1;
            this.trackBar1.Value = 1;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // lb_coilNum
            // 
            this.lb_coilDiameter.AutoSize = true;
            this.lb_coilDiameter.Font = new System.Drawing.Font("Segoe UI Semibold", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_coilDiameter.Location = new System.Drawing.Point(317, 40);
            this.lb_coilDiameter.Name = "lb_coilDiameter";
            this.lb_coilDiameter.Size = new System.Drawing.Size(28, 37);
            this.lb_coilDiameter.TabIndex = 2;
            this.lb_coilDiameter.Text = "1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.label2.Location = new System.Drawing.Point(18, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(191, 28);
            this.label2.TabIndex = 3;
            this.label2.Text = "Coil Diameter (mm)";
            // 
            // cb_distribution
            // 
            this.cb_distribution.AutoSize = true;
            this.cb_distribution.Enabled = false;
            this.cb_distribution.Font = new System.Drawing.Font("Segoe UI Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_distribution.Location = new System.Drawing.Point(24, 346);
            this.cb_distribution.Name = "cb_distribution";
            this.cb_distribution.Size = new System.Drawing.Size(154, 25);
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
            this.cb_clockwise.Location = new System.Drawing.Point(24, 377);
            this.cb_clockwise.Name = "cb_clockwise";
            this.cb_clockwise.Size = new System.Drawing.Size(100, 25);
            this.cb_clockwise.TabIndex = 5;
            this.cb_clockwise.Text = "Clockwise?";
            this.cb_clockwise.UseVisualStyleBackColor = true;
            this.cb_clockwise.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(18, 88);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 28);
            this.label1.TabIndex = 6;
            this.label1.Text = "Pitch (mm)";
            // 
            // lb_pitchValue
            // 
            this.lb_pitchValue.AutoSize = true;
            this.lb_pitchValue.Font = new System.Drawing.Font("Segoe UI Semibold", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_pitchValue.Location = new System.Drawing.Point(317, 125);
            this.lb_pitchValue.Name = "lb_pitchValue";
            this.lb_pitchValue.Size = new System.Drawing.Size(28, 37);
            this.lb_pitchValue.TabIndex = 7;
            this.lb_pitchValue.Text = "1";
            // 
            // trackBar2
            // 
            this.trackBar2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.trackBar2.Location = new System.Drawing.Point(27, 125);
            this.trackBar2.Minimum = 1;
            this.trackBar2.Name = "trackBar2";
            this.trackBar2.Size = new System.Drawing.Size(274, 45);
            this.trackBar2.TabIndex = 8;
            this.trackBar2.Value = 1;
            this.trackBar2.Scroll += new System.EventHandler(this.trackBar2_Scroll);
            // 
            // label3
            // 
            this.lb_length.AutoSize = true;
            this.lb_length.Font = new System.Drawing.Font("Segoe UI Semibold", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_length.Location = new System.Drawing.Point(317, 211);
            this.lb_length.Name = "label3";
            this.lb_length.Size = new System.Drawing.Size(28, 37);
            this.lb_length.TabIndex = 9;
            this.lb_length.Text = "1";
            // 
            // trackBar3
            // 
            this.trackBar3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.trackBar3.Location = new System.Drawing.Point(27, 211);
            this.trackBar3.Maximum = 20;
            this.trackBar3.Minimum = 1;
            this.trackBar3.Name = "trackBar3";
            this.trackBar3.Size = new System.Drawing.Size(274, 45);
            this.trackBar3.TabIndex = 10;
            this.trackBar3.Value = 1;
            this.trackBar3.Scroll += new System.EventHandler(this.trackBar3_Scroll);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.label4.Location = new System.Drawing.Point(18, 173);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(123, 28);
            this.label4.TabIndex = 11;
            this.label4.Text = "Length (cm)";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.label5.Location = new System.Drawing.Point(22, 250);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(216, 28);
            this.label5.TabIndex = 14;
            this.label5.Text = "Spring Diameter (mm)";
            // 
            // trackBar4
            // 
            this.trackBar4.Cursor = System.Windows.Forms.Cursors.Hand;
            this.trackBar4.Location = new System.Drawing.Point(27, 287);
            this.trackBar4.Maximum = 20;
            this.trackBar4.Minimum = 1;
            this.trackBar4.Name = "trackBar4";
            this.trackBar4.Size = new System.Drawing.Size(274, 45);
            this.trackBar4.TabIndex = 13;
            this.trackBar4.Value = 1;
            this.trackBar4.Scroll += new System.EventHandler(this.trackBar4_Scroll);
            // 
            // label6
            // 
            this.lb_springDiameter.AutoSize = true;
            this.lb_springDiameter.Font = new System.Drawing.Font("Segoe UI Semibold", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_springDiameter.Location = new System.Drawing.Point(317, 287);
            this.lb_springDiameter.Name = "label6";
            this.lb_springDiameter.Size = new System.Drawing.Size(28, 37);
            this.lb_springDiameter.TabIndex = 12;
            this.lb_springDiameter.Text = "1";
            // 
            // PropertyWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(374, 433);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.trackBar4);
            this.Controls.Add(this.lb_springDiameter);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.trackBar3);
            this.Controls.Add(this.lb_length);
            this.Controls.Add(this.trackBar2);
            this.Controls.Add(this.lb_pitchValue);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cb_clockwise);
            this.Controls.Add(this.cb_distribution);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lb_coilDiameter);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.btn_OK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "PropertyWindow";
            this.Text = "Spring Properties";
            this.Load += new System.EventHandler(this.PropertyWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar4)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Label lb_coilDiameter;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cb_distribution;
        private System.Windows.Forms.CheckBox cb_clockwise;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lb_pitchValue;
        private System.Windows.Forms.TrackBar trackBar2;
        private System.Windows.Forms.Label lb_length;
        private System.Windows.Forms.TrackBar trackBar3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TrackBar trackBar4;
        private System.Windows.Forms.Label lb_springDiameter;
    }
}