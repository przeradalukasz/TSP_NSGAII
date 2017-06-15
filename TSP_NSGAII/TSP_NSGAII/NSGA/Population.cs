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
        private readonly List<Path> _matingPool;
        private readonly double _mutationRate;
        private readonly double _crossoverRate;
        private readonly double _tournamentSize;
        private readonly int _popSize;
        private readonly Random _rnd;

        public int Generations;
        public int SinceChange;

        public double[,] AdjacencyMatrix;
        public Town[] Towns;
        public List<List<Path>> Fronts;
        public Path[] Pop { get; set; }
        public Path[] Children { get; set; }
        public Path[] Parents { get; set; }
        public Population(double[,] adjacencyMatrix, Town[] towns, double mutationRate, double crossoverRate, double tournamentSize, int num, Random random)
        {
            this._mutationRate = mutationRate;
            this._crossoverRate = crossoverRate;
            _tournamentSize = tournamentSize;
            AdjacencyMatrix = adjacencyMatrix;
            Towns = towns;
            _rnd = random;
            Children = new Path[num];
            Parents = new Path[num];
            _popSize = num;
            for (int i = 0; i < Parents.Length; i++)
            {
                Path temp = new Path(adjacencyMatrix, towns, _rnd);
                temp.RandomPath();
                Parents[i] = temp;
            }
            for (int i = 0; i < Children.Length; i++)
            {
                Path temp = new Path(adjacencyMatrix, towns, _rnd);
                temp.RandomPath();
                Children[i] = temp;
            }
            
            SinceChange = 0;

            _matingPool = new List<Path>();
            Fronts = new List<List<Path>>();
        }

        public void NaturalSelection()
        {
            Pop = Children.Concat(Parents).ToArray();

            _matingPool.Clear();
            Fronts.Clear();
            
            int i = 0;
            while (Pop.Length != 0)
            {
                List<Path> front = GetNondominatedIndividuals(Pop);
                if (front.Count == 0)
                {
                    Fronts.Add(Pop.ToList());
                    break;
                }
                Fronts.Add(front);
                foreach (var path in Fronts[i])
                {
                    Pop = Pop.Where(val => val != path).ToArray();
                }
                i++;
            }
            int diff = _popSize;
            foreach (var front in Fronts)
            {
                if (front.Count <= diff)
                {
                    _matingPool.AddRange(front);
                    diff = diff - front.Count;
                }
                else
                {
                    CalculateCrowdedDist(front);
                    var orderedFront = front.OrderBy(f => f.CrowdedDistance).ToList();
                    _matingPool.AddRange(orderedFront.Take(diff).ToList());
                    break;
                }
            }

        }

        private void CalculateCrowdedDist(List<Path> front)
        {
            front = front.OrderBy(x => x.Distance).ToList();
            front.First().CrowdedDistance = Double.MaxValue; ;
            front.Last().CrowdedDistance = Double.MaxValue;
            List<Path> noBoundaryPop = new List<Path>();
            noBoundaryPop.AddRange(front);
            noBoundaryPop.RemoveAt(0);
            if (noBoundaryPop.Count != 0)
            {
                noBoundaryPop.RemoveAt(noBoundaryPop.Count - 1);
            }
            

            foreach (var path in noBoundaryPop)
            {
                path.CrowdedDistance = FirstObjCrowdedDistance(path,front);
                
            }
            front = front.OrderBy(x => x.UnbalancingDegree).ToList();
            front.First().CrowdedDistance = Double.MaxValue; ;
            front.Last().CrowdedDistance = Double.MaxValue;
            noBoundaryPop = new List<Path>();
            noBoundaryPop.AddRange(front);
            noBoundaryPop.RemoveAt(0);
            if (noBoundaryPop.Count != 0)
            {
                noBoundaryPop.RemoveAt(noBoundaryPop.Count - 1);
            }
            foreach (var path in noBoundaryPop)
            {
                path.CrowdedDistance = path.CrowdedDistance + SecondObjCrowdedDistance(path, front);
            }

        }

        private double SecondObjCrowdedDistance(Path path, List<Path> front)
        {
            var index = front.FindIndex(p=>p.Equals(path));
            return front[index + 1].UnbalancingDegree - front[index - 1].UnbalancingDegree;
            
        }

        private double FirstObjCrowdedDistance(Path path, List<Path> front)
        {
            var index = front.FindIndex(p => p.Equals(path));
            return front[index + 1].Distance - front[index - 1].Distance;
        }

        public void Generate()
        {
            int k = 0;
            foreach (var parent in _matingPool)
            {
                Parents[k] = new Path(AdjacencyMatrix, Towns, parent.Towns, _rnd);
                k++;
            }

            for (int i = 1; i < Children.Length; i++)
            {
                //int a = (int)(_rnd.NextDouble() * _matingPool.Count);
                //int b = (int)(_rnd.NextDouble() * _matingPool.Count);
                int a = GetBest(_matingPool);
                int b = GetBest(_matingPool);
                Path partnerA = (Path)_matingPool[a];
                Path partnerB = (Path)_matingPool[b];
                Path child = new Path(AdjacencyMatrix, Towns, partnerA.CrossOver(partnerB), _rnd);
                child.Mutate4(_mutationRate);
                Children[i] = child;
            }
            Generations++;

        }

        public void GenerateTournament()
        {
            int k = 0;
            foreach (var parent in _matingPool)
            {
                Parents[k] = new Path(AdjacencyMatrix, Towns, parent.Towns, _rnd);
                k++;
            }

            for (int i = 1; i < Children.Length; i++)
            {
                if (_rnd.NextDouble() < _crossoverRate)
                {
                    List<Path> tournament = new List<Path>();
                    for (int j = 0; j < _tournamentSize; j++)
                    {
                        int a = (int)(_rnd.NextDouble() * _matingPool.Count);
                        tournament.Add((Path)_matingPool[a]);
                    }
                    Path partnerA = tournament.First();
                    Path partnerB = tournament.Skip(1).First();
                    Path child = new Path(AdjacencyMatrix, Towns, partnerA.CrossOver(partnerB), _rnd);
                    child.Mutate4(_mutationRate);
                    Children[i] = child;
                }
                else
                {
                    int a = (int)(_rnd.NextDouble() * _matingPool.Count);
                    Children[i] = (Path)_matingPool[a];
                }
            }
            Generations++;

        }

        public List<Path> GetNondominatedIndividuals(Path[] population)
        {
            var pNondominated = new List<Path>();
            foreach (var individualToCheck in population)
            {
                bool isDominated = population.Where(individual => !individualToCheck.Equals(individual)).Any(individual => Dominates(individualToCheck, individual));

                if (!isDominated)
                {
                    pNondominated.Add(individualToCheck);
                }
            }
            return pNondominated;
        }

        public bool Dominates(Path individualToCheck, Path individual)
        {
            bool betterForAllCriteriums = true;

            if (individualToCheck.Distance < individual.Distance || individualToCheck.UnbalancingDegree < individual.UnbalancingDegree)          
            {
                betterForAllCriteriums = false;
            }

            return betterForAllCriteriums;
        }

        private int GetBest(List<Path> matingPool)
        {
            int best = -1;
            for (int i = 0; i < 2; i++)
            {
                int ind = (int)(_rnd.NextDouble() * _matingPool.Count);
                if (best == -1 || best > ind)
                {
                    best = ind;
                }
            }
            return best;
        }
    }
}
