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
        private Mutex mutex;
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
                bool flag = true;
                while (flag) {
                    if (cicloActual < Simulador.quantum)
                    {
                        //Solicitamos bus de memoria
                        lock (Simulador.memInstruccionesP0)
                        {
                            //Hacemos fetch de la instrucción
                            IR[0] = Simulador.memInstruccionesP0[PC];
                            IR[1] = Simulador.memInstruccionesP0[PC + 1];
                            IR[2] = Simulador.memInstruccionesP0[PC + 2];
                            IR[3] = Simulador.memInstruccionesP0[PC + 3];
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
                                break;
                            //Caso erroneo
                            default:
                                throw new Exception("ERROR: Instruccion " + IR[0] + " no identificada.");
                        }
                        Simulador.barrera.SignalAndWait();
                        cicloActual++;
                        Console.WriteLine("Nucleo " + numNucleo + " ha pasado al ciclo " + cicloActual);
                    }
                    else {
                        lock (Simulador.contextoP0) {
                            Simulador.contextoP0[contextoActual][0] = PC;                       //Salvamos el PC en el contexto
                            for (int i = 0; i < registros.Length; i++) {                        //Salvamos los registros en el contexto
                                Simulador.contextoP0[contextoActual][i + 1] = registros[i];
                            }
                            Simulador.contextoP0[contextoActual][34] = -1;                      //Marcamos el contexto como en desuso

                            //Buscamos un contexto en desuso
                            contextoActual = (contextoActual + 1) % 7;
                            while (Simulador.contextoP0[contextoActual][34] != -1 || Simulador.contextoP0[contextoActual][33] != -1) {
                                contextoActual = (contextoActual + 1) % 7;
                            }
                            //Cargamos el nuevo contexto
                            PC = Simulador.contextoP0[contextoActual][0];
                            for (int i = 1; i < 33; i++) {
                                registros[i - 1] = Simulador.contextoP0[contextoActual][i];
                            }
                            Simulador.contextoP0[contextoActual][34] = 0;                       //Marcamos el nuevo contexto como "en uso"
                            Console.WriteLine("PROCESADOR " + numProc + " NUCLEO " + numNucleo + " hizo cambio de contexto al " + contextoActual);
                        }
                    }

                    //Revisamos si ya todos los hilillos del contexto terminaron para terminar el nucleo.
                    lock (Simulador.contextoP0)
                    {
                        bool continuar = false;
                        for (int i = 0; i < 7; i++)
                        {
                            if (Simulador.contextoP0[i][33] == -1)
                            {
                                continuar = true;
                            }
                        }
                        flag = continuar;
                    }
                }
            } else {
                bool flag = true;
                while (flag)
                {
                    if (cicloActual < Simulador.quantum)
                    {
                        //Solicitamos bus de memoria
                        lock (Simulador.memInstruccionesP0)
                        {
                            //Hacemos fetch de la instrucción
                            IR[0] = Simulador.memInstruccionesP1[PC];
                            IR[1] = Simulador.memInstruccionesP1[PC + 1];
                            IR[2] = Simulador.memInstruccionesP1[PC + 2];
                            IR[3] = Simulador.memInstruccionesP1[PC + 3];
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
                                break;
                            //Caso erroneo
                            default:
                                throw new Exception("ERROR: Instruccion " + IR[0] + " no identificada.");
                        }
                        Simulador.barrera.SignalAndWait();
                        cicloActual++;
                        Console.WriteLine("Nucleo " + numNucleo + " ha pasado al ciclo " + cicloActual);

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

                            //Buscamos un contexto en desuso y no terminado
                            contextoActual = (contextoActual + 1) % 7;
                            while (Simulador.contextoP1[contextoActual][34] != -1 || Simulador.contextoP1[contextoActual][33] != -1)
                            {
                                contextoActual = (contextoActual + 1) % 7;
                            }
                            //Cargamos el nuevo contexto
                            PC = Simulador.contextoP1[contextoActual][0];
                            for (int i = 1; i < 33; i++)
                            {
                                registros[i - 1] = Simulador.contextoP1[contextoActual][i];
                            }
                            Simulador.contextoP1[contextoActual][34] = 0;                       //Marcamos el nuevo contexto como "en uso"
                        }
                        Console.WriteLine("PROCESADOR " + numProc + " NUCLEO " + numNucleo + " hizo cambio de contexto al " + contextoActual);
                    }

                    lock (Simulador.contextoP1) {
                        bool continuar = false;
                        for (int i = 0; i < 7; i++)
                        {
                            if (Simulador.contextoP1[i][33] == -1)
                            {
                                continuar = true;
                            }
                        }
                        flag = continuar;
                    }
                }
            }
            printLogExecution();
            

        }

        /// <summary>
        /// Ejecuta la instrucción DADD
        /// </summary>
        private void instruccionDADD() {
            logExecution += "Instrucción DADD ejecutada.\n";
            //Console.WriteLine("Instrucción DADD ejecutada.");

        }

        /// <summary>
        /// Ejecuta la instrucción DADDI
        /// </summary>
        private void instruccionDADDI()
        {
            logExecution += "Instrucción DADDI ejecutada.\n";
            //Console.WriteLine("Instrucción DADDI ejecutada.");
        }

        /// <summary>
        /// Ejecuta la instrucción DSUB
        /// </summary>
        private void instruccionDSUB()
        {
            logExecution += "Instrucción DSUB ejecutada.\n";
            //Console.WriteLine("Instrucción DSUB ejecutada.");
        }

        /// <summary>
        /// Ejecuta la instrucción DMUL
        /// </summary>
        private void instruccionDMUL()
        {
            logExecution += "Instrucción DMUL ejecutada.\n";
            //Console.WriteLine("Instrucción DMUL ejecutada.");
        }

        /// <summary>
        /// Ejecuta la instrucción DDIV
        /// </summary>
        private void instruccionDDIV()
        {
            logExecution += "Instrucción DDIV ejecutada.\n";
            //Console.WriteLine("Instrucción DDIV ejecutada.");
        }

        /// <summary>
        /// Ejecuta la instrucción BEQZ
        /// </summary>
        private void instruccionBEQZ()
        {
            logExecution += "Instrucción BEQZ ejecutada.\n";
            //Console.WriteLine("Instrucción BEQZ ejecutada.");
        }

        /// <summary>
        /// Ejecuta la instrucción BNEZ
        /// </summary>
        private void instruccionBNEZ()
        {
            logExecution += "Instrucción BNEZ ejecutada.\n";
            //Console.WriteLine("Instrucción BNEZ ejecutada.");
        }

        /// <summary>
        /// Ejecuta la instrucción JAL
        /// </summary>
        private void instruccionJAL()
        {
            logExecution += "Instrucción JAL ejecutada.\n";
            //Console.WriteLine("Instrucción JAL ejecutada.");
        }


        /// <summary>
        /// Ejecuta la instrucción JR
        /// </summary>
        private void instruccionJR()
        {
            logExecution += "Instrucción JR ejecutada.\n";
            //Console.WriteLine("Instrucción JR ejecutada.");
        }

        /// <summary>
        /// Ejecuta la instrucción LW
        /// </summary>
        private void instruccionLW()
        {
            logExecution += "Instrucción LW ejecutada.\n";
            //Console.WriteLine("Instrucción LW ejecutada.");
        }

        /// <summary>
        /// Ejecuta la instrucción SW
        /// </summary>
        private void instruccionSW()
        {
            logExecution += "Instrucción SW ejecutada.\n";
            //Console.WriteLine("Instrucción SW ejecutada.");
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
                }
                
            }
            else {
                lock (Simulador.contextoP1)
                {
                    Simulador.contextoP1[contextoActual][33] = cicloActual;
                }
            }

            logExecution += "Instrucción FIN ejecutada.\n";
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

    }
}
