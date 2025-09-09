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
                    stopwords = Array.Empty<string>();
                }
            } catch(Exception ex) {
                stopwords = Array.Empty<string>();
                Console.WriteLine(ex);
            }
        }
        
        private bool EsStopword(string token)
        {
            for (int i = 0; i < stopwords.Length; i++)
            {
                if (stopwords[i] == token)
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Carga archivos TXT desde una carpeta y los convierte en un array de DocumentoOriginal
        /// </summary>
        /// <param name="rutaCarpeta">Ruta de la carpeta que contiene los archivos TXT</param>
        /// <returns>Array de DocumentoOriginal con el contenido de los archivos</returns>
        public DocumentoOriginal[] CargarArchivosTxt(string rutaCarpeta)
        {
            try
            {
                string rutaCompleta = Path.GetFullPath(rutaCarpeta);
                
                if (!Directory.Exists(rutaCompleta))
                {
                    Console.WriteLine($"Error: La carpeta '{rutaCompleta}' no existe.");
                    return new DocumentoOriginal[0];
                }
                
                string[] archivos = Directory.GetFiles(rutaCompleta, "*.txt");
                
                if (archivos.Length == 0)
                {
                    Console.WriteLine($"No se encontraron archivos TXT en la carpeta '{rutaCompleta}'.");
                    return new DocumentoOriginal[0];
                }
                
                var documentos = new DocumentoOriginal[archivos.Length];
                int documentosCargados = 0;
                
                for (int i = 0; i < archivos.Length; i++)
                {
                    try
                    {
                        string archivo = archivos[i];
                        string nombre = Path.GetFileName(archivo);
                        string contenido = File.ReadAllText(archivo, Encoding.UTF8);
                        string url = Decodificador.DecodificarArchivo(nombre);
                        
                        // Tokenización y filtrado de stopwords
                        string[] tokens = TokenizarYFiltrar(contenido);
                        
                        documentos[i] = new DocumentoOriginal(nombre, url, contenido, tokens);
                        documentosCargados++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error procesando archivo {archivos[i]}: {ex.Message}");
                        documentos[i] = new DocumentoOriginal("", "", "", new string[0]);
                    }
                }
                
                Console.WriteLine($"Carga completada: {documentosCargados} documentos procesados exitosamente");
                return documentos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general al cargar archivos TXT: {ex.Message}");
                return new DocumentoOriginal[0];
            }
        }
        
        /// <summary>
        /// Muestra información básica sobre los documentos cargados
        /// </summary>
        /// <param name="documentos">Array de documentos a analizar</param>
        public void MostrarInfoDocumentos(DocumentoOriginal[] documentos)
        {
            if (documentos == null || documentos.Length == 0)
            {
                Console.WriteLine("No hay documentos cargados.");
                return;
            }
            
            Console.WriteLine($"\n=== INFORMACIÓN DE DOCUMENTOS ===");
            Console.WriteLine($"Total de documentos: {documentos.Length}");
            
            int totalTokens = 0;
            int documentosConContenido = 0;
            string[] terminosUnicos = new string[10000]; // Array fijo para términos únicos
            int terminosUnicosCount = 0;
            
            foreach (var doc in documentos)
            {
                if (doc.Tokens.Length > 0)
                {
                    documentosConContenido++;
                    totalTokens += doc.Tokens.Length;
                    
                    foreach (string token in doc.Tokens)
                    {
                        // Verificar si el término ya existe
                        bool existe = false;
                        for (int i = 0; i < terminosUnicosCount; i++)
                        {
                            if (terminosUnicos[i] == token)
                            {
                                existe = true;
                                break;
                            }
                        }
                        
                        if (!existe && terminosUnicosCount < terminosUnicos.Length)
                        {
                            terminosUnicos[terminosUnicosCount] = token;
                            terminosUnicosCount++;
                        }
                    }
                }
            }
            
            Console.WriteLine($"Documentos con contenido: {documentosConContenido}");
            Console.WriteLine($"Total de tokens: {totalTokens:N0}");
            Console.WriteLine($"Términos únicos: {terminosUnicosCount:N0}");
            Console.WriteLine($"Promedio de tokens por documento: {(documentosConContenido > 0 ? totalTokens / documentosConContenido : 0):F1}");
            
            // Mostrar algunos ejemplos de documentos
            Console.WriteLine($"\n=== EJEMPLOS DE DOCUMENTOS ===");
            int ejemplosMostrados = 0;
            for (int i = 0; i < documentos.Length && ejemplosMostrados < 5; i++)
            {
                if (documentos[i].Tokens.Length > 0)
                {
                    Console.WriteLine($"D{i + 1}: {documentos[i].URL}");
                    Console.WriteLine($"   Tokens: {documentos[i].Tokens.Length}");
                    Console.WriteLine($"   Primeros tokens: {string.Join(", ", documentos[i].Tokens.Take(10))}");
                    ejemplosMostrados++;
                }
            }
        }
        
        public void ProcesarDocumentos(string rutaCarpeta, double porcentajeZipf)
        {
            try
            {
                // 1. Cargar y tokenizar documentos
                var documentosOriginales = CargarYTokenizarDocumentos(rutaCarpeta);
                if (documentosOriginales.Length == 0)
                {
                    Console.WriteLine("No se encontraron documentos para procesar.");
                    return;
                }
                
                // 2. Crear índice invertido básico
                var indiceInvertido = CrearIndiceInvertido(documentosOriginales);
                
                // 3. Calcular TF, DF, IDF
                var terminosConEstadisticas = CalcularEstadisticasTerminos(indiceInvertido, documentosOriginales.Length);
                
                // 4. Aplicar ley de Zipf
                var terminosFiltrados = AplicarLeyZipf(terminosConEstadisticas, porcentajeZipf);
                
                // 5. Calcular vectores TF-IDF para cada documento
                var documentosPreprocesados = CalcularVectoresTFIDF(documentosOriginales, terminosFiltrados);
                
                // 6. Crear índice preprocesado final
                indiceActual = new IndicePreprocesado(
                    documentosOriginales.Length,
                    terminosFiltrados.Length,
                    porcentajeZipf,
                    terminosFiltrados.Select(t => t.palabra).ToArray(),
                    terminosFiltrados,
                    documentosPreprocesados
                );
                
                // 7. Guardar en archivo
                GuardarIndicePreprocesado();
                
                Console.WriteLine($"Procesamiento completado:");
                Console.WriteLine($"- Documentos procesados: {documentosOriginales.Length}");
                Console.WriteLine($"- Términos originales: {terminosConEstadisticas.Length}");
                Console.WriteLine($"- Términos después de Zipf: {terminosFiltrados.Length}");
                Console.WriteLine($"- Archivo guardado: indicePreprocesado.json");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante el procesamiento: {ex.Message}");
            }
        }
        
        private DocumentoOriginal[] CargarYTokenizarDocumentos(string rutaCarpeta)
        {
            var documentos = new List<DocumentoOriginal>();
            string rutaCompleta = Path.GetFullPath(rutaCarpeta);
            
            if (!Directory.Exists(rutaCompleta))
            {
                return new DocumentoOriginal[0];
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
                    string[] tokens = TokenizarYFiltrar(contenido);
                    
                    documentos.Add(new DocumentoOriginal(nombre, url, contenido, tokens));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            
            return documentos.ToArray();
        }
        
        private string[] TokenizarYFiltrar(string texto)
        {
            // Normalizar texto: minúsculas, remover caracteres especiales
            string textoNormalizado = Regex.Replace(texto.ToLower(), @"[^\w\s]", " ");
            
            // Dividir en tokens
            string[] tokens = textoNormalizado.Split(new char[] { ' ', '\t', '\n', '\r' }, 
                StringSplitOptions.RemoveEmptyEntries);
            
            // Filtrar stopwords y tokens muy cortos
            string[] tokensFiltrados = new string[tokens.Length]; // Array del mismo tamaño
            int tokensFiltradosCount = 0;
            
            foreach (string token in tokens)
            {
                if (token.Length > 2 && !EsStopword(token))
                {
                    tokensFiltrados[tokensFiltradosCount] = token;
                    tokensFiltradosCount++;
                }
            }
            
            // Crear array del tamaño exacto
            string[] resultado = new string[tokensFiltradosCount];
            for (int i = 0; i < tokensFiltradosCount; i++)
            {
                resultado[i] = tokensFiltrados[i];
            }
            
            return resultado;
        }
        
        private IndiceInvertidoBasico CrearIndiceInvertido(DocumentoOriginal[] documentos)
        {
            var indice = new IndiceInvertidoBasico();
            
            for (int docId = 0; docId < documentos.Length; docId++)
            {
                var documento = documentos[docId];
                
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
                    indice.AgregarTermino(terminosUnicos[i], docId, frecuencias[i]);
                }
            }
            
            return indice;
        }
        
        private Termino[] CalcularEstadisticasTerminos(IndiceInvertidoBasico indice, int totalDocumentos)
        {
            Termino[] terminos = new Termino[indice.TerminosCount];
            
            for (int i = 0; i < indice.TerminosCount; i++)
            {
                string palabra = indice.Terminos[i];
                var documentos = indice.ObtenerDocumentos(palabra);
                
                // Calcular Document Frequency (DF)
                int df = documentos.Length;
                
                // Calcular IDF
                double idf = Math.Log10((double)totalDocumentos / df);
                
                // Crear string de documentos con frecuencias usando array
                string[] docStrings = new string[documentos.Length];
                for (int j = 0; j < documentos.Length; j++)
                {
                    docStrings[j] = $"D{documentos[j].DocId + 1}:{documentos[j].Frecuencia}";
                }
                
                // Unir strings manualmente
                string docStringFinal = "";
                for (int k = 0; k < docStrings.Length; k++)
                {
                    if (k > 0) docStringFinal += ",";
                    docStringFinal += docStrings[k];
                }
                
                terminos[i] = new Termino(
                    palabra,
                    df,
                    idf,
                    docStringFinal
                );
            }
            
            return terminos;
        }
        
        private Termino[] AplicarLeyZipf(Termino[] terminos, double porcentajeReduccion)
        {
            // Crear copia del array para ordenar
            Termino[] terminosOrdenados = new Termino[terminos.Length];
            for (int i = 0; i < terminos.Length; i++)
            {
                terminosOrdenados[i] = terminos[i];
            }
            
            OrdenamientoRadix.RadixSortPorFrecuenciaDocs(terminosOrdenados);
            
            // Calcular cuántos términos mantener
            int totalTerminos = terminosOrdenados.Length;
            int terminosAMantener = (int)Math.Ceiling(totalTerminos * (1.0 - porcentajeReduccion / 100.0));
            
            // Crear array con los términos a mantener
            Termino[] resultado = new Termino[terminosAMantener];
            for (int i = 0; i < terminosAMantener; i++)
            {
                resultado[i] = terminosOrdenados[i];
            }
            
            return resultado;
        }
        
        private Documento[] CalcularVectoresTFIDF(DocumentoOriginal[] documentos, Termino[] terminos)
        {
            var documentosPreprocesados = new Documento[documentos.Length];
            
            for (int docId = 0; docId < documentos.Length; docId++)
            {
                var documento = documentos[docId];
                var vector = new double[terminos.Length];
                
                // Contar frecuencias en este documento usando arrays
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
                
                // Calcular TF-IDF para cada término
                for (int termId = 0; termId < terminos.Length; termId++)
                {
                    var termino = terminos[termId];
                    
                    // Buscar frecuencia del término en este documento
                    int tf = 0;
                    for (int i = 0; i < terminosCount; i++)
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
                
                string rutaCompleta = Path.GetFullPath(rutaArchivo);
                Console.WriteLine($"Ubicación: {rutaCompleta}");
                Console.WriteLine($"Tamaño del archivo: {new FileInfo(rutaCompleta).Length / 1024.0:F1} KB");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar el índice: {ex.Message}");
            }
        }
        
        public void CargarIndicePreprocesado()
        {
            try
            {
                string rutaArchivo = "../../../../indicePreprocesado.json";
                
                if (!File.Exists(rutaArchivo))
                {
                    Console.WriteLine($"El archivo {rutaArchivo} no existe. Primero debe procesar los documentos.");
                    return;
                }
                
                string json = File.ReadAllText(rutaArchivo, Encoding.UTF8);
                indiceActual = JsonSerializer.Deserialize<IndicePreprocesado>(json);
                
                Console.WriteLine($"Archivo {rutaArchivo} cargado correctamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar el índice: {ex.Message}");
            }
        }

        public void Consultar(string consulta)
        {
            // Tokenizar consulta
            string[] tokensConsulta = TokenizarYFiltrar(consulta);
            
            
            // Crear vector de consulta
            var vectorConsulta = new double[indiceActual.totalTerms];
            
            for (int i = 0; i < indiceActual.totalTerms; i++)
            {
                string termino = indiceActual.terminos[i];
                
                if (tokensConsulta.Contains(termino))
                {
                    // TF-IDF de la consulta (asumiendo frecuencia 1 para cada término)
                    vectorConsulta[i] = indiceActual.terminosDet[i].IDF;
                }
                else
                {
                    vectorConsulta[i] = 0.0;
                }
            }
            
            // Calcular similitud coseno con cada documento usando arrays
            int[] docIds = new int[indiceActual.totalDocs];
            double[] similitudes = new double[indiceActual.totalDocs];
            int resultadosCount = 0;
            
            for (int docId = 0; docId < indiceActual.totalDocs; docId++)
            {
                double similitud = CalcularSimilitudCoseno(vectorConsulta, indiceActual.documentos[docId].arrIDF);
                if (similitud > 0)
                {
                    docIds[resultadosCount] = docId;
                    similitudes[resultadosCount] = similitud;
                    resultadosCount++;
                }
            }
            
            // Ordenar por similitud descendente usando Bubble Sort
            for (int i = 0; i < resultadosCount - 1; i++)
            {
                for (int j = 0; j < resultadosCount - i - 1; j++)
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
            
            // Mostrar resultados
            Console.WriteLine($"\nConsulta: '{consulta}'");
            // Encontrar términos que están en el índice
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
            Console.WriteLine($"\nResultados (top 10):");
            Console.WriteLine("Documento\tSimilitud\tURL");
            
            for (int i = 0; i < Math.Min(10, resultadosCount); i++)
            {
                int docId = docIds[i];
                double similitud = similitudes[i];
                var documento = indiceActual.documentos[docId];
                Console.WriteLine($"D{docId + 1}\t\t{similitud:F3}\t\t{documento.URL}");
            }
        }
        
        private double CalcularSimilitudCoseno(double[] vector1, double[] vector2)
        {
            if (vector1.Length != vector2.Length)
                return 0.0;
            
            double productoPunto = 0.0;
            double magnitud1 = 0.0;
            double magnitud2 = 0.0;
            
            for (int i = 0; i < vector1.Length; i++)
            {
                productoPunto += vector1[i] * vector2[i];
                magnitud1 += vector1[i] * vector1[i];
                magnitud2 += vector2[i] * vector2[i];
            }
            
            if (magnitud1 == 0.0 || magnitud2 == 0.0)
                return 0.0;
            
            return productoPunto / (Math.Sqrt(magnitud1) * Math.Sqrt(magnitud2));
        }
    }
    
}

