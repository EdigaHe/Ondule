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
        Guid[] helicalCurve(Rhino.DocObjects.ObjRef obj, Guid numCurve, Guid numPipe, double pitch, double turns, double springD, double coilD);
        Guid[] machineCurve(Rhino.DocObjects.ObjRef obj, Guid numCurve, Guid numPipe, double pitch, double turns, double springD, double width, double height);
        Guid[] zCurve(Rhino.DocObjects.ObjRef obj, Guid numCurve, Guid numSweep, double width, double height);
        Guid pipeCurve(Rhino.Geometry.Curve numCurve, Guid numPipe, double coilD);
        Guid sweepCurve(Rhino.Geometry.Curve numCurve, Guid numPipe, double width, double height);
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

        public Guid[] helicalCurve(Rhino.DocObjects.ObjRef obj, Guid numCurve, Guid numPipe, double pitch, double turns, double springD, double coilD)
        {
            return rhinoModel.helicalCurve(obj, numCurve, numPipe, pitch, turns, springD, coilD);
        }

        public Guid[] machineCurve(Rhino.DocObjects.ObjRef obj, Guid numCurve, Guid numPipe, double pitch, double turns, double springD, double width, double height)
        {
            return rhinoModel.machineCurve(obj, numCurve, numPipe, pitch, turns, springD, width, height);
        }

        public Guid[] zCurve(Rhino.DocObjects.ObjRef obj, Guid numCurve, Guid numSweep, double width, double height)
        {
            return rhinoModel.zCurve(obj, numCurve, numSweep, width, height);
        }

        public Guid pipeCurve(Rhino.Geometry.Curve crv, Guid numPipe, double coilD)
        {
            return rhinoModel.pipeCurve(crv, numPipe, coilD);
        }

        public Guid sweepCurve(Rhino.Geometry.Curve crv, Guid numPipe, double width, double height)
        {
            return rhinoModel.sweepCurve(crv, numPipe, width, height);
        }

    }
}
