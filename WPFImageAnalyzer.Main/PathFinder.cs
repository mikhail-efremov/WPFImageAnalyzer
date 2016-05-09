using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFImageAnalyzer
{
    public class PathFinder
    {
        public void Find(IList<Rgb> rgbList)
        {
            var anchors = FindAnchors(rgbList, 50);
            var wrap = RgbLeeWrap.Convert(rgbList);

            var tdaWrap = ToTwoDimensionalArray(wrap);


            for (var i = 0; i < anchors.Count; i++)
            {
                var x = (int) anchors[i].X;
                var y = (int) anchors[i].Y;

                tdaWrap[x, y].Weight = 0;
                tdaWrap[x, y].Start = true;

                var done = false;

                FindPath(tdaWrap, x, y);
            }
            
            Console.Write("");
        }

        private bool FindPath(RgbLeeWrap[,] tdaWrap, int x, int y)
        {
            var done = false;
/*
            while (!done)
            {
                if (tdaWrap[x - 1, y].SelectedIndex == 1)
                {
                    tdaWrap[x - 1, y].Weight++;
                }
                if (tdaWrap[x + 1, y].SelectedIndex == 1)
                {
                    tdaWrap[x + 1, y].Weight++;
                }
                if (tdaWrap[x, y - 1].SelectedIndex == 1)
                {
                    tdaWrap[x, y - 1].Weight++;
                }
                if (tdaWrap[x, y + 1].SelectedIndex == 1)
                {
                    tdaWrap[x, y + 1].Weight++;
                }
            }
*/
            bool add = true;

            int width = tdaWrap.GetLength(0);
            int height = tdaWrap.GetLength(1);
            var step = 0;

         //   while (add == true)
            {
                bool first = true;
                add = false;
                var controller = 0;
                for (x = 0; x < width - 1; x++)
                {
                    for (y = 0; y < height - 1; y++)
                    {
                        if (tdaWrap[x, y].Weight == step && tdaWrap[x, y].SelectedIndex == 1)
                        {
                            if (tdaWrap[x - 1, y].SelectedIndex == 1)
                            {
                                if(tdaWrap[x - 1, y].Weight == -1)
                                    tdaWrap[x - 1, y].Weight += 2;
                                else
                                {
                                    tdaWrap[x - 1, y].Weight++;
                                }
                                controller++;
                            }
                            if (tdaWrap[x + 1, y].SelectedIndex == 1)
                            {
                                if (tdaWrap[x + 1, y].Weight == -1)
                                    tdaWrap[x + 1, y].Weight += 2;
                                else
                                {
                                    tdaWrap[x + 1, y].Weight++;
                                }
                                controller++;
                            }
                            if (tdaWrap[x, y - 1].SelectedIndex == 1)
                            {
                                if (tdaWrap[x, y - 1].Weight == -1)
                                    tdaWrap[x, y - 1].Weight += 2;
                                else
                                {
                                    tdaWrap[x, y - 1].Weight++;
                                }
                                controller++;
                            }
                            if (tdaWrap[x, y + 1].SelectedIndex == 1)
                            {
                                if (tdaWrap[x, y + 1].Weight == -1)
                                    tdaWrap[x, y + 1].Weight += 2;
                                {
                                    tdaWrap[x, y + 1].Weight++;
                                }
                                controller++;
                            }
                            if (controller != 0)
                            {
                                step++;
                                first = false;
                            }
                        }
                    }
                }
                add = true;
            }
            var arr = new List<RgbLeeWrap>();
            for (var x1 = 0; x1 < width - 1; x1++)
            {
                for (var y1 = 0; y1 < height - 1; y1++)
                {
                    if (tdaWrap[x1, y1].Weight > 0 && tdaWrap[x1, y1].Start == false && tdaWrap[x1, y1].SelectedIndex > 0)
                    {
                        tdaWrap[x1, y1].SelectedIndex = -1;
                        arr.Add(tdaWrap[x1, y1]);
                    }
                }
            }
            //maximum is 9; its bad
            return true;
        }

        private RgbLeeWrap[,] ToTwoDimensionalArray(IList<RgbLeeWrap> array)
        {
            var narray = new RgbLeeWrap[(int)array[array.Count - 1].X + 1, (int)array[array.Count - 1].Y + 1];

            for (var i = 0; i < array.Count - 1; i++)
            {
                narray[(int)array[i].X, (int)array[i].Y] = array[i];
            }
            return narray;
        }

        private IList<Rgb> FindAnchors(IList<Rgb> rgbList, int sensability)
        {
            var anchors = new List<Rgb>();

            for (var i = 0; i < rgbList.Count; i += sensability)
            {
                if (rgbList[i].SelectedIndex == 1)
                {
                    anchors.Add(rgbList[i]);
                }
            }
            return anchors;
        }

        private void LeeAlgoritm(IList<Rgb> anchorsList, IList<RgbLeeWrap> wrapList )
        {
            var wrap = PrepareWrap(anchorsList, wrapList);

            for (var i = 0; i < wrap.Count; i++)
            {
                if (wrap[i].Weight == 0)
                {

                }
            }
        }

        private IList<RgbLeeWrap> PrepareWrap(IList<Rgb> anchorsList, IList<RgbLeeWrap> wrapList)
        {
            for (var i = 0; i < anchorsList.Count; i++)
            {
                for (var j = 0; j < wrapList.Count; j++)
                {
                    if (anchorsList[i].X == wrapList[j].X && anchorsList[i].Y == wrapList[j].Y)
                    {
                        wrapList[j].Weight = 0;
                        break;
                    }
                }
            }
            return wrapList;
        }

        private class RgbLeeWrap : Rgb
        {
            public int Weight;
            public bool Start;

            private RgbLeeWrap() : base(0,0,0,0,0,0)
            {

            }

            public RgbLeeWrap(Rgb rgb) : base(rgb.A, rgb.R, rgb.G, rgb.B, rgb.X, rgb.Y, rgb.Z, rgb.Possition)
            {
                Weight = -1;
                Start = false;
            }

            public static IList<RgbLeeWrap> Convert(IList<Rgb> rgbList)
            {
                var wrap = rgbList.Select(t => new RgbLeeWrap(t)).ToList();
                for (var i = 0; i < rgbList.Count; i++)
                {
                    if (rgbList[i].SelectedIndex == 1)
                        wrap[i].SelectedIndex = 1;
                }
                return wrap;
            }
        }
    }
}

/*
 Инициализация

Пометить стартовую ячейку 0
d := 0 
Распространение волны

ЦИКЛ
  ДЛЯ каждой ячейки loc, помеченной числом d
    пометить все соседние свободные непомеченные ячейки числом d + 1
  КЦ
  d := d + 1
ПОКА (финишная ячейка не помечена) И (есть возможность распространения волны) 
Восстановление пути

ЕСЛИ финишная ячейка помечена
ТО
  перейти в финишную ячейку
  ЦИКЛ
    выбрать среди соседних ячейку, помеченную числом на 1 меньше числа в текущей ячейке
    перейти в выбранную ячейку и добавить её к пути
  ПОКА текущая ячейка — не стартовая
  ВОЗВРАТ путь найден
ИНАЧЕ
  ВОЗВРАТ путь не найден
 */
