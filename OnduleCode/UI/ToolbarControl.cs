using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Rhino.UI;
using System.Xml;
using Rhino.DocObjects;
using Rhino.Geometry;
using System.Drawing.Drawing2D;

namespace OndulePlugin
{
    public partial class OnduleTopBarControl : MetroFramework.Controls.MetroUserControl, View,  IControllerModelObserver
    {
        private bool is_freeform = false;
        private bool is_LinearConstraint = false;
        private int power = 4;

        public OnduleUnit currUnit = new OnduleUnit();
        public int currIdx = 0;

        private double d_min = 1.6;
        private double d_max = 7.6;
        private double tg_min = 0.6;

        private double gap_min = 0.6;
        private double E = 1000;
        private List<double> stiffRange = new List<double>();

        private bool isAllDir = false;
        private bool isLinearLock = false;
        private bool isTwistLock = false;

        private double maxBendingAngle = 0;
        private double maxTwistingAngle = 0;
        private double maxTwistingForce = 10;

        #region Variables for the new parameter control panel

        double printing_tolerance = 0.6;

        //private OnduleUnit _springUnit = new OnduleUnit();
        //private OnduleUnit _tempRenderedSpring = new OnduleUnit();
        Boolean isOuterClothShown = false;
        Boolean isOuterClothIncluded = false;
        string specifier = "F1";
        string specifier1 = "F0";

        #region Constraint control related variables
        float bendDirCenterX = 150, bendDirCenterY = 10, bendDirRadius = 7;
        float bendAngleCenterX=150, bendAngleCenterY=110, bendAngleRadius = 7;
        float btBendAngleCenterX = 20, btBendAngleCenterY = 20;
        bool isBendDir = false;
        bool isBendAngle = false;
        bool isBTBendAngle = false;
        float bend_dir_traj_centerX = 150;
        float bend_dir_traj_centerY = 50;
        float bend_dir_traj_r = 40;
        float bend_angle_traj_centerX =150;
        float bend_angle_traj_centerY = 150;
        float bend_angle_traj_r = 40;
        float bt_bend_angle_traj_centerX = 20;
        float bt_bend_angle_traj_centerY = 60;
        double bend_dir_angle = 0;
        double bend_angle = 0;
        double bt_bend_angle = 0;
        Label bendDirValueLabel;
        Label bendAngleValueLabel;
        Label btBendAngleValueLabel;
        Label torsionLabel;
        Label lt_torsionLabel;
        Label bt_torsionLabel;

        float linearCenterX = 117, linearCenterY = 55, linearRadius = 7;
        float defaultStateX = 117;
        bool isLinearChange = false;
        double compDis = 0.6;
        double tenDis = 0.6;
        float compOffset = 0;
        float tenOffset = 0;
        Label compressDisValueLabel;
        Label tensionDisValueLabel;
        Label applyLoadValueLabel;
        Label lt_applyLoadValueLabel;
        Label applyTorqueValueLabel;
        Label bendingAngleLabel;
        Label btBendingAngleLabel;
        double applyTorqueValue;
        double lt_applyLoadValue;
        double applyLoadValue;
        float initialLen = 80;

        float twistCenterX = 150, twistCenterY = 20, twistRadius = 7;
        float twist_angle_traj_centerX = 150;
        float twist_angle_traj_centerY = 60;
        float twist_angle_traj_r = 40;
        bool isTwistAngle = false;
        double twist_angle = 0;
        Label twistValueLabel;

        float ltLinearCenterX = 117, ltLinearCenterY = 40, ltLinearRadius = 7;
        float ltAngleCenterX = 40, ltAngleCenterY = 100, ltAngleRadius = 7;
        float lt_twist_angle_traj_centerX = 40;
        float lt_twist_angle_traj_centerY = 120;
        float lt_twist_angle_traj_r = 20;
        float lt_compOffset = 0;
        float lt_tenOffset = 0;
        double lt_compDis = 0.6;
        double lt_tenDis = 0.6;
        double lt_twist_angle = 0;
        bool isLTDisChange = false;
        bool isLTAngle = false;
        Label ltTwistValueLabel;
        Label ltCompressDisValueLabel;
        Label ltTensionDisValueLabel;
        Label ltApplyTorqueValueLabel;
        Label btApplyTorqueValueLabel;

        int currConstraintCtrl = -1; // 0: linear only
                                     // 1: twist only
                                     // 2: linear + twist
                                     // 3: bend only
                                     // 4: bend + twist

        ProcessingWarningWindow processingwindow = new ProcessingWarningWindow();
        #endregion

        #endregion

        #region Initialization & Construction
        Controller controller;
        public void setController(Controller cont)
        {
            controller = cont;
            
            if (EventWatcherHandlers.Instance.IsEnabled == false)
            {
                EventWatcherHandlers.Instance.Enable(true);
                EventWatcherHandlers.Instance.setRhinoModel(ref controller);
            }
        }

        //public void SetDoubleBuffered(System.Windows.Forms.Control c)
        //{
        //    if (System.Windows.Forms.SystemInformation.TerminalServerSession)
        //        return;
        //    System.Reflection.PropertyInfo aProp = typeof(System.Windows.Forms.Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        //    aProp.SetValue(c, true, null);
        //}

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;
        //        cp.ExStyle |= 0x02000000;
        //        return cp;
        //    }
        //}

        public static void SetDouble(Control cc)
        {
            cc.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance |
                         System.Reflection.BindingFlags.NonPublic).SetValue(cc, true, null);
        }


        public OnduleTopBarControl()
        {

           

            //SetDoubleBuffered(this.ConstraintCanvas);

            InitializeComponent();

            set_control_panel_statues(false);
            SetDouble(this);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            this.BackColor = Color.FromArgb(255, 255, 255, 255);
            processingwindow.Hide();

            //SetDouble(this.ConstraintCanvas);

            // Hide some components in the control panel
            //this.WDTitleLabel.Hide();
            //this.MinWDLabel.Hide();
            //this.MaxWDLabel.Hide();
            //this.TurnGapTitleLabel.Hide();
            //this.MinTGLabel.Hide();
            //this.MaxTGLabel.Hide();
            //this.WireDiameterTrackBar.Hide();
            //this.TurnGapTrackBar.Hide();
            //this.WDValueLabel.Hide();
            //this.TGValueLabel.Hide();
        }
        #endregion

        #region No Reference
        //private void mt_Select_Click(object sender, EventArgs e)
        //{
        //    controller.selection();
        //}

        //private void mt_wireFrame_Click(object sender, EventArgs e)
        //{

        //    controller.wireframe();
        //}
        #endregion

        #region deformation triggers (reserved for the other window control)

        //private void Bend_Click(object sender, EventArgs e)
        //{
        //    // ask the user to select the medium axis
        //    const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.Curve;
        //    Rhino.DocObjects.ObjRef objRef;
        //    Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one object", false, filter, out objRef);

        //    if (rc == Rhino.Commands.Result.Success)
        //    {
        //        // send the object reference to the rhinomodel basically
        //        controller.bendDeform(objRef);
        //    }
        //}

        #region Old version of calling Linear + Twist
        //private void LinearTwist_Click(object sender, EventArgs e)
        //{
        //    // ask the user to select the medium axis
        //    const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.Curve;
        //    Rhino.DocObjects.ObjRef objRef;
        //    Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one object", false, filter, out objRef);

        //    if (rc == Rhino.Commands.Result.Success)
        //    {
        //        // send the object reference to the rhinomodel basically
        //        controller.linearTwistDeform(objRef);
        //    }
        //}
        #endregion

        //private void LinearBend_Click(object sender, EventArgs e)
        //{
        //    // ask the user to select the medium axis
        //    const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.Curve;
        //    Rhino.DocObjects.ObjRef objRef;
        //    Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one object", false, filter, out objRef);

        //    if (rc == Rhino.Commands.Result.Success)
        //    {
        //        // send the object reference to the rhinomodel basically
        //        controller.linearBendDeform(objRef);
        //    }
        //}

        //private void LinearTwistBend_Click(object sender, EventArgs e)
        //{
        //    is_freeform = true;

        //    // ### FOR DEBUG ###
        //    String path = @"Resources\FreeForm_mode.png";
        //    // ### FOR RELEASE ###
        //    //String path = @"OndulePlugin\Resources\FreeForm_mode.png";

        //    //// ask the user to select the medium axis
        //    //const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.Curve;
        //    //Rhino.DocObjects.ObjRef objRef;
        //    //Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one object", false, filter, out objRef);

        //    //if (rc == Rhino.Commands.Result.Success)
        //    //{
        //    //    // send the object reference to the rhinomodel basically
        //    //    controller.allDeform(objRef);
        //    //}
        //}

        //private void TwistBend_Click(object sender, EventArgs e)
        //{
        //    // ask the user to select the medium axis
        //    const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.Curve;
        //    Rhino.DocObjects.ObjRef objRef;
        //    Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one object", false, filter, out objRef);

        //    if (rc == Rhino.Commands.Result.Success)
        //    {
        //        // send the object reference to the rhinomodel basically
        //        controller.twistBendDeform(objRef);
        //    }
        //}

        #endregion

        #region [Not Used] Old approach of generating the medial axis
        //private void MedialAxisGeneration_Click(object sender, EventArgs e)
        //{
        //    controller.medialAxisTransform();
        //}
        #endregion

        #region Old linear implementation
        //private void LinearBtn_Click(object sender, EventArgs e)
        //{
        //    // ask the user to select the medium axis
        //    const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.Curve;
        //    Rhino.DocObjects.ObjRef objRef;
        //    Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one object", false, filter, out objRef);

        //    if (rc == Rhino.Commands.Result.Success)
        //    {
        //        // send the object reference to the rhinomodel basically
        //        controller.linearDeform(objRef);
        //    }
        //}
        #endregion

        private void set_control_panel_statues(Boolean isactivated)
        {
            this.StiffnessRadioButton.Enabled = isactivated;
            this.AdvancedRadioButton.Enabled = isactivated;
            this.WireDiameterTrackBar.Enabled = false;
            this.TurnGapTrackBar.Enabled = false;
            this.StiffnessTrackBar.Enabled = isactivated;

            if (currUnit.IsFreeformOnly)
            {
                this.OnduleConstraintCheckbox.Enabled = false;
            }
            else
            {
                this.OnduleConstraintCheckbox.Enabled = isactivated;
            }
            
            this.LinearConstraintRadioButton.Enabled = false;
            this.TwistConstraintRadioButton.Enabled = false;
            this.LinearTwistConstraintRadioButton.Enabled = false;
            this.BendConstraintRadioButton.Enabled = false;
            this.AllDirectionsRadioButton.Enabled = false;
            this.ClothBox.Enabled = isactivated;
            this.ClothBox.Checked = false;
            this.MinStiffnessLabel.Enabled = isactivated;
            this.MaxStiffnessLabel.Enabled = isactivated;
            this.MinWDLabel.Enabled = false;
            this.MaxWDLabel.Enabled = false;
            this.MinTGLabel.Enabled = false;
            this.MaxTGLabel.Enabled = false;
            this.OnduleSpringGenerationTitleLabel.Enabled = isactivated;
            this.WDValueLabel.Enabled = false;
            this.TGValueLabel.Enabled = false;
            this.WDTitleLabel.Enabled = false;
            this.TurnGapTitleLabel.Enabled = false;
        }
        private double calculate_rod_diameter(double len)
        {
            double d = 1.5;
            if (len >= 0 && len <= 30)
            {
                d = 1.5;
            }
            else if (len > 30 && len <= 50)
            {
                d = 1.6;
            }
            else if (len > 50 && len <= 70)
            {
                d = 1.7;
            }
            else if (len > 70 && len <= 90)
            {
                d = 1.8;
            }
            else if (len > 90 && len <= 110)
            {
                d = 1.9;
            }
            else if (len > 110)
            {
                d = 2;
            }

            return d;
        }

        private void initialize_parameter_panel(ref OnduleUnit currUnit, int curridx)
        {
            this.StiffnessRadioButton.Checked = true;
            string specifier = "F1";

            double sizeOfInnerStructure = 2 * (calculate_rod_diameter(currUnit.MA.GetLength()) + 0.5 * 3 + 1); 
            double clothWireDiameter = 1.6;
            double pitch = -1;   // The outer cloth always has the minimun pitch
            if (currUnit.MA.GetLength() <= 20)
            {
                pitch = clothWireDiameter + 0.4;   // The outer cloth always has the minimun pitch
            }
            else if (currUnit.MA.GetLength() <= 50 && currUnit.MA.GetLength() > 20)
            {
                pitch = clothWireDiameter + 0.8;   // The outer cloth always has the minimun pitch
            }
            else if (currUnit.MA.GetLength() <= 80 && currUnit.MA.GetLength() > 40)
            {
                pitch = clothWireDiameter + 1.2;   // The outer cloth always has the minimun pitch
            }
            else
            {
                pitch = clothWireDiameter + 1.6;   // The outer cloth always has the minimun pitch
            }

            if (currUnit.IsFreeformOnly)
            {
                if (currUnit.ClothIDs.Count  ==  currUnit.CappedSpringIDs.Count && currUnit.ClothIDs.ElementAt(0) == currUnit.CappedSpringIDs.ElementAt(0))
                {
                    double d_max_temp = (currUnit.CoilDiameter.Min() - 1.6) / 2;
                    d_max = d_max_temp > 7.6 ? 7.6 : d_max_temp;
                    d_max = d_max < 1.6 ? 1.6 : d_max;

                }
                else
                {
                    double d_max_temp = (currUnit.CoilDiameter.Min() - 2 * clothWireDiameter - 1.6) / 2;
                    d_max = d_max_temp > 7.6 ? 7.6 : d_max_temp;
                    d_max = d_max < 1.6 ? 1.6 : d_max;
                }
                
            }
            else
            {
                if (currUnit.ClothIDs.Count == currUnit.CappedSpringIDs.Count && currUnit.ClothIDs.ElementAt(0) == currUnit.CappedSpringIDs.ElementAt(0))
                {
                    double d_max_temp = (currUnit.CoilDiameter.Min() - sizeOfInnerStructure - 1.6) / 2;
                    d_max = d_max_temp > 7.6 ? 7.6 : d_max_temp;
                    d_max = d_max < 1.6 ? 1.6 : d_max;
                }
                else
                {
                    double d_max_temp = (currUnit.CoilDiameter.Min() - 2 * clothWireDiameter - sizeOfInnerStructure - 1.6) / 2;
                    d_max = d_max_temp > 7.6 ? 7.6 : d_max_temp;
                    d_max = d_max < 1.6 ? 1.6 : d_max;
                }
                
            }

            tg_min = pitch;


            // Initial the wire diameter trackbar and the turn gap trackbar

            this.WireDiameterTrackBar.Minimum = Convert.ToInt32(d_min / 0.1);
            double d_max_initial, d_max_real;
            d_max_initial = ((currUnit.MA.GetLength() - tg_min) < d_max) ? (currUnit.MA.GetLength() - tg_min) : d_max;
            if (currUnit.Pitch != -1)
            {
                d_max_real = ((currUnit.MA.GetLength() - currUnit.Pitch) < d_max) ? (currUnit.MA.GetLength() - currUnit.Pitch) : d_max;
            }
            else
            {
                d_max_real = ((currUnit.MA.GetLength() - tg_min) < d_max) ? (currUnit.MA.GetLength() - tg_min) : d_max;
            }

            this.WireDiameterTrackBar.Maximum = Convert.ToInt32(d_max_initial / 0.1);
            this.MinWDLabel.Text = d_min.ToString(specifier);
            this.MaxWDLabel.Text = d_max_initial.ToString(specifier);

            if (currUnit.WireDiameter == -1)
            {
                this.WireDiameterTrackBar.Value = this.WireDiameterTrackBar.Minimum;
                this.WDValueLabel.Text = Convert.ToDouble(this.WireDiameterTrackBar.Minimum * 0.1).ToString(specifier) + " mm";
                currUnit.WireDiameter = Convert.ToDouble(this.WireDiameterTrackBar.Minimum * 0.1);
            }
            else
            {
                if (currUnit.WireDiameter > d_max_real)
                {
                    // if the current wire diameter exceeds the actual max wire diameter (not the calculated one)
                    this.WireDiameterTrackBar.Value = Convert.ToInt32(d_max_real / 0.1);
                    this.WDValueLabel.Text = d_max_real.ToString(specifier) + " mm";
                    currUnit.WireDiameter = d_max_real;
                }
                else
                {
                    this.WireDiameterTrackBar.Value = Convert.ToInt32(currUnit.WireDiameter / 0.1);
                    this.WDValueLabel.Text = currUnit.WireDiameter.ToString(specifier) + " mm";
                }
            }

            double tg_max_initial, tg_max_real;
            //tg_max_initial = currUnit.MA.GetLength() - d_min;

            this.TurnGapTrackBar.Minimum = Convert.ToInt32(tg_min / 0.1);
            this.MinTGLabel.Text = tg_min.ToString(specifier);
            //tg_max_real = currUnit.MA.GetLength() - this.WireDiameterTrackBar.Value * 0.1;
            tg_max_initial = (4 * d_max_real) < (currUnit.MA.GetLength() - d_min) ? (4 * d_max_real) : (currUnit.MA.GetLength() - d_min);
            tg_max_real = tg_max_initial;
            this.MaxTGLabel.Text = tg_max_initial.ToString(specifier);
            this.TurnGapTrackBar.Maximum = Convert.ToInt32(tg_max_initial / 0.1);

            if (currUnit.Pitch == -1)
            {
                this.TurnGapTrackBar.Value = this.TurnGapTrackBar.Minimum;
                this.TGValueLabel.Text = Convert.ToDouble(this.TurnGapTrackBar.Minimum * 0.1).ToString(specifier) + " mm";
                currUnit.Pitch = Convert.ToDouble(this.TurnGapTrackBar.Minimum * 0.1);
            }
            else
            {
                if(currUnit.Pitch > tg_max_real){
                    // if the curent turn gap exceeds the real max turn gap, set the current turn gap the max real turn gap
                    if(this.TurnGapTrackBar.Maximum < Convert.ToInt32(tg_max_real / 0.1))
                    {
                        this.TurnGapTrackBar.Value = this.TurnGapTrackBar.Maximum;
                        this.TGValueLabel.Text = (this.TurnGapTrackBar.Maximum * 0.1).ToString(specifier) + " mm";
                        currUnit.Pitch = this.TurnGapTrackBar.Maximum * 0.1;
                    }
                    else if (this.TurnGapTrackBar.Minimum > Convert.ToInt32(tg_max_real / 0.1))
                    {
                        this.TurnGapTrackBar.Value = this.TurnGapTrackBar.Minimum;
                        this.TGValueLabel.Text = (this.TurnGapTrackBar.Minimum * 0.1).ToString(specifier) + " mm";
                        currUnit.Pitch = this.TurnGapTrackBar.Minimum * 0.1;
                    }
                    else
                    {
                        this.TurnGapTrackBar.Value = Convert.ToInt32(tg_max_real / 0.1);
                        this.TGValueLabel.Text = tg_max_real.ToString(specifier) + " mm";
                        currUnit.Pitch = tg_max_real;
                    }
                }
                else {
                    if (this.TurnGapTrackBar.Maximum < Convert.ToInt32(currUnit.Pitch / 0.1))
                    {
                        this.TurnGapTrackBar.Value = this.TurnGapTrackBar.Maximum;
                        this.TGValueLabel.Text = (this.TurnGapTrackBar.Maximum * 0.1).ToString(specifier) + " mm";
                    }
                    else if (this.TurnGapTrackBar.Minimum > Convert.ToInt32(currUnit.Pitch / 0.1))
                    {
                        this.TurnGapTrackBar.Value = this.TurnGapTrackBar.Minimum;
                        this.TGValueLabel.Text = (this.TurnGapTrackBar.Minimum * 0.1).ToString(specifier) + " mm";
                    }
                    else
                    {
                        this.TurnGapTrackBar.Value = Convert.ToInt32(currUnit.Pitch / 0.1);
                        this.TGValueLabel.Text = currUnit.Pitch.ToString(specifier) + " mm";
                    }
                }
            }

            // Calculate the stiffness and update the stiffness track bar
            double coilD = currUnit.MeanCoilDiameter;
            double turnN = currUnit.MA.GetLength() / (currUnit.Pitch + currUnit.WireDiameter);

            double turnN_max = currUnit.MA.GetLength() / (d_min + tg_min);
            double turnN_min = currUnit.MA.GetLength() / (d_max_real + tg_max_real);
            //double turnN_min = 1;

            //double k = Math.Pow(currUnit.WireDiameter, power) / turnN;
            //double k_min = Math.Pow(currUnit.WireDiameter, power) / turnN_max;
            //double k_max = Math.Pow(currUnit.WireDiameter, power) / turnN_min;
            //this.StiffnessTrackBar.Minimum = Convert.ToInt32(k_min * 1000);
            //this.StiffnessTrackBar.Maximum = Convert.ToInt32(k_max * 1000);
            //this.StiffnessTrackBar.Value = Convert.ToInt32(k * 1000);

            double k_max = (d_max_real + tg_max_real)/2;
            double k_min = d_min + tg_min;
            double k = (currUnit.WireDiameter + currUnit.Pitch)>=((d_max_real + tg_max_real) / 2)?((d_max_real + tg_max_real) / 2): (currUnit.WireDiameter + currUnit.Pitch);
            this.StiffnessTrackBar.Minimum = Convert.ToInt32(k_min * 100);
            this.StiffnessTrackBar.Maximum = Convert.ToInt32(k_max * 100);
            this.StiffnessTrackBar.Value = Convert.ToInt32(k * 100);

            currUnit.Stiffness = (currUnit.WireDiameter + currUnit.Pitch) * Math.Pow(currUnit.WireDiameter, power)/ currUnit.MA.GetLength();
            currUnit.CoilNum = turnN;

            // Update the currently selected unit
            controller.updateUnitFromGlobal(curridx, currUnit);

        }
        private void UnitBtn_Click(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            // Pass the current selected part's parameters
            processingwindow.Show();
            processingwindow.Refresh();

            Button temp = sender as Button;
            temp.BackColor = Color.FromArgb(93, 188, 210);
            int start = temp.Name.IndexOf('U');
            int end = temp.Name.IndexOf('_');
            int idx = Int32.Parse(temp.Name.Substring(start + 1, end - start - 1));

            // Change the currently selected button back to gray
            if(currIdx >= 0)
            {
                Button oldBtn = this.OnduleUnitFlowPanel.Controls[currIdx] as Button;
                oldBtn.BackColor = Color.FromArgb(150, 150, 150);

                controller.deHighlight(currUnit, this.isOuterClothShown);
            }

            currUnit = controller.getUnitFromGlobal(idx);
            currIdx = idx;

            if (currUnit.ConstraintType != -1)
            {
                this.currConstraintCtrl = currUnit.ConstraintType;
                this.ConstraintCanvas.Enabled = true;
                this.LinearConstraintRadioButton.Enabled = true;
                this.TwistConstraintRadioButton.Enabled = true;
                this.LinearTwistConstraintRadioButton.Enabled = true;
                this.BendConstraintRadioButton.Enabled = true;
                this.AllDirectionsRadioButton.Enabled = true;

                this.ConstraintCanvas.Controls.Clear();
                this.ConstraintCanvas.Refresh();
                this.OnduleConstraintCheckbox.Checked = true;

                switch (this.currConstraintCtrl)
                {
                    case 0:
                        {
                            this.LinearConstraintRadioButton.Checked = true;
                            this.TwistConstraintRadioButton.Checked = false;
                            this.LinearTwistConstraintRadioButton.Checked = false;
                            this.BendConstraintRadioButton.Checked = false;
                            this.AllDirectionsRadioButton.Checked = false;

                        }
                        break;
                    case 1:
                        {
                            this.LinearConstraintRadioButton.Checked = false;
                            this.TwistConstraintRadioButton.Checked = true;
                            this.LinearTwistConstraintRadioButton.Checked = false;
                            this.BendConstraintRadioButton.Checked = false;
                            this.AllDirectionsRadioButton.Checked = false;
                        }
                        break;
                    case 2:
                        {
                            this.LinearConstraintRadioButton.Checked = false;
                            this.TwistConstraintRadioButton.Checked = false;
                            this.LinearTwistConstraintRadioButton.Checked = true;
                            this.BendConstraintRadioButton.Checked = false;
                            this.AllDirectionsRadioButton.Checked = false;
                        }
                        break;
                    case 3:
                        {
                            this.LinearConstraintRadioButton.Checked = false;
                            this.TwistConstraintRadioButton.Checked = false;
                            this.LinearTwistConstraintRadioButton.Checked = false;
                            this.BendConstraintRadioButton.Checked = true;
                            this.AllDirectionsRadioButton.Checked = false;
                        }
                        break;
                    default: break;
                }
            }
            else
            {
                this.currConstraintCtrl = currUnit.ConstraintType;
                this.ConstraintCanvas.Enabled = false;
                this.ConstraintCanvas.Controls.Clear();
                this.ConstraintCanvas.Refresh();
                this.LinearConstraintRadioButton.Enabled = false;
                this.TwistConstraintRadioButton.Enabled = false;
                this.LinearTwistConstraintRadioButton.Enabled = false;
                this.BendConstraintRadioButton.Enabled = false;
                this.AllDirectionsRadioButton.Enabled = false;
                this.OnduleConstraintCheckbox.Checked = false;
                this.LinearConstraintRadioButton.Checked = false;
                this.TwistConstraintRadioButton.Checked = false;
                this.LinearTwistConstraintRadioButton.Checked = false;
                this.BendConstraintRadioButton.Checked = false;
                this.AllDirectionsRadioButton.Checked = false;
            }

            //controller.springGeneration(ref currUnit);
            //// Enable the spring control panel if it is not enabled
            //set_control_panel_statues(true);

            //// Initial the parameter panel
            //initialize_parameter_panel(ref currUnit, currIdx);

            // Update the model's color in the Rhino scene
            controller.highlightCurrent(currUnit, this.isOuterClothShown);

            processingwindow.Hide();
        }

        private void debugBtn_Click(object sender, EventArgs e)
        {
            if (currUnit != null && currIdx != -1)
            {
                // Currently we don't need to specify the polysurface explicitly
                // The currUnit includes the GUID of the surface
                DeformationDesignForm coilwindow = new DeformationDesignForm(currUnit, currIdx, controller);
                coilwindow.Show();
            }
            else
            {
                DeformationDesignForm coilwindow = new DeformationDesignForm(controller);
                coilwindow.Show();
            }
        }

        private void ConversionBtn_Click(object sender, EventArgs e)
        {

            #region Start the loading 

            processingwindow.Show();
            processingwindow.Refresh();

            this.currConstraintCtrl = -1;
            this.ConstraintCanvas.Enabled = false;
            this.ConstraintCanvas.Controls.Clear();
            this.ConstraintCanvas.Refresh();
            this.OnduleConstraintCheckbox.Checked = false;
            this.LinearConstraintRadioButton.Checked = false;
            this.TwistConstraintRadioButton.Checked = false;
            this.LinearTwistConstraintRadioButton.Checked = false;
            this.BendConstraintRadioButton.Checked = false;
            this.AllDirectionsRadioButton.Checked = false;

            #endregion

            // ask the user to select the medium axis
            is_freeform = false;
            // ### FOR DEBUG ###
            String path = @"Resources\FreeForm_default.png";
            // ### FOR RELEASE ###
            //String path = @"OndulePlugin\Resources\FreeForm_default.png";

            //ObjRef armOffsetObjRef = new ObjRef(sufObjId);//get the objRef from the GUID

            //Brep surfaceBrep = armOffsetObjRef.Brep(); // because we know the geometry is Brep, we directly find it from the objRef 


            OnduleUnit tempNewUnit = new OnduleUnit();
            tempNewUnit = controller.medialAxisGeneration();

            // The unitID of tempNewUnit stores the selected outer surface
            // Compute the spring diameter based on all discontinue curves
            ObjRef selectedObjRef = new ObjRef(tempNewUnit.BREPID);//get the objRef from the GUID
            Brep surfaceBrep = selectedObjRef.Brep(); // because we know the geometry is Brep, we directly find it from the objRef 

            tempNewUnit.G = 350000; // TO-DO: change the value based on the material
            tempNewUnit.Length = tempNewUnit.MA.GetLength();

            List<double> coilDs = new List<double>();
            List<int> coilNs = new List<int>();
            double wireD = 1.6;   // all discontinued curves share the same wire diameter
            double springPitch = d_min + gap_min;
            List<double> disLen = new List<double>();

            #region Compute the discontinued curves, coil diameters, wire diameter, coild numbers, and pitches

            #region Compute the segment with a consistent curvature
            // Find the continous curve with a consistent curvature

            double rangeCurvatureThreshold = 50;

            // Curve discontinuity exists in the medial curves
            double totalLen = tempNewUnit.MA.GetLength();
            double unitLen = 5;
            List<double> curvatureList = new List<double>();

            for (int i = 0; i < totalLen/unitLen; i++)
            {
                double len_param;
                tempNewUnit.MA.LengthParameter(i * unitLen, out len_param);
                double curr_curvature = tempNewUnit.MA.CurvatureAt(len_param).Length;

                curvatureList.Add(curr_curvature);
            }

            // Based on the recorded curvatures at the *num_seg* points along the medial axis,
            // we find the longest curve to replace the medial axis.
            List<int> flags = new List<int>();
            double pre_curvature = curvatureList.ElementAt(0);
            for(int idx = 0; idx < curvatureList.Count; idx++)
            {
                double curr_curvature = curvatureList.ElementAt(idx);
                if(curr_curvature / pre_curvature >= 1/ rangeCurvatureThreshold 
                    && curr_curvature / pre_curvature <= rangeCurvatureThreshold)
                {
                    continue;
                }
                else
                {
                    pre_curvature = curr_curvature;
                    flags.Add(idx);
                }
            }
            flags.Add(curvatureList.Count - 1);

            int max_span_len = 0;
            int pre_pos = 0;
            int start_pos = -1; // the start position of the longest span in flags, including the start position
            int end_pos = -1;   // the end position of the longest span in flags, excluding the end position
      
            foreach(int flag_pos in flags)
            {
                int span_len = flag_pos - pre_pos;
                if(span_len >= max_span_len)
                {
                    max_span_len = span_len;
                    start_pos = pre_pos;
                    end_pos = flag_pos;
                    
                }
                pre_pos = flag_pos;
            }

            // Cut the medial axis into the new curve with a more consistent curvature
            double new_MA_start_param, new_MA_end_param;
            tempNewUnit.MA.LengthParameter(start_pos * unitLen, out new_MA_start_param);
            if(end_pos == curvatureList.Count-1)
                tempNewUnit.MA.LengthParameter(end_pos * unitLen, out new_MA_end_param);
            else
                tempNewUnit.MA.LengthParameter((end_pos - 1) * unitLen, out new_MA_end_param);

            Curve c1;
            Curve effectiveMA;
            if(new_MA_start_param == 0)
            {
                Curve c_candidate1 = tempNewUnit.MA.Split(new_MA_end_param)[0];
                Curve c_candidate2 = tempNewUnit.MA.Split(new_MA_end_param)[1];
                if(c_candidate1.GetLength() >= c_candidate2.GetLength())
                {
                    effectiveMA = c_candidate1;
                }
                else
                {
                    effectiveMA = c_candidate2;
                } 
            }
            else
            {
                c1 = tempNewUnit.MA.Split(new_MA_start_param)[1];
                effectiveMA = c1.Split(new_MA_end_param)[0];
            }

            tempNewUnit.SelectedSeg = effectiveMA;
            #endregion
            ///////// edited by Liang He on 1/17/2019


            double lengthPara;
            effectiveMA.LengthParameter(effectiveMA.GetLength(), out lengthPara);
            bool discontinuity = true;
            List<double> discontinuitylist = new List<double>();
            double startingPt = 0;
            while (discontinuity)
            {
                double t;
                discontinuity = effectiveMA.GetNextDiscontinuity(Continuity.Cinfinity_continuous, startingPt, lengthPara, out t);
                if (double.IsNaN(t) == false)
                {
                    discontinuitylist.Add(t);
                    startingPt = t;
                }
            }

            Curve[] discontinueCrv = null;
            if (discontinuitylist != null && discontinuitylist.Count > 0)
            {
                discontinueCrv = effectiveMA.Split(discontinuitylist);
            }
            double endPara1;

            if (discontinueCrv != null)
            {
                foreach (Curve crv in discontinueCrv)
                {
                    crv.LengthParameter(crv.GetLength(), out endPara1);
                    double r1 = 5, r2 = 5;

                    double srvStartPara = 0;
                    double srvEndPara = 0;
                    crv.LengthParameter(0, out srvStartPara);
                    crv.LengthParameter(crv.GetLength(), out srvEndPara);
                    Plane crvSegStartPln = new Plane(crv.PointAtStart, crv.TangentAt(srvStartPara));
                    Plane crvSegEndPln = new Plane(crv.PointAtEnd, crv.TangentAt(srvEndPara));

                    Curve[] interStartCrvs;
                    Curve[] interEndCrvs;
                    Point3d[] interStartPts;
                    Point3d[] interEndPts;

                    Rhino.Geometry.Intersect.Intersection.BrepPlane(surfaceBrep, crvSegStartPln, 0.001, out interStartCrvs, out interStartPts);
                    Rhino.Geometry.Intersect.Intersection.BrepPlane(surfaceBrep, crvSegEndPln, 0.001, out interEndCrvs, out interEndPts);

                    disLen.Add(crv.GetLength());

                    foreach (Curve c in interStartCrvs)
                    {
                        double p;
                        c.ClosestPoint(crv.PointAtStart, out p);
                        r1 = c.PointAt(p).DistanceTo(crv.PointAtStart);
                    }

                    foreach (Curve c in interEndCrvs)
                    {
                        double p;
                        c.ClosestPoint(crv.PointAtEnd, out p);
                        r2 = c.PointAt(p).DistanceTo(crv.PointAtEnd);
                    }

                    // The coil diameter is the average of both diameters
                    double cD = (r1 + r2) / 2;
                    coilDs.Add(cD);

                    // For the initial Ondule unit we use the minimum diameter for the wire diameter
                    double initDia = d_min;
                    wireD = initDia;
                    int cn = Convert.ToInt32(Math.Ceiling(crv.GetLength() / (initDia + gap_min)));
                    coilNs.Add(cn);

                    springPitch = initDia + gap_min;
                }
            }
            else
            {
                // If there are no discontinued curves, we add only one curve 
                double srvStartPara = 0;
                double srvEndPara = 0;
                effectiveMA.LengthParameter(0, out srvStartPara);
                effectiveMA.LengthParameter(effectiveMA.GetLength(), out srvEndPara);
                Plane crvSegStartPln = new Plane(effectiveMA.PointAtStart, effectiveMA.TangentAt(srvStartPara));
                Plane crvSegEndPln = new Plane(effectiveMA.PointAtEnd, effectiveMA.TangentAt(srvEndPara));

                Curve[] interStartCrvs;
                Curve[] interEndCrvs;
                Point3d[] interStartPts;
                Point3d[] interEndPts;

                Rhino.Geometry.Intersect.Intersection.BrepPlane(surfaceBrep, crvSegStartPln, 0.001, out interStartCrvs, out interStartPts);
                Rhino.Geometry.Intersect.Intersection.BrepPlane(surfaceBrep, crvSegEndPln, 0.001, out interEndCrvs, out interEndPts);

                disLen.Add(effectiveMA.GetLength());

                double r1 = 0;
                double r2 = 0;
                foreach (Curve c in interStartCrvs)
                {
                    double p;
                    c.ClosestPoint(effectiveMA.PointAtStart, out p);
                    r1 = c.PointAt(p).DistanceTo(effectiveMA.PointAtStart);
                }

                foreach (Curve c in interEndCrvs)
                {
                    double p;
                    c.ClosestPoint(effectiveMA.PointAtEnd, out p);
                    r2 = c.PointAt(p).DistanceTo(effectiveMA.PointAtEnd);
                }
                // Use the entire central axis and get the average diameter of the spring coil
                coilDs.Add((r1 + r2) / 2);
                wireD = d_min;
                springPitch = d_min + gap_min;

                int cn = Convert.ToInt32(Math.Ceiling(effectiveMA.GetLength() / (d_min + gap_min)));
                coilNs.Add(cn);
            }

            #endregion

            // initial the parameters for the generated unit
            tempNewUnit.CoilDiameter = coilDs;
            double mean_CoilD = (coilDs.Min() + coilDs.Max())/2;

            tempNewUnit.WireDiameter = (wireD<=(mean_CoilD / 12)? (mean_CoilD / 12):wireD);
            //tempNewUnit.CoilNum = coilNs;
            tempNewUnit.DiscontinuedLengths = disLen;
            tempNewUnit.ID = controller.getCountGlobal();
            tempNewUnit.Pitch = springPitch-wireD;
            tempNewUnit.Length = effectiveMA.GetLength();

            controller.addUnitToGlobal(tempNewUnit);

            if (tempNewUnit.BREPID != Guid.Empty)
            {
                // Add one unit button in the flow panel
                Button unitBtn = new Button();
                int crntIdx = controller.getCountGlobal() - 1;
                unitBtn.Name = "OU" + crntIdx.ToString() + "_" + tempNewUnit.BREPID.ToString();
                unitBtn.Text = "";
                unitBtn.BackColor = Color.FromArgb(93, 188, 210);

                // Change the currently selected button back to gray
                if(OnduleUnitFlowPanel.Controls.Count > 0)
                {
                    Button oldBtn = this.OnduleUnitFlowPanel.Controls[currIdx] as Button;
                    oldBtn.BackColor = Color.FromArgb(150, 150, 150);

                    controller.deHighlight(currUnit, this.isOuterClothShown);
                }

                unitBtn.Width = 15;
                unitBtn.Height = 34;
                unitBtn.FlatStyle = FlatStyle.Flat;
                unitBtn.FlatAppearance.BorderSize = 0;
                unitBtn.Click += UnitBtn_Click;

                currIdx = crntIdx;
                currUnit = controller.getUnitFromGlobal(crntIdx);

                OnduleUnitFlowPanel.Controls.Add(unitBtn);

                #region Automatically convert the current unit into a spring
               
                controller.springGeneration(ref currUnit);
                // Enable the spring control panel if it is not enabled
                set_control_panel_statues(true);

                // Initial the parameter panel
                initialize_parameter_panel(ref currUnit, currIdx);

                controller.highlightCurrent(currUnit, this.isOuterClothShown);

                #endregion

            }

            //this.Controls.Remove(loadingLayer);
            //this.Controls.Remove(loadingLayerBk);
            processingwindow.Hide();
        }

        private void ExportBtn_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("Hello World!");
        }

        private void SimulationBtn_Click(object sender, EventArgs e)
        {

        }

        private void SegmentationBtn_Click(object sender, EventArgs e)
        {
            if (currUnit != null && currIdx != -1)
            {

                set_control_panel_statues(false);
                //// Hide some components in the control panel
                //this.WDTitleLabel.Hide();
                //this.MinWDLabel.Hide();
                //this.MaxWDLabel.Hide();
                //this.TurnGapTitleLabel.Hide();
                //this.MinTGLabel.Hide();
                //this.MaxTGLabel.Hide();
                //this.WireDiameterTrackBar.Hide();
                //this.TurnGapTrackBar.Hide();
                //this.WDValueLabel.Hide();
                //this.TGValueLabel.Hide();

                controller.selectMASegment(ref currUnit);
                controller.updateUnitFromGlobal(currIdx, currUnit);

                processingwindow.Show();
                processingwindow.Refresh();
                controller.springGeneration(ref currUnit);
                // Enable the spring control panel if it is not enabled
                set_control_panel_statues(true);

                // Initial the parameter panel
                initialize_parameter_panel(ref currUnit, currIdx);
                processingwindow.Hide();
            }
        }

        private void OnduleTopBarControl_KeyDown(object sender, KeyEventArgs e)
        {
            if( currUnit != null && currIdx != -1)
            {
                if(Control.ModifierKeys == Keys.ShiftKey)
                {
                    //If shift key was pressed
                }
            }
        }

    

        private void StiffnessRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            this.WDTitleLabel.Enabled = false;
            this.MinWDLabel.Enabled = false;
            this.MaxWDLabel.Enabled = false;
            this.TurnGapTitleLabel.Enabled = false;
            this.MinTGLabel.Enabled = false;
            this.MaxTGLabel.Enabled = false;
            this.WireDiameterTrackBar.Enabled = false;
            this.TurnGapTrackBar.Enabled = false;
            this.WDValueLabel.Enabled = false;
            this.TGValueLabel.Enabled = false;

            this.MinStiffnessLabel.Enabled = true;
            this.MaxStiffnessLabel.Enabled = true;
            this.StiffnessTrackBar.Enabled = true;
        }

        private void AdvancedRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            this.WDTitleLabel.Enabled = true;
            this.MinWDLabel.Enabled = true;
            this.MaxWDLabel.Enabled = true;
            this.TurnGapTitleLabel.Enabled = true;
            this.MinTGLabel.Enabled = true;
            this.MaxTGLabel.Enabled = true;
            this.WireDiameterTrackBar.Enabled = true;
            this.TurnGapTrackBar.Enabled = true;
            this.WDValueLabel.Enabled = true;
            this.TGValueLabel.Enabled = true;

            this.MinStiffnessLabel.Enabled = false;
            this.MaxStiffnessLabel.Enabled = false;
            this.StiffnessTrackBar.Enabled = false;
        }

        private void ConstraintCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            switch (currConstraintCtrl)
            {
                case 0:
                    {
                        // listening the mousedown event when the linear only mode is activated
                        if(Math.Sqrt(Math.Pow(e.X - linearCenterX, 2) + Math.Pow(e.Y - linearCenterY, 2)) <= linearRadius)
                        {
                            isLinearChange = true;
                            defaultStateX = e.X;
                        }
                    }break;
                case 1:
                    {
                        // listening the mousedown event when the twist only mode is activated
                        if (Math.Sqrt(Math.Pow(e.X - twistCenterX, 2) + Math.Pow(e.Y - twistCenterY, 2)) <= twistRadius)
                        {
                            isTwistAngle = true;
                        }
                    }
                    break;
                case 2:
                    {
                        // listening the mousedown event when the linear + twist mode is activated
                        if(Math.Sqrt(Math.Pow(e.X - ltLinearCenterX, 2) + Math.Pow(e.Y - ltLinearCenterY, 2)) <= ltLinearRadius)
                        {
                            isLTDisChange = true;
                            defaultStateX = e.X;
                        }

                        if(Math.Sqrt(Math.Pow(e.X - ltAngleCenterX, 2) + Math.Pow(e.Y - ltAngleCenterY, 2)) <= ltAngleRadius)
                        {
                            isLTAngle = true;
                        }
                    }
                    break;
                case 3:
                    {
                        // listening the mousedown event when the bend only mode is activated
                        if (!isAllDir)
                        {
                            if (Math.Sqrt(Math.Pow(e.X - bendDirCenterX, 2) + Math.Pow(e.Y - bendDirCenterY, 2)) <= bendDirRadius)
                            {
                                isBendDir = true;
                            }
                        }

                        if(Math.Sqrt(Math.Pow(e.X - bendAngleCenterX, 2) + Math.Pow(e.Y - bendAngleCenterY, 2)) <= bendAngleRadius)
                        {
                            isBendAngle = true;
                        }
                    }
                    break;
                case 4:
                    {
                        if(Math.Sqrt(Math.Pow(e.X - btBendAngleCenterX, 2) + Math.Pow(e.Y - btBendAngleCenterY, 2)) <= bendAngleRadius)
                        {
                            isBTBendAngle = true;
                        }
                    }
                    break;
                default:break;
            }
        }

        private void ConstraintCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            switch (currConstraintCtrl)
            {
                case 0:
                    {
                        double thickness = 3;
                        //double max_comp_real = (currUnit.SelectedSeg.GetLength() - 2 * thickness - printing_tolerance - tenDis) / 2;
                        //double max_ten_real = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * printing_tolerance - compDis;
                        //double max_comp_real = (currUnit.SelectedSeg.GetLength() - 2 * thickness -  printing_tolerance)/2;
                        //double max_ten_real = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * printing_tolerance;
                        double max_comp_real = (currUnit.SelectedSeg.GetLength() - 2 * thickness  - tenDis) / 2;
                        double max_ten_real = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * compDis;

                        double newMaxComp = (currUnit.SelectedSeg.GetLength() - 2 * thickness - printing_tolerance) / 2;
                        double newMaxExtension = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * printing_tolerance;

                        // listening the mousemove event when the linear only mode is activated
                        if (isLinearChange)
                        {
                            if(e.X == defaultStateX)
                            {
                                //compDis = 0;
                                //tenDis = 0;

                                compOffset = 0;
                                tenOffset = 0;
                            }
                            else if(e.X < defaultStateX)
                            {
                                //tenDis = 0;
                                tenOffset = 0;

                                double min_dis = printing_tolerance / newMaxComp * initialLen;
                                double real_max_comp_offset = max_comp_real / newMaxComp * initialLen;

                                if (defaultStateX - e.X <= min_dis)
                                {
                                    linearCenterX = defaultStateX - (float)min_dis;
                                    compOffset = (float)min_dis;
                                    compDis = printing_tolerance;

                                }
                                else if(defaultStateX - e.X <= real_max_comp_offset)
                                {
                                    linearCenterX = defaultStateX - (defaultStateX - e.X);
                                    compOffset = defaultStateX - e.X;
                                    double ratio = (defaultStateX - e.X) / real_max_comp_offset;
                                    compDis = ratio * max_comp_real;
                                }
                                else
                                {
                                    linearCenterX = defaultStateX - (float)real_max_comp_offset;
                                    compOffset = (float)real_max_comp_offset;
                                    compDis = max_comp_real;
                                }
                            }
                            else if(e.X > defaultStateX)
                            {
                                //compDis = 0;
                                compOffset = 0;

                                double min_dis = printing_tolerance / newMaxExtension * initialLen;
                                double real_max_ext_offset = max_ten_real / newMaxExtension * initialLen;

                                if (e.X - defaultStateX <= min_dis)
                                {
                                    linearCenterX = defaultStateX + (float)min_dis;
                                    tenOffset = (float)min_dis;
                                    tenDis = printing_tolerance;
                                }
                                else if (e.X - defaultStateX <= real_max_ext_offset)
                                {
                                    linearCenterX = defaultStateX + e.X - defaultStateX;
                                    tenOffset = e.X - defaultStateX;
                                    double ratio = (e.X - defaultStateX) / real_max_ext_offset;
                                    tenDis = ratio * max_ten_real;
                                }
                                else
                                {
                                    linearCenterX = defaultStateX + (float)real_max_ext_offset;
                                    tenOffset = (float)real_max_ext_offset;
                                    tenDis = max_ten_real;
                                }
                            }

                            //double newMaxComp = (currUnit.SelectedSeg.GetLength() - 2 * thickness - printing_tolerance - tenDis) / 2;
                            //double newMaxExtension = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * printing_tolerance - compDis;
                            
                            //double newMaxComp = (currUnit.SelectedSeg.GetLength() - 2 * thickness - tenDis) / 2;
                            //double newMaxExtension = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * compDis;

                            double compRatio = compDis / newMaxComp * 100;
                            double tenRatio = tenDis / newMaxExtension * 100;
                            compressDisValueLabel.Text = compDis.ToString(specifier) + " mm (" + compRatio.ToString(specifier) + "%)";
                            tensionDisValueLabel.Text = tenDis.ToString(specifier) + " mm (" + tenRatio.ToString(specifier) + "%)";

                            applyLoadValue = (compDis > tenDis ? compDis : tenDis) * Math.Pow(currUnit.WireDiameter, 4) * 1008 / (8 * Math.Pow(currUnit.MeanCoilDiameter, 3) * currUnit.CoilNum);
                            applyLoadValueLabel.Text = applyLoadValue.ToString(specifier) + " N";
                            this.ConstraintCanvas.Refresh();
                        }
                        
                    }
                    break;
                case 1:
                    {
                        // listening the mousemove event when the twist only mode is activated
                        if (isTwistAngle)
                        {
                            float new_X, new_Y;
                            float c_X = e.X;
                            float c_Y = e.Y;

                            if ((c_X - twist_angle_traj_centerX) > 0)
                            {
                                float k = (c_Y - twist_angle_traj_centerY) / (c_X - twist_angle_traj_centerX);
                                new_X = twist_angle_traj_centerX + twist_angle_traj_r / ((float)Math.Sqrt(1 + k * k));
                                new_Y = (new_X - twist_angle_traj_centerX) * k + twist_angle_traj_centerY;

                                twistCenterX = new_X;
                                twistCenterY = new_Y;

                                // update the twist angle
                                Vector2d v1 = new Vector2d(0, -twist_angle_traj_r);
                                Vector2d v2 = new Vector2d(twistCenterX - twist_angle_traj_centerX, twistCenterY - twist_angle_traj_centerY);

                                twist_angle = Math.Acos((v1.X * v2.X + v1.Y * v2.Y) / (v1.Length * v2.Length)) * 180 / Math.PI;

                                if (twist_angle > maxTwistingAngle)
                                {
                                    double ra_angle = maxTwistingAngle / 180 * Math.PI;
                                    twistCenterX = twist_angle_traj_centerX + (float)Math.Sin(ra_angle) * twist_angle_traj_r;
                                    twistCenterY = twist_angle_traj_centerY - (float)Math.Cos(ra_angle) * twist_angle_traj_r;
                                    twistValueLabel.Text = maxTwistingAngle.ToString(specifier1) + "°";
                                    twist_angle = maxTwistingAngle;
                                }
                                else
                                {
                                    twistValueLabel.Text = twist_angle.ToString(specifier1) + "°";
                                }

                                applyTorqueValue = twist_angle * Math.PI / 180 * 2825 * Math.Pow(currUnit.WireDiameter, 4) / (64 * currUnit.MeanCoilDiameter * currUnit.CoilNum) / (currUnit.MeanCoilDiameter / 2);
                                applyTorqueValueLabel.Text = applyTorqueValue.ToString(specifier) + " N";

                                this.ConstraintCanvas.Refresh();
                            }
                            else if ((c_X - twist_angle_traj_centerX) < 0)
                            {
                                float k = (c_Y - twist_angle_traj_centerY) / (c_X - twist_angle_traj_centerX);
                                new_X = twist_angle_traj_centerX - twist_angle_traj_r / ((float)Math.Sqrt(1 + k * k));
                                new_Y = (new_X - twist_angle_traj_centerX) * k + twist_angle_traj_centerY;

                                twistCenterX = new_X;
                                twistCenterY = new_Y;

                                //// update the twist angle
                                //Vector2d v1 = new Vector2d(0, -twist_angle_traj_r);
                                //Vector2d v2 = new Vector2d(twistCenterX - twist_angle_traj_centerX, twistCenterY - twist_angle_traj_centerY);
                                //twist_angle = 360 - Math.Acos((v1.X * v2.X + v1.Y * v2.Y) / (v1.Length * v2.Length)) * 180 / Math.PI;
                                //twistValueLabel.Text = twist_angle.ToString(specifier1) + "°";

                                //applyTorqueValue = twist_angle * Math.PI / 180 * 2825 * Math.Pow(currUnit.WireDiameter, 4) / (64 * currUnit.MeanCoilDiameter * currUnit.CoilNum) / (currUnit.MeanCoilDiameter / 2);
                                //applyTorqueValueLabel.Text = applyTorqueValue.ToString(specifier) + " N";

                                if (twistCenterX < twist_angle_traj_centerX && twistCenterY <= twist_angle_traj_centerY)
                                {
                                    twistCenterX = twist_angle_traj_centerX;
                                    twistCenterY = twist_angle_traj_centerY - twist_angle_traj_r;

                                    twist_angle = 0;
                                    twistValueLabel.Text = twist_angle.ToString(specifier1) + "°";
                                    applyTorqueValue = twist_angle * Math.PI / 180 * 2825 * Math.Pow(currUnit.WireDiameter, 4) / (64 * currUnit.MeanCoilDiameter * currUnit.CoilNum) / (currUnit.MeanCoilDiameter / 2);
                                    applyTorqueValueLabel.Text = applyTorqueValue.ToString(specifier) + " N";
                                }
                                else if (twistCenterX < twist_angle_traj_centerX && twistCenterY > twist_angle_traj_centerY)
                                {
                                    double ra_angle = maxTwistingAngle / 180 * Math.PI;
                                    twistCenterX = twist_angle_traj_centerX + (float)Math.Sin(ra_angle) * twist_angle_traj_r;
                                    twistCenterY = twist_angle_traj_centerY - (float)Math.Cos(ra_angle) * twist_angle_traj_r;
                                    twist_angle = maxTwistingAngle;

                                    twistValueLabel.Text = twist_angle.ToString(specifier1) + "°";
                                    applyTorqueValue = twist_angle * Math.PI / 180 * 2825 * Math.Pow(currUnit.WireDiameter, 4) / (64 * currUnit.MeanCoilDiameter * currUnit.CoilNum) / (currUnit.MeanCoilDiameter / 2);
                                    applyTorqueValueLabel.Text = applyTorqueValue.ToString(specifier) + " N";
                                }

                                this.ConstraintCanvas.Refresh();
                            }
                        }
                    }
                    break;
                case 2:
                    {
                        // listening the mousemove event when the linear + twist mode is activated
                        double thickness = 3;

                        double max_comp_real = (currUnit.SelectedSeg.GetLength() - 2 * thickness - tenDis) / 2;
                        double max_ten_real = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * compDis;

                        double newMaxComp = (currUnit.SelectedSeg.GetLength() - 2 * thickness - printing_tolerance) / 2;
                        double newMaxExtension = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * printing_tolerance;

                        //double max_comp_real = (currUnit.MA.GetLength() - 2 * thickness - printing_tolerance - lt_tenDis) / 2;
                        //double max_ten_real = currUnit.MA.GetLength() - 2 * thickness - 2 * printing_tolerance - lt_compDis;

                        if (isLTDisChange)
                        {
                            if (e.X == defaultStateX)
                            {
                                //lt_compDis = 0;
                                //lt_tenDis = 0;

                                lt_compOffset = 0;
                                lt_tenOffset = 0;
                            }
                            else if (e.X < defaultStateX)
                            {
                                //lt_tenDis = 0;
                                lt_tenOffset = 0;

                                //double min_dis = printing_tolerance / max_comp_real * initialLen;
                                double min_dis = printing_tolerance / newMaxComp * initialLen;
                                double real_max_comp_offset = max_comp_real / newMaxComp * initialLen;

                                if (defaultStateX - e.X <= min_dis)
                                {
                                    ltLinearCenterX = defaultStateX - (float)min_dis;
                                    lt_compOffset = (float)min_dis;
                                    lt_compDis = printing_tolerance;
                                }
                                else if (defaultStateX - e.X <= real_max_comp_offset)
                                {
                                    ltLinearCenterX = defaultStateX - (defaultStateX - e.X);
                                    lt_compOffset = defaultStateX - e.X;
                                    double ratio = (defaultStateX - e.X) / real_max_comp_offset;
                                    lt_compDis = ratio * max_comp_real;
                                }
                                else
                                {
                                    ltLinearCenterX = defaultStateX - (float)real_max_comp_offset;
                                    lt_compOffset = (float)real_max_comp_offset;
                                    lt_compDis = max_comp_real;
                                }
                            }
                            else if (e.X > defaultStateX)
                            {
                                //lt_compDis = 0;
                                lt_compOffset = 0;

                                //double min_dis = printing_tolerance / max_ten_real * initialLen;
                                double min_dis = printing_tolerance / newMaxExtension * initialLen;
                                double real_max_ext_offset = max_ten_real / newMaxExtension * initialLen;

                                if (e.X - defaultStateX <= min_dis)
                                {
                                    ltLinearCenterX = defaultStateX + (float)min_dis;
                                    lt_tenOffset = (float)min_dis;
                                    lt_tenDis = printing_tolerance;
                                }
                                else if (e.X - defaultStateX <= real_max_ext_offset)
                                {
                                    ltLinearCenterX = defaultStateX + e.X - defaultStateX;
                                    lt_tenOffset = e.X - defaultStateX;
                                    double ratio = (e.X - defaultStateX) / real_max_ext_offset;
                                    lt_tenDis = ratio * max_ten_real;
                                }
                                else
                                {
                                    ltLinearCenterX = defaultStateX + (float)real_max_ext_offset;
                                    lt_tenOffset = (float)real_max_ext_offset;
                                    lt_tenDis = max_ten_real;
                                }
                            }

                            //double new_max_comp_real = (currUnit.MA.GetLength() - 2 * thickness - printing_tolerance - lt_tenDis) / 2;
                            //double new_max_ten_real = currUnit.MA.GetLength() - 2 * thickness - 2 * printing_tolerance - lt_compDis;

                            //double lt_compRatio = lt_compDis / new_max_comp_real * 100;
                            //double lt_tenRatio = lt_tenDis / new_max_ten_real * 100;

                            double lt_compRatio = lt_compDis / newMaxComp * 100;
                            double lt_tenRatio = lt_tenDis / newMaxExtension * 100;
                            ltCompressDisValueLabel.Text = lt_compDis.ToString(specifier) + " mm (" + lt_compRatio.ToString(specifier) + "%)";
                            ltTensionDisValueLabel.Text = lt_tenDis.ToString(specifier) + " mm (" + lt_tenRatio.ToString(specifier) + "%)";

                            lt_applyLoadValue = (lt_compDis > lt_tenDis ? lt_compDis : lt_tenDis) * Math.Pow(currUnit.WireDiameter, 4) * 1008 / (8 * Math.Pow(currUnit.MeanCoilDiameter, 3) * currUnit.CoilNum);
                            lt_applyLoadValueLabel.Text = lt_applyLoadValue.ToString(specifier) + " N";
                            this.ConstraintCanvas.Refresh();
                        }

                        if (isLTAngle)
                        {
                            float new_X, new_Y;
                            float c_X = e.X;
                            float c_Y = e.Y;

                            if ((c_X - lt_twist_angle_traj_centerX) > 0)
                            {
                                float k = (c_Y - lt_twist_angle_traj_centerY) / (c_X - lt_twist_angle_traj_centerX);
                                new_X = lt_twist_angle_traj_centerX + lt_twist_angle_traj_r / ((float)Math.Sqrt(1 + k * k));
                                new_Y = (new_X - lt_twist_angle_traj_centerX) * k + lt_twist_angle_traj_centerY;

                                ltAngleCenterX = new_X;
                                ltAngleCenterY = new_Y;

                                // update the bend direction angle
                                Vector2d v1 = new Vector2d(0, -lt_twist_angle_traj_r);
                                Vector2d v2 = new Vector2d(ltAngleCenterX - lt_twist_angle_traj_centerX, ltAngleCenterY - lt_twist_angle_traj_centerY);
                                lt_twist_angle = Math.Acos((v1.X * v2.X + v1.Y * v2.Y) / (v1.Length * v2.Length)) * 180 / Math.PI;
                                ltTwistValueLabel.Text = lt_twist_angle.ToString(specifier1) + "°";

                                this.ConstraintCanvas.Refresh();
                            }
                            else if ((c_X - lt_twist_angle_traj_centerX) < 0)
                            {
                                float k = (c_Y - lt_twist_angle_traj_centerY) / (c_X - lt_twist_angle_traj_centerX);
                                new_X = lt_twist_angle_traj_centerX - lt_twist_angle_traj_r / ((float)Math.Sqrt(1 + k * k));
                                new_Y = (new_X - lt_twist_angle_traj_centerX) * k + lt_twist_angle_traj_centerY;

                                ltAngleCenterX = new_X;
                                ltAngleCenterY = new_Y;

                                // update the bend direction angle
                                Vector2d v1 = new Vector2d(0, -lt_twist_angle_traj_r);
                                Vector2d v2 = new Vector2d(ltAngleCenterX - lt_twist_angle_traj_centerX, ltAngleCenterY - lt_twist_angle_traj_centerY);
                                lt_twist_angle = 360 - Math.Acos((v1.X * v2.X + v1.Y * v2.Y) / (v1.Length * v2.Length)) * 180 / Math.PI;
                                ltTwistValueLabel.Text = lt_twist_angle.ToString(specifier1) + "°";

                                this.ConstraintCanvas.Refresh();
                            }
                        }
                    }
                    break;
                case 3:
                    {
                        // listening the mousemove event when the bend only mode is activated
                        if (isBendDir)
                        {
                            float new_X, new_Y;
                            float c_X = e.X;
                            float c_Y = e.Y;

                            if((c_X - bend_dir_traj_centerX) > 0)
                            {
                                float k = (c_Y - bend_dir_traj_centerY) / (c_X - bend_dir_traj_centerX);
                                new_X = bend_dir_traj_centerX + bend_dir_traj_r / ((float)Math.Sqrt(1 + k*k));
                                new_Y = (new_X - bend_dir_traj_centerX) * k + bend_dir_traj_centerY;

                                bendDirCenterX = new_X;
                                bendDirCenterY = new_Y;

                                // update the bend direction angle
                                Vector2d v1 = new Vector2d(0, -bend_dir_traj_r);
                                Vector2d v2 = new Vector2d(bendDirCenterX - bend_dir_traj_centerX, bendDirCenterY - bend_dir_traj_centerY);
                                bend_dir_angle = Math.Acos((v1.X * v2.X + v1.Y * v2.Y) / (v1.Length * v2.Length))*180/Math.PI;
                                bendDirValueLabel.Text = bend_dir_angle.ToString(specifier1) + "°";

                                this.ConstraintCanvas.Refresh();
                            }
                            else if((c_X - bend_dir_traj_centerX) < 0)
                            {
                                float k = (c_Y - bend_dir_traj_centerY) / (c_X - bend_dir_traj_centerX);
                                new_X = bend_dir_traj_centerX - bend_dir_traj_r / ((float)Math.Sqrt(1 + k * k));
                                new_Y = (new_X - bend_dir_traj_centerX) * k + bend_dir_traj_centerY;

                                bendDirCenterX = new_X;
                                bendDirCenterY = new_Y;

                                // update the bend direction angle
                                Vector2d v1 = new Vector2d(0, -bend_dir_traj_r);
                                Vector2d v2 = new Vector2d(bendDirCenterX - bend_dir_traj_centerX, bendDirCenterY - bend_dir_traj_centerY);
                                bend_dir_angle = 360 - Math.Acos((v1.X * v2.X + v1.Y * v2.Y) / (v1.Length * v2.Length))*180/Math.PI;
                                bendDirValueLabel.Text = bend_dir_angle.ToString(specifier1) + "°";

                                this.ConstraintCanvas.Refresh();
                            }

                            currUnit.BendDirAngle = bend_dir_angle/180*Math.PI;
                            controller.updateUnitFromGlobal(currIdx, currUnit);
                            controller.updateInPlaneBendDir(currUnit);
                        }

                        if (isBendAngle)
                        {
                            float new_X, new_Y;
                            float c_X = e.X;
                            float c_Y = e.Y;

                            if ((c_X - bend_angle_traj_centerX) > 0)
                            {
                                float k = (c_Y - bend_angle_traj_centerY) / (c_X - bend_angle_traj_centerX);
                                new_X = bend_angle_traj_centerX + bend_angle_traj_r / ((float)Math.Sqrt(1 + k * k));
                                new_Y = (new_X - bend_angle_traj_centerX) * k + bend_angle_traj_centerY;

                                bendAngleCenterX = new_X;
                                bendAngleCenterY = new_Y;

                                if (bendAngleCenterX < bend_angle_traj_centerX && bendAngleCenterY <= bend_angle_traj_centerY)
                                {
                                    bendAngleCenterX = bend_angle_traj_centerX;
                                    bendAngleCenterY = bend_angle_traj_centerY - bend_angle_traj_r;
                                }
                                else if (bendAngleCenterX < bend_angle_traj_centerX && bendAngleCenterY > bend_angle_traj_centerY)
                                {
                                    bendAngleCenterX = bend_angle_traj_centerX;
                                    bendAngleCenterY = bend_angle_traj_centerY + bend_angle_traj_r;
                                }

                                Vector2d v1 = new Vector2d(0, -bend_angle_traj_r);
                                Vector2d v2 = new Vector2d(bendAngleCenterX - bend_angle_traj_centerX, bendAngleCenterY - bend_angle_traj_centerY);
                                bend_angle = Math.Acos((v1.X * v2.X + v1.Y * v2.Y) / (v1.Length * v2.Length)) * 180 / Math.PI;

                                if(bend_angle > maxBendingAngle)
                                {
                                    double ra_angle = maxBendingAngle / 180 * Math.PI;
                                    bendAngleCenterX = bend_angle_traj_centerX + (float)Math.Sin(ra_angle) * bend_angle_traj_r;
                                    bendAngleCenterY = bend_angle_traj_centerY - (float)Math.Cos(ra_angle) * bend_angle_traj_r;
                                    bendAngleValueLabel.Text = maxBendingAngle.ToString(specifier1) + "°";
                                    bend_angle = maxBendingAngle;
                                }
                                else
                                {
                                    bendAngleValueLabel.Text = bend_angle.ToString(specifier1) + "°";
                                }
                                

                                this.ConstraintCanvas.Refresh();
                            }
                            else if ((c_X - bend_angle_traj_centerX) < 0)
                            {
                                float k = (c_Y - bend_angle_traj_centerY) / (c_X - bend_angle_traj_centerX);
                                new_X = bend_angle_traj_centerX - bend_angle_traj_r / ((float)Math.Sqrt(1 + k * k));
                                new_Y = (new_X - bend_angle_traj_centerX) * k + bend_angle_traj_centerY;

                                bendAngleCenterX = new_X;
                                bendAngleCenterY = new_Y;

                                if (bendAngleCenterX < bend_angle_traj_centerX && bendAngleCenterY <= bend_angle_traj_centerY)
                                {
                                    bendAngleCenterX = bend_angle_traj_centerX;
                                    bendAngleCenterY = bend_angle_traj_centerY - bend_angle_traj_r;

                                    bend_angle = 0;
                                    bendAngleValueLabel.Text = bend_angle.ToString(specifier1) + "°";
                                }
                                else if (bendAngleCenterX < bend_angle_traj_centerX && bendAngleCenterY > bend_angle_traj_centerY)
                                {
                                    bendAngleCenterX = bend_angle_traj_centerX;
                                    bendAngleCenterY = bend_angle_traj_centerY + bend_angle_traj_r;

                                    bend_angle = 180;
                                    bendAngleValueLabel.Text = bend_angle.ToString(specifier1) + "°";
                                }

                                this.ConstraintCanvas.Refresh();
                            }
                        }
                    }
                    break;
                case 4:
                    {
                        if (isBTBendAngle)
                        {
                            float new_X, new_Y;
                            float c_X = e.X;
                            float c_Y = e.Y;

                            if ((c_X - bt_bend_angle_traj_centerX) > 0)
                            {
                                float k = (c_Y - bt_bend_angle_traj_centerY) / (c_X - bt_bend_angle_traj_centerX);
                                new_X = bt_bend_angle_traj_centerX + bend_angle_traj_r / ((float)Math.Sqrt(1 + k * k));
                                new_Y = (new_X - bt_bend_angle_traj_centerX) * k + bt_bend_angle_traj_centerY;

                                btBendAngleCenterX = new_X;
                                btBendAngleCenterY = new_Y;

                                if (btBendAngleCenterX < bt_bend_angle_traj_centerX && btBendAngleCenterY <= bt_bend_angle_traj_centerY)
                                {
                                    btBendAngleCenterX = bt_bend_angle_traj_centerX;
                                    btBendAngleCenterY = bt_bend_angle_traj_centerY - bend_angle_traj_r;
                                }
                                else if (btBendAngleCenterX < bt_bend_angle_traj_centerX && btBendAngleCenterY > bt_bend_angle_traj_centerY)
                                {
                                    btBendAngleCenterX = bt_bend_angle_traj_centerX;
                                    btBendAngleCenterY = bt_bend_angle_traj_centerY + bend_angle_traj_r;
                                }

                                Vector2d v1 = new Vector2d(0, -bend_angle_traj_r);
                                Vector2d v2 = new Vector2d(btBendAngleCenterX - bt_bend_angle_traj_centerX, btBendAngleCenterY - bt_bend_angle_traj_centerY);
                                bt_bend_angle = Math.Acos((v1.X * v2.X + v1.Y * v2.Y) / (v1.Length * v2.Length)) * 180 / Math.PI;

                                if (bt_bend_angle > maxBendingAngle)
                                {
                                    double ra_angle = maxBendingAngle / 180 * Math.PI;
                                    btBendAngleCenterX = bt_bend_angle_traj_centerX + (float)Math.Sin(ra_angle) * bend_angle_traj_r;
                                    btBendAngleCenterY = bt_bend_angle_traj_centerY - (float)Math.Cos(ra_angle) * bend_angle_traj_r;
                                    btBendAngleValueLabel.Text = maxBendingAngle.ToString(specifier1) + "°";
                                    bt_bend_angle = maxBendingAngle;
                                }
                                else
                                {
                                    btBendAngleValueLabel.Text = bt_bend_angle.ToString(specifier1) + "°";
                                }

                                this.ConstraintCanvas.Refresh();
                            }
                            else if ((c_X - bt_bend_angle_traj_centerX) < 0)
                            {
                                float k = (c_Y - bt_bend_angle_traj_centerY) / (c_X - bt_bend_angle_traj_centerX);
                                new_X = bt_bend_angle_traj_centerX - bend_angle_traj_r / ((float)Math.Sqrt(1 + k * k));
                                new_Y = (new_X - bt_bend_angle_traj_centerX) * k + bt_bend_angle_traj_centerY;

                                btBendAngleCenterX = new_X;
                                btBendAngleCenterY = new_Y;

                                if (btBendAngleCenterX < bt_bend_angle_traj_centerX && btBendAngleCenterY <= bt_bend_angle_traj_centerY)
                                {
                                    btBendAngleCenterX = bt_bend_angle_traj_centerX;
                                    btBendAngleCenterY = bt_bend_angle_traj_centerY - bend_angle_traj_r;

                                    bt_bend_angle = 0;
                                    btBendAngleValueLabel.Text = bt_bend_angle.ToString(specifier1) + "°";
                                }
                                else if (btBendAngleCenterX < bt_bend_angle_traj_centerX && btBendAngleCenterY > bt_bend_angle_traj_centerY)
                                {
                                    btBendAngleCenterX = bt_bend_angle_traj_centerX;
                                    btBendAngleCenterY = bt_bend_angle_traj_centerY + bend_angle_traj_r;

                                    bt_bend_angle = 180;
                                    btBendAngleValueLabel.Text = bt_bend_angle.ToString(specifier1) + "°";
                                }

                                this.ConstraintCanvas.Refresh();
                            }
                        }  
                    }
                    break;
                default: break;
            }
        }

        private void ConstraintCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            switch (currConstraintCtrl)
            {
                case 0:
                    {
                        // listening the mouseup event when the linear only mode is activated
                        if (isLinearChange)
                        {
                            isLinearChange = !isLinearChange;
                            double thickness = 3;
                            //double max_comp_real = (currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * printing_tolerance - tenDis) / 2;
                            //double max_ten_real = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * printing_tolerance - compDis;
                            //double max_comp_real = (currUnit.SelectedSeg.GetLength() - 2 * thickness - printing_tolerance)/2;
                            //double max_ten_real = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * printing_tolerance;
                            double max_comp_real = (currUnit.SelectedSeg.GetLength() - 2 * thickness - tenDis) / 2;
                            double max_ten_real = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * compDis;

                            double newMaxComp = (currUnit.SelectedSeg.GetLength() - 2 * thickness - printing_tolerance) / 2;
                            double newMaxExtension = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * printing_tolerance;

                            compOffset = 0;
                            tenOffset = 0;
                            linearCenterX = 117;
                            defaultStateX = 117;

                            double min_dis = printing_tolerance / newMaxComp * initialLen;

                            if (compDis <= min_dis)
                            {
                                compOffset = (float)min_dis;
                                compDis = printing_tolerance;
                            }

                            min_dis = printing_tolerance / newMaxExtension * initialLen;

                            if (tenDis <= min_dis)
                            {
                                tenOffset = (float)min_dis;
                                tenDis = printing_tolerance;
                            }

                            applyLoadValue = (compDis > tenDis ? compDis : tenDis) * Math.Pow(currUnit.WireDiameter, 4) * 1008 / (8 * Math.Pow(currUnit.MeanCoilDiameter, 3) * currUnit.CoilNum);
                            applyLoadValueLabel.Text = applyLoadValue.ToString(specifier) + " N";
                            this.ConstraintCanvas.Refresh();


                            // Update the prismatic joint on the model
                            // Get the current compression displacement from LinearConsCompressTrackbar
                            processingwindow.Show();
                            processingwindow.Refresh();

                            currUnit.CompressionDis = compDis;
                            // Get the current extension displacement from LinearConsStretchTrackbar
                            currUnit.ExtensionDis = tenDis;

                            controller.updateUnitFromGlobal(currIdx, currUnit);
                            controller.addLinearConstraint(ref currUnit);
                            
                            processingwindow.Hide();
                        }
                    }
                    break;
                case 1:
                    {
                        // listening the mouseup event when the twist only mode is activated
                        if (isTwistAngle)
                        {
                            isTwistAngle = !isTwistAngle;
                            currUnit.TwistAngle = twist_angle;
                            applyTorqueValue = currUnit.TwistAngle * Math.PI / 180 * 2825 * Math.Pow(currUnit.WireDiameter, 4) / (64 * currUnit.MeanCoilDiameter * currUnit.CoilNum) / (currUnit.MeanCoilDiameter / 2);
                            applyTorqueValueLabel.Text = applyTorqueValue.ToString(specifier) + " N";
                            this.ConstraintCanvas.Refresh();

                            // Update the bearing mechanism on the model
                            processingwindow.Show();
                            processingwindow.Refresh();

                            controller.updateUnitFromGlobal(currIdx, currUnit);
                            controller.addTwistConstraint(ref currUnit);
                            processingwindow.Hide();
                        }
                    }
                    break;
                case 2:
                    {
                        bool needUpdate = false;
                        // listening the mouseup event when the linear + twist mode is activated
                        if (isLTDisChange)
                        {
                            isLTDisChange = !isLTDisChange;
                            double thickness = 3;
                            double max_comp_real = (currUnit.MA.GetLength() - 2 * thickness - printing_tolerance - lt_tenDis) / 2;
                            double max_ten_real = currUnit.MA.GetLength() - 2 * thickness - 2 * printing_tolerance - lt_compDis;

                            lt_compOffset = 0;
                            lt_tenOffset = 0;
                            ltLinearCenterX = 117;
                            defaultStateX = 117;

                            double min_dis = printing_tolerance / max_comp_real * initialLen;

                            if (lt_compDis <= min_dis)
                            {
                                lt_compOffset = (float)min_dis;
                                lt_compDis = printing_tolerance;
                            }

                            min_dis = printing_tolerance / max_ten_real * initialLen;

                            if (lt_tenDis <= min_dis)
                            {
                                lt_tenOffset = (float)min_dis;
                                lt_tenDis = printing_tolerance;
                            }

                            needUpdate = true;

                            lt_applyLoadValue = (lt_compDis > lt_tenDis ? lt_compDis : lt_tenDis) * Math.Pow(currUnit.WireDiameter, 4) * 892 / (8 * Math.Pow(currUnit.MeanCoilDiameter, 3) * currUnit.CoilNum);
                            lt_applyLoadValueLabel.Text = applyLoadValue.ToString(specifier) + "N";
                            this.ConstraintCanvas.Refresh();
                        }

                        if (isLTAngle)
                        {
                            isLTAngle = !isLTAngle;
                            needUpdate = true;
                        }

                        if (needUpdate)
                        {
                            // Update the prismatic joint + bearing mechanism on the model
                             processingwindow.Show();
                            processingwindow.Refresh();

                            // Get the current compression displacement from LinearConsCompressTrackbar
                            currUnit.CompressionDis = lt_compDis;
                            // Get the current extension displacement from LinearConsStretchTrackbar
                            currUnit.ExtensionDis = lt_tenDis;
                            // Get the current twist angle from TwistTrackbar
                            currUnit.TwistAngle = lt_twist_angle / 180 * Math.PI;

                            controller.updateUnitFromGlobal(currIdx, currUnit);
                            controller.addLinearTwistConstraint(ref currUnit);
                           
                            processingwindow.Hide();
                        }
                    }
                    break;
                case 3:
                    {
                        // listening the mouseup event when the bend only mode is activated
                        bool needUpdate = false;

                        if (isBendDir)
                        {
                            isBendDir = !isBendDir;
                            needUpdate = true;
                        }
                        if (isBendAngle)
                        {
                            isBendAngle = !isBendAngle;
                            needUpdate = true;
                        }

                        if (needUpdate)
                        {
                            // Update the chain mechanism on the model

                            processingwindow.Show();
                            processingwindow.Refresh();

                            currUnit.BendDirAngle = bend_dir_angle/180*Math.PI;
                            currUnit.BendAngle = bend_angle / 180 * Math.PI;
                            controller.updateUnitFromGlobal(currIdx, currUnit);
                            controller.addBendConstraint(ref currUnit, isAllDir);

                            controller.hideBendDirOrbit(currUnit);
                            processingwindow.Hide();
                        }
                        // apply the selected angle
                    }
                    break;
                case 4:
                    {
                        bool needUpdate = false;
                        if (isBTBendAngle)
                        {
                            isBTBendAngle = !isBTBendAngle;
                            needUpdate = true;
                        }

                        if (needUpdate)
                        {
                            // Update the chain mechanism on the model

                            processingwindow.Show();
                            processingwindow.Refresh();

                            currUnit.BendAngle = bt_bend_angle / 180 * Math.PI;
                            controller.updateUnitFromGlobal(currIdx, currUnit);
                            controller.addBendConstraint(ref currUnit, isAllDir);

                            controller.hideBendDirOrbit(currUnit);
                            processingwindow.Hide();
                        }
                    }
                    break;
                default: break;
            }
        }

        private void ConstraintCanvas_Paint(object sender, PaintEventArgs e)
        {
            
            switch (currConstraintCtrl)
            {
                case 0:
                    {
                        // Linear only interface
                        double thickness = 3;
                        //double max_comp_real = (currUnit.SelectedSeg.GetLength() - 2 * thickness - printing_tolerance) / 2;
                        //double max_ten_real = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * printing_tolerance;
                        //double max_comp_real = (currUnit.SelectedSeg.GetLength() - 2 * thickness - printing_tolerance)/2;
                        //double max_ten_real = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * printing_tolerance;
                        double max_comp_real = (currUnit.SelectedSeg.GetLength() - 2 * thickness - tenDis) / 2;
                        double max_ten_real = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * compDis;

                        double newMaxComp = (currUnit.SelectedSeg.GetLength() - 2 * thickness - printing_tolerance) / 2;
                        double newMaxExtension = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * printing_tolerance;

                        Rectangle rectangle = e.ClipRectangle;
                        BufferedGraphicsContext GraphicsContext = BufferedGraphicsManager.Current;
                        BufferedGraphics myBuffer = GraphicsContext.Allocate(e.Graphics, e.ClipRectangle);
                        Graphics g = myBuffer.Graphics;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.Clear(BackColor);
                        SolidBrush bkBrush = new SolidBrush(Color.FromArgb(224, 224, 224));
                        g.FillRectangle(bkBrush, rectangle);

                        Pen p = new Pen(Color.FromArgb(158, 158, 158), 4);
                        Pen p_spring = new Pen(Color.FromArgb(158, 158, 158), 2);
                        Pen p_angle = new Pen(Color.FromArgb(108, 180, 241), 1);
                        Pen p_angle1 = new Pen(Color.FromArgb(108, 180, 241), 3);
                        Pen b_outline = new Pen(Color.FromArgb(255, 255, 255), 2);
                        SolidBrush blockBrush = new SolidBrush(Color.FromArgb(110, 159, 239));

                        float l_centerX = linearCenterX;
                        float l_centerY = linearCenterY;
                        float l_radius = linearRadius;
                        float l_l, l_t, l_w, l_h;
                        circle_drawing_coordinates_conversion(l_centerX, l_centerY, l_radius, out l_l, out l_t, out l_w, out l_h);
                        
                        // Draw the compression line and the tension line
                        SolidBrush p_compress = new SolidBrush(Color.FromArgb(125, 87, 167, 242));
                        SolidBrush p_tension = new SolidBrush(Color.FromArgb(125, 223, 199, 41));

                        float c_dis = (float)(compDis / newMaxComp * initialLen);
                        float t_dis = (float)(tenDis / newMaxExtension * initialLen);

                        if (c_dis != 0)
                        {
                            g.FillRectangle(p_compress, 117 - c_dis, 20, c_dis, 70);
                        }

                        if (t_dis != 0)
                        {
                            g.FillRectangle(p_tension, 117, 20, t_dis, 70);
                        }

                        g.DrawLine(p, new PointF(20, 90), new PointF(20, 20));
                        g.DrawLine(p_spring, new PointF(20, 55), new PointF(25,55));
                        // g.DrawLine(p_spring, new PointF(linearCenterX - linearRadius-5, 55), new PointF(linearCenterX-linearRadius, 55));
                        

                        double gap = (initialLen - compOffset + tenOffset + linearRadius)/20;
                        double offset1 = 0;
                        double offset2 = 0;
                        double amp = 10;

                        for(int i = 0; i<20; i++)
                        {
                            int left = i % 4;

                            if (left == 0)
                            {
                                offset1 = 0;
                                offset2 = amp*(-1);
                            }   
                            else if (left == 1)
                            {
                                offset1 = amp * (-1);
                                offset2 = 0;
                            }      
                            else if (left == 2)
                            {
                                offset1 = 0;
                                offset2 = amp;
                            }
                            else if(left == 3)
                            {
                                offset1 = amp;
                                offset2 = 0;
                            }

                            g.DrawLine(p_spring, new PointF((float)(25 + i * gap), (float)(linearCenterY + offset1)), new PointF((float)(25 + (i + 1) * gap), (float)(linearCenterY+offset2)));
                        }

                        g.DrawEllipse(b_outline, l_l, l_t, l_w, l_h);
                        g.FillEllipse(blockBrush, l_l, l_t, l_w, l_h);

                        myBuffer.Render(e.Graphics);
                        g.Dispose();
                        myBuffer.Dispose();
                    }
                    break;
                case 1:
                    {
                        // Twist only interface

                        Rectangle rectangle = e.ClipRectangle;
                        BufferedGraphicsContext GraphicsContext = BufferedGraphicsManager.Current;
                        BufferedGraphics myBuffer = GraphicsContext.Allocate(e.Graphics, e.ClipRectangle);
                        Graphics g = myBuffer.Graphics;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.Clear(BackColor);
                        SolidBrush bkBrush = new SolidBrush(Color.FromArgb(224, 224, 224));
                        g.FillRectangle(bkBrush, rectangle);

                        Pen p = new Pen(Color.FromArgb(158, 158, 158), 2);
                        Pen p_angle = new Pen(Color.FromArgb(108, 180, 241), 1);
                        Pen p_angle1 = new Pen(Color.FromArgb(108, 180, 241), 3);
                        Pen b_outline = new Pen(Color.FromArgb(255, 255, 255), 2);
                        SolidBrush blockBrush = new SolidBrush(Color.FromArgb(110, 159, 239));
                        SolidBrush disabledBrush = new SolidBrush(Color.FromArgb(200, 200, 200));

                        float t_centerX = twistCenterX;
                        float t_centerY = twistCenterY;
                        float t_r = twistRadius;

                        float traj_l, traj_t, traj_w, traj_h;
                        float t_l, t_t, t_w, t_h;
                        circle_drawing_coordinates_conversion(twist_angle_traj_centerX, twist_angle_traj_centerY, twist_angle_traj_r, out traj_l, out traj_t, out traj_w, out traj_h);
                        circle_drawing_coordinates_conversion(t_centerX, t_centerY, t_r, out t_l, out t_t, out t_w, out t_h);

                        g.FillPie(disabledBrush, new Rectangle((int)twist_angle_traj_centerX - (int)twist_angle_traj_r, (int)twist_angle_traj_centerY - (int)twist_angle_traj_r, 2 * (int)twist_angle_traj_r, 2 * (int)twist_angle_traj_r), -90 + (float)maxTwistingAngle, 290-(float)maxTwistingAngle);
                        g.FillPie(disabledBrush, new Rectangle((int)twist_angle_traj_centerX - (int)twist_angle_traj_r, (int)twist_angle_traj_centerY - (int)twist_angle_traj_r, 2 * (int)twist_angle_traj_r, 2 * (int)twist_angle_traj_r), -180, 90);

                        g.DrawEllipse(p, traj_l, traj_t, traj_w, traj_h);
                        g.DrawLine(p_angle, new PointF(twist_angle_traj_centerX, twist_angle_traj_centerY), new PointF(twist_angle_traj_centerX, twist_angle_traj_centerY - twist_angle_traj_r));
                        g.DrawArc(p_angle1, twist_angle_traj_centerX - twist_angle_traj_r, twist_angle_traj_centerY - twist_angle_traj_r, 2 * twist_angle_traj_r, 2 * twist_angle_traj_r, -90, (float)twist_angle);
                        g.DrawLine(p_angle, new PointF(twist_angle_traj_centerX, twist_angle_traj_centerY), new PointF(twistCenterX, twistCenterY));
                        g.DrawEllipse(b_outline, t_l, t_t, t_w, t_h);
                        g.FillEllipse(blockBrush, t_l, t_t, t_w, t_h);

                        myBuffer.Render(e.Graphics);
                        g.Dispose();
                        myBuffer.Dispose();

                    }
                    break;
                case 2:
                    {
                        // Linear + twist interface

                        double thickness = 3;

                        double max_comp_real = (currUnit.SelectedSeg.GetLength() - 2 * thickness - tenDis) / 2;
                        double max_ten_real = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * compDis;

                        double newMaxComp = (currUnit.SelectedSeg.GetLength() - 2 * thickness - printing_tolerance) / 2;
                        double newMaxExtension = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * printing_tolerance;
                        

                        //double max_comp_real = (currUnit.MA.GetLength() - 2 * thickness - printing_tolerance) / 2;
                        //double max_ten_real = currUnit.MA.GetLength() - 2 * thickness - 2 * printing_tolerance;

                        Rectangle rectangle = e.ClipRectangle;
                        BufferedGraphicsContext GraphicsContext = BufferedGraphicsManager.Current;
                        BufferedGraphics myBuffer = GraphicsContext.Allocate(e.Graphics, e.ClipRectangle);
                        Graphics g = myBuffer.Graphics;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.Clear(BackColor);
                        SolidBrush bkBrush = new SolidBrush(Color.FromArgb(224, 224, 224));
                        g.FillRectangle(bkBrush, rectangle);

                        Pen p = new Pen(Color.FromArgb(158, 158, 158), 4);
                        Pen p_spring = new Pen(Color.FromArgb(158, 158, 158), 2);
                        Pen p_angle = new Pen(Color.FromArgb(108, 180, 241), 1);
                        Pen p_angle1 = new Pen(Color.FromArgb(108, 180, 241), 3);
                        Pen b_outline = new Pen(Color.FromArgb(255, 255, 255), 2);
                        SolidBrush blockBrush = new SolidBrush(Color.FromArgb(110, 159, 239));

                        float lt_centerX = ltLinearCenterX;
                        float lt_centerY = ltLinearCenterY;
                        float lt_radius = ltLinearRadius;
                        float lt_l, lt_t, lt_w, lt_h;
                        circle_drawing_coordinates_conversion(lt_centerX, lt_centerY, lt_radius, out lt_l, out lt_t, out lt_w, out lt_h);

                        // Draw the compression line and the tension line
                        SolidBrush p_compress = new SolidBrush(Color.FromArgb(125, 87, 167, 242));
                        SolidBrush p_tension = new SolidBrush(Color.FromArgb(125, 223, 199, 41));


                        float c_dis = (float)(lt_compDis / max_comp_real * initialLen);
                        float t_dis = (float)(lt_tenDis / max_ten_real * initialLen);

                        if (c_dis != 0)
                        {
                            g.FillRectangle(p_compress, 117 - c_dis, 20, c_dis, 40);
                        }
                        if (t_dis != 0)
                        {
                            g.FillRectangle(p_tension, 117, 20, t_dis, 40);
                        }

                        g.DrawLine(p, new PointF(20, 60), new PointF(20, 20));
                        g.DrawLine(p_spring, new PointF(20, 40), new PointF(25, 40));
                        g.DrawLine(p_spring, new PointF(ltLinearCenterX - ltLinearRadius - 5, ltLinearCenterY), new PointF(ltLinearCenterX - ltLinearRadius, ltLinearCenterY));
                        g.DrawEllipse(b_outline, lt_l, lt_t, lt_w, lt_h);
                        g.FillEllipse(blockBrush, lt_l, lt_t, lt_w, lt_h);

                        double gap = (initialLen - lt_compOffset + lt_tenOffset) / 20;
                        double offset1 = 0;
                        double offset2 = 0;
                        double amp = 10;

                        for (int i = 0; i < 20; i++)
                        {
                            int left = i % 4;

                            if (left == 0)
                            {
                                offset1 = 0;
                                offset2 = amp * (-1);
                            }
                            else if (left == 1)
                            {
                                offset1 = amp * (-1);
                                offset2 = 0;
                            }
                            else if (left == 2)
                            {
                                offset1 = 0;
                                offset2 = amp;
                            }
                            else if (left == 3)
                            {
                                offset1 = amp;
                                offset2 = 0;
                            }

                            g.DrawLine(p_spring, new PointF((float)(25 + i * gap), (float)(ltLinearCenterY + offset1)), new PointF((float)(25 + (i + 1) * gap), (float)(ltLinearCenterY + offset2)));
                        }

                        //// Draw the twisting angle selection part
                        //float lt_angle_centerX = ltAngleCenterX;
                        //float lt_angle_centerY = ltAngleCenterY;
                        //float lt_angle_r = ltAngleRadius;

                        //float lt_traj_l, lt_traj_t, lt_traj_w, lt_traj_h;
                        //float lt_angle_l, lt_angle_t, lt_angle_w, lt_angle_h;
                        //circle_drawing_coordinates_conversion(lt_twist_angle_traj_centerX, lt_twist_angle_traj_centerY, lt_twist_angle_traj_r, out lt_traj_l, out lt_traj_t, out lt_traj_w, out lt_traj_h);
                        //circle_drawing_coordinates_conversion(lt_angle_centerX, lt_angle_centerY, lt_angle_r, out lt_angle_l, out lt_angle_t, out lt_angle_w, out lt_angle_h);

                        //g.DrawEllipse(p, lt_traj_l, lt_traj_t, lt_traj_w, lt_traj_h);
                        //g.DrawLine(p_angle, new PointF(lt_twist_angle_traj_centerX, lt_twist_angle_traj_centerY), new PointF(lt_twist_angle_traj_centerX, lt_twist_angle_traj_centerY - lt_twist_angle_traj_r));
                        //g.DrawArc(p_angle1, lt_twist_angle_traj_centerX - lt_twist_angle_traj_r, lt_twist_angle_traj_centerY - lt_twist_angle_traj_r, 2 * lt_twist_angle_traj_r, 2 * lt_twist_angle_traj_r, -90, (float)lt_twist_angle);
                        //g.DrawLine(p_angle, new PointF(lt_twist_angle_traj_centerX, lt_twist_angle_traj_centerY), new PointF(ltAngleCenterX, ltAngleCenterY));
                        //g.DrawEllipse(b_outline, lt_angle_l, lt_angle_t, lt_angle_w, lt_angle_h);
                        //g.FillEllipse(blockBrush, lt_angle_l, lt_angle_t, lt_angle_w, lt_angle_h);


                        myBuffer.Render(e.Graphics);
                        g.Dispose();
                        myBuffer.Dispose();
                    }
                    break;
                case 3:
                    {
                        // Bend interface
                        if (!isAllDir)
                        {
                            // C# GDI+ & double buffered approach
                            Rectangle rectangle = e.ClipRectangle;
                            BufferedGraphicsContext GraphicsContext = BufferedGraphicsManager.Current;
                            BufferedGraphics myBuffer = GraphicsContext.Allocate(e.Graphics, e.ClipRectangle);
                            Graphics g = myBuffer.Graphics;
                            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            g.Clear(BackColor);
                            SolidBrush bkBrush = new SolidBrush(Color.FromArgb(224, 224, 224));
                            g.FillRectangle(bkBrush, rectangle);

                            Pen p = new Pen(Color.FromArgb(158, 158, 158), 2);
                            Pen p_new = new Pen(Color.FromArgb(108, 180, 241), 2);
                            Pen p_angle = new Pen(Color.FromArgb(108, 180, 241), 1);
                            Pen p_angle1 = new Pen(Color.FromArgb(108, 180, 241), 3);
                            Pen b_outline = new Pen(Color.FromArgb(255, 255, 255), 2);
                            SolidBrush blockBrush = new SolidBrush(Color.FromArgb(110, 159, 239));
                            SolidBrush disabledBrush = new SolidBrush(Color.FromArgb(200, 200, 200));

                            float b_centerX = bendDirCenterX;
                            float b_centerY = bendDirCenterY;
                            float b_r = bendDirRadius;

                            float a_centerX = bendAngleCenterX;
                            float a_centerY = bendAngleCenterY;
                            float a_r = bendAngleRadius;

                            float traj_l, traj_t, traj_w, traj_h;
                            float b_l, b_t, b_w, b_h;
                            float a_l, a_t, a_w, a_h;
                            circle_drawing_coordinates_conversion(bend_dir_traj_centerX, bend_dir_traj_centerY, bend_dir_traj_r, out traj_l, out traj_t, out traj_w, out traj_h);
                            circle_drawing_coordinates_conversion(b_centerX, b_centerY, b_r, out b_l, out b_t, out b_w, out b_h);
                            circle_drawing_coordinates_conversion(a_centerX, a_centerY, a_r, out a_l, out a_t, out a_w, out a_h);

                            g.FillPie(disabledBrush, new Rectangle((int)bend_angle_traj_centerX - (int)bend_angle_traj_r, (int)bend_angle_traj_centerY - (int)bend_angle_traj_r, 2 * (int)bend_angle_traj_r, 2 * (int)bend_angle_traj_r), -90 + (float)maxBendingAngle, 180 - (float)maxBendingAngle);


                            g.DrawEllipse(p_new, traj_l, traj_t, traj_w, traj_h);
                            //g.DrawLine(p_angle, new PointF(bend_dir_traj_centerX, bend_dir_traj_centerY), new PointF(bend_dir_traj_centerX, bend_dir_traj_centerY - bend_dir_traj_r));
                            g.DrawArc(p_angle1, bend_dir_traj_centerX - bend_dir_traj_r, bend_dir_traj_centerY - bend_dir_traj_r, 2 * bend_dir_traj_r, 2 * bend_dir_traj_r, -90, (float)bend_dir_angle);
                            //g.DrawLine(p_angle, new PointF(bend_dir_traj_centerX, bend_dir_traj_centerY), new PointF(bendDirCenterX,bendDirCenterY));
                            g.DrawEllipse(b_outline, b_l, b_t, b_w, b_h);
                            g.FillEllipse(blockBrush, b_l, b_t, b_w, b_h);

                            g.DrawLine(p, new PointF(bend_angle_traj_centerX, bend_angle_traj_centerY), new PointF(bend_angle_traj_centerX, bend_angle_traj_centerY - bend_angle_traj_r));
                            g.DrawLine(p, new PointF(bend_angle_traj_centerX, bend_angle_traj_centerY), new PointF(bendAngleCenterX, bendAngleCenterY));
                            //g.DrawLine(p, new PointF(bend_angle_traj_centerX, bend_angle_traj_centerY), new PointF(bendAngleCenterX, bendAngleCenterY + bend_angle_traj_r));

                            g.DrawPie(p, new Rectangle((int)bend_angle_traj_centerX - (int)bend_angle_traj_r, (int)bend_angle_traj_centerY - (int)bend_angle_traj_r, 2 * (int)bend_angle_traj_r, 2 * (int)bend_angle_traj_r), -90, 180);
                            g.DrawArc(p_angle1, bend_angle_traj_centerX - bend_angle_traj_r, bend_angle_traj_centerY - bend_angle_traj_r, 2 * bend_angle_traj_r, 2 * bend_angle_traj_r, -90, (float)bend_angle);
                           
                            //g.DrawArc(p_angle1, bend_angle_traj_centerX - bend_angle_traj_r, bend_angle_traj_centerY - bend_angle_traj_r, 2 * bend_angle_traj_r, 2 * bend_angle_traj_r, -90, (float)bend_angle);
                            g.DrawEllipse(b_outline, a_l, a_t, a_w, a_h);
                            g.FillEllipse(blockBrush, a_l, a_t, a_w, a_h);

                            myBuffer.Render(e.Graphics);
                            g.Dispose();
                            myBuffer.Dispose();
                        }
                        else
                        {
                            Rectangle rectangle = e.ClipRectangle;
                            BufferedGraphicsContext GraphicsContext = BufferedGraphicsManager.Current;
                            BufferedGraphics myBuffer = GraphicsContext.Allocate(e.Graphics, e.ClipRectangle);
                            Graphics g = myBuffer.Graphics;
                            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            g.Clear(BackColor);
                            SolidBrush bkBrush = new SolidBrush(Color.FromArgb(224, 224, 224));
                            g.FillRectangle(bkBrush, rectangle);

                            Pen p = new Pen(Color.FromArgb(175, 175, 175), 2);
                            Pen p_angle = new Pen(Color.FromArgb(175, 175, 175), 1);
                            Pen p_angle1 = new Pen(Color.FromArgb(175, 175, 175), 3);
                            Pen b_outline = new Pen(Color.FromArgb(255, 255, 255), 2);
                            SolidBrush blockBrush = new SolidBrush(Color.FromArgb(175, 175, 175));


                            Pen p_active = new Pen(Color.FromArgb(158, 158, 158), 2);
                            Pen p_angle_active = new Pen(Color.FromArgb(108, 180, 241), 1);
                            Pen p_angle1_active = new Pen(Color.FromArgb(108, 180, 241), 3);
                            Pen b_outline_active = new Pen(Color.FromArgb(255, 255, 255), 2);
                            SolidBrush blockBrush_active = new SolidBrush(Color.FromArgb(110, 159, 239));

                            float b_centerX = bendDirCenterX;
                            float b_centerY = bendDirCenterY;
                            float b_r = bendDirRadius;

                            float a_centerX = bendAngleCenterX;
                            float a_centerY = bendAngleCenterY;
                            float a_r = bendAngleRadius;

                            float traj_l, traj_t, traj_w, traj_h;
                            float b_l, b_t, b_w, b_h;
                            float a_l, a_t, a_w, a_h;
                            circle_drawing_coordinates_conversion(bend_dir_traj_centerX, bend_dir_traj_centerY, bend_dir_traj_r, out traj_l, out traj_t, out traj_w, out traj_h);
                            circle_drawing_coordinates_conversion(b_centerX, b_centerY, b_r, out b_l, out b_t, out b_w, out b_h);
                            circle_drawing_coordinates_conversion(a_centerX, a_centerY, a_r, out a_l, out a_t, out a_w, out a_h);

                            g.DrawEllipse(p, traj_l, traj_t, traj_w, traj_h);
                            g.DrawLine(p_angle, new PointF(bend_dir_traj_centerX, bend_dir_traj_centerY), new PointF(bend_dir_traj_centerX, bend_dir_traj_centerY - bend_dir_traj_r));
                            g.DrawArc(p_angle1, bend_dir_traj_centerX - bend_dir_traj_r, bend_dir_traj_centerY - bend_dir_traj_r, 2 * bend_dir_traj_r, 2 * bend_dir_traj_r, -90, (float)bend_dir_angle);
                            g.DrawLine(p_angle, new PointF(bend_dir_traj_centerX, bend_dir_traj_centerY), new PointF(bendDirCenterX, bendDirCenterY));
                            g.DrawEllipse(b_outline, b_l, b_t, b_w, b_h);
                            g.FillEllipse(blockBrush, b_l, b_t, b_w, b_h);

                            g.DrawLine(p_active, new PointF(bend_angle_traj_centerX, bend_angle_traj_centerY), new PointF(bend_angle_traj_centerX, bend_angle_traj_centerY - bend_angle_traj_r));
                            g.DrawLine(p_active, new PointF(bend_angle_traj_centerX, bend_angle_traj_centerY), new PointF(bendAngleCenterX, bendAngleCenterY));
                            g.DrawArc(p_angle1_active, bend_angle_traj_centerX - bend_angle_traj_r, bend_angle_traj_centerY - bend_angle_traj_r, 2 * bend_angle_traj_r, 2 * bend_angle_traj_r, -90, (float)bend_angle);
                            g.DrawEllipse(b_outline_active, a_l, a_t, a_w, a_h);
                            g.FillEllipse(blockBrush_active, a_l, a_t, a_w, a_h);

                            myBuffer.Render(e.Graphics);
                            g.Dispose();
                            myBuffer.Dispose();
                        }
                    }
                    break;
                case 4:
                    {
                        // C# GDI+ & double buffered approach
                        Rectangle rectangle = e.ClipRectangle;
                        BufferedGraphicsContext GraphicsContext = BufferedGraphicsManager.Current;
                        BufferedGraphics myBuffer = GraphicsContext.Allocate(e.Graphics, e.ClipRectangle);
                        Graphics g = myBuffer.Graphics;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.Clear(BackColor);
                        SolidBrush bkBrush = new SolidBrush(Color.FromArgb(224, 224, 224));
                        g.FillRectangle(bkBrush, rectangle);

                        Pen p = new Pen(Color.FromArgb(158, 158, 158), 2);
                        Pen p_new = new Pen(Color.FromArgb(108, 180, 241), 2);
                        Pen p_angle = new Pen(Color.FromArgb(108, 180, 241), 1);
                        Pen p_angle1 = new Pen(Color.FromArgb(108, 180, 241), 3);
                        Pen p_angle1_active = new Pen(Color.FromArgb(108, 180, 241), 3);
                        Pen b_outline = new Pen(Color.FromArgb(255, 255, 255), 2);
                        SolidBrush blockBrush = new SolidBrush(Color.FromArgb(110, 159, 239));
                        SolidBrush disabledBrush = new SolidBrush(Color.FromArgb(200, 200, 200));

                        float a_centerX = btBendAngleCenterX;
                        float a_centerY = btBendAngleCenterY;
                        float a_r = bendAngleRadius;
                        float a_l, a_t, a_w, a_h;

                        circle_drawing_coordinates_conversion(a_centerX, a_centerY, a_r, out a_l, out a_t, out a_w, out a_h);

                        g.FillPie(disabledBrush, new Rectangle((int)bt_bend_angle_traj_centerX - (int)bend_angle_traj_r, (int)bt_bend_angle_traj_centerY - (int)bend_angle_traj_r, 2 * (int)bend_angle_traj_r, 2 * (int)bend_angle_traj_r), -90 + (float)maxBendingAngle, 180 - (float)maxBendingAngle);

                        g.DrawLine(p, new PointF(bt_bend_angle_traj_centerX, bt_bend_angle_traj_centerY), new PointF(bt_bend_angle_traj_centerX, bt_bend_angle_traj_centerY - bend_angle_traj_r));
                        g.DrawLine(p, new PointF(bt_bend_angle_traj_centerX, bt_bend_angle_traj_centerY), new PointF(btBendAngleCenterX, btBendAngleCenterY));
                        //g.DrawLine(p, new PointF(bend_angle_traj_centerX, bend_angle_traj_centerY), new PointF(bendAngleCenterX, bendAngleCenterY + bend_angle_traj_r));
                        
                        g.DrawPie(p, new Rectangle((int)bt_bend_angle_traj_centerX - (int)bend_angle_traj_r, (int)bt_bend_angle_traj_centerY - (int)bend_angle_traj_r, 2 * (int)bend_angle_traj_r, 2 * (int)bend_angle_traj_r), -90, 180);
                        g.DrawArc(p_angle1, bt_bend_angle_traj_centerX - bend_angle_traj_r, bt_bend_angle_traj_centerY - bend_angle_traj_r, 2 * bend_angle_traj_r, 2 * bend_angle_traj_r, -90, (float)bend_angle);

                        g.DrawArc(p_angle1_active, bt_bend_angle_traj_centerX - bend_angle_traj_r, bt_bend_angle_traj_centerY - bend_angle_traj_r, 2 * bend_angle_traj_r, 2 * bend_angle_traj_r, -90, (float)bt_bend_angle);
                        //g.DrawArc(p_angle1, bend_angle_traj_centerX - bend_angle_traj_r, bend_angle_traj_centerY - bend_angle_traj_r, 2 * bend_angle_traj_r, 2 * bend_angle_traj_r, -90, (float)bend_angle);
                        g.DrawEllipse(b_outline, a_l, a_t, a_w, a_h);
                        g.FillEllipse(blockBrush, a_l, a_t, a_w, a_h);

                        myBuffer.Render(e.Graphics);
                        g.Dispose();
                        myBuffer.Dispose();
                    }
                    break;
                default: break;
            }
        }

        private void ClothBox_CheckedChanged(object sender, EventArgs e)
        {
            isOuterClothIncluded = !isOuterClothIncluded;

            isOuterClothShown = !isOuterClothShown;
            if (currUnit.ClothIDs.Count > 0)
            {
                controller.showClothSpring(currUnit.ClothIDs, isOuterClothShown);
            }
        }

        private void WireDiameterTrackBar_Scroll(object sender, EventArgs e)
        {

            double sizeOfInnerStructure = 2 * (calculate_rod_diameter(currUnit.MA.GetLength()) + 0.5 * 3 + 1);
            double clothWireDiameter = 1.6;
            double pitch = -1;   // The outer cloth always has the minimun pitch
            if (currUnit.MA.GetLength() <= 20)
            {
                pitch = clothWireDiameter + 0.4;   // The outer cloth always has the minimun pitch
            }
            else if (currUnit.MA.GetLength() <= 50 && currUnit.MA.GetLength() > 20)
            {
                pitch = clothWireDiameter + 0.8;   // The outer cloth always has the minimun pitch
            }
            else if (currUnit.MA.GetLength() <= 80 && currUnit.MA.GetLength() > 40)
            {
                pitch = clothWireDiameter + 1.2;   // The outer cloth always has the minimun pitch
            }
            else
            {
                pitch = clothWireDiameter + 1.6;   // The outer cloth always has the minimun pitch
            }

            if (currUnit.IsFreeformOnly)
            {
                if (currUnit.ClothIDs.Count == currUnit.CappedSpringIDs.Count && currUnit.ClothIDs.ElementAt(0) == currUnit.CappedSpringIDs.ElementAt(0))
                {
                    double d_max_temp = (currUnit.CoilDiameter.Min() - 1.6) / 2;
                    d_max = d_max_temp > 7.6 ? 7.6 : d_max_temp;
                    d_max = d_max < 1.6 ? 1.6 : d_max;

                }
                else
                {
                    double d_max_temp = (currUnit.CoilDiameter.Min() - 2 * clothWireDiameter - 1.6) / 2;
                    d_max = d_max_temp > 7.6 ? 7.6 : d_max_temp;
                    d_max = d_max < 1.6 ? 1.6 : d_max;
                }

            }
            else
            {
                if (currUnit.ClothIDs.Count == currUnit.CappedSpringIDs.Count && currUnit.ClothIDs.ElementAt(0) == currUnit.CappedSpringIDs.ElementAt(0))
                {
                    double d_max_temp = (currUnit.CoilDiameter.Min() - sizeOfInnerStructure - 1.6) / 2;
                    d_max = d_max_temp > 7.6 ? 7.6 : d_max_temp;
                    d_max = d_max < 1.6 ? 1.6 : d_max;
                }
                else
                {
                    double d_max_temp = (currUnit.CoilDiameter.Min() - 2 * clothWireDiameter - sizeOfInnerStructure - 1.6) / 2;
                    d_max = d_max_temp > 7.6 ? 7.6 : d_max_temp;
                    d_max = d_max < 1.6 ? 1.6 : d_max;
                }

            }

            tg_min = pitch;

            double wd = this.WireDiameterTrackBar.Value * 0.1;
            double len = currUnit.MA.GetLength();

            // Update the wire diameter track bar with the selected turn gap (in the case the user 
            // comes back from the stiffness track bar)
            double d_max_real= ((len - currUnit.Pitch) < d_max) ? (len - currUnit.Pitch) : d_max;
            if(wd > d_max_real)
            {
                wd = d_max_real;
                this.WireDiameterTrackBar.Value = Convert.ToInt32(wd / 0.1);
            }
            currUnit.WireDiameter = wd;
            string specifier = "F1";
            this.WDValueLabel.Text = wd.ToString(specifier) + " mm";

            // Update the turn gap trackbar, using the updated wire diameter value
            //double tg_max_real = len - wd;
            double tg_max_real = ((4 * wd) < (len-wd))? (4*wd): (len-wd);
            if (currUnit.Pitch > tg_max_real)
            {
                currUnit.Pitch = tg_max_real;
                this.TurnGapTrackBar.Value = Convert.ToInt32(currUnit.Pitch / 0.1);
                this.TGValueLabel.Text = currUnit.Pitch.ToString(specifier);
            }

            // Update the stiffness trackbar, using the updated wire diameter value 
            double coilD = currUnit.MeanCoilDiameter;
            //int turnN = Convert.ToInt32(Math.Floor((currUnit.MA.GetLength() / (currUnit.Pitch + currUnit.WireDiameter))>=1? (currUnit.MA.GetLength() / (currUnit.Pitch + currUnit.WireDiameter)) : 1));
            double turnN = currUnit.MA.GetLength() / (currUnit.Pitch + currUnit.WireDiameter);
            //int turnN_max = Convert.ToInt32(Math.Floor(currUnit.MA.GetLength() / (currUnit.Pitch + d_min)));
            //int turnN_min = Convert.ToInt32(Math.Floor(currUnit.MA.GetLength() / (currUnit.Pitch + d_max_real)))>=1? Convert.ToInt32(Math.Floor(currUnit.MA.GetLength() / (currUnit.Pitch + d_max_real))) : 1;

            double turnN_max = currUnit.MA.GetLength() / (d_min + tg_min);
            double turnN_min = currUnit.MA.GetLength() / (d_max_real + tg_max_real);

            //double k_max = Math.Pow(currUnit.WireDiameter, power) / turnN_min;
            //double k = Math.Pow(currUnit.WireDiameter, power) / turnN;
            //double k_min = Math.Pow(currUnit.WireDiameter, power) / turnN_max;

            //this.StiffnessTrackBar.Minimum = Convert.ToInt32(k_min * 1000);
            //this.StiffnessTrackBar.Maximum = Convert.ToInt32(k_max * 1000);
            //this.StiffnessTrackBar.Value = Convert.ToInt32(k * 1000);


            double k_max = (d_max_real + tg_max_real)/2;
            double k_min = d_min + tg_min;
            double k = (currUnit.WireDiameter + currUnit.Pitch)>=((d_max_real + tg_max_real) / 2)?((d_max_real + tg_max_real) / 2):(currUnit.WireDiameter + currUnit.Pitch);
           // this.StiffnessTrackBar.Minimum = Convert.ToInt32(k_min * 100);
            //this.StiffnessTrackBar.Maximum = Convert.ToInt32(k_max * 100);
            this.StiffnessTrackBar.Value = Convert.ToInt32(k * 100);

            currUnit.Stiffness = (currUnit.WireDiameter + currUnit.Pitch) *Math.Pow(currUnit.WireDiameter, power) / currUnit.MA.GetLength();
            currUnit.CoilNum =turnN;

            // Update the variables if possible along with the additional joints
            switch (currConstraintCtrl)
            {
                case 0:
                    {
                        applyLoadValue = (compDis > tenDis ? compDis : tenDis) * Math.Pow(currUnit.WireDiameter, 4) * 1008 / (8 * Math.Pow(currUnit.MeanCoilDiameter, 3) * currUnit.CoilNum);
                        applyLoadValueLabel.Text = applyLoadValue.ToString(specifier) + " N";
                        this.ConstraintCanvas.Refresh();
                    }
                    break;
                case 1:
                    {
                        double idealMaxTwistingAngle = (maxTwistingForce * (currUnit.MeanCoilDiameter / 2) * 64 * currUnit.MeanCoilDiameter * currUnit.CoilNum / (2825 * Math.Pow(currUnit.WireDiameter, 4))) / Math.PI * 180;
                        maxTwistingAngle = idealMaxTwistingAngle <= 90 ? idealMaxTwistingAngle : 90;
                        torsionLabel.Text = "Max twisting angle (0°-" + maxTwistingAngle.ToString(specifier1) + "°):";

                        if (twist_angle > maxTwistingAngle)
                        {
                            double ra_angle = maxTwistingAngle / 180 * Math.PI;
                            twistCenterX = twist_angle_traj_centerX + (float)Math.Sin(ra_angle) * twist_angle_traj_r;
                            twistCenterY = twist_angle_traj_centerY - (float)Math.Cos(ra_angle) * twist_angle_traj_r;
                            twistValueLabel.Text = maxTwistingAngle.ToString(specifier1) + "°";
                            twist_angle = maxTwistingAngle;
                        }
                        else
                        {
                            //twist_angle = Math.Acos((v1.X * v2.X + v1.Y * v2.Y) / (v1.Length * v2.Length)) * 180 / Math.PI;
                            twistValueLabel.Text = twist_angle.ToString(specifier1) + "°";
                        }

                        applyTorqueValue = twist_angle * Math.PI / 180 * 2825 * Math.Pow(currUnit.WireDiameter, 4) / (64 * currUnit.MeanCoilDiameter * currUnit.CoilNum) / (currUnit.MeanCoilDiameter / 2);
                        applyTorqueValueLabel.Text = applyTorqueValue.ToString(specifier) + " N";

                        this.ConstraintCanvas.Refresh();
                    }
                    break;
                case 2:
                    {
                        lt_applyLoadValue = (lt_compDis > lt_tenDis ? lt_compDis : lt_tenDis) * Math.Pow(currUnit.WireDiameter, 4) * 1008 / (8 * Math.Pow(currUnit.MeanCoilDiameter, 3) * currUnit.CoilNum);
                        lt_applyLoadValueLabel.Text = lt_applyLoadValue.ToString(specifier) + " N";

                        double idealMaxTwistingAngle = (maxTwistingForce * (currUnit.MeanCoilDiameter / 2) * 64 * currUnit.MeanCoilDiameter * currUnit.CoilNum / (2825 * Math.Pow(currUnit.WireDiameter, 4))) / Math.PI * 180;
                        maxTwistingAngle = idealMaxTwistingAngle <= 90 ? idealMaxTwistingAngle : 90;
                        lt_torsionLabel.Text = "Max twisting angle is 0° to " + maxTwistingAngle.ToString(specifier1) + "°";

                        double ltApplyTorqueValue = maxTwistingAngle * Math.PI / 180 * 2825 * Math.Pow(currUnit.WireDiameter, 4) / (64 * currUnit.MeanCoilDiameter * currUnit.CoilNum) / (currUnit.MeanCoilDiameter / 2);
                        ltApplyTorqueValueLabel.Text = ltApplyTorqueValue.ToString(specifier) + " N";

                        this.ConstraintCanvas.Refresh();
                    }
                    break;
                case 3:
                    {
                        maxBendingAngle = 90 - (180-((currUnit.CoilNum * currUnit.WireDiameter) / (currUnit.SelectedSeg.GetLength()/4)) / Math.PI * 180)/2;
                        //maxBendingAngle = Math.Asin(currUnit.CoilNum * currUnit.WireDiameter / currUnit.SelectedSeg.GetLength()) * 360 / (2 * Math.PI);
                        bendingAngleLabel.Text = "Bending angle " + "(0°-" + maxBendingAngle.ToString(specifier1) + "°): ";

                        if (bend_angle > maxBendingAngle)
                        {
                            double ra_angle = maxBendingAngle / 180 * Math.PI;
                            bendAngleCenterX = bend_angle_traj_centerX + (float)Math.Sin(ra_angle) * bend_angle_traj_r;
                            bendAngleCenterY = bend_angle_traj_centerY - (float)Math.Cos(ra_angle) * bend_angle_traj_r;
                            bendAngleValueLabel.Text = maxBendingAngle.ToString(specifier1) + "°";
                            bend_angle = maxBendingAngle;
                        }
                        else
                        {
                            bendAngleValueLabel.Text = bend_angle.ToString(specifier1) + "°";
                        }

                        this.ConstraintCanvas.Refresh();
                    }
                    break;
                case 4:
                    {
                        double idealMaxTwistingAngle = (maxTwistingForce * (currUnit.MeanCoilDiameter / 2) * 64 * currUnit.MeanCoilDiameter * currUnit.CoilNum / (2825 * Math.Pow(currUnit.WireDiameter, 4))) / Math.PI * 180;
                        maxTwistingAngle = idealMaxTwistingAngle <= 90 ? idealMaxTwistingAngle : 90;
                        bt_torsionLabel.Text = "Twisting angle is 0° to " + maxTwistingAngle.ToString(specifier1) + "°";

                        double btApplyTorqueValue = maxTwistingAngle * Math.PI / 180 * 2825 * Math.Pow(currUnit.WireDiameter, 4) / (64 * currUnit.MeanCoilDiameter * currUnit.CoilNum) / (currUnit.MeanCoilDiameter / 2);
                        btApplyTorqueValueLabel.Text = "Force to max twisting: " + btApplyTorqueValue.ToString(specifier) + " N";

                        maxBendingAngle = 90 - (180 - ((currUnit.CoilNum * currUnit.WireDiameter) / (currUnit.SelectedSeg.GetLength() / 4)) / Math.PI * 180) / 2;
                        //maxBendingAngle = Math.Asin(currUnit.CoilNum * currUnit.WireDiameter / currUnit.SelectedSeg.GetLength()) * 360 / (2 * Math.PI);
                        btBendingAngleLabel.Text = "Bending angle " + "(0°-" + maxBendingAngle.ToString(specifier1) + "°): ";


                        if (bt_bend_angle > maxBendingAngle)
                        {
                            double ra_angle = maxBendingAngle / 180 * Math.PI;
                            btBendAngleCenterX = bt_bend_angle_traj_centerX + (float)Math.Sin(ra_angle) * bend_angle_traj_r;
                            btBendAngleCenterY = bt_bend_angle_traj_centerY - (float)Math.Cos(ra_angle) * bend_angle_traj_r;
                            btBendAngleValueLabel.Text = maxBendingAngle.ToString(specifier1) + "°";
                            bt_bend_angle = maxBendingAngle;
                        }
                        else
                        {
                            btBendAngleValueLabel.Text = bt_bend_angle.ToString(specifier1) + "°";
                        }

                        this.ConstraintCanvas.Refresh();
                    }
                    break;
                default: break;
            }
        }

        private void WireDiameterTrackBar_MouseUp(object sender, MouseEventArgs e)
        {
            // Redraw the scene to render the spring
            this.ClothBox.Checked = false;
            controller.springGeneration(ref currUnit);

            // Update the current unit
            controller.updateUnitFromGlobal(currIdx, currUnit);
        }

        private void TurnGapTrackBar_Scroll(object sender, EventArgs e)
        {

            double sizeOfInnerStructure = 2 * (calculate_rod_diameter(currUnit.MA.GetLength()) + 0.5 * 3 + 1);
            double clothWireDiameter = 1.6;
            double pitch = -1;   // The outer cloth always has the minimun pitch
            if (currUnit.MA.GetLength() <= 20)
            {
                pitch = clothWireDiameter + 0.4;   // The outer cloth always has the minimun pitch
            }
            else if (currUnit.MA.GetLength() <= 50 && currUnit.MA.GetLength() > 20)
            {
                pitch = clothWireDiameter + 0.8;   // The outer cloth always has the minimun pitch
            }
            else if (currUnit.MA.GetLength() <= 80 && currUnit.MA.GetLength() > 40)
            {
                pitch = clothWireDiameter + 1.2;   // The outer cloth always has the minimun pitch
            }
            else
            {
                pitch = clothWireDiameter + 1.6;   // The outer cloth always has the minimun pitch
            }

            if (currUnit.IsFreeformOnly)
            {
                if (currUnit.ClothIDs.Count == currUnit.CappedSpringIDs.Count && currUnit.ClothIDs.ElementAt(0) == currUnit.CappedSpringIDs.ElementAt(0))
                {
                    double d_max_temp = (currUnit.CoilDiameter.Min() - 1.6) / 2;
                    d_max = d_max_temp > 7.6 ? 7.6 : d_max_temp;
                    d_max = d_max < 1.6 ? 1.6 : d_max;

                }
                else
                {
                    double d_max_temp = (currUnit.CoilDiameter.Min() - 2 * clothWireDiameter - 1.6) / 2;
                    d_max = d_max_temp > 7.6 ? 7.6 : d_max_temp;
                    d_max = d_max < 1.6 ? 1.6 : d_max;
                }

            }
            else
            {
                if (currUnit.ClothIDs.Count == currUnit.CappedSpringIDs.Count && currUnit.ClothIDs.ElementAt(0) == currUnit.CappedSpringIDs.ElementAt(0))
                {
                    double d_max_temp = (currUnit.CoilDiameter.Min() - sizeOfInnerStructure - 1.6) / 2;
                    d_max = d_max_temp > 7.6 ? 7.6 : d_max_temp;
                    d_max = d_max < 1.6 ? 1.6 : d_max;
                }
                else
                {
                    double d_max_temp = (currUnit.CoilDiameter.Min() - 2 * clothWireDiameter - sizeOfInnerStructure - 1.6) / 2;
                    d_max = d_max_temp > 7.6 ? 7.6 : d_max_temp;
                    d_max = d_max < 1.6 ? 1.6 : d_max;
                }

            }

            tg_min = pitch;



            double p = this.TurnGapTrackBar.Value * 0.1;
            double len = currUnit.MA.GetLength();

            // Update the turn gap track bar with selected wire diameter (in the case the user 
            // comes back from the stiffness track bar)
            //double tg_max_real = len - currUnit.WireDiameter;

            double tg_max_real = (4 * currUnit.WireDiameter) < (len - currUnit.WireDiameter)? (4 * currUnit.WireDiameter): (len - currUnit.WireDiameter);
            if(p > tg_max_real)
            {
                p = tg_max_real;
                this.TurnGapTrackBar.Value = Convert.ToInt32(p / 0.1);
            }
            currUnit.Pitch = p;
            string specifier = "F1";
            this.TGValueLabel.Text = p.ToString(specifier) + " mm";

            // Update the wire diameter trackbar, using the updated turn gap value
            double d_max_real = ((len - p) < d_max) ? (len - p) : d_max;
            if(currUnit.WireDiameter > d_max_real)
            {
                currUnit.WireDiameter = d_max_real;
                this.WireDiameterTrackBar.Value = Convert.ToInt32(currUnit.WireDiameter / 0.1);
                this.WDValueLabel.Text = currUnit.WireDiameter.ToString(specifier);
            }
            

            // Update the stiffness trackbar, using the updated turn gap value
            double coilD = currUnit.MeanCoilDiameter;
            //int turnN = Convert.ToInt32(Math.Floor((currUnit.MA.GetLength() / (currUnit.Pitch + currUnit.WireDiameter))>=1? (currUnit.MA.GetLength() / (currUnit.Pitch + currUnit.WireDiameter)) : 1));
            double turnN = currUnit.MA.GetLength() / (currUnit.Pitch + currUnit.WireDiameter);
            //int turnN_max = Convert.ToInt32(Math.Floor(currUnit.MA.GetLength() / (currUnit.WireDiameter + tg_min)));
            //int turnN_min = Convert.ToInt32(Math.Floor(currUnit.MA.GetLength() / (currUnit.WireDiameter + tg_max_real)))>=1? Convert.ToInt32(Math.Floor(currUnit.MA.GetLength() / (currUnit.WireDiameter + tg_max_real))): 1; 
            //double turnN_max = currUnit.MA.GetLength() / (currUnit.WireDiameter + tg_min);
            //double turnN_min = currUnit.MA.GetLength() / (currUnit.WireDiameter + tg_max_real);
            double turnN_max = currUnit.MA.GetLength() / (d_min + tg_min);
            double turnN_min = currUnit.MA.GetLength() / (d_max_real + tg_max_real);
            //double turnN_min = 1;
            //double turnN_max = d_min + tg_min;

            //double k = Math.Pow(currUnit.WireDiameter, power) / turnN;
            //double k_min = Math.Pow(currUnit.WireDiameter, power) / turnN_max;
            //double k_max = Math.Pow(currUnit.WireDiameter, power) / turnN_min;
            //this.StiffnessTrackBar.Minimum = Convert.ToInt32(k_min * 1000);
            //this.StiffnessTrackBar.Maximum = Convert.ToInt32(k_max * 1000);
            //this.StiffnessTrackBar.Value = Convert.ToInt32(k * 1000);

            double k_max = (d_max_real + tg_max_real)/2;
            double k_min = d_min + tg_min;
            double k = (currUnit.WireDiameter + currUnit.Pitch) >= ((d_max_real + tg_max_real) / 2) ? ((d_max_real + tg_max_real) / 2) : (currUnit.WireDiameter + currUnit.Pitch);
           // this.StiffnessTrackBar.Minimum = Convert.ToInt32(k_min * 100);
           // this.StiffnessTrackBar.Maximum = Convert.ToInt32(k_max * 100);
            this.StiffnessTrackBar.Value = Convert.ToInt32(k * 100);

            currUnit.Stiffness = (currUnit.WireDiameter + currUnit.Pitch) * Math.Pow(currUnit.WireDiameter, power) / currUnit.MA.GetLength();
            currUnit.CoilNum = turnN;

            // Update the variables if possible along with the additional joints
            switch (currConstraintCtrl)
            {
                case 0:
                    {
                        applyLoadValue = (compDis > tenDis ? compDis : tenDis) * Math.Pow(currUnit.WireDiameter, 4) * 1008 / (8 * Math.Pow(currUnit.MeanCoilDiameter, 3) * currUnit.CoilNum);
                        applyLoadValueLabel.Text = applyLoadValue.ToString(specifier) + " N";
                       
                        this.ConstraintCanvas.Refresh();
                    }
                    break;
                case 1:
                    {
                        double idealMaxTwistingAngle = (maxTwistingForce * (currUnit.MeanCoilDiameter / 2) * 64 * currUnit.MeanCoilDiameter * currUnit.CoilNum / (2825 * Math.Pow(currUnit.WireDiameter, 4))) / Math.PI * 180;
                        maxTwistingAngle = idealMaxTwistingAngle <= 90 ? idealMaxTwistingAngle : 90;
                        torsionLabel.Text = "Max twisting angle (0°-" + maxTwistingAngle.ToString(specifier1) + "°):";

                        if (twist_angle > maxTwistingAngle)
                        {
                            double ra_angle = maxTwistingAngle / 180 * Math.PI;
                            twistCenterX = twist_angle_traj_centerX + (float)Math.Sin(ra_angle) * twist_angle_traj_r;
                            twistCenterY = twist_angle_traj_centerY - (float)Math.Cos(ra_angle) * twist_angle_traj_r;
                            twistValueLabel.Text = maxTwistingAngle.ToString(specifier1) + "°";
                            twist_angle = maxTwistingAngle;
                        }
                        else
                        {
                            //twist_angle = Math.Acos((v1.X * v2.X + v1.Y * v2.Y) / (v1.Length * v2.Length)) * 180 / Math.PI;
                            twistValueLabel.Text = twist_angle.ToString(specifier1) + "°";
                        }

                        applyTorqueValue = twist_angle * Math.PI / 180 * 2825 * Math.Pow(currUnit.WireDiameter, 4) / (64 * currUnit.MeanCoilDiameter * currUnit.CoilNum) / (currUnit.MeanCoilDiameter / 2);
                        applyTorqueValueLabel.Text = applyTorqueValue.ToString(specifier) + " N";

                        this.ConstraintCanvas.Refresh();
                    }
                    break;
                case 2:
                    {
                        lt_applyLoadValue = (lt_compDis > lt_tenDis ? lt_compDis : lt_tenDis) * Math.Pow(currUnit.WireDiameter, 4) * 1008 / (8 * Math.Pow(currUnit.MeanCoilDiameter, 3) * currUnit.CoilNum);
                        lt_applyLoadValueLabel.Text = lt_applyLoadValue.ToString(specifier) + " N";

                        double idealMaxTwistingAngle = (maxTwistingForce * (currUnit.MeanCoilDiameter / 2) * 64 * currUnit.MeanCoilDiameter * currUnit.CoilNum / (2825 * Math.Pow(currUnit.WireDiameter, 4))) / Math.PI * 180;
                        maxTwistingAngle = idealMaxTwistingAngle <= 90 ? idealMaxTwistingAngle : 90;
                        lt_torsionLabel.Text = "Max twisting angle is 0° to " + maxTwistingAngle.ToString(specifier1) + "°";

                        double ltApplyTorqueValue = maxTwistingAngle * Math.PI / 180 * 2825 * Math.Pow(currUnit.WireDiameter, 4) / (64 * currUnit.MeanCoilDiameter * currUnit.CoilNum) / (currUnit.MeanCoilDiameter / 2);
                        ltApplyTorqueValueLabel.Text = ltApplyTorqueValue.ToString(specifier) + " N";

                        this.ConstraintCanvas.Refresh();
                    }
                    break;
                case 3:
                    {
                        maxBendingAngle = 90 - (180 - ((currUnit.CoilNum * currUnit.WireDiameter) / (currUnit.SelectedSeg.GetLength() / 4)) / Math.PI * 180) / 2;
                        //maxBendingAngle = Math.Acos(currUnit.CoilNum * currUnit.WireDiameter / currUnit.SelectedSeg.GetLength()) * 360 / (2 * Math.PI);
                        bendingAngleLabel.Text = "Bending angle " + "(0°-" + maxBendingAngle.ToString(specifier1) + "°): ";

                        if (bend_angle > maxBendingAngle)
                        {
                            double ra_angle = maxBendingAngle / 180 * Math.PI;
                            bendAngleCenterX = bend_angle_traj_centerX + (float)Math.Sin(ra_angle) * bend_angle_traj_r;
                            bendAngleCenterY = bend_angle_traj_centerY - (float)Math.Cos(ra_angle) * bend_angle_traj_r;
                            bendAngleValueLabel.Text = maxBendingAngle.ToString(specifier1) + "°";
                            bend_angle = maxBendingAngle;
                        }
                        else
                        {
                            bendAngleValueLabel.Text = bend_angle.ToString(specifier1) + "°";
                        }

                        this.ConstraintCanvas.Refresh();
                    }
                    break;
                case 4:
                    {
                        double idealMaxTwistingAngle = (maxTwistingForce * (currUnit.MeanCoilDiameter / 2) * 64 * currUnit.MeanCoilDiameter * currUnit.CoilNum / (2825 * Math.Pow(currUnit.WireDiameter, 4))) / Math.PI * 180;
                        maxTwistingAngle = idealMaxTwistingAngle <= 90 ? idealMaxTwistingAngle : 90;
                        bt_torsionLabel.Text = "Twisting angle is 0° to " + maxTwistingAngle.ToString(specifier1) + "°";

                        double btApplyTorqueValue = maxTwistingAngle * Math.PI / 180 * 2825 * Math.Pow(currUnit.WireDiameter, 4) / (64 * currUnit.MeanCoilDiameter * currUnit.CoilNum) / (currUnit.MeanCoilDiameter / 2);
                        btApplyTorqueValueLabel.Text = "Force to max twisting: " + btApplyTorqueValue.ToString(specifier) + " N";

                        maxBendingAngle = 90 - (180 - ((currUnit.CoilNum * currUnit.WireDiameter) / (currUnit.SelectedSeg.GetLength() / 4)) / Math.PI * 180) / 2;
                        //maxBendingAngle = Math.Asin(currUnit.CoilNum * currUnit.WireDiameter / currUnit.SelectedSeg.GetLength()) * 360 / (2 * Math.PI);
                        btBendingAngleLabel.Text = "Bending angle " + "(0°-" + maxBendingAngle.ToString(specifier1) + "°): ";

                        if (bt_bend_angle > maxBendingAngle)
                        {
                            double ra_angle = maxBendingAngle / 180 * Math.PI;
                            btBendAngleCenterX = bt_bend_angle_traj_centerX + (float)Math.Sin(ra_angle) * bend_angle_traj_r;
                            btBendAngleCenterY = bt_bend_angle_traj_centerY - (float)Math.Cos(ra_angle) * bend_angle_traj_r;
                            btBendAngleValueLabel.Text = maxBendingAngle.ToString(specifier1) + "°";
                            bt_bend_angle = maxBendingAngle;
                        }
                        else
                        {
                            btBendAngleValueLabel.Text = bt_bend_angle.ToString(specifier1) + "°";
                        }
                        this.ConstraintCanvas.Refresh();
                    }
                    break;
                default: break;
            }
        }

        private void TurnGapTrackBar_MouseUp(object sender, MouseEventArgs e)
        {
            // Redraw the scene to render the spring
            this.ClothBox.Checked = false;
            controller.springGeneration(ref currUnit);

            // Update the current unit
            controller.updateUnitFromGlobal(currIdx, currUnit);
        }

        private void MinStiffnessLabel_Click(object sender, EventArgs e)
        {

        }

        private void StiffnessTrackBar_MouseUp(object sender, MouseEventArgs e)
        {
            // Redraw the scene to render the spring
            this.ClothBox.Checked = false;
            controller.springGeneration(ref currUnit);

            // Update the current unit
            controller.updateUnitFromGlobal(currIdx, currUnit);
        }
        private void get_updated_wd_tg(double k, double len, double d_max, double tg_max, ref double wd, ref double tg)
        {
            double num = len /(wd + tg);
            double old_k = wd+tg;
            double new_k = old_k;
            double old_wd = wd;
            double old_tg = tg;

            double ratio = (k - old_k) / ((d_max + tg_max)/2 - tg_min - d_min);

            double tg_updated = ratio * (tg_max / 2 - tg_min) + tg;
 
            if(tg_updated < tg_min)
            {
                tg = tg_min;
                wd = k - tg;
            }
            else if(tg_updated > tg_max)
            {
                tg = tg_max;
                wd = k - tg;
            }
            else
            {
                wd = ratio * (d_max / 2 - d_min) + wd;

                if (wd < d_min)
                {
                    wd = d_min;
                    tg = k - wd;
                }
                else if (wd > d_max)
                {
                    wd = d_max;
                    tg = k - wd;
                }
                else
                {
                    tg = tg_updated;
                }
            }
        }

        private void StiffnessTrackBar_Scroll(object sender, EventArgs e)
        {
            double sizeOfInnerStructure = 2 * (calculate_rod_diameter(currUnit.MA.GetLength()) + 0.5 * 3 + 1);
            double clothWireDiameter = 1.6;
            double pitch = -1;   // The outer cloth always has the minimun pitch
            if (currUnit.MA.GetLength() <= 20)
            {
                pitch = clothWireDiameter + 0.4;   // The outer cloth always has the minimun pitch
            }
            else if (currUnit.MA.GetLength() <= 50 && currUnit.MA.GetLength() > 20)
            {
                pitch = clothWireDiameter + 0.8;   // The outer cloth always has the minimun pitch
            }
            else if (currUnit.MA.GetLength() <= 80 && currUnit.MA.GetLength() > 40)
            {
                pitch = clothWireDiameter + 1.2;   // The outer cloth always has the minimun pitch
            }
            else
            {
                pitch = clothWireDiameter + 1.6;   // The outer cloth always has the minimun pitch
            }

            if (currUnit.IsFreeformOnly)
            {
                if (currUnit.ClothIDs.Count == currUnit.CappedSpringIDs.Count && currUnit.ClothIDs.ElementAt(0) == currUnit.CappedSpringIDs.ElementAt(0))
                {
                    double d_max_temp = (currUnit.CoilDiameter.Min() - 1.6) / 2;
                    d_max = d_max_temp > 7.6 ? 7.6 : d_max_temp;
                    d_max = d_max < 1.6 ? 1.6 : d_max;

                }
                else
                {
                    double d_max_temp = (currUnit.CoilDiameter.Min() - 2 * clothWireDiameter - 1.6) / 2;
                    d_max = d_max_temp > 7.6 ? 7.6 : d_max_temp;
                    d_max = d_max < 1.6 ? 1.6 : d_max;
                }

            }
            else
            {
                if (currUnit.ClothIDs.Count == currUnit.CappedSpringIDs.Count && currUnit.ClothIDs.ElementAt(0) == currUnit.CappedSpringIDs.ElementAt(0))
                {
                    double d_max_temp = (currUnit.CoilDiameter.Min() - sizeOfInnerStructure - 1.6) / 2;
                    d_max = d_max_temp > 7.6 ? 7.6 : d_max_temp;
                    d_max = d_max < 1.6 ? 1.6 : d_max;
                }
                else
                {
                    double d_max_temp = (currUnit.CoilDiameter.Min() - 2 * clothWireDiameter - sizeOfInnerStructure - 1.6) / 2;
                    d_max = d_max_temp > 7.6 ? 7.6 : d_max_temp;
                    d_max = d_max < 1.6 ? 1.6 : d_max;
                }

            }

            tg_min = pitch;



            double current_k = Convert.ToDouble(this.StiffnessTrackBar.Value) /100.0;
            double len = currUnit.MA.GetLength();

            currUnit.Stiffness = current_k*Math.Pow(currUnit.WireDiameter, power)/ currUnit.MA.GetLength();
            string specifier = "F1";

            // Update the wire diameter track bar's max
            double d_max_real = ((len-tg_min) < d_max) ? (len-tg_min) : d_max;
            //double tg_max_real = len - currUnit.WireDiameter;
            double tg_max_real = (4 * currUnit.WireDiameter) < (len - currUnit.WireDiameter) ? (4 * currUnit.WireDiameter) : (len - currUnit.WireDiameter);

            double wd_value = currUnit.WireDiameter, tg_value = currUnit.Pitch;
            get_updated_wd_tg(current_k, len, d_max_real, tg_max_real, ref wd_value, ref tg_value);

            currUnit.WireDiameter = wd_value;
            currUnit.Pitch = tg_value;
            this.WireDiameterTrackBar.Value = Convert.ToInt32(currUnit.WireDiameter / 0.1);
            this.WDValueLabel.Text = currUnit.WireDiameter.ToString(specifier);
            this.TurnGapTrackBar.Value = Convert.ToInt32(currUnit.Pitch/0.1);
            this.TGValueLabel.Text = currUnit.Pitch.ToString(specifier);

            double turnN = len / (currUnit.WireDiameter + currUnit.Pitch);
            currUnit.CoilNum = turnN;

            // Update the variables if possible along with the additional joints
            switch (currConstraintCtrl)
            {
                case 0:
                    {
                        applyLoadValue = (compDis > tenDis ? compDis : tenDis) * Math.Pow(currUnit.WireDiameter, 4) * 1008 / (8 * Math.Pow(currUnit.MeanCoilDiameter, 3) * currUnit.CoilNum);
                        applyLoadValueLabel.Text = applyLoadValue.ToString(specifier) + " N";
                        this.ConstraintCanvas.Refresh();
                    } break;
                case 1:
                    {
                        
                        double idealMaxTwistingAngle = (maxTwistingForce * (currUnit.MeanCoilDiameter / 2) * 64 * currUnit.MeanCoilDiameter * currUnit.CoilNum / (2825 * Math.Pow(currUnit.WireDiameter, 4))) / Math.PI * 180;
                        maxTwistingAngle = idealMaxTwistingAngle <= 90 ? idealMaxTwistingAngle : 90;
                        torsionLabel.Text = "Max twisting angle (0°-" + maxTwistingAngle.ToString(specifier1) + "°):";

                        if (twist_angle > maxTwistingAngle)
                        {
                            double ra_angle = maxTwistingAngle / 180 * Math.PI;
                            twistCenterX = twist_angle_traj_centerX + (float)Math.Sin(ra_angle) * twist_angle_traj_r;
                            twistCenterY = twist_angle_traj_centerY - (float)Math.Cos(ra_angle) * twist_angle_traj_r;
                            twistValueLabel.Text = maxTwistingAngle.ToString(specifier1) + "°";
                            twist_angle = maxTwistingAngle;
                        }
                        else
                        {
                            //twist_angle = Math.Acos((v1.X * v2.X + v1.Y * v2.Y) / (v1.Length * v2.Length)) * 180 / Math.PI;
                            twistValueLabel.Text = twist_angle.ToString(specifier1) + "°";
                        }

                        applyTorqueValue = twist_angle * Math.PI / 180 * 2825 * Math.Pow(currUnit.WireDiameter, 4) / (64 * currUnit.MeanCoilDiameter * currUnit.CoilNum) / (currUnit.MeanCoilDiameter / 2);
                        applyTorqueValueLabel.Text = applyTorqueValue.ToString(specifier) + " N";

                        this.ConstraintCanvas.Refresh();

                    } break;
                case 2:
                    {
                        lt_applyLoadValue = (lt_compDis > lt_tenDis ? lt_compDis : lt_tenDis) * Math.Pow(currUnit.WireDiameter, 4) * 1008 / (8 * Math.Pow(currUnit.MeanCoilDiameter, 3) * currUnit.CoilNum);
                        lt_applyLoadValueLabel.Text = lt_applyLoadValue.ToString(specifier) + " N";

                        double idealMaxTwistingAngle = (maxTwistingForce * (currUnit.MeanCoilDiameter / 2) * 64 * currUnit.MeanCoilDiameter * currUnit.CoilNum / (2825 * Math.Pow(currUnit.WireDiameter, 4))) / Math.PI * 180;
                        maxTwistingAngle = idealMaxTwistingAngle <= 90 ? idealMaxTwistingAngle : 90;
                        lt_torsionLabel.Text = "Max twisting angle is 0° to " + maxTwistingAngle.ToString(specifier1) + "°";
   
                        double ltApplyTorqueValue = maxTwistingAngle * Math.PI / 180 * 2825 * Math.Pow(currUnit.WireDiameter, 4) / (64 * currUnit.MeanCoilDiameter * currUnit.CoilNum) / (currUnit.MeanCoilDiameter / 2);
                        ltApplyTorqueValueLabel.Text = ltApplyTorqueValue.ToString(specifier) + " N";

                        this.ConstraintCanvas.Refresh();
                    } break;
                case 3:
                    {
                        maxBendingAngle = 90 - (180 - ((currUnit.CoilNum * currUnit.WireDiameter) / (currUnit.SelectedSeg.GetLength() / 4)) / Math.PI * 180) / 2;
                        //maxBendingAngle = Math.Asin(currUnit.CoilNum * currUnit.WireDiameter / currUnit.SelectedSeg.GetLength()) * 360 / (2 * Math.PI);
                        bendingAngleLabel.Text = "Bending angle " + "(0°-" + maxBendingAngle.ToString(specifier1) + "°): ";
                        

                        if (bend_angle > maxBendingAngle)
                        {
                            double ra_angle = maxBendingAngle / 180 * Math.PI;
                            bendAngleCenterX = bend_angle_traj_centerX + (float)Math.Sin(ra_angle) * bend_angle_traj_r;
                            bendAngleCenterY = bend_angle_traj_centerY - (float)Math.Cos(ra_angle) * bend_angle_traj_r;
                            bendAngleValueLabel.Text = maxBendingAngle.ToString(specifier1) + "°";
                            bend_angle = maxBendingAngle;
                        }
                        else
                        {
                            bendAngleValueLabel.Text = bend_angle.ToString(specifier1) + "°";
                        }

                        this.ConstraintCanvas.Refresh();
                    }
                    break;
                case 4:
                    {
                        double idealMaxTwistingAngle = (maxTwistingForce * (currUnit.MeanCoilDiameter / 2) * 64 * currUnit.MeanCoilDiameter * currUnit.CoilNum / (2825 * Math.Pow(currUnit.WireDiameter, 4))) / Math.PI * 180;
                        maxTwistingAngle = idealMaxTwistingAngle <= 90 ? idealMaxTwistingAngle : 90;
                        bt_torsionLabel.Text = "Twisting angle is 0° to " + maxTwistingAngle.ToString(specifier1) + "°";

                        double btApplyTorqueValue = maxTwistingAngle * Math.PI / 180 * 2825 * Math.Pow(currUnit.WireDiameter, 4) / (64 * currUnit.MeanCoilDiameter * currUnit.CoilNum) / (currUnit.MeanCoilDiameter / 2);
                        btApplyTorqueValueLabel.Text = "Force to max twisting: " + btApplyTorqueValue.ToString(specifier) + " N";

                        maxBendingAngle = 90 - (180 - ((currUnit.CoilNum * currUnit.WireDiameter) / (currUnit.SelectedSeg.GetLength() / 4)) / Math.PI * 180) / 2;
                        //maxBendingAngle = Math.Asin(currUnit.CoilNum * currUnit.WireDiameter / currUnit.SelectedSeg.GetLength()) * 360 / (2 * Math.PI);
                        btBendingAngleLabel.Text = "Bending angle " + "(0°-" + maxBendingAngle.ToString(specifier1) + "°): ";


                        if (bt_bend_angle > maxBendingAngle)
                        {
                            double ra_angle = maxBendingAngle / 180 * Math.PI;
                            btBendAngleCenterX = bt_bend_angle_traj_centerX + (float)Math.Sin(ra_angle) * bend_angle_traj_r;
                            btBendAngleCenterY = bt_bend_angle_traj_centerY - (float)Math.Cos(ra_angle) * bend_angle_traj_r;
                            btBendAngleValueLabel.Text = maxBendingAngle.ToString(specifier1) + "°";
                            bt_bend_angle = maxBendingAngle;
                        }
                        else
                        {
                            btBendAngleValueLabel.Text = bt_bend_angle.ToString(specifier1) + "°";
                        }
                        this.ConstraintCanvas.Refresh();
                    }
                    break;
                default:break;
            }
            

        }

        private void OnduleConstraintCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.OnduleConstraintCheckbox.Checked)
            {
                this.LinearConstraintRadioButton.Enabled = true;
                this.TwistConstraintRadioButton.Enabled = true;
                this.LinearTwistConstraintRadioButton.Enabled = true;
                this.BendConstraintRadioButton.Enabled = true;
                this.AllDirectionsRadioButton.Enabled = true;
                this.ConstraintCanvas.Enabled = true;

                //this.StiffnessRadioButton.Enabled = false;
                //this.AdvancedRadioButton.Enabled = false;
                //this.MaxStiffnessLabel.Enabled = false;
                //this.MinStiffnessLabel.Enabled = false;
                //this.StiffnessTrackBar.Enabled = false;
                //this.WireDiameterTrackBar.Enabled = false;
                //this.MaxWDLabel.Enabled = false;
                //this.MinWDLabel.Enabled = false;
                //this.WDTitleLabel.Enabled = false;
                //this.WDValueLabel.Enabled = false;
                //this.MaxTGLabel.Enabled = false;
                //this.MinTGLabel.Enabled = false;
                //this.TGValueLabel.Enabled = false;
                //this.TurnGapTitleLabel.Enabled = false;

                controller.showInternalStructure(currUnit, currIdx);
            }
            else
            {
                this.LinearConstraintRadioButton.Enabled = false;
                this.TwistConstraintRadioButton.Enabled = false;
                this.LinearTwistConstraintRadioButton.Enabled = false;
                this.BendConstraintRadioButton.Enabled = false;
                this.AllDirectionsRadioButton.Enabled = false;
                this.ConstraintCanvas.Enabled = false;

                //this.StiffnessRadioButton.Enabled = true;
                //this.AdvancedRadioButton.Enabled = true;
                //this.MaxStiffnessLabel.Enabled = true;
                //this.MinStiffnessLabel.Enabled = true;
                //this.StiffnessTrackBar.Enabled = true;
                //this.WireDiameterTrackBar.Enabled = true;
                //this.MaxWDLabel.Enabled = true;
                //this.MinWDLabel.Enabled = true;
                //this.WDTitleLabel.Enabled = true;
                //this.WDValueLabel.Enabled = true;
                //this.MaxTGLabel.Enabled = true;
                //this.MinTGLabel.Enabled = true;
                //this.TGValueLabel.Enabled = true;
                //this.TurnGapTitleLabel.Enabled = true;

                controller.hideInternalStructure(currUnit, currIdx);
            }
        }

        private void LinearConstraintRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (this.LinearConstraintRadioButton.Checked)
            {
                // Show the linear control pannel
                currConstraintCtrl = 0;
                this.ConstraintCanvas.Controls.Clear();

                //this.AllDirectionsRadioButton.Enabled = false;

                double thickness = 3;
                //double max_comp_real = (currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * printing_tolerance - tenDis) / 2;
                //double max_ten_real = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * printing_tolerance - compDis;
                //double max_comp_real = (currUnit.SelectedSeg.GetLength() - 2 * thickness - printing_tolerance)/2;
                //double max_ten_real = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * printing_tolerance;
                double max_comp_real = (currUnit.SelectedSeg.GetLength() - 2 * thickness - tenDis) / 2;
                double max_ten_real = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * compDis;

                double newMaxComp = (currUnit.SelectedSeg.GetLength() - 2 * thickness - printing_tolerance) / 2;
                double newMaxExtension = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * printing_tolerance;

                double compRatio = compDis / newMaxComp * 100;
                double tenRatio = tenDis / newMaxExtension * 100;

                applyLoadValueLabel = new Label();
                applyLoadValue = (compDis > tenDis ? compDis : tenDis) * Math.Pow(currUnit.WireDiameter, 4) * 1008 / (8 * Math.Pow(currUnit.MeanCoilDiameter, 3) * currUnit.CoilNum);
                applyLoadValueLabel.Text = applyLoadValue.ToString(specifier) + " N";
                applyLoadValueLabel.Width = 50;
                applyLoadValueLabel.Height = 20;
                applyLoadValueLabel.Left = 70;
                applyLoadValueLabel.Top = 145;

                this.ConstraintCanvas.Controls.Add(applyLoadValueLabel);

                Label applyLoad = new Label();
                applyLoad.Text = "Max load:";
                applyLoad.Width = 70;
                applyLoad.Height = 20;
                applyLoad.Left = 15;
                applyLoad.Top = 145;
                this.ConstraintCanvas.Controls.Add(applyLoad);

                CheckBox lockCheckbox = new CheckBox();
                lockCheckbox.Text = "Lock";
                lockCheckbox.Width = 200;
                lockCheckbox.Height = 20;
                lockCheckbox.Left = 17;
                lockCheckbox.Top = 165;
                lockCheckbox.BringToFront();
                this.ConstraintCanvas.Controls.Add(lockCheckbox);
                lockCheckbox.CheckedChanged += LockCheckbox_CheckedChanged;

                compressDisValueLabel = new Label();
                compressDisValueLabel.Text = compDis.ToString(specifier) + " mm (" + compRatio.ToString(specifier) + "%)";
                compressDisValueLabel.Height = 20;
                compressDisValueLabel.Left = 110;
                compressDisValueLabel.Top = 105;
                this.ConstraintCanvas.Controls.Add(compressDisValueLabel);

                tensionDisValueLabel = new Label();
                tensionDisValueLabel.Text = tenDis.ToString(specifier) + " mm (" + tenRatio.ToString(specifier) + "%)";
                tensionDisValueLabel.Height = 20;
                tensionDisValueLabel.Left = 95;
                tensionDisValueLabel.Top = 125;
                this.ConstraintCanvas.Controls.Add(tensionDisValueLabel);

                Label compressLabel = new Label();
                compressLabel.Text = "Max compression:";
                compressLabel.Width = 100;
                compressLabel.Height = 20;
                compressLabel.Left = 15;
                compressLabel.Top = 105;
                this.ConstraintCanvas.Controls.Add(compressLabel);

                Label tensionLabel = new Label();
                tensionLabel.Text = "Max extension:";
                tensionLabel.Width = 90;
                tensionLabel.Height = 20;
                tensionLabel.Left = 15;
                tensionLabel.Top = 125;
                this.ConstraintCanvas.Controls.Add(tensionLabel);

                this.ConstraintCanvas.Refresh();

                if (currUnit.ConstraintType == currConstraintCtrl && currUnit.InnerStructureIDs.Count > 0)
                {
                    // show the existing internal structure
                    controller.showInternalStructure(currUnit, currIdx);
                }
                else
                {
                    // Initialize the internal structure
                    processingwindow.Show();
                    processingwindow.Refresh();

                    controller.hideBendDirOrbit(currUnit);

                    currUnit.ConstraintType = currConstraintCtrl;

                    currUnit.CompressionDis = compDis;
                    // Get the current extension displacement from LinearConsStretchTrackbar
                    currUnit.ExtensionDis = tenDis;

                    controller.updateUnitFromGlobal(currIdx, currUnit);
                    controller.addLinearConstraint(ref currUnit);

                    processingwindow.Hide();
                }
  
            }
        }

        private void LockCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            if (!isLinearLock)
            {
                controller.addLockToLinearConstraint(ref currUnit, true);
                isLinearLock = true;
            }
            else
            {
                controller.addLockToLinearConstraint(ref currUnit, false);
                isLinearLock = false;
            }
        }

        private void TwistConstraintRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (this.TwistConstraintRadioButton.Checked)
            {
                // Show the twist control pannel
                currConstraintCtrl = 1;
                this.ConstraintCanvas.Controls.Clear();

                //this.AllDirectionsRadioButton.Enabled = false;

                applyTorqueValueLabel = new Label();
                if (currUnit.TwistAngle < 0) currUnit.TwistAngle = 0;
                applyTorqueValue = currUnit.TwistAngle*Math.PI/180 * 2825 * Math.Pow(currUnit.WireDiameter,4)/(64*currUnit.MeanCoilDiameter*currUnit.CoilNum) / (currUnit.MeanCoilDiameter/2);
                applyTorqueValueLabel.Text = applyTorqueValue.ToString(specifier) + " N";
                applyTorqueValueLabel.Width = 80;
                applyTorqueValueLabel.Height = 20;
                applyTorqueValueLabel.Left = 125;
                applyTorqueValueLabel.Top = 140;

                this.ConstraintCanvas.Controls.Add(applyTorqueValueLabel);

                PictureBox twistingAnnotationPictureBox = new PictureBox();
                twistingAnnotationPictureBox.Image = Image.FromFile(@"..\Resources\twistingAnnotation1.png");
                twistingAnnotationPictureBox.Width = 71;
                twistingAnnotationPictureBox.Height =76;
                twistingAnnotationPictureBox.Left = 20;
                twistingAnnotationPictureBox.Top = 18;
                this.ConstraintCanvas.Controls.Add(twistingAnnotationPictureBox);

                Label applyTorque = new Label();
                applyTorque.Text = "Force to max twisting:";
                applyTorque.Width = 145;
                applyTorque.Height = 20;
                applyTorque.Left = 15;
                applyTorque.Top = 140;
                this.ConstraintCanvas.Controls.Add(applyTorque);

                torsionLabel = new Label();
                double idealMaxTwistingAngle = (maxTwistingForce * (currUnit.MeanCoilDiameter / 2) * 64 * currUnit.MeanCoilDiameter * currUnit.CoilNum / (2825 * Math.Pow(currUnit.WireDiameter, 4))) / Math.PI * 180;
                maxTwistingAngle = idealMaxTwistingAngle <= 90 ? idealMaxTwistingAngle : 90;
                torsionLabel.Text = "Max twisting angle (0°-" + maxTwistingAngle.ToString(specifier1) + "°):";
                torsionLabel.Width = 140;
                torsionLabel.Height = 20;
                torsionLabel.Left = 15;
                torsionLabel.Top = 120;
                this.ConstraintCanvas.Controls.Add(torsionLabel);

                twistValueLabel = new Label();
                twistValueLabel.Text = twist_angle.ToString(specifier1) + "°";
                twistValueLabel.Width = 30;
                twistValueLabel.Height = 20;
                twistValueLabel.Left = 155;
                twistValueLabel.Top = 120;
                twistValueLabel.BackColor = Color.FromArgb(1, 255, 255, 255);
                this.ConstraintCanvas.Controls.Add(twistValueLabel);

                CheckBox twistLockCheckbox = new CheckBox();
                twistLockCheckbox.Text = "Lock";
                twistLockCheckbox.Width = 200;
                twistLockCheckbox.Height = 20;
                twistLockCheckbox.Left = 17;
                twistLockCheckbox.Top = 165;
                twistLockCheckbox.BringToFront();
                this.ConstraintCanvas.Controls.Add(twistLockCheckbox);
                twistLockCheckbox.CheckedChanged += TwistLockCheckbox_CheckedChanged;

                this.ConstraintCanvas.Refresh();

                if (currUnit.ConstraintType == currConstraintCtrl && currUnit.InnerStructureIDs.Count > 0)
                {
                    // show the existing internal structure
                    controller.showInternalStructure(currUnit, currIdx);
                }
                else
                {
                    // Initialize the internal structure
                    processingwindow.Show();
                    processingwindow.Refresh();

                    controller.hideBendDirOrbit(currUnit);

                    currUnit.ConstraintType = currConstraintCtrl;
                    currUnit.TwistAngle = twist_angle;

                    controller.updateUnitFromGlobal(currIdx, currUnit);
                    controller.addTwistConstraint(ref currUnit);
                    processingwindow.Hide();
                }
 
            }
        }

        private void TwistLockCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            if (!isTwistLock)
            {
                controller.addLockToTwistConstraint(ref currUnit, true);
                isTwistLock = true;
            }
            else
            {
                controller.addLockToTwistConstraint(ref currUnit, false);
                isTwistLock = false;
            }
        }

        private void LinearTwistConstraintRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (this.LinearTwistConstraintRadioButton.Checked)
            {

                // Show the linear + twist control pannel
                currConstraintCtrl = 2;
                this.ConstraintCanvas.Controls.Clear();

                //this.AllDirectionsRadioButton.Enabled = false;

                double thickness = 3;

                double max_comp_real = (currUnit.SelectedSeg.GetLength() - 2 * thickness - tenDis) / 2;
                double max_ten_real = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * compDis;

                double newMaxComp = (currUnit.SelectedSeg.GetLength() - 2 * thickness - printing_tolerance) / 2;
                double newMaxExtension = currUnit.SelectedSeg.GetLength() - 2 * thickness - 2 * printing_tolerance;

                double compRatio = compDis / newMaxComp * 100;
                double tenRatio = tenDis / newMaxExtension * 100;

                //double max_comp_real = (currUnit.MA.GetLength() - 2 * thickness - printing_tolerance - lt_tenDis) / 2;
                //double max_ten_real = currUnit.MA.GetLength() - 2 * thickness - 2 * printing_tolerance - lt_compDis;

                //double compRatio = lt_compDis / max_comp_real * 100;
                //double tenRatio = lt_tenDis / max_ten_real * 100;

                lt_applyLoadValueLabel = new Label();
                lt_applyLoadValue = (lt_compDis > lt_tenDis ? lt_compDis : lt_tenDis) * Math.Pow(currUnit.WireDiameter, 4) * 892 / (8 * Math.Pow(currUnit.MeanCoilDiameter, 3) * currUnit.CoilNum);
                lt_applyLoadValueLabel.Text = applyLoadValue.ToString(specifier) + "N";
                lt_applyLoadValueLabel.Width = 80;
                lt_applyLoadValueLabel.Height = 20;
                lt_applyLoadValueLabel.Left = 75;
                lt_applyLoadValueLabel.Top = 115;

                this.ConstraintCanvas.Controls.Add(lt_applyLoadValueLabel);

                Label applyLoad = new Label();
                applyLoad.Text = "Max Load:";
                applyLoad.Width = 60;
                applyLoad.Height = 20;
                applyLoad.Left = 15;
                applyLoad.Top = 115;
                this.ConstraintCanvas.Controls.Add(applyLoad);

                ltCompressDisValueLabel = new Label();
                ltCompressDisValueLabel.Text = lt_compDis.ToString(specifier) + " mm (" + lt_compDis.ToString(specifier) + "%)";
                ltCompressDisValueLabel.Height = 20;
                ltCompressDisValueLabel.Left = 105;
                ltCompressDisValueLabel.BringToFront();
                ltCompressDisValueLabel.Top = 75;
                this.ConstraintCanvas.Controls.Add(ltCompressDisValueLabel);

                ltTensionDisValueLabel = new Label();
                ltTensionDisValueLabel.Text = lt_tenDis.ToString(specifier) + " mm (" + lt_tenDis.ToString(specifier) + "%)";
                ltTensionDisValueLabel.Height = 20;
                ltTensionDisValueLabel.Left = 95;
                ltTensionDisValueLabel.BringToFront();
                ltTensionDisValueLabel.Top = 95;
                this.ConstraintCanvas.Controls.Add(ltTensionDisValueLabel);

                Label compressLabel = new Label();
                compressLabel.Width = 100;
                compressLabel.Height = 20;
                compressLabel.Left = 15;
                compressLabel.Top = 75;
                compressLabel.SendToBack();
                compressLabel.Text = "Max compression:";
                
                this.ConstraintCanvas.Controls.Add(compressLabel);

                Label tensionLabel = new Label();
                tensionLabel.Width = 80;
                tensionLabel.Height = 20;
                tensionLabel.Left = 15;
                tensionLabel.Top = 95;
                tensionLabel.SendToBack();
                tensionLabel.Text = "Max extension:";
                
                this.ConstraintCanvas.Controls.Add(tensionLabel);

                // Splitter
                Label splitLabel = new Label();
                splitLabel.Text = "- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -";
                splitLabel.Width = 195;
                splitLabel.Height = 10;
                splitLabel.Left = 15;
                splitLabel.Top = 135;
                this.ConstraintCanvas.Controls.Add(splitLabel);

                // Twisting angle selection part
                lt_torsionLabel = new Label();
                double idealMaxTwistingAngle = (maxTwistingForce * (currUnit.MeanCoilDiameter / 2) * 64 * currUnit.MeanCoilDiameter * currUnit.CoilNum / (2825 * Math.Pow(currUnit.WireDiameter, 4))) / Math.PI * 180;
                maxTwistingAngle = idealMaxTwistingAngle <= 90 ? idealMaxTwistingAngle : 90;
                lt_torsionLabel.Text = "Twisting Angle is 0° to " + maxTwistingAngle.ToString(specifier1) + "°";
                lt_torsionLabel.Width = 200;
                lt_torsionLabel.Height = 20;
                lt_torsionLabel.Left = 15;
                lt_torsionLabel.Top = 155;
                this.ConstraintCanvas.Controls.Add(lt_torsionLabel);

                ltApplyTorqueValueLabel = new Label();
                double ltApplyTorqueValue = maxTwistingAngle * Math.PI / 180 * 2825 * Math.Pow(currUnit.WireDiameter, 4) / (64 * currUnit.MeanCoilDiameter * currUnit.CoilNum) / (currUnit.MeanCoilDiameter / 2);
                ltApplyTorqueValueLabel.Text = "Force to max twisting: " + ltApplyTorqueValue.ToString(specifier) + " N";
                ltApplyTorqueValueLabel.Width = 200;
                ltApplyTorqueValueLabel.Height = 20;
                ltApplyTorqueValueLabel.Left = 15;
                ltApplyTorqueValueLabel.Top = 175;
                this.ConstraintCanvas.Controls.Add(ltApplyTorqueValueLabel);

                //ltTwistValueLabel = new Label();
                //ltTwistValueLabel.Text = lt_twist_angle.ToString(specifier1) + "°";
                //ltTwistValueLabel.Width = 30;
                //ltTwistValueLabel.Height = 20;
                //ltTwistValueLabel.Left = 90;
                //ltTwistValueLabel.Top = 120;
                //ltTwistValueLabel.BackColor = Color.FromArgb(1, 255, 255, 255);
                //this.ConstraintCanvas.Controls.Add(ltTwistValueLabel);

                this.ConstraintCanvas.Refresh();

                if (currUnit.ConstraintType == currConstraintCtrl && currUnit.InnerStructureIDs.Count > 0)
                {
                    // show the existing internal structure
                    controller.showInternalStructure(currUnit, currIdx);
                }
                else
                {
                    processingwindow.Show();
                    processingwindow.Refresh();

                    controller.hideBendDirOrbit(currUnit);

                    // Initialize the internal structure
                    currUnit.ConstraintType = currConstraintCtrl;
                    // Get the current compression displacement from LinearConsCompressTrackbar
                    currUnit.CompressionDis = lt_compDis;
                    // Get the current extension displacement from LinearConsStretchTrackbar
                    currUnit.ExtensionDis = lt_tenDis;
                    // Get the current twist angle from TwistTrackbar
                    currUnit.TwistAngle = lt_twist_angle;

                    controller.updateUnitFromGlobal(currIdx, currUnit);
                    controller.addLinearTwistConstraint(ref currUnit);

                    processingwindow.Hide();
                }    
            }
        }

        private void BendConstraintRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (this.BendConstraintRadioButton.Checked)
            {
                // Show the bend control pannel
                isAllDir = false;
                currConstraintCtrl = 3;
                this.ConstraintCanvas.Controls.Clear();

                //this.AllDirectionsRadioButton.Enabled = true;

                PictureBox bendingAnnotationPictureBox = new PictureBox();
                bendingAnnotationPictureBox.Image = Image.FromFile(@"..\Resources\bendingAnnotation.png");
                bendingAnnotationPictureBox.Width = 71;
                bendingAnnotationPictureBox.Height = 76;
                bendingAnnotationPictureBox.Left = 10;
                bendingAnnotationPictureBox.Top = 10;
                this.ConstraintCanvas.Controls.Add(bendingAnnotationPictureBox);

                Label directionLabel = new Label();
                directionLabel.Text = "Bending direction";
                directionLabel.Width = 100;
                directionLabel.Height = 50;
                directionLabel.Left = 10;
                directionLabel.Top = 86;
                this.ConstraintCanvas.Controls.Add(directionLabel);

                bendingAngleLabel = new Label();
                maxBendingAngle = 90 - (180 - ((currUnit.CoilNum * currUnit.WireDiameter) / (currUnit.SelectedSeg.GetLength() / 4)) / Math.PI * 180) / 2;
                //maxBendingAngle = Math.Asin(currUnit.CoilNum * currUnit.WireDiameter / currUnit.SelectedSeg.GetLength()) * 360 / (2 * Math.PI);
                bendingAngleLabel.Text = "Bending angle " + "(0°-" + maxBendingAngle.ToString(specifier1) + "°): ";
                bendingAngleLabel.Width = 120;
                bendingAngleLabel.Height = 20;
                bendingAngleLabel.Left = 10;
                bendingAngleLabel.Top = 135;
                this.ConstraintCanvas.Controls.Add(bendingAngleLabel);

                bendDirValueLabel = new Label();
                bendDirValueLabel.Text = bend_dir_angle.ToString(specifier1) + "°";
                //bendDirValueLabel.Width = 30;
                //bendDirValueLabel.Height = 20;
                //bendDirValueLabel.Left = 52;
                //bendDirValueLabel.Top = 63;
                //bendDirValueLabel.BackColor = Color.FromArgb(1, 255, 255, 255);
                //this.ConstraintCanvas.Controls.Add(bendDirValueLabel);

                bendAngleValueLabel = new Label();
                bendAngleValueLabel.Text = bend_angle.ToString(specifier1) + "°";
                bendAngleValueLabel.Width = 30;
                bendAngleValueLabel.Height = 20;
                bendAngleValueLabel.Left = 10;
                bendAngleValueLabel.Top = 155;
                bendAngleValueLabel.BackColor = Color.FromArgb(1, 255, 255, 255);
                this.ConstraintCanvas.Controls.Add(bendAngleValueLabel);

                if (!isAllDir)
                {
                    directionLabel.Enabled = true;
                    directionLabel.ForeColor = Color.FromArgb(0, 0, 0);
                    bendDirValueLabel.Enabled = true;
                    bendDirValueLabel.ForeColor = Color.FromArgb(0, 0, 0);
                    this.ConstraintCanvas.Refresh();
                }
                else
                {
                    directionLabel.Enabled = false;
                    directionLabel.ForeColor = Color.FromArgb(175, 175, 175);
                    bendDirValueLabel.Enabled = false;
                    bendDirValueLabel.ForeColor = Color.FromArgb(175, 175, 175);
                    this.ConstraintCanvas.Refresh();
                }

                if (currUnit.ConstraintType == currConstraintCtrl && currUnit.InnerStructureIDs.Count > 0)
                {
                    // show the existing internal structure
                    controller.showInternalStructure(currUnit, currIdx);
                }
                else
                {

                    // Initialize the internal structure
                    processingwindow.Show();
                    processingwindow.Refresh();

                    currUnit.ConstraintType = currConstraintCtrl;
                    currUnit.BendDirAngle = bend_dir_angle;
                    currUnit.BendAngle = bend_angle;
                    controller.updateUnitFromGlobal(currIdx, currUnit);
                    if (!isAllDir)
                    {
                        // Initialize the direction on the model
                        controller.updateInPlaneBendDir(currUnit);
                    }

                    controller.addBendConstraint(ref currUnit, isAllDir);

                    processingwindow.Hide();
                }
            }
        }
        private void circle_drawing_coordinates_conversion(float centerX, float centerY, float r, out float left, out float top, out float w, out float h)
        {
            left = centerX - r;
            top = centerY - r;
            w = 2 * r;
            h = 2 * r;
        }

        private void AllDirectionsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            //#region old checkbox version
            //if (this.AllDirectionsRadioButton.Checked)
            //{
            //    if (!isAllDir)
            //    {
            //        // Update the internal chain model to ball chain
            //        processingwindow.Show();
            //        processingwindow.Refresh();

            //        isAllDir = true;
            //        currUnit.ConstraintType = currConstraintCtrl;
            //        currUnit.BendDirAngle = bend_dir_angle;
            //        currUnit.BendAngle = bend_angle;
            //        controller.updateUnitFromGlobal(currIdx, currUnit);
            //        controller.addBendConstraint(ref currUnit, isAllDir);

            //        processingwindow.Hide();

            //    }
            //    isAllDir = true;
            //    this.ConstraintCanvas.Refresh();
            //}
            //else
            //{
            //    if (isAllDir)
            //    {
            //        // Update the internal chain model to cylinder chain
            //        // Initialize the internal structure
            //        processingwindow.Show();
            //        processingwindow.Refresh();

            //        isAllDir = false;
            //        currUnit.ConstraintType = currConstraintCtrl;
            //        currUnit.BendDirAngle = bend_dir_angle;
            //        currUnit.BendAngle = bend_angle;
            //        controller.updateUnitFromGlobal(currIdx, currUnit);

            //        // Initialize the direction on the model
            //        controller.updateInPlaneBendDir(currUnit);
            //        controller.addBendConstraint(ref currUnit, isAllDir);
                    
            //        processingwindow.Hide();
            //    }       
            //    isAllDir = false;
            //    this.ConstraintCanvas.Refresh(); 
            //}
            //#endregion

            if (this.AllDirectionsRadioButton.Checked)
            {
                // Show the bend control pannel
                currConstraintCtrl = 4;
                this.ConstraintCanvas.Controls.Clear();
                isAllDir = true;

                btBendingAngleLabel = new Label();
                maxBendingAngle = 90 - (180 - ((currUnit.CoilNum * currUnit.WireDiameter) / (currUnit.SelectedSeg.GetLength() / 4)) / Math.PI * 180) / 2;
                //maxBendingAngle = Math.Asin(currUnit.CoilNum * currUnit.WireDiameter / currUnit.SelectedSeg.GetLength()) * 360 / (2 * Math.PI);
                btBendingAngleLabel.Text = "Bending angle " + "(0°-" + maxBendingAngle.ToString(specifier1) + "°): ";
                btBendingAngleLabel.Width = 120;
                btBendingAngleLabel.Height = 20;
                btBendingAngleLabel.Left = 15;
                btBendingAngleLabel.Top = 115;
                this.ConstraintCanvas.Controls.Add(btBendingAngleLabel);

                btBendAngleValueLabel = new Label();
                btBendAngleValueLabel.Text = bt_bend_angle.ToString(specifier1) + "°";
                btBendAngleValueLabel.Width = 30;
                btBendAngleValueLabel.Height = 20;
                btBendAngleValueLabel.Left = 135;
                btBendAngleValueLabel.Top = 115;
                btBendAngleValueLabel.BackColor = Color.FromArgb(1, 255, 255, 255);
                this.ConstraintCanvas.Controls.Add(btBendAngleValueLabel);

                // Splitter
                Label splitLabel = new Label();
                splitLabel.Text = "- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -";
                splitLabel.Width = 195;
                splitLabel.Height = 10;
                splitLabel.Left = 15;
                splitLabel.Top = 135;
                this.ConstraintCanvas.Controls.Add(splitLabel);

                // Twisting angle selection part
                bt_torsionLabel = new Label();
                double idealMaxTwistingAngle = (maxTwistingForce * (currUnit.MeanCoilDiameter / 2) * 64 * currUnit.MeanCoilDiameter * currUnit.CoilNum / (2825 * Math.Pow(currUnit.WireDiameter, 4))) / Math.PI * 180;
                maxTwistingAngle = idealMaxTwistingAngle <= 90 ? idealMaxTwistingAngle : 90;
                bt_torsionLabel.Text = "Twisting angle is 0° to " + maxTwistingAngle.ToString(specifier1) + "°";
                bt_torsionLabel.Width = 200;
                bt_torsionLabel.Height = 20;
                bt_torsionLabel.Left = 15;
                bt_torsionLabel.Top = 155;
                this.ConstraintCanvas.Controls.Add(bt_torsionLabel);

                btApplyTorqueValueLabel = new Label();
                double btApplyTorqueValue = maxTwistingAngle * Math.PI / 180 * 2825 * Math.Pow(currUnit.WireDiameter, 4) / (64 * currUnit.MeanCoilDiameter * currUnit.CoilNum) / (currUnit.MeanCoilDiameter / 2);
                btApplyTorqueValueLabel.Text = "Force to max twisting: " + btApplyTorqueValue.ToString(specifier) + " N";
                btApplyTorqueValueLabel.Width = 200;
                btApplyTorqueValueLabel.Height = 20;
                btApplyTorqueValueLabel.Left = 15;
                btApplyTorqueValueLabel.Top = 175;
                this.ConstraintCanvas.Controls.Add(btApplyTorqueValueLabel);

                this.ConstraintCanvas.Refresh();

                if (currUnit.ConstraintType == currConstraintCtrl && currUnit.InnerStructureIDs.Count > 0)
                {
                    // show the existing internal structure
                    controller.showInternalStructure(currUnit, currIdx);
                }
                else
                {
                    // Initialize the internal structure
                    processingwindow.Show();
                    processingwindow.Refresh();

                    currUnit.ConstraintType = currConstraintCtrl;
                    currUnit.BendDirAngle = bend_dir_angle;
                    currUnit.BendAngle = bend_angle;
                    controller.updateUnitFromGlobal(currIdx, currUnit);
                    controller.addBendConstraint(ref currUnit, isAllDir);

                    processingwindow.Hide();
                }
            }

        }

    }
}
