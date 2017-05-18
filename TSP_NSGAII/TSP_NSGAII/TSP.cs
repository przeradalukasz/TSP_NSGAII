using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TSP_NSGAII
{
    public class TSP
    {
        public static double[,] AdjacencyMatrix { get; set; } 
        public static Town[] Towns { get; set; } 

        private static readonly Random rnd = new Random(DateTime.Now.Millisecond);
        
        static void Main()
        {
            Towns = Utils.LoadTownsData(@"C:\dj.csv");

            AdjacencyMatrix = Utils.LoadDistanceData(Towns);

            Population population = new Population(AdjacencyMatrix, Towns, 0.05, 0.95, 10, 500, rnd);
            
            while (population.Generations < 50)
            {
                population.NaturalSelection();
                population.Generate();
            }
            population.NaturalSelection();

            Utils.SaveResultsToCsv(@"C:\Users\ronal_000\Desktop\plox.csv", population);
            
            
            Path best = population.Fronts[0].OrderBy(x => x.Distance).First();
            
           
            Utils.DrawResultPath(@"C:\Users\ronal_000\Desktop\dj2.bmp", best.Towns);

            Console.ReadLine();

        }
    }
}
