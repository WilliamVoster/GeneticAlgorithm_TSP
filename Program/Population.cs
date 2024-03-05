
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

        public void inintialize()
        {

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

                //TODO: calc distance travelled as fitness

            }
        }
        
    }
}
