
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
        private Dictionary<Chromosome, Double> population;
        private TSP problem;
        public Population(TSP problem) 
        {
            this.problem = problem;
            this.population = new Dictionary<Chromosome, Double>();
        }

        
    }
}
