using Newtonsoft.Json.Bson;
using Program;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;


namespace Program
{
    internal class GA
    {
        static int numThreads = 7;
        static Barrier barrier = new Barrier(numThreads);
        static Population[] islands = new Population[numThreads];

        public int countChildFitter = 0;
        public int countParentFitter = 0;
        public static void Main(string[] args)
        {

            // TODO
            // _ grow reschedulre mutation prob with iterations. Still has some violations in final

            string solutionDir = Directory.GetCurrentDirectory() + "\\..\\..\\..\\";

            TSP training_problem_0 = TSP.readJSON(solutionDir + "Data/train_0.json");

            Visualizer visualizer = new Visualizer(
                training_problem_0,
                solutionDir + "/Program/plotting.py", 
                solutionDir + "/Program/plottingData/chromosome.json");

            // Single threaded:
            //GA geneticAlgorithm = new GA(training_problem_0, 100, 10000, visualizer, 1);
            //geneticAlgorithm.run();


            // Multi threading:
            Thread[] threads = new Thread[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                TSP problem_0 = TSP.readJSON(solutionDir + "Data/train_0.json");

                GA geneticAlgorithm = new GA(problem_0, 80, 10000, visualizer, i);

                Thread thread = new Thread(geneticAlgorithm.run);
                threads[i] = thread;
                threads[i].Start();
            }
            for (int i = 0; i < numThreads; i++)
            {
                threads[i].Join();
            }

            int bestIsland = -1;
            double bestFitness = double.MaxValue;

            for(int i = 0; i < numThreads; i++)
            {
                if (islands[i].population[0].fitness < bestFitness)
                {
                    bestIsland = i;
                    bestFitness = (double)islands[i].population[0].fitness;
                }
            }
            Chromosome bestIndividual = islands[bestIsland].population[0];

            double fitness = bestIndividual.calcFitness(training_problem_0, false);

            visualizer.visualize(bestIndividual);

            string jsonValidateRoutesFilePath = Directory.GetCurrentDirectory() + "\\..\\..\\..\\" + "/Program/plottingData/validate.json";
            bestIndividual.saveNursePathsToJson(jsonValidateRoutesFilePath);

            Console.WriteLine(bestIndividual.print(training_problem_0));
            bestIndividual.print(training_problem_0, Directory.GetCurrentDirectory() + "\\..\\..\\..\\" + "/Program/plottingData/bestIndividual_stringformat.json");

            Console.WriteLine("Test");



        }


        TSP problem;
        Visualizer visualizer;
        int numIterations;
        Population population;
        int populationSize;
        Random random;
        //int seed = 1;
        int islandID;
        int numParentsToSelect;
        double reorderMutationThreshold;
        double transferMutationThreshold;
        double reorderByScheduleMutationThreshold;
        double crossoverRate;
        double crowdingEffect;
        int numNeighborsToCompare;
        public GA(TSP problem, int populationSize, int numIterations, Visualizer visualizer, int islandID) 
        {
            this.problem = problem;
            this.populationSize = populationSize;
            this.numIterations = numIterations; 
            this.visualizer = visualizer;
            this.random = new Random(islandID);
            this.islandID = islandID;

            // only even number of parents to select
            numParentsToSelect = (int)((double)populationSize * 0.4);
            if (numParentsToSelect % 2 != 0)
                numParentsToSelect--;

            // Mutation probabilities per patient
            reorderMutationThreshold = 0.008;
            transferMutationThreshold = 0.008;
            // Mutation probabilities per nurse
            reorderByScheduleMutationThreshold = 0.5;

            crossoverRate = 0.4;

            this.crowdingEffect = 5.0;
            numNeighborsToCompare = (int)((double)populationSize * 0.1);

        }

        public void run()
        {

            // Initialization
            population = new Population(populationSize, problem);
            population.inintializeEvenPatientSplit(true);

            lock (islands)
            {
                islands[islandID] = population;
            }
            

            Chromosome bestIndividual = (Chromosome)population.population[0].Clone();
            bestIndividual.fitness = double.MaxValue;


            //test
            List<int[]> fitter = new List<int[]>();
            List<double> avgFitness = new List<double>();
            double sumFitness;
            double fitnessAvg;
            double previousFitnesAvg = double.MaxValue;
            int countEqualFitness = 0;
            //___


            problem.calcAvgNumPatientsPerNurse();
            population.calcFitness();
            for (int i = 0; i < numIterations; i++)
            {

                // Testing and logging
                if (i % 100 == 0 && islandID == 0)
                {

                    int[] pair = new int[] {countChildFitter, countParentFitter};
                    fitter.Add(pair);

                    sumFitness = population.calcFitness(); // all new added children already have calculated fitnesses
                    avgFitness.Add(sumFitness / populationSize);

                    Console.WriteLine($"island {islandID} " + "Generation:  " + i + "\tFitness: " + population.population[0].fitness + " \t Avg fitness: " + sumFitness / populationSize);

                }
                fitnessAvg = (population.calcFitness() / populationSize);
                


                // Start______________________________________________________________________
                population.sort();

                if (population.population[0].fitness < bestIndividual.fitness)
                    bestIndividual = (Chromosome)population.population[0].Clone();


                // Parent Selection
                //Chromosome[] parents = elitistSelection(numParentsToSelect);
                Chromosome[] parents = fitnessProportionateSelection(numParentsToSelect);


                // Crossover
                Chromosome[] children = crossover(parents);


                // Mutation
                children = mutation(children);


                // Offspring Selection
                double distanceWeight = crowdingEffect - crowdingEffect * 0.5 * ((double)i / (double)numIterations);
                if (fitnessAvg == previousFitnesAvg)
                    countEqualFitness++;
                else
                    countEqualFitness = 0;

                //offspringSelection_fitnessProportionate(children, parents);
                //offspringSelection_HammingDistanceCrowding(children, parents, numNeighborsToCompare, distanceWeight);
                //offspringSelection_HammingDistanceCrowding(children, parents, 5, 10);
                //offspringSelection_HammingDistanceCrowding(children, parents, 5, distanceWeight);
                offspringSelection_HammingDistanceCrowding(children, parents, 5, distanceWeight + 0.5 * (double)countEqualFitness);



                // Multi threading -> sharing individuals
                if (i % 1000 == 0 && i != 0)
                {
                    //Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} Waiting");

                    barrier.SignalAndWait();

                    //Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} Synched");

                    int individualID = random.Next((int)(0.2 * populationSize)); // select random individual from top 20%
                    Chromosome migrant = (Chromosome)population.population[individualID].Clone();
                    int destination = random.Next(numThreads);

                    lock (islands)
                    {
                        for (int k = 0; k < numThreads; k++)
                        {
                            if (k == destination)
                            {
                                islands[destination].population[populationSize - 1 - k] = migrant;

                            }
                        }
                    }
                }



                previousFitnesAvg = fitnessAvg;

            }

            //// Termination
            //visualizer.visualize(bestIndividual);

            //string jsonValidateRoutesFilePath = Directory.GetCurrentDirectory() + "\\..\\..\\..\\" + "/Program/plottingData/validate.json";
            //bestIndividual.saveNursePathsToJson(jsonValidateRoutesFilePath);

            //Console.WriteLine(bestIndividual.print(problem));
            //bestIndividual.print(problem, Directory.GetCurrentDirectory() + "\\..\\..\\..\\" + "/Program/plottingData/bestIndividual_stringformat.json");

            //Console.WriteLine("Test");

        }

        public void tempCheckDuplicates(Chromosome[] pop)
        {
            return;
            for (int i = 0; i < pop.Length; i++)
            {
                for(int j = 0; j < pop[i].nursePaths.GetLength(0); j++)
                {
                    if (pop[i].nursePaths[j, 0] == null) continue;
                    for(int k = 0; k < pop[i].nursePaths.GetLength(1); k++)
                    {
                        if (pop[i].nursePaths[j, k] == null) continue;
                        for (int l = 0; l < pop[i].nursePaths.GetLength(0); l++)
                        {
                            if (pop[i].nursePaths[l, 0] == null) continue;
                            for (int m = 0; m < pop[i].nursePaths.GetLength(1); m++)
                            {
                                if (pop[i].nursePaths[l, m] == null) continue;
                                if (j == l && k == m) continue;
                                if (pop[i].nursePaths[j, k] == null || pop[i].nursePaths[l, m] == null) continue;
                                if (pop[i].nursePaths[j, k] == pop[i].nursePaths[l, m])
                                {
                                    Console.WriteLine("PROBLEM!");
                                    return;
                                }
                            }
                        }
                    }

                }
            }
        }

        private Chromosome[] elitistSelection(int n)
        {
            Chromosome[] selected = new Chromosome[n];

            Array.Copy(population.population, 0, selected, 0, n);

            return selected;
        }

        private Chromosome[] fitnessProportionateSelection(int n)
        {
            Chromosome[] selected = new Chromosome[n];
            double winner;
            double proportion;
            double previousProportions;
            double sumFitness = population.calcFitness();

            for (int i = 0; i < n; i++)
            {
                winner = random.NextDouble();
                previousProportions = 0.0;

                for (int j = 0; j < population.population.Length; j++)
                {
                    proportion = (double)population.population[j].fitness / sumFitness;

                    if (winner >= previousProportions && winner < (proportion + previousProportions))
                    {
                        selected[i] = population.population[j];
                        break;
                    }

                    previousProportions += proportion;
                }
            }

            for (int i = 0; i < selected.Length; i++)
            {
                if (selected[i] == null)
                {
                    Console.WriteLine("HOW?");
                }
            }

            return selected;
        }
        
        private Chromosome[] crossover(Chromosome[] parents)
        {
            int nurseIndex1;
            int nurseIndex2;
            int patient1;
            int patient2;
            int?[] patients1 = new int?[parents[0].numPatients];
            int?[] patients2 = new int?[parents[0].numPatients];
            Chromosome[] children = new Chromosome[parents.Length];
            bool abortCrossover = false;

            // Make copies of parents, called children, and modify them instead
            for (int i = 0; i < parents.Length; i++)
            {
                children[i] = (Chromosome)parents[i].Clone();
            }


            for (int i = 0; i < children.Length; i += 2)
            {
                if (random.NextDouble() < crossoverRate) continue;

                for (int j = 0; j < patients1.Length; j++)
                {
                    if (patients1[j] == null && patients2[j] == null) break;
                    patients1[j] = null;
                    patients2[j] = null;
                }


                children[i].updateNumNurses();
                children[i + 1].updateNumNurses();
                nurseIndex1 = children[i].getNextAvailableNurse(random.Next(0, children[i].numNurses));
                nurseIndex2 = children[i + 1].getNextAvailableNurse(random.Next(0, children[i + 1].numNurses));

                // Patients from one nurse in parent1 are deleted in parent2
                for (int j = 0; j < children[i].numPatients; j++)
                {
                    if (children[i].nursePaths[nurseIndex1, j] == null) break;

                    patient1 = (int)children[i].nursePaths[nurseIndex1, j];

                    //children[i + 1].deleteByValue(patient1);

                    patients1[j] = patient1;

                    if (j > 0.8 * (double)children[i].numPatients)
                    {
                        abortCrossover = true;
                        break;
                    }
                }

                // Patients from one nurse in parent2 are deleted in parent1
                for (int j = 0; j < children[i + 1].numPatients; j++)
                {
                    if (children[i + 1].nursePaths[nurseIndex2, j] == null) break;

                    patient2 = (int)children[i + 1].nursePaths[nurseIndex2, j];

                    //children[i].deleteByValue(patient2);

                    patients2[j] = patient2;

                    if (j > 0.8 * (double)children[i].numPatients)
                    {
                        abortCrossover = true;
                        break;
                    }
                }

                if (abortCrossover)
                {
                    abortCrossover = false;
                    continue;
                }

                for (int j = 0;j < patients1.Length; j++)
                {
                    if (patients1[j] == null) break;
                    children[i + 1].deleteByValue((int)patients1[j]);
                }
                for (int j = 0; j < patients2.Length; j++)
                {
                    if (patients2[j] == null) break;
                    children[i].deleteByValue((int)patients2[j]);
                }

                // For unvisited patients in parent find a nurse to visit them
                children[i].insertByDistance(patients2, problem.travel_times);
                children[i + 1].insertByDistance(patients1, problem.travel_times);

                children[i].fitness = null;
                children[i + 1].fitness = null;

            }
            return children;
        }


        private Chromosome[] mutation(Chromosome[] children) 
        {
            // Two mutation types:
            // Reorder patient within nurse route
            // Give patient to another nurse route

            int?[,] nursePaths;
            bool doReorderMutation;
            bool doTransferMutation;
            bool doReorderByScheduleMutation;
            int nurseIndex;
            int patientIndex;
            int patientValue;
            List<int[]> patients;
            int[] patient;

            for (int i = 0; i < children.Length; i++)
            {
                nursePaths = children[i].nursePaths;

                for (int j = 0; j < nursePaths.GetLength(0); j++)
                {
                    if (nursePaths[j, 0] == null) continue;

                    for (int k = 0; k < nursePaths.GetLength(1); k++)
                    {
                        if (nursePaths[j, k] == null) break;

                        doReorderMutation = random.NextDouble() < reorderMutationThreshold;
                        doTransferMutation = random.NextDouble() < transferMutationThreshold;

                        if (doReorderMutation)
                        {
                            // Swap patient with random other patient within same nurse route
                            int l;
                            for (l = 0; l < nursePaths.GetLength(1); l++)
                                if (nursePaths[j, l] == null) break;

                            patientIndex = random.Next(0, l - 1);

                            if (patientIndex == k)
                                if (l != 1) // skip mutation if only 1 patient in route
                                    patientIndex++;

                            patientValue = (int)nursePaths[j, patientIndex];
                            nursePaths[j, patientIndex] = nursePaths[j, k];
                            nursePaths[j, k] = patientValue;

                        }

                        if (doTransferMutation)
                        {
                            nurseIndex = children[i].getNextAvailableNurse(random.Next(0, children[i].numNurses));
                            int l;
                            for (l = 0; l < nursePaths.GetLength(1); l++)
                                if (nursePaths[nurseIndex, l] == null) break;

                            // If route is full -> check another route
                            if (l == nursePaths.GetLength(1))
                            {
                                nurseIndex = children[i].getNextAvailableNurse(nurseIndex);
                                for (l = 0; l < nursePaths.GetLength(1); l++)
                                    if (nursePaths[nurseIndex, l] == null) break;
                            }
                            patientIndex = random.Next(0, l);

                            // Shift patients to the right within the array/route
                            for (int m = children[i].numPatients - 1; m > patientIndex; m--)
                            {
                                if (nursePaths[nurseIndex, m - 1] == null) continue;

                                nursePaths[nurseIndex, m] = nursePaths[nurseIndex, m - 1];
                            }

                            // Which gives a space for the new patient to be inserted
                            nursePaths[nurseIndex, patientIndex] = (int)nursePaths[j, k];

                            // Deleting from original location by 
                            // Shifting patients to the left within the array/route at point of deletion
                            for (int m = k; m < nursePaths.GetLength(1); m++)
                            {
                                // if no more to shift
                                if (nursePaths[j, m] == null) break;

                                // if m+1 is last index, set m to null
                                if (m + 1 >= nursePaths.GetLength(1))
                                {
                                    nursePaths[j, m] = null;
                                    break;
                                }

                                nursePaths[j, m] = nursePaths[j, m + 1];
                            }
                        }

                    }

                    doReorderByScheduleMutation = random.NextDouble() < reorderByScheduleMutationThreshold;

                    if (doReorderByScheduleMutation)
                    {
                        patients = new List<int[]>();

                        for (int k = 0; k < nursePaths.GetLength(1); k++)
                        {
                            if (nursePaths[j, k] == null) break;

                            patient = new int[] { k, problem.patients[(int)nursePaths[j, k]].start_time, (int)nursePaths[j, k] };

                            patients.Add(patient);
                        }
                        patients = patients.OrderBy(e => e[1]).ToList();

                        for (int k = 0; k < nursePaths.GetLength(1); k++)
                        {
                            if (nursePaths[j, k] == null) break;

                            nursePaths[j, k] = patients[k][2];
                        }
                    }
                }

            }
            return children;
        }
        
        private void offspringSelection_HammingDistanceCrowding(
            Chromosome[] children, Chromosome[] parents, int numIndividualsToComapreDistance, double crowdingWeight)
        {
            // compare "distance" or degree of similarity between top x individuals in population
            // only needs to be different from the good solutions to avoid premature convergance
            // might not be as wide of a search space as if compared distance to whole population

            // replaces bottom y individuals from population where y = number of children
            // if and only if children are better than parents, in terms of distance-weighted fitness measure

            int sumChildDistance;
            int sumParentDistance;
            double childWeightedFitness;
            double parentWeightedFitness;

            for (int i = 0; i < children.Length; i++) 
            {
                sumChildDistance = 0;
                sumParentDistance = 0;
                for (int j = 0; j <= numIndividualsToComapreDistance; j++)
                {
                    for (int k = 0; k < children[i].nursePaths.GetLength(0); k++)
                    {
                        if (children[i].nursePaths[k, 0] == null) continue;

                        for (int l = 0; l < children[i].nursePaths.GetLength(1); l++)
                        {
                            if (children[i].nursePaths[k, l] == null) break;

                            if (children[i].nursePaths[k, l] != population.population[j].nursePaths[k, l])
                                sumChildDistance++;

                            if (parents[i].nursePaths[k, l] != population.population[j].nursePaths[k, l])
                                sumParentDistance++;
                            
                        }
                    }
                }

                childWeightedFitness = children[i].calcFitness(problem) - sumChildDistance / numIndividualsToComapreDistance * crowdingWeight;
                parentWeightedFitness = parents[i].calcFitness(problem) - sumParentDistance / numIndividualsToComapreDistance * crowdingWeight;

                if (childWeightedFitness < parentWeightedFitness)
                {
                    population.population[population.population.Length - 1 - i] = children[i];
                    countChildFitter++;
                }
                else
                {
                    countParentFitter++;
                }

            }

        }

        private void offspringSelection_fitnessProportionate(Chromosome[] children, Chromosome[] parents)
        {
            double childFitness;
            double parentFitness;
            double sumFitness;

            for (int i = 0; i < children.Length; i++) 
            {
                childFitness = children[i].calcFitness(problem);
                parentFitness = parents[i].calcFitness(problem);

                sumFitness = childFitness + parentFitness;

                if(random.NextDouble() < (childFitness / sumFitness))
                {
                    population.population[population.population.Length - 1 - i] = children[i];
                    countChildFitter++;
                }
                else
                {
                    countParentFitter++;
                }
            }
        }

        

    }


}
//// TEsting threading
//class Program2
//{
//    static Barrier barrier = new Barrier(3); // Specify the number of threads

//    static void Main(string[] args)
//    {
//        // Create threads
//        Thread thread1 = new Thread(ThreadMethod);
//        Thread thread2 = new Thread(ThreadMethod);
//        Thread thread3 = new Thread(ThreadMethod);

//        // Start threads
//        thread1.Start();
//        thread2.Start();
//        thread3.Start();

//        // Wait for all threads to finish
//        thread1.Join();
//        thread2.Join();
//        thread3.Join();

//        Console.WriteLine("All threads completed.");
//    }

//    static void ThreadMethod()
//    {
//        for (int i = 0; i < 5; i++) // Each thread will have 5 iterations
//        {
//            // Perform some work
//            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} - Iteration {i + 1}");

//            // Signal that this thread has reached the barrier
//            barrier.SignalAndWait();

//            // Perform data swapping after all threads have reached the barrier
//            if (barrier.CurrentPhaseNumber == 0)
//            {
//                Console.WriteLine("Data swapping...");
//                // Simulate data swapping
//                Thread.Sleep(1000);
//            }

//            // Continue working after data swapping
//            // Here you can add the code for further processing
//        }
//    }
//}
