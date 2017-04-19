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
        Guid[] helicalCurve(ObjRef obj, Guid numCurve, Guid numPipe, double pitch, double turns, double springD, double coilD);
        Guid[] machineCurve(ObjRef obj, Guid numCurve, Guid numSweep, double pitch, double turns, double springD, double width, double height);
        Guid[] zCurve(ObjRef obj, Guid numCurve, Guid numSweep, double width, double height);
        Guid pipeCurve(Curve crv, Guid numPipe, double coilD);
        Guid sweepCurve(Curve crv, Guid numPipe, double width, double height);
    }

    public class IncRhinoModel : RhinoModel
    {
        
        RhinoDoc myDoc = null;

        public IncRhinoModel()
        {
            myDoc = PluginBarCommand.rhinoDoc;
            if (myDoc == null)
            {
                myDoc = RhinoDoc.ActiveDoc;
            }

            myDoc.Views.Redraw();
        }

        // Instantiate a helical curve object and add to the viewport
        public Guid[] helicalCurve(ObjRef obj, Guid numCurve, Guid numPipe, double pitch, double turns, double springD, double coilD)
        {
            Curve crv = obj.Curve();
            Curve spiralCrv = null;

            spiralCrv = NurbsCurve.CreateSpiral(crv, crv.Domain.T0, crv.Domain.T1, new Point3d(0, 0, 0), pitch, turns, springD / 2, springD / 2, 12);

            if (numCurve.CompareTo(Guid.Empty) == 0)
            {
                numCurve = myDoc.Objects.AddCurve(spiralCrv); 
            }
            else
            {
                myDoc.Objects.Replace(numCurve, spiralCrv);
            }

            numPipe = pipeCurve(spiralCrv, numPipe, coilD);

            myDoc.Views.Redraw();
            return new Guid[]{numCurve, numPipe};
        }
        

        // Instantiate a machined curve object and add to the viewport
        public Guid[] machineCurve(ObjRef obj, Guid numCurve, Guid numSweep, double pitch, double turns, double springD, double width, double height)
        {
            Curve crv = obj.Curve();
            Curve spiralCrv = null;

            spiralCrv = NurbsCurve.CreateSpiral(crv, crv.Domain.T0, crv.Domain.T1, new Point3d(0, 0, 0), pitch, turns, springD / 2, springD / 2, 12);

            if (numCurve.CompareTo(Guid.Empty) == 0)
            {
                numCurve = myDoc.Objects.AddCurve(spiralCrv);
            }
            else
            {
                myDoc.Objects.Replace(numCurve, spiralCrv);
            }

            numSweep = sweepCurve(spiralCrv, numSweep, width, height);

            myDoc.Views.Redraw();
            return new Guid[] { numCurve, numSweep };
        }


        // Instantiate a polyline object and add to the viewport
        public Guid[] zCurve(ObjRef obj, Guid numCurve, Guid numSweep, double width, double height)
        {
            Curve crv = obj.Curve();
            Polyline zCrv = new Polyline();
            //zCrv.Add(0, 0, 0);

            double depth = 5;

            for (int i = 0; i < 50; i += 3)
            {
                if (i % 2 == 0)
                {
                    zCrv.Add(i, -depth / 2, 0);
                }
                else
                {
                    zCrv.Add(i, depth / 2, 0);
                }
            }

            Curve zzz = zCrv.ToNurbsCurve();

            // Translate
            zzz.Translate(crv.PointAtStart.X, crv.PointAtStart.Y, crv.PointAtStart.Z);

            // Rotate
            double crvX = crv.PointAtEnd.X - crv.PointAtStart.X;
            double crvY = crv.PointAtEnd.Y - crv.PointAtStart.Y;
            double crvZ = crv.PointAtEnd.Z - crv.PointAtStart.Z;

            double zzzX = zzz.PointAtEnd.X - zzz.PointAtStart.X;
            double zzzY = zzz.PointAtEnd.Y - zzz.PointAtStart.Y;
            double zzzZ = zzz.PointAtEnd.Z - zzz.PointAtStart.Z;

            double crvDist = Math.Pow(Math.Pow(crvX, 2) + Math.Pow(crvY, 2) + Math.Pow(crvZ, 2), 0.5);
            double zzzDist = Math.Pow(Math.Pow(zzzX, 2) + Math.Pow(zzzY, 2) + Math.Pow(zzzZ, 2), 0.5);

            double dot = crvX  * zzzX + crvY  * zzzY + crvZ  * zzzZ;
            double angle = -Math.Acos(dot / (crvDist * zzzDist));
            Vector3d cross = new Vector3d(crvY * zzzZ - crvZ * zzzY, crvZ * zzzX - crvX * zzzZ, crvX * zzzY - crvY * zzzX);

            zzz.Rotate(angle, cross, crv.PointAtStart);

            // Scale

            if (numCurve.CompareTo(Guid.Empty) == 0)
            {
                numCurve = myDoc.Objects.AddCurve(zzz);
            }
            else
            {
                myDoc.Objects.Replace(numCurve, zzz);
            }

            numSweep = sweepCurve(zzz, numSweep, width, height);

            myDoc.Views.Redraw();
            return new Guid[] { numCurve, numSweep };
        }




        public Guid pipeCurve(Curve crv, Guid numPipe, double coilD)
        {
            Brep[] pipe = null;
            PipeCapMode cap = PipeCapMode.Flat;

            pipe = Brep.CreatePipe(crv, coilD / 2, false, cap, false, 0.1, 0.1);

            if (numPipe.CompareTo(Guid.Empty) == 0)
            {
                numPipe = myDoc.Objects.AddBrep(pipe[0]);
            }
            else
            {
                myDoc.Objects.Replace(numPipe, pipe[0]);
            }

            return numPipe;
        }

        public Guid sweepCurve(Curve crv, Guid numSweep, double width, double height)
        {
            Brep[] sweep = null;
            SweepOneRail sweepOne = new SweepOneRail();
            Polyline rect = new Polyline();

            Point3d startPoint = crv.PointAtStart;
            Vector3d startVector = crv.TangentAtStart;

            Plane norm = new Plane(startPoint, startVector);
            Rectangle3d rect2 = new Rectangle3d(norm, width, height);

            //double xPos = startPoint.X + width / 2;
            //double xNeg = startPoint.X - width / 2;
            //double yPos = startPoint.Y + height / 2;
            //double yNeg = startPoint.Y - height / 2;

            //double sum = startVector.X*startPoint.X + startVector.Y*startPoint.Y + startVector.Z*startPoint.Z;

            //double x0 = startVector.X * xPos; // ++
            //double x1 = startVector.X * xNeg; // -+
            //double x2 = startVector.X * xNeg; // --
            //double x3 = startVector.X * xPos; // +-
            //double x4 = startVector.X * xPos; // ++

            //double y0 = startVector.Y * yPos; // ++
            //double y1 = startVector.Y * yPos; // -+
            //double y2 = startVector.Y * yNeg; // --
            //double y3 = startVector.Y * yNeg; // +-
            //double y4 = startVector.Y * yPos; // ++

            //double z0 = (sum - x0 - y0) / startVector.Z; // ++
            //double z1 = (sum - x1 - y1) / startVector.Z; // -+
            //double z2 = (sum - x2 - y2) / startVector.Z; // --
            //double z3 = (sum - x3 - y3) / startVector.Z; // +-
            //double z4 = (sum - x4 - y4) / startVector.Z; // ++

            //rect.Add(x0, y0, z0);
            //rect.Add(x1, y1, z1);
            //rect.Add(x2, y2, z2);
            //rect.Add(x3, y3, z3);
            //rect.Add(x4, y4, z4);

            //rect.Add(width/2,0,height/2);
            //rect.Add(-width / 2, 0, height / 2);
            //rect.Add(-width / 2, 0, -height / 2);
            //rect.Add(width / 2, 0, -height / 2);
            //rect.Add(width / 2, 0, height / 2);

            Curve rectCrv = rect2.ToNurbsCurve();

            sweep = sweepOne.PerformSweep(crv, rectCrv);

            if (numSweep.CompareTo(Guid.Empty) == 0)
            {
                numSweep = myDoc.Objects.AddBrep(sweep[0]);
            }
            else
            {
                myDoc.Objects.Replace(numSweep, sweep[0]);
            }

            return numSweep;
        }

    }

}