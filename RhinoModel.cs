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
        void lineToSpring(ObjRef obj);
        void curveToSpring(ObjRef obj, string type, string mode);

        Guid helicalCurve(ObjRef obj, Guid num, double pitch, double turns, double springD);
        Guid zCurve(ObjRef obj, Guid num);
    }

    public class IncRhinoModel : RhinoModel
    {
        
        RhinoDoc myDoc = null;
        Guid num;

        public IncRhinoModel()
        {
            myDoc = PluginBarCommand.rhinoDoc;
            if (myDoc == null)
            {
                myDoc = RhinoDoc.ActiveDoc;
            }

            myDoc.Views.Redraw();
        }

        public void deformBrep(ObjRef obj)
        {
            Brep bbox = obj.Brep().GetBoundingBox(true).ToBrep();
            myDoc.Objects.AddBrep(bbox);
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
                //int lineCounter = 0;
                //string aLine;
                StringReader strReader = new StringReader(readConverted);
              
                result += String.Format("G1 B{0} C{1} F500 \n", 0, 0);

                if (result != string.Empty)
                {
                    File.WriteAllText("../ExportSTL/" +  ".ngc", result);
                }

            }

        }

        public void lineToSpring(ObjRef obj)
        {
            Curve crv = obj.Curve();
            Curve spiralCrv = NurbsCurve.CreateSpiral(crv, crv.Domain.T0, crv.Domain.T1, new Point3d(0, 0, 0), 2, 10, 5, 5, 12);

            myDoc.Objects.AddCurve(spiralCrv); 
            myDoc.Views.Redraw();
        }

        // IN USE
        public void curveToSpring(ObjRef obj, string type, string mode)
        {
            Curve crv = obj.Curve();

            Curve spiralCrv = null;
            Polyline zCrv = null;
            Curve zzz = null;

            if (type == "Helical" || type == "Machined")
            {
                spiralCrv = NurbsCurve.CreateSpiral(crv, crv.Domain.T0, crv.Domain.T1, new Point3d(0, 0, 0), 2, 10, 5, 5, 12);
                num = myDoc.Objects.AddCurve(spiralCrv);
            }
            else if (type == "Z")
            {
                zCrv = new Polyline();
                zCrv.Add(0, 0, 0);

                for (int i = 0; i < 50; i += 5)
                {
                    if (i % 2 == 0)
                    {
                        zCrv.Add(0, i, 0);
                    }
                    else
                    {
                        zCrv.Add(5, i, 0);
                    }

                }

                zzz = zCrv.ToNurbsCurve();
                myDoc.Objects.AddCurve(zzz);
            }

            myDoc.Views.Redraw();
        }


        public Guid helicalCurve(ObjRef obj, Guid num, double pitch, double turns, double springD)
        {
            Curve crv = obj.Curve();
            Curve spiralCrv = null;
            
            spiralCrv = NurbsCurve.CreateSpiral(crv, crv.Domain.T0, crv.Domain.T1, new Point3d(0, 0, 0), pitch, turns, springD / 2, springD / 2, 12);

            if (num.CompareTo(Guid.Empty) == 0)
            {
                num = myDoc.Objects.AddCurve(spiralCrv);
            }
            else
            {
                myDoc.Objects.Replace(num, spiralCrv);
            }

            myDoc.Views.Redraw();
            return num;
        }


        public Guid zCurve(ObjRef obj, Guid num)
        {
            Polyline zCrv = new Polyline();
            zCrv.Add(0, 0, 0);

            for (int i = 0; i < 50; i += 5)
            {
                if (i % 2 == 0)
                {
                    zCrv.Add(0, i, 0);
                }
                else
                {
                    zCrv.Add(5, i, 0);
                }

            }

            Curve zzz = zCrv.ToNurbsCurve();

            if (num.CompareTo(Guid.Empty) == 0)
            {
                num = myDoc.Objects.AddCurve(zzz);
            }
            else
            {
                myDoc.Objects.Replace(num, zzz);
            }

            myDoc.Views.Redraw();
            return num;
        }

    }

}