using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_NSGAII
{
    public class Town
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public Town(int id, double x, double y)
        {
            Id = id;
            X = x;
            Y = y;
        }

        public Town(Town town)
        {
            Id = town.Id;
            X = town.X;
            Y = town.Y;
        }

        public double DistanceTo(Town to)
        {
            if (this == to)
            {
                return 0;
            }
            return Math.Sqrt(Math.Pow((to.X - X), 2)
                            + Math.Pow((to.Y - Y), 2));
        }
    }
}

