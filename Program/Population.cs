
using System;
using System.Collections.Generic;
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
        private Dictionary<Chromosome, Double> fitnessLookUp;

        public Population(int popSize, TSP problem) 
        {
            this.problem = problem;
            this.population = new Chromosome[popSize];
            this.fitnessLookUp = new Dictionary<Chromosome, Double>();
        }

        private void calcFitness()
        {
            foreach (Chromosome chromosome in population)
            {

            }
        }
        
    }
}
