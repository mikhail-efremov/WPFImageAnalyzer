using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFChart3D;
using System.Windows.Forms;

namespace WPFImageAnalyzer
{
    public static class StaticInterfaceHandler
    {
        public static List<Tuple<int, int>> GetSplitterArray(MainWindow mainWindow)
        {
            var splitterList = new List<Tuple<int,int>>();

            if (mainWindow.CheckBoxPersentage.IsChecked == true)
            {
                int splitter01;
                if (int.TryParse(mainWindow.splitterPersentage101.Text, out splitter01))
                {
                    int splitter02;
                    if (int.TryParse(mainWindow.splitterPersentage102.Text, out splitter02))
                    {
                        if (splitter01 < splitter02)
                        {
                            if (splitter01 > 0 && splitter01 < 100 && splitter02 > 0 && splitter02 < 100)
                                splitterList.Add(new Tuple<int, int>(splitter01, splitter02));
                        }
                    }
                }
                else MessageBox.Show(@"Cannot parse value at 1st textBox. Value have to be int, less then 0" +
                     @"and greater then 0");
            }
            if (mainWindow.CheckBoxPersentage1.IsChecked == true)
            {
                int splitter01;
                if (int.TryParse(mainWindow.splitterPersentage201.Text, out splitter01))
                {
                    int splitter02;
                    if (int.TryParse(mainWindow.splitterPersentage202.Text, out splitter02))
                    {
                        if (splitter01 < splitter02)
                        {
                            if (splitter01 > 0 && splitter01 < 100 && splitter02 > 0 && splitter02 < 100)
                                splitterList.Add(new Tuple<int, int>(splitter01, splitter02));
                        }
                    }
                }
                else MessageBox.Show(@"Cannot parse value at 2st textBox. Value have to be int, less then 0" +
                     @"and greater then 0");
            }
            if (mainWindow.CheckBoxPersentage2.IsChecked == true)
            {
                int splitter01;
                if (int.TryParse(mainWindow.splitterPersentage301.Text, out splitter01))
                {
                    int splitter02;
                    if (int.TryParse(mainWindow.splitterPersentage302.Text, out splitter02))
                    {
                        if (splitter01 < splitter02)
                        {
                            if (splitter01 > 0 && splitter01 < 100 && splitter02 > 0 && splitter02 < 100)
                                splitterList.Add(new Tuple<int, int>(splitter01, splitter02));
                        }
                    }
                }
                else MessageBox.Show(@"Cannot parse value at 3st textBox. Value have to be int, less then 0" +
                     @"and greater then 0");
            }
            if (mainWindow.CheckBoxPersentage3.IsChecked == true)
            {
                int splitter01;
                if (int.TryParse(mainWindow.splitterPersentage401.Text, out splitter01))
                {
                    int splitter02;
                    if (int.TryParse(mainWindow.splitterPersentage402.Text, out splitter02))
                    {
                        if (splitter01 < splitter02)
                        {
                            if (splitter01 > 0 && splitter01 < 100 && splitter02 > 0 && splitter02 < 100)
                                splitterList.Add(new Tuple<int, int>(splitter01, splitter02));
                        }
                    }
                }
                else MessageBox.Show(@"Cannot parse value at 4st textBox. Value have to be int, less then 0" +
                     @"and greater then 0");
            }

            return splitterList;
        }
    }
}