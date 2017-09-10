﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Input;
using System.Drawing;

namespace PluginBar
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

        void printSTL(ObjRef obj, Point3d centerPt);

        void deformBrep(ObjRef obj);

        void wireframeAll();

        void selection();

        void linearDeform(ObjRef objRef);
        void twistDeform(ObjRef objRef);
        void bendDeform(ObjRef objRef);
        void linearTwistDeform(ObjRef objRef);
        void linearBendDeform(ObjRef objRef);
        void twistBendDeform(ObjRef objRef);
        void allDeform(ObjRef objRef);
        void medialAxisTransform();
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
        Point3d middlePt;

        int neighbor_shreshold = 10;
        int initial_radius = 100;
        double preserve = (3.14159265358979323846 / 180.0) * 65;
        double planar = (3.14159265358979323846 / 180.0) * 132;
        List<Point3d> ma_effect = new List<Point3d>();
        List<Point3d> final_ma = new List<Point3d>();

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

        private Curve springGeneration(Curve centerCrv, Brep surfaceBrep, double pitch)
        {
            //DEBUG: Currently the bug is the center curve is only cut when there is a discontinuity, this is not good enough to have a nice spring approximation to the outer shell's shape.
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
                    NurbsCurve spiralCrv = NurbsCurve.CreateSpiral(crv, 0, endPara, spiralStartPt, pitch, 0, r1, r2, 12);
                    spiralStartPt = spiralCrv.PointAtEnd;
                    spiralCrvList.Add(spiralCrv);
                }
            }

            Curve joinedSpiral = Curve.JoinCurves(spiralCrvList)[0];
            if (joinedSpiral != null)
            {
                myDoc.Objects.AddCurve(joinedSpiral);
            }
            #endregion

            #region 3. sweep section to create spring solid

            Plane spiralStartPln = new Plane(joinedSpiral.PointAtStart, joinedSpiral.TangentAtStart);
            Circle startCircle = new Circle(spiralStartPln, joinedSpiral.PointAtStart, 1); //spring's cross section's radius is currently 1. This should be tuned based on the shape the user selected and also the test limit we have.
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
                    //myDoc.Objects.AddBrep(b.CapPlanarHoles(myDoc.ModelRelativeTolerance));
                }
            }
            #endregion

            return startCircle.ToNurbsCurve();
        }
        /// <summary>
        /// This method generates linear deformation structure. Currently support generating compression
        /// </summary>
        /// <param name="objRef"></param>
        public void linearDeform(ObjRef objRef)
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
            myDoc.Objects.AddSurface(startPlnSuf);
            myDoc.Objects.AddSurface(endPlnSuf);
            myDoc.Views.Redraw();
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
            // compressCrv is the trajectory of compression
            compressCrv = splitCrvs[1];
            Curve pressCrv = splitCrvs[0];
            myDoc.Objects.AddCurve(compressCrv, greenAttribute);
            myDoc.Views.Redraw();
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
            // stretchCrv is the trajectory of stretching
            stretchCrv = splitCrvsStretch[1];
            Point3d railEnd = splitCrvsStretch[0].PointAtEnd;

            double stopperPara;
            splitCrvsStretch[0].LengthParameter(splitCrvsStretch[0].GetLength(), out stopperPara);
            Plane stopperPln = new Plane(railEnd, splitCrvsStretch[0].TangentAt(stopperPara));// the plane at the start of the curve

            Rhino.DocObjects.ObjectAttributes attr = new Rhino.DocObjects.ObjectAttributes();
            attr.LayerIndex = 6;
            myDoc.Objects.AddCurve(stretchCrv, attr);
            myDoc.Views.Redraw();
            #endregion

            #region generating the outer spring
            Curve sideCrv = springGeneration(centerCrv, surfaceBrep, 5);
            #endregion

            #region delete the part that spring will replaced
            // chop the shells into 3 piece
            List<Brep> splitsurf = new List<Brep>();
            Brep[] firstSplit = surfaceBrep.Split(startPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);

            Brep[] test = firstSplit[0].Split(endPlnSuf.ToBrep(), myDoc.ModelAbsoluteTolerance);
            if(test != null && test.Length>0)
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

            foreach(Brep b in splitsurf)
            {
                Point3d bcenter = b.GetBoundingBox(true).Center;
                Vector3d v1 = bcenter - endPln.Origin;
                Vector3d v2 = bcenter - startPln.Origin;
                if(v1 * v2 >0)
                {
                    Brep temp = b.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);
                    finalPreservedBrepList.Add(temp);
                }

            }


            foreach(Brep b in finalPreservedBrepList)
            {
                myDoc.Objects.AddBrep(b,redAttribute);
            }

            #endregion

            #region Generate support structure
            Brep prism, stopper;
            generateLinearSupport(startPln, endPln, centerCrv, compressCrv, pressCrv, stretchCrv, railEnd, stopperPln, out stopper, out prism);
            #endregion

            myDoc.Objects.Hide(sufObjId, true);// hide the original shell
            myDoc.Views.Redraw();

            List<Brep> top = new List<Brep>();
            top.Add(finalPreservedBrepList[1]);
            top.Add(prism);
            top.Add(stopper);
            compressAnimation(centerCrv, compressCrv, sideCrv, top);
        }

        /// <summary>
        /// This method generates twist deformation structure.
        /// <param name="objRef"></param>
        public void twistDeform(ObjRef objRef)
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
            myDoc.Objects.AddSurface(startPlnSuf);
            myDoc.Objects.AddSurface(endPlnSuf);
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

            myDoc.Objects.Delete(sId,false);
            myDoc.Views.Redraw();
            #endregion

            #region generate the outer spring
            springGeneration(centerCrv, surfaceBrep, 5);
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
            #region generate the central support structure
            generateTwistSupport(startPln, endPln, centerCrv);
            #endregion

            myDoc.Objects.Hide(sufObjId, true);// hide the original shell
            myDoc.Views.Redraw();

        }
        /// <summary>
        /// This method generates bend deformation structure.
        /// </summary>
        /// <param name="objRef"></param>
        public void bendDeform(ObjRef objRef)
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
            Rhino.Geometry.PlaneSurface startPlnSuf = new PlaneSurface(startPln, new Interval(-50, 50), new Interval(-50, 50));// convert the plane object to an actual surface so that we can draw it in the rhino doc
            Rhino.Geometry.PlaneSurface endPlnSuf = new PlaneSurface(endPln, new Interval(-50, 50), new Interval(-50, 50));// 15 is a random number. It should actually be the ourter shell's radius or larger.
            myDoc.Objects.AddSurface(startPlnSuf);
            myDoc.Objects.AddSurface(endPlnSuf);
            #endregion

            #region get the anchor control points for deciding the bending direction
            this.centerCrv = centerCrv;
            var attributes = new ObjectAttributes();
            attributes.ObjectColor = Color.White;
            attributes.ColorSource = ObjectColorSource.ColorFromObject;
          
            // listen to the user's selected sphere: either at the start or at the end sphere
            Rhino.Input.Custom.GetPoint gp_pt = new Rhino.Input.Custom.GetPoint();
            gp_pt.DynamicDraw += Gp_sphereSelDynamicDraw;
            gp_pt.MouseMove += Gp_sphereSelMouseMove;
            gp_pt.Get(true);

            if(distanceBtwTwoPts(this.bendPtSel, this.centerCrv.PointAtStart) <= 2)
            {
                // selected the start sphere
                Rhino.Geometry.Sphere startSphere = new Rhino.Geometry.Sphere(startPt, 2);
                myDoc.Objects.AddSphere(startSphere, attributes);
                Plane anglePln = new Plane(startPt, centerCrv.TangentAt(startPara)); // the plane at the starting plane of the central curve
                this.bendPlane = anglePln;
                
                this.bendCtrlPt = startPt;
                Rhino.Geometry.Circle dirCircle = new Rhino.Geometry.Circle(anglePln, startPt, 20);
                this.angleCircle = dirCircle;

                myDoc.Objects.AddCircle(dirCircle);
                myDoc.Views.Redraw();

                // start the command 
                is_BendStart = true;

                if(is_BendStart)
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
                    RhinoApp.WriteLine("direction: (" + tempBendInfo.dir.X + "," + tempBendInfo.dir.Y + "," + tempBendInfo.dir.Z + ")");
                    RhinoApp.WriteLine("strength: " + tempBendInfo.strength);
                    bendInfoList.Add(tempBendInfo);
                }

                #region generating the outer spring
                springGeneration(centerCrv, surfaceBrep, 5);
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

                // generate the bend support structure
                generateBendSupport(startPln, endPln, this.centerCrv, bendInfoList, startPt, endPt, true);

                // Clear the bend information list
                bendInfoList.Clear();
               
            }
            else if (distanceBtwTwoPts(this.bendPtSel, this.centerCrv.PointAtEnd) <= 2)
            {
                // selected the end sphere
                Rhino.Geometry.Sphere endSphere = new Rhino.Geometry.Sphere(endPt, 2);
                myDoc.Objects.AddSphere(endSphere, attributes);
                Plane anglePln = new Plane(endPt, centerCrv.TangentAt(endPara)); // the plane at the end plane of the central curve
                this.bendPlane = anglePln;

                this.bendCtrlPt = endPt;
                Rhino.Geometry.Circle dirCircle = new Rhino.Geometry.Circle(anglePln, endPt, 20);
                this.angleCircle = dirCircle;

                myDoc.Objects.AddCircle(dirCircle);
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
                    RhinoApp.WriteLine("direction: (" + tempBendInfo.dir.X + "," + tempBendInfo.dir.Y + "," + tempBendInfo.dir.Z + ")");
                    RhinoApp.WriteLine("strength: " + tempBendInfo.strength);
                    bendInfoList.Add(tempBendInfo);
                }

                #region generating the outer spring
                springGeneration(centerCrv, surfaceBrep, 5);
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

                // generate the bend support structure
                generateBendSupport(startPln, endPln, this.centerCrv, bendInfoList, startPt, endPt, false);

                // Clear the bend information list
                bendInfoList.Clear();
            }

            #endregion

            myDoc.Objects.Hide(sufObjId, true);// hide the original shell
            myDoc.Views.Redraw();
        }
        /// <summary>
        /// This method generates linear (compress & stretch) + twist deformation structure.
        /// </summary>
        /// <param name="objRef"></param>
        public void linearTwistDeform(ObjRef objRef)
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

            // Linear part
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
            myDoc.Objects.AddSurface(startPlnSuf);
            myDoc.Objects.AddSurface(endPlnSuf);
            myDoc.Views.Redraw();
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
            myDoc.Objects.AddCurve(compressCrv, greenAttribute);
            myDoc.Views.Redraw();
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
            myDoc.Objects.AddCurve(stretchCrv, attr);
            myDoc.Views.Redraw();
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
            Rhino.Input.Custom.GetPoint gp_mp = new Rhino.Input.Custom.GetPoint();
            gp_mp.DynamicDraw += Gp_AnglePlnDynamicDraw;// this is called everymoment before the user click the left button to draw sphere (with shift key pressed)
            gp_mp.MouseMove += Gp_AnglePlnMouseMove;// this is called everymoment before the user click the left button to draw sphere
            gp_mp.Get(true);

            sphere = new Rhino.Geometry.Sphere(this.angleCtrlPt, 2);
            Guid sId = myDoc.Objects.AddSphere(sphere, attributes);
            myDoc.Views.Redraw();

            Rhino.Input.Custom.GetPoint gp_mp_sec = new Rhino.Input.Custom.GetPoint();
            gp_mp_sec.DynamicDraw += Gp_AngleSelectionDynamicDraw;// this is called everymoment before the user click the left button to draw sphere (with shift key pressed)
            gp_mp_sec.MouseMove += Gp_AngleSelectionMouseMove;// this is called everymoment before the user click the left button to draw sphere
            gp_mp_sec.Get(true);

            myDoc.Objects.Delete(sId, false);
            myDoc.Views.Redraw();
            #endregion

            #region generating the outer spring
            springGeneration(centerCrv, surfaceBrep, 5);
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

            #region Generate support structure
            generateLinearTwistSupport(startPln, endPln, centerCrv, compressCrv, pressCrv, stretchCrv, railEnd, stopperPln);

            #endregion

            myDoc.Objects.Hide(sufObjId, true);// hide the original shell
            myDoc.Views.Redraw();


        }
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
            myDoc.Objects.AddSurface(startPlnSuf);
            myDoc.Objects.AddSurface(endPlnSuf);
            myDoc.Views.Redraw();
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
            myDoc.Objects.AddCurve(compressCrv, greenAttribute);
            myDoc.Views.Redraw();
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
            myDoc.Objects.AddCurve(stretchCrv, attr);
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

            if (distanceBtwTwoPts(this.bendPtSel, this.centerCrv.PointAtStart) <= 2)
            {
                // selected the start sphere
                Rhino.Geometry.Sphere startSphere = new Rhino.Geometry.Sphere(startPt, 2);
                myDoc.Objects.AddSphere(startSphere, attributes);
                Plane bendAnglePln = new Plane(startPt, centerCrv.TangentAt(startPara)); // the plane at the starting plane of the central curve
                this.bendPlane = bendAnglePln;

                this.bendCtrlPt = startPt;
                Rhino.Geometry.Circle dirCircle = new Rhino.Geometry.Circle(bendAnglePln, startPt, 20);
                this.angleCircle = dirCircle;

                myDoc.Objects.AddCircle(dirCircle);
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
                    RhinoApp.WriteLine("direction: (" + tempBendInfo.dir.X + "," + tempBendInfo.dir.Y + "," + tempBendInfo.dir.Z + ")");
                    RhinoApp.WriteLine("strength: " + tempBendInfo.strength);
                    bendInfoList.Add(tempBendInfo);
                }

                #region generate the outer spring
                springGeneration(centerCrv, surfaceBrep, 5);
                #endregion

                // generate the bend support structure
                generateLinearBendSupport(startPln, endPln, centerCrv, compressCrv, pressCrv, stretchCrv, railEnd, stopperPln, bendInfoList, startPt, endPt, true);

                // Clear the bend information list
                bendInfoList.Clear();

            }
            else if (distanceBtwTwoPts(this.bendPtSel, this.centerCrv.PointAtEnd) <= 2)
            {
                // selected the end sphere
                Rhino.Geometry.Sphere endSphere = new Rhino.Geometry.Sphere(endPt, 2);
                myDoc.Objects.AddSphere(endSphere, attributes);
                Plane bendAnglePln = new Plane(endPt, centerCrv.TangentAt(endPara)); // the plane at the end plane of the central curve
                this.bendPlane = bendAnglePln;

                this.bendCtrlPt = endPt;
                Rhino.Geometry.Circle dirCircle = new Rhino.Geometry.Circle(bendAnglePln, endPt, 20);
                this.angleCircle = dirCircle;

                myDoc.Objects.AddCircle(dirCircle);
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
                    RhinoApp.WriteLine("direction: (" + tempBendInfo.dir.X + "," + tempBendInfo.dir.Y + "," + tempBendInfo.dir.Z + ")");
                    RhinoApp.WriteLine("strength: " + tempBendInfo.strength);
                    bendInfoList.Add(tempBendInfo);
                }

                #region generate the outer spring
                springGeneration(centerCrv, surfaceBrep, 5);
                #endregion

                // generate the bend support structure
                generateLinearBendSupport(startPln, endPln, centerCrv, compressCrv, pressCrv, stretchCrv, railEnd, stopperPln, bendInfoList, startPt, endPt, false);

                // Clear the bend information list
                bendInfoList.Clear();
            }
            isBendLinear = false;
            #endregion

            #region generating the outer spring
            springGeneration(centerCrv, surfaceBrep, 5);
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
            myDoc.Objects.AddSurface(startPlnSuf);
            myDoc.Objects.AddSurface(endPlnSuf);
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

            if (distanceBtwTwoPts(this.bendPtSel, this.centerCrv.PointAtStart) <= 2)
            {
                // selected the start sphere
                Rhino.Geometry.Sphere startSphere = new Rhino.Geometry.Sphere(startPt, 2);
                myDoc.Objects.AddSphere(startSphere, attributes);
                Plane bendAnglePln = new Plane(startPt, centerCrv.TangentAt(startPara)); // the plane at the starting plane of the central curve
                this.bendPlane = bendAnglePln;

                this.bendCtrlPt = startPt;
                Rhino.Geometry.Circle dirCircle = new Rhino.Geometry.Circle(bendAnglePln, startPt, 20);
                this.angleCircle = dirCircle;

                myDoc.Objects.AddCircle(dirCircle);
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
                    RhinoApp.WriteLine("direction: (" + tempBendInfo.dir.X + "," + tempBendInfo.dir.Y + "," + tempBendInfo.dir.Z + ")");
                    RhinoApp.WriteLine("strength: " + tempBendInfo.strength);
                    bendInfoList.Add(tempBendInfo);
                }

                #region generate the outer spring
                springGeneration(centerCrv, surfaceBrep, 5);
                #endregion

                // generate the bend support structure
                generateTwistBendSupport(startPln, endPln, this.centerCrv, bendInfoList, startPt, endPt, true);

                // Clear the bend information list
                bendInfoList.Clear();

            }
            else if (distanceBtwTwoPts(this.bendPtSel, this.centerCrv.PointAtEnd) <= 2)
            {
                // selected the end sphere
                Rhino.Geometry.Sphere endSphere = new Rhino.Geometry.Sphere(endPt, 2);
                myDoc.Objects.AddSphere(endSphere, attributes);
                Plane bendAnglePln = new Plane(endPt, centerCrv.TangentAt(endPara)); // the plane at the end plane of the central curve
                this.bendPlane = bendAnglePln;

                this.bendCtrlPt = endPt;
                Rhino.Geometry.Circle dirCircle = new Rhino.Geometry.Circle(bendAnglePln, endPt, 20);
                this.angleCircle = dirCircle;

                myDoc.Objects.AddCircle(dirCircle);
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
                    RhinoApp.WriteLine("direction: (" + tempBendInfo.dir.X + "," + tempBendInfo.dir.Y + "," + tempBendInfo.dir.Z + ")");
                    RhinoApp.WriteLine("strength: " + tempBendInfo.strength);
                    bendInfoList.Add(tempBendInfo);
                }

                #region generate the outer spring
                springGeneration(centerCrv, surfaceBrep, 5);
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
            myDoc.Objects.AddSurface(startPlnSuf);
            myDoc.Objects.AddSurface(endPlnSuf);
            #endregion

            #region generate the outer spring
            springGeneration(centerCrv, surfaceBrep, 5);
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

        /// <summary>
        /// This function generate support structure for compression
        /// </summary>
        /// <param name="startSuf">The start surface plane of the center axis</param>
        /// <param name="endSuf">The end surface plane of the center axis</param>
        /// <param name="centerCrv">Center axis</param>
        /// <param name="compressCrv">Compression curve, which is one side of the centerCrve splited by the user's control point input</param>
        /// <param name="pressCrv">the other side of the center curve split</param>
        private void generateLinearSupport(Plane startSuf, Plane endSuf, Curve centerCrv, Curve compressCrv, Curve pressCrv, Curve stretchCrv, Point3d railEnd, Plane stopperPlnOrig, out Brep stopperBrep, out Brep prismBrep)
        {
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


            // base structure 2 bars
            double baseStructureDisToCenter = 4;
            Curve baseStructureCrv1 = linearCrv.DuplicateCurve();
            Curve baseStructureCrv2 = linearCrv.DuplicateCurve();
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

            foreach(var p in guiderInnerCornerPt)
            {
                myDoc.Objects.AddPoint(p);
                myDoc.Views.Redraw();
            }
            Curve guiderOuterRectCrv = new Polyline(guiderOuterCornerPt).ToNurbsCurve();
            Curve guiderInnerRectCrv = new Polyline(guiderInnerCornerPt).ToNurbsCurve();

            var outerRect = sweep.PerformSweep(linearCrv, guiderOuterRectCrv)[0];
            var innerRect = sweep.PerformSweep(linearCrv, guiderInnerRectCrv)[0];


            var baseBreps = Brep.CreateBooleanIntersection(outerRect, innerRect, myDoc.ModelRelativeTolerance);
            baseBreps[0] = baseBreps[0].CapPlanarHoles(myDoc.ModelRelativeTolerance);
            baseBreps[1] = baseBreps[1].CapPlanarHoles(myDoc.ModelRelativeTolerance);

            myDoc.Objects.AddBrep(baseBreps[0]);
            myDoc.Objects.AddBrep(baseBreps[1]);

            List<Point3d> baseVertexList = new List<Point3d>();
            foreach(Brep b in baseBreps)
            {
                Rhino.Geometry.Collections.BrepVertexList vertexList = b.Vertices;
                if(vertexList!=null && vertexList.Count>0)
                {
                    foreach(var v in vertexList)
                    {
                        baseVertexList.Add(v.Location);
                    }
                }
            }

            Plane guiderPln = new Plane(linearCrv.PointAtEnd, linearCrv.TangentAtEnd);
            PlaneSurface guiderPlnSuf = new PlaneSurface(guiderPln, new Interval(-30, 30), new Interval(-30, 30));
            List<Point3d> guiderPointsList = new List<Point3d>();
            foreach (Point3d p in baseVertexList)
            {
                double u, v;
                guiderPlnSuf.ClosestPoint(p, out u, out v);
                if(guiderPlnSuf.PointAt(u, v).DistanceTo(p)<0.5)
                {
                    guiderPointsList.Add(p);
                }
            }

            Rhino.Geometry.PointCloud ptCloud = new PointCloud(guiderPointsList);
            for (int i=0; i<4;i++)
            {
                int removeIdx = ptCloud.ClosestPoint(guiderPln.Origin);
                ptCloud.RemoveAt(removeIdx);
            }
            guiderPointsList.Clear();
            foreach (var p in ptCloud)
                guiderPointsList.Add(p.Location);

            guiderPointsList.Add(guiderPointsList[0]);
            Curve guiderTopCrv = new Polyline(guiderPointsList).ToNurbsCurve();

            //prismic structure that limits rotation
            Point3d centerPrismPt = centerCrv.PointAtStart;
            double primBaseSideHalfLength = 1;
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
            prismBrep = sweep.PerformSweep(pressCrv, prismCrv)[0];
            prismBrep = prismBrep.CapPlanarHoles(myDoc.ModelRelativeTolerance);
            myDoc.Objects.AddBrep(prismBrep);
            myDoc.Views.Redraw();


            // stopper (disc)
            Plane stopperPln = new Plane(pressCrv.PointAtEnd, pressCrv.TangentAtEnd);
            Circle stopperCir = new Circle(stopperPln, pressCrv.PointAtEnd, 2);
            double t;
            compressCrv.LengthParameter(3, out t);
            Curve stopperCrv = compressCrv.Split(t)[0];

            stopperBrep = sweep.PerformSweep(stopperCrv, stopperCir.ToNurbsCurve())[0];
            stopperBrep = stopperBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);

            myDoc.Objects.AddCurve(stopperCrv);
            myDoc.Objects.AddBrep(stopperBrep);
            myDoc.Views.Redraw();
            
            
            // guider hole

            Point3d guiderPt = railEnd;
            double guiderPtGap = 0.2;

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
            splitedLeftOver[0].LengthParameter(splitedLeftOver[0].GetLength() - 3, out t);
            
            var splitedLeftCrvs = splitedLeftOver[0].Split(t);
            Brep guiderCenter = sweep.PerformSweep(splitedLeftCrvs[1], guiderCrv)[0];
            guiderCenter = guiderCenter.CapPlanarHoles(myDoc.ModelRelativeTolerance);

            //guider outcube
            Brep outerGuider = sweep.PerformSweep(splitedLeftCrvs[1], guiderTopCrv)[0];
            outerGuider = outerGuider.CapPlanarHoles(myDoc.ModelRelativeTolerance);
            outerGuider.Flip();
            var guiderFinal = Brep.CreateBooleanDifference(guiderCenter, outerGuider,myDoc.ModelRelativeTolerance)[0];
            myDoc.Objects.Add(guiderFinal);


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
                var baseStructureDisToCenter = 4;
                Curve baseStructureCrv1 = linearCrv.DuplicateCurve();
                Curve baseStructureCrv2 = linearCrv.DuplicateCurve();
                normal_dir.Rotate(Math.PI / 2, axis);
                p_end = endPt + normal_dir;
                p_end = endSuf.ClosestPoint(p_end);
                normal_dir = (Vector3d)(p_end - endPt);
                baseStructureCrv1.Translate(4/normal_dir.Length * normal_dir);
                normal_dir.Rotate(Math.PI, axis);
                p_end = endPt + normal_dir;
                p_end = endSuf.ClosestPoint(p_end);
                normal_dir = (Vector3d)(p_end - endPt);
                baseStructureCrv2.Translate(4 / normal_dir.Length * normal_dir);

                //myDoc.Objects.AddCurve(baseStructureCrv1, attributes);
                //myDoc.Views.Redraw();
                //myDoc.Objects.AddCurve(baseStructureCrv2, attributes);
                //myDoc.Views.Redraw();

                Point3d[] guider1CornerPt = new Point3d[5];
                Point3d[] guider2CornerPt = new Point3d[5];
                Point3d[] guiderOuter1Pt = new Point3d[5];
                Point3d[] guiderOuter2Pt = new Point3d[5];
                Point3d[] cornerPt = new Point3d[5];

                Transform txp = Transform.Translation(1.5 / normal_dir.Length * normal_dir);
                Transform txp_big = Transform.Translation(1.5 / normal_dir.Length * normal_dir);
                normal_dir.Rotate(Math.PI / 2, axis);
                Transform tyn = Transform.Translation(0.75 / normal_dir.Length * normal_dir);
                Transform tyn_big = Transform.Translation(2.5 / normal_dir.Length * normal_dir);
                normal_dir.Rotate(Math.PI / 2, axis);
                Transform txn = Transform.Translation(1.5 / normal_dir.Length * normal_dir);
                Transform txn_big = Transform.Translation(1.5 / normal_dir.Length * normal_dir);
                normal_dir.Rotate(Math.PI / 2, axis);
                Transform typ = Transform.Translation(0.75 / normal_dir.Length * normal_dir);
                Transform typ_big = Transform.Translation(2.5 / normal_dir.Length * normal_dir);

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
                myDoc.Objects.AddSurface(guiderPlnSuf);
                myDoc.Views.Redraw();
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
                myDoc.Objects.AddCurve(guiderTopCrv, attributes);
                myDoc.Views.Redraw();
                #endregion

                #region prismic structure that limits rotation
                normal_dir = newDir;
                Point3d centerPrismPt = centerCrv.PointAtStart;
                double primBaseSideHalfLength = 0.75;
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
                myDoc.Views.Redraw();
                #endregion

                #region stopper (disc)
                Plane stopperPln = new Plane(pressCrv.PointAtEnd, pressCrv.TangentAtEnd);
                Circle stopperCir = new Circle(stopperPln, pressCrv.PointAtEnd, 2);
                double t;
                compressCrv.LengthParameter(3, out t);
                Curve stopperCrv = compressCrv.Split(t)[0];

                var stopperBrep = sweep.PerformSweep(stopperCrv, stopperCir.ToNurbsCurve())[0];
                stopperBrep = stopperBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);

                myDoc.Objects.AddCurve(stopperCrv);
                myDoc.Objects.AddBrep(stopperBrep);
                myDoc.Views.Redraw();
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
                splitedLeftOver[0].LengthParameter(splitedLeftOver[0].GetLength() - 3, out t);

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
                normal_dir.Rotate(Math.PI / 2, axis);
                Point3d base1Pt = endSuf.ClosestPoint(baseStructureCrv1.PointAtEnd);
                Point3d hingePt1 = base1Pt + normal_dir / 3;
                Point3d hingeInnerPt1 = base1Pt + normal_dir / 5;
                normal_dir.Rotate(Math.PI, axis);
                Point3d hingePt2 = base1Pt;
                Point3d hingeInnerPt2 = base1Pt + 4 / normal_dir.Length * normal_dir;
                double scale = (linearCrv.GetLength() / 5 / 2) / axis.Length;   // 5 is the number of hinge
                Point3d hingePt3 = hingePt1 + axis * scale;
                Point3d hingePt4 = hingePt2 + axis * scale;

                normal_dir = newDir;
                normal_dir.Rotate(-Math.PI / 2, axis);
                Point3d base2Pt = endSuf.ClosestPoint(baseStructureCrv2.PointAtEnd);
                Point3d hingePt5 = base2Pt + normal_dir / 3;
                Point3d hingeInnerPt5 = base2Pt + normal_dir / 5;
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
                double scale1 = scale * 3 / 4;
                double scale2 = scale / 4;
                double scale3 = scale / 2;
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
                linearCrv.LengthParameter(linearCrv.GetLength()-2 * scale1, out endPara);
                Point3d end1 = linearCrv.PointAt(endPara);

                double endCrvPara1 = 0;
                linearCrv.ClosestPoint(end1, out endCrvPara1);
                Curve[] splitCrvs11 = linearCrv.Split(endCrvPara1);
                Curve divideCrv1 = splitCrvs11[0];

                attributes.ObjectColor = Color.Yellow;

                // store all curve segments
                Point3d[] points1;
                divideCrv1.DivideByCount(5, true, out points1); // 5 is the number of hinge
                
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
                    double s = 4 / hinge_normal.Length; // 3 is the thickness of the hinge 
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
                    double s = 4 / hinge_normal.Length; // 3 is the thickness of the hinge 
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
                myDoc.Views.Redraw();

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
                myDoc.Views.Redraw();

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
                #endregion

            }

            myDoc.Views.Redraw();
        }
        private void generateLinearTwistSupport(Plane startSuf, Plane endSuf, Curve centerCrv, Curve compressCrv, Curve pressCrv, Curve stretchCrv, Point3d railEnd, Plane stopperPlnOrig)
        {
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


            // base structure 2 bars
            double baseStructureDisToCenter = 4;
            Curve baseStructureCrv1 = linearCrv.DuplicateCurve();
            Curve baseStructureCrv2 = linearCrv.DuplicateCurve();
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

            foreach (var p in guiderInnerCornerPt)
            {
                myDoc.Objects.AddPoint(p);
                myDoc.Views.Redraw();
            }
            Curve guiderOuterRectCrv = new Polyline(guiderOuterCornerPt).ToNurbsCurve();
            Curve guiderInnerRectCrv = new Polyline(guiderInnerCornerPt).ToNurbsCurve();

            var outerRect = sweep.PerformSweep(linearCrv, guiderOuterRectCrv)[0];
            var innerRect = sweep.PerformSweep(linearCrv, guiderInnerRectCrv)[0];


            var baseBreps = Brep.CreateBooleanIntersection(outerRect, innerRect, myDoc.ModelRelativeTolerance);
            baseBreps[0] = baseBreps[0].CapPlanarHoles(myDoc.ModelRelativeTolerance);
            baseBreps[1] = baseBreps[1].CapPlanarHoles(myDoc.ModelRelativeTolerance);

            myDoc.Objects.AddBrep(baseBreps[0]);
            myDoc.Objects.AddBrep(baseBreps[1]);

            List<Point3d> baseVertexList = new List<Point3d>();
            foreach (Brep b in baseBreps)
            {
                Rhino.Geometry.Collections.BrepVertexList vertexList = b.Vertices;
                if (vertexList != null && vertexList.Count > 0)
                {
                    foreach (var v in vertexList)
                    {
                        baseVertexList.Add(v.Location);
                    }
                }
            }

            Plane guiderPln = new Plane(linearCrv.PointAtEnd, linearCrv.TangentAtEnd);
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


            //cylindric structure that allows rotation
            Point3d centerCylinPt = centerCrv.PointAtStart;
            double cylinBaseRadius = 1;
            Circle cylinCircle = new Circle(startSuf, centerCylinPt, cylinBaseRadius);
            Curve cylinCrv = cylinCircle.ToNurbsCurve();
            Brep cylinBrep = sweep.PerformSweep(pressCrv, cylinCrv)[0];
            cylinBrep = cylinBrep.CapPlanarHoles(myDoc.ModelRelativeTolerance);
            myDoc.Objects.AddBrep(cylinBrep);
            myDoc.Views.Redraw();

            // stopper (disc)
            Plane stopperPln = new Plane(pressCrv.PointAtEnd, pressCrv.TangentAtEnd);
            Circle stopperCir = new Circle(stopperPln, pressCrv.PointAtEnd, 2);
            double t;
            compressCrv.LengthParameter(3, out t);
            Curve stopperCrv = compressCrv.Split(t)[0];

            var stopperBrep = sweep.PerformSweep(stopperCrv, stopperCir.ToNurbsCurve())[0];
            stopperBrep = stopperBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);

            myDoc.Objects.AddCurve(stopperCrv);
            myDoc.Objects.AddBrep(stopperBrep);
            myDoc.Views.Redraw();


            // guider hole
            Point3d guiderPt = railEnd;
            double guiderPtGap = 0.2;
            Circle guiderCircle = new Circle(stopperPlnOrig, guiderPt, cylinBaseRadius+guiderPtGap); 
            Curve guiderCrv = guiderCircle.ToNurbsCurve();

            var attributes = new ObjectAttributes();
            attributes.ObjectColor = Color.Yellow;
            attributes.ColorSource = ObjectColorSource.ColorFromObject;
            myDoc.Objects.AddCurve(guiderCrv, attributes);

            pressCrv.ClosestPoint(railEnd, out t);
            var splitedLeftOver = pressCrv.Split(t);
            splitedLeftOver[0].LengthParameter(splitedLeftOver[0].GetLength() - 3, out t);

            var splitedLeftCrvs = splitedLeftOver[0].Split(t);
            Brep guiderCenter = sweep.PerformSweep(splitedLeftCrvs[1], guiderCrv)[0];
            guiderCenter = guiderCenter.CapPlanarHoles(myDoc.ModelRelativeTolerance);

            //guider outcube
            Brep outerGuider = sweep.PerformSweep(splitedLeftCrvs[1], guiderTopCrv)[0];
            outerGuider = outerGuider.CapPlanarHoles(myDoc.ModelRelativeTolerance);
            outerGuider.Flip();
            var guiderFinal = Brep.CreateBooleanDifference(guiderCenter, outerGuider,myDoc.ModelRelativeTolerance)[0];
            myDoc.Objects.Add(guiderFinal);

            myDoc.Views.Redraw();
        }
        private void generateTwistSupport(Plane startSuf, Plane endSuf, Curve centerCrv)
        {
            // create sweep function
            var sweep = new Rhino.Geometry.SweepOneRail();
            sweep.AngleToleranceRadians = myDoc.ModelAngleToleranceRadians;
            sweep.ClosedSweep = false;
            sweep.SweepTolerance = myDoc.ModelAbsoluteTolerance;

            // compute the base height and generate the guide curves
            double t;
            centerCrv.LengthParameter(centerCrv.GetLength() - 5, out t);  // the height is currently 10. It should be confined with the limit from the test
            Curve guiderCrv = centerCrv.Split(t)[1];
            Curve cylinCrv = centerCrv.Split(t)[0];
            guiderCrv.LengthParameter(0.5, out t);
            Curve cylinGap = guiderCrv.Split(t)[0];
            Curve guiderCrvLeftover = guiderCrv.Split(t)[1];

            //var attributes = new ObjectAttributes();
            //attributes.ObjectColor = Color.Yellow;
            //attributes.ColorSource = ObjectColorSource.ColorFromObject;
            //myDoc.Objects.AddCurve(cylinGap, attributes);

            //var attributes1 = new ObjectAttributes();
            //attributes1.ObjectColor = Color.Blue;
            //attributes1.ColorSource = ObjectColorSource.ColorFromObject;
            //myDoc.Objects.AddCurve(guiderCrvLeftover, attributes1);

            myDoc.Views.Redraw();

            List<Curve> cylinCrvList = new List<Curve>();
            cylinCrvList.Add(cylinCrv);
            cylinCrvList.Add(cylinGap);
            Curve cylinCrvAll = Curve.JoinCurves(cylinCrvList)[0];

            // base structure 2 bars
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

            foreach (var p in guiderInnerCornerPt)
            {
                myDoc.Objects.AddPoint(p);
                myDoc.Views.Redraw();
            }
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
            foreach (Brep b in baseBreps)
            {
                Rhino.Geometry.Collections.BrepVertexList vertexList = b.Vertices;
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

            //cylindral structure that enables rotation
            Point3d centerCylin = centerCrv.PointAtStart;
            double cylinBaseSideRadius = 1;
            Curve cylinCircle = new Circle(startSuf, centerCylin, cylinBaseSideRadius).ToNurbsCurve();
            Brep cylinBrep = sweep.PerformSweep(cylinCrvAll, cylinCircle)[0];
            cylinBrep = cylinBrep.CapPlanarHoles(myDoc.ModelRelativeTolerance);
            myDoc.Objects.AddBrep(cylinBrep);
            myDoc.Views.Redraw();

            // stopper (disc)
            Plane stopperPln = new Plane(cylinCrvAll.PointAtEnd, cylinCrvAll.TangentAtEnd);
            Circle stopperCir = new Circle(stopperPln, cylinCrvAll.PointAtEnd, 3);
            double tt;
            guiderCrvLeftover.LengthParameter(guiderCrvLeftover.GetLength() - 3, out tt);
            Curve stopperCrv = guiderCrvLeftover.Split(tt)[1];
            var stopperBrep = sweep.PerformSweep(stopperCrv, stopperCir.ToNurbsCurve())[0];
            stopperBrep = stopperBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);

            myDoc.Objects.AddCurve(stopperCrv);
            myDoc.Objects.AddBrep(stopperBrep);
            myDoc.Views.Redraw();

            // guider hole

            Point3d guiderPt = cylinCrv.PointAtEnd;
            double guiderPtGap = 0.2;
            double newRadius = cylinBaseSideRadius + guiderPtGap;
            Plane stopperPln1 = new Plane(cylinCrv.PointAtEnd, cylinCrv.TangentAtEnd);
            Curve guiderCircle = new Circle(stopperPln,guiderPt, newRadius).ToNurbsCurve();

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


            myDoc.Views.Redraw();
        }
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

                    foreach (var p in guiderInnerCornerPt)
                    {
                        myDoc.Objects.AddPoint(p);
                        myDoc.Views.Redraw();
                    }
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

                    myDoc.Objects.AddCurve(stopperCrv);
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
                    myDoc.Objects.AddCurve(hingeOuterRectCrv, attributes);
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
                    myDoc.Objects.AddCurve(hingeInnerRectCrv, attributes);
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
                    myDoc.Objects.AddPoint(end_pt, attributes);
                    myDoc.Views.Redraw();

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
                        myDoc.Objects.AddPoint(p, attributes);
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

                        myDoc.Objects.AddCurve(tempOuterCrv, attributes);
                        myDoc.Objects.AddCurve(tempInnerCrv, attributes);
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

                    //// prepared for difference boolean with the central rod
                    //innerBrepsDup = innerBreps;

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

                    foreach (var p in guiderInnerCornerPt)
                    {
                        myDoc.Objects.AddPoint(p);
                        myDoc.Views.Redraw();
                    }
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

                    myDoc.Objects.AddCurve(stopperCrv);
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

                    //// prepared for difference boolean with the central rod
                    //innerBrepsDup = innerBreps;

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
        private void generateBendSupport(Plane startSuf, Plane endSuf, Curve centerCrv, List<bend_info> bendInfoList, Point3d startPt, Point3d endPt, bool isStart)
        {
            foreach(bend_info b in bendInfoList)
            {
                Vector3d normal_dir = b.dir;
                Vector3d axis;
                if (isStart)
                {
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
                    double scale = (centerCrv.GetLength() / 8 / 2) / axis.Length;   // 8 is the number of hinge
                    Point3d hingePt3 = hingePt1 + axis * scale;
                    Point3d hingePt4 = hingePt2 + axis * scale;

                    Point3d[] hingeOuterCornerPt = new Point3d[5];
                    hingeOuterCornerPt[0] = hingePt1;
                    hingeOuterCornerPt[1] = hingePt2;
                    hingeOuterCornerPt[2] = hingePt4;
                    hingeOuterCornerPt[3] = hingePt3;
                    hingeOuterCornerPt[4] = hingePt1;
                    Curve hingeOuterRectCrv = new Polyline(hingeOuterCornerPt).ToNurbsCurve();
                    myDoc.Objects.AddCurve(hingeOuterRectCrv, attributes);
                    myDoc.Views.Redraw();
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
                    myDoc.Objects.AddCurve(hingeInnerRectCrv, attributes);
                    myDoc.Views.Redraw();
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
                    divideCrv.DivideByCount(8, true, out points); // 8 is the number of hinge

                    // store tangent vectors at each point
                    List<Vector3d> tangents = new List<Vector3d>(); 
                    foreach (Point3d p in points)
                    {
                        double para = 0;
                        divideCrv.ClosestPoint(p, out para);
                        tangents.Add(divideCrv.TangentAt(para));
                        myDoc.Objects.AddPoint(p, attributes);
                    }

                    // store transforms from the first point to each point
                    List<List<Transform>> trans = new List<List<Transform>>();
                    Vector3d v0 = tangents.ElementAt(0);
                    Point3d p0 = points.ElementAt(0);
                    int idx = 0;
                    foreach(Vector3d v1 in tangents)
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
                    foreach(List<Transform> t in trans)
                    {
                        Curve tempOuterCrv = hingeOuterRectCrv.DuplicateCurve();
                        tempOuterCrv.Transform(t.ElementAt(0));
                        tempOuterCrv.Transform(t.ElementAt(1));
                        outerCrvs.Add(tempOuterCrv);

                        Curve tempInnerCrv = hingeInnerRectCrv.DuplicateCurve();
                        tempInnerCrv.Transform(t.ElementAt(0));
                        tempInnerCrv.Transform(t.ElementAt(1));
                        innerCrvs.Add(tempInnerCrv);

                        myDoc.Objects.AddCurve(tempOuterCrv, attributes);
                        myDoc.Objects.AddCurve(tempInnerCrv, attributes);
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
                        Vector3d hinge_normal = surf.NormalAt(wde/2, hgt/2);
                        double s = 1 / hinge_normal.Length; // 3 is the thickness of the hinge 
                        Transform rectTrans = Transform.Translation(hinge_normal * s);
                        c.Transform(rectTrans);
                       
                        Brep brep= Brep.CreateFromSurface(Surface.CreateExtrusion(c, -2*hinge_normal*s));
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

                    //// prepared for difference boolean with the central rod
                    //innerBrepsDup = innerBreps;

                    #endregion

                    #endregion

                    #region sweep the central rod
                    double half_width_scale = 1 / normal_dir.Length; // 1 is the half width of the central rod, adjustable
                    double half_height_scale = 1 / b.dir.Length; // 2 is the half width of the central rod, adjustable, equal to the extusion height
                    Point3d rodPt1 = startPt + normal_dir * half_width_scale + b.dir*half_height_scale;
                    normal_dir.Rotate(Math.PI, axis);
                    Point3d rodPt2 = startPt + normal_dir * half_width_scale + b.dir*half_height_scale;
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
                        myDoc.Objects.AddBrep(tempB[1], attributes);
                        tempB[0].Flip();
                        prev_brep = tempB[0];
                        //myDoc.Views.Redraw();
                    }

                    // generate the hinges
                    var firstHinge = Brep.CreateBooleanDifference(innerBreps[0], outerBreps[0], myDoc.ModelRelativeTolerance);
                    myDoc.Objects.AddBrep(firstHinge[0], attributes);
                    myDoc.Views.Redraw();

                    foreach (List<Transform> t in trans)
                    {
                        if(trans.IndexOf(t) != 0)
                        {
                            Brep tempBrep = firstHinge[0].DuplicateBrep();
                            tempBrep.Transform(t.ElementAt(0));
                            tempBrep.Transform(t.ElementAt(1));
                            myDoc.Objects.AddBrep(tempBrep, attributes);
                        }
 
                    }


                    #endregion

                }
                else
                {
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
                    double scale = (centerCrv.GetLength() / 8 / 2) / axis.Length;   // 8 is the number of hinge
                    Point3d hingePt3 = hingePt1 + axis * scale;
                    Point3d hingePt4 = hingePt2 + axis * scale;

                    Point3d[] hingeOuterCornerPt = new Point3d[5];
                    hingeOuterCornerPt[0] = hingePt1;
                    hingeOuterCornerPt[1] = hingePt2;
                    hingeOuterCornerPt[2] = hingePt4;
                    hingeOuterCornerPt[3] = hingePt3;
                    hingeOuterCornerPt[4] = hingePt1;
                    Curve hingeOuterRectCrv = new Polyline(hingeOuterCornerPt).ToNurbsCurve();
                    myDoc.Objects.AddCurve(hingeOuterRectCrv, attributes);
                    myDoc.Views.Redraw();
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
                    myDoc.Objects.AddCurve(hingeInnerRectCrv, attributes);
                    myDoc.Views.Redraw();
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
                    divideCrv.DivideByCount(8, true, out ps); // 8 is the number of hinge
                    for(int j = ps.Count()-1; j>=0; j--)
                    {
                        points.Add(ps[j]);
                    }

                    // store tangent vectors at each point
                    List<Vector3d> tangents = new List<Vector3d>();
                    foreach (Point3d p in points)
                    {
                        double para = 0;
                        divideCrv.ClosestPoint(p, out para);
                        tangents.Add(divideCrv.TangentAt(para)*(-1));
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

                        myDoc.Objects.AddCurve(tempOuterCrv, attributes);
                        myDoc.Objects.AddCurve(tempInnerCrv, attributes);
                        myDoc.Views.Redraw();
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

                    //// prepared for difference boolean with the central rod
                    //innerBrepsDup = innerBreps;

                    #endregion

                    #endregion

                    #region sweep the central rod
                    double half_width_scale = 1 / normal_dir.Length; // 1 is the half width of the central rod, adjustable
                    double half_height_scale = 1 / b.dir.Length; // 2 is the half width of the central rod, adjustable, equal to the extusion height
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
                        myDoc.Objects.AddBrep(tempB[1], attributes);
                        tempB[0].Flip();
                        prev_brep = tempB[0];
                        //myDoc.Views.Redraw();
                    }

                    // generate the hinges
                    var firstHinge = Brep.CreateBooleanDifference(innerBreps[0], outerBreps[0], myDoc.ModelRelativeTolerance);
                    myDoc.Objects.AddBrep(firstHinge[0], attributes);
                    myDoc.Views.Redraw();

                    foreach (List<Transform> t in trans)
                    {
                        if (trans.IndexOf(t) != 0)
                        {
                            Brep tempBrep = firstHinge[0].DuplicateBrep();
                            tempBrep.Transform(t.ElementAt(0));
                            tempBrep.Transform(t.ElementAt(1));
                            myDoc.Objects.AddBrep(tempBrep, attributes);
                        }

                    }

                    #endregion
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
            e.Display.DrawSphere(new Sphere(this.bendCtrlPt, 2), Color.White);
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
                if (distanceBtwTwoPts(this.bendPtSel, this.centerCrv.PointAtEnd) <= 2)
                {
                    // hightlight the end sphere
                    e.Display.DrawSphere(new Sphere(this.centerCrv.PointAtEnd, 2), Color.Yellow);
                }
                else
                {
                    e.Display.DrawSphere(new Sphere(this.centerCrv.PointAtEnd, 2), Color.White);
                }
            }
            else
            {
                if (distanceBtwTwoPts(this.bendPtSel, this.centerCrv.PointAtStart) <= 2)
                {
                    // highlight the start sphere
                    e.Display.DrawSphere(new Sphere(this.centerCrv.PointAtStart, 2), Color.Yellow);
                }
                else if (distanceBtwTwoPts(this.bendPtSel, this.centerCrv.PointAtEnd) <= 2)
                {
                    // hightlight the end sphere
                    e.Display.DrawSphere(new Sphere(this.centerCrv.PointAtEnd, 2), Color.Yellow);
                }
                else
                {
                    e.Display.DrawSphere(new Sphere(this.centerCrv.PointAtStart, 2), Color.White);
                    e.Display.DrawSphere(new Sphere(this.centerCrv.PointAtEnd, 2), Color.White);
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
            e.Display.DrawSphere(new Sphere(angleCtrlPt, 2), System.Drawing.Color.White);
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
            e.Display.DrawSphere(new Sphere(angleCtrlSecPt, 2), System.Drawing.Color.White);

            // compute the angle in real time
            Vector3d v1 = (Vector3d)(angleCtrlPt - this.middlePt);
            Vector3d v2 = (Vector3d)(angleCtrlSecPt - this.middlePt);
            this.twistAngle = Vector3d.VectorAngle(v1, v2);
            RhinoApp.WriteLine("The rotation angle: " + twistAngle);

            e.Display.DrawCircle(this.angleCircle, Color.White);
            e.Display.DrawLine(new Line(this.middlePt, this.angleCtrlSecPt), Color.White);
            e.Display.DrawArc(new Arc(anglePlane, 15, this.twistAngle), Color.White);
            e.Display.Draw3dText(new Rhino.Display.Text3d(this.twistAngle.ToString()), Color.White, new Point3d(angleCtrlSecPt.X+3, angleCtrlSecPt.Y+3, angleCtrlSecPt.Z+3));
        }
        private void Gp_CurveDynamicDraw(object sender, Rhino.Input.Custom.GetPointDrawEventArgs e)
        {
            e.Display.DrawSphere(new Sphere(centerPt, 2), System.Drawing.Color.Azure);
        }
        private void Gp_CurveDynamicDrawStretch(object sender, Rhino.Input.Custom.GetPointDrawEventArgs e)
        {
            e.Display.DrawSphere(new Sphere(centerPt, 2), System.Drawing.Color.BlueViolet);
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