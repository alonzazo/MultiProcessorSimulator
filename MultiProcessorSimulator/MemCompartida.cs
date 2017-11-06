using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiProcessorSimulator
{
    class MemCompartida
    {
        public int[] MC1;
        public int[] MC2;

        public MemCompartida()
        {
            MC1 = new int[64];
            MC2 = new int[32];
        }
    }
}
