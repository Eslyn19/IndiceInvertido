using DatosProyectoI.Model;
using DatosProyectoI.Algoritmos;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DatosProyectoI.Services
{
    public class Builder
    {
        private static Builder instance;
        private static object candado = new object();
        
        private string[] stopwords;
        private IndicePreprocesado indiceActual;

        private Builder()
        {
            CargarStopwords();
            indiceActual = new IndicePreprocesado();
        }
        
        // Patron singleton
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

        private void CargarStopwords()
        {
            string ruta = "Stopwords.txt";
            try
            {
                if (File.Exists(ruta))
                {
                    stopwords = File.ReadAllLines(ruta, Encoding.UTF8);
                }
                else
                {
                    stopwords = new string[0];
                }
                
            } catch(Exception ex) {
                Console.WriteLine(ex);
                stopwords = new string[0];
            }
        }
        
        private bool EsStopword(string token)
        {
            if (stopwords == null || stopwords.Length == 0)
            {
                return false;
            }
            
            for (int i = 0; i < stopwords.Length; i++)
            {
                if (stopwords[i] == token)
                {
                    return true;
                }
            }
            return false;
        }
        
        public void ProcesarDocumentos(string ruta, double zipf)
        {
            try
            {
                var documentos = CargarTokenizador(ruta);

                var indiceInvertido = CrearIndiceInvertido(documentos);

                var calculoTF_IDF = CalculoTF_IDF(indiceInvertido, documentos.Length);

                var leyZipf = AplicarLeyZipf(calculoTF_IDF, zipf);
                
                var docsPreprocesados = CalcularVectoresTFIDF(documentos, leyZipf);
                
                // Crear indice preprocesado
                indiceActual = new IndicePreprocesado(
                    documentos.Length,
                    leyZipf.Length,
                    zipf,
                    leyZipf.Select(t => t.palabra).ToArray(),
                    leyZipf,
                    docsPreprocesados
                );
                
                // Guardar en archivo
                GuardarIndicePreprocesado();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante el procesamiento: {ex.Message}");
            }
        }
        
        private ActualDoc[] CargarTokenizador(string rutaCarpeta)
        {
            var documentos = new List<ActualDoc>();
            string rutaCompleta = Path.GetFullPath(rutaCarpeta);
            
            if (!Directory.Exists(rutaCompleta))
            {
                return new ActualDoc[0];
            }
            
            string[] archivos = Directory.GetFiles(rutaCompleta, "*.txt");
            
            foreach (string archivo in archivos)
            {
                try
                {
                    string nombre = Path.GetFileName(archivo);
                    string contenido = File.ReadAllText(archivo, Encoding.UTF8);
                    string url = Decodificador.DecodificarArchivo(nombre);
                    
                    // Tokenización y filtrado de stopwords
                    string[] tokens = Tokenizador(contenido);
                    
                    documentos.Add(new ActualDoc(nombre, url, contenido, tokens));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            
            return documentos.ToArray();
        }
        
        private string[] Tokenizador(string texto)
        {
            string normalizar = Regex.Replace(texto.ToLower(), @"[^\w\s]", " ");
            
            // Dividir en tokens
            string[] tokens = normalizar.Split(new char[] { ' ', '\n' }, 
                StringSplitOptions.RemoveEmptyEntries);
            
            string[] tokensFiltrados = new string[tokens.Length];
            int cont = 0; // cantidad tokens filtrados
            
            foreach (string token in tokens)
            {
                if (token.Length > 2 && !EsStopword(token))
                {
                    tokensFiltrados[cont] = token;
                    cont++;
                }
            }
            
       
            string[] result = new string[cont];
            for (int i = 0; i < cont; i++)
            {
                result[i] = tokensFiltrados[i];
            }
            
            return result;
        }
        
        private IndiceInvertido CrearIndiceInvertido(ActualDoc[] documentos)
        {
            var indice = new IndiceInvertido();
            
            for (int k = 0; k < documentos.Length; k++)
            {
                var documento = documentos[k];
                
                // Contar frecuencias de términos en este documento usando arrays
                string[] terminosUnicos = new string[documento.Tokens.Length];
                int[] frecuencias = new int[documento.Tokens.Length];
                int terminosCount = 0;
                
                foreach (string token in documento.Tokens)
                {
                    // Buscar si el término ya existe
                    int indiceTermino = -1;
                    for (int i = 0; i < terminosCount; i++)
                    {
                        if (terminosUnicos[i] == token)
                        {
                            indiceTermino = i;
                            break;
                        }
                    }
                    
                    if (indiceTermino >= 0)
                    {
                        frecuencias[indiceTermino]++;
                    }
                    else
                    {
                        terminosUnicos[terminosCount] = token;
                        frecuencias[terminosCount] = 1;
                        terminosCount++;
                    }
                }
                
                // Agregar al índice invertido
                for (int i = 0; i < terminosCount; i++)
                {
                    indice.AgregarTermino(terminosUnicos[i], k, frecuencias[i]);
                }
            }
            
            return indice;
        }
        
        private Termino[] CalculoTF_IDF(IndiceInvertido index, int N)
        {
            Termino[] terminos = new Termino[index.terminosCont];
            
            for (int i = 0; i < index.terminosCont; i++)
            {
                string palabra = index.Terminos[i];
                var documentos = index.ObtenerDocumentos(palabra);
                
                // Calcular df
                int df = documentos.Length;
                
                // Calcular IDF
                double idf = Math.Log10((double)N / df);
                
                // Crear string de documentos con frecuencias
                string[] docStrings = new string[documentos.Length];
                for (int j = 0; j < documentos.Length; j++)
                {
                    docStrings[j] = $"D{documentos[j].DocId + 1}:{documentos[j].Frecuencia}";
                }
                
                string frecuencia = "";
                for (int k = 0; k < docStrings.Length; k++)
                {
                    if (k > 0) frecuencia += ",";
                    frecuencia += docStrings[k];
                }

                terminos[i] = new Termino(palabra, df, idf, frecuencia);
            }
            
            return terminos;
        }
        
        private Termino[] AplicarLeyZipf(Termino[] terminos, double porc)
        {
            // Crear copia del array para ordenar
            Termino[] ordenados = new Termino[terminos.Length];
            for (int i = 0; i < terminos.Length; i++)
            {
                ordenados[i] = terminos[i];
            }
            
            OrdenamientoRadix.RadixSort(ordenados);
            
            // Calcular cuantos terminos dejarse
            int Total = ordenados.Length;
            int terminosMantenidos = (int)Math.Ceiling(Total  * (1.0 - porc / 100.0));
            
            // Arreglo de terminos mantenidos
            Termino[] res = new Termino[terminosMantenidos];
            for (int i = 0; i < terminosMantenidos; i++)
            {
                res[i] = ordenados[i];
            }
            
            return res;
        }
        
        private Documento[] CalcularVectoresTFIDF(ActualDoc[] documentos, Termino[] terminos)
        {
            var documentosPreprocesados = new Documento[documentos.Length];
            
            for (int docId = 0; docId < documentos.Length; docId++)
            {
                var documento = documentos[docId];
                var vector = new double[terminos.Length];
                
                // Contar frecuencias por documento
                string[] terminosUnicos = new string[documento.Tokens.Length];
                int[] frecuencias = new int[documento.Tokens.Length];
                int cont = 0;
                
                foreach (string token in documento.Tokens)
                {
                    // Buscar si el término ya existe
                    int indiceTermino = -1;
                    for (int i = 0; i < cont; i++)
                    {
                        if (terminosUnicos[i] == token)
                        {
                            indiceTermino = i;
                            break;
                        }
                    }
                    
                    if (indiceTermino >= 0)
                    {
                        frecuencias[indiceTermino]++;
                    }
                    else
                    {
                        terminosUnicos[cont] = token;
                        frecuencias[cont] = 1;
                        cont++;
                    }
                }
                
                // Calcular TF-IDF para cada término
                for (int termId = 0; termId < terminos.Length; termId++)
                {
                    var termino = terminos[termId];
                    
                    // Buscar frecuencia del término en este documento
                    int tf = 0;
                    for (int i = 0; i < cont; i++)
                    {
                        if (terminosUnicos[i] == termino.palabra)
                        {
                            tf = frecuencias[i];
                        break;
                        }
                    }
                    
                    if (tf > 0)
                    {
                        double tfidf = tf * termino.IDF;
                        vector[termId] = tfidf;
                    }
                    else
                    {
                        vector[termId] = 0.0;
                    }
                }
                
                documentosPreprocesados[docId] = new Documento(
                    documento.Nombre,
                    documento.URL,
                    documento.Tokens.Length,
                    vector
                );
            }
            
            return documentosPreprocesados;
        }
        
        private void GuardarIndicePreprocesado()
        {
            try
            {
                string rutaArchivo = "../../../../indicePreprocesado.json";
                
                var opciones = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                
                string json = JsonSerializer.Serialize(indiceActual, opciones);
                File.WriteAllText(rutaArchivo, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar indice, {ex.Message}");
            }
        }
        
        public void CargarIndicePreprocesado()
        {
            try
            {
                string rutaArchivo = "../../../../indicePreprocesado.json";
                
                if (!File.Exists(rutaArchivo))
                {
                    Console.WriteLine($"El archivo {rutaArchivo} no existe");
                    return;
                }
                
                string json = File.ReadAllText(rutaArchivo, Encoding.UTF8);
                indiceActual = JsonSerializer.Deserialize<IndicePreprocesado>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar el índice: {ex.Message}");
            }
        }

        public void Consultar(string consulta)
        {
            // Tokenizar consulta
            string[] tokensConsulta = Tokenizador(consulta);
            
            // Crear vector de consulta
            var vectorConsulta = new double[indiceActual.totalTerms];
            
            for (int i = 0; i < indiceActual.totalTerms; i++)
            {
                string termino = indiceActual.terminos[i];
                
                if (tokensConsulta.Contains(termino))
                {
                    vectorConsulta[i] = indiceActual.terminosDet[i].IDF;
                }
                else
                {
                    vectorConsulta[i] = 0.0;
                }
            }
            
            // Calcular similitud coseno con cada documento
            int[] docIds = new int[indiceActual.totalDocs];
            double[] similitudes = new double[indiceActual.totalDocs];
            int resultadosCount = 0;
            
            for (int docId = 0; docId < indiceActual.totalDocs; docId++)
            {
                double similitud = CalcularCoseno(vectorConsulta, indiceActual.documentos[docId].arrIDF);
                if (similitud > 0)
                {
                    docIds[resultadosCount] = docId;
                    similitudes[resultadosCount] = similitud;
                    resultadosCount++;
                }
            }
            
            // Ordenar por similitud descendente usando Bubble Sort
            OrdenamientoBorbuja(similitudes, docIds, resultadosCount);
            
            Console.WriteLine($"\nConsulta: '{consulta}'");
            string[] terminosEncontrados = new string[tokensConsulta.Length];
            int terminosEncontradosCount = 0;
            
            for (int i = 0; i < tokensConsulta.Length; i++)
            {
                string token = tokensConsulta[i];
                for (int j = 0; j < indiceActual.totalTerms; j++)
                {
                    if (indiceActual.terminos[j] == token)
                    {
                        terminosEncontrados[terminosEncontradosCount] = token;
                        terminosEncontradosCount++;
                        break;
                    }
                }
            }
            
            // Crear string de términos encontrados
            string terminosStr = "";
            for (int i = 0; i < terminosEncontradosCount; i++)
            {
                if (i > 0) terminosStr += ", ";
                terminosStr += terminosEncontrados[i];
            }
            
            Console.WriteLine($"Términos encontrados: {terminosStr}");
            Console.WriteLine($"\nTop 10: ");
            Console.WriteLine("Documento\tSimilitud\tURL");
            
            for (int i = 0; i < Math.Min(10, resultadosCount); i++)
            {
                int docId = docIds[i];
                double similitud = similitudes[i];
                var documento = indiceActual.documentos[docId];
                Console.WriteLine($"D{docId + 1}\t\t{similitud:F3}\t\t{documento.URL}");
            }
            Console.ReadKey();
        }

        private void OrdenamientoBorbuja(double[] similitudes, int[] docIds, int count)
        {
            for (int i = 0; i < count - 1; i++)
            {
                for (int j = 0; j < count - i - 1; j++)
                {
                    if (similitudes[j] < similitudes[j + 1])
                    {
                        // Intercambiar similitudes
                        double tempSim = similitudes[j];
                        similitudes[j] = similitudes[j + 1];
                        similitudes[j + 1] = tempSim;
                        
                        // Intercambiar docIds
                        int tempDoc = docIds[j];
                        docIds[j] = docIds[j + 1];
                        docIds[j + 1] = tempDoc;
                    }
                }
            }
        }

        private double CalcularCoseno(double[] arr, double[] arr2)
        {
            if (arr.Length != arr2.Length) { 
                return 0.0;
            }
            
            double productoPunto = 0.0;
            double magnitud = 0.0; 
            double magnitud2 = 0.0;
            
            for (int i = 0; i < arr.Length; i++)
            {
                productoPunto += arr[i] * arr2[i];
                magnitud += arr[i] * arr[i];
                magnitud2 += arr2[i] * arr2[i];
            }

            if (magnitud == 0.0 || magnitud2 == 0.0)
            {
                return 0.0;
            }
            
            return productoPunto / (Math.Sqrt(magnitud) * Math.Sqrt(magnitud2));
        }
    }
    
}

