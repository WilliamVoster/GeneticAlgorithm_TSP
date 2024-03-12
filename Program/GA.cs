using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;


namespace Program
{
    internal class GA
    {
        public static void Main(string[] args)
        {
            ////Test
            //int l;
            //for (l = 0; l < 10; l++)
            //    if (l == 2) break;
            //Console.WriteLine(l);
            //return;

            string solutionDir = Directory.GetCurrentDirectory() + "\\..\\..\\..\\";

            TSP training_problem_0 = TSP.readJSON(solutionDir + "Data/train_0.json");

            Visualizer visualizer = new Visualizer(
                training_problem_0,
                solutionDir + "/Program/plotting.py", 
                solutionDir + "/Program/plottingData/chromosome.json");


            GA geneticAlgorithm = new GA(training_problem_0, 100, 5000, visualizer);
            geneticAlgorithm.run();



            //TODO:
            // x Fix fitness function to incorporate constraints
            // ? calcfitness() on population is faster than calling calc on every new child
            // x better parameters, for num children to create
            // x implement mutation
            // x better seleciton, fitness proportional selection
            // x visualization, in python?
            // _ add big dot for the depot in visualization
            // _ fix issue with duplicate stops, within same route


        }

        TSP problem;
        Visualizer visualizer;
        int numIterations;
        Population population;
        int populationSize;
        Random random;
        int seed = 1;
        int numParentsToSelect;
        double reorderMutationThreshold;
        double transferMutationThreshold;
        public GA(TSP problem, int populationSize, int numIterations, Visualizer visualizer) 
        {
            this.problem = problem;
            this.populationSize = populationSize;
            this.numIterations = numIterations; 
            this.visualizer = visualizer;
            this.random = new Random(seed);

            // Mutation probabilities per patient
            reorderMutationThreshold = 0.05;
            transferMutationThreshold = 0.05;

            numParentsToSelect = (int)((double)populationSize * 0.3);
            if (numParentsToSelect % 2 != 0)
                numParentsToSelect--;
        }

        public void run()
        {

            // Initialization
            population = new Population(50, problem);
            population.inintializeEvenPatientSplit(false);

            problem.calcAvgNumPatientsPerNurse();
            population.calcFitness();
            for (int i = 0; i < numIterations; i++)
            {
                //population.calcFitness(); // all new added children have calculated fitnesses
                population.sort();

                if (i == 0) { visualizer.visualize(population.population[0]); }


                // Parent Selection
                //Chromosome[] parents = elitistSelection(numParentsToSelect);
                Chromosome[] parents = fitnessProportionateSelection(numParentsToSelect);


                // Crossover
                Chromosome[] children = crossover(parents);


                // Mutation
                children = mutation(children);


                // Offspring Selection
                offspringSelection_HammingDistanceCrowding(children, parents, 5, 1.0);


                // Logging and visuals
                if (i % 100 == 0)
                    Console.WriteLine("Generation:  " + i + "\tFitness: " + population.population[0].fitness);
                //visualizer.visualize(population.population[0]);

                if (i == numIterations - 1)
                    visualizer.visualize(population.population[0]);
            }

            // Termination

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

            // Make copies of parents, called children, and modify them instead
            for (int i = 0; i < parents.Length; i++)
            {
                children[i] = (Chromosome)parents[i].Clone();
            }

            for (int i = 0; i < children.Length; i += 2)
            {
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
                }

                // Patients from one nurse in parent2 are deleted in parent1
                for (int j = 0; j < children[i + 1].numPatients; j++)
                {
                    if (children[i + 1].nursePaths[nurseIndex2, j] == null) break;

                    patient2 = (int)children[i + 1].nursePaths[nurseIndex2, j];

                    //children[i].deleteByValue(patient2);

                    patients2[j] = patient2;

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

        private void clusteringKMeans()
        {

        }

        private Chromosome[] mutation(Chromosome[] children) 
        {
            // Two mutation types:
            // Reorder patient within nurse route
            // Give patient to another nurse route

            int?[,] nursePaths;
            bool doReorderMutation;
            bool doTransferMutation;
            int nurseIndex;
            int patientIndex;
            int patientValue;

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
                }
            }

        }


        

    }


}

