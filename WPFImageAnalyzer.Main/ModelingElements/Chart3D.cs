using System.Collections;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using static System.Single;

namespace WPFChart3D
{
    class Chart3D
    {
        public static int ShapeNo = 5;

        public Vertex3D this[int n]
        {
            get
            {
                return MVertices[n];
            }
            set
            {
                MVertices[n] = value;
            }
        }

        public float XCenter()
        {
            return (MxMin + MxMax) / 2;
        }

        public float YCenter()
        {
            return (MyMin + MyMax) / 2;
        }

        public float XRange()
        {
            return MxMax - MxMin;
        }
        public float YRange()
        {
            return MyMax - MyMin;
        }
        public float ZRange()
        {
            return MzMax - MzMin;
        }
        public float XMin()
        {
            return MxMin;
        }

        public float XMax()
        {
            return MxMax;
        }
        public float YMin()
        {
            return MyMin;
        }

        public float YMax()
        {
            return MyMax;
        }
        public float ZMin()
        {
            return MzMin;
        }

        public float ZMax()
        {
            return MzMax;
        }

        public int GetDataNo()
        {
            return MVertices.Length;
        }

        public void SetDataNo(int nSize)
        {
            MVertices = new Vertex3D[nSize];
        }

        public void GetDataRange()
        {
            var nDataNo = GetDataNo();
            if (nDataNo == 0) return;
            MxMin = MaxValue;
            MyMin = MaxValue;
            MzMin = MaxValue;
            MxMax = MinValue;
            MyMax = MinValue;
            MzMax = MinValue;
            for (var i = 0; i < nDataNo; i++)
            {
                if(this[i] == null)
                    continue;
                var xV = this[i].x;
                var yV = this[i].y;
                var zV = this[i].z;
                if (MxMin > xV) MxMin = xV;
                if (MyMin > yV) MyMin = yV;
                if (MzMin > zV) MzMin = zV;
                if (MxMax < xV) MxMax = xV;
                if (MyMax < yV) MyMax = yV;
                if (MzMax < zV) MzMax = zV;
            }
        }

        public void SetAxes(float x0, float y0, float z0, float xL, float yL, float zL)
        {
            _mXAxisLength = xL;
            _mYAxisLength = yL;
            _mZAxisLength = zL;
            _mXAxisCenter = x0;
            _mYAxisCenter = y0;
            _mZAxisCenter = z0;
            _mBUseAxes = true;
        }

        public void SetAxes()
        {
            SetAxes(0.05f);
        }

        public void SetAxes(float margin)
        {
            var xRange = MxMax - MxMin;
            var yRange = MyMax - MyMin;
            var zRange = MzMax - MzMin;

            var xC = MxMin - margin * xRange;
            var yC = MyMin - margin * yRange;
            var zC = MzMin - margin * zRange;
            var xL = (1 + 2 * margin) * xRange;
            var yL = (1 + 2 * margin) * yRange;
            var zL = (1 + 2 * margin) * zRange;

            SetAxes(xC, yC, zC, xL, yL, zL);
        }

        // add the axes mesh to the Mesh3D array
        // if you are using the projection matrix which is not uniform along all the axess, you need change this function
        public void AddAxesMeshes(ArrayList meshs)
        {
            if (!_mBUseAxes) return;

            var radius = (_mXAxisLength+_mYAxisLength+_mZAxisLength) / (3*m_axisLengthWidthRatio);

            Mesh3D xAxisCylinder = new Cylinder3D(radius, radius, _mXAxisLength, 6);
            xAxisCylinder.SetColor(MAxisColor);
            TransformMatrix.Transform(xAxisCylinder, new Point3D( _mXAxisCenter + _mXAxisLength / 2, _mYAxisCenter, _mZAxisCenter), 0, 90);
            meshs.Add(xAxisCylinder);

            Mesh3D xAxisCone = new Cone3D(2 * radius, 2 * radius, radius * 5, 6);
            xAxisCone.SetColor(MAxisColor);
            TransformMatrix.Transform(xAxisCone, new Point3D(_mXAxisCenter + _mXAxisLength, _mYAxisCenter, _mZAxisCenter), 0, 90);
            meshs.Add(xAxisCone);
         
            Mesh3D yAxisCylinder = new Cylinder3D(radius, radius, _mYAxisLength, 6);
            yAxisCylinder.SetColor(MAxisColor);
            TransformMatrix.Transform(yAxisCylinder, new Point3D(_mXAxisCenter , _mYAxisCenter+ _mYAxisLength / 2, _mZAxisCenter), 90, 90);
            meshs.Add(yAxisCylinder);
            
            Mesh3D yAxisCone = new Cone3D(2 * radius, 2 * radius, radius * 5, 6);
            yAxisCone.SetColor(MAxisColor);
            TransformMatrix.Transform(yAxisCone, new Point3D(_mXAxisCenter, _mYAxisCenter + _mYAxisLength, _mZAxisCenter), 90, 90);
            meshs.Add(yAxisCone);
   
            Mesh3D zAxisCylinder = new Cylinder3D(radius, radius, _mZAxisLength, 6);
            zAxisCylinder.SetColor(MAxisColor);
            TransformMatrix.Transform(zAxisCylinder, new Point3D(_mXAxisCenter , _mYAxisCenter, _mZAxisCenter + _mZAxisLength / 2), 0, 0);
            meshs.Add(zAxisCylinder);
                         
            Mesh3D zAxisCone = new Cone3D(2 * radius, 2 * radius, radius * 5, 6);
            zAxisCone.SetColor(MAxisColor);
            TransformMatrix.Transform(zAxisCone, new Point3D(_mXAxisCenter, _mYAxisCenter, _mZAxisCenter + _mZAxisLength), 0, 0);
            meshs.Add(zAxisCone);
        }

        // select 
        public virtual void Select(ViewportRect rect, TransformMatrix matrix, Viewport3D viewport3D)
        {
        }

        // highlight selected model
        public virtual void HighlightSelection(MeshGeometry3D meshGeometry, Color selectColor)
        {
        }

        public enum Shape { Bar, Ellipse, Cylinder, Cone, Pyramid };    // shape of the 3d dot in the plot

        protected Vertex3D [] MVertices;                               // 3d plot data
        protected float MxMin, MxMax, MyMin, MyMax, MzMin, MzMax; // data range

        private readonly float m_axisLengthWidthRatio = 200;                     // axis length / width ratio
        private float _mXAxisLength, _mYAxisLength, _mZAxisLength;      // axis length
        private float _mXAxisCenter, _mYAxisCenter, _mZAxisCenter;      // axis start point
        private bool _mBUseAxes = false;                                // use axis
        public Color MAxisColor = Color.FromRgb(0, 0, 196);            // axis color

    }
}
