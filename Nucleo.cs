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
			bool volverAEmpezar = false;
            if(numNucleo == 0)//Es del nucleo 0
            {
                while (!terminado)
                {
                    volverAEmpezar = false;
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
                                        if (Monitor.TryEnter(Simulador.directorioP0))//Bloqueo la memoria 
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;//Aumento el ciclo por bloquear memoria
                                            Simulador.barrera.SignalAndWait();
                                            if (Monitor.TryEnter(Simulador.memCompartidaP0))//Bloqueo el directorio de la victima
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
                                                Simulador.cacheDatosN0[posCache, 1] = 0;//Invalido la posicion en la cache

                                            }
                                            else//Si no puedo blouear el directorio de la victima
                                            {
                                                //Monitor.Exit(Simulador.memCompartidaP0);//Libero la memoria                                                Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                                terminado = false;
                                                volverAEmpezar = true;
                                            }
											Monitor.Exit(Simulador.directorioP0);//Libero directorio de la victima
                                        }
                                        else//No puedo bloquear la memoria
                                        {
                                            terminado = false;
                                            //Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                            volverAEmpezar = true;
                                        }
                                    }
                                    else //Si el bloque esta en el procesador 1
                                    {
                                        if (Monitor.TryEnter(Simulador.directorioP1))//Bloqueo la memoria remota 
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;//Aumento el ciclo por bloquear memoria
                                            Simulador.barrera.SignalAndWait();
                                            if (Monitor.TryEnter(Simulador.memCompartidaP1))//Bloqueo el directorio remoto de la victima
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
                                                //Monitor.Exit(Simulador.memCompartidaP1);//Libero la memoria 
                                                Simulador.directorioP1[Simulador.cacheDatosN0[posCache, 0]-16, 0] = 0;//Pongo U a directorio
                                                Simulador.directorioP1[Simulador.cacheDatosN0[posCache, 0]-16, 1] = 0;//Pongo 0 el bit del N0
                                                Monitor.Exit(Simulador.memCompartidaP1);//Libero directorio de la victima
                                                Simulador.cacheDatosN0[posCache, 1] = 0;//Invalido la posicion en la cache

                                            }
                                            else//Si no puedo blouear el directorio de la victima
                                            {
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                                terminado = false;
                                                //Monitor.Exit(Simulador.memCompartidaP1);//Libero la memoria
                                                //Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                volverAEmpezar = true;
                                            }
											Monitor.Exit(Simulador.directorioP1);
                                        }
                                        else//No puedo bloquear la memoria
                                        {
                                            terminado = false;
                                            //Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                            volverAEmpezar = true;
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
                                            Monitor.Exit(Simulador.directorioP0);		//Libero el directorio
                                            Simulador.cacheDatosN0[posCache, 1] = 0;	//Invalido la posicion en la cache

                                        }
                                        else//No puedo bloquear el directorio
                                        {
                                            terminado = false;
                                            //Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                            volverAEmpezar = true;
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
                                            //Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                            volverAEmpezar = true;
                                        }
                                    }
                                }

                            }
                            if (!volverAEmpezar) {
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
                                                
                                                terminado = true;//Solo falta obtener de cache

                                            }
                                            else//No se puede bloquear la memoria
                                            {
                                                terminado = false;
                                                //Falta regresar a estado anterior
                                                //Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                //Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                                volverAEmpezar = true;
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
                                                        
                                                        Simulador.directorioP0[numeroBloque, 1] = 1;//Indico que esta en cache
                                                        //Monitor.Exit(Simulador.directorioP0);//Libero el directorio

                                                    }
                                                    else//No se puede bloquear la memoria
                                                    {
                                                        terminado = false;
                                                        //Falta regresar a estado anterior
                                                        //Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                        //Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache remota
                                                        //Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                                        Simulador.reloj++;
                                                        cicloActual++;
                                                        Simulador.barrera.SignalAndWait();
                                                        volverAEmpezar = true;
                                                    }
													Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache
                                                }
                                                else//No se puede bloquear la cache
                                                {
                                                    terminado = false;
                                                    //Falta regresar a estado anterior
                                                    //Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                    //Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                    volverAEmpezar = true;
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
                                                        
                                                        Simulador.directorioP0[numeroBloque, 1] = 1;//Indico que esta en cache
                                                        //Monitor.Exit(Simulador.directorioP0);//Libero el directorio                    
                                                    }
                                                    else//No se puede bloquear la memoria
                                                    {
                                                        terminado = false;
                                                        //Falta regresar a estado anterior
                                                        //Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                        //Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache remota
                                                        //Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                                        Simulador.reloj++;
                                                        cicloActual++;
                                                        Simulador.barrera.SignalAndWait();
                                                        volverAEmpezar = true;
                                                    }
													Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache
                                                }
                                                else//No se puede bloquear la cache
                                                {
                                                    terminado = false;
                                                    //Falta regresar a estado anterior
                                                    //Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                    //Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                    volverAEmpezar = true;
                                                }
                                            }
                                        }
										Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                    }
                                    else//No se puede bloquear el directorio
                                    {
                                        terminado = false;
                                        //Falta regresar a estado anterior
                                        //Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                        Simulador.reloj++;
                                        cicloActual++;
                                        Simulador.barrera.SignalAndWait();
                                        volverAEmpezar = true;
                                    }
                                }
                                else	//Si el directorio que hay que bloquear es del P1
                                {
                                    if (Monitor.TryEnter(Simulador.directorioP1))//Bloqueo el directorio del P1
                                    {
                                        Simulador.reloj += 5;
                                        cicloActual += 5;//Aumento un ciclo por acceso a directorio remoto
                                        for (int i = 0; i < 5; i++)
                                            Simulador.barrera.SignalAndWait();
                                        if (Simulador.directorioP1[numeroBloque - 16, 0] == 0 || Simulador.directorioP1[numeroBloque - 16, 0] == 1)//Si en el directorio esta U o C
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
                                                if (Simulador.directorioP1[numeroBloque - 16, 0] == 0)
                                                    Simulador.directorioP1[numeroBloque - 16, 0] = 1;//Pongo directorio en C
                                                Simulador.directorioP1[numeroBloque - 16, 1] = 1;//Indico que esta en cache
                                                
                                                terminado = true;//Solo falta obtener de cache

                                            }
                                            else//No se puede bloquear la memoria
                                            {
                                                terminado = false;
                                                //Falta regresar a estado anterior
                                                //Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                //Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                                volverAEmpezar = true;
                                            }
                                        }
                                        else//Si en el directorio esta M
                                        {
                                            if (Simulador.directorioP1[numeroBloque - 16, 2] == 1)//Esta en cache del N1
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
                                                        Simulador.directorioP1[numeroBloque - 16, 0] = 1;//Pongo directorio en C
                                                        Simulador.cacheDatosN1[posCache, 1] = 1;//Pongo cache en C
                                                        
                                                        Simulador.directorioP1[numeroBloque - 16, 1] = 1;//Indico que esta en cache
                                                        //Monitor.Exit(Simulador.directorioP1);//Libero el directorio

                                                    }
                                                    else//No se puede bloquear la memoria
                                                    {
                                                        terminado = false;
                                                        //Falta regresar a estado anterior
                                                        //Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                        //Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache remota
                                                        //Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                                        Simulador.reloj++;
                                                        cicloActual++;
                                                        Simulador.barrera.SignalAndWait();
                                                        volverAEmpezar = true;
                                                    }
													Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache
                                                }
                                                else//No se puede bloquear la cache
                                                {
                                                    terminado = false;
                                                    //Falta regresar a estado anterior
                                                    //Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                    //Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                    volverAEmpezar = true;
                                                }
                                            }
                                            else//Esta en cache del N2
                                            {
                                                if (Monitor.TryEnter(Simulador.cacheDatosN2))//Bloqueo la cache donde esta
                                                {
                                                    if (Monitor.TryEnter(Simulador.memCompartidaP1))//Bloqueo la memoria del P0
                                                    {
                                                        Simulador.reloj++;
                                                        cicloActual++;									//Aumento ciclo por ingresar a memoria
                                                        Simulador.barrera.SignalAndWait();
                                                        guardarAMemoria(2, 1, numeroBloque, posCache);	//Guardo lo que tiene la cache en memoria
                                                        Simulador.reloj += 16;
                                                        cicloActual += 16;								//Aumento 16 por escribir desde cache remoto
                                                        for (int i = 0; i < 16; i++)
                                                            Simulador.barrera.SignalAndWait();
                                                        Monitor.Exit(Simulador.memCompartidaP1);		//Libero la memoria
                                                        for (int j = 0; j < 6; j++)						//Copio lo que hay en la cache del N2 en la cache del N0
                                                        {
                                                            Simulador.cacheDatosN0[posCache, j] = Simulador.cacheDatosN2[posCache, j];
                                                        }
                                                        Simulador.reloj++;
                                                        cicloActual++;
                                                        Simulador.barrera.SignalAndWait();
                                                        Simulador.directorioP1[numeroBloque - 16, 0] = 1;	//Pongo directorio en C
                                                        Simulador.cacheDatosN2[posCache, 1] = 1;			//Pongo cache en C
                                                        
                                                        Simulador.directorioP1[numeroBloque - 16, 1] = 1;	//Indico que esta en cache
                                                        //Monitor.Exit(Simulador.directorioP1);//Libero el directorio                    
                                                    }
                                                    else//No se puede bloquear la memoria
                                                    {
                                                        terminado = false;
                                                        //Falta regresar a estado anterior
                                                        //Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                        //Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache remota
                                                        //Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                                        Simulador.reloj++;
                                                        cicloActual++;
                                                        Simulador.barrera.SignalAndWait();
                                                        volverAEmpezar = true;
                                                    }
													Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache
                                                }
                                                else//No se puede bloquear la cache
                                                {
                                                    terminado = false;
                                                    //Falta regresar a estado anterior
                                                    //Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                                    //Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                    volverAEmpezar = true;
                                                }
                                            }
                                        }
										Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                    }
                                    else//No se puede bloquear el directorio
                                    {
                                        terminado = false;
                                        //Falta regresar a estado anterior
                                        //Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                                        Simulador.reloj++;
                                        cicloActual++;
                                        Simulador.barrera.SignalAndWait();
                                        volverAEmpezar = true;
                                    }
                                }
                            }

                        }
                        else
                            terminado = true;
                        if (terminado)//Si esta en cache y esta modificado o compartido
                        {
                            registros[IR[2]] = Simulador.cacheDatosN0[posCache, numPalabra + 2];//Copiamos lo que tiene la cache en el registro
                            //Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
                            terminado = true;
                        }
						Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache local
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
                    volverAEmpezar = false;
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
                                        if (Monitor.TryEnter(Simulador.directorioP0))//Bloqueo la memoria
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;//Aumento el ciclo por bloquear memoria
                                            Simulador.barrera.SignalAndWait();
                                            if (Monitor.TryEnter(Simulador.memCompartidaP0))//Bloqueo el directorio de la victima
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
                                                Simulador.directorioP0[Simulador.cacheDatosN1[posCache, 0], 2] = 0;//Pongo 0 el bit del N0
                                                Simulador.cacheDatosN1[posCache, 1] = 0;//Invalido la posicion en la cache

                                            }
                                            else//Si no puedo blouear el directorio de la victima
                                            {                                                
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                                terminado = false;
                                                volverAEmpezar = true;
                                            }
											Monitor.Exit(Simulador.directorioP0);//Libero directorio de la victima
                                        }
                                        else//No puedo bloquear la memoria
                                        {
                                            terminado = false;                                           
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                            volverAEmpezar = true;
                                        }
                                    }
                                    else //Si el bloque esta en el procesador 1
                                    {
                                        if (Monitor.TryEnter(Simulador.directorioP1))//Bloqueo la memoria remota
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;//Aumento el ciclo por bloquear memoria
                                            Simulador.barrera.SignalAndWait();
                                            if (Monitor.TryEnter(Simulador.memCompartidaP1))//Bloqueo el directorio remoto de la victima
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
                                                Simulador.directorioP1[Simulador.cacheDatosN1[posCache, 0]-16, 2] = 0;//Pongo 0 el bit del N0
                                                Simulador.cacheDatosN1[posCache, 1] = 0;//Invalido la posicion en la cache

                                            }
                                            else//Si no puedo blouear el directorio de la victima
                                            {
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                                terminado = false;
                                                volverAEmpezar = true;
                                            }
											Monitor.Exit(Simulador.directorioP1);//Libero directorio de la victima
                                        }
                                        else//No puedo bloquear la memoria
                                        {
                                            terminado = false;
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                            volverAEmpezar = true;
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
                                            Simulador.directorioP0[Simulador.cacheDatosN1[posCache, 0], 2] = 0;//Cambio bit para indicar que bloque no esta en esa cache
                                            if (Simulador.directorioP0[Simulador.cacheDatosN1[posCache, 0], 1] == 0 && Simulador.directorioP0[Simulador.cacheDatosN1[posCache, 0], 3] == 0)//Si ninguna otra cache tiene el bloque
                                            {
                                                Simulador.directorioP0[Simulador.cacheDatosN1[posCache, 0], 0] = 0;
                                            }
                                            
                                            Simulador.cacheDatosN1[posCache, 1] = 0;//Invalido la posicion en la cache
											Monitor.Exit(Simulador.directorioP0);//Libero el directorio

                                        }
                                        else//No puedo bloquear el directorio
                                        {
                                            terminado = false;
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                            volverAEmpezar = true;
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
                                            Simulador.directorioP1[Simulador.cacheDatosN1[posCache, 0]-16, 2] = 0;//Cambio bit para indicar que bloque no esta en esa cache
                                            if (Simulador.directorioP1[Simulador.cacheDatosN1[posCache, 0]-16, 1] == 0 && Simulador.directorioP1[Simulador.cacheDatosN1[posCache, 0]-16, 3] == 0)//Si ninguna otra cache tiene el bloque
                                            {
                                                Simulador.directorioP1[Simulador.cacheDatosN1[posCache, 0]-16, 0] = 0;
                                            }
                                            Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                            Simulador.cacheDatosN1[posCache, 1] = 0;//Invalido la posicion en la cache

                                        }
                                        else//No puedo bloquear el directorio
                                        {
                                            terminado = false;
                                            
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                            volverAEmpezar = true;
                                        }
                                    }
                                }

                            }
                            if (!volverAEmpezar)
                            {
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
                                                Simulador.directorioP0[numeroBloque, 2] = 1;//Indico que esta en cache
                                                
                                                terminado = true;//Solo falta obtener de cache

                                            }
                                            else//No se puede bloquear la memoria
                                            {
                                                terminado = false;
                                                //Falta regresar a estado anterior
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                        else//Si en el directorio esta M
                                        {
                                            if (Simulador.directorioP0[numeroBloque, 1] == 1)//Esta en cache del N0
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
                                                        
                                                        Simulador.directorioP0[numeroBloque, 2] = 1;//Indico que esta en cache

                                                    }
                                                    else//No se puede bloquear la memoria
                                                    {
                                                        terminado = false;
                                                        //Falta regresar a estado anterior
                                                        Simulador.reloj++;
                                                        cicloActual++;
                                                        Simulador.barrera.SignalAndWait();
                                                    }
													Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache
                                                }
                                                else//No se puede bloquear la cache
                                                {
                                                    terminado = false;
                                                    //Falta regresar a estado anterior
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
                                                        
                                                        Simulador.directorioP0[numeroBloque, 2] = 1;//Indico que esta en cache                    
                                                    }
                                                    else//No se puede bloquear la memoria
                                                    {
                                                        terminado = false;
                                                        //Falta regresar a estado anterior
                                                        Simulador.reloj++;
                                                        cicloActual++;
                                                        Simulador.barrera.SignalAndWait();
                                                    }
													Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache
                                                }
                                                else//No se puede bloquear la cache
                                                {
                                                    terminado = false;
                                                    //Falta regresar a estado anterior
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                }
                                            }
                                        }
										Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                    }
                                    else//No se puede bloquear el directorio
                                    {
                                        terminado = false;
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
                                        if (Simulador.directorioP1[numeroBloque - 16, 0] == 0 || Simulador.directorioP1[numeroBloque - 16, 0] == 1)//Si en el directorio esta U o C
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
                                                if (Simulador.directorioP1[numeroBloque - 16, 0] == 0)
                                                    Simulador.directorioP1[numeroBloque - 16, 0] = 1;//Pongo directorio en C
                                                Simulador.directorioP1[numeroBloque - 16, 2] = 1;//Indico que esta en cache
                                                
                                                terminado = true;//Solo falta obtener de cache

                                            }
                                            else//No se puede bloquear la memoria
                                            {
                                                terminado = false;
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                        else//Si en el directorio esta M
                                        {
                                            if (Simulador.directorioP1[numeroBloque - 16, 1] == 1)//Esta en cache del N0
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
                                                        Simulador.directorioP1[numeroBloque - 16, 0] = 1;//Pongo directorio en C
                                                        Simulador.cacheDatosN0[posCache, 1] = 1;//Pongo cache en C
                                                        
                                                        Simulador.directorioP1[numeroBloque - 16, 2] = 1;//Indico que esta en cache

                                                    }
                                                    else//No se puede bloquear la memoria
                                                    {
                                                        terminado = false;
                                                        Simulador.reloj++;
                                                        cicloActual++;
                                                        Simulador.barrera.SignalAndWait();
                                                    }
													Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache
                                                }
                                                else//No se puede bloquear la cache
                                                {
                                                    terminado = false;
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
                                                        Simulador.directorioP1[numeroBloque - 16, 0] = 1;//Pongo directorio en C
                                                        Simulador.cacheDatosN2[posCache, 1] = 1;//Pongo cache en C
                                                        Simulador.directorioP1[numeroBloque - 16, 2] = 1;//Indico que esta en cache                
                                                    }
                                                    else//No se puede bloquear la memoria
                                                    {
                                                        terminado = false;
                                                        Simulador.reloj++;
                                                        cicloActual++;
                                                        Simulador.barrera.SignalAndWait();
                                                    }
													Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache
                                                }
                                                else//No se puede bloquear la cache
                                                {
                                                    terminado = false;
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                }
                                            }
                                        }
										Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                    }
                                    else//No se puede bloquear el directorio
                                    {
                                        terminado = false;
                                        Simulador.reloj++;
                                        cicloActual++;
                                        Simulador.barrera.SignalAndWait();
                                    }
                                }
                            }

                        }
                        else
                            terminado = true;
                        if (terminado)//Si esta en cache y esta modificado o compartido
                        {
                            registros[IR[2]] = Simulador.cacheDatosN1[posCache, numPalabra + 2];//Copiamos lo que tiene la cache en el registro
                            terminado = true;
                        }
						Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache local
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
					volverAEmpezar = false;
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
                                        if (Monitor.TryEnter(Simulador.directorioP0))//Bloqueo la memoria
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;//Aumento el ciclo por bloquear memoria
                                            Simulador.barrera.SignalAndWait();
                                            if (Monitor.TryEnter(Simulador.memCompartidaP0))//Bloqueo el directorio de la victima
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
                                                Simulador.directorioP0[Simulador.cacheDatosN2[posCache, 0], 3] = 0;//Pongo 0 el bit del N0
                                                
                                                Simulador.cacheDatosN2[posCache, 1] = 0;//Invalido la posicion en la cache

                                            }
                                            else//Si no puedo blouear el directorio de la victima
                                            {
                                                
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                                terminado = false;
												volverAEmpezar = true;
                                            }
											Monitor.Exit(Simulador.directorioP0);//Libero directorio de la victima
                                        }
                                        else//No puedo bloquear la memoria
                                        {
                                            terminado = false;
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
											volverAEmpezar = true;
                                        }
                                    }
                                    else //Si el bloque esta en el procesador 1
                                    {
                                        if (Monitor.TryEnter(Simulador.directorioP1))//Bloqueo la memoria remota 
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;//Aumento el ciclo por bloquear memoria
                                            Simulador.barrera.SignalAndWait();
                                            if (Monitor.TryEnter(Simulador.memCompartidaP1))//Bloqueo el directorio remoto de la victima
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
                                                Simulador.directorioP1[Simulador.cacheDatosN2[posCache, 0]-16, 3] = 0;//Pongo 0 el bit del N0
                                                
                                                Simulador.cacheDatosN2[posCache, 1] = 0;//Invalido la posicion en la cache

                                            }
                                            else//Si no puedo blouear el directorio de la victima
                                            {
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                                terminado = false;
												volverAEmpezar = true;
                                            }
											Monitor.Exit(Simulador.directorioP1);//Libero directorio de la victima
                                        }
                                        else//No puedo bloquear la memoria
                                        {
                                            terminado = false;
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
											volverAEmpezar = true;
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
                                            Simulador.directorioP0[Simulador.cacheDatosN2[posCache, 0], 3] = 0;//Cambio bit para indicar que bloque no esta en esa cache
                                            if (Simulador.directorioP0[Simulador.cacheDatosN2[posCache, 0], 1] == 0 && Simulador.directorioP0[Simulador.cacheDatosN2[posCache, 0], 2] == 0)//Si ninguna otra cache tiene el bloque
                                            {
                                                Simulador.directorioP0[Simulador.cacheDatosN2[posCache, 0], 0] = 0;
                                            }
                                            Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                            Simulador.cacheDatosN2[posCache, 1] = 0;//Invalido la posicion en la cache

                                        }
                                        else//No puedo bloquear el directorio
                                        {
                                            terminado = false;
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
											volverAEmpezar = true;
                                        }
                                    }
                                    else //Si el bloque esta en el procesador 1
                                    {
                                        if (Monitor.TryEnter(Simulador.directorioP1))//Bloqueo el directorio remoto donde esta la victima
                                        {
                                            Simulador.reloj++;
                                            cicloActual++;//Aumento el ciclo por bloquear directorio local
                                            Simulador.barrera.SignalAndWait();
                                            Simulador.directorioP1[Simulador.cacheDatosN2[posCache, 0]-16, 3] = 0;//Cambio bit para indicar que bloque no esta en esa cache
                                            if (Simulador.directorioP1[Simulador.cacheDatosN2[posCache, 0]-16, 1] == 0 && Simulador.directorioP1[Simulador.cacheDatosN2[posCache, 0]-16, 2] == 0)//Si ninguna otra cache tiene el bloque
                                            {
                                                Simulador.directorioP1[Simulador.cacheDatosN2[posCache, 0]-16, 0] = 0;
                                            }
                                            Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                            Simulador.cacheDatosN2[posCache, 1] = 0;//Invalido la posicion en la cache

                                        }
                                        else//No puedo bloquear el directorio
                                        {
                                            terminado = false;                                            
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
											volverAEmpezar = true;
                                        }
                                    }
                                }

                            }
							if (!volverAEmpezar){
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
                                            Simulador.directorioP0[numeroBloque, 3] = 1;//Indico que esta en cache
                                            
                                            terminado = true;//Solo falta obtener de cache

                                        }
                                        else//No se puede bloquear la memoria
                                        {
                                            terminado = false;
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                        }
                                    }
                                    else//Si en el directorio esta M
                                    {
                                        if (Simulador.directorioP0[numeroBloque, 1] == 1)//Esta en cache del N0
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
                                                    for (int j = 0; j < 6; j++)//Copio lo que hay en la cache del N1 en la cache del N2
                                                    {
                                                        Simulador.cacheDatosN2[posCache, j] = Simulador.cacheDatosN0[posCache, j];
                                                    }
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                    Simulador.directorioP0[numeroBloque, 0] = 1;//Pongo directorio en C
                                                    Simulador.cacheDatosN0[posCache, 1] = 1;//Pongo cache en C
                                                    Simulador.directorioP0[numeroBloque, 3] = 1;//Indico que esta en cache

                                                }
                                                else//No se puede bloquear la memoria
                                                {
                                                    terminado = false;
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                }
												Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache
                                            }
                                            else//No se puede bloquear la cache
                                            {
                                                terminado = false;
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
                                                    
                                                    Simulador.directorioP0[numeroBloque, 3] = 1;//Indico que esta en cache                   
                                                }
                                                else//No se puede bloquear la memoria
                                                {
                                                    terminado = false;
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                }
												Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache
                                            }
                                            else//No se puede bloquear la cache
                                            {
                                                terminado = false;
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                    }
									Monitor.Exit(Simulador.directorioP0);//Libero el directorio
                                }
                                else//No se puede bloquear el directorio
                                {
                                    terminado = false;
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
                                            for (int i = 0; i < 16; i++)
                                                Simulador.barrera.SignalAndWait();
                                            Monitor.Exit(Simulador.memCompartidaP1);//Libero la memoria
                                            if (Simulador.directorioP1[numeroBloque-16, 0] == 0)
                                                Simulador.directorioP1[numeroBloque-16, 0] = 1;//Pongo directorio en C
                                            Simulador.directorioP1[numeroBloque-16, 3] = 1;//Indico que esta en cache
                                            
                                            terminado = true;//Solo falta obtener de cache

                                        }
                                        else//No se puede bloquear la memoria
                                        {
                                            terminado = false;
                                            Simulador.reloj++;
                                            cicloActual++;
                                            Simulador.barrera.SignalAndWait();
                                        }
                                    }
                                    else//Si en el directorio esta M
                                    {
                                        if (Simulador.directorioP1[numeroBloque-16, 1] == 1)//Esta en cache del N0
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
                                                    Simulador.directorioP1[numeroBloque-16, 3] = 1;//Indico que esta en cache

                                                }
                                                else//No se puede bloquear la memoria
                                                {
                                                    terminado = false;
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                }
												Monitor.Exit(Simulador.cacheDatosN0);//Libero la cache
                                            }
                                            else//No se puede bloquear la cache
                                            {
                                                terminado = false;
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
                                                    
                                                    Simulador.directorioP1[numeroBloque-16, 3] = 1;//Indico que esta en cache         
                                                }
                                                else//No se puede bloquear la memoria
                                                {
                                                    terminado = false;
                                                    Simulador.reloj++;
                                                    cicloActual++;
                                                    Simulador.barrera.SignalAndWait();
                                                }
												Monitor.Exit(Simulador.cacheDatosN1);//Libero la cache
                                            }
                                            else//No se puede bloquear la cache
                                            {
                                                terminado = false;
                                                Simulador.reloj++;
                                                cicloActual++;
                                                Simulador.barrera.SignalAndWait();
                                            }
                                        }
                                    }
									Monitor.Exit(Simulador.directorioP1);//Libero el directorio
                                }
                                else//No se puede bloquear el directorio
                                {
                                    terminado = false;
                                    Simulador.reloj++;
                                    cicloActual++;
                                    Simulador.barrera.SignalAndWait();
                                }
                            }

							}
                            
                        }
                        else
                            terminado = true;
                        if (terminado)//Si esta en cache y esta modificado o compartido
                        {
                            registros[IR[2]] = Simulador.cacheDatosN2[posCache, numPalabra + 2];//Copiamos lo que tiene la cache en el registro
                            terminado = true;
                        }
						Monitor.Exit(Simulador.cacheDatosN2);//Libero la cache local
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