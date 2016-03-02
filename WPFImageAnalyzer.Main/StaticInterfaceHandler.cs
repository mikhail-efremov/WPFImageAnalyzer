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
        public static int[] GetSplitterArray(MainWindow mainWindow)
        {
            var splitterList = new List<int>();

            if (mainWindow.checkBoxPersentage.IsChecked == true)
            {
                int splitter;
                if (Int32.TryParse(mainWindow.splitterPersentage.Text, out splitter))
                {
                    if (splitter > 0 && splitter < 100)
                        splitterList.Add(splitter);
                }
                else MessageBox.Show("Cannot parse value at 1st textBox. Value have to be int, less then 0" +
                     "and greater then 0");
            }
            if (mainWindow.checkBoxPersentage1.IsChecked == true)
            {
                int splitter;
                if (Int32.TryParse(mainWindow.splitterPersentage1.Text, out splitter))
                {
                    if (splitter > 0 && splitter < 100)
                        splitterList.Add(splitter);
                }
                else MessageBox.Show("Cannot parse value at 2st textBox. Value have to be int, less then 0" +
                     "and greater then 0");
            }
            if (mainWindow.checkBoxPersentage2.IsChecked == true)
            {
                int splitter;
                if (Int32.TryParse(mainWindow.splitterPersentage2.Text, out splitter))
                {
                    if (splitter > 0 && splitter < 100)
                        splitterList.Add(splitter);
                }
                else MessageBox.Show("Cannot parse value at 3st textBox. Value have to be int, less then 0" +
                     "and greater then 0");
            }
            if (mainWindow.checkBoxPersentage3.IsChecked == true)
            {
                int splitter;
                if (Int32.TryParse(mainWindow.splitterPersentage3.Text, out splitter))
                {
                    if (splitter > 0 && splitter < 100)
                        splitterList.Add(splitter);
                }
                else MessageBox.Show("Cannot parse value at 4st textBox. Value have to be int, less then 0" +
                     "and greater then 0");
            }

            return splitterList.ToArray();
        }
    }
}