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
        }
        #endregion

        #region No Reference
        private void mt_Select_Click(object sender, EventArgs e)
        {
            controller.selection();
        }

        private void mt_wireFrame_Click(object sender, EventArgs e)
        {

            controller.wireframe();
        }
        #endregion

        #region deformation triggers (reserved for the other window control)

        private void Bend_Click(object sender, EventArgs e)
        {
            // ask the user to select the medium axis
            const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.Curve;
            Rhino.DocObjects.ObjRef objRef;
            Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one object", false, filter, out objRef);

            if (rc == Rhino.Commands.Result.Success)
            {
                // send the object reference to the rhinomodel basically
                controller.bendDeform(objRef);
            }
        }

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

        private void LinearBend_Click(object sender, EventArgs e)
        {
            // ask the user to select the medium axis
            const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.Curve;
            Rhino.DocObjects.ObjRef objRef;
            Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one object", false, filter, out objRef);

            if (rc == Rhino.Commands.Result.Success)
            {
                // send the object reference to the rhinomodel basically
                controller.linearBendDeform(objRef);
            }
        }

        private void LinearTwistBend_Click(object sender, EventArgs e)
        {
            is_freeform = true;

            // ### FOR DEBUG ###
            String path = @"Resources\FreeForm_mode.png";
            // ### FOR RELEASE ###
            //String path = @"OndulePlugin\Resources\FreeForm_mode.png";

            SpringfyBtn.BackgroundImage = Image.FromFile(path);
            SpringfyBtn.Cursor = Cursors.Default;
            SpringfyBtn.BackColor = Color.White;

            //// ask the user to select the medium axis
            //const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.Curve;
            //Rhino.DocObjects.ObjRef objRef;
            //Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one object", false, filter, out objRef);

            //if (rc == Rhino.Commands.Result.Success)
            //{
            //    // send the object reference to the rhinomodel basically
            //    controller.allDeform(objRef);
            //}
        }

        private void TwistBend_Click(object sender, EventArgs e)
        {
            // ask the user to select the medium axis
            const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.Curve;
            Rhino.DocObjects.ObjRef objRef;
            Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one object", false, filter, out objRef);

            if (rc == Rhino.Commands.Result.Success)
            {
                // send the object reference to the rhinomodel basically
                controller.twistBendDeform(objRef);
            }
        }

        #endregion

        #region [Not Used] Old approach of generating the medial axis
        //private void MedialAxisGeneration_Click(object sender, EventArgs e)
        //{
        //    controller.medialAxisTransform();
        //}
        #endregion

        private void SpringGen_Click(object sender, EventArgs e)
        {
            #region Not used
            //// ask the user to select the medium axis
            //const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.Curve;
            //Rhino.DocObjects.ObjRef objRef;
            //Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one object", false, filter, out objRef);

            //if (rc == Rhino.Commands.Result.Success)
            //{
            //    controller.springGeneration(objRef.Curve());
            //}
            #endregion

        }

        private void MATButton_Click(object sender, EventArgs e)
        {
            // ask the user to select the medium axis
            is_freeform = false;
            // ### FOR DEBUG ###
            String path = @"Resources\FreeForm_default.png";
            // ### FOR RELEASE ###
            //String path = @"OndulePlugin\Resources\FreeForm_default.png";

            SpringfyBtn.BackgroundImage = Image.FromFile(path);
            SpringfyBtn.Cursor = Cursors.Default;
            SpringfyBtn.BackColor = Color.White;
            
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
            double springPitch = d_min+gap_min;
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
                springPitch = d_min+gap_min;

                int cn = Convert.ToInt32(Math.Ceiling(tempNewUnit.MA.GetLength() / (d_min + gap_min)));
                coilNs.Add(cn);
            }

            #endregion

            // initial the parameters for the generated unit
            tempNewUnit.CoilDiameter = coilDs;
            tempNewUnit.WireDiameter = wireD;
            tempNewUnit.CoilNum = coilNs;
            tempNewUnit.DiscontinuedLengths = disLen;
            tempNewUnit.ID = controller.getCountGlobal();
            tempNewUnit.Pitch = springPitch;
            tempNewUnit.Length = tempNewUnit.MA.GetLength();

            controller.addUnitToGlobal(tempNewUnit);

            if(tempNewUnit.BREPID != Guid.Empty)
            {
                // Add one unit button in the flow panel
                Button unitBtn = new Button();
                int crntIdx = controller.getCountGlobal()-1;
                unitBtn.Name = "OU"+ crntIdx.ToString()+ "_" + tempNewUnit.BREPID.ToString();
                unitBtn.Text = "";
                unitBtn.BackColor = Color.FromArgb(150, 150, 150);
                unitBtn.Width = 20;
                unitBtn.Height = 90;
                unitBtn.FlatStyle = FlatStyle.Flat;
                unitBtn.FlatAppearance.BorderSize = 0;
                unitBtn.Click += UnitBtn_Click;
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
        }

        private void MATButton_MouseEnter(object sender, EventArgs e)
        {
            // ### FOR DEBUG ###
            String path = @"Resources\MAT_active.png";
            // ### FOR RELEASE ###
            //String path = @"OndulePlugin\Resources\MAT_active.png";

            MATBtn.BackgroundImage = Image.FromFile(path);
            MATBtn.Cursor = Cursors.Default;
            MATBtn.BackColor = Color.White;
        }

        private void MATButton_MouseLeave(object sender, EventArgs e)
        {
            // ### FOR DEBUG ###
            String path = @"Resources\MAT_default.png";
            // ### FOR RELEASE ###
            //String path = @"OndulePlugin\Resources\MAT_default.png";

            MATBtn.BackgroundImage = Image.FromFile(path);
            MATBtn.BackColor = Color.White;
            MATBtn.Cursor = Cursors.Default;
        }

        private void MATButton_MouseHover(object sender, EventArgs e)
        {
            // ### FOR DEBUG ###
            String path = @"Resources\MAT_active.png";
            // ### FOR RELEASE ###
            //String path = @"OndulePlugin\Resources\MAT_active.png";

            MATBtn.BackgroundImage = Image.FromFile(path);
            MATBtn.Cursor = Cursors.Default;
            MATBtn.BackColor = Color.White;
        }

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

        private void LinearLockBtn_Click(object sender, EventArgs e)
        {

        }

        private void TwistLockBtn_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void FreeFormBtn_MouseEnter(object sender, EventArgs e)
        {
            if (!is_freeform)
            {
                // ### FOR DEBUG ###
                String path = @"Resources\FreeForm_active.png";
                // ### FOR RELEASE ###
                //String path = @"OndulePlugin\Resources\FreeForm_active.png";

                SpringfyBtn.BackgroundImage = Image.FromFile(path);
                SpringfyBtn.Cursor = Cursors.Default;
                SpringfyBtn.BackColor = Color.White;
            }
            
        }

        private void FreeFormBtn_MouseHover(object sender, EventArgs e)
        {
            if (!is_freeform)
            {
                // ### FOR DEBUG ###
                String path = @"Resources\FreeForm_active.png";
                // ### FOR RELEASE ###
                //String path = @"OndulePlugin\Resources\FreeForm_active.png";

                SpringfyBtn.BackgroundImage = Image.FromFile(path);
                SpringfyBtn.Cursor = Cursors.Default;
                SpringfyBtn.BackColor = Color.White;
            }
        }

        private void FreeFormBtn_MouseLeave(object sender, EventArgs e)
        {
            if (!is_freeform)
            {
                // ### FOR DEBUG ###
                String path = @"Resources\FreeForm_default.png";
                // ### FOR RELEASE ###
                //String path = @"OndulePlugin\Resources\FreeForm_default.png";

                SpringfyBtn.BackgroundImage = Image.FromFile(path);
                SpringfyBtn.Cursor = Cursors.Default;
                SpringfyBtn.BackColor = Color.White;
            }
        }

 



        private void PreviewBtn_Click(object sender, EventArgs e)
        {
            // ask the user to select the medium axis
            const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.Curve;
            Rhino.DocObjects.ObjRef objRef;
            Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one object", false, filter, out objRef);

            if (rc == Rhino.Commands.Result.Success)
            {
                // send the object reference to the rhinomodel basically
                controller.allDeform(objRef);
            }
        }

        private void Springfy_Click(object sender, EventArgs e)
        {
            if (currUnit != null && currIdx != -1)
            {
                // Currently we don't need to specify the polysurface explicitly
                // The currUnit includes the GUID of the surface

                //const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.PolysrfFilter;// filter allows us to constrain the type of objects the user can select
                //Rhino.DocObjects.ObjRef sufObjRef;
                //Guid sufObjId = Guid.Empty; // all rhino doc objects has a unique ID. We can always find the object by create an objRef with the id
                //Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one surface to print", false, filter, out sufObjRef);
                //if (rc == Rhino.Commands.Result.Success)
                //{
                //    sufObjId = sufObjRef.ObjectId;
                //}

                DeformationDesignForm coilwindow = new DeformationDesignForm(currUnit, currIdx,controller);
                coilwindow.Show();
            }
            else
            {
                DeformationDesignForm coilwindow = new DeformationDesignForm(controller);
                coilwindow.Show();
            }
          
        }

        private void ExportBtn_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("Hello World!");
        }
    }
}
