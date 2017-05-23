using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_NSGAII
{
    public class FuzzyNumber
    {
        public double L { get; set; }
        public double M { get; set; }
        public double U { get; set; }

        public FuzzyNumber(double l, double m, double u)
        {
            L = l;
            M = m;
            U = u;
        }
        public FuzzyNumber()
        { }
    }
}
