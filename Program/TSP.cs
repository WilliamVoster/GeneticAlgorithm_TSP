using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Program
{
    internal class TSP
    {
        public int nbr_nurses {  get; set; }
        public string instance_name { get; set; }
        public int capacity_nurse { get; set; }
        public Depot depot { get; set; }
        public Dictionary<int, Patient> patients { get; set; }
        public Double[,] travel_times { get; set; }
        public int avgNumPatientsPerNurse { get; private set; }
        public int[] closestStops { get; private set; }


        public static TSP readJSON(string filepath)
        {
            string jsonString = File.ReadAllText(filepath);
            TSP myProblem = JsonConvert.DeserializeObject<TSP>(jsonString);
            return myProblem;
        }

        public void calcAvgNumPatientsPerNurse()
        {
            double sumPatientDemand = 0.0;
            foreach (var patient in patients.Values)
            {
                sumPatientDemand += patient.demand;
            }
            avgNumPatientsPerNurse = (int)(sumPatientDemand / patients.Count / capacity_nurse);
        }

        public void saveClosestStops()
        {
            closestStops = new int[travel_times.GetLength(0)];

            for (int i = 0; i < travel_times.GetLength(0); i++)
            {
                double closest = Double.MaxValue;
                for(int j = 0; j < travel_times.GetLength(1); j++)
                {
                    if (i == j) continue;
                    if (i == 0 || j == 0) continue;

                    if (travel_times[i, j] < closest)
                    {
                        closest = travel_times[i, j];

                        closestStops[i] = j;
                    }
                }
            }
        }

    }

    internal class Depot
    {
        public int return_time { get; set; }
        public int x_coord { get; set; }
        public int y_coord { get; set; }
    }
    internal class Patient
    {
        public int x_coord { get; set; }
        public int y_coord { get; set; }
        public int demand { get; set; }
        public int start_time { get; set; }
        public int end_time { get; set; }
        public int care_time { get; set; }
    }
}
