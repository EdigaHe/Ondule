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

        // all slected point's normals
        List<Vector3d> normalList = new List<Vector3d>();

        List<Point3d> wireframePtList = new List<Point3d>();

        List<Point3d> dyndrawLst = new List<Point3d>();
        Rhino.Collections.RhinoList<Guid> ptIds = new Rhino.Collections.RhinoList<Guid>();

        ObjectAttributes redAttribute, whiteAttribute, greenAttribute, orangeAttribute;

        Curve centerCrv = null;

        Point3d centerPt;
        Point3d angleCtrlPt;
        Plane anglePlane;
        Point3d angleCtrlSecPt;
        Circle angleCircle;
        double twistAngle = 0;

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

        private void springGeneration(Curve centerCrv, Brep surfaceBrep, double pitch)
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
                    myDoc.Objects.AddBrep(b.CapPlanarHoles(myDoc.ModelRelativeTolerance));
                }
            }
            #endregion
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
            
            Rhino.DocObjects.ObjectAttributes attr = new Rhino.DocObjects.ObjectAttributes();
            attr.LayerIndex = 6;
            myDoc.Objects.AddCurve(stretchCrv, attr);
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
                    finalPreservedBrepList.Add(b);
                }

            }


            foreach(Brep b in finalPreservedBrepList)
            {
                myDoc.Objects.AddBrep(b,redAttribute);
            }

            #endregion

            #region Generate support structure
            generateLinearSupport(startPln, endPln, centerCrv, compressCrv, pressCrv, stretchCrv, railEnd);
            #endregion

            myDoc.Objects.Hide(sufObjId, true);// hide the original shell
            myDoc.Views.Redraw();
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
            double centerPara = 0;
            centerCrv.LengthParameter(centerCrv.GetLength() / 2, out centerPara);
            Point3d centerPt = centerCrv.PointAt(centerPara);
            
            Plane anglePln = new Plane(centerPt, centerCrv.TangentAt(centerPara)); // the plane at the center of the central curve
            this.anglePlane = anglePln;
            this.centerPt = centerPt;
            this.angleCtrlPt = centerPt;

            Rhino.Geometry.Sphere sphere = new Rhino.Geometry.Sphere(centerPt, 2);
            Rhino.Geometry.PlaneSurface centerPlnSurf = new PlaneSurface(anglePln, new Interval(-25, 25), new Interval(-25, 25));
            myDoc.Objects.AddSurface(centerPlnSurf);

            var attributes = new ObjectAttributes();
            attributes.ObjectColor = Color.White;
            attributes.ColorSource = ObjectColorSource.ColorFromObject;
            myDoc.Objects.AddSphere(sphere, attributes);
            myDoc.Views.Redraw();

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
                    finalPreservedBrepList.Add(b);
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


        }


        /// <summary>
        /// This function generate support structure for compression
        /// </summary>
        /// <param name="startSuf">The start surface plane of the center axis</param>
        /// <param name="endSuf">The end surface plane of the center axis</param>
        /// <param name="centerCrv">Center axis</param>
        /// <param name="compressCrv">Compression curve, which is one side of the centerCrve splited by the user's control point input</param>
        /// <param name="pressCrv">the other side of the center curve split</param>
        private void generateLinearSupport(Plane startSuf, Plane endSuf, Curve centerCrv, Curve compressCrv, Curve pressCrv, Curve stretchCrv, Point3d railEnd)
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
            Brep prismBrep = sweep.PerformSweep(pressCrv, prismCrv)[0];
            prismBrep = prismBrep.CapPlanarHoles(myDoc.ModelRelativeTolerance);
            myDoc.Objects.AddBrep(prismBrep);
            myDoc.Views.Redraw();


            // stopper (disc)
            Plane stopperPln = new Plane(pressCrv.PointAtEnd, pressCrv.TangentAtEnd);
            Circle stopperCir = new Circle(stopperPln, pressCrv.PointAtEnd, 2);
            double t;
            compressCrv.LengthParameter(compressCrv.GetLength()-3, out t);
            Curve stopperCrv = compressCrv.Split(t)[1];
            var stopperBrep = sweep.PerformSweep(stopperCrv, stopperCir.ToNurbsCurve())[0];
            stopperBrep = stopperBrep.CapPlanarHoles(myDoc.ModelAbsoluteTolerance);

            myDoc.Objects.AddCurve(stopperCrv);
            myDoc.Objects.AddBrep(stopperBrep);
            myDoc.Views.Redraw();
            
            
            // guider hole

            Point3d guiderPt = railEnd;
            double guiderPtGap = 0.2;
            txp = Transform.Translation(stopperPln.XAxis * (primBaseSideHalfLength + guiderPtGap));
            typ = Transform.Translation(stopperPln.YAxis * (primBaseSideHalfLength + guiderPtGap));
            txn = Transform.Translation(stopperPln.XAxis * -(primBaseSideHalfLength + guiderPtGap));
            tyn = Transform.Translation(stopperPln.YAxis * -(primBaseSideHalfLength + guiderPtGap));

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
            var guiderFinal = Brep.CreateBooleanIntersection(outerGuider, guiderCenter, myDoc.ModelRelativeTolerance)[0];
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
            e.Display.DrawLine(new Line(this.centerPt, this.angleCtrlPt), Color.White);
            this.angleCircle = new Circle(this.anglePlane, this.centerPt, Math.Sqrt(Math.Pow(this.angleCtrlPt.X - this.centerPt.X, 2) + Math.Pow(this.angleCtrlPt.Y - this.centerPt.Y, 2)) + Math.Pow(this.angleCtrlPt.Z - this.centerPt.Z, 2));
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
            Vector3d v1 = (Vector3d)(angleCtrlPt - centerPt);
            Vector3d v2 = (Vector3d)(angleCtrlSecPt - centerPt);
            this.twistAngle = Vector3d.VectorAngle(v1, v2);
            RhinoApp.WriteLine("The rotation angle: " + twistAngle);

            e.Display.DrawCircle(this.angleCircle, Color.White);
            e.Display.DrawLine(new Line(this.centerPt, this.angleCtrlSecPt), Color.White);
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

        
        #endregion

    }

}