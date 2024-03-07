
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
        private Chromosome[] population;
        private Dictionary<Chromosome, double> fitnessLookUp;
        public int popSize {  get; private set; }

        public Population(int popSize, TSP problem) 
        {
            this.popSize = popSize;
            this.problem = problem;
            this.population = new Chromosome[popSize];
            this.fitnessLookUp = new Dictionary<Chromosome, double>();
        }

        public void inintializeEvenPatientSplit()
        {
            int numNurses = problem.nbr_nurses;
            int numPatients = problem.patients.Count;
            int numPatientsPerNurse = numPatients / numNurses + 1;

            for (int i = 0; i < popSize; i++)
            {
                Chromosome chromosome = new Chromosome(numNurses, numPatients);

                int nurseID = 0;
                int countPatients = 0;
                foreach (int patientID in problem.patients.Keys)
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

        public void calcFitness()
        {
            //Chromosome a = new Chromosome(5, 10);
            //this.fitnessLookUp.Add(a, 0.12);

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

                population[i].fitness = fitness;

                fitnessLookUp[population[i]] = fitness;

            }
        }
        
    }
}
