using DatosProyectoI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatosProyectoI.EstructuraDatos
{
    internal class ListaCircularIterator<T> : IIterator<T>
    {
        private ListaCircular<T> lista;
        private Nodo<T> actual;
        private bool inicial;

        public ListaCircularIterator(ListaCircular<T> lista)
        {
            this.lista = lista;
            this.actual = lista.cabeza;
            this.inicial = false;
        }

        public T Current => actual.dato;

        public bool HasNext()
        {
            return lista.Count > 0;
        }

        public T Next()
        {
            if (lista.Count == 0)
            {
                return default(T);
            }

            if (!inicial)
            {
                inicial = true;
                return actual.dato;
            }

            actual = actual.siguiente;
            return actual.dato;
        }

        public void Reset()
        {
            actual = lista.cabeza;
            inicial = false;
        }
    }
}
