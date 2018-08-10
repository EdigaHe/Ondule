﻿namespace OndulePlugin
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
            this.SplitterBottomMenu = new System.Windows.Forms.Splitter();
            this.OnduleSpringGenerationTitleLabel = new System.Windows.Forms.Label();
            this.splitter4 = new System.Windows.Forms.Splitter();
            this.SplitterSpringGeneration = new System.Windows.Forms.Splitter();
            this.SplitterPreparation = new System.Windows.Forms.Splitter();
            this.SplitterConstraints = new System.Windows.Forms.Splitter();
            this.OnduleConstraintCheckbox = new System.Windows.Forms.CheckBox();
            this.linearConstraintCanvas = new System.Windows.Forms.Panel();
            this.LinearConstraintRadioButton = new System.Windows.Forms.RadioButton();
            this.TwistConstraintRadioButton = new System.Windows.Forms.RadioButton();
            this.BendConstraintRadioButton = new System.Windows.Forms.RadioButton();
            this.LinearTwistConstraintRadioButton = new System.Windows.Forms.RadioButton();
            this.AllDirectionsCheckBox = new System.Windows.Forms.CheckBox();
            this.ClothBox = new System.Windows.Forms.CheckBox();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.StiffnessRadioButton = new System.Windows.Forms.RadioButton();
            this.AdvancedRadioButton = new System.Windows.Forms.RadioButton();
            this.controlPanel = new System.Windows.Forms.Panel();
            this.TGValueLabel = new System.Windows.Forms.Label();
            this.WDValueLabel = new System.Windows.Forms.Label();
            this.MinTGLabel = new System.Windows.Forms.Label();
            this.MinWDLabel = new System.Windows.Forms.Label();
            this.MaxTGLabel = new System.Windows.Forms.Label();
            this.MaxWDLabel = new System.Windows.Forms.Label();
            this.TurnGapTrackBar = new System.Windows.Forms.TrackBar();
            this.TurnGapTitleLabel = new System.Windows.Forms.Label();
            this.WDTitleLabel = new System.Windows.Forms.Label();
            this.MaxStiffnessLabel = new System.Windows.Forms.Label();
            this.MinStiffnessLabel = new System.Windows.Forms.Label();
            this.StiffnessTrackBar = new System.Windows.Forms.TrackBar();
            this.WireDiameterTrackBar = new System.Windows.Forms.TrackBar();
            this.panel1 = new System.Windows.Forms.Panel();
            this.controlPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TurnGapTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.StiffnessTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.WireDiameterTrackBar)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // OnduleUnitFlowPanel
            // 
            this.OnduleUnitFlowPanel.AutoScroll = true;
            this.OnduleUnitFlowPanel.BackColor = System.Drawing.Color.White;
            this.OnduleUnitFlowPanel.Location = new System.Drawing.Point(12, 81);
            this.OnduleUnitFlowPanel.Name = "OnduleUnitFlowPanel";
            this.OnduleUnitFlowPanel.Size = new System.Drawing.Size(340, 40);
            this.OnduleUnitFlowPanel.TabIndex = 85;
            this.OnduleUnitFlowPanel.WrapContents = false;
            // 
            // OnduleUnitsLabel
            // 
            this.OnduleUnitsLabel.AutoSize = true;
            this.OnduleUnitsLabel.BackColor = System.Drawing.Color.Gray;
            this.OnduleUnitsLabel.ForeColor = System.Drawing.Color.White;
            this.OnduleUnitsLabel.Location = new System.Drawing.Point(10, 59);
            this.OnduleUnitsLabel.Name = "OnduleUnitsLabel";
            this.OnduleUnitsLabel.Size = new System.Drawing.Size(249, 25);
            this.OnduleUnitsLabel.TabIndex = 86;
            this.OnduleUnitsLabel.Text = "Generated Ondule Units:";
            // 
            // debugBtn
            // 
            this.debugBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.debugBtn.ForeColor = System.Drawing.Color.White;
            this.debugBtn.Location = new System.Drawing.Point(233, 556);
            this.debugBtn.Name = "debugBtn";
            this.debugBtn.Size = new System.Drawing.Size(120, 47);
            this.debugBtn.TabIndex = 87;
            this.debugBtn.Text = "Debug";
            this.debugBtn.UseVisualStyleBackColor = false;
            this.debugBtn.Click += new System.EventHandler(this.debugBtn_Click);
            // 
            // ConversionBtn
            // 
            this.ConversionBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ConversionBtn.ForeColor = System.Drawing.Color.White;
            this.ConversionBtn.Location = new System.Drawing.Point(10, 9);
            this.ConversionBtn.Name = "ConversionBtn";
            this.ConversionBtn.Size = new System.Drawing.Size(170, 45);
            this.ConversionBtn.TabIndex = 88;
            this.ConversionBtn.Text = "Convert to Spring";
            this.ConversionBtn.UseVisualStyleBackColor = false;
            this.ConversionBtn.Click += new System.EventHandler(this.ConversionBtn_Click);
            // 
            // ExportBtn
            // 
            this.ExportBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ExportBtn.ForeColor = System.Drawing.Color.White;
            this.ExportBtn.Location = new System.Drawing.Point(120, 557);
            this.ExportBtn.Name = "ExportBtn";
            this.ExportBtn.Size = new System.Drawing.Size(80, 47);
            this.ExportBtn.TabIndex = 89;
            this.ExportBtn.Text = "Print";
            this.ExportBtn.UseVisualStyleBackColor = false;
            this.ExportBtn.Click += new System.EventHandler(this.ExportBtn_Click);
            // 
            // SimulationBtn
            // 
            this.SimulationBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.SimulationBtn.ForeColor = System.Drawing.Color.White;
            this.SimulationBtn.Location = new System.Drawing.Point(11, 557);
            this.SimulationBtn.Name = "SimulationBtn";
            this.SimulationBtn.Size = new System.Drawing.Size(104, 47);
            this.SimulationBtn.TabIndex = 90;
            this.SimulationBtn.Text = "Simulation";
            this.SimulationBtn.UseVisualStyleBackColor = false;
            this.SimulationBtn.Click += new System.EventHandler(this.SimulationBtn_Click);
            // 
            // SegmentationBtn
            // 
            this.SegmentationBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.SegmentationBtn.ForeColor = System.Drawing.Color.White;
            this.SegmentationBtn.Location = new System.Drawing.Point(184, 9);
            this.SegmentationBtn.Name = "SegmentationBtn";
            this.SegmentationBtn.Size = new System.Drawing.Size(170, 45);
            this.SegmentationBtn.TabIndex = 92;
            this.SegmentationBtn.Text = "Segmentation";
            this.SegmentationBtn.UseVisualStyleBackColor = false;
            this.SegmentationBtn.Click += new System.EventHandler(this.SegmentationBtn_Click);
            // 
            // SplitterBottomMenu
            // 
            this.SplitterBottomMenu.BackColor = System.Drawing.Color.Gray;
            this.SplitterBottomMenu.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.SplitterBottomMenu.Enabled = false;
            this.SplitterBottomMenu.Location = new System.Drawing.Point(0, 547);
            this.SplitterBottomMenu.Name = "SplitterBottomMenu";
            this.SplitterBottomMenu.Size = new System.Drawing.Size(368, 71);
            this.SplitterBottomMenu.TabIndex = 90;
            this.SplitterBottomMenu.TabStop = false;
            // 
            // OnduleSpringGenerationTitleLabel
            // 
            this.OnduleSpringGenerationTitleLabel.AutoSize = true;
            this.OnduleSpringGenerationTitleLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.OnduleSpringGenerationTitleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OnduleSpringGenerationTitleLabel.ForeColor = System.Drawing.Color.White;
            this.OnduleSpringGenerationTitleLabel.Location = new System.Drawing.Point(8, 140);
            this.OnduleSpringGenerationTitleLabel.Name = "OnduleSpringGenerationTitleLabel";
            this.OnduleSpringGenerationTitleLabel.Size = new System.Drawing.Size(318, 29);
            this.OnduleSpringGenerationTitleLabel.TabIndex = 97;
            this.OnduleSpringGenerationTitleLabel.Text = "Ondule Spring Generation";
            // 
            // splitter4
            // 
            this.splitter4.BackColor = System.Drawing.Color.White;
            this.splitter4.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter4.Enabled = false;
            this.splitter4.Location = new System.Drawing.Point(0, 164);
            this.splitter4.Name = "splitter4";
            this.splitter4.Size = new System.Drawing.Size(368, 157);
            this.splitter4.TabIndex = 98;
            this.splitter4.TabStop = false;
            // 
            // SplitterSpringGeneration
            // 
            this.SplitterSpringGeneration.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.SplitterSpringGeneration.Dock = System.Windows.Forms.DockStyle.Top;
            this.SplitterSpringGeneration.Enabled = false;
            this.SplitterSpringGeneration.Location = new System.Drawing.Point(0, 134);
            this.SplitterSpringGeneration.Name = "SplitterSpringGeneration";
            this.SplitterSpringGeneration.Size = new System.Drawing.Size(368, 30);
            this.SplitterSpringGeneration.TabIndex = 99;
            this.SplitterSpringGeneration.TabStop = false;
            // 
            // SplitterPreparation
            // 
            this.SplitterPreparation.BackColor = System.Drawing.Color.Gray;
            this.SplitterPreparation.Dock = System.Windows.Forms.DockStyle.Top;
            this.SplitterPreparation.Enabled = false;
            this.SplitterPreparation.Location = new System.Drawing.Point(0, 0);
            this.SplitterPreparation.Name = "SplitterPreparation";
            this.SplitterPreparation.Size = new System.Drawing.Size(368, 134);
            this.SplitterPreparation.TabIndex = 100;
            this.SplitterPreparation.TabStop = false;
            // 
            // SplitterConstraints
            // 
            this.SplitterConstraints.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.SplitterConstraints.Dock = System.Windows.Forms.DockStyle.Top;
            this.SplitterConstraints.Enabled = false;
            this.SplitterConstraints.Location = new System.Drawing.Point(0, 321);
            this.SplitterConstraints.Name = "SplitterConstraints";
            this.SplitterConstraints.Size = new System.Drawing.Size(368, 26);
            this.SplitterConstraints.TabIndex = 101;
            this.SplitterConstraints.TabStop = false;
            // 
            // OnduleConstraintCheckbox
            // 
            this.OnduleConstraintCheckbox.AutoSize = true;
            this.OnduleConstraintCheckbox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.OnduleConstraintCheckbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OnduleConstraintCheckbox.ForeColor = System.Drawing.Color.White;
            this.OnduleConstraintCheckbox.Location = new System.Drawing.Point(11, 324);
            this.OnduleConstraintCheckbox.Name = "OnduleConstraintCheckbox";
            this.OnduleConstraintCheckbox.Size = new System.Drawing.Size(268, 33);
            this.OnduleConstraintCheckbox.TabIndex = 102;
            this.OnduleConstraintCheckbox.Text = "Ondule Constraints";
            this.OnduleConstraintCheckbox.UseVisualStyleBackColor = false;
            // 
            // linearConstraintCanvas
            // 
            this.linearConstraintCanvas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.linearConstraintCanvas.Location = new System.Drawing.Point(131, 357);
            this.linearConstraintCanvas.Name = "linearConstraintCanvas";
            this.linearConstraintCanvas.Size = new System.Drawing.Size(222, 152);
            this.linearConstraintCanvas.TabIndex = 114;
            // 
            // LinearConstraintRadioButton
            // 
            this.LinearConstraintRadioButton.AutoSize = true;
            this.LinearConstraintRadioButton.BackColor = System.Drawing.Color.White;
            this.LinearConstraintRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LinearConstraintRadioButton.Location = new System.Drawing.Point(11, 357);
            this.LinearConstraintRadioButton.Name = "LinearConstraintRadioButton";
            this.LinearConstraintRadioButton.Size = new System.Drawing.Size(143, 29);
            this.LinearConstraintRadioButton.TabIndex = 117;
            this.LinearConstraintRadioButton.Text = "Linear Only";
            this.LinearConstraintRadioButton.UseVisualStyleBackColor = false;
            // 
            // TwistConstraintRadioButton
            // 
            this.TwistConstraintRadioButton.AutoSize = true;
            this.TwistConstraintRadioButton.BackColor = System.Drawing.Color.White;
            this.TwistConstraintRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TwistConstraintRadioButton.Location = new System.Drawing.Point(11, 387);
            this.TwistConstraintRadioButton.Name = "TwistConstraintRadioButton";
            this.TwistConstraintRadioButton.Size = new System.Drawing.Size(135, 29);
            this.TwistConstraintRadioButton.TabIndex = 118;
            this.TwistConstraintRadioButton.Text = "Twist Only";
            this.TwistConstraintRadioButton.UseVisualStyleBackColor = false;
            // 
            // BendConstraintRadioButton
            // 
            this.BendConstraintRadioButton.AutoSize = true;
            this.BendConstraintRadioButton.BackColor = System.Drawing.Color.White;
            this.BendConstraintRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BendConstraintRadioButton.Location = new System.Drawing.Point(11, 448);
            this.BendConstraintRadioButton.Name = "BendConstraintRadioButton";
            this.BendConstraintRadioButton.Size = new System.Drawing.Size(135, 29);
            this.BendConstraintRadioButton.TabIndex = 119;
            this.BendConstraintRadioButton.Text = "Bend Only";
            this.BendConstraintRadioButton.UseVisualStyleBackColor = false;
            // 
            // LinearTwistConstraintRadioButton
            // 
            this.LinearTwistConstraintRadioButton.AutoSize = true;
            this.LinearTwistConstraintRadioButton.BackColor = System.Drawing.Color.White;
            this.LinearTwistConstraintRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LinearTwistConstraintRadioButton.Location = new System.Drawing.Point(11, 417);
            this.LinearTwistConstraintRadioButton.Name = "LinearTwistConstraintRadioButton";
            this.LinearTwistConstraintRadioButton.Size = new System.Drawing.Size(165, 29);
            this.LinearTwistConstraintRadioButton.TabIndex = 120;
            this.LinearTwistConstraintRadioButton.Text = "Linear + Twist";
            this.LinearTwistConstraintRadioButton.UseVisualStyleBackColor = false;
            // 
            // AllDirectionsCheckBox
            // 
            this.AllDirectionsCheckBox.AutoSize = true;
            this.AllDirectionsCheckBox.BackColor = System.Drawing.Color.White;
            this.AllDirectionsCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AllDirectionsCheckBox.Location = new System.Drawing.Point(33, 480);
            this.AllDirectionsCheckBox.Name = "AllDirectionsCheckBox";
            this.AllDirectionsCheckBox.Size = new System.Drawing.Size(157, 29);
            this.AllDirectionsCheckBox.TabIndex = 121;
            this.AllDirectionsCheckBox.Text = "All Directions";
            this.AllDirectionsCheckBox.UseVisualStyleBackColor = false;
            // 
            // ClothBox
            // 
            this.ClothBox.AutoSize = true;
            this.ClothBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClothBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ClothBox.ForeColor = System.Drawing.Color.White;
            this.ClothBox.Location = new System.Drawing.Point(11, 521);
            this.ClothBox.Name = "ClothBox";
            this.ClothBox.Size = new System.Drawing.Size(382, 33);
            this.ClothBox.TabIndex = 123;
            this.ClothBox.Text = "Show decorative spring cloth";
            this.ClothBox.UseVisualStyleBackColor = false;
            this.ClothBox.CheckedChanged += new System.EventHandler(this.ClothBox_CheckedChanged);
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter1.Enabled = false;
            this.splitter1.Location = new System.Drawing.Point(0, 522);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(368, 25);
            this.splitter1.TabIndex = 124;
            this.splitter1.TabStop = false;
            // 
            // StiffnessRadioButton
            // 
            this.StiffnessRadioButton.AutoSize = true;
            this.StiffnessRadioButton.BackColor = System.Drawing.Color.White;
            this.StiffnessRadioButton.Checked = true;
            this.StiffnessRadioButton.Location = new System.Drawing.Point(2, -2);
            this.StiffnessRadioButton.Name = "StiffnessRadioButton";
            this.StiffnessRadioButton.Size = new System.Drawing.Size(253, 29);
            this.StiffnessRadioButton.TabIndex = 125;
            this.StiffnessRadioButton.TabStop = true;
            this.StiffnessRadioButton.Text = "Basic stiffness control";
            this.StiffnessRadioButton.UseVisualStyleBackColor = false;
            this.StiffnessRadioButton.CheckedChanged += new System.EventHandler(this.StiffnessRadioButton_CheckedChanged);
            // 
            // AdvancedRadioButton
            // 
            this.AdvancedRadioButton.AutoSize = true;
            this.AdvancedRadioButton.BackColor = System.Drawing.Color.White;
            this.AdvancedRadioButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.AdvancedRadioButton.Location = new System.Drawing.Point(2, 21);
            this.AdvancedRadioButton.Name = "AdvancedRadioButton";
            this.AdvancedRadioButton.Size = new System.Drawing.Size(378, 29);
            this.AdvancedRadioButton.TabIndex = 126;
            this.AdvancedRadioButton.Text = "Advanced spring parameter control";
            this.AdvancedRadioButton.UseVisualStyleBackColor = false;
            this.AdvancedRadioButton.CheckedChanged += new System.EventHandler(this.AdvancedRadioButton_CheckedChanged);
            // 
            // controlPanel
            // 
            this.controlPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.controlPanel.Controls.Add(this.TGValueLabel);
            this.controlPanel.Controls.Add(this.WDValueLabel);
            this.controlPanel.Controls.Add(this.MinTGLabel);
            this.controlPanel.Controls.Add(this.MinWDLabel);
            this.controlPanel.Controls.Add(this.MaxTGLabel);
            this.controlPanel.Controls.Add(this.MaxWDLabel);
            this.controlPanel.Controls.Add(this.TurnGapTrackBar);
            this.controlPanel.Controls.Add(this.TurnGapTitleLabel);
            this.controlPanel.Controls.Add(this.WDTitleLabel);
            this.controlPanel.Controls.Add(this.MaxStiffnessLabel);
            this.controlPanel.Controls.Add(this.MinStiffnessLabel);
            this.controlPanel.Controls.Add(this.StiffnessTrackBar);
            this.controlPanel.Controls.Add(this.WireDiameterTrackBar);
            this.controlPanel.Location = new System.Drawing.Point(11, 217);
            this.controlPanel.Name = "controlPanel";
            this.controlPanel.Size = new System.Drawing.Size(344, 95);
            this.controlPanel.TabIndex = 127;
            // 
            // TGValueLabel
            // 
            this.TGValueLabel.AutoSize = true;
            this.TGValueLabel.Location = new System.Drawing.Point(98, 63);
            this.TGValueLabel.Name = "TGValueLabel";
            this.TGValueLabel.Size = new System.Drawing.Size(42, 25);
            this.TGValueLabel.TabIndex = 20;
            this.TGValueLabel.Text = "0.4";
            // 
            // WDValueLabel
            // 
            this.WDValueLabel.AutoSize = true;
            this.WDValueLabel.Location = new System.Drawing.Point(113, 19);
            this.WDValueLabel.Name = "WDValueLabel";
            this.WDValueLabel.Size = new System.Drawing.Size(42, 25);
            this.WDValueLabel.TabIndex = 19;
            this.WDValueLabel.Text = "1.6";
            // 
            // MinTGLabel
            // 
            this.MinTGLabel.AutoSize = true;
            this.MinTGLabel.Location = new System.Drawing.Point(159, 62);
            this.MinTGLabel.Name = "MinTGLabel";
            this.MinTGLabel.Size = new System.Drawing.Size(42, 25);
            this.MinTGLabel.TabIndex = 18;
            this.MinTGLabel.Text = "0.4";
            // 
            // MinWDLabel
            // 
            this.MinWDLabel.AutoSize = true;
            this.MinWDLabel.Location = new System.Drawing.Point(158, 18);
            this.MinWDLabel.Name = "MinWDLabel";
            this.MinWDLabel.Size = new System.Drawing.Size(42, 25);
            this.MinWDLabel.TabIndex = 17;
            this.MinWDLabel.Text = "1.6";
            // 
            // MaxTGLabel
            // 
            this.MaxTGLabel.AutoSize = true;
            this.MaxTGLabel.Location = new System.Drawing.Point(297, 62);
            this.MaxTGLabel.Name = "MaxTGLabel";
            this.MaxTGLabel.Size = new System.Drawing.Size(48, 25);
            this.MaxTGLabel.TabIndex = 16;
            this.MaxTGLabel.Text = "100";
            // 
            // MaxWDLabel
            // 
            this.MaxWDLabel.AutoSize = true;
            this.MaxWDLabel.Location = new System.Drawing.Point(297, 18);
            this.MaxWDLabel.Name = "MaxWDLabel";
            this.MaxWDLabel.Size = new System.Drawing.Size(42, 25);
            this.MaxWDLabel.TabIndex = 13;
            this.MaxWDLabel.Text = "7.6";
            // 
            // TurnGapTrackBar
            // 
            this.TurnGapTrackBar.Location = new System.Drawing.Point(193, 59);
            this.TurnGapTrackBar.Maximum = 50;
            this.TurnGapTrackBar.Minimum = 4;
            this.TurnGapTrackBar.Name = "TurnGapTrackBar";
            this.TurnGapTrackBar.Size = new System.Drawing.Size(102, 90);
            this.TurnGapTrackBar.TabIndex = 12;
            this.TurnGapTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.TurnGapTrackBar.Value = 27;
            // 
            // TurnGapTitleLabel
            // 
            this.TurnGapTitleLabel.AutoSize = true;
            this.TurnGapTitleLabel.Location = new System.Drawing.Point(12, 62);
            this.TurnGapTitleLabel.Name = "TurnGapTitleLabel";
            this.TurnGapTitleLabel.Size = new System.Drawing.Size(162, 25);
            this.TurnGapTitleLabel.TabIndex = 15;
            this.TurnGapTitleLabel.Text = "Turn Gap (mm):";
            // 
            // WDTitleLabel
            // 
            this.WDTitleLabel.AutoSize = true;
            this.WDTitleLabel.Location = new System.Drawing.Point(12, 18);
            this.WDTitleLabel.Name = "WDTitleLabel";
            this.WDTitleLabel.Size = new System.Drawing.Size(208, 25);
            this.WDTitleLabel.TabIndex = 14;
            this.WDTitleLabel.Text = "Wire Diameter (mm):";
            // 
            // MaxStiffnessLabel
            // 
            this.MaxStiffnessLabel.AutoSize = true;
            this.MaxStiffnessLabel.Location = new System.Drawing.Point(287, 36);
            this.MaxStiffnessLabel.Name = "MaxStiffnessLabel";
            this.MaxStiffnessLabel.Size = new System.Drawing.Size(101, 25);
            this.MaxStiffnessLabel.TabIndex = 2;
            this.MaxStiffnessLabel.Text = "More stiff";
            // 
            // MinStiffnessLabel
            // 
            this.MinStiffnessLabel.AutoSize = true;
            this.MinStiffnessLabel.Location = new System.Drawing.Point(13, 36);
            this.MinStiffnessLabel.Name = "MinStiffnessLabel";
            this.MinStiffnessLabel.Size = new System.Drawing.Size(98, 25);
            this.MinStiffnessLabel.TabIndex = 1;
            this.MinStiffnessLabel.Text = "Less stiff";
            // 
            // StiffnessTrackBar
            // 
            this.StiffnessTrackBar.Location = new System.Drawing.Point(59, 34);
            this.StiffnessTrackBar.Name = "StiffnessTrackBar";
            this.StiffnessTrackBar.Size = new System.Drawing.Size(231, 90);
            this.StiffnessTrackBar.TabIndex = 0;
            this.StiffnessTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.StiffnessTrackBar.Value = 5;
            // 
            // WireDiameterTrackBar
            // 
            this.WireDiameterTrackBar.Location = new System.Drawing.Point(193, 13);
            this.WireDiameterTrackBar.Maximum = 76;
            this.WireDiameterTrackBar.Minimum = 16;
            this.WireDiameterTrackBar.Name = "WireDiameterTrackBar";
            this.WireDiameterTrackBar.Size = new System.Drawing.Size(102, 90);
            this.WireDiameterTrackBar.TabIndex = 11;
            this.WireDiameterTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.WireDiameterTrackBar.Value = 46;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.AdvancedRadioButton);
            this.panel1.Controls.Add(this.StiffnessRadioButton);
            this.panel1.Location = new System.Drawing.Point(10, 172);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(346, 39);
            this.panel1.TabIndex = 128;
            // 
            // OnduleTopBarControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.ClothBox);
            this.Controls.Add(this.controlPanel);
            this.Controls.Add(this.AllDirectionsCheckBox);
            this.Controls.Add(this.LinearTwistConstraintRadioButton);
            this.Controls.Add(this.BendConstraintRadioButton);
            this.Controls.Add(this.TwistConstraintRadioButton);
            this.Controls.Add(this.LinearConstraintRadioButton);
            this.Controls.Add(this.linearConstraintCanvas);
            this.Controls.Add(this.OnduleConstraintCheckbox);
            this.Controls.Add(this.SplitterConstraints);
            this.Controls.Add(this.OnduleSpringGenerationTitleLabel);
            this.Controls.Add(this.SegmentationBtn);
            this.Controls.Add(this.SimulationBtn);
            this.Controls.Add(this.ExportBtn);
            this.Controls.Add(this.ConversionBtn);
            this.Controls.Add(this.debugBtn);
            this.Controls.Add(this.OnduleUnitsLabel);
            this.Controls.Add(this.OnduleUnitFlowPanel);
            this.Controls.Add(this.splitter4);
            this.Controls.Add(this.SplitterSpringGeneration);
            this.Controls.Add(this.SplitterPreparation);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.SplitterBottomMenu);
            this.Name = "OnduleTopBarControl";
            this.Size = new System.Drawing.Size(368, 618);
            this.Style = MetroFramework.MetroColorStyle.White;
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnduleTopBarControl_KeyDown);
            this.controlPanel.ResumeLayout(false);
            this.controlPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TurnGapTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.StiffnessTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.WireDiameterTrackBar)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
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
        private System.Windows.Forms.Splitter SplitterBottomMenu;
        private System.Windows.Forms.Label OnduleSpringGenerationTitleLabel;
        private System.Windows.Forms.Splitter splitter4;
        private System.Windows.Forms.Splitter SplitterSpringGeneration;
        private System.Windows.Forms.Splitter SplitterPreparation;
        private System.Windows.Forms.Splitter SplitterConstraints;
        private System.Windows.Forms.CheckBox OnduleConstraintCheckbox;
        private System.Windows.Forms.Panel linearConstraintCanvas;
        private System.Windows.Forms.RadioButton LinearConstraintRadioButton;
        private System.Windows.Forms.RadioButton TwistConstraintRadioButton;
        private System.Windows.Forms.RadioButton BendConstraintRadioButton;
        private System.Windows.Forms.RadioButton LinearTwistConstraintRadioButton;
        private System.Windows.Forms.CheckBox AllDirectionsCheckBox;
        private System.Windows.Forms.CheckBox ClothBox;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.RadioButton StiffnessRadioButton;
        private System.Windows.Forms.RadioButton AdvancedRadioButton;
        private System.Windows.Forms.Panel controlPanel;
        private System.Windows.Forms.TrackBar StiffnessTrackBar;
        private System.Windows.Forms.Label MinStiffnessLabel;
        private System.Windows.Forms.Label MaxStiffnessLabel;
        private System.Windows.Forms.Label MinTGLabel;
        private System.Windows.Forms.Label MinWDLabel;
        private System.Windows.Forms.Label MaxTGLabel;
        private System.Windows.Forms.Label MaxWDLabel;
        private System.Windows.Forms.TrackBar TurnGapTrackBar;
        private System.Windows.Forms.Label TurnGapTitleLabel;
        private System.Windows.Forms.Label WDTitleLabel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TrackBar WireDiameterTrackBar;
        private System.Windows.Forms.Label TGValueLabel;
        private System.Windows.Forms.Label WDValueLabel;
    }
}
