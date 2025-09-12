using DatosProyectoI.Algoritmos;
using DatosProyectoI.EstructuraDatos;
using DatosProyectoI.Model;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DatosProyectoI.Services
{
    public class Builder
    {
        private static Builder instance = null;
        private static readonly object padlock = new object();

        private string[] stopwords;
        private IndicePreprocesado indiceActual;
        private ListaCircular<string> terminosTemporales;

        private Builder()
        {
            CargarStopwords();
            indiceActual = new IndicePreprocesado();
            terminosTemporales = new ListaCircular<string>();
        }

        // Patron singleton
        public static Builder Instance
        {
            get
            {
                if (instance == null)
        {
                // garantiza que un hilo no ingrese a una sección crítica del código mientras que otro hilo esté
                // en la sección crítica. Si otro hilo intenta ingresar un código bloqueado,
                // esperará y bloqueará hasta que se libere el objeto. (Microsoft)
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Builder();
                    }
                }
             }
                return instance;
            }

        }

        // Cargar el archivo .JSON con stops words
        private void CargarStopwords()
        {
            string ruta = "../../../../Stopwords.json";
            try
            {
                if (File.Exists(ruta))
                {
                    // Lee todo el archivo en una sola cadena
                    string jsonContent = File.ReadAllText(ruta, Encoding.UTF8);

                    // Deserializa la cadena JSON en un array de strings
                    stopwords = JsonSerializer.Deserialize<string[]>(jsonContent) ?? new string[0];
                }
                else
                {
                    Console.WriteLine("Archivo Stopwords.json no encontrado");
                    stopwords = new string[0];
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar stopwords: {ex.Message}");
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
        
        // Metodo para la opcion 1 del menu, crea el indice preprocesado 
        // y lo agrega al disco duro en formato JSON
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

        // Devuelve la ruta relativa de carpeta 'Documentos' del proyecto
        public string CrearDesdeRuta()
        {
            return "../../../../Documentos";
        }

        // Metodo para agregar un documento a la lista de documentos recorriendo
        // la carpeta de 'Documentos' donde se encuentran todos los .txt's y pasandolos
        // por el decofificador y tokenizacion.
        private DocumentoActual[] CargarTokenizador(string rutaCarpeta)
        {
            var documentos = new List<DocumentoActual>();
            string rutaCompleta = Path.GetFullPath(rutaCarpeta);

            if (!Directory.Exists(rutaCompleta))
            {
                return new DocumentoActual[0];
            }

            string[] archivos = Directory.GetFiles(rutaCompleta, "*.txt");

            foreach (string archivo in archivos)
            {
                try
                {
                    string nombre = Path.GetFileName(archivo);
                    string contenido = File.ReadAllText(archivo, Encoding.UTF8);
                    string url = Decodificador.DecodificarArchivo(nombre);

                    // Tokenizacion y filtrado de stopwords
                    string[] tokens = Tokenizador(contenido);

                    // Agregar al arreglo de documentos actuales[]
                    documentos.Add(new DocumentoActual(nombre, url, contenido, tokens));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return documentos.ToArray();
        }

        // Metodo para convertir secuencia de texto en partes mas pequeñas
        private string[] Tokenizador(string texto)
        {
            // Limpiar la lista circular de términos temporales (Metodo ICollection)
            terminosTemporales.Clear();

            string normalizar = Regex.Replace(texto.ToLower(), @"[^\w\s]", " ");

            // Dividir en tokens
            string[] tokens = normalizar.Split(new char[] { ' ', '\n' },
                StringSplitOptions.RemoveEmptyEntries);

            // Usar lista circular para almacenar terminos filtrados
            foreach (string token in tokens)
            {
                if (token.Length > 2 && !EsStopword(token))
                {
                    terminosTemporales.Add(token);
                }
            }

            // Convertir lista circular a array (Metodo ICollection)
            return terminosTemporales.ToArray();
        }

        private IndiceInvertido CrearIndiceInvertido(DocumentoActual[] documentos)
        {
            // Contenedor de toda la informacion
            var indice = new IndiceInvertido();

            for (int k = 0; k < documentos.Length; k++)
            {
                var documento = documentos[k];

                // Inicializacion de arreglos temporales
                string[] terminosUnicos = new string[documento.Tokens.Length];
                int[] frecuencias = new int[documento.Tokens.Length];
                int terminosCount = 0;

                // Recorre cada token en el documento actual
                foreach (string token in documento.Tokens)
                {
                    // Buscar si el término ya existe
                    int indiceTermino = -1;
                    for (int i = 0; i < terminosCount; i++)
                    {
                        // Busca si el token ya se proceso anteriormente en este documento
                        // Si lo encuentra, guarda su posición en indiceTermino
                        if (terminosUnicos[i] == token)
                        {
                            indiceTermino = i;
                            break;
                        }
                    }

                    // Si lo encuentra incrementa su frecuencia
                    // y si no estable nuevo con frecuencia 1
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

        // Metodo para calcular el IDF para cada término en
        // el índice invertido
        private Termino[] CalculoTF_IDF(IndiceInvertido index, int N)
        {
            // cantidad de terminos unicos en el indice (index.terminosCont)
            Termino[] terminosPreprocesados = new Termino[index.terminosCont];

            // Recorre cada termino unico en el indice para obtener los documentos
            // que contienen esa palabra
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
                    // Documento con frecuencia. Ejemplo ( "D1:5" ) 
                    // Esto se agrega al .JSON
                    docStrings[j] = $"D{documentos[j].DocId + 1}:{documentos[j].Frecuencia}";
                }

                string frecuencia = "";
                for (int k = 0; k < docStrings.Length; k++)
                {
                    if (k > 0)
                    {
                        frecuencia += ","; // Concatenar
                    }
                    frecuencia += docStrings[k];
                }

                // Creacion de objetos terminos para el arreglo
                terminosPreprocesados[i] = new Termino(palabra, df, idf, frecuencia);
            }

            return terminosPreprocesados;
        }

        // Metodo para filtrar el tamaño de un conjunto de datos. Obtiene los
        // terminos menos frecuentes para quedarse con los mas frecuentes
        private Termino[] AplicarLeyZipf(Termino[] terminos, double porc)
        {
            // Copiar el arreglo
            Termino[] copiaTerminos = new Termino[terminos.Length];
            for (int i = 0; i < terminos.Length; i++)
            {
                copiaTerminos[i] = terminos[i];
            }

            // Llamar al metodo de ordenamiento Radix
            OrdenamientoRadix.RadixSort(copiaTerminos);

            // Calcular cuantos terminos se mantendran
            int total = copiaTerminos.Length;
            int terminosMantenidos = (int)Math.Ceiling(total * (1.0 - porc / 100.0));

            // Devolver el arreglo de terminos mantenidos
            Termino[] terminosCalculados = new Termino[terminosMantenidos];
            for (int i = 0; i < terminosMantenidos; i++)
            {
                terminosCalculados[i] = copiaTerminos[i];
            }

            return terminosCalculados;
        }

        // Calcula los vectores TF-IDF para cada documento en la coleccion.
        // Convierte cada documento de texto en un vector numerico donde cada posicion
        // representa un termino del vocabulario y su valor es el peso TF-IDF.
        private Documento[] CalcularVectoresTFIDF(DocumentoActual[] documentos, Termino[] terminos)
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

                // Misma logica que en CrearIndiceInvertido()
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

                    // Calcular su tf * tf segun su frecuencia
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

                // Agregar documento a arreglo
                documentosPreprocesados[docId] = new Documento(
                    documento.Nombre,
                    documento.URL,
                    documento.Tokens.Length,
                    vector
                );
            }

            return documentosPreprocesados;
        }

        // Metodo para guardar el nuevo archivo con el indice
        // preprocesado en el archivo .JSON
        private void GuardarIndicePreprocesado()
        {
            try
            {
                string rutaArchivo = "../../../../indicePreprocesado.json";

                var opciones = new JsonSerializerOptions
                {
                    WriteIndented = true, // Saltos de linea en archivo
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // configura codificador
                };
                
                // Serializacion a json
                string json = JsonSerializer.Serialize(indiceActual, opciones);
                File.WriteAllText(rutaArchivo, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar indice, {ex.Message}");
            }
        }

        // Opcion 2 del menu para cargar el archivo .JSON con el contenido 
        // del indice preprocesado
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

                // Desearilizacion de json
                string json = File.ReadAllText(rutaArchivo, Encoding.UTF8);
                indiceActual = JsonSerializer.Deserialize<IndicePreprocesado>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar el indice: {ex.Message}");
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

                // Contar frecuencia del término en la consulta (TF)
                int tf = 0;
                for (int j = 0; j < tokensConsulta.Length; j++)
                {
                    if (tokensConsulta[j] == termino)
                    {
                        tf++;
                    }
                }

                // Calcular TF-IDF para la consulta (TF lineal)
                if (tf > 0)
                {
                    vectorConsulta[i] = tf * indiceActual.terminosDet[i].IDF;
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
            if (arr.Length != arr2.Length)
            {
                return 0.0;
            }

            double productoPunto = 0.0;
            double A = 0.0;
            double B = 0.0;

            for (int i = 0; i < arr.Length; i++)
            {
                productoPunto += arr[i] * arr2[i]; // (𝐴 ∗ 𝐵)
                A += arr[i] * arr[i];
                B += arr2[i] * arr2[i];
            }

            if (A == 0.0 || B == 0.0)
            {
                return 0.0;
            }

            //return productoPunto / Math.Sqrt(A) * Math.Sqrt(B);
            return productoPunto / Math.Sqrt(B);
        }

    }
}


