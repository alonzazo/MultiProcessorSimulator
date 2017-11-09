using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiProcessorSimulator
{
    class Program
    {

        static void Main(string[] args)
        {
            Simulador simulador = new Simulador();
            simulador.correr();
        }

        
    }
}
