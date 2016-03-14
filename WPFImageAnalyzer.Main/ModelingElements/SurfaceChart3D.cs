//------------------------------------------------------------------
// (c) Copywrite Jianzhong Zhang
// This code is under The Code Project Open License
// Please read the attached license document before using this class
//------------------------------------------------------------------

// class of general surface chart, not ready yet
// a few function will be used in child class
// version 0.1


using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Media3D;

namespace WPFChart3D
{
    class SurfaceChart3D: Chart3D
    {
        // selection
        public override void Select(ViewportRect rect, TransformMatrix matrix, Viewport3D viewport3D)
        {
            int nDotNo = GetDataNo();
            if (nDotNo == 0) return;

            double xMin = rect.XMin();
            double xMax = rect.XMax();
            double yMin = rect.YMin();
            double yMax = rect.YMax();

            for (int i = 0; i < nDotNo; i++)
            {
                Point pt = matrix.VertexToViewportPt(new Point3D(MVertices[i].x, MVertices[i].y, MVertices[i].z),
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
        public override void HighlightSelection(System.Windows.Media.Media3D.MeshGeometry3D meshGeometry, System.Windows.Media.Color selectColor)
        {
            int nDotNo = GetDataNo();
            if (nDotNo == 0) return;

            Point mapPt;
            for (int i = 0; i < nDotNo; i++)
            {
                if (MVertices[i].selected)
                {
                    mapPt = TextureMapping.GetMappingPosition(selectColor, true);
                }
                else
                {
                    mapPt = TextureMapping.GetMappingPosition(MVertices[i].color, true);
                }
                int nMin = MVertices[i].nMinI;
                meshGeometry.TextureCoordinates[nMin] = mapPt;
            }
        }
    }
}
