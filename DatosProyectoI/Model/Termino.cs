using System.Text;

namespace DatosProyectoI.Model
{
    public class Termino
    {
        public string palabra { get; set; }        // Termino
        public int frecuenciaDocs { get; set; }    // frecuencia de documentos
        public double IDF { get; set; }            // Indice de rareza
        public string DocsFrecuencia { get; set; } // Documentos en frecuencia
        
        public Termino()
        {
            palabra = "";
            frecuenciaDocs = 0;
            IDF = 0.0;
            DocsFrecuencia = "";
        }
        
        public Termino(string p, int df, double idf, string d)
        {
            palabra = p;
            frecuenciaDocs = df;
            IDF = idf;
            DocsFrecuencia = d;
        }
    }
}
