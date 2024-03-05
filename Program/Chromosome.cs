using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Program
{
    internal class Chromosome
    {
        private int[] data;
        public Chromosome(int size) 
        {
            this.data = new int[size];
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Chromosome other = (Chromosome)obj;

            for (int i = 0; i < this.data.Length; i++)
            {
                if (other.data[i] != this.data[i])
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + this.data.GetHashCode();
                return hash;
            }
        }

    }
}
