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
        void clearInnerStructure(ref OnduleUnit obj);

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

        void selectMASegment(ref OnduleUnit obj);

        OnduleUnit getUnitFromGlobal(int index);
        void updateUnitFromGlobal(int index, OnduleUnit newUnit);
        void addUnitToGlobal(OnduleUnit newUnit);
        void removeUnitGlobal(int index);
        int getCountGlobal();

        void showInternalStructure(OnduleUnit obj, int index);
        void hideInternalStructure(OnduleUnit obj, int index);

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
            globalUnits.ElementAt(index).ID = newUnit.ID;
            globalUnits.ElementAt(index).BREPID = newUnit.BREPID;
            globalUnits.ElementAt(index).CoilDiameter = newUnit.CoilDiameter;
            globalUnits.ElementAt(index).CoilNum = newUnit.CoilNum;
            globalUnits.ElementAt(index).Pitch = newUnit.Pitch;
            globalUnits.ElementAt(index).Length = newUnit.Length;
            globalUnits.ElementAt(index).WireDiameter = newUnit.WireDiameter;
            globalUnits.ElementAt(index).G = newUnit.G;
            globalUnits.ElementAt(index).startPt = newUnit.startPt;
            globalUnits.ElementAt(index).endPt = newUnit.endPt;
            globalUnits.ElementAt(index).MA = newUnit.MA;
            globalUnits.ElementAt(index).DiscontinuedLengths = newUnit.DiscontinuedLengths;
            globalUnits.ElementAt(index).SelectedSeg = newUnit.SelectedSeg;
            globalUnits.ElementAt(index).BendAngle = newUnit.BendAngle;
            globalUnits.ElementAt(index).TwistAngle = newUnit.TwistAngle;
            globalUnits.ElementAt(index).CompressionDis = newUnit.CompressionDis;
            globalUnits.ElementAt(index).ExtensionDis = newUnit.ExtensionDis;
            globalUnits.ElementAt(index).MAID = newUnit.MAID;
            globalUnits.ElementAt(index).CtrlPt1ID = newUnit.CtrlPt1ID;
            globalUnits.ElementAt(index).CtrlPt2ID = newUnit.CtrlPt2ID;
            globalUnits.ElementAt(index).SegID = newUnit.SegID;
            globalUnits.ElementAt(index).Stiffness = newUnit.Stiffness;
            globalUnits.ElementAt(index).PreservedBreps = newUnit.PreservedBreps;
            globalUnits.ElementAt(index).ReplacedBreps = newUnit.ReplacedBreps;
            globalUnits.ElementAt(index).CappedSpringIDs = newUnit.CappedSpringIDs;
            globalUnits.ElementAt(index).ClothIDs = newUnit.ClothIDs;
            globalUnits.ElementAt(index).InnerStructureIDs = newUnit.InnerStructureIDs;
            globalUnits.ElementAt(index).PreservedBrepIDs = newUnit.PreservedBrepIDs;
            globalUnits.ElementAt(index).MeanCoilDiameter = newUnit.MeanCoilDiameter;
            globalUnits.ElementAt(index).BendDirAngle = newUnit.BendDirAngle;
            globalUnits.ElementAt(index).ConstraintType = newUnit.ConstraintType;
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

        public void showInternalStructure(OnduleUnit obj, int index)
        {
            rhinoModel.showInternalStructure(obj, index);
        }
        public void hideInternalStructure(OnduleUnit obj, int index)
        {
            rhinoModel.hideInternalStructure(obj, index);
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
        public void clearInnerStructure(ref OnduleUnit obj)
        {
            rhinoModel.clearInnerStructure(ref obj);
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

        public void selectMASegment(ref OnduleUnit obj)
        {
            rhinoModel.selectMASegment(ref obj);
        }
    }
}
