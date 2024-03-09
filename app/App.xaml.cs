using System.Configuration;
using System.Data;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using app;

namespace app
{
    /// <summary>
    /// App starting point
    /// </summary>
    public partial class App : Application
    {
        [STAThread] // Single threaded application
        public static void Main(string[] args)
        {
            var application = new app();
            var window = new MainWindow();

            ////window.Show();
            application.Run(window);


            int populationSize = 100;
            int chromosomeLength = 8;
            int[][] targets = {
                new int[] { 1, 2, 3, 4, 5, 6, 7, 8 },
                new int[] { 8, 7, 6, 5, 4, 3, 2, 1 }
            };
            double crossoverRate = 0.8;
            double mutationRate = 0.1;
            int maxGenerations = 1000;

            GeneticAlgorithm geneticAlgorithm = new GeneticAlgorithm(
                populationSize, chromosomeLength, targets, crossoverRate, mutationRate, maxGenerations);
            geneticAlgorithm.Evolve();

        }

        //public App()
        //{

        //}
    }

    public class Chromosome
    {
        public int[] Genes { get; private set; }
        public int Fitness { get; set; }

        private Random random;

        public Chromosome(int length, Random random)
        {
            Genes = new int[length];
            this.random = random;
            Initialize();
        }

        public void Initialize()
        {
            List<int> genesList = Enumerable.Range(1, Genes.Length).ToList();
            for (int i = 0; i < Genes.Length; i++)
            {
                int index = random.Next(genesList.Count);
                Genes[i] = genesList[index];
                genesList.RemoveAt(index);
            }
        }

        public void CalculateFitness(int[] target)
        {
            Fitness = 0;
            for (int i = 0; i < Genes.Length; i++)
            {
                if (Genes[i] == target[i])
                    Fitness++;
            }
        }

        public Chromosome Order1Crossover(Chromosome partner, Random random)
        {
            Chromosome child = new Chromosome(Genes.Length, random);
            int startPos = random.Next(Genes.Length);
            int endPos = random.Next(startPos + 1, Genes.Length);

            for (int i = startPos; i < endPos; i++)
            {
                child.Genes[i] = Genes[i];
            }

            for (int i = 0; i < partner.Genes.Length; i++)
            {
                if (!child.Genes.Contains(partner.Genes[i]))
                {
                    for (int j = 0; j < child.Genes.Length; j++)
                    {
                        if (child.Genes[j] == 0)
                        {
                            child.Genes[j] = partner.Genes[i];
                            break;
                        }
                    }
                }
            }
            return child;
        }

        public void SwapMutation(Random random)
        {
            int index1 = random.Next(Genes.Length);
            int index2 = random.Next(Genes.Length);
            int temp = Genes[index1];
            Genes[index1] = Genes[index2];
            Genes[index2] = temp;
        }

        public void InverseMutation(Random random)
        {
            int startPos = random.Next(Genes.Length);
            int endPos = random.Next(startPos + 1, Genes.Length);
            Array.Reverse(Genes, startPos, endPos - startPos);
        }
    }

    public class Population
    {
        public List<Chromosome> Chromosomes { get; private set; }
        public int Generation { get; private set; }

        public Population(int populationSize, int chromosomeLength, Random random)
        {
            Chromosomes = new List<Chromosome>();
            Generation = 1;
            for (int i = 0; i < populationSize; i++)
            {
                Chromosomes.Add(new Chromosome(chromosomeLength, random));
            }
        }

        public void CalculateFitness(int[][] targets)
        {
            foreach (var chromosome in Chromosomes)
            {
                int fitness = 0;
                foreach (var target in targets)
                {
                    for (int i = 0; i < chromosome.Genes.Length; i++)
                    {
                        if (chromosome.Genes[i] == target[i])
                            fitness++;
                    }
                }
                chromosome.Fitness = fitness;
            }
        }

        public Chromosome BasicSelection()
        {
            Chromosomes = Chromosomes.OrderByDescending(x => x.Fitness).ToList();
            return Chromosomes[0]; // Return the fittest chromosome
        }

        public Chromosome TournamentSelection(Random random, int tournamentSize)
        {
            List<Chromosome> tournament = new List<Chromosome>();
            for (int i = 0; i < tournamentSize; i++)
            {
                tournament.Add(Chromosomes[random.Next(Chromosomes.Count)]);
            }
            return tournament.OrderByDescending(x => x.Fitness).First();
        }

        public void GenerateNextGeneration(double crossoverRate, double mutationRate, Random random)
        {
            List<Chromosome> newPopulation = new List<Chromosome>();

            while (newPopulation.Count < Chromosomes.Count)
            {
                Chromosome parent1 = TournamentSelection(random, 5);
                Chromosome parent2 = TournamentSelection(random, 5);

                if (random.NextDouble() < crossoverRate)
                {
                    Chromosome child = parent1.Order1Crossover(parent2, random);
                    if (random.NextDouble() < mutationRate)
                    {
                        child.SwapMutation(random);
                    }
                    if (random.NextDouble() < mutationRate)
                    {
                        child.InverseMutation(random);
                    }
                    newPopulation.Add(child);
                }
            }

            Chromosomes = newPopulation;
            Generation++;
        }
    }

    public class GeneticAlgorithm
    {
        private Population population;
        private int[][] targets;
        private double crossoverRate;
        private double mutationRate;
        private int maxGenerations;
        private Random random;

        public GeneticAlgorithm(int populationSize, int chromosomeLength, int[][] targets, double crossoverRate, double mutationRate, int maxGenerations)
        {
            random = new Random();
            population = new Population(populationSize, chromosomeLength, random);
            this.targets = targets;
            this.crossoverRate = crossoverRate;
            this.mutationRate = mutationRate;
            this.maxGenerations = maxGenerations;
        }

        public void Evolve()
        {
            int generation = 1;
            while (generation <= maxGenerations)
            {
                population.CalculateFitness(targets);
                Chromosome fittest = population.BasicSelection();
                Console.WriteLine($"Generation {generation}: Fitness = {fittest.Fitness}");
                if (fittest.Fitness == targets.Length * targets[0].Length)
                {
                    Console.WriteLine($"Target reached in {generation} generations");
                    break;
                }
                population.GenerateNextGeneration(crossoverRate, mutationRate, random);
                generation++;
            }
        }
    }

    
}
