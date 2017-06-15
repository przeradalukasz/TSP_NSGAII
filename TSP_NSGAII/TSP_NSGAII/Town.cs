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
        public string State { get; set; }
        public string County { get; set; }

        public Town(int id, double x, double y, string name, string state, string county)
        {
            Id = id;
            X = x;
            Y = y;
            State = state;
            County = county;
            Name = name;
        }

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

        public Town()
        { }

        public double DistanceTo(Town to)
        {
            //if (this == to)
            //{
            //    return 0;
            //}
            //return Math.Sqrt(Math.Pow((to.X - X), 2)
            //                + Math.Pow((to.Y - Y), 2

            double theta = Y - to.Y;
            double dist = Math.Sin(Deg2Rad(Y)) * Math.Sin(Deg2Rad(to.Y)) + Math.Cos(Deg2Rad(Y)) * Math.Cos(Deg2Rad(to.Y)) * Math.Cos(Deg2Rad(theta));
            dist = Math.Acos(dist);
            dist = Rad2Deg(dist);
            return dist * 60 * 1.1515;
        }
        private static double Deg2Rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        private static double Rad2Deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }


    }
}

