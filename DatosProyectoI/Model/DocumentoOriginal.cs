namespace DatosProyectoI.Model
{
    /// <summary>
    /// Clase auxiliar para almacenar temporalmente los documentos durante el procesamiento inicial
    /// </summary>
    public class DocumentoOriginal
    {
        public string Nombre { get; set; }
        public string URL { get; set; }
        public string Contenido { get; set; }
        public string[] Tokens { get; set; }
        
        public DocumentoOriginal(string nombre, string url, string contenido, string[] tokens)
        {
            Nombre = nombre;
            URL = url;
            Contenido = contenido;
            Tokens = tokens;
        }
    }
}
