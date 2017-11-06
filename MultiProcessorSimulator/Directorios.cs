using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiProcessorSimulator
{
    class Directorios
    {
        public int[,] D1;
        public int[,] D2;

        public Directorios()
        {
            D1 = new int[16, 4];
            D2 = new int[8, 4];
        }
    }
}
