﻿using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OndulePlugin
{
    public class OnduleUnit
    {
        private int _ID;       // the ID of the Ondule unit
        private Guid _brepID;   // the GUID of the selected brep 
        private List<int> _coilNum = new List<int>();   // By default: length/(d_min+gap_min)
        private List<double> _coilDiameter = new List<double>();
        private double _pitch;   // By default: d_min+gap_min
        private List<double> _discontinueLengths = new List<double>();
        private double _wireDiameter = 1.6; // By default: d_min 
        private double _length;
        private double _G;  // Modulus of rigidity
        private Rhino.Geometry.Curve _ma;
        private Point3d _startPt;
        private Point3d _endPt;
        //private List<double> _stiffness;
        //private double _linearDeflection;
        //private double _bendAngle;
        //private double _twistAngle;
        List<Brep> _preservedBrepList = new List<Brep>();   // Record the preserved breps
        List<Brep> _replacedBrepList = new List<Brep>();    // Record the replaced breps
        List<Guid> _capperSpringIDList = new List<Guid>();  // Record the IDs of generated spring breps 

        public OnduleUnit()
        {

        }
        public OnduleUnit(int id, Guid brepid, List<int> cNum, List<double> cDia, double p, double wDia, double L, double G, Curve MA, Point3d strPt, Point3d endPt)
        {
            this._ID = id;
            this._brepID = brepid;
            this._coilNum = cNum;
            this._coilDiameter = cDia;
            this._pitch = p;
            this._wireDiameter = wDia;
            this._length = L;
            this._G = G;
            this._ma = MA;
            this._startPt = strPt;
            this._endPt = endPt;
        }
        public List<double> DiscontinuedLengths
        {
            get { return this._discontinueLengths; }
            set { this._discontinueLengths = value; }
        }
        public int ID
        {
            get { return this._ID; }
            set { this._ID = value; }
        }
        public Guid BREPID
        {
            get { return this._brepID; }
            set { this._brepID = value; }
        }

        public double G
        {
            get { return this._G; }
            set { this._G = value; }
        }

        #region Not Used Yet
        //public List<double> Stiffness
        //{
        //    get { return this._stiffness; }
        //    set { this._stiffness = value; }
        //}
        //public double LinearDeflection
        //{
        //    get { return this._linearDeflection; }
        //    set { this._linearDeflection = value; }
        //}
        //public double BendAngle
        //{
        //    get { return this._bendAngle; }
        //    set { this._bendAngle = value; }
        //}
        //public double TwistAngle
        //{
        //    get { return this._twistAngle; }
        //    set { this._twistAngle = value; }
        //}
        #endregion

        public double Length
        {
            get { return this._length; }
            set { this._length = value; }
        }
        public List<int> CoilNum
        {
            get { return this._coilNum; }
            set { this._coilNum = value; }
        }
        
        public List<double> CoilDiameter
        {
            get { return this._coilDiameter; }
            set { this._coilDiameter = value; }
        }
 
        public double Pitch
        {
            get { return this._pitch; }
            set { this._pitch = value; }
        }

        public double WireDiameter
        {
            get { return this._wireDiameter; }
            set { this._wireDiameter = value; }
        }

        public Rhino.Geometry.Curve MA
        {
            get { return this._ma; }
            set { this._ma = value; }
        }

        public List<Brep> PreservedBreps
        {
            get { return this._preservedBrepList; }
            set { this._preservedBrepList = value; }
        }
        public List<Brep> ReplacedBreps
        {
            get { return this._replacedBrepList; }
            set { this._replacedBrepList = value; }
        }
        public List<Guid> CappedSpringIDs
        {
            get { return this._capperSpringIDList; }
            set { this._capperSpringIDList = value; }
        }

        public Point3d startPt
        {
            get { return this._startPt; }
            set { this._startPt = value; }
        }

        public Point3d endPt
        {
            get { return this._endPt; }
            set { this._endPt = value; }
        }
    }
}
