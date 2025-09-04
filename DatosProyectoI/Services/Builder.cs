using DatosProyectoI.Model;
using DatosProyectoI.EstructuraDatos;
using DatosProyectoI.Algoritmos;

namespace DatosProyectoI.Services
{
    internal class Builder
    {
        private static Builder instance;
        private static object candado = new object();
        
        private ListaCircular<Documento> documentos;
        private ListaCircular<Termino> terminos;
        private string[] stopwords;

        private Builder()
        {
            documentos = new ListaCircular<Documento>();
            terminos = new ListaCircular<Termino>();
            CargarStopwords();
        }
        
        // Patron singelton
        public static Builder getInstance()
        {
            if (instance == null)
            {
                lock (candado)
                {
                    if (instance == null)
                    {
                        instance = new Builder();
                    }
                }
            }
            return instance;
        }

        public int DocumentosCount => documentos.Count;

        //public void CrearRuta()
        //{
        //    Console.WriteLine("Ingrese la ruta donde se encuentran los documentos:");
        //    string rutaDocs = Console.ReadLine();
            
        //    if (string.IsNullOrWhiteSpace(rutaDocs))
        //    {
        //        Console.WriteLine("Ruta no válida. Usando ruta por defecto: ../../../../Documentos2");
        //        rutaDocs = "../../../../Documentos2";
        //    }
            
        //    CargarDocumentos(rutaDocs);
        //}

        private void CargarStopwords()
        {
            stopwords = new string[] {
                "el", "la", "lo", "los", "las", "un", "una", "unos", "unas",
                "de", "del", "en", "con", "por", "para", "sobre", "bajo",
                "entre", "durante", "desde", "hasta", "hacia", "contra",
                "sin", "segun", "mediante", "excepto", "salvo", "menos",
                "más", "muy", "mucho", "poco", "bastante", "demasiado",
                "todo", "toda", "todos", "todas", "alguno", "alguna",
                "algunos", "algunas", "ninguno", "ninguna", "ningunos", "ningunas",
                "este", "esta", "estos", "estas", "ese", "esa", "esos", "esas",
                "aquel", "aquella", "aquellos", "aquellas", "y", "o", "pero",
                "sino", "aunque", "porque", "si", "que", "como", "cuando",
                "donde", "quien", "cual", "cuyo", "cuya", "cuyos", "cuyas",
                "es", "son", "está", "están", "era", "eran", "fue", "fueron",
                "será", "serán", "ha", "han", "había", "habían", "habrá", "habrán",
                "tiene", "tienen", "tendrá", "tendrán",
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
                    Console.WriteLine($"Error cargando la ruta '{rutaCompleta}'");
                    return;
                }
                
                string[] archivos = Directory.GetFiles(rutaCompleta, "*.txt");
                foreach (string file in archivos)
                {
                    string nombre = Path.GetFileName(file);
                    string contenido = File.ReadAllText(file, System.Text.Encoding.UTF8);
                    
                    Documento documento = new Documento(nombre, contenido);
                    documento.ProcesarDoc(stopwords); 
                    documentos.Add(documento); // agregar a lista
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar documentos: {ex.Message}");
            }
        }

        public void ConstruirIndiceInvertido()
        {
            var terminosUnicos = new ListaCircular<string>();
            
            // Recopilar los terminos unicos
            foreach (Documento doc in documentos)
            {
                foreach (string token in doc.Tokens)
                {
                    if (!terminosUnicos.Contains(token))
                    {
                        terminosUnicos.Add(token);
                    }
                }
            }

            // Crear objetos Termino para cada termino unico
            foreach (string terminoStr in terminosUnicos)
            {
                Termino termino = new Termino(terminoStr);
                
                // Procesar todos los documentos para este termino
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
            var terminosArr = new Termino[terminos.Count];
            int j = 0;

            foreach (Termino t in terminos)
            {
                terminosArr[j++] = t;
            }

            // Ordenar por recorrido Radix
            OrdenamientoRadix.RadixSort(terminosArr);

            int MaxFrec = terminosArr[0].frecuencia;
            int MostrarCant = Math.Max(1, (int)Math.Ceiling(terminosArr.Length * (porcentaje / 100.0)));
            
            for (int i = 0; i < MostrarCant; i++)
            {
                double frecuenciaEsperada = (double)MaxFrec / (i + 1);
                double diferencia = Math.Abs(terminosArr[i].frecuencia - frecuenciaEsperada);
            }
        }

        private double CalcularMagnitud(double[] arr)
        {
            double suma = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                suma += arr[i] * arr[i];
            }
            return Math.Sqrt(suma);
        }

        private double CalcularProductoPunto(double[] arr, double[] arr2)
        {
            double suma = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                suma += arr[i] * arr2[i];
            }
            return suma;
        }

        private double CalcularSimilitudCoseno(List<string> terminosConsulta, Documento doc)
        {
            double[] DocumentoArr = new double[terminosConsulta.Count];

            for (int i = 0; i < terminosConsulta.Count; i++)
            {
                string terminoStr = terminosConsulta[i];

                // buscar termino
                Termino termino = null;
                foreach (Termino t in terminos)
                {
                    if (t.palabra.Equals(terminoStr, StringComparison.OrdinalIgnoreCase))
                    {
                        termino = t;
                        break;
                    }
                }

                DocumentoArr[i] = termino != null ? termino.CalcularTFIDF(doc) : 0;
            }

            double magnitudDoc = CalcularMagnitud(DocumentoArr);
            if (magnitudDoc == 0) 
            { 
                return 0; 
            }

            double[] ConsultaArr = new double[terminosConsulta.Count];
            for (int i = 0; i < terminosConsulta.Count; i++)
            {
                ConsultaArr[i] = 1.0; // producto con peso 1
            }

            double magnitudConsulta = CalcularMagnitud(ConsultaArr);
            double prodPunto = CalcularProductoPunto(ConsultaArr, DocumentoArr);

            return prodPunto / (magnitudConsulta * magnitudDoc);
        }

        public void Consultar(string consulta)
        {
            string[] palabrasConsulta = consulta.ToLower().Split(new char[] { ' ', '.', ',' }, StringSplitOptions.RemoveEmptyEntries);

            var terminosConsulta = new List<string>();
            foreach (string palabra in palabrasConsulta)
            {
                if (!stopwords.Contains(palabra) && !terminosConsulta.Contains(palabra))
                {
                    terminosConsulta.Add(palabra);
                }
            }

            // Buscar documentos que contengan los terminos
            var DocsImportantes = new ListaCircular<Documento>();
            
            foreach (string terminoStr in terminosConsulta)
            {
                foreach (Termino termino in terminos)
                {
                    if (termino.palabra.Equals(terminoStr, StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (Documento doc in termino.Documentos)
                        {
                            if (!DocsImportantes.Contains(doc))
                            {
                                DocsImportantes.Add(doc);
                            }
                        }
                        break;
                    }
                }
            }
            
            MostrarMatrizTFIDF(terminosConsulta, DocsImportantes);
        }

        private void MostrarMatrizTFIDF(List<string> terminosConsulta, ListaCircular<Documento> docs)
        {
            // Verifica si existe el termino de la consulta
            if (docs.Count == 0)
            {
                Console.WriteLine("Consulta no encontrada");
                return;
            }

            Console.WriteLine("\n> similitud coseno <");
            Console.WriteLine("Consulta vs Documento\tSimilitud Coseno");

            // Calcular similitud coseno para cada documento
            var TerminosSimiles = new List<(Documento doc, double similitudCoseno)>();
            
            foreach (Documento doc in docs)
            {
                double similitudCoseno = CalcularSimilitudCoseno(terminosConsulta, doc);
                TerminosSimiles.Add((doc, similitudCoseno));
            }
            
            // Mostrar resultados (primeros 10)
            var Tops = TerminosSimiles
                .OrderByDescending(x => x.similitudCoseno)
                .Take(10)
                .ToList();

            for (int i = 0; i < Tops.Count; i++)
            {
                var resultado = Tops[i];
                string nombreDoc = resultado.doc.nombre.Length > 20 ? 
                    resultado.doc.nombre : 
                    resultado.doc.nombre;
                
                Console.WriteLine($"q vs D{i + 1} ({nombreDoc})\t≈ {resultado.similitudCoseno:F3}");
            }
        }

    }
}