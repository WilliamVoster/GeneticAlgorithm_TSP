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

            TSPProblem problem = TSPProblem.readJSON(solutionDir + "Data/train_0.json");

            Console.WriteLine("test");
            Console.ReadLine();


        }
    }


}

