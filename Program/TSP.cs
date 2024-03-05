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


        public static TSP readJSON(string filepath)
        {
            string jsonString = File.ReadAllText(filepath);
            TSP myProblem = JsonConvert.DeserializeObject<TSP>(jsonString);
            return myProblem;
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
