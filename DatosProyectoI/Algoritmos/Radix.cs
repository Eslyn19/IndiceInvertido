using DatosProyectoI.Model;

namespace DatosProyectoI.Algoritmos
{
    internal class OrdenamientoRadix
    {
        public static void OrdenarPorFrecuencia(Termino[] terminos)
        {
            if (terminos == null || terminos.Length <= 1)
            {
                return; // si es vacio
            }

            // Encontrar la frecuencia maxima
            int mx = 0;
            for (int i = 0; i < terminos.Length; i++)
            {
                if (terminos[i].frecuencia > mx)
                    mx = terminos[i].frecuencia;
            }

            // Aplicar ordenamiento radix
            for (int exp = 1; mx / exp > 0; exp *= 10)
            {
                OrdenarPorDigito(terminos, exp);
            }

            // Invertir orden en ascendente
            Array.Reverse(terminos);
        }

        private static void OrdenarPorDigito(Termino[] terminos, int exp)
        {
            int[] conteo = new int[10]; // Digitos posibles 0-9
            Termino[] resultado = new Termino[terminos.Length];

            // Contar la frecuencia de cada digito
            foreach (var t in terminos)
            {
                int digito = (t.frecuencia / exp) % 10;
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
    
            // Pasa los elementos ordenados a el arreglo original
            for (int i = 0; i < terminos.Length; i++)
            {
                terminos[i] = resultado[i];
            }
        }
    }
}
