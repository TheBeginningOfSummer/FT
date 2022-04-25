using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT.Data
{
    public class Position
    {
        public int X;
        public int Y;

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Position()
        {

        }
    }

    public class TypeOfTray
    {
        public int Index { get; set; }
        public string TrayType { get; set; }
        public int Length { get; set; }
        public int Width { get; set; }
    }
}
