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
        Guid[] helicalCurve(ObjRef obj, Guid numCurve, Guid numPipe, 
                            double pitch, double turns, double springD,
                            double coilD);
        Guid[] machineCurve(ObjRef obj, Guid numCurve, Guid numSweep, 
                            double pitch, double turns, double springD, 
                            double width, double height);
        Guid[] zCurve(ObjRef obj, Guid numCurve, Guid numSweep, double width, 
                            double height);
        Guid pipeCurve(Curve crv, Guid numPipe, double coilD);
        Guid sweepCurve(Curve crv, Guid numPipe, double width, double height);
    }

    public class IncRhinoModel : RhinoModel
    {

        RhinoDoc myDoc;

        // Public constructor
        public IncRhinoModel()
        {
            // Obtain an instance to the RhinoDoc
            myDoc = PluginBarCommand.rhinoDoc;
            if (myDoc == null) myDoc = RhinoDoc.ActiveDoc;
            myDoc.Views.Redraw();
        }

        // CURVE CREATION METHODS
        // _________________________________________________________________

        // Instantiate a helical curve object and add to the viewport
        public Guid[] helicalCurve(ObjRef obj, Guid curveID, Guid pipeID, 
                                   double pitch, double turns, double springD, 
                                   double coilD)
        {
            // Axis Curve: the axis around which the path will be formed  
            Curve axisCurve = obj.Curve();

            // Path Curve: the curve that forms the outline of the spring
            Curve pathCurve = NurbsCurve.CreateSpiral(axisCurve,
                                                axisCurve.Domain.T0, 
                                                axisCurve.Domain.T1, 
                                                new Point3d(0, 0, 0), 
                                                pitch, 
                                                turns, 
                                                springD / 2,
                                                springD / 2,
                                                12);

            // Assess whether the path curve object has been created
            // Depending on the answer, create or replace
            if (curveID.CompareTo(Guid.Empty) == 0)
            {
                curveID = myDoc.Objects.AddCurve(pathCurve); 
            }
            else
            {
                myDoc.Objects.Replace(curveID, pathCurve);
            }

			pipeID = pipeCurve(pathCurve, pipeID, coilD);
			myDoc.Views.Redraw(); // Call the viewport to refresh
			return new Guid[]{curveID, pipeID};
        }
        

        // Instantiate a machined curve object and add to the viewport
        public Guid[] machineCurve(ObjRef obj, Guid curveID, Guid sweepID, 
                                   double pitch, double turns, double springD, 
                                   double width, double height)
        {
			// Axis Curve: the axis around which the path will be formed
            Curve axisCurve = obj.Curve();

			// Path Curve: the curve that forms the outline of the spring
            Curve pathCurve = null;

			// Assess whether the path curve object has been created
			// Depending on the answer, create or replace
			pathCurve = NurbsCurve.CreateSpiral(axisCurve,
                                                axisCurve.Domain.T0,
                                                axisCurve.Domain.T1, 
                                                new Point3d(0, 0, 0), 
                                                pitch, 
                                                turns, 
                                                springD / 2,
                                                springD / 2, 
                                                12);

			// Assess whether the path curve object has been created
			// Depending on the answer, create or replace
			if (curveID.CompareTo(Guid.Empty) == 0)
            {
                curveID = myDoc.Objects.AddCurve(pathCurve);
            }
            else
            {
                myDoc.Objects.Replace(curveID, pathCurve);
            }

            sweepID = sweepCurve(pathCurve, sweepID, width, height);
			myDoc.Views.Redraw(); // Call the viewport to refresh
			return new Guid[] { curveID, sweepID };
        }


        // Instantiate a polyline object and add to the viewport
        public Guid[] zCurve(ObjRef obj, Guid numCurve, Guid numSweep, 
                             double width, double height)
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

		// CROSS-SECTION --> VOLUME METHODS
		// _________________________________________________________________

		// Creates a 3D curve from a 1D path & circular cross section)
        public Guid pipeCurve(Curve pathCurve, Guid numPipe, double coilD)
        {			
            // Create the 3D geometry
			Brep[] pipe = Brep.CreatePipe(pathCurve, 
                                          coilD / 2,
                                          false, 
                                          PipeCapMode.Flat, 
                                          false, 
                                          0.1, 
                                          0.1);

			// Assess whether the area curve object has been created
			// Depending on the answer, create or replace
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

		// Creates a 3D geometry from a 1D path & rectangular cross section
        public Guid sweepCurve(Curve pathCurve, Guid sweepID, double width, double height)
        {
            // Access location and orientation characteristics of the path curve
            Point3d startPoint = pathCurve.PointAtStart;
            Vector3d startVector = pathCurve.TangentAtStart;

            // Acquire reference to the plane orthogonal to the start of the
            // path curve, and create a rectangle on that plane
            Plane norm = new Plane(startPoint, startVector);
            Rectangle3d rectangle = new Rectangle3d(norm, width, height);
            Curve rectCurve = rectangle.ToNurbsCurve();

            // Create the 3D geometry
            SweepOneRail sweepObject = new SweepOneRail();
            Brep[] sweep = sweepObject.PerformSweep(pathCurve, rectCurve);

			// Assess whether the area curve object has been created
			// Depending on the answer, create or replace
			if (sweepID.CompareTo(Guid.Empty) == 0)
            {
                sweepID = myDoc.Objects.AddBrep(sweep[0]);
            }
            else
            {
                myDoc.Objects.Replace(sweepID, sweep[0]);
            }

            return sweepID;
        }

    }

}