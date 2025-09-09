namespace DatosProyectoI.Model
{
    /// <summary>
    /// Clase auxiliar para almacenar la relación entre un documento y su frecuencia para un término específico
    /// </summary>
    public class DocumentoConFrecuencia
    {
        public int DocId { get; set; }
        public int Frecuencia { get; set; }
        
        public DocumentoConFrecuencia(int docId, int frecuencia)
        {
            DocId = docId;
            Frecuencia = frecuencia;
        }
    }
}
