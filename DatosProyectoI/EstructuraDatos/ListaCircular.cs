using System.Collections;
using DatosProyectoI.Model;

namespace DatosProyectoI.EstructuraDatos
{
    internal class ListaCircular<T> : ICollection<T>
    {
        internal Nodo<T> cabeza;
        
        private int contador;
        
        public ListaCircular()
        {
            cabeza = null;    
            contador = 0;
        }

        // Metodos implementados de Libreria ICollection
        public int Count => contador;
        public bool IsReadOnly => false;
        
        public void Add(T item)
        {
            Nodo<T> nuevo = new Nodo<T>(item);

            if (cabeza == null)
            {
                cabeza = nuevo;
                cabeza.siguiente = cabeza;
                cabeza.anterior = cabeza;
            } 
            else
            {
                Nodo<T> aux = cabeza.anterior;
                aux.siguiente = nuevo;
                nuevo.anterior = aux;
                nuevo.siguiente = cabeza;
                cabeza.anterior = nuevo;
            }
            contador++;
        }

        public void Clear()
        {
            cabeza = null;
            contador = 0;
        }
        
        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex < contador) throw new ArgumentException("Array no tiene suficiente espacio");
            
            if (cabeza == null) return;
            
            Nodo<T> actual = cabeza;
            int index = arrayIndex;
            do
            {
                array[index] = actual.dato;
                actual = actual.siguiente;
                index++;
            } while (actual != cabeza);
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            if (cabeza == null) yield break;
            
            Nodo<T> actual = cabeza;
            do
            {
                yield return actual.dato;
                actual = actual.siguiente;
            } while (actual != cabeza);
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IIterator<T> GetIterator()
        {
            return new ListaCircularIterator<T>(this);
        }

        // Método para convertir a array
        public T[] ToArray()
        {
            T[] array = new T[contador];
            CopyTo(array, 0);
            return array;
        }
    }
}
