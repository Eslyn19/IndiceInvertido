namespace DatosProyectoI
{
    internal class App
    {
        public void IniciarAplicacion() 
        { 
            Builder builder = new Builder();

            string rutaDocumentos = "../../../../Documentos2";
            builder.CargarDocumentos(rutaDocumentos);

            double porcentajeZipf = SolicitarPorcentaje();

            builder.ConstruirIndiceInvertido();

            builder.AplicarLeyZipf(porcentajeZipf);
            
            string consulta;
            do
            {
                Console.Write("\nIngresa tu consulta: ");
                consulta = Console.ReadLine();
                
                if (!string.IsNullOrWhiteSpace(consulta) && consulta.ToLower() != "salir")
                {
                    builder.Consultar(consulta);
                }
            } while (consulta?.ToLower() != "salir") ;
        }

        private static double SolicitarPorcentaje()
        {
            double porcentaje;
            bool entradaValida = false;

            do
            {
                Console.Write("Porcentaje para Ley de Zipf: ");
                string entrada = Console.ReadLine();

                if (double.TryParse(entrada, out porcentaje))
                {
                    if (porcentaje > 0 && porcentaje <= 100)
                    {
                        entradaValida = true;
                    }
                    else
                    {
                        Console.WriteLine("El porcentaje debe estar entre 0 y 100.");
                    }
                }
                else
                {
                    Console.WriteLine("Por favor, ingrese un número válido.");
                }
            } while (!entradaValida);

            return porcentaje;
        }
    }
}
