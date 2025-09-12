using DatosProyectoI.Model;

namespace DatosProyectoI.Algoritmos
{
    internal class OrdenamientoRadix
    {
        // Funcion principal que ordena terminos[]
        // del tamano n usando Ordenamiento Radix
        public static void RadixSort(Termino[] terminos)
        {
            // Encontrar el numero maximo para conocer
            // el numero de digitos
            int maxFrecuencia = GetMax(terminos);

            // Ordenamiento de conteo para cada digito
            for (int exp = 1; maxFrecuencia / exp > 0; exp *= 10)
            {
                CountSort(terminos, exp);
            }

            // Invertir arreglo terminos en orden descendente
            // segun tenga frecuencia de mayor a menor 
            Array.Reverse(terminos);
        }

        // Funcion para obtener el maximo valor en arreglo
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

        //  Funcion para ordenar conteo de un arreglo
        //  de acuerdo a el digito representado por exp
        private static void CountSort(Termino[] terminos, int exp)
        {
            // Salida del arreglo
            int[] conteo = new int[10]; 
            Termino[] resultado = new Termino[terminos.Length];

            // Guarda numero de ocurrencias en conteo[]
            for (int i = 0; i < terminos.Length; i++)
            {
                int digito = (terminos[i].frecuenciaDocs / exp) % 10;
                conteo[digito]++;
            }

            // Cambia conteo[i] para que conteo[i] ahora
            // contenga la posicion actual del digito
            // en la salida[] 
            for (int i = 1; i < 10; i++)
            {
                conteo[i] += conteo[i - 1];
            }

            // Construir resultado del resultado[i]
            for (int i = terminos.Length - 1; i >= 0; i--)
            {
                int digito = (terminos[i].frecuenciaDocs / exp) % 10;
                resultado[conteo[digito] - 1] = terminos[i];
                conteo[digito]--;
            }

            // Copia la salida del arreglo a terminos[]
            // para que terminos[] contenga numberos
            // ordenados de acuerdo al digito actual
            for (int i = 0; i < terminos.Length; i++)
            {
                terminos[i] = resultado[i];
            }
        }

    }
}
