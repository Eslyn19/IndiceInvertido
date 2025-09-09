using DatosProyectoI.Model;

namespace DatosProyectoI.Model
{
    /// <summary>
    /// Clase para construir el índice invertido usando solo arrays (sin Dictionary/List)
    /// </summary>
    public class IndiceInvertidoBasico
    {
        public string[] Terminos { get; set; }
        public DocumentoConFrecuencia[][] DocumentosPorTermino { get; set; }
        public int TerminosCount { get; set; }
        
        public IndiceInvertidoBasico()
        {
            Terminos = new string[10000]; // Array fijo para términos
            DocumentosPorTermino = new DocumentoConFrecuencia[10000][];
            TerminosCount = 0;
        }
        
        public void AgregarTermino(string termino, int docId, int frecuencia)
        {
            // Buscar si el término ya existe
            int indiceTermino = -1;
            for (int i = 0; i < TerminosCount; i++)
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
                    DocumentosPorTermino[indiceTermino] = new DocumentoConFrecuencia[1000];
                }
                
                // Buscar espacio libre en el array
                for (int i = 0; i < DocumentosPorTermino[indiceTermino].Length; i++)
                {
                    if (DocumentosPorTermino[indiceTermino][i] == null)
                    {
                        DocumentosPorTermino[indiceTermino][i] = new DocumentoConFrecuencia(docId, frecuencia);
                        break;
                    }
                }
            }
            else
            {
                // Nuevo término
                if (TerminosCount < Terminos.Length)
                {
                    Terminos[TerminosCount] = termino;
                    DocumentosPorTermino[TerminosCount] = new DocumentoConFrecuencia[1000];
                    DocumentosPorTermino[TerminosCount][0] = new DocumentoConFrecuencia(docId, frecuencia);
                    TerminosCount++;
                }
            }
        }
        
        public DocumentoConFrecuencia[] ObtenerDocumentos(string termino)
        {
            for (int i = 0; i < TerminosCount; i++)
            {
                if (Terminos[i] == termino)
                {
                    if (DocumentosPorTermino[i] == null) return new DocumentoConFrecuencia[0];
                    
                    // Contar documentos no nulos
                    int count = 0;
                    for (int j = 0; j < DocumentosPorTermino[i].Length; j++)
                    {
                        if (DocumentosPorTermino[i][j] != null) count++;
                        else break;
                    }
                    
                    // Crear array del tamaño exacto
                    DocumentoConFrecuencia[] resultado = new DocumentoConFrecuencia[count];
                    for (int j = 0; j < count; j++)
                    {
                        resultado[j] = DocumentosPorTermino[i][j];
                    }
                    
                    return resultado;
                }
            }
            return new DocumentoConFrecuencia[0];
        }
    }
}
