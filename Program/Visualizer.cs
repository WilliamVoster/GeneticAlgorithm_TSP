using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Program
{
    internal class Visualizer
    {
        TSP problem;
        string pythonFilePath;
        string jsonFilePath;
        public Visualizer(TSP problem, string pythonFilePath, string jsonFilePath) 
        {
            this.problem = problem;
            this.pythonFilePath = pythonFilePath;
            this.jsonFilePath = jsonFilePath;
        }

        private (List<int[]>, Dictionary<int, int[]>[]) calcEdges(Chromosome chromosome)
        {
            //int numEdges;
            int[] edge = new int[2];
            int[] coord = new int[2];
            List<int[]> edges = new List<int[]>();
            Dictionary<int, int[]>[] positionsList = new Dictionary<int, int[]>[chromosome.maxNumNurses];
            

            for (int i = 0; i < chromosome.nursePaths.GetLength(0); i++)
            {
                if (chromosome.nursePaths[i, 0] == null) continue;

                // Add depot location
                edge = new int[] { 0, 0 };
                coord = new int[] { problem.depot.x_coord, problem.depot.y_coord };
                Dictionary<int, int[]> positions = new Dictionary<int, int[]>();
                positions.Add(0, coord);
                positionsList[i] = positions;

                for (int j = 0; j < chromosome.nursePaths.GetLength(1); j++)
                {
                    if (chromosome.nursePaths[i, j] == null) break;

                    edge = new int[] { edge[1], (int)chromosome.nursePaths[i, j] };
                    edges.Add(edge);

                    coord = new int[] { problem.patients[edge[1]].x_coord, problem.patients[edge[1]].y_coord };
                    positionsList[i].Add((int)chromosome.nursePaths[i, j], coord);
                }

                // Add edge back to depot again
                edge = new int[] { edge[1], 0 };
                edges.Add(edge);
            }

            return (edges, positionsList);
        }

        public void visualize(Chromosome chromosome) 
        {
            (List<int[]>, Dictionary<int, int[]>[]) edgesAndPositions = calcEdges(chromosome);

            string jsonString = JsonConvert.SerializeObject(edgesAndPositions);

            File.WriteAllText(jsonFilePath, jsonString);



            string pythonInterpreter = "python";

            ProcessStartInfo startInfo = new ProcessStartInfo();        // Container for process details
            startInfo.FileName = pythonInterpreter;                     // Set the filename (Python interpreter)
            startInfo.Arguments = pythonFilePath + " " + jsonFilePath;  // Set the arguments (Python script)
            startInfo.UseShellExecute = false;                          // Do not use the shell to execute the process
            startInfo.RedirectStandardOutput = true;                    // Redirect standard output for capturing output

            using (Process process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                Console.WriteLine(output);
            }

        }

    }
}
