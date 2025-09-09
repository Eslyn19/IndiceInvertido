using System.Text;

namespace DatosProyectoI.Services
{
    internal class Decodificador
    {
        public static string DecodificarArchivo(string file)
        {
            try
            {
                string nuevaRuta = file;
                if (nuevaRuta.EndsWith(".txt"))
                {
                    nuevaRuta = nuevaRuta.Substring(0, nuevaRuta.Length - 4);
                }

                byte[] Bytes = Convert.FromBase64String(nuevaRuta);
                string nuevaURL = Encoding.UTF8.GetString(Bytes);
                
                return nuevaURL;
            }
            catch (Exception ex)
            {
                return $"Error al decodificar: {ex.Message}";
            }
        }
    }
}