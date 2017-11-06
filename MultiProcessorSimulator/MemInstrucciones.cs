using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiProcessorSimulator
{
    class MemInstrucciones
    {
        public int[] I1;
        public int[] I2;

        public MemInstrucciones()
        {
            I1 = new int[384];
            I2 = new int[256];
        }
    }
}
