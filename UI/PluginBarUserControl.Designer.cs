namespace PluginBar
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
            this.button1 = new System.Windows.Forms.Button();
            this.mt_Select = new MetroFramework.Controls.MetroTile();
            this.mt_LinearDeform = new MetroFramework.Controls.MetroTile();
            this.mt_wireFrame = new MetroFramework.Controls.MetroTile();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.Control;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F);
            this.button1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.button1.Location = new System.Drawing.Point(1840, 63);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(178, 62);
            this.button1.TabIndex = 43;
            this.button1.Text = "Popup Window";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // mt_Select
            // 
            this.mt_Select.Location = new System.Drawing.Point(181, 21);
            this.mt_Select.Name = "mt_Select";
            this.mt_Select.Size = new System.Drawing.Size(172, 70);
            this.mt_Select.TabIndex = 48;
            this.mt_Select.Text = "Select an Area";
            this.mt_Select.Click += new System.EventHandler(this.mt_Select_Click);
            // 
            // mt_LinearDeform
            // 
            this.mt_LinearDeform.Location = new System.Drawing.Point(359, 21);
            this.mt_LinearDeform.Name = "mt_LinearDeform";
            this.mt_LinearDeform.Size = new System.Drawing.Size(172, 70);
            this.mt_LinearDeform.TabIndex = 49;
            this.mt_LinearDeform.Text = "Compress/Stretch";
            this.mt_LinearDeform.Click += new System.EventHandler(this.mt_LinearDeform_Click);
            // 
            // mt_wireFrame
            // 
            this.mt_wireFrame.Location = new System.Drawing.Point(3, 21);
            this.mt_wireFrame.Name = "mt_wireFrame";
            this.mt_wireFrame.Size = new System.Drawing.Size(172, 70);
            this.mt_wireFrame.TabIndex = 0;
            this.mt_wireFrame.Text = "GeneratePointCloud";
            this.mt_wireFrame.Click += new System.EventHandler(this.mt_wireFrame_Click);
            // 
            // PluginBarUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoScroll = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.mt_wireFrame);
            this.Controls.Add(this.mt_LinearDeform);
            this.Controls.Add(this.mt_Select);
            this.Controls.Add(this.button1);
            this.Name = "PluginBarUserControl";
            this.Size = new System.Drawing.Size(2030, 332);
            this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button button1;
        private MetroFramework.Controls.MetroTile mt_Select;
        private MetroFramework.Controls.MetroTile mt_LinearDeform;
        private MetroFramework.Controls.MetroTile mt_wireFrame;
    }
}
