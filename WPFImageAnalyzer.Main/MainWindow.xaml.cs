//------------------------------------------------------------------
// (c) Copywrite Jianzhong Zhang
// This code is under The Code Project Open License
// Please read the attached license document before using this class
//------------------------------------------------------------------

// window class for testing 3d chart using WPF
// version 0.1

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Microsoft.Win32;
using WPFImageAnalyzer;
using SysDrawing = System.Drawing;

namespace WPFChart3D
{
    public partial class MainWindow : Window
    {
        // transform class object for rotate the 3d model
        private TransformMatrix _mTransformMatrix = new TransformMatrix();

        // ***************************** 3d chart ***************************
        private Chart3D _m_3DChart;       // data for 3d chart
        public int MnChartModelIndex = -1;         // model index in the Viewport3d
        public int MnSurfaceChartGridNo = 100;     // surface chart grid no. in each axis
        public int MnScatterPlotDataNo = 5000;     // total data number of the scatter plot

        // ***************************** selection rect ***************************
        ViewportRect m_selectRect = new ViewportRect();
        public int m_nRectModelIndex = -1;

        private string filePath = String.Empty;

        public MainWindow()
        {
            InitializeComponent();

            // selection rect
            m_selectRect.SetRect(new Point(-0.5, -0.5), new Point(-0.5, -0.5));
            var model3d = new Model3D();
            var meshs = m_selectRect.GetMeshes();
            m_nRectModelIndex = model3d.UpdateModel(meshs, null, m_nRectModelIndex, mainViewport);

            // display the 3d chart data no.
            gridNo.Text = String.Format("{0:d}", MnSurfaceChartGridNo);
//            dataNo.Text = String.Format("{0:d}", m_nScatterPlotDataNo);

            // display surface chart
            TestScatterPlot(1000);
            TransformChart();
        }

        // function for testing 3d scatter plot
        public void TestScatterPlot(int nDotNo)
        {
            // 1. set scatter chart data no.
            _m_3DChart = new ScatterChart3D();
            _m_3DChart.SetDataNo(nDotNo);

            // 2. set property of each dot (size, position, shape, color)
            var randomObject = new Random();
            var nDataRange = 200;
            for (int i = 0; i < nDotNo; i++)
            {
                var plotItem = new ScatterPlotItem
                {
                    w = 4,
                    h = 6,
                    x = randomObject.Next(nDataRange),
                    y = randomObject.Next(nDataRange),
                    z = randomObject.Next(nDataRange),
                    shape = randomObject.Next(4)
                };

                var nR = (byte)randomObject.Next(256);
                var nG = (byte)randomObject.Next(256);
                var nB = (byte)randomObject.Next(256);

                plotItem.color = Color.FromRgb(nR, nG, nB);
                ((ScatterChart3D)_m_3DChart).SetVertex(i, plotItem);
            }

            // 3. set the axes
            _m_3DChart.GetDataRange();
            _m_3DChart.SetAxes();

            // 4. get Mesh3D array from the scatter plot
            var meshs = ((ScatterChart3D)_m_3DChart).GetMeshes();

            // 5. display model vertex no and triangle no
            UpdateModelSizeInfo(meshs);

            // 6. display scatter plot in Viewport3D
            var model3d = new Model3D();
            MnChartModelIndex = model3d.UpdateModel(meshs, null, MnChartModelIndex, mainViewport);
 
            // 7. set projection matrix
            float viewRange = nDataRange;
            _mTransformMatrix.CalculateProjectionMatrix(0, viewRange, 0, viewRange, 0, viewRange, 0.5);
            TransformChart();
        }

        // function for set a scatter plot, every dot is just a simple pyramid.
        public void TestSimpleScatterPlot(int nDotNo)
        {
            // 1. set the scatter plot size
            _m_3DChart = new ScatterChart3D();
            _m_3DChart.SetDataNo(nDotNo);

            // 2. set the properties of each dot
            var randomObject = new Random();
            var nDataRange = 200;
            for (int i = 0; i < nDotNo; i++)
            {
                var plotItem = new ScatterPlotItem
                {
                    w = 2,
                    h = 2,
                    x = randomObject.Next(nDataRange),
                    y = randomObject.Next(nDataRange),
                    z = randomObject.Next(nDataRange),
                    shape = (int) Chart3D.Shape.Pyramid
                };
                
                var nR = (byte)randomObject.Next(256);
                var nG = (byte)randomObject.Next(256);
                var nB = (byte)randomObject.Next(256);

                plotItem.color = Color.FromRgb(nR, nG, nB);
                ((ScatterChart3D)_m_3DChart).SetVertex(i, plotItem);
            }
            // 3. set axes
            _m_3DChart.GetDataRange();
            _m_3DChart.SetAxes();

            // 4. Get Mesh3D array from scatter plot
            var meshs = ((ScatterChart3D)_m_3DChart).GetMeshes();

            // 5. display vertex no and triangle no.
            UpdateModelSizeInfo(meshs);

            // 6. show 3D scatter plot in Viewport3d
            var model3d = new Model3D();
            MnChartModelIndex = model3d.UpdateModel(meshs, null, MnChartModelIndex, mainViewport);

            // 7. set projection matrix
            float viewRange = nDataRange;
            _mTransformMatrix.CalculateProjectionMatrix(0, viewRange, 0, viewRange, 0, viewRange, 0.5);
            TransformChart();
        }

        public void OnViewportMouseDown(object sender, MouseButtonEventArgs args)
        {
            var pt = args.GetPosition(mainViewport);
            if (args.ChangedButton == MouseButton.Left)         // rotate or drag 3d model
            {
                _mTransformMatrix.OnLBtnDown(pt);
            }
            else if (args.ChangedButton == MouseButton.Right)   // select rect
            {
                m_selectRect.OnMouseDown(pt, mainViewport, m_nRectModelIndex);
            }
        }

        public void OnViewportMouseMove(object sender, MouseEventArgs args)
        {
            var pt = args.GetPosition(mainViewport);

            if (args.LeftButton == MouseButtonState.Pressed)                // rotate or drag 3d model
            {
                _mTransformMatrix.OnMouseMove(pt, mainViewport);

                TransformChart();
            }
            else if (args.RightButton == MouseButtonState.Pressed)          // select rect
            {
                m_selectRect.OnMouseMove(pt, mainViewport, m_nRectModelIndex);
            }
        }

        public void OnViewportMouseUp(object sender, MouseButtonEventArgs args)
        {
            args.GetPosition(mainViewport);
            if (args.ChangedButton == MouseButton.Left)
            {
                _mTransformMatrix.OnLBtnUp();
            }
            else if (args.ChangedButton == MouseButton.Right)
            {
                if (MnChartModelIndex == -1) return;
                // 1. get the mesh structure related to the selection rect
                var meshGeometry = Model3D.GetGeometry(mainViewport, MnChartModelIndex);
                if (meshGeometry == null) return;
              
                // 2. set selection in 3d chart
                _m_3DChart.Select(m_selectRect, _mTransformMatrix, mainViewport);

                // 3. update selection display
                _m_3DChart.HighlightSelection(meshGeometry, Color.FromRgb(200, 200, 200));
            }
        }

        // zoom in 3d display
        public void OnKeyDown(object sender, KeyEventArgs args)
        {
            _mTransformMatrix.OnKeyDown(args);
            TransformChart();
        }

        private void UpdateModelSizeInfo(ArrayList meshs)
        {
            var nMeshNo = meshs.Count;
            var nChartVertNo = 0;
            var nChartTriangelNo = 0;
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
            var op = new OpenFileDialog
            {
                Title = "Select a picture",
                Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
                         "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                         "Portable Network Graphic (*.png)|*.png"
            };
            if (op.ShowDialog() == true)
            {
                imgPhoto.Source = new BitmapImage(new Uri(op.FileName));
                filePath = op.FileName;
            }
        }

        // this function is used to rotate, drag and zoom the 3d chart
        private void TransformChart()
        {
            if (MnChartModelIndex == -1) return;
            var visual3d = (ModelVisual3D)(mainViewport.Children[MnChartModelIndex]);
            if (visual3d.Content == null) return;
            var group1 = visual3d.Content.Transform as Transform3DGroup;
            group1.Children.Clear();
            group1.Children.Add(new MatrixTransform3D(_mTransformMatrix.m_totalMatrix));
        }
        
        private DateTime downTime;
        private object downSender;
        private void imgPhoto_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                downSender = sender;
                downTime = DateTime.Now;
            }
        }

        private void imgPhoto_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released && sender == downSender)
            {
                var timeSinceDown = DateTime.Now - downTime;
                if (timeSinceDown.TotalMilliseconds < 100)
                {
                    var img = imgPhoto.Source as BitmapSource;
                    var stride = img.PixelWidth * 4;
                    var size = img.PixelHeight * stride;
                    byte[] pixels = new byte[size];
                    img.CopyPixels(pixels, stride, 0);

                    var downPosition = e.GetPosition(sender as Image);
                    int index = (int)downPosition.Y * stride + 4 * (int)downPosition.X;

                    /*
                    byte blue = pixels[index];       //exactly red?
                    byte green = pixels[index + 1];
                    byte red = pixels[index + 2];    //exactly blue?
                    byte alpha = pixels[index + 3];
                    */
                    var rect = new Rect(downPosition.X, downPosition.Y, 10, 10);

                    var points = new List<Point>();
                    
                    var bmp = new SysDrawing.Bitmap(filePath);
                    
                    for (var i = 0; i < bmp.Size.Height; i++)
                        for (var j = 0; j < bmp.Size.Width; j++)
                        {
                            if (rect.Contains(new Point(j, i)))
                            {
                                points.Add(new Point(j, i));
                            }
                        }
                    
                    var rgbList = (from point in points let indexer = (int) point.Y*stride + 4*(int) point.X
                                   select new Rgb(pixels[indexer + 2], pixels[indexer + 1], pixels[indexer], point.X, point.Y)).ToList();
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
        public void DrawScatterPlot(List<Rgb> rgbList)
        {
            // 1. set scatter chart data no.
            _m_3DChart = new ScatterChart3D();
            _m_3DChart.SetDataNo(rgbList.Count);

            // 2. set property of each dot (size, position, shape, color)
            var randomObject = new Random();
            var nDataRange = 200;

            var zArray = new float[rgbList.Count];
            for (var i = 0; i < rgbList.Count; i++)
            {
                var r = rgbList[i].R;
                var g = rgbList[i].G;
                var b = rgbList[i].B;


                zArray[i] = (float)(Math.Sqrt(Math.Pow(r + 512, 2) + Math.Pow(g + 256, 2) + b * b));
    //            zArray[i] = (float)(Math.Sqrt(Math.Pow(r, 2) + Math.Pow(g, 2) + b * b));
            }

            //------------------------------------------------------------------------------------------------//
            
            var zMax = zArray.Max();
            var zMin = zArray.Min();

            var onePercent = (zMax - zMin) / 100;

            var percents = StaticInterfaceHandler.GetSplitterArray(this);
            var diffs = ToDiffArray(percents, onePercent, zMin);

            zArray = SortByDiffs(zArray, diffs, 30, new[]
            {
                CheckBoxInvisible.IsChecked != null && CheckBoxInvisible.IsChecked.Value,
                CheckBoxInvisible1.IsChecked != null && CheckBoxInvisible1.IsChecked.Value,
                CheckBoxInvisible2.IsChecked != null && CheckBoxInvisible2.IsChecked.Value,
                CheckBoxInvisible3.IsChecked != null && CheckBoxInvisible3.IsChecked.Value
            });


            //------------------------------------------------------------------------------------------------//
            
            for (var i = 0; i < rgbList.Count; i++)
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (zArray[i] == 0)
                    continue;

                var plotItem = new ScatterPlotItem
                {
                    w = 3,
                    h = 5
                };
                
                var r = rgbList[i].R;
                var g = rgbList[i].G;
                var b = rgbList[i].B;
                    
                plotItem.x = (float)rgbList[i].X;
                plotItem.y = (float)rgbList[i].Y;
                plotItem.z = zArray[i] - 500;
                
                plotItem.shape = randomObject.Next(4);

                var nR = r;
                var nG = g;
                var nB = b;

                //Region for contur drawing
                if (diffs.Length > 0)
                    plotItem.color = zArray[i] > diffs[0] - 5 && zArray[i] < diffs[0]
                        ? Color.FromRgb(255, 50, 50)
                        : Color.FromRgb(nR, nG, nB);
                else
                    plotItem.color = Color.FromRgb(nR, nG, nB);
                ((ScatterChart3D)_m_3DChart).SetVertex(i, plotItem);
            }

            // 3. set the axes
            _m_3DChart.GetDataRange();
            _m_3DChart.SetAxes();

            // 4. get Mesh3D array from the scatter plot
            ArrayList meshs = ((ScatterChart3D)_m_3DChart).GetMeshes();

            // 5. display model vertex no and triangle no
            UpdateModelSizeInfo(meshs);

            // 6. display scatter plot in Viewport3D
            var model3D = new Model3D();
            MnChartModelIndex = model3D.UpdateModel(meshs, null, MnChartModelIndex, mainViewport);

            // 7. set projection matrix
            var viewRange = (float)nDataRange;
            _mTransformMatrix.CalculateProjectionMatrix(0, viewRange, 0, viewRange, 0, viewRange, 0.5);
            TransformChart();
        }

        // function for testing surface chart
        public void DrawSurfacePlot(List<Rgb> rgbList)
        {
            var nXNo = rgbList.Count;
            var nYNo = rgbList.Count;
            // 1. set the surface grid
            _m_3DChart = new UniformSurfaceChart3D();
            ((UniformSurfaceChart3D)_m_3DChart).SetGridRgb(rgbList, nXNo, nYNo, -100, 100, -100, 100);

            // 2. set surface chart z value
            var xC = _m_3DChart.XCenter();
            var yC = _m_3DChart.YCenter();
            var nVertNo = _m_3DChart.GetDataNo();
            for (var i = 0; i < nVertNo; i++)
            {
                var vert = _m_3DChart[i];

                var r = 0.15 * Math.Sqrt((vert.x - xC) * (vert.x - xC) + (vert.y - yC) * (vert.y - yC));
                double zV;
                if (r < 1e-10) zV = 1;
                else zV = Math.Sin(r) / r;

                _m_3DChart[i].z = (float)zV;
            }
            _m_3DChart.GetDataRange();

            // 3. set the surface chart color according to z vaule
            var zMin = _m_3DChart.ZMin();
            var zMax = _m_3DChart.ZMax();
            for (int i = 0; i < nVertNo; i++)
            {
                var vert = _m_3DChart[i];
                var h = (vert.z - zMin) / (zMax - zMin);

                var color = new Color(); // TextureMapping.PseudoColor(h);
                if (i >= rgbList.Count)
                    color = Color.FromRgb(0, 0, 0);
                else
                {
                    color.R = rgbList[i].R;
                    color.G = rgbList[i].G;
                    color.B = rgbList[i].B;
                }
                _m_3DChart[i].color = color;
            }

            // 4. Get the Mesh3D array from surface chart
            var meshs = ((UniformSurfaceChart3D)_m_3DChart).GetMeshes();

            // 5. display vertex no and triangle no of this surface chart
            UpdateModelSizeInfo(meshs);

            // 6. Set the model display of surface chart
            var model3d = new Model3D();
            var backMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Gray));
            MnChartModelIndex = model3d.UpdateModel(meshs, backMaterial, MnChartModelIndex, mainViewport);

            // 7. set projection matrix, so the data is in the display region
            var xMin = _m_3DChart.XMin();
            var xMax = _m_3DChart.XMax();
            _mTransformMatrix.CalculateProjectionMatrix(xMin, xMax, xMin, xMax, zMin, zMax, 0.5);
            TransformChart();
        }

        private float[] ToDiffArray(int[] percentageArray, float percent, float min)
        {
            var diffArray = new float[percentageArray.Length];

            for (int i = 0; i < percentageArray.Length; i++)
            {
                diffArray[i] = percent * percentageArray[i] + min;
            }
            Array.Sort(diffArray);

            return diffArray;
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
            var stride = img.PixelWidth * 4;
            var size = img.PixelHeight * stride;

            for (int i = 0; i < bmp.Size.Height; i++)
                for (int j = 0; j < bmp.Size.Width; j++)
                        points.Add(new Point(j, i));

            var pixels = new byte[size];
            img.CopyPixels(pixels, stride, 0);

            var rgbList = (from point in points let indexer = (int) point.Y*stride + 4*(int) point.X
                           select new Rgb(pixels[indexer + 2], pixels[indexer + 1], pixels[indexer], point.X, point.Y)).ToList();
            DrawScatterPlot(rgbList);
        }

        private float[] SortByDiffs(float[] zArray, float[] diffs, int breakage, bool[] invisible)
        {
            for (var i = 0; i < zArray.Length; i++)//:((
            {
                if (diffs.Length == 1)
                {
                    if (zArray[i] > diffs[0])
                    {
                        if(invisible[0])
                            zArray[i] = 0;
                        else
                            zArray[i] = zArray[i] + breakage;
                    }
                }
                if (diffs.Length == 2)
                {
                    if (zArray[i] > diffs[0] && zArray[i] <= diffs[1])
                    {
                        if(invisible[0])
                            zArray[i] = 0;
                        else
                            zArray[i] = zArray[i] + breakage;
                    }
                    if (zArray[i] > diffs[1])
                    {
                        if(invisible[1])
                            zArray[i] = 0;
                        else
                            zArray[i] = zArray[i] + breakage;
                    }
                }
                if (diffs.Length == 3)
                {
                    if (zArray[i] > diffs[0] && zArray[i] <= diffs[1])
                    {
                        if(invisible[0])
                            zArray[i] = 0;
                        else
                            zArray[i] = zArray[i] + breakage;
                    }
                    if (zArray[i] > diffs[1] && zArray[i] <= diffs[2])
                    {
                        if(invisible[1])
                            zArray[i] = 0;
                        else
                            zArray[i] = zArray[i] + breakage;
                    }
                    if (zArray[i] > diffs[2])
                    {
                        if (invisible[2])
                            zArray[i] = 0;
                        else
                            zArray[i] = zArray[i] + breakage;
                    }
                }
                if (diffs.Length == 4)
                {
                    if (zArray[i] > diffs[0] && zArray[i] <= diffs[1])
                    {
                        if(invisible[0])
                            zArray[i] = 0;
                        else
                            zArray[i] = zArray[i] + breakage;
                    }
                    if (zArray[i] > diffs[1] && zArray[i] <= diffs[2])
                    {
                        if (invisible[1])
                            zArray[i] = 0;
                        else
                            zArray[i] = zArray[i] + breakage;
                    }
                    if (zArray[i] > diffs[2] && zArray[i] <= diffs[3])
                    {
                        if (invisible[2])
                            zArray[i] = 0;
                        else
                            zArray[i] = zArray[i] + breakage;
                    }
                    if (zArray[i] > diffs[3])
                    {
                        if (invisible[3])
                            zArray[i] = 0;
                        else
                            zArray[i] = zArray[i] + breakage;
                    }
                }
            }
            return zArray;
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
            return $"[{Possition}] {Value}";
        }
    }

    public class Rgb
    {
        public byte R;
        public byte G;
        public byte B;
        public double X;
        public double Y;

        public Rgb(byte r, byte g, byte b, double x, double y)
        {
            R = r;
            G = g;
            B = b;

            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"R:{R} G:{G} B{B}";
        }
    }
}