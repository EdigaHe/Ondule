using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OndulePlugin
{
    public partial class DeformationDesignForm : MetroFramework.Forms.MetroForm, IControllerModelObserver
    {
        Boolean isTwistable = false;
        Boolean isBendable = false;
        Boolean isLinearLimited = false;
        Boolean isLinearTwistLimited = false;
        Boolean isAllDirBending = false;
        Boolean isOuterClothShown = false;

        private OnduleUnit _springUnit = new OnduleUnit();
        private OnduleUnit _tempRenderedSpring = new OnduleUnit();

        private int _springIndex = -1;
        private double _springParam_d = 0;          // wire diameter
        private List<double> _springParam_D = new List<double>();        // coil diameter
        private List<int> _springParam_CoilNum = new List<int>();       // spring coil turns
        private double _springParam_G = 350000;     // PLA's Shear Modulus/Modulus of Rigidity
        private double _springParam_L = 1.0;        // spring length
        private double _springParamPitch;     // Pitch between two consecutive turns

        // TO-DO: Calculate the stiffness, the linear deflection, the bending angle, and the twist angle
        //private double _springPropStiffness = 1;
        //private double _springPropLinearDeflection = 0;
        //private double _springPropBendAngle = 0;
        //private double _springPropTwistAngle = 0;
        //private List<double> _tempLinearDeflection = new List<double>();
        //private double _tempStiffness = 0;
        //private double _tempBendAngle = 0;
        //private double _tempTwistAngle = 0;

        // We need to experiment and get the empirical values for the following parameters
        // so that we know the limits of the printable spring design
        private double d_min = 1.6;
        private double d_max = 7.6;
        private int N_min = 1;
        private int N_max = 10;
        private double gap_min = 0.4;
        private double thickness = 2;   // the thickness of the stopper and the cap is 2mm
        
        private Controller controller;

        public DeformationDesignForm(Controller controller)
        {
            InitializeComponent();
            this.controller = controller;
            this._springParam_d = 1.6;
            this._springParam_D.Add(30);
            this._springParamPitch = 4;
            this._springParam_CoilNum.Add(2);
            //this._springPropStiffness = this._springParam_G * Math.Pow(this._springParam_d, 4) / (8 * this._springParam_CoilNum.ElementAt(0) * Math.Pow(this._springParam_D.ElementAt(0), 3));
            //this._springPropLinearDeflection = 1 / ((this._springPropStiffness == 0) ? 1: this._springPropStiffness);
        }
        public DeformationDesignForm(OnduleUnit passUnit, int index, Controller controller)
        {
            InitializeComponent();
            this._springUnit = passUnit;
            this._springIndex = index;
            this.controller = controller;

            // Calculate the intinal spring design
            Rhino.Geometry.Curve ma = passUnit.MA;
            this._springParam_L = ma.GetLength();
            this._springParam_d = passUnit.WireDiameter;
            this._springParam_D = passUnit.CoilDiameter;
            this._springParam_CoilNum = passUnit.CoilNum;  
            this._springParam_G = passUnit.G;
            this._springParamPitch = passUnit.Pitch;

            //this._springPropStiffness = passUnit.Stiffness.Sum();
            //this._springPropLinearDeflection = 1 / this._springPropStiffness;
            //this._springPropBendAngle = passUnit.BendAngle;
            //this._springPropTwistAngle = passUnit.TwistAngle;

            // The value of the trackBar X 0.1 = the real wire diameter
            this.WireDiameterTrackbar.Maximum = 76;
            this.WireDiameterTrackbar.Minimum = 16;
            this.WireDiameterTrackbar.Value = (int)(this._springParam_d/0.1);
            this.WireDiameterTrackbar.TickStyle = TickStyle.None;

            // The value of the trackBar X 0.1 = the real pitch
            this.PitchTrackbar.Maximum = (int)(this._springParam_L / 0.1);
            this.PitchTrackbar.Minimum = 4;
            this.PitchTrackbar.Value = (int)(this._springParamPitch / 0.1);
            this.PitchTrackbar.TickStyle = TickStyle.None;

            // Initialize the min and max for the linear compression and tension 
            // The thickness of the stopper is 2mm
            // The thickness of the cap is 2mm
            this.LinearConsCompressMin.Text = "0";
            this.LinearConsCompressMax.Text = Math.Round((this._springParam_L-2*thickness) / 2, 1).ToString();
            this.LinearConsStretchMin.Text = "0";
            this.LinearConsStretchMax.Text = Math.Round(this._springParam_L - 2*thickness, 1).ToString();
            this.LinearConsCompressTrackbar.Minimum = 0;
            this.LinearConsCompressTrackbar.Maximum = (int)((Math.Round((this._springParam_L - 2*thickness) / 2, 1)) / 0.1);
            this.LinearConsStretchTrackbar.Minimum = 0;
            this.LinearConsStretchTrackbar.Maximum = (int)((Math.Round(this._springParam_L - 2*thickness, 1)) / 0.1);
            this.LinearConsCompressValue.Text = "0";
            this.LinearConsStretchValue.Text = "0";

            //this.BendTrackbar.Minimum = 0;
            //this.BendTrackbar.Maximum = 60;
            //this.BendTrackbar.Value = (int)this._springPropBendAngle;
            //this.bendingMin.Text = this.BendTrackbar.Minimum.ToString();
            //this.bendingMax.Text = this.BendTrackbar.Maximum.ToString();
            //this.BendTrackbar.TickStyle = TickStyle.None;
            //this.setBendAngle.Text = this.BendTrackbar.Value.ToString();

            //this.TwistTrackbar.Minimum = 0;
            //this.TwistTrackbar.Maximum = 45;
            //this.TwistTrackbar.Value = (int)this._springPropTwistAngle;
            //this.twistMin.Text = this.TwistTrackbar.Minimum.ToString();
            //this.twistMax.Text = this.TwistTrackbar.Maximum.ToString();
            //this.TwistTrackbar.TickStyle = TickStyle.None;
            //this.setTwistAngle.Text = this.TwistTrackbar.Value.ToString();

            //if(this._springParam_L/this._springParam_CoilNum <= 2 * this._springParam_d )
            //{
            //    this._springParamPitch = 2 * this._springParam_d;
            //}
            //else
            //{
            //    this._springParamPitch = this._springParam_L / this._springParam_CoilNum;
            //}
            ///////// Warning: ignoring C and S from the spring theory ///////

            //double stiffness = (this._springParam_G * Math.Pow(this._springParam_d, 7)) / (8 * this._springParam_CoilNum * Math.Pow(this._springParam_D, 3));

            //OnduleUnit t = new OnduleUnit(passUnit.UnitID, this._springParam_CoilNum, this._springParam_d, this._springParamPitch, this._springParam_D, this._springParam_L, this._springParam_G, passUnit.MA, passUnit.startPt, passUnit.endPt, stiffness);
            //controller.updateUnitFromGlobal(index,t);

            // Initial the temp rendered spring with the passed ondule unit
            this._tempRenderedSpring = passUnit;
        }

        private void PropertyWindow_Load(object sender, EventArgs e)
        {

        }

        private void BendingResetBtn_Click(object sender, EventArgs e)
        {
            // reset the bending angle
            //this.BendTrackbar.Value = (int)(this._springPropBendAngle);
            //this.setBendAngle.Text = this.BendTrackbar.Value.ToString();
        }

        private void TwistingResetBtn_Click(object sender, EventArgs e)
        {
            // reset the twist angle
            //this.TwistTrackbar.Value = (int)(this._springPropTwistAngle);
            //this.setTwistAngle.Text = this.TwistTrackbar.Value.ToString();
        }

        private void BendTrackbar_ValueChanged(object sender, EventArgs e)
        {
            // change the bend angle value
            //this._tempBendAngle = this.BendTrackbar.Value;
            //this.setBendAngle.Text = this.BendTrackbar.Value.ToString();
        }

        private void TwistTrackbar_ValueChanged(object sender, EventArgs e)
        {
            // change the twist angle value
            this._tempRenderedSpring.TwistAngle = this.TwistTrackbar.Value;
            if (isTwistable)
            {
                // TO-DO: Update the outer spring to satisfy the expected twiting angle
            }    
        }

        private void InverseComputeSpringParameters(double expectLinearDeflection, 
            double expectStiffness, double expectBendAngle, double expectTwistAngle, OnduleUnit prevUnit, out OnduleUnit updatedUnit)
        {
            updatedUnit = prevUnit;
            double prevWireDiameter = prevUnit.WireDiameter;
            List<double> prevDisLens = prevUnit.DiscontinuedLengths;
            List<int> prevCoilNum = prevUnit.CoilNum;

            double updatedWireDiameter = prevWireDiameter;
            List<int> updatedCoilNum = new List<int>();
            foreach(int cn in prevUnit.CoilNum)
            {
                updatedCoilNum.Add(cn);
            }

            // First, we test if the expected stiffness falls in the range where valid wire diameter (d_min to d_max) applies.
            double minStiffness = 0;
            double maxStiffness = 0;
            double d_min_updated = d_min;
            double d_max_updated = d_max;
           
            int idx = 0;
            foreach(double Dia in prevUnit.CoilDiameter)
            {
                minStiffness += prevUnit.G * Math.Pow(d_min_updated, 4) / (8 * Math.Pow(Dia, 3) * prevUnit.CoilNum.ElementAt(idx));
                maxStiffness += prevUnit.G * Math.Pow(d_max_updated, 4) / (8 * Math.Pow(Dia, 3) * prevUnit.CoilNum.ElementAt(idx));
                idx++;
            }
            if (expectStiffness >= minStiffness && expectStiffness <= maxStiffness)
            {
                // only change the wire diameter
                double f = 0;
                int t = 0;
                foreach(double D in prevUnit.CoilDiameter)
                {
                    f += prevUnit.G / (8 * prevUnit.CoilNum.ElementAt(t) * Math.Pow(D, 3));
                    t++;
                }
                updatedWireDiameter = Math.Sqrt(Math.Sqrt(expectStiffness / f));
            }
            else if(expectStiffness < minStiffness)
            {
                // use d_min_updated as the wire diameter and increase the coil number
                double minStiffnessUpdated = 0;

                do
                {
                    int t = 0;
                    foreach (double D in prevUnit.CoilDiameter)
                    {
                        int N = updatedCoilNum.ElementAt(t);
                        // test if the updated pitch is still bigger than the wire diameter
                        if (prevUnit.DiscontinuedLengths.ElementAt(t) / (N + 1) > d_min_updated)
                        {
                            updatedCoilNum[t] = N + 1;
                        }
                        t++;
                    }

                    int ix = 0;
                    foreach (double Dia in prevUnit.CoilDiameter)
                    {
                        minStiffnessUpdated += prevUnit.G * Math.Pow(d_min_updated, 4) / (8 * Math.Pow(Dia, 3) * updatedCoilNum.ElementAt(ix));
                        ix++;
                    }
                } while (expectStiffness < minStiffnessUpdated);

                double f = 0;
                int ii = 0;
                foreach (double D in prevUnit.CoilDiameter)
                {
                    f += prevUnit.G / (8 * updatedCoilNum.ElementAt(ii) * Math.Pow(D, 3));
                    ii++;
                }
                updatedWireDiameter = Math.Sqrt(Math.Sqrt(expectStiffness / f));

            }
            else if(expectStiffness > maxStiffness)
            {
                // use d_max_updated as the wire diameter and decrease the coil number
                double maxStiffnessUpdated = 0;

                do
                {
                    int t = 0;
                    foreach (double D in prevUnit.CoilDiameter)
                    {
                        int N = updatedCoilNum.ElementAt(t);
                        // test if the updated pitch is still bigger than the wire diameter
                        if (N > 1 && prevUnit.DiscontinuedLengths.ElementAt(t) / (N - 1) > d_max_updated)
                        {
                            updatedCoilNum[t] = N - 1;
                        }
                        t++;
                    }

                    int ix = 0;
                    foreach (double Dia in prevUnit.CoilDiameter)
                    {
                        maxStiffnessUpdated += prevUnit.G * Math.Pow(d_max_updated, 4) / (8 * Math.Pow(Dia, 3) * updatedCoilNum.ElementAt(ix));
                        ix++;
                    }
                } while (expectStiffness > maxStiffnessUpdated);

                double f = 0;
                int ii = 0;
                foreach (double D in prevUnit.CoilDiameter)
                {
                    f += prevUnit.G / (8 * updatedCoilNum.ElementAt(ii) * Math.Pow(D, 3));
                    ii++;
                }
                updatedWireDiameter = Math.Sqrt(Math.Sqrt(expectStiffness / f));
            }

            updatedUnit.CoilNum = updatedCoilNum;
            updatedUnit.WireDiameter = updatedWireDiameter;
        }

        private void Preview_Click(object sender, EventArgs e)
        {

            //Rhino.DocObjects.ObjRef obj = new Rhino.DocObjects.ObjRef(this._springUnit.UnitID);
            this.ShowOuterSpringCheckBox.Checked = false;
            controller.springGeneration(ref this._tempRenderedSpring);

            // Check if generate the prismatic joint for only linear deformation
            if (this.LinearOnly_radioButton.Checked)
            {

                // Get the current compression displacement from LinearConsCompressTrackbar
                this._tempRenderedSpring.CompressionDis = this.LinearConsCompressTrackbar.Value * 0.1;
                // Get the current extension displacement from LinearConsStretchTrackbar
                this._tempRenderedSpring.ExtensionDis = this.LinearConsStretchTrackbar.Value * 0.1;
                controller.addLinearConstraint(ref this._tempRenderedSpring);
            }
            // Check if generate the bearing for only twist deformation
            else if (this.TwistOnly_radioButton.Checked)
            {
                // Get the current twist angle from TwistTrackbar
                this._tempRenderedSpring.TwistAngle = this.TwistTrackbar.Value;
                controller.addTwistConstraint(ref this._tempRenderedSpring);
            }
            // Check if generate the prismatic joint and the bearing for linear + twist deformation
            else if (this.LinearTwist_radioButton.Checked)
            {
                // Get the current compression displacement from LinearConsCompressTrackbar
                this._tempRenderedSpring.CompressionDis = this.LinearConsCompressTrackbar.Value * 0.1;
                // Get the current extension displacement from LinearConsStretchTrackbar
                this._tempRenderedSpring.ExtensionDis = this.LinearConsStretchTrackbar.Value * 0.1;
                // Get the current twist angle from TwistTrackbar
                this._tempRenderedSpring.TwistAngle = this.TwistTrackbar.Value;

                controller.addLinearTwistConstraint(ref this._tempRenderedSpring);
            }
            // Check if generate the joint chain for bending deformation
            else if (this.BendOnly_radioButton.Checked)
            {
                this._tempRenderedSpring.BendAngle = this.BendConsDirectionTrackbar.Value;
                controller.addBendConstraint(ref this._tempRenderedSpring, isAllDirBending);
            }
            // Free form spring generation, clear the innerstructureIDs
            else
            {
                controller.clearInnerStructure(ref this._tempRenderedSpring);
            }


        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void LockCheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void WireDiameterResetBtn_Click(object sender, EventArgs e)
        {
            // Reset the wire diameter value
            int wd_value = Convert.ToInt32(this._springUnit.WireDiameter / 0.1);
            this.WireDiameterTrackbar.Value = wd_value;
            this.WDValLabel.Text = this._springUnit.WireDiameter.ToString();
        }

        private void WDTrackbar_ValueChanged(object sender, EventArgs e)
        {
            // Change the wire diameter value and update the temp unit
            double wd = this.WireDiameterTrackbar.Value * 0.1;
            this._tempRenderedSpring.WireDiameter = wd;
            this.WDValLabel.Text = wd.ToString();
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Pitch_ValueChanged(object sender, EventArgs e)
        {
            // Change the pitch value and update the temp unit
            double p = this.PitchTrackbar.Value * 0.1;
            this._tempRenderedSpring.Pitch = p;
            this.PitchValLabel.Text = p.ToString();
        }

        private void PitchResetBtn_Click(object sender, EventArgs e)
        {
            // Reset the pitch value
            int pitch_value = Convert.ToInt32(this._springUnit.Pitch / 0.1);
            this.PitchTrackbar.Value = pitch_value;
            this.PitchValLabel.Text = this._springUnit.Pitch.ToString();
        }

        private void LinearConsCompressTrackbar_Scroll(object sender, EventArgs e)
        {
            double c_value = this.LinearConsCompressTrackbar.Value * 0.1;
            this.LinearConsCompressValue.Text = c_value.ToString();
            double s_value = Math.Round(this._springParam_L - 2*thickness - c_value * 2,1);
            this.LinearConsStretchMax.Text = s_value.ToString();
            this.LinearConsStretchTrackbar.Maximum = (int)(s_value / 0.1);
        }

        private void LinearConsStretchTrackbar_Scroll(object sender, EventArgs e)
        {
            double s_value = this.LinearConsStretchTrackbar.Value * 0.1;
            this.LinearConsStretchValue.Text = s_value.ToString();
            double c_value = Math.Round((this._springParam_L - 2*thickness - s_value) / 2, 1);
            this.LinearConsCompressMax.Text = c_value.ToString();
            this.LinearConsCompressTrackbar.Maximum = (int)(c_value / 0.1);
        }

        private void AllDirBendingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            isAllDirBending = !isAllDirBending;
        }

        private void ShowOuterSpringCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            isOuterClothShown = !isOuterClothShown;
            if(this._tempRenderedSpring.ClothIDs.Count > 0)
            {
                controller.showClothSpring(this._tempRenderedSpring.ClothIDs, isOuterClothShown);
            }
        }
    }
}
