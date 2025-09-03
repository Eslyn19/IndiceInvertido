namespace DatosProyectoI.Model
{
    internal class Nodo<T>
    {
        public T dato { set; get; }
        public Nodo<T> anterior { set; get; }
        public Nodo<T> siguiente { set; get; }

        public Nodo(T _dato)
        {
            this.dato = _dato;
            anterior = null;
            siguiente = null;
        }
    }
}
