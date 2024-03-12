
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Program
{
    internal class Population
    {
        private TSP problem;
        public Chromosome[] population { get; private set; }
        private Dictionary<Chromosome, double> fitnessLookUp;
        public int popSize {  get; private set; }

        public Population(int popSize, TSP problem) 
        {
            this.popSize = popSize;
            this.problem = problem;
            this.population = new Chromosome[popSize];
            this.fitnessLookUp = new Dictionary<Chromosome, double>();
        }

        public void inintializeEvenPatientSplit(bool shuffled = true)
        {
            int numNurses = problem.nbr_nurses;
            int numPatients = problem.patients.Count;
            int numPatientsPerNurse = numPatients / numNurses + 1;

            for (int i = 0; i < popSize; i++)
            {
                Chromosome chromosome = new Chromosome(numNurses, numPatients);

                int nurseID = 0;
                int countPatients = 0;
                List<int> patientsList = problem.patients.Keys.ToList();

                if (shuffled)
                    patientsList = Population.shuffleList(patientsList);

                foreach (int patientID in patientsList)
                {
                    chromosome.nursePaths[nurseID, countPatients] = patientID;

                    if (countPatients >= numPatientsPerNurse)
                    {
                        countPatients = 0;
                        nurseID++;
                    }
                    else
                        countPatients++;

                }
                population[i] = chromosome;
            }
        }

        private static List<int> shuffleList(List<int> list)
        {
            Random random = new Random();
            int n = list.Count;
            int k;
            int value;
            while (n > 0)
            {
                k = random.Next(n);
                value = list[k];
                list[k] = list[n - 1];
                list[n - 1] = value;
                n--;
            }
            return list;
        }

        public void calcFitness()
        {
            double fitness;
            for (int i = 0; i < population.Length; i++)
            {
                if (population[i].fitness != null)
                    continue;

                if (fitnessLookUp.TryGetValue(population[i], out fitness))
                {
                    population[i].fitness = fitness;
                    continue;
                }

                fitness = population[i].calcFitness(problem);

                fitnessLookUp[population[i]] = fitness;

            }
        }
        
        public void sort()
        {
            Array.Sort(population);
        }
    }
}
