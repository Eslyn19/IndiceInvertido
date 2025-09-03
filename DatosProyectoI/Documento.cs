using System.Linq;

namespace DatosProyectoI
{
    internal class Documento
    {
        public string nombre { get; set; }
        public string contenido { get; set; }
        public string[] Tokens { get; set; }
        public int TotTerminos { get; set; }

        public Documento(string n, string c)
        {
            nombre = n;
            contenido = c;
            Tokens = new string[0];
            TotTerminos = 0;
        }

        // Preprocesamiento de los archivos de texto
        public void ProcesarDoc(string[] stopwords)
        {
            // Tokenización
            string[] palabras = contenido.ToLower().Split(new char[] { ' ', '.', ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            // eliminar stopwords
            Tokens = palabras
                .Select(palabra => palabra.Trim())
                .Where(palabra => !stopwords.Contains(palabra))
                .ToArray();

            TotTerminos = Tokens.Length;
        }

        // Obtener frecuencia de los terminos
        public int getFrecuenciaTerm(string termino)
        {
            int frecuencia = 0;
            foreach (string token in Tokens)
            {
                if (token.Equals(termino, StringComparison.OrdinalIgnoreCase))
                {
                    frecuencia++;
                }
            }
            return frecuencia;
        }
    }
}