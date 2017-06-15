using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics.Integration;

using Newtonsoft.Json;

namespace TSP_NSGAII
{
    public static class Utils
    {
        public static Town[] LoadTownsDataFromCsv(string path)
        {
            var towns = new Town[TotalLines(path)];
            using (var fs = File.OpenRead(path))
            using (var reader = new StreamReader(fs))
            {
                int i = 0;
                while (!reader.EndOfStream)
                {

                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    towns[i] = new Town(Int32.Parse(values[0]), Double.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture), Double.Parse(values[2], System.Globalization.CultureInfo.InvariantCulture));
                    i++;
                }
            }
            return towns;
        }

        public static List<Town> LoadTownsDataFromJson(string path)
        {
            List<Town> towns;
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                towns = JsonConvert.DeserializeObject<List<Town>>(json);
            }
            return towns;
        }

        public static double[,] CalculateDistanceMatrix(List<Town> towns)
        {
            var adjacencyMatrix = new double[towns.Count + 1, towns.Count + 1];
            
            for (int i = 1; i <= towns.Count; i++)
            {
                for (int j = 1; j <= towns.Count; j++)
                {
                    adjacencyMatrix[i, j] = towns[i - 1].DistanceTo(towns[j - 1]);
                }
            }
            return adjacencyMatrix;;
        }


        internal static void DrawResultPath(string path, Town[] towns)
        {

            int minX = (int)towns.Min(e => e.X);
            int minY = (int)towns.Min(e => e.Y);
            int maxX = (int)towns.Max(e => e.X);
            int maxY = (int)towns.Max(e => e.Y);
            Bitmap bmp = new Bitmap(maxX - minX + 10, maxY - minY + 10);

            Pen blackPen = new Pen(Color.Black, 3);
            SolidBrush redBrush = new SolidBrush(Color.Red);

            using (var graphics = Graphics.FromImage(bmp))
            {
                for (int i = 0; i < towns.Length; i++)
                {
                    //bmp.SetPixel((int)best.Towns[i].X - minX, (int)best.Towns[i].Y - minY, Color.Black);
                    graphics.FillRectangle(redBrush, (int)towns[i].X - minX - 5, (int)towns[i].Y - 5 - minY, 10, 10);
                    if (i != 0)
                    {
                        graphics.DrawLine(blackPen, (int)towns[i].X - minX, (int)towns[i].Y - minY, (int)towns[i - 1].X - minX, (int)towns[i - 1].Y - minY);
                    }
                }
            }

            bmp.Save(path);
        }

        public static void SaveResultsToCsv2(string path, Path[] paths)
        {
            var csv = new StringBuilder();

            foreach (var pathEl in paths)
            {

                var first = Math.Round(pathEl.Distance, 0);
                var second = Math.Round(pathEl.UnbalancingDegree, 0);

                var newLine = string.Format("{0};{1}", first, second);
                csv.AppendLine(newLine);
            }
            File.WriteAllText(path, csv.ToString());
        }

        public static void SaveResultsToCsv(string path, Population population)
        {
            var csv = new StringBuilder();

            foreach (var populationChild in population.Children)
            {

                var first = populationChild.Distance;
                var second = populationChild.UnbalancingDegree;

                var newLine = string.Format("{0};{1}", first, second);
                csv.AppendLine(newLine);
            }
            File.WriteAllText(path, csv.ToString());
        }
        

        private static int TotalLines(string filePath)
        {
            using (StreamReader r = new StreamReader(filePath))
            {
                int i = 0;
                while (r.ReadLine() != null) { i++; }
                return i;
            }
        }

        public static FuzzyNumber[,] LoadFuzzyDistanceDataFromJson(string path)
        {
            FuzzyNumber[,] adjMatrix;
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                adjMatrix = JsonConvert.DeserializeObject<FuzzyNumber[,]>(json);
            }
            return adjMatrix;
        }

        public static double[,] LoadCrispDistanceDataFromJson(string path)
        {
            double[,] adjMatrix;
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                adjMatrix = JsonConvert.DeserializeObject<double[,]>(json);
            }
            return adjMatrix;
        }

        public static double[,] Defuzzification(FuzzyNumber[,] fuzzyAdjacencyMatrix)
        {
            //var adjacencyMatrix = new double[fuzzyAdjacencyMatrix.GetLength(1), fuzzyAdjacencyMatrix.GetLength(1)];

            //for (int i = 1; i < fuzzyAdjacencyMatrix.GetLength(1); i++)
            //{
            //    for (int j = i; j < fuzzyAdjacencyMatrix.GetLength(1); j++)
            //    {
            //        adjacencyMatrix[i, j] = adjacencyMatrix[j, i] = fuzzyAdjacencyMatrix[i, j].M;
            //    }
            //}
            //return adjacencyMatrix; 
            var adjacencyMatrix = new double[fuzzyAdjacencyMatrix.GetLength(1), fuzzyAdjacencyMatrix.GetLength(1)];

            for (int i = 1; i < fuzzyAdjacencyMatrix.GetLength(1); i++)
            {
                for (int j = i; j < fuzzyAdjacencyMatrix.GetLength(1); j++)
                {
                    adjacencyMatrix[i, j] = adjacencyMatrix[j, i] = CalculateStrictValue(fuzzyAdjacencyMatrix[i, j]);
                }
            }
            return adjacencyMatrix;
        }

        public static double CalculateStrictValue(FuzzyNumber fuzzyNumber)
        {
            var leftX = fuzzyNumber.M - fuzzyNumber.L;
            var leftValue = fuzzyNumber.L;
            var rightX = fuzzyNumber.U - fuzzyNumber.M;
            var rightValue = fuzzyNumber.U;

            var finalX = leftX - rightX;
            var finalValue = rightValue + leftValue;
            return SimpsonRule.IntegrateThreePoint(x => 0.5*(finalX * x + finalValue), 0.0, 1.0);
            
        }

        public static object CalculateAverageDistanceAll(double[,] adjacencyMatrix)
        {
            double result = 0;
            List<double> distances = new List<double>();
            for (int i = 1; i < adjacencyMatrix.GetLength(1); i++)
            {
                for (int j = i; j < adjacencyMatrix.GetLength(1); j++)
                {
                    distances.Add(adjacencyMatrix[i, j]);
                }
            }
            double average = distances.Average();



            return average;
        }

        public static double CalculateStandardDeviationAll(double[,] adjacencyMatrix)
        {
            List<double> distances = new List<double>();
            for (int i = 1; i < adjacencyMatrix.GetLength(1); i++)
            {
                for (int j = i; j < adjacencyMatrix.GetLength(1); j++)
                {
                    distances.Add(adjacencyMatrix[i, j]);
                }
            }
            double average = distances.Average();
            double sumOfSquaresOfDifferences = distances.Select(val => (val - average) * (val - average)).Sum();
            double sd = Math.Sqrt(sumOfSquaresOfDifferences / distances.Count);


            return sd;
        }

        public static double CalculateStandardDeviation(double[,] adjacencyMatrix, List<Town> towns)
        {
            List<double> distances = new List<double>();
            foreach (var town in towns)
            {
                foreach (var town1 in towns)
                {
                    distances.Add(adjacencyMatrix[town.Id, town1.Id]);
                }
            }
            double average = distances.Average();
            double sumOfSquaresOfDifferences = distances.Select(val => (val - average) * (val - average)).Sum();
            double sd = Math.Sqrt(sumOfSquaresOfDifferences / distances.Count);


            return sd;
        }

        public static object CalculateAverageDistance(double[,] adjacencyMatrix, List<Town> towns)
        {
            double result = 0;
            List<double> distances = new List<double>();
            foreach (var town in towns)
            {
                foreach (var town1 in towns)
                {
                    distances.Add(adjacencyMatrix[town.Id, town1.Id]);
                }
            }
            double average = distances.Average();
            


            return average;
        }

        public static List<Town> FilterByCounty(List<Town> towns, string[] countyName)
        {
            return towns.Where(town => countyName.Contains(town.County)).ToList();
        }

        public static List<Town> OrderTowns(List<Town> towns)
        {
            for (int i = 0; i < towns.Count; i++)
            {
                towns[i].Id = i+1;
            }
            return towns;
        }

        public static double[,] ChangeToMinutes(double[,] adjacencyMatrix)
        {
            for (int i = 0; i < adjacencyMatrix.GetLength(1); i++)
            {
                for (int j = 0; j < adjacencyMatrix.GetLength(1); j++)
                {
                    adjacencyMatrix[i, j] = Math.Round(adjacencyMatrix[i, j] / 60,2);
                }
            }
            return adjacencyMatrix;
        }


        internal static double[,] CutUnnecesssaryElements(double[,] adjacencyMatrix, List<Town> towns)
        {
            var newAdjacencyMatrix = new double[towns.Count + 1, towns.Count + 1];
            int i = 1;
            foreach (var town1 in towns)
            {
                int j = 1;
                foreach (var town2 in towns)
                {

                    newAdjacencyMatrix[i, j] = adjacencyMatrix[town1.Id, town2.Id];
                    j++;
                }
                i++;
            }


           
            return newAdjacencyMatrix; 
        }
    }
}
