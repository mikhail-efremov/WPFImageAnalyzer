//------------------------------------------------------------------
// (c) Copywrite Jianzhong Zhang
// This code is under The Code Project Open License
// Please read the attached license document before using this class
//------------------------------------------------------------------

// window class for testing 3d chart using WPF
// version 0.1

using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Collections;
using Microsoft.Win32;
using System.Windows.Controls;
using SysDrawing = System.Drawing;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WPFChart3D
{
    public partial class MainWindow : Window
    {
        // transform class object for rotate the 3d model
        public TransformMatrix m_transformMatrix = new TransformMatrix();

        // ***************************** 3d chart ***************************
        private Chart3D m_3dChart;       // data for 3d chart
        public int m_nChartModelIndex = -1;         // model index in the Viewport3d
        public int m_nSurfaceChartGridNo = 100;     // surface chart grid no. in each axis
        public int m_nScatterPlotDataNo = 5000;     // total data number of the scatter plot

        // ***************************** selection rect ***************************
        ViewportRect m_selectRect = new ViewportRect();
        public int m_nRectModelIndex = -1;

        private string filePath = String.Empty;

        public MainWindow()
        {
            InitializeComponent();

            // selection rect
            m_selectRect.SetRect(new Point(-0.5, -0.5), new Point(-0.5, -0.5));
            Model3D model3d = new Model3D();
            ArrayList meshs = m_selectRect.GetMeshes();
            m_nRectModelIndex = model3d.UpdateModel(meshs, null, m_nRectModelIndex, this.mainViewport);

            // display the 3d chart data no.
            gridNo.Text = String.Format("{0:d}", m_nSurfaceChartGridNo);
//            dataNo.Text = String.Format("{0:d}", m_nScatterPlotDataNo);

            // display surface chart
            TestScatterPlot(1000);
            TransformChart();
        }

        // function for testing surface chart
        public void TestSurfacePlot(int nGridNo)
        {
            int nXNo = nGridNo;
            int nYNo = nGridNo;
            // 1. set the surface grid
            m_3dChart = new UniformSurfaceChart3D();
            ((UniformSurfaceChart3D)m_3dChart).SetGrid(nXNo, nYNo, -100, 100, -100, 100);

            // 2. set surface chart z value
            double xC = m_3dChart.XCenter();
            double yC = m_3dChart.YCenter();
            int nVertNo = m_3dChart.GetDataNo();
            double zV;
            for (int i = 0; i < nVertNo; i++)
            {
                Vertex3D vert = m_3dChart[i];

                double r = 0.15 * Math.Sqrt((vert.x - xC) * (vert.x - xC) + (vert.y - yC) * (vert.y - yC));
                if (r < 1e-10) zV = 1;
                else zV = Math.Sin(r) / r;

                m_3dChart[i].z = (float)zV;
            }
            m_3dChart.GetDataRange();

            // 3. set the surface chart color according to z vaule
            double zMin = m_3dChart.ZMin();
            double zMax = m_3dChart.ZMax();
            for (int i = 0; i < nVertNo; i++)
            {
                Vertex3D vert = m_3dChart[i];
                double h = (vert.z - zMin) / (zMax - zMin);

                Color color = TextureMapping.PseudoColor(h);
                m_3dChart[i].color = color;
            }

            // 4. Get the Mesh3D array from surface chart
            ArrayList meshs = ((UniformSurfaceChart3D)m_3dChart).GetMeshes();

            // 5. display vertex no and triangle no of this surface chart
            UpdateModelSizeInfo(meshs);
            
            // 6. Set the model display of surface chart
            Model3D model3d = new Model3D();
            Material backMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Gray));
            m_nChartModelIndex = model3d.UpdateModel(meshs, backMaterial, m_nChartModelIndex, this.mainViewport);

            // 7. set projection matrix, so the data is in the display region
            float xMin = m_3dChart.XMin();
            float xMax = m_3dChart.XMax();
            m_transformMatrix.CalculateProjectionMatrix(xMin, xMax, xMin, xMax, zMin, zMax, 0.5);
            TransformChart();         
        }

        // function for testing 3d scatter plot
        public void TestScatterPlot(int nDotNo)
        {
            // 1. set scatter chart data no.
            m_3dChart = new ScatterChart3D();
            m_3dChart.SetDataNo(nDotNo);

            // 2. set property of each dot (size, position, shape, color)
            Random randomObject = new Random();
            int nDataRange = 200;
            for (int i = 0; i < nDotNo; i++)
            {
                ScatterPlotItem plotItem = new ScatterPlotItem();

                plotItem.w = 4;
                plotItem.h = 6;

                plotItem.x = (float)randomObject.Next(nDataRange);
                plotItem.y = (float)randomObject.Next(nDataRange);
                plotItem.z = (float)randomObject.Next(nDataRange);

                plotItem.shape = randomObject.Next(4);

                Byte nR = (Byte)randomObject.Next(256);
                Byte nG = (Byte)randomObject.Next(256);
                Byte nB = (Byte)randomObject.Next(256);

                plotItem.color = Color.FromRgb(nR, nG, nB);
                ((ScatterChart3D)m_3dChart).SetVertex(i, plotItem);
            }

            // 3. set the axes
            m_3dChart.GetDataRange();
            m_3dChart.SetAxes();

            // 4. get Mesh3D array from the scatter plot
            ArrayList meshs = ((ScatterChart3D)m_3dChart).GetMeshes();

            // 5. display model vertex no and triangle no
            UpdateModelSizeInfo(meshs);

            // 6. display scatter plot in Viewport3D
            Model3D model3d = new Model3D();
            m_nChartModelIndex = model3d.UpdateModel(meshs, null, m_nChartModelIndex, this.mainViewport);
 
            // 7. set projection matrix
            float viewRange = (float)nDataRange;
            m_transformMatrix.CalculateProjectionMatrix(0, viewRange, 0, viewRange, 0, viewRange, 0.5);
            TransformChart();
        }

        // function for set a scatter plot, every dot is just a simple pyramid.
        public void TestSimpleScatterPlot(int nDotNo)
        {
            // 1. set the scatter plot size
            m_3dChart = new ScatterChart3D();
            m_3dChart.SetDataNo(nDotNo);

            // 2. set the properties of each dot
            Random randomObject = new Random();
            int nDataRange = 200;
            for (int i = 0; i < nDotNo; i++)
            {
                ScatterPlotItem plotItem = new ScatterPlotItem();

                plotItem.w = 2;
                plotItem.h = 2;

                plotItem.x = (float)randomObject.Next(nDataRange);
                plotItem.y = (float)randomObject.Next(nDataRange);
                plotItem.z = (float)randomObject.Next(nDataRange);

                plotItem.shape = (int)Chart3D.SHAPE.PYRAMID;

                Byte nR = (Byte)randomObject.Next(256);
                Byte nG = (Byte)randomObject.Next(256);
                Byte nB = (Byte)randomObject.Next(256);

                plotItem.color = Color.FromRgb(nR, nG, nB);
                ((ScatterChart3D)m_3dChart).SetVertex(i, plotItem);
            }
            // 3. set axes
            m_3dChart.GetDataRange();
            m_3dChart.SetAxes();

            // 4. Get Mesh3D array from scatter plot
            ArrayList meshs = ((ScatterChart3D)m_3dChart).GetMeshes();

            // 5. display vertex no and triangle no.
            UpdateModelSizeInfo(meshs);

            // 6. show 3D scatter plot in Viewport3d
            Model3D model3d = new Model3D();
            m_nChartModelIndex = model3d.UpdateModel(meshs, null, m_nChartModelIndex, this.mainViewport);

            // 7. set projection matrix
            float viewRange = (float)nDataRange;
            m_transformMatrix.CalculateProjectionMatrix(0, viewRange, 0, viewRange, 0, viewRange, 0.5);
            TransformChart();
        }

        public void OnViewportMouseDown(object sender, MouseButtonEventArgs args)
        {
            Point pt = args.GetPosition(mainViewport);
            if (args.ChangedButton == MouseButton.Left)         // rotate or drag 3d model
            {
                m_transformMatrix.OnLBtnDown(pt);
            }
            else if (args.ChangedButton == MouseButton.Right)   // select rect
            {
                m_selectRect.OnMouseDown(pt, mainViewport, m_nRectModelIndex);
            }
        }

        public void OnViewportMouseMove(object sender, System.Windows.Input.MouseEventArgs args)
        {
            Point pt = args.GetPosition(mainViewport);

            if (args.LeftButton == MouseButtonState.Pressed)                // rotate or drag 3d model
            {
                m_transformMatrix.OnMouseMove(pt, mainViewport);

                TransformChart();
            }
            else if (args.RightButton == MouseButtonState.Pressed)          // select rect
            {
                m_selectRect.OnMouseMove(pt, mainViewport, m_nRectModelIndex);
            }
            else
            {
                /*
                String s1;
                Point pt2 = m_transformMatrix.VertexToScreenPt(new Point3D(0.5, 0.5, 0.3), mainViewport);
                s1 = string.Format("Screen:({0:d},{1:d}), Predicated: ({2:d}, H:{3:d})", 
                    (int)pt.X, (int)pt.Y, (int)pt2.X, (int)pt2.Y);
                this.statusPane.Text = s1;
                */
            }
        }

        public void OnViewportMouseUp(object sender, MouseButtonEventArgs args)
        {
            Point pt = args.GetPosition(mainViewport);
            if (args.ChangedButton == MouseButton.Left)
            {
                m_transformMatrix.OnLBtnUp();
            }
            else if (args.ChangedButton == MouseButton.Right)
            {
                if (m_nChartModelIndex == -1) return;
                // 1. get the mesh structure related to the selection rect
                MeshGeometry3D meshGeometry = Model3D.GetGeometry(mainViewport, m_nChartModelIndex);
                if (meshGeometry == null) return;
              
                // 2. set selection in 3d chart
                m_3dChart.Select(m_selectRect, m_transformMatrix, mainViewport);

                // 3. update selection display
                m_3dChart.HighlightSelection(meshGeometry, Color.FromRgb(200, 200, 200));
            }
        }

        // zoom in 3d display
        public void OnKeyDown(object sender, KeyEventArgs args)
        {
            m_transformMatrix.OnKeyDown(args);
            TransformChart();
        }

        private void surfaceButton_Click(object sender, RoutedEventArgs e)
        {
            int nGridNo = Int32.Parse(gridNo.Text);
            if (nGridNo < 2) return;
            if (nGridNo > 500)
            {
                MessageBox.Show("too many data");
                return;
            }
            TestSurfacePlot(nGridNo);
        }

        private void scatterButton_Click(object sender, RoutedEventArgs e)
        {
            int nDataNo = 10000;
            if (nDataNo < 3) return;

            if ((bool)checkBoxShape.IsChecked)
            {
                if (nDataNo > 10000)
                {
                    MessageBox.Show("too many data");
                    return;
                }
                TestScatterPlot(nDataNo);
            }
            else
            {
                if (nDataNo > 100000)
                {
                    MessageBox.Show("too many data");
                    return;
                }
                TestSimpleScatterPlot(nDataNo);
            }
        }

        private void UpdateModelSizeInfo(ArrayList meshs)
        {
            int nMeshNo = meshs.Count;
            int nChartVertNo = 0;
            int nChartTriangelNo = 0;
            for (int i = 0; i < nMeshNo; i++)
            {
                nChartVertNo += ((Mesh3D)meshs[i]).GetVertexNo();
                nChartTriangelNo += ((Mesh3D)meshs[i]).GetTriangleNo();
            }
            labelVertNo.Content = String.Format("Vertex No: {0:d}", nChartVertNo);
            labelTriNo.Content = String.Format("Triangle No: {0:d}", nChartTriangelNo);
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                imgPhoto.Source = new BitmapImage(new Uri(op.FileName));
                filePath = op.FileName;
            }
        }

        // this function is used to rotate, drag and zoom the 3d chart
        private void TransformChart()
        {
            if (m_nChartModelIndex == -1) return;
            ModelVisual3D visual3d = (ModelVisual3D)(this.mainViewport.Children[m_nChartModelIndex]);
            if (visual3d.Content == null) return;
            Transform3DGroup group1 = visual3d.Content.Transform as Transform3DGroup;
            group1.Children.Clear();
            group1.Children.Add(new MatrixTransform3D(m_transformMatrix.m_totalMatrix));
        }
        
        private DateTime downTime;
        private object downSender;
        private void imgPhoto_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.downSender = sender;
                this.downTime = DateTime.Now;
            }
        }

        private void imgPhoto_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released && sender == this.downSender)
            {
                TimeSpan timeSinceDown = DateTime.Now - this.downTime;
                if (timeSinceDown.TotalMilliseconds < 100)
                {
                    var img = imgPhoto.Source as BitmapSource;
                    int stride = img.PixelWidth * 4;
                    int size = img.PixelHeight * stride;
                    byte[] pixels = new byte[size];
                    img.CopyPixels(pixels, stride, 0);

                    var downPosition = e.GetPosition(sender as Image);
                    int index = (int)downPosition.Y * stride + 4 * (int)downPosition.X;

                    byte blue = pixels[index];       //exactly red?
                    byte green = pixels[index + 1];
                    byte red = pixels[index + 2];    //exactly blue?
                    byte alpha = pixels[index + 3];

                    var rect = new Rect(downPosition.X, downPosition.Y, 10, 10);

                    var points = new List<Point>();
                    
                    var bmp = new SysDrawing.Bitmap(filePath);
                    
                    for (int i = 0; i < bmp.Size.Height; i++)
                        for (int j = 0; j < bmp.Size.Width; j++)
                        {
                            if (rect.Contains(new Point(j, i)))
                            {
                                points.Add(new Point(j, i));
                            }
                        }
                    
                    var rgbList = new List<RGB>();
                    foreach (var point in points)
                    {
                        var indexer = (int)point.Y * stride + 4 * (int)point.X;
                        var RGB = new RGB(pixels[indexer + 2], pixels[indexer + 1], pixels[indexer],
                            point.X, point.Y);
                        rgbList.Add(RGB);
                    }
                    if (checkBoxUse_Shape.IsChecked == true)
                    {
                        DrawScatterPlot(rgbList);
                    }
                    else if (checkBoxUse_Surface.IsChecked == true)
                    {
                        DrawSurfacePlot(rgbList);
                    }
                }
            }
        }

        // function for testing 3d scatter plot
        public void DrawScatterPlot(List<RGB> rgbList)
        {
            // 1. set scatter chart data no.
            m_3dChart = new ScatterChart3D();
            m_3dChart.SetDataNo(rgbList.Count);

            // 2. set property of each dot (size, position, shape, color)
            Random randomObject = new Random();
            int nDataRange = 200;

            var zArray = new float[rgbList.Count];
            for (int i = 0; i < rgbList.Count; i++)
            {
                var r = (Byte)rgbList[i].R;
                var g = (Byte)rgbList[i].G;
                var b = (Byte)rgbList[i].B;

                zArray[i] = (float)(Math.Sqrt(Math.Pow(r + 512, 2) + Math.Pow(g + 256, 2) + b * b));
            }

            //------------------------------------------------------------------------------------------------//
            
            var zMax = zArray.Max();
            var zMin = zArray.Min();

            var diff = (zMax - zMin) / 100;
            diff = (diff * Convert.ToInt32(splitterPersentage.Text)) + zMin;
/*           
            int count = 0;
            long sum = 0;
            for (int i = 0; i < zArray.Length; i++)
            {
                sum += (long)zArray[i];
                count++;
            }
            diff = sum / count;
*/          
            var lowList = new List<PossAndDiff>();
            var hightList = new List<PossAndDiff>();

            var possValueList = new List<PossAndDiff>();
            for (int i = 0; i < zArray.Length; i++)
            {
                if (zArray[i] < diff)
                {
                    lowList.Add(new PossAndDiff(i, zArray[i]));
                }
                if (zArray[i] >= diff)
                {
                    hightList.Add(new PossAndDiff(i, zArray[i]));
                }
            }

            var lowArray = lowList.ToArray();
            var hightArray = hightList.ToArray();

            for (int i = 0; i < lowArray.Length; i++)
            {
                lowArray[i] = lowArray[i];
            }
            for (int i = 0; i < hightArray.Length; i++)
            {
                hightArray[i] = hightArray[i];
            }
            var resArray = new float[zArray.Length];
            //Array.Copy(lowArray, 0, resArray, 0, lowArray.Length);
            //Array.Copy(hightArray, 0, resArray, lowArray.Length, hightArray.Length);
            
                        for (int i = 0; i < resArray.Length; i++)
                        {
                            foreach (var low in lowArray)
                            {
                                if (low.Possition == i)
                                {
                                    resArray[i] = low.Value - 50;
                                }
                            }
                            foreach (var hight in hightArray)
                            {
                                if (hight.Possition == i)
                                {
                                    resArray[i] = hight.Value;
                                }
                            }
                        }
            
            /*
            var possArray = new PossAndDiff[resArray.Length];
            for (int i = 0; i < resArray.Length; i++)
            {
                if (i < lowArray.Length)
                {
                    if (lowArray[i].Possition == i)
                    {
                        possArray[i] = lowArray[i];
                        resArray[i] = lowArray[i].Value - 50;
                        continue;
                    }
                }
                if (i < hightArray.Length)
                {
                    if (hightArray[i].Possition == i)
                    {
                        possArray[i] = hightArray[i];
                        resArray[i] = hightArray[i].Value;
                    }
                }
            }

            for (int i = 0; i < possArray.Length; i++)
            {
                if (possArray[i].Possition != i)
                    possArray = null;
            }
            */
            zArray = resArray;
            
            //------------------------------------------------------------------------------------------------//

            for (int i = 0; i < rgbList.Count; i++)
            {
                ScatterPlotItem plotItem = new ScatterPlotItem();

                plotItem.w = 4;
                plotItem.h = 6;

                var r = (Byte)rgbList[i].R;
                var g = (Byte)rgbList[i].G;
                var b = (Byte)rgbList[i].B;
                /*
                                plotItem.x = (float)r;
                                plotItem.y = (float)g;
                                plotItem.z = (float)b;
                */
                plotItem.x = (float)rgbList[i].X;
                plotItem.y = (float)rgbList[i].Y;
                plotItem.z = zArray[i] - 500;

                plotItem.shape = randomObject.Next(4);

                Byte nR = r;
                Byte nG = g;
                Byte nB = b;

                plotItem.color = Color.FromRgb(nR, nG, nB);
                ((ScatterChart3D)m_3dChart).SetVertex(i, plotItem);
            }

            // 3. set the axes
            m_3dChart.GetDataRange();
            m_3dChart.SetAxes();

            // 4. get Mesh3D array from the scatter plot
            ArrayList meshs = ((ScatterChart3D)m_3dChart).GetMeshes();

            // 5. display model vertex no and triangle no
            UpdateModelSizeInfo(meshs);

            // 6. display scatter plot in Viewport3D
            Model3D model3d = new Model3D();
            m_nChartModelIndex = model3d.UpdateModel(meshs, null, m_nChartModelIndex, this.mainViewport);

            // 7. set projection matrix
            float viewRange = (float)nDataRange;
            m_transformMatrix.CalculateProjectionMatrix(0, viewRange, 0, viewRange, 0, viewRange, 0.5);
            TransformChart();
        }

        // function for testing surface chart
        public void DrawSurfacePlot(List<RGB> rgbList)
        {
            int nXNo = rgbList.Count;
            int nYNo = rgbList.Count;
            // 1. set the surface grid
            m_3dChart = new UniformSurfaceChart3D();
            ((UniformSurfaceChart3D)m_3dChart).SetGridRGB(rgbList, nXNo, nYNo, -100, 100, -100, 100);

            // 2. set surface chart z value
            double xC = m_3dChart.XCenter();
            double yC = m_3dChart.YCenter();
            int nVertNo = m_3dChart.GetDataNo();
            double zV;
            for (int i = 0; i < nVertNo; i++)
            {
                Vertex3D vert = m_3dChart[i];

                double r = 0.15 * Math.Sqrt((vert.x - xC) * (vert.x - xC) + (vert.y - yC) * (vert.y - yC));
                if (r < 1e-10) zV = 1;
                else zV = Math.Sin(r) / r;

                m_3dChart[i].z = (float)zV;
            }
            m_3dChart.GetDataRange();

            // 3. set the surface chart color according to z vaule
            double zMin = m_3dChart.ZMin();
            double zMax = m_3dChart.ZMax();
            for (int i = 0; i < nVertNo; i++)
            {
                Vertex3D vert = m_3dChart[i];
                double h = (vert.z - zMin) / (zMax - zMin);

                Color color = new Color(); // TextureMapping.PseudoColor(h);
                if (i >= rgbList.Count)
                    color = Color.FromRgb(0, 0, 0);
                else
                {
                    color.R = rgbList[i].R;
                    color.G = rgbList[i].G;
                    color.B = rgbList[i].B;
                }
                m_3dChart[i].color = color;
            }

            // 4. Get the Mesh3D array from surface chart
            ArrayList meshs = ((UniformSurfaceChart3D)m_3dChart).GetMeshes();

            // 5. display vertex no and triangle no of this surface chart
            UpdateModelSizeInfo(meshs);

            // 6. Set the model display of surface chart
            Model3D model3d = new Model3D();
            Material backMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Gray));
            m_nChartModelIndex = model3d.UpdateModel(meshs, backMaterial, m_nChartModelIndex, this.mainViewport);

            // 7. set projection matrix, so the data is in the display region
            float xMin = m_3dChart.XMin();
            float xMax = m_3dChart.XMax();
            m_transformMatrix.CalculateProjectionMatrix(xMin, xMax, xMin, xMax, zMin, zMax, 0.5);
            TransformChart();
        }

        private void scatterAllImageButton_Click(object sender, RoutedEventArgs e)
        {
            DrawScatterAllImagePlot();
        }

        public void DrawScatterAllImagePlot()
        {
            var bmp = new SysDrawing.Bitmap(filePath);
            var points = new List<Point>();
            var img = imgPhoto.Source as BitmapSource;
            int stride = img.PixelWidth * 4;
            int size = img.PixelHeight * stride;

            for (int i = 0; i < bmp.Size.Height; i++)
                for (int j = 0; j < bmp.Size.Width; j++)
                        points.Add(new Point(j, i));

            byte[] pixels = new byte[size];
            img.CopyPixels(pixels, stride, 0);

            var rgbList = new List<RGB>();
            foreach (var point in points)
            {
                var indexer = (int)point.Y * stride + 4 * (int)point.X;
                var RGB = new RGB(pixels[indexer + 2], pixels[indexer + 1], pixels[indexer],
                    point.X, point.Y);
                rgbList.Add(RGB);
            }
            DrawScatterPlot(rgbList);
        }
    }

    public class PossAndDiff
    {
        public int Possition;
        public float Value;

        public PossAndDiff(int possition, float value)
        {
            Possition = possition;
            Value = value;
        }

        public override string ToString()
        {
            return String.Format("[{0}] {1}", Possition, Value);
        }
    }

    public class RGB
    {
        public byte R;
        public byte G;
        public byte B;
        public double X;
        public double Y;

        public RGB(byte r, byte g, byte b, double x, double y)
        {
            R = r;
            G = g;
            B = b;

            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return String.Format("R:{0} G:{1} B{2}",R,G,B);
        }
    }
}