using Newtonsoft.Json.Bson;
using System;
using System.IO;
using System.Reflection;


namespace Program
{
    internal class GA
    {
        public static void Main(string[] args)
        {

            string solutionDir = Directory.GetCurrentDirectory() + "\\..\\..\\..\\";

            TSP training_problem_0 = TSP.readJSON(solutionDir + "Data/train_0.json");

            GA geneticAlgorithm = new GA(training_problem_0, 100);

            geneticAlgorithm.run();

            //Population population = new Population(50, problem);

            //population.calcFitness();

            // TODO remember to set fitness to null on chromosome if mutate/change genome!!

            Console.WriteLine("test");
            //Console.ReadLine();


        }

        TSP problem;
        int numIterations;
        Population population;
        Random random;
        public GA(TSP problem, int numIterations) 
        {
            this.problem = problem;
            this.numIterations = numIterations; 
            this.random = new Random();
        }

        public void run()
        {

            //Initialization
            population = new Population(50, problem);
            population.inintializeEvenPatientSplit();

            problem.calcAvgNumPatientsPerNurse();
            int numParents = 2;
            for (int i = 0; i < numIterations; i++)
            {
                population.calcFitness();
                population.sort();

                //Parent Selection
                Chromosome[] parents = elitistSelection(numParents); // TODO: instead of copying and creating new parents, just look at the original individuals in population


                //Crossover
                Chromosome[] children = crossover(parents);

                //Mutatuon
                // TODO, remember to set child fitness = null


                //Offspring Selection
                offspringSelection_HammingDistanceCrowding(children, parents, 5, 0.3);

            }

            //Termination

        }

        private Chromosome[] elitistSelection(int n)
        {
            Chromosome[] selected = new Chromosome[n];

            //for (int i = 0; i < n; i++)
            //{
            //    selected[i] = population.population[i];
            //    //selected[i].fitness = null;
            //}

            //Chromosome[] populationCopy = (Chromosome[])population.population.Clone();

            Array.Copy(population.population, 0, selected, 0, n);

            return selected;
        }

        private void fitnessProportionateSelection()
        {
            
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

            // Make copies of parents, children, and modify them instead
            for (int i = 0; i < parents.Length; i++)
            {
                children[i] = (Chromosome)parents[i].Clone();
            }

            for (int i = 0; i < children.Length; i += 2)
            {
                children[i].updateNumNurses();
                children[i + 1].updateNumNurses();
                nurseIndex1 = random.Next(0, children[i].numNurses);
                nurseIndex2 = random.Next(0, children[i + 1].numNurses);

                // Patients from one nurse in parent1 are deleted in parent2
                for (int j = 0; j < children[i].numPatients; j++)
                {
                    if (children[i].nursePaths[nurseIndex1, j] == null)
                        break;

                    patient1 = (int)children[i].nursePaths[nurseIndex1, j];

                    children[i + 1].deleteByValue(patient1);

                    patients1[j] = patient1;
                }

                // Patients from one nurse in parent2 are deleted in parent1
                for (int j = 0; j < children[i + 1].numPatients; j++)
                {
                    if (children[i + 1].nursePaths[nurseIndex2, j] == null)
                        break;

                    patient2 = (int)children[i + 1].nursePaths[nurseIndex2, j];

                    children[i].deleteByValue(patient2);

                    patients2[j] = patient2;

                }

                // For unvisited patients in parent find a nurse to visit them
                children[i].insertByDistance(patients1, problem.travel_times);
                children[i + 1].insertByDistance(patients2, problem.travel_times);

                children[i].fitness = null;
                children[i + 1].fitness = null;

            }

            return children;
        }

        private void clusteringKMeans()
        {

        }
        
        private void offspringSelection_HammingDistanceCrowding(Chromosome[] children, Chromosome[] parents, int numIndividualsToComapreDistance, double crowdingWeight)
        {
            // compare "distance" or degree of similarity between top x individuals in population
            // only needs to be different from the good solutions to avoid premature convergance
            // might not be as wide of a search space as if compared distance to whole population

            // replaces bottom y individuals from population where y = number of children

            double diversity;
            int sumChildDistance;
            int sumParentDistance;
            double childWeightedFitness;
            double parentWeightedFitness;

            for (int i = 0; i <= children.Length; i++) 
            {
                sumChildDistance = 0;
                sumParentDistance = 0;
                for (int j = 0; j <= numIndividualsToComapreDistance; j++)
                {
                    for (int k = 0; i < children[i].numNurses; k++)
                    {
                        if (children[i].nursePaths[k, 0] == null)
                            break;

                        for (int l = 0; l <= children[i].numPatients; l++)
                        {
                            if (children[i].nursePaths[k, l] == null)
                                break;

                            if (children[i].nursePaths[k, l] != population.population[j].nursePaths[k, l])
                                sumChildDistance++;

                            if (parents[i].nursePaths[k, l] != population.population[j].nursePaths[k, l])
                                sumParentDistance++;
                            
                        }
                    }
                }

                childWeightedFitness = children[i].calcFitness(problem) + sumChildDistance / numIndividualsToComapreDistance * crowdingWeight;
                parentWeightedFitness = parents[i].calcFitness(problem) + sumParentDistance / numIndividualsToComapreDistance * crowdingWeight;

                if (childWeightedFitness < parentWeightedFitness)
                {
                    population.population[-i - 1] = children[i];
                }
            }

        }
    

    
    
    }


}

