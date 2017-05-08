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
        public static double[,] AdjacencyMatrix { get; set; } //find towns according to their 1-indexed ids
        public static Town[] Towns { get; set; } //0-indexed!

        private static readonly Random rnd = new Random(DateTime.Now.Millisecond);

        public static List<List<Path>> fronts = new List<List<Path>>();
        static void Main()
        {
            //double start = System.();

            // load the town information from csv file using the given FileIO class
            Towns = new Town[TotalLines(@"C:\dj.csv")];
            PictureBox pp = new PictureBox();
            using (var fs = File.OpenRead(@"C:\dj.csv"))
            using (var reader = new StreamReader(fs))
            {
                int i = 0;
                while (!reader.EndOfStream)
                {

                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    Towns[i] = new Town(Int32.Parse(values[0]), Double.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture), Double.Parse(values[2], System.Globalization.CultureInfo.InvariantCulture));
                    i++;
                }
            }

            // and make the adjacency matrix of distances

            AdjacencyMatrix = new double[Towns.Length + 1, Towns.Length + 1];
            //want the indices to correspond to the ids
            for (int i = 1; i <= Towns.Length; i++)
            {
                for (int j = 1; j <= Towns.Length; j++)
                {
                    AdjacencyMatrix[i, j] = Towns[i - 1].DistanceTo(Towns[j - 1]);
                }
            }

            //it's a random process so let's run it this many times and take the best answer
            int runs = 1;

            //save the best path from each run
            Path[] bests = new Path[runs];

            //a population needs to know the towns and the matrix
            //mutation rate, population size
            Population population = new Population(AdjacencyMatrix, Towns, 0.05, 500, rnd);


            //haven't found a better route in 100 generations, probably won't
            while (population.Generations < 500)
            {

                // Generate mating pool
                population.NaturalSelection();
                //Create next generation
                population.Generate();
                // Calculate fitness
                //
            }
            population.NaturalSelection();


            //find the best of the best
            Path best = population.Fronts[0].OrderBy(x => x.Distance).First();
            Console.WriteLine("\nbests [0]: " + best.Distance);

            for (int i = 1; i < runs; i++)
            {
                Console.WriteLine("bests [" + i + "]: " + bests[i].Distance);
                if (bests[i].Distance < best.Distance)
                {
                    best = bests[i];
                }
            }

            Console.WriteLine("\ntotal best: \n" + best.MakeString() + "\nwith a distance of: " + best.Distance);

            DrawResults(best);

            Console.ReadLine();

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

        private static void DrawResults(Path best)
        {
            int minX = (int)best.Towns.Min(e => e.X);
            int minY = (int)best.Towns.Min(e => e.Y);
            int maxX = (int)best.Towns.Max(e => e.X);
            int maxY = (int)best.Towns.Max(e => e.Y);
            Bitmap bmp = new Bitmap(maxX - minX + 10, maxY - minY + 10);

            Pen blackPen = new Pen(Color.Black, 3);
            SolidBrush redBrush = new SolidBrush(Color.Red);

            using (var graphics = Graphics.FromImage(bmp))
            {
                for (int i = 0; i < best.Towns.Length; i++)
                {
                    //bmp.SetPixel((int)best.Towns[i].X - minX, (int)best.Towns[i].Y - minY, Color.Black);
                    graphics.FillRectangle(redBrush, (int)best.Towns[i].X - minX - 5, (int)best.Towns[i].Y - 5 - minY, 10, 10);
                    if (i != 0)
                    {
                        graphics.DrawLine(blackPen, (int)best.Towns[i].X - minX, (int)best.Towns[i].Y - minY, (int)best.Towns[i - 1].X - minX, (int)best.Towns[i - 1].Y - minY);
                    }
                }
            }

            bmp.Save(@"C:\Users\ronal_000\Desktop\dj2.bmp");
        }
    }
}
