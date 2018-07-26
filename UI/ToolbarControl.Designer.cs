namespace OndulePlugin
{
  partial class OnduleTopBarControl
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.SimulationLabel = new System.Windows.Forms.Label();
            this.SkeletonLabel = new System.Windows.Forms.Label();
            this.ExportLabel = new System.Windows.Forms.Label();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.ExportBtn = new System.Windows.Forms.Button();
            this.PreviewBtn = new System.Windows.Forms.Button();
            this.MATBtn = new System.Windows.Forms.Button();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.OnduleUnitFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.OnduleUnitsLabel = new System.Windows.Forms.Label();
            this.debugBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            this.SuspendLayout();
            // 
            // SimulationLabel
            // 
            this.SimulationLabel.AutoSize = true;
            this.SimulationLabel.BackColor = System.Drawing.Color.White;
            this.SimulationLabel.Location = new System.Drawing.Point(299, 10);
            this.SimulationLabel.Name = "SimulationLabel";
            this.SimulationLabel.Size = new System.Drawing.Size(112, 25);
            this.SimulationLabel.TabIndex = 63;
            this.SimulationLabel.Text = "Simulation";
            // 
            // SkeletonLabel
            // 
            this.SkeletonLabel.AutoSize = true;
            this.SkeletonLabel.BackColor = System.Drawing.Color.Transparent;
            this.SkeletonLabel.Location = new System.Drawing.Point(21, 10);
            this.SkeletonLabel.Name = "SkeletonLabel";
            this.SkeletonLabel.Size = new System.Drawing.Size(96, 25);
            this.SkeletonLabel.TabIndex = 71;
            this.SkeletonLabel.Text = "Skeleton";
            this.SkeletonLabel.Click += new System.EventHandler(this.label1_Click);
            // 
            // ExportLabel
            // 
            this.ExportLabel.AutoSize = true;
            this.ExportLabel.BackColor = System.Drawing.Color.White;
            this.ExportLabel.Location = new System.Drawing.Point(477, 10);
            this.ExportLabel.Name = "ExportLabel";
            this.ExportLabel.Size = new System.Drawing.Size(74, 25);
            this.ExportLabel.TabIndex = 75;
            this.ExportLabel.Text = "Export";
            // 
            // pictureBox4
            // 
            this.pictureBox4.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox4.Image = global::SlinkyBar.Properties.Resources.split_v;
            this.pictureBox4.InitialImage = global::SlinkyBar.Properties.Resources.split_v;
            this.pictureBox4.Location = new System.Drawing.Point(428, 10);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(2, 120);
            this.pictureBox4.TabIndex = 79;
            this.pictureBox4.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.Image = global::SlinkyBar.Properties.Resources.split_v;
            this.pictureBox1.InitialImage = global::SlinkyBar.Properties.Resources.split_v;
            this.pictureBox1.Location = new System.Drawing.Point(255, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(2, 120);
            this.pictureBox1.TabIndex = 76;
            this.pictureBox1.TabStop = false;
            // 
            // ExportBtn
            // 
            this.ExportBtn.BackColor = System.Drawing.Color.Transparent;
            this.ExportBtn.BackgroundImage = global::SlinkyBar.Properties.Resources.Print_default;
            this.ExportBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ExportBtn.ForeColor = System.Drawing.Color.Transparent;
            this.ExportBtn.Location = new System.Drawing.Point(470, 42);
            this.ExportBtn.Name = "ExportBtn";
            this.ExportBtn.Size = new System.Drawing.Size(82, 82);
            this.ExportBtn.TabIndex = 66;
            this.ExportBtn.UseVisualStyleBackColor = false;
            this.ExportBtn.Click += new System.EventHandler(this.ExportBtn_Click);
            // 
            // PreviewBtn
            // 
            this.PreviewBtn.BackColor = System.Drawing.Color.Transparent;
            this.PreviewBtn.BackgroundImage = global::SlinkyBar.Properties.Resources.Preview_default;
            this.PreviewBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PreviewBtn.ForeColor = System.Drawing.Color.Transparent;
            this.PreviewBtn.Location = new System.Drawing.Point(301, 42);
            this.PreviewBtn.Name = "PreviewBtn";
            this.PreviewBtn.Size = new System.Drawing.Size(82, 82);
            this.PreviewBtn.TabIndex = 64;
            this.PreviewBtn.UseVisualStyleBackColor = false;
            this.PreviewBtn.Click += new System.EventHandler(this.PreviewBtn_Click);
            // 
            // MATBtn
            // 
            this.MATBtn.BackColor = System.Drawing.Color.Transparent;
            this.MATBtn.BackgroundImage = global::SlinkyBar.Properties.Resources.MAT_defaut;
            this.MATBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.MATBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MATBtn.ForeColor = System.Drawing.Color.Transparent;
            this.MATBtn.Location = new System.Drawing.Point(26, 42);
            this.MATBtn.Name = "MATBtn";
            this.MATBtn.Size = new System.Drawing.Size(82, 82);
            this.MATBtn.TabIndex = 58;
            this.MATBtn.UseVisualStyleBackColor = false;
            this.MATBtn.Click += new System.EventHandler(this.MATButton_Click);
            this.MATBtn.MouseEnter += new System.EventHandler(this.MATButton_MouseEnter);
            this.MATBtn.MouseLeave += new System.EventHandler(this.MATButton_MouseLeave);
            this.MATBtn.MouseHover += new System.EventHandler(this.MATButton_MouseHover);
            // 
            // pictureBox5
            // 
            this.pictureBox5.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox5.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox5.Image = global::SlinkyBar.Properties.Resources.split_v;
            this.pictureBox5.InitialImage = global::SlinkyBar.Properties.Resources.split_v;
            this.pictureBox5.Location = new System.Drawing.Point(178, 4);
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.Size = new System.Drawing.Size(2, 120);
            this.pictureBox5.TabIndex = 80;
            this.pictureBox5.TabStop = false;
            // 
            // OnduleUnitFlowPanel
            // 
            this.OnduleUnitFlowPanel.AutoScroll = true;
            this.OnduleUnitFlowPanel.AutoSize = true;
            this.OnduleUnitFlowPanel.BackColor = System.Drawing.Color.Gainsboro;
            this.OnduleUnitFlowPanel.Location = new System.Drawing.Point(602, 36);
            this.OnduleUnitFlowPanel.Name = "OnduleUnitFlowPanel";
            this.OnduleUnitFlowPanel.Size = new System.Drawing.Size(596, 94);
            this.OnduleUnitFlowPanel.TabIndex = 85;
            // 
            // OnduleUnitsLabel
            // 
            this.OnduleUnitsLabel.AutoSize = true;
            this.OnduleUnitsLabel.BackColor = System.Drawing.Color.White;
            this.OnduleUnitsLabel.Location = new System.Drawing.Point(599, 8);
            this.OnduleUnitsLabel.Name = "OnduleUnitsLabel";
            this.OnduleUnitsLabel.Size = new System.Drawing.Size(136, 25);
            this.OnduleUnitsLabel.TabIndex = 86;
            this.OnduleUnitsLabel.Text = "Ondule Units";
            // 
            // debugBtn
            // 
            this.debugBtn.Location = new System.Drawing.Point(1559, 42);
            this.debugBtn.Name = "debugBtn";
            this.debugBtn.Size = new System.Drawing.Size(105, 33);
            this.debugBtn.TabIndex = 87;
            this.debugBtn.Text = "Debug";
            this.debugBtn.UseVisualStyleBackColor = true;
            this.debugBtn.Click += new System.EventHandler(this.debugBtn_Click);
            // 
            // OnduleTopBarControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.Controls.Add(this.debugBtn);
            this.Controls.Add(this.OnduleUnitsLabel);
            this.Controls.Add(this.OnduleUnitFlowPanel);
            this.Controls.Add(this.pictureBox5);
            this.Controls.Add(this.pictureBox4);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.ExportLabel);
            this.Controls.Add(this.SkeletonLabel);
            this.Controls.Add(this.ExportBtn);
            this.Controls.Add(this.PreviewBtn);
            this.Controls.Add(this.SimulationLabel);
            this.Controls.Add(this.MATBtn);
            this.Name = "OnduleTopBarControl";
            this.Size = new System.Drawing.Size(1685, 210);
            this.Style = MetroFramework.MetroColorStyle.White;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion
        private System.Windows.Forms.Label SimulationLabel;
        private System.Windows.Forms.Button PreviewBtn;
        private System.Windows.Forms.Button ExportBtn;
        private System.Windows.Forms.Button MATBtn;
        private System.Windows.Forms.Label SkeletonLabel;
        private System.Windows.Forms.Label ExportLabel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.PictureBox pictureBox5;
        private System.Windows.Forms.FlowLayoutPanel OnduleUnitFlowPanel;
        private System.Windows.Forms.Label OnduleUnitsLabel;
        private System.Windows.Forms.Button debugBtn;
    }
}
