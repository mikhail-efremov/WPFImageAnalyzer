//------------------------------------------------------------------
// (c) Copywrite Jianzhong Zhang
// This code is under The Code Project Open License
// Please read the attached license document before using this class
//------------------------------------------------------------------

// class of a special surface chart, (uniform grid in x-y direction)
// version 0.1


using System.Collections;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Collections.Generic;

namespace WPFChart3D
{
    class UniformSurfaceChart3D: SurfaceChart3D
    {
        private int _mNGridXNo, _mNGridYNo;               // the grid number on each axis

        public void SetPoint(int i, int j, float x, float y, float z)
        {
            var nI = j * _mNGridXNo + i;
            MVertices[nI].x = x;
            MVertices[nI].y = y;
            MVertices[nI].z = z;
        }

        public void SetZ(int i, int j, float z)
        {
            MVertices[j*_mNGridXNo + i].z = z;
        }

        public void SetColor(int i, int j, Color color)
        {
            var nI = j * _mNGridXNo + i;
            MVertices[nI].color = color;
       }

        public void SetGrid(int xNo, int yNo, float xMin, float xMax, float yMin, float yMax)
        {
            SetDataNo(xNo * yNo);
            _mNGridXNo = xNo;
            _mNGridYNo = yNo;
            MxMin = xMin;
            MxMax = xMax;
            MyMin = yMin;
            MyMax = yMax;
            var dx = (MxMax - MxMin) / ((float)xNo - 1);
            var dy = (MyMax - MyMin) / ((float)yNo - 1);
            for (int i = 0; i < xNo; i++)
            {
                for (int j = 0; j < yNo; j++)
                {
                    var xV = MxMin + dx * i;
                    var yV = MyMin + dy * j;
                    MVertices[j * xNo + i] = new Vertex3D();
                    SetPoint(i, j, xV, yV, 0);
                }
            }
        }

        public void SetGridRgb(List<Rgb> rgbList, int xNo, int yNo, float xMin, float xMax, float yMin, float yMax)
        {
            SetDataNo(xNo * yNo);
            _mNGridXNo = xNo;
            _mNGridYNo = yNo;
            MxMin = xMin;
            MxMax = xMax;
            MyMin = yMin;
            MyMax = yMax;
            var dx = (MxMax - MxMin) / ((float)xNo - 1);
            var dy = (MyMax - MyMin) / ((float)yNo - 1);
            for (var i = 0; i < xNo; i++)
            {
                for (var j = 0; j < yNo; j++)
                {
                    var xV = MxMin + dx * i;
                    var yV = MyMin + dy * j;
                    MVertices[j * xNo + i] = new Vertex3D();
                    SetPoint(i, j, xV, yV, 0);
                }
            }
        }

        // convert the uniform surface chart to a array of Mesh3D (only one element)
        public ArrayList GetMeshes()
        {
            var meshes = new ArrayList();
            var surfaceMesh = new ColorMesh3D();

            surfaceMesh.SetSize(_mNGridXNo * _mNGridYNo, 2 * (_mNGridXNo - 1) * (_mNGridYNo - 1));

            for (var i = 0; i < _mNGridXNo; i++)
            {
                for (var j = 0; j < _mNGridYNo; j++)
                {
                    var nI = j * _mNGridXNo + i;
                    var vert = MVertices[nI];
                    MVertices[nI].nMinI = nI;
                    surfaceMesh.SetPoint(nI, new Point3D(vert.x, vert.y, vert.z));
                    surfaceMesh.SetColor(nI, vert.color);
                }
            }
            // set triangle
            var nT = 0;
            for (var i = 0; i < _mNGridXNo-1; i++)
            {
                for (var j = 0; j < _mNGridYNo-1; j++)
                {
                    var n00 = j * _mNGridXNo + i;
                    var n10 = j * _mNGridXNo + i + 1;
                    var n01 = (j + 1) * _mNGridXNo + i;
                    var n11 = (j + 1) * _mNGridXNo + i + 1;

                    surfaceMesh.SetTriangle(nT, n00, n10, n01);
                    nT++;
                    surfaceMesh.SetTriangle(nT, n01, n10, n11);
                    nT++;
                }
            }
            meshes.Add(surfaceMesh);

            return meshes;
        }
    }
}
