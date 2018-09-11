namespace PluginBar.UI
{
    partial class SpringPopUp2
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
            this.tb_force = new System.Windows.Forms.TextBox();
            this.tb_disp = new System.Windows.Forms.TextBox();
            this.slider_coilD = new System.Windows.Forms.TrackBar();
            this.slider_springD = new System.Windows.Forms.TrackBar();
            this.lb_force = new System.Windows.Forms.Label();
            this.lb_disp = new System.Windows.Forms.Label();
            this.lb_coilD = new System.Windows.Forms.Label();
            this.lb_springDd = new System.Windows.Forms.Label();
            this.lb_coilDVal = new System.Windows.Forms.Label();
            this.lb_springDVal = new System.Windows.Forms.Label();
            this.lb_turns = new System.Windows.Forms.Label();
            this.slider_turns = new System.Windows.Forms.TrackBar();
            this.lb_turnsVal = new System.Windows.Forms.Label();
            this.cb_curve = new System.Windows.Forms.CheckBox();
            this.button_OK = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.slider_coilD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.slider_springD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.slider_turns)).BeginInit();
            this.SuspendLayout();
            // 
            // tb_force
            // 
            this.tb_force.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tb_force.Location = new System.Drawing.Point(269, 28);
            this.tb_force.Name = "tb_force";
            this.tb_force.Size = new System.Drawing.Size(100, 30);
            this.tb_force.TabIndex = 0;
            this.tb_force.TextChanged += new System.EventHandler(this.tb_force_TextChanged);
            // 
            // tb_disp
            // 
            this.tb_disp.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tb_disp.Location = new System.Drawing.Point(270, 84);
            this.tb_disp.Name = "tb_disp";
            this.tb_disp.Size = new System.Drawing.Size(100, 30);
            this.tb_disp.TabIndex = 1;
            this.tb_disp.TextChanged += new System.EventHandler(this.tb_disp_TextChanged);
            // 
            // slider_coilD
            // 
            this.slider_coilD.LargeChange = 2;
            this.slider_coilD.Location = new System.Drawing.Point(25, 192);
            this.slider_coilD.Maximum = 5;
            this.slider_coilD.Minimum = 1;
            this.slider_coilD.Name = "slider_coilD";
            this.slider_coilD.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.slider_coilD.Size = new System.Drawing.Size(298, 56);
            this.slider_coilD.TabIndex = 2;
            this.slider_coilD.Value = 1;
            this.slider_coilD.Scroll += new System.EventHandler(this.slider_coilD_Scroll);
            // 
            // slider_springD
            // 
            this.slider_springD.LargeChange = 2;
            this.slider_springD.Location = new System.Drawing.Point(25, 275);
            this.slider_springD.Minimum = 1;
            this.slider_springD.Name = "slider_springD";
            this.slider_springD.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.slider_springD.Size = new System.Drawing.Size(298, 56);
            this.slider_springD.TabIndex = 3;
            this.slider_springD.Value = 1;
            this.slider_springD.Scroll += new System.EventHandler(this.slider_springD_Scroll);
            // 
            // lb_force
            // 
            this.lb_force.AutoSize = true;
            this.lb_force.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.lb_force.Location = new System.Drawing.Point(25, 23);
            this.lb_force.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lb_force.Name = "lb_force";
            this.lb_force.Size = new System.Drawing.Size(119, 35);
            this.lb_force.TabIndex = 5;
            this.lb_force.Text = "Force (N)";
            // 
            // lb_disp
            // 
            this.lb_disp.AutoSize = true;
            this.lb_disp.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.lb_disp.Location = new System.Drawing.Point(26, 79);
            this.lb_disp.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lb_disp.Name = "lb_disp";
            this.lb_disp.Size = new System.Drawing.Size(237, 35);
            this.lb_disp.TabIndex = 6;
            this.lb_disp.Text = "Displacement (mm)";
            // 
            // lb_coilD
            // 
            this.lb_coilD.AutoSize = true;
            this.lb_coilD.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.lb_coilD.Location = new System.Drawing.Point(25, 152);
            this.lb_coilD.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lb_coilD.Name = "lb_coilD";
            this.lb_coilD.Size = new System.Drawing.Size(238, 35);
            this.lb_coilD.TabIndex = 7;
            this.lb_coilD.Text = "Coil Diameter (mm)";
            // 
            // lb_springDd
            // 
            this.lb_springDd.AutoSize = true;
            this.lb_springDd.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.lb_springDd.Location = new System.Drawing.Point(25, 231);
            this.lb_springDd.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lb_springDd.Name = "lb_springDd";
            this.lb_springDd.Size = new System.Drawing.Size(268, 35);
            this.lb_springDd.TabIndex = 8;
            this.lb_springDd.Text = "Spring Diameter (mm)";
            // 
            // lb_coilDVal
            // 
            this.lb_coilDVal.AutoSize = true;
            this.lb_coilDVal.Font = new System.Drawing.Font("Segoe UI Semibold", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_coilDVal.Location = new System.Drawing.Point(343, 183);
            this.lb_coilDVal.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lb_coilDVal.Name = "lb_coilDVal";
            this.lb_coilDVal.Size = new System.Drawing.Size(30, 41);
            this.lb_coilDVal.TabIndex = 9;
            this.lb_coilDVal.Text = "1";
            // 
            // lb_springDVal
            // 
            this.lb_springDVal.AutoSize = true;
            this.lb_springDVal.Font = new System.Drawing.Font("Segoe UI Semibold", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_springDVal.Location = new System.Drawing.Point(343, 263);
            this.lb_springDVal.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lb_springDVal.Name = "lb_springDVal";
            this.lb_springDVal.Size = new System.Drawing.Size(30, 41);
            this.lb_springDVal.TabIndex = 10;
            this.lb_springDVal.Text = "1";
            // 
            // lb_turns
            // 
            this.lb_turns.AutoSize = true;
            this.lb_turns.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.lb_turns.Location = new System.Drawing.Point(25, 318);
            this.lb_turns.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lb_turns.Name = "lb_turns";
            this.lb_turns.Size = new System.Drawing.Size(208, 35);
            this.lb_turns.TabIndex = 11;
            this.lb_turns.Text = "Number of Turns";
            // 
            // slider_turns
            // 
            this.slider_turns.LargeChange = 2;
            this.slider_turns.Location = new System.Drawing.Point(25, 356);
            this.slider_turns.Minimum = 1;
            this.slider_turns.Name = "slider_turns";
            this.slider_turns.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.slider_turns.Size = new System.Drawing.Size(298, 56);
            this.slider_turns.TabIndex = 12;
            this.slider_turns.Value = 1;
            this.slider_turns.Scroll += new System.EventHandler(this.slider_turns_Scroll);
            // 
            // lb_turnsVal
            // 
            this.lb_turnsVal.AutoSize = true;
            this.lb_turnsVal.Font = new System.Drawing.Font("Segoe UI Semibold", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_turnsVal.Location = new System.Drawing.Point(343, 344);
            this.lb_turnsVal.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lb_turnsVal.Name = "lb_turnsVal";
            this.lb_turnsVal.Size = new System.Drawing.Size(30, 41);
            this.lb_turnsVal.TabIndex = 13;
            this.lb_turnsVal.Text = "1";
            // 
            // cb_curve
            // 
            this.cb_curve.AutoSize = true;
            this.cb_curve.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_curve.Location = new System.Drawing.Point(26, 418);
            this.cb_curve.Name = "cb_curve";
            this.cb_curve.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.cb_curve.Size = new System.Drawing.Size(195, 24);
            this.cb_curve.TabIndex = 14;
            this.cb_curve.Text = "?Along Existing Curve";
            this.cb_curve.UseVisualStyleBackColor = true;
            // 
            // button_OK
            // 
            this.button_OK.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.button_OK.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_OK.Location = new System.Drawing.Point(269, 406);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(110, 47);
            this.button_OK.TabIndex = 15;
            this.button_OK.Text = "OK";
            this.button_OK.UseVisualStyleBackColor = false;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // SpringPopUp2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.ClientSize = new System.Drawing.Size(394, 476);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.cb_curve);
            this.Controls.Add(this.lb_turnsVal);
            this.Controls.Add(this.slider_turns);
            this.Controls.Add(this.lb_turns);
            this.Controls.Add(this.lb_springDVal);
            this.Controls.Add(this.lb_coilDVal);
            this.Controls.Add(this.lb_springDd);
            this.Controls.Add(this.lb_coilD);
            this.Controls.Add(this.lb_disp);
            this.Controls.Add(this.lb_force);
            this.Controls.Add(this.slider_springD);
            this.Controls.Add(this.slider_coilD);
            this.Controls.Add(this.tb_disp);
            this.Controls.Add(this.tb_force);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SpringPopUp2";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Text = "Spring Characteristics";
            this.Load += new System.EventHandler(this.SpringPopUp2_Load);
            ((System.ComponentModel.ISupportInitialize)(this.slider_coilD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.slider_springD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.slider_turns)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tb_force;
        private System.Windows.Forms.TextBox tb_disp;
        private System.Windows.Forms.TrackBar slider_coilD;
        private System.Windows.Forms.TrackBar slider_springD;
        private System.Windows.Forms.Label lb_force;
        private System.Windows.Forms.Label lb_disp;
        private System.Windows.Forms.Label lb_coilD;
        private System.Windows.Forms.Label lb_springDd;
        private System.Windows.Forms.Label lb_coilDVal;
        private System.Windows.Forms.Label lb_springDVal;
        private System.Windows.Forms.Label lb_turns;
        private System.Windows.Forms.TrackBar slider_turns;
        private System.Windows.Forms.Label lb_turnsVal;
        private System.Windows.Forms.CheckBox cb_curve;
        private System.Windows.Forms.Button button_OK;
    }
}