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
            Rhino.Geometry.PlaneSurface startPlnSuf = new PlaneSurface(startPln, new Interval(-15, 15), new Interval(-15, 15));// convert the plane object to an actual surface so that we can draw it in the rhino doc
            Rhino.Geometry.PlaneSurface endPlnSuf = new PlaneSurface(endPln, new Interval(-15, 15), new Interval(-15, 15));// 15 is a random number. It should actually be the ourter shell's radius or larger.
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

            #region generate spring 
            //DEBUG: Currently the bug is the center curve is only cut when there is a discontinuity, this is not good enough to have a nice spring approximation to the outer shell's shape.
            //1. Find center curve's discontinuity
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

            //2. gennerate spiral for each segment of the curve
            Point3d spiralStartPt = new Point3d(0, 0, 0);
            List<NurbsCurve> spiralCrvList = new List<NurbsCurve>();
            if (discontinueCrv != null)
            {
                foreach (Curve crv in discontinueCrv)
                {
                    crv.LengthParameter(crv.GetLength(), out endPara);
                    double r1=5, r2=5;
                    r1 = surfaceBrep.ClosestPoint(crv.PointAtStart).DistanceTo(crv.PointAtStart);
                    r2 = surfaceBrep.ClosestPoint(crv.PointAtEnd).DistanceTo(crv.PointAtEnd);
                    NurbsCurve spiralCrv = NurbsCurve.CreateSpiral(crv, 0, endPara, spiralStartPt, 5, 0, r1, r2, 12);
                    spiralStartPt = spiralCrv.PointAtEnd;
                    spiralCrvList.Add(spiralCrv);
                }
            }

            Curve joinedSpiral = Curve.JoinCurves(spiralCrvList)[0];
            if (joinedSpiral != null)
            {
                myDoc.Objects.AddCurve(joinedSpiral);
            }

            //3. sweep section to create spring solid
            Plane spiralStartPln = new Plane(joinedSpiral.PointAtStart, joinedSpiral.TangentAtStart);
            Circle startCircle = new Circle(spiralStartPln, joinedSpiral.PointAtStart, 1); //spring's cross section's radius is currently 1. This should be tuned based on the shape the user selected and also the test limit we have.
            var sweep = new Rhino.Geometry.SweepOneRail();
            sweep.AngleToleranceRadians = myDoc.ModelAngleToleranceRadians;
            sweep.ClosedSweep = false;
            sweep.SweepTolerance = myDoc.ModelAbsoluteTolerance;

            var breps = sweep.PerformSweep(joinedSpiral, startCircle.ToNurbsCurve());
            List<Brep> cappedSpring = new List<Brep>();
            if(breps.Length>0)
            {
                foreach (Brep b in breps)
                {
                    cappedSpring.Add(b.CapPlanarHoles(myDoc.ModelRelativeTolerance));
                    myDoc.Objects.AddBrep(b.CapPlanarHoles(myDoc.ModelRelativeTolerance));
                }
            }
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
            generateCompressSupport(startPln, endPln, centerCrv, compressCrv, pressCrv);
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
        private void generateCompressSupport(Plane startSuf, Plane endSuf, Curve centerCrv, Curve compressCrv, Curve pressCrv)
        {
            // create sweep function
            var sweep = new Rhino.Geometry.SweepOneRail();
            sweep.AngleToleranceRadians = myDoc.ModelAngleToleranceRadians;
            sweep.ClosedSweep = false;
            sweep.SweepTolerance = myDoc.ModelAbsoluteTolerance;


            // base structure 2 bars
            double baseStructureDisToCenter = 4;
            Curve baseStructureCrv1 = compressCrv.DuplicateCurve();
            Curve baseStructureCrv2 = compressCrv.DuplicateCurve();
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

            var outerRect = sweep.PerformSweep(compressCrv, guiderOuterRectCrv)[0];
            var innerRect = sweep.PerformSweep(compressCrv, guiderInnerRectCrv)[0];


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

            Plane guiderPln = new Plane(compressCrv.PointAtEnd, compressCrv.TangentAtEnd);
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

            Point3d guiderPt = pressCrv.PointAtEnd;
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
            pressCrv.LengthParameter(pressCrv.GetLength()-3, out t);
            var splitedPressCrvs = pressCrv.Split(t);
            Brep guiderCenter = sweep.PerformSweep(splitedPressCrvs[1], guiderCrv)[0];
            guiderCenter = guiderCenter.CapPlanarHoles(myDoc.ModelRelativeTolerance);

            //guider outcube
            Brep outerGuider = sweep.PerformSweep(splitedPressCrvs[1], guiderTopCrv)[0];
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