using DatosProyectoI.Model;

namespace DatosProyectoI.Model
{
    public class IndiceInvertido
    {
        public string[] Terminos { get; set; }
        public FrecuenciaDoc[][] DocumentosPorTermino { get; set; }
        public int terminosCont { get; set; }
        
        public IndiceInvertido()
        {
            Terminos = new string[10000]; // Array fijo para términos
            DocumentosPorTermino = new FrecuenciaDoc[10000][];
            terminosCont = 0;
        }

        // Agrega un término al índice con su frecuencia en un documento
        public void AgregarTermino(string termino, int docId, int frecuencia)
        {
            // Buscar si el término ya existe
            int indiceTermino = -1;
            for (int i = 0; i < terminosCont; i++)
            {
                if (Terminos[i] == termino)
                {
                    indiceTermino = i;
                    break;
                }
            }
            
            if (indiceTermino >= 0)
            {
                // Término existe, agregar documento
                if (DocumentosPorTermino[indiceTermino] == null)
                {
                    DocumentosPorTermino[indiceTermino] = new FrecuenciaDoc[1000];
                }
                
                // Buscar espacio libre en el array
                for (int i = 0; i < DocumentosPorTermino[indiceTermino].Length; i++)
                {
                    if (DocumentosPorTermino[indiceTermino][i] == null)
                    {
                        DocumentosPorTermino[indiceTermino][i] = new FrecuenciaDoc(docId, frecuencia);
                        break;
                    }
                }
            }
            else
            {
                // Nuevo término
                if (terminosCont < Terminos.Length)
                {
                    Terminos[terminosCont] = termino;
                    DocumentosPorTermino[terminosCont] = new FrecuenciaDoc[1000];
                    DocumentosPorTermino[terminosCont][0] = new FrecuenciaDoc(docId, frecuencia);
                    terminosCont++;
                }
            }
        }

        // Retorna todos los documentos que contienen un termino especifico
        public FrecuenciaDoc[] ObtenerDocumentos(string termino)
        {
            for (int i = 0; i < terminosCont; i++)
            {
                if (Terminos[i] == termino)
                {
                    if (DocumentosPorTermino[i] == null)
                    {
                        return new FrecuenciaDoc[0];
                    }
                    
                    // Contar documentos no nulos
                    int count = 0;
                    for (int j = 0; j < DocumentosPorTermino[i].Length; j++)
                    {
                        if (DocumentosPorTermino[i][j] != null)
                        {
                            count++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    // Crear array del tamaño exacto
                    FrecuenciaDoc[] resultado = new FrecuenciaDoc[count];
                    for (int j = 0; j < count; j++)
                    {
                        resultado[j] = DocumentosPorTermino[i][j];
                    }
                    
                    return resultado;
                }
            }
            return new FrecuenciaDoc[0];
        }
    }
}
