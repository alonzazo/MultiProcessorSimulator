using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiProcessorSimulator
{
    class Nucleo
    {
        private int numProc;
        private int numNucleo;
        private int cicloActual;
        private int contextoActual;
        private int PC;
        private int[] IR;
        private int[] registros;
        
        private String logExecution;

        public Nucleo(int numProc, int numNucleo, int[] contexto, int numContexto) {
            this.numProc = numProc;
            this.numNucleo = numNucleo;
            cicloActual = 0;
            contextoActual = numContexto;
            IR = new int[4];
            registros = new int[32];
            logExecution = "Ejecuciones del núcleo " + numNucleo + "\n";
            PC = contexto[0];
            //Cargamos el primer contexto
            for (int i = 1; i < 33; i++) {
                registros[i-1] = contexto[i];
            }
        }

        
        /// <summary>
        /// Inicia la corrida de la lógica de un núcleo.
        /// </summary>
        public void run() {
            if (numProc == 0) {

               
                int numBloque;
                int posCache;
                int numInstruccion;
                bool flag = true;
                while (flag) {
                    if ((cicloActual == 0 || cicloActual % Simulador.quantum != 0) && Simulador.contextoP0[contextoActual][33] == -1)
                    {
                        //Solicitamos bus de memoria
                        /*lock (Simulador.memInstruccionesP0)
                        {
                            //Hacemos fetch de la instrucción
                            IR[0] = Simulador.memInstruccionesP0[PC];
                            IR[1] = Simulador.memInstruccionesP0[PC + 1];
                            IR[2] = Simulador.memInstruccionesP0[PC + 2];
                            IR[3] = Simulador.memInstruccionesP0[PC + 3];
                        }*/
                        lock (Simulador.contextoP0)
                        {
                            if(Simulador.contextoP0[contextoActual][37] == -1)
                            {
                                Simulador.contextoP0[contextoActual][37] = 0;
                                Simulador.contextoP0[contextoActual][36] = Simulador.reloj;
                                logExecution += "Reloj de inicio guardado en el contexto " + contextoActual + "del P0" + "\n";
                            }

                        }

                        if (numNucleo == 0)
                        {
                            lock (Simulador.cacheInstruccionesN0)
                            {
                                numBloque = obtenerNumBloqueInstruccion(numProc);
                                posCache = obtenerPosCache(numBloque);
                                numInstruccion = obtenerNumInstruccionBloque();
                                if (!instruccionEnCache(numBloque, posCache, numNucleo))
                                {

                                    lock (Simulador.memInstruccionesP0)
                                    {
                                        insertarBloqueCacheInstrucciones(numBloque, posCache, numNucleo);
                                    }
                                }
                                //insertarBloqueCacheInstrucciones(numBloque, posCache, numNucleo);
                                IR[0] = Simulador.cacheInstruccionesN0[posCache, numInstruccion * 4 + 1];
                                IR[1] = Simulador.cacheInstruccionesN0[posCache, numInstruccion * 4 + 2];
                                IR[2] = Simulador.cacheInstruccionesN0[posCache, numInstruccion * 4 + 3];
                                IR[3] = Simulador.cacheInstruccionesN0[posCache, numInstruccion * 4 + 4];
                            }
                        }
                        else
                        {
                            lock (Simulador.cacheInstruccionesN1)
                            {
                                numBloque = obtenerNumBloqueInstruccion(numProc);
                                posCache = obtenerPosCache(numBloque);
                                numInstruccion = obtenerNumInstruccionBloque();
                                if (!instruccionEnCache(numBloque, posCache, numNucleo))
                                {

                                    lock (Simulador.memInstruccionesP0)
                                    {
                                        insertarBloqueCacheInstrucciones(numBloque, posCache, numNucleo);
                                    }
                                }
                                //insertarBloqueCacheInstrucciones(numBloque, posCache, numNucleo);
                                IR[0] = Simulador.cacheInstruccionesN1[posCache, numInstruccion * 4 + 1];
                                IR[1] = Simulador.cacheInstruccionesN1[posCache, numInstruccion * 4 + 2];
                                IR[2] = Simulador.cacheInstruccionesN1[posCache, numInstruccion * 4 + 3];
                                IR[3] = Simulador.cacheInstruccionesN1[posCache, numInstruccion * 4 + 4];
                            }
                        }
                        //Aumentamos 4 al PC
                        PC += 4;

                        //Decode del IR
                        switch (IR[0])
                        { //Se identifica el op. code
                          //Caso del DADDI
                            case 8:
                                instruccionDADDI();
                                break;
                            //Caso del DADD
                            case 32:
                                instruccionDADD();
                                break;
                            //Caso del DSUB
                            case 34:
                                instruccionDSUB();
                                break;
                            //Caso del DMUL
                            case 12:
                                instruccionDMUL();
                                break;
                            //Caso del DDIV
                            case 14:
                                instruccionDDIV();
                                break;
                            //Caso del BEQZ
                            case 4:
                                instruccionBEQZ();
                                break;
                            //Caso del BNEZ
                            case 5:
                                instruccionBNEZ();
                                break;
                            //Caso del JAL
                            case 3:
                                instruccionJAL();
                                break;
                            //Caso del JR
                            case 2:
                                instruccionJR();
                                break;
                            //Caso del LW
                            case 35:
                                instruccionLW();
                                break;
                            //Caso del SW
                            case 43:
                                instruccionSW();
                                break;
                            //Caso del FIN
                            case 63:
                                instruccionFIN();
                                lock (Simulador.contextoP0)
                                {
                                    Simulador.contextoP0[contextoActual][0] = PC;                       //Salvamos el PC en el contexto
                                    for (int i = 0; i < registros.Length; i++)
                                    {                        //Salvamos los registros en el contexto
                                        Simulador.contextoP0[contextoActual][i + 1] = registros[i];
                                    }
                                    Simulador.contextoP0[contextoActual][34] = -1;                      //Marcamos el contexto como en desuso

                                    //Buscamos un contexto en desuso
                                    for (int i = 0; i < Simulador.contextoP0.Length; i++)
                                    {
                                        contextoActual = (contextoActual + 1) % 7;
                                        if (Simulador.contextoP0[contextoActual][34] == -1 &&           //Está en desuso?
                                            Simulador.contextoP0[contextoActual][33] == -1)             //Ya terminó?
                                        {
                                            break;
                                        }
                                    }
                                    if (Simulador.contextoP0[contextoActual][33] == -1)
                                    {               //Verificar si el contextoActual ya terminó
                                                    //Cargamos el nuevo contexto
                                        PC = Simulador.contextoP0[contextoActual][0];
                                        for (int i = 1; i < 33; i++)
                                        {
                                            registros[i - 1] = Simulador.contextoP0[contextoActual][i];
                                        }
                                        Simulador.contextoP0[contextoActual][34] = 0;                       //Marcamos el nuevo contexto como "en uso"
                                        Console.WriteLine("PROCESADOR " + numProc + " NUCLEO " + numNucleo + " hizo cambio de contexto al " + contextoActual);

                                        Simulador.printContexto();
                                    }
                                    Console.WriteLine("Nucleo " + numNucleo + " ha pasado al ciclo " + cicloActual);
                                }
                                break;
                            //Caso erroneo
                            default:
                                throw new Exception("ERROR: Instruccion " + IR[0] + " no identificada.");
                        }
                        //Simulador.barrera.SignalAndWait();
                        //cicloActual++;
                        //Console.WriteLine("Nucleo " + numNucleo + " ha pasado al ciclo " + cicloActual);
                    }
                    else {
                        lock (Simulador.contextoP0) {
                            if (Simulador.contextoP0[contextoActual][33] == -1 )
                            {
                                Simulador.contextoP0[contextoActual][0] = PC;                       //Salvamos el PC en el contexto
                                for (int i = 0; i < registros.Length; i++)
                                {                        //Salvamos los registros en el contexto
                                    Simulador.contextoP0[contextoActual][i + 1] = registros[i];
                                }
                                Simulador.contextoP0[contextoActual][34] = -1;                      //Marcamos el contexto como en desuso

                            }

                            //Buscamos un contexto en desuso
                            for (int i = 0; i < Simulador.contextoP0.Length; i++) {
                                contextoActual = (contextoActual + 1) % 7;
                                if (Simulador.contextoP0[contextoActual][34] == -1 &&           //Está en desuso?
                                    Simulador.contextoP0[contextoActual][33] == -1)             //Ya terminó?
                                {
                                    break;
                                }
                            }
                            if (Simulador.contextoP0[contextoActual][33] == -1) {               //Verificar si el contextoActual ya terminó
                                //Cargamos el nuevo contexto
                                PC = Simulador.contextoP0[contextoActual][0];
                                for (int i = 1; i < 33; i++)
                                {
                                    registros[i - 1] = Simulador.contextoP0[contextoActual][i];
                                }
                                Simulador.contextoP0[contextoActual][34] = 0;                       //Marcamos el nuevo contexto como "en uso"
                                Console.WriteLine("PROCESADOR " + numProc + " NUCLEO " + numNucleo + " hizo cambio de contexto al " + contextoActual);

                                Simulador.printContexto();
                            }
                            //cicloActual++;
                            //Console.WriteLine("Nucleo " + numNucleo + " ha pasado al ciclo " + cicloActual);
                        }
                        

                    }


                    Simulador.barrera.SignalAndWait();
                    Simulador.reloj++;
                    cicloActual++;
                    Console.WriteLine("Nucleo " + numNucleo + " ha pasado al ciclo " + cicloActual);
                    //Revisamos si ya todos los hilillos del contexto terminaron para terminar el nucleo.
                    lock (Simulador.contextoP0)
                    {
                        bool continuar = false;
                        for (int i = 0; i < 7; i++)
                        {
                            if (Simulador.contextoP0[i][33] == -1)
                            {
                                continuar = true;
                                break;
                            }
                        }
                        flag = continuar;
                    }
                }
            } else {
                bool flag = true;
                int numBloque;
                int posCache;
                int numInstruccion;
                while (flag)
                {
                    if ((cicloActual == 0 || cicloActual % Simulador.quantum != 0) && Simulador.contextoP1[contextoActual][33] == -1)
                    {
                        lock (Simulador.contextoP1)
                        {
                            if (Simulador.contextoP1[contextoActual][37] == -1)
                            {
                                Simulador.contextoP1[contextoActual][37] = 0;
                                Simulador.contextoP1[contextoActual][36] = Simulador.reloj;
                                logExecution += "Reloj de inicio guardado en el contexto " + contextoActual + "del P1" + "\n";
                            }

                        }
                        lock (Simulador.cacheInstruccionesN2)
                        {
                            numBloque = obtenerNumBloqueInstruccion(numProc);
                            posCache = obtenerPosCache(numBloque);
                            numInstruccion = obtenerNumInstruccionBloque();
                            if (!instruccionEnCache(numBloque, posCache, numNucleo))
                            {

                                lock (Simulador.memInstruccionesP1)
                                {
                                    insertarBloqueCacheInstrucciones(numBloque, posCache, numNucleo);
                                }
                            }
                            //insertarBloqueCacheInstrucciones(numBloque, posCache, numNucleo);
                            IR[0] = Simulador.cacheInstruccionesN2[posCache, numInstruccion * 4 + 1];
                            IR[1] = Simulador.cacheInstruccionesN2[posCache, numInstruccion * 4 + 2];
                            IR[2] = Simulador.cacheInstruccionesN2[posCache, numInstruccion * 4 + 3];
                            IR[3] = Simulador.cacheInstruccionesN2[posCache, numInstruccion * 4 + 4];
                        }
                        //Solicitamos bus de memoria
                        /*lock (Simulador.memInstruccionesP1)
                        {
                            //Hacemos fetch de la instrucción
                            IR[0] = Simulador.memInstruccionesP1[PC];
                            IR[1] = Simulador.memInstruccionesP1[PC + 1];
                            IR[2] = Simulador.memInstruccionesP1[PC + 2];
                            IR[3] = Simulador.memInstruccionesP1[PC + 3];
                        }
                        */
                        //Aumentamos 4 al PC
                        PC += 4;

                        //Decode del IR
                        switch (IR[0])
                        { //Se identifica el op. code
                          //Caso del DADDI
                            case 8:
                                instruccionDADDI();
                                break;
                            //Caso del DADD
                            case 32:
                                instruccionDADD();
                                break;
                            //Caso del DSUB
                            case 34:
                                instruccionDSUB();
                                break;
                            //Caso del DMUL
                            case 12:
                                instruccionDMUL();
                                break;
                            //Caso del DDIV
                            case 14:
                                instruccionDDIV();
                                break;
                            //Caso del BEQZ
                            case 4:
                                instruccionBEQZ();
                                break;
                            //Caso del BNEZ
                            case 5:
                                instruccionBNEZ();
                                break;
                            //Caso del JAL
                            case 3:
                                instruccionJAL();
                                break;
                            //Caso del JR
                            case 2:
                                instruccionJR();
                                break;
                            //Caso del LW
                            case 35:
                                instruccionLW();
                                break;
                            //Caso del SW
                            case 43:
                                instruccionSW();
                                break;
                            //Caso del FIN
                            case 63:
                                instruccionFIN();
                                lock (Simulador.contextoP1)
                                {
                                    Simulador.contextoP1[contextoActual][0] = PC;                       //Salvamos el PC en el contexto
                                    for (int i = 0; i < registros.Length; i++)
                                    {                        //Salvamos los registros en el contexto
                                        Simulador.contextoP1[contextoActual][i + 1] = registros[i];
                                    }
                                    Simulador.contextoP1[contextoActual][34] = -1;                      //Marcamos el contexto como en desuso


                                    //Buscamos un contexto en desuso
                                    for (int i = 0; i < Simulador.contextoP1.Length; i++)
                                    {
                                        contextoActual = (contextoActual + 1) % 7;
                                        if (Simulador.contextoP1[contextoActual][34] == -1 ||           //Está en desuso?
                                            Simulador.contextoP1[contextoActual][33] == -1)             //Ya terminó?
                                        {
                                            break;
                                        }
                                    }
                                    if (Simulador.contextoP1[contextoActual][33] == -1)                 //Verificar si el contextoActual ya terminó
                                    {
                                        //Cargamos el nuevo contexto
                                        PC = Simulador.contextoP1[contextoActual][0];
                                        for (int i = 1; i < 33; i++)
                                        {
                                            registros[i - 1] = Simulador.contextoP1[contextoActual][i];
                                        }
                                        Simulador.contextoP1[contextoActual][34] = 0;                       //Marcamos el nuevo contexto como "en uso"
                                        Console.WriteLine("PROCESADOR " + numProc + " NUCLEO " + numNucleo + " hizo cambio de contexto al " + contextoActual);

                                        Simulador.printContexto();
                                    }
                                    Console.WriteLine("Nucleo " + numNucleo + " ha pasado al ciclo " + cicloActual);
                                }
                                break;
                            //Caso erroneo
                            default:
                                throw new Exception("ERROR: Instruccion " + IR[0] + " no identificada.");
                        }
                        //Simulador.barrera.SignalAndWait();
                        //cicloActual++;
                        //Console.WriteLine("Nucleo " + numNucleo + " ha pasado al ciclo " + cicloActual);
                    }
                
                    else {
                        lock (Simulador.contextoP1)
                        {
                            if(Simulador.contextoP1[contextoActual][33] == -1)
                            {
                                Simulador.contextoP1[contextoActual][0] = PC;                       //Salvamos el PC en el contexto
                                for (int i = 0; i < registros.Length; i++)
                                {                        //Salvamos los registros en el contexto
                                    Simulador.contextoP1[contextoActual][i + 1] = registros[i];
                                }
                                Simulador.contextoP1[contextoActual][34] = -1;                      //Marcamos el contexto como en desuso

                            }


                            //Buscamos un contexto en desuso
                            for (int i = 0; i < Simulador.contextoP1.Length; i++)
                            {
                                contextoActual = (contextoActual + 1) % 7;
                                if (Simulador.contextoP1[contextoActual][34] == -1 ||           //Está en desuso?
                                    Simulador.contextoP1[contextoActual][33] == -1)             //Ya terminó?
                                {
                                    break;
                                }
                            }
                            if (Simulador.contextoP1[contextoActual][33] == -1)                 //Verificar si el contextoActual ya terminó
                            {               
                                //Cargamos el nuevo contexto
                                PC = Simulador.contextoP1[contextoActual][0];
                                for (int i = 1; i < 33; i++)
                                {
                                    registros[i - 1] = Simulador.contextoP1[contextoActual][i];
                                }
                                Simulador.contextoP1[contextoActual][34] = 0;                       //Marcamos el nuevo contexto como "en uso"
                                Console.WriteLine("PROCESADOR " + numProc + " NUCLEO " + numNucleo + " hizo cambio de contexto al " + contextoActual);

                                Simulador.printContexto();
                            }
                            //cicloActual++;
                            //Console.WriteLine("Nucleo " + numNucleo + " ha pasado al ciclo " + cicloActual);
                        }
                    }

                    Simulador.barrera.SignalAndWait();
                    Simulador.reloj++;
                    cicloActual++;
                    Console.WriteLine("Nucleo " + numNucleo + " ha pasado al ciclo " + cicloActual);

                    lock (Simulador.contextoP1) {
                        bool continuar = false;
                        for (int i = 0; i < 7; i++)
                        {
                            if (Simulador.contextoP1[i][33] == -1)
                            {
                                continuar = true;
                                break;
                            }
                        }
                        flag = continuar;
                    }
                }
            }
            Simulador.barrera.RemoveParticipant();
            printLogExecution();
            

        }

        /// <summary>
        /// Ejecuta la instrucción DADD
        /// </summary>
        private void instruccionDADD() {
            logExecution += "Instrucción DADD ejecutada.\n";
            registros[IR[3]] = registros[IR[1]] + registros[IR[2]];
            //Console.WriteLine("Instrucción DADD ejecutada.");

        }

        /// <summary>
        /// Ejecuta la instrucción DADDI
        /// </summary>
        private void instruccionDADDI()
        {
            logExecution += "Instrucción DADDI ejecutada.\n";
            registros[IR[2]] = registros[IR[1]] + IR[3];
            //Console.WriteLine("Instrucción DADDI ejecutada.");
        }

        /// <summary>
        /// Ejecuta la instrucción DSUB
        /// </summary>
        private void instruccionDSUB()
        {

            logExecution += "Instrucción DSUB ejecutada en el contexto " + contextoActual + "\n";
            registros[IR[3]] = registros[IR[1]] - registros[IR[2]];
            //Console.WriteLine("Instrucción DSUB ejecutada en el contexto ");
        }

        /// <summary>
        /// Ejecuta la instrucción DMUL
        /// </summary>
        private void instruccionDMUL()
        {
            logExecution += "Instrucción DMUL ejecutada.\n";
            registros[IR[3]] = registros[IR[1]] * registros[IR[2]];
            //Console.WriteLine("Instrucción DMUL ejecutada.");
        }

        /// <summary>
        /// Ejecuta la instrucción DDIV
        /// </summary>
        private void instruccionDDIV()
        {
            logExecution += "Instrucción DDIV ejecutada.\n";
            registros[IR[3]] = registros[IR[1]] / registros[IR[2]];
            //Console.WriteLine("Instrucción DDIV ejecutada.");
        }

        /// <summary>
        /// Ejecuta la instrucción BEQZ
        /// </summary>
        private void instruccionBEQZ()
        {
            logExecution += "Instrucción BEQZ ejecutada en el contexto " + contextoActual + "\n";
            //Console.WriteLine("Instrucción BEQZ ejecutada en el contexto ");
            
            if (registros[IR[1]] == 0)
            {
                PC += IR[3] * 4;
            }
        }

        /// <summary>
        /// Ejecuta la instrucción BNEZ
        /// </summary>
        private void instruccionBNEZ()
        {
            logExecution += "Instrucción BNEZ ejecutada en el contexto " + contextoActual + "\n";
            if (registros[IR[1]] != 0)
            {
                PC += IR[3] * 4;
            }
            //Console.WriteLine("Instrucción BNEZ ejecutada en el contexto ");
        }

        /// <summary>
        /// Ejecuta la instrucción JAL
        /// </summary>
        private void instruccionJAL()
        {
            registros[31] = PC;
            PC += IR[3];
            logExecution += "Instrucción JAL ejecutada en el contexto " + contextoActual + "\n";
            
            //Console.WriteLine("Instrucción JAL ejecutada en el contexto ");
        }


        /// <summary>
        /// Ejecuta la instrucción JR
        /// </summary>
        private void instruccionJR()
        {
            PC = registros[IR[1]];
            logExecution += "Instrucción JR ejecutada en el contexto" + contextoActual + "\n";
            //Console.WriteLine("Instrucción JR ejecutada en el contexto ");
        }

        /// <summary>
        /// Ejecuta la instrucción LW
        /// </summary>
        private void instruccionLW()
        {
            logExecution += "Instrucción LW ejecutada en el contexto " + contextoActual + "\n";
            int numeroBloque = obtenerNumBloque();
            int numPalabra = obtenerNumPalabra();
            int posCache = obtenerPosCache(numeroBloque);
            int numProcesadorBloque = numDirectorio(numeroBloque);
            int numProcesadorBloqueVictima;

            bool terminado = false;

            if(numNucleo == 0)//Es del nucleo 0
            {
                while (!terminado)
                {
                    if (Monitor.TryEnter(Simulador.cacheDatosN0))//Bloqueo la cache local
                    {
                        if (!bloqueEnCache(posCache, numeroBloque)) //Si no se encuentra en cache o esta invalido
                        {
                            if (Simulador.cacheDatosN0[posCache, 1] != 0) //Si la victima no esta invalida
                            {
                                numProcesadorBloqueVictima = numDirectorio(Simulador.cacheDatosN0[posCache, 0]);//Obtengo en cual procesador se encuentra el bloque
                                if (Simulador.cacheDatosN0[posCache, 1] == 2) //Si la victima esta modificada
                                {
                                    if (numProcesadorBloqueVictima == 0)//Si el bloque esta en el procesador 0
                                    {
                                        if (Monitor.TryEnter(Simulador.memCompartidaP0))//Bloqueo la memoria
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;//Aumento el ciclo por bloquear memoria
                                            Simulador.barrera.SignalAndWait();
                                            if (Monitor.TryEnter(Simulador.directorioP0))//Bloqueo el directorio de la victima
                                            {
                                                Simulador.reloj += 1;
                                                cicloActual += 1;//Aumento ciclo por acceder a directorio local
                                                Simulador.barrera.SignalAndWait();
                                                guardarAMemoria(0, numProcesadorBloqueVictima, Simulador.cacheDatosN0[posCache, 0], posCache); //Guardo el bloque modificado de a victima a memoria
                                                Simulador.reloj += 16;
                                                cicloActual += 16; //Aumento el ciclo por guardar en memoria compartida del procesador
                                                for (int i = 0; i < 16; i++)
                                                    Simulador.barrera.SignalAndWait();
                                                Monitor.Exit(Simulador.memCompartidaP0);//Libero la memoria 
                                                Simulador.directorioP0[Simulador.cacheDatosN0[posCache, 0], 0] = 0;//Pongo U a directorio
                                                Simulador.directorioP0[Simulador.cacheDatosN0[posCache, 0], 1] = 0;//Pongo 0 el bit del N0
                                                Monitor.Exit(Simulador.directorioP0);//Libero directorio de la victima
                                                Simulador.cacheDatosN0[posCache, 1] = 0;//Invalido la posicion en la cache

                                            }
                                            else//Si no puedo blouear el directorio de la victima
                                            {
                                                Monitor.Exit(Simulador.memCompartidaP0);//Libero la memoria
                                                Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                                terminado = false;
                                            }
                                        }
                                        else//No puedo bloquear la memoria
                                        {
                                            terminado = false;
                                            Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                        }
                                    }
                                    else //Si el bloque esta en el procesador 1
                                    {
                                        if (Monitor.TryEnter(Simulador.memCompartidaP1))//Bloqueo la memoria remota
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;//Aumento el ciclo por bloquear memoria
                                            Simulador.barrera.SignalAndWait();
                                            if (Monitor.TryEnter(Simulador.directorioP1))//Bloqueo el directorio remoto de la victima
                                            {
                                                Simulador.reloj += 5;
                                                cicloActual += 5;//Aumento ciclo por acceder a directorio remoto
                                                for (int i = 0; i < 5; i++)
                                                    Simulador.barrera.SignalAndWait();
                                                guardarAMemoria(0, numProcesadorBloqueVictima, Simulador.cacheDatosN0[posCache, 0], posCache); //Guardo el bloque modificado de a victima a memoria
                                                Simulador.reloj += 40;
                                                cicloActual += 40; //Aumento el ciclo por guardar en memoria compartida del otro procesador
                                                for (int i = 0; i < 40; i++)
                                                    Simulador.barrera.SignalAndWait();
                                                Monitor.Exit(Simulador.memCompartidaP1);//Libero la memoria 
                                                Simulador.directorioP1[Simulador.cacheDatosN0[posCache, 0]-16, 0] = 0;//Pongo U a directorio
                                                Simulador.directorioP1[Simulador.cacheDatosN0[posCache, 0]-16, 1] = 0;//Pongo 0 el bit del N0
                                                Monitor.Exit(Simulador.directorioP1);//Libero directorio de la victima
                                                Simulador.cacheDatosN0[posCache, 1] = 0;//Invalido la posicion en la cache

                                            }
                                            else//Si no puedo blouear el directorio de la victima
                                            {
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                                terminado = false;
                                                Monitor.Exit(Simulador.memCompartidaP1);//Libero la memoria
                                                Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                            }
                                        }
                                        else//No puedo bloquear la memoria
                                        {
                                            terminado = false;
                                            Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                        }
                                    }
                                }
                                else //Si la victima esta compartida
                                {
                                    if (numProcesadorBloqueVictima == 0)//Si el bloque esta en el procesador 0
                                    {
                                        if (Monitor.TryEnter(Simulador.directorioP0))//Bloqueo el directorio donde esta la victima
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;//Aumento el ciclo por bloquear memoria
                                            Simulador.barrera.SignalAndWait();
                                            Simulador.directorioP0[Simulador.cacheDatosN0[posCache, 0], 1] = 0;//Cambio bit para indicar que bloque no esta en esa cache
                                            if (Simulador.directorioP0[Simulador.cacheDatosN0[posCache, 0], 2] == 0 && Simulador.directorioP0[Simulador.cacheDatosN0[posCache, 0], 3] == 0)//Si ninguna otra cache tiene el bloque
                                            {
                                                Simulador.directorioP0[Simulador.cacheDatosN0[posCache, 0], 0] = 0;
                                            }
                                            Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                            Simulador.cacheDatosN0[posCache, 1] = 0;//Invalido la posicion en la cache

                                        }
                                        else//No puedo bloquear el directorio
                                        {
                                            terminado = false;
                                            Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                        }
                                    }
                                    else //Si el bloque esta en el procesador 1
                                    {
                                        if (Monitor.TryEnter(Simulador.directorioP1))//Bloqueo el directorio remoto donde esta la victima
                                        {
                                            Simulador.reloj += 5;
                                            cicloActual += 5;//Aumento el ciclo por bloquear memoria remota
                                            for (int i = 0; i < 5; i++)
                                                Simulador.barrera.SignalAndWait();
                                            Simulador.directorioP1[Simulador.cacheDatosN0[posCache, 0]-16, 1] = 0;//Cambio bit para indicar que bloque no esta en esa cache
                                            if (Simulador.directorioP1[Simulador.cacheDatosN0[posCache, 0]-16, 2] == 0 && Simulador.directorioP1[Simulador.cacheDatosN0[posCache, 0]-16, 3] == 0)//Si ninguna otra cache tiene el bloque
                                            {
                                                Simulador.directorioP1[Simulador.cacheDatosN0[posCache, 0]-16, 0] = 0;
                                            }
                                            Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                            Simulador.cacheDatosN0[posCache, 1] = 0;//Invalido la posicion en la cache

                                        }
                                        else//No puedo bloquear el directorio
                                        {
                                            terminado = false;
                                            Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                        }
                                    }
                                }

                            }
                            //Ya me encargue de la victima del reemplazo
                            if (numProcesadorBloque == 0)//Si el directorio que hay que bloquear es del P0
                            {
                                if (Monitor.TryEnter(Simulador.directorioP0))//Bloqueo el directorio del P0
                                {
                                    Simulador.reloj++;
                                    cicloActual++;//Aumento un ciclo por acceso a directorio local
                                    Simulador.barrera.SignalAndWait();
                                    if (Simulador.directorioP0[numeroBloque, 0] == 0 || Simulador.directorioP0[numeroBloque, 0] == 1)//Si en el directorio esta U o C
                                    {
                                        if (Monitor.TryEnter(Simulador.memCompartidaP0))//Bloqueo la memoria
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                            guardarBloqueEnCache(posCache, numeroBloque, numPalabra, 1);//Guardo el bloque en la cache
                                            Simulador.reloj += 16;
                                            cicloActual += 16;//Aumento 16 por escribir desde memoria local
                                            for (int i = 0; i < 16; i++)
                                                Simulador.barrera.SignalAndWait();
                                            Monitor.Exit(Simulador.memCompartidaP0);//Libero la memoria
                                            if (Simulador.directorioP0[numeroBloque, 0] == 0)
                                                Simulador.directorioP0[numeroBloque, 0] = 1;//Pongo directorio en C
                                            Simulador.directorioP0[numeroBloque, 1] = 1;//Indico que esta en cache
                                            Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                            terminado = true;//Solo falta obtener de cache

                                        }
                                        else//No se puede bloquear la memoria
                                        {
                                            terminado = false;
                                            //Falta regresar a estado anterior
                                            Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                            Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                        }
                                    }
                                    else//Si en el directorio esta M
                                    {
                                        if (Simulador.directorioP0[numeroBloque, 2] == 1)//Esta en cache del N1
                                        {
                                            if (Monitor.TryEnter(Simulador.cacheDatosN1))//Bloqueo la cache donde esta
                                            {
                                                if (Monitor.TryEnter(Simulador.memCompartidaP0))//Bloqueo la memoria del P0
                                                {
                                                    Simulador.reloj++;
                                                    cicloActual++;//Aumento ciclo por ingresar a memoria
                                                    Simulador.barrera.SignalAndWait();
                                                    guardarAMemoria(1, 0, numeroBloque, posCache);//Guardo lo que tiene la cache en memoria
                                                    Simulador.reloj += 16;
                                                    cicloActual += 16;//Aumento 16 por escribir desde memoria local
                                                    for (int i = 0; i < 16; i++)
                                                        Simulador.barrera.SignalAndWait();
                                                    Monitor.Exit(Simulador.memCompartidaP0);//Libero la memoria
                                                    for (int j = 0; j < 6; j++)//Copio lo que hay en la cache del N1 en la cache del N0
                                                    {
                                                        Simulador.cacheDatosN0[posCache, j] = Simulador.cacheDatosN1[posCache, j];
                                                    }
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                    Simulador.directorioP0[numeroBloque, 0] = 1;//Pongo directorio en C
                                                    Simulador.cacheDatosN1[posCache, 1] = 1;//Pongo cache en C
                                                    Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache
                                                    Simulador.directorioP0[numeroBloque, 1] = 1;//Indico que esta en cache
                                                    Monitor.Exit(Simulador.directorioP0);//Libero el directorio

                                                }
                                                else//No se puede bloquear la memoria
                                                {
                                                    terminado = false;
                                                    //Falta regresar a estado anterior
                                                    Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                    Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache remota
                                                    Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                }
                                            }
                                            else//No se puede bloquear la cache
                                            {
                                                terminado = false;
                                                //Falta regresar a estado anterior
                                                Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                        else//Esta en cache del N2
                                        {
                                            if (Monitor.TryEnter(Simulador.cacheDatosN2))//Bloqueo la cache donde esta
                                            {
                                                if (Monitor.TryEnter(Simulador.memCompartidaP0))//Bloqueo la memoria del P0
                                                {
                                                    Simulador.reloj++;
                                                    cicloActual++;//Aumento ciclo por ingresar a memoria
                                                    Simulador.barrera.SignalAndWait();
                                                    guardarAMemoria(2, 0, numeroBloque, posCache);//Guardo lo que tiene la cache en memoria
                                                    Simulador.reloj += 40;
                                                    cicloActual += 40;//Aumento 40 por escribir desde cache remoto
                                                    for (int i = 0; i < 40; i++)
                                                        Simulador.barrera.SignalAndWait();
                                                    Monitor.Exit(Simulador.memCompartidaP0);//Libero la memoria
                                                    for (int j = 0; j < 6; j++)//Copio lo que hay en la cache del N2 en la cache del N0
                                                    {
                                                        Simulador.cacheDatosN0[posCache, j] = Simulador.cacheDatosN2[posCache, j];
                                                    }
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                    Simulador.directorioP0[numeroBloque, 0] = 1;//Pongo directorio en C
                                                    Simulador.cacheDatosN2[posCache, 1] = 1;//Pongo cache en C
                                                    Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache
                                                    Simulador.directorioP0[numeroBloque, 1] = 1;//Indico que esta en cache
                                                    Monitor.Exit(Simulador.directorioP0);//Libero el directorio                    
                                                }
                                                else//No se puede bloquear la memoria
                                                {
                                                    terminado = false;
                                                    //Falta regresar a estado anterior
                                                    Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                    Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache remota
                                                    Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                }
                                            }
                                            else//No se puede bloquear la cache
                                            {
                                                terminado = false;
                                                //Falta regresar a estado anterior
                                                Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                    }
                                }
                                else//No se puede bloquear el directorio
                                {
                                    terminado = false;
                                    //Falta regresar a estado anterior
                                    Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                    Simulador.reloj++;
                                    cicloActual++;
                                    Simulador.barrera.SignalAndWait();
                                }
                            }
                            else//Si el directorio que hay que bloquear es del P1
                            {
                                if (Monitor.TryEnter(Simulador.directorioP1))//Bloqueo el directorio del P1
                                {
                                    Simulador.reloj += 5;
                                    cicloActual += 5;//Aumento un ciclo por acceso a directorio remoto
                                    for (int i = 0; i < 5; i++)
                                        Simulador.barrera.SignalAndWait();
                                    if (Simulador.directorioP1[numeroBloque-16, 0] == 0 || Simulador.directorioP1[numeroBloque-16, 0] == 1)//Si en el directorio esta U o C
                                    {
                                        if (Monitor.TryEnter(Simulador.memCompartidaP1))//Bloqueo la memoria
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                            guardarBloqueEnCache(posCache, numeroBloque, numPalabra, 1);//Guardo el bloque en la cache
                                            Simulador.reloj += 40;
                                            cicloActual += 40;//Aumento 16 por escribir desde memoria remota
                                            for (int i = 0; i < 40; i++)
                                                Simulador.barrera.SignalAndWait();
                                            Monitor.Exit(Simulador.memCompartidaP1);//Libero la memoria
                                            if (Simulador.directorioP1[numeroBloque-16, 0] == 0)
                                                Simulador.directorioP1[numeroBloque-16, 0] = 1;//Pongo directorio en C
                                            Simulador.directorioP1[numeroBloque-16, 1] = 1;//Indico que esta en cache
                                            Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                            terminado = true;//Solo falta obtener de cache

                                        }
                                        else//No se puede bloquear la memoria
                                        {
                                            terminado = false;
                                            //Falta regresar a estado anterior
                                            Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                            Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                        }
                                    }
                                    else//Si en el directorio esta M
                                    {
                                        if (Simulador.directorioP1[numeroBloque-16, 2] == 1)//Esta en cache del N1
                                        {
                                            if (Monitor.TryEnter(Simulador.cacheDatosN1))//Bloqueo la cache donde esta
                                            {
                                                if (Monitor.TryEnter(Simulador.memCompartidaP1))//Bloqueo la memoria del P0
                                                {
                                                    Simulador.reloj ++;
                                                    cicloActual++;//Aumento ciclo por ingresar a memoria
                                                    Simulador.barrera.SignalAndWait();
                                                    guardarAMemoria(1, 1, numeroBloque, posCache);//Guardo lo que tiene la cache en memoria
                                                    Simulador.reloj += 40;
                                                    cicloActual += 40;//Aumento 16 por escribir a memoria remota
                                                    for (int i = 0; i < 40; i++)
                                                        Simulador.barrera.SignalAndWait();
                                                    Monitor.Exit(Simulador.memCompartidaP1);//Libero la memoria
                                                    for (int j = 0; j < 6; j++)//Copio lo que hay en la cache del N1 en la cache del N0
                                                    {
                                                        Simulador.cacheDatosN0[posCache, j] = Simulador.cacheDatosN1[posCache, j];
                                                    }
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                    Simulador.directorioP1[numeroBloque-16, 0] = 1;//Pongo directorio en C
                                                    Simulador.cacheDatosN1[posCache, 1] = 1;//Pongo cache en C
                                                    Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache
                                                    Simulador.directorioP1[numeroBloque-16, 1] = 1;//Indico que esta en cache
                                                    Monitor.Exit(Simulador.directorioP1);//Libero el directorio

                                                }
                                                else//No se puede bloquear la memoria
                                                {
                                                    terminado = false;
                                                    //Falta regresar a estado anterior
                                                    Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                    Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache remota
                                                    Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                }
                                            }
                                            else//No se puede bloquear la cache
                                            {
                                                terminado = false;
                                                //Falta regresar a estado anterior
                                                Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                        else//Esta en cache del N2
                                        {
                                            if (Monitor.TryEnter(Simulador.cacheDatosN2))//Bloqueo la cache donde esta
                                            {
                                                if (Monitor.TryEnter(Simulador.memCompartidaP1))//Bloqueo la memoria del P0
                                                {
                                                    Simulador.reloj++;
                                                    cicloActual++;//Aumento ciclo por ingresar a memoria
                                                    Simulador.barrera.SignalAndWait();
                                                    guardarAMemoria(2, 1, numeroBloque, posCache);//Guardo lo que tiene la cache en memoria
                                                    Simulador.reloj += 16;
                                                    cicloActual += 16;//Aumento 16 por escribir desde cache remoto
                                                    for (int i = 0; i < 16; i++)
                                                        Simulador.barrera.SignalAndWait();
                                                    Monitor.Exit(Simulador.memCompartidaP1);//Libero la memoria
                                                    for (int j = 0; j < 6; j++)//Copio lo que hay en la cache del N2 en la cache del N0
                                                    {
                                                        Simulador.cacheDatosN0[posCache, j] = Simulador.cacheDatosN2[posCache, j];
                                                    }
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                    Simulador.directorioP1[numeroBloque-16, 0] = 1;//Pongo directorio en C
                                                    Simulador.cacheDatosN2[posCache, 1] = 1;//Pongo cache en C
                                                    Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache
                                                    Simulador.directorioP1[numeroBloque-16, 1] = 1;//Indico que esta en cache
                                                    Monitor.Exit(Simulador.directorioP1);//Libero el directorio                    
                                                }
                                                else//No se puede bloquear la memoria
                                                {
                                                    terminado = false;
                                                    //Falta regresar a estado anterior
                                                    Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                    Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache remota
                                                    Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                }
                                            }
                                            else//No se puede bloquear la cache
                                            {
                                                terminado = false;
                                                //Falta regresar a estado anterior
                                                Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                    }
                                }
                                else//No se puede bloquear el directorio
                                {
                                    terminado = false;
                                    //Falta regresar a estado anterior
                                    Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                    Simulador.reloj++;
                                    cicloActual++;
                                    Simulador.barrera.SignalAndWait();
                                }
                            }

                        }
                        else
                            terminado = true;
                        if (terminado)//Si esta en cache y esta modificado o compartido
                        {
                            registros[IR[2]] = Simulador.cacheDatosN0[posCache, numPalabra + 2];//Copiamos lo que tiene la cache en el registro
                            Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                            terminado = true;
                        }
                    }
                    else//No se pudo bloquear la cache local
                    {
                        terminado = false;
                        Simulador.reloj++;
                        cicloActual++;
                        Simulador.barrera.SignalAndWait();
                    }
                }
            }
            else if(numNucleo == 1)//Es del nucleo 1
            {
                while (!terminado)
                {
                    if (Monitor.TryEnter(Simulador.cacheDatosN1))//Bloqueo la cache local
                    {
                        if (!bloqueEnCache(posCache, numeroBloque)) //Si no se encuentra en cache o esta invalido
                        {
                            if (Simulador.cacheDatosN1[posCache, 1] != 0) //Si la victima no esta invalida
                            {
                                numProcesadorBloqueVictima = numDirectorio(Simulador.cacheDatosN1[posCache, 0]);//Obtengo en cual procesador se encuentra el bloque
                                if (Simulador.cacheDatosN1[posCache, 1] == 2) //Si la victima esta modificada
                                {
                                    if (numProcesadorBloqueVictima == 0)//Si el bloque esta en el procesador 0
                                    {
                                        if (Monitor.TryEnter(Simulador.memCompartidaP0))//Bloqueo la memoria
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;//Aumento el ciclo por bloquear memoria
                                            Simulador.barrera.SignalAndWait();
                                            if (Monitor.TryEnter(Simulador.directorioP0))//Bloqueo el directorio de la victima
                                            {
                                                Simulador.reloj += 1;
                                                cicloActual += 1;//Aumento ciclo por acceder a directorio local
                                                Simulador.barrera.SignalAndWait();
                                                guardarAMemoria(1, numProcesadorBloqueVictima, Simulador.cacheDatosN1[posCache, 0], posCache); //Guardo el bloque modificado de a victima a memoria
                                                Simulador.reloj += 16;
                                                cicloActual += 16; //Aumento el ciclo por guardar en memoria compartida del procesador
                                                for (int i = 0; i < 16; i++)
                                                    Simulador.barrera.SignalAndWait();
                                                Monitor.Exit(Simulador.memCompartidaP0);//Libero la memoria 
                                                Simulador.directorioP0[Simulador.cacheDatosN1[posCache, 0], 0] = 0;//Pongo U a directorio
                                                Simulador.directorioP0[Simulador.cacheDatosN1[posCache, 0], 1] = 0;//Pongo 0 el bit del N0
                                                Monitor.Exit(Simulador.directorioP0);//Libero directorio de la victima
                                                Simulador.cacheDatosN1[posCache, 1] = 0;//Invalido la posicion en la cache

                                            }
                                            else//Si no puedo blouear el directorio de la victima
                                            {
                                                Monitor.Exit(Simulador.memCompartidaP0);//Libero la memoria
                                                Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache local
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                                terminado = false;
                                            }
                                        }
                                        else//No puedo bloquear la memoria
                                        {
                                            terminado = false;
                                            Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache local
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                        }
                                    }
                                    else //Si el bloque esta en el procesador 1
                                    {
                                        if (Monitor.TryEnter(Simulador.memCompartidaP1))//Bloqueo la memoria remota
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;//Aumento el ciclo por bloquear memoria
                                            Simulador.barrera.SignalAndWait();
                                            if (Monitor.TryEnter(Simulador.directorioP1))//Bloqueo el directorio remoto de la victima
                                            {
                                                Simulador.reloj += 5;
                                                cicloActual += 5;//Aumento ciclo por acceder a directorio remoto
                                                for (int i = 0; i < 5; i++)
                                                    Simulador.barrera.SignalAndWait();
                                                guardarAMemoria(1, numProcesadorBloqueVictima, Simulador.cacheDatosN1[posCache, 0], posCache); //Guardo el bloque modificado de a victima a memoria
                                                Simulador.reloj += 40;
                                                cicloActual += 40; //Aumento el ciclo por guardar en memoria compartida del otro procesador
                                                for (int i = 0; i < 40; i++)
                                                    Simulador.barrera.SignalAndWait();
                                                Monitor.Exit(Simulador.memCompartidaP1);//Libero la memoria 
                                                Simulador.directorioP1[Simulador.cacheDatosN1[posCache, 0]-16, 0] = 0;//Pongo U a directorio
                                                Simulador.directorioP1[Simulador.cacheDatosN1[posCache, 0]-16, 1] = 0;//Pongo 0 el bit del N0
                                                Monitor.Exit(Simulador.directorioP1);//Libero directorio de la victima
                                                Simulador.cacheDatosN1[posCache, 1] = 0;//Invalido la posicion en la cache

                                            }
                                            else//Si no puedo blouear el directorio de la victima
                                            {
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                                terminado = false;
                                                Monitor.Exit(Simulador.memCompartidaP1);//Libero la memoria
                                                Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache local
                                            }
                                        }
                                        else//No puedo bloquear la memoria
                                        {
                                            terminado = false;
                                            Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache local
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                        }
                                    }
                                }
                                else //Si la victima esta compartida
                                {
                                    if (numProcesadorBloqueVictima == 0)//Si el bloque esta en el procesador 0
                                    {
                                        if (Monitor.TryEnter(Simulador.directorioP0))//Bloqueo el directorio donde esta la victima
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;//Aumento el ciclo por bloquear memoria
                                            Simulador.barrera.SignalAndWait();
                                            Simulador.directorioP0[Simulador.cacheDatosN1[posCache, 0], 1] = 0;//Cambio bit para indicar que bloque no esta en esa cache
                                            if (Simulador.directorioP0[Simulador.cacheDatosN1[posCache, 0], 2] == 0 && Simulador.directorioP0[Simulador.cacheDatosN1[posCache, 0], 3] == 0)//Si ninguna otra cache tiene el bloque
                                            {
                                                Simulador.directorioP0[Simulador.cacheDatosN1[posCache, 0], 0] = 0;
                                            }
                                            Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                            Simulador.cacheDatosN1[posCache, 1] = 0;//Invalido la posicion en la cache

                                        }
                                        else//No puedo bloquear el directorio
                                        {
                                            terminado = false;
                                            Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache local
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                        }
                                    }
                                    else //Si el bloque esta en el procesador 1
                                    {
                                        if (Monitor.TryEnter(Simulador.directorioP1))//Bloqueo el directorio remoto donde esta la victima
                                        {
                                            Simulador.reloj += 5;
                                            cicloActual += 5;//Aumento el ciclo por bloquear directorio remota
                                            for (int i = 0; i < 5; i++)
                                                Simulador.barrera.SignalAndWait();
                                            Simulador.directorioP1[Simulador.cacheDatosN1[posCache, 0]-16, 1] = 0;//Cambio bit para indicar que bloque no esta en esa cache
                                            if (Simulador.directorioP1[Simulador.cacheDatosN1[posCache, 0]-16, 2] == 0 && Simulador.directorioP1[Simulador.cacheDatosN1[posCache-16, 0], 3] == 0)//Si ninguna otra cache tiene el bloque
                                            {
                                                Simulador.directorioP1[Simulador.cacheDatosN1[posCache, 0]-16, 0] = 0;
                                            }
                                            Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                            Simulador.cacheDatosN1[posCache, 1] = 0;//Invalido la posicion en la cache

                                        }
                                        else//No puedo bloquear el directorio
                                        {
                                            terminado = false;
                                            Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache local
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                        }
                                    }
                                }

                            }
                            //Ya me encargue de la victima del reemplazo
                            if (numProcesadorBloque == 0)//Si el directorio que hay que bloquear es del P0
                            {
                                if (Monitor.TryEnter(Simulador.directorioP0))//Bloqueo el directorio del P0
                                {
                                    Simulador.reloj++;
                                    cicloActual++;//Aumento un ciclo por acceso a directorio local
                                    Simulador.barrera.SignalAndWait();
                                    if (Simulador.directorioP0[numeroBloque, 0] == 0 || Simulador.directorioP0[numeroBloque, 0] == 1)//Si en el directorio esta U o C
                                    {
                                        if (Monitor.TryEnter(Simulador.memCompartidaP0))//Bloqueo la memoria
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                            guardarBloqueEnCache(posCache, numeroBloque, numPalabra, 1);//Guardo el bloque en la cache
                                            Simulador.reloj += 16;
                                            cicloActual += 16;//Aumento 16 por escribir desde memoria local
                                            for (int i = 0; i < 16; i++)
                                                Simulador.barrera.SignalAndWait();
                                            Monitor.Exit(Simulador.memCompartidaP0);//Libero la memoria
                                            if (Simulador.directorioP0[numeroBloque, 0] == 0)
                                                Simulador.directorioP0[numeroBloque, 0] = 1;//Pongo directorio en C
                                            Simulador.directorioP0[numeroBloque, 1] = 1;//Indico que esta en cache
                                            Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                            terminado = true;//Solo falta obtener de cache

                                        }
                                        else//No se puede bloquear la memoria
                                        {
                                            terminado = false;
                                            //Falta regresar a estado anterior
                                            Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache local
                                            Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                        }
                                    }
                                    else//Si en el directorio esta M
                                    {
                                        if (Simulador.directorioP0[numeroBloque, 2] == 1)//Esta en cache del N0
                                        {
                                            if (Monitor.TryEnter(Simulador.cacheDatosN0))//Bloqueo la cache donde esta
                                            {
                                                if (Monitor.TryEnter(Simulador.memCompartidaP0))//Bloqueo la memoria del P0
                                                {
                                                    Simulador.reloj++;
                                                    cicloActual++;//Aumento ciclo por ingresar a memoria
                                                    Simulador.barrera.SignalAndWait();
                                                    guardarAMemoria(0, 0, numeroBloque, posCache);//Guardo lo que tiene la cache en memoria
                                                    Simulador.reloj += 16;
                                                    cicloActual += 16;//Aumento 16 por escribir desde memoria local
                                                    for (int i = 0; i < 16; i++)
                                                        Simulador.barrera.SignalAndWait();
                                                    Monitor.Exit(Simulador.memCompartidaP0);//Libero la memoria
                                                    for (int j = 0; j < 6; j++)//Copio lo que hay en la cache del N1 en la cache del N0
                                                    {
                                                        Simulador.cacheDatosN1[posCache, j] = Simulador.cacheDatosN0[posCache, j];
                                                    }
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                    Simulador.directorioP0[numeroBloque, 0] = 1;//Pongo directorio en C
                                                    Simulador.cacheDatosN0[posCache, 1] = 1;//Pongo cache en C
                                                    Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache
                                                    Simulador.directorioP0[numeroBloque, 1] = 1;//Indico que esta en cache
                                                    Monitor.Exit(Simulador.directorioP0);//Libero el directorio

                                                }
                                                else//No se puede bloquear la memoria
                                                {
                                                    terminado = false;
                                                    //Falta regresar a estado anterior
                                                    Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache local
                                                    Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache remota
                                                    Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                }
                                            }
                                            else//No se puede bloquear la cache
                                            {
                                                terminado = false;
                                                //Falta regresar a estado anterior
                                                Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache local
                                                Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                        else//Esta en cache del N2
                                        {
                                            if (Monitor.TryEnter(Simulador.cacheDatosN2))//Bloqueo la cache donde esta
                                            {
                                                if (Monitor.TryEnter(Simulador.memCompartidaP0))//Bloqueo la memoria del P0
                                                {
                                                    Simulador.reloj++;
                                                    cicloActual++;//Aumento ciclo por ingresar a memoria
                                                    Simulador.barrera.SignalAndWait();
                                                    guardarAMemoria(2, 0, numeroBloque, posCache);//Guardo lo que tiene la cache en memoria
                                                    Simulador.reloj += 40;
                                                    cicloActual += 40;//Aumento 40 por escribir desde cache remoto
                                                    for (int i = 0; i < 40; i++)
                                                        Simulador.barrera.SignalAndWait();
                                                    Monitor.Exit(Simulador.memCompartidaP0);//Libero la memoria
                                                    for (int j = 0; j < 6; j++)//Copio lo que hay en la cache del N2 en la cache del N0
                                                    {
                                                        Simulador.cacheDatosN1[posCache, j] = Simulador.cacheDatosN2[posCache, j];
                                                    }
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                    Simulador.directorioP0[numeroBloque, 0] = 1;//Pongo directorio en C
                                                    Simulador.cacheDatosN2[posCache, 1] = 1;//Pongo cache en C
                                                    Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache
                                                    Simulador.directorioP0[numeroBloque, 1] = 1;//Indico que esta en cache
                                                    Monitor.Exit(Simulador.directorioP0);//Libero el directorio                    
                                                }
                                                else//No se puede bloquear la memoria
                                                {
                                                    terminado = false;
                                                    //Falta regresar a estado anterior
                                                    Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache local
                                                    Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache remota
                                                    Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                }
                                            }
                                            else//No se puede bloquear la cache
                                            {
                                                terminado = false;
                                                //Falta regresar a estado anterior
                                                Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache local
                                                Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                    }
                                }
                                else//No se puede bloquear el directorio
                                {
                                    terminado = false;
                                    //Falta regresar a estado anterior
                                    Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache local
                                    Simulador.reloj++;
                                    cicloActual++;
                                    Simulador.barrera.SignalAndWait();
                                }
                            }
                            else//Si el directorio que hay que bloquear es del P1
                            {
                                if (Monitor.TryEnter(Simulador.directorioP1))//Bloqueo el directorio del P1
                                {
                                    Simulador.reloj += 5;
                                    cicloActual += 5;//Aumento un ciclo por acceso a directorio remoto
                                    for (int i = 0; i < 5; i++)
                                        Simulador.barrera.SignalAndWait();
                                    if (Simulador.directorioP1[numeroBloque-16, 0] == 0 || Simulador.directorioP1[numeroBloque-16, 0] == 1)//Si en el directorio esta U o C
                                    {
                                        if (Monitor.TryEnter(Simulador.memCompartidaP1))//Bloqueo la memoria
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                            guardarBloqueEnCache(posCache, numeroBloque, numPalabra, 1);//Guardo el bloque en la cache
                                            Simulador.reloj += 40;
                                            cicloActual += 40;//Aumento 16 por escribir desde memoria remota
                                            for (int i = 0; i < 40; i++)
                                                Simulador.barrera.SignalAndWait();
                                            Monitor.Exit(Simulador.memCompartidaP1);//Libero la memoria
                                            if (Simulador.directorioP1[numeroBloque-16, 0] == 0)
                                                Simulador.directorioP1[numeroBloque-16, 0] = 1;//Pongo directorio en C
                                            Simulador.directorioP1[numeroBloque-16, 1] = 1;//Indico que esta en cache
                                            Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                            terminado = true;//Solo falta obtener de cache

                                        }
                                        else//No se puede bloquear la memoria
                                        {
                                            terminado = false;
                                            //Falta regresar a estado anterior
                                            Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache local
                                            Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                        }
                                    }
                                    else//Si en el directorio esta M
                                    {
                                        if (Simulador.directorioP1[numeroBloque-16, 2] == 1)//Esta en cache del N0
                                        {
                                            if (Monitor.TryEnter(Simulador.cacheDatosN0))//Bloqueo la cache donde esta
                                            {
                                                if (Monitor.TryEnter(Simulador.memCompartidaP1))//Bloqueo la memoria del P0
                                                {
                                                    Simulador.reloj++;
                                                    cicloActual++;//Aumento ciclo por ingresar a memoria
                                                    Simulador.barrera.SignalAndWait();
                                                    guardarAMemoria(0, 1, numeroBloque, posCache);//Guardo lo que tiene la cache en memoria
                                                    Simulador.reloj += 40;
                                                    cicloActual += 40;//Aumento 16 por escribir a memoria remota
                                                    for (int i = 0; i < 40; i++)
                                                        Simulador.barrera.SignalAndWait();
                                                    Monitor.Exit(Simulador.memCompartidaP1);//Libero la memoria
                                                    for (int j = 0; j < 6; j++)//Copio lo que hay en la cache del N1 en la cache del N0
                                                    {
                                                        Simulador.cacheDatosN1[posCache, j] = Simulador.cacheDatosN0[posCache, j];
                                                    }
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                    Simulador.directorioP1[numeroBloque-16, 0] = 1;//Pongo directorio en C
                                                    Simulador.cacheDatosN0[posCache, 1] = 1;//Pongo cache en C
                                                    Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache
                                                    Simulador.directorioP1[numeroBloque-16, 1] = 1;//Indico que esta en cache
                                                    Monitor.Exit(Simulador.directorioP1);//Libero el directorio

                                                }
                                                else//No se puede bloquear la memoria
                                                {
                                                    terminado = false;
                                                    //Falta regresar a estado anterior
                                                    Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                    Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache remota
                                                    Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                }
                                            }
                                            else//No se puede bloquear la cache
                                            {
                                                terminado = false;
                                                //Falta regresar a estado anterior
                                                Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache local
                                                Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                        else//Esta en cache del N2
                                        {
                                            if (Monitor.TryEnter(Simulador.cacheDatosN2))//Bloqueo la cache donde esta
                                            {
                                                if (Monitor.TryEnter(Simulador.memCompartidaP1))//Bloqueo la memoria del P0
                                                {
                                                    Simulador.reloj++;
                                                    cicloActual++;//Aumento ciclo por ingresar a memoria
                                                    Simulador.barrera.SignalAndWait();
                                                    guardarAMemoria(2, 1, numeroBloque, posCache);//Guardo lo que tiene la cache en memoria
                                                    Simulador.reloj += 16;
                                                    cicloActual += 16;//Aumento 16 por escribir desde cache remoto
                                                    for (int i = 0; i < 16; i++)
                                                        Simulador.barrera.SignalAndWait();
                                                    Monitor.Exit(Simulador.memCompartidaP1);//Libero la memoria
                                                    for (int j = 0; j < 6; j++)//Copio lo que hay en la cache del N2 en la cache del N0
                                                    {
                                                        Simulador.cacheDatosN1[posCache, j] = Simulador.cacheDatosN2[posCache, j];
                                                    }
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                    Simulador.directorioP1[numeroBloque-16, 0] = 1;//Pongo directorio en C
                                                    Simulador.cacheDatosN2[posCache, 1] = 1;//Pongo cache en C
                                                    Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache
                                                    Simulador.directorioP1[numeroBloque-16, 1] = 1;//Indico que esta en cache
                                                    Monitor.Exit(Simulador.directorioP1);//Libero el directorio                    
                                                }
                                                else//No se puede bloquear la memoria
                                                {
                                                    terminado = false;
                                                    //Falta regresar a estado anterior
                                                    Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache local
                                                    Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache remota
                                                    Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                }
                                            }
                                            else//No se puede bloquear la cache
                                            {
                                                terminado = false;
                                                //Falta regresar a estado anterior
                                                Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache local
                                                Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                    }
                                }
                                else//No se puede bloquear el directorio
                                {
                                    terminado = false;
                                    //Falta regresar a estado anterior
                                    Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache local
                                    Simulador.reloj++;
                                    cicloActual++;
                                    Simulador.barrera.SignalAndWait();
                                }
                            }

                        }
                        else
                            terminado = true;
                        if (terminado)//Si esta en cache y esta modificado o compartido
                        {
                            registros[IR[2]] = Simulador.cacheDatosN1[posCache, numPalabra + 2];//Copiamos lo que tiene la cache en el registro
                            Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache local
                            terminado = true;
                        }
                    }
                    else//No se pudo bloquear la cache local
                    {
                        terminado = false;
                        Simulador.reloj++;
                        cicloActual++;
                        Simulador.barrera.SignalAndWait();
                    }
                }

            }
            else//Es del nucleo 2
            {
                while (!terminado)
                {
                    if (Monitor.TryEnter(Simulador.cacheDatosN2))//Bloqueo la cache local
                    {
                        if (!bloqueEnCache(posCache, numeroBloque)) //Si no se encuentra en cache o esta invalido
                        {
                            if (Simulador.cacheDatosN2[posCache, 1] != 0) //Si la victima no esta invalida
                            {
                                numProcesadorBloqueVictima = numDirectorio(Simulador.cacheDatosN2[posCache, 0]);//Obtengo en cual procesador se encuentra el bloque
                                if (Simulador.cacheDatosN2[posCache, 1] == 2) //Si la victima esta modificada
                                {
                                    if (numProcesadorBloqueVictima == 0)//Si el bloque esta en el procesador 0
                                    {
                                        if (Monitor.TryEnter(Simulador.memCompartidaP0))//Bloqueo la memoria
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;//Aumento el ciclo por bloquear memoria
                                            Simulador.barrera.SignalAndWait();
                                            if (Monitor.TryEnter(Simulador.directorioP0))//Bloqueo el directorio de la victima
                                            {
                                                Simulador.reloj += 5;
                                                cicloActual += 5;//Aumento ciclo por acceder a directorio remoto
                                                for(int i = 0; i < 5; i++)
                                                    Simulador.barrera.SignalAndWait();
                                                guardarAMemoria(2, numProcesadorBloqueVictima, Simulador.cacheDatosN2[posCache, 0], posCache); //Guardo el bloque modificado de a victima a memoria
                                                Simulador.reloj += 40;
                                                cicloActual += 40; //Aumento el ciclo por guardar en memoria compartida del procesador
                                                for (int i = 0; i < 40; i++)
                                                    Simulador.barrera.SignalAndWait();
                                                Monitor.Exit(Simulador.memCompartidaP0);//Libero la memoria 
                                                Simulador.directorioP0[Simulador.cacheDatosN2[posCache, 0], 0] = 0;//Pongo U a directorio
                                                Simulador.directorioP0[Simulador.cacheDatosN2[posCache, 0], 1] = 0;//Pongo 0 el bit del N0
                                                Monitor.Exit(Simulador.directorioP0);//Libero directorio de la victima
                                                Simulador.cacheDatosN2[posCache, 1] = 0;//Invalido la posicion en la cache

                                            }
                                            else//Si no puedo blouear el directorio de la victima
                                            {
                                                Monitor.Exit(Simulador.memCompartidaP0);//Libero la memoria
                                                Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache local
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                                terminado = false;
                                            }
                                        }
                                        else//No puedo bloquear la memoria
                                        {
                                            terminado = false;
                                            Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache local
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                        }
                                    }
                                    else //Si el bloque esta en el procesador 1
                                    {
                                        if (Monitor.TryEnter(Simulador.memCompartidaP1))//Bloqueo la memoria remota
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;//Aumento el ciclo por bloquear memoria
                                            Simulador.barrera.SignalAndWait();
                                            if (Monitor.TryEnter(Simulador.directorioP1))//Bloqueo el directorio remoto de la victima
                                            {
                                                Simulador.reloj ++;
                                                cicloActual ++;//Aumento ciclo por acceder a directorio local
                                                Simulador.barrera.SignalAndWait();
                                                guardarAMemoria(2, numProcesadorBloqueVictima, Simulador.cacheDatosN2[posCache, 0], posCache); //Guardo el bloque modificado de a victima a memoria
                                                Simulador.reloj += 16;
                                                cicloActual += 16; //Aumento el ciclo por guardar en memoria compartida del otro procesador
                                                for (int i = 0; i < 16; i++)
                                                    Simulador.barrera.SignalAndWait();
                                                Monitor.Exit(Simulador.memCompartidaP1);//Libero la memoria 
                                                Simulador.directorioP1[Simulador.cacheDatosN2[posCache, 0]-16, 0] = 0;//Pongo U a directorio
                                                Simulador.directorioP1[Simulador.cacheDatosN2[posCache, 0]-16, 1] = 0;//Pongo 0 el bit del N0
                                                Monitor.Exit(Simulador.directorioP1);//Libero directorio de la victima
                                                Simulador.cacheDatosN2[posCache, 1] = 0;//Invalido la posicion en la cache

                                            }
                                            else//Si no puedo blouear el directorio de la victima
                                            {
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                                terminado = false;
                                                Monitor.Exit(Simulador.memCompartidaP1);//Libero la memoria
                                                Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache local
                                            }
                                        }
                                        else//No puedo bloquear la memoria
                                        {
                                            terminado = false;
                                            Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache local
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                        }
                                    }
                                }
                                else //Si la victima esta compartida
                                {
                                    if (numProcesadorBloqueVictima == 0)//Si el bloque esta en el procesador 0
                                    {
                                        if (Monitor.TryEnter(Simulador.directorioP0))//Bloqueo el directorio donde esta la victima
                                        {
                                            Simulador.reloj += 5;
                                            cicloActual += 5;//Aumento el ciclo por bloquear directorio remota
                                            for (int i = 0; i < 5; i++)
                                                Simulador.barrera.SignalAndWait();
                                            Simulador.directorioP0[Simulador.cacheDatosN2[posCache, 0], 1] = 0;//Cambio bit para indicar que bloque no esta en esa cache
                                            if (Simulador.directorioP0[Simulador.cacheDatosN2[posCache, 0], 2] == 0 && Simulador.directorioP0[Simulador.cacheDatosN2[posCache, 0], 3] == 0)//Si ninguna otra cache tiene el bloque
                                            {
                                                Simulador.directorioP0[Simulador.cacheDatosN2[posCache, 0], 0] = 0;
                                            }
                                            Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                            Simulador.cacheDatosN2[posCache, 1] = 0;//Invalido la posicion en la cache

                                        }
                                        else//No puedo bloquear el directorio
                                        {
                                            terminado = false;
                                            Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache local
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                        }
                                    }
                                    else //Si el bloque esta en el procesador 1
                                    {
                                        if (Monitor.TryEnter(Simulador.directorioP1))//Bloqueo el directorio remoto donde esta la victima
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;//Aumento el ciclo por bloquear directorio local
                                            Simulador.barrera.SignalAndWait();
                                            Simulador.directorioP1[Simulador.cacheDatosN2[posCache, 0]-16, 1] = 0;//Cambio bit para indicar que bloque no esta en esa cache
                                            if (Simulador.directorioP1[Simulador.cacheDatosN2[posCache, 0]-16, 2] == 0 && Simulador.directorioP1[Simulador.cacheDatosN2[posCache, 0]-16, 3] == 0)//Si ninguna otra cache tiene el bloque
                                            {
                                                Simulador.directorioP1[Simulador.cacheDatosN2[posCache, 0]-16, 0] = 0;
                                            }
                                            Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                            Simulador.cacheDatosN2[posCache, 1] = 0;//Invalido la posicion en la cache

                                        }
                                        else//No puedo bloquear el directorio
                                        {
                                            terminado = false;
                                            Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache local
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                        }
                                    }
                                }

                            }
                            //Ya me encargue de la victima del reemplazo
                            if (numProcesadorBloque == 0)//Si el directorio que hay que bloquear es del P0
                            {
                                if (Monitor.TryEnter(Simulador.directorioP0))//Bloqueo el directorio del P0
                                {
                                    Simulador.reloj+=5;
                                    cicloActual+=5;//Aumento un ciclo por acceso a directorio remoto
                                    for(int i = 0; i < 5; i++)
                                        Simulador.barrera.SignalAndWait();
                                    if (Simulador.directorioP0[numeroBloque, 0] == 0 || Simulador.directorioP0[numeroBloque, 0] == 1)//Si en el directorio esta U o C
                                    {
                                        if (Monitor.TryEnter(Simulador.memCompartidaP0))//Bloqueo la memoria
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                            guardarBloqueEnCache(posCache, numeroBloque, numPalabra, 1);//Guardo el bloque en la cache
                                            Simulador.reloj += 40;
                                            cicloActual += 40;//Aumento 16 por escribir desde memoria remoto
                                            for (int i = 0; i < 40; i++)
                                                Simulador.barrera.SignalAndWait();
                                            Monitor.Exit(Simulador.memCompartidaP0);//Libero la memoria
                                            if (Simulador.directorioP0[numeroBloque, 0] == 0)
                                                Simulador.directorioP0[numeroBloque, 0] = 1;//Pongo directorio en C
                                            Simulador.directorioP0[numeroBloque, 1] = 1;//Indico que esta en cache
                                            Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                            terminado = true;//Solo falta obtener de cache

                                        }
                                        else//No se puede bloquear la memoria
                                        {
                                            terminado = false;
                                            //Falta regresar a estado anterior
                                            Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache local
                                            Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                        }
                                    }
                                    else//Si en el directorio esta M
                                    {
                                        if (Simulador.directorioP0[numeroBloque, 2] == 1)//Esta en cache del N0
                                        {
                                            if (Monitor.TryEnter(Simulador.cacheDatosN0))//Bloqueo la cache donde esta
                                            {
                                                if (Monitor.TryEnter(Simulador.memCompartidaP0))//Bloqueo la memoria del P0
                                                {
                                                    Simulador.reloj++;
                                                    cicloActual++;//Aumento ciclo por ingresar a memoria
                                                    Simulador.barrera.SignalAndWait();
                                                    guardarAMemoria(0, 0, numeroBloque, posCache);//Guardo lo que tiene la cache en memoria
                                                    Simulador.reloj += 16;
                                                    cicloActual += 16;//Aumento 16 por escribir desde memoria local
                                                    for (int i = 0; i < 40; i++)
                                                        Simulador.barrera.SignalAndWait();
                                                    Monitor.Exit(Simulador.memCompartidaP0);//Libero la memoria
                                                    for (int j = 0; j < 6; j++)//Copio lo que hay en la cache del N1 en la cache del N2
                                                    {
                                                        Simulador.cacheDatosN2[posCache, j] = Simulador.cacheDatosN0[posCache, j];
                                                    }
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                    Simulador.directorioP0[numeroBloque, 0] = 1;//Pongo directorio en C
                                                    Simulador.cacheDatosN0[posCache, 1] = 1;//Pongo cache en C
                                                    Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache
                                                    Simulador.directorioP0[numeroBloque, 1] = 1;//Indico que esta en cache
                                                    Monitor.Exit(Simulador.directorioP0);//Libero el directorio

                                                }
                                                else//No se puede bloquear la memoria
                                                {
                                                    terminado = false;
                                                    //Falta regresar a estado anterior
                                                    Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                    Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache remota
                                                    Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                }
                                            }
                                            else//No se puede bloquear la cache
                                            {
                                                terminado = false;
                                                //Falta regresar a estado anterior
                                                Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache local
                                                Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                        else//Esta en cache del N1
                                        {
                                            if (Monitor.TryEnter(Simulador.cacheDatosN1))//Bloqueo la cache donde esta
                                            {
                                                if (Monitor.TryEnter(Simulador.memCompartidaP0))//Bloqueo la memoria del P0
                                                {
                                                    Simulador.reloj++;
                                                    cicloActual++;//Aumento ciclo por ingresar a memoria
                                                    Simulador.barrera.SignalAndWait();
                                                    guardarAMemoria(1, 0, numeroBloque, posCache);//Guardo lo que tiene la cache en memoria
                                                    Simulador.reloj += 16;
                                                    cicloActual += 16;//Aumento 16 por escribir desde cache local
                                                    for (int i = 0; i < 16; i++)
                                                        Simulador.barrera.SignalAndWait();
                                                    Monitor.Exit(Simulador.memCompartidaP0);//Libero la memoria
                                                    for (int j = 0; j < 6; j++)//Copio lo que hay en la cache del N2 en la cache del N0
                                                    {
                                                        Simulador.cacheDatosN2[posCache, j] = Simulador.cacheDatosN1[posCache, j];
                                                    }
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                    Simulador.directorioP0[numeroBloque, 0] = 1;//Pongo directorio en C
                                                    Simulador.cacheDatosN1[posCache, 1] = 1;//Pongo cache en C
                                                    Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache
                                                    Simulador.directorioP0[numeroBloque, 1] = 1;//Indico que esta en cache
                                                    Monitor.Exit(Simulador.directorioP0);//Libero el directorio                    
                                                }
                                                else//No se puede bloquear la memoria
                                                {
                                                    terminado = false;
                                                    //Falta regresar a estado anterior
                                                    Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache local
                                                    Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache remota
                                                    Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                }
                                            }
                                            else//No se puede bloquear la cache
                                            {
                                                terminado = false;
                                                //Falta regresar a estado anterior
                                                Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache local
                                                Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                    }
                                }
                                else//No se puede bloquear el directorio
                                {
                                    terminado = false;
                                    //Falta regresar a estado anterior
                                    Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache local
                                    Simulador.reloj++;
                                    cicloActual++;
                                    Simulador.barrera.SignalAndWait();
                                }
                            }
                            else//Si el directorio que hay que bloquear es del P1
                            {
                                if (Monitor.TryEnter(Simulador.directorioP1))//Bloqueo el directorio del P1
                                {
                                    Simulador.reloj ++;
                                    cicloActual ++;//Aumento un ciclo por acceso a directorio remoto
                                    Simulador.barrera.SignalAndWait();
                                    if (Simulador.directorioP1[numeroBloque-16, 0] == 0 || Simulador.directorioP1[numeroBloque-16, 0] == 1)//Si en el directorio esta U o C
                                    {
                                        if (Monitor.TryEnter(Simulador.memCompartidaP1))//Bloqueo la memoria
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                            guardarBloqueEnCache(posCache, numeroBloque, numPalabra, 1);//Guardo el bloque en la cache
                                            Simulador.reloj += 16;
                                            cicloActual += 16;//Aumento 16 por escribir desde memoria remota
                                            for (int i = 0; i < 40; i++)
                                                Simulador.barrera.SignalAndWait();
                                            Monitor.Exit(Simulador.memCompartidaP1);//Libero la memoria
                                            if (Simulador.directorioP1[numeroBloque-16, 0] == 0)
                                                Simulador.directorioP1[numeroBloque-16, 0] = 1;//Pongo directorio en C
                                            Simulador.directorioP1[numeroBloque-16, 1] = 1;//Indico que esta en cache
                                            Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                            terminado = true;//Solo falta obtener de cache

                                        }
                                        else//No se puede bloquear la memoria
                                        {
                                            terminado = false;
                                            //Falta regresar a estado anterior
                                            Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache local
                                            Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                        }
                                    }
                                    else//Si en el directorio esta M
                                    {
                                        if (Simulador.directorioP1[numeroBloque-16, 2] == 1)//Esta en cache del N0
                                        {
                                            if (Monitor.TryEnter(Simulador.cacheDatosN0))//Bloqueo la cache donde esta
                                            {
                                                if (Monitor.TryEnter(Simulador.memCompartidaP1))//Bloqueo la memoria del P0
                                                {
                                                    Simulador.reloj++;
                                                    cicloActual++;//Aumento ciclo por ingresar a memoria
                                                    Simulador.barrera.SignalAndWait();
                                                    guardarAMemoria(0, 1, numeroBloque, posCache);//Guardo lo que tiene la cache en memoria
                                                    Simulador.reloj += 40;
                                                    cicloActual += 40;//Aumento 16 por escribir a memoria remota
                                                    for (int i = 0; i < 40; i++)
                                                        Simulador.barrera.SignalAndWait();
                                                    Monitor.Exit(Simulador.memCompartidaP1);//Libero la memoria
                                                    for (int j = 0; j < 6; j++)//Copio lo que hay en la cache del N1 en la cache del N0
                                                    {
                                                        Simulador.cacheDatosN2[posCache, j] = Simulador.cacheDatosN0[posCache, j];
                                                    }
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                    Simulador.directorioP1[numeroBloque-16, 0] = 1;//Pongo directorio en C
                                                    Simulador.cacheDatosN0[posCache, 1] = 1;//Pongo cache en C
                                                    Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache
                                                    Simulador.directorioP1[numeroBloque-16, 1] = 1;//Indico que esta en cache
                                                    Monitor.Exit(Simulador.directorioP1);//Libero el directorio

                                                }
                                                else//No se puede bloquear la memoria
                                                {
                                                    terminado = false;
                                                    //Falta regresar a estado anterior
                                                    Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                    Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache remota
                                                    Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                }
                                            }
                                            else//No se puede bloquear la cache
                                            {
                                                terminado = false;
                                                //Falta regresar a estado anterior
                                                Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                        else//Esta en cache del N1
                                        {
                                            if (Monitor.TryEnter(Simulador.cacheDatosN1))//Bloqueo la cache donde esta
                                            {
                                                if (Monitor.TryEnter(Simulador.memCompartidaP1))//Bloqueo la memoria del P0
                                                {
                                                    Simulador.reloj++;
                                                    cicloActual++;//Aumento ciclo por ingresar a memoria
                                                    Simulador.barrera.SignalAndWait();
                                                    guardarAMemoria(1, 1, numeroBloque, posCache);//Guardo lo que tiene la cache en memoria
                                                    Simulador.reloj += 40;
                                                    cicloActual += 40;//Aumento 40 por escribir desde cache remoto
                                                    for (int i = 0; i < 40; i++)
                                                        Simulador.barrera.SignalAndWait();
                                                    Monitor.Exit(Simulador.memCompartidaP1);//Libero la memoria
                                                    for (int j = 0; j < 6; j++)//Copio lo que hay en la cache del N2 en la cache del N0
                                                    {
                                                        Simulador.cacheDatosN2[posCache, j] = Simulador.cacheDatosN1[posCache, j];
                                                    }
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                    Simulador.directorioP1[numeroBloque-16, 0] = 1;//Pongo directorio en C
                                                    Simulador.cacheDatosN1[posCache, 1] = 1;//Pongo cache en C
                                                    Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache
                                                    Simulador.directorioP1[numeroBloque-16, 1] = 1;//Indico que esta en cache
                                                    Monitor.Exit(Simulador.directorioP1);//Libero el directorio                    
                                                }
                                                else//No se puede bloquear la memoria
                                                {
                                                    terminado = false;
                                                    //Falta regresar a estado anterior
                                                    Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache local
                                                    Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache remota
                                                    Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                }
                                            }
                                            else//No se puede bloquear la cache
                                            {
                                                terminado = false;
                                                //Falta regresar a estado anterior
                                                Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache local
                                                Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                    }
                                }
                                else//No se puede bloquear el directorio
                                {
                                    terminado = false;
                                    //Falta regresar a estado anterior
                                    Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache local
                                    Simulador.reloj++;
                                    cicloActual++;
                                    Simulador.barrera.SignalAndWait();
                                }
                            }

                        }
                        else
                            terminado = true;
                        if (terminado)//Si esta en cache y esta modificado o compartido
                        {
                            registros[IR[2]] = Simulador.cacheDatosN2[posCache, numPalabra + 2];//Copiamos lo que tiene la cache en el registro
                            Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache local
                            terminado = true;
                        }
                    }
                    else//No se pudo bloquear la cache local
                    {
                        terminado = false;
                        Simulador.reloj++;
                        cicloActual++;
                        Simulador.barrera.SignalAndWait();
                    }
                }

            }

            //Console.WriteLine("Instrucción LW ejecutada en el contexto ");
        }


        /// <summary>
        /// Ejecuta la instrucción SW
        /// </summary>
        private void instruccionSW()
        {
            logExecution += "Instrucción SW ejecutada en el contexto " + contextoActual + "\n";
            int numeroBloque = obtenerNumBloque();
            int numPalabra = obtenerNumPalabra();
            int posCache = obtenerPosCache(numeroBloque);
            int numProcesadorBloque = numDirectorio(numeroBloque);

            bool terminado = false;

            if(numNucleo == 0)//SW para nucleo 0
            {
                while (!terminado)
                {
                    if (Monitor.TryEnter(Simulador.cacheDatosN0))//Bloque la cache del nucleo 0
                    {
                        if (bloqueEnCache(posCache, numeroBloque))//Si esta modificado o compartido
                        {
                            if(Simulador.cacheDatosN0[posCache,1] == 2)//Si esta modificado
                            {
                                terminado = true;
                                /*if (numeroBloque < 16)//Si esta en memoria P0
                                {
                                    if (Monitor.TryEnter(Simulador.memCompartidaP0))//Bloquear memoria P0
                                    {
                                        Simulador.reloj++;
                                        cicloActual++;
                                        Simulador.barrera.SignalAndWait();
                                        guardarAMemoria(0, 0, numeroBloque, posCache);//Guardo el bloque modificado
                                        Simulador.reloj+=16;
                                        cicloActual+=16;
                                        for(int i = 0;  i < 16; i++)
                                            Simulador.barrera.SignalAndWait();
                                        Monitor.Exit(Simulador.memCompartidaP0);
                                        terminado = true; //Solo falta subir cache a registro
                                    }
                                    else//No se puede bloquear
                                    {
                                        terminado = false;
                                        Simulador.reloj++;
                                        cicloActual++;
                                        Monitor.Exit(Simulador.cacheDatosN0);
                                        Simulador.barrera.SignalAndWait();
                                    }
                                }
                                else//Si esta en memoria P1
                                {
                                    if (Monitor.TryEnter(Simulador.memCompartidaP1))//Bloquear memoria P1
                                    {
                                        Simulador.reloj++;
                                        cicloActual++;
                                        Simulador.barrera.SignalAndWait();
                                        guardarAMemoria(0, 1, numeroBloque, posCache);//Guardo el bloque modificado
                                        Simulador.reloj += 40;
                                        cicloActual += 40;
                                        for (int i = 0; i < 40; i++)
                                            Simulador.barrera.SignalAndWait();
                                        Monitor.Exit(Simulador.memCompartidaP1);
                                        terminado = true; //Solo falta subir cache a registro
                                    }
                                    else//No se puede bloquear
                                    {
                                        terminado = false;
                                        Simulador.reloj++;
                                        cicloActual++;
                                        Monitor.Exit(Simulador.cacheDatosN0);
                                        Simulador.barrera.SignalAndWait();
                                    }
                                }*/
                            }
                            else if(Simulador.cacheDatosN0[posCache, 1] == 1)//Si esta compartido
                            {
                                if(numeroBloque < 16)//Directorio P0
                                {
                                    if (Monitor.TryEnter(Simulador.directorioP0))//Bloquear directorio P0
                                    {
                                        Simulador.reloj ++;
                                        cicloActual ++;
                                        Simulador.barrera.SignalAndWait();
                                        if (Simulador.directorioP0[numeroBloque, 2] == 1)//Esta tambien en cache del N1
                                        {
                                            if (Monitor.TryEnter(Simulador.cacheDatosN1))//Bloqueo cache del nucleo 1
                                            {
                                                Simulador.cacheDatosN1[posCache, 1] = 0; //Pongo invalido en cache N1
                                                Monitor.Exit(Simulador.cacheDatosN1);//Libero cache N1
                                            }
                                            else//No se puede bloquear
                                            {
                                                terminado = false;
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Monitor.Exit(Simulador.cacheDatosN0);
                                                Monitor.Exit(Simulador.directorioP0);
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                        else//Esta tambien en cache del N2
                                        {
                                            if (Monitor.TryEnter(Simulador.cacheDatosN2))//Bloqueo cache del nucleo 2
                                            {
                                                Simulador.cacheDatosN2[posCache, 1] = 0; //Pongo invalido en cache N2
                                                Monitor.Exit(Simulador.cacheDatosN2);//Libero cache N1
                                            }
                                            else//No se puede bloquear
                                            {
                                                terminado = false;
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Monitor.Exit(Simulador.cacheDatosN0);
                                                Monitor.Exit(Simulador.directorioP0);
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                        Simulador.directorioP0[numeroBloque, 0] = 2;//Pongo directorio en M
                                        Simulador.directorioP0[numeroBloque, 2] = 0;//Cambio bit N1 a 0
                                        Simulador.directorioP0[numeroBloque, 3] = 0;//Cambio bit N2 a 0
                                        terminado = true; //solo falta subir registro a cache
                                    }
                                    else//No se puede bloquear directorio
                                    {
                                        terminado = false;
                                        Simulador.reloj++;
                                        cicloActual++;
                                        Monitor.Exit(Simulador.cacheDatosN0);
                                        Simulador.barrera.SignalAndWait();
                                    }
                                }
                                else//Directorio P1
                                {
                                    if (Monitor.TryEnter(Simulador.directorioP1))//Bloquear directorio P1
                                    {
                                        Simulador.reloj += 5;
                                        cicloActual += 5;
                                        for (int i = 0; i < 5; i++)
                                            Simulador.barrera.SignalAndWait();
                                        if (Simulador.directorioP1[numeroBloque-16, 2] == 1)//Esta tambien en cache del N1
                                        {
                                            if (Monitor.TryEnter(Simulador.cacheDatosN1))//Bloqueo cache del nucleo 1
                                            {
                                                Simulador.cacheDatosN1[posCache, 1] = 0; //Pongo invalido en cache N1
                                                Monitor.Exit(Simulador.cacheDatosN1);//Libero cache N1
                                            }
                                            else//No se puede bloquear
                                            {
                                                terminado = false;
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Monitor.Exit(Simulador.cacheDatosN0);
                                                Monitor.Exit(Simulador.directorioP1);
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                        else//Esta tambien en cache del N2
                                        {
                                            if (Monitor.TryEnter(Simulador.cacheDatosN2))//Bloqueo cache del nucleo 2
                                            {
                                                Simulador.cacheDatosN2[posCache, 1] = 0; //Pongo invalido en cache N2
                                                Monitor.Exit(Simulador.cacheDatosN2);//Libero cache N1
                                            }
                                            else//No se puede bloquear
                                            {
                                                terminado = false;
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Monitor.Exit(Simulador.cacheDatosN0);
                                                Monitor.Exit(Simulador.directorioP1);
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                        Simulador.directorioP1[numeroBloque-16, 0] = 2;//Pongo directorio en M
                                        Simulador.directorioP1[numeroBloque-16, 2] = 0;//Cambio bit N1 a 0
                                        Simulador.directorioP1[numeroBloque-16, 3] = 0;//Cambio bit N2 a 0
                                        terminado = true; //solo falta subir registro a cache
                                    }
                                    else//No se puede bloquear directorio
                                    {
                                        terminado = false;
                                        Simulador.reloj++;
                                        cicloActual++;
                                        Monitor.Exit(Simulador.cacheDatosN0);
                                        Simulador.barrera.SignalAndWait();
                                    }
                                }
                            }
                        }
                        else//Esta invalido o no esta
                        {
                            if(Simulador.cacheDatosN0[posCache, 1] == 2)//Si victima esta modificada
                            {
                                if(Simulador.cacheDatosN0[posCache,0] < 16)//Victima es de memoria de P0
                                {
                                    if (Monitor.TryEnter(Simulador.directorioP0))//Bloqueo directorio P0
                                    {
                                        Simulador.reloj++;
                                        cicloActual++;
                                        Simulador.barrera.SignalAndWait();
                                        if (Monitor.TryEnter(Simulador.memCompartidaP0))//Bloqueo memoria
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                            guardarAMemoria(0, 0, numeroBloque, posCache);
                                            Simulador.reloj += 16;
                                            cicloActual += 16;
                                            for (int i = 0; i < 16; i++)
                                                Simulador.barrera.SignalAndWait();
                                            Monitor.Exit(Simulador.memCompartidaP0);
                                            Simulador.directorioP0[Simulador.cacheDatosN0[posCache, 0], 1] = 0;//Indico que ya no esta en cache N0
                                            Simulador.directorioP0[Simulador.cacheDatosN0[posCache, 0], 0] = 0;//La pongo Uncached
                                            Monitor.Exit(Simulador.directorioP0);
                                        }
                                        else//No se puede bloquear memoria
                                        {
                                            terminado = false;
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                            Monitor.Exit(Simulador.cacheDatosN0);
                                        }                 
                                    }
                                    else//No se puede bloquear
                                    {
                                        terminado = false;
                                        Simulador.reloj++;
                                        cicloActual++;
                                        Simulador.barrera.SignalAndWait();
                                        Monitor.Exit(Simulador.cacheDatosN0);
                                    }
                                }
                                else//Victima es de memoria de P1
                                {
                                    if (Monitor.TryEnter(Simulador.directorioP1))//Bloqueo directorio P1
                                    {
                                        Simulador.reloj += 5;
                                        cicloActual += 5;
                                        for (int i = 0; i < 5; i++)
                                            Simulador.barrera.SignalAndWait();
                                        if (Monitor.TryEnter(Simulador.memCompartidaP1))//Bloqueo memoria
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                            guardarAMemoria(0, 1, numeroBloque, posCache);
                                            Simulador.reloj += 40;
                                            cicloActual += 40;
                                            for (int i = 0; i < 40; i++)
                                                Simulador.barrera.SignalAndWait();
                                            Monitor.Exit(Simulador.memCompartidaP1);
                                            Simulador.directorioP1[Simulador.cacheDatosN0[posCache, 0]-16, 1] = 0;//Indico que ya no esta en cache N0
                                            Simulador.directorioP1[Simulador.cacheDatosN0[posCache, 0]-16, 0] = 0;//La pongo Uncached
                                            Monitor.Exit(Simulador.directorioP1);
                                        }
                                        else//No se puede bloquear memoria
                                        {
                                            terminado = false;
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                        }
                                    }
                                    else//No se puede bloquear
                                    {
                                        terminado = false;
                                        Simulador.reloj++;
                                        cicloActual++;
                                        Simulador.barrera.SignalAndWait();
                                        Monitor.Exit(Simulador.cacheDatosN0);
                                    }                             
                                }
                            }
                            if (Simulador.cacheDatosN0[posCache, 1] == 1)//Si no esta invalida, entra si es C
                            {
                                if (Simulador.cacheDatosN0[posCache, 0] < 16)//Victima esta en directorio P0
                                {
                                    if (Monitor.TryEnter(Simulador.directorioP0))//Bloqueo directorio P0
                                    {
                                        Simulador.reloj ++;
                                        cicloActual ++;
                                        Simulador.barrera.SignalAndWait();
                                        Simulador.directorioP0[Simulador.cacheDatosN0[posCache, 0], 1] = 0;//Indico que ya no esta en cache N0
                                        if(Simulador.directorioP0[Simulador.cacheDatosN0[posCache, 0], 2] == 0 && Simulador.directorioP0[Simulador.cacheDatosN0[posCache, 0], 3] == 0)//Si ninuna otra cache lo tiene
                                            Simulador.directorioP0[Simulador.cacheDatosN0[posCache, 0], 0] = 0;//La pongo Uncached
                                        Monitor.Exit(Simulador.directorioP0);
                                    }
                                    else//No se puede bloquear
                                    {
                                        terminado = false;
                                        Simulador.reloj++;
                                        cicloActual++;
                                        Simulador.barrera.SignalAndWait();
                                        Monitor.Exit(Simulador.cacheDatosN0);
                                    }
                                }
                                else//Victima esta en directorio P1
                                {
                                    if (Monitor.TryEnter(Simulador.directorioP1))//Bloqueo directorio P1
                                    {
                                        Simulador.reloj+=5;
                                        cicloActual+=5;
                                        for (int i = 0; i < 5; i++)
                                            Simulador.barrera.SignalAndWait();
                                        Simulador.directorioP1[Simulador.cacheDatosN0[posCache, 0]-16, 1] = 0;//Indico que ya no esta en cache N0
                                        if (Simulador.directorioP1[Simulador.cacheDatosN0[posCache, 0]-16, 2] == 0 && Simulador.directorioP1[Simulador.cacheDatosN0[posCache, 0]-16, 3] == 0)//Si ninuna otra cache lo tiene
                                            Simulador.directorioP1[Simulador.cacheDatosN0[posCache, 0]-16, 0] = 0;//La pongo Uncached
                                        Monitor.Exit(Simulador.directorioP1);
                                    }
                                    else//No se puede bloquear
                                    {
                                        terminado = false;
                                        Simulador.reloj++;
                                        cicloActual++;
                                        Simulador.barrera.SignalAndWait();
                                        Monitor.Exit(Simulador.cacheDatosN0);
                                    }
                                }
                            }
                            //Me encargue de victima o estaba invalido en cache, le caigo encima
                            if(numeroBloque < 16)//Esta en P0
                            {
                                if (Monitor.TryEnter(Simulador.directorioP0))//Bloque el directorio
                                {
                                    Simulador.reloj++;
                                    cicloActual++;
                                    Simulador.barrera.SignalAndWait();
                                    if (Simulador.directorioP0[numeroBloque, 0] == 2)//Si el bloque que necesito esta M
                                    {
                                        if (Simulador.directorioP0[numeroBloque, 2] == 1)//Esta modificado en N1
                                        {
                                            if (Monitor.TryEnter(Simulador.cacheDatosN1))//Bloque la cache que lo tiene
                                            {
                                                    if (Monitor.TryEnter(Simulador.memCompartidaP0))//Bloque memoria de P0
                                                    {
                                                        Simulador.reloj++;
                                                        cicloActual++;
                                                        Simulador.barrera.SignalAndWait();
                                                        guardarAMemoria(1, 0, numeroBloque, posCache);
                                                        Simulador.reloj += 16;
                                                        cicloActual += 16;
                                                        for (int i = 0; i < 16; i++)
                                                            Simulador.barrera.SignalAndWait();
                                                        Monitor.Exit(Simulador.memCompartidaP0);
                                                    }
                                                    else//No puedo bloquear
                                                    {
                                                        terminado = false;
                                                        Simulador.reloj++;
                                                        cicloActual++;
                                                        Simulador.barrera.SignalAndWait();
                                                        Monitor.Exit(Simulador.cacheDatosN0);
                                                        Monitor.Exit(Simulador.cacheDatosN1);
                                                        Monitor.Exit(Simulador.directorioP0);
                                                    }
                                                Simulador.cacheDatosN1[posCache, 1] = 0;//Coloco invalido bloque en cache
                                                Monitor.Exit(Simulador.cacheDatosN1);
                                            }
                                            else//No lo puedo bloquear
                                            {
                                                terminado = false;
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                                Monitor.Exit(Simulador.cacheDatosN0);
                                                Monitor.Exit(Simulador.directorioP0);
                                            }
                                        }
                                        else //Esta modificado en N2
                                        {
                                            if (Monitor.TryEnter(Simulador.cacheDatosN2))//Bloque la cache que lo tiene
                                            {
                                                    if (Monitor.TryEnter(Simulador.memCompartidaP0))//Bloque memoria de P0
                                                    {
                                                        Simulador.reloj++;
                                                        cicloActual++;
                                                        Simulador.barrera.SignalAndWait();
                                                        guardarAMemoria(2, 0, numeroBloque, posCache);
                                                        Simulador.reloj += 40;
                                                        cicloActual += 40;
                                                        for (int i = 0; i < 40; i++)
                                                            Simulador.barrera.SignalAndWait();
                                                        Monitor.Exit(Simulador.memCompartidaP0);
                                                    }
                                                    else//No puedo bloquear
                                                    {
                                                        terminado = false;
                                                        Simulador.reloj++;
                                                        cicloActual++;
                                                        Simulador.barrera.SignalAndWait();
                                                        Monitor.Exit(Simulador.cacheDatosN0);
                                                        Monitor.Exit(Simulador.cacheDatosN1);
                                                        Monitor.Exit(Simulador.directorioP0);
                                                    }
                                                
                                                Simulador.cacheDatosN2[posCache, 1] = 0;//Coloco invalido bloque en cache
                                                Monitor.Exit(Simulador.cacheDatosN2);
                                            }
                                            else//No lo puedo bloquear
                                            {
                                                terminado = false;
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                                Monitor.Exit(Simulador.cacheDatosN0);
                                                Monitor.Exit(Simulador.directorioP0);
                                            }
                                        }
                                    }
                                    else if (Simulador.directorioP0[numeroBloque, 0] == 1)//Si el bloque que necesito esta C
                                    {
                                        if (Simulador.directorioP0[numeroBloque, 2] == 1)//Esta en cache del N1
                                        {
                                            if (Monitor.TryEnter(Simulador.cacheDatosN1))//Bloqueo cache del nucleo 1
                                            {
                                                Simulador.cacheDatosN1[posCache, 1] = 0; //Pongo invalido en cache N1
                                                Monitor.Exit(Simulador.cacheDatosN1);//Libero cache N1
                                            }
                                            else//No se puede bloquear
                                            {
                                                terminado = false;
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Monitor.Exit(Simulador.cacheDatosN0);
                                                Monitor.Exit(Simulador.directorioP0);
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                        else if (Simulador.directorioP0[numeroBloque, 3] == 1)//Esta en cache del N2
                                        {
                                            if (Monitor.TryEnter(Simulador.cacheDatosN2))//Bloqueo cache del nucleo 2
                                            {
                                                Simulador.cacheDatosN2[posCache, 1] = 0; //Pongo invalido en cache N2
                                                Monitor.Exit(Simulador.cacheDatosN2);//Libero cache N1
                                            }
                                            else//No se puede bloquear
                                            {
                                                terminado = false;
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Monitor.Exit(Simulador.cacheDatosN0);
                                                Monitor.Exit(Simulador.directorioP0);
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                    }
                                        if (Monitor.TryEnter(Simulador.memCompartidaP0))//Bloque la memoria que lo tiene
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                            guardarBloqueEnCache(posCache, numeroBloque, numPalabra, 2);//Guardo el bloque en cache
                                            Simulador.reloj += 16;
                                            cicloActual += 16;
                                            for (int i = 0; i < 16; i++)
                                                Simulador.barrera.SignalAndWait();
                                            Monitor.Exit(Simulador.memCompartidaP0);
                                        }
                                        else//No se puede bloquear
                                        {
                                            terminado = false;
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                            Monitor.Exit(Simulador.cacheDatosN0);
                                            Monitor.Exit(Simulador.directorioP0);
                                        }
                                    
                                    Simulador.directorioP0[numeroBloque, 0] = 2; //Coloco directorio en M
                                    Simulador.directorioP0[numeroBloque, 1] = 1; //Indico que esta en cache del N0
                                    Simulador.directorioP0[numeroBloque, 2] = 0; //Indico que esta en cache del N1
                                    Simulador.directorioP0[numeroBloque, 3] = 0;//Indico que esta en cache del N2
                                    terminado = true;
                                    Monitor.Exit(Simulador.directorioP0);
                                }
                                else//No se puede bloquear
                                {
                                    terminado = false;
                                    Simulador.reloj++;
                                    cicloActual++;
                                    Simulador.barrera.SignalAndWait();
                                    Monitor.Exit(Simulador.cacheDatosN0);
                                }
                            }
                            else//Esta en P1
                            {
                                if (Monitor.TryEnter(Simulador.directorioP1))//Bloque el directorio
                                {
                                    Simulador.reloj+=5;
                                    cicloActual+=5;
                                    for(int i = 0; i < 5; i++)
                                        Simulador.barrera.SignalAndWait();
                                    if (Simulador.directorioP1[numeroBloque-16, 0] == 2)//Si el bloque que necesito esta M
                                    {
                                        if (Simulador.directorioP1[numeroBloque-16, 2] == 1)//Esta modificado en N1
                                        {
                                            if (Monitor.TryEnter(Simulador.cacheDatosN1))//Bloque la cache que lo tiene
                                            {
                                                if (Monitor.TryEnter(Simulador.memCompartidaP1))//Bloque memoria de P1
                                                {
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                    guardarAMemoria(1, 1, numeroBloque, posCache);
                                                    Simulador.reloj += 40;
                                                    cicloActual += 40;
                                                    for (int i = 0; i < 40; i++)
                                                        Simulador.barrera.SignalAndWait();
                                                    Monitor.Exit(Simulador.memCompartidaP1);
                                                 }
                                                 else//No puedo bloquear
                                                 {
                                                    terminado = false;
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                    Monitor.Exit(Simulador.cacheDatosN0);
                                                    Monitor.Exit(Simulador.cacheDatosN1);
                                                    Monitor.Exit(Simulador.directorioP1);
                                                  }
                                                  Simulador.cacheDatosN1[posCache, 1] = 0;//Coloco invalido bloque en cache
                                                  Monitor.Exit(Simulador.cacheDatosN1);
                                            }
                                            else//No lo puedo bloquear
                                            {
                                                terminado = false;
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                                Monitor.Exit(Simulador.cacheDatosN0);
                                                Monitor.Exit(Simulador.directorioP1);
                                            }
                                        }
                                        else //Esta modificado en N2
                                        {
											if (Monitor.TryEnter(Simulador.cacheDatosN2))//Bloque la cache que lo tiene
											{
												if (Monitor.TryEnter(Simulador.memCompartidaP1))//Bloque memoria de P1
													{
														Simulador.reloj++;
														cicloActual++;
														Simulador.barrera.SignalAndWait();
														guardarAMemoria(2, 1, numeroBloque, posCache);
														Simulador.reloj += 16;
														cicloActual += 16;
														for (int i = 0; i < 16; i++)
															Simulador.barrera.SignalAndWait();
														Monitor.Exit(Simulador.memCompartidaP1);
													}
													else//No puedo bloquear
													{
														terminado = false;
														Simulador.reloj++;
														cicloActual++;
														Simulador.barrera.SignalAndWait();
														Monitor.Exit(Simulador.cacheDatosN0);
														Monitor.Exit(Simulador.cacheDatosN2);
														Monitor.Exit(Simulador.directorioP1);
													}

												Simulador.cacheDatosN2[posCache, 1] = 0;//Coloco invalido bloque en cache
												Monitor.Exit(Simulador.cacheDatosN2);
											}
                                            else//No lo puedo bloquear
                                            {
                                                terminado = false;
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                                Monitor.Exit(Simulador.cacheDatosN0);
                                                Monitor.Exit(Simulador.directorioP1);
                                            }
                                        }
                                    }
                                    else if (Simulador.directorioP1[numeroBloque-16, 0] == 1)//Si el bloque que necesito esta C
                                    {
                                        if (Simulador.directorioP1[numeroBloque-16, 2] == 1)//Esta en cache del N1
                                        {
                                            if (Monitor.TryEnter(Simulador.cacheDatosN1))//Bloqueo cache del nucleo 1
                                            {
                                                Simulador.cacheDatosN1[posCache, 1] = 0; //Pongo invalido en cache N1
                                                Monitor.Exit(Simulador.cacheDatosN1);//Libero cache N1
                                            }
                                            else//No se puede bloquear
                                            {
                                                terminado = false;
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Monitor.Exit(Simulador.cacheDatosN0);
                                                Monitor.Exit(Simulador.directorioP1);
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                        else if (Simulador.directorioP1[numeroBloque-16, 3] == 1)//Esta en cache del N2
                                        {
                                            if (Monitor.TryEnter(Simulador.cacheDatosN2))//Bloqueo cache del nucleo 2
                                            {
                                                Simulador.cacheDatosN2[posCache, 1] = 0; //Pongo invalido en cache N2
                                                Monitor.Exit(Simulador.cacheDatosN2);//Libero cache N1
                                            }
                                            else//No se puede bloquear
                                            {
                                                terminado = false;
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Monitor.Exit(Simulador.cacheDatosN0);
                                                Monitor.Exit(Simulador.directorioP1);
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                    }
									if (Monitor.TryEnter(Simulador.memCompartidaP1))//Bloque la memoria que lo tiene
										{
											Simulador.reloj++;
											cicloActual++;
											Simulador.barrera.SignalAndWait();
											guardarBloqueEnCache(posCache, numeroBloque, numPalabra, 2);//Guardo el bloque en cache
											Simulador.reloj += 40;
											cicloActual += 40;
											for (int i = 0; i < 40; i++)
												Simulador.barrera.SignalAndWait();
											Monitor.Exit(Simulador.memCompartidaP1);
										}
										else//No se puede bloquear
										{
											terminado = false;
											Simulador.reloj++;
											cicloActual++;
											Simulador.barrera.SignalAndWait();
											Monitor.Exit(Simulador.cacheDatosN0);
											Monitor.Exit(Simulador.directorioP1);
										}
									Simulador.directorioP1[numeroBloque-16, 0] = 2; //Coloco directorio en M
									Simulador.directorioP1[numeroBloque-16, 1] = 1; //Indico que esta en cache del N0
									Simulador.directorioP1[numeroBloque-16, 2] = 0; //Indico que esta en cache del N1
									Simulador.directorioP1[numeroBloque-16, 3] = 0;//Indico que esta en cache del N2
									terminado = true;
									Monitor.Exit(Simulador.directorioP1);
                                }
                                else//No se puede bloquear
                                {
                                    terminado = false;
                                    Simulador.reloj++;
                                    cicloActual++;
                                    Simulador.barrera.SignalAndWait();
                                    Monitor.Exit(Simulador.cacheDatosN0);
                                }
                            }
                            
                        }
                        if (terminado)
                        {
                            Simulador.cacheDatosN0[posCache, numPalabra + 2] = registros[IR[2]];                    
                        }
                        Monitor.Exit(Simulador.cacheDatosN0);
                    }
                    else//No se puede bloquear la cache del nucleo 0
                    {
                        terminado = false;
                        Simulador.reloj++;
                        cicloActual++;
                        Simulador.barrera.SignalAndWait();
                    }
                }
            }
            else if(numNucleo == 1)//SW para nucleo 1
            {

            }
            else//SW para nucleo 2
            {

            }



            //Console.WriteLine("Instrucción SW ejecutada en el contexto ");
        }
        /// <summary>
        /// Ejecuta la instrucción FIN
        /// </summary>
        private void instruccionFIN()
        {
            if (numProc == 0)
            {
                lock (Simulador.contextoP0) {
                    Simulador.contextoP0[contextoActual][33] = cicloActual;
                    Simulador.contextoP0[contextoActual][37] = Simulador.reloj;
                }
                
            }
            else {
                lock (Simulador.contextoP1)
                {
                    Simulador.contextoP1[contextoActual][33] = cicloActual;
                    Simulador.contextoP1[contextoActual][37] = Simulador.reloj;
                }
            }

            logExecution += "Instrucción FIN ejecutada en el contexto " + contextoActual + "\n";
            Console.WriteLine("Instrucción FIN ejecutada en contexto " + contextoActual);
        }

        /// <summary>
        /// Imprime los registros actuales del núcleo
        /// </summary>
        public void printRegistros() {
            Console.Write("\n REGISTROS PROCESADOR " + numProc);
            foreach (int registro in registros) {
                Console.Write(registro + " ");
            }
        }

        public void printLogExecution()
        {
            Console.WriteLine(logExecution);
        }
        /// <summary>
        /// Retorna el contexto actual del núcleo
        /// </summary>
        public int[] getContexto() {
            int[] contextoActual = new int[36];
            return contextoActual;
        }

        public int obtenerNumBloque(){
            int numBloque = (registros[IR[1]] + IR[3]) / 16;
            return numBloque;
        }

        public int obtenerNumPalabra()
        {
            int numPalabra = ((registros[IR[1]] + IR[3]) % 16) / 4;
            return numPalabra;
        }

        public int obtenerPosCache(int numBloque)
        {
            int posCache = numBloque % 4;
            return posCache;
        }

        public int obtenerNumBloqueInstruccion(int numProcesador)
        {
            int numBloque;
            if (numProcesador == 0) 
                numBloque = (PC / 16);
            else
                numBloque = (PC / 16);
            return numBloque;
        }

        public int obtenerNumInstruccionBloque()
        {
            int numPalabra = (PC % 16) / 4;
            return numPalabra;
        }

        public bool instruccionEnCache(int numBloque, int posCache,int numNucleo)
        {
            bool esta = false;
            switch (numNucleo)
            {
                case 0:
                    if (Simulador.cacheInstruccionesN0[posCache,0] == numBloque)
                        esta = true;
                    else
                        esta = false;
                    break;
                case 1:
                    if (Simulador.cacheInstruccionesN1[posCache,0] == numBloque)
                        esta = true;
                    else
                        esta = false;
                    break;
                case 2:
                    if (Simulador.cacheInstruccionesN2[posCache,0] == numBloque)
                        esta = true;
                    else
                        esta = false;
                    break;

            }
            return esta;
        }

        public void insertarBloqueCacheInstrucciones(int numBloque,int posCache, int numNucleo)
        {
            switch (numNucleo)
            {
                case 0:
                    Simulador.cacheInstruccionesN0[posCache, 0] = numBloque;
                    for(int i = 1; i < 17; i++)
                    {
                        //Console.WriteLine(numNucleo + " " + Simulador.memInstruccionesP0[((numBloque - 16) * 16 + (i - 1))] + "\n\n");
                        Simulador.cacheInstruccionesN0[posCache, i] = Simulador.memInstruccionesP0[((numBloque-16)*16 + (i-1))];
                    }
                    break;
                case 1:
                    Simulador.cacheInstruccionesN1[posCache, 0] = numBloque;
                    for (int i = 1; i < 17; i++)
                    {
                        //Console.WriteLine(numNucleo + " " + Simulador.memInstruccionesP0[((numBloque - 16) * 16 + (i - 1))] + "\n\n");
                        Simulador.cacheInstruccionesN1[posCache, i] = Simulador.memInstruccionesP0[((numBloque - 16) * 16 + (i - 1))];
                    }
                    break;
                case 2:
                    Simulador.cacheInstruccionesN2[posCache, 0] = numBloque;
                    for (int i = 1; i < 17; i++)
                    {
                        //Console.WriteLine(numNucleo + " " + Simulador.memInstruccionesP0[((numBloque - 16) * 16 + (i - 1))] + "\n\n");
                        Simulador.cacheInstruccionesN2[posCache, i] = Simulador.memInstruccionesP1[((numBloque - 8) * 16 + (i - 1))];
                    }
                    break;
            }
        }

        public bool bloqueEnCache(int posCache, int numBloque)
        {
            bool valido = false;
            switch (numNucleo)
            {
                case 0:
                    if (Simulador.cacheDatosN0[posCache, 0] == numBloque)
                        if (Simulador.cacheDatosN0[posCache, 1] == 1 || Simulador.cacheDatosN0[posCache, 1] == 2)
                            valido = true;
                    break;
                case 1:
                    if (Simulador.cacheDatosN1[posCache, 0] == numBloque)
                        if (Simulador.cacheDatosN1[posCache, 1] == 1 || Simulador.cacheDatosN1[posCache, 1] == 2)
                            valido = true;
                    break;
                case 2:
                    if (Simulador.cacheDatosN2[posCache, 0] == numBloque)
                        if (Simulador.cacheDatosN2[posCache, 1] == 1 || Simulador.cacheDatosN2[posCache, 1] == 2)
                            valido = true;
                    break;
            }
            return valido;
        }

        public int numDirectorio(int numBloque)
        {
            int directorio = -1;
            if (numBloque < 16)
            {
                directorio = 0;
            }
            else
                directorio = 1;
            return directorio;
        }

        public void guardarBloqueEnCache(int posCache, int numBloque, int numPalabra, int estado)
        {
            switch (numNucleo)
            {
                case 0:
                    Simulador.cacheDatosN0[posCache, 0] = numBloque;
                    Simulador.cacheDatosN0[posCache, 1] = estado;
                    for (int i = 2; i < 6; i++)
                    {
                        if(numBloque < 16)
                            Simulador.cacheDatosN0[posCache, i] = Simulador.memCompartidaP0[numBloque * 4 + i-2];
                        else
                            Simulador.cacheDatosN0[posCache, i] = Simulador.memCompartidaP1[(numBloque-16) * 4 + i - 2];
                    }
                    break;
                case 1:
                    Simulador.cacheDatosN1[posCache, 0] = numBloque;
                    Simulador.cacheDatosN1[posCache, 1] = estado;
                    for (int i = 2; i < 6; i++)
                    {
                        if (numBloque < 16)
                            Simulador.cacheDatosN1[posCache, i] = Simulador.memCompartidaP0[numBloque * 4 + i - 2];
                        else
                            Simulador.cacheDatosN1[posCache, i] = Simulador.memCompartidaP1[(numBloque - 16) * 4 + i - 2];
                    }
                    break;
                case 2:
                    Simulador.cacheDatosN2[posCache, 0] = numBloque;
                    Simulador.cacheDatosN2[posCache, 1] = estado;
                    for (int i = 2; i < 6; i++)
                    {
                        if (numBloque < 16)
                            Simulador.cacheDatosN2[posCache, i] = Simulador.memCompartidaP0[numBloque * 4 + i - 2];
                        else
                            Simulador.cacheDatosN2[posCache, i] = Simulador.memCompartidaP1[(numBloque - 16) * 4 + i - 2];
                    }
                    break;
            }
        }

        public int obtenerCacheEnModificado(int numDirectorio, int numBloque)
        {
            int cache = -1;
            if (numDirectorio == 0)
            {
                for (int i = 1; i < 4; i++)
                {
                    if (Simulador.directorioP0[numBloque, i] == 1)
                    {
                        cache = i - 2;
                        i = Simulador.directorioP0.Length;
                    }
                }
            }
            else
            {
                for (int i = 1; i < 4; i++)
                {
                    if (Simulador.directorioP1[numBloque-16, i] == 1)
                    {
                        cache = i - 2;
                        i = Simulador.directorioP1.Length;
                    }
                }
            }
            return cache;
        }

        public void guardarAMemoria(int numCache, int numMemProcesador, int numBloque, int posCache)
        {
            if(numBloque < 16)
            {
                if (numCache == 0)
                {
                    for (int i = 2; i < 6; i++)
                    {
                        Simulador.memCompartidaP0[numBloque * 4 + (i - 2)] = Simulador.cacheDatosN0[posCache, i];
                    }
                }
                else if (numCache == 1)
                {
                    for (int i = 2; i < 6; i++)
                    {
                        Simulador.memCompartidaP0[numBloque * 4 + (i - 2)] = Simulador.cacheDatosN1[posCache, i];
                    }
                }
                else
                {
                    for (int i = 2; i < 6; i++)
                    {
                        Simulador.memCompartidaP0[numBloque * 4 + (i - 2)] = Simulador.cacheDatosN2[posCache, i];
                    }
                }
            }
            else
            {
                if (numCache == 0)
                {
                    for (int i = 2; i < 6; i++)
                    {
                        Simulador.memCompartidaP1[(numBloque - 16) * 4 + (i - 2)] = Simulador.cacheDatosN0[posCache, i];
                    }
                }
                else if (numCache == 1)
                {
                    for (int i = 2; i < 6; i++)
                    {
                        Simulador.memCompartidaP1[(numBloque - 16) * 4 + (i - 2)] = Simulador.cacheDatosN1[posCache, i];
                    }
                }
                else
                {
                    for (int i = 2; i < 6; i++)
                    {
                        Simulador.memCompartidaP1[(numBloque - 16) * 4 + (i - 2)] = Simulador.cacheDatosN2[posCache, i];
                    }
                }
            }
        }
    }
}
