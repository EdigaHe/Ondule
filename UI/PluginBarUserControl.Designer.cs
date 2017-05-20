﻿namespace PluginBar
{
  partial class PluginBarUserControl
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
            this.button2 = new System.Windows.Forms.Button();
            this.dd_mode = new System.Windows.Forms.ComboBox();
            this.dd_type = new System.Windows.Forms.ComboBox();
            this.lb_type = new System.Windows.Forms.Label();
            this.lb_mode = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.SystemColors.Control;
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.button2.Location = new System.Drawing.Point(399, 11);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(172, 50);
            this.button2.TabIndex = 48;
            this.button2.Text = "Create Spring";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // dd_mode
            // 
            this.dd_mode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dd_mode.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dd_mode.Items.AddRange(new object[] {
            "Stretching",
            "Bending",
            "Twisting",
            "Scaling"});
            this.dd_mode.Location = new System.Drawing.Point(18, 34);
            this.dd_mode.Name = "dd_mode";
            this.dd_mode.Size = new System.Drawing.Size(162, 28);
            this.dd_mode.TabIndex = 50;
            // 
            // dd_type
            // 
            this.dd_type.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dd_type.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dd_type.Items.AddRange(new object[] {
            "Helical",
            "Machined",
            "Cantilever"});
            this.dd_type.Location = new System.Drawing.Point(213, 34);
            this.dd_type.Name = "dd_type";
            this.dd_type.Size = new System.Drawing.Size(151, 28);
            this.dd_type.TabIndex = 51;
            // 
            // lb_type
            // 
            this.lb_type.AutoSize = true;
            this.lb_type.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_type.Location = new System.Drawing.Point(232, 11);
            this.lb_type.Name = "lb_type";
            this.lb_type.Size = new System.Drawing.Size(117, 20);
            this.lb_type.TabIndex = 52;
            this.lb_type.Text = "Type of Spring";
            // 
            // lb_mode
            // 
            this.lb_mode.AutoSize = true;
            this.lb_mode.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_mode.Location = new System.Drawing.Point(14, 11);
            this.lb_mode.Name = "lb_mode";
            this.lb_mode.Size = new System.Drawing.Size(166, 20);
            this.lb_mode.TabIndex = 53;
            this.lb_mode.Text = "Mode of Deformation";
            // 
            // PluginBarUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoScroll = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.Controls.Add(this.lb_mode);
            this.Controls.Add(this.lb_type);
            this.Controls.Add(this.dd_type);
            this.Controls.Add(this.dd_mode);
            this.Controls.Add(this.button2);
            this.Name = "PluginBarUserControl";
            this.Size = new System.Drawing.Size(593, 80);
            this.Load += new System.EventHandler(this.PluginBarUserControl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button button2;
    public System.Windows.Forms.ComboBox dd_mode;
    public System.Windows.Forms.ComboBox dd_type;
    private System.Windows.Forms.Label lb_type;
    private System.Windows.Forms.Label lb_mode;
    }
}
