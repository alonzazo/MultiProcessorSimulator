using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiProcessorSimulator
{
    class MemInstrucciones
    {
        public int[] I0;
        public int[] I1;

        public MemInstrucciones()
        {
            I0 = new int[384];
            I1 = new int[256];
        }
    }
}
