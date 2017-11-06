using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiProcessorSimulator
{
    class Program
    {
        private MemCompartida memCompartida;
        private MemInstrucciones memInstrucciones;

        private CacheDatos cacheDatosN0;
        private CacheDatos cacheDatosN1;
        private CacheDatos cacheDatosN2;

        private CacheInstrucciones cacheInstruccionesN1;
        private CacheInstrucciones cacheInstruccionesN2;
        private CacheInstrucciones cacheInstruccionesN3;

        private Directorios directorios;
        private Contexto contextoP0;
        private Contexto contextoP1;

        private Registros registrosN0;
        private Registros registrosN1;
        private Registros registrosN2;
        private RegistrosInstruccion rInstruccionN0;
        private RegistrosInstruccion rInstruccionN1;
        private RegistrosInstruccion rInstruccionN2;

        static void Main(string[] args)
        {
        }
    }
}
