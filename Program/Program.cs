using System;
using System.IO;
using System.Reflection;


namespace Program
{
    internal class Program
    {
        public static void Main(string[] args)
        {

            string solutionDir = Directory.GetCurrentDirectory() + "\\..\\..\\..\\";

            TSP problem = TSP.readJSON(solutionDir + "Data/train_0.json");

            Population population = new Population(50, problem);

            population.calcFitness();

            Console.WriteLine("test");
            Console.ReadLine();


        }
    }


}

