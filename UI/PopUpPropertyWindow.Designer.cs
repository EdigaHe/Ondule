namespace OndulePlugin
{
    partial class DeformationDesignForm
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
            this.CancelBtn = new System.Windows.Forms.Button();
            this.PitchTrackbar = new System.Windows.Forms.TrackBar();
            this.ConstraintsTitle = new System.Windows.Forms.Label();
            this.TwistTrackbar = new System.Windows.Forms.TrackBar();
            this.FreeformTitle = new System.Windows.Forms.Label();
            this.WireDiameterTrackbar = new System.Windows.Forms.TrackBar();
            this.OKBtn = new System.Windows.Forms.Button();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.twistMin = new System.Windows.Forms.Label();
            this.twistMax = new System.Windows.Forms.Label();
            this.WDMax = new System.Windows.Forms.Label();
            this.WDMin = new System.Windows.Forms.Label();
            this.PitchMax = new System.Windows.Forms.Label();
            this.PitchMin = new System.Windows.Forms.Label();
            this.BendTrackbar = new System.Windows.Forms.TrackBar();
            this.bendingMax = new System.Windows.Forms.Label();
            this.bendingMin = new System.Windows.Forms.Label();
            this.PitchResetBtn = new System.Windows.Forms.Button();
            this.WDResetBtn = new System.Windows.Forms.Button();
            this.BendAngleResetBtn = new System.Windows.Forms.Button();
            this.TwistAngleResetBtn = new System.Windows.Forms.Button();
            this.LockCheckBox = new System.Windows.Forms.CheckBox();
            this.Preview = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.BendAngleValLabel = new System.Windows.Forms.Label();
            this.TwistAnglValLabel = new System.Windows.Forms.Label();
            this.CoilPitchLabel = new System.Windows.Forms.Label();
            this.StiffnessLabel = new System.Windows.Forms.Label();
            this.BendAngleLabel = new System.Windows.Forms.Label();
            this.TwistAngleLabel = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.LinearOnlyCheckbox = new System.Windows.Forms.CheckBox();
            this.BendOnlyCheckbox = new System.Windows.Forms.CheckBox();
            this.TwistOnlyCheckbox = new System.Windows.Forms.CheckBox();
            this.LinearTwistCheckbox = new System.Windows.Forms.CheckBox();
            this.compressLabel = new System.Windows.Forms.Label();
            this.BendConsDirectionTrackbar = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.LinearConsCompressTrackbar = new System.Windows.Forms.TrackBar();
            this.LinearConsStretchTrackbar = new System.Windows.Forms.TrackBar();
            this.bendDirLabel = new System.Windows.Forms.Label();
            this.stretchLabel = new System.Windows.Forms.Label();
            this.BendConsAngleMin = new System.Windows.Forms.Label();
            this.BendConsAngleMax = new System.Windows.Forms.Label();
            this.LinearConsStretchMin = new System.Windows.Forms.Label();
            this.LinearConsStretchMax = new System.Windows.Forms.Label();
            this.LinearConsCompressMin = new System.Windows.Forms.Label();
            this.LinearConsCompressMax = new System.Windows.Forms.Label();
            this.LinearConsCompressValue = new System.Windows.Forms.Label();
            this.LinearConsStretchValue = new System.Windows.Forms.Label();
            this.BendConsDirectionValue = new System.Windows.Forms.Label();
            this.PitchValLabel = new System.Windows.Forms.Label();
            this.WDValLabel = new System.Windows.Forms.Label();
            this.AllDirBendingCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.PitchTrackbar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TwistTrackbar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.WireDiameterTrackbar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BendTrackbar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BendConsDirectionTrackbar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LinearConsCompressTrackbar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LinearConsStretchTrackbar)).BeginInit();
            this.SuspendLayout();
            // 
            // CancelBtn
            // 
            this.CancelBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.CancelBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.CancelBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CancelBtn.Font = new System.Drawing.Font("Segoe UI Semibold", 8F, System.Drawing.FontStyle.Bold);
            this.CancelBtn.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.CancelBtn.Location = new System.Drawing.Point(594, 1097);
            this.CancelBtn.Margin = new System.Windows.Forms.Padding(6);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(155, 55);
            this.CancelBtn.TabIndex = 0;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.UseVisualStyleBackColor = false;
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // PitchTrackbar
            // 
            this.PitchTrackbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            this.PitchTrackbar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.PitchTrackbar.Location = new System.Drawing.Point(424, 348);
            this.PitchTrackbar.Margin = new System.Windows.Forms.Padding(6);
            this.PitchTrackbar.Maximum = 30;
            this.PitchTrackbar.Minimum = 1;
            this.PitchTrackbar.Name = "PitchTrackbar";
            this.PitchTrackbar.Size = new System.Drawing.Size(256, 90);
            this.PitchTrackbar.TabIndex = 1;
            this.PitchTrackbar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.PitchTrackbar.Value = 1;
            this.PitchTrackbar.Scroll += new System.EventHandler(this.Pitch_ValueChanged);
            this.PitchTrackbar.ValueChanged += new System.EventHandler(this.Pitch_ValueChanged);
            // 
            // ConstraintsTitle
            // 
            this.ConstraintsTitle.AutoSize = true;
            this.ConstraintsTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.ConstraintsTitle.ForeColor = System.Drawing.Color.White;
            this.ConstraintsTitle.Location = new System.Drawing.Point(17, 772);
            this.ConstraintsTitle.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ConstraintsTitle.Name = "ConstraintsTitle";
            this.ConstraintsTitle.Size = new System.Drawing.Size(345, 45);
            this.ConstraintsTitle.TabIndex = 6;
            this.ConstraintsTitle.Text = "Additional Constraints";
            // 
            // TwistTrackbar
            // 
            this.TwistTrackbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            this.TwistTrackbar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.TwistTrackbar.Location = new System.Drawing.Point(424, 605);
            this.TwistTrackbar.Margin = new System.Windows.Forms.Padding(6);
            this.TwistTrackbar.Minimum = 1;
            this.TwistTrackbar.Name = "TwistTrackbar";
            this.TwistTrackbar.Size = new System.Drawing.Size(256, 90);
            this.TwistTrackbar.TabIndex = 8;
            this.TwistTrackbar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.TwistTrackbar.Value = 1;
            this.TwistTrackbar.Scroll += new System.EventHandler(this.TwistTrackbar_ValueChanged);
            this.TwistTrackbar.ValueChanged += new System.EventHandler(this.TwistTrackbar_ValueChanged);
            // 
            // FreeformTitle
            // 
            this.FreeformTitle.AutoSize = true;
            this.FreeformTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.FreeformTitle.ForeColor = System.Drawing.Color.White;
            this.FreeformTitle.Location = new System.Drawing.Point(17, 117);
            this.FreeformTitle.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.FreeformTitle.Name = "FreeformTitle";
            this.FreeformTitle.Size = new System.Drawing.Size(270, 45);
            this.FreeformTitle.TabIndex = 3;
            this.FreeformTitle.Text = "Freeform Control";
            // 
            // WireDiameterTrackbar
            // 
            this.WireDiameterTrackbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            this.WireDiameterTrackbar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.WireDiameterTrackbar.Location = new System.Drawing.Point(424, 232);
            this.WireDiameterTrackbar.Margin = new System.Windows.Forms.Padding(6);
            this.WireDiameterTrackbar.Minimum = 1;
            this.WireDiameterTrackbar.Name = "WireDiameterTrackbar";
            this.WireDiameterTrackbar.Size = new System.Drawing.Size(256, 90);
            this.WireDiameterTrackbar.TabIndex = 10;
            this.WireDiameterTrackbar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.WireDiameterTrackbar.Value = 1;
            this.WireDiameterTrackbar.Scroll += new System.EventHandler(this.WDTrackbar_ValueChanged);
            this.WireDiameterTrackbar.ValueChanged += new System.EventHandler(this.WDTrackbar_ValueChanged);
            // 
            // OKBtn
            // 
            this.OKBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.OKBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.OKBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OKBtn.Font = new System.Drawing.Font("Segoe UI Semibold", 8F, System.Drawing.FontStyle.Bold);
            this.OKBtn.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.OKBtn.Location = new System.Drawing.Point(445, 1097);
            this.OKBtn.Margin = new System.Windows.Forms.Padding(6);
            this.OKBtn.Name = "OKBtn";
            this.OKBtn.Size = new System.Drawing.Size(137, 55);
            this.OKBtn.TabIndex = 13;
            this.OKBtn.Text = "OK";
            this.OKBtn.UseVisualStyleBackColor = false;
            this.OKBtn.Click += new System.EventHandler(this.OKBtn_Click);
            // 
            // pictureBox3
            // 
            this.pictureBox3.Location = new System.Drawing.Point(25, 192);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(313, 370);
            this.pictureBox3.TabIndex = 17;
            this.pictureBox3.TabStop = false;
            // 
            // twistMin
            // 
            this.twistMin.AutoSize = true;
            this.twistMin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.twistMin.ForeColor = System.Drawing.Color.White;
            this.twistMin.Location = new System.Drawing.Point(363, 610);
            this.twistMin.Name = "twistMin";
            this.twistMin.Size = new System.Drawing.Size(39, 26);
            this.twistMin.TabIndex = 20;
            this.twistMin.Text = "0° ";
            // 
            // twistMax
            // 
            this.twistMax.AutoSize = true;
            this.twistMax.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.twistMax.ForeColor = System.Drawing.Color.White;
            this.twistMax.Location = new System.Drawing.Point(679, 612);
            this.twistMax.Name = "twistMax";
            this.twistMax.Size = new System.Drawing.Size(51, 26);
            this.twistMax.TabIndex = 21;
            this.twistMax.Text = "45° ";
            // 
            // WDMax
            // 
            this.WDMax.AutoSize = true;
            this.WDMax.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.WDMax.ForeColor = System.Drawing.Color.White;
            this.WDMax.Location = new System.Drawing.Point(655, 240);
            this.WDMax.Name = "WDMax";
            this.WDMax.Size = new System.Drawing.Size(80, 26);
            this.WDMax.TabIndex = 23;
            this.WDMax.Text = "7.6mm";
            // 
            // WDMin
            // 
            this.WDMin.AutoSize = true;
            this.WDMin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.WDMin.ForeColor = System.Drawing.Color.White;
            this.WDMin.Location = new System.Drawing.Point(357, 237);
            this.WDMin.Name = "WDMin";
            this.WDMin.Size = new System.Drawing.Size(80, 26);
            this.WDMin.TabIndex = 22;
            this.WDMin.Text = "1.6mm";
            // 
            // PitchMax
            // 
            this.PitchMax.AutoSize = true;
            this.PitchMax.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.PitchMax.ForeColor = System.Drawing.Color.White;
            this.PitchMax.Location = new System.Drawing.Point(679, 358);
            this.PitchMax.Name = "PitchMax";
            this.PitchMax.Size = new System.Drawing.Size(36, 26);
            this.PitchMax.TabIndex = 25;
            this.PitchMax.Text = "10";
            // 
            // PitchMin
            // 
            this.PitchMin.AutoSize = true;
            this.PitchMin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.PitchMin.ForeColor = System.Drawing.Color.White;
            this.PitchMin.Location = new System.Drawing.Point(367, 358);
            this.PitchMin.Name = "PitchMin";
            this.PitchMin.Size = new System.Drawing.Size(42, 26);
            this.PitchMin.TabIndex = 24;
            this.PitchMin.Text = "0.4";
            // 
            // BendTrackbar
            // 
            this.BendTrackbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            this.BendTrackbar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BendTrackbar.Location = new System.Drawing.Point(424, 478);
            this.BendTrackbar.Margin = new System.Windows.Forms.Padding(6);
            this.BendTrackbar.Minimum = 1;
            this.BendTrackbar.Name = "BendTrackbar";
            this.BendTrackbar.Size = new System.Drawing.Size(256, 90);
            this.BendTrackbar.TabIndex = 27;
            this.BendTrackbar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.BendTrackbar.Value = 1;
            this.BendTrackbar.Scroll += new System.EventHandler(this.BendTrackbar_ValueChanged);
            this.BendTrackbar.ValueChanged += new System.EventHandler(this.BendTrackbar_ValueChanged);
            // 
            // bendingMax
            // 
            this.bendingMax.AutoSize = true;
            this.bendingMax.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.bendingMax.ForeColor = System.Drawing.Color.White;
            this.bendingMax.Location = new System.Drawing.Point(679, 485);
            this.bendingMax.Name = "bendingMax";
            this.bendingMax.Size = new System.Drawing.Size(51, 26);
            this.bendingMax.TabIndex = 28;
            this.bendingMax.Text = "60° ";
            // 
            // bendingMin
            // 
            this.bendingMin.AutoSize = true;
            this.bendingMin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.bendingMin.ForeColor = System.Drawing.Color.White;
            this.bendingMin.Location = new System.Drawing.Point(363, 483);
            this.bendingMin.Name = "bendingMin";
            this.bendingMin.Size = new System.Drawing.Size(39, 26);
            this.bendingMin.TabIndex = 29;
            this.bendingMin.Text = "0° ";
            // 
            // PitchResetBtn
            // 
            this.PitchResetBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.PitchResetBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.PitchResetBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PitchResetBtn.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
            this.PitchResetBtn.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.PitchResetBtn.Location = new System.Drawing.Point(649, 299);
            this.PitchResetBtn.Margin = new System.Windows.Forms.Padding(6);
            this.PitchResetBtn.Name = "PitchResetBtn";
            this.PitchResetBtn.Size = new System.Drawing.Size(80, 37);
            this.PitchResetBtn.TabIndex = 33;
            this.PitchResetBtn.Text = "Reset";
            this.PitchResetBtn.UseVisualStyleBackColor = false;
            this.PitchResetBtn.Click += new System.EventHandler(this.PitchResetBtn_Click);
            // 
            // WDResetBtn
            // 
            this.WDResetBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.WDResetBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.WDResetBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.WDResetBtn.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
            this.WDResetBtn.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.WDResetBtn.Location = new System.Drawing.Point(648, 186);
            this.WDResetBtn.Margin = new System.Windows.Forms.Padding(6);
            this.WDResetBtn.Name = "WDResetBtn";
            this.WDResetBtn.Size = new System.Drawing.Size(81, 38);
            this.WDResetBtn.TabIndex = 34;
            this.WDResetBtn.Text = "Reset";
            this.WDResetBtn.UseVisualStyleBackColor = false;
            this.WDResetBtn.Click += new System.EventHandler(this.WireDiameterResetBtn_Click);
            // 
            // BendAngleResetBtn
            // 
            this.BendAngleResetBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.BendAngleResetBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.BendAngleResetBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BendAngleResetBtn.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
            this.BendAngleResetBtn.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.BendAngleResetBtn.Location = new System.Drawing.Point(649, 418);
            this.BendAngleResetBtn.Margin = new System.Windows.Forms.Padding(6);
            this.BendAngleResetBtn.Name = "BendAngleResetBtn";
            this.BendAngleResetBtn.Size = new System.Drawing.Size(77, 43);
            this.BendAngleResetBtn.TabIndex = 35;
            this.BendAngleResetBtn.Text = "Reset";
            this.BendAngleResetBtn.UseVisualStyleBackColor = false;
            this.BendAngleResetBtn.Click += new System.EventHandler(this.BendingResetBtn_Click);
            // 
            // TwistAngleResetBtn
            // 
            this.TwistAngleResetBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.TwistAngleResetBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.TwistAngleResetBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TwistAngleResetBtn.Font = new System.Drawing.Font("Segoe UI Semibold", 8F);
            this.TwistAngleResetBtn.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.TwistAngleResetBtn.Location = new System.Drawing.Point(648, 548);
            this.TwistAngleResetBtn.Margin = new System.Windows.Forms.Padding(6);
            this.TwistAngleResetBtn.Name = "TwistAngleResetBtn";
            this.TwistAngleResetBtn.Size = new System.Drawing.Size(80, 41);
            this.TwistAngleResetBtn.TabIndex = 36;
            this.TwistAngleResetBtn.Text = "Reset";
            this.TwistAngleResetBtn.UseVisualStyleBackColor = false;
            this.TwistAngleResetBtn.Click += new System.EventHandler(this.TwistingResetBtn_Click);
            // 
            // LockCheckBox
            // 
            this.LockCheckBox.AutoSize = true;
            this.LockCheckBox.ForeColor = System.Drawing.Color.White;
            this.LockCheckBox.Location = new System.Drawing.Point(275, 1030);
            this.LockCheckBox.Name = "LockCheckBox";
            this.LockCheckBox.Size = new System.Drawing.Size(168, 29);
            this.LockCheckBox.TabIndex = 38;
            this.LockCheckBox.Text = "External lock";
            this.LockCheckBox.UseVisualStyleBackColor = true;
            this.LockCheckBox.CheckedChanged += new System.EventHandler(this.LockCheckBox_CheckedChanged);
            // 
            // Preview
            // 
            this.Preview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Preview.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Preview.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Preview.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Preview.Font = new System.Drawing.Font("Segoe UI Semibold", 8F, System.Drawing.FontStyle.Bold);
            this.Preview.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.Preview.Location = new System.Drawing.Point(32, 1097);
            this.Preview.Margin = new System.Windows.Forms.Padding(6);
            this.Preview.Name = "Preview";
            this.Preview.Size = new System.Drawing.Size(137, 55);
            this.Preview.TabIndex = 39;
            this.Preview.Text = "Preview";
            this.Preview.UseVisualStyleBackColor = false;
            this.Preview.Click += new System.EventHandler(this.Preview_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 17F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(19, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(615, 62);
            this.label1.TabIndex = 40;
            this.label1.Text = "Ondule Deformation Design";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label2.Location = new System.Drawing.Point(23, 84);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(796, 25);
            this.label2.TabIndex = 41;
            this.label2.Text = "---------------------------------------------------------------------------------" +
    "-------------------------------";
            // 
            // BendAngleValLabel
            // 
            this.BendAngleValLabel.AutoSize = true;
            this.BendAngleValLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BendAngleValLabel.ForeColor = System.Drawing.Color.DeepSkyBlue;
            this.BendAngleValLabel.Location = new System.Drawing.Point(486, 428);
            this.BendAngleValLabel.Name = "BendAngleValLabel";
            this.BendAngleValLabel.Size = new System.Drawing.Size(25, 25);
            this.BendAngleValLabel.TabIndex = 43;
            this.BendAngleValLabel.Text = "0";
            this.BendAngleValLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TwistAnglValLabel
            // 
            this.TwistAnglValLabel.AutoSize = true;
            this.TwistAnglValLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TwistAnglValLabel.ForeColor = System.Drawing.Color.DeepSkyBlue;
            this.TwistAnglValLabel.Location = new System.Drawing.Point(486, 560);
            this.TwistAnglValLabel.Name = "TwistAnglValLabel";
            this.TwistAnglValLabel.Size = new System.Drawing.Size(25, 25);
            this.TwistAnglValLabel.TabIndex = 44;
            this.TwistAnglValLabel.Text = "0";
            this.TwistAnglValLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CoilPitchLabel
            // 
            this.CoilPitchLabel.AutoSize = true;
            this.CoilPitchLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.CoilPitchLabel.ForeColor = System.Drawing.Color.White;
            this.CoilPitchLabel.Location = new System.Drawing.Point(361, 309);
            this.CoilPitchLabel.Name = "CoilPitchLabel";
            this.CoilPitchLabel.Size = new System.Drawing.Size(108, 26);
            this.CoilPitchLabel.TabIndex = 45;
            this.CoilPitchLabel.Text = "Turn Gap:";
            // 
            // StiffnessLabel
            // 
            this.StiffnessLabel.AutoSize = true;
            this.StiffnessLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.StiffnessLabel.ForeColor = System.Drawing.Color.White;
            this.StiffnessLabel.Location = new System.Drawing.Point(361, 194);
            this.StiffnessLabel.Name = "StiffnessLabel";
            this.StiffnessLabel.Size = new System.Drawing.Size(164, 26);
            this.StiffnessLabel.TabIndex = 46;
            this.StiffnessLabel.Text = "Wire Diameter: ";
            // 
            // BendAngleLabel
            // 
            this.BendAngleLabel.AutoSize = true;
            this.BendAngleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.BendAngleLabel.ForeColor = System.Drawing.Color.White;
            this.BendAngleLabel.Location = new System.Drawing.Point(361, 428);
            this.BendAngleLabel.Name = "BendAngleLabel";
            this.BendAngleLabel.Size = new System.Drawing.Size(128, 26);
            this.BendAngleLabel.TabIndex = 47;
            this.BendAngleLabel.Text = "Bend angle:";
            // 
            // TwistAngleLabel
            // 
            this.TwistAngleLabel.AutoSize = true;
            this.TwistAngleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.TwistAngleLabel.ForeColor = System.Drawing.Color.White;
            this.TwistAngleLabel.Location = new System.Drawing.Point(361, 557);
            this.TwistAngleLabel.Name = "TwistAngleLabel";
            this.TwistAngleLabel.Size = new System.Drawing.Size(127, 26);
            this.TwistAngleLabel.TabIndex = 48;
            this.TwistAngleLabel.Text = "Twist angle:";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(28, 579);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(313, 147);
            this.pictureBox1.TabIndex = 49;
            this.pictureBox1.TabStop = false;
            // 
            // LinearOnlyCheckbox
            // 
            this.LinearOnlyCheckbox.AutoSize = true;
            this.LinearOnlyCheckbox.ForeColor = System.Drawing.Color.White;
            this.LinearOnlyCheckbox.Location = new System.Drawing.Point(28, 839);
            this.LinearOnlyCheckbox.Name = "LinearOnlyCheckbox";
            this.LinearOnlyCheckbox.Size = new System.Drawing.Size(154, 29);
            this.LinearOnlyCheckbox.TabIndex = 50;
            this.LinearOnlyCheckbox.Text = "Linear Only";
            this.LinearOnlyCheckbox.UseVisualStyleBackColor = true;
            this.LinearOnlyCheckbox.CheckedChanged += new System.EventHandler(this.LinearOnlyCheckbox_CheckedChanged);
            // 
            // BendOnlyCheckbox
            // 
            this.BendOnlyCheckbox.AutoSize = true;
            this.BendOnlyCheckbox.ForeColor = System.Drawing.Color.White;
            this.BendOnlyCheckbox.Location = new System.Drawing.Point(555, 804);
            this.BendOnlyCheckbox.Name = "BendOnlyCheckbox";
            this.BendOnlyCheckbox.Size = new System.Drawing.Size(144, 29);
            this.BendOnlyCheckbox.TabIndex = 51;
            this.BendOnlyCheckbox.Text = "Bend Only";
            this.BendOnlyCheckbox.UseVisualStyleBackColor = true;
            this.BendOnlyCheckbox.CheckedChanged += new System.EventHandler(this.BendOnlyCheckbox_CheckedChanged);
            // 
            // TwistOnlyCheckbox
            // 
            this.TwistOnlyCheckbox.AutoSize = true;
            this.TwistOnlyCheckbox.ForeColor = System.Drawing.Color.White;
            this.TwistOnlyCheckbox.Location = new System.Drawing.Point(197, 839);
            this.TwistOnlyCheckbox.Name = "TwistOnlyCheckbox";
            this.TwistOnlyCheckbox.Size = new System.Drawing.Size(144, 29);
            this.TwistOnlyCheckbox.TabIndex = 52;
            this.TwistOnlyCheckbox.Text = "Twist Only";
            this.TwistOnlyCheckbox.UseVisualStyleBackColor = true;
            this.TwistOnlyCheckbox.CheckedChanged += new System.EventHandler(this.TwistOnlyCheckbox_CheckedChanged);
            // 
            // LinearTwistCheckbox
            // 
            this.LinearTwistCheckbox.AutoSize = true;
            this.LinearTwistCheckbox.ForeColor = System.Drawing.Color.White;
            this.LinearTwistCheckbox.Location = new System.Drawing.Point(362, 839);
            this.LinearTwistCheckbox.Name = "LinearTwistCheckbox";
            this.LinearTwistCheckbox.Size = new System.Drawing.Size(178, 29);
            this.LinearTwistCheckbox.TabIndex = 53;
            this.LinearTwistCheckbox.Text = "Linear + Twist";
            this.LinearTwistCheckbox.UseVisualStyleBackColor = true;
            this.LinearTwistCheckbox.CheckedChanged += new System.EventHandler(this.LinearTwistCheckbox_CheckedChanged);
            // 
            // compressLabel
            // 
            this.compressLabel.AutoSize = true;
            this.compressLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.compressLabel.ForeColor = System.Drawing.Color.White;
            this.compressLabel.Location = new System.Drawing.Point(27, 878);
            this.compressLabel.Name = "compressLabel";
            this.compressLabel.Size = new System.Drawing.Size(118, 26);
            this.compressLabel.TabIndex = 54;
            this.compressLabel.Text = "Compress:";
            // 
            // BendConsDirectionTrackbar
            // 
            this.BendConsDirectionTrackbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            this.BendConsDirectionTrackbar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BendConsDirectionTrackbar.Location = new System.Drawing.Point(539, 911);
            this.BendConsDirectionTrackbar.Margin = new System.Windows.Forms.Padding(6);
            this.BendConsDirectionTrackbar.Minimum = 1;
            this.BendConsDirectionTrackbar.Name = "BendConsDirectionTrackbar";
            this.BendConsDirectionTrackbar.Size = new System.Drawing.Size(209, 90);
            this.BendConsDirectionTrackbar.TabIndex = 55;
            this.BendConsDirectionTrackbar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.BendConsDirectionTrackbar.Value = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label3.Location = new System.Drawing.Point(25, 744);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(796, 25);
            this.label3.TabIndex = 56;
            this.label3.Text = "---------------------------------------------------------------------------------" +
    "-------------------------------";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(20, 1020);
            this.label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(246, 45);
            this.label4.TabIndex = 57;
            this.label4.Text = "Additional Lock";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label5.Location = new System.Drawing.Point(24, 989);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(796, 25);
            this.label5.TabIndex = 58;
            this.label5.Text = "---------------------------------------------------------------------------------" +
    "-------------------------------";
            // 
            // LinearConsCompressTrackbar
            // 
            this.LinearConsCompressTrackbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            this.LinearConsCompressTrackbar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.LinearConsCompressTrackbar.Location = new System.Drawing.Point(18, 911);
            this.LinearConsCompressTrackbar.Margin = new System.Windows.Forms.Padding(6);
            this.LinearConsCompressTrackbar.Minimum = 1;
            this.LinearConsCompressTrackbar.Name = "LinearConsCompressTrackbar";
            this.LinearConsCompressTrackbar.Size = new System.Drawing.Size(248, 90);
            this.LinearConsCompressTrackbar.TabIndex = 59;
            this.LinearConsCompressTrackbar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.LinearConsCompressTrackbar.Value = 1;
            this.LinearConsCompressTrackbar.Scroll += new System.EventHandler(this.LinearConsCompressTrackbar_Scroll);
            // 
            // LinearConsStretchTrackbar
            // 
            this.LinearConsStretchTrackbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            this.LinearConsStretchTrackbar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.LinearConsStretchTrackbar.Location = new System.Drawing.Point(294, 911);
            this.LinearConsStretchTrackbar.Margin = new System.Windows.Forms.Padding(6);
            this.LinearConsStretchTrackbar.Minimum = 1;
            this.LinearConsStretchTrackbar.Name = "LinearConsStretchTrackbar";
            this.LinearConsStretchTrackbar.Size = new System.Drawing.Size(235, 90);
            this.LinearConsStretchTrackbar.TabIndex = 60;
            this.LinearConsStretchTrackbar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.LinearConsStretchTrackbar.Value = 1;
            this.LinearConsStretchTrackbar.Scroll += new System.EventHandler(this.LinearConsStretchTrackbar_Scroll);
            // 
            // bendDirLabel
            // 
            this.bendDirLabel.AutoSize = true;
            this.bendDirLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.bendDirLabel.ForeColor = System.Drawing.Color.White;
            this.bendDirLabel.Location = new System.Drawing.Point(546, 878);
            this.bendDirLabel.Name = "bendDirLabel";
            this.bendDirLabel.Size = new System.Drawing.Size(104, 26);
            this.bendDirLabel.TabIndex = 61;
            this.bendDirLabel.Text = "Direction:";
            // 
            // stretchLabel
            // 
            this.stretchLabel.AutoSize = true;
            this.stretchLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.stretchLabel.ForeColor = System.Drawing.Color.White;
            this.stretchLabel.Location = new System.Drawing.Point(304, 878);
            this.stretchLabel.Name = "stretchLabel";
            this.stretchLabel.Size = new System.Drawing.Size(87, 26);
            this.stretchLabel.TabIndex = 62;
            this.stretchLabel.Text = "Stretch:";
            // 
            // BendConsAngleMin
            // 
            this.BendConsAngleMin.AutoSize = true;
            this.BendConsAngleMin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.BendConsAngleMin.ForeColor = System.Drawing.Color.White;
            this.BendConsAngleMin.Location = new System.Drawing.Point(550, 966);
            this.BendConsAngleMin.Name = "BendConsAngleMin";
            this.BendConsAngleMin.Size = new System.Drawing.Size(39, 26);
            this.BendConsAngleMin.TabIndex = 63;
            this.BendConsAngleMin.Text = "0° ";
            // 
            // BendConsAngleMax
            // 
            this.BendConsAngleMax.AutoSize = true;
            this.BendConsAngleMax.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.BendConsAngleMax.ForeColor = System.Drawing.Color.White;
            this.BendConsAngleMax.Location = new System.Drawing.Point(700, 966);
            this.BendConsAngleMax.Name = "BendConsAngleMax";
            this.BendConsAngleMax.Size = new System.Drawing.Size(63, 26);
            this.BendConsAngleMax.TabIndex = 64;
            this.BendConsAngleMax.Text = "360° ";
            // 
            // LinearConsStretchMin
            // 
            this.LinearConsStretchMin.AutoSize = true;
            this.LinearConsStretchMin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.LinearConsStretchMin.ForeColor = System.Drawing.Color.White;
            this.LinearConsStretchMin.Location = new System.Drawing.Point(306, 966);
            this.LinearConsStretchMin.Name = "LinearConsStretchMin";
            this.LinearConsStretchMin.Size = new System.Drawing.Size(24, 26);
            this.LinearConsStretchMin.TabIndex = 65;
            this.LinearConsStretchMin.Text = "0";
            // 
            // LinearConsStretchMax
            // 
            this.LinearConsStretchMax.AutoSize = true;
            this.LinearConsStretchMax.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.LinearConsStretchMax.ForeColor = System.Drawing.Color.White;
            this.LinearConsStretchMax.Location = new System.Drawing.Point(497, 966);
            this.LinearConsStretchMax.Name = "LinearConsStretchMax";
            this.LinearConsStretchMax.Size = new System.Drawing.Size(24, 26);
            this.LinearConsStretchMax.TabIndex = 66;
            this.LinearConsStretchMax.Text = "0";
            // 
            // LinearConsCompressMin
            // 
            this.LinearConsCompressMin.AutoSize = true;
            this.LinearConsCompressMin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.LinearConsCompressMin.ForeColor = System.Drawing.Color.White;
            this.LinearConsCompressMin.Location = new System.Drawing.Point(27, 966);
            this.LinearConsCompressMin.Name = "LinearConsCompressMin";
            this.LinearConsCompressMin.Size = new System.Drawing.Size(24, 26);
            this.LinearConsCompressMin.TabIndex = 67;
            this.LinearConsCompressMin.Text = "0";
            // 
            // LinearConsCompressMax
            // 
            this.LinearConsCompressMax.AutoSize = true;
            this.LinearConsCompressMax.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.LinearConsCompressMax.ForeColor = System.Drawing.Color.White;
            this.LinearConsCompressMax.Location = new System.Drawing.Point(236, 963);
            this.LinearConsCompressMax.Name = "LinearConsCompressMax";
            this.LinearConsCompressMax.Size = new System.Drawing.Size(24, 26);
            this.LinearConsCompressMax.TabIndex = 68;
            this.LinearConsCompressMax.Text = "0";
            // 
            // LinearConsCompressValue
            // 
            this.LinearConsCompressValue.AutoSize = true;
            this.LinearConsCompressValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LinearConsCompressValue.ForeColor = System.Drawing.Color.SandyBrown;
            this.LinearConsCompressValue.Location = new System.Drawing.Point(151, 879);
            this.LinearConsCompressValue.Name = "LinearConsCompressValue";
            this.LinearConsCompressValue.Size = new System.Drawing.Size(76, 25);
            this.LinearConsCompressValue.TabIndex = 69;
            this.LinearConsCompressValue.Text = "label3";
            this.LinearConsCompressValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LinearConsStretchValue
            // 
            this.LinearConsStretchValue.AutoSize = true;
            this.LinearConsStretchValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LinearConsStretchValue.ForeColor = System.Drawing.Color.SandyBrown;
            this.LinearConsStretchValue.Location = new System.Drawing.Point(397, 879);
            this.LinearConsStretchValue.Name = "LinearConsStretchValue";
            this.LinearConsStretchValue.Size = new System.Drawing.Size(76, 25);
            this.LinearConsStretchValue.TabIndex = 70;
            this.LinearConsStretchValue.Text = "label3";
            this.LinearConsStretchValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // BendConsDirectionValue
            // 
            this.BendConsDirectionValue.AutoSize = true;
            this.BendConsDirectionValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BendConsDirectionValue.ForeColor = System.Drawing.Color.SandyBrown;
            this.BendConsDirectionValue.Location = new System.Drawing.Point(653, 879);
            this.BendConsDirectionValue.Name = "BendConsDirectionValue";
            this.BendConsDirectionValue.Size = new System.Drawing.Size(76, 25);
            this.BendConsDirectionValue.TabIndex = 71;
            this.BendConsDirectionValue.Text = "label3";
            this.BendConsDirectionValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PitchValLabel
            // 
            this.PitchValLabel.AutoSize = true;
            this.PitchValLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PitchValLabel.ForeColor = System.Drawing.Color.DeepSkyBlue;
            this.PitchValLabel.Location = new System.Drawing.Point(478, 310);
            this.PitchValLabel.Name = "PitchValLabel";
            this.PitchValLabel.Size = new System.Drawing.Size(45, 25);
            this.PitchValLabel.TabIndex = 72;
            this.PitchValLabel.Text = "0.4";
            this.PitchValLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // WDValLabel
            // 
            this.WDValLabel.AutoSize = true;
            this.WDValLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WDValLabel.ForeColor = System.Drawing.Color.DeepSkyBlue;
            this.WDValLabel.Location = new System.Drawing.Point(519, 195);
            this.WDValLabel.Name = "WDValLabel";
            this.WDValLabel.Size = new System.Drawing.Size(81, 25);
            this.WDValLabel.TabIndex = 73;
            this.WDValLabel.Text = "1.6mm";
            this.WDValLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // AllDirBendingCheckBox
            // 
            this.AllDirBendingCheckBox.AutoSize = true;
            this.AllDirBendingCheckBox.BackColor = System.Drawing.Color.Black;
            this.AllDirBendingCheckBox.ForeColor = System.Drawing.Color.White;
            this.AllDirBendingCheckBox.Location = new System.Drawing.Point(555, 839);
            this.AllDirBendingCheckBox.Name = "AllDirBendingCheckBox";
            this.AllDirBendingCheckBox.Size = new System.Drawing.Size(167, 29);
            this.AllDirBendingCheckBox.TabIndex = 74;
            this.AllDirBendingCheckBox.Text = "All directions";
            this.AllDirBendingCheckBox.UseVisualStyleBackColor = false;
            this.AllDirBendingCheckBox.CheckedChanged += new System.EventHandler(this.AllDirBendingCheckBox_CheckedChanged);
            // 
            // DeformationDesignForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(774, 1183);
            this.ControlBox = false;
            this.Controls.Add(this.AllDirBendingCheckBox);
            this.Controls.Add(this.WDValLabel);
            this.Controls.Add(this.PitchValLabel);
            this.Controls.Add(this.BendConsDirectionValue);
            this.Controls.Add(this.LinearConsStretchValue);
            this.Controls.Add(this.LinearConsCompressValue);
            this.Controls.Add(this.LinearConsCompressMax);
            this.Controls.Add(this.LinearConsCompressMin);
            this.Controls.Add(this.LinearConsStretchMax);
            this.Controls.Add(this.LinearConsStretchMin);
            this.Controls.Add(this.BendConsAngleMax);
            this.Controls.Add(this.BendConsAngleMin);
            this.Controls.Add(this.stretchLabel);
            this.Controls.Add(this.bendDirLabel);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.compressLabel);
            this.Controls.Add(this.LinearConsStretchTrackbar);
            this.Controls.Add(this.LinearConsCompressTrackbar);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.BendConsDirectionTrackbar);
            this.Controls.Add(this.LinearTwistCheckbox);
            this.Controls.Add(this.TwistOnlyCheckbox);
            this.Controls.Add(this.BendOnlyCheckbox);
            this.Controls.Add(this.LinearOnlyCheckbox);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.TwistAngleLabel);
            this.Controls.Add(this.BendAngleLabel);
            this.Controls.Add(this.StiffnessLabel);
            this.Controls.Add(this.CoilPitchLabel);
            this.Controls.Add(this.TwistAnglValLabel);
            this.Controls.Add(this.BendAngleValLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Preview);
            this.Controls.Add(this.LockCheckBox);
            this.Controls.Add(this.TwistAngleResetBtn);
            this.Controls.Add(this.BendAngleResetBtn);
            this.Controls.Add(this.WDResetBtn);
            this.Controls.Add(this.PitchResetBtn);
            this.Controls.Add(this.bendingMin);
            this.Controls.Add(this.bendingMax);
            this.Controls.Add(this.BendTrackbar);
            this.Controls.Add(this.PitchMax);
            this.Controls.Add(this.PitchMin);
            this.Controls.Add(this.WDMax);
            this.Controls.Add(this.WDMin);
            this.Controls.Add(this.twistMax);
            this.Controls.Add(this.twistMin);
            this.Controls.Add(this.OKBtn);
            this.Controls.Add(this.WireDiameterTrackbar);
            this.Controls.Add(this.TwistTrackbar);
            this.Controls.Add(this.ConstraintsTitle);
            this.Controls.Add(this.FreeformTitle);
            this.Controls.Add(this.PitchTrackbar);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.pictureBox3);
            this.DisplayHeader = false;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MinimumSize = new System.Drawing.Size(774, 1183);
            this.Name = "DeformationDesignForm";
            this.Padding = new System.Windows.Forms.Padding(20, 30, 20, 20);
            this.Resizable = false;
            this.Style = MetroFramework.MetroColorStyle.Yellow;
            this.Text = "Ondule Deformation Design";
            this.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.TopMost = true;
            this.Load += new System.EventHandler(this.PropertyWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.PitchTrackbar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TwistTrackbar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.WireDiameterTrackbar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BendTrackbar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BendConsDirectionTrackbar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LinearConsCompressTrackbar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LinearConsStretchTrackbar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.TrackBar PitchTrackbar;
        private System.Windows.Forms.Label ConstraintsTitle;
        private System.Windows.Forms.TrackBar TwistTrackbar;
        private System.Windows.Forms.Label FreeformTitle;
        private System.Windows.Forms.TrackBar WireDiameterTrackbar;
        private System.Windows.Forms.Button OKBtn;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Label twistMin;
        private System.Windows.Forms.Label twistMax;
        private System.Windows.Forms.Label WDMax;
        private System.Windows.Forms.Label WDMin;
        private System.Windows.Forms.Label PitchMax;
        private System.Windows.Forms.Label PitchMin;
        private System.Windows.Forms.TrackBar BendTrackbar;
        private System.Windows.Forms.Label bendingMax;
        private System.Windows.Forms.Label bendingMin;
        private System.Windows.Forms.Button PitchResetBtn;
        private System.Windows.Forms.Button WDResetBtn;
        private System.Windows.Forms.Button BendAngleResetBtn;
        private System.Windows.Forms.Button TwistAngleResetBtn;
        private System.Windows.Forms.CheckBox LockCheckBox;
        private System.Windows.Forms.Button Preview;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label BendAngleValLabel;
        private System.Windows.Forms.Label TwistAnglValLabel;
        private System.Windows.Forms.Label CoilPitchLabel;
        private System.Windows.Forms.Label StiffnessLabel;
        private System.Windows.Forms.Label BendAngleLabel;
        private System.Windows.Forms.Label TwistAngleLabel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.CheckBox LinearOnlyCheckbox;
        private System.Windows.Forms.CheckBox BendOnlyCheckbox;
        private System.Windows.Forms.CheckBox TwistOnlyCheckbox;
        private System.Windows.Forms.CheckBox LinearTwistCheckbox;
        private System.Windows.Forms.Label compressLabel;
        private System.Windows.Forms.TrackBar BendConsDirectionTrackbar;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TrackBar LinearConsCompressTrackbar;
        private System.Windows.Forms.TrackBar LinearConsStretchTrackbar;
        private System.Windows.Forms.Label bendDirLabel;
        private System.Windows.Forms.Label stretchLabel;
        private System.Windows.Forms.Label BendConsAngleMin;
        private System.Windows.Forms.Label BendConsAngleMax;
        private System.Windows.Forms.Label LinearConsStretchMin;
        private System.Windows.Forms.Label LinearConsStretchMax;
        private System.Windows.Forms.Label LinearConsCompressMin;
        private System.Windows.Forms.Label LinearConsCompressMax;
        private System.Windows.Forms.Label LinearConsCompressValue;
        private System.Windows.Forms.Label LinearConsStretchValue;
        private System.Windows.Forms.Label BendConsDirectionValue;
        private System.Windows.Forms.Label PitchValLabel;
        private System.Windows.Forms.Label WDValLabel;
        private System.Windows.Forms.CheckBox AllDirBendingCheckBox;
    }
}