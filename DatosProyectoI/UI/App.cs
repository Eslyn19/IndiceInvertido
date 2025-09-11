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
            bool continuar = true;
            
            while (continuar)
            {
                Console.Clear();
                Console.WriteLine("=== SISTEMA DE BÚSQUEDA CON ÍNDICE PREPROCESADO ===");
                Console.WriteLine();
                Console.WriteLine("Opciones:");
                Console.WriteLine("1. Procesar documentos y crear índice preprocesado");
                Console.WriteLine("2. Cargar índice preprocesado existente");
                Console.WriteLine("3. Realizar consulta");
                Console.WriteLine("0. Salir");
                Console.WriteLine();
                Console.Write("Seleccione una opción: ");
                
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
                        continuar = false;
                        break;
                    default:
                        Console.WriteLine("Opcion invalida");
                        Console.ReadLine();
                        break;
                }
            }
        }
        
        private void OpcionProcesarDocumentos()
        {
            Console.Clear();
            Console.WriteLine("=== PROCESAR DOCUMENTOS ===");
            Console.WriteLine();
            
            string rutaDocs = builder.CrearDesdeRuta();
            Console.WriteLine($"Ruta de documentos: {rutaDocs}");
            
            Console.WriteLine();
            Console.Write("Ingrese el porcentaje de reducción con la ley de Zipf (0-40): ");
            string porcentajeStr = Console.ReadLine();
            
            if (!double.TryParse(porcentajeStr, out double porcentaje) || porcentaje < 0 || porcentaje > 40)
            {
                Console.WriteLine("Porcentaje inválido. Usando 20% por defecto.");
                porcentaje = 20.0;
            }
            
            Console.WriteLine();
            Console.WriteLine("Procesando documentos...");
            
            builder.ProcesarDocumentos(rutaDocs, porcentaje);
            
            Console.WriteLine();
            Console.WriteLine("Presione Enter para continuar...");
            Console.ReadLine();
        }
        
        private void OpcionCargarIndice()
        {
            Console.Clear();
            Console.WriteLine("=== CARGAR ÍNDICE PREPROCESADO ===");
            Console.WriteLine();
            
            Console.WriteLine("Cargando índice preprocesado...");
            builder.CargarIndicePreprocesado();
            
            Console.WriteLine();
            Console.WriteLine("Presione Enter para continuar...");
            Console.ReadLine();
        }
        
        private void OpcionConsulta()
        {
            bool continuar = true;
            
            while (continuar)
            {
                Console.Clear();
                Console.WriteLine("=== REALIZAR CONSULTA ===");
                Console.WriteLine("Escriba 'SALIR' para volver al menú principal");
                Console.WriteLine();
                
                Console.Write("Ingrese su consulta: ");
                string consulta = Console.ReadLine();
                
                if (consulta?.ToUpper() == "SALIR")
                {
                    continuar = false;
                }
                else if (!string.IsNullOrWhiteSpace(consulta))
                {
                    builder.Consultar(consulta);
                    
                    Console.WriteLine();
                    Console.WriteLine("Presione Enter para hacer otra consulta...");
                    Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("Consulta inválida. Presione Enter para continuar...");
                    Console.ReadLine();
                }
            }
        }
        
    }
}
