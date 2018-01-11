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
        void twistDeform(ObjRef obj);
        void bendDeform(ObjRef obj);
        void linearTwistDeform(ObjRef obj);
        void linearBendDeform(ObjRef obj);
        void twistBendDeform(ObjRef obj);
        void allDeform(ObjRef obj);
        void medialAxisTransform();

        void springGeneration(ObjRef obj);

        void medialAxisGeneration();

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

        public void linearDeform(ObjRef objRef)
        {
            rhinoModel.linearDeform(objRef);
        }

        public void twistDeform(ObjRef objRef)
        {
            rhinoModel.twistDeform(objRef);
        }
        public void bendDeform(ObjRef objRef)
        {
            rhinoModel.bendDeform(objRef);
        }
        public void linearTwistDeform(ObjRef objRef)
        {
            rhinoModel.linearTwistDeform(objRef);
        }
        public void linearBendDeform(ObjRef objRef)
        {
            rhinoModel.linearBendDeform(objRef);
        }
        public void twistBendDeform(ObjRef objRef)
        {
            rhinoModel.twistBendDeform(objRef);
        }
        public void allDeform(ObjRef objRef)
        {
            rhinoModel.allDeform(objRef);
        }
        public void selection()
        {
            rhinoModel.selection();
        }

        public void wireframe()
        {
            rhinoModel.wireframeAll();
        }

        public void deformBrep(Rhino.DocObjects.ObjRef obj)
        {
            rhinoModel.deformBrep(obj);
        }
        public void printSTL(Rhino.DocObjects.ObjRef obj, Rhino.Geometry.Point3d pt)
        {
            rhinoModel.printSTL(obj, pt);
        }
        public void medialAxisTransform()
        {
            rhinoModel.medialAxisTransform();
        }

        public void springGeneration(ObjRef obj)
        {
            rhinoModel.springGen(obj);
        }

        public void medialAxisGeneration()
        {
            rhinoModel.maGen();
        }
    }
}
