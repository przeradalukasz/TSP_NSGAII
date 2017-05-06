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
        public Path[] Pop { get; set; }
        public Path[] Children { get; set; }

        public Path[] Parents { get; set; }

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
            Children = new Path[num/2];
            Parents = new Path[num/2];
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
            Pop = Parents.Concat(Children).ToArray();
            SinceChange = 0;
            CalcFitness(); //called here rather than the main method on initialisation

            matingPool = new ArrayList();
            AllTime = new Path();
            AllTime.Distance = 100000; //abnormally high number
            AllTime.UnbalancingDegree = 100000; //abnormally high number

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

            // Clear the ArrayList
            matingPool.Clear();

            //always choose the best from the previous generation
            Path generation_best = new Path();

            double maxFitnessDistance = 0;
            double maxFitnessUnbalancingDegree = 0;
            double totalFitnessDistance = 0.0;
            double totalFitnessUnbalancingDegree = 0.0;

            double gen_best = 100000; //some big number

            //getting the relative fitness of each path to another
            for (int i = 0; i < Pop.Length; i++)
            {

                totalFitnessDistance += Pop[i].FitnessDistance;
                totalFitnessUnbalancingDegree += Pop[i].FitnessUnbalancingDegree;

                //update the generation best path while we're at it
                if (gen_best > Pop[i].Distance)
                {
                    generation_best = Pop[i];
                    gen_best = generation_best.Distance;
                }

                //the biggest fitness value of any generation must be kept
                if (Pop[i].FitnessDistance > maxFitnessDistance)
                {
                    maxFitnessDistance = Pop[i].FitnessDistance;
                }
                if (Pop[i].FitnessUnbalancingDegree > maxFitnessUnbalancingDegree)
                {
                    maxFitnessUnbalancingDegree = Pop[i].FitnessUnbalancingDegree;
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

                double fitness = Pop[i].FitnessDistance / maxFitnessDistance;

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

        private ICollection<Path> GetNondominatedIndividuals(Population population)
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

            if (individualToCheck.FitnessDistance > individual.FitnessDistance || individualToCheck.FitnessUnbalancingDegree > individual.FitnessUnbalancingDegree)          
            {
                betterForAllCriteriums = false;
            }

            return betterForAllCriteriums;
        }


        //private void NonDominatedSorting()
        //{
        //    int iRange = Pop.Length;               //range value for front
        //    while (population.Count > 0)
        //    {
        //        Population pNondominated = GetNondominatedIndividuals(population);
        //        FitessSharing(pNondominated, iRange);

        //        foreach (var individual in pNondominated)
        //        {
        //            basePopulation.Add(individual);
        //            population.Remove(individual);
        //        }
        //        iRange = (int)Math.Floor(pNondominated.GetMinFitnessInPopulation());
        //    }
        //}



        //public Population NaturalSelecton2()
        //{
        //    var population = Pop.Clone();
        //    basePopulation.Clear();

        //    NonDominatedSorting(basePopulation, population);

        //    Console.Write(basePopulation);

        //    var populationFittest = new Population();

        //    for (int j = 0; j < BasePopulationSize; j++)
        //    {
        //        Individual fittesIndiv = basePopulation.GetFittest(ProbabilityOfChoosingBest);
        //        populationFittest.Add(fittesIndiv);
        //        basePopulation.Remove(fittesIndiv);
        //    }

        //    return populationFittest;
        //}

        //private void NonDominatedSorting(Population basePopulation, Population population)
        //{
        //    if (basePopulation == null) throw new ArgumentNullException("basePopulation");

        //    int iRange = population.Count;               //range value for front
        //    while (population.Count > 0)
        //    {
        //        Population pNondominated = GetNondominatedIndividuals(population);
        //        CrowdingDistance(pNondominated, iRange);

        //        foreach (var individual in pNondominated)
        //        {
        //            basePopulation.Add(individual);
        //            population.Remove(individual);
        //        }
        //        iRange = (int)Math.Floor(pNondominated.GetMinFitnessInPopulation());
        //    }
        //}

        //private void CrowdingDistance(Path[] front, int iRange)
        //{
        //    foreach (var individualToCheck in front)
        //    {
        //        double dSumOfDistances = (from individual in front
        //                                  where !individualToCheck.Equals(individual)
        //                                  select Distance(individualToCheck, individual) into distance
        //                                  where distance < NicheRadius
        //                                  select 1 - (distance / NicheRadius)).Sum();
        //        individualToCheck.Fitness = dSumOfDistances != 0.0 ? iRange / dSumOfDistances : iRange;
        //    }
        //}

        //private Path[] GetNondominatedIndividuals(Path[] pop)
        //{
        //    var pNondominated = new ArrayList(pop.Length);
        //    foreach (var individualToCheck in pop)
        //    {
        //        bool isDominated = pop.Where(individual => !individualToCheck.Equals(individual)).Any(individual => Dominates(individualToCheck, individual));

        //        if (!isDominated)
        //        {
        //            pNondominated.Add(individualToCheck);
        //        }
        //    }
        //    return (Path[])pNondominated.ToArray();
        //}

        //private bool Dominates(Path individualToCheck, Path individual)
        //{
        //    bool bBetterForAllCriteriums = true;

        //    if (individualToCheck.Distance > individual.Distance || individualToCheck.Distance2 > individual.Distance2)
        //    {
        //        bBetterForAllCriteriums = false;
        //    }

        //    return bBetterForAllCriteriums;
        //}

    }
}
