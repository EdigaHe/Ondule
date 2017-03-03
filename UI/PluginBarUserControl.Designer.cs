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
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.Control;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.button1.Location = new System.Drawing.Point(14, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(150, 50);
            this.button1.TabIndex = 43;
            this.button1.Text = "Dimensions";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btn_deform
            // 
            this.btn_deform.BackColor = System.Drawing.SystemColors.Control;
            this.btn_deform.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_deform.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btn_deform.Location = new System.Drawing.Point(170, 12);
            this.btn_deform.Name = "btn_deform";
            this.btn_deform.Size = new System.Drawing.Size(150, 50);
            this.btn_deform.TabIndex = 47;
            this.btn_deform.Text = "Deform It";
            this.btn_deform.UseVisualStyleBackColor = false;
            this.btn_deform.Click += new System.EventHandler(this.btn_deform_Click);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.SystemColors.Control;
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.button2.Location = new System.Drawing.Point(326, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(150, 50);
            this.button2.TabIndex = 48;
            this.button2.Text = "Characteristics";
            this.button2.UseVisualStyleBackColor = false;
            // 
            // PluginBarUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoScroll = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btn_deform);
            this.Controls.Add(this.button1);
            this.Name = "PluginBarUserControl";
            this.Size = new System.Drawing.Size(504, 75);
            this.Load += new System.EventHandler(this.PluginBarUserControl_Load);
            this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btn_deform;
        private System.Windows.Forms.Button button2;
    }
}
