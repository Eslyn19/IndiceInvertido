using Microsoft.VisualBasic;

namespace DatosProyectoI.Model
{
    /* 
        Clase documento actual
        Clase auxiliar que se utiliza para mantener temporalmente la información de un documento durante el 
        procesamiento, antes de que sea convertido a la estructura final del índice.
    */ 
    public class DocumentoActual
    {
        public string Nombre { get; set; }
        public string URL { get; set; }
        public string Contenido { get; set; }
        public string[] Tokens { get; set; }
        
        public DocumentoActual(string n, string u, string c, string[] t)
        {
            Nombre = n;
            URL = u;
            Contenido = c;
            Tokens = t;
        }
    }
}
