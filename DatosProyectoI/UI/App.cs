using DatosProyectoI.Services;

namespace DatosProyectoI.UI
{
    internal class App
    {
        public void IniciarAplicacion() 
        {
            Builder builder = Builder.getInstance();

            string rutaDocs = "../../../../Documentos2";
            builder.CargarDocumentos(rutaDocs);

            double porcentajeZipf = SolicitarPorcentaje();

            builder.ConstruirIndiceInvertido();

            builder.AplicarLeyZipf(porcentajeZipf);
            
            string consulta;
            do
            {
                Console.Write("\nIngresa tu consulta: ");
                consulta = Console.ReadLine();
                
                builder.Consultar(consulta);
            } while (consulta?.ToLower() != "salir") ;
        }

        private static double SolicitarPorcentaje()
        {
            double porcentaje;
            bool ent = false;

            do
            {
                Console.Write("Porcentaje para Ley de Zipf: ");
                string entrada = Console.ReadLine();

                if (double.TryParse(entrada, out porcentaje))
                {
                    if (porcentaje > 0 && porcentaje <= 100)
                    {
                        ent = true;
                    }
                    else
                    {
                        Console.WriteLine("El porcentaje debe estar entre 0 y 100.");
                    }
                }
            } while (!ent);

            return porcentaje;
        }
    }
}
