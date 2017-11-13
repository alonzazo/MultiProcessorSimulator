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
        private int[] memCompartidaP0; // size = 64
        private int[] memCompartidaP1; // size = 32
        private int[] memInstruccionesP0; // size = 384
        private int[] memInstruccionesP1; // size = 256

        private int[,] cacheDatosN0;// 4x6
        private int[,] cacheDatosN1;// 4x6
        private int[,] cacheDatosN2;// 4x6

        private int[,] cacheInstruccionesN0;// 4x17
        private int[,] cacheInstruccionesN1;// 4x17
        private int[,] cacheInstruccionesN2;// 4x17

        private int[,] directorioP0;//16x4
        private int[,] directorioP1;//8x4
        private int[,] contextoP0; //7x36
        private int[,] contextoP1; //7x36

        private int[] registrosN0;// size = 32
        private int[] registrosN1;// size = 32
        private int[] registrosN2;// size = 32
        private int[] rInstruccionN0;// size = 4
        private int[] rInstruccionN1;// size = 4
        private int[] rInstruccionN2;// size = 4

        public void correr()
        {
            string [] hilosP0 = solicitarHilos(0);
            string[] hilosP1 = solicitarHilos(1);
            int quantum = solicitarQuantum();
            //Inicializo estructuras
            inicializar();
            //Lleno memoria de instrucciones
            guardarInstrucciones(memInstruccionesP0, hilosP0);
            guardarInstrucciones(memInstruccionesP1, hilosP1);
            //Imprimo memoria de instrucciones para verificar
            printMemoria(memInstruccionesP0);
            printMemoria(memInstruccionesP1);
            //Pregunto por modo de ejecucion
            Console.WriteLine("\n");
            Console.WriteLine("Elija su modo de ejecución: Digite 1 para lento o 2 para rápido");
            int modo = Int32.Parse(Console.ReadLine());
            
            //Asignar a cada procesador sus hilos correpondientes
        }

        public void inicializar()
        {
            //Inicializo las memorias de instrucciones
            memInstruccionesP0 = new int[384]; // size = 384
            memInstruccionesP1 = new int[256]; // size = 256
            //Inicializo la memoria compartida
            memCompartidaP0 = new int[64]; // size = 64
            memCompartidaP1 = new int[32]; // size = 32
            //Inicializo registros
            registrosN0 = new int[32];
            registrosN1 = new int[32];
            registrosN2 = new int[32];
            //Inicializo registros de instrucciones
            rInstruccionN0 = new int[4];
            rInstruccionN1 = new int[4];
            rInstruccionN2 = new int[4];
            //Inicializo caches
            cacheDatosN0 = new int[4,6];
            cacheDatosN1 = new int[4, 6];
            cacheDatosN2 = new int[4, 6];
            //Inicializo caches de instrucciones
            cacheInstruccionesN0 = new int[4, 17];
            cacheInstruccionesN1 = new int[4, 17];
            cacheInstruccionesN2 = new int[4, 17];
            //Inicializo directorios
            directorioP0 = new int[16,4];//16x4
            directorioP1 = new int[8, 4];//8x4
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

        public void logicaNucleo() {
            
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
