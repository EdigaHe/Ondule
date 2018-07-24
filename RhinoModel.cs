using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Input;
using System.Drawing;
using System.Diagnostics;

namespace OndulePlugin
{
    public struct ma_parameters
    {
        public float initial_radius;
        public bool nan_for_initr;
        public double denoise_preserve;
        public double denoise_planar;
    }
    public struct ma_result
    {
        public Vector3d c;
        public int qidx;
    }

    public struct ma_data
    {
        public List<Point3d> coords;
        public List<Vector3d> normals;
        public List<Point3d> ma_coords;
        public List<int> ma_qidx;

        public List<float> lfs;
        public List<bool> mask;

        public List<Point3d> nearest_point;
    }

    public struct bend_info
    {
        public Vector3d dir;
        public double strength;
    }

    public interface RhinoModel
    {

        #region Default Rhino plugin functions
        void printSTL(ObjRef obj, Point3d centerPt);
        void deformBrep(ObjRef obj);
        void wireframeAll();
        void selection();
        #endregion

        //void linearDeform(ObjRef objRef);
        //void twistDeform(ObjRef objRef);
        //void bendDeform(ObjRef objRef);
        ////void linearTwistDeform(ObjRef objRef);
        //void linearBendDeform(ObjRef objRef);
        //void twistBendDeform(ObjRef objRef);
        //void allDeform(ObjRef objRef);
        void medialAxisTransform();



        #region Functions for new Ondule interface
        void springGen(ref OnduleUnit objRef);
        OnduleUnit maGen();
        void addTwistConstraint(OnduleUnit obj);
        void addLinearConstraint(OnduleUnit obj);
        void addLinearTwistConstraint(OnduleUnit obj);
        void addBendConstraint(OnduleUnit obj, Boolean dir);
        #endregion
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
        #region declarations of local variables for all features
        RhinoDoc myDoc = null;

        List<Point3d> wireframePtList = new List<Point3d>();

        List<Point3d> dyndrawLst = new List<Point3d>();
        Rhino.Collections.RhinoList<Guid> ptIds = new Rhino.Collections.RhinoList<Guid>();

        ObjectAttributes redAttribute, whiteAttribute, greenAttribute, orangeAttribute;

        Curve centerCrv = null;
        Point3d centerPt;
        Point3d middlePt;

        int neighbor_shreshold = 10;
        int initial_radius = 100;
        double preserve = (3.14159265358979323846 / 180.0) * 65;
        double planar = (3.14159265358979323846 / 180.0) * 132;
        List<Point3d> ma_effect = new List<Point3d>();
        List<Point3d> final_ma = new List<Point3d>();

        // points on median axis
        List<Point3d> maPoints = new List<Point3d>(); 

        Point3d angleCtrlPt;
        Point3d bendCtrlPt;
        Point3d bendCtrlSecPt;
        Plane anglePlane;
        Plane bendPlane;
        Point3d angleCtrlSecPt;
        Point3d bendPtSel;
        Circle angleCircle;
        double twistAngle = 0;

        bool isBendLinear = false;

        List<bend_info> bendInfoList = new List<bend_info>();

        bool is_BendStart = false;

        List<double> DiameterList = new List<double>();
        #endregion

        #region local variables for the new interface
        Guid ctrlPt1, ctrlPt2;
        #endregion

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

        /// <summary>
        /// Generation of the helical spring. By default, spring index (C) is 9 (C ~ (6-12))
        /// Calaculate the smallest D in this part and if the computed smallest d is less than printable min diameter (2mm),
        /// use min printable diameter (2mm), if C falls out of the suggested range, report a warning. Otherwise, print C and d.
        /// </summary>
        /// <param name="centerCrv"> The central curve</param>
        /// <param name="surfaceBrep"> Outer surface</param>
        /// <param name="pitch"> Computed pitch based on the user's input tight level of the spring</param>
        /// <param name="designType">The design of the spring-based structure</param>
        /// <param name="K"> stiffness of the generated spring</param>
        /// <returns></returns>
        private Curve springGeneration(Curve centerCrv, Brep surfaceBrep, double pitch, int designType, out double K)
        {
            //DEBUG: Currently the bug is the center curve is only cut when there is a discontinuity, this is not good enough to have a nice spring approximation to the outer shell's shape.
            // Record the diameters of all segments in the selected part
            DiameterList.Clear();
            double minDiameter = 1000000000;

            #region 1. Find center curve's discontinuity

            double lengthPara;
            centerCrv.LengthParameter(centerCrv.GetLength(), out lengthPara);
            bool discontinuity = true;
            List<double> discontinuitylist = new List<double>();
            double startingPt = 0;
            while (discontinuity)
            {
                double t;
                discontinuity = centerCrv.GetNextDiscontinuity(Continuity.Cinfinity_continuous, startingPt, lengthPara, out t);
                if (double.IsNaN(t) == false)
                {
                    discontinuitylist.Add(t);
                    startingPt = t;
                }
            }

            Curve[] discontinueCrv = null;
            if (discontinuitylist != null && discontinuitylist.Count > 0)
            {
                discontinueCrv = centerCrv.Split(discontinuitylist);
            }
            #endregion

            #region 2. gennerate spiral for each segment of the curve
            Point3d spiralStartPt = new Point3d(0, 0, 0);
            List<NurbsCurve> spiralCrvList = new List<NurbsCurve>();
            double endPara;
            if (discontinueCrv != null)
            {
                foreach (Curve crv in discontinueCrv)
                {
                    crv.LengthParameter(crv.GetLength(), out endPara);
                    double r1 = 5, r2 = 5;
                    r1 = surfaceBrep.ClosestPoint(crv.PointAtStart).DistanceTo(crv.PointAtStart);
                    r2 = surfaceBrep.ClosestPoint(crv.PointAtEnd).DistanceTo(crv.PointAtEnd);
                    DiameterList.Add(r1);
                    DiameterList.Add(r2);
                    if (r1 <= minDiameter) minDiameter = r1;
                    if (r2 <= minDiameter) minDiameter = r2;

                    // test if pitch is greater than coild diameter + 0.8mm (test result)
                    double initDia = (r1<r2?r1:r2) / 12;
                    if (initDia < 2)
                    {
                        initDia = 2;
                    }

                    while (pitch <= initDia + 0.8) { pitch += 0.2; }

                    NurbsCurve spiralCrv = NurbsCurve.CreateSpiral(crv, 0, endPara, spiralStartPt, pitch, 0, r1-1, r2-1, 30);
                    spiralStartPt = spiralCrv.PointAtEnd;
                    spiralCrvList.Add(spiralCrv);
                }
            }

            Curve joinedSpiral = Curve.JoinCurves(spiralCrvList)[0];
            if (joinedSpiral != null)
            {
                //myDoc.Objects.AddCurve(joinedSpiral);
            }
            #endregion

            #region 3. sweep section to create spring solid

            Plane spiralStartPln = new Plane(joinedSpiral.PointAtStart, joinedSpiral.TangentAtStart);

            // compute the coild diameter, must be greater than 2mm (test result)
            double C = 6;
            double mindia = minDiameter / C;
            if(mindia >= 2)
            {
                RhinoApp.WriteLine("coil diameter is: " + mindia);
            }
            else
            {
                double newMinDia = 2;
                if (minDiameter / newMinDia < 6)
                {
                    // warning!!
                    RhinoApp.WriteLine("The spring index (C) is less than 6. Anyways, coild diameter is set to 2mm.");
                }
                mindia = newMinDia;
            }

            Circle startCircle = new Circle(spiralStartPln, joinedSpiral.PointAtStart, mindia/2); //spring's cross section's radius is currently 1. This should be tuned based on the shape the user selected and also the test limit we have.
            var sweep = new Rhino.Geometry.SweepOneRail();
            sweep.AngleToleranceRadians = myDoc.ModelAngleToleranceRadians;
            sweep.ClosedSweep = false;
            sweep.SweepTolerance = myDoc.ModelAbsoluteTolerance;

            var breps = sweep.PerformSweep(joinedSpiral, startCircle.ToNurbsCurve());
            List<Brep> cappedSpring = new List<Brep>();
            if (breps.Length > 0)
            {
                foreach (Brep b in breps)
                {
                    cappedSpring.Add(b.CapPlanarHoles(myDoc.ModelRelativeTolerance));
                    myDoc.Objects.AddBrep(b.CapPlanarHoles(myDoc.ModelRelativeTolerance));
                }
            }
            #endregion

            #region compute the spring stiffness K = (d^4 * G) / (8 * (D_avg/d)^3 * (L/Pitch))
            double G = 2.4;     // PLA's shear modulus is 2.4GPa (0.35 * 10E6 psi)
            double D_avg = 0;
            foreach(double r in DiameterList)
            {
                D_avg += r;
            }
            D_avg = D_avg / DiameterList.Count();

            double L = centerCrv.GetLength();
            K = (Math.Pow(mindia, 4) * G) / (8 * Math.Pow((D_avg / mindia), 3) * (L / pitch));

            #endregion

            return startCircle.ToNurbsCurve();
        }

        /// <summary>
        /// New algorithm for spring generation
        /// </summary>
        public void springGen(ref OnduleUnit objRef)
        {
            #region The generated spring is in white
            var white_attributes = new ObjectAttributes();
            white_attributes.ObjectColor = Color.White;
            white_attributes.ColorSource = ObjectColorSource.ColorFromObject;
            #endregion

            #region Get the outer polysurface and generate start and end planes based on the central axis
            //// in the current code we ask the user to select the outer shell of the geometry. In the real case this should be the same
            //// part as our point cloud selection so user don't need to select themselves.
            //const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.PolysrfFilter;// filter allows us to constrain the type of objects the user can select
            //Rhino.DocObjects.ObjRef sufObjRef;
            //Guid sufObjId = Guid.Empty; // all rhino doc objects has a unique ID. We can always find the object by create an objRef with the id
            //Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one surface to print", false, filter, out sufObjRef);
            //if (rc == Rhino.Commands.Result.Success)
            //{
            //    sufObjId = sufObjRef.ObjectId;
            //}

            // Get the outer polysurface
            Guid sufObjID = objRef.BREPID;
            ObjRef armOffsetObjRef = new ObjRef(sufObjID);//get the objRef from the GUID
            Brep surfaceBrep = armOffsetObjRef.Brep(); // because we know the geometry is Brep, we directly find it from the objRef 

            // Create the tangent planes 
            Curve centerCrv = objRef.MA;
            Point3d startPt = centerCrv.PointAtStart;
            Point3d endPt = centerCrv.PointAtEnd;
            double startPara = 0, endPara = 0;
            centerCrv.LengthParameter(0, out startPara);
            centerCrv.LengthParameter(centerCrv.GetLength(), out endPara);
            Plane startPln = new Plane(startPt, centerCrv.TangentAt(startPara));// the plane at the start of the curve
            Plane endPln = new Plane(endPt, centerCrv.TangentAt(endPara));// the plane at the end of the curve
            Rhino.Geometry.PlaneSurface startPlnSuf = new PlaneSurface(startPln, new Interval(-300, 300), new Interval(-300, 300));// convert the plane object to an actual surface so that we can draw it in the rhino doc
            Rhino.Geometry.PlaneSurface endPlnSuf = new PlaneSurface(endPln, new Interval(-300, 300), new Interval(-300, 300));// 15 is a random number. It should actually be the ourter shell's radius or larger.
            //myDoc.Objects.AddSurface(startPlnSuf);
            //myDoc.Objects.AddSurface(endPlnSuf);
            //myDoc.Views.Redraw();
            #endregion

            List<Brep> toBeReplacedBrep = new List<Brep>();
            if(objRef.ReplacedBreps.Count > 0)
            {
                // Update the spring design
                toBeReplacedBrep = objRef.ReplacedBreps;
            }
            else
            {
                // Chop the brep and store the splitted parts
                #region chop the shells and record the part that spring will replace
                // chop the shells into 3 piece
                List<Brep> splitsurf = new List<Brep>();
                Brep[] firstSplit = surfaceBrep.Split(startPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);

                Brep[] test = firstSplit[0].Split(endPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);
                if (test != null && test.Length > 0)
                {
                    splitsurf.AddRange(test);
                    splitsurf.Add(firstSplit[1]);
                }
                else
                {
                    test = firstSplit[1].Split(endPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);
                    splitsurf.AddRange(test);
                    splitsurf.Add(firstSplit[0]);
                }

                List<Brep> finalPreservedBrepList = new List<Brep>();
                List<Brep> replacedBrepList = new List<Brep>(); // all parts that are replaced by springs

                foreach (Brep b in splitsurf)
                {
                    Point3d bcenter = b.GetBoundingBox(true).Center;
                    Vector3d v1 = bcenter - endPln.Origin;
                    Vector3d v2 = bcenter - startPln.Origin;
                    if (v1 * v2 > 0)
                    {
                        Brep temp = b.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
                        finalPreservedBrepList.Add(temp);
                    }
                    else
                    {
                        //Brep tempReplace = b.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
                        //replacedBrepList.Add(tempReplace);
                        replacedBrepList.Add(b);
                    }

                }

                // Update the Ondule unit's preservedbrep list and replacedbrep list
                objRef.PreservedBreps = finalPreservedBrepList;
                objRef.ReplacedBreps = replacedBrepList;

                myDoc.Objects.Hide(sufObjID, true);// hide the original shell

                foreach (Brep b in finalPreservedBrepList)
                {
                    myDoc.Objects.AddBrep(b);
                }

                #endregion

                toBeReplacedBrep = replacedBrepList;
                
            }

            #region replace the selected part with helical spring (for arbitrary geometry)
            foreach (Brep b in toBeReplacedBrep)
            {
                //myDoc.Objects.AddBrep(b, redAttribute);
                //myDoc.Views.Redraw();

                #region generate the spiral that fits in the geometry

                // centerCrv - central curve
                // b - brep of the selected surface
                // pitch: 1mm
                double pitch = objRef.Pitch+objRef.WireDiameter;

                //DEBUG: Currently the bug is the center curve is only cut when there is a discontinuity, this is not good enough to have a nice spring approximation to the outer shell's shape.
                // Record the diameters of all segments in the selected part
                DiameterList.Clear();
                double minDiameter = 1000000000;

                #region 1. Find center curve's discontinuity

                double lengthPara;
                centerCrv.LengthParameter(centerCrv.GetLength(), out lengthPara);
                bool discontinuity = true;
                List<double> discontinuitylist = new List<double>();
                double startingPt = 0;
                while (discontinuity)
                {
                    double t;
                    discontinuity = centerCrv.GetNextDiscontinuity(Continuity.Cinfinity_continuous, startingPt, lengthPara, out t);
                    if (double.IsNaN(t) == false)
                    {
                        discontinuitylist.Add(t);
                        startingPt = t;
                    }
                }

                Curve[] discontinueCrv = null;
                if (discontinuitylist != null && discontinuitylist.Count > 0)
                {
                    discontinueCrv = centerCrv.Split(discontinuitylist);
                }
                #endregion

                #region 2. gennerate spiral for each segment of the curve

                Point3d spiralStartPt = new Point3d(0, 0, 0);
                List<NurbsCurve> spiralCrvList = new List<NurbsCurve>();

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

                        Rhino.Geometry.Intersect.Intersection.BrepPlane(b, crvSegStartPln, myDoc.ModelAbsoluteTolerance, out interStartCrvs, out interStartPts);
                        Rhino.Geometry.Intersect.Intersection.BrepPlane(b, crvSegEndPln, myDoc.ModelAbsoluteTolerance, out interEndCrvs, out interEndPts);

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

                        //r1 = surfaceBrep.ClosestPoint(crv.PointAtStart).DistanceTo(crv.PointAtStart);
                        //r2 = surfaceBrep.ClosestPoint(crv.PointAtEnd).DistanceTo(crv.PointAtEnd);
                        DiameterList.Add(r1);
                        DiameterList.Add(r2);
                        if (r1 <= minDiameter) minDiameter = r1;
                        if (r2 <= minDiameter) minDiameter = r2;

                        //double initDia = (r1 + r2) / 2 / 10;
                        //if (initDia < 2)
                        //{
                        //    initDia = 2;
                        //}

                        //// test if pitch is greater than coil diameter + 0.8mm (test result)
                        //while (pitch <= initDia + 0.8) { pitch += 0.2; }

                        // DEBUG: currently we create the spiral that approximates to the outer geometry (1 mm distance) but still inside the body
                        // Make sure the generated spiral is inside the model body so we can cut the geometry from inside
                        if (spiralStartPt.Equals(new Point3d(0, 0, 0)))
                        {
                            spiralStartPt = startPln.ClosestPoint(spiralStartPt);
                        }

                        NurbsCurve spiralCrv = NurbsCurve.CreateSpiral(crv, 0, endPara1, spiralStartPt, pitch, 0, r1 - objRef.WireDiameter, r2 - objRef.WireDiameter, 30);
                        spiralStartPt = spiralCrv.PointAtEnd;
                        spiralCrvList.Add(spiralCrv);
                    }
                }

                #endregion

                #region 3. approximate the spiral to the geometry and sweep the spiral

                Curve joinedSpiral = Curve.JoinCurves(spiralCrvList)[0];
                Point3d[] points;

                List<Point3d> centralSpiral = new List<Point3d>();

                // DEBUG: currently the spring diameter is hard coded.
                int sampleNum = 300;
                double wireDia = objRef.WireDiameter;
                joinedSpiral.DivideByCount(sampleNum, true, out points); // 300 is the number of sample points
                Plane initialPln = new Plane();

                for (int i = 0; i < points.Count(); i++)
                {
                    Point3d samplePoint = points[i];

                    double pointPara = 0;
                    joinedSpiral.LengthParameter(joinedSpiral.GetLength() / sampleNum * i, out pointPara);
                    Plane tempPln = new Plane(samplePoint, joinedSpiral.TangentAt(pointPara));

                    // Get the closest point to  the smaple point on the medial axis
                    double para;
                    centerCrv.ClosestPoint(samplePoint, out para);
                    Point3d pt_cen = centerCrv.PointAt(para);
                    //pt_cen = tempPln.ClosestPoint(pt_cen);


                    ////Point3d pt_cen = tempPln.ClosestPoint(samplePointOuter);

                    //Point3d sec_pt = samplePoint;
                    //Vector3d v = (Vector3d)(samplePoint - pt_cen);
                    //Transform rayTran = Transform.Translation(4*v);
                    //sec_pt.Transform(rayTran);

                    //List<Point3d> ray = new List<Point3d>();
                    //ray.Add(pt_cen);
                    //ray.Add(samplePoint);
                    //ray.Add(sec_pt);

                    //Curve rayCrv = Curve.CreateInterpolatedCurve(ray, 3);
                    ////myDoc.Objects.AddCurve(rayCrv, whiteAttribute);
                    ////myDoc.Views.Redraw();

                    //Point3d[] interPts;
                    //Curve[] overlapCrvs;

                    //Point3d pt_pln = new Point3d();
                    //Rhino.Geometry.Intersect.Intersection.CurveBrep(rayCrv, b, myDoc.ModelAbsoluteTolerance, out overlapCrvs, out interPts);
                    //foreach(Point3d p in interPts)
                    //{
                    //    //myDoc.Objects.AddPoint(p, whiteAttribute);
                    //    //myDoc.Views.Redraw();

                    //    pt_pln = tempPln.ClosestPoint(p);
                    //    break;
                    //}

                    Point3d pt_pln;
                    Point3d[] projBrepPts;
                    List<Brep> bs = new List<Brep>();
                    bs.Add(b);
                    List<Point3d> projPts = new List<Point3d>();
                    projPts.Add(pt_cen);
                    Vector3d projDir = (Vector3d)(samplePoint - pt_cen);
                    projBrepPts = Rhino.Geometry.Intersect.Intersection.ProjectPointsToBreps(bs, projPts, projDir, myDoc.ModelAbsoluteTolerance);


                    foreach (Point3d projPt in projBrepPts)
                    {
                        Vector3d projVec = (Vector3d)(projPt - samplePoint);
                        if (projVec * projDir > 0)
                        {
                            pt_pln = projPt;

                            Vector3d shrinkDir = (Vector3d)(samplePoint - pt_pln);
                            shrinkDir = shrinkDir / shrinkDir.Length;
                            shrinkDir = shrinkDir * (wireDia / 2);
                            Transform shrinkTran = Transform.Translation(shrinkDir);

                            Point3d pt_center = pt_pln;
                            pt_center.Transform(shrinkTran);

                            centralSpiral.Add(pt_center);
                        }
                    }
                }

                Curve spiralTraj = Curve.CreateInterpolatedCurve(centralSpiral, 3);
                //myDoc.Objects.AddCurve(spiralTraj, whiteAttribute);
                //myDoc.Views.Redraw();

                double springStartPara = 0;
                spiralTraj.LengthParameter(0, out springStartPara);
                initialPln = new Plane(centralSpiral[0], spiralTraj.TangentAt(springStartPara));

                Curve crossCircle = new Circle(initialPln, centralSpiral[0], wireDia / 2).ToNurbsCurve();
                //myDoc.Objects.AddCurve(crossRect, whiteAttribute);
                //myDoc.Views.Redraw();

                var sweep = new Rhino.Geometry.SweepOneRail();
                sweep.AngleToleranceRadians = myDoc.ModelAngleToleranceRadians;
                sweep.ClosedSweep = false;
                sweep.SweepTolerance = myDoc.ModelAbsoluteTolerance;

                var breps = sweep.PerformSweep(spiralTraj, crossCircle);
                List<Brep> cappedSpring = new List<Brep>();
                if (breps.Length > 0)
                {
                    foreach (Brep br in breps)
                    {
                        cappedSpring.Add(br.CapPlanarHoles(myDoc.ModelRelativeTolerance));
                    }
                }

                Sphere springFrontEnd = new Sphere(centralSpiral[0], wireDia / 2);
                Sphere springRearEnd = new Sphere(centralSpiral[centralSpiral.Count() - 1], wireDia / 2);

                Brep frontSphere = Brep.CreateFromSphere(springFrontEnd);
                Brep rearSphere = Brep.CreateFromSphere(springRearEnd);
                cappedSpring.Add(frontSphere);
                cappedSpring.Add(rearSphere);

                // Update the Ondule unit's capped spring IDs list
                if (objRef.CappedSpringIDs.Count > 0)
                {
                    foreach (Guid springID in objRef.CappedSpringIDs)
                    {
                        myDoc.Objects.Delete(springID,true);
                    }
                    objRef.CappedSpringIDs.Clear();
                }

                foreach (Brep spring in cappedSpring)
                {
                    Guid s_ID = myDoc.Objects.AddBrep(spring, white_attributes);
                    objRef.CappedSpringIDs.Add(s_ID);
                }

                // DEBUG: gap length is hard coded right now, may changed by the input

                //Vector3d rectDir = (Vector3d)(pt_pln - joinedSpiral.PointAtStart);
                //Transform tr = Transform.Translation(rectDir/rectDir.Length);
                //Point3d rectPt = pt_pln;
                //rectPt.Transform(tr);

                //Vector3d axis = spiralStartPln.Normal;
                //Point3d[] rectCrossPts = new Point3d[5];

                //rectDir.Rotate(Math.PI, axis);
                //Transform tr0 = Transform.Translation(rectDir);
                //Point3d theOtherPt = joinedSpiral.PointAtStart;
                //theOtherPt.Transform(tr0);


                //rectDir = rectDir / rectDir.Length * gapLen/2;
                //rectDir.Rotate(Math.PI*3 / 2, axis);
                //Transform tr1 = Transform.Translation(rectDir);
                //rectCrossPts[0] = theOtherPt;
                //rectCrossPts[0].Transform(tr1);
                //rectCrossPts[0] = spiralStartPln.ClosestPoint(rectCrossPts[0]);
                //rectCrossPts[1] = rectPt;
                //rectCrossPts[1].Transform(tr1);
                //rectCrossPts[1] = spiralStartPln.ClosestPoint(rectCrossPts[1]);

                //rectDir.Rotate(Math.PI, axis);
                //Transform tr2 = Transform.Translation(rectDir);
                //rectCrossPts[3] = theOtherPt;
                //rectCrossPts[3].Transform(tr2);
                //rectCrossPts[3] = spiralStartPln.ClosestPoint(rectCrossPts[3]);
                //rectCrossPts[2] = rectPt;
                //rectCrossPts[2].Transform(tr2);
                //rectCrossPts[2] = spiralStartPln.ClosestPoint(rectCrossPts[2]);
                //rectCrossPts[4] = rectCrossPts[0];

                ////////////////////////////////////////////////////

                //Point3d[] rectCrossPts = new Point3d[5];
                //Transform tt1 = Transform.Translation(spiralStartPln.XAxis);
                //Transform tt2 = Transform.Translation(spiralStartPln.YAxis);
                //Transform tt3 = Transform.Translation(-spiralStartPln.XAxis);
                //Transform tt4 = Transform.Translation(-spiralStartPln.YAxis);
                //rectCrossPts[0] = joinedSpiral.PointAtStart;
                //rectCrossPts[1] = joinedSpiral.PointAtStart;
                //rectCrossPts[2] = joinedSpiral.PointAtStart;
                //rectCrossPts[3] = joinedSpiral.PointAtStart;

                //rectCrossPts[0].Transform(tt1);
                //rectCrossPts[1].Transform(tt2);
                //rectCrossPts[2].Transform(tt3);
                //rectCrossPts[3].Transform(tt4);
                //rectCrossPts[4] = rectCrossPts[0];

                //var sweep = new Rhino.Geometry.SweepOneRail();
                //sweep.AngleToleranceRadians = myDoc.ModelAngleToleranceRadians;
                //sweep.ClosedSweep = false;
                //sweep.SweepTolerance = myDoc.ModelAbsoluteTolerance;

                //Curve crossRect = new Polyline(rectCrossPts).ToNurbsCurve();
                ////Curve crossRect = new Circle(spiralStartPln, joinedSpiral.PointAtStart, 1).ToNurbsCurve();
                //myDoc.Objects.AddCurve(crossRect, whiteAttribute);
                //myDoc.Views.Redraw();

                //var breps = sweep.PerformSweep(joinedSpiral, crossRect);
                //List<Brep> cappedSpring = new List<Brep>();
                //if (breps.Length > 0)
                //{
                //    foreach (Brep br in breps)
                //    {
                //        cappedSpring.Add(br.CapPlanarHoles(myDoc.ModelRelativeTolerance));
                //    }
                //}

                //foreach(Brep spring in cappedSpring)
                //{
                //    myDoc.Objects.AddBrep(spring);
                //}

                #endregion

                #endregion

            }
            #endregion

            myDoc.Views.Redraw();
        }

        /// <summary>
        /// New algorithm for medial axis generation
        /// </summary>
        /// <param name="objRef"></param>
        public OnduleUnit maGen()
        {
            #region The generated medial axis is in yellow
            var yellow_attributes = new ObjectAttributes();
            yellow_attributes.ObjectColor = Color.Yellow;
            yellow_attributes.ColorSource = ObjectColorSource.ColorFromObject;
            #endregion

            // TO-DEBUG: Currently the user has to select one object before click the "Generate Medial Axis" button
            // Limitations: if there are sharp cornners existing on the geometry, the generated medial axis is not accurate. 
            //              In other word, we should fillet the edges of the model if possible.
            // Convert all objects in Rhino to mesh and save as stl files in the current directory

            string dir = @"C:\OnduleTest";


            string oldSTLFile = dir+@"\temp_stl.stl";
            if (File.Exists(oldSTLFile)) File.Delete(oldSTLFile);

            OnduleUnit rel = new OnduleUnit();

            ObjRef objSel_ref;
            Guid sufObjId = Guid.Empty;
            var rc = RhinoGet.GetOneObject("Select surface or polysurface to mesh", false, ObjectType.AnyObject, out objSel_ref);
            if (rc == Rhino.Commands.Result.Success){
                String str1 = "_ExportFileAs=_Binary ";
                String str2 = "_ExportUnfinishedObjects=_No ";
                String str3 = "_UseSimpleDialog=_No ";
                String str4 = "_UseSimpleParameters=_Yes ";
                //String str5 = "_Enter _DetailedOptions ";
                //String str6 = "_JaggedSeams=_No ";
                //String str7 = "_PackTextures=_No ";
                //String str8 = "_Refine=_Yes ";
                //String str9 = "_SimplePlane=_Yes ";
                //String str10 = "_Weld=_No ";
                //String str11 = "_AdvancedOptions ";
                //String str12 = "_Angle=15 ";
                //String str13 = "_AspectRatio=0 ";
                //String str14 = "_Distance=0.01 ";
                //String str15 = "_Grid=16 ";
                //String str16 = "_MaxEdgeLength=0 ";
                //String str17 = "_MinEdgeLength=0.0001 ";
                String str18 = "_Enter _Enter";
                String str = str1 + str2 + str3 + str4 + str18;

                //String str = "_Enter _Enter";
                var stlScript = string.Format("_-Export \"{0}\" {1}", oldSTLFile, str);
                Rhino.RhinoApp.RunScript(stlScript,false);

                List<Curve> cvs = new List<Curve>();
                Curve joined;

                // clean old files
                string oldFile1 = dir+@"\temp_off_skeleton.txt";
                string oldFile2 = dir+@"\temp_off.off";
                string oldFile3 = dir+@"\temp_off_convert.off";
                string oldFile4 = dir+@"\temp_off_skeleton.off";

                if (File.Exists(oldFile1)) File.Delete(oldFile1);
                if (File.Exists(oldFile2)) File.Delete(oldFile2);
                if (File.Exists(oldFile3)) File.Delete(oldFile3);
                if (File.Exists(oldFile4)) File.Delete(oldFile4);

                sufObjId = objSel_ref.ObjectId;

                ObjRef dupObjRef = new ObjRef(sufObjId);
                var brep_mesh = dupObjRef.Mesh();

                #region Using meshlab server to convert the mesh into off file
                Process meshCompiler = new Process();
                ProcessStartInfo meshStartInfo = new ProcessStartInfo();
                meshStartInfo.CreateNoWindow = true;
                meshStartInfo.UseShellExecute = false;

                meshStartInfo.FileName = @"meshlabserver\meshlabserver.exe";

                // Note: unifying duplicated vertices is necessary
                meshStartInfo.Arguments = @"-i "+dir+@"\temp_stl.stl -o "+dir+@"\temp_off.off -s "+@"meshlabserver\clean.mlx";

                meshCompiler.StartInfo = meshStartInfo;
                meshCompiler.Start();
                meshCompiler.WaitForExit();
                #endregion

                #region call the medial axis generation cmd
                Process matCompiler = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.FileName = @"skeletonization\skeletonization.exe";
                
                startInfo.Arguments = dir+@"\temp_off.off --debug 0.4";

                matCompiler.StartInfo = startInfo;
                matCompiler.Start();
                matCompiler.WaitForExit();
                //Process.Start(startInfo);


                string curFile =dir+ @"\temp_off_skeleton.txt";
                //System.Threading.Thread.Sleep(10000);

                String line;
                try
                {
                    //Pass the file path and file name to the StreamReader constructor
                    StreamReader sr = new StreamReader(curFile);

                    //Read the first line of text
                    line = sr.ReadLine();
                    maPoints.Clear();

                    do
                    {
                        // if there is only one number skip this line,
                        // otherwise store those points
                        string[] dots = line.Split('\t');
                        if (dots.Length == 1 && maPoints.Count != 0)
                        {
                            // show the dots and medial axis in Rhino
                            //foreach (Point3d p in maPoints)
                            //{
                            //    myDoc.Objects.AddPoint(p);
                            //}

                            Rhino.Geometry.Curve ma = Rhino.Geometry.Curve.CreateControlPointCurve(maPoints, 5);
                            //var attributes = new ObjectAttributes();
                            //attributes.ObjectColor = Color.White;
                            //attributes.ColorSource = ObjectColorSource.ColorFromObject;
                            cvs.Add(ma);
                            //myDoc.Objects.AddCurve(ma, attributes);
                            //myDoc.Views.Redraw();

                            maPoints.Clear();

                        }
                        else if (dots.Length == 3)
                        {
                            Point3d tempPt = new Point3d();
                            tempPt.X = Convert.ToDouble(dots[0]);
                            tempPt.Y = Convert.ToDouble(dots[1]);
                            tempPt.Z = Convert.ToDouble(dots[2]);
                            maPoints.Add(tempPt);
                        }

                        line = sr.ReadLine();
                    } while (line != null);

                    if (maPoints.Count != 0)
                    {
                        Rhino.Geometry.Curve ma = Rhino.Geometry.Curve.CreateControlPointCurve(maPoints, 5);
                        cvs.Add(ma);
                        joined = Curve.JoinCurves(cvs)[0];
                        myDoc.Objects.AddCurve(joined, yellow_attributes);
                        rel.MA = joined;

                        Point3d startPt = joined.PointAtStart;
                        Point3d endPt = joined.PointAtEnd;
                        Sphere ctrlPtStart = new Sphere(startPt, 1.5);
                        Sphere ctrlPtEnd = new Sphere(endPt, 1.5);
                        // TO-DO: should hide the control points when the deformation design interface is called
                        ctrlPt1 = myDoc.Objects.AddSphere(ctrlPtStart, yellow_attributes);
                        ctrlPt2 = myDoc.Objects.AddSphere(ctrlPtEnd, yellow_attributes);
                        rel.endPt = endPt;
                        rel.startPt = startPt;

                        myDoc.Views.Redraw();
                    }
                    //close the file
                    sr.Close();
                }
                catch (Exception e)
                {
                    RhinoApp.WriteLine(e.ToString());
                }

                #endregion

            }
            rel.BREPID = sufObjId;
            return rel;
        }


        public void addBendConstraint(OnduleUnit obj, Boolean dir)
        {
            // Hide the control points
            myDoc.Objects.Hide(ctrlPt1, true);// hide the original shell
            myDoc.Objects.Hide(ctrlPt2, true);// hide the original shell

            #region Get the outer brep surface
            ObjRef armOffsetObjRef = new ObjRef(obj.BREPID);    //get the objRef from the GUID
            Brep surfaceBrep = armOffsetObjRef.Brep();          // curently we only recognize the polysurface as a brep, TO-DO: recognize the imported mesh
            #endregion

            # region Get the start and end planes
            Curve centerCrv = obj.MA;
            Point3d startPt = centerCrv.PointAtStart;
            Point3d endPt = centerCrv.PointAtEnd;
            double startPara = 0, endPara = 0;
            centerCrv.LengthParameter(0, out startPara);
            centerCrv.LengthParameter(centerCrv.GetLength(), out endPara);
            Plane startPln = new Plane(startPt, centerCrv.TangentAt(startPara));    // the plane at the start of the curve
            Plane endPln = new Plane(endPt, centerCrv.TangentAt(endPara));          // the plane at the end of the curve
            #endregion

            #region Generate the joint chain
            generateBendSupport(startPln, endPln, centerCrv, dir, obj.BendAngle);
            #endregion
        }
        /// <summary>
        /// Add prismatic joint and bearing for both linear deformation and twisting
        /// </summary>
        /// <param name="obj"></param>
        public void addLinearTwistConstraint(OnduleUnit obj)
        {
            // Hide the control points
            myDoc.Objects.Hide(ctrlPt1, true);// hide the original shell
            myDoc.Objects.Hide(ctrlPt2, true);// hide the original shell

            double cmpressDis = obj.CompressionDis;
            double tensionDis = obj.ExtensionDis;

            #region Get the outer brep surface
            ObjRef armOffsetObjRef = new ObjRef(obj.BREPID);    //get the objRef from the GUID
            Brep surfaceBrep = armOffsetObjRef.Brep();          // curently we only recognize the polysurface as a brep, TO-DO: recognize the imported mesh
            #endregion

            # region Get the start and end planes
            Curve centerCrv = obj.MA;
            Point3d startPt = centerCrv.PointAtStart;
            Point3d endPt = centerCrv.PointAtEnd;
            double startPara = 0, endPara = 0;
            centerCrv.LengthParameter(0, out startPara);
            centerCrv.LengthParameter(centerCrv.GetLength(), out endPara);
            Plane startPln = new Plane(startPt, centerCrv.TangentAt(startPara));    // the plane at the start of the curve
            Plane endPln = new Plane(endPt, centerCrv.TangentAt(endPara));          // the plane at the end of the curve
            #endregion

            #region Generate the prismatic joint and bearing for the linear constraint
            generateLinearTwistSupport(startPln, endPln, centerCrv, cmpressDis, tensionDis);
            #endregion
        }
        /// <summary>
        /// Add the prismatic joint as the linear constraint, only the central structure, no outer spring
        /// </summary>
        /// <param name="obj"> The temporarily rendered Ondule unit </param>
        public void addLinearConstraint(OnduleUnit obj)
        {
            // Hide the control points
            
            myDoc.Objects.Hide(ctrlPt1, true);// hide the original shell
            myDoc.Objects.Hide(ctrlPt2, true);// hide the original shell

            double cmpressDis = obj.CompressionDis;
            double tensionDis = obj.ExtensionDis;

            #region Get the outer brep surface
            ObjRef armOffsetObjRef = new ObjRef(obj.BREPID);    //get the objRef from the GUID
            Brep surfaceBrep = armOffsetObjRef.Brep();          // curently we only recognize the polysurface as a brep, TO-DO: recognize the imported mesh
            #endregion

            # region Get the start and end planes
            Curve centerCrv = obj.MA;
            Point3d startPt = centerCrv.PointAtStart;
            Point3d endPt = centerCrv.PointAtEnd;
            double startPara = 0, endPara = 0;
            centerCrv.LengthParameter(0, out startPara);
            centerCrv.LengthParameter(centerCrv.GetLength(), out endPara);
            Plane startPln = new Plane(startPt, centerCrv.TangentAt(startPara));    // the plane at the start of the curve
            Plane endPln = new Plane(endPt, centerCrv.TangentAt(endPara));          // the plane at the end of the curve
            #endregion

            #region Generate the prismatic joint for the linear constraint
            generateLinearSupport(startPln, endPln, centerCrv, cmpressDis, tensionDis);
            #endregion

        }
        /// <summary>
        /// Add the bearing structure as the twist constraint, only the central structure, no outer spring
        /// </summary>
        /// <param name="obj"> The temporarily rendered Ondule unit </param>
        public void addTwistConstraint(OnduleUnit obj)
        {
            // Hide the control points
            myDoc.Objects.Hide(ctrlPt1, true);// hide the original shell
            myDoc.Objects.Hide(ctrlPt2, true);// hide the original shell

            #region Get the outer brep surface
            ObjRef armOffsetObjRef = new ObjRef(obj.BREPID);    //get the objRef from the GUID
            Brep surfaceBrep = armOffsetObjRef.Brep();          // curently we only recognize the polysurface as a brep, TO-DO: recognize the imported mesh
            #endregion

            # region Get the start and end planes
            Curve centerCrv = obj.MA;
            Point3d startPt = centerCrv.PointAtStart;
            Point3d endPt = centerCrv.PointAtEnd;
            double startPara = 0, endPara = 0;
            centerCrv.LengthParameter(0, out startPara);
            centerCrv.LengthParameter(centerCrv.GetLength(), out endPara);
            Plane startPln = new Plane(startPt, centerCrv.TangentAt(startPara));    // the plane at the start of the curve
            Plane endPln = new Plane(endPt, centerCrv.TangentAt(endPara));          // the plane at the end of the curve
            #endregion

            #region generate the central support structure
            Brep stopper, cylinder;
            generateTwistSupport(startPln, endPln, centerCrv, out stopper, out cylinder);
            #endregion

        }

        private void generateLinearSupport(Plane startPln, Plane endPln, Curve centerCrv, double compreDis, double tensionDis)
        {
            double thickness = 2;       // the thickness of the stopper and the cap
            double gap = 0.4;
            double wall = 0.8;
            double tensionDisNew = centerCrv.GetLength() - 2 * thickness - 2 * compreDis;

            // create sweep function
            var sweep = new Rhino.Geometry.SweepOneRail();
            sweep.AngleToleranceRadians = myDoc.ModelAngleToleranceRadians;
            sweep.ClosedSweep = false;
            sweep.SweepTolerance = myDoc.ModelAbsoluteTolerance;

            // Prepare the curves

            double t1, t2, t3, t4;
            centerCrv.LengthParameter(centerCrv.GetLength() - compreDis, out t1);
            Curve compCrvRear = centerCrv.Split(t1)[1];
            Curve leftover1 = centerCrv.Split(t1)[0];

            leftover1.LengthParameter(leftover1.GetLength() - thickness, out t2);
            Curve stopperCrv = leftover1.Split(t2)[1];
            Curve leftover2 = leftover1.Split(t2)[0];

            leftover2.LengthParameter(leftover2.GetLength() - tensionDisNew, out t3);
            Curve tensionCrv = leftover2.Split(t3)[1];
            Curve leftover3 = leftover2.Split(t3)[0];

            leftover3.LengthParameter(leftover3.GetLength() - thickness, out t4);
            Curve capCrv = leftover3.Split(t4)[1];
            Curve compCrvFront = leftover3.Split(t4)[0];

            #region Add color coded curves for debugging
            //var red_attributes = new ObjectAttributes();
            //red_attributes.ObjectColor = Color.Red;
            //red_attributes.ColorSource = ObjectColorSource.ColorFromObject;

            //var black_attributes = new ObjectAttributes();
            //black_attributes.ObjectColor = Color.Black;
            //black_attributes.ColorSource = ObjectColorSource.ColorFromObject;

            //var green_attributes = new ObjectAttributes();
            //green_attributes.ObjectColor = Color.Green;
            //green_attributes.ColorSource = ObjectColorSource.ColorFromObject;

            //var purple_attributes = new ObjectAttributes();
            //purple_attributes.ObjectColor = Color.Purple;
            //purple_attributes.ColorSource = ObjectColorSource.ColorFromObject;

            //var blue_attributes = new ObjectAttributes();
            //blue_attributes.ObjectColor = Color.Blue;
            //blue_attributes.ColorSource = ObjectColorSource.ColorFromObject;

            //myDoc.Objects.AddCurve(compCrvRear, red_attributes);
            //myDoc.Objects.AddSphere(new Sphere(compCrvRear.PointAtStart, 2), red_attributes);
            //myDoc.Objects.AddCurve(stopperCrv, black_attributes);
            //myDoc.Objects.AddSphere(new Sphere(stopperCrv.PointAtStart, 2), black_attributes);
            //myDoc.Objects.AddCurve(tensionCrv, blue_attributes);
            //myDoc.Objects.AddSphere(new Sphere(tensionCrv.PointAtStart, 2), blue_attributes);
            //myDoc.Objects.AddCurve(capCrv, purple_attributes);
            //myDoc.Objects.AddSphere(new Sphere(capCrv.PointAtStart, 2), purple_attributes);
            //myDoc.Objects.AddCurve(compCrvFront, green_attributes);
            //myDoc.Objects.AddSphere(new Sphere(compCrvFront.PointAtStart, 2), green_attributes);
            //myDoc.Views.Redraw();
            #endregion

            var red_attributes = new ObjectAttributes();
            red_attributes.ObjectColor = Color.Red;
            red_attributes.ColorSource = ObjectColorSource.ColorFromObject;

            #region The rod of the prismatic joint

            List<Curve> cylinCrvList = new List<Curve>();
            cylinCrvList.Add(compCrvFront);
            cylinCrvList.Add(capCrv);
            cylinCrvList.Add(tensionCrv);
            cylinCrvList.Add(stopperCrv);
            Curve pjRodCrv = Curve.JoinCurves(cylinCrvList)[0];

            Point3d centerCylin = compCrvFront.PointAtStart;
            double cylinBaseSideRadius = 1.5;   // the radius of the central bone
            Curve cylinCircle = new Circle(startPln, centerCylin, cylinBaseSideRadius).ToNurbsCurve();
            var cylinBreps = sweep.PerformSweep(pjRodCrv, cylinCircle);
            Brep cylinBrep = cylinBreps[0];
            cylinBrep = cylinBrep.CapPlanarHoles(myDoc.ModelRelativeTolerance);
            myDoc.Objects.AddBrep(cylinBrep, red_attributes);
            #endregion

            #region The stopper (disc)
            Plane stopperPln = new Plane(stopperCrv.PointAtStart, stopperCrv.TangentAtStart);
            double discRadius = 2.3;
            Circle stopperCir = new Circle(stopperPln, stopperCrv.PointAtStart, discRadius);
            var stopperBrep = sweep.PerformSweep(stopperCrv, stopperCir.ToNurbsCurve())[0];
            stopperBrep = stopperBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
            myDoc.Objects.AddBrep(stopperBrep, red_attributes);

            Point3d[] sliderStopperPts = new Point3d[5];
            double sliderW = 2;
            double sliderH = discRadius + gap + 2 * wall;
            Transform txp_rect = Transform.Translation(stopperPln.XAxis * (sliderW/2));
            Transform typ_rect = Transform.Translation(stopperPln.YAxis * sliderH);
            Transform txn_rect = Transform.Translation(stopperPln.XAxis * -(sliderW / 2));
            Transform tyn_rect = Transform.Translation(stopperPln.YAxis * 0);

            sliderStopperPts[0] = stopperCrv.PointAtStart;
            sliderStopperPts[1] = stopperCrv.PointAtStart;
            sliderStopperPts[2] = stopperCrv.PointAtStart;
            sliderStopperPts[3] = stopperCrv.PointAtStart;
            sliderStopperPts[4] = stopperCrv.PointAtStart;

            sliderStopperPts[0].Transform(txp_rect); sliderStopperPts[0].Transform(typ_rect);
            sliderStopperPts[1].Transform(txn_rect); sliderStopperPts[1].Transform(typ_rect);
            sliderStopperPts[2].Transform(txn_rect); sliderStopperPts[2].Transform(tyn_rect);
            sliderStopperPts[3].Transform(txp_rect); sliderStopperPts[3].Transform(tyn_rect);
            sliderStopperPts[4] = sliderStopperPts[0];

            Curve sliderRect = new Polyline(sliderStopperPts).ToNurbsCurve();
            var sliderBrep = sweep.PerformSweep(stopperCrv, sliderRect)[0];
            sliderBrep = sliderBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
            //myDoc.Objects.AddBrep(sliderBrep, red_attributes);

            //myDoc.Views.Redraw();
            #endregion

            #region The cylindral guider
            List<Curve> wallCrvList = new List<Curve>();
            wallCrvList.Add(capCrv);
            wallCrvList.Add(tensionCrv);
            wallCrvList.Add(stopperCrv);
            wallCrvList.Add(compCrvRear);
            Curve wallCrv = Curve.JoinCurves(wallCrvList)[0];

            Point3d wallCircleCenter = capCrv.PointAtStart;
            double innerRadius = discRadius + gap;   // the radius of the central bone
            double middleRadius = innerRadius + wall;
            double outerRadius = middleRadius + 2 * wall;
            Plane wallPln = new Plane(capCrv.PointAtStart, capCrv.TangentAtStart);

            // Align wallPln and stopperPln so the slider block is the opening slot
            //Vector3d wallPlnX = wallPln.XAxis;
            //Vector3d stopperPlnX = stopperPln.XAxis;
            //Double angleBetween;
            //angleBetween = Vector3d.VectorAngle(wallPlnX, stopperPlnX);
            //Transform rotation = Transform.Rotation(angleBetween, capCrv.PointAtStart);
            //wallPln.Transform(rotation);

            Curve innerCircle = new Circle(wallPln, wallCircleCenter, innerRadius).ToNurbsCurve();
            Curve middleCircle = new Circle(wallPln, wallCircleCenter, middleRadius).ToNurbsCurve();
            Curve outerCircle = new Circle(wallPln, wallCircleCenter, outerRadius).ToNurbsCurve();
            var innerBreps = sweep.PerformSweep(wallCrv, innerCircle);
            Brep innerBrep = innerBreps[0];
            innerBrep = innerBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
            System.Threading.Thread.Sleep(25);
            var middleBreps = sweep.PerformSweep(wallCrv, middleCircle);
            Brep middleBrep = middleBreps[0];
            middleBrep = middleBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
            System.Threading.Thread.Sleep(25);
            var outerBreps = sweep.PerformSweep(wallCrv, outerCircle);
            Brep outerBrep = outerBreps[0];
            outerBrep = outerBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
            System.Threading.Thread.Sleep(25);
            var middleBrepsDup = sweep.PerformSweep(wallCrv, middleCircle);
            Brep middleBrepDup = middleBrepsDup[0];
            middleBrepDup = middleBrepDup.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
            System.Threading.Thread.Sleep(25);

            var sliderWalls = Brep.CreateBooleanDifference(middleBrep,innerBrep, myDoc.ModelAbsoluteTolerance);
            Brep sliderWall = sliderWalls[0];
            var outerWalls = Brep.CreateBooleanDifference(outerBrep, middleBrepDup, myDoc.ModelAbsoluteTolerance);
            Brep outerWall = outerWalls[0];
            var sliderBlocks = Brep.CreateBooleanDifference(sliderBrep, outerWall, myDoc.ModelAbsoluteTolerance);
            Brep sliderBlock = sliderBlocks[0];

            myDoc.Objects.AddBrep(sliderBlock, red_attributes);


            //Point3d[] sliderWallPts = new Point3d[5];
            //double slotW = sliderW + gap * 2;
            //double slotH = sliderH;
            //Transform txp_rect1 = Transform.Translation(wallPln.XAxis * (slotW / 2));
            //Transform typ_rect1 = Transform.Translation(wallPln.YAxis * slotH);
            //Transform txn_rect1 = Transform.Translation(wallPln.XAxis * -(slotW / 2));
            //Transform tyn_rect1 = Transform.Translation(wallPln.YAxis * 0);

            //sliderWallPts[0] = capCrv.PointAtStart;
            //sliderWallPts[1] = capCrv.PointAtStart;
            //sliderWallPts[2] = capCrv.PointAtStart;
            //sliderWallPts[3] = capCrv.PointAtStart;
            //sliderWallPts[4] = capCrv.PointAtStart;

            //sliderWallPts[0].Transform(txp_rect1); sliderWallPts[0].Transform(typ_rect1);
            //sliderWallPts[1].Transform(txn_rect1); sliderWallPts[1].Transform(typ_rect1);
            //sliderWallPts[2].Transform(txn_rect1); sliderWallPts[2].Transform(tyn_rect1);
            //sliderWallPts[3].Transform(txp_rect1); sliderWallPts[3].Transform(tyn_rect1);
            //sliderWallPts[4] = sliderWallPts[0];

            //Curve slotRect = new Polyline(sliderWallPts).ToNurbsCurve();
            //var slotBrep = sweep.PerformSweep(wallCrv, slotRect)[0];
            //slotBrep = slotBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
            //var linearWalls = Brep.CreateBooleanDifference(sliderWall, slotBrep, myDoc.ModelAbsoluteTolerance);
            //Brep linearWall = linearWalls[0];

            //myDoc.Objects.AddBrep(linearWall, red_attributes);

            List<Curve> crvList1 = new List<Curve>();
            crvList1.Add(stopperCrv);
            crvList1.Add(compCrvRear);
            Curve crv1 = Curve.JoinCurves(crvList1)[0];

            Point3d[] sliderWallPtsSet1 = new Point3d[5];
            Point3d[] sliderWallPtsSet2 = new Point3d[5];
            double slotW = sliderW + gap * 2;
            double slotH = sliderH;
            Transform txp_rect1 = Transform.Translation(stopperPln.XAxis * (slotW / 2));
            Transform typ_rect1 = Transform.Translation(stopperPln.YAxis * slotH);
            Transform txn_rect1 = Transform.Translation(stopperPln.XAxis * -(slotW / 2));
            Transform tyn_rect1 = Transform.Translation(stopperPln.YAxis * -slotH);

            Transform txp_rect2 = Transform.Translation(stopperPln.XAxis * slotH);
            Transform typ_rect2 = Transform.Translation(stopperPln.YAxis * (slotW / 2));
            Transform txn_rect2 = Transform.Translation(stopperPln.XAxis * -slotH);
            Transform tyn_rect2 = Transform.Translation(stopperPln.YAxis * -(slotW / 2));

            sliderWallPtsSet1[0] = stopperCrv.PointAtStart;
            sliderWallPtsSet1[1] = stopperCrv.PointAtStart;
            sliderWallPtsSet1[2] = stopperCrv.PointAtStart;
            sliderWallPtsSet1[3] = stopperCrv.PointAtStart;
            sliderWallPtsSet1[4] = stopperCrv.PointAtStart;
            sliderWallPtsSet2[0] = stopperCrv.PointAtStart;
            sliderWallPtsSet2[1] = stopperCrv.PointAtStart;
            sliderWallPtsSet2[2] = stopperCrv.PointAtStart;
            sliderWallPtsSet2[3] = stopperCrv.PointAtStart;
            sliderWallPtsSet2[4] = stopperCrv.PointAtStart;


            sliderWallPtsSet1[0].Transform(txp_rect1); sliderWallPtsSet1[0].Transform(typ_rect1);
            sliderWallPtsSet1[1].Transform(txn_rect1); sliderWallPtsSet1[1].Transform(typ_rect1);
            sliderWallPtsSet1[2].Transform(txn_rect1); sliderWallPtsSet1[2].Transform(tyn_rect1);
            sliderWallPtsSet1[3].Transform(txp_rect1); sliderWallPtsSet1[3].Transform(tyn_rect1);
            sliderWallPtsSet1[4] = sliderWallPtsSet1[0];

            sliderWallPtsSet2[0].Transform(txp_rect2); sliderWallPtsSet2[0].Transform(typ_rect2);
            sliderWallPtsSet2[1].Transform(txn_rect2); sliderWallPtsSet2[1].Transform(typ_rect2);
            sliderWallPtsSet2[2].Transform(txn_rect2); sliderWallPtsSet2[2].Transform(tyn_rect2);
            sliderWallPtsSet2[3].Transform(txp_rect2); sliderWallPtsSet2[3].Transform(tyn_rect2);
            sliderWallPtsSet2[4] = sliderWallPtsSet2[0];

            Curve slotRect1 = new Polyline(sliderWallPtsSet1).ToNurbsCurve();
            Curve slotRect2 = new Polyline(sliderWallPtsSet2).ToNurbsCurve();

            var slotBrepSet1Part1 = sweep.PerformSweep(crv1, slotRect1)[0];
            slotBrepSet1Part1 = slotBrepSet1Part1.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
            var slotBrepSet1Part2 = sweep.PerformSweep(tensionCrv, slotRect1)[0];
            slotBrepSet1Part2 = slotBrepSet1Part2.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);

            var slotBrepSet2Part1 = sweep.PerformSweep(crv1, slotRect2)[0];
            slotBrepSet2Part1 = slotBrepSet2Part1.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
            var slotBrepSet2Part2 = sweep.PerformSweep(tensionCrv, slotRect2)[0];
            slotBrepSet2Part2 = slotBrepSet2Part2.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);

            List<Brep> slotBreps1 = new List<Brep>();
            slotBreps1.Add(slotBrepSet2Part1);
            List<Brep> slotBreps2 = new List<Brep>();
            slotBreps2.Add(slotBrepSet2Part2);

            var linearWalls = Brep.CreateBooleanDifference(sliderWall, slotBrepSet1Part1, myDoc.ModelAbsoluteTolerance);
            var lo1 = linearWalls[0];
            var lo2 = Brep.CreateBooleanDifference(lo1, slotBrepSet2Part1, myDoc.ModelAbsoluteTolerance)[0];

            var lo3 = Brep.CreateBooleanIntersection(lo2, slotBrepSet1Part2, myDoc.ModelAbsoluteTolerance)[0];
            var lo4s = Brep.CreateBooleanIntersection(lo3, slotBrepSet2Part2, myDoc.ModelAbsoluteTolerance);

            myDoc.Objects.AddBrep(lo4s[0], red_attributes);
            myDoc.Views.Redraw();
            

            #endregion

            #region The cap
            double holeRadius = cylinBaseSideRadius + gap / 2;
            Curve holeCircle = new Circle(wallPln, wallCircleCenter, holeRadius).ToNurbsCurve();

            var caps = sweep.PerformSweep(capCrv, middleCircle);
            Brep cap = caps[0];
            cap = cap.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
            var holeBreps = sweep.PerformSweep(capCrv, holeCircle);
            Brep holeBrep = holeBreps[0];
            holeBrep = holeBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);

            var capBreps = Brep.CreateBooleanDifference(cap, holeBrep, myDoc.ModelAbsoluteTolerance);
            Brep capBrep = capBreps[0];
            myDoc.Objects.AddBrep(capBrep, red_attributes);
            #endregion

            myDoc.Views.Redraw();
        }
        private void generateTwistSupport(Plane startSuf, Plane endSuf, Curve centerCrv, out Brep StopperBlock, out Brep cylindShaft)
        {
            // create sweep function
            var sweep = new Rhino.Geometry.SweepOneRail();
            sweep.AngleToleranceRadians = myDoc.ModelAngleToleranceRadians;
            sweep.ClosedSweep = false;
            sweep.SweepTolerance = myDoc.ModelAbsoluteTolerance;

            // compute the base height and generate the guide curves
            double t;
            double gap_bearing_stopper = 0.4;
            double stopperheight = 3;
            centerCrv.LengthParameter(centerCrv.GetLength() - stopperheight, out t);  // the height is currently 3. It should be confined with the limit from the test
            Curve guiderCrv = centerCrv.Split(t)[1];            // indicates the length of the bearing wall
            Curve cylinCrv = centerCrv.Split(t)[0];             // indicates the length of the guide
            guiderCrv.LengthParameter(gap_bearing_stopper, out t);
            Curve cylinGap = guiderCrv.Split(t)[0];             // indicates the gap between the stopper and the bearing block
            Curve guiderCrvLeftover = guiderCrv.Split(t)[1];    // indicates the length of the bearing block + the gap between the block and the bottom

            #region The generated structure is in red
            var red_attributes = new ObjectAttributes();
            red_attributes.ObjectColor = Color.Red;
            red_attributes.ColorSource = ObjectColorSource.ColorFromObject;
            #endregion

            #region For debug
            //var attributes = new ObjectAttributes();
            //attributes.ObjectColor = Color.Yellow;
            //attributes.ColorSource = ObjectColorSource.ColorFromObject;
            //myDoc.Objects.AddCurve(cylinGap, attributes);

            //var attributes1 = new ObjectAttributes();
            //attributes1.ObjectColor = Color.Blue;
            //attributes1.ColorSource = ObjectColorSource.ColorFromObject;
            //myDoc.Objects.AddCurve(guiderCrvLeftover, attributes1);

            //myDoc.Views.Redraw();
            #endregion

            #region cylindral structure that enables rotation
            List<Curve> cylinCrvList = new List<Curve>();
            cylinCrvList.Add(cylinCrv);
            cylinCrvList.Add(cylinGap);
            Curve cylinCrvAll = Curve.JoinCurves(cylinCrvList)[0];   // the central bone part

            Point3d centerCylin = centerCrv.PointAtStart;
            double cylinBaseSideRadius = 1.5;   // the radius of the central bone
            Curve cylinCircle = new Circle(startSuf, centerCylin, cylinBaseSideRadius).ToNurbsCurve();
            var cylinBreps = sweep.PerformSweep(cylinCrvAll, cylinCircle);
            Brep cylinBrep = cylinBreps[0];
            cylinBrep = cylinBrep.CapPlanarHoles(myDoc.ModelRelativeTolerance);
            myDoc.Objects.AddBrep(cylinBrep, red_attributes);

            cylindShaft = cylinBrep;



            //Rhino.Geometry.Sphere startSphere = new Rhino.Geometry.Sphere(cylinCrvAll.PointAtEnd, 1.5);
            //myDoc.Objects.AddSphere(startSphere, attributes);

            myDoc.Views.Redraw();

            #endregion

            #region stopper (disc)
            Plane stopperPln = new Plane(cylinCrvAll.PointAtEnd, cylinCrvAll.TangentAtEnd);
            double discRadius = 2.3;
            Circle stopperCir = new Circle(stopperPln, cylinCrvAll.PointAtEnd, discRadius);
            double tt;
            guiderCrvLeftover.LengthParameter(guiderCrvLeftover.GetLength() - (stopperheight - 2 * gap_bearing_stopper), out tt);
            Curve stopperCrv = guiderCrvLeftover.Split(tt)[1];
            var stopperBrep = sweep.PerformSweep(stopperCrv, stopperCir.ToNurbsCurve())[0];
            stopperBrep = stopperBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);

            //myDoc.Objects.AddCurve(stopperCrv);
            myDoc.Objects.AddBrep(stopperBrep, red_attributes);
            StopperBlock = stopperBrep;
            myDoc.Views.Redraw();
            #endregion

            #region create the bearing wall
            Plane bearingPln = new Plane(guiderCrv.PointAtEnd, guiderCrv.TangentAtEnd);
            double wallInnerRadius = 2.7;   // TODO: update this value based on the test
            double wallOuterRadius = 3.5;   // TODO: update this value based on the test, currently the wall thickness is 0.8mm
            Circle bearingInnerCir = new Circle(bearingPln, guiderCrv.PointAtEnd, wallInnerRadius);
            Circle bearingOuterCir = new Circle(bearingPln, guiderCrv.PointAtEnd, wallOuterRadius);
            var bearingOuterWallBrep = sweep.PerformSweep(guiderCrv, bearingOuterCir.ToNurbsCurve())[0];
            bearingOuterWallBrep = bearingOuterWallBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
            System.Threading.Thread.Sleep(25);
            var bearingInnerWallBrep = sweep.PerformSweep(guiderCrv, bearingInnerCir.ToNurbsCurve())[0];
            bearingInnerWallBrep = bearingInnerWallBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
            System.Threading.Thread.Sleep(25);
            var bearingWall = Brep.CreateBooleanDifference(bearingInnerWallBrep, bearingOuterWallBrep, myDoc.ModelAbsoluteTolerance);
            //myDoc.Objects.AddBrep(bearingWall[0], red_attributes);
            //myDoc.Objects.AddBrep(bearingWall[1]);
            //myDoc.Views.Redraw();

            #endregion

            #region create cut-outs for support removal
            Point3d[] cutout1CornerPt = new Point3d[5];
            Point3d[] cutout2CornerPt = new Point3d[5];
            Transform txp_rect1 = Transform.Translation(startSuf.XAxis * (wallOuterRadius + 0.5));
            Transform typ_rect1 = Transform.Translation(startSuf.YAxis * (wallInnerRadius - 1.2));
            Transform txn_rect1 = Transform.Translation(startSuf.XAxis * -(wallOuterRadius + 0.5));
            Transform tyn_rect1 = Transform.Translation(startSuf.YAxis * -(wallInnerRadius - 1.2));

            Transform txp_rect2 = Transform.Translation(startSuf.XAxis * (wallInnerRadius - 1.2));
            Transform typ_rect2 = Transform.Translation(startSuf.YAxis * (wallOuterRadius + 0.5));
            Transform txn_rect2 = Transform.Translation(startSuf.XAxis * -(wallInnerRadius - 1.2));
            Transform tyn_rect2 = Transform.Translation(startSuf.YAxis * -(wallOuterRadius + 0.5));

            //cutout1CornerPt[0] = guiderCrv.PointAtEnd;
            cutout1CornerPt[0] = centerCrv.PointAtStart;
            cutout1CornerPt[1] = centerCrv.PointAtStart;
            cutout1CornerPt[2] = centerCrv.PointAtStart;
            cutout1CornerPt[3] = centerCrv.PointAtStart;
            cutout1CornerPt[0].Transform(txp_rect1); cutout1CornerPt[0].Transform(typ_rect1);
            cutout1CornerPt[1].Transform(txn_rect1); cutout1CornerPt[1].Transform(typ_rect1);
            cutout1CornerPt[2].Transform(txn_rect1); cutout1CornerPt[2].Transform(tyn_rect1);
            cutout1CornerPt[3].Transform(txp_rect1); cutout1CornerPt[3].Transform(tyn_rect1);
            cutout1CornerPt[4] = cutout1CornerPt[0];

            cutout2CornerPt[0] = centerCrv.PointAtStart;
            cutout2CornerPt[1] = centerCrv.PointAtStart;
            cutout2CornerPt[2] = centerCrv.PointAtStart;
            cutout2CornerPt[3] = centerCrv.PointAtStart;
            cutout2CornerPt[0].Transform(txp_rect2); cutout2CornerPt[0].Transform(typ_rect2);
            cutout2CornerPt[1].Transform(txn_rect2); cutout2CornerPt[1].Transform(typ_rect2);
            cutout2CornerPt[2].Transform(txn_rect2); cutout2CornerPt[2].Transform(tyn_rect2);
            cutout2CornerPt[3].Transform(txp_rect2); cutout2CornerPt[3].Transform(tyn_rect2);
            cutout2CornerPt[4] = cutout2CornerPt[0];

            Curve baseRectCrv1 = new Polyline(cutout1CornerPt).ToNurbsCurve();
            Curve baseRectCrv2 = new Polyline(cutout2CornerPt).ToNurbsCurve();

            var blue_attr = new ObjectAttributes();
            blue_attr.ObjectColor = Color.Blue;
            blue_attr.ColorSource = ObjectColorSource.ColorFromObject;

            var cutout1Brep = sweep.PerformSweep(centerCrv, baseRectCrv1)[0];
            cutout1Brep = cutout1Brep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
            System.Threading.Thread.Sleep(25);
            var cutout2Brep = sweep.PerformSweep(centerCrv, baseRectCrv2)[0];
            cutout2Brep = cutout2Brep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
            System.Threading.Thread.Sleep(25);

            var bearingwallBrep = bearingWall[0];
            List<Brep> cutoutBreps = new List<Brep>();
            cutoutBreps.Add(cutout2Brep);

            // Cut the cylinder into four pieces
            var leftover1 = Brep.CreateBooleanDifference(bearingwallBrep, cutout1Brep, myDoc.ModelRelativeTolerance);
            var leftover2 = Brep.CreateBooleanDifference(leftover1, cutoutBreps, myDoc.ModelRelativeTolerance);
            for (int brepIdx = 0; brepIdx < leftover2.Length; brepIdx++)
            {
                myDoc.Objects.AddBrep(leftover2[brepIdx], red_attributes);
            }
            #endregion

            #region create the cap for the stopper
            double capThickness = 2;
            double cap_t;
            cylinCrv.LengthParameter(cylinCrv.GetLength() - capThickness, out cap_t);  // the height is currently 3. It should be confined with the limit from the test
            Curve capCrv = cylinCrv.Split(cap_t)[1];             // indicates the thickness of the cap

            Plane capPln = new Plane(capCrv.PointAtEnd, capCrv.TangentAtEnd);
            Circle capCir = new Circle(capPln, capCrv.PointAtEnd, wallOuterRadius);
            var capBrep = sweep.PerformSweep(capCrv, capCir.ToNurbsCurve())[0];
            capBrep = capBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
            System.Threading.Thread.Sleep(25);
            myDoc.Objects.AddBrep(capBrep, red_attributes);
            #endregion

            myDoc.Views.Redraw();
        }
        private void generateLinearTwistSupport(Plane startPln, Plane endPln, Curve centerCrv, double compreDis, double tensionDis)
        {
            double thickness = 2;       // the thickness of the stopper and the cap
            double gap = 0.4;
            double wall = 0.8;
            double tensionDisNew = centerCrv.GetLength() - 2 * thickness - 2 * compreDis;

            // create sweep function
            var sweep = new Rhino.Geometry.SweepOneRail();
            sweep.AngleToleranceRadians = myDoc.ModelAngleToleranceRadians;
            sweep.ClosedSweep = false;
            sweep.SweepTolerance = myDoc.ModelAbsoluteTolerance;

            // Prepare the curves

            double t1, t2, t3, t4;
            centerCrv.LengthParameter(centerCrv.GetLength() - compreDis, out t1);
            Curve compCrvRear = centerCrv.Split(t1)[1];
            Curve leftover1 = centerCrv.Split(t1)[0];

            leftover1.LengthParameter(leftover1.GetLength() - thickness, out t2);
            Curve stopperCrv = leftover1.Split(t2)[1];
            Curve leftover2 = leftover1.Split(t2)[0];

            leftover2.LengthParameter(leftover2.GetLength() - tensionDisNew, out t3);
            Curve tensionCrv = leftover2.Split(t3)[1];
            Curve leftover3 = leftover2.Split(t3)[0];

            leftover3.LengthParameter(leftover3.GetLength() - thickness, out t4);
            Curve capCrv = leftover3.Split(t4)[1];
            Curve compCrvFront = leftover3.Split(t4)[0];

            #region Add color coded curves for debugging
            //var red_attributes = new ObjectAttributes();
            //red_attributes.ObjectColor = Color.Red;
            //red_attributes.ColorSource = ObjectColorSource.ColorFromObject;

            //var black_attributes = new ObjectAttributes();
            //black_attributes.ObjectColor = Color.Black;
            //black_attributes.ColorSource = ObjectColorSource.ColorFromObject;

            //var green_attributes = new ObjectAttributes();
            //green_attributes.ObjectColor = Color.Green;
            //green_attributes.ColorSource = ObjectColorSource.ColorFromObject;

            //var purple_attributes = new ObjectAttributes();
            //purple_attributes.ObjectColor = Color.Purple;
            //purple_attributes.ColorSource = ObjectColorSource.ColorFromObject;

            //var blue_attributes = new ObjectAttributes();
            //blue_attributes.ObjectColor = Color.Blue;
            //blue_attributes.ColorSource = ObjectColorSource.ColorFromObject;

            //myDoc.Objects.AddCurve(compCrvRear, red_attributes);
            //myDoc.Objects.AddSphere(new Sphere(compCrvRear.PointAtStart, 2), red_attributes);
            //myDoc.Objects.AddCurve(stopperCrv, black_attributes);
            //myDoc.Objects.AddSphere(new Sphere(stopperCrv.PointAtStart, 2), black_attributes);
            //myDoc.Objects.AddCurve(tensionCrv, blue_attributes);
            //myDoc.Objects.AddSphere(new Sphere(tensionCrv.PointAtStart, 2), blue_attributes);
            //myDoc.Objects.AddCurve(capCrv, purple_attributes);
            //myDoc.Objects.AddSphere(new Sphere(capCrv.PointAtStart, 2), purple_attributes);
            //myDoc.Objects.AddCurve(compCrvFront, green_attributes);
            //myDoc.Objects.AddSphere(new Sphere(compCrvFront.PointAtStart, 2), green_attributes);
            //myDoc.Views.Redraw();
            #endregion

            var red_attributes = new ObjectAttributes();
            red_attributes.ObjectColor = Color.Red;
            red_attributes.ColorSource = ObjectColorSource.ColorFromObject;

            #region The rod of the prismatic joint

            List<Curve> cylinCrvList = new List<Curve>();
            cylinCrvList.Add(compCrvFront);
            cylinCrvList.Add(capCrv);
            cylinCrvList.Add(tensionCrv);
            cylinCrvList.Add(stopperCrv);
            Curve pjRodCrv = Curve.JoinCurves(cylinCrvList)[0];

            Point3d centerCylin = compCrvFront.PointAtStart;
            double cylinBaseSideRadius = 1.5;   // the radius of the central bone
            Curve cylinCircle = new Circle(startPln, centerCylin, cylinBaseSideRadius).ToNurbsCurve();
            var cylinBreps = sweep.PerformSweep(pjRodCrv, cylinCircle);
            Brep cylinBrep = cylinBreps[0];
            cylinBrep = cylinBrep.CapPlanarHoles(myDoc.ModelRelativeTolerance);
            myDoc.Objects.AddBrep(cylinBrep, red_attributes);
            #endregion

            #region The stopper (disc)
            Plane stopperPln = new Plane(stopperCrv.PointAtStart, stopperCrv.TangentAtStart);
            double discRadius = 2.3;
            Circle stopperCir = new Circle(stopperPln, stopperCrv.PointAtStart, discRadius);
            var stopperBrep = sweep.PerformSweep(stopperCrv, stopperCir.ToNurbsCurve())[0];
            stopperBrep = stopperBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
            myDoc.Objects.AddBrep(stopperBrep, red_attributes);
            #endregion

            #region The cylindral guider
            List<Curve> wallCrvList = new List<Curve>();
            wallCrvList.Add(capCrv);
            wallCrvList.Add(tensionCrv);
            wallCrvList.Add(stopperCrv);
            wallCrvList.Add(compCrvRear);
            Curve wallCrv = Curve.JoinCurves(wallCrvList)[0];

            Point3d wallCircleCenter = capCrv.PointAtStart;
            double innerRadius = discRadius + gap;   // the radius of the central bone
            double middleRadius = innerRadius + wall;
            Plane wallPln = new Plane(capCrv.PointAtStart, capCrv.TangentAtStart);

            Curve innerCircle = new Circle(wallPln, wallCircleCenter, innerRadius).ToNurbsCurve();
            Curve middleCircle = new Circle(wallPln, wallCircleCenter, middleRadius).ToNurbsCurve();
 
            var innerBreps = sweep.PerformSweep(wallCrv, innerCircle);
            Brep innerBrep = innerBreps[0];
            innerBrep = innerBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
            System.Threading.Thread.Sleep(25);
            var middleBreps = sweep.PerformSweep(wallCrv, middleCircle);
            Brep middleBrep = middleBreps[0];
            middleBrep = middleBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
            System.Threading.Thread.Sleep(25);

            var sliderWalls = Brep.CreateBooleanDifference(middleBrep, innerBrep, myDoc.ModelAbsoluteTolerance);
            Brep sliderWall = sliderWalls[0];


            Point3d[] sliderWallPts = new Point3d[5];
            Point3d[] sliderWallPts1 = new Point3d[5];

            double slotW = 2;
            double slotH = discRadius + gap + 2 * wall;
            Transform txp_rect1 = Transform.Translation(wallPln.XAxis * slotH );
            Transform typ_rect1 = Transform.Translation(wallPln.YAxis * (slotW / 2));
            Transform txn_rect1 = Transform.Translation(wallPln.XAxis * -slotH);
            Transform tyn_rect1 = Transform.Translation(wallPln.YAxis * -(slotW / 2));

            Transform txp_rect = Transform.Translation(wallPln.XAxis * (slotW / 2));
            Transform typ_rect = Transform.Translation(wallPln.YAxis * slotH);
            Transform txn_rect = Transform.Translation(wallPln.XAxis * -(slotW / 2));
            Transform tyn_rect = Transform.Translation(wallPln.YAxis * -slotH);

            sliderWallPts[0] = capCrv.PointAtStart;
            sliderWallPts[1] = capCrv.PointAtStart;
            sliderWallPts[2] = capCrv.PointAtStart;
            sliderWallPts[3] = capCrv.PointAtStart;
            sliderWallPts[4] = capCrv.PointAtStart;
            sliderWallPts1[0] = capCrv.PointAtStart;
            sliderWallPts1[1] = capCrv.PointAtStart;
            sliderWallPts1[2] = capCrv.PointAtStart;
            sliderWallPts1[3] = capCrv.PointAtStart;
            sliderWallPts1[4] = capCrv.PointAtStart;

            sliderWallPts1[0].Transform(txp_rect1); sliderWallPts1[0].Transform(typ_rect1);
            sliderWallPts1[1].Transform(txn_rect1); sliderWallPts1[1].Transform(typ_rect1);
            sliderWallPts1[2].Transform(txn_rect1); sliderWallPts1[2].Transform(tyn_rect1);
            sliderWallPts1[3].Transform(txp_rect1); sliderWallPts1[3].Transform(tyn_rect1);
            sliderWallPts1[4] = sliderWallPts1[0];

            sliderWallPts[0].Transform(txp_rect); sliderWallPts[0].Transform(typ_rect);
            sliderWallPts[1].Transform(txn_rect); sliderWallPts[1].Transform(typ_rect);
            sliderWallPts[2].Transform(txn_rect); sliderWallPts[2].Transform(tyn_rect);
            sliderWallPts[3].Transform(txp_rect); sliderWallPts[3].Transform(tyn_rect);
            sliderWallPts[4] = sliderWallPts[0];

            Curve slotRect = new Polyline(sliderWallPts).ToNurbsCurve();
            Curve slotRect1 = new Polyline(sliderWallPts1).ToNurbsCurve();
            var slotBrep = sweep.PerformSweep(wallCrv, slotRect)[0];
            slotBrep = slotBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
            var slotBrep1 = sweep.PerformSweep(wallCrv, slotRect1)[0];
            slotBrep1 = slotBrep1.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
   
            List<Brep> cutoutBreps = new List<Brep>();
            cutoutBreps.Add(slotBrep1);

            // Cut the cylinder into four pieces
            var wallLeftover1 = Brep.CreateBooleanDifference(sliderWall, slotBrep, myDoc.ModelRelativeTolerance);
            var wallLeftover2 = Brep.CreateBooleanDifference(wallLeftover1, cutoutBreps, myDoc.ModelRelativeTolerance);
            for (int brepIdx = 0; brepIdx < wallLeftover2.Length; brepIdx++)
            {
                myDoc.Objects.AddBrep(wallLeftover2[brepIdx], red_attributes);
            }
            #endregion

            #region The cap
            double holeRadius = cylinBaseSideRadius + gap / 2;
            Curve holeCircle = new Circle(wallPln, wallCircleCenter, holeRadius).ToNurbsCurve();

            var caps = sweep.PerformSweep(capCrv, middleCircle);
            Brep cap = caps[0];
            cap = cap.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
            var holeBreps = sweep.PerformSweep(capCrv, holeCircle);
            Brep holeBrep = holeBreps[0];
            holeBrep = holeBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);

            var capBreps = Brep.CreateBooleanDifference(cap, holeBrep, myDoc.ModelAbsoluteTolerance);
            Brep capBrep = capBreps[0];
            myDoc.Objects.AddBrep(capBrep, red_attributes);
            #endregion

            myDoc.Views.Redraw();
        }
        private void generateBendSupport(Plane startPln, Plane endPln, Curve centerCrv, Boolean dir, int angle)
        {
            double rodRadius = 1;       // the rod radius
            double jointRadius = 2;     // the joint radius
            double chainUnitLen = 8;    // the length of the chain unit
            double chainLen = centerCrv.GetLength();
            double paddingLen = (chainLen - Math.Floor(chainLen / chainUnitLen) * chainUnitLen) / 2;     // padding for the first chain and the last chain units 
            double gap = 0.4;
            double holderThickness = 0.8;

            #region create sweep function
            var sweep = new Rhino.Geometry.SweepOneRail();
            sweep.AngleToleranceRadians = myDoc.ModelAngleToleranceRadians;
            sweep.ClosedSweep = false;
            sweep.SweepTolerance = myDoc.ModelAbsoluteTolerance;
            #endregion

            #region create the color
            var red_attributes = new ObjectAttributes();
            red_attributes.ObjectColor = Color.Red;
            red_attributes.ColorSource = ObjectColorSource.ColorFromObject;

            var black_attributes = new ObjectAttributes();
            black_attributes.ObjectColor = Color.Black;
            black_attributes.ColorSource = ObjectColorSource.ColorFromObject;
            #endregion

            int segmentNum = (int)(Math.Floor(chainLen / chainUnitLen));

            List<Curve> segments = new List<Curve>();
            Curve trajectory = centerCrv;

            for (int idx = 0; idx < segmentNum - 1; idx++)
            {
                double t;
                double l;
                if (idx == 0)
                {
                    // the first and the last segment, use the padded length for the chain length
                    l = chainUnitLen + paddingLen;
                }
                else
                {
                    l = chainUnitLen;
                }

                trajectory.LengthParameter(trajectory.GetLength() - l, out t);

                Curve chainTra = trajectory.Split(t)[1];
                trajectory = trajectory.Split(t)[0];
                segments.Add(chainTra);
            }
            segments.Add(trajectory);

            if (dir)
            {
                #region Allows all direction bending, using ball joints

                for(int idx=0; idx<segments.Count(); idx++)
                {
                    #region For debug: show all first points on all curves
                    //Sphere sphere = new Sphere(j.PointAtStart, 1.5);
                    //myDoc.Objects.AddSphere(sphere, red_attributes);
                    //myDoc.Views.Redraw();
                    #endregion

                    Curve chainCrv = segments.ElementAt(idx);

                    if(idx == 0)
                    {
                        // The first chain unit, only generate the holder
                        Sphere innerBall = new Sphere(chainCrv.PointAtStart, jointRadius + gap);
                        Sphere outerBall = new Sphere(chainCrv.PointAtStart, jointRadius + gap + holderThickness);

                        Curve nextChainCrv = segments.ElementAt(idx + 1);
                        double t;
                        nextChainCrv.LengthParameter(nextChainCrv.GetLength() - 0.2 * jointRadius, out t);
                        Curve capCrv = nextChainCrv.Split(t)[1];
                        Curve capLeftoverCrv = nextChainCrv.Split(t)[0];
                        Plane capPln = new Plane(capCrv.PointAtStart, capCrv.TangentAtStart);

                        Circle removeCir = new Circle(capPln, capCrv.PointAtStart, jointRadius + gap + holderThickness);
                        var removeBrep = sweep.PerformSweep(capLeftoverCrv, removeCir.ToNurbsCurve())[0];
                        removeBrep = removeBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);

                        var removeBrepDup = removeBrep.DuplicateBrep();

                        var outerBalls = Brep.CreateBooleanIntersection(outerBall.ToBrep(), removeBrep, myDoc.ModelAbsoluteTolerance);
                        var innerBalls = Brep.CreateBooleanIntersection(innerBall.ToBrep(), removeBrepDup, myDoc.ModelAbsoluteTolerance);
                        var shells = Brep.CreateBooleanDifference(outerBalls[0], innerBalls[0], myDoc.ModelAbsoluteTolerance);

                        myDoc.Objects.AddBrep(shells[0], red_attributes);

                        // Add the rod
                        chainCrv.LengthParameter(jointRadius + gap + holderThickness / 2, out t);
                        Curve rodTrajectory = chainCrv.Split(t)[1];

                        myDoc.Objects.AddCurve(rodTrajectory, red_attributes);
                        myDoc.Views.Redraw();

                        Plane rodPln = new Plane(rodTrajectory.PointAtStart, rodTrajectory.TangentAtStart);
                        Curve cylinCircle = new Circle(rodPln, rodTrajectory.PointAtStart, rodRadius).ToNurbsCurve();
                        var cylinBreps = sweep.PerformSweep(rodTrajectory, cylinCircle);
                        Brep cylinBrep = cylinBreps[0];
                        cylinBrep = cylinBrep.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                        myDoc.Objects.AddBrep(cylinBrep, red_attributes);
                    }
                    else if(idx == segments.Count() - 1)
                    {
                        // The last chain unit, only generate the ball joint
                        Sphere ball = new Sphere(chainCrv.PointAtEnd, jointRadius);
                        myDoc.Objects.AddSphere(ball, red_attributes);

                        Plane rodPln = new Plane(chainCrv.PointAtStart, chainCrv.TangentAtStart);
                        Curve cylinCircle = new Circle(rodPln, chainCrv.PointAtStart, rodRadius).ToNurbsCurve();
                        var cylinBreps = sweep.PerformSweep(chainCrv, cylinCircle);
                        Brep cylinBrep = cylinBreps[0];
                        cylinBrep = cylinBrep.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                        myDoc.Objects.AddBrep(cylinBrep, red_attributes);
                    }
                    else
                    {
                        // The middle chain units, generate both holder and ball joint
                        // Add the ball
                        Sphere ball = new Sphere(chainCrv.PointAtEnd, jointRadius);
                        myDoc.Objects.AddSphere(ball, red_attributes);

                        // Add the holder
                        Sphere innerBall = new Sphere(chainCrv.PointAtStart, jointRadius + gap);
                        Sphere outerBall = new Sphere(chainCrv.PointAtStart, jointRadius + gap + holderThickness);

                        Curve nextChainCrv = segments.ElementAt(idx + 1);
                        double t;
                        nextChainCrv.LengthParameter(nextChainCrv.GetLength() - 0.2 * jointRadius, out t);
                        Curve capCrv = nextChainCrv.Split(t)[1];
                        Curve capLeftoverCrv = nextChainCrv.Split(t)[0];
                        Plane capPln = new Plane(capCrv.PointAtStart, capCrv.TangentAtStart);

                        Circle removeCir = new Circle(capPln, capCrv.PointAtStart, jointRadius + gap + holderThickness);
                        var removeBrep = sweep.PerformSweep(capLeftoverCrv, removeCir.ToNurbsCurve())[0];
                        removeBrep = removeBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);

                        var removeBrepDup = removeBrep.DuplicateBrep();

                        var outerBalls = Brep.CreateBooleanIntersection(outerBall.ToBrep(), removeBrep, myDoc.ModelAbsoluteTolerance);
                        var innerBalls = Brep.CreateBooleanIntersection(innerBall.ToBrep(), removeBrepDup, myDoc.ModelAbsoluteTolerance);
                        var shells = Brep.CreateBooleanDifference(outerBalls[0], innerBalls[0], myDoc.ModelAbsoluteTolerance);

                        myDoc.Objects.AddBrep(shells[0], red_attributes);

                        // Add the rod
                        chainCrv.LengthParameter(jointRadius + gap + holderThickness / 2, out t);
                        Curve rodTrajectory = chainCrv.Split(t)[1];

                        Plane rodPln = new Plane(rodTrajectory.PointAtStart, rodTrajectory.TangentAtStart);
                        Curve cylinCircle = new Circle(rodPln, rodTrajectory.PointAtStart, rodRadius).ToNurbsCurve();
                        var cylinBreps = sweep.PerformSweep(rodTrajectory, cylinCircle);
                        Brep cylinBrep = cylinBreps[0];
                        cylinBrep = cylinBrep.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                        myDoc.Objects.AddBrep(cylinBrep, red_attributes);
                    }
                }
                #endregion
            }
            else
            {
                #region Allows one-direction bending, using cylinder joints

                double bendAngle = angle * Math.PI / 180;
                #region Test if all planes generated along the central axis has the same directions
                //double t;
                //double dis = 0;
                //for (dis = 0; dis < centerCrv.GetLength(); dis++)
                //{
                //    centerCrv.LengthParameter(dis, out t);
                //    Plane p = new Plane(centerCrv.PointAt(t), centerCrv.TangentAt(t));
                //    Point3d pt1 = centerCrv.PointAt(t);
                //    Vector3d direction = p.XAxis;
                //    direction.Rotate(bendAngle, centerCrv.TangentAt(t));
                //    Point3d pt2 = pt1 + direction;
                //    Line l = new Line(pt1, pt2);
                //    Curve l_c = l.ToNurbsCurve();

                //    myDoc.Objects.AddCurve(l_c, red_attributes);
                //    myDoc.Views.Redraw();
                //}
                #endregion

                for (int idx = 0; idx < segments.Count(); idx++)
                {
                    #region For debug: show all first points on all curves
                    //Sphere sphere = new Sphere(j.PointAtStart, 1.5);
                    //myDoc.Objects.AddSphere(sphere, red_attributes);
                    //myDoc.Views.Redraw();
                    #endregion

                    Curve chainCrv = segments.ElementAt(idx);

                    if (idx == 0)
                    {
                        // The first chain unit, only generate the holder
                        // Add the holder
                        Plane p = new Plane(chainCrv.PointAtStart, chainCrv.TangentAtStart);
                        Vector3d direction = p.XAxis;
                        Vector3d jointNormal = p.XAxis;
                        Point3d origPt = chainCrv.PointAtStart;
                        direction.Rotate(bendAngle, chainCrv.TangentAtStart);
                        jointNormal.Rotate(bendAngle + Math.PI/2, chainCrv.TangentAtStart);
                        Point3d dir1Pt = origPt + (jointNormal / jointNormal.Length) * (rodRadius + gap + holderThickness);
                        Point3d dir2Pt = origPt - (jointNormal / jointNormal.Length) * (rodRadius + gap + holderThickness);
                        Curve jointTrajectoryPart1 = new Line(origPt, dir1Pt).ToNurbsCurve();
                        Curve jointTrajectoryPart2 = new Line(origPt, dir2Pt).ToNurbsCurve();

                        Plane jointCylinderPln = new Plane(chainCrv.PointAtStart, jointNormal);

                        Curve jointCylinderInnerCircle = new Circle(jointCylinderPln, chainCrv.PointAtStart, rodRadius+gap).ToNurbsCurve();
                        var jointCylinderInnerPart1 = sweep.PerformSweep(jointTrajectoryPart1, jointCylinderInnerCircle)[0];
                        jointCylinderInnerPart1 = jointCylinderInnerPart1.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                        var jointCylinderInnerPart2 = sweep.PerformSweep(jointTrajectoryPart2, jointCylinderInnerCircle)[0];
                        jointCylinderInnerPart2 = jointCylinderInnerPart2.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                        var jointCylinderInnerBrep = Brep.CreateBooleanDifference(jointCylinderInnerPart2, jointCylinderInnerPart1, myDoc.ModelAbsoluteTolerance)[0];

                        Curve jointCylinderOuterCircle = new Circle(jointCylinderPln, chainCrv.PointAtStart, rodRadius + gap+holderThickness).ToNurbsCurve();
                        var jointCylinderOuterPart1 = sweep.PerformSweep(jointTrajectoryPart1, jointCylinderOuterCircle)[0];
                        jointCylinderOuterPart1 = jointCylinderOuterPart1.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                        var jointCylinderOuterPart2 = sweep.PerformSweep(jointTrajectoryPart2, jointCylinderOuterCircle)[0];
                        jointCylinderOuterPart2 = jointCylinderOuterPart2.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                        var jointCylinderOuterBrep = Brep.CreateBooleanDifference(jointCylinderOuterPart2, jointCylinderOuterPart1, myDoc.ModelAbsoluteTolerance)[0];

                        var holderBrep = Brep.CreateBooleanDifference(jointCylinderInnerBrep, jointCylinderOuterBrep, myDoc.ModelAbsoluteTolerance)[0];
                        //myDoc.Objects.AddBrep(holderBrep, red_attributes);

                        // Remove the central part so that a hollow holder is created
                        Point3d[] cavityPts = new Point3d[5];

                        cavityPts[0] = chainCrv.PointAtStart;
                        cavityPts[1] = chainCrv.PointAtStart;
                        cavityPts[2] = chainCrv.PointAtStart;
                        cavityPts[3] = chainCrv.PointAtStart;
                        cavityPts[4] = chainCrv.PointAtStart;

                        Point3d dirPt = chainCrv.PointAtStart + direction;
                        Point3d dirPtinPlane = jointCylinderPln.ClosestPoint(dirPt);
                        Vector3d cavX = dirPtinPlane - chainCrv.PointAtStart;

                        Vector3d longerDir = cavX;
                        Vector3d shorterDir = chainCrv.TangentAtStart;
                        longerDir.Rotate(Math.PI/2, jointNormal);
                        if (longerDir * shorterDir >= 0)
                        {
                            longerDir.Rotate(Math.PI, jointNormal);
                        }

                        Transform txp_rect = Transform.Translation(cavX / direction.Length * (rodRadius + gap + holderThickness));
                        Transform typ_rect = Transform.Translation(-longerDir / longerDir.Length * (rodRadius + gap));
                        Transform txn_rect = Transform.Translation(-cavX / direction.Length * (rodRadius + gap + holderThickness));
                        Transform tyn_rect = Transform.Translation(longerDir / longerDir.Length * (rodRadius + gap + holderThickness));

                        cavityPts[0].Transform(txp_rect); cavityPts[0].Transform(typ_rect);
                        cavityPts[1].Transform(txn_rect); cavityPts[1].Transform(typ_rect);
                        cavityPts[2].Transform(txn_rect); cavityPts[2].Transform(tyn_rect);
                        cavityPts[3].Transform(txp_rect); cavityPts[3].Transform(tyn_rect);
                        cavityPts[4] = cavityPts[0];

                        Curve cavityRect = new Polyline(cavityPts).ToNurbsCurve();


                        Point3d cavDir1Pt = origPt + (jointNormal / jointNormal.Length) * (rodRadius + gap);
                        Point3d cavDir2Pt = origPt - (jointNormal / jointNormal.Length) * (rodRadius + gap);
                        Curve cavityTrajectoryPart1 = new Line(origPt, cavDir1Pt).ToNurbsCurve();
                        Curve cavityTrajectoryPart2 = new Line(origPt, cavDir2Pt).ToNurbsCurve();

                        var cavityBrep1 = sweep.PerformSweep(cavityTrajectoryPart1, cavityRect)[0];
                        cavityBrep1 = cavityBrep1.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
                        var cavityBrep2 = sweep.PerformSweep(cavityTrajectoryPart2, cavityRect)[0];
                        cavityBrep2 = cavityBrep2.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);

                        var cavityUnion = Brep.CreateBooleanDifference(cavityBrep1, cavityBrep2, myDoc.ModelAbsoluteTolerance)[0];

                        var cavityBrep = Brep.CreateBooleanIntersection(holderBrep, cavityUnion, myDoc.ModelAbsoluteTolerance)[0];
                        myDoc.Objects.AddBrep(cavityBrep, red_attributes);
                        myDoc.Views.Redraw();

                        // Add the rod
                        double t;
                        chainCrv.LengthParameter(rodRadius + gap + holderThickness / 2, out t);
                        Curve rodTrajectory = chainCrv.Split(t)[1];

                        Plane rodPln = new Plane(rodTrajectory.PointAtStart, rodTrajectory.TangentAtStart);
                        Curve cylinCircle = new Circle(rodPln, rodTrajectory.PointAtStart, rodRadius).ToNurbsCurve();
                        var cylinBreps = sweep.PerformSweep(rodTrajectory, cylinCircle);
                        Brep cylinBrep = cylinBreps[0];
                        cylinBrep = cylinBrep.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                        myDoc.Objects.AddBrep(cylinBrep, red_attributes);
                    }
                    else if (idx == segments.Count() - 1)
                    {
                        // The last chain unit, only generate the ball joint
                        Plane p = new Plane(chainCrv.PointAtEnd, chainCrv.TangentAtEnd);
                        Vector3d direction = p.XAxis;
                        Vector3d jointNormal = p.XAxis;
                        Point3d origPt = chainCrv.PointAtEnd;
                        
                        direction.Rotate(bendAngle, chainCrv.TangentAtEnd);
                        jointNormal.Rotate(bendAngle+Math.PI/2, chainCrv.TangentAtEnd);
                        Point3d dir1Pt = origPt + (jointNormal / jointNormal.Length) * (rodRadius + gap + holderThickness);
                        Point3d dir2Pt = origPt - (jointNormal / jointNormal.Length) * (rodRadius + gap + holderThickness);
                        Curve jointTrajectoryPart1 = new Line(origPt, dir1Pt).ToNurbsCurve();
                        Curve jointTrajectoryPart2 = new Line(origPt, dir2Pt).ToNurbsCurve();

                        Plane jointCylinderPln = new Plane(chainCrv.PointAtEnd, jointNormal);
                        Curve jointCylinderCircle = new Circle(jointCylinderPln, chainCrv.PointAtEnd, rodRadius).ToNurbsCurve();
                        var jointCylinderPart1 = sweep.PerformSweep(jointTrajectoryPart1, jointCylinderCircle)[0];
                        jointCylinderPart1 = jointCylinderPart1.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                        var jointCylinderPart2 = sweep.PerformSweep(jointTrajectoryPart2, jointCylinderCircle)[0];
                        jointCylinderPart2 = jointCylinderPart2.CapPlanarHoles(myDoc.ModelRelativeTolerance);

                        var jointUnion = Brep.CreateBooleanDifference(jointCylinderPart2, jointCylinderPart1, myDoc.ModelAbsoluteTolerance)[0];
                        myDoc.Objects.AddBrep(jointUnion, red_attributes);
                        //myDoc.Objects.AddBrep(jointCylinderPart1, red_attributes);
                        //myDoc.Objects.AddBrep(jointCylinderPart2, red_attributes);

                        Plane rodPln = new Plane(chainCrv.PointAtStart, chainCrv.TangentAtStart);
                        Curve cylinCircle = new Circle(rodPln, chainCrv.PointAtStart, rodRadius).ToNurbsCurve();
                        var cylinBreps = sweep.PerformSweep(chainCrv, cylinCircle);
                        Brep cylinBrep = cylinBreps[0];
                        cylinBrep = cylinBrep.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                        myDoc.Objects.AddBrep(cylinBrep, red_attributes);
                    }
                    else
                    {
                        // The middle chain units, generate both holder and ball joint
                        // Add the cylinder joint
                        Plane pJoint = new Plane(chainCrv.PointAtEnd, chainCrv.TangentAtEnd);
                        Vector3d directionJoint = pJoint.XAxis;
                        Vector3d jointNormalJoint = pJoint.XAxis;
                        Point3d origPtJoint = chainCrv.PointAtEnd;

                        directionJoint.Rotate(bendAngle, chainCrv.TangentAtEnd);
                        jointNormalJoint.Rotate(bendAngle + Math.PI / 2, chainCrv.TangentAtEnd);
                        Point3d dir1PtJoint = origPtJoint + (jointNormalJoint / jointNormalJoint.Length) * (rodRadius + gap + holderThickness);
                        Point3d dir2PtJoint = origPtJoint - (jointNormalJoint / jointNormalJoint.Length) * (rodRadius + gap + holderThickness);
                        Curve jointTrajectoryPart1End = new Line(origPtJoint, dir1PtJoint).ToNurbsCurve();
                        Curve jointTrajectoryPart2End = new Line(origPtJoint, dir2PtJoint).ToNurbsCurve();

                        Plane jointCylinderPlnEnd = new Plane(chainCrv.PointAtEnd, jointNormalJoint);
                        Curve jointCylinderCircle = new Circle(jointCylinderPlnEnd, chainCrv.PointAtEnd, rodRadius).ToNurbsCurve();
                        var jointCylinderPart1 = sweep.PerformSweep(jointTrajectoryPart1End, jointCylinderCircle)[0];
                        jointCylinderPart1 = jointCylinderPart1.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                        var jointCylinderPart2 = sweep.PerformSweep(jointTrajectoryPart2End, jointCylinderCircle)[0];
                        jointCylinderPart2 = jointCylinderPart2.CapPlanarHoles(myDoc.ModelRelativeTolerance);

                        var jointUnion = Brep.CreateBooleanDifference(jointCylinderPart2, jointCylinderPart1, myDoc.ModelAbsoluteTolerance)[0];
                        myDoc.Objects.AddBrep(jointUnion, red_attributes);

                        // Add the holder
                        Plane p = new Plane(chainCrv.PointAtStart, chainCrv.TangentAtStart);
                        Vector3d direction = p.XAxis;
                        Vector3d jointNormal = p.XAxis;
                        Point3d origPt = chainCrv.PointAtStart;
                        direction.Rotate(bendAngle, chainCrv.TangentAtStart);
                        jointNormal.Rotate(bendAngle + Math.PI/2, chainCrv.TangentAtStart);
                        Point3d dir1Pt = origPt + (jointNormal / jointNormal.Length) * (rodRadius + gap + holderThickness);
                        Point3d dir2Pt = origPt - (jointNormal / jointNormal.Length) * (rodRadius + gap + holderThickness);
                        Curve jointTrajectoryPart1 = new Line(origPt, dir1Pt).ToNurbsCurve();
                        Curve jointTrajectoryPart2 = new Line(origPt, dir2Pt).ToNurbsCurve();

                        Plane jointCylinderPln = new Plane(chainCrv.PointAtStart, jointNormal);

                        Curve jointCylinderInnerCircle = new Circle(jointCylinderPln, chainCrv.PointAtStart, rodRadius+gap).ToNurbsCurve();
                        var jointCylinderInnerPart1 = sweep.PerformSweep(jointTrajectoryPart1, jointCylinderInnerCircle)[0];
                        jointCylinderInnerPart1 = jointCylinderInnerPart1.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                        var jointCylinderInnerPart2 = sweep.PerformSweep(jointTrajectoryPart2, jointCylinderInnerCircle)[0];
                        jointCylinderInnerPart2 = jointCylinderInnerPart2.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                        var jointCylinderInnerBrep = Brep.CreateBooleanDifference(jointCylinderInnerPart2, jointCylinderInnerPart1, myDoc.ModelAbsoluteTolerance)[0];

                        Curve jointCylinderOuterCircle = new Circle(jointCylinderPln, chainCrv.PointAtStart, rodRadius + gap+holderThickness).ToNurbsCurve();
                        var jointCylinderOuterPart1 = sweep.PerformSweep(jointTrajectoryPart1, jointCylinderOuterCircle)[0];
                        jointCylinderOuterPart1 = jointCylinderOuterPart1.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                        var jointCylinderOuterPart2 = sweep.PerformSweep(jointTrajectoryPart2, jointCylinderOuterCircle)[0];
                        jointCylinderOuterPart2 = jointCylinderOuterPart2.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                        var jointCylinderOuterBrep = Brep.CreateBooleanDifference(jointCylinderOuterPart2, jointCylinderOuterPart1, myDoc.ModelAbsoluteTolerance)[0];

                        var holderBrep = Brep.CreateBooleanDifference(jointCylinderInnerBrep, jointCylinderOuterBrep, myDoc.ModelAbsoluteTolerance)[0];
                        //myDoc.Objects.AddBrep(holderBrep, red_attributes);

                        // Remove the central part so that a hollow holder is created
                        Point3d[] cavityPts = new Point3d[5];

                        cavityPts[0] = chainCrv.PointAtStart;
                        cavityPts[1] = chainCrv.PointAtStart;
                        cavityPts[2] = chainCrv.PointAtStart;
                        cavityPts[3] = chainCrv.PointAtStart;
                        cavityPts[4] = chainCrv.PointAtStart;

                        Point3d dirPt = chainCrv.PointAtStart + direction;
                        Point3d dirPtinPlane = jointCylinderPln.ClosestPoint(dirPt);
                        Vector3d cavX = dirPtinPlane - chainCrv.PointAtStart;

                        Vector3d longerDir = cavX;
                        Vector3d shorterDir = chainCrv.TangentAtStart;
                        longerDir.Rotate(Math.PI/2, jointNormal);
                        if (longerDir * shorterDir >= 0)
                        {
                            longerDir.Rotate(Math.PI, jointNormal);
                        }

                        Transform txp_rect = Transform.Translation(cavX / direction.Length * (rodRadius + gap + holderThickness));
                        Transform typ_rect = Transform.Translation(-longerDir / longerDir.Length * (rodRadius + gap));
                        Transform txn_rect = Transform.Translation(-cavX / direction.Length * (rodRadius + gap + holderThickness));
                        Transform tyn_rect = Transform.Translation(longerDir / longerDir.Length * (rodRadius + gap + holderThickness));

                        cavityPts[0].Transform(txp_rect); cavityPts[0].Transform(typ_rect);
                        cavityPts[1].Transform(txn_rect); cavityPts[1].Transform(typ_rect);
                        cavityPts[2].Transform(txn_rect); cavityPts[2].Transform(tyn_rect);
                        cavityPts[3].Transform(txp_rect); cavityPts[3].Transform(tyn_rect);
                        cavityPts[4] = cavityPts[0];

                        Curve cavityRect = new Polyline(cavityPts).ToNurbsCurve();


                        Point3d cavDir1Pt = origPt + (jointNormal / jointNormal.Length) * (rodRadius + gap);
                        Point3d cavDir2Pt = origPt - (jointNormal / jointNormal.Length) * (rodRadius + gap);
                        Curve cavityTrajectoryPart1 = new Line(origPt, cavDir1Pt).ToNurbsCurve();
                        Curve cavityTrajectoryPart2 = new Line(origPt, cavDir2Pt).ToNurbsCurve();

                        var cavityBrep1 = sweep.PerformSweep(cavityTrajectoryPart1, cavityRect)[0];
                        cavityBrep1 = cavityBrep1.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
                        var cavityBrep2 = sweep.PerformSweep(cavityTrajectoryPart2, cavityRect)[0];
                        cavityBrep2 = cavityBrep2.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);

                        var cavityUnion = Brep.CreateBooleanDifference(cavityBrep1, cavityBrep2, myDoc.ModelAbsoluteTolerance)[0];

                        var cavityBrep = Brep.CreateBooleanIntersection(holderBrep, cavityUnion, myDoc.ModelAbsoluteTolerance)[0];
                        myDoc.Objects.AddBrep(cavityBrep, red_attributes);
                        myDoc.Views.Redraw();

                        // Add the rod
                        double t;
                        chainCrv.LengthParameter(rodRadius + gap + holderThickness / 2, out t);
                        Curve rodTrajectory = chainCrv.Split(t)[1];

                        Plane rodPln = new Plane(rodTrajectory.PointAtStart, rodTrajectory.TangentAtStart);
                        Curve cylinCircle = new Circle(rodPln, rodTrajectory.PointAtStart, rodRadius).ToNurbsCurve();
                        var cylinBreps = sweep.PerformSweep(rodTrajectory, cylinCircle);
                        Brep cylinBrep = cylinBreps[0];
                        cylinBrep = cylinBrep.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                        myDoc.Objects.AddBrep(cylinBrep, red_attributes);
                    }
                }
                #endregion
            }

            myDoc.Views.Redraw();
        }

        #region Old versions of generating the deformation constraints
        #region Old version of generate the bend support
        ///// <summary>
        ///// This method generates bend deformation structure.
        ///// </summary>
        ///// <param name="objRef"></param>
        //public void bendDeform(ObjRef objRef)
        //{
        //    // in the current code we ask the user to select the outer shell of the geometry. In the real case this should be the same
        //    // part as our point cloud selection so user don't need to select themselves.
        //    const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.PolysrfFilter;// filter allows us to constrain the type of objects the user can select
        //    Rhino.DocObjects.ObjRef sufObjRef;
        //    Guid sufObjId = Guid.Empty; // all rhino doc objects has a unique ID. We can always find the object by create an objRef with the id
        //    Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one surface to print", false, filter, out sufObjRef);
        //    if (rc == Rhino.Commands.Result.Success)
        //    {
        //        sufObjId = sufObjRef.ObjectId;
        //    }
        //    ObjRef armOffsetObjRef = new ObjRef(sufObjId);//get the objRef from the GUID

        //    Brep surfaceBrep = armOffsetObjRef.Brep(); // because we know the geometry is Brep, we directly find it from the objRef 

        //    #region generate start and end plane of the curve
        //    Curve centerCrv = objRef.Curve();
        //    Point3d startPt = centerCrv.PointAtStart;
        //    Point3d endPt = centerCrv.PointAtEnd;
        //    double startPara = 0, endPara = 0;
        //    centerCrv.LengthParameter(0, out startPara);
        //    centerCrv.LengthParameter(centerCrv.GetLength(), out endPara);
        //    Plane startPln = new Plane(startPt, centerCrv.TangentAt(startPara));// the plane at the start of the curve
        //    Plane endPln = new Plane(endPt, centerCrv.TangentAt(endPara));// the plane at the end of the curve
        //    Rhino.Geometry.PlaneSurface startPlnSuf = new PlaneSurface(startPln, new Interval(-50, 50), new Interval(-50, 50));// convert the plane object to an actual surface so that we can draw it in the rhino doc
        //    Rhino.Geometry.PlaneSurface endPlnSuf = new PlaneSurface(endPln, new Interval(-50, 50), new Interval(-50, 50));// 15 is a random number. It should actually be the ourter shell's radius or larger.
        //    //myDoc.Objects.AddSurface(startPlnSuf);
        //    //myDoc.Objects.AddSurface(endPlnSuf);
        //    #endregion

        //    #region get the anchor control points for deciding the bending direction
        //    this.centerCrv = centerCrv;
        //    var attributes = new ObjectAttributes();
        //    attributes.ObjectColor = Color.White;
        //    attributes.ColorSource = ObjectColorSource.ColorFromObject;

        //    // listen to the user's selected sphere: either at the start or at the end sphere
        //    Rhino.Input.Custom.GetPoint gp_pt = new Rhino.Input.Custom.GetPoint();
        //    gp_pt.DynamicDraw += Gp_sphereSelDynamicDraw;
        //    gp_pt.MouseMove += Gp_sphereSelMouseMove;
        //    gp_pt.Get(true);

        //    if (distanceBtwTwoPts(this.bendPtSel, this.centerCrv.PointAtStart) <= 1.5)
        //    {
        //        // selected the start sphere
        //        Rhino.Geometry.Sphere startSphere = new Rhino.Geometry.Sphere(startPt, 1.5);
        //        Guid sphId = myDoc.Objects.AddSphere(startSphere, attributes);
        //        Plane anglePln = new Plane(startPt, centerCrv.TangentAt(startPara)); // the plane at the starting plane of the central curve
        //        this.bendPlane = anglePln;

        //        this.bendCtrlPt = startPt;
        //        Rhino.Geometry.Circle dirCircle = new Rhino.Geometry.Circle(anglePln, startPt, 20);
        //        this.angleCircle = dirCircle;

        //        Guid sId = myDoc.Objects.AddCircle(dirCircle);
        //        myDoc.Views.Redraw();

        //        // start the command 
        //        is_BendStart = true;

        //        if (is_BendStart)
        //        {
        //            // if the user doesn't press the 'Enter' key it will continue to accept the next direction
        //            bend_info tempBendInfo = new bend_info();

        //            // step 1: decide the direction
        //            Rhino.Input.Custom.GetPoint gp_dir = new Rhino.Input.Custom.GetPoint();
        //            //gp.MouseDown += Gp_BendAngleSelMouseDown; // only when the user click it starts to change the angle;
        //            gp_dir.MouseMove += Gp_BendAngleSelMouseMove; // Every move compute the changed angle
        //            gp_dir.DynamicDraw += Gp_BendAngelSelDynamicDraw; // Draw the refreshed ball in the scenes
        //            gp_dir.Get(true);
        //            Point3d p = this.angleCircle.ClosestPoint(gp_dir.Point());
        //            tempBendInfo.dir = (Vector3d)(p - startPt);

        //            // step 2: decide the bending strength
        //            Rhino.Input.Custom.GetPoint gp_bs = new Rhino.Input.Custom.GetPoint();
        //            gp_bs.MouseMove += Gp_BendStrengthSelMouseMove;
        //            gp_bs.DynamicDraw += Gp_BendStrengthSelDynamicDraw;
        //            gp_bs.Get(true);
        //            Point3d pp = gp_bs.Point();
        //            tempBendInfo.strength = Math.Sqrt(Math.Pow(pp.X - p.X, 2) + Math.Pow(pp.Y - p.Y, 2) + Math.Pow(pp.Z - p.Z, 2));  // check if this is discrete or continuous

        //            // add current bending information in the bending list
        //            //RhinoApp.WriteLine("direction: (" + tempBendInfo.dir.X + "," + tempBendInfo.dir.Y + "," + tempBendInfo.dir.Z + ")");
        //            //RhinoApp.WriteLine("strength: " + tempBendInfo.strength);
        //            bendInfoList.Add(tempBendInfo);
        //        }

        //        myDoc.Objects.Delete(sId, false);
        //        myDoc.Objects.Delete(sphId, false);
        //        myDoc.Views.Redraw();

        //        #region generating the outer spring
        //        double K;
        //        springGeneration(centerCrv, surfaceBrep, 5, 3, out K);
        //        RhinoApp.WriteLine("The K value of the generated soring is: " + K);
        //        #endregion

        //        #region delete the part that spring will replaced
        //        // chop the shells into 3 piece
        //        List<Brep> splitsurf = new List<Brep>();
        //        Brep[] firstSplit = surfaceBrep.Split(startPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);

        //        Brep[] test = firstSplit[0].Split(endPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);
        //        if (test != null && test.Length > 0)
        //        {
        //            splitsurf.AddRange(test);
        //            splitsurf.Add(firstSplit[1]);
        //        }
        //        else
        //        {
        //            test = firstSplit[1].Split(endPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);
        //            splitsurf.AddRange(test);
        //            splitsurf.Add(firstSplit[0]);
        //        }
        //        List<Brep> finalPreservedBrepList = new List<Brep>();
        //        foreach (Brep b in splitsurf)
        //        {
        //            Point3d bcenter = b.GetBoundingBox(true).Center;
        //            Vector3d v1 = bcenter - endPln.Origin;
        //            Vector3d v2 = bcenter - startPln.Origin;
        //            if (v1 * v2 > 0)
        //            {
        //                Brep temp = b.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
        //                finalPreservedBrepList.Add(temp);
        //            }

        //        }


        //        foreach (Brep b in finalPreservedBrepList)
        //        {
        //            myDoc.Objects.AddBrep(b, redAttribute);
        //        }

        //        #endregion

        //        // generate the bend support structure
        //        List<Brep> centrShaft = new List<Brep>();
        //        List<Brep> hinges = new List<Brep>();
        //        generateBendSupport(startPln, endPln, this.centerCrv, bendInfoList, startPt, endPt, true, out centrShaft, out hinges);

        //        // Clear the bend information list
        //        bendInfoList.Clear();

        //        // Animation
        //        List<Brep> animList = new List<Brep>();
        //        animList.Add(finalPreservedBrepList[1]);

        //        // add all breps of the central connector and all hinges
        //        foreach (Brep s in centrShaft)
        //        {
        //            animList.Add(s);
        //        }
        //        foreach (Brep h in hinges)
        //        {
        //            animList.Add(h);
        //        }
        //    }
        //    else if (distanceBtwTwoPts(this.bendPtSel, this.centerCrv.PointAtEnd) <= 1.5)
        //    {
        //        // selected the end sphere
        //        Rhino.Geometry.Sphere endSphere = new Rhino.Geometry.Sphere(endPt, 1.5);
        //        Guid sphId = myDoc.Objects.AddSphere(endSphere, attributes);
        //        Plane anglePln = new Plane(endPt, centerCrv.TangentAt(endPara)); // the plane at the end plane of the central curve
        //        this.bendPlane = anglePln;

        //        this.bendCtrlPt = endPt;
        //        Rhino.Geometry.Circle dirCircle = new Rhino.Geometry.Circle(anglePln, endPt, 20);
        //        this.angleCircle = dirCircle;

        //        Guid sId = myDoc.Objects.AddCircle(dirCircle);
        //        myDoc.Views.Redraw();

        //        // start the command 
        //        is_BendStart = true;

        //        if (is_BendStart)
        //        {
        //            // if the user doesn't press the 'Enter' key it will continue to accept the next direction
        //            bend_info tempBendInfo = new bend_info();

        //            // step 1: decide the direction
        //            Rhino.Input.Custom.GetPoint gp_dir = new Rhino.Input.Custom.GetPoint();
        //            //gp.MouseDown += Gp_BendAngleSelMouseDown; // only when the user click it starts to change the angle;
        //            gp_dir.MouseMove += Gp_BendAngleSelMouseMove; // Every move compute the changed angle
        //            gp_dir.DynamicDraw += Gp_BendAngelSelDynamicDraw; // Draw the refreshed ball in the scenes
        //            gp_dir.Get(true);
        //            Point3d p = this.angleCircle.ClosestPoint(gp_dir.Point());
        //            tempBendInfo.dir = (Vector3d)(p - endPt);

        //            // step 2: decide the bending strength
        //            Rhino.Input.Custom.GetPoint gp_bs = new Rhino.Input.Custom.GetPoint();
        //            gp_bs.MouseMove += Gp_BendStrengthSelMouseMove;
        //            gp_bs.DynamicDraw += Gp_BendStrengthSelDynamicDraw;
        //            gp_bs.Get(true);
        //            Point3d pp = gp_bs.Point();
        //            tempBendInfo.strength = Math.Sqrt(Math.Pow(pp.X - p.X, 2) + Math.Pow(pp.Y - p.Y, 2) + Math.Pow(pp.Z - p.Z, 2));  // check if this is discrete or continuous

        //            // add current bending information in the bending list
        //            RhinoApp.WriteLine("direction: (" + tempBendInfo.dir.X + "," + tempBendInfo.dir.Y + "," + tempBendInfo.dir.Z + ")");
        //            RhinoApp.WriteLine("strength: " + tempBendInfo.strength);
        //            bendInfoList.Add(tempBendInfo);
        //        }
        //        myDoc.Objects.Delete(sId, false);
        //        myDoc.Objects.Delete(sphId, false);
        //        myDoc.Views.Redraw();

        //        #region generating the outer spring
        //        double K;
        //        springGeneration(centerCrv, surfaceBrep, 5, 3, out K);
        //        RhinoApp.WriteLine("The K value of the generated soring is: " + K);
        //        #endregion

        //        #region delete the part that spring will replaced
        //        // chop the shells into 3 piece
        //        List<Brep> splitsurf = new List<Brep>();
        //        Brep[] firstSplit = surfaceBrep.Split(startPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);

        //        Brep[] test = firstSplit[0].Split(endPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);
        //        if (test != null && test.Length > 0)
        //        {
        //            splitsurf.AddRange(test);
        //            splitsurf.Add(firstSplit[1]);
        //        }
        //        else
        //        {
        //            test = firstSplit[1].Split(endPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);
        //            splitsurf.AddRange(test);
        //            splitsurf.Add(firstSplit[0]);
        //        }
        //        List<Brep> finalPreservedBrepList = new List<Brep>();
        //        foreach (Brep b in splitsurf)
        //        {
        //            Point3d bcenter = b.GetBoundingBox(true).Center;
        //            Vector3d v1 = bcenter - endPln.Origin;
        //            Vector3d v2 = bcenter - startPln.Origin;
        //            if (v1 * v2 > 0)
        //            {
        //                Brep temp = b.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
        //                finalPreservedBrepList.Add(temp);
        //            }

        //        }


        //        foreach (Brep b in finalPreservedBrepList)
        //        {
        //            myDoc.Objects.AddBrep(b, redAttribute);
        //        }

        //        #endregion

        //        // generate the bend support structure
        //        List<Brep> centrShaft = new List<Brep>();
        //        List<Brep> hinges = new List<Brep>();
        //        generateBendSupport(startPln, endPln, this.centerCrv, bendInfoList, startPt, endPt, false, out centrShaft, out hinges);

        //        // Clear the bend information list
        //        bendInfoList.Clear();

        //        // Animation
        //        List<Brep> animList = new List<Brep>();
        //        animList.Add(finalPreservedBrepList[1]);

        //        // add all breps of the central connector and all hinges
        //        foreach (Brep s in centrShaft)
        //        {
        //            animList.Add(s);
        //        }
        //        foreach (Brep h in hinges)
        //        {
        //            animList.Add(h);
        //        }
        //    }

        //    #endregion

        //    myDoc.Objects.Hide(sufObjId, true);// hide the original shell
        //    myDoc.Views.Redraw();

        //}
        #endregion

        #region Old version of generate the support structure for linear deformation
        ///// <summary>
        ///// This function generate support structure for compression
        ///// </summary>
        ///// <param name="startSuf">The start surface plane of the center axis</param>
        ///// <param name="endSuf">The end surface plane of the center axis</param>
        ///// <param name="centerCrv">Center axis</param>
        ///// <param name="compressCrv">Compression curve, which is one side of the centerCrve splited by the user's control point input</param>
        ///// <param name="pressCrv">the other side of the center curve split</param>
        //private void generateLinearSupport(Plane startSuf, Plane endSuf, Curve centerCrv, Curve compressCrv, Curve pressCrv, Curve stretchCrv, Point3d railEnd, Plane stopperPlnOrig, out Brep stopperBrep, out Brep prismBrep)
        //{

        //    // join compressCrv and stretchCrv to create a real rail for both compress and stretch
        //    List<Curve> crves = new List<Curve>();
        //    crves.Add(compressCrv);
        //    crves.Add(stretchCrv);

        //    Curve linearCrv = Curve.JoinCurves(crves)[0];

        //    // base structure 2 bars
        //    double baseStructureDisToCenter = 3.3;
        //    Curve baseStructureCrv1 = linearCrv.DuplicateCurve();
        //    Curve baseStructureCrv2 = linearCrv.DuplicateCurve();
        //    baseStructureCrv1.Translate(endSuf.YAxis * baseStructureDisToCenter);
        //    baseStructureCrv2.Translate(endSuf.YAxis * (-baseStructureDisToCenter));
        //    Point3d[] guiderOuterCornerPt = new Point3d[5];
        //    Point3d[] guiderInnerCornerPt = new Point3d[5];
        //    Point3d[] cornerPt = new Point3d[5];
        //    Transform txp = Transform.Translation(endSuf.XAxis * 2.8);
        //    Transform typ = Transform.Translation(endSuf.YAxis * 0.5);
        //    Transform txn = Transform.Translation(endSuf.XAxis * -2.8);
        //    Transform tyn = Transform.Translation(endSuf.YAxis * -0.5);
        //    cornerPt[0] = baseStructureCrv1.PointAtEnd;
        //    cornerPt[1] = baseStructureCrv1.PointAtEnd;
        //    cornerPt[2] = baseStructureCrv1.PointAtEnd;
        //    cornerPt[3] = baseStructureCrv1.PointAtEnd;
        //    cornerPt[0].Transform(txp); cornerPt[0].Transform(typ);
        //    cornerPt[1].Transform(txn); cornerPt[1].Transform(typ);
        //    cornerPt[2].Transform(txn); cornerPt[2].Transform(tyn);
        //    cornerPt[3].Transform(txp); cornerPt[3].Transform(tyn);

        //    guiderOuterCornerPt[0] = cornerPt[0];
        //    guiderOuterCornerPt[1] = cornerPt[1];

        //    guiderInnerCornerPt[0] = cornerPt[2];
        //    guiderInnerCornerPt[1] = cornerPt[3];

        //    Curve baseRectCrv1 = new Polyline(cornerPt).ToNurbsCurve();
        //    cornerPt[0] = baseStructureCrv2.PointAtEnd;
        //    cornerPt[1] = baseStructureCrv2.PointAtEnd;
        //    cornerPt[2] = baseStructureCrv2.PointAtEnd;
        //    cornerPt[3] = baseStructureCrv2.PointAtEnd;
        //    cornerPt[0].Transform(txp); cornerPt[0].Transform(typ);
        //    cornerPt[1].Transform(txn); cornerPt[1].Transform(typ);
        //    cornerPt[2].Transform(txn); cornerPt[2].Transform(tyn);
        //    cornerPt[3].Transform(txp); cornerPt[3].Transform(tyn);
        //    cornerPt[4] = cornerPt[0];

        //    guiderOuterCornerPt[2] = cornerPt[2];
        //    guiderOuterCornerPt[3] = cornerPt[3];
        //    guiderOuterCornerPt[4] = guiderOuterCornerPt[0];

        //    guiderInnerCornerPt[2] = cornerPt[0];
        //    guiderInnerCornerPt[3] = cornerPt[1];
        //    guiderInnerCornerPt[4] = guiderInnerCornerPt[0];

        //    guiderInnerCornerPt[0].Transform(txn);
        //    guiderInnerCornerPt[1].Transform(txp);
        //    guiderInnerCornerPt[2].Transform(txp);
        //    guiderInnerCornerPt[3].Transform(txn);
        //    guiderInnerCornerPt[4].Transform(txn);

        //    //foreach(var p in guiderInnerCornerPt)
        //    //{
        //    //    myDoc.Objects.AddPoint(p);
        //    //    myDoc.Views.Redraw();
        //    //}
        //    Curve guiderOuterRectCrv = new Polyline(guiderOuterCornerPt).ToNurbsCurve();
        //    Curve guiderInnerRectCrv = new Polyline(guiderInnerCornerPt).ToNurbsCurve();

        //    var outerRect = sweep.PerformSweep(linearCrv, guiderOuterRectCrv)[0];
        //    var innerRect = sweep.PerformSweep(linearCrv, guiderInnerRectCrv)[0];


        //    var baseBreps = Brep.CreateBooleanIntersection(outerRect, innerRect, myDoc.ModelRelativeTolerance);
        //    baseBreps[0] = baseBreps[0].CapPlanarHoles(myDoc.ModelRelativeTolerance);
        //    baseBreps[1] = baseBreps[1].CapPlanarHoles(myDoc.ModelRelativeTolerance);

        //    myDoc.Objects.AddBrep(baseBreps[0]);
        //    myDoc.Objects.AddBrep(baseBreps[1]);

        //    List<Point3d> baseVertexList = new List<Point3d>();
        //    foreach (Brep b in baseBreps)
        //    {
        //        Rhino.Geometry.Collections.BrepVertexList vertexList = b.Vertices;
        //        if (vertexList != null && vertexList.Count > 0)
        //        {
        //            foreach (var v in vertexList)
        //            {
        //                baseVertexList.Add(v.Location);
        //            }
        //        }
        //    }

        //    Plane guiderPln = new Plane(linearCrv.PointAtEnd, linearCrv.TangentAtEnd);
        //    PlaneSurface guiderPlnSuf = new PlaneSurface(guiderPln, new Interval(-30, 30), new Interval(-30, 30));
        //    List<Point3d> guiderPointsList = new List<Point3d>();
        //    foreach (Point3d p in baseVertexList)
        //    {
        //        double u, v;
        //        guiderPlnSuf.ClosestPoint(p, out u, out v);
        //        if (guiderPlnSuf.PointAt(u, v).DistanceTo(p) < 0.5)
        //        {
        //            guiderPointsList.Add(p);
        //        }
        //    }

        //    Rhino.Geometry.PointCloud ptCloud = new PointCloud(guiderPointsList);
        //    for (int i = 0; i < 4; i++)
        //    {
        //        int removeIdx = ptCloud.ClosestPoint(guiderPln.Origin);
        //        ptCloud.RemoveAt(removeIdx);
        //    }
        //    guiderPointsList.Clear();
        //    foreach (var p in ptCloud)
        //        guiderPointsList.Add(p.Location);

        //    guiderPointsList.Add(guiderPointsList[0]);
        //    Curve guiderTopCrv = new Polyline(guiderPointsList).ToNurbsCurve();

        //    //prismic structure that limits rotation
        //    Point3d centerPrismPt = centerCrv.PointAtStart;
        //    double primBaseSideHalfLength = 1.5;
        //    txp = Transform.Translation(startSuf.XAxis * primBaseSideHalfLength);
        //    typ = Transform.Translation(startSuf.YAxis * primBaseSideHalfLength);
        //    txn = Transform.Translation(startSuf.XAxis * -primBaseSideHalfLength);
        //    tyn = Transform.Translation(startSuf.YAxis * -primBaseSideHalfLength);

        //    cornerPt[0] = centerPrismPt;
        //    cornerPt[1] = centerPrismPt;
        //    cornerPt[2] = centerPrismPt;
        //    cornerPt[3] = centerPrismPt;
        //    cornerPt[0].Transform(txp); cornerPt[0].Transform(typ);
        //    cornerPt[1].Transform(txn); cornerPt[1].Transform(typ);
        //    cornerPt[2].Transform(txn); cornerPt[2].Transform(tyn);
        //    cornerPt[3].Transform(txp); cornerPt[3].Transform(tyn);
        //    cornerPt[4] = cornerPt[0];
        //    Curve prismCrv = new Polyline(cornerPt).ToNurbsCurve();
        //    prismBrep = sweep.PerformSweep(pressCrv, prismCrv)[0];
        //    prismBrep = prismBrep.CapPlanarHoles(myDoc.ModelRelativeTolerance);
        //    myDoc.Objects.AddBrep(prismBrep);
        //    //myDoc.Views.Redraw();


        //    // stopper (disc)
        //    Plane stopperPln = new Plane(pressCrv.PointAtEnd, pressCrv.TangentAtEnd);
        //    Circle stopperCir = new Circle(stopperPln, pressCrv.PointAtEnd, 2.3);
        //    double t;
        //    compressCrv.LengthParameter(2, out t);          // the thickness of the stopper (disc)
        //    Curve stopperCrv = compressCrv.Split(t)[0];

        //    stopperBrep = sweep.PerformSweep(stopperCrv, stopperCir.ToNurbsCurve())[0];
        //    stopperBrep = stopperBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);

        //    //myDoc.Objects.AddCurve(stopperCrv);
        //    myDoc.Objects.AddBrep(stopperBrep);
        //    //myDoc.Views.Redraw();


        //    // guider hole

        //    Point3d guiderPt = railEnd;
        //    double guiderPtGap = 0.5;

        //    txp = Transform.Translation(stopperPlnOrig.XAxis * (primBaseSideHalfLength + guiderPtGap));
        //    typ = Transform.Translation(stopperPlnOrig.YAxis * (primBaseSideHalfLength + guiderPtGap));
        //    txn = Transform.Translation(stopperPlnOrig.XAxis * -(primBaseSideHalfLength + guiderPtGap));
        //    tyn = Transform.Translation(stopperPlnOrig.YAxis * -(primBaseSideHalfLength + guiderPtGap));

        //    cornerPt[0] = guiderPt;
        //    cornerPt[1] = guiderPt;
        //    cornerPt[2] = guiderPt;
        //    cornerPt[3] = guiderPt;
        //    cornerPt[0].Transform(txp); cornerPt[0].Transform(typ);
        //    cornerPt[1].Transform(txn); cornerPt[1].Transform(typ);
        //    cornerPt[2].Transform(txn); cornerPt[2].Transform(tyn);
        //    cornerPt[3].Transform(txp); cornerPt[3].Transform(tyn);
        //    cornerPt[4] = cornerPt[0];
        //    Curve guiderCrv = new Polyline(cornerPt).ToNurbsCurve();

        //    pressCrv.ClosestPoint(railEnd, out t);
        //    var splitedLeftOver = pressCrv.Split(t);
        //    splitedLeftOver[0].LengthParameter(splitedLeftOver[0].GetLength() - 4, out t);

        //    var splitedLeftCrvs = splitedLeftOver[0].Split(t);
        //    Brep guiderCenter = sweep.PerformSweep(splitedLeftCrvs[1], guiderCrv)[0];
        //    guiderCenter = guiderCenter.CapPlanarHoles(myDoc.ModelRelativeTolerance);

        //    //guider outcube
        //    Brep outerGuider = sweep.PerformSweep(splitedLeftCrvs[1], guiderTopCrv)[0];
        //    outerGuider = outerGuider.CapPlanarHoles(myDoc.ModelRelativeTolerance);
        //    outerGuider.Flip();
        //    var guiderFinal = Brep.CreateBooleanDifference(guiderCenter, outerGuider, myDoc.ModelRelativeTolerance)[0];
        //    myDoc.Objects.Add(guiderFinal);

        //    // Only refresh once
        //    myDoc.Views.Redraw();
        //}
        #endregion

        #region Old version of linear deformation
        //public void linearDeform(ObjRef objRef)
        //{
        //    // in the current code we ask the user to select the outer shell of the geometry. In the real case this should be the same
        //    // part as our point cloud selection so user don't need to select themselves.
        //    const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.PolysrfFilter;// filter allows us to constrain the type of objects the user can select
        //    Rhino.DocObjects.ObjRef sufObjRef;
        //    Guid sufObjId = Guid.Empty; // all rhino doc objects has a unique ID. We can always find the object by create an objRef with the id
        //    Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one surface to print", false, filter, out sufObjRef);
        //    if (rc == Rhino.Commands.Result.Success)
        //    {
        //        sufObjId = sufObjRef.ObjectId;
        //    }
        //    ObjRef armOffsetObjRef = new ObjRef(sufObjId);//get the objRef from the GUID

        //    Brep surfaceBrep = armOffsetObjRef.Brep(); // because we know the geometry is Brep, we directly find it from the objRef 

        //    #region generate start and end plane of the curve
        //    Curve centerCrv = objRef.Curve();
        //    Point3d startPt = centerCrv.PointAtStart;
        //    Point3d endPt = centerCrv.PointAtEnd;
        //    double startPara = 0, endPara = 0;
        //    centerCrv.LengthParameter(0, out startPara);
        //    centerCrv.LengthParameter(centerCrv.GetLength(), out endPara);
        //    Plane startPln = new Plane(startPt, centerCrv.TangentAt(startPara));// the plane at the start of the curve
        //    Plane endPln = new Plane(endPt, centerCrv.TangentAt(endPara));// the plane at the end of the curve
        //    Rhino.Geometry.PlaneSurface startPlnSuf = new PlaneSurface(startPln, new Interval(-25, 25), new Interval(-25, 25));// convert the plane object to an actual surface so that we can draw it in the rhino doc
        //    Rhino.Geometry.PlaneSurface endPlnSuf = new PlaneSurface(endPln, new Interval(-25, 25), new Interval(-25, 25));// 15 is a random number. It should actually be the ourter shell's radius or larger.
        //    //myDoc.Objects.AddSurface(startPlnSuf);
        //    //myDoc.Objects.AddSurface(endPlnSuf);
        //    //myDoc.Views.Redraw();
        //    #endregion

        //    // use Rhino's GetPoint function to dynamically draw the a sphere and find the control point
        //    #region control point
        //    this.centerCrv = centerCrv;
        //    Rhino.Input.Custom.GetPoint gp = new Rhino.Input.Custom.GetPoint();
        //    gp.DynamicDraw += Gp_CurveDynamicDraw;// this is called everymoment before the user click the left button to draw sphere (with shift key pressed)
        //    gp.MouseMove += Gp_CurveMouseMove;// this is called everymoment before the user click the left button to draw sphere
        //    gp.Get(true);
        //    #endregion

        //    #region compress curve generation
        //    Curve compressCrv = centerCrv.DuplicateCurve();
        //    double compressCrvPara = 0;
        //    compressCrv.ClosestPoint(centerPt, out compressCrvPara);
        //    Curve[] splitCrvs = compressCrv.Split(compressCrvPara);
        //    // compressCrv is the trajectory of compression
        //    compressCrv = splitCrvs[1];
        //    Curve pressCrv = splitCrvs[0];
        //    myDoc.Objects.AddCurve(compressCrv, greenAttribute);
        //    myDoc.Views.Redraw();
        //    #endregion

        //    #region stretch part
        //    Curve stretchCrv = pressCrv.DuplicateCurve();
        //    Rhino.Input.Custom.GetPoint gp_s = new Rhino.Input.Custom.GetPoint();
        //    gp_s.DynamicDraw += Gp_CurveDynamicDrawStretch;
        //    gp_s.MouseMove += Gp_CurveMouseMoveStretch;
        //    gp_s.Get(true);

        //    double stretchCrvPara = 0;
        //    stretchCrv.ClosestPoint(centerPt, out stretchCrvPara);
        //    Curve[] splitCrvsStretch = stretchCrv.Split(stretchCrvPara);
        //    // stretchCrv is the trajectory of stretching
        //    stretchCrv = splitCrvsStretch[1];
        //    Point3d railEnd = splitCrvsStretch[0].PointAtEnd;

        //    double stopperPara;
        //    splitCrvsStretch[0].LengthParameter(splitCrvsStretch[0].GetLength(), out stopperPara);
        //    Plane stopperPln = new Plane(railEnd, splitCrvsStretch[0].TangentAt(stopperPara));// the plane at the start of the curve

        //    Rhino.DocObjects.ObjectAttributes attr = new Rhino.DocObjects.ObjectAttributes();
        //    attr.LayerIndex = 6;
        //    myDoc.Objects.AddCurve(stretchCrv, attr);
        //    myDoc.Views.Redraw();
        //    #endregion

        //    #region generating the outer spring
        //    double K;
        //    Curve sideCrv = springGeneration(centerCrv, surfaceBrep, 8, 1, out K);
        //    RhinoApp.WriteLine("The K value of the generated soring is: " + K);
        //    #endregion

        //    #region delete the part that spring will replaced
        //    // chop the shells into 3 piece
        //    List<Brep> splitsurf = new List<Brep>();
        //    Brep[] firstSplit = surfaceBrep.Split(startPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);

        //    Brep[] test = firstSplit[0].Split(endPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);
        //    if (test != null && test.Length > 0)
        //    {
        //        splitsurf.AddRange(test);
        //        splitsurf.Add(firstSplit[1]);
        //    }
        //    else
        //    {
        //        test = firstSplit[1].Split(endPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);
        //        splitsurf.AddRange(test);
        //        splitsurf.Add(firstSplit[0]);
        //    }

        //    List<Brep> finalPreservedBrepList = new List<Brep>();

        //    foreach (Brep b in splitsurf)
        //    {
        //        Point3d bcenter = b.GetBoundingBox(true).Center;
        //        Vector3d v1 = bcenter - endPln.Origin;
        //        Vector3d v2 = bcenter - startPln.Origin;
        //        if (v1 * v2 > 0)
        //        {
        //            Brep temp = b.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
        //            finalPreservedBrepList.Add(temp);
        //        }

        //    }


        //    foreach (Brep b in finalPreservedBrepList)
        //    {
        //        myDoc.Objects.AddBrep(b, redAttribute);
        //    }

        //    #endregion

        //    #region Generate support structure
        //    Brep prism, stopper;
        //    generateLinearSupport(startPln, endPln, centerCrv, compressCrv, pressCrv, stretchCrv, railEnd, stopperPln, out stopper, out prism);
        //    #endregion

        //    myDoc.Objects.Hide(sufObjId, true);// hide the original shell
        //    myDoc.Views.Redraw();

        //    // Animation part
        //    // Debug: the prismatic joint is not moving along the central curve
        //    List<Brep> top = new List<Brep>();
        //    top.Add(finalPreservedBrepList[1]);
        //    top.Add(prism);
        //    top.Add(stopper);
        //    compressAnimation(centerCrv, compressCrv, sideCrv, top);
        //}

        #endregion

        #region old version of twisting bearing generation
        /// <summary>
        /// This method generates twist deformation structure.
        /// <param name="objRef"></param>
        //public void twistDeform(ObjRef objRef)
        //{
        //    // in the current code we ask the user to select the outer shell of the geometry. In the real case this should be the same
        //    // part as our point cloud selection so user don't need to select themselves.
        //    const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.PolysrfFilter;// filter allows us to constrain the type of objects the user can select
        //    Rhino.DocObjects.ObjRef sufObjRef;
        //    Guid sufObjId = Guid.Empty; // all rhino doc objects has a unique ID. We can always find the object by create an objRef with the id
        //    Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one surface to print", false, filter, out sufObjRef);
        //    if (rc == Rhino.Commands.Result.Success)
        //    {
        //        sufObjId = sufObjRef.ObjectId;
        //    }
        //    ObjRef armOffsetObjRef = new ObjRef(sufObjId);//get the objRef from the GUID

        //    Brep surfaceBrep = armOffsetObjRef.Brep(); // because we know the geometry is Brep, we directly find it from the objRef 

        //    #region generate start and end plane of the curve
        //    Curve centerCrv = objRef.Curve();
        //    Point3d startPt = centerCrv.PointAtStart;
        //    Point3d endPt = centerCrv.PointAtEnd;
        //    double startPara = 0, endPara = 0;
        //    centerCrv.LengthParameter(0, out startPara);
        //    centerCrv.LengthParameter(centerCrv.GetLength(), out endPara);
        //    Plane startPln = new Plane(startPt, centerCrv.TangentAt(startPara));// the plane at the start of the curve
        //    Plane endPln = new Plane(endPt, centerCrv.TangentAt(endPara));// the plane at the end of the curve
        //    Rhino.Geometry.PlaneSurface startPlnSuf = new PlaneSurface(startPln, new Interval(-25, 25), new Interval(-25, 25));// convert the plane object to an actual surface so that we can draw it in the rhino doc
        //    Rhino.Geometry.PlaneSurface endPlnSuf = new PlaneSurface(endPln, new Interval(-25, 25), new Interval(-25, 25));// 15 is a random number. It should actually be the ourter shell's radius or larger.
        //    //myDoc.Objects.AddSurface(startPlnSuf);
        //    //myDoc.Objects.AddSurface(endPlnSuf);
        //    #endregion

        //    #region get midpoint of the medial axis and control points for adjusting the rotation angle
        //    double middlePara = 0;
        //    centerCrv.LengthParameter(centerCrv.GetLength() / 2, out middlePara);
        //    Point3d middlePt = centerCrv.PointAt(middlePara);

        //    Plane anglePln = new Plane(middlePt, centerCrv.TangentAt(middlePara)); // the plane at the center of the central curve
        //    this.anglePlane = anglePln;
        //    this.middlePt = middlePt;
        //    this.angleCtrlPt = middlePt;

        //    Rhino.Geometry.Sphere sphere = new Rhino.Geometry.Sphere(middlePt, 1.5);
        //    Rhino.Geometry.PlaneSurface middlePlnSurf = new PlaneSurface(anglePln, new Interval(-25, 25), new Interval(-25, 25));
        //    //myDoc.Objects.AddSurface(middlePlnSurf);

        //    var attributes = new ObjectAttributes();
        //    attributes.ObjectColor = Color.White;
        //    attributes.ColorSource = ObjectColorSource.ColorFromObject;
        //    //myDoc.Objects.AddSphere(sphere, attributes);
        //    //myDoc.Views.Redraw();

        //    this.centerCrv = centerCrv;
        //    Rhino.Input.Custom.GetPoint gp = new Rhino.Input.Custom.GetPoint();
        //    gp.DynamicDraw += Gp_AnglePlnDynamicDraw;// this is called everymoment before the user click the left button to draw sphere (with shift key pressed)
        //    gp.MouseMove += Gp_AnglePlnMouseMove;// this is called everymoment before the user click the left button to draw sphere
        //    gp.Get(true);

        //    sphere = new Rhino.Geometry.Sphere(this.angleCtrlPt, 1.5);
        //    Guid sId = myDoc.Objects.AddSphere(sphere, attributes);
        //    myDoc.Views.Redraw();

        //    Rhino.Input.Custom.GetPoint gp_sec = new Rhino.Input.Custom.GetPoint();
        //    gp_sec.DynamicDraw += Gp_AngleSelectionDynamicDraw;// this is called everymoment before the user click the left button to draw sphere (with shift key pressed)
        //    gp_sec.MouseMove += Gp_AngleSelectionMouseMove;// this is called everymoment before the user click the left button to draw sphere
        //    gp_sec.Get(true);

        //    myDoc.Objects.Delete(sId,false);
        //    myDoc.Views.Redraw();
        //    #endregion

        //    #region generate the outer spring
        //    double K;
        //    springGeneration(centerCrv, surfaceBrep, 5, 2, out K);
        //    RhinoApp.WriteLine("The K value of the generated soring is: " + K);
        //    #endregion

        //    #region delete the part that spring will replaced
        //    // chop the shells into 3 piece
        //    List<Brep> splitsurf = new List<Brep>();
        //    Brep[] firstSplit = surfaceBrep.Split(startPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);

        //    Brep[] test = firstSplit[0].Split(endPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);
        //    if (test != null && test.Length > 0)
        //    {
        //        splitsurf.AddRange(test);
        //        splitsurf.Add(firstSplit[1]);
        //    }
        //    else
        //    {
        //        test = firstSplit[1].Split(endPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);
        //        splitsurf.AddRange(test);
        //        splitsurf.Add(firstSplit[0]);
        //    }
        //    List<Brep> finalPreservedBrepList = new List<Brep>();
        //    foreach (Brep b in splitsurf)
        //    {
        //        Point3d bcenter = b.GetBoundingBox(true).Center;
        //        Vector3d v1 = bcenter - endPln.Origin;
        //        Vector3d v2 = bcenter - startPln.Origin;
        //        if (v1 * v2 > 0)
        //        {
        //            Brep temp = b.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
        //            finalPreservedBrepList.Add(temp);
        //        }

        //    }


        //    foreach (Brep b in finalPreservedBrepList)
        //    {
        //        myDoc.Objects.AddBrep(b, redAttribute);
        //    }

        //    #endregion

        //    #region generate the central support structure
        //    Brep stopper, cylind;
        //    generateTwistSupport(startPln, endPln, centerCrv, out stopper, out cylind);
        //    #endregion

        //    myDoc.Objects.Hide(sufObjId, true);// hide the original shell
        //    myDoc.Views.Redraw();

        //    // Animation
        //    List<Brep> animList = new List<Brep>();
        //    animList.Add(finalPreservedBrepList[1]);
        //    animList.Add(cylind);
        //    animList.Add(stopper);

        //}
        #endregion

        #region Old version of linear + twist implementation
        /// <summary>
        /// This method generates linear (compress & stretch) + twist deformation structure.
        /// </summary>
        /// <param name="objRef"></param>
        //public void linearTwistDeform(ObjRef objRef)
        //{
        //    // in the current code we ask the user to select the outer shell of the geometry. In the real case this should be the same
        //    // part as our point cloud selection so user don't need to select themselves.
        //    const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.PolysrfFilter;// filter allows us to constrain the type of objects the user can select
        //    Rhino.DocObjects.ObjRef sufObjRef;
        //    Guid sufObjId = Guid.Empty; // all rhino doc objects has a unique ID. We can always find the object by create an objRef with the id
        //    Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one surface to print", false, filter, out sufObjRef);
        //    if (rc == Rhino.Commands.Result.Success)
        //    {
        //        sufObjId = sufObjRef.ObjectId;
        //    }
        //    ObjRef armOffsetObjRef = new ObjRef(sufObjId);//get the objRef from the GUID

        //    Brep surfaceBrep = armOffsetObjRef.Brep(); // because we know the geometry is Brep, we directly find it from the objRef 

        //    // Linear part
        //    #region generate start and end plane of the curve
        //    Curve centerCrv = objRef.Curve();
        //    Point3d startPt = centerCrv.PointAtStart;
        //    Point3d endPt = centerCrv.PointAtEnd;
        //    double startPara = 0, endPara = 0;
        //    centerCrv.LengthParameter(0, out startPara);
        //    centerCrv.LengthParameter(centerCrv.GetLength(), out endPara);
        //    Plane startPln = new Plane(startPt, centerCrv.TangentAt(startPara));// the plane at the start of the curve
        //    Plane endPln = new Plane(endPt, centerCrv.TangentAt(endPara));// the plane at the end of the curve
        //    Rhino.Geometry.PlaneSurface startPlnSuf = new PlaneSurface(startPln, new Interval(-2000, 2000), new Interval(-2000, 2000));// convert the plane object to an actual surface so that we can draw it in the rhino doc
        //    Rhino.Geometry.PlaneSurface endPlnSuf = new PlaneSurface(endPln, new Interval(-2000, 2000), new Interval(-2000, 2000));// 15 is a random number. It should actually be the ourter shell's radius or larger.
        //    //myDoc.Objects.AddSurface(startPlnSuf);
        //    //myDoc.Objects.AddSurface(endPlnSuf);
        //    //myDoc.Views.Redraw();
        //    #endregion

        //    // use Rhino's GetPoint function to dynamically draw the a sphere and find the control point
        //    #region control point
        //    this.centerCrv = centerCrv;
        //    Rhino.Input.Custom.GetPoint gp = new Rhino.Input.Custom.GetPoint();
        //    gp.DynamicDraw += Gp_CurveDynamicDraw;// this is called everymoment before the user click the left button to draw sphere (with shift key pressed)
        //    gp.MouseMove += Gp_CurveMouseMove;// this is called everymoment before the user click the left button to draw sphere
        //    gp.Get(true);
        //    #endregion

        //    #region compress curve generation
        //    Curve compressCrv = centerCrv.DuplicateCurve();
        //    double compressCrvPara = 0;
        //    compressCrv.ClosestPoint(centerPt, out compressCrvPara);
        //    Curve[] splitCrvs = compressCrv.Split(compressCrvPara);
        //    compressCrv = splitCrvs[1];
        //    Curve pressCrv = splitCrvs[0];
        //    //myDoc.Objects.AddCurve(compressCrv, greenAttribute);
        //    //myDoc.Views.Redraw();
        //    #endregion

        //    #region stretch part
        //    Curve stretchCrv = pressCrv.DuplicateCurve();
        //    Rhino.Input.Custom.GetPoint gp_s = new Rhino.Input.Custom.GetPoint();
        //    gp_s.DynamicDraw += Gp_CurveDynamicDrawStretch;
        //    gp_s.MouseMove += Gp_CurveMouseMoveStretch;
        //    gp_s.Get(true);

        //    double stretchCrvPara = 0;
        //    stretchCrv.ClosestPoint(centerPt, out stretchCrvPara);
        //    Curve[] splitCrvsStretch = stretchCrv.Split(stretchCrvPara);
        //    stretchCrv = splitCrvsStretch[1];
        //    Point3d railEnd = splitCrvsStretch[0].PointAtEnd;

        //    double stopperPara;
        //    splitCrvsStretch[0].LengthParameter(splitCrvsStretch[0].GetLength(), out stopperPara);
        //    Plane stopperPln = new Plane(railEnd, splitCrvsStretch[0].TangentAt(stopperPara));// the plane at the start of the curve

        //    Rhino.DocObjects.ObjectAttributes attr = new Rhino.DocObjects.ObjectAttributes();
        //    attr.LayerIndex = 6;
        //   // myDoc.Objects.AddCurve(stretchCrv, attr);
        //   // myDoc.Views.Redraw();
        //    #endregion

        //    #region get midpoint of the medial axis and control points for adjusting the rotation angle
        //    double middlePara = 0;
        //    centerCrv.LengthParameter(centerCrv.GetLength() / 2, out middlePara);
        //    Point3d middlePt = centerCrv.PointAt(middlePara);

        //    Plane anglePln = new Plane(middlePt, centerCrv.TangentAt(middlePara)); // the plane at the center of the central curve
        //    this.anglePlane = anglePln;
        //    this.middlePt = middlePt;
        //    this.angleCtrlPt = middlePt;

        //    Rhino.Geometry.Sphere sphere = new Rhino.Geometry.Sphere(middlePt, 2);
        //    Rhino.Geometry.PlaneSurface middlePlnSurf = new PlaneSurface(anglePln, new Interval(-25, 25), new Interval(-25, 25));
        //    //myDoc.Objects.AddSurface(middlePlnSurf);

        //    var attributes = new ObjectAttributes();
        //    attributes.ObjectColor = Color.White;
        //    attributes.ColorSource = ObjectColorSource.ColorFromObject;
        //    //myDoc.Objects.AddSphere(sphere, attributes);
        //    //myDoc.Views.Redraw();

        //    this.centerCrv = centerCrv;
        //    Rhino.Input.Custom.GetPoint gp_mp = new Rhino.Input.Custom.GetPoint();
        //    gp_mp.DynamicDraw += Gp_AnglePlnDynamicDraw;// this is called everymoment before the user click the left button to draw sphere (with shift key pressed)
        //    gp_mp.MouseMove += Gp_AnglePlnMouseMove;// this is called everymoment before the user click the left button to draw sphere
        //    gp_mp.Get(true);

        //    sphere = new Rhino.Geometry.Sphere(this.angleCtrlPt, 2);
        //    Guid sId = myDoc.Objects.AddSphere(sphere, attributes);
        //    myDoc.Views.Redraw();

        //    Rhino.Input.Custom.GetPoint gp_mp_sec = new Rhino.Input.Custom.GetPoint();
        //    gp_mp_sec.DynamicDraw += Gp_AngleSelectionDynamicDraw;// this is called everymoment before the user click the left button to draw sphere (with shift key pressed)
        //    gp_mp_sec.MouseMove += Gp_AngleSelectionMouseMove;// this is called everymoment before the user click the left button to draw sphere
        //    gp_mp_sec.Get(true);

        //    myDoc.Objects.Delete(sId, false);
        //    myDoc.Views.Redraw();
        //    #endregion

        //    #region generating the outer spring
        //    double K;
        //    springGeneration(centerCrv, surfaceBrep, 3, 4, out K);
        //    RhinoApp.WriteLine("The K value of the generated soring is: " + K);
        //    #endregion

        //    #region delete the part that spring will replaced
        //    // chop the shells into 3 piece
        //    List<Brep> splitsurf = new List<Brep>();
        //    Brep[] firstSplit = surfaceBrep.Split(startPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);

        //    Brep[] test = firstSplit[0].Split(endPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);
        //    if (test != null && test.Length > 0)
        //    {
        //        splitsurf.AddRange(test);
        //        splitsurf.Add(firstSplit[1]);
        //    }
        //    else
        //    {
        //        test = firstSplit[1].Split(endPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);
        //        splitsurf.AddRange(test);
        //        splitsurf.Add(firstSplit[0]);
        //    }
        //    List<Brep> finalPreservedBrepList = new List<Brep>();
        //    foreach (Brep b in splitsurf)
        //    {
        //        Point3d bcenter = b.GetBoundingBox(true).Center;
        //        Vector3d v1 = bcenter - endPln.Origin;
        //        Vector3d v2 = bcenter - startPln.Origin;
        //        if (v1 * v2 > 0)
        //        {
        //            Brep temp = b.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
        //            finalPreservedBrepList.Add(temp);
        //        }

        //    }

        //    foreach (Brep b in finalPreservedBrepList)
        //    {
        //        myDoc.Objects.AddBrep(b, redAttribute);
        //    }

        //    #endregion

        //    #region Generate support structure
        //    generateLinearTwistSupport(startPln, endPln, centerCrv, compressCrv, pressCrv, stretchCrv, railEnd, stopperPln);

        //    #endregion

        //    myDoc.Objects.Hide(sufObjId, true);// hide the original shell
        //    myDoc.Views.Redraw();


        //}

        #endregion
        #endregion

        /// <summary>
        /// Simulation of linear deformation
        /// </summary>
        /// <param name="longCurve"></param>
        /// <param name="shortCurve"></param>
        /// <param name="sideCrv"></param>
        /// <param name="top"></param>
        public void compressAnimation(Curve longCurve, Curve  shortCurve, Curve sideCrv, List<Brep> top)
        {
            double[] dividedParameter = shortCurve.DivideByCount(10, true);
            double[] longCrvDividedPar = longCurve.DivideByCount(10, true);
            double targetLength = longCurve.Split(longCrvDividedPar[1])[0].GetLength();

            List<Point3d> ptOnCrv = new List<Point3d>();
            foreach (double d in dividedParameter)
            {
                ptOnCrv.Add(shortCurve.PointAt(d));
            }
            List<Line> lines = new List<Line>();
            var Goals = new List<KangarooSolver.IGoal>();
            for (int i = 0; i < ptOnCrv.Count - 1; i++)
            {
                Line l = new Line(ptOnCrv[i], ptOnCrv[i + 1]);
                lines.Add(l);
                KangarooSolver.GoalObject springGoal = new KangarooSolver.Goals.Spring(l, targetLength, 10000);
                Goals.Add(springGoal);

            }

            var goal_OnCurve = new KangarooSolver.Goals.OnCurve(ptOnCrv, longCurve, 10000);
            var goal_Anchor = new KangarooSolver.Goals.Anchor(longCurve.PointAtStart, 100000);
            Goals.Add(goal_OnCurve);
            Goals.Add(goal_Anchor);

            var PS = new KangarooSolver.PhysicalSystem();

            foreach (var g in Goals)
                PS.AssignPIndex(g, 0.01);

            int counter = 0;
            double threshold = 1e-9;
            Guid id;
            List<Guid> topId = new List<Guid>();
            Curve joined;
            Curve spiral;
            double endPara;
            List<Brep> newtop = new List<Brep>();
            Guid sweepId;

            SweepOneRail sweep = new SweepOneRail();
            sweep.SetToRoadlikeTop();
            Brep sweepBrep;
            List<Brep> sweepBrepList = new List<Brep>();

            do
            {
                //PS.SimpleStep(Goals);
                //PS.Step(Goals, true, threshold); // The step will iterate until either reaching 15ms or the energy threshold
                PS.MomentumStep(Goals, 0.98, counter);
                counter++;
                List<Object> output = PS.GetOutput(Goals);
                List<Curve> cvs = new List<Curve>();
                for (int i = 0; i < output.Count; i++)
                {
                    if (output[i] != null)
                    {
                        Line l = (Line)Convert.ChangeType(output[i], typeof(Line));
                        cvs.Add(l.ToNurbsCurve());
                    }

                }
                joined = Curve.JoinCurves(cvs)[0];
                joined.LengthParameter(joined.GetLength(), out endPara);
                spiral = NurbsCurve.CreateSpiral(joined, 0, endPara, new Point3d(0, 0, 0), 5, 0, 15, 15, 5);
                //sweepCrv = spiral;
                //drawCircle = new Circle(new Plane(joined.PointAtEnd, joined.TangentAtEnd), joined.PointAtEnd, 20);


                //sweepBrep = sweep.PerformSweep(spiral, sideCrv)[0];
                //sweepBrepList.Add(sweepBrep);

                Vector3d vec = new Vector3d(joined.PointAtStart - longCurve.PointAtStart);
                Transform trans = Transform.Translation(vec);

                newtop.Clear();
                topId.Clear();
                foreach (Brep b in top)
                {
                    Brep newb = b.DuplicateBrep();
                    newb.Transform(trans);
                    newtop.Add(newb);
                    Guid newbid = myDoc.Objects.Add(newb);
                    topId.Add(newbid);
                }
                id = myDoc.Objects.AddCurve(spiral);
                //sweepId = myDoc.Objects.AddBrep(sweepBrep);
                myDoc.Views.Redraw();
                System.Threading.Thread.Sleep(10);
                myDoc.Objects.Delete(id, true);
                myDoc.Objects.Delete(topId, true);
                //myDoc.Objects.Delete(sweepId, true);
            } while (counter < 30);

            //myDoc.Objects.AddBrep(newtop);
            myDoc.Objects.AddCurve(spiral);
            //myDoc.Objects.AddBrep(sweepBrep);
            myDoc.Views.Redraw();
        }


        /// <summary>
        /// This method generates linear (compress & stretch) + bend deformation structure.
        /// </summary>
        /// <param name="objRef"></param>
        public void linearBendDeform(ObjRef objRef)
        {
            // in the current code we ask the user to select the outer shell of the geometry. In the real case this should be the same
            // part as our point cloud selection so user don't need to select themselves.
            const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.PolysrfFilter;// filter allows us to constrain the type of objects the user can select
            Rhino.DocObjects.ObjRef sufObjRef;
            Guid sufObjId = Guid.Empty; // all rhino doc objects has a unique ID. We can always find the object by create an objRef with the id
            Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one surface to print", false, filter, out sufObjRef);
            if (rc == Rhino.Commands.Result.Success)
            {
                sufObjId = sufObjRef.ObjectId;
            }
            ObjRef armOffsetObjRef = new ObjRef(sufObjId);//get the objRef from the GUID

            Brep surfaceBrep = armOffsetObjRef.Brep(); // because we know the geometry is Brep, we directly find it from the objRef 

            // linear part
            #region generate start and end plane of the curve
            Curve centerCrv = objRef.Curve();
            Point3d startPt = centerCrv.PointAtStart;
            Point3d endPt = centerCrv.PointAtEnd;
            double startPara = 0, endPara = 0;
            centerCrv.LengthParameter(0, out startPara);
            centerCrv.LengthParameter(centerCrv.GetLength(), out endPara);
            Plane startPln = new Plane(startPt, centerCrv.TangentAt(startPara));// the plane at the start of the curve
            Plane endPln = new Plane(endPt, centerCrv.TangentAt(endPara));// the plane at the end of the curve
            Rhino.Geometry.PlaneSurface startPlnSuf = new PlaneSurface(startPln, new Interval(-25, 25), new Interval(-25, 25));// convert the plane object to an actual surface so that we can draw it in the rhino doc
            Rhino.Geometry.PlaneSurface endPlnSuf = new PlaneSurface(endPln, new Interval(-25, 25), new Interval(-25, 25));// 15 is a random number. It should actually be the ourter shell's radius or larger.
            //myDoc.Objects.AddSurface(startPlnSuf);
            //myDoc.Objects.AddSurface(endPlnSuf);
            //myDoc.Views.Redraw();
            #endregion

            // use Rhino's GetPoint function to dynamically draw the a sphere and find the control point
            #region control point
            this.centerCrv = centerCrv;
            Rhino.Input.Custom.GetPoint gp = new Rhino.Input.Custom.GetPoint();
            gp.DynamicDraw += Gp_CurveDynamicDraw;// this is called everymoment before the user click the left button to draw sphere (with shift key pressed)
            gp.MouseMove += Gp_CurveMouseMove;// this is called everymoment before the user click the left button to draw sphere
            gp.Get(true);
            #endregion

            #region compress curve generation
            Curve compressCrv = centerCrv.DuplicateCurve();
            double compressCrvPara = 0;
            compressCrv.ClosestPoint(centerPt, out compressCrvPara);
            Curve[] splitCrvs = compressCrv.Split(compressCrvPara);
            compressCrv = splitCrvs[1];
            Curve pressCrv = splitCrvs[0];
            //myDoc.Objects.AddCurve(compressCrv, greenAttribute);
            //myDoc.Views.Redraw();
            #endregion

            #region stretch part
            Curve stretchCrv = pressCrv.DuplicateCurve();
            Rhino.Input.Custom.GetPoint gp_s = new Rhino.Input.Custom.GetPoint();
            gp_s.DynamicDraw += Gp_CurveDynamicDrawStretch;
            gp_s.MouseMove += Gp_CurveMouseMoveStretch;
            gp_s.Get(true);

            double stretchCrvPara = 0;
            stretchCrv.ClosestPoint(centerPt, out stretchCrvPara);
            Curve[] splitCrvsStretch = stretchCrv.Split(stretchCrvPara);
            stretchCrv = splitCrvsStretch[1];
            Point3d railEnd = splitCrvsStretch[0].PointAtEnd;

            double stopperPara;
            splitCrvsStretch[0].LengthParameter(splitCrvsStretch[0].GetLength(), out stopperPara);
            Plane stopperPln = new Plane(railEnd, splitCrvsStretch[0].TangentAt(stopperPara));// the plane at the start of the curve

            Rhino.DocObjects.ObjectAttributes attr = new Rhino.DocObjects.ObjectAttributes();
            attr.LayerIndex = 6;
            //myDoc.Objects.AddCurve(stretchCrv, attr);
            //myDoc.Views.Redraw();
            #endregion

            #region delete the part that spring will replaced
            // chop the shells into 3 piece
            List<Brep> splitsurf = new List<Brep>();
            Brep[] firstSplit = surfaceBrep.Split(startPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);

            Brep[] test = firstSplit[0].Split(endPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);
            if (test != null && test.Length > 0)
            {
                splitsurf.AddRange(test);
                splitsurf.Add(firstSplit[1]);
            }
            else
            {
                test = firstSplit[1].Split(endPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);
                splitsurf.AddRange(test);
                splitsurf.Add(firstSplit[0]);
            }
            List<Brep> finalPreservedBrepList = new List<Brep>();
            foreach (Brep b in splitsurf)
            {
                Point3d bcenter = b.GetBoundingBox(true).Center;
                Vector3d v1 = bcenter - endPln.Origin;
                Vector3d v2 = bcenter - startPln.Origin;
                if (v1 * v2 > 0)
                {
                    Brep temp = b.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
                    finalPreservedBrepList.Add(temp);
                }

            }


            foreach (Brep b in finalPreservedBrepList)
            {
                myDoc.Objects.AddBrep(b, redAttribute);
            }

            #endregion

            // Bend part

            #region get the anchor control points for deciding the bending direction
            this.centerCrv = centerCrv;
            var attributes = new ObjectAttributes();
            attributes.ObjectColor = Color.White;
            attributes.ColorSource = ObjectColorSource.ColorFromObject;

            isBendLinear = true;
            // listen to the user's selected sphere: either at the start or at the end sphere
            Rhino.Input.Custom.GetPoint gp_pt = new Rhino.Input.Custom.GetPoint();
            gp_pt.DynamicDraw += Gp_sphereSelDynamicDraw;
            gp_pt.MouseMove += Gp_sphereSelMouseMove;
            gp_pt.Get(true);

            if (distanceBtwTwoPts(this.bendPtSel, this.centerCrv.PointAtStart) <= 1.5)
            {
                // selected the start sphere
                Rhino.Geometry.Sphere startSphere = new Rhino.Geometry.Sphere(startPt, 1.5);
                Guid sphId = myDoc.Objects.AddSphere(startSphere, attributes);
                Plane bendAnglePln = new Plane(startPt, centerCrv.TangentAt(startPara)); // the plane at the starting plane of the central curve
                this.bendPlane = bendAnglePln;

                this.bendCtrlPt = startPt;
                Rhino.Geometry.Circle dirCircle = new Rhino.Geometry.Circle(bendAnglePln, startPt, 20);
                this.angleCircle = dirCircle;

                Guid sId = myDoc.Objects.AddCircle(dirCircle);
                myDoc.Views.Redraw();

                // start the command 
                is_BendStart = true;

                if (is_BendStart)
                {
                    // if the user doesn't press the 'Enter' key it will continue to accept the next direction
                    bend_info tempBendInfo = new bend_info();

                    // step 1: decide the direction
                    Rhino.Input.Custom.GetPoint gp_dir = new Rhino.Input.Custom.GetPoint();
                    //gp.MouseDown += Gp_BendAngleSelMouseDown; // only when the user click it starts to change the angle;
                    gp_dir.MouseMove += Gp_BendAngleSelMouseMove; // Every move compute the changed angle
                    gp_dir.DynamicDraw += Gp_BendAngelSelDynamicDraw; // Draw the refreshed ball in the scenes
                    gp_dir.Get(true);
                    Point3d p = this.angleCircle.ClosestPoint(gp_dir.Point());
                    tempBendInfo.dir = (Vector3d)(p - startPt);

                    // step 2: decide the bending strength
                    Rhino.Input.Custom.GetPoint gp_bs = new Rhino.Input.Custom.GetPoint();
                    gp_bs.MouseMove += Gp_BendStrengthSelMouseMove;
                    gp_bs.DynamicDraw += Gp_BendStrengthSelDynamicDraw;
                    gp_bs.Get(true);
                    Point3d pp = gp_bs.Point();
                    tempBendInfo.strength = Math.Sqrt(Math.Pow(pp.X - p.X, 2) + Math.Pow(pp.Y - p.Y, 2) + Math.Pow(pp.Z - p.Z, 2));  // check if this is discrete or continuous

                    // add current bending information in the bending list
                    //RhinoApp.WriteLine("direction: (" + tempBendInfo.dir.X + "," + tempBendInfo.dir.Y + "," + tempBendInfo.dir.Z + ")");
                    //RhinoApp.WriteLine("strength: " + tempBendInfo.strength);
                    bendInfoList.Add(tempBendInfo);
                }
                myDoc.Objects.Delete(sId, false);
                myDoc.Objects.Delete(sphId, false);
                myDoc.Views.Redraw();

                #region generate the outer spring
                double K;
                springGeneration(centerCrv, surfaceBrep, 5, 5, out K);
                RhinoApp.WriteLine("The K value of the generated soring is: " + K);
                #endregion

                // generate the bend support structure
                generateLinearBendSupport(startPln, endPln, centerCrv, compressCrv, pressCrv, stretchCrv, railEnd, stopperPln, bendInfoList, startPt, endPt, true);

                // Clear the bend information list
                bendInfoList.Clear();

            }
            else if (distanceBtwTwoPts(this.bendPtSel, this.centerCrv.PointAtEnd) <= 1.5)
            {
                // selected the end sphere
                Rhino.Geometry.Sphere endSphere = new Rhino.Geometry.Sphere(endPt, 1.5);
                Guid sphId = myDoc.Objects.AddSphere(endSphere, attributes);
                Plane bendAnglePln = new Plane(endPt, centerCrv.TangentAt(endPara)); // the plane at the end plane of the central curve
                this.bendPlane = bendAnglePln;

                this.bendCtrlPt = endPt;
                Rhino.Geometry.Circle dirCircle = new Rhino.Geometry.Circle(bendAnglePln, endPt, 20);
                this.angleCircle = dirCircle;

                Guid sId = myDoc.Objects.AddCircle(dirCircle);
                myDoc.Views.Redraw();

                // start the command 
                is_BendStart = true;

                if (is_BendStart)
                {
                    // if the user doesn't press the 'Enter' key it will continue to accept the next direction
                    bend_info tempBendInfo = new bend_info();

                    // step 1: decide the direction
                    Rhino.Input.Custom.GetPoint gp_dir = new Rhino.Input.Custom.GetPoint();
                    //gp.MouseDown += Gp_BendAngleSelMouseDown; // only when the user click it starts to change the angle;
                    gp_dir.MouseMove += Gp_BendAngleSelMouseMove; // Every move compute the changed angle
                    gp_dir.DynamicDraw += Gp_BendAngelSelDynamicDraw; // Draw the refreshed ball in the scenes
                    gp_dir.Get(true);
                    Point3d p = this.angleCircle.ClosestPoint(gp_dir.Point());
                    tempBendInfo.dir = (Vector3d)(p - endPt);

                    // step 2: decide the bending strength
                    Rhino.Input.Custom.GetPoint gp_bs = new Rhino.Input.Custom.GetPoint();
                    gp_bs.MouseMove += Gp_BendStrengthSelMouseMove;
                    gp_bs.DynamicDraw += Gp_BendStrengthSelDynamicDraw;
                    gp_bs.Get(true);
                    Point3d pp = gp_bs.Point();
                    tempBendInfo.strength = Math.Sqrt(Math.Pow(pp.X - p.X, 2) + Math.Pow(pp.Y - p.Y, 2) + Math.Pow(pp.Z - p.Z, 2));  // check if this is discrete or continuous

                    // add current bending information in the bending list
                    //RhinoApp.WriteLine("direction: (" + tempBendInfo.dir.X + "," + tempBendInfo.dir.Y + "," + tempBendInfo.dir.Z + ")");
                    //RhinoApp.WriteLine("strength: " + tempBendInfo.strength);
                    bendInfoList.Add(tempBendInfo);
                }
                myDoc.Objects.Delete(sId, false);
                myDoc.Objects.Delete(sphId, false);
                myDoc.Views.Redraw();

                #region generate the outer spring
                double K;
                springGeneration(centerCrv, surfaceBrep, 5, 5, out K);
                RhinoApp.WriteLine("The K value of the generated soring is: " + K);
                #endregion

                // generate the bend support structure
                generateLinearBendSupport(startPln, endPln, centerCrv, compressCrv, pressCrv, stretchCrv, railEnd, stopperPln, bendInfoList, startPt, endPt, false);

                // Clear the bend information list
                bendInfoList.Clear();
            }
            isBendLinear = false;
            #endregion

            myDoc.Objects.Hide(sufObjId, true);// hide the original shell
            myDoc.Views.Redraw();

        }

        /// <summary>
        /// This method generates twist + bend deformation structure.
        /// </summary>
        /// <param name="objRef"></param>
        public void twistBendDeform(ObjRef objRef)
        {
            // in the current code we ask the user to select the outer shell of the geometry. In the real case this should be the same
            // part as our point cloud selection so user don't need to select themselves.
            const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.PolysrfFilter;// filter allows us to constrain the type of objects the user can select
            Rhino.DocObjects.ObjRef sufObjRef;
            Guid sufObjId = Guid.Empty; // all rhino doc objects has a unique ID. We can always find the object by create an objRef with the id
            Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one surface to print", false, filter, out sufObjRef);
            if (rc == Rhino.Commands.Result.Success)
            {
                sufObjId = sufObjRef.ObjectId;
            }
            ObjRef armOffsetObjRef = new ObjRef(sufObjId);//get the objRef from the GUID

            Brep surfaceBrep = armOffsetObjRef.Brep(); // because we know the geometry is Brep, we directly find it from the objRef 


            // twist part
            #region generate start and end plane of the curve
            Curve centerCrv = objRef.Curve();
            Point3d startPt = centerCrv.PointAtStart;
            Point3d endPt = centerCrv.PointAtEnd;
            double startPara = 0, endPara = 0;
            centerCrv.LengthParameter(0, out startPara);
            centerCrv.LengthParameter(centerCrv.GetLength(), out endPara);
            Plane startPln = new Plane(startPt, centerCrv.TangentAt(startPara));// the plane at the start of the curve
            Plane endPln = new Plane(endPt, centerCrv.TangentAt(endPara));// the plane at the end of the curve
            Rhino.Geometry.PlaneSurface startPlnSuf = new PlaneSurface(startPln, new Interval(-25, 25), new Interval(-25, 25));// convert the plane object to an actual surface so that we can draw it in the rhino doc
            Rhino.Geometry.PlaneSurface endPlnSuf = new PlaneSurface(endPln, new Interval(-25, 25), new Interval(-25, 25));// 15 is a random number. It should actually be the ourter shell's radius or larger.
            //myDoc.Objects.AddSurface(startPlnSuf);
            //myDoc.Objects.AddSurface(endPlnSuf);
            #endregion

            #region get midpoint of the medial axis and control points for adjusting the rotation angle
            double middlePara = 0;
            centerCrv.LengthParameter(centerCrv.GetLength() / 2, out middlePara);
            Point3d middlePt = centerCrv.PointAt(middlePara);

            Plane anglePln = new Plane(middlePt, centerCrv.TangentAt(middlePara)); // the plane at the center of the central curve
            this.anglePlane = anglePln;
            this.middlePt = middlePt;
            this.angleCtrlPt = middlePt;

            Rhino.Geometry.Sphere sphere = new Rhino.Geometry.Sphere(middlePt, 2);
            Rhino.Geometry.PlaneSurface middlePlnSurf = new PlaneSurface(anglePln, new Interval(-25, 25), new Interval(-25, 25));
            //myDoc.Objects.AddSurface(middlePlnSurf);

            var attributes = new ObjectAttributes();
            attributes.ObjectColor = Color.White;
            attributes.ColorSource = ObjectColorSource.ColorFromObject;
            //myDoc.Objects.AddSphere(sphere, attributes);
            //myDoc.Views.Redraw();

            this.centerCrv = centerCrv;
            Rhino.Input.Custom.GetPoint gp = new Rhino.Input.Custom.GetPoint();
            gp.DynamicDraw += Gp_AnglePlnDynamicDraw;// this is called everymoment before the user click the left button to draw sphere (with shift key pressed)
            gp.MouseMove += Gp_AnglePlnMouseMove;// this is called everymoment before the user click the left button to draw sphere
            gp.Get(true);

            sphere = new Rhino.Geometry.Sphere(this.angleCtrlPt, 2);
            Guid sId = myDoc.Objects.AddSphere(sphere, attributes);
            myDoc.Views.Redraw();

            Rhino.Input.Custom.GetPoint gp_sec = new Rhino.Input.Custom.GetPoint();
            gp_sec.DynamicDraw += Gp_AngleSelectionDynamicDraw;// this is called everymoment before the user click the left button to draw sphere (with shift key pressed)
            gp_sec.MouseMove += Gp_AngleSelectionMouseMove;// this is called everymoment before the user click the left button to draw sphere
            gp_sec.Get(true);

            myDoc.Objects.Delete(sId, false);
            myDoc.Views.Redraw();
            #endregion

            #region delete the part that spring will replaced
            // chop the shells into 3 piece
            List<Brep> splitsurf = new List<Brep>();
            Brep[] firstSplit = surfaceBrep.Split(startPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);

            Brep[] test = firstSplit[0].Split(endPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);
            if (test != null && test.Length > 0)
            {
                splitsurf.AddRange(test);
                splitsurf.Add(firstSplit[1]);
            }
            else
            {
                test = firstSplit[1].Split(endPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);
                splitsurf.AddRange(test);
                splitsurf.Add(firstSplit[0]);
            }
            List<Brep> finalPreservedBrepList = new List<Brep>();
            foreach (Brep b in splitsurf)
            {
                Point3d bcenter = b.GetBoundingBox(true).Center;
                Vector3d v1 = bcenter - endPln.Origin;
                Vector3d v2 = bcenter - startPln.Origin;
                if (v1 * v2 > 0)
                {
                    Brep temp = b.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
                    finalPreservedBrepList.Add(temp);
                }

            }


            foreach (Brep b in finalPreservedBrepList)
            {
                myDoc.Objects.AddBrep(b, redAttribute);
            }

            #endregion

            // bend part
            #region get the anchor control points for deciding the bending direction
            this.centerCrv = centerCrv;
            attributes.ObjectColor = Color.White;
            attributes.ColorSource = ObjectColorSource.ColorFromObject;

            // listen to the user's selected sphere: either at the start or at the end sphere
            Rhino.Input.Custom.GetPoint gp_pt = new Rhino.Input.Custom.GetPoint();
            gp_pt.DynamicDraw += Gp_sphereSelDynamicDraw;
            gp_pt.MouseMove += Gp_sphereSelMouseMove;
            gp_pt.Get(true);

            if (distanceBtwTwoPts(this.bendPtSel, this.centerCrv.PointAtStart) <= 1.5)
            {
                // selected the start sphere
                Rhino.Geometry.Sphere startSphere = new Rhino.Geometry.Sphere(startPt, 1.5);
                Guid sphId = myDoc.Objects.AddSphere(startSphere, attributes);
                Plane bendAnglePln = new Plane(startPt, centerCrv.TangentAt(startPara)); // the plane at the starting plane of the central curve
                this.bendPlane = bendAnglePln;

                this.bendCtrlPt = startPt;
                Rhino.Geometry.Circle dirCircle = new Rhino.Geometry.Circle(bendAnglePln, startPt, 20);
                this.angleCircle = dirCircle;

                Guid ssId = myDoc.Objects.AddCircle(dirCircle);
                myDoc.Views.Redraw();

                // start the command 
                is_BendStart = true;

                if (is_BendStart)
                {
                    // if the user doesn't press the 'Enter' key it will continue to accept the next direction
                    bend_info tempBendInfo = new bend_info();

                    // step 1: decide the direction
                    Rhino.Input.Custom.GetPoint gp_dir = new Rhino.Input.Custom.GetPoint();
                    //gp.MouseDown += Gp_BendAngleSelMouseDown; // only when the user click it starts to change the angle;
                    gp_dir.MouseMove += Gp_BendAngleSelMouseMove; // Every move compute the changed angle
                    gp_dir.DynamicDraw += Gp_BendAngelSelDynamicDraw; // Draw the refreshed ball in the scenes
                    gp_dir.Get(true);
                    Point3d p = this.angleCircle.ClosestPoint(gp_dir.Point());
                    tempBendInfo.dir = (Vector3d)(p - startPt);

                    // step 2: decide the bending strength
                    Rhino.Input.Custom.GetPoint gp_bs = new Rhino.Input.Custom.GetPoint();
                    gp_bs.MouseMove += Gp_BendStrengthSelMouseMove;
                    gp_bs.DynamicDraw += Gp_BendStrengthSelDynamicDraw;
                    gp_bs.Get(true);
                    Point3d pp = gp_bs.Point();
                    tempBendInfo.strength = Math.Sqrt(Math.Pow(pp.X - p.X, 2) + Math.Pow(pp.Y - p.Y, 2) + Math.Pow(pp.Z - p.Z, 2));  // check if this is discrete or continuous

                    // add current bending information in the bending list
                    //RhinoApp.WriteLine("direction: (" + tempBendInfo.dir.X + "," + tempBendInfo.dir.Y + "," + tempBendInfo.dir.Z + ")");
                    //RhinoApp.WriteLine("strength: " + tempBendInfo.strength);
                    bendInfoList.Add(tempBendInfo);
                }

                myDoc.Objects.Delete(ssId, false);
                myDoc.Objects.Delete(sphId, false);
                myDoc.Views.Redraw();

                #region generate the outer spring
                double K;
                springGeneration(centerCrv, surfaceBrep, 3, 6, out K);
                RhinoApp.WriteLine("The K value of the generated soring is: " + K);
                #endregion

                // generate the bend support structure
                generateTwistBendSupport(startPln, endPln, this.centerCrv, bendInfoList, startPt, endPt, true);

                // Clear the bend information list
                bendInfoList.Clear();

            }
            else if (distanceBtwTwoPts(this.bendPtSel, this.centerCrv.PointAtEnd) <= 1.5)
            {
                // selected the end sphere
                Rhino.Geometry.Sphere endSphere = new Rhino.Geometry.Sphere(endPt, 1.5);
                Guid sphId = myDoc.Objects.AddSphere(endSphere, attributes);
                Plane bendAnglePln = new Plane(endPt, centerCrv.TangentAt(endPara)); // the plane at the end plane of the central curve
                this.bendPlane = bendAnglePln;

                this.bendCtrlPt = endPt;
                Rhino.Geometry.Circle dirCircle = new Rhino.Geometry.Circle(bendAnglePln, endPt, 20);
                this.angleCircle = dirCircle;

                Guid ssId = myDoc.Objects.AddCircle(dirCircle);
                myDoc.Views.Redraw();

                // start the command 
                is_BendStart = true;

                if (is_BendStart)
                {
                    // if the user doesn't press the 'Enter' key it will continue to accept the next direction
                    bend_info tempBendInfo = new bend_info();

                    // step 1: decide the direction
                    Rhino.Input.Custom.GetPoint gp_dir = new Rhino.Input.Custom.GetPoint();
                    //gp.MouseDown += Gp_BendAngleSelMouseDown; // only when the user click it starts to change the angle;
                    gp_dir.MouseMove += Gp_BendAngleSelMouseMove; // Every move compute the changed angle
                    gp_dir.DynamicDraw += Gp_BendAngelSelDynamicDraw; // Draw the refreshed ball in the scenes
                    gp_dir.Get(true);
                    Point3d p = this.angleCircle.ClosestPoint(gp_dir.Point());
                    tempBendInfo.dir = (Vector3d)(p - endPt);

                    // step 2: decide the bending strength
                    Rhino.Input.Custom.GetPoint gp_bs = new Rhino.Input.Custom.GetPoint();
                    gp_bs.MouseMove += Gp_BendStrengthSelMouseMove;
                    gp_bs.DynamicDraw += Gp_BendStrengthSelDynamicDraw;
                    gp_bs.Get(true);
                    Point3d pp = gp_bs.Point();
                    tempBendInfo.strength = Math.Sqrt(Math.Pow(pp.X - p.X, 2) + Math.Pow(pp.Y - p.Y, 2) + Math.Pow(pp.Z - p.Z, 2));  // check if this is discrete or continuous

                    // add current bending information in the bending list
                    // RhinoApp.WriteLine("direction: (" + tempBendInfo.dir.X + "," + tempBendInfo.dir.Y + "," + tempBendInfo.dir.Z + ")");
                    // RhinoApp.WriteLine("strength: " + tempBendInfo.strength);
                    bendInfoList.Add(tempBendInfo);
                }

                myDoc.Objects.Delete(ssId, false);
                myDoc.Objects.Delete(sphId, false);
                myDoc.Views.Redraw();

                #region generate the outer spring
                double K;
                springGeneration(centerCrv, surfaceBrep, 3, 6, out K);
                RhinoApp.WriteLine("The K value of the generated soring is: " + K);
                #endregion

                // generate the bend support structure
                generateTwistBendSupport(startPln, endPln, this.centerCrv, bendInfoList, startPt, endPt, false);

                // Clear the bend information list
                bendInfoList.Clear();
            }

            #endregion


            myDoc.Objects.Hide(sufObjId, true);// hide the original shell
            myDoc.Views.Redraw();

        }

        /// <summary>
        /// This method generates linear (compress & stretch) + bend + twist deformation structure.
        /// </summary>
        /// <param name="objRef"></param>
        public void allDeform(ObjRef objRef)
        {
            // in the current code we ask the user to select the outer shell of the geometry. In the real case this should be the same
            // part as our point cloud selection so user don't need to select themselves.
            const Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.PolysrfFilter;// filter allows us to constrain the type of objects the user can select
            Rhino.DocObjects.ObjRef sufObjRef;
            Guid sufObjId = Guid.Empty; // all rhino doc objects has a unique ID. We can always find the object by create an objRef with the id
            Rhino.Commands.Result rc = Rhino.Input.RhinoGet.GetOneObject("Select one surface to print", false, filter, out sufObjRef);
            if (rc == Rhino.Commands.Result.Success)
            {
                sufObjId = sufObjRef.ObjectId;
            }
            ObjRef armOffsetObjRef = new ObjRef(sufObjId);//get the objRef from the GUID

            Brep surfaceBrep = armOffsetObjRef.Brep(); // because we know the geometry is Brep, we directly find it from the objRef 

            #region generate start and end plane of the curve
            Curve centerCrv = objRef.Curve();
            Point3d startPt = centerCrv.PointAtStart;
            Point3d endPt = centerCrv.PointAtEnd;
            double startPara = 0, endPara = 0;
            centerCrv.LengthParameter(0, out startPara);
            centerCrv.LengthParameter(centerCrv.GetLength(), out endPara);
            Plane startPln = new Plane(startPt, centerCrv.TangentAt(startPara));// the plane at the start of the curve
            Plane endPln = new Plane(endPt, centerCrv.TangentAt(endPara));// the plane at the end of the curve
            Rhino.Geometry.PlaneSurface startPlnSuf = new PlaneSurface(startPln, new Interval(-25, 25), new Interval(-25, 25));// convert the plane object to an actual surface so that we can draw it in the rhino doc
            Rhino.Geometry.PlaneSurface endPlnSuf = new PlaneSurface(endPln, new Interval(-25, 25), new Interval(-25, 25));// 15 is a random number. It should actually be the ourter shell's radius or larger.
            //myDoc.Objects.AddSurface(startPlnSuf);
            //myDoc.Objects.AddSurface(endPlnSuf);
            #endregion

            #region generate the outer spring
            double K;
            springGeneration(centerCrv, surfaceBrep, 3, 7, out K);
            RhinoApp.WriteLine("The K value of the generated soring is: " + K);
            #endregion

            #region delete the part that spring will replaced
            // chop the shells into 3 piece
            List<Brep> splitsurf = new List<Brep>();
            Brep[] firstSplit = surfaceBrep.Split(startPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);

            Brep[] test = firstSplit[0].Split(endPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);
            if (test != null && test.Length > 0)
            {
                splitsurf.AddRange(test);
                splitsurf.Add(firstSplit[1]);
            }
            else
            {
                test = firstSplit[1].Split(endPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);
                splitsurf.AddRange(test);
                splitsurf.Add(firstSplit[0]);
            }
            List<Brep> finalPreservedBrepList = new List<Brep>();
            foreach (Brep b in splitsurf)
            {
                Point3d bcenter = b.GetBoundingBox(true).Center;
                Vector3d v1 = bcenter - endPln.Origin;
                Vector3d v2 = bcenter - startPln.Origin;
                if (v1 * v2 > 0)
                {
                    Brep temp = b.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
                    finalPreservedBrepList.Add(temp);
                }

            }


            foreach (Brep b in finalPreservedBrepList)
            {
                myDoc.Objects.AddBrep(b, redAttribute);
            }

            #endregion

            myDoc.Objects.Hide(sufObjId, true);// hide the original shell
            myDoc.Views.Redraw();
        }

       
        private void generateLinearBendSupport(Plane startSuf, Plane endSuf, Curve centerCrv, Curve compressCrv, Curve pressCrv, Curve stretchCrv, Point3d railEnd, Plane stopperPlnOrig, List<bend_info> bendInfoList, Point3d startPt, Point3d endPt, bool isStart)
        {
            foreach (bend_info b in bendInfoList)
            {
                var attributes = new ObjectAttributes();
                attributes.ObjectColor = Color.Yellow;
                attributes.ColorSource = ObjectColorSource.ColorFromObject;

                Vector3d newDir = b.dir;
                Point3d p_end = endPt + newDir;
                p_end = endSuf.ClosestPoint(p_end);
                newDir = (Vector3d)(p_end - endPt);

                Vector3d normal_dir = newDir;
                Vector3d axis= -endSuf.Normal;
                
                #region linear part
                // create sweep function
                var sweep = new Rhino.Geometry.SweepOneRail();
                sweep.AngleToleranceRadians = myDoc.ModelAngleToleranceRadians;
                sweep.ClosedSweep = false;
                sweep.SweepTolerance = myDoc.ModelAbsoluteTolerance;

                // join compressCrv and stretchCrv to create a real rail for both compress and stretch
                List<Curve> crves = new List<Curve>();
                crves.Add(compressCrv);
                crves.Add(stretchCrv);

                Curve linearCrv = Curve.JoinCurves(crves)[0];

                #region base structure 2 bars
                double baseStructureDisToCenter = 4.4;
                Curve baseStructureCrv1 = linearCrv.DuplicateCurve();
                Curve baseStructureCrv2 = linearCrv.DuplicateCurve();
                normal_dir.Rotate(Math.PI / 2, axis);
                p_end = endPt + normal_dir;
                p_end = endSuf.ClosestPoint(p_end);
                normal_dir = (Vector3d)(p_end - endPt);
                baseStructureCrv1.Translate(baseStructureDisToCenter / normal_dir.Length * normal_dir);
                normal_dir.Rotate(Math.PI, axis);
                p_end = endPt + normal_dir;
                p_end = endSuf.ClosestPoint(p_end);
                normal_dir = (Vector3d)(p_end - endPt);
                baseStructureCrv2.Translate(baseStructureDisToCenter / normal_dir.Length * normal_dir);

                //myDoc.Objects.AddCurve(baseStructureCrv1, attributes);
                //myDoc.Views.Redraw();
                //myDoc.Objects.AddCurve(baseStructureCrv2, attributes);
                //myDoc.Views.Redraw();

                Point3d[] guider1CornerPt = new Point3d[5];
                Point3d[] guider2CornerPt = new Point3d[5];
                Point3d[] guiderOuter1Pt = new Point3d[5];
                Point3d[] guiderOuter2Pt = new Point3d[5];
                Point3d[] cornerPt = new Point3d[5];

                Transform txp = Transform.Translation(1 / normal_dir.Length * normal_dir);
                Transform txp_big = Transform.Translation(1 / normal_dir.Length * normal_dir);
                normal_dir.Rotate(Math.PI / 2, axis);
                Transform tyn = Transform.Translation(0.75 / normal_dir.Length * normal_dir);
                Transform tyn_big = Transform.Translation(3.8 / normal_dir.Length * normal_dir);
                normal_dir.Rotate(Math.PI / 2, axis);
                Transform txn = Transform.Translation(1 / normal_dir.Length * normal_dir);
                Transform txn_big = Transform.Translation(1 / normal_dir.Length * normal_dir);
                normal_dir.Rotate(Math.PI / 2, axis);
                Transform typ = Transform.Translation(0.75 / normal_dir.Length * normal_dir);
                Transform typ_big = Transform.Translation(3.8 / normal_dir.Length * normal_dir);

                cornerPt[0] = baseStructureCrv1.PointAtEnd;
                cornerPt[1] = baseStructureCrv1.PointAtEnd;
                cornerPt[2] = baseStructureCrv1.PointAtEnd;
                cornerPt[3] = baseStructureCrv1.PointAtEnd;
                cornerPt[0].Transform(txp); cornerPt[0].Transform(typ); cornerPt[0] = endSuf.ClosestPoint(cornerPt[0]);
                cornerPt[1].Transform(txn); cornerPt[1].Transform(typ); cornerPt[1] = endSuf.ClosestPoint(cornerPt[1]);
                cornerPt[2].Transform(txn); cornerPt[2].Transform(tyn); cornerPt[2] = endSuf.ClosestPoint(cornerPt[2]);
                cornerPt[3].Transform(txp); cornerPt[3].Transform(tyn); cornerPt[3] = endSuf.ClosestPoint(cornerPt[3]);
                cornerPt[4] = cornerPt[0];

                guider1CornerPt[0] = cornerPt[0];
                guider1CornerPt[1] = cornerPt[1];
                guider1CornerPt[2] = cornerPt[2];
                guider1CornerPt[3] = cornerPt[3];
                guider1CornerPt[4] = cornerPt[4];

                cornerPt[0] = baseStructureCrv2.PointAtEnd;
                cornerPt[1] = baseStructureCrv2.PointAtEnd;
                cornerPt[2] = baseStructureCrv2.PointAtEnd;
                cornerPt[3] = baseStructureCrv2.PointAtEnd;
                cornerPt[0].Transform(txp); cornerPt[0].Transform(typ); cornerPt[0] = endSuf.ClosestPoint(cornerPt[0]);
                cornerPt[1].Transform(txn); cornerPt[1].Transform(typ); cornerPt[1] = endSuf.ClosestPoint(cornerPt[1]);
                cornerPt[2].Transform(txn); cornerPt[2].Transform(tyn); cornerPt[2] = endSuf.ClosestPoint(cornerPt[2]);
                cornerPt[3].Transform(txp); cornerPt[3].Transform(tyn); cornerPt[3] = endSuf.ClosestPoint(cornerPt[3]);
                cornerPt[4] = cornerPt[0];

                guider2CornerPt[0] = cornerPt[0];
                guider2CornerPt[1] = cornerPt[1];
                guider2CornerPt[2] = cornerPt[2];
                guider2CornerPt[3] = cornerPt[3];
                guider2CornerPt[4] = cornerPt[4];

                Curve guiderRectCrv1 = new Polyline(guider1CornerPt).ToNurbsCurve();
                Curve guiderRectCrv2 = new Polyline(guider2CornerPt).ToNurbsCurve();

                // Two guiders
                var rect1 = sweep.PerformSweep(linearCrv, guiderRectCrv1)[0];
                var rect2 = sweep.PerformSweep(linearCrv, guiderRectCrv2)[0];

                rect1 = rect1.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                rect2 = rect2.CapPlanarHoles(myDoc.ModelRelativeTolerance);

                //myDoc.Objects.AddBrep(rect1);
                //myDoc.Objects.AddBrep(rect2);

                // calculate outer sweeps
                cornerPt[0] = baseStructureCrv1.PointAtEnd;
                cornerPt[1] = baseStructureCrv1.PointAtEnd;
                cornerPt[2] = baseStructureCrv1.PointAtEnd;
                cornerPt[3] = baseStructureCrv1.PointAtEnd;

                cornerPt[0].Transform(txp_big); cornerPt[0].Transform(typ_big); cornerPt[0] = endSuf.ClosestPoint(cornerPt[0]);
                cornerPt[1].Transform(txn_big); cornerPt[1].Transform(typ_big); cornerPt[1] = endSuf.ClosestPoint(cornerPt[1]);
                cornerPt[2].Transform(txn_big); cornerPt[2].Transform(tyn_big); cornerPt[2] = endSuf.ClosestPoint(cornerPt[2]);
                cornerPt[3].Transform(txp_big); cornerPt[3].Transform(tyn_big); cornerPt[3] = endSuf.ClosestPoint(cornerPt[3]);
                cornerPt[4] = cornerPt[0];

                guiderOuter1Pt[0] = cornerPt[0];
                guiderOuter1Pt[1] = cornerPt[1];
                guiderOuter1Pt[2] = cornerPt[2];
                guiderOuter1Pt[3] = cornerPt[3];
                guiderOuter1Pt[4] = cornerPt[4];

                cornerPt[0] = baseStructureCrv2.PointAtEnd;
                cornerPt[1] = baseStructureCrv2.PointAtEnd;
                cornerPt[2] = baseStructureCrv2.PointAtEnd;
                cornerPt[3] = baseStructureCrv2.PointAtEnd;
                cornerPt[0].Transform(txp_big); cornerPt[0].Transform(typ_big); cornerPt[0] = endSuf.ClosestPoint(cornerPt[0]);
                cornerPt[1].Transform(txn_big); cornerPt[1].Transform(typ_big); cornerPt[1] = endSuf.ClosestPoint(cornerPt[1]);
                cornerPt[2].Transform(txn_big); cornerPt[2].Transform(tyn_big); cornerPt[2] = endSuf.ClosestPoint(cornerPt[2]);
                cornerPt[3].Transform(txp_big); cornerPt[3].Transform(tyn_big); cornerPt[3] = endSuf.ClosestPoint(cornerPt[3]);
                cornerPt[4] = cornerPt[0];

                guiderOuter2Pt[0] = cornerPt[0];
                guiderOuter2Pt[1] = cornerPt[1];
                guiderOuter2Pt[2] = cornerPt[2];
                guiderOuter2Pt[3] = cornerPt[3];
                guiderOuter2Pt[4] = cornerPt[4];

                Curve guiderOuterRectCrv1 = new Polyline(guiderOuter1Pt).ToNurbsCurve();
                Curve guiderOuterRectCrv2 = new Polyline(guiderOuter2Pt).ToNurbsCurve();

                var outRect1 = sweep.PerformSweep(linearCrv, guiderOuterRectCrv1)[0];
                var outRect2 = sweep.PerformSweep(linearCrv, guiderOuterRectCrv2)[0];

                outRect1 = outRect1.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                outRect2 = outRect2.CapPlanarHoles(myDoc.ModelRelativeTolerance);

                //myDoc.Objects.AddBrep(outRect1);
                //myDoc.Objects.AddBrep(outRect2);
                //myDoc.Views.Redraw();

                List<Point3d> baseVertexList = new List<Point3d>();
                
                Rhino.Geometry.Collections.BrepVertexList vertexList = outRect1.Vertices;
                if (vertexList != null && vertexList.Count > 0)
                {
                    foreach (var v in vertexList)
                    {
                        baseVertexList.Add(v.Location);
                    }
                }

                Rhino.Geometry.Collections.BrepVertexList vertexList2 = outRect2.Vertices;
                if (vertexList2 != null && vertexList2.Count > 0)
                {
                    foreach (var v in vertexList2)
                    {
                        baseVertexList.Add(v.Location);
                    }
                }

                Plane guiderPln = new Plane(linearCrv.PointAtEnd, linearCrv.TangentAtEnd);
                PlaneSurface guiderPlnSuf = new PlaneSurface(guiderPln, new Interval(-30, 30), new Interval(-30, 30));
                //myDoc.Objects.AddSurface(guiderPlnSuf);
                //myDoc.Views.Redraw();
                List<Point3d> guiderPointsList = new List<Point3d>();
                foreach (Point3d p in baseVertexList)
                {
                    double u, v;
                    guiderPlnSuf.ClosestPoint(p, out u, out v);
                    if (guiderPlnSuf.PointAt(u, v).DistanceTo(p) < 0.5)
                    {
                        guiderPointsList.Add(p);
                    }
                }

                Rhino.Geometry.PointCloud ptCloud = new PointCloud(guiderPointsList);
                for (int i = 0; i < 4; i++)
                {
                    int removeIdx = ptCloud.ClosestPoint(guiderPln.Origin);
                    ptCloud.RemoveAt(removeIdx);
                }
                guiderPointsList.Clear();
                foreach (var p in ptCloud)
                    guiderPointsList.Add(p.Location);

                guiderPointsList.Add(guiderPointsList[0]);
                Curve guiderTopCrv = new Polyline(guiderPointsList).ToNurbsCurve();
                attributes.ObjectColor = Color.Red;
                //myDoc.Objects.AddCurve(guiderTopCrv, attributes);
                //myDoc.Views.Redraw();
                #endregion

                #region prismic structure that limits rotation
                normal_dir = newDir;
                Point3d centerPrismPt = centerCrv.PointAtStart;
                double primBaseSideHalfLength = 1.5;
                txp = Transform.Translation(startSuf.XAxis * primBaseSideHalfLength);
                typ = Transform.Translation(startSuf.YAxis * primBaseSideHalfLength);
                txn = Transform.Translation(startSuf.XAxis * -primBaseSideHalfLength);
                tyn = Transform.Translation(startSuf.YAxis * -primBaseSideHalfLength);

                cornerPt[0] = centerPrismPt;
                cornerPt[1] = centerPrismPt;
                cornerPt[2] = centerPrismPt;
                cornerPt[3] = centerPrismPt;
                cornerPt[0].Transform(txp); cornerPt[0].Transform(typ);
                cornerPt[1].Transform(txn); cornerPt[1].Transform(typ);
                cornerPt[2].Transform(txn); cornerPt[2].Transform(tyn);
                cornerPt[3].Transform(txp); cornerPt[3].Transform(tyn);
                cornerPt[4] = cornerPt[0];
                Curve prismCrv = new Polyline(cornerPt).ToNurbsCurve();
                Brep prismBrep = sweep.PerformSweep(pressCrv, prismCrv)[0];
                prismBrep = prismBrep.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                myDoc.Objects.AddBrep(prismBrep);
                //myDoc.Views.Redraw();
                #endregion

                #region stopper (disc)
                Plane stopperPln = new Plane(pressCrv.PointAtEnd, pressCrv.TangentAtEnd);
                Circle stopperCir = new Circle(stopperPln, pressCrv.PointAtEnd, 2.3);
                double t;
                compressCrv.LengthParameter(2, out t);
                Curve stopperCrv = compressCrv.Split(t)[0];

                var stopperBrep = sweep.PerformSweep(stopperCrv, stopperCir.ToNurbsCurve())[0];
                stopperBrep = stopperBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);

                //myDoc.Objects.AddCurve(stopperCrv);
                myDoc.Objects.AddBrep(stopperBrep);
                //myDoc.Views.Redraw();
                #endregion

                // guider hole
                Point3d guiderPt = railEnd;
                double guiderPtGap = 0.5;

                txp = Transform.Translation(stopperPlnOrig.XAxis * (primBaseSideHalfLength + guiderPtGap));
                typ = Transform.Translation(stopperPlnOrig.YAxis * (primBaseSideHalfLength + guiderPtGap));
                txn = Transform.Translation(stopperPlnOrig.XAxis * -(primBaseSideHalfLength + guiderPtGap));
                tyn = Transform.Translation(stopperPlnOrig.YAxis * -(primBaseSideHalfLength + guiderPtGap));

                cornerPt[0] = guiderPt;
                cornerPt[1] = guiderPt;
                cornerPt[2] = guiderPt;
                cornerPt[3] = guiderPt;
                cornerPt[0].Transform(txp); cornerPt[0].Transform(typ);
                cornerPt[1].Transform(txn); cornerPt[1].Transform(typ);
                cornerPt[2].Transform(txn); cornerPt[2].Transform(tyn);
                cornerPt[3].Transform(txp); cornerPt[3].Transform(tyn);
                cornerPt[4] = cornerPt[0];
                Curve guiderCrv = new Polyline(cornerPt).ToNurbsCurve();

                pressCrv.ClosestPoint(railEnd, out t);
                var splitedLeftOver = pressCrv.Split(t);
                splitedLeftOver[0].LengthParameter(splitedLeftOver[0].GetLength() - 4, out t);

                var splitedLeftCrvs = splitedLeftOver[0].Split(t);
                Brep guiderCenter = sweep.PerformSweep(splitedLeftCrvs[1], guiderCrv)[0];
                guiderCenter = guiderCenter.CapPlanarHoles(myDoc.ModelRelativeTolerance);

                //guider outcube
                Brep outerGuider = sweep.PerformSweep(splitedLeftCrvs[1], guiderTopCrv)[0];
                outerGuider = outerGuider.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                outerGuider.Flip();
                var guiderFinal = Brep.CreateBooleanDifference(guiderCenter, outerGuider, myDoc.ModelRelativeTolerance)[0];
                myDoc.Objects.Add(guiderFinal);


                #endregion

                #region bend part
                #region construct the outer rectangle of each hinge
                normal_dir = newDir;

                int hingeNum = 0;
                double angle = 0;
                double inputDegree = Math.PI / 6;
                double thickness = 2;

                double minDia = 100000000;
                double barLen = -1;
                foreach (double d in DiameterList)
                {
                    if (d <= minDia) minDia = d;
                }
                if ((minDia - 5.6) > 1)
                {
                    barLen = (minDia - 5.6) / 2;
                }
                else
                {
                    barLen = -1;
                }

                if (barLen != -1)
                {
                    computeLatticeHinges(linearCrv.GetLength(), out angle, thickness, out hingeNum, barLen, inputDegree);
                    hingeNum--;

                    normal_dir.Rotate(Math.PI / 2, axis);
                    Point3d base1Pt = endSuf.ClosestPoint(baseStructureCrv1.PointAtEnd);
                    Point3d hingePt1 = base1Pt + (barLen + 1) / normal_dir.Length * normal_dir;
                    Point3d hingeInnerPt1 = base1Pt + barLen / normal_dir.Length * normal_dir;
                    normal_dir.Rotate(Math.PI, axis);
                    Point3d hingePt2 = base1Pt;
                    Point3d hingeInnerPt2 = base1Pt + 4 / normal_dir.Length * normal_dir;
                    double scale = 0;
                    //attributes.ObjectColor = Color.Blue;
                    //myDoc.Objects.AddCurve(linearCrv, attributes);
                    //myDoc.Views.Redraw();

                    if (linearCrv.GetLength() / hingeNum > 3.6)
                    {
                        scale = ((linearCrv.GetLength() / hingeNum - 2) / 2 + 2) / axis.Length;
                    }
                    else
                    {
                        scale = 2.8 / axis.Length;
                    }

                    Point3d hingePt3 = hingePt1 + axis * scale;
                    Point3d hingePt4 = hingePt2 + axis * scale;

                    normal_dir = newDir;
                    normal_dir.Rotate(-Math.PI / 2, axis);
                    Point3d base2Pt = endSuf.ClosestPoint(baseStructureCrv2.PointAtEnd);
                    Point3d hingePt5 = base2Pt + (barLen + 1) / normal_dir.Length * normal_dir;
                    Point3d hingeInnerPt5 = base2Pt + barLen / normal_dir.Length * normal_dir;
                    normal_dir.Rotate(-Math.PI, axis);
                    Point3d hingePt6 = base2Pt;
                    Point3d hingeInnerPt6 = base2Pt + 4 / normal_dir.Length * normal_dir;
                    Point3d hingePt7 = hingePt5 + axis * scale;
                    Point3d hingePt8 = hingePt6 + axis * scale;

                    Point3d[] hingeOuterCornerPt1 = new Point3d[5];
                    hingeOuterCornerPt1[0] = hingePt1;
                    hingeOuterCornerPt1[1] = hingePt2;
                    hingeOuterCornerPt1[2] = hingePt4;
                    hingeOuterCornerPt1[3] = hingePt3;
                    hingeOuterCornerPt1[4] = hingePt1;
                    Curve hingeOuterRectCrv1 = new Polyline(hingeOuterCornerPt1).ToNurbsCurve();

                    Point3d[] hingeOuterCornerPt2 = new Point3d[5];
                    hingeOuterCornerPt2[0] = hingePt5;
                    hingeOuterCornerPt2[1] = hingePt6;
                    hingeOuterCornerPt2[2] = hingePt8;
                    hingeOuterCornerPt2[3] = hingePt7;
                    hingeOuterCornerPt2[4] = hingePt5;
                    Curve hingeOuterRectCrv2 = new Polyline(hingeOuterCornerPt2).ToNurbsCurve();
                    #endregion

                    #region construct the inner rectangle of each hinge
                    double scale1 = scale - 1 / axis.Length;
                    double scale2 = 1/axis.Length;

                    Point3d[] hingeInnerCornerPt1 = new Point3d[5];
                    hingeInnerCornerPt1[0] = hingeInnerPt1 + axis * scale2;
                    hingeInnerCornerPt1[1] = hingeInnerPt2 + axis * scale2;
                    hingeInnerCornerPt1[2] = hingeInnerPt2 + axis * scale1;
                    hingeInnerCornerPt1[3] = hingeInnerPt1 + axis * scale1;
                    hingeInnerCornerPt1[4] = hingeInnerPt1 + axis * scale2;
                    Curve hingeInnerRectCrv1 = new Polyline(hingeInnerCornerPt1).ToNurbsCurve();

                    Point3d[] hingeInnerCornerPt2 = new Point3d[5];
                    hingeInnerCornerPt2[0] = hingeInnerPt5 + axis * scale2;
                    hingeInnerCornerPt2[1] = hingeInnerPt6 + axis * scale2;
                    hingeInnerCornerPt2[2] = hingeInnerPt6 + axis * scale1;
                    hingeInnerCornerPt2[3] = hingeInnerPt5 + axis * scale1;
                    hingeInnerCornerPt2[4] = hingeInnerPt5 + axis * scale2;
                    Curve hingeInnerRectCrv2 = new Polyline(hingeInnerCornerPt2).ToNurbsCurve();
                    #endregion

                    #region Array all outer and inner rectangles of the hinge along the curve

                    #region Divide the curve by N points

                    // front and rear portions that need to be removed from the center curve
                    double frontPara;
                    linearCrv.LengthParameter(0, out frontPara);
                    Point3d front1 = linearCrv.PointAt(frontPara);
                    double endPara;
                    linearCrv.LengthParameter(linearCrv.GetLength() - scale1, out endPara);
                    Point3d end1 = linearCrv.PointAt(endPara);

                    double endCrvPara1 = 0;
                    linearCrv.ClosestPoint(end1, out endCrvPara1);
                    Curve[] splitCrvs11 = linearCrv.Split(endCrvPara1);
                    Curve divideCrv1 = splitCrvs11[0];

                    attributes.ObjectColor = Color.Yellow;

                    // store all curve segments
                    Point3d[] points1;
                    divideCrv1.DivideByCount(hingeNum, true, out points1); // 5 is the number of hinge

                    // store tangent vectors at each point
                    List<Vector3d> tangents1 = new List<Vector3d>();
                    foreach (Point3d p in points1)
                    {
                        double para = 0;
                        divideCrv1.ClosestPoint(p, out para);
                        tangents1.Add(divideCrv1.TangentAt(para) * (-1));
                        //myDoc.Objects.AddPoint(p, attributes);
                    }

                    // store transforms from the end point to each point
                    List<List<Transform>> trans1 = new List<List<Transform>>();
                    Vector3d v0_1 = tangents1[0];
                    Point3d p0_1 = points1[0];
                    int idx = 0;
                    foreach (Vector3d v1 in tangents1)
                    {
                        Transform translate = Transform.Translation(points1.ElementAt(idx) - p0_1);
                        Transform rotate = Transform.Rotation(v0_1, v1, points1.ElementAt(idx));
                        List<Transform> tr = new List<Transform>();
                        tr.Add(translate);
                        tr.Add(rotate);
                        trans1.Add(tr);
                        idx++;
                    }

                    // create all outer and inner ractangles along the curve
                    List<Curve> outerCrvs_base1 = new List<Curve>();
                    List<Curve> innerCrvs_base1 = new List<Curve>();
                    List<Curve> outerCrvs_base2 = new List<Curve>();
                    List<Curve> innerCrvs_base2 = new List<Curve>();

                    foreach (List<Transform> tr in trans1)
                    {
                        Curve tempOuterCrv = hingeOuterRectCrv1.DuplicateCurve();
                        tempOuterCrv.Transform(tr.ElementAt(0));
                        tempOuterCrv.Transform(tr.ElementAt(1));
                        outerCrvs_base1.Add(tempOuterCrv);

                        Curve tempInnerCrv = hingeInnerRectCrv1.DuplicateCurve();
                        tempInnerCrv.Transform(tr.ElementAt(0));
                        tempInnerCrv.Transform(tr.ElementAt(1));
                        innerCrvs_base1.Add(tempInnerCrv);

                        //myDoc.Objects.AddCurve(tempOuterCrv, attributes);
                        //myDoc.Objects.AddCurve(tempInnerCrv, attributes);
                        //myDoc.Views.Redraw();
                    }

                    foreach (List<Transform> tr in trans1)
                    {
                        Curve tempOuterCrv = hingeOuterRectCrv2.DuplicateCurve();
                        tempOuterCrv.Transform(tr.ElementAt(0));
                        tempOuterCrv.Transform(tr.ElementAt(1));
                        outerCrvs_base2.Add(tempOuterCrv);

                        Curve tempInnerCrv = hingeInnerRectCrv2.DuplicateCurve();
                        tempInnerCrv.Transform(tr.ElementAt(0));
                        tempInnerCrv.Transform(tr.ElementAt(1));
                        innerCrvs_base2.Add(tempInnerCrv);

                        //myDoc.Objects.AddCurve(tempOuterCrv, attributes);
                        //myDoc.Objects.AddCurve(tempInnerCrv, attributes);
                        //myDoc.Views.Redraw();
                    }
                    #endregion

                    #region extrude the arrays of rectangles toward both sides
                    List<Brep> outerBreps_base1 = new List<Brep>();
                    List<Brep> innerBreps_base1 = new List<Brep>();
                    List<Brep> outerBreps_base2 = new List<Brep>();
                    List<Brep> innerBreps_base2 = new List<Brep>();

                    attributes.ObjectColor = Color.Red;

                    foreach (Curve c in outerCrvs_base1)
                    {

                        Surface surf = Brep.CreatePlanarBreps(c)[0].Faces[0];
                        double wde;
                        double hgt;
                        surf.GetSurfaceSize(out wde, out hgt);
                        Vector3d hinge_normal = surf.NormalAt(wde / 2, hgt / 2);
                        double s = 1 / hinge_normal.Length; // 3 is the thickness of the hinge 
                        Transform rectTrans = Transform.Translation(hinge_normal * s);
                        c.Transform(rectTrans);

                        Brep brep = Brep.CreateFromSurface(Surface.CreateExtrusion(c, -2 * hinge_normal * s));
                        brep = brep.CapPlanarHoles(myDoc.ModelRelativeTolerance);

                        outerBreps_base1.Add(brep);
                    }

                    foreach (Curve c in innerCrvs_base1)
                    {

                        Surface surf = Brep.CreatePlanarBreps(c)[0].Faces[0];
                        double wde;
                        double hgt;
                        surf.GetSurfaceSize(out wde, out hgt);
                        Vector3d hinge_normal = surf.NormalAt(wde / 2, hgt / 2);
                        double s = 3 / hinge_normal.Length; // 3 is the thickness of the hinge 
                        Transform rectTrans = Transform.Translation(hinge_normal * s);
                        c.Transform(rectTrans);

                        Brep brep = Brep.CreateFromSurface(Surface.CreateExtrusion(c, -2 * hinge_normal * s));
                        brep = brep.CapPlanarHoles(myDoc.ModelRelativeTolerance);

                        innerBreps_base1.Add(brep);
                    }

                    foreach (Curve c in outerCrvs_base2)
                    {

                        Surface surf = Brep.CreatePlanarBreps(c)[0].Faces[0];
                        double wde;
                        double hgt;
                        surf.GetSurfaceSize(out wde, out hgt);
                        Vector3d hinge_normal = surf.NormalAt(wde / 2, hgt / 2);
                        double s = 1 / hinge_normal.Length; // 3 is the thickness of the hinge 
                        Transform rectTrans = Transform.Translation(hinge_normal * s);
                        c.Transform(rectTrans);

                        Brep brep = Brep.CreateFromSurface(Surface.CreateExtrusion(c, -2 * hinge_normal * s));
                        brep = brep.CapPlanarHoles(myDoc.ModelRelativeTolerance);

                        outerBreps_base2.Add(brep);
                    }

                    foreach (Curve c in innerCrvs_base2)
                    {

                        Surface surf = Brep.CreatePlanarBreps(c)[0].Faces[0];
                        double wde;
                        double hgt;
                        surf.GetSurfaceSize(out wde, out hgt);
                        Vector3d hinge_normal = surf.NormalAt(wde / 2, hgt / 2);
                        double s = 3 / hinge_normal.Length; // 3 is the thickness of the hinge 
                        Transform rectTrans = Transform.Translation(hinge_normal * s);
                        c.Transform(rectTrans);

                        Brep brep = Brep.CreateFromSurface(Surface.CreateExtrusion(c, -2 * hinge_normal * s));
                        brep = brep.CapPlanarHoles(myDoc.ModelRelativeTolerance);

                        innerBreps_base2.Add(brep);
                    }
                    #endregion

                    #endregion

                    #region boolean difference
                    // generate the central connections
                    List<Brep> b_list = new List<Brep>();
                    Brep prev_brep = rect1;

                    for (int id = 0; id < innerBreps_base1.Count(); id++)
                    {
                        var tempB = Brep.CreateBooleanDifference(innerBreps_base1[id], prev_brep, myDoc.ModelRelativeTolerance);
                        if (tempB != null)
                        {
                            if (tempB.Count() > 1)
                            {
                                myDoc.Objects.AddBrep(tempB[1], attributes);
                                tempB[0].Flip();
                                prev_brep = tempB[0];
                            }
                            else if (tempB.Count() == 1)
                            {
                                myDoc.Objects.AddBrep(tempB[0], attributes);
                                tempB[0].Flip();
                                prev_brep = tempB[0];
                            }
                        }
                    }
                    myDoc.Objects.AddBrep(prev_brep, attributes);

                    prev_brep = rect2;

                    for (int id = 0; id < innerBreps_base2.Count(); id++)
                    {
                        var tempB = Brep.CreateBooleanDifference(innerBreps_base2[id], prev_brep, myDoc.ModelRelativeTolerance);
                        if (tempB != null)
                        {
                            if (tempB.Count() > 1)
                            {
                                myDoc.Objects.AddBrep(tempB[1], attributes);
                                tempB[0].Flip();
                                prev_brep = tempB[0];
                            }
                            else if (tempB.Count() == 1)
                            {
                                myDoc.Objects.AddBrep(tempB[0], attributes);
                                tempB[0].Flip();
                                prev_brep = tempB[0];
                            }
                        }
                    }
                    myDoc.Objects.AddBrep(prev_brep, attributes);

                    // generate the hinges
                    var firstHinge1 = Brep.CreateBooleanDifference(innerBreps_base1[0], outerBreps_base1[0], myDoc.ModelRelativeTolerance);

                    myDoc.Objects.AddBrep(firstHinge1[0], attributes);
                    //myDoc.Views.Redraw();

                    foreach (List<Transform> tt in trans1)
                    {
                        if (trans1.IndexOf(tt) != 0)
                        {
                            Brep tempBrep = firstHinge1[0].DuplicateBrep();
                            tempBrep.Transform(tt.ElementAt(0));
                            tempBrep.Transform(tt.ElementAt(1));
                            myDoc.Objects.AddBrep(tempBrep, attributes);
                        }
                    }

                    var firstHinge2 = Brep.CreateBooleanDifference(innerBreps_base2[0], outerBreps_base2[0], myDoc.ModelRelativeTolerance);
                    myDoc.Objects.AddBrep(firstHinge2[0], attributes);
                    //myDoc.Views.Redraw();

                    foreach (List<Transform> tt in trans1)
                    {
                        if (trans1.IndexOf(tt) != 0)
                        {
                            Brep tempBrep = firstHinge2[0].DuplicateBrep();
                            tempBrep.Transform(tt.ElementAt(0));
                            tempBrep.Transform(tt.ElementAt(1));
                            myDoc.Objects.AddBrep(tempBrep, attributes);
                        }
                    }

                    #endregion
                }
                #endregion

            }

            myDoc.Views.Redraw();
        }


        #region Old version of linear + twist support implementation
        //private void generateLinearTwistSupport(Plane startSuf, Plane endSuf, Curve centerCrv, Curve compressCrv, Curve pressCrv, Curve stretchCrv, Point3d railEnd, Plane stopperPlnOrig)
        //{
        //    // create sweep function
        //    var sweep = new Rhino.Geometry.SweepOneRail();
        //    sweep.AngleToleranceRadians = myDoc.ModelAngleToleranceRadians;
        //    sweep.ClosedSweep = false;
        //    sweep.SweepTolerance = myDoc.ModelAbsoluteTolerance;

        //    // join compressCrv and stretchCrv to create a real rail for both compress and stretch
        //    List<Curve> crves = new List<Curve>();
        //    crves.Add(compressCrv);
        //    crves.Add(stretchCrv);

        //    Curve linearCrv = Curve.JoinCurves(crves)[0];


        //    // base structure 2 bars
        //    double baseStructureDisToCenter = 4;
        //    Curve baseStructureCrv1 = linearCrv.DuplicateCurve();
        //    Curve baseStructureCrv2 = linearCrv.DuplicateCurve();
        //    baseStructureCrv1.Translate(endSuf.YAxis * baseStructureDisToCenter);
        //    baseStructureCrv2.Translate(endSuf.YAxis * (-baseStructureDisToCenter));
        //    Point3d[] guiderOuterCornerPt = new Point3d[5];
        //    Point3d[] guiderInnerCornerPt = new Point3d[5];
        //    Point3d[] cornerPt = new Point3d[5];
        //    Transform txp = Transform.Translation(endSuf.XAxis * 3);
        //    Transform typ = Transform.Translation(endSuf.YAxis * 1);
        //    Transform txn = Transform.Translation(endSuf.XAxis * -3);
        //    Transform tyn = Transform.Translation(endSuf.YAxis * -1);
        //    cornerPt[0] = baseStructureCrv1.PointAtEnd;
        //    cornerPt[1] = baseStructureCrv1.PointAtEnd;
        //    cornerPt[2] = baseStructureCrv1.PointAtEnd;
        //    cornerPt[3] = baseStructureCrv1.PointAtEnd;
        //    cornerPt[0].Transform(txp); cornerPt[0].Transform(typ);
        //    cornerPt[1].Transform(txn); cornerPt[1].Transform(typ);
        //    cornerPt[2].Transform(txn); cornerPt[2].Transform(tyn);
        //    cornerPt[3].Transform(txp); cornerPt[3].Transform(tyn);

        //    guiderOuterCornerPt[0] = cornerPt[0];
        //    guiderOuterCornerPt[1] = cornerPt[1];

        //    guiderInnerCornerPt[0] = cornerPt[2];
        //    guiderInnerCornerPt[1] = cornerPt[3];

        //    Curve baseRectCrv1 = new Polyline(cornerPt).ToNurbsCurve();
        //    cornerPt[0] = baseStructureCrv2.PointAtEnd;
        //    cornerPt[1] = baseStructureCrv2.PointAtEnd;
        //    cornerPt[2] = baseStructureCrv2.PointAtEnd;
        //    cornerPt[3] = baseStructureCrv2.PointAtEnd;
        //    cornerPt[0].Transform(txp); cornerPt[0].Transform(typ);
        //    cornerPt[1].Transform(txn); cornerPt[1].Transform(typ);
        //    cornerPt[2].Transform(txn); cornerPt[2].Transform(tyn);
        //    cornerPt[3].Transform(txp); cornerPt[3].Transform(tyn);
        //    cornerPt[4] = cornerPt[0];

        //    guiderOuterCornerPt[2] = cornerPt[2];
        //    guiderOuterCornerPt[3] = cornerPt[3];
        //    guiderOuterCornerPt[4] = guiderOuterCornerPt[0];

        //    guiderInnerCornerPt[2] = cornerPt[0];
        //    guiderInnerCornerPt[3] = cornerPt[1];
        //    guiderInnerCornerPt[4] = guiderInnerCornerPt[0];

        //    guiderInnerCornerPt[0].Transform(txn);
        //    guiderInnerCornerPt[1].Transform(txp);
        //    guiderInnerCornerPt[2].Transform(txp);
        //    guiderInnerCornerPt[3].Transform(txn);
        //    guiderInnerCornerPt[4].Transform(txn);

        //    //foreach (var p in guiderInnerCornerPt)
        //    //{
        //    //    myDoc.Objects.AddPoint(p);
        //    //    myDoc.Views.Redraw();
        //    //}
        //    Curve guiderOuterRectCrv = new Polyline(guiderOuterCornerPt).ToNurbsCurve();
        //    Curve guiderInnerRectCrv = new Polyline(guiderInnerCornerPt).ToNurbsCurve();

        //    var outerRect = sweep.PerformSweep(linearCrv, guiderOuterRectCrv)[0];
        //    var innerRect = sweep.PerformSweep(linearCrv, guiderInnerRectCrv)[0];


        //    var baseBreps = Brep.CreateBooleanIntersection(outerRect, innerRect, myDoc.ModelRelativeTolerance);
        //    baseBreps[0] = baseBreps[0].CapPlanarHoles(myDoc.ModelRelativeTolerance);
        //    baseBreps[1] = baseBreps[1].CapPlanarHoles(myDoc.ModelRelativeTolerance);

        //    myDoc.Objects.AddBrep(baseBreps[0]);
        //    myDoc.Objects.AddBrep(baseBreps[1]);

        //    List<Point3d> baseVertexList = new List<Point3d>();
        //    foreach (Brep b in baseBreps)
        //    {
        //        Rhino.Geometry.Collections.BrepVertexList vertexList = b.Vertices;
        //        if (vertexList != null && vertexList.Count > 0)
        //        {
        //            foreach (var v in vertexList)
        //            {
        //                baseVertexList.Add(v.Location);
        //            }
        //        }
        //    }

        //    Plane guiderPln = new Plane(linearCrv.PointAtEnd, linearCrv.TangentAtEnd);
        //    PlaneSurface guiderPlnSuf = new PlaneSurface(guiderPln, new Interval(-30, 30), new Interval(-30, 30));
        //    List<Point3d> guiderPointsList = new List<Point3d>();
        //    foreach (Point3d p in baseVertexList)
        //    {
        //        double u, v;
        //        guiderPlnSuf.ClosestPoint(p, out u, out v);
        //        if (guiderPlnSuf.PointAt(u, v).DistanceTo(p) < 0.5)
        //        {
        //            guiderPointsList.Add(p);
        //        }
        //    }

        //    Rhino.Geometry.PointCloud ptCloud = new PointCloud(guiderPointsList);
        //    for (int i = 0; i < 4; i++)
        //    {
        //        int removeIdx = ptCloud.ClosestPoint(guiderPln.Origin);
        //        ptCloud.RemoveAt(removeIdx);
        //    }
        //    guiderPointsList.Clear();
        //    foreach (var p in ptCloud)
        //        guiderPointsList.Add(p.Location);

        //    guiderPointsList.Add(guiderPointsList[0]);
        //    Curve guiderTopCrv = new Polyline(guiderPointsList).ToNurbsCurve();


        //    //cylindric structure that allows rotation
        //    Point3d centerCylinPt = centerCrv.PointAtStart;
        //    double cylinBaseRadius = 1;
        //    Circle cylinCircle = new Circle(startSuf, centerCylinPt, cylinBaseRadius);
        //    Curve cylinCrv = cylinCircle.ToNurbsCurve();
        //    Brep cylinBrep = sweep.PerformSweep(pressCrv, cylinCrv)[0];
        //    cylinBrep = cylinBrep.CapPlanarHoles(myDoc.ModelRelativeTolerance);
        //    myDoc.Objects.AddBrep(cylinBrep);
        //    //myDoc.Views.Redraw();

        //    // stopper (disc)
        //    Plane stopperPln = new Plane(pressCrv.PointAtEnd, pressCrv.TangentAtEnd);
        //    Circle stopperCir = new Circle(stopperPln, pressCrv.PointAtEnd, 2);
        //    double t;
        //    compressCrv.LengthParameter(3, out t);
        //    Curve stopperCrv = compressCrv.Split(t)[0];

        //    var stopperBrep = sweep.PerformSweep(stopperCrv, stopperCir.ToNurbsCurve())[0];
        //    stopperBrep = stopperBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);

        //    //myDoc.Objects.AddCurve(stopperCrv);
        //    myDoc.Objects.AddBrep(stopperBrep);
        //    //myDoc.Views.Redraw();


        //    // guider hole
        //    Point3d guiderPt = railEnd;
        //    double guiderPtGap = 0.2;
        //    Circle guiderCircle = new Circle(stopperPlnOrig, guiderPt, cylinBaseRadius+guiderPtGap); 
        //    Curve guiderCrv = guiderCircle.ToNurbsCurve();

        //    var attributes = new ObjectAttributes();
        //    attributes.ObjectColor = Color.Yellow;
        //    attributes.ColorSource = ObjectColorSource.ColorFromObject;
        //    //myDoc.Objects.AddCurve(guiderCrv, attributes);

        //    pressCrv.ClosestPoint(railEnd, out t);
        //    var splitedLeftOver = pressCrv.Split(t);
        //    splitedLeftOver[0].LengthParameter(splitedLeftOver[0].GetLength() - 3, out t);

        //    var splitedLeftCrvs = splitedLeftOver[0].Split(t);
        //    Brep guiderCenter = sweep.PerformSweep(splitedLeftCrvs[1], guiderCrv)[0];
        //    guiderCenter = guiderCenter.CapPlanarHoles(myDoc.ModelRelativeTolerance);

        //    //guider outcube
        //    Brep outerGuider = sweep.PerformSweep(splitedLeftCrvs[1], guiderTopCrv)[0];
        //    outerGuider = outerGuider.CapPlanarHoles(myDoc.ModelRelativeTolerance);
        //    outerGuider.Flip();
        //    var guiderFinal = Brep.CreateBooleanDifference(guiderCenter, outerGuider,myDoc.ModelRelativeTolerance)[0];
        //    myDoc.Objects.Add(guiderFinal);

        //    myDoc.Views.Redraw();
        //}
        #endregion

        private void generateTwistBendSupport(Plane startSuf, Plane endSuf, Curve centerCrv, List<bend_info> bendInfoList, Point3d startPt, Point3d endPt, bool isStart)
        {
            foreach (bend_info b in bendInfoList)
            {
                Vector3d normal_dir = b.dir;
                Vector3d axis;
                if (isStart)
                {
                    #region twist part
                    // create sweep function
                    var sweep = new Rhino.Geometry.SweepOneRail();
                    sweep.AngleToleranceRadians = myDoc.ModelAngleToleranceRadians;
                    sweep.ClosedSweep = false;
                    sweep.SweepTolerance = myDoc.ModelAbsoluteTolerance;

                    // compute the base height and generate the guide curves
                    double t;
                    centerCrv.LengthParameter(centerCrv.GetLength() - 3, out t);  // the height is currently 5. It should be confined with the limit from the test
                    Curve guiderCrv = centerCrv.Split(t)[1];
                    Curve cylinCrv = centerCrv.Split(t)[0];
                    guiderCrv.LengthParameter(0.5, out t);
                    Curve cylinGap = guiderCrv.Split(t)[0];
                    Curve guiderCrvLeftover = guiderCrv.Split(t)[1];

                    List<Curve> cylinCrvList = new List<Curve>();
                    cylinCrvList.Add(cylinCrv);
                    cylinCrvList.Add(cylinGap);
                    Curve cylinCrvAll = Curve.JoinCurves(cylinCrvList)[0];

                    #region base structure 2 bars
                    double baseStructureDisToCenter = 3.3;
                    Curve baseStructureCrv1 = guiderCrv.DuplicateCurve();
                    Curve baseStructureCrv2 = guiderCrv.DuplicateCurve();
                    baseStructureCrv1.Translate(endSuf.YAxis * baseStructureDisToCenter);
                    baseStructureCrv2.Translate(endSuf.YAxis * (-baseStructureDisToCenter));
                    Point3d[] guiderOuterCornerPt = new Point3d[5];
                    Point3d[] guiderInnerCornerPt = new Point3d[5];
                    Point3d[] cornerPt = new Point3d[5];
                    Transform txp = Transform.Translation(endSuf.XAxis * 2.8);
                    Transform typ = Transform.Translation(endSuf.YAxis * 0.5);
                    Transform txn = Transform.Translation(endSuf.XAxis * -2.8);
                    Transform tyn = Transform.Translation(endSuf.YAxis * -0.5);
                    cornerPt[0] = baseStructureCrv1.PointAtEnd;
                    cornerPt[1] = baseStructureCrv1.PointAtEnd;
                    cornerPt[2] = baseStructureCrv1.PointAtEnd;
                    cornerPt[3] = baseStructureCrv1.PointAtEnd;
                    cornerPt[0].Transform(txp); cornerPt[0].Transform(typ);
                    cornerPt[1].Transform(txn); cornerPt[1].Transform(typ);
                    cornerPt[2].Transform(txn); cornerPt[2].Transform(tyn);
                    cornerPt[3].Transform(txp); cornerPt[3].Transform(tyn);


                    guiderOuterCornerPt[0] = cornerPt[0];
                    guiderOuterCornerPt[1] = cornerPt[1];

                    guiderInnerCornerPt[0] = cornerPt[2];
                    guiderInnerCornerPt[1] = cornerPt[3];

                    Curve baseRectCrv1 = new Polyline(cornerPt).ToNurbsCurve();
                    cornerPt[0] = baseStructureCrv2.PointAtEnd;
                    cornerPt[1] = baseStructureCrv2.PointAtEnd;
                    cornerPt[2] = baseStructureCrv2.PointAtEnd;
                    cornerPt[3] = baseStructureCrv2.PointAtEnd;
                    cornerPt[0].Transform(txp); cornerPt[0].Transform(typ);
                    cornerPt[1].Transform(txn); cornerPt[1].Transform(typ);
                    cornerPt[2].Transform(txn); cornerPt[2].Transform(tyn);
                    cornerPt[3].Transform(txp); cornerPt[3].Transform(tyn);
                    cornerPt[4] = cornerPt[0];

                    guiderOuterCornerPt[2] = cornerPt[2];
                    guiderOuterCornerPt[3] = cornerPt[3];
                    guiderOuterCornerPt[4] = guiderOuterCornerPt[0];

                    guiderInnerCornerPt[2] = cornerPt[0];
                    guiderInnerCornerPt[3] = cornerPt[1];
                    guiderInnerCornerPt[4] = guiderInnerCornerPt[0];

                    guiderInnerCornerPt[0].Transform(txn);
                    guiderInnerCornerPt[1].Transform(txp);
                    guiderInnerCornerPt[2].Transform(txp);
                    guiderInnerCornerPt[3].Transform(txn);
                    guiderInnerCornerPt[4].Transform(txn);

                    //foreach (var p in guiderInnerCornerPt)
                    //{
                    //    myDoc.Objects.AddPoint(p);
                    //    myDoc.Views.Redraw();
                    //}
                    Curve guiderOuterRectCrv = new Polyline(guiderOuterCornerPt).ToNurbsCurve();
                    Curve guiderInnerRectCrv = new Polyline(guiderInnerCornerPt).ToNurbsCurve();

                    var outerRect = sweep.PerformSweep(guiderCrv, guiderOuterRectCrv)[0];
                    var innerRect = sweep.PerformSweep(guiderCrv, guiderInnerRectCrv)[0];


                    var baseBreps = Brep.CreateBooleanIntersection(outerRect, innerRect, myDoc.ModelRelativeTolerance);
                    baseBreps[0] = baseBreps[0].CapPlanarHoles(myDoc.ModelRelativeTolerance);
                    baseBreps[1] = baseBreps[1].CapPlanarHoles(myDoc.ModelRelativeTolerance);

                    myDoc.Objects.AddBrep(baseBreps[0]);
                    myDoc.Objects.AddBrep(baseBreps[1]);

                    List<Point3d> baseVertexList = new List<Point3d>();
                    foreach (Brep bb in baseBreps)
                    {
                        Rhino.Geometry.Collections.BrepVertexList vertexList = bb.Vertices;
                        if (vertexList != null && vertexList.Count > 0)
                        {
                            foreach (var v in vertexList)
                            {
                                baseVertexList.Add(v.Location);
                            }
                        }
                    }

                    Plane guiderPln = new Plane(guiderCrv.PointAtEnd, guiderCrv.TangentAtEnd);
                    PlaneSurface guiderPlnSuf = new PlaneSurface(guiderPln, new Interval(-30, 30), new Interval(-30, 30));
                    List<Point3d> guiderPointsList = new List<Point3d>();
                    foreach (Point3d p in baseVertexList)
                    {
                        double u, v;
                        guiderPlnSuf.ClosestPoint(p, out u, out v);
                        if (guiderPlnSuf.PointAt(u, v).DistanceTo(p) < 0.5)
                        {
                            guiderPointsList.Add(p);
                        }
                    }

                    Rhino.Geometry.PointCloud ptCloud = new PointCloud(guiderPointsList);
                    for (int i = 0; i < 4; i++)
                    {
                        int removeIdx = ptCloud.ClosestPoint(guiderPln.Origin);
                        ptCloud.RemoveAt(removeIdx);
                    }
                    guiderPointsList.Clear();
                    foreach (var p in ptCloud)
                        guiderPointsList.Add(p.Location);

                    guiderPointsList.Add(guiderPointsList[0]);
                    Curve guiderTopCrv = new Polyline(guiderPointsList).ToNurbsCurve();
                    #endregion

                    //cylindral structure that enables rotation
                    Point3d centerCylin = centerCrv.PointAtStart;
                    double cylinBaseSideRadius = 1.5;
                    Curve cylinCircle = new Circle(startSuf, centerCylin, cylinBaseSideRadius).ToNurbsCurve();
                    Brep cylinBrep = sweep.PerformSweep(cylinCrvAll, cylinCircle)[0];
                    cylinBrep = cylinBrep.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                    //myDoc.Objects.AddBrep(cylinBrep);
                    //myDoc.Views.Redraw();

                    // stopper (disc)
                    Plane stopperPln = new Plane(cylinCrvAll.PointAtEnd, cylinCrvAll.TangentAtEnd);
                    Circle stopperCir = new Circle(stopperPln, cylinCrvAll.PointAtEnd, 2.3);
                    double tt;
                    guiderCrvLeftover.LengthParameter(guiderCrvLeftover.GetLength() - 2, out tt);
                    Curve stopperCrv = guiderCrvLeftover.Split(tt)[1];
                    var stopperBrep = sweep.PerformSweep(stopperCrv, stopperCir.ToNurbsCurve())[0];
                    stopperBrep = stopperBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);

                    //myDoc.Objects.AddCurve(stopperCrv);
                    myDoc.Objects.AddBrep(stopperBrep);
                    //myDoc.Views.Redraw();

                    // guider hole

                    Point3d guiderPt = cylinCrv.PointAtEnd;
                    double guiderPtGap = 0.5;
                    double newRadius = cylinBaseSideRadius + guiderPtGap;
                    Plane stopperPln1 = new Plane(cylinCrv.PointAtEnd, cylinCrv.TangentAtEnd);
                    Curve guiderCircle = new Circle(stopperPln1, guiderPt, newRadius).ToNurbsCurve();

                    double ttt;
                    cylinCrv.LengthParameter(cylinCrv.GetLength() - 4, out ttt);

                    var splitedLeftCrvs = cylinCrv.Split(ttt);
                    Brep guiderCenter = sweep.PerformSweep(splitedLeftCrvs[1], guiderCircle)[0];
                    guiderCenter = guiderCenter.CapPlanarHoles(myDoc.ModelRelativeTolerance);

                    //guider outcube
                    Brep outerGuider = sweep.PerformSweep(splitedLeftCrvs[1], guiderTopCrv)[0];
                    outerGuider = outerGuider.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                    var guiderFinal = Brep.CreateBooleanIntersection(outerGuider, guiderCenter, myDoc.ModelRelativeTolerance)[0];
                    myDoc.Objects.Add(guiderFinal);
                    #endregion

                    #region bend part
                    axis = startSuf.Normal;

                    #region construct the outer rectangle of each hinge
                    var attributes = new ObjectAttributes();
                    attributes.ObjectColor = Color.Purple;
                    attributes.ColorSource = ObjectColorSource.ColorFromObject;
                    normal_dir.Rotate(Math.PI / 2, axis);
                    Point3d hingePt1 = startPt + normal_dir / 3;
                    Point3d hingeInnerPt1 = startPt + normal_dir / 5;
                    normal_dir.Rotate(Math.PI, axis);
                    Point3d hingePt2 = startPt + normal_dir / 3;
                    Point3d hingeInnerPt2 = startPt + normal_dir / 5;
                    double scale = (cylinCrv.GetLength() / 8 / 2) / axis.Length;   // 8 is the number of hinge
                    Point3d hingePt3 = hingePt1 + axis * scale;
                    Point3d hingePt4 = hingePt2 + axis * scale;

                    Point3d[] hingeOuterCornerPt = new Point3d[5];
                    hingeOuterCornerPt[0] = hingePt1;
                    hingeOuterCornerPt[1] = hingePt2;
                    hingeOuterCornerPt[2] = hingePt4;
                    hingeOuterCornerPt[3] = hingePt3;
                    hingeOuterCornerPt[4] = hingePt1;
                    Curve hingeOuterRectCrv = new Polyline(hingeOuterCornerPt).ToNurbsCurve();
                   // myDoc.Objects.AddCurve(hingeOuterRectCrv, attributes);
                    //myDoc.Views.Redraw();
                    #endregion

                    #region construct the inner rectangle of each hinge
                    double scale1 = scale / 2;
                    double scale2 = scale / 4;
                    Point3d[] hingeInnerCornerPt = new Point3d[5];
                    hingeInnerCornerPt[0] = hingeInnerPt1 + axis * scale2;
                    hingeInnerCornerPt[1] = hingeInnerPt2 + axis * scale2;
                    hingeInnerCornerPt[2] = hingeInnerPt2 + axis * scale2 + axis * scale1;
                    hingeInnerCornerPt[3] = hingeInnerPt1 + axis * scale2 + axis * scale1;
                    hingeInnerCornerPt[4] = hingeInnerPt1 + axis * scale2;
                    Curve hingeInnerRectCrv = new Polyline(hingeInnerCornerPt).ToNurbsCurve();
                   // myDoc.Objects.AddCurve(hingeInnerRectCrv, attributes);
                    //myDoc.Views.Redraw();
                    #endregion

                    #region Array all outer and inner rectangles of the hinge along the curve

                    #region Divide the curve by N points
                    // front and rear portions that need to be removed from the center curve
                    Point3d front = startPt + axis * scale1;
                    
                    double hingeEndPara;
                    splitedLeftCrvs[0].LengthParameter(splitedLeftCrvs[0].GetLength() - 2* endSuf.Normal.Length * scale1, out hingeEndPara);
                    Point3d end_pt = splitedLeftCrvs[0].PointAt(hingeEndPara);
                    attributes.ObjectColor = Color.White;
                    //myDoc.Objects.AddPoint(end_pt, attributes);
                    //myDoc.Views.Redraw();

                    Point3d end = end_pt;
                    double frontCrvPara = 0;
                    cylinCrv.ClosestPoint(front, out frontCrvPara);
                    Curve[] splitCrvs = cylinCrv.Split(frontCrvPara);
                    double endCrvPara = 0;
                    splitCrvs[1].ClosestPoint(end, out endCrvPara);
                    Curve[] splitCrvs1 = splitCrvs[1].Split(endCrvPara);
                    Curve divideCrv = splitCrvs1[0];

                    // store all curve segments
                    Point3d[] points;
                    divideCrv.DivideByCount(8, true, out points); // 8 is the number of hinge

                    // store tangent vectors at each point
                    List<Vector3d> tangents = new List<Vector3d>();
                    foreach (Point3d p in points)
                    {
                        double para = 0;
                        divideCrv.ClosestPoint(p, out para);
                        tangents.Add(divideCrv.TangentAt(para));
                        //myDoc.Objects.AddPoint(p, attributes);
                    }

                    // store transforms from the first point to each point
                    List<List<Transform>> trans = new List<List<Transform>>();
                    Vector3d v0 = tangents.ElementAt(0);
                    Point3d p0 = points.ElementAt(0);
                    int idx = 0;
                    foreach (Vector3d v1 in tangents)
                    {
                        Transform translate = Transform.Translation(points.ElementAt(idx) - p0);
                        Transform rotate = Transform.Rotation(v0, v1, points.ElementAt(idx));
                        List<Transform> tr = new List<Transform>();
                        tr.Add(translate);
                        tr.Add(rotate);
                        trans.Add(tr);
                        idx++;
                    }

                    // create all outer and inner ractangles along the curve
                    List<Curve> outerCrvs = new List<Curve>();
                    List<Curve> innerCrvs = new List<Curve>();
                    foreach (List<Transform> tr in trans)
                    {
                        Curve tempOuterCrv = hingeOuterRectCrv.DuplicateCurve();
                        tempOuterCrv.Transform(tr.ElementAt(0));
                        tempOuterCrv.Transform(tr.ElementAt(1));
                        outerCrvs.Add(tempOuterCrv);

                        Curve tempInnerCrv = hingeInnerRectCrv.DuplicateCurve();
                        tempInnerCrv.Transform(tr.ElementAt(0));
                        tempInnerCrv.Transform(tr.ElementAt(1));
                        innerCrvs.Add(tempInnerCrv);

                        //myDoc.Objects.AddCurve(tempOuterCrv, attributes);
                        //myDoc.Objects.AddCurve(tempInnerCrv, attributes);
                    }
                    #endregion

                    #region extrude the arrays of rectangles toward both sides
                    List<Brep> outerBreps = new List<Brep>();
                    List<Brep> innerBreps = new List<Brep>();
                    //List<Brep> innerBrepsDup = new List<Brep>();

                    foreach (Curve c in outerCrvs)
                    {

                        Surface surf = Brep.CreatePlanarBreps(c)[0].Faces[0];
                        double wde;
                        double hgt;
                        surf.GetSurfaceSize(out wde, out hgt);
                        Vector3d hinge_normal = surf.NormalAt(wde / 2, hgt / 2);
                        double s = 1 / hinge_normal.Length; // 3 is the thickness of the hinge 
                        Transform rectTrans = Transform.Translation(hinge_normal * s);
                        c.Transform(rectTrans);

                        Brep brep = Brep.CreateFromSurface(Surface.CreateExtrusion(c, -2 * hinge_normal * s));
                        brep = brep.CapPlanarHoles(myDoc.ModelRelativeTolerance);

                        outerBreps.Add(brep);
                    }

                    foreach (Curve c in innerCrvs)
                    {

                        Surface surf = Brep.CreatePlanarBreps(c)[0].Faces[0];
                        double wde;
                        double hgt;
                        surf.GetSurfaceSize(out wde, out hgt);
                        Vector3d hinge_normal = surf.NormalAt(wde / 2, hgt / 2);
                        double s = 2 / hinge_normal.Length; // 3 is the thickness of the hinge 
                        Transform rectTrans = Transform.Translation(hinge_normal * s);
                        c.Transform(rectTrans);

                        Brep brep = Brep.CreateFromSurface(Surface.CreateExtrusion(c, -2 * hinge_normal * s));
                        brep = brep.CapPlanarHoles(myDoc.ModelRelativeTolerance);

                        innerBreps.Add(brep);
                    }

                    #endregion

                    #endregion

                    #region boolean difference
                    // generate the central connections
                    List<Brep> b_list = new List<Brep>();
                    cylinBrep.Flip();
                    Brep prev_brep = cylinBrep;

                    for (int id = 0; id < innerBreps.Count(); id++)
                    {
                        attributes.ObjectColor = Color.White;
                        var tempB = Brep.CreateBooleanDifference(innerBreps[id], prev_brep, myDoc.ModelRelativeTolerance);
                        myDoc.Objects.AddBrep(tempB[1], attributes);
                        tempB[0].Flip();
                        prev_brep = tempB[0];
                    }
                    myDoc.Objects.AddBrep(prev_brep, attributes);

                    // generate the hinges
                    var firstHinge = Brep.CreateBooleanDifference(innerBreps[0], outerBreps[0], myDoc.ModelRelativeTolerance);
                    myDoc.Objects.AddBrep(firstHinge[0], attributes);
                    //myDoc.Views.Redraw();

                    foreach (List<Transform> tr in trans)
                    {
                        if (trans.IndexOf(tr) != 0)
                        {
                            Brep tempBrep = firstHinge[0].DuplicateBrep();
                            tempBrep.Transform(tr.ElementAt(0));
                            tempBrep.Transform(tr.ElementAt(1));
                            myDoc.Objects.AddBrep(tempBrep, attributes);
                        }
                    }
                    #endregion

                    #endregion

                }
                else
                {
                    #region twist part
                    // create sweep function
                    var sweep = new Rhino.Geometry.SweepOneRail();
                    sweep.AngleToleranceRadians = myDoc.ModelAngleToleranceRadians;
                    sweep.ClosedSweep = false;
                    sweep.SweepTolerance = myDoc.ModelAbsoluteTolerance;

                    // compute the base height and generate the guide curves
                    double t;
                    centerCrv.LengthParameter(centerCrv.GetLength() - 5, out t);  // the height is currently 5. It should be confined with the limit from the test
                    Curve guiderCrv = centerCrv.Split(t)[1];
                    Curve cylinCrv = centerCrv.Split(t)[0];
                    guiderCrv.LengthParameter(0.5, out t);
                    Curve cylinGap = guiderCrv.Split(t)[0];
                    Curve guiderCrvLeftover = guiderCrv.Split(t)[1];

                    List<Curve> cylinCrvList = new List<Curve>();
                    cylinCrvList.Add(cylinCrv);
                    cylinCrvList.Add(cylinGap);
                    Curve cylinCrvAll = Curve.JoinCurves(cylinCrvList)[0];

                    #region base structure 2 bars
                    double baseStructureDisToCenter = 4;
                    Curve baseStructureCrv1 = guiderCrv.DuplicateCurve();
                    Curve baseStructureCrv2 = guiderCrv.DuplicateCurve();
                    baseStructureCrv1.Translate(endSuf.YAxis * baseStructureDisToCenter);
                    baseStructureCrv2.Translate(endSuf.YAxis * (-baseStructureDisToCenter));
                    Point3d[] guiderOuterCornerPt = new Point3d[5];
                    Point3d[] guiderInnerCornerPt = new Point3d[5];
                    Point3d[] cornerPt = new Point3d[5];
                    Transform txp = Transform.Translation(endSuf.XAxis * 3);
                    Transform typ = Transform.Translation(endSuf.YAxis * 1);
                    Transform txn = Transform.Translation(endSuf.XAxis * -3);
                    Transform tyn = Transform.Translation(endSuf.YAxis * -1);
                    cornerPt[0] = baseStructureCrv1.PointAtEnd;
                    cornerPt[1] = baseStructureCrv1.PointAtEnd;
                    cornerPt[2] = baseStructureCrv1.PointAtEnd;
                    cornerPt[3] = baseStructureCrv1.PointAtEnd;
                    cornerPt[0].Transform(txp); cornerPt[0].Transform(typ);
                    cornerPt[1].Transform(txn); cornerPt[1].Transform(typ);
                    cornerPt[2].Transform(txn); cornerPt[2].Transform(tyn);
                    cornerPt[3].Transform(txp); cornerPt[3].Transform(tyn);


                    guiderOuterCornerPt[0] = cornerPt[0];
                    guiderOuterCornerPt[1] = cornerPt[1];

                    guiderInnerCornerPt[0] = cornerPt[2];
                    guiderInnerCornerPt[1] = cornerPt[3];

                    Curve baseRectCrv1 = new Polyline(cornerPt).ToNurbsCurve();
                    cornerPt[0] = baseStructureCrv2.PointAtEnd;
                    cornerPt[1] = baseStructureCrv2.PointAtEnd;
                    cornerPt[2] = baseStructureCrv2.PointAtEnd;
                    cornerPt[3] = baseStructureCrv2.PointAtEnd;
                    cornerPt[0].Transform(txp); cornerPt[0].Transform(typ);
                    cornerPt[1].Transform(txn); cornerPt[1].Transform(typ);
                    cornerPt[2].Transform(txn); cornerPt[2].Transform(tyn);
                    cornerPt[3].Transform(txp); cornerPt[3].Transform(tyn);
                    cornerPt[4] = cornerPt[0];

                    guiderOuterCornerPt[2] = cornerPt[2];
                    guiderOuterCornerPt[3] = cornerPt[3];
                    guiderOuterCornerPt[4] = guiderOuterCornerPt[0];

                    guiderInnerCornerPt[2] = cornerPt[0];
                    guiderInnerCornerPt[3] = cornerPt[1];
                    guiderInnerCornerPt[4] = guiderInnerCornerPt[0];

                    guiderInnerCornerPt[0].Transform(txn);
                    guiderInnerCornerPt[1].Transform(txp);
                    guiderInnerCornerPt[2].Transform(txp);
                    guiderInnerCornerPt[3].Transform(txn);
                    guiderInnerCornerPt[4].Transform(txn);

                    //foreach (var p in guiderInnerCornerPt)
                    //{
                    //    myDoc.Objects.AddPoint(p);
                    //    myDoc.Views.Redraw();
                    //}
                    Curve guiderOuterRectCrv = new Polyline(guiderOuterCornerPt).ToNurbsCurve();
                    Curve guiderInnerRectCrv = new Polyline(guiderInnerCornerPt).ToNurbsCurve();

                    var outerRect = sweep.PerformSweep(guiderCrv, guiderOuterRectCrv)[0];
                    var innerRect = sweep.PerformSweep(guiderCrv, guiderInnerRectCrv)[0];


                    var baseBreps = Brep.CreateBooleanIntersection(outerRect, innerRect, myDoc.ModelRelativeTolerance);
                    baseBreps[0] = baseBreps[0].CapPlanarHoles(myDoc.ModelRelativeTolerance);
                    baseBreps[1] = baseBreps[1].CapPlanarHoles(myDoc.ModelRelativeTolerance);

                    myDoc.Objects.AddBrep(baseBreps[0]);
                    myDoc.Objects.AddBrep(baseBreps[1]);

                    List<Point3d> baseVertexList = new List<Point3d>();
                    foreach (Brep bb in baseBreps)
                    {
                        Rhino.Geometry.Collections.BrepVertexList vertexList = bb.Vertices;
                        if (vertexList != null && vertexList.Count > 0)
                        {
                            foreach (var v in vertexList)
                            {
                                baseVertexList.Add(v.Location);
                            }
                        }
                    }

                    Plane guiderPln = new Plane(guiderCrv.PointAtEnd, guiderCrv.TangentAtEnd);
                    PlaneSurface guiderPlnSuf = new PlaneSurface(guiderPln, new Interval(-30, 30), new Interval(-30, 30));
                    List<Point3d> guiderPointsList = new List<Point3d>();
                    foreach (Point3d p in baseVertexList)
                    {
                        double u, v;
                        guiderPlnSuf.ClosestPoint(p, out u, out v);
                        if (guiderPlnSuf.PointAt(u, v).DistanceTo(p) < 0.5)
                        {
                            guiderPointsList.Add(p);
                        }
                    }

                    Rhino.Geometry.PointCloud ptCloud = new PointCloud(guiderPointsList);
                    for (int i = 0; i < 4; i++)
                    {
                        int removeIdx = ptCloud.ClosestPoint(guiderPln.Origin);
                        ptCloud.RemoveAt(removeIdx);
                    }
                    guiderPointsList.Clear();
                    foreach (var p in ptCloud)
                        guiderPointsList.Add(p.Location);

                    guiderPointsList.Add(guiderPointsList[0]);
                    Curve guiderTopCrv = new Polyline(guiderPointsList).ToNurbsCurve();
                    #endregion

                    //cylindral structure that enables rotation
                    Point3d centerCylin = centerCrv.PointAtStart;
                    double cylinBaseSideRadius = 1;
                    Curve cylinCircle = new Circle(startSuf, centerCylin, cylinBaseSideRadius).ToNurbsCurve();
                    Brep cylinBrep = sweep.PerformSweep(cylinCrvAll, cylinCircle)[0];
                    cylinBrep = cylinBrep.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                    //myDoc.Objects.AddBrep(cylinBrep);
                    //myDoc.Views.Redraw();

                    // stopper (disc)
                    Plane stopperPln = new Plane(cylinCrvAll.PointAtEnd, cylinCrvAll.TangentAtEnd);
                    Circle stopperCir = new Circle(stopperPln, cylinCrvAll.PointAtEnd, 2.5);
                    double tt;
                    guiderCrvLeftover.LengthParameter(guiderCrvLeftover.GetLength() - 3, out tt);
                    Curve stopperCrv = guiderCrvLeftover.Split(tt)[1];
                    var stopperBrep = sweep.PerformSweep(stopperCrv, stopperCir.ToNurbsCurve())[0];
                    stopperBrep = stopperBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);

                    //myDoc.Objects.AddCurve(stopperCrv);
                    myDoc.Objects.AddBrep(stopperBrep);
                    //myDoc.Views.Redraw();

                    // guider hole

                    Point3d guiderPt = cylinCrv.PointAtEnd;
                    double guiderPtGap = 0.2;
                    double newRadius = cylinBaseSideRadius + guiderPtGap;
                    Plane stopperPln1 = new Plane(cylinCrv.PointAtEnd, cylinCrv.TangentAtEnd);
                    Curve guiderCircle = new Circle(stopperPln1, guiderPt, newRadius).ToNurbsCurve();

                    double ttt;
                    cylinCrv.LengthParameter(cylinCrv.GetLength() - 3, out ttt);

                    var splitedLeftCrvs = cylinCrv.Split(ttt);
                    Brep guiderCenter = sweep.PerformSweep(splitedLeftCrvs[1], guiderCircle)[0];
                    guiderCenter = guiderCenter.CapPlanarHoles(myDoc.ModelRelativeTolerance);

                    //guider outcube
                    Brep outerGuider = sweep.PerformSweep(splitedLeftCrvs[1], guiderTopCrv)[0];
                    outerGuider = outerGuider.CapPlanarHoles(myDoc.ModelRelativeTolerance);
                    var guiderFinal = Brep.CreateBooleanIntersection(outerGuider, guiderCenter, myDoc.ModelRelativeTolerance)[0];
                    myDoc.Objects.Add(guiderFinal);
                    #endregion

                    #region bend part

                    axis = -endSuf.Normal;

                    #region construct the outer rectangle of each hinge
                    var attributes = new ObjectAttributes();
                    attributes.ObjectColor = Color.Purple;
                    attributes.ColorSource = ObjectColorSource.ColorFromObject;
                    normal_dir.Rotate(Math.PI / 2, axis);
                    Point3d hingePt1 = endPt + normal_dir / 3;
                    Point3d hingeInnerPt1 = endPt + normal_dir / 5;
                    normal_dir.Rotate(Math.PI, axis);
                    Point3d hingePt2 = endPt + normal_dir / 3;
                    Point3d hingeInnerPt2 = endPt + normal_dir / 5;
                    double scale = (cylinCrv.GetLength() / 8 / 2) / axis.Length;   // 8 is the number of hinge
                    Point3d hingePt3 = hingePt1 + axis * scale;
                    Point3d hingePt4 = hingePt2 + axis * scale;

                    Point3d[] hingeOuterCornerPt = new Point3d[5];
                    hingeOuterCornerPt[0] = hingePt1;
                    hingeOuterCornerPt[1] = hingePt2;
                    hingeOuterCornerPt[2] = hingePt4;
                    hingeOuterCornerPt[3] = hingePt3;
                    hingeOuterCornerPt[4] = hingePt1;
                    Curve hingeOuterRectCrv = new Polyline(hingeOuterCornerPt).ToNurbsCurve();
                    //myDoc.Objects.AddCurve(hingeOuterRectCrv, attributes);
                    //myDoc.Views.Redraw();
                    #endregion

                    #region construct the inner rectangle of each hinge
                    double scale1 = scale / 2;
                    double scale2 = scale / 4;
                    Point3d[] hingeInnerCornerPt = new Point3d[5];
                    hingeInnerCornerPt[0] = hingeInnerPt1 + axis * scale2;
                    hingeInnerCornerPt[1] = hingeInnerPt2 + axis * scale2;
                    hingeInnerCornerPt[2] = hingeInnerPt2 + axis * scale2 + axis * scale1;
                    hingeInnerCornerPt[3] = hingeInnerPt1 + axis * scale2 + axis * scale1;
                    hingeInnerCornerPt[4] = hingeInnerPt1 + axis * scale2;
                    Curve hingeInnerRectCrv = new Polyline(hingeInnerCornerPt).ToNurbsCurve();
                    //myDoc.Objects.AddCurve(hingeInnerRectCrv, attributes);
                    //myDoc.Views.Redraw();
                    #endregion

                    #region Array all outer and inner rectangles of the hinge along the curve

                    #region Divide the curve by N points
                    // front and rear portions that need to be removed from the center curve
                    double hingeEndPara;
                    splitedLeftCrvs[0].LengthParameter(splitedLeftCrvs[0].GetLength() - 1.5 * endSuf.Normal.Length * scale1, out hingeEndPara);
                    Point3d end_pt = splitedLeftCrvs[0].PointAt(hingeEndPara);

                    //myDoc.Objects.AddPoint(end_pt, attributes);
                    //myDoc.Views.Redraw();

                    Point3d front = end_pt;
                    Point3d end = startPt + startSuf.Normal * scale1;
                    double frontCrvPara = 0;
                    cylinCrv.ClosestPoint(front, out frontCrvPara);
                    Curve[] splitCrvs = cylinCrv.Split(frontCrvPara);
                    double endCrvPara = 0;
                    splitCrvs[0].ClosestPoint(end, out endCrvPara);
                    Curve[] splitCrvs1 = splitCrvs[0].Split(endCrvPara);
                    Curve divideCrv = splitCrvs1[1];

                    attributes.ObjectColor = Color.Yellow;

                    // store all curve segments
                    Point3d[] ps;
                    List<Point3d> points = new List<Point3d>();
                    divideCrv.DivideByCount(8, true, out ps); // 8 is the number of hinge
                    for (int j = ps.Count() - 1; j >= 0; j--)
                    {
                        points.Add(ps[j]);
                    }

                    // store tangent vectors at each point
                    List<Vector3d> tangents = new List<Vector3d>();
                    foreach (Point3d p in points)
                    {
                        double para = 0;
                        divideCrv.ClosestPoint(p, out para);
                        tangents.Add(divideCrv.TangentAt(para) * (-1));
                        //myDoc.Objects.AddPoint(p, attributes);
                    }

                    // store transforms from the end point to each point
                    List<List<Transform>> trans = new List<List<Transform>>();
                    double initPara = 0;
                    centerCrv.ClosestPoint(endPt, out initPara);
                    Vector3d v0 = centerCrv.TangentAt(initPara)*(-1);
                    Point3d p0 = endPt;
                    int idx = 0;
                    foreach (Vector3d v1 in tangents)
                    {
                        Transform translate = Transform.Translation(points.ElementAt(idx) - p0);
                        Transform rotate = Transform.Rotation(v0, v1, points.ElementAt(idx));
                        List<Transform> tr = new List<Transform>();
                        tr.Add(translate);
                        tr.Add(rotate);
                        trans.Add(tr);
                        idx++;
                    }

                    // create all outer and inner ractangles along the curve
                    List<Curve> outerCrvs = new List<Curve>();
                    List<Curve> innerCrvs = new List<Curve>();
                    foreach (List<Transform> tr in trans)
                    {
                        Curve tempOuterCrv = hingeOuterRectCrv.DuplicateCurve();
                        tempOuterCrv.Transform(tr.ElementAt(0));
                        tempOuterCrv.Transform(tr.ElementAt(1));
                        outerCrvs.Add(tempOuterCrv);

                        Curve tempInnerCrv = hingeInnerRectCrv.DuplicateCurve();
                        tempInnerCrv.Transform(tr.ElementAt(0));
                        tempInnerCrv.Transform(tr.ElementAt(1));
                        innerCrvs.Add(tempInnerCrv);

                        //myDoc.Objects.AddCurve(tempOuterCrv, attributes);
                        //myDoc.Objects.AddCurve(tempInnerCrv, attributes);
                        //myDoc.Views.Redraw();
                    }
                    #endregion

                    #region extrude the arrays of rectangles toward both sides
                    List<Brep> outerBreps = new List<Brep>();
                    List<Brep> innerBreps = new List<Brep>();
                    //List<Brep> innerBrepsDup = new List<Brep>();

                    foreach (Curve c in outerCrvs)
                    {

                        Surface surf = Brep.CreatePlanarBreps(c)[0].Faces[0];
                        double wde;
                        double hgt;
                        surf.GetSurfaceSize(out wde, out hgt);
                        Vector3d hinge_normal = surf.NormalAt(wde / 2, hgt / 2);
                        double s = 1 / hinge_normal.Length; // 3 is the thickness of the hinge 
                        Transform rectTrans = Transform.Translation(hinge_normal * s);
                        c.Transform(rectTrans);

                        Brep brep = Brep.CreateFromSurface(Surface.CreateExtrusion(c, -2 * hinge_normal * s));
                        brep = brep.CapPlanarHoles(myDoc.ModelRelativeTolerance);

                        outerBreps.Add(brep);
                    }

                    foreach (Curve c in innerCrvs)
                    {

                        Surface surf = Brep.CreatePlanarBreps(c)[0].Faces[0];
                        double wde;
                        double hgt;
                        surf.GetSurfaceSize(out wde, out hgt);
                        Vector3d hinge_normal = surf.NormalAt(wde / 2, hgt / 2);
                        double s = 2 / hinge_normal.Length; // 3 is the thickness of the hinge 
                        Transform rectTrans = Transform.Translation(hinge_normal * s);
                        c.Transform(rectTrans);

                        Brep brep = Brep.CreateFromSurface(Surface.CreateExtrusion(c, -2 * hinge_normal * s));
                        brep = brep.CapPlanarHoles(myDoc.ModelRelativeTolerance);

                        innerBreps.Add(brep);
                    }

                    #endregion

                    #endregion

                    #region boolean difference
                    // generate the central connections
                    List<Brep> b_list = new List<Brep>();
                    cylinBrep.Flip();
                    Brep prev_brep = cylinBrep;

                    for (int id = 0; id < innerBreps.Count(); id++)
                    {
                        if(id == 0)
                        {
                            var tempB = Brep.CreateBooleanDifference(innerBreps[id], prev_brep, myDoc.ModelRelativeTolerance);
                            myDoc.Objects.AddBrep(tempB[0], attributes);
                            tempB[1].Flip();
                            prev_brep = tempB[1];
                        }
                        else
                        {
                            var tempB = Brep.CreateBooleanDifference(innerBreps[id], prev_brep, myDoc.ModelRelativeTolerance);
                            if(tempB.Count() == 2)
                            {
                                myDoc.Objects.AddBrep(tempB[1], attributes);
                                tempB[0].Flip();
                                prev_brep = tempB[0];
                            }
                            else if (tempB.Count() == 1)
                            {
                                myDoc.Objects.AddBrep(tempB[0], attributes);
                                tempB[0].Flip();
                                prev_brep = tempB[0];
                            }
                            
                        }
                    }
                    myDoc.Objects.AddBrep(prev_brep, attributes);

                    // generate the hinges
                    Surface initOuterSurf = Brep.CreatePlanarBreps(hingeOuterRectCrv)[0].Faces[0];
                    double initOuterWde;
                    double initOuterhgt;
                    initOuterSurf.GetSurfaceSize(out initOuterWde, out initOuterhgt);
                    Vector3d hinge_normal1 = initOuterSurf.NormalAt(initOuterWde / 2, initOuterhgt / 2);
                    double initOuters = 1 / hinge_normal1.Length; // 3 is the thickness of the hinge 
                    Transform rectTranS = Transform.Translation(hinge_normal1 * initOuters);
                    hingeOuterRectCrv.Transform(rectTranS);

                    Brep hingeOuterBrep = Brep.CreateFromSurface(Surface.CreateExtrusion(hingeOuterRectCrv, -2 * hinge_normal1 * initOuters));
                    hingeOuterBrep = hingeOuterBrep.CapPlanarHoles(myDoc.ModelRelativeTolerance);

                    Surface initInnerSurf = Brep.CreatePlanarBreps(hingeInnerRectCrv)[0].Faces[0];
                    double initInnerWde;
                    double initInnerhgt;
                    initInnerSurf.GetSurfaceSize(out initInnerWde, out initInnerhgt);
                    Vector3d hinge_normal2 = initInnerSurf.NormalAt(initInnerWde / 2, initInnerhgt / 2);
                    double initInners = 1 / hinge_normal2.Length; // 3 is the thickness of the hinge 
                    rectTranS = Transform.Translation(hinge_normal2 * initInners);
                    hingeInnerRectCrv.Transform(rectTranS);

                    Brep hingeInnerBrep = Brep.CreateFromSurface(Surface.CreateExtrusion(hingeInnerRectCrv, -2 * hinge_normal2 * initInners));
                    hingeInnerBrep = hingeInnerBrep.CapPlanarHoles(myDoc.ModelRelativeTolerance);

                    var firstHinge = Brep.CreateBooleanDifference(hingeInnerBrep, hingeOuterBrep, myDoc.ModelRelativeTolerance);

                    foreach (List<Transform> tr in trans)
                    {

                        Brep tempBrep = firstHinge[0].DuplicateBrep();
                        tempBrep.Transform(tr.ElementAt(0));
                        tempBrep.Transform(tr.ElementAt(1));
                        myDoc.Objects.AddBrep(tempBrep, attributes);
                    }

                    #endregion

                    #endregion
                }
            }

            myDoc.Views.Redraw();
        }

        void computeLatticeHinges(double crvLen, out double actualDegree, double thickness, out int hingeNum, double sConnectionLen, double inputDegree)
        {
            // Give an initial max flexural strength (tortional yield stress): 80MPa, tortional modulus: 4000MPa. More information: https://www.makeitfrom.com/material-properties/Polylactic-Acid-PLA-Polylactide
            double tmax = 80;
            double G_tmax = 4000/tmax;
            int num;
            double degree;

            // n*l = angle*G*thickness*0.676125/tmax. More information: http://www.deferredprocrastination.co.uk/blog/2011/laser-cut-lattice-living-hinges/
            // minimum gap is 0.8mm from the test
            double minGap = 0.8;

            num = (int)(G_tmax * thickness * 0.676125 * inputDegree / sConnectionLen);
            degree = inputDegree;

            while(crvLen/num-2 <= minGap)
            {
                num -= 1;
                degree = ((double)num * sConnectionLen) / (G_tmax * thickness * 0.676125);
            }

            hingeNum = num;
            actualDegree = degree;
        }
        private void generateBendSupport(Plane startSuf, Plane endSuf, Curve centerCrv, List<bend_info> bendInfoList, Point3d startPt, Point3d endPt, bool isStart, out List<Brep> centrShaft, out List<Brep> hinges)
        {
            centrShaft = new List<Brep>();
            hinges = new List<Brep>();
            foreach (bend_info b in bendInfoList)
            {
                Vector3d normal_dir = b.dir;
                Vector3d axis;
                if (isStart)
                {
                    #region start side
                    axis = startSuf.Normal;

                    int hingeNum = 0;
                    double angle = 0;
                    double inputDegree = Math.PI/6;
                    double thickness = 2;

                    double minDia = 100000000;
                    double barLen = -1;
                    foreach (double d in DiameterList)
                    {
                        if (d <= minDia) minDia = d;
                    }
                    if ((minDia - 4) > 1)
                    {
                        barLen = (minDia - 3) / 2;
                    }
                    else
                    {
                        barLen = -1;
                    }

                    if(barLen != -1)
                    {
                        computeLatticeHinges(centerCrv.GetLength(), out angle, thickness, out hingeNum, barLen, inputDegree);
                        hingeNum--;
                        #region construct the outer rectangle of each hinge
                        var attributes = new ObjectAttributes();
                        attributes.ObjectColor = Color.Purple;
                        attributes.ColorSource = ObjectColorSource.ColorFromObject;
                        normal_dir.Rotate(Math.PI / 2, axis);
                        Point3d hingePt1 = startPt + (barLen+1) / normal_dir.Length * normal_dir;
                        Point3d hingeInnerPt1 = startPt + barLen/normal_dir.Length* normal_dir;
                        normal_dir.Rotate(Math.PI, axis);
                        Point3d hingePt2 = startPt + (barLen + 1) / normal_dir.Length * normal_dir;
                        Point3d hingeInnerPt2 = startPt + barLen / normal_dir.Length * normal_dir;

                        double scale = 0;
                        if (centerCrv.GetLength() / hingeNum > 3.6)
                        {
                            scale = ((centerCrv.GetLength() / hingeNum - 2) / 2 + 2) / axis.Length;
                        }
                        else
                        {
                            scale = 2.8 / axis.Length;
                        }

                        Point3d hingePt3 = hingePt1 + axis * scale;
                        Point3d hingePt4 = hingePt2 + axis * scale;

                        Point3d[] hingeOuterCornerPt = new Point3d[5];
                        hingeOuterCornerPt[0] = hingePt1;
                        hingeOuterCornerPt[1] = hingePt2;
                        hingeOuterCornerPt[2] = hingePt4;
                        hingeOuterCornerPt[3] = hingePt3;
                        hingeOuterCornerPt[4] = hingePt1;
                        Curve hingeOuterRectCrv = new Polyline(hingeOuterCornerPt).ToNurbsCurve();
                        //myDoc.Objects.AddCurve(hingeOuterRectCrv, attributes);
                        //myDoc.Views.Redraw();
                        #endregion

                        #region construct the inner rectangle of each hinge
                        double scale1 = scale-2/axis.Length;
                        double scale2 = 1/axis.Length;
                        Point3d[] hingeInnerCornerPt = new Point3d[5];
                        hingeInnerCornerPt[0] = hingeInnerPt1 + axis * scale2;
                        hingeInnerCornerPt[1] = hingeInnerPt2 + axis * scale2;
                        hingeInnerCornerPt[2] = hingeInnerPt2 + axis * scale2 + axis * scale1;
                        hingeInnerCornerPt[3] = hingeInnerPt1 + axis * scale2 + axis * scale1;
                        hingeInnerCornerPt[4] = hingeInnerPt1 + axis * scale2;
                        Curve hingeInnerRectCrv = new Polyline(hingeInnerCornerPt).ToNurbsCurve();
                        //myDoc.Objects.AddCurve(hingeInnerRectCrv, attributes);
                        //myDoc.Views.Redraw();
                        #endregion

                        #region Array all outer and inner rectangles of the hinge along the curve

                        #region Divide the curve by N points
                        // front and rear portions that need to be removed from the center curve
                        Point3d front = startPt + axis * scale1;
                        Point3d end = endPt - endSuf.Normal * scale1;
                        double frontCrvPara = 0;
                        centerCrv.ClosestPoint(front, out frontCrvPara);
                        Curve[] splitCrvs = centerCrv.Split(frontCrvPara);
                        double endCrvPara = 0;
                        splitCrvs[1].ClosestPoint(end, out endCrvPara);
                        Curve[] splitCrvs1 = splitCrvs[1].Split(endCrvPara);
                        Curve divideCrv = splitCrvs1[0];

                        // store all curve segments
                        Point3d[] points;
                        divideCrv.DivideByCount(hingeNum, true, out points); // 8 is the number of hinge

                        // store tangent vectors at each point
                        List<Vector3d> tangents = new List<Vector3d>();
                        foreach (Point3d p in points)
                        {
                            double para = 0;
                            divideCrv.ClosestPoint(p, out para);
                            tangents.Add(divideCrv.TangentAt(para));
                            //myDoc.Objects.AddPoint(p, attributes);
                        }

                        // store transforms from the first point to each point
                        List<List<Transform>> trans = new List<List<Transform>>();
                        Vector3d v0 = tangents.ElementAt(0);
                        Point3d p0 = points.ElementAt(0);
                        int idx = 0;
                        foreach (Vector3d v1 in tangents)
                        {
                            Transform translate = Transform.Translation(points.ElementAt(idx) - p0);
                            Transform rotate = Transform.Rotation(v0, v1, points.ElementAt(idx));
                            List<Transform> t = new List<Transform>();
                            t.Add(translate);
                            t.Add(rotate);
                            trans.Add(t);
                            idx++;
                        }

                        // create all outer and inner ractangles along the curve
                        List<Curve> outerCrvs = new List<Curve>();
                        List<Curve> innerCrvs = new List<Curve>();
                        foreach (List<Transform> t in trans)
                        {
                            Curve tempOuterCrv = hingeOuterRectCrv.DuplicateCurve();
                            tempOuterCrv.Transform(t.ElementAt(0));
                            tempOuterCrv.Transform(t.ElementAt(1));
                            outerCrvs.Add(tempOuterCrv);

                            Curve tempInnerCrv = hingeInnerRectCrv.DuplicateCurve();
                            tempInnerCrv.Transform(t.ElementAt(0));
                            tempInnerCrv.Transform(t.ElementAt(1));
                            innerCrvs.Add(tempInnerCrv);

                            //myDoc.Objects.AddCurve(tempOuterCrv, attributes);
                            //myDoc.Objects.AddCurve(tempInnerCrv, attributes);
                        }
                        #endregion

                        #region extrude the arrays of rectangles toward both sides
                        List<Brep> outerBreps = new List<Brep>();
                        List<Brep> innerBreps = new List<Brep>();
                        //List<Brep> innerBrepsDup = new List<Brep>();

                        foreach (Curve c in outerCrvs)
                        {

                            Surface surf = Brep.CreatePlanarBreps(c)[0].Faces[0];
                            double wde;
                            double hgt;
                            surf.GetSurfaceSize(out wde, out hgt);
                            Vector3d hinge_normal = surf.NormalAt(wde / 2, hgt / 2);
                            double s = 1 / hinge_normal.Length; // 3 is the thickness of the hinge 
                            Transform rectTrans = Transform.Translation(hinge_normal * s);
                            c.Transform(rectTrans);

                            Brep brep = Brep.CreateFromSurface(Surface.CreateExtrusion(c, -2 * hinge_normal * s));
                            brep = brep.CapPlanarHoles(myDoc.ModelRelativeTolerance);

                            outerBreps.Add(brep);

                            //myDoc.Objects.AddBrep(brep, attributes);
                            //myDoc.Views.Redraw();
                        }

                        foreach (Curve c in innerCrvs)
                        {

                            Surface surf = Brep.CreatePlanarBreps(c)[0].Faces[0];
                            double wde;
                            double hgt;
                            surf.GetSurfaceSize(out wde, out hgt);
                            Vector3d hinge_normal = surf.NormalAt(wde / 2, hgt / 2);
                            double s = 3 / hinge_normal.Length; // 3 is the thickness of the hinge 
                            Transform rectTrans = Transform.Translation(hinge_normal * s);
                            c.Transform(rectTrans);

                            Brep brep = Brep.CreateFromSurface(Surface.CreateExtrusion(c, -2 * hinge_normal * s));
                            brep = brep.CapPlanarHoles(myDoc.ModelRelativeTolerance);

                            innerBreps.Add(brep);

                            //myDoc.Objects.AddBrep(brep, attributes);
                            //myDoc.Views.Redraw();
                        }

                        #endregion

                        #endregion

                        #region sweep the central rod
                        double half_width_scale = 0.75 / normal_dir.Length; // 1 is the half width of the central rod, adjustable
                        double half_height_scale = 0.5 / b.dir.Length; // 2 is the half width of the central rod, adjustable, equal to the extusion height
                        Point3d rodPt1 = startPt + normal_dir * half_width_scale + b.dir * half_height_scale;
                        normal_dir.Rotate(Math.PI, axis);
                        Point3d rodPt2 = startPt + normal_dir * half_width_scale + b.dir * half_height_scale;
                        Point3d rodPt3 = rodPt2 - b.dir * 2 * half_height_scale;
                        Point3d rodPt4 = rodPt1 - b.dir * 2 * half_height_scale;


                        Point3d[] rodCornerPt = new Point3d[5];
                        rodCornerPt[0] = rodPt1;
                        rodCornerPt[1] = rodPt2;
                        rodCornerPt[2] = rodPt3;
                        rodCornerPt[3] = rodPt4;
                        rodCornerPt[4] = rodPt1;
                        Curve rodRectCrv = new Polyline(rodCornerPt).ToNurbsCurve();

                        attributes.ObjectColor = Color.Yellow;
                        myDoc.Objects.AddCurve(rodRectCrv, attributes);
                        var sweep = new Rhino.Geometry.SweepOneRail();
                        sweep.AngleToleranceRadians = myDoc.ModelAngleToleranceRadians;
                        sweep.ClosedSweep = false;
                        sweep.SweepTolerance = myDoc.ModelRelativeTolerance;

                        Brep[] rod_brep = sweep.PerformSweep(centerCrv, rodRectCrv);

                        rod_brep[0] = rod_brep[0].CapPlanarHoles(myDoc.ModelRelativeTolerance);
                        //myDoc.Objects.AddBrep(rod_brep[0], attributes);
                        //myDoc.Views.Redraw();

                        #endregion

                        #region boolean difference
                        // generate the central connections
                        List<Brep> b_list = new List<Brep>();
                        rod_brep[0].Flip();
                        Brep prev_brep = rod_brep[0];

                        for (int id = 0; id < innerBreps.Count(); id++)
                        {
                            var tempB = Brep.CreateBooleanDifference(innerBreps[id], prev_brep, myDoc.ModelRelativeTolerance);
                            if(tempB != null)
                            {
                                if (tempB.Count() == 2)
                                {
                                    myDoc.Objects.AddBrep(tempB[1], attributes);
                                    centrShaft.Add(tempB[1]);
                                    tempB[0].Flip();
                                    prev_brep = tempB[0];
                                }
                                else if (tempB.Count() == 1)
                                {
                                    myDoc.Objects.AddBrep(tempB[0], attributes);
                                    centrShaft.Add(tempB[0]);
                                    tempB[0].Flip();
                                    prev_brep = tempB[0];
                                }

                            }
                            myDoc.Views.Redraw();
                        }

                        // generate the hinges
                        //innerBreps[0].Flip();
                        var firstHinge = Brep.CreateBooleanDifference(innerBreps[0], outerBreps[0], myDoc.ModelRelativeTolerance);
                        myDoc.Objects.AddBrep(firstHinge[0], attributes);
                        //myDoc.Views.Redraw();

                        hinges.Add(firstHinge[0]);
                        foreach (List<Transform> t in trans)
                        {
                            if (trans.IndexOf(t) != 0)
                            {
                                Brep tempBrep = firstHinge[0].DuplicateBrep();
                                tempBrep.Transform(t.ElementAt(0));
                                tempBrep.Transform(t.ElementAt(1));
                                myDoc.Objects.AddBrep(tempBrep, attributes);

                                hinges.Add(tempBrep);
                            }

                        }

                        #endregion
                    }

                    #endregion
                }
                else
                {
                    axis = -endSuf.Normal;

                    int hingeNum = 0;
                    double angle = 0;
                    double inputDegree = Math.PI / 3;
                    double thickness = 2;

                    double minDia = 100000000;
                    double barLen = -1;
                    foreach (double d in DiameterList)
                    {
                        if (d <= minDia) minDia = d;
                    }
                    if ((minDia - 5) > 1)
                    {
                        barLen = (minDia - 3) / 2;
                    }
                    else
                    {
                        barLen = -1;
                    }

                    if (barLen != -1)
                    {
                        computeLatticeHinges(centerCrv.GetLength(), out angle, thickness, out hingeNum, barLen, inputDegree);
                        hingeNum--;

                        #region construct the outer rectangle of each hinge
                        var attributes = new ObjectAttributes();
                        attributes.ObjectColor = Color.Purple;
                        attributes.ColorSource = ObjectColorSource.ColorFromObject;
                        normal_dir.Rotate(Math.PI / 2, axis);
                        Point3d hingePt1 = endPt + (barLen + 1) / normal_dir.Length * normal_dir;
                        Point3d hingeInnerPt1 = endPt + barLen / normal_dir.Length * normal_dir;
                        normal_dir.Rotate(Math.PI, axis);
                        Point3d hingePt2 = endPt + (barLen + 1) / normal_dir.Length * normal_dir;
                        Point3d hingeInnerPt2 = endPt + barLen / normal_dir.Length * normal_dir;

                        double scale = 0;
                        if (centerCrv.GetLength() / hingeNum > 3.6)
                        {
                            scale = ((centerCrv.GetLength() / hingeNum - 2) / 2 + 2) / axis.Length;
                        }
                        else
                        {
                            scale = 2.8 / axis.Length;
                        }

                        Point3d hingePt3 = hingePt1 + axis * scale;
                        Point3d hingePt4 = hingePt2 + axis * scale;

                        Point3d[] hingeOuterCornerPt = new Point3d[5];
                        hingeOuterCornerPt[0] = hingePt1;
                        hingeOuterCornerPt[1] = hingePt2;
                        hingeOuterCornerPt[2] = hingePt4;
                        hingeOuterCornerPt[3] = hingePt3;
                        hingeOuterCornerPt[4] = hingePt1;
                        Curve hingeOuterRectCrv = new Polyline(hingeOuterCornerPt).ToNurbsCurve();
                        //myDoc.Objects.AddCurve(hingeOuterRectCrv, attributes);
                        //myDoc.Views.Redraw();
                        #endregion

                        #region construct the inner rectangle of each hinge
                        double scale1 = scale - 2 / axis.Length;
                        double scale2 = 1 / axis.Length;
                        Point3d[] hingeInnerCornerPt = new Point3d[5];
                        hingeInnerCornerPt[0] = hingeInnerPt1 + axis * scale2;
                        hingeInnerCornerPt[1] = hingeInnerPt2 + axis * scale2;
                        hingeInnerCornerPt[2] = hingeInnerPt2 + axis * scale2 + axis * scale1;
                        hingeInnerCornerPt[3] = hingeInnerPt1 + axis * scale2 + axis * scale1;
                        hingeInnerCornerPt[4] = hingeInnerPt1 + axis * scale2;
                        Curve hingeInnerRectCrv = new Polyline(hingeInnerCornerPt).ToNurbsCurve();
                        //myDoc.Objects.AddCurve(hingeInnerRectCrv, attributes);
                        //myDoc.Views.Redraw();
                        #endregion

                        #region Array all outer and inner rectangles of the hinge along the curve

                        #region Divide the curve by N points
                        // front and rear portions that need to be removed from the center curve
                        Point3d front = endPt + axis * scale1;
                        Point3d end = startPt + startSuf.Normal * scale1;
                        double frontCrvPara = 0;
                        centerCrv.ClosestPoint(front, out frontCrvPara);
                        Curve[] splitCrvs = centerCrv.Split(frontCrvPara);
                        double endCrvPara = 0;
                        splitCrvs[0].ClosestPoint(end, out endCrvPara);
                        Curve[] splitCrvs1 = splitCrvs[0].Split(endCrvPara);
                        Curve divideCrv = splitCrvs1[1];

                        attributes.ObjectColor = Color.Yellow;

                        // store all curve segments
                        Point3d[] ps;
                        List<Point3d> points = new List<Point3d>();
                        divideCrv.DivideByCount(hingeNum, true, out ps); // 8 is the number of hinge
                        for (int j = ps.Count() - 1; j >= 0; j--)
                        {
                            points.Add(ps[j]);
                        }

                        // store tangent vectors at each point
                        List<Vector3d> tangents = new List<Vector3d>();
                        foreach (Point3d p in points)
                        {
                            double para = 0;
                            divideCrv.ClosestPoint(p, out para);
                            tangents.Add(divideCrv.TangentAt(para) * (-1));
                            //myDoc.Objects.AddPoint(p, attributes);
                        }

                        // store transforms from the first point to each point
                        List<List<Transform>> trans = new List<List<Transform>>();
                        Vector3d v0 = tangents.ElementAt(0);
                        Point3d p0 = points.ElementAt(0);
                        int idx = 0;
                        foreach (Vector3d v1 in tangents)
                        {
                            Transform translate = Transform.Translation(points.ElementAt(idx) - p0);
                            Transform rotate = Transform.Rotation(v0, v1, points.ElementAt(idx));
                            List<Transform> t = new List<Transform>();
                            t.Add(translate);
                            t.Add(rotate);
                            trans.Add(t);
                            idx++;
                        }

                        // create all outer and inner ractangles along the curve
                        List<Curve> outerCrvs = new List<Curve>();
                        List<Curve> innerCrvs = new List<Curve>();
                        foreach (List<Transform> t in trans)
                        {
                            Curve tempOuterCrv = hingeOuterRectCrv.DuplicateCurve();
                            tempOuterCrv.Transform(t.ElementAt(0));
                            tempOuterCrv.Transform(t.ElementAt(1));
                            outerCrvs.Add(tempOuterCrv);

                            Curve tempInnerCrv = hingeInnerRectCrv.DuplicateCurve();
                            tempInnerCrv.Transform(t.ElementAt(0));
                            tempInnerCrv.Transform(t.ElementAt(1));
                            innerCrvs.Add(tempInnerCrv);

                            //myDoc.Objects.AddCurve(tempOuterCrv, attributes);
                            //myDoc.Objects.AddCurve(tempInnerCrv, attributes);
                            //myDoc.Views.Redraw();
                        }
                        #endregion

                        #region extrude the arrays of rectangles toward both sides
                        List<Brep> outerBreps = new List<Brep>();
                        List<Brep> innerBreps = new List<Brep>();
                        //List<Brep> innerBrepsDup = new List<Brep>();

                        foreach (Curve c in outerCrvs)
                        {

                            Surface surf = Brep.CreatePlanarBreps(c)[0].Faces[0];
                            double wde;
                            double hgt;
                            surf.GetSurfaceSize(out wde, out hgt);
                            Vector3d hinge_normal = surf.NormalAt(wde / 2, hgt / 2);
                            double s = 1 / hinge_normal.Length; // 3 is the thickness of the hinge 
                            Transform rectTrans = Transform.Translation(hinge_normal * s);
                            c.Transform(rectTrans);

                            Brep brep = Brep.CreateFromSurface(Surface.CreateExtrusion(c, -2 * hinge_normal * s));
                            brep = brep.CapPlanarHoles(myDoc.ModelRelativeTolerance);

                            outerBreps.Add(brep);
                        }

                        foreach (Curve c in innerCrvs)
                        {

                            Surface surf = Brep.CreatePlanarBreps(c)[0].Faces[0];
                            double wde;
                            double hgt;
                            surf.GetSurfaceSize(out wde, out hgt);
                            Vector3d hinge_normal = surf.NormalAt(wde / 2, hgt / 2);
                            double s = 3 / hinge_normal.Length; // 3 is the thickness of the hinge 
                            Transform rectTrans = Transform.Translation(hinge_normal * s);
                            c.Transform(rectTrans);

                            Brep brep = Brep.CreateFromSurface(Surface.CreateExtrusion(c, -2 * hinge_normal * s));
                            brep = brep.CapPlanarHoles(myDoc.ModelRelativeTolerance);

                            innerBreps.Add(brep);
                        }

                        //// prepared for difference boolean with the central rod
                        //innerBrepsDup = innerBreps;

                        #endregion

                        #endregion

                        #region sweep the central rod
                        double half_width_scale = 1.5 / normal_dir.Length; // 1 is the half width of the central rod, adjustable
                        double half_height_scale = 1.5 / b.dir.Length; // 2 is the half width of the central rod, adjustable, equal to the extusion height
                        Point3d rodPt1 = endPt + normal_dir * half_width_scale + b.dir * half_height_scale;
                        normal_dir.Rotate(Math.PI, axis);
                        Point3d rodPt2 = endPt + normal_dir * half_width_scale + b.dir * half_height_scale;
                        Point3d rodPt3 = rodPt2 - b.dir * 2 * half_height_scale;
                        Point3d rodPt4 = rodPt1 - b.dir * 2 * half_height_scale;


                        Point3d[] rodCornerPt = new Point3d[5];
                        rodCornerPt[0] = rodPt1;
                        rodCornerPt[1] = rodPt2;
                        rodCornerPt[2] = rodPt3;
                        rodCornerPt[3] = rodPt4;
                        rodCornerPt[4] = rodPt1;
                        Curve rodRectCrv = new Polyline(rodCornerPt).ToNurbsCurve();

                        attributes.ObjectColor = Color.Yellow;
                        myDoc.Objects.AddCurve(rodRectCrv, attributes);
                        var sweep = new Rhino.Geometry.SweepOneRail();
                        sweep.AngleToleranceRadians = myDoc.ModelAngleToleranceRadians;
                        sweep.ClosedSweep = false;
                        sweep.SweepTolerance = myDoc.ModelRelativeTolerance;

                        Brep[] rod_brep = sweep.PerformSweep(centerCrv, rodRectCrv);

                        rod_brep[0] = rod_brep[0].CapPlanarHoles(myDoc.ModelRelativeTolerance);
                        //myDoc.Objects.AddBrep(rod_brep[0], attributes);


                        #endregion

                        #region boolean difference
                        // generate the central connections
                        List<Brep> b_list = new List<Brep>();
                        rod_brep[0].Flip();
                        Brep prev_brep = rod_brep[0];

                        for (int id = 0; id < innerBreps.Count(); id++)
                        {
                            var tempB = Brep.CreateBooleanDifference(innerBreps[id], prev_brep, myDoc.ModelRelativeTolerance);

                            if (tempB != null)
                            {
                                if (tempB.Count() == 2)
                                {
                                    myDoc.Objects.AddBrep(tempB[1], attributes);
                                    centrShaft.Add(tempB[1]);
                                    tempB[0].Flip();
                                    prev_brep = tempB[0];
                                }
                                else if (tempB.Count() == 1)
                                {
                                    myDoc.Objects.AddBrep(tempB[0], attributes);
                                    centrShaft.Add(tempB[0]);
                                    tempB[0].Flip();
                                    prev_brep = tempB[0];
                                }

                            }

                            //myDoc.Views.Redraw();
                        }

                        // generate the hinges
                        //innerBreps[0].Flip();
                        var firstHinge = Brep.CreateBooleanDifference(innerBreps[0], outerBreps[0], myDoc.ModelRelativeTolerance);
                        myDoc.Objects.AddBrep(firstHinge[0], attributes);
                        myDoc.Views.Redraw();

                        hinges.Add(firstHinge[0]);

                        foreach (List<Transform> t in trans)
                        {
                            if (trans.IndexOf(t) != 0)
                            {
                                Brep tempBrep = firstHinge[0].DuplicateBrep();
                                tempBrep.Transform(t.ElementAt(0));
                                tempBrep.Transform(t.ElementAt(1));
                                myDoc.Objects.AddBrep(tempBrep, attributes);

                                hinges.Add(tempBrep);
                            }

                        }

                        #endregion
                    }


                }

                myDoc.Views.Redraw();
            }
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
        private double distanceBtwTwoPts(Point3d pt1, Point3d pt2)
        {
            double dis = 0;
            dis = Math.Sqrt(Math.Pow(pt1.X - pt2.X, 2) + Math.Pow(pt1.Y - pt2.Y, 2) + Math.Pow(pt1.Z - pt2.Z, 2));
            return dis;
        }
        private void Gp_BendAngleSelMouseMove(object sender, Rhino.Input.Custom.GetPointMouseEventArgs e)
        {
            
            Point3d tp = e.Point;
            // RhinoApp.WriteLine("current point's position: (" + this.bendCtrlPt.X + ", " + this.bendCtrlPt.Y + ", " + this.bendCtrlPt.Z + ")");
            this.bendCtrlPt = this.angleCircle.ClosestPoint(tp);
        }
        private void Gp_BendAngelSelDynamicDraw(object sender, Rhino.Input.Custom.GetPointDrawEventArgs e)
        {
            // to be added: draw the sphere around the circle to indicate the direction
            e.Display.DrawSphere(new Sphere(this.bendCtrlPt, 1.5), Color.White);
        }
        private void Gp_BendStrengthSelMouseMove(object sender, Rhino.Input.Custom.GetPointMouseEventArgs e)
        {
            // to be added: compute the distance from the selected point
            Point3d tempPt = e.Point;
            this.bendCtrlSecPt = e.Point;
            // may be listen to the 'Enter' key press event here?
        }
        private void Gp_BendStrengthSelDynamicDraw(object sender, Rhino.Input.Custom.GetPointDrawEventArgs e)
        {
            // to be added: compute the bending strength based on the distance and draw the line and arrow
            e.Display.DrawArrow(new Line(this.bendCtrlPt, this.bendCtrlSecPt), Color.Yellow);
        }
        private void Gp_sphereSelMouseMove(object sender, Rhino.Input.Custom.GetPointMouseEventArgs e)
        {
            this.bendPtSel = e.Point;
            //RhinoApp.WriteLine("current point's position: (" + this.bendPtSel.X + ", " + this.bendPtSel.Y + ", " + this.bendPtSel.Z + ")");
        }
        private void Gp_sphereSelDynamicDraw(object sender, Rhino.Input.Custom.GetPointDrawEventArgs e)
        {
            if (isBendLinear)
            {
                if (distanceBtwTwoPts(this.bendPtSel, this.centerCrv.PointAtEnd) <=1.5)
                {
                    // hightlight the end sphere
                    e.Display.DrawSphere(new Sphere(this.centerCrv.PointAtEnd, 1.5), Color.Yellow);
                }
                else
                {
                    e.Display.DrawSphere(new Sphere(this.centerCrv.PointAtEnd, 1.5), Color.White);
                }
            }
            else
            {
                if (distanceBtwTwoPts(this.bendPtSel, this.centerCrv.PointAtStart) <= 1.5)
                {
                    // highlight the start sphere
                    e.Display.DrawSphere(new Sphere(this.centerCrv.PointAtStart, 1.5), Color.Yellow);
                }
                else if (distanceBtwTwoPts(this.bendPtSel, this.centerCrv.PointAtEnd) <= 1.5)
                {
                    // hightlight the end sphere
                    e.Display.DrawSphere(new Sphere(this.centerCrv.PointAtEnd, 1.5), Color.Yellow);
                }
                else
                {
                    e.Display.DrawSphere(new Sphere(this.centerCrv.PointAtStart, 1.5), Color.White);
                    e.Display.DrawSphere(new Sphere(this.centerCrv.PointAtEnd, 1.5), Color.White);
                }
            } 
        }
        private void Gp_CurveMouseMoveStretch(object sender, Rhino.Input.Custom.GetPointMouseEventArgs e)
        {
            if (e.ShiftKeyDown == true)
            {
                double ptPara = 0;
                centerCrv.ClosestPoint(e.Point, out ptPara);
                centerPt = centerCrv.PointAt(ptPara);
            }
        }
        private void Gp_AnglePlnMouseMove(object sender, Rhino.Input.Custom.GetPointMouseEventArgs e)
        {
            // get the projected point of the cursor point on the angle control plane 
            angleCtrlPt = this.anglePlane.ClosestPoint(e.Point);
            angleCtrlSecPt = angleCtrlPt;
        }
        private void Gp_AnglePlnDynamicDraw(object sender, Rhino.Input.Custom.GetPointDrawEventArgs e)
        {
            e.Display.DrawSphere(new Sphere(angleCtrlPt, 1.5), System.Drawing.Color.White);
            e.Display.DrawLine(new Line(this.middlePt, this.angleCtrlPt), Color.White);
            this.angleCircle = new Circle(this.anglePlane, this.middlePt, Math.Sqrt(Math.Pow(this.angleCtrlPt.X - this.middlePt.X, 2) + Math.Pow(this.angleCtrlPt.Y - this.middlePt.Y, 2)) + Math.Pow(this.angleCtrlPt.Z - this.middlePt.Z, 2));
            e.Display.DrawCircle(this.angleCircle, Color.White);
        }
        private void Gp_AngleSelectionMouseMove(object sender, Rhino.Input.Custom.GetPointMouseEventArgs e)
        {
            // get the point around the circle 
            Point3d pln_close = this.anglePlane.ClosestPoint(e.Point);

            angleCtrlSecPt = this.angleCircle.ClosestPoint(pln_close);
        }
        private void Gp_AngleSelectionDynamicDraw(object sender, Rhino.Input.Custom.GetPointDrawEventArgs e)
        {
            e.Display.DrawSphere(new Sphere(angleCtrlSecPt, 1.5), System.Drawing.Color.White);

            // compute the angle in real time
            Vector3d v1 = (Vector3d)(angleCtrlPt - this.middlePt);
            Vector3d v2 = (Vector3d)(angleCtrlSecPt - this.middlePt);
            this.twistAngle = Vector3d.VectorAngle(v1, v2);
            RhinoApp.WriteLine("The rotation angle: " + twistAngle);

            e.Display.DrawCircle(this.angleCircle, Color.White);
            e.Display.DrawLine(new Line(this.middlePt, this.angleCtrlSecPt), Color.White);
            e.Display.DrawArc(new Arc(anglePlane, 15, this.twistAngle), Color.White);
            //e.Display.Draw3dText(new Rhino.Display.Text3d(this.twistAngle.ToString()), Color.White, new Point3d(angleCtrlSecPt.X+3, angleCtrlSecPt.Y+3, angleCtrlSecPt.Z+3));
        }
        private void Gp_CurveDynamicDraw(object sender, Rhino.Input.Custom.GetPointDrawEventArgs e)
        {
            e.Display.DrawSphere(new Sphere(centerPt, 1.5), System.Drawing.Color.Azure);
        }
        private void Gp_CurveDynamicDrawStretch(object sender, Rhino.Input.Custom.GetPointDrawEventArgs e)
        {
            e.Display.DrawSphere(new Sphere(centerPt, 1.5), System.Drawing.Color.BlueViolet);
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


        #region Medial axis generation

        float compute_radius(Vector3d p, Vector3d n, Vector3d q)
        {
            // Compute radius of the ball that touches points p and q and whose center falls on the normal n from p
            double d = Math.Sqrt(Math.Pow(p.X - q.X, 2) + Math.Pow(p.Y - q.Y, 2) + Math.Pow(p.Z - q.Z, 2));
            double cos_theta = (n.X * (p - q).X + n.Y * (p - q).Y + n.Z * (p - q).Z) / d;
            return (float)(d / (2 * cos_theta));
        }

        float cos_angle(Vector3d p, Vector3d q)
        {
            // Calculate the cosine of angle between vector p and q
            float result = (float)((p.X * q.X + p.Y * q.Y + p.Z * q.Z) / (Math.Sqrt(p.X * p.X + p.Y * p.Y + p.Z * p.Z) * Math.Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z)));
            if (result > 1) return 1;
            else if (result < -1) return -1;
            return result;
        }
        ma_result sb_point(ma_parameters input_parameters, Vector3d p, Vector3d n, ma_data madata)
        {
            // Calculate a medial ball for a given oriented point using the shrinking ball algorithm
            int j = 0;
            float delta_convergance = 1E-5f;
            float r = input_parameters.initial_radius, d;
            Vector3d q, c_next;
            int qidx = -1;
            Vector3d c = p - n * r;

            Rhino.Geometry.PointCloud pt = new PointCloud();
            for (int i = 0; i < madata.coords.Count; i++)
            {
                pt.Add(madata.coords[i]);
            }

            if (true)
            {
                Point3d pp = new Point3d(c);
                List<Point3d> nearest_pp = new List<Point3d>();

                while (true)
                {
                    // find closest point to pp
                    nearest_pp = NClosestPoints(pt, pp, 0);
                    q = (Vector3d)nearest_pp[0];
                    //d = (float)Math.Sqrt((nearest_pp[0].X - pp.X) * (nearest_pp[0].X - pp.X) + (nearest_pp[0].Y - pp.Y) * (nearest_pp[0].Y - pp.Y) + (nearest_pp[0].Z - pp.Z) * (nearest_pp[0].Z - pp.Z));
                    d = (float)(Math.Pow(nearest_pp[0].X - pp.X, 2) + Math.Pow(nearest_pp[0].Y - pp.Y, 2) + Math.Pow(nearest_pp[0].Z - pp.Z, 2));
                    // this should handle all (special) cases where we want to break the loop
                    // - normal case when ball no longer shrinks
                    // - the case where q==p
                    // - any duplicate point cases
                    if ((d >= (r - delta_convergance) * (r - delta_convergance)) || (p == q))
                        break;

                    // compute next ball center
                    r = compute_radius(p, n, q);
                    c_next = p - n * r;

                    if (true)
                    {
                        // denoising
                        if (input_parameters.denoise_preserve != 0 || input_parameters.denoise_planar != 0)
                        {
                            float a = cos_angle(p - c_next, q - c_next);
                            float separation_angle = (float)Math.Acos((double)a);

                            if (j == 0 && input_parameters.denoise_planar > 0 && separation_angle < input_parameters.denoise_planar)
                            {
                                break;
                            }
                            if (j > 0 && input_parameters.denoise_preserve > 0 && (separation_angle < input_parameters.denoise_preserve && r > (Math.Sqrt(Math.Pow(q.X - p.X, 2) + Math.Pow(q.Y - p.Y, 2) + Math.Pow(q.Z - p.Z, 2)))))
                            {
                                break;
                            }
                        }

                        // stop iteration if this looks like an infinite loop
                        if (j > 30) break;

                        c = c_next;
                        j++;
                    }


                }
                if (j == 0 && input_parameters.nan_for_initr)
                {
                    ma_result result = new ma_result();
                    result.c = new Vector3d();
                    result.qidx = -1;
                    return result;
                }
                else
                {
                    ma_result result = new ma_result();
                    result.c = c;
                    result.qidx = qidx;
                    return result;
                }
            }

        }

        //void sb_points(ma_parameters input_parameters, ma_data madata, bool inner, progress_callback callback)
        /// <summary>
        /// Compute the points on the medial axis
        /// </summary>
        /// <param name="input_parameters">input parameters for the iterations</param>
        /// <param name="madata">data storage</param>
        void sb_points(ma_parameters input_parameters, ma_data madata)
        {
            //int progress = 0;
            //int accum = 0;

            for (int i = 0; i < madata.coords.Count; i++)
            {
                Vector3d p = (Vector3d)madata.coords.ElementAt(i);
                Vector3d n = -madata.normals.ElementAt(i);

                ma_result r = sb_point(input_parameters, p, n, madata);


                madata.ma_coords.Add(new Point3d(r.c));
                //madata.ma_qidx.Add(r.qidx);
            }
        }

        // void compute_masb_points(ma_parameters input_parameters, ma_data madata, progress_callback callback ={ })
        void compute_masb_points(ma_parameters input_parameters, ma_data madata)
        {
            // starting inside processsing
            //sb_points(input_parameters, madata, 1, callback);
            sb_points(input_parameters, madata);

            // clean the points - remove the points outside the model

            ma_effect.Clear();
            //cleanPoints(ma_effect, madata.ma_coords, madata);
            foreach (Point3d p in madata.ma_coords)
            {
                Rhino.Geometry.PointCloud pc = new PointCloud();
                pc.AddRange(madata.coords);

                List<Point3d> pts = new List<Point3d>();
                pts = NClosestPoints(pc, p, 1);
                int idx1 = madata.coords.IndexOf(pts[0]);
                int idx2 = madata.coords.IndexOf(pts[1]);

                Vector3d n1 = madata.normals[idx1];
                Vector3d n2 = madata.normals[idx2];

                Vector3d v1 = p - pts[0];
                Vector3d v2 = p - pts[1];

                if (n1 * v1 > 0 && n2 * v2 > 0)
                {
                    ma_effect.Add(p);
                }
            }

        }

        void cleanPoints(List<Point3d> new_list, List<Point3d> ori_list, ma_data madata)
        {
            foreach (Point3d p in ori_list)
            {
                Rhino.Geometry.PointCloud pc = new PointCloud();
                pc.AddRange(madata.coords);

                List<Point3d> pts = new List<Point3d>();
                pts = NClosestPoints(pc, p, 1);
                int idx1 = madata.coords.IndexOf(pts[0]);
                int idx2 = madata.coords.IndexOf(pts[1]);

                Vector3d n1 = madata.normals[idx1];
                Vector3d n2 = madata.normals[idx2];

                Vector3d v1 = p - pts[0];
                Vector3d v2 = p - pts[1];

                if (n1 * v1 > 0 && n2 * v2 > 0)
                {
                    new_list.Add(p);
                }
            }

        }

        /// <summary>
        /// find the nearest k neighbors
        /// </summary>
        /// <param name="pc"> point cloud / point set </param>
        /// <param name="pt"> given point / point query </param>
        /// <param name="range"> number of nearest points (0: one closest point; 1: two closest points)</param>
        /// <returns>nearest points</returns>
        List<Point3d> NClosestPoints(Rhino.Geometry.PointCloud pc, Point3d pt, int range)
        {
            List<Point3d> NClosestPoint = new List<Point3d>();

            if (range > pc.Count)
                return null;

            Rhino.Geometry.PointCloud pcTemp = new PointCloud();

            for (int j = 0; j < pc.Count; j++)
            {
                pcTemp.Add(pc.GetPoints()[j]);
            }
            if (range > 0)
            {
                for (int i = 0; i <= range; i++)
                {
                    int idx = pcTemp.ClosestPoint(pt);
                    if (idx == -1)
                    {
                        return new List<Point3d>();
                    }
                    NClosestPoint.Add(pcTemp.GetPoints()[idx]);
                    pcTemp.RemoveAt(idx);
                }
            }
            else if (range == 0)
            {
                int idx = pcTemp.ClosestPoint(pt);
                if (idx == -1)
                {
                    return new List<Point3d>();
                }
                Point3d temp_pt = pcTemp.GetPoints()[idx];
                if (temp_pt.X == pt.X && temp_pt.Y == pt.Y && temp_pt.Z == pt.Z)
                {
                    pcTemp.RemoveAt(idx);
                    int new_idx = pcTemp.ClosestPoint(pt);
                    NClosestPoint.Add(pcTemp.GetPoints()[new_idx]);
                }
                else
                {
                    NClosestPoint.Add(pcTemp.GetPoints()[idx]);
                }

            }

            return NClosestPoint;
        }

        void compute_normals(ma_data madata, RhinoDoc doc)
        {
            List<Point3d> PCList = new List<Point3d>();
            PCList.AddRange(madata.coords);

            double dev;
            foreach (Point3d point in PCList)
            {
                dynamic Neighbors = PCList.FindAll(v => v.DistanceTo(point) < neighbor_shreshold);
                Plane NP = default(Plane);
                Plane.FitPlaneToPoints(Neighbors, out NP, out dev);

                Point3d vp = new Point3d(0, 0, 0);
                int count = 0;
                foreach (Point3d p in Neighbors)
                {
                    vp = vp + p;
                    count++;
                }
                vp = vp / count;
                RhinoApp.WriteLine("vp is {0}", vp);

                if (NP.Normal * (vp - point) > 0)
                    madata.normals.Add(NP.Normal);
                else
                    madata.normals.Add(-NP.Normal);

                // draw all normals in blue
                //Rhino.DocObjects.ObjectAttributes attr = new Rhino.DocObjects.ObjectAttributes();
                //attr.LayerIndex = 6;
                //doc.Objects.AddLine(point, point + madata.normals.Last(), attr);
            }
        }

        /// <summary>
        /// Generate the medial axis
        /// </summary>
        public void medialAxisTransform()
        {
            // initial values fed into the shrinking ball algorithm
            ma_parameters input_parameters;

            // original values for those parameters
            // input_parameters.initial_radius = 200;
            // input_parameters.denoise_preserve = (3.14159265358979323846 / 180.0) * 20;
            // input_parameters.denoise_planar = (3.14159265358979323846 / 180.0) * 32;

            input_parameters.initial_radius = initial_radius;
            input_parameters.denoise_preserve = preserve;
            input_parameters.denoise_planar = planar;
            input_parameters.nan_for_initr = true;

            ma_data madata = new ma_data();

            // add coords
            madata.coords = new List<Rhino.Geometry.Point3d>();

            for (int i = 0; i < dyndrawLst.Count; i++)
            {
                madata.coords.Add(dyndrawLst.ElementAt(i));
            }

            // compute normals for the points
            // and record the nearest point for each point in the point cloud
            madata.normals = new List<Rhino.Geometry.Vector3d>();
            madata.nearest_point = new List<Point3d>();
            compute_normals(madata, myDoc);

            // Perform the actual processing
            madata.ma_coords = new List<Point3d>();
            madata.ma_qidx = new List<int>();
            compute_masb_points(input_parameters, madata);

            // 8/4/17 optimized clustering algorithm
            // clustering all generated points around the medial axis
            int ma_pcluster_shreshold = 5;
            List<Point3d> clustered_points = new List<Point3d>();
            for (int i = 0; i < ma_effect.Count; i++)
            {
                dynamic N = ma_effect.FindAll(v => v.DistanceTo(ma_effect.ElementAt(i)) <= ma_pcluster_shreshold);

                if (((List<Point3d>)N).Count > 3)
                {
                    clustered_points.Add(ma_effect.ElementAt(i));
                }
            }

            List<Point3d> ma_pts_list = new List<Point3d>();
            bool[] ma_pts_flag = new bool[clustered_points.Count];
            for (int u = 0; u < clustered_points.Count; u++)
                ma_pts_flag[u] = false;

            for (int r = 0; r < clustered_points.Count; r++)
            {
                if (!ma_pts_flag[r])
                {
                    ma_pts_flag[r] = true;
                    dynamic N = clustered_points.FindAll(v => v.DistanceTo(clustered_points.ElementAt(r)) <= 10);

                    Point3d vp = new Point3d(0, 0, 0);
                    int count = 0;
                    foreach (Point3d pt in N)
                    {
                        int idx = clustered_points.IndexOf(pt);
                        ma_pts_flag[idx] = true;
                        vp = vp + pt;
                        count++;
                    }

                    if (count > 3)
                    {
                        // add the average point into ma_pts_list
                        vp = vp / count;
                        ma_pts_list.Add(vp);

                    }
                }

            }

            if (final_ma.Count > 0)
                final_ma.Clear();
            cleanPoints(final_ma, ma_pts_list, madata);
            Rhino.DocObjects.ObjectAttributes attr = new Rhino.DocObjects.ObjectAttributes();
            attr.LayerIndex = 7;
            //for (int t = 0; t < ma_effect.Count; t++)
            //{
            //    myDoc.Objects.AddPoint(ma_effect[t], attr);
            //}
            //Rhino.Geometry.Point3d.SortAndCullPointList(final_ma, 0);

            //#region reorder the points
            //// Reorder the points in the cloud on the final medial axis
            //List<Point3d> ordered_final_ma = new List<Point3d>();
            //Vector3d direction = new Vector3d(0, 0, 0);

            //foreach(Point3d pt in final_ma)
            //{
            //    if (ordered_final_ma.Count >= 2)
            //    {
            //        // search for the closest two points in the new list
            //        Rhino.Geometry.PointCloud ma_pc = new PointCloud();
            //        ma_pc.AddRange(ordered_final_ma);
            //        List<Point3d> closestTwo = NClosestPoints(ma_pc, pt, 1);
            //        Point3d closest1 = closestTwo[0];
            //        Point3d closest2 = closestTwo[1];

            //        // construct vectors based on closest1, closest2, and pt
            //        Vector3d v1 = (Vector3d)(closest1 - pt);
            //        Vector3d v2 = (Vector3d)(closest2 - pt);

            //        if(v1.X*v2.X+v1.Y*v2.Y+v1.Z*v2.Z <= 0)
            //        {
            //            // pt is between closest1 and closest2
            //            int idx1 = ordered_final_ma.IndexOf(closest1);
            //            int idx2 = ordered_final_ma.IndexOf(closest2);

            //            if(idx1-idx2 > -1 && idx1-idx2 < 1)
            //            {
            //                // the two points are next to each other, otherwise abandon current point
            //                int min = idx1 > idx2 ? idx2 : idx1;

            //                // insert pt into between closest1 and closest2
            //                ordered_final_ma.Insert(min + 1, pt);
            //            } 
            //        }
            //        else
            //        {
            //            // test if the vector made from the current point the closest point 
            //            // has the same direction with the main direction of the point cloud
            //            double codirection = v1.X * direction.X + v1.Y * direction.Y + v1.Z * direction.Z;

            //            if (codirection >= 0)
            //            {
            //                // current point is before all points
            //                int idx1 = ordered_final_ma.IndexOf(closest1);
            //                int idx2 = ordered_final_ma.IndexOf(closest2);


            //                // the two points are next to each other, otherwise abandon current point
            //                int min = idx1 > idx2 ? idx2 : idx1;

            //                // insert pt into between closest1 and closest2
            //                 ordered_final_ma.Insert(min, pt);


            //            }
            //            else
            //            {
            //                // current point is after all points
            //                int idx1 = ordered_final_ma.IndexOf(closest1);
            //                int idx2 = ordered_final_ma.IndexOf(closest2);


            //                // the two points are next to each other, otherwise abandon current point
            //                int max = idx1 > idx2 ? idx1 : idx2;

            //                // insert pt into between closest1 and closest2
            //                ordered_final_ma.Insert(max + 1, pt);

            //            }
            //        }

            //        // update the direction even if the current point is excluded from the new list
            //        //direction = (Vector3d)(ordered_final_ma.Last() - ordered_final_ma.ElementAt(0));

            //    }
            //    else if (ordered_final_ma.Count == 0)
            //    {
            //        // directly add the first point
            //        ordered_final_ma.Add(pt);
            //    }
            //    else if(ordered_final_ma.Count == 1)
            //    {
            //        // add the second point and compute the direction
            //        ordered_final_ma.Add(pt);
            //        direction = (Vector3d)(ordered_final_ma.Last() - ordered_final_ma.ElementAt(0));
            //    }

            //}
            //#endregion

            #region reorder the points
            List<Point3d> ordered_final_ma = new List<Point3d>();


            Point3d avg = new Point3d(0, 0, 0);
            foreach (Point3d pt in final_ma)
            {
                avg += pt;
            }
            avg = avg / final_ma.Count;

            // find the point that has the longest distance from the centroid of the point cloud
            double max_dis = 0;
            Point3d bound_pt = new Point3d();
            foreach (Point3d pt in final_ma)
            {
                double dis = Math.Pow(pt.X - avg.X, 2) + Math.Pow(pt.Y - avg.Y, 2) + Math.Pow(pt.Z - avg.Z, 2);

                if (dis >= max_dis)
                {
                    max_dis = dis;
                    bound_pt = pt;
                }
            }

            int totalnumber = final_ma.Count;
            //Point3d center = new Point3d();
            for (int ptidx = 0; ptidx < totalnumber; ptidx++)
            {
                if (ptidx == 0)
                {
                    // add the boundary point
                    ordered_final_ma.Add(bound_pt);
                    final_ma.Remove(bound_pt);
                    // center = bound_pt/(ptidx+1);
                }
                else
                {
                    // find the closest point of the last point in ordered_final_ma
                    Rhino.Geometry.PointCloud final_ma_pc = new Rhino.Geometry.PointCloud();
                    final_ma_pc.AddRange(final_ma);
                    List<Point3d> closestPt = NClosestPoints(final_ma_pc, ordered_final_ma.Last(), 0);

                    if (closestPt.Count > 0)
                    {
                        // angle less than 30 degree 
                        ordered_final_ma.Add(closestPt[0]);
                        final_ma.Remove(closestPt[0]);
                    }


                }
            }

            #endregion

            Rhino.Geometry.Curve ma = Rhino.Geometry.Curve.CreateControlPointCurve(ordered_final_ma, 5);
            myDoc.Objects.AddCurve(ma, attr);

            Rhino.DocObjects.ObjectAttributes attr1 = new Rhino.DocObjects.ObjectAttributes();
            attr1.LayerIndex = 8;
            for (int t = 0; t < ordered_final_ma.Count; t++)
            {
                myDoc.Objects.AddPoint(ordered_final_ma[t], attr1);
            }
            myDoc.Views.Redraw();
        }

        #endregion

    }

}