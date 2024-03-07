using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Program
{
    internal class Chromosome : IComparable<Chromosome>, ICloneable
    {
        public int?[,] nursePaths { get; private set; }
        public double? fitness { get; set; }
        public int numNurses;
        public int numPatients;
        public Chromosome(int numNurses, int numPatients) 
        {
            
            this.nursePaths = new int?[numNurses, numPatients];
            this.numNurses = numNurses;
            this.numPatients = numPatients;
        }
        private Chromosome(int numNurses, int numPatients, double? fitness, int?[,] nursePaths)
        {
            this.numNurses = numNurses;
            this.numPatients = numPatients;
            this.fitness = fitness;
            this.nursePaths = nursePaths;
        }

        public void updateNumNurses() // updates number of nurses to reflect the current utilization of nurses, i.e. if not are all used
        {
            for (int i = 0; i < nursePaths.GetLength(0); i++) 
            {
                if (nursePaths[i, 0] == null)
                {
                    numNurses = i;
                    return;
                }
            }
            numNurses = nursePaths.GetLength(0);
        }
        
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Chromosome other = (Chromosome)obj;

            for (int i = 0; i < numNurses; i++)
            {
                if (nursePaths[i, 0] == null)
                    break;

                for (int j = 0; j < numPatients; j++)
                {
                    if (nursePaths[i, j] == null)
                        break;

                    if (other.nursePaths[i, j] != nursePaths[i, j])
                        return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                for (int i = 0; i < this.numNurses; i++)
                {
                    for (int j = 0; j < this.numPatients; j++)
                    {
                        if (nursePaths[i, j] == null)
                            break;

                        hash = hash * 23 + nursePaths[i, j].GetHashCode();
                    }
                }
                return hash;
            }
        }

        public double calcFitness(TSP problem)
        {
            if (fitness != null)
                return (double)fitness;

            fitness = 0.0;
            int previousLocation = 0;   // Starts at depot
            int currentLocation;

            for (int i = 0;i < numNurses; i++)
            {
                if (nursePaths[i, 0] == null)
                    break;

                for (int j = 0;j < numPatients; j++)
                {
                    if (nursePaths[i, j] == null)
                        break;

                    currentLocation = (int)nursePaths[i, j] - 1; // -1 since patientID start on 1, not 0

                    fitness += problem.travel_times[previousLocation, currentLocation];

                    previousLocation = currentLocation;
                }
                
            }

            // Add distance travelled for route back to depot
            fitness += problem.travel_times[previousLocation, 0];

            return (double)fitness;
        }

        public int CompareTo(Chromosome other)
        {
            return ((double)fitness).CompareTo((double)other.fitness);
        }

        public object Clone()
        {
            return new Chromosome(numNurses, numPatients, fitness, (int?[,])nursePaths.Clone());
        }

    }
}
