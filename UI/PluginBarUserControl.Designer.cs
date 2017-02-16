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
            this.btn_deform = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.Control;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F);
            this.button1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.button1.Location = new System.Drawing.Point(3, 21);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(178, 62);
            this.button1.TabIndex = 43;
            this.button1.Text = "Popup Window";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btn_deform
            // 
            this.btn_deform.BackColor = System.Drawing.SystemColors.Control;
            this.btn_deform.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F);
            this.btn_deform.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btn_deform.Location = new System.Drawing.Point(229, 21);
            this.btn_deform.Name = "btn_deform";
            this.btn_deform.Size = new System.Drawing.Size(168, 62);
            this.btn_deform.TabIndex = 47;
            this.btn_deform.Text = "Deform It";
            this.btn_deform.UseVisualStyleBackColor = false;
            this.btn_deform.Click += new System.EventHandler(this.btn_deform_Click);
            // 
            // PluginBarUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoScroll = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.btn_deform);
            this.Controls.Add(this.button1);
            this.Name = "PluginBarUserControl";
            this.Size = new System.Drawing.Size(607, 105);
            this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btn_deform;
    }
}
