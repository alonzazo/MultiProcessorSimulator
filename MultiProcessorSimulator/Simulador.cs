using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

        Barrier barrera;

        public void correr()
        {
            string [] hilosP0 = solicitarHilos(0);
            string[] hilosP1 = solicitarHilos(1);
            int quantum = solicitarQuantum();
            //Inicializo estructuras
            inicializar();

            //Lleno memoria de instrucciones
            guardarInstrucciones(contextoP0, memInstruccionesP0, hilosP0);
            guardarInstrucciones(contextoP1, memInstruccionesP1, hilosP1);
            /*
                        //Imprimo memoria de instrucciones para verificar
                        Console.WriteLine("Memoria instrucciones P0");
                        printMemoria(memInstruccionesP0);
                            Console.Write("\n");
                        Console.WriteLine("Memoria instrucciones P1");
                        printMemoria(memInstruccionesP1);
                        Console.WriteLine("");
                        printContexto();

                        barrera = new Barrier(4); //Inicializacion de la barrera

                        Thread nucleo0 = new Thread(new ThreadStart(logicaNucleo));
                        nucleo0.Start();

                        Thread nucleo1 = new Thread(new ThreadStart(logicaNucleo));
                        nucleo1.Start();

                        Thread nucleo2 = new Thread(new ThreadStart(logicaNucleo));
                        nucleo2.Start();

                        barrera.SignalAndWait(); // Barrera de finalización

                        //Pregunto por modo de ejecucion
                        Console.WriteLine("\n");
                        Console.WriteLine("Elija su modo de ejecución: Digite 1 para lento o 2 para rápido");
                        int modo = Int32.Parse(Console.ReadLine());

                        //Asignar a cada procesador sus hilos correpondientes

                        //finalizar
                        finalizar(hilosP0, hilosP1);*/
            finalizar(hilosP0, hilosP1);
            Console.Read();
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
            iniciarCache(cacheDatosN0);
            iniciarCache(cacheDatosN1);
            iniciarCache(cacheDatosN2);

            //Inicializo caches de instrucciones
            cacheInstruccionesN0 = new int[4, 17];
            cacheInstruccionesN1 = new int[4, 17];
            cacheInstruccionesN2 = new int[4, 17];
            iniciarCache(cacheInstruccionesN0);
            iniciarCache(cacheInstruccionesN1);
            iniciarCache(cacheInstruccionesN2);

            //Inicializo directorios
            directorioP0 = new int[16,4];//16x4
            directorioP1 = new int[8, 4];//8x4
            iniciarDirectorio(directorioP0);
            iniciarDirectorio(directorioP1); 

            //Inicializo contextos
            contextoP0 = new int[7,36];
            contextoP1 = new int[7, 36];
        }

        public void iniciarCache(int[,] cache)
        {
            //Inicializo los numero de bloque con -1
            for (int i = 0; i < 4; ++i)
            {
                cache[i, 0] = -1;
            }
        }

        public void iniciarDirectorio(int[,] Dir)
        {
            for(int i =0; i < Dir.GetLength(0); ++i)
            {
                Dir[i, 1] = 1;
                Dir[i, 2] = 1;
                Dir[i, 3] = 1;
            }
        }

        public void printCache(int[,] cache)
        {
            int cont = 0;
            for (int i = 0; i < cache.GetLength(0); ++i)
            {
                for (int j = 0; j < cache.GetLength(1); ++j)
                {
                    ++cont;
                    Console.Write(cache[i, j]);
                    if(cache.GetLength(1) == 17)
                    {
                        if (j == 0)
                        {
                            Console.Write(" ");
                            cont = 0;
                        }
                        else if (cont == 4)
                        {
                            Console.Write(" ");
                            cont = 0;
                        }
                    }
                    else
                    {
                        Console.Write(" ");
                    }
                        
                }
                Console.Write("\n");
            } 
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

        // EFECTO: Guarda las instrucciones en la memoria y crea los contextos iniciales
        // REQUIERE: contexto destino, memoria donde se guardaran los hilillos, los hilillos fuente
        // MODIFICA: 
        public void guardarInstrucciones(int [,] contexto, int[] mem, string[] hilillos)
        {

            string[] lines;
            int bloque = 0;
            for (int i = 0; i< hilillos.Length; ++i)
            {
                contexto[i,0] = bloque;
                lines = System.IO.File.ReadAllLines(hilillos[i]);
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
            Console.WriteLine("Impreso desdel el nucleo 1");
            barrera.SignalAndWait();
        }

        public void printMemoria(int [] mem)
        {
            int cont = 0;
            for(int i = 0; i< mem.Length; ++i)
            {
                ++cont;
                if( i % 64 == 0 && i != 0)
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

        public void printContexto()
        {
            Console.Write("Contexto 0\n");
            for (int i = 0; i < 7; i++) {
                for (int j = 0; j < 36; j++) {
                    Console.Write(contextoP0[i,j] + " ");
                }
                Console.Write("\n");
            }
            Console.Write("Contexto 1\n");
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 36; j++)
                {
                    Console.Write(contextoP1[i, j] + " ");
                }
                Console.Write("\n");
            }
        }

        // EFECTO: Ejecuta instrucción daddi
        // REQUIERE: vector de registros, vector de registro de instrucciones
        // MODIFICA: 
        public void daddi(int[] R, int[] IR)
        {
            R[IR[2]] = R[IR[1]] + IR[3];
        }

        // EFECTO: Ejecuta instrucción dadd
        // REQUIERE: vector de registros, vector de registro de instrucciones
        // MODIFICA: 
        public void dadd(int[] R, int[] IR)
        {
            R[IR[3]] = R[IR[1]] + R[IR[2]];
        }

        // EFECTO: Ejecuta instrucción dmul
        // REQUIERE: vector de registros, vector de registro de instrucciones
        // MODIFICA: 
        public void dmul(int[] R, int[] IR)
        {
            R[IR[3]] = R[IR[1]] * R[IR[2]];
        }

        // EFECTO: Ejecuta instrucción ddiv
        // REQUIERE: vector de registros, vector de registro de instrucciones
        // MODIFICA: 
        public void ddiv(int[] R, int[] IR)
        {
            R[IR[3]] = R[IR[1]] / R[IR[2]];
        }

        // EFECTO: Ejecuta instrucción fin
        // REQUIERE: vector de registros, vector de registro de instrucciones
        // MODIFICA: 
        public void fin(int[,] contexto, int[] R)
        {
            
        }


        public void finalizar(string[] hilosP0, string[] hilosP1)
        {
            //Imprimo memorias
            Console.WriteLine("Memoria compartida P0");
            printMemoria(memCompartidaP0);
            Console.WriteLine("\n");
            Console.WriteLine("Memoria compartida P1");
            printMemoria(memCompartidaP1);
            Console.WriteLine("\n");
            //Imprimo cache de datos
            Console.WriteLine("Cache N0");
            printCache(cacheDatosN0);
            Console.WriteLine("Cache N1");
            printCache(cacheDatosN1);
            Console.WriteLine("Cache N2");
            printCache(cacheDatosN2);
            //Imprimo cache de instrucciones
            Console.WriteLine("Cache Instrucciones N0");
            printCache(cacheInstruccionesN0);
            Console.WriteLine("Cache Instrucciones N1");
            printCache(cacheInstruccionesN1);
            Console.WriteLine("Cache Instrucciones N2");
            printCache(cacheInstruccionesN2);

            Console.WriteLine("\n");
            for(int i = 0; i< hilosP0.Length; ++i)
            {
                string[] aux = hilosP0[i].Split('\\');
                string nombre = aux[aux.Length - 1];
                Console.WriteLine("Contenido del hilillo " + nombre);
                Console.WriteLine("Registros:");
                for(int j = 1; j<33; ++j)
                {
                    Console.Write(contextoP0[i, j]);
                    Console.Write(" ");
                }
                Console.Write("\n");
                Console.WriteLine("Cantidad de ciclos en ejecutarse: " + contextoP0[i, 33]);
                Console.WriteLine("Nombre del procesador donde se ejecuto: P0");
                Console.WriteLine("Valor del reloj al inicio de hilillo: " + contextoP0[i, 34]);
                Console.WriteLine("Valor del reloj al fin de hilillo: " + contextoP0[i, 35]);
                Console.Write("\n");
            }

            Console.Write("");
            for (int i = 0; i < hilosP1.Length; ++i)
            {
                string[] aux = hilosP1[i].Split('\\');
                string nombre = aux[aux.Length - 1];
                Console.WriteLine("Contenido del hilillo " + nombre);
                Console.WriteLine("Registros:");
                for (int j = 1; j < 33; ++j)
                {
                    Console.Write(contextoP1[i, j]);
                    Console.Write(" ");
                }
                Console.Write("\n");
                Console.WriteLine("Cantidad de ciclos en ejecutarse: " + contextoP1[i, 33]);
                Console.WriteLine("Nombre del procesador donde se ejecuto: P1");
                Console.WriteLine("Valor del reloj al inicio de hilillo: " + contextoP1[i, 34]);
                Console.WriteLine("Valor del reloj al fin de hilillo: " + contextoP1[i, 35]);
                Console.Write("\n");
            }
        }
    }
}
