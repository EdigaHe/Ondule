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
            this.mt_LinearDeform = new MetroFramework.Controls.MetroTile();
            this.Twist = new System.Windows.Forms.Button();
            this.Bend = new System.Windows.Forms.Button();
            this.LinearTwist = new System.Windows.Forms.Button();
            this.LinearBend = new System.Windows.Forms.Button();
            this.LinearTwistBend = new System.Windows.Forms.Button();
            this.TwistBend = new System.Windows.Forms.Button();
            this.SpringGen = new System.Windows.Forms.Button();
            this.MATButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.label2 = new System.Windows.Forms.Label();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.splitter3 = new System.Windows.Forms.Splitter();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // mt_LinearDeform
            // 
            this.mt_LinearDeform.BackColor = System.Drawing.Color.White;
            this.mt_LinearDeform.Location = new System.Drawing.Point(315, 47);
            this.mt_LinearDeform.Name = "mt_LinearDeform";
            this.mt_LinearDeform.Size = new System.Drawing.Size(104, 70);
            this.mt_LinearDeform.TabIndex = 49;
            this.mt_LinearDeform.Text = "Compress/Stretch";
            this.mt_LinearDeform.Click += new System.EventHandler(this.mt_LinearDeform_Click);
            // 
            // Twist
            // 
            this.Twist.Location = new System.Drawing.Point(425, 47);
            this.Twist.Name = "Twist";
            this.Twist.Size = new System.Drawing.Size(102, 70);
            this.Twist.TabIndex = 50;
            this.Twist.Text = "Twist";
            this.Twist.UseVisualStyleBackColor = true;
            this.Twist.Click += new System.EventHandler(this.Twist_Click);
            // 
            // Bend
            // 
            this.Bend.Location = new System.Drawing.Point(533, 47);
            this.Bend.Name = "Bend";
            this.Bend.Size = new System.Drawing.Size(96, 70);
            this.Bend.TabIndex = 51;
            this.Bend.Text = "Bend";
            this.Bend.UseVisualStyleBackColor = true;
            this.Bend.Click += new System.EventHandler(this.Bend_Click);
            // 
            // LinearTwist
            // 
            this.LinearTwist.Location = new System.Drawing.Point(635, 47);
            this.LinearTwist.Name = "LinearTwist";
            this.LinearTwist.Size = new System.Drawing.Size(156, 70);
            this.LinearTwist.TabIndex = 52;
            this.LinearTwist.Text = "Linear + Twist";
            this.LinearTwist.UseVisualStyleBackColor = true;
            this.LinearTwist.Click += new System.EventHandler(this.LinearTwist_Click);
            // 
            // LinearBend
            // 
            this.LinearBend.Location = new System.Drawing.Point(797, 47);
            this.LinearBend.Name = "LinearBend";
            this.LinearBend.Size = new System.Drawing.Size(168, 70);
            this.LinearBend.TabIndex = 53;
            this.LinearBend.Text = "Linear + Bend";
            this.LinearBend.UseVisualStyleBackColor = true;
            this.LinearBend.Click += new System.EventHandler(this.LinearBend_Click);
            // 
            // LinearTwistBend
            // 
            this.LinearTwistBend.Location = new System.Drawing.Point(1123, 47);
            this.LinearTwistBend.Name = "LinearTwistBend";
            this.LinearTwistBend.Size = new System.Drawing.Size(145, 70);
            this.LinearTwistBend.TabIndex = 54;
            this.LinearTwistBend.Text = "Linear + Twist + Bend";
            this.LinearTwistBend.UseVisualStyleBackColor = true;
            this.LinearTwistBend.Click += new System.EventHandler(this.LinearTwistBend_Click);
            // 
            // TwistBend
            // 
            this.TwistBend.Location = new System.Drawing.Point(971, 47);
            this.TwistBend.Name = "TwistBend";
            this.TwistBend.Size = new System.Drawing.Size(146, 70);
            this.TwistBend.TabIndex = 55;
            this.TwistBend.Text = "Twist + Bend";
            this.TwistBend.UseVisualStyleBackColor = true;
            this.TwistBend.Click += new System.EventHandler(this.TwistBend_Click);
            // 
            // SpringGen
            // 
            this.SpringGen.Location = new System.Drawing.Point(1716, 46);
            this.SpringGen.Name = "SpringGen";
            this.SpringGen.Size = new System.Drawing.Size(122, 70);
            this.SpringGen.TabIndex = 57;
            this.SpringGen.Text = "Spring generation";
            this.SpringGen.UseVisualStyleBackColor = true;
            this.SpringGen.Click += new System.EventHandler(this.SpringGen_Click);
            // 
            // MATButton
            // 
            this.MATButton.Location = new System.Drawing.Point(23, 47);
            this.MATButton.Name = "MATButton";
            this.MATButton.Size = new System.Drawing.Size(239, 70);
            this.MATButton.TabIndex = 58;
            this.MATButton.Text = "Generate Medial Axis";
            this.MATButton.UseVisualStyleBackColor = true;
            this.MATButton.Click += new System.EventHandler(this.MATButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(21, 12);
            this.label1.Name = "label1";
            this.label1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label1.Size = new System.Drawing.Size(241, 25);
            this.label1.TabIndex = 59;
            this.label1.Text = "Please select an object:";
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.Color.White;
            this.splitter1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitter1.Location = new System.Drawing.Point(1294, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(189, 155);
            this.splitter1.TabIndex = 60;
            this.splitter1.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(310, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(375, 25);
            this.label2.TabIndex = 61;
            this.label2.Text = "Add a Ondule spring for deformations:";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // splitter2
            // 
            this.splitter2.BackColor = System.Drawing.Color.White;
            this.splitter2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitter2.Location = new System.Drawing.Point(289, 0);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(1005, 155);
            this.splitter2.TabIndex = 62;
            this.splitter2.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(1315, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 25);
            this.label3.TabIndex = 63;
            this.label3.Text = "Preview:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1319, 47);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(135, 63);
            this.button1.TabIndex = 64;
            this.button1.Text = "Simulation";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // splitter3
            // 
            this.splitter3.BackColor = System.Drawing.Color.White;
            this.splitter3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitter3.Location = new System.Drawing.Point(0, 0);
            this.splitter3.Name = "splitter3";
            this.splitter3.Size = new System.Drawing.Size(289, 155);
            this.splitter3.TabIndex = 65;
            this.splitter3.TabStop = false;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(1498, 46);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(125, 63);
            this.button2.TabIndex = 66;
            this.button2.Text = "Print";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // PluginBarUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoScroll = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.MATButton);
            this.Controls.Add(this.SpringGen);
            this.Controls.Add(this.TwistBend);
            this.Controls.Add(this.LinearTwistBend);
            this.Controls.Add(this.LinearBend);
            this.Controls.Add(this.LinearTwist);
            this.Controls.Add(this.Bend);
            this.Controls.Add(this.Twist);
            this.Controls.Add(this.mt_LinearDeform);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.splitter2);
            this.Controls.Add(this.splitter3);
            this.Name = "PluginBarUserControl";
            this.Size = new System.Drawing.Size(1869, 155);
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion
        private System.Windows.Forms.Button Twist;
        private System.Windows.Forms.Button Bend;
        private System.Windows.Forms.Button LinearTwist;
        private System.Windows.Forms.Button LinearBend;
        private System.Windows.Forms.Button LinearTwistBend;
        private System.Windows.Forms.Button TwistBend;
        private System.Windows.Forms.Button SpringGen;
        private System.Windows.Forms.Button MATButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Splitter splitter2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Splitter splitter3;
        private System.Windows.Forms.Button button2;
        private MetroFramework.Controls.MetroTile mt_LinearDeform;
    }
}
