using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Program
{
    internal class Chromosome
    {
        public int?[,] nursePaths;
        public double? fitness { get; set; }
        private int numNurses;
        private int numPatients;
        public Chromosome(int numNurses, int numPatients) 
        {
            
            this.nursePaths = new int?[numNurses, numPatients];
            this.numNurses = numNurses;
            this.numPatients = numPatients;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Chromosome other = (Chromosome)obj;

            for (int i = 0; i < this.numNurses; i++)
            {
                for (int j = 0; j < this.numPatients; j++)
                {
                    if (nursePaths[i, j] == null)
                        break;

                    if (other.nursePaths[i, j] != this.nursePaths[i, j])
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

                for (int i = 0; i < this.numNurses; i++)
                {
                    for (int j = 0; j < this.numPatients; j++)
                    {
                        if (nursePaths[i, j] == null)
                            break;

                        hash = hash * 23 + nursePaths[i, j].GetHashCode();
                    }
                }
                return hash;
            }
        }

    }
}
