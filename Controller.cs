using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace PluginBar
{
    public interface IControllerModelObserver
    {

    }

    public interface Controller
    {
        void printSTL(Rhino.DocObjects.ObjRef obj, Rhino.Geometry.Point3d pt);
        void deformBrep(Rhino.DocObjects.ObjRef obj);
        void ConvertLineToSpring(Rhino.DocObjects.ObjRef obj);
        void curveToSpring(Rhino.DocObjects.ObjRef obj, string type, string mode);

        Guid helicalCurve(Rhino.DocObjects.ObjRef obj, Guid num, double pitch, double turns, double springD);
        Guid zCurve(Rhino.DocObjects.ObjRef obj, Guid num);
    }

    public class IncController : Controller
    {

        RhinoModel rhinoModel;
        View view;


        public IncController(View v,  RhinoModel rm)
        {
            view = v;
            rhinoModel = rm;
            view.setController(this);
        }

        public void ConvertLineToSpring(Rhino.DocObjects.ObjRef obj)
        {
            rhinoModel.lineToSpring(obj);
        }


        // IN USE
        public void curveToSpring(Rhino.DocObjects.ObjRef obj, string type, string mode)
        {
            rhinoModel.curveToSpring(obj, type, mode);
        }

        public Guid helicalCurve(Rhino.DocObjects.ObjRef obj, Guid num, double pitch, double turns, double springD)
        {
            return rhinoModel.helicalCurve(obj, num, pitch, turns, springD);
        }

        public Guid zCurve(Rhino.DocObjects.ObjRef obj, Guid num)
        {
            return rhinoModel.zCurve(obj, num);
        }

        public void deformBrep(Rhino.DocObjects.ObjRef obj)
        {
            rhinoModel.deformBrep(obj);
        }

        public void printSTL(Rhino.DocObjects.ObjRef obj, Rhino.Geometry.Point3d pt)
        {
            rhinoModel.printSTL(obj, pt);
        }
    }
}
