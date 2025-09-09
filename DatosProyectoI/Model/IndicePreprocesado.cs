using System.Text;

namespace DatosProyectoI.Model
{
    public class IndicePreprocesado
    {
        public int totalDocs { get; set; }          // Total de documentos preprocesados
        public int totalTerms { get; set; }         // Total de terminos unicos en documento 
        public double porcZipf { get; set; }        // Porcentaje de zifp a aplicar
        public string[] terminos { get; set; }      // Arreglo de terminos
        public Termino[] terminosDet { get; set; }  // Arreglo informatico detallado de terminos
        public Documento[] documentos { get; set; } // Documentos procesados
        
        public IndicePreprocesado()
        {
            totalDocs = 0;
            totalTerms = 0;
            porcZipf = 0.0;
            terminos = new string[0];
            terminosDet = new Termino[0];
            documentos = new Documento[0];
        }
        
        public IndicePreprocesado(int tD, int tT, double Z, string[] t, Termino[] tDo, Documento[] docs)
        {
            totalDocs = tD;
            totalTerms = tT;
            porcZipf = Z;
            terminos = t;
            terminosDet = tDo;
            documentos = docs;
        }
    }
}
