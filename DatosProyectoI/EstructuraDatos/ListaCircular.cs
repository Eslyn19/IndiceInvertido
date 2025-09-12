using System.Collections;
using DatosProyectoI.Model;

namespace DatosProyectoI.EstructuraDatos
{
    // Lista circular doblemente enlazada
    internal class ListaCircular<T> : ICollection<T>
    {
        internal Nodo<T> cabeza;
        
        private int contador; // Numero total de elementos en la lista
        
        public ListaCircular()
        {
            cabeza = null;    
            contador = 0;
        }

        // Metodos implementados de Libreria ICollection 
        public int Count => contador; // propiedad de lectura
        public bool IsReadOnly => false; // cuerpo de lectura
        
        // Añadr elementos a la lista
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

        // vaciar lista
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

        // Copia los elementos de la lista en un array de tipo T
        // a partir de un indice especifico.
        public void CopyTo(T[] array, int IndiceArreglo)
        {
            if (cabeza == null)
            {
                return;
            }
            else
            {
                Nodo<T> actual = cabeza;
                int i = IndiceArreglo;
                do
                {
                    array[i] = actual.dato;
                    actual = actual.siguiente;
                    i++;
                } while (actual != cabeza);
            }
        }
        
        // Metodo para que la lista sea iterable
        public IEnumerator<T> GetEnumerator()
        {
            // *yield* devuelve cada elemento uno por uno sin
            // necesidad de crear una nueva colección temporal.
            if (cabeza == null)
            {
                yield break;
            }
            else
            {

                Nodo<T> actual = cabeza;
                do
                {
                    yield return actual.dato;
                    actual = actual.siguiente;
                } while (actual != cabeza);
            }
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IIterator<T> GetIterator()
        {
            return new ListaCircularIterator<T>(this);
        }

        // Convierte la lista en array
        public T[] ToArray()
        {
            T[] arreglo = new T[contador];
            CopyTo(arreglo, 0);
            return arreglo;
        }
    }
}
