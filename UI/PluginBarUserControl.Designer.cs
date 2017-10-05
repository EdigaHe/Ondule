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
            this.mt_Select = new MetroFramework.Controls.MetroTile();
            this.mt_LinearDeform = new MetroFramework.Controls.MetroTile();
            this.mt_wireFrame = new MetroFramework.Controls.MetroTile();
            this.Twist = new System.Windows.Forms.Button();
            this.Bend = new System.Windows.Forms.Button();
            this.LinearTwist = new System.Windows.Forms.Button();
            this.LinearBend = new System.Windows.Forms.Button();
            this.LinearTwistBend = new System.Windows.Forms.Button();
            this.TwistBend = new System.Windows.Forms.Button();
            this.MedialAxisGeneration = new System.Windows.Forms.Button();
            this.SpringGen = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // mt_Select
            // 
            this.mt_Select.Location = new System.Drawing.Point(149, 21);
            this.mt_Select.Name = "mt_Select";
            this.mt_Select.Size = new System.Drawing.Size(114, 70);
            this.mt_Select.TabIndex = 48;
            this.mt_Select.Text = "Select an Area";
            this.mt_Select.Click += new System.EventHandler(this.mt_Select_Click);
            // 
            // mt_LinearDeform
            // 
            this.mt_LinearDeform.Location = new System.Drawing.Point(566, 21);
            this.mt_LinearDeform.Name = "mt_LinearDeform";
            this.mt_LinearDeform.Size = new System.Drawing.Size(127, 70);
            this.mt_LinearDeform.TabIndex = 49;
            this.mt_LinearDeform.Text = "Compress/Stretch";
            this.mt_LinearDeform.Click += new System.EventHandler(this.mt_LinearDeform_Click);
            // 
            // mt_wireFrame
            // 
            this.mt_wireFrame.Location = new System.Drawing.Point(3, 21);
            this.mt_wireFrame.Name = "mt_wireFrame";
            this.mt_wireFrame.Size = new System.Drawing.Size(140, 70);
            this.mt_wireFrame.TabIndex = 0;
            this.mt_wireFrame.Text = "GeneratePointCloud";
            this.mt_wireFrame.Click += new System.EventHandler(this.mt_wireFrame_Click);
            // 
            // Twist
            // 
            this.Twist.Location = new System.Drawing.Point(699, 21);
            this.Twist.Name = "Twist";
            this.Twist.Size = new System.Drawing.Size(102, 70);
            this.Twist.TabIndex = 50;
            this.Twist.Text = "Twist";
            this.Twist.UseVisualStyleBackColor = true;
            this.Twist.Click += new System.EventHandler(this.Twist_Click);
            // 
            // Bend
            // 
            this.Bend.Location = new System.Drawing.Point(807, 21);
            this.Bend.Name = "Bend";
            this.Bend.Size = new System.Drawing.Size(96, 70);
            this.Bend.TabIndex = 51;
            this.Bend.Text = "Bend";
            this.Bend.UseVisualStyleBackColor = true;
            this.Bend.Click += new System.EventHandler(this.Bend_Click);
            // 
            // LinearTwist
            // 
            this.LinearTwist.Location = new System.Drawing.Point(909, 21);
            this.LinearTwist.Name = "LinearTwist";
            this.LinearTwist.Size = new System.Drawing.Size(156, 70);
            this.LinearTwist.TabIndex = 52;
            this.LinearTwist.Text = "Linear + Twist";
            this.LinearTwist.UseVisualStyleBackColor = true;
            this.LinearTwist.Click += new System.EventHandler(this.LinearTwist_Click);
            // 
            // LinearBend
            // 
            this.LinearBend.Location = new System.Drawing.Point(1071, 21);
            this.LinearBend.Name = "LinearBend";
            this.LinearBend.Size = new System.Drawing.Size(168, 70);
            this.LinearBend.TabIndex = 53;
            this.LinearBend.Text = "Linear + Bend";
            this.LinearBend.UseVisualStyleBackColor = true;
            this.LinearBend.Click += new System.EventHandler(this.LinearBend_Click);
            // 
            // LinearTwistBend
            // 
            this.LinearTwistBend.Location = new System.Drawing.Point(1398, 21);
            this.LinearTwistBend.Name = "LinearTwistBend";
            this.LinearTwistBend.Size = new System.Drawing.Size(145, 70);
            this.LinearTwistBend.TabIndex = 54;
            this.LinearTwistBend.Text = "Linear + Twist + Bend";
            this.LinearTwistBend.UseVisualStyleBackColor = true;
            this.LinearTwistBend.Click += new System.EventHandler(this.LinearTwistBend_Click);
            // 
            // TwistBend
            // 
            this.TwistBend.Location = new System.Drawing.Point(1246, 21);
            this.TwistBend.Name = "TwistBend";
            this.TwistBend.Size = new System.Drawing.Size(146, 70);
            this.TwistBend.TabIndex = 55;
            this.TwistBend.Text = "Twist + Bend";
            this.TwistBend.UseVisualStyleBackColor = true;
            this.TwistBend.Click += new System.EventHandler(this.TwistBend_Click);
            // 
            // MedialAxisGeneration
            // 
            this.MedialAxisGeneration.Location = new System.Drawing.Point(286, 21);
            this.MedialAxisGeneration.Name = "MedialAxisGeneration";
            this.MedialAxisGeneration.Size = new System.Drawing.Size(207, 70);
            this.MedialAxisGeneration.TabIndex = 56;
            this.MedialAxisGeneration.Text = "Generate Medial Axis";
            this.MedialAxisGeneration.UseVisualStyleBackColor = true;
            this.MedialAxisGeneration.Click += new System.EventHandler(this.MedialAxisGeneration_Click);
            // 
            // SpringGen
            // 
            this.SpringGen.Location = new System.Drawing.Point(1550, 21);
            this.SpringGen.Name = "SpringGen";
            this.SpringGen.Size = new System.Drawing.Size(122, 70);
            this.SpringGen.TabIndex = 57;
            this.SpringGen.Text = "Spring generation";
            this.SpringGen.UseVisualStyleBackColor = true;
            this.SpringGen.Click += new System.EventHandler(this.SpringGen_Click);
            // 
            // PluginBarUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoScroll = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.Controls.Add(this.SpringGen);
            this.Controls.Add(this.MedialAxisGeneration);
            this.Controls.Add(this.TwistBend);
            this.Controls.Add(this.LinearTwistBend);
            this.Controls.Add(this.LinearBend);
            this.Controls.Add(this.LinearTwist);
            this.Controls.Add(this.Bend);
            this.Controls.Add(this.Twist);
            this.Controls.Add(this.mt_wireFrame);
            this.Controls.Add(this.mt_LinearDeform);
            this.Controls.Add(this.mt_Select);
            this.Name = "PluginBarUserControl";
            this.Size = new System.Drawing.Size(2030, 332);
            this.ResumeLayout(false);

    }

    #endregion
        private MetroFramework.Controls.MetroTile mt_Select;
        private MetroFramework.Controls.MetroTile mt_LinearDeform;
        private MetroFramework.Controls.MetroTile mt_wireFrame;
        private System.Windows.Forms.Button Twist;
        private System.Windows.Forms.Button Bend;
        private System.Windows.Forms.Button LinearTwist;
        private System.Windows.Forms.Button LinearBend;
        private System.Windows.Forms.Button LinearTwistBend;
        private System.Windows.Forms.Button TwistBend;
        private System.Windows.Forms.Button MedialAxisGeneration;
        private System.Windows.Forms.Button SpringGen;
    }
}
