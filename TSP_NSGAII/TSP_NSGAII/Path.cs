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
        private double[,] Matrix { get; set; } 
        public double FitnessDistance { get; set; }
        public double FitnessUnbalancingDegree { get; set; }
        public double CrowdedDistance { get; set; }
        public Random rnd;
        
        public Path(double[,] matrix, Town[] allTowns, Random random)
        {
            Matrix = matrix;
            AllTowns = allTowns;
            Towns = new Town[allTowns.Length + 1];
            Distance = 0.0;
            UnbalancingDegree = 0.0;
            Size = 0;
            FitnessDistance = 0.0;
            FitnessUnbalancingDegree = 0.0;
            CrowdedDistance = 0.0;
            rnd = random;
        }
        
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
            CrowdedDistance = 0.0;
            Distance = CalcDistance();
            UnbalancingDegree = CalcUnbalancingDegree();
            Size = Towns.Length;
            rnd = random;
        }

        public Path() { }
        
        //mutation generates diversity - essential
        //spare mutation function? - swap 2 random Towns (not the start/end though) - works w/ higher mutation factor
        public void Mutate(double mutationRate)
        {

            if (rnd.NextDouble() < mutationRate)
            {
                int a = (int)(rnd.NextDouble() * (Size - 3));
                int b = (int)(rnd.Next(1, 100) / 100 * (Size - 3));
                //do the swap
                Town townA = new Town(Towns[a + 1].Id, Towns[a + 1].X, Towns[a + 1].Y);
                Town townB = new Town(Towns[b + 1].Id, Towns[b + 1].X, Towns[b + 1].Y);
                Towns[a + 1] = townB;
                Towns[b + 1] = townA;

                Distance = CalcDistance(); 
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
        
        public void RandomPath()
        {
            bool[] visited = new bool[AllTowns.Length];
            int added = 0;
            while (added < AllTowns.Length)
            {
                int rand = rnd.Next(0, AllTowns.Length);
                if (!visited[rand])
                {
                    Towns[added] = AllTowns[rand];
                    visited[rand] = true;
                    added++;
                }
            }
            Towns[added] = Towns[0];
            Distance = CalcDistance();
            UnbalancingDegree = CalcUnbalancingDegree();
            Size = Towns.Length;
        }

        //the algorithm is described here: http://en.wikipedia.org/wiki/Edge_recombination_operator
        public Town[] CrossOver(Path partner)
        {
            try
            {
                ArrayList[] neighbours = GetNeighbours();
                ArrayList[] pNeighbours = partner.GetNeighbours();
                
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

                ArrayList list = new ArrayList(); 
                int n = (rnd.NextDouble() < 0.5) ? Towns[0].Id : partner.Towns[0].Id;
               
                while (list.Count < Towns.Length - 1)
                {
                    list.Add(n);
                    for (int i = 1; i < union.Length; i++)
                    {
                        union[i].Remove(n);
                    }
                    int n_int = n;

                    ArrayList find_n = union[n_int];

                    if (find_n.Count > 0)
                    {
                        int leastI = (int)find_n[0];
                        int least_size = union[leastI].Count;
                        for (int i = 1; i < find_n.Count; i++)
                        {
                            int testI = (int)find_n[i];
                            if (union[testI].Count < least_size)
                            {
                                least_size = union[testI].Count;
                                leastI = (int)find_n[i];
                            }
                            else if (union[testI].Count == least_size)
                            {
                                if (rnd.NextDouble() < 0.5)
                                {
                                    leastI = (int)find_n[i];
                                }
                            }
                        }
                        n = leastI;
                    }
                    else
                    {
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
                }

                int first = (int)list[0];
                list.Add(first); 

                Town[] newTowns = new Town[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    int temp = (int)list[i];
                    newTowns[i] = AllTowns[temp - 1];
                }
                return newTowns;
            }
            catch (Exception e)
            {
                return Towns;
            }

        }
        private ArrayList[] GetNeighbours()
        {

            ArrayList[] lists = new ArrayList[Towns.Length]; 

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

