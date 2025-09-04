using System.Collections;
using DatosProyectoI.Model;

namespace DatosProyectoI.EstructuraDatos
{
    internal class ListaCircular<T> : ICollection<T>
    {
        private Nodo<T> cabeza;
        
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
            if (cabeza == null)
            {
                return false; 
            }

            Nodo<T> aux = cabeza;
            do
            {
                if (aux.dato.Equals(item))
                {
                    return true;
                }
                aux = aux.siguiente;
            } while (aux != cabeza);
            
            return false;
        }

        public bool Remove(T item)
        {
            if (cabeza == null)
            {
                return false;
            }

            Nodo<T> aux = cabeza;
            do
            {
                if (aux.dato.Equals(item))
                {
                    if (contador == 1)
                    {
                        cabeza = null;
                    }
                    else
                    {
                        aux.anterior.siguiente = aux.siguiente;
                        aux.siguiente.anterior = aux.anterior;

                        if (aux == cabeza)
                        {
                            cabeza = aux.siguiente;
                        }

                        contador--;
                        return true;
                    }
                }
                aux = aux.siguiente;
            } while (aux != cabeza);

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(T[] artray, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (cabeza == null)
            {
                yield break;
            }

            Nodo<T> actual = cabeza;
            do
            {
                yield return actual.dato;
                actual = actual.siguiente;
            } while (actual != cabeza);
        }

    }
}
