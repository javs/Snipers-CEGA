
using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using TgcViewer;
using TgcViewer.Utils.Input;

namespace AlumnoEjemplos.CEGA.Units
{
    /// <summary>
    /// Controla una camara en primera persona.
    /// </summary>
    class FpsCamera : TgcCamera
    {
        /// <summary>
        /// Hacia donde es X, desde la perspectiva de la camara.
        /// </summary>
        Vector3 xAxis;

        /// <summary>
        /// Hacia donde es Y, desde la perspectiva de la camara.
        /// </summary>
        Vector3 yAxis;

        /// <summary>
        /// Hacia donde es Z, desde la perspectiva de la camara.
        /// </summary>
        Vector3 zAxis;

        /// <summary>
        /// Hacia adonde es "adelante" (Z+ desde la perspectiva del que camina).
        /// </summary>
        Vector3 forward;

        /// <summary>
        /// Hacia donde mira la camara, desde los ejes del mundo.
        /// </summary>
        Vector3 target;

        /// <summary>
        /// Donde esta la camara, desde los ejes del mundo.
        /// </summary>
        Vector3 eye;

        /// <summary>
        /// Hacia donde es arriba, desde los ejes del mundo.
        /// </summary>
        readonly Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);

        /// <summary>
        /// Matriz de vista
        /// </summary>
        Matrix vM;

        /// <summary>
        /// true si la posicion cambio desde el ultimo render.
        /// </summary>
        bool positionChanged;

        /// <summary>
        /// true si la direccion de vista cambio desde el ultimo render.
        /// </summary>
        bool rotationChanged;

        public float MovementSpeed { get; set; }

        /// <summary>
        /// Cuanto mas rapido puede ir cuando camina hacia adelante.
        /// </summary>
        public float ForwardFactor { get; set; }

        public float RotationSpeed { get; set; }

        public FpsCamera()
        {
            positionChanged = true;
            rotationChanged = true;

            vM = Matrix.Identity;

            target  = new Vector3(0.0f, 5.0f, 1.0f);
            eye     = new Vector3(0.0f, 5.0f, 0.0f);

            xAxis   = new Vector3();
            yAxis   = new Vector3();
            zAxis   = new Vector3();
            forward = new Vector3();

            MovementSpeed = 25.0f;
            ForwardFactor = 1.5f;
            RotationSpeed = 0.2f;

            Enable = true;
        }

        public Vector3 getPosition()
        {
            return eye;
        }

        /// <returns>
        /// Retorna el versor donde mira la camara, en relacion al mundo.
        /// </returns>
        public Vector3 getLookAt()
        {
            return target;
        }

        /// <summary>
        /// Actualizar el estado interno de la cámara en cada frame
        /// </summary>
        public void updateCamera()
        {
            if (!Enable)
                return;

            float elapsedTime = GuiController.Instance.ElapsedTime;
            TgcD3dInput input = GuiController.Instance.D3dInput;

            Vector3 movement = new Vector3(0.0f, 0.0f, 0.0f);

            if (input.keyDown(Key.W))
                move(forward * (MovementSpeed * ForwardFactor * elapsedTime));

            if (input.keyDown(Key.A))
                move(xAxis * (- MovementSpeed * elapsedTime));

            if (input.keyDown(Key.S))
                move(forward * (-MovementSpeed * elapsedTime));

            if (input.keyDown(Key.D))
                move(xAxis * (MovementSpeed * elapsedTime));

            float rotX = input.XposRelative * RotationSpeed;
            float rotY = input.YposRelative * RotationSpeed;
            
            if (rotY != 0.0f || rotX != 0.0f)
                look(rotX, rotY);
        }

        /// <summary>
        /// Rota en los deltas indicados.
        /// </summary>
        /// <param name="rotX"></param>
        /// <param name="rotY"></param>
        private void look(float rotX, float rotY)
        {
            // code me
        }

        /// <summary>
        /// Actualizar la matriz View en base a los valores de la cámara
        /// </summary>
        public void updateViewMatrix(Microsoft.DirectX.Direct3D.Device d3dDevice)
        {
            if (!Enable)
               return;

            rebuildViewMatrix();

            d3dDevice.Transform.View = vM;
        }

        void move(Vector3 delta)
        {
            eye    += delta;
            target += delta;

            positionChanged = true;
        }

        /// <summary>
        /// Actualiza la matriz de vista, solo si es necesario.
        /// </summary>
        void rebuildViewMatrix()
        {
            if (rotationChanged)
                goto Rotation;
            else if (positionChanged)
                goto Position;
            else
                return;
            
        Rotation:
            zAxis = target - eye;
            zAxis.Normalize();

            xAxis = Vector3.Cross(up, zAxis);
            xAxis.Normalize();

            yAxis = Vector3.Cross(zAxis, xAxis);
            yAxis.Normalize();

            forward = Vector3.Cross(xAxis, up);
            forward.Normalize();

            vM.M11 = xAxis.X;   vM.M12 = yAxis.X;   vM.M13 = zAxis.X; // (1,4) = 0
            vM.M21 = xAxis.Y;   vM.M22 = yAxis.Y;   vM.M23 = zAxis.Y; // (2,4) = 0
            vM.M31 = xAxis.Z;   vM.M32 = yAxis.Z;   vM.M33 = zAxis.Z; // (3,4) = 0

            rotationChanged = false;

        Position:
            vM.M41 = -Vector3.Dot(xAxis, eye);
            vM.M42 = -Vector3.Dot(yAxis, eye);
            vM.M43 = -Vector3.Dot(zAxis, eye);
            // (4,4) = 1

            positionChanged = false;
        }

        public bool Enable { get; set; }
    }
}
