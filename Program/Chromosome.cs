using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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
        public int maxNumNurses;
        public int numPatients { get; private set; }

        public static double timeViolationPenaltyModifier = 10;
        public static double capacityViolationPenaltyModifier = 500;
        public static double tooEarlyHeuristicPenaltyModifier = 100;

        public Chromosome(int numNurses, int numPatients, int maxNumNurses) 
        {
            
            this.nursePaths = new int?[numNurses, numPatients];
            this.numNurses = numNurses;
            this.maxNumNurses = maxNumNurses;
            this.numPatients = numPatients;
        }
        private Chromosome(int numNurses, int maxNumNurses, int numPatients, double? fitness, int?[,] nursePaths)
        {
            this.numNurses = numNurses;
            this.maxNumNurses = maxNumNurses;
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
            double distance;
            int stopIndex;
            for (int i = 0; i < patients.Length; i++)
            {
                if (patients[i] == null) break;
                distance = Double.MaxValue;

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

        public void updateNumNurses() 
        {
            // updates number of nurses to reflect the current utilization of nurses, i.e. if not are all used
            numNurses = 0;
            for (int i = 0; i < nursePaths.GetLength(0); i++) 
            {
                if (nursePaths[i, 0] != null)
                    numNurses++;
                else continue;
            }
        }

        public int getNextAvailableNurse(int nurse)
        {
            if (nursePaths[nurse, 0] != null) return nurse;

            return getNextAvailableNurse((nurse + 1) % (maxNumNurses - 1));
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

                for (int i = 0; i < this.maxNumNurses; i++)
                {
                    if (nursePaths[i, 0] == null) continue;

                    for (int j = 0; j < this.numPatients; j++)
                    {
                        if (nursePaths[i, j] == null) break;

                        hash = hash * 23 + nursePaths[i, j].GetHashCode();
                    }
                }
                return hash;
            }
        }

        private double sigmoidScaled(double x, double scalingFactor)
        {
            return 2 / (1 + Math.Pow(Math.E, -scalingFactor * x)) - 1;
        }

        public double calcFitness(TSP problem, bool withHeuristic = true)
        {
            if (fitness != null && withHeuristic) return (double)fitness;


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
            double totalRouteTime;
            Patient patient;

            for (int i = 0; i < maxNumNurses; i++)
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

                    //if (totalRouteTime > patient.start_time)
                    //    countTooLate += 1;

                    if (totalRouteTime < patient.start_time)
                    {
                        sumTooEarly += patient.start_time - totalRouteTime;
                        totalRouteTime += patient.start_time - totalRouteTime;
                    }

                    totalRouteTime += patient.care_time;

                    if (totalRouteTime >= patient.end_time)
                        countTooLate += 1;

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
                fitness += timeViolationPenaltyModifier * countTooLate;
                fitness += capacityViolationPenaltyModifier * sigmoidScaled(totalCapacityViolation, 0.1);

                if (countTooLate != 0)
                    fitness += tooEarlyHeuristicPenaltyModifier * sigmoidScaled(sumTooEarly, 0.0003);
            }
            
            return (double)fitness;
        }

        public int CompareTo(Chromosome other)
        {
            return ((double)fitness).CompareTo((double)other.fitness);
        }

        public object Clone()
        {
            return new Chromosome(numNurses, maxNumNurses, numPatients, fitness, (int?[,])nursePaths.Clone());
        }

        public void saveNursePathsToJson(string filePath)
        {
            List<List<int>> paths = new List<List<int>>();
            for (int i = 0; i < maxNumNurses; i++)
            {

                List<int> inner = new List<int>();

                for (int j = 0; j < numPatients; j++)
                {
                    if (nursePaths[i, j] == null) break;

                    inner.Add((int)nursePaths[i, j]);
                }

                paths.Add(inner);
            }

            string jsonString = JsonConvert.SerializeObject(paths);

            File.WriteAllText(filePath, jsonString);
        }

        public string print(TSP problem, string filePath = null)
        {
            int previousLocation;
            int currentLocation;
            int usedCapacity;
            double totalRouteTime;
            Patient patient;

            StringBuilder sb = new StringBuilder();

            sb.Append($"Nurse capacity: {problem.capacity_nurse}\n");

            sb.Append($"Depot return time: {problem.depot.return_time}\n");
            
            sb.Append('-', 100);
            sb.Append("\n");


            for (int i = 0; i < maxNumNurses; i++)
            {
                if (nursePaths[i, 0] == null) continue;

                previousLocation = 0;
                totalRouteTime = 0;
                usedCapacity = 0;
                for (int j = 0; j < numPatients; j++)
                {
                    if (nursePaths[i, j] == null) break;

                    currentLocation = (int)nursePaths[i, j];
                    patient = problem.patients[currentLocation];
                    usedCapacity += patient.demand;

                    totalRouteTime += problem.travel_times[previousLocation, currentLocation];

                    previousLocation = currentLocation;
                }
                totalRouteTime += problem.travel_times[previousLocation, 0];


                sb.Append($"Nurse {i+1, -7}");

                sb.Append($"{totalRouteTime, 6:F2}");

                sb.Append($"{usedCapacity, 8}");

                sb.Append($"\t\tD(0)");
                sb.Append($"   →   ");

                previousLocation = 0;
                totalRouteTime = 0;
                for (int j = 0; j < numPatients; j++)
                {
                    if (nursePaths[i, j] == null) break;

                    currentLocation = (int)nursePaths[i, j];
                    patient = problem.patients[currentLocation];
                    totalRouteTime += problem.travel_times[previousLocation, currentLocation];


                    sb.Append($"{currentLocation}");

                    sb.Append($"({totalRouteTime:F2}");
                    totalRouteTime += patient.care_time;
                    sb.Append($"-{totalRouteTime:F2})");

                    sb.Append($"[{patient.start_time}-{patient.end_time}]");

                    sb.Append($"   →   ");


                    previousLocation = currentLocation;
                }

                sb.Append($"D({totalRouteTime:F2})");

                sb.Append("\n");

            }

            sb.Append('-', 100);
            sb.Append("\n");

            double ojbectiveFitness = calcFitness(problem, false);

            sb.Append($"Objective value (total duration): {ojbectiveFitness}");

            if (filePath != null)
            {
                File.WriteAllText(filePath, sb.ToString());
            }

            return sb.ToString();

        }
    }
}
