using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_NSGAII
{
    public class Path
    {
        public double Distance { get; set; }

        public double UnbalancingDegree { get; set; }
        public Town[] Towns { get; set; }
        public Town[] AllTowns { get; set; }
        private int Size { get; set; }
        private double[,] Matrix { get; set; } //1-indexed matrix
        public double FitnessDistance { get; set; }
        public double FitnessUnbalancingDegree { get; set; }
        public Random rnd;

        //constructor used for random paths
        public Path(double[,] matrix, Town[] allTowns, Random random)
        {
            Matrix = matrix;
            AllTowns = allTowns;
            Towns = new Town[allTowns.Length + 1]; //one more so the last one is the first one
            Distance = 0.0;
            UnbalancingDegree = 0.0;
            Size = 0;
            FitnessDistance = 0.0;
            FitnessUnbalancingDegree = 0.0;
            rnd = random;
        }

        //most commonly used constructor - for the crossover function
        public Path(double[,] matrix, Town[] allTowns, Town[] inOrder, Random random)
        {
            Matrix = matrix;
            AllTowns = allTowns;
            Towns = inOrder;
            Distance = 0.0;
            UnbalancingDegree = 0.0;
            Size = 0;
            FitnessDistance = 0.0;
            FitnessUnbalancingDegree = 0.0;
            Distance = CalcDistance();
            UnbalancingDegree = CalcUnbalancingDegree();
            Size = Towns.Length;
            rnd = random;
        }

        public Path() { }//used when filling the mating pool. not very important.

        //return a correctly-formatted string for submission
        public string MakeString()
        {
            string str = "";

            for (int i = 0; i < Size; i++)
            {
                str += Towns[i].Id + ".";
            }

            return str;
        }

        //mutation generates diversity - essential
        //spare mutation function? - swap 2 random Towns (not the start/end though) - works w/ higher mutation factor
        public void Mutate(double mutationRate)
        {

            if (rnd.NextDouble() < mutationRate)
            {
                int a = (int)(rnd.NextDouble() * (Size - 3));
                int b = (int)(rnd.Next(1, 100) / 100 * (Size - 3));
                //do the swap
                Town townA = new Town(Towns[a + 1].Id, Towns[a + 1].X, Towns[a + 1].Y);//towns[a+1].copy();
                Town townB = new Town(Towns[b + 1].Id, Towns[b + 1].X, Towns[b + 1].Y);
                Towns[a + 1] = townB;
                Towns[b + 1] = townA;

                Distance = CalcDistance(); //it has a new distance of course!
                                           //System.out.println("swapping "+townA.name+" with "+townB.name);
                UnbalancingDegree = CalcUnbalancingDegree();
            }
        }

        //mutation generates diversity - essential
        //better mutation function? - reverses a random subtour and puts it back in a random postion
        // - works w/ lower mutation factor
        public void Mutate4(double mutationRate)
        {

            if (rnd.NextDouble() < mutationRate)
            {
                //the start of the subtour between second and 4th last
                int a = (int)(rnd.NextDouble() * (Size - 5) + 1);
                //where to place it between second and second last
                int b = (int)(rnd.NextDouble() * (Size - 5) + 1);
                Town[] subTowns = new Town[3];
                //reverse when putting back in!
                for (int i = 0; i < subTowns.Length; i++)
                {
                    subTowns[i] = Towns[a + i];
                }
                if (a < b)
                { //move everything between them down
                    for (int i = a; i < b; i++)
                    {
                        Towns[i] = Towns[i + 3];
                    }
                }
                else
                { //move everything between them up
                    for (int i = a; i > b; i--)
                    {
                        Towns[i + 2] = Towns[i - 1];
                    }

                }
                //put them back in reverse order
                for (int i = b, j = subTowns.Length - 1; i < b + subTowns.Length; i++, j--)
                {
                    Towns[i] = subTowns[j];
                }

                Distance = CalcDistance();//it has a new distance of course!
                UnbalancingDegree = CalcUnbalancingDegree();

            }
        }

        //simply counts up according to the adjacency matrix
        public double CalcDistance()
        {
            double result = 0.0;
            for (int i = 0; i < Towns.Length - 1; i++)
            {
                result += Matrix[Towns[i].Id, Towns[i + 1].Id];
            }
            return result;
        }
        public double CalcUnbalancingDegree()
        {
            double minDistance = 100000.0;
            double maxDistance = 0.0;
            
            for (int i = 0; i < Towns.Length - 1; i++)
            {
                double currentDistance = Matrix[Towns[i].Id, Towns[i + 1].Id];
                if (currentDistance < minDistance)
                    minDistance = currentDistance;
                if (currentDistance > maxDistance)
                    maxDistance = currentDistance;
            }
            return maxDistance - minDistance;
        }
        

        //at the start random paths are required. but they must be valid
        public void RandomPath()
        {

            //keeps track of visited towns
            bool[] visited = new bool[AllTowns.Length];
            int added = 0;//haven't been to any
            while (added < AllTowns.Length)
            {
                int rand = rnd.Next(0, AllTowns.Length);
                //have we visited this random town
                if (!visited[rand])
                {
                    Towns[added] = AllTowns[rand];
                    visited[rand] = true;
                    added++;
                }
            }
            //join up the start and the end and calculate the distance
            Towns[added] = Towns[0];
            Distance = CalcDistance();
            UnbalancingDegree = CalcUnbalancingDegree();
            Size = Towns.Length;
        }

        //crossover is the way to get better Paths from a previous generation. Also maintains diversity. 
        //using the edge recombinator technique - return an array of towns in order
        //the algorithm is described here: http://en.wikipedia.org/wiki/Edge_recombination_operator
        public Town[] CrossOver(Path partner)
        {
            try
            {
                //get the adjacency matrices
                ArrayList[] neighbours = GetNeighbours();
                ArrayList[] pNeighbours = partner.GetNeighbours();

                //take the union of these matrices
                ArrayList[] union = new ArrayList[Towns.Length];

                for (int i = 1; i < neighbours.Length; i++)
                {
                    ArrayList temp = new ArrayList(4);

                    for (int j = 0; j < 2; j++)
                    {
                        int candidate = (int)neighbours[i][j];
                        temp.Add(candidate);
                    }

                    for (int j = 0; j < 2; j++)
                    {
                        int candidate = (int)pNeighbours[i][j];
                        if (!temp.Contains(candidate))
                        {
                            temp.Add(candidate);
                        }
                    }
                    union[i] = temp;
                }

                ArrayList list = new ArrayList(); //to store the combined path
                                                  //pick the first at random from the firsts
                int n = (rnd.NextDouble() < 0.5) ? Towns[0].Id : partner.Towns[0].Id;
                //until each town is in once
                while (list.Count < Towns.Length - 1)
                {
                    list.Add(n);
                    //remove n from all neighbour lists
                    for (int i = 1; i < union.Length; i++)
                    {
                        union[i].Remove(n);
                    }
                    int n_int = n;

                    ArrayList find_n = union[n_int];

                    if (find_n.Count > 0)
                    {//n's neighbour list is non-empty
                     //set initial values
                        int leastI = (int)find_n[0];
                        int least_size = union[leastI].Count;
                        for (int i = 1; i < find_n.Count; i++)
                        {
                            int testI = (int)find_n[i];
                            if (union[testI].Count < least_size)
                            {
                                //found new lowest, update both
                                least_size = union[testI].Count;
                                leastI = (int)find_n[i];
                            }
                            else if (union[testI].Count == least_size)
                            {
                                //found equal lowest, choose randomly
                                if (rnd.NextDouble() < 0.5)
                                {
                                    leastI = (int)find_n[i];
                                }
                            }
                        }

                        n = leastI;

                    }
                    else
                    {//neighbour list is empty so choose closest from the closest unvisited


                        double least = 100000;
                        int choose = 1;

                        for (int i = 1; i < Towns.Length; i++)
                        {
                            int temp = i;
                            if (!list.Contains(temp) && Matrix[Towns[i].Id, Towns[n_int].Id] < least)
                            {
                                least = Matrix[Towns[i].Id, Towns[n_int].Id];
                                choose = temp;
                            }
                        }

                        n = choose;
                    }

                }//end while - we've found enough Towns

                int first = (int)list[0];
                list.Add(first); //join it up!

                Town[] newTowns = new Town[list.Count];//the array we will return
                                                       //convert those Integers to Towns
                for (int i = 0; i < list.Count; i++)
                {
                    int temp = (int)list[i];
                    newTowns[i] = AllTowns[temp - 1];
                }

                return newTowns;

                //slim possibility of a NullPointerException though
            }
            catch (Exception e)
            {
                Console.WriteLine(e + " caught while towns = \n" + MakeString());
                return Towns;
            }

        }

        //convience function used by the crossover function. 
        //returns a kind of "neighbour matrix" of Integers. 
        private ArrayList[] GetNeighbours()
        {

            ArrayList[] lists = new ArrayList[Towns.Length]; //1-indexed

            //the start/end is a special case
            ArrayList temp = new ArrayList(2);
            temp.Add(Towns[Towns.Length - 2].Id);
            temp.Add(Towns[1].Id);
            lists[Towns[0].Id] = temp;

            for (int i = 1; i < Towns.Length - 1; i++)
            {
                ArrayList temp2 = new ArrayList(2);
                temp2.Add(Towns[i - 1].Id);
                temp2.Add(Towns[i + 1].Id);
                lists[Towns[i].Id] = temp2;
            }
            return lists;
        }


    }

}

