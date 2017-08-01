using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Rhino.DocObjects;

namespace PluginBar
{
    public interface IControllerModelObserver
    {

    }

    public interface Controller
    {
        void printSTL(Rhino.DocObjects.ObjRef obj, Rhino.Geometry.Point3d pt);
        void deformBrep(Rhino.DocObjects.ObjRef obj);
        void wireframe();
        void selection();
        void linearDeform(ObjRef obj);
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

<<<<<<< HEAD
        public void linearDeform(ObjRef objRef)
=======
//<<<<<<< HEAD
//=======
        public void ConvertLineToSpring(Rhino.DocObjects.ObjRef obj)
>>>>>>> origin/master
        {
            rhinoModel.linearDeform(objRef);
        }

        public void selection()
        {
            rhinoModel.selection();
        }

        public void wireframe()
        {
            rhinoModel.wireframeAll();
        }
//>>>>>>> origin/master

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
