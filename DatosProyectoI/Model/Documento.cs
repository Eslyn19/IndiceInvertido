using System.Text;

namespace DatosProyectoI.Model
{
    public class Documento
    {
        public string nombre { get; set; }   // Nombre del documento
        public string URL { get; set; }      // URL del documento (ruta)
        public int terminos { get; set; }    // Total de terminos por documento
        public double[] arrIDF { get; set; } // Arreglo para el indice de rareza
        
        public Documento()
        {
            nombre = "";
            URL = "";
            terminos = 0;
            arrIDF = new double[0];
        }
        
        public Documento(string n, string url, int t, double[] arr)
        {
            this.nombre = n;
            URL = url;
            terminos = t;
            arrIDF = arr;
        }
    }
}
