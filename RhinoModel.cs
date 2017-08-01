using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Input;

namespace PluginBar
{
    public interface RhinoModel
    {

        void printSTL(ObjRef obj, Point3d centerPt);

        void deformBrep(ObjRef obj);

        void wireframeAll();

        void selection();

        void linearDeform(ObjRef objRef);

    }

    class PointEqualityComparer : IEqualityComparer<Point3d>
    {
        public bool Equals(Point3d p1, Point3d p2)
        {

            if (p1 == null && p1 == null)
                return true;
            else if (p1 == null | p2 == null)
                return false;
            else if (p1.DistanceTo(p2)<1)
                return true;
            else
                return false;
        }

        public int GetHashCode(Point3d bx)
        {
            int hCode = (int)bx.X ^ (int)bx.Y ^ (int)bx.Z;
            return hCode.GetHashCode();
        }
    }
    public class IncRhinoModel : RhinoModel
    {

        RhinoDoc myDoc = null;

        List<Point3d> wireframePtList = new List<Point3d>();

        List<Point3d> dyndrawLst = new List<Point3d>();
        Rhino.Collections.RhinoList<Guid> ptIds = new Rhino.Collections.RhinoList<Guid>();

        ObjectAttributes redAttribute, whiteAttribute, greenAttribute, orangeAttribute;

        Curve centerCrv = null;
        Point3d centerPt;

        public IncRhinoModel()
        {
            myDoc = PluginBarCommand.rhinoDoc;
            if (myDoc == null)
            {
                myDoc = RhinoDoc.ActiveDoc;
            }

            int redIndex = myDoc.Materials.Add();
            Rhino.DocObjects.Material redMat = myDoc.Materials[redIndex];
            redMat.DiffuseColor = System.Drawing.Color.Red;
            redMat.SpecularColor = System.Drawing.Color.Red;
            redMat.CommitChanges();
            redAttribute = new ObjectAttributes();
            redAttribute.LayerIndex = 1;
            redAttribute.MaterialIndex = redIndex;
            redAttribute.MaterialSource = Rhino.DocObjects.ObjectMaterialSource.MaterialFromObject;

            int whiteIndex = myDoc.Materials.Add();
            Rhino.DocObjects.Material whiteMat = myDoc.Materials[whiteIndex];
            whiteMat.DiffuseColor = System.Drawing.Color.White;
            whiteMat.SpecularColor = System.Drawing.Color.White;
            whiteMat.Transparency = 0.1;
            whiteMat.CommitChanges();
            whiteAttribute = new ObjectAttributes();
            
            whiteAttribute.LayerIndex = 2;
            whiteAttribute.MaterialIndex = whiteIndex;
            whiteAttribute.MaterialSource = Rhino.DocObjects.ObjectMaterialSource.MaterialFromObject;

            int greenIndex = myDoc.Materials.Add();
            Rhino.DocObjects.Material greenMat = myDoc.Materials[greenIndex];
            greenMat.DiffuseColor = System.Drawing.Color.Green;
            greenMat.SpecularColor = System.Drawing.Color.Green;
            greenMat.CommitChanges();
            greenAttribute = new ObjectAttributes();
            greenAttribute.LayerIndex = 3;
            greenAttribute.MaterialIndex = greenIndex;
            greenAttribute.MaterialSource = Rhino.DocObjects.ObjectMaterialSource.MaterialFromObject;

            int orangeIndex = myDoc.Materials.Add();
            Rhino.DocObjects.Material orangeMat = myDoc.Materials[orangeIndex];
            orangeMat.DiffuseColor = System.Drawing.Color.Orange;
            orangeMat.Transparency = 0.3;
            orangeMat.SpecularColor = System.Drawing.Color.Orange;
            orangeMat.CommitChanges();
            orangeAttribute = new ObjectAttributes();
            orangeAttribute.LayerIndex = 4;
            orangeAttribute.MaterialIndex = orangeIndex;
            orangeAttribute.MaterialSource = Rhino.DocObjects.ObjectMaterialSource.MaterialFromObject;


            myDoc.Views.Redraw();

        }

        public void linearDeform(ObjRef objRef)
        {

            string debugOutput = "";
            const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.PolysrfFilter;

            Rhino.DocObjects.ObjRef sufObjRef;

            Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one surface to print", false, filter, out sufObjRef);

            if (rc == Rhino.Commands.Result.Success)
            {
                String scriptString = "";
                scriptString = String.Format("offsetSrf SelLast {0} D {1} F {2} {3}", "Enter", "1", "FlipAll", "Enter");
                Rhino.RhinoApp.SetFocusToMainWindow(); // Change the focus so the user can select a curve
                Rhino.RhinoApp.RunScript(scriptString, true); // Send command to command line

                int t = 0;
            }







            // generate the start and end plane of the curve
            Curve c = objRef.Curve();
            Point3d startPt = c.PointAtStart;
            Point3d endPt = c.PointAtEnd;
            double startPara = 0, endPara = 0;
            c.LengthParameter(0, out startPara);
            c.LengthParameter(c.GetLength(), out endPara);
            Plane startPln = new Plane(startPt, c.TangentAt(startPara));
            Plane endPln = new Plane(endPt, c.TangentAt(endPara));
            Rhino.Geometry.PlaneSurface startPlnSuf = new PlaneSurface(startPln, new Interval(-10, 10), new Interval(-10, 10));
            Rhino.Geometry.PlaneSurface endPlnSuf = new PlaneSurface(endPln, new Interval(-10, 10), new Interval(-10, 10));
            myDoc.Objects.AddSurface(startPlnSuf);
            myDoc.Objects.AddSurface(endPlnSuf);
            myDoc.Views.Redraw();

            // control point can move along the curve, once click the button the end of the point is the spring length
            centerCrv = c;
            Rhino.Input.Custom.GetPoint gp = new Rhino.Input.Custom.GetPoint();
            gp.DynamicDraw += Gp_CurveDynamicDraw;
            gp.MouseMove += Gp_CurveMouseMove;
            gp.Get(true);

            myDoc.Objects.AddSphere(new Sphere(centerPt, 2));
            myDoc.Views.Redraw();

            //compress curve
            Curve compressCrv = c.DuplicateCurve();
            double compressCrvPara = 0;
            compressCrv.ClosestPoint(centerPt, out compressCrvPara);
            Curve[] splitCrvs = compressCrv.Split(compressCrvPara);
            myDoc.Objects.AddCurve(splitCrvs[1], greenAttribute);
            myDoc.Views.Redraw();

            //curve discontinuity?
            double lengthPara;
            c.LengthParameter(c.GetLength(), out lengthPara);
            bool discontinuity = true;
            List<double> discontinuitylist = new List<double>();
            double startingPt = 0;
            while (discontinuity)
            {
                double t;
                discontinuity = c.GetNextDiscontinuity(Continuity.Cinfinity_continuous, startingPt, lengthPara, out t);
                if (double.IsNaN(t) == false)
                {
                    discontinuitylist.Add(t);
                    startingPt = t;
                }
            }

            Curve[] discontinueCrv = null;
            if (discontinuitylist != null && discontinuitylist.Count > 0)
            {
                discontinueCrv = c.Split(discontinuitylist);
            }

            // gennerate spiral for each segment of the curve
            Point3d spiralStartPt = new Point3d(0, 0, 0);
            List<NurbsCurve> spiralCrvList = new List<NurbsCurve>();
            if (discontinueCrv != null)
            {
                foreach (Curve crv in discontinueCrv)
                {
                    crv.LengthParameter(crv.GetLength(), out endPara);
                    NurbsCurve spiralCrv = NurbsCurve.CreateSpiral(crv, 0, endPara, spiralStartPt, 5, 0, 5, 5, 12);
                    spiralStartPt = spiralCrv.PointAtEnd;
                    spiralCrvList.Add(spiralCrv);
                }
            }
            Curve joinedSpiral = Curve.JoinCurves(spiralCrvList)[0];
            if (joinedSpiral != null)
            {
                myDoc.Objects.AddCurve(joinedSpiral);
            }

            // sweep section to create spring solid
            Plane spiralStartPln = new Plane(joinedSpiral.PointAtStart, joinedSpiral.TangentAtStart);
            Circle startCircle = new Circle(spiralStartPln, joinedSpiral.PointAtStart, 1);
            myDoc.Objects.AddCurve(startCircle.ToNurbsCurve());
            var sweep = new Rhino.Geometry.SweepOneRail();
            sweep.AngleToleranceRadians = myDoc.ModelAngleToleranceRadians;
            sweep.ClosedSweep = false;
            sweep.SweepTolerance = myDoc.ModelAbsoluteTolerance;

            var breps = sweep.PerformSweep(joinedSpiral, startCircle.ToNurbsCurve());
            Brep capped = breps[0].CapPlanarHoles(myDoc.ModelRelativeTolerance);

            myDoc.Objects.AddBrep(capped);

            myDoc.Views.Redraw();

            // offset the surface?


        }

        private void Gp_CurveMouseMove(object sender, Rhino.Input.Custom.GetPointMouseEventArgs e)
        {
            if (e.ShiftKeyDown == true)
            {
                double ptPara = 0;
                centerCrv.ClosestPoint(e.Point, out ptPara);
                centerPt = centerCrv.PointAt(ptPara);
            }
            else
            {
                centerPt = centerCrv.PointAtStart;
            }
        }

        private void Gp_CurveDynamicDraw(object sender, Rhino.Input.Custom.GetPointDrawEventArgs e)
        {
            e.Display.DrawSphere(new Sphere(centerPt, 2), System.Drawing.Color.Azure);
        }

        public void selection()
        {
            Rhino.Input.Custom.GetPoint gp = new Rhino.Input.Custom.GetPoint();
            gp.DynamicDraw += Gp_SelectionDynamicDraw;
            gp.MouseMove += Gp_SelectionMouseMove;
            string currentPath = System.IO.Directory.GetCurrentDirectory();
            string parentPath = System.IO.Directory.GetParent(currentPath).ToString() + "\\Resources\\ball.cur";
            System.Windows.Forms.Cursor myCursor = new System.Windows.Forms.Cursor(parentPath);
            gp.SetCursor(myCursor);
            gp.Get(true);
            if(dyndrawLst.Count>0)
            {
                myDoc.Objects.AddPoints(dyndrawLst);
                myDoc.Views.Redraw();
            }
            

        }

        private void Gp_SelectionMouseMove(object sender, Rhino.Input.Custom.GetPointMouseEventArgs e)
        {
            if(e.ShiftKeyDown == true)
            {
                foreach (var id in ptIds)
                    myDoc.Objects.Show(id, true);
                PointEqualityComparer ptCompare = new PointEqualityComparer();
                foreach (Point3d p in wireframePtList)
                {
                    if (e.Point.DistanceTo(p) < 5 && dyndrawLst.Contains(p, ptCompare) == false)
                    {
                        dyndrawLst.Add(p);
                    }
                }
            }
            else
            {
                foreach (var id in ptIds)
                    myDoc.Objects.Hide(id, true);
            }
        }

        private void Gp_SelectionDynamicDraw(object sender, Rhino.Input.Custom.GetPointDrawEventArgs e)
        {
            e.Display.DrawPoints(dyndrawLst, Rhino.Display.PointStyle.Simple, 10, System.Drawing.Color.Red);
        }

        public void deformBrep(ObjRef obj)
        {
            Brep bbox = obj.Brep().GetBoundingBox(true).ToBrep();

            myDoc.Objects.AddBrep(bbox);
            myDoc.Views.Redraw();
        }


        public void wireframeAll()
        {
            String scriptString = "";
            scriptString = String.Format("Explode {0}", "Enter");
            Rhino.RhinoApp.SetFocusToMainWindow(); // Change the focus so the user can select a curve
            Rhino.RhinoApp.RunScript(scriptString, false); // Send command to command line


            PointEqualityComparer ptCompare = new PointEqualityComparer();
            ObjectEnumeratorSettings settings = new ObjectEnumeratorSettings();
            settings.ObjectTypeFilter = ObjectType.Extrusion | ObjectType.Brep | ObjectType.BrepLoop | ObjectType.PolysrfFilter | ObjectType.Surface;

            double tolerance = 0;
            double overlapTolerance = 0;
            foreach (RhinoObject rhObj in myDoc.Objects.GetObjectList(settings))
            {
                Brep brep = new ObjRef(rhObj.Id).Brep();
                int icut = 3;
                Curve[] wireframeCvs = null;

                double area = brep.GetArea();

                if (area < 10)
                   icut = -1;
                else if (area < 100)
                   icut = -1;
                else if (area < 1000)
                   icut = 1;

                wireframeCvs = brep.GetWireframe(icut);

                List<Point3d> pts = new List<Point3d>();
                for(int i=0; i<wireframeCvs.Length;i++)
                {
                    for(int j=1; j<wireframeCvs.Length;j++)
                    {
                        Rhino.Geometry.Intersect.CurveIntersections intersection = Rhino.Geometry.Intersect.Intersection.CurveCurve(wireframeCvs[i], wireframeCvs[j], tolerance, overlapTolerance);
                        if(intersection!=null && intersection.Count>0)
                        {
                            foreach(var v in intersection)
                            {
                                if (pts.Contains(v.PointA, ptCompare) == false)
                                    pts.Add(v.PointA);
                            }
                        }
                    }
                }
                ptIds.AddRange(myDoc.Objects.AddPoints(pts,orangeAttribute));

                wireframePtList.AddRange(pts);
            }
            foreach (var id in ptIds)
                myDoc.Objects.Hide(id, true);


            myDoc.Views.Redraw();

        }

        public void printSTL(ObjRef obj, Point3d centerPt)
        {


            Mesh meshObj = obj.Mesh();
            
            ObjectAttributes atr = myDoc.CreateDefaultAttributes();
            atr.LayerIndex = 3;

            Point3d ctpt = centerPt;
            myDoc.Objects.AddPoint(ctpt, atr);
            myDoc.Views.Redraw();



            Point3d edpt = ctpt;


            var rc = Rhino.RhinoApp.RunScript("-Export " + "temp" + ".stl y=n Enter Enter", false);


            //start python to convert it to new points
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.FileName = "../Slic3r/slic3r.exe";

                p.StartInfo.Arguments = "../ExportSTL/" + "temp" + ".stl" + " --load ../Slic3r/magnetprinter_config.ini  --print-center 0,0";// +offsetX.ToString() + "," + offsetY.ToString();//-4,0"
            p.Start();

            //read in the converted file and save it to the document folder
            int milliseconds = 2500;
            System.Threading.Thread.Sleep(milliseconds);


            string readConverted = string.Empty;
            try
            {
                readConverted = System.IO.File.ReadAllText("../ExportSTL/temp.ngc");
            }
            catch (ArgumentNullException e)
            {
                string error = "what the fuck" + e.Message;
            }

            string result = string.Empty;

            if (readConverted != string.Empty)
            {
                int lineCounter = 0;
                string aLine;
                StringReader strReader = new StringReader(readConverted);
              
                result += String.Format("G1 B{0} C{1} F500 \n", 0, 0);

                if (result != string.Empty)
                {
                    File.WriteAllText("../ExportSTL/" +  ".ngc", result);
                }

            }

        }

        
 
    }

}