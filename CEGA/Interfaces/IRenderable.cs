using System;
using TgcViewer.Example;

namespace AlumnoEjemplos.CEGA
{
    interface IRenderable : IDisposable
    {
        /// <summary>
        /// Renderiza el objeto, controlando el post procesamiento
        /// </summary>
        /// TODO JJ: cambiar example por scene
        void Render(Snipers scene);

        /// <summary>
        /// Renderiza elementos de User Interface, para ser aplicados por
        /// encima del post-procesamiento.
        /// </summary>
        /// <param name="scene"></param>
        void RenderUI(Snipers scene);
    }
}
