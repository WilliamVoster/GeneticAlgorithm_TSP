using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

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
            ScriptEngine engine = Python.CreateEngine();
            ScriptScope scope = engine.CreateScope();
            ScriptSource script = engine.CreateScriptSourceFromFile(file);
            script.Execute(scope);

            // You can call Python functions or access variables from the scope
            dynamic addResult = scope.GetVariable("add_numbers")(5, 7);
            int xValue = scope.GetVariable<int>("x");
            int yValue = scope.GetVariable<int>("y");

            Console.WriteLine("Result of add_numbers(5, 7): " + addResult);
            Console.WriteLine("Value of x: " + xValue);
            Console.WriteLine("Value of y: " + yValue);

        }
    }
}
