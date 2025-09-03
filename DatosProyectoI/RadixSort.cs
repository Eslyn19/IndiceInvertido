namespace DatosProyectoI
{
    internal class RadixSort
    {
        public static void OrdenarTerminos(Termino[] terminos)
        {
            if (terminos == null || terminos.Length <= 1)
                return;

            // Encontrar la longitud máxima de las palabras
            int maxLength = 0;
            foreach (var termino in terminos)
            {
                if (termino.palabra.Length > maxLength)
                    maxLength = termino.palabra.Length;
            }

            // Aplicar Radix Sort desde el dígito menos significativo (último carácter)
            for (int pos = maxLength - 1; pos >= 0; pos--)
            {
                OrdenarPorPosicion(terminos, pos);
            }
        }

        private static void OrdenarPorPosicion(Termino[] terminos, int posicion)
        {
            // Array para contar la frecuencia de cada carácter
            int[] conteo = new int[256]; // ASCII completo
            Termino[] resultado = new Termino[terminos.Length];

            // Contar la frecuencia de cada carácter en la posición dada
            foreach (var termino in terminos)
            {
                char caracter = posicion < termino.palabra.Length ? 
                    termino.palabra[posicion] : '\0';
                conteo[caracter]++;
            }

            // Modificar conteo para que contenga las posiciones reales
            for (int i = 1; i < 256; i++)
            {
                conteo[i] += conteo[i - 1];
            }

            // Construir el array resultado
            for (int i = terminos.Length - 1; i >= 0; i--)
            {
                char caracter = posicion < terminos[i].palabra.Length ? 
                    terminos[i].palabra[posicion] : '\0';
                resultado[conteo[caracter] - 1] = terminos[i];
                conteo[caracter]--;
            }

            // Copiar el resultado de vuelta al array original
            for (int i = 0; i < terminos.Length; i++)
            {
                terminos[i] = resultado[i];
            }
        }

        public static void OrdenarPorFrecuencia(Termino[] terminos)
        {
            if (terminos == null || terminos.Length <= 1)
                return;

            // Encontrar la frecuencia máxima
            int maxFrecuencia = 0;
            foreach (var termino in terminos)
            {
                if (termino.frecuencia > maxFrecuencia)
                    maxFrecuencia = termino.frecuencia;
            }

            // Aplicar Radix Sort por frecuencia
            for (int exp = 1; maxFrecuencia / exp > 0; exp *= 10)
            {
                OrdenarPorDigito(terminos, exp);
            }

            // Invertir para tener orden descendente (mayor frecuencia primero)
            Array.Reverse(terminos);
        }

        private static void OrdenarPorDigito(Termino[] terminos, int exp)
        {
            int[] conteo = new int[10];
            Termino[] resultado = new Termino[terminos.Length];

            // Contar la frecuencia de cada dígito
            foreach (var termino in terminos)
            {
                int digito = (termino.frecuencia / exp) % 10;
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
                int digito = (terminos[i].frecuencia / exp) % 10;
                resultado[conteo[digito] - 1] = terminos[i];
                conteo[digito]--;
            }

            // Copiar resultado
            for (int i = 0; i < terminos.Length; i++)
            {
                terminos[i] = resultado[i];
            }
        }
    }
}
