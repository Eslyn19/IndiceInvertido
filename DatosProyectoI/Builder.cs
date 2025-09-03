using System.Linq;

namespace DatosProyectoI
{
    internal class Builder
    {
        private ListaCircular<Documento> documentos;
        private ListaCircular<Termino> terminos;
        private string[] stopwords;

        public Builder()
        {
            documentos = new ListaCircular<Documento>();
            terminos = new ListaCircular<Termino>();
            InicializarStopwords();
        }

        public int DocumentosCount => documentos.Count;

        private void InicializarStopwords()
        {
            stopwords = new string[] {
                "el", "la", "lo", "los", "las", "un", "una", "unos", "unas",
                "de", "del", "en", "con", "por", "para", "sobre", "bajo",
                "entre", "durante", "desde", "hasta", "hacia", "contra",
                "sin", "según", "mediante", "excepto", "salvo", "menos",
                "más", "muy", "mucho", "poco", "bastante", "demasiado",
                "todo", "toda", "todos", "todas", "alguno", "alguna",
                "algunos", "algunas", "ninguno", "ninguna", "ningunos", "ningunas",
                "este", "esta", "estos", "estas", "ese", "esa", "esos", "esas",
                "aquel", "aquella", "aquellos", "aquellas", "y", "o", "pero",
                "sino", "aunque", "porque", "si", "que", "como", "cuando",
                "donde", "quien", "cual", "cuyo", "cuya", "cuyos", "cuyas",
                "es", "son", "está", "están", "era", "eran", "fue", "fueron",
                "será", "serán", "ha", "han", "había", "habían", "habrá", "habrán",
                "tiene", "tienen", "tenía", "tenían", "tendrá", "tendrán",
                "puede", "pueden", "podía", "podían", "podrá", "podrán",
                "debe", "deben", "debía", "debían", "deberá", "deberán",
                "quiere", "quieren", "quería", "querían", "querrá", "querrán", 
                "the", "of", "se", "and", "a", "e", "i", "o", "u", "b", "c", "d",
                "e", "f", "g", "h", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t",
                "v", "w", "x", "y", "z", "in", "1", "11", "00", "0", "no", "su", "in", "on",
                "2", "3", "4", "5", "6", "7", "8", "9", "al", "cr", "https", "to", "2562", "for",
                "doi", "10", "ac", "cr", "is", "as", "2277", "506", "12", "13", "or", "are", "be",
                "ii", "31", "nc", "2022", "2024", "2025"
            };
        }

        public void CargarDocumentos(string rutaCarpeta)
        {
            try
            {
                string rutaCompleta = Path.GetFullPath(rutaCarpeta);
                
                if (!Directory.Exists(rutaCompleta))
                {
                    Console.WriteLine($"Error: La carpeta '{rutaCompleta}' no existe.");
                    return;
                }
                
                string[] archivos = Directory.GetFiles(rutaCompleta, "*.txt");
                
                foreach (string archivo in archivos)
                {
                    string nombre = Path.GetFileName(archivo);
                    string contenido = File.ReadAllText(archivo, System.Text.Encoding.UTF8);
                    
                    Documento documento = new Documento(nombre, contenido);
                    documento.ProcesarDoc(stopwords);
                    documentos.Add(documento);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar documentos: {ex.Message}");
            }
        }

        public void ConstruirIndiceInvertido()
        {
            var terminosUnicos = new Dictionary<string, int>();
            
            // Recopilar todos los terminos unicos con sus frecuencias totales
            foreach (Documento doc in documentos)
            {
                foreach (string token in doc.Tokens)
                {
                    if (terminosUnicos.ContainsKey(token))
                    {
                        terminosUnicos[token]++;
                    }
                    else
                    {
                        terminosUnicos[token] = 1;
                    }
                }
            }

            // Crear objetos Termino directamente con frecuencias ya calculadas
            foreach (var kvp in terminosUnicos)
            {
                string terminoStr = kvp.Key;
                int frecuenciaTotal = kvp.Value;
                
                Termino termino = new Termino(terminoStr);
                
                // Solo procesar documentos que contengan este término
                foreach (Documento doc in documentos)
                {
                    int frecuencia = doc.getFrecuenciaTerm(terminoStr);
                    if (frecuencia > 0)
                    {
                        termino.AgregarDoc(doc, frecuencia);
                    }
                }
                
                termino.CalcularIDF(documentos.Count);
                terminos.Add(termino);
            }
        }

        public void AplicarLeyZipf(double porcentaje)
        {
            // Convertir a array para ordenar
            var terminosArray = new Termino[terminos.Count];
            int index = 0;
            foreach (Termino termino in terminos)
            {
                terminosArray[index++] = termino;
            }

            // Ordenar por frecuencia usando Radix Sort
            RadixSort.OrdenarPorFrecuencia(terminosArray);

            // Aplicar ley de Zipf: frecuencia esperada = frecuencia_máxima / rango
            int frecuenciaMaxima = terminosArray[0].frecuencia;
            
            // Calcular cuántos términos mostrar basado en el porcentaje
            int cantidadMostrar = Math.Max(1, (int)Math.Ceiling(terminosArray.Length * (porcentaje / 100.0)));
            
            for (int i = 0; i < cantidadMostrar; i++)
            {
                double frecuenciaEsperada = (double)frecuenciaMaxima / (i + 1);
                double diferencia = Math.Abs(terminosArray[i].frecuencia - frecuenciaEsperada);
                
                Console.WriteLine($"{i + 1}. '{terminosArray[i].palabra}' - " +
                                $"Frecuencia real: {terminosArray[i].frecuencia}, " +
                                $"Esperada: {frecuenciaEsperada:F2}, " +
                                $"Diferencia: {diferencia:F2}");
            }
        }

        private double CalcularSimilitudCoseno(List<string> terminosConsulta, Documento documento)
        {
            // Crear vector del documento (TF-IDF de cada término en el documento)
            double[] vectorDocumento = new double[terminosConsulta.Count];
            double magnitudDocumento = 0;
            
            for (int i = 0; i < terminosConsulta.Count; i++)
            {
                string terminoStr = terminosConsulta[i];
                
                // Buscar el término en el índice
                Termino termino = null;
                foreach (Termino t in terminos)
                {
                    if (t.palabra.Equals(terminoStr, StringComparison.OrdinalIgnoreCase))
                    {
                        termino = t;
                        break;
                    }
                }
                
                if (termino != null)
                {
                    vectorDocumento[i] = termino.CalcularTFIDF(documento);
                }
                else
                {
                    vectorDocumento[i] = 0;
                }
                
                magnitudDocumento += vectorDocumento[i] * vectorDocumento[i];
            }
            
            magnitudDocumento = Math.Sqrt(magnitudDocumento);
            
            // Si el documento no tiene magnitud para estos términos, no hay similitud
            if (magnitudDocumento == 0)
                return 0;
            
            // Crear vector de la consulta (todos los términos tienen peso 1, normalizado)
            double[] vectorConsulta = new double[terminosConsulta.Count];
            for (int i = 0; i < terminosConsulta.Count; i++)
            {
                vectorConsulta[i] = 1.0; // Cada término en la consulta tiene peso 1
            }
            
            // Magnitud de la consulta (raíz cuadrada de la suma de cuadrados)
            double magnitudConsulta = Math.Sqrt(terminosConsulta.Count);
            
            // Calcular producto punto
            double productoPunto = 0;
            for (int i = 0; i < terminosConsulta.Count; i++)
            {
                productoPunto += vectorConsulta[i] * vectorDocumento[i];
            }
            
            // Calcular similitud coseno: cos(θ) = (A · B) / (||A|| * ||B||)
            return productoPunto / (magnitudConsulta * magnitudDocumento);
        }

        public void Consultar(string consulta)
        {
            // Procesar la consulta
            string[] palabrasConsulta = consulta.ToLower()
                .Split(new char[] { ' ', '.', ',', ';', ':', '!', '?' }, 
                       StringSplitOptions.RemoveEmptyEntries);

            var terminosConsulta = new List<string>();
            foreach (string palabra in palabrasConsulta)
            {
                if (!stopwords.Contains(palabra) && !terminosConsulta.Contains(palabra))
                {
                    terminosConsulta.Add(palabra);
                }
            }

            // Buscar documentos que contengan los términos
            var documentosRelevantes = new ListaCircular<Documento>();
            
            foreach (string terminoStr in terminosConsulta)
            {
                foreach (Termino termino in terminos)
                {
                    if (termino.palabra.Equals(terminoStr, StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (Documento doc in termino.Documentos)
                        {
                            if (!documentosRelevantes.Contains(doc))
                            {
                                documentosRelevantes.Add(doc);
                            }
                        }
                        break;
                    }
                }
            }
            
            MostrarMatrizTFIDF(terminosConsulta, documentosRelevantes);
        }

        private void MostrarMatrizTFIDF(List<string> terminosConsulta, ListaCircular<Documento> documentosRelevantes)
        {
            Console.WriteLine("\n> similitud coseno <");
            Console.WriteLine("Consulta vs Documento\tSimilitud Coseno");

            // Calcular similitud coseno para cada documento
            var resultadosConSimilitud = new List<(Documento doc, double similitudCoseno)>();
            
            foreach (Documento doc in documentosRelevantes)
            {
                // Calcular similitud coseno
                double similitudCoseno = CalcularSimilitudCoseno(terminosConsulta, doc);
                
                resultadosConSimilitud.Add((doc, similitudCoseno));
            }
            
            // Ordenar por similitud coseno descendente y tomar los primeros 10
            var top10 = resultadosConSimilitud
                .OrderByDescending(x => x.similitudCoseno)
                .Take(10)
                .ToList();

            // Mostrar resultados ordenados
            for (int i = 0; i < top10.Count; i++)
            {
                var resultado = top10[i];
                string nombreDoc = resultado.doc.nombre.Length > 20 ? 
                    resultado.doc.nombre : 
                    resultado.doc.nombre;
                
                Console.WriteLine($"q vs D{i + 1} ({nombreDoc})\t≈ {resultado.similitudCoseno:F3}");
            }
        }

    }
}