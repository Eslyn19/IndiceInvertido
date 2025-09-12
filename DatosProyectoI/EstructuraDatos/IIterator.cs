using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatosProyectoI.EstructuraDatos
{
    // Interfaz del patron iterador
    internal interface IIterator<T>
    {
        T Actual { get; }
        T Next();
        bool HasNext();

    }
}
