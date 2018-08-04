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
            this.OnduleUnitFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.OnduleUnitsLabel = new System.Windows.Forms.Label();
            this.debugBtn = new System.Windows.Forms.Button();
            this.ConversionBtn = new System.Windows.Forms.Button();
            this.ExportBtn = new System.Windows.Forms.Button();
            this.SimulationBtn = new System.Windows.Forms.Button();
            this.SegmentationBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // OnduleUnitFlowPanel
            // 
            this.OnduleUnitFlowPanel.AutoScroll = true;
            this.OnduleUnitFlowPanel.AutoSize = true;
            this.OnduleUnitFlowPanel.BackColor = System.Drawing.Color.Gainsboro;
            this.OnduleUnitFlowPanel.Location = new System.Drawing.Point(14, 117);
            this.OnduleUnitFlowPanel.Name = "OnduleUnitFlowPanel";
            this.OnduleUnitFlowPanel.Size = new System.Drawing.Size(371, 53);
            this.OnduleUnitFlowPanel.TabIndex = 85;
            // 
            // OnduleUnitsLabel
            // 
            this.OnduleUnitsLabel.AutoSize = true;
            this.OnduleUnitsLabel.BackColor = System.Drawing.Color.White;
            this.OnduleUnitsLabel.Location = new System.Drawing.Point(18, 78);
            this.OnduleUnitsLabel.Name = "OnduleUnitsLabel";
            this.OnduleUnitsLabel.Size = new System.Drawing.Size(249, 25);
            this.OnduleUnitsLabel.TabIndex = 86;
            this.OnduleUnitsLabel.Text = "Generated Ondule Units:";
            // 
            // debugBtn
            // 
            this.debugBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.debugBtn.ForeColor = System.Drawing.Color.White;
            this.debugBtn.Location = new System.Drawing.Point(269, 675);
            this.debugBtn.Name = "debugBtn";
            this.debugBtn.Size = new System.Drawing.Size(105, 42);
            this.debugBtn.TabIndex = 87;
            this.debugBtn.Text = "Debug";
            this.debugBtn.UseVisualStyleBackColor = false;
            this.debugBtn.Click += new System.EventHandler(this.debugBtn_Click);
            // 
            // ConversionBtn
            // 
            this.ConversionBtn.Location = new System.Drawing.Point(14, 23);
            this.ConversionBtn.Name = "ConversionBtn";
            this.ConversionBtn.Size = new System.Drawing.Size(192, 40);
            this.ConversionBtn.TabIndex = 88;
            this.ConversionBtn.Text = "Convert to Spring";
            this.ConversionBtn.UseVisualStyleBackColor = true;
            this.ConversionBtn.Click += new System.EventHandler(this.ConversionBtn_Click);
            // 
            // ExportBtn
            // 
            this.ExportBtn.Location = new System.Drawing.Point(151, 680);
            this.ExportBtn.Name = "ExportBtn";
            this.ExportBtn.Size = new System.Drawing.Size(70, 33);
            this.ExportBtn.TabIndex = 89;
            this.ExportBtn.Text = "Print";
            this.ExportBtn.UseVisualStyleBackColor = true;
            this.ExportBtn.Click += new System.EventHandler(this.ExportBtn_Click);
            // 
            // SimulationBtn
            // 
            this.SimulationBtn.Location = new System.Drawing.Point(23, 680);
            this.SimulationBtn.Name = "SimulationBtn";
            this.SimulationBtn.Size = new System.Drawing.Size(122, 33);
            this.SimulationBtn.TabIndex = 90;
            this.SimulationBtn.Text = "Simulation";
            this.SimulationBtn.UseVisualStyleBackColor = true;
            this.SimulationBtn.Click += new System.EventHandler(this.SimulationBtn_Click);
            // 
            // SegmentationBtn
            // 
            this.SegmentationBtn.Location = new System.Drawing.Point(212, 23);
            this.SegmentationBtn.Name = "SegmentationBtn";
            this.SegmentationBtn.Size = new System.Drawing.Size(173, 40);
            this.SegmentationBtn.TabIndex = 92;
            this.SegmentationBtn.Text = "Segmentation";
            this.SegmentationBtn.UseVisualStyleBackColor = true;
            this.SegmentationBtn.Click += new System.EventHandler(this.SegmentationBtn_Click);
            // 
            // OnduleTopBarControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.Controls.Add(this.SegmentationBtn);
            this.Controls.Add(this.SimulationBtn);
            this.Controls.Add(this.ExportBtn);
            this.Controls.Add(this.ConversionBtn);
            this.Controls.Add(this.debugBtn);
            this.Controls.Add(this.OnduleUnitsLabel);
            this.Controls.Add(this.OnduleUnitFlowPanel);
            this.Name = "OnduleTopBarControl";
            this.Size = new System.Drawing.Size(400, 730);
            this.Style = MetroFramework.MetroColorStyle.White;
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnduleTopBarControl_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion
        private System.Windows.Forms.FlowLayoutPanel OnduleUnitFlowPanel;
        private System.Windows.Forms.Label OnduleUnitsLabel;
        private System.Windows.Forms.Button debugBtn;
        private System.Windows.Forms.Button ConversionBtn;
        private System.Windows.Forms.Button ExportBtn;
        private System.Windows.Forms.Button SimulationBtn;
        private System.Windows.Forms.Button SegmentationBtn;
    }
}
