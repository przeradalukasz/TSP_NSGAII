using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_NSGAII
{
    public class Population
    {
        private ArrayList matingPool;
        private double mutationRate;
        private int popSize;
        public Path[] Pop { get; set; }
        public Path[] Children { get; set; }

        public Path[] Parents { get; set; }

        public int Generations;
        public int SinceChange;
        
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
            Children = new Path[num];
            Parents = new Path[num];
            popSize = num;
            for (int i = 0; i < Parents.Length; i++)
            {
                //populate with random paths
                Path temp = new Path(adjacencyMatrix, towns, rnd);
                temp.RandomPath();
                Parents[i] = temp;
            }
            for (int i = 0; i < Children.Length; i++)
            {
                //populate with random paths
                Path temp = new Path(adjacencyMatrix, towns, rnd);
                temp.RandomPath();
                Children[i] = temp;
            }
            
            SinceChange = 0;
            //CalcFitness(); //called here rather than the main method on initialisation

            matingPool = new ArrayList();
        }

        //very important for the makeup of the mating pool
        public void CalcFitness()
        {
            CalcFitnessFirstObj();
            CalcFitnessSecondObj();
            
        }

        private void CalcFitnessFirstObj()
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

        private void CalcFitnessSecondObj()
        {
            //get unique distances of Paths in the population
            ArrayList degree = new ArrayList();
            for (int i = 0; i < Pop.Length; i++)
            {
                double deg = 1 / Pop[i].UnbalancingDegree;
                if (!degree.Contains(deg))
                {
                    degree.Add(deg);
                }
            }

            //convert arraylist to array
            ArrayList ascendingDegree = new ArrayList(degree.Count);
            for (int i = 0; i < ascendingDegree.Capacity; i++)
            {
                double temp = (double)degree[i];
                ascendingDegree.Add(temp);
            }
            ascendingDegree.Sort();
            //ascendingDists.Reverse();//sort to ascending order

            for (int i = 0; i < Pop.Length; i++)
            {

                for (int j = 0; j < ascendingDegree.Count; j++)
                {
                    double lol = Math.Abs((1 / Pop[i].UnbalancingDegree) - (double)ascendingDegree[j]);
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


        //Fill the mating pool according to each Path's relative fitness.
        //The all-time best Path is also added to make sure the mating pool has at least one instance of it.
        public void NaturalSelection()
        {
            Pop = Children.Concat(Children).ToArray();

            CalcFitness();

            matingPool.Clear();

            List<List<Path>> fronts = new List<List<Path>>();
            
            int i = 0;
            while (Pop.Length != 0)
            {
                List<Path> front = GetNondominatedIndividuals(Pop);
                if (front.Count == 0)
                {
                    fronts.Add(Pop.ToList());
                    break;
                }
                fronts.Add(front);
                foreach (var path in fronts[i])
                {
                    Pop = Pop.Where(val => val != path).ToArray();
                }
                i++;
            }
            int diff = popSize;
            foreach (var front in fronts)
            {
                if (front.Count <= diff)
                {
                    matingPool.AddRange(front);
                    diff = diff - front.Count;
                }
                else
                {
                    CalculateCrowdedCist(front);
                    matingPool.AddRange(front.Take(diff).ToList());
                    diff = 0;
                    break;
                }
            }
            


        }

        private void CalculateCrowdedCist(List<Path> front)
        {
            
        }

        public void Generate()
        {
            int j = 0;
            //foreach (var child in Children)
            //{
            //    Parents[j] = new Path(AdjacencyMatrix, Towns, child.Towns, rnd);
            //}
            

            // Refill the population with children from the mating pool
            for (int i = 1; i < Children.Length; i++)
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
                Children[i] = child;

            }
            Generations++;

        }

        private List<Path> GetNondominatedIndividuals(Path[] population)
        {
            var pNondominated = new List<Path>();
            foreach (var individualToCheck in Pop)
            {
                bool isDominated = Pop.Where(individual => !individualToCheck.Equals(individual)).Any(individual => Dominates(individualToCheck, individual));

                if (!isDominated)
                {
                    pNondominated.Add(individualToCheck);
                }
            }
            return pNondominated;
        }

        private bool Dominates(Path individualToCheck, Path individual)
        {
            bool betterForAllCriteriums = true;

            if (individualToCheck.FitnessDistance > individual.FitnessDistance || individualToCheck.FitnessDistance > individual.FitnessDistance)          
            {
                betterForAllCriteriums = false;
            }

            return betterForAllCriteriums;
        }
    }
}
