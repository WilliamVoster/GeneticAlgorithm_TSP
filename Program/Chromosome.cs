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

        public void insertByDistance(int?[] patients, Double[,] travel_times)
        {
            int row = -1;
            int col = -1;
            int leftNode;
            int? leftCol;
            double leftOptionDistance;
            int rightNode;
            int? rightCol;
            double rightOptionDistance;
            double distance = Double.MaxValue;
            int stopIndex;
            for (int i = 0; i < patients.Length; i++)
            {
                if (patients[i] == null) break;

                for (int j = 0; j < nursePaths.GetLength(0); j++)
                {
                    if (nursePaths[j, 0] == null) continue;

                    for (int k = 0; k < nursePaths.GetLength(1); k++)
                    {
                        if (nursePaths[j, k] == null) break;

                        if (nursePaths[j, k] == patients[i]) continue;

                        if (travel_times[(int)patients[i], (int)nursePaths[j, k]] < distance)
                        {
                            distance = travel_times[(int)patients[i], (int)nursePaths[j, k]];
                            row = j; 
                            col = k;
                        }
                    }
                }
                
                if (col - 1 < 0)
                {
                    leftNode = 0;
                    leftCol = null; // i.e. root, wich is not represented in nursePaths
                }
                else
                {
                    leftNode = (int)nursePaths[row, col - 1];
                    leftCol = col - 1;
                }
                if (col + 1 > numPatients - 1)
                {
                    rightNode = 0;
                    rightCol = null; // i.e. root, wich is not represented in nursePaths
                }
                else
                {
                    if (nursePaths[row, col + 1] == null)
                    {
                        rightNode = 0;
                        rightCol = null; // i.e. root, wich is not represented in nursePaths
                    }
                    else
                    {
                        rightNode = (int)nursePaths[row, col + 1];
                        rightCol = col + 1;
                    }
                }

                leftOptionDistance = travel_times[leftNode, (int)patients[i]];
                leftOptionDistance += travel_times[(int)patients[i], (int)nursePaths[row, col]];
                leftOptionDistance -= travel_times[leftNode, (int)nursePaths[row, col]];

                rightOptionDistance = travel_times[rightNode, (int)patients[i]];
                rightOptionDistance += travel_times[(int)patients[i], (int)nursePaths[row, col]];
                rightOptionDistance -= travel_times[rightNode, (int)nursePaths[row, col]];

                if (leftOptionDistance < rightOptionDistance)
                {
                    stopIndex = leftCol == null ? 0 : (int)leftCol;
                }
                else
                {
                    // If right node is depot -> just append to end of nurse's path and go to next
                    if (rightCol == null)
                    {
                        for (int j = 0; j < nursePaths.GetLength(1); j++)
                        {
                            if (nursePaths[row, j] == null)
                            {
                                nursePaths[row, j] = patients[i];
                                break;
                            }
                        }
                        continue;
                    }

                    stopIndex = (int)rightCol;
                }

                // Shift patients to the right within the array/path
                // which gives a space for the new patient to be inserted
                for (int j = numPatients - 1; j > stopIndex; j--)
                {
                    if (nursePaths[row, j - 1] == null) continue;

                    nursePaths[row, j] = nursePaths[row, j - 1];
                }

                nursePaths[row, stopIndex] = patients[i];
                

            }

        }

        public void deleteByValue(int patient)
        {
            for (int i = 0; i < nursePaths.GetLength(0); i++)
            {
                if (nursePaths[i, 0] == null) continue;
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
            numNurses = 0;
            for (int i = 0; i < nursePaths.GetLength(0); i++) 
            {
                if (nursePaths[i, 0] != null)
                    numNurses++;
            }
            numNurses = nursePaths.GetLength(0);
        }

        public int getNextAvailableNurse(int nurse)
        {
            if (nursePaths[nurse, 0] != null) return nurse;

            return getNextAvailableNurse((nurse + 1) % (numNurses - 1));
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

        public double calcFitness(TSP problem, bool withHeuristic = true)
        {
            if (fitness != null) return (double)fitness;


            // Fitness factors
            // distance travelled
            // how much each nurse over capacity
            // count num too late at patient / depot
            // count num too early at patient

            fitness = 0.0;
            int previousLocation;
            int currentLocation;
            int usedCapacity;
            int totalCapacityViolation = 0;
            int countTooLate = 0;
            double sumTooEarly = 0;
            double totalDrivingTime = 0.0;
            double totalRouteTime = 0.0;
            Patient patient;

            for (int i = 0; i < numNurses; i++)
            {
                if (nursePaths[i, 0] == null) continue;

                previousLocation = 0; // Starts at depot
                totalRouteTime = 0; // Different for each nurse route
                usedCapacity = 0;

                for (int j = 0; j < numPatients; j++)
                {
                    if (nursePaths[i, j] == null) break;

                    currentLocation = (int)nursePaths[i, j];
                    patient = problem.patients[currentLocation];

                    totalDrivingTime += problem.travel_times[previousLocation, currentLocation];
                    totalRouteTime += problem.travel_times[previousLocation, currentLocation];
                    usedCapacity += patient.demand;

                    if (usedCapacity > problem.capacity_nurse)
                        totalCapacityViolation += usedCapacity - problem.capacity_nurse;

                    if (totalRouteTime > patient.start_time)
                        countTooLate += 1;

                    if (totalRouteTime < patient.start_time)
                        sumTooEarly += patient.start_time - totalRouteTime;

                    totalRouteTime += patient.care_time;

                    previousLocation = currentLocation;
                }

                // Travel back to the depot at the end of each route
                totalDrivingTime += problem.travel_times[previousLocation, 0];
                totalRouteTime += problem.travel_times[previousLocation, 0];

                if (totalRouteTime > problem.depot.return_time)
                    countTooLate += 1;
            }

            fitness = totalDrivingTime;

            if (withHeuristic)
            {
                fitness += 100  * countTooLate;
                fitness += 10.0 * Math.Pow(totalCapacityViolation, 2);
                fitness += 0.1  * Math.Pow(sumTooEarly, 2);
            }

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
