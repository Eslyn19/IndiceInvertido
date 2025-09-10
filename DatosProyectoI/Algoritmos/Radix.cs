using DatosProyectoI.Model;

namespace DatosProyectoI.Algoritmos
{
    internal class OrdenamientoRadix
    {
        public static void RadixSort(Termino[] terminos)
        {
            int maxFrecuencia = GetMax(terminos);

            for (int exp = 1; maxFrecuencia / exp > 0; exp *= 10)
            {
                CountSort(terminos, exp);
            }

            // Invertir para obtener orden descendente (mayor a menor frecuencia)
            Array.Reverse(terminos);
        }

        private static int GetMax(Termino[] terminos)
        {
            int mx = terminos[0].frecuenciaDocs;
            for (int i = 1; i < terminos.Length; i++)
            {
                if (terminos[i].frecuenciaDocs > mx)
                    mx = terminos[i].frecuenciaDocs;
            }
            return mx;
        }

        private static void CountSort(Termino[] terminos, int exp)
        {
            int[] conteo = new int[10]; // Digitos posibles 0-9
            Termino[] resultado = new Termino[terminos.Length];

            // Contar la frecuencia de cada digito
            for (int i = 0; i < terminos.Length; i++)
            {
                int digito = (terminos[i].frecuenciaDocs / exp) % 10;
                conteo[digito]++;
            }

            // Modificar conteo para posiciones reales
            for (int i = 1; i < 10; i++)
            {
                conteo[i] += conteo[i - 1];
            }

            // Construir resultado
            for (int i = terminos.Length - 1; i >= 0; i--)
            {
                int digito = (terminos[i].frecuenciaDocs / exp) % 10;
                resultado[conteo[digito] - 1] = terminos[i];
                conteo[digito]--;
            }

            // Copiar los elementos ordenados al array original
            for (int i = 0; i < terminos.Length; i++)
            {
                terminos[i] = resultado[i];
            }
        }
    }
}
