using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Rhino.DocObjects;

namespace OndulePlugin
{
    public interface IControllerModelObserver
    {

    }


    public interface Controller
    {
        #region default functions
        void printSTL(Rhino.DocObjects.ObjRef obj, Rhino.Geometry.Point3d pt);
        void deformBrep(Rhino.DocObjects.ObjRef obj);
        void wireframe();
        #endregion

        void selection();
        void addLinearConstraint(ref OnduleUnit obj);   // new linear constraint
        void addTwistConstraint(ref OnduleUnit obj);    // new twisting constraint
        void addBendConstraint(ref OnduleUnit obj, Boolean dir);
        void addLinearTwistConstraint(ref OnduleUnit obj);
        void showClothSpring(List<Guid> IDs, Boolean ishown);

        #region old versions of deformation behaviors
        //void linearTwistDeform(ObjRef obj);
        //void linearBendDeform(ObjRef obj);
        //void twistBendDeform(ObjRef obj);
        //void bendDeform(ObjRef obj);
        //void linearDeform(ObjRef obj);
        // void twistDeform(ObjRef obj);
        //void allDeform(ObjRef obj);
        #endregion

        void medialAxisTransform();

        void springGeneration(ref OnduleUnit obj);

        OnduleUnit medialAxisGeneration();

        OnduleUnit getUnitFromGlobal(int index);
        void updateUnitFromGlobal(int index, OnduleUnit newUnit);
        void addUnitToGlobal(OnduleUnit newUnit);
        void removeUnitGlobal(int index);
        int getCountGlobal(); 

    }

    public class IncController : Controller
    {

        RhinoModel rhinoModel;
        View view;

        public List<OnduleUnit> globalUnits = new List<OnduleUnit>();
        /// <summary>
        /// get the element from the list
        /// </summary>
        /// <param name="index">the location of the element</param>
        /// <param name="globalUnits">the list that stores all elements</param>
        /// <returns></returns>
        public OnduleUnit getUnitFromGlobal(int index)
        {
            return globalUnits.ElementAt(index);
        }
        /// <summary>
        /// update the unit at index location
        /// </summary>
        /// <param name="index">the index of the element that needs to be updated</param>
        /// <param name="newUnit">the new unit</param>
        /// <param name="globalUnits">the list that stores all elements</param>
        public void updateUnitFromGlobal(int index, OnduleUnit newUnit)
        {
            globalUnits.ElementAt(index).CoilDiameter = newUnit.CoilDiameter;
            globalUnits.ElementAt(index).CoilNum = newUnit.CoilNum;
            globalUnits.ElementAt(index).WireDiameter = newUnit.WireDiameter;
            globalUnits.ElementAt(index).endPt = newUnit.endPt;
            globalUnits.ElementAt(index).G = newUnit.G;
            globalUnits.ElementAt(index).Length = newUnit.Length;
            globalUnits.ElementAt(index).MA = newUnit.MA;
            globalUnits.ElementAt(index).Pitch = newUnit.Pitch;
            globalUnits.ElementAt(index).startPt = newUnit.startPt;
            globalUnits.ElementAt(index).BREPID = newUnit.BREPID;
            globalUnits.ElementAt(index).ID = newUnit.ID;
        }
        public void addUnitToGlobal(OnduleUnit newUnit)
        {
            globalUnits.Add(newUnit);
        }
        public void removeUnitGlobal(int index)
        {
            globalUnits.RemoveAt(index);
        }
        public int getCountGlobal()
        {
            return globalUnits.Count();
        }

        public IncController(View v,  RhinoModel rm)
        {
            view = v;
            rhinoModel = rm;
            view.setController(this);
        }

        
        public void addLinearConstraint(ref OnduleUnit obj)
        {
            rhinoModel.addLinearConstraint(ref obj);
        }
        public void addTwistConstraint(ref OnduleUnit obj)
        {
            rhinoModel.addTwistConstraint(ref obj);
        }
        public void addLinearTwistConstraint(ref OnduleUnit obj)
        {
            rhinoModel.addLinearTwistConstraint(ref obj);
        }
        public void addBendConstraint(ref OnduleUnit obj, Boolean dir)
        {
            rhinoModel.addBendConstraint(ref obj, dir);
        }
        public void showClothSpring(List<Guid> IDs, Boolean isshown)
        {
            rhinoModel.showClothSpring(IDs, isshown);
        }
        #region old versions of deforamtion behaviors
        //public void linearDeform(ObjRef objRef)
        //{
        //    rhinoModel.linearDeform(objRef);
        //}

        //public void twistDeform(ObjRef objRef)
        //{
        //    rhinoModel.twistDeform(objRef);
        //}
        //public void bendDeform(ObjRef objRef)
        //{
        //    rhinoModel.bendDeform(objRef);
        //}
        //public void linearTwistDeform(ObjRef objRef)
        //{
        //    rhinoModel.linearTwistDeform(objRef);
        //}
        //public void linearBendDeform(ObjRef objRef)
        //{
        //    rhinoModel.linearBendDeform(objRef);
        //}
        //public void twistBendDeform(ObjRef objRef)
        //{
        //    rhinoModel.twistBendDeform(objRef);
        //}
        //public void allDeform(ObjRef objRef)
        //{
        //    rhinoModel.allDeform(objRef);
        //}
        #endregion

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

        public void springGeneration(ref OnduleUnit obj)
        {

            rhinoModel.springGen(ref obj);
        }

        public OnduleUnit medialAxisGeneration()
        {
            return rhinoModel.maGen();
        }
    }
}
