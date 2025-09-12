using DatosProyectoI.Model;
using DatosProyectoI.Services;
using System.Text;
using System.Linq;

namespace DatosProyectoI.UI
{
    internal class App
    {
        private Builder builder;

        public App()
        {
            builder = Builder.getInstance();
        }

        public void IniciarAplicacion()
        {
            MostrarMenu();
        }

        private void MostrarMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Estructura de datos: índice invertido\n");
                Console.WriteLine("1. Crear indice invertido");
                Console.WriteLine("2. Cargar indice preprocesado existente");
                Console.WriteLine("3. Realizar consulta\n");
                Console.Write("Opcion: ");

                string opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        OpcionProcesarDocumentos();
                        break;
                    case "2":
                        OpcionCargarIndice();
                        break;
                    case "3":
                        OpcionConsulta();
                        break;
                    case "0":
                        break;
                    default:
                        break;
                }
            }
        }

        private void OpcionProcesarDocumentos()
        {
            Console.Clear();
            Console.WriteLine("Procesando documentos");

            string rutaDocs = builder.CrearDesdeRuta();
            Console.WriteLine($"Ruta de documentos: {rutaDocs}");

            double porcentaje = 0.0;
            Console.Write("\nDigite un porcentaje para la Ley de Zipf (0% - 40%): ");
            try
            {
                porcentaje = Convert.ToDouble(Console.ReadLine());
                if (porcentaje < 0 || porcentaje > 40)
                {
                    porcentaje = 10.0; // Dar por defecto
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("\nProcesando documentos...");

            builder.ProcesarDocumentos(rutaDocs, porcentaje);

            Console.WriteLine("\nIndice creado correctamente!");
            Console.ReadLine();
        }

        private void OpcionCargarIndice()
        {
            Console.Clear();
            Console.WriteLine("Cargando indice preprocesado existente...\n");

            try
            {
                builder.CargarIndicePreprocesado();
                Console.WriteLine("Cargado correctamente!");
                Console.ReadLine();
            } 
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void OpcionConsulta()
        {
            while (true)
            {
                Console.Clear();
                Console.Write("Buscar consulta: ");
                string consulta = Console.ReadLine();

                if (consulta == "SALIR")
                {
                    break;
                }
                else
                {
                    builder.Consultar(consulta);
                    Console.ReadLine();
                }
            }
        }

    }
}
