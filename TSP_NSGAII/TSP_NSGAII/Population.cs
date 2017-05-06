using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_NSGAII
{
    public class Population
    {
        private ArrayList matingPool;
        private double mutationRate;
        public Path[] Pop { get; set; }
        public Path[] Children { get; set; }

        public int Generations;
        public int SinceChange;
        public Path AllTime;
        public double[,] AdjacencyMatrix;
        public Town[] Towns;

        Random rnd;

        //This is called by the main method
        public Population(double[,] adjacencyMatrix, Town[] towns, double mutationRate, int num, Random random)
        {
            this.mutationRate = mutationRate;
            AdjacencyMatrix = adjacencyMatrix;
            Towns = towns;
            rnd = random;
            Pop = new Path[num]; //array - data structure to store the paths
            Children = new Path[num];
            for (int i = 0; i < num; i++)
            {
                //populate with random paths
                Path temp = new Path(adjacencyMatrix, towns, rnd);
                temp.RandomPath();
                Pop[i] = temp;
            }
            for (int i = 0; i < num; i++)
            {
                //populate with random paths
                Path temp = new Path(adjacencyMatrix, towns, rnd);
                temp.RandomPath();
                Children[i] = temp;
            }

            SinceChange = 0;
            CalcFitness(); //called here rather than the main method on initialisation

            matingPool = new ArrayList();
            AllTime = new Path();
            AllTime.Distance = 100000; //abnormally high number

        }



        //Fill the mating pool according to each Path's relative fitness.
        //The all-time best Path is also added to make sure the mating pool has at least one instance of it.
        public void NaturalSelection()
        {
            Pop = Pop.Concat(Children).ToArray();

            CalcFitness();


            // Clear the ArrayList
            matingPool.Clear();

            //always choose the best from the previous generation
            Path generation_best = new Path();

            double maxFitness = 0;
            double totalFitness = 0.0;

            double gen_best = 100000; //some big number

            //getting the relative fitness of each path to another
            for (int i = 0; i < Pop.Length; i++)
            {

                totalFitness += Pop[i].FitnessDistance;

                //update the generation best path while we're at it
                if (gen_best > Pop[i].Distance)
                {
                    generation_best = Pop[i];
                    gen_best = generation_best.Distance;
                }

                //the biggest fitness value of any generation must be kept
                if (Pop[i].FitnessDistance > maxFitness)
                {
                    maxFitness = Pop[i].FitnessDistance;
                }
            }


            //we've found a new best from all generations, so print it
            if (generation_best.Distance < AllTime.Distance)
            {
                AllTime = generation_best;
                Console.WriteLine("new best: " + AllTime.Distance + " after " + Generations);
                SinceChange = 0;
            }
            else
            {
                SinceChange++;
            }

            // Based on fitness, each member will get added to the mating pool a certain number of times
            // a higher fitness = more entries to mating pool = more likely to be picked as a parent
            // a lower fitness = fewer entries to mating pool = less likely to be picked as a parent
            for (int i = 0; i < Pop.Length; i++)
            {

                double fitness = Pop[i].FitnessDistance / maxFitness;

                int n = (int)(fitness * 1000);          // Arbitrary multiplier
                for (int j = 0; j < n; j++)
                {         // and pick two random numbers
                    matingPool.Add(Pop[i]);
                }
            }

        }

        public void Generate()
        {

            //making sure the generation doesn't get any worse
            Pop[0] = AllTime;

            // Refill the population with children from the mating pool
            for (int i = 1; i < Pop.Length; i++)
            {

                int a = (int)(rnd.NextDouble() * matingPool.Count);
                int b = (int)(rnd.NextDouble() * matingPool.Count);
                Path partnerA = (Path)matingPool[a];
                Path partnerB = (Path)matingPool[b];
                Path child = new Path(AdjacencyMatrix, Towns, partnerA.CrossOver(partnerB), rnd);
                //System.out.println("crossing "+partnerA.distance+" with "+partnerB.distance);
                //if(Math.random()<0.5){
                child.Mutate4(mutationRate);
                //}else{
                //	child.mutate(mutationRate);
                //}
                Pop[i] = child;

            }
            Generations++;

        }

        public void CalcFitness()
        {
            CalcFitnessFirstObj();
            //CalcFitnessSecondObj();
        }

        //very important for the makeup of the mating pool
        public void CalcFitnessFirstObj()
        {

            //get unique distances of Paths in the population
            ArrayList dists = new ArrayList();
            for (int i = 0; i < Pop.Length; i++)
            {
                double dist = 1 / Pop[i].Distance;
                if (!dists.Contains(dist))
                {
                    dists.Add(dist);
                }
            }

            //convert arraylist to array
            ArrayList ascendingDists = new ArrayList(dists.Count);
            for (int i = 0; i < ascendingDists.Capacity; i++)
            {
                double temp = (double)dists[i];
                ascendingDists.Add(temp);
            }
            ascendingDists.Sort();
            //ascendingDists.Reverse();//sort to ascending order

            for (int i = 0; i < Pop.Length; i++)
            {

                for (int j = 0; j < ascendingDists.Count; j++)
                {
                    double lol = Math.Abs((1 / Pop[i].Distance) - (double)ascendingDists[j]);
                    if (lol < 0.0000000000000000000001)
                    {
                        //fitness is successive integers raised to the power of 4.
                        //need to keep distance between better Paths to reward difference
                        //at the higher end.
                        Pop[i].FitnessDistance = Math.Pow(j, 4);
                        break;
                    }
                }

            }
        }


        public void CalcFitnessSecondObj()
        {

            //get unique distances of Paths in the population
            ArrayList dists = new ArrayList();
            for (int i = 0; i < Pop.Length; i++)
            {
                double dist = 1 / Pop[i].Distance;
                if (!dists.Contains(dist))
                {
                    dists.Add(dist);
                }
            }

            //convert arraylist to array
            ArrayList ascendingDists = new ArrayList(dists.Count);
            for (int i = 0; i < ascendingDists.Capacity; i++)
            {
                double temp = (double)dists[i];
                ascendingDists.Add(temp);
            }
            ascendingDists.Sort();
            //ascendingDists.Reverse();//sort to ascending order

            for (int i = 0; i < Pop.Length; i++)
            {

                for (int j = 0; j < ascendingDists.Count; j++)
                {
                    double lol = Math.Abs((1 / Pop[i].Distance) - (double)ascendingDists[j]);
                    if (lol < 0.0000000000000000000001)
                    {
                        //fitness is successive integers raised to the power of 4.
                        //need to keep distance between better Paths to reward difference
                        //at the higher end.
                        Pop[i].FitnessUnbalancingDegree = Math.Pow(j, 4);
                        break;
                    }
                }

            }
        }

    }
}
