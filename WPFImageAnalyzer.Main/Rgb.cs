namespace WPFImageAnalyzer
{
    public class Rgb
    {
        public byte A;
        public byte R;
        public byte G;
        public byte B;
        public double X;
        public double Y;
        public double Z;
        public int Possition;
        public int SelectedIndex;

        public Rgb(byte a, byte r, byte g, byte b, double x, double y)
        {
            A = a;
            R = r;
            G = g;
            B = b;

            X = x;
            Y = y;
        }

        public Rgb(byte a, byte r, byte g, byte b, double x, double y, double z, int poss)
        {
            A = a;
            R = r;
            G = g;
            B = b;

            X = x;
            Y = y;
            Z = z;

            Possition = poss;
        }

        public override string ToString()
        {
            return $"R:{R} G:{G} B{B}";
        }
    }
}