using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Program
{
    internal class Chromosome : IComparable<Chromosome>, ICloneable
    {
        public int?[,] nursePaths { get; set; }
        public double? fitness { get; set; }
        public int numNurses;
        public int numPatients { get; private set; }
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

        public void insertByDistance(int[] patients, Double[,] travel_times, int[] closestStops)
        {
            int closestStop;
            HashSet<int> checkedStops;
            for (int i = 0; i < patients.Length; i++)
            {

                closestStop = closestStops[patients[i]];

                if (closestStop in patients)
                {
                checkedStops = new HashSet<int>(travel_times[4]);

                    for (int j = 0; j < travel_times.GetLength(1); j++)
                    {
                        
                    }
                }




            }




            //// ______________________________________________________
            double closest;
            double distance;
            for (int i = 0; i < patients.Length; i++)
            {
                closest = Double.MaxValue;
                distance = 0.0;

                int closestStop = closestStops[patients[i]];

                for (int j = 0; j < nursePaths.GetLength(0); j++)
                {
                    if (nursePaths[j, 0] == null)
                        break;

                    for (int k = 0; k < nursePaths.GetLength(1); k++)
                    {
                        if (nursePaths[j, k] == null)
                            break;

                        patients[i]; // to add

                        distance += travel_times[j, closestStops[patients[i]]];
                    }
                }
            }
        }

        public void deleteByValue(int patient)
        {
            for (int i = 0; i < nursePaths.GetLength(0); i++)
            {
                for (int j = 0; j < nursePaths.GetLength(1); j++)
                {
                    if (nursePaths[i, j] == null)
                        break;

                    if (nursePaths[i, j] == patient)
                    {
                        // Shift patients to the left within the array/path
                        // which 'deletes'/drops the patient from the route
                        while (nursePaths[i, j] != null && j + 1 < numPatients)
                        {
                            nursePaths[i, j] = nursePaths[i, j + 1];
                            j++;
                        }

                        return;
                    }
                }
            }
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
