using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OndulePlugin
{
    public class OnduleUnit
    {
        private int _constraintType = -1;
        private int _ID;       // the ID of the Ondule unit
        private Guid _brepID;   // the GUID of the selected brep 
        private double _coilNum;
        private List<double> _coilDiameter = new List<double>();
        private double _meanCoilDiameter;
        private double _pitch;   // By default: d_min+gap_min
        private List<double> _discontinueLengths = new List<double>();
        private double _wireDiameter = 1.6; // By default: d_min 
        private double _length;
        private double _G;  // Modulus of rigidity
        private Rhino.Geometry.Curve _ma;
        private Rhino.Geometry.Curve _selectedSeg;
        private Point3d _startPt;
        private Point3d _endPt;
        //private List<double> _stiffness;
        //private double _linearDeflection;
        private double _bendAngle;
        private double _twistAngle;
        private double _compressionDis;
        private double _extensionDis;
        private double _bendDirAngle;
        private Guid _MAID;
        private Guid _ctrlPt1ID;
        private Guid _ctrlPt2ID;
        private Guid _segID;
        private double _stiffness;
        private List<Brep> _preservedBrepList = new List<Brep>();   // Record the preserved breps
        private List<Brep> _replacedBrepList = new List<Brep>();    // Record the replaced breps
        private List<Guid> _capperSpringIDList = new List<Guid>();  // Record the IDs of generated spring breps 
        private List<Guid> _clothIDList = new List<Guid>();         // Record the IDs of the generated spring cloth
        private List<Guid> _innerStructureIDList = new List<Guid>();// Record the IDs of the inner constraint structure
        private List<Guid> _preservedBrepsIDList = new List<Guid>();// Record the IDs of the preserved breps
        private bool _isFreeformOnly;
        public OnduleUnit()
        {
            this._ID = -1;
            this._brepID = new Guid();
            this._pitch = -1;
            this._length = -1;
            this._G = -1;
            this._bendAngle = -1;
            this._twistAngle = -1;
            this._compressionDis = -1;
            this._extensionDis = -1;
            this._MAID = new Guid();
            this._ctrlPt1ID = new Guid();
            this._ctrlPt2ID = new Guid();
            this._segID = new Guid();
            this._coilNum = -1;
            this._isFreeformOnly = false;
        }
        public double Stiffness
        {
            get { return this._stiffness; }
            set { this._stiffness = value; }
        }
        public bool IsFreeformOnly
        {
            get { return this._isFreeformOnly; }
            set { this._isFreeformOnly = value; }
        }

        public int ConstraintType
        {
            get { return this._constraintType; }
            set { this._constraintType = value; }
        }
        public double MeanCoilDiameter
        {
            get { return this._meanCoilDiameter; }
            set { this._meanCoilDiameter = value; }
        }
        public double CompressionDis
        {
            get { return this._compressionDis; }
            set { this._compressionDis = value; }
        }
        public double ExtensionDis
        {
            get { return this._extensionDis; }
            set { this._extensionDis = value; }
        }
        public double TwistAngle
        {
            get { return this._twistAngle; }
            set { this._twistAngle = value; }
        }

        public Guid MAID
        {
            get { return this._MAID; }
            set { this._MAID = value; }
        }
        public Guid CtrlPt1ID
        {
            get { return this._ctrlPt1ID; }
            set { this._ctrlPt1ID = value; }
        }
        public Guid CtrlPt2ID
        {
            get { return this._ctrlPt2ID; }
            set { this._ctrlPt2ID = value; }
        }
        public Guid SegID
        {
            get { return this._segID; }
            set { this._segID = value; }
        }
        public double BendAngle
        {
            get { return this._bendAngle; }
            set { this._bendAngle = value; }
        }

        public OnduleUnit(int id, Guid brepid, int cNum, List<double> cDia, double p, double wDia, double L, double G, Curve MA, Point3d strPt, Point3d endPt)
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
        public double CoilNum
        {
            get { return this._coilNum; }
            set { this._coilNum = value; }
        }
        public List<Guid> InnerStructureIDs
        {
            get { return this._innerStructureIDList; }
            set { this._innerStructureIDList = value; }
        }
        public List<double> CoilDiameter
        {
            get { return this._coilDiameter; }
            set { this._coilDiameter = value; }
        }
        public List<Guid> PreservedBrepIDs
        {
            get { return this._preservedBrepsIDList; }
            set { this._preservedBrepsIDList = value; }
        }
        public Rhino.Geometry.Curve SelectedSeg
        {
            get { return this._selectedSeg; }
            set { this._selectedSeg = value; }
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
        public List<Guid> ClothIDs
        {
            get { return this._clothIDList; }
            set { this._clothIDList = value; }
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

        public double BendDirAngle
        {
            get { return this._bendDirAngle; }
            set { this._bendDirAngle = value; }
        }
    }
}
