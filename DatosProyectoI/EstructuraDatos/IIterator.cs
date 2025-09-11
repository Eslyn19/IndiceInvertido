using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatosProyectoI.EstructuraDatos
{
    internal interface IIterator<T>
    {
        T Current { get; }
        bool HasNext();
        T Next();
    }
}
