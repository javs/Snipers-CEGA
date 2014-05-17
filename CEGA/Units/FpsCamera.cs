
using Microsoft.DirectX;
using TgcViewer.Utils.Input;

namespace AlumnoEjemplos.CEGA.Units
{
    /// <summary>
    /// Controla una camara en primera persona.
    /// </summary>
    class FpsCamera : TgcCamera
    {
        public Vector3 getPosition()
        {
            return new Vector3();
        }

        public Vector3 getLookAt()
        {
            return new Vector3();
        }

        /// <summary>
        /// Actualizar el estado interno de la cámara en cada frame
        /// </summary>
        public void updateCamera()
        {

        }

        /// <summary>
        /// Actualizar la matriz View en base a los valores de la cámara
        /// </summary>
        public void updateViewMatrix(Microsoft.DirectX.Direct3D.Device d3dDevice)
        {

        }

        public bool Enable { get; set; }
    }
}
