using System.Collections.Generic;

namespace WPFImageAnalyzer
{
    public class RgbCortage
    {
        public List<List<Rgb>> Container { get; set; }

        public RgbCortage()
        {
            Container = new List<List<Rgb>>();
        }

        public void RegisterNewContainer(IEnumerable<Rgb> container)
        {
            var nc = new List<Rgb>();
            nc.AddRange(container);
            Container.Add(nc);
        }
    }
}