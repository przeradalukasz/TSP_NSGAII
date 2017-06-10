using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace TSP_NSGAII
{
    public class TSP
    {
        public static double[,] AdjacencyMatrix { get; set; } 
        public static List<Town> Towns { get; set; } 

        private static readonly Random rnd = new Random(DateTime.Now.Millisecond);
        
        static void Main()
        {
            //Towns = Utils.LoadTownsDataFromCsv(@"C:\MagisterkaDane\djj.csv").ToList();
            //AdjacencyMatrix = Utils.CalculateDistanceMatrix(Towns);

            //FuzzyNumber[,] fuzzyAdjacencyMatrix = Utils.LoadFuzzyDistanceDataFromJson(@"C:\MagisterkaDane\Dane\adjacencyMatrixNewHampshire.json");
            //AdjacencyMatrix = Utils.Defuzzification(fuzzyAdjacencyMatrix);
            //string adjacencyMatrixJson = JsonConvert.SerializeObject(AdjacencyMatrix, Formatting.Indented);
            //File.WriteAllText(@"C:\MagisterkaDane\Dane\StrictadjacencyMatrixNewHampshire.json", adjacencyMatrixJson);
            //Belknap Carroll Cheshire Coos Grafton Hillsborough Merrimack Rockingham Strafford Sullivan 
            Towns = Utils.LoadTownsDataFromJson(@"C:\MagisterkaDane\Dane\newHampshireCities.json");

            Towns = Utils.FilterByCounty(Towns,new []{ "Cheshire", "Hillsborough" });

            AdjacencyMatrix = Utils.LoadCrispDistanceDataFromJson(@"C:\MagisterkaDane\Dane\crispAdjacencyMatrixNewHampshire.json");
            var averageDistanceAll = Utils.CalculateAverageDistanceAll(AdjacencyMatrix);
            var standardDeviationAll = Utils.CalculateStandardDeviationAll(AdjacencyMatrix);
            var averageDistance = Utils.CalculateAverageDistance(AdjacencyMatrix, Towns);
            var standardDeviation = Utils.CalculateStandardDeviation(AdjacencyMatrix, Towns);

            Population population = new Population(AdjacencyMatrix, Towns.ToArray(), 0.05, 0.95, 10, 500, rnd);
            
            while (population.Generations < 10)
            {
                population.NaturalSelection();
                population.Generate();
                Console.WriteLine("Gen" + population.Generations);
            }
            population.NaturalSelection();

            Utils.SaveResultsToCsv2(@"C:\Users\lprzerad\Desktop\Results\results.csv", population.Fronts[0].ToArray());
            
            
            Path bestDistance = population.Fronts[0].OrderBy(x => x.Distance).First();
            Path bestUnbalancing = population.Fronts[0].OrderBy(x => x.UnbalancingDegree).First();


            Utils.DrawResultPath(@"C:\Users\lprzerad\Desktop\Results\bestDistanceDrawn.bmp", bestDistance.Towns);
            Console.WriteLine("Press any key...");
            Console.ReadLine();

        }
    }
}
