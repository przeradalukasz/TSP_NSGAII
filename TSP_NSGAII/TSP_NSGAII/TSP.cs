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

            Towns = Utils.LoadTownsDataFromJson(@"C:\MagisterkaDane\Dane\Sizes\100\average.json");
            
            //AdjacencyMatrix = Utils.CalculateDistanceMatrix(Towns);

            //Towns = Utils.FilterByCounty(Towns,new []{ "Rockingham", "Cheshire", "Merrimack",  "Belknap", "Hillsborough", "Strafford", "Sullivan", "Carroll", "Coos" });
            //Towns = Towns.Take(202).ToList();



            AdjacencyMatrix = Utils.LoadCrispDistanceDataFromJson(@"C:\MagisterkaDane\Dane\crispAdjacencyMatrixNewHampshire.json");
            AdjacencyMatrix = Utils.CutUnnecesssaryElements(AdjacencyMatrix,Towns);
            Towns = Utils.OrderTowns(Towns);



            //var averageDistanceAll = Utils.CalculateAverageDistanceAll(AdjacencyMatrix);
            //var standardDeviationAll = Utils.CalculateStandardDeviationAll(AdjacencyMatrix);
            //var averageDistance = Utils.CalculateAverageDistance(AdjacencyMatrix, Towns);
            //var standardDeviation = Utils.CalculateStandardDeviation(AdjacencyMatrix, Towns);

            //var count1 = Towns.Count(town => town.County.Equals("Rockingham"));
            //var count2 = Towns.Count(town => town.County.Equals("Cheshire"));
            //var count3 = Towns.Count(town => town.County.Equals("Merrimack"));
            //var count4 = Towns.Count(town => town.County.Equals("Hillsborough"));
            //var count5 = Towns.Count(town => town.County.Equals("Belknap"));
            //var count6 = Towns.Count(town => town.County.Equals("Strafford"));
            //var count7 = Towns.Count(town => town.County.Equals("Sullivan"));
            //var count8 = Towns.Count(town => town.County.Equals("Carroll"));
            //var count9 = Towns.Count(town => town.County.Equals("Coos"));


            //string TownsJson = JsonConvert.SerializeObject(Towns, Formatting.Indented);
            //File.WriteAllText(@"C:\MagisterkaDane\Dane\lol123.json", TownsJson);
            List<Path> allTimeBest = new List<Path>();
            List<Path> allTimeBestFirstFront = new List<Path>();
            Population population = new Population(AdjacencyMatrix, Towns.ToArray(), 0.05, 0.95, 10, 1000, rnd);
            for (int i = 0; i < 10; i++)
            {
                population = new Population(AdjacencyMatrix, Towns.ToArray(), 0.05, 0.95, 10, 1000, rnd);
                Console.WriteLine("Iteration: " + i + 1);
                while (population.Generations < 200)
                {
                    population.NaturalSelection();
                    population.Generate();
                    Console.WriteLine("Gen" + population.Generations);
                }
                population.NaturalSelection();
                allTimeBest.AddRange(population.Fronts[0]);
                
            }

            allTimeBestFirstFront = population.GetNondominatedIndividuals(allTimeBest.ToArray());



            Utils.SaveResultsToCsv2(@"C:\MagisterkaDane\Wyniki\allTimeBestPareto.csv", allTimeBest.ToArray());
            Utils.SaveResultsToCsv2(@"C:\MagisterkaDane\Wyniki\allTimeBestFirstFrontPareto.csv", allTimeBestFirstFront.ToArray());

            


            //Utils.DrawResultPath(@"C:\MagisterkaDane\Wyniki\bestDistanceDrawn.bmp", bestDistance.Towns);
            Console.WriteLine("Press any key...");
            Console.ReadLine();

        }
    }
}
