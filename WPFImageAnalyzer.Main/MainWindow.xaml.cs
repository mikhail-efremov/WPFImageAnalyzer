using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Microsoft.Win32;
using WPFImageAnalyzer;
using SysDrawing = System.Drawing;

// ReSharper disable once CheckNamespace
namespace WPFChart3D
{
    public partial class MainWindow : Window
    {
        // transform class object for rotate the 3d model
        private readonly TransformMatrix _mTransformMatrix = new TransformMatrix();

        // ***************************** 3d chart ***************************
        private Chart3D _m_3DChart;       // data for 3d chart
        public int MnChartModelIndex = -1;         // model index in the Viewport3d
        public int MnSurfaceChartGridNo = 100;     // surface chart grid no. in each axis
        public int MnScatterPlotDataNo = 5000;     // total data number of the scatter plot

        // ***************************** selection rect ***************************
        readonly ViewportRect _mSelectRect = new ViewportRect();
        public int MnRectModelIndex = -1;

        private bool _imageSetted = false;
        private string _filePath = string.Empty;

        public MainWindow()
        {
            InitializeComponent();

            // selection rect
            _mSelectRect.SetRect(new Point(-0.5, -0.5), new Point(-0.5, -0.5));
            var model3D = new Model3D();
            var meshs = _mSelectRect.GetMeshes();
            MnRectModelIndex = model3D.UpdateModel(meshs, null, MnRectModelIndex, mainViewport);
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
            var model3D = new Model3D();
            MnChartModelIndex = model3D.UpdateModel(meshs, null, MnChartModelIndex, mainViewport);
 
            // 7. set projection matrix
            var viewRange = nDataRange;
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
            const int nDataRange = 200;
            for (var i = 0; i < nDotNo; i++)
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
            var model3D = new Model3D();
            MnChartModelIndex = model3D.UpdateModel(meshs, null, MnChartModelIndex, mainViewport);

            // 7. set projection matrix
            var viewRange = nDataRange;
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
                _mSelectRect.OnMouseDown(pt, mainViewport, MnRectModelIndex);
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
                _mSelectRect.OnMouseMove(pt, mainViewport, MnRectModelIndex);
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
                _m_3DChart.Select(_mSelectRect, _mTransformMatrix, mainViewport);

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

        private void UpdateModelSizeInfo(IList meshs)
        {
            var nMeshNo = meshs.Count;
            var nChartVertNo = 0;
            var nChartTriangelNo = 0;
            for (var i = 0; i < nMeshNo; i++)
            {
                nChartVertNo += ((Mesh3D)meshs[i]).GetVertexNo();
                nChartTriangelNo += ((Mesh3D)meshs[i]).GetTriangleNo();
            }
            labelVertNo.Content = $"Vertex No: {nChartVertNo:d}";
            labelTriNo.Content = $"Triangle No: {nChartTriangelNo:d}";
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
                _filePath = op.FileName;
            }
        }

        private string SetNewFileName(string oldFileName)
        {
            var dir = System.IO.Path.GetDirectoryName(oldFileName);
            var name = System.IO.Path.GetFileNameWithoutExtension(oldFileName);
            var exten = System.IO.Path.GetExtension(oldFileName);

            while (true)
            {
                var newFileName = dir + "\\temp\\" + name + DateTime.Now.Millisecond + exten;
                _imageSetted = true;
                if (!System.IO.File.Exists(newFileName))
                    return newFileName;
            }
        }

        // this function is used to rotate, drag and zoom the 3d chart
        private void TransformChart()
        {
            if (MnChartModelIndex == -1) return;
            var visual3D = (ModelVisual3D)(mainViewport.Children[MnChartModelIndex]);
            var group1 = visual3D.Content?.Transform as Transform3DGroup;
            if (group1 == null) return;
            group1.Children.Clear();
            group1.Children.Add(new MatrixTransform3D(_mTransformMatrix.m_totalMatrix));
        }
        
        // function for testing 3d scatter plot
        public void DrawScatterPlot(List<Rgb> rgbList)
        {
            if (rgbList == null) throw new ArgumentNullException(nameof(rgbList));
            // 1. set scatter chart data no.
            _m_3DChart = new ScatterChart3D();
            _m_3DChart.SetDataNo(rgbList.Count);

            // 2. set property of each dot (size, position, shape, color)
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

            zArray = SortByDiffs(zArray, diffs, int.Parse(splitterBackage.Text), new[]
            {
                CheckBoxInvisible.IsChecked != null && CheckBoxInvisible.IsChecked.Value,
                CheckBoxInvisible1.IsChecked != null && CheckBoxInvisible1.IsChecked.Value,
                CheckBoxInvisible2.IsChecked != null && CheckBoxInvisible2.IsChecked.Value,
                CheckBoxInvisible3.IsChecked != null && CheckBoxInvisible3.IsChecked.Value
            });

            //------------------------------------------------------------------------------------------------//

            var rgbCortage = new RgbCortage();
            for (var i = 0; i < rgbList.Count; i++)
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (zArray[i] == 0)
                    continue;

                var plotItem = new ScatterPlotItem
                {
                    w = 1,
                    h = 1
                };

                var r = rgbList[i].R;
                var g = rgbList[i].G;
                var b = rgbList[i].B;

                plotItem.x = (float) rgbList[i].X;
                plotItem.y = (float) rgbList[i].Y;
                plotItem.z = zArray[i] - 500;

                //   plotItem.shape = randomObject.Next(4);

                plotItem.shape = (int) Chart3D.Shape.Cylinder;

                plotItem.color = Color.FromRgb(r, g, b);
                ((ScatterChart3D) _m_3DChart).SetVertex(i, plotItem);
            }
            
            for (var i = 0; i < diffs.Length; i++)
            {
                var diff = diffs[i];
                float maxDiff;
                if (i == 0)
                    maxDiff = zArray.Max();
                else
                    maxDiff = diffs[i-1] + diff;
                const int lowSensivity = 0;
                var highSensivity = maxDiff - diff;
                
                rgbCortage.RegisterNewContainer(GetBordersLine(zArray, rgbList, diff, lowSensivity, highSensivity));
            }

            foreach (var container in rgbCortage.Container)
            {
                foreach (var rad in container)
                {
                    var color = new Color();
                    if (container.Equals(rgbCortage.Container[0]))
                    {
                        if (ColorPicker0.SelectedColor != null) color = (Color)ColorPicker0.SelectedColor;
                    }
                    else if (container.Equals(rgbCortage.Container[1]))
                    {
                        if (ColorPicker1.SelectedColor != null) color = (Color)ColorPicker1.SelectedColor;
                    }
                    else if (container.Equals(rgbCortage.Container[2]))
                    {
                        if (ColorPicker2.SelectedColor != null) color = (Color) ColorPicker2.SelectedColor;
                    }
                    else if (container.Equals(rgbCortage.Container[3]))
                    {
                        if (ColorPicker3.SelectedColor != null) color = (Color) ColorPicker3.SelectedColor;
                    }

                    var plotItem = new ScatterPlotItem
                    {
                        w = 1,
                        h = 1,
                        x = (float) rad.X,
                        y = (float) rad.Y,
                        z = (float) rad.Z - 500,
                        shape = (int) Chart3D.Shape.Cylinder,
                        color = color
                    };
                    ((ScatterChart3D)_m_3DChart).SetVertex(rad.Possition, plotItem);
                }
            }

            _m_3DChart.GetDataRange();
            _m_3DChart.SetAxes();

            var meshs = ((ScatterChart3D)_m_3DChart).GetMeshes();

            UpdateModelSizeInfo(meshs);

            var model3D = new Model3D();
            MnChartModelIndex = model3D.UpdateModel(meshs, null, MnChartModelIndex, mainViewport);

            var viewRange = (float)nDataRange;
            _mTransformMatrix.CalculateProjectionMatrix(0, viewRange, 0, viewRange, 0, viewRange, 0.5);
            TransformChart();

            SetEditedPicture(rgbCortage, rgbList);
        }

        private void SetEditedPicture(RgbCortage rgbCortage, IList<Rgb> rgbList)
        {
            foreach (var container in rgbCortage.Container)
            {
                foreach (var rad in container)
                {
                    rgbList[rad.Possition] = new Rgb(rad.A, rad.R, rad.G, rad.B, rad.X, rad.Y, rad.Z, rad.Possition);
                }
            }
            
            var bmp = new SysDrawing.Bitmap(_filePath);

            Color sColor0 = new Color();
            Color sColor1 = new Color();
            Color sColor2 = new Color();
            Color sColor3 = new Color();

            if (ColorPicker0.SelectedColor != null)
            {
                sColor0 = ColorPicker0.SelectedColor.Value;
            }
            if (ColorPicker1.SelectedColor != null)
            {
                sColor1 = ColorPicker1.SelectedColor.Value;
            }
            if (ColorPicker2.SelectedColor != null)
            {
                sColor2 = ColorPicker2.SelectedColor.Value;
            }
            if (ColorPicker3.SelectedColor != null)
            {
                sColor3 = ColorPicker3.SelectedColor.Value;
            }

            foreach (var container in rgbCortage.Container)
            {
                foreach (var rad in container)
                {
                    var color = new SysDrawing.Color();
                    if (container.Equals(rgbCortage.Container[0]))
                        color = SysDrawing.Color.FromArgb(sColor0.R, sColor0.G, sColor0.B);
                    else if (container.Equals(rgbCortage.Container[1]))
                        color = SysDrawing.Color.FromArgb(sColor1.R, sColor1.G, sColor1.B);
                    else if (container.Equals(rgbCortage.Container[2]))
                        color = SysDrawing.Color.FromArgb(sColor2.R, sColor2.G, sColor2.B);
                    else if (container.Equals(rgbCortage.Container[3]))
                        color = SysDrawing.Color.FromArgb(sColor3.R, sColor3.G, sColor3.B);

                    bmp.SetPixel((int)rad.X, (int)rad.Y, color);
                }
            }
            
            var newFileName = SetNewFileName(_filePath);

            bmp.Save(newFileName);

            var logo = new BitmapImage();
            logo.BeginInit();
            logo.UriSource = new Uri(newFileName);
            logo.EndInit();

            imgTest.Source = logo;
        }

        private static IEnumerable<Rgb> GetBordersLine(
            IReadOnlyList<float> zArray, 
            IReadOnlyList<Rgb> rgbList, 
            float diff,
            float lowSensivity, 
            float highSensivity)
        {
            var radgisticList = new List<Rgb>();
            
            for (var i = 0; i < zArray.Count; i++)
            {
                if (zArray[i] > diff + lowSensivity && zArray[i] < diff + highSensivity)
                {
                    radgisticList.Add(new Rgb(
                        rgbList[i].A,
                        rgbList[i].R,
                        rgbList[i].G,
                        rgbList[i].B,
                        rgbList[i].X,
                        rgbList[i].Y,
                        zArray[i],
                        i));
                }
            }
            return radgisticList;
        }
        
        private static float[] ToDiffArray(IReadOnlyList<int> percentageArray, float percent, float min)
        {
            var diffArray = new float[percentageArray.Count];

            for (var i = 0; i < percentageArray.Count; i++)
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
            var bmp = new SysDrawing.Bitmap(_filePath);
            var points = new List<Point>();
            var img = imgPhoto.Source as BitmapSource;
            if (img != null)
            {
                var stride = img.PixelWidth * 4;
                var size = img.PixelHeight * stride;

                for (var i = 0; i < bmp.Size.Height; i++)
                    for (var j = 0; j < bmp.Size.Width; j++)
                        points.Add(new Point(j, i));

                var pixels = new byte[size];
                img.CopyPixels(pixels, stride, 0);

                var rgbList = (from point in points
                               let indexer = (int) point.Y*stride + 4*(int) point.X
                               select new Rgb(pixels[indexer + 3], pixels[indexer + 2], pixels[indexer + 1], pixels[indexer], point.X, point.Y)
                               ).ToList();

                DrawScatterPlot(rgbList);
            }
            GC.Collect();
        }

        private static float[] SortByDiffs(float[] zArray, IReadOnlyList<float> diffs, int breakage, IReadOnlyList<bool> invisible)
        {
            for (var i = 0; i < zArray.Length; i++)//:((
            {
                if (diffs.Count == 1)
                {
                    if (zArray[i] > diffs[0])
                    {
                        if(invisible[0])
                            zArray[i] = 0;
                        else
                            zArray[i] = zArray[i] + breakage;
                    }
                }
                if (diffs.Count == 2)
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
                if (diffs.Count == 3)
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
                if (diffs.Count == 4)
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
}