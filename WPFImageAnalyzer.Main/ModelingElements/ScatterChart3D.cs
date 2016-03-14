using System.Collections;
using System.Windows.Media.Media3D;
using System.Windows.Controls;

namespace WPFChart3D
{
    class ScatterChart3D: Chart3D
    {
        public ScatterPlotItem Get(int n)
        {
            return (ScatterPlotItem)MVertices[n];
        }
        
        public void SetVertex(int n, ScatterPlotItem value)
        {
            MVertices[n] = value;
        }

        // convert the 3D scatter plot into a array of Mesh3D object
        public ArrayList GetMeshes()
        {
            var nDotNo = GetDataNo();
            if (nDotNo == 0) return null;
            var meshs = new ArrayList();

            var nVertIndex = 0;
            for (var i = 0; i < nDotNo; i++)
            {
                var plotItem = Get(i);
                if(plotItem == null)
                    continue;
                var nType = plotItem.shape % ShapeNo;
                var w = plotItem.w;
                var h = plotItem.h;
                Mesh3D dot;
                MVertices[i].nMinI = nVertIndex;
                switch (nType)
                {
                    case (int)Shape.Bar:
                        dot = new Bar3D(0, 0, 0, w, w, h);
                        break;
                    case (int)Shape.Cone:
                        dot = new Cone3D(w, w, h, 7);
                        break;
                    case (int)Shape.Cylinder:
                        dot = new Cylinder3D(w, w, h, 7);
                        break;
                    case (int)Shape.Ellipse:
                        dot = new Ellipse3D(w, w, h, 7);
                        break;
                    case (int)Shape.Pyramid:
                        dot = new Pyramid3D(w, w, h);
                        break;
                    default:
                        dot = new Bar3D(0, 0, 0, w, w, h);
                        break;
                }
                nVertIndex += dot.GetVertexNo();
                MVertices[i].nMaxI = nVertIndex - 1;

                TransformMatrix.Transform(dot, new Point3D(plotItem.x, plotItem.y, plotItem.z), 0, 0);
                dot.SetColor(plotItem.color);
                meshs.Add(dot);
            }
            AddAxesMeshes(meshs);

            return meshs;
        }

        // selection
        public override void Select(ViewportRect rect, TransformMatrix matrix, Viewport3D viewport3D)
        {
            var nDotNo = GetDataNo();
            if (nDotNo == 0) return;

            var xMin = rect.XMin();
            var xMax = rect.XMax();
            var yMin = rect.YMin();
            var yMax = rect.YMax();

            for (var i = 0; i < nDotNo; i++)
            {
                var plotItem = Get(i);
                if(plotItem == null)
                    continue;
                var pt = matrix.VertexToViewportPt(new Point3D(plotItem.x, plotItem.y, plotItem.z),
                    viewport3D);

                if ((pt.X > xMin) && (pt.X < xMax) && (pt.Y > yMin) && (pt.Y < yMax))
                {
                    MVertices[i].selected = true;
                }
                else
                {
                    MVertices[i].selected = false;
                }
            }
       }

        // highlight the selection
        public override void HighlightSelection(MeshGeometry3D meshGeometry, System.Windows.Media.Color selectColor)
        {
            var nDotNo = GetDataNo();
            if (nDotNo == 0) return;

            for (var i = 0; i < nDotNo; i++)
            {
                var mapPt = TextureMapping.GetMappingPosition(MVertices[i].selected ? selectColor 
                    : MVertices[i].color, false);
                var nMin = MVertices[i].nMinI;
                var nMax = MVertices[i].nMaxI;
                for(var j=nMin; j<=nMax; j++)
                {
                    meshGeometry.TextureCoordinates[j] = mapPt;
                }
            }
        }
    }
}