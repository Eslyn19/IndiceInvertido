namespace DatosProyectoI.Model
{
    /*
        Almacena la frecuencia de un término en un documento específico
        Esta clase es utilizada en el índice invertido para almacenar la información de frecuencia de 
        términos por documento.
     */
    public class FrecuenciaDoc
    {
        public int DocId { get; set; }
        public int Frecuencia { get; set; }
        
        public FrecuenciaDoc(int d, int f)
        {
            DocId = d;
            Frecuencia = f;
        }
    }
}
