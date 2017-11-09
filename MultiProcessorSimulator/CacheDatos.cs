using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiProcessorSimulator
{
    class CacheDatos
    {
        public int[,] CD;

        public CacheDatos()
        {
            CD = new int[4, 6];

            //Inicializo los numero de bloque con -1
            for(int i = 0; i<4; ++i)
            {
                CD[i, 0] = -1;
            }
        }
    }
}
