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

namespace OndulePlugin
{
    public partial class OnduleTopBarControl : MetroFramework.Controls.MetroUserControl, View,  IControllerModelObserver
    {
        private bool is_freeform = false;
        private bool is_LinearConstraint = false;
        
        public OnduleUnit currUnit = new OnduleUnit();
        public int currIdx = 0;

        private double d_min = 1.6;
        private double d_max = 7.6;
        private int N_min = 1;
        private int N_max = 10;
        private double gap_min = 0.4;
        private List<double> stiffRange = new List<double>();

        #region Variables for the new parameter control panel
        //private OnduleUnit _springUnit = new OnduleUnit();
        //private OnduleUnit _tempRenderedSpring = new OnduleUnit();
        Boolean isOuterClothShown = false;


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

        public OnduleTopBarControl()
        {
            InitializeComponent();

            set_control_panel_statues(false);
            // Hide some components in the control panel
            this.WDTitleLabel.Hide();
            this.MinWDLabel.Hide();
            this.MaxWDLabel.Hide();
            this.TurnGapTitleLabel.Hide();
            this.MinTGLabel.Hide();
            this.MaxTGLabel.Hide();
            this.WireDiameterTrackBar.Hide();
            this.TurnGapTrackBar.Hide();
            this.WDValueLabel.Hide();
            this.TGValueLabel.Hide();
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
            this.WireDiameterTrackBar.Enabled = isactivated;
            this.TurnGapTrackBar.Enabled = isactivated;
            this.StiffnessTrackBar.Enabled = isactivated;
            this.OnduleConstraintCheckbox.Enabled = isactivated;
            this.LinearConstraintRadioButton.Enabled = isactivated;
            this.TwistConstraintRadioButton.Enabled = isactivated;
            this.LinearTwistConstraintRadioButton.Enabled = isactivated;
            this.BendConstraintRadioButton.Enabled = isactivated;
            this.AllDirectionsCheckBox.Enabled = isactivated;
            this.ClothBox.Enabled = isactivated;
            this.MinStiffnessLabel.Enabled = isactivated;
            this.MaxStiffnessLabel.Enabled = isactivated;
            this.MinWDLabel.Enabled = isactivated;
            this.MaxWDLabel.Enabled = isactivated;
            this.MinTGLabel.Enabled = isactivated;
            this.MaxTGLabel.Enabled = isactivated;
            this.OnduleSpringGenerationTitleLabel.Enabled = isactivated;
            this.WDValueLabel.Enabled = isactivated;
            this.TGValueLabel.Enabled = isactivated;
        }
        private void initialize_parameter_panel(OnduleUnit currUnit, int curridx)
        {
            this.StiffnessRadioButton.Checked = true;

            if(currUnit.WireDiameter == -1)
            {
                this.WireDiameterTrackBar.Value = this.WireDiameterTrackBar.Minimum;
                this.WDValueLabel.Text = Convert.ToDouble(this.WireDiameterTrackBar.Minimum * 0.1).ToString();
                currUnit.WireDiameter = Convert.ToDouble(this.WireDiameterTrackBar.Minimum * 0.1);
            }
            else
            {
                this.WireDiameterTrackBar.Value = Convert.ToInt32(currUnit.WireDiameter / 0.1);
                this.WDValueLabel.Text = currUnit.WireDiameter.ToString();
            }

            if(currUnit.Pitch == -1)
            {
                this.TurnGapTrackBar.Value = this.TurnGapTrackBar.Minimum;
                this.TGValueLabel.Text = Convert.ToDouble(this.TurnGapTrackBar.Minimum * 0.1).ToString();
                currUnit.Pitch = Convert.ToDouble(this.TurnGapTrackBar.Minimum * 0.1);
            }
            else
            {
                this.TurnGapTrackBar.Value = Convert.ToInt32(currUnit.Pitch / 0.1);
                this.TGValueLabel.Text = currUnit.Pitch.ToString();
            }

            // Calculate the stiffness and update the stiffness track bar
            double coilD = currUnit.CoilDiameter.ElementAt(0);
            int turnN = Convert.ToInt32(Math.Floor(currUnit.MA.GetLength() / (currUnit.Pitch + currUnit.WireDiameter)));

            int turnN_max = Convert.ToInt32(Math.Floor(currUnit.MA.GetLength() / (0.6 + 1.6)));
            int turnN_min = 1;
            
            double k = currUnit.WireDiameter * currUnit.WireDiameter * currUnit.WireDiameter * currUnit.WireDiameter * 1000 / (8 * turnN * coilD * coilD * coilD);
            double k_min = 1.6 * 1.6 * 1.6 * 1.6 * 1000 / (8 * coilD * coilD * coilD * turnN_max);
            double k_max = 7.6 * 7.6 * 7.6 * 7.6 * 1000 / (8 * coilD * coilD * coilD * turnN_min);
            this.StiffnessTrackBar.Minimum = Convert.ToInt32(k_min / 0.1);
            this.StiffnessTrackBar.Maximum = Convert.ToInt32(k_max / 0.1);
            this.StiffnessTrackBar.Value = Convert.ToInt32(k / 0.1);

            currUnit.Stiffness = k;
            currUnit.CoilNum = turnN;

            // Update the currently selected unit
            controller.updateUnitFromGlobal(curridx, currUnit);

        }
        private void UnitBtn_Click(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            // Pass the current selected part's parameters
            Button temp = sender as Button;
            int start = temp.Name.IndexOf('U');
            int end = temp.Name.IndexOf('_');
            int idx = Int32.Parse(temp.Name.Substring(start + 1, end - start - 1));
            currUnit = controller.getUnitFromGlobal(idx);
            currIdx = idx;

            controller.springGeneration(ref currUnit);
            // Enable the spring control panel if it is not enabled
            set_control_panel_statues(true);

            // Initial the parameter panel
            initialize_parameter_panel(currUnit, currIdx);
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
            double wireD = 0;   // all discontinued curves share the same wire diameter
            double springPitch = d_min + gap_min;
            List<double> disLen = new List<double>();

            #region Compute the discontinued curves, coil diameters, wire diameter, coild numbers, and pitches
            double lengthPara;
            tempNewUnit.MA.LengthParameter(tempNewUnit.MA.GetLength(), out lengthPara);
            bool discontinuity = true;
            List<double> discontinuitylist = new List<double>();
            double startingPt = 0;
            while (discontinuity)
            {
                double t;
                discontinuity = tempNewUnit.MA.GetNextDiscontinuity(Continuity.Cinfinity_continuous, startingPt, lengthPara, out t);
                if (double.IsNaN(t) == false)
                {
                    discontinuitylist.Add(t);
                    startingPt = t;
                }
            }

            Curve[] discontinueCrv = null;
            if (discontinuitylist != null && discontinuitylist.Count > 0)
            {
                discontinueCrv = tempNewUnit.MA.Split(discontinuitylist);
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
                tempNewUnit.MA.LengthParameter(0, out srvStartPara);
                tempNewUnit.MA.LengthParameter(tempNewUnit.MA.GetLength(), out srvEndPara);
                Plane crvSegStartPln = new Plane(tempNewUnit.MA.PointAtStart, tempNewUnit.MA.TangentAt(srvStartPara));
                Plane crvSegEndPln = new Plane(tempNewUnit.MA.PointAtEnd, tempNewUnit.MA.TangentAt(srvEndPara));

                Curve[] interStartCrvs;
                Curve[] interEndCrvs;
                Point3d[] interStartPts;
                Point3d[] interEndPts;

                Rhino.Geometry.Intersect.Intersection.BrepPlane(surfaceBrep, crvSegStartPln, 0.001, out interStartCrvs, out interStartPts);
                Rhino.Geometry.Intersect.Intersection.BrepPlane(surfaceBrep, crvSegEndPln, 0.001, out interEndCrvs, out interEndPts);

                disLen.Add(tempNewUnit.MA.GetLength());

                double r1 = 0;
                double r2 = 0;
                foreach (Curve c in interStartCrvs)
                {
                    double p;
                    c.ClosestPoint(tempNewUnit.MA.PointAtStart, out p);
                    r1 = c.PointAt(p).DistanceTo(tempNewUnit.MA.PointAtStart);
                }

                foreach (Curve c in interEndCrvs)
                {
                    double p;
                    c.ClosestPoint(tempNewUnit.MA.PointAtEnd, out p);
                    r2 = c.PointAt(p).DistanceTo(tempNewUnit.MA.PointAtEnd);
                }
                // Use the entire central axis and get the average diameter of the spring coil
                coilDs.Add((r1 + r2) / 2);
                wireD = d_min;
                springPitch = d_min + gap_min;

                int cn = Convert.ToInt32(Math.Ceiling(tempNewUnit.MA.GetLength() / (d_min + gap_min)));
                coilNs.Add(cn);
            }

            #endregion

            // initial the parameters for the generated unit
            tempNewUnit.CoilDiameter = coilDs;
            tempNewUnit.WireDiameter = wireD;
            //tempNewUnit.CoilNum = coilNs;
            tempNewUnit.DiscontinuedLengths = disLen;
            tempNewUnit.ID = controller.getCountGlobal();
            tempNewUnit.Pitch = springPitch;
            tempNewUnit.Length = tempNewUnit.MA.GetLength();

            controller.addUnitToGlobal(tempNewUnit);

            if (tempNewUnit.BREPID != Guid.Empty)
            {
                // Add one unit button in the flow panel
                Button unitBtn = new Button();
                int crntIdx = controller.getCountGlobal() - 1;
                unitBtn.Name = "OU" + crntIdx.ToString() + "_" + tempNewUnit.BREPID.ToString();
                unitBtn.Text = "";
                unitBtn.BackColor = Color.FromArgb(150, 150, 150);
                unitBtn.Width = 15;
                unitBtn.Height = 34;
                unitBtn.FlatStyle = FlatStyle.Flat;
                unitBtn.FlatAppearance.BorderSize = 0;
                unitBtn.Click += UnitBtn_Click;

                currIdx = crntIdx;
                currUnit = controller.getUnitFromGlobal(crntIdx);

                // Update the database (XML) with the newly added unit
                //XmlDocument xmlDoc = new XmlDocument();
                //xmlDoc.Load(@"database\OnduleDB.xml");
                //XmlNodeList elemList = xmlDoc.GetElementsByTagName("Ondule");
                //for(int i=0; i<elemList.Count; ++i)
                //{
                //    // Create the node of the selected part
                //    XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, unitBtn.Name ,null);
                //    XmlNode nodeCoilNum = xmlDoc.CreateElement("CoilNum");
                //    nodeCoilNum.InnerText = "5";
                //    XmlNode nodeDiameter = xmlDoc.CreateElement("Diameter");
                //    XmlNode nodePitch = xmlDoc.CreateElement("Pitch");
                //    XmlNode nodeCoilDia = xmlDoc.CreateElement("CoilDiameter");
                //    elemList[i].AppendChild(node);
                //}

                OnduleUnitFlowPanel.Controls.Add(unitBtn);
                
            }
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
                controller.selectMASegment(ref currUnit);
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

        private void StiffnessTrackBar_Scroll(object sender, EventArgs e)
        {

        }

        private void StiffnessRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            this.WDTitleLabel.Hide();
            this.MinWDLabel.Hide();
            this.MaxWDLabel.Hide();
            this.TurnGapTitleLabel.Hide();
            this.MinTGLabel.Hide();
            this.MaxTGLabel.Hide();
            this.WireDiameterTrackBar.Hide();
            this.TurnGapTrackBar.Hide();
            this.WDValueLabel.Hide();
            this.TGValueLabel.Hide();

            this.MinStiffnessLabel.Show();
            this.MaxStiffnessLabel.Show();
            this.StiffnessTrackBar.Show();
        }

        private void AdvancedRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            this.WDTitleLabel.Show();
            this.MinWDLabel.Show();
            this.MaxWDLabel.Show();
            this.TurnGapTitleLabel.Show();
            this.MinTGLabel.Show();
            this.MaxTGLabel.Show();
            this.WireDiameterTrackBar.Show();
            this.TurnGapTrackBar.Show();
            this.WDValueLabel.Show();
            this.TGValueLabel.Show();

            this.MinStiffnessLabel.Hide();
            this.MaxStiffnessLabel.Hide();
            this.StiffnessTrackBar.Hide();
        }

        private void ClothBox_CheckedChanged(object sender, EventArgs e)
        {
            isOuterClothShown = !isOuterClothShown;
            if (currUnit.ClothIDs.Count > 0)
            {
                controller.showClothSpring(currUnit.ClothIDs, isOuterClothShown);
            }
        }
    }
}
