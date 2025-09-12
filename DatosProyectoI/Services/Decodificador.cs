using System.Text;

namespace DatosProyectoI.Services
{
    internal class Decodificador
    {
        public static string DecodificarArchivo(string archivo)
        {
            try
            {
                string nuevaRuta = archivo;
                if (nuevaRuta.EndsWith(".txt"))
                {
                    // Extrae los ultimos 4 digitos -> .txt
                    nuevaRuta = nuevaRuta.Substring(0, nuevaRuta.Length - 4);
                }

                byte[] getBytes = Convert.FromBase64String(nuevaRuta);
                string nuevaURL = Encoding.UTF8.GetString(getBytes);
                
                return nuevaURL;
            }
            catch (Exception ex)
            {
                return $"Error al decodificar: {ex.Message}";
            }
        }
    }
}