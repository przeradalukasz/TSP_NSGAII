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
        public static List<Town> Towns { get; set; } 

        private static readonly Random rnd = new Random(DateTime.Now.Millisecond);
        
        static void Main()
        {
            //Towns = Utils.LoadTownsDataFromCsv(@"C:\dj.csv").ToList();

            Towns = Utils.LoadTownsDataFromJson(@"C:\TSPData\towns.json");
            FuzzyNumber[,] fuzzyAdjacencyMatrix = Utils.LoadFuzzyDistanceDataFromJson(@"C:\TSPData\adjacencyMatrix.json");
            AdjacencyMatrix = Utils.Defuzzification(fuzzyAdjacencyMatrix);

            //AdjacencyMatrix = Utils.CalculateDistanceMatrix(Towns);

            Population population = new Population(AdjacencyMatrix, Towns.ToArray(), 0.05, 0.95, 10, 500, rnd);
            
            while (population.Generations < 150)
            {
                population.NaturalSelection();
                population.Generate();
            }
            population.NaturalSelection();

            Utils.SaveResultsToCsv(@"C:\Users\ronal_000\Desktop\plox.csv", population);
            
            
            Path bestDistance = population.Fronts[0].OrderBy(x => x.Distance).First();
            Path bestUnbalancing = population.Fronts[0].OrderBy(x => x.UnbalancingDegree).First();


            Utils.DrawResultPath(@"C:\Users\ronal_000\Desktop\dj2.bmp", bestDistance.Towns);

            Console.ReadLine();

        }
    }
}
