using DatosProyectoI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DatosProyectoI.EstructuraDatos
{
    internal class ListaCircularIterator<T> : IIterator<T>
    {
        private ListaCircular<T> lista;
        private Nodo<T> actual;
        private bool inicial; // bandera para devolver primer elemento

        public ListaCircularIterator(ListaCircular<T> lista)
        {
            this.lista = lista;
            this.actual = lista.cabeza;
            this.inicial = false;
        }

        // Devuelve el nodo actual del iterador
        public T Actual => actual.dato;

        // Comprueba si hay siguiente elemento en lista
        public bool HasNext() { return lista.Count > 0; }

        public T Next()
        {
            // Si vacia
            if (lista.Count == 0)
            {
                return default;
            }

            // Devuelve el dato del nodo cabeza sin avanzar el puntero actual.
            if (!inicial)
            {
                inicial = true;
                return actual.dato;
            }

            // Avanzar al siguiente y retornar dato
            actual = actual.siguiente;
            return actual.dato;
        }

    }
}
