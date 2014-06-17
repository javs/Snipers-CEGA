using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlumnoEjemplos.CEGA.Interfaces
{
    /// <summary>
    /// Establece que un objeto debe procesar updates.
    /// </summary>
    interface IUpdatable
    {
        void Update(float elapsedTime);
    }
}
