using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Program
{
    internal class Visualize
    {
        string file;
        public Visualize(string pythonFilePath) 
        {
            this.file = pythonFilePath;
        }

        public void Run() 
        {

            NursePath nursePath = new NursePath();
        }

        private class NursePath
        {
            public NursePath()
            {
                Console.WriteLine("test");
            }
        }
    }
}
