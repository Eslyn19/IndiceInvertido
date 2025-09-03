namespace DatosProyectoI
{
    internal class Termino
    {
        public string palabra { get; set; } // termino
        public int frecuencia { get; set; } // frecuencia por documento
        public int DocumentosCont { get; set; } // contenedor documentos
        public ListaCircular<Documento> Documentos { get; set; } // lista de documentos
        public double IDF { get; set; } // indice de rareza

        public Termino(string _palabra)
        {
            palabra =_palabra;
            frecuencia = 0;
            DocumentosCont = 0;
            Documentos = new ListaCircular<Documento>();
            IDF = 0.0;
        }

        // Agregar documento a la lista circular de documentos
        public void AgregarDoc(Documento doc, int _frecuencia)
        {
            if (!Documentos.Contains(doc))
            {
                Documentos.Add(doc);
                DocumentosCont++;
            }
            frecuencia += _frecuencia;
        }

        public void CalcularIDF(int N)
        {
            if (DocumentosCont > 0)
            {
                IDF = Math.Log10((double)N / DocumentosCont);
            }
        }

        public double CalcularTFIDF(Documento doc)
        {
            int tf = doc.getFrecuenciaTerm(palabra);
            if (tf > 0)
            {
                return tf * IDF;
            }
            return 0.0;
        }
    }
}
