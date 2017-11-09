using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiProcessorSimulator
{
    class Simulador
    {
        private MemCompartida memCompartida;
        private MemInstrucciones memInstrucciones;

        private CacheDatos cacheDatosN0;
        private CacheDatos cacheDatosN1;
        private CacheDatos cacheDatosN2;

        private CacheInstrucciones cacheInstruccionesN0;
        private CacheInstrucciones cacheInstruccionesN1;
        private CacheInstrucciones cacheInstruccionesN2;

        private Directorios directorios;
        private Contexto contextoP0;
        private Contexto contextoP1;

        private Registros registrosN0;
        private Registros registrosN1;
        private Registros registrosN2;
        private RegistrosInstruccion rInstruccionN0;
        private RegistrosInstruccion rInstruccionN1;
        private RegistrosInstruccion rInstruccionN2;

        public void correr()
        {
            string [] hilosP0 = solicitarHilos(0);
            string[] hilosP1 = solicitarHilos(1);
            int quantum = solicitarQuantum();
            //Inicializo estructuras
            inicializar();
            //Lleno memoria de instrucciones
            guardarInstrucciones(memInstrucciones.I0, hilosP0);
            guardarInstrucciones(memInstrucciones.I1, hilosP1);
            //Imprimo memoria de instrucciones para verificar
            printMemoria(memInstrucciones.I0);
            printMemoria(memInstrucciones.I1);
            //Pregunto por modo de ejecucion
            Console.WriteLine("\n");
            Console.WriteLine("Elija su modo de ejecución: Digite 1 para lento o 2 para rápido");
            int modo = Int32.Parse(Console.ReadLine());
            //Asignar a cada procesador sus hilos correpondientes
        }

        public void inicializar()
        {
            //Inicializo las memorias de instrucciones
            memInstrucciones = new MemInstrucciones();
            //Inicializo la memoria compartida
            memCompartida = new MemCompartida();
            //Inicializo registros
            registrosN0 = new Registros();
            registrosN1 = new Registros();
            registrosN2 = new Registros();
            //Inicializo registros de instrucciones
            rInstruccionN0 = new RegistrosInstruccion();
            rInstruccionN1 = new RegistrosInstruccion();
            rInstruccionN2 = new RegistrosInstruccion();
            //Inicializo caches
            cacheDatosN0 = new CacheDatos();
            cacheDatosN1 = new CacheDatos();
            cacheDatosN2 = new CacheDatos();
            //Inicializo caches de instrucciones
            cacheInstruccionesN0 = new CacheInstrucciones();
            cacheInstruccionesN1 = new CacheInstrucciones();
            cacheInstruccionesN2 = new CacheInstrucciones();
            //Inicializo directorios
            directorios = new Directorios();
        }

        public string[] solicitarHilos(int proc)
        {
            String path = System.IO.Directory.GetParent(System.IO.Directory.GetParent(Environment.CurrentDirectory).FullName).FullName;
            String dP0 = System.IO.Path.Combine(path, "P0");
            String dP1 = System.IO.Path.Combine(path, "P1");
            string[] lines;
            if (proc == 0)
            {
                lines = Directory.GetFiles(dP0, "*.txt", SearchOption.TopDirectoryOnly);
            }
            else
            {
                lines = Directory.GetFiles(dP1, "*.txt", SearchOption.TopDirectoryOnly);
            }
            int i = 1;
            foreach (string line in lines)
            {
                string[] aux = line.Split('\\');

                Console.WriteLine("\t" + i + ". " + aux[aux.Length - 1]);
                ++i;
            }
            Console.WriteLine("Digite los numeros correspondientes a cada hilo a correr separado por un espacio");
            var res = Console.ReadLine();
            string[] splitRes = res.Split(' ');
            if (splitRes.Length <= lines.Length)
            {
                string[] hilos = new string[splitRes.Length];
                for (int n = 0; n < splitRes.Length; ++n)
                {
                    if (Int32.Parse(splitRes[n]) <= lines.Length)
                    {
                        hilos[n] = lines[Int32.Parse(splitRes[n]) - 1];
                    }
                    else
                    {
                        Console.WriteLine("El numero que digitó no corresponde a algún hilillo. Intente de nuevo");
                    }

                }
                /*    Console.WriteLine("*******************");
                    foreach (string p in hilosP0)
                    {
                        Console.WriteLine("\t" + p);
                    }*/
                return hilos;
            }
            else
            {
                Console.WriteLine("La cantidad de números que digitó, excede el número de hilillos disponible. Intente de nuevo");
            }


            Console.ReadLine();
            return null;

        }

        public int solicitarQuantum()
        {
            Console.WriteLine("Digite el quantum que va a asignar a los procesadores");
            var res = Console.ReadLine();
            return Int32.Parse(res);
        }

        public void guardarInstrucciones(int[] mem, string[] hilos)
        {
            string[] lines;
            int bloque = 0;
            for (int i = 0; i< hilos.Length; ++i)
            {
                lines = System.IO.File.ReadAllLines(hilos[i]);
                foreach (string line in lines)
                {
                    string[] aux = line.Split(' ');
                    mem[bloque] = Int32.Parse(aux[0]);
                    mem[bloque+1] = Int32.Parse(aux[1]);
                    mem[bloque+2] = Int32.Parse(aux[2]);
                    mem[bloque+3] = Int32.Parse(aux[3]);
                    bloque += 4;
                    if(bloque >= mem.Length)
                    {
                        break;
                    }
                }
                if (bloque >= mem.Length)
                {
                    break;
                }
            }
            
        }

        public void printMemoria(int [] mem)
        {
            int cont = 0;
            for(int i = 0; i< mem.Length; ++i)
            {
                ++cont;
                if( i % 64 == 0)
                {
                    Console.Write("\n");
                }
                Console.Write(mem[i]);
                if(cont == 4)
                {
                    Console.Write(" ");
                    cont = 0;
                }
              
            }
        }
    }
}
