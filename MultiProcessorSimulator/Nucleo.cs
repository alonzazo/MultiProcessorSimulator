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
                lock (Simulador.contextoP0)
                {
                    Simulador.contextoP0[contextoActual][36] = cicloActual;
                    logExecution += "Reloj de inicio guardado en el contexto" + contextoActual + "del P0" + "\n";
                }
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
                        if(numNucleo == 0)
                        {
                            lock (Simulador.cacheInstruccionesN0)
                            {
                                numBloque = obtenerNumBloqueInstruccion(numProc);
                                posCache = obtenerPosCache(numBloque);
                                numInstruccion = obtenerNumInstruccionBloque();
                                if (!instruccionEnCache(numBloque, posCache, 0))
                                {

                                    lock (Simulador.memInstruccionesP0)
                                    {
                                        insertarBloqueCacheInstrucciones(numBloque, posCache, numNucleo);
                                    }
                                }
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
                                if (!instruccionEnCache(numBloque, posCache, 0))
                                {

                                    lock (Simulador.memInstruccionesP0)
                                    {
                                        insertarBloqueCacheInstrucciones(numBloque, posCache, numNucleo);
                                    }
                                }
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
                                        if (Simulador.contextoP0[contextoActual][34] == -1 ||           //Está en desuso?
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
                            Simulador.contextoP0[contextoActual][0] = PC;                       //Salvamos el PC en el contexto
                            for (int i = 0; i < registros.Length; i++) {                        //Salvamos los registros en el contexto
                                Simulador.contextoP0[contextoActual][i + 1] = registros[i];
                            }
                            Simulador.contextoP0[contextoActual][34] = -1;                      //Marcamos el contexto como en desuso

                            //Buscamos un contexto en desuso
                            for (int i = 0; i < Simulador.contextoP0.Length; i++) {
                                contextoActual = (contextoActual + 1) % 7;
                                if (Simulador.contextoP0[contextoActual][34] == -1 ||           //Está en desuso?
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
                lock (Simulador.contextoP1)
                {
                    Simulador.contextoP1[contextoActual][36] = cicloActual;
                }
                bool flag = true;
                int numBloque;
                int posCache;
                int numInstruccion;
                while (flag)
                {
                    if ((cicloActual == 0 || cicloActual % Simulador.quantum != 0) && Simulador.contextoP1[contextoActual][33] == -1)
                    {
                        lock (Simulador.cacheInstruccionesN2)
                        {
                            numBloque = obtenerNumBloqueInstruccion(numProc);
                            posCache = obtenerPosCache(numBloque);
                            numInstruccion = obtenerNumInstruccionBloque();
                            if (!instruccionEnCache(numBloque, posCache, 0))
                            {

                                lock (Simulador.memInstruccionesP1)
                                {
                                    insertarBloqueCacheInstrucciones(numBloque, posCache, numNucleo);
                                }
                            }
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
                            //cicloActual++;
                            //Console.WriteLine("Nucleo " + numNucleo + " ha pasado al ciclo " + cicloActual);
                        }
                    }

                    Simulador.barrera.SignalAndWait();
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
            logExecution += "Instrucción DSUB ejecutada en el contexto" + contextoActual + "\n";
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
            logExecution += "Instrucción BEQZ ejecutada en el contexto" + contextoActual + "\n";
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
            logExecution += "Instrucción BNEZ ejecutada en el contexto" + contextoActual + "\n";
            //Console.WriteLine("Instrucción BNEZ ejecutada en el contexto ");
        }

        /// <summary>
        /// Ejecuta la instrucción JAL
        /// </summary>
        private void instruccionJAL()
        {
            logExecution += "Instrucción JAL ejecutada en el contexto" + contextoActual + "\n";
            //Console.WriteLine("Instrucción JAL ejecutada en el contexto ");
        }


        /// <summary>
        /// Ejecuta la instrucción JR
        /// </summary>
        private void instruccionJR()
        {
            PC = IR[1];
            logExecution += "Instrucción JR ejecutada en el contexto" + contextoActual + "\n";
            //Console.WriteLine("Instrucción JR ejecutada en el contexto ");
        }

        /// <summary>
        /// Ejecuta la instrucción LW
        /// </summary>
        private void instruccionLW()
        {
            logExecution += "Instrucción LW ejecutada en el contexto" + contextoActual + "\n";
            //Console.WriteLine("Instrucción LW ejecutada en el contexto ");
        }

        /// <summary>
        /// Ejecuta la instrucción SW
        /// </summary>
        private void instruccionSW()
        {
            logExecution += "Instrucción SW ejecutada en el contexto" + contextoActual + "\n";
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
                    Simulador.contextoP0[contextoActual][37] = cicloActual;
                }
                
            }
            else {
                lock (Simulador.contextoP1)
                {
                    Simulador.contextoP1[contextoActual][33] = cicloActual;
                    Simulador.contextoP1[contextoActual][37] = cicloActual;
                }
            }

            logExecution += "Instrucción FIN ejecutada en el contexto" + contextoActual + "\n";
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
            int numBloque = (IR[1] + IR[3]) / 16;
            return numBloque;
        }

        public int obtenerNumPalabra()
        {
            int numPalabra = ((IR[1] + IR[3]) % 16) / 4;
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
                numBloque = (PC / 16)+16;
            else
                numBloque = (PC / 16) + 8;
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
    }
}
