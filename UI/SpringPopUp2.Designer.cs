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
            this.slider_coilD = new System.Windows.Forms.TrackBar();
            this.slider_springD = new System.Windows.Forms.TrackBar();
            this.lb_coilD = new System.Windows.Forms.Label();
            this.lb_springDd = new System.Windows.Forms.Label();
            this.lb_coilDVal = new System.Windows.Forms.Label();
            this.lb_springDVal = new System.Windows.Forms.Label();
            this.lb_turns = new System.Windows.Forms.Label();
            this.slider_turns = new System.Windows.Forms.TrackBar();
            this.lb_turnsVal = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.lb_stiffness = new System.Windows.Forms.Label();
            this.slider_stiffness = new System.Windows.Forms.TrackBar();
            this.lb_stiffnessVal = new System.Windows.Forms.Label();
            this.lb_slendernessVal = new System.Windows.Forms.Label();
            this.lb_slenderness = new System.Windows.Forms.Label();
            this.slider_slenderness = new System.Windows.Forms.TrackBar();
            this.lb_indexVal = new System.Windows.Forms.Label();
            this.lb_index = new System.Windows.Forms.Label();
            this.slider_index = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.slider_coilD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.slider_springD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.slider_turns)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.slider_stiffness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.slider_slenderness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.slider_index)).BeginInit();
            this.SuspendLayout();
            // 
            // slider_coilD
            // 
            this.slider_coilD.LargeChange = 2;
            this.slider_coilD.Location = new System.Drawing.Point(31, 59);
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
            this.slider_springD.Location = new System.Drawing.Point(31, 142);
            this.slider_springD.Minimum = 1;
            this.slider_springD.Name = "slider_springD";
            this.slider_springD.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.slider_springD.Size = new System.Drawing.Size(298, 56);
            this.slider_springD.TabIndex = 3;
            this.slider_springD.Value = 3;
            this.slider_springD.Scroll += new System.EventHandler(this.slider_springD_Scroll);
            // 
            // lb_coilD
            // 
            this.lb_coilD.AutoSize = true;
            this.lb_coilD.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.lb_coilD.Location = new System.Drawing.Point(25, 19);
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
            this.lb_springDd.Location = new System.Drawing.Point(31, 98);
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
            this.lb_coilDVal.Location = new System.Drawing.Point(349, 50);
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
            this.lb_springDVal.Location = new System.Drawing.Point(349, 130);
            this.lb_springDVal.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lb_springDVal.Name = "lb_springDVal";
            this.lb_springDVal.Size = new System.Drawing.Size(35, 41);
            this.lb_springDVal.TabIndex = 10;
            this.lb_springDVal.Text = "3";
            // 
            // lb_turns
            // 
            this.lb_turns.AutoSize = true;
            this.lb_turns.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.lb_turns.Location = new System.Drawing.Point(31, 185);
            this.lb_turns.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lb_turns.Name = "lb_turns";
            this.lb_turns.Size = new System.Drawing.Size(208, 35);
            this.lb_turns.TabIndex = 11;
            this.lb_turns.Text = "Number of Turns";
            // 
            // slider_turns
            // 
            this.slider_turns.LargeChange = 2;
            this.slider_turns.Location = new System.Drawing.Point(31, 223);
            this.slider_turns.Minimum = 1;
            this.slider_turns.Name = "slider_turns";
            this.slider_turns.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.slider_turns.Size = new System.Drawing.Size(298, 56);
            this.slider_turns.TabIndex = 12;
            this.slider_turns.Value = 3;
            this.slider_turns.Scroll += new System.EventHandler(this.slider_turns_Scroll);
            // 
            // lb_turnsVal
            // 
            this.lb_turnsVal.AutoSize = true;
            this.lb_turnsVal.Font = new System.Drawing.Font("Segoe UI Semibold", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_turnsVal.Location = new System.Drawing.Point(349, 211);
            this.lb_turnsVal.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lb_turnsVal.Name = "lb_turnsVal";
            this.lb_turnsVal.Size = new System.Drawing.Size(35, 41);
            this.lb_turnsVal.TabIndex = 13;
            this.lb_turnsVal.Text = "3";
            // 
            // button_OK
            // 
            this.button_OK.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.button_OK.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_OK.Location = new System.Drawing.Point(334, 520);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(110, 47);
            this.button_OK.TabIndex = 15;
            this.button_OK.Text = "OK";
            this.button_OK.UseVisualStyleBackColor = false;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // lb_stiffness
            // 
            this.lb_stiffness.AutoSize = true;
            this.lb_stiffness.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.lb_stiffness.Location = new System.Drawing.Point(31, 267);
            this.lb_stiffness.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lb_stiffness.Name = "lb_stiffness";
            this.lb_stiffness.Size = new System.Drawing.Size(209, 35);
            this.lb_stiffness.TabIndex = 16;
            this.lb_stiffness.Text = "Relative Stiffness";
            // 
            // slider_stiffness
            // 
            this.slider_stiffness.LargeChange = 2;
            this.slider_stiffness.Location = new System.Drawing.Point(37, 305);
            this.slider_stiffness.Maximum = 5;
            this.slider_stiffness.Minimum = 1;
            this.slider_stiffness.Name = "slider_stiffness";
            this.slider_stiffness.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.slider_stiffness.Size = new System.Drawing.Size(298, 56);
            this.slider_stiffness.TabIndex = 17;
            this.slider_stiffness.Value = 3;
            this.slider_stiffness.Scroll += new System.EventHandler(this.slider_stiffness_Scroll);
            // 
            // lb_stiffnessVal
            // 
            this.lb_stiffnessVal.AutoSize = true;
            this.lb_stiffnessVal.Font = new System.Drawing.Font("Segoe UI Semibold", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_stiffnessVal.Location = new System.Drawing.Point(349, 294);
            this.lb_stiffnessVal.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lb_stiffnessVal.Name = "lb_stiffnessVal";
            this.lb_stiffnessVal.Size = new System.Drawing.Size(35, 41);
            this.lb_stiffnessVal.TabIndex = 18;
            this.lb_stiffnessVal.Text = "3";
            // 
            // lb_slendernessVal
            // 
            this.lb_slendernessVal.AutoSize = true;
            this.lb_slendernessVal.Font = new System.Drawing.Font("Segoe UI Semibold", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_slendernessVal.Location = new System.Drawing.Point(349, 378);
            this.lb_slendernessVal.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lb_slendernessVal.Name = "lb_slendernessVal";
            this.lb_slendernessVal.Size = new System.Drawing.Size(35, 41);
            this.lb_slendernessVal.TabIndex = 21;
            this.lb_slendernessVal.Text = "3";
            // 
            // lb_slenderness
            // 
            this.lb_slenderness.AutoSize = true;
            this.lb_slenderness.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.lb_slenderness.Location = new System.Drawing.Point(31, 346);
            this.lb_slenderness.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lb_slenderness.Name = "lb_slenderness";
            this.lb_slenderness.Size = new System.Drawing.Size(218, 35);
            this.lb_slenderness.TabIndex = 20;
            this.lb_slenderness.Text = "Slenderness Ratio";
            // 
            // slider_slenderness
            // 
            this.slider_slenderness.LargeChange = 2;
            this.slider_slenderness.Location = new System.Drawing.Point(31, 390);
            this.slider_slenderness.Maximum = 5;
            this.slider_slenderness.Minimum = 1;
            this.slider_slenderness.Name = "slider_slenderness";
            this.slider_slenderness.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.slider_slenderness.Size = new System.Drawing.Size(298, 56);
            this.slider_slenderness.TabIndex = 19;
            this.slider_slenderness.Value = 3;
            this.slider_slenderness.Scroll += new System.EventHandler(this.slider_slenderness_Scroll);
            // 
            // lb_indexVal
            // 
            this.lb_indexVal.AutoSize = true;
            this.lb_indexVal.Font = new System.Drawing.Font("Segoe UI Semibold", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_indexVal.Location = new System.Drawing.Point(349, 466);
            this.lb_indexVal.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lb_indexVal.Name = "lb_indexVal";
            this.lb_indexVal.Size = new System.Drawing.Size(35, 41);
            this.lb_indexVal.TabIndex = 24;
            this.lb_indexVal.Text = "9";
            // 
            // lb_index
            // 
            this.lb_index.AutoSize = true;
            this.lb_index.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.lb_index.Location = new System.Drawing.Point(31, 434);
            this.lb_index.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lb_index.Name = "lb_index";
            this.lb_index.Size = new System.Drawing.Size(160, 35);
            this.lb_index.TabIndex = 23;
            this.lb_index.Text = "Spring Index";
            // 
            // slider_index
            // 
            this.slider_index.LargeChange = 4;
            this.slider_index.Location = new System.Drawing.Point(31, 478);
            this.slider_index.Maximum = 24;
            this.slider_index.Minimum = 12;
            this.slider_index.Name = "slider_index";
            this.slider_index.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.slider_index.Size = new System.Drawing.Size(298, 56);
            this.slider_index.SmallChange = 2;
            this.slider_index.TabIndex = 22;
            this.slider_index.Value = 18;
            this.slider_index.Scroll += new System.EventHandler(this.slider_index_Scroll);
            // 
            // SpringPopUp2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.ClientSize = new System.Drawing.Size(456, 579);
            this.Controls.Add(this.lb_indexVal);
            this.Controls.Add(this.lb_index);
            this.Controls.Add(this.slider_index);
            this.Controls.Add(this.lb_slendernessVal);
            this.Controls.Add(this.lb_slenderness);
            this.Controls.Add(this.slider_slenderness);
            this.Controls.Add(this.lb_stiffnessVal);
            this.Controls.Add(this.slider_stiffness);
            this.Controls.Add(this.lb_stiffness);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.lb_turnsVal);
            this.Controls.Add(this.slider_turns);
            this.Controls.Add(this.lb_turns);
            this.Controls.Add(this.lb_springDVal);
            this.Controls.Add(this.lb_coilDVal);
            this.Controls.Add(this.lb_springDd);
            this.Controls.Add(this.lb_coilD);
            this.Controls.Add(this.slider_springD);
            this.Controls.Add(this.slider_coilD);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SpringPopUp2";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Text = "Spring Characteristics";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.SpringPopUp2_Load);
            ((System.ComponentModel.ISupportInitialize)(this.slider_coilD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.slider_springD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.slider_turns)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.slider_stiffness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.slider_slenderness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.slider_index)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar slider_coilD;
        private System.Windows.Forms.TrackBar slider_springD;
        private System.Windows.Forms.Label lb_coilD;
        private System.Windows.Forms.Label lb_springDd;
        private System.Windows.Forms.Label lb_coilDVal;
        private System.Windows.Forms.Label lb_springDVal;
        private System.Windows.Forms.Label lb_turns;
        private System.Windows.Forms.TrackBar slider_turns;
        private System.Windows.Forms.Label lb_turnsVal;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Label lb_stiffness;
        private System.Windows.Forms.TrackBar slider_stiffness;
        private System.Windows.Forms.Label lb_stiffnessVal;
        private System.Windows.Forms.Label lb_slendernessVal;
        private System.Windows.Forms.Label lb_slenderness;
        private System.Windows.Forms.TrackBar slider_slenderness;
        private System.Windows.Forms.Label lb_indexVal;
        private System.Windows.Forms.Label lb_index;
        private System.Windows.Forms.TrackBar slider_index;
    }
}