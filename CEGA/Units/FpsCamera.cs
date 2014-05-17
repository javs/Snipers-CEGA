
using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using System.Drawing;
using System.Windows.Forms;
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
        /// Matriz de rotacion absoluta.
        /// </summary>
        Matrix rM;

        /// <summary>
        /// Retorna la matriz de rotacion absoluta.
        /// </summary>
        public Matrix RotationMatrix { get { return rM; } }

        /// <summary>
        /// Matriz de traslacion absoluta.
        /// </summary>
        Matrix tM;

        /// <summary>
        /// Retorna la matriz de rotacion absoluta.
        /// </summary>
        public Matrix TranslationMatrix { get { return tM; } }

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
        /// Cuánto mas rapido puede ir cuando camina hacia adelante.
        /// </summary>
        public float ForwardFactor { get; set; }

        /// <summary>
        /// Velocidad de rotacion.
        /// </summary>
        public float RotationSpeed { get; set; }

        /// <summary>
        /// true si el mouse esta actualmente capturado por la camara.
        /// </summary>
        private bool lockMouse;

        /// <summary>
        /// Centro de la ventana actual, en coordenadas de la pantalla.
        /// </summary>
        private Point windowCenter;

        /// <summary>
        /// Controla la captura del mouse.
        /// </summary>
        public bool LockMouse
        {
            set
            {
                lockMouse = value;

                if (lockMouse)
                    Cursor.Hide();
                else
                    Cursor.Show();
            }

            get { return lockMouse; }
        }

        public FpsCamera()
        {
            positionChanged = true;
            rotationChanged = true;

            target  = new Vector3(0.0f, 5.0f, 1.0f);
            eye     = new Vector3(0.0f, 5.0f, 0.0f);

            vM = Matrix.Identity;
            rM = Matrix.Identity;
            tM = Matrix.Identity;

            xAxis   = new Vector3();
            yAxis   = new Vector3();
            zAxis   = new Vector3();
            forward = new Vector3();

            MovementSpeed = 25.0f;
            ForwardFactor = 1.5f;
            RotationSpeed = 0.05f;

            Control window =
                GuiController.Instance.D3dDevice.CreationParameters.FocusWindow;

            windowCenter = window.PointToScreen(
                new Point(window.Width / 2, window.Height / 2));

            lockMouse = false;

            Enable = true;

            setCamera(eye, target);
        }

        public Vector3 getPosition()
        {
            return eye;
        }

        /// <returns>
        /// Retorna el vector donde mira la camara (a una distancia
        /// de 1.0f del ojo), en relacion al mundo.
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

            // posicion
            //
            bool moved = false;
            Vector3 movement = new Vector3(0.0f, 0.0f, 0.0f);
            
            if (input.keyDown(Key.W))
            { 
                movement += forward * (   MovementSpeed * ForwardFactor * elapsedTime);
                moved = true;
            }

            if (input.keyDown(Key.A))
            {
                movement += xAxis   * ( - MovementSpeed *                 elapsedTime);
                moved = true;
            }

            if (input.keyDown(Key.S))
            {
                movement += forward * ( - MovementSpeed *                 elapsedTime);
                moved = true;
            }

            if (input.keyDown(Key.D))
            {
                movement += xAxis   * (   MovementSpeed *                 elapsedTime);
                moved = true;
            }

            if (moved)
                move(movement);

            // rotacion
            //

            if (input.keyPressed(Key.L))
                LockMouse = !LockMouse;

            // invertidos: moverse en x cambia el heading (rotacion sobre y) y viceversa.
            float rotY = input.XposRelative * RotationSpeed;
            float rotX = input.YposRelative * RotationSpeed;
            
            if (rotY != 0.0f || rotX != 0.0f)
                look(rotX, rotY);

            if (lockMouse)
                Cursor.Position = windowCenter;
        }

        /// <summary>
        /// Rota en los deltas indicados.
        /// </summary>
        /// <param name="rotX"></param>
        /// <param name="rotY"></param>
        private void look(float rotX, float rotY)
        {
            Matrix deltaRM =
                Matrix.RotationAxis(xAxis, rotX) *
                Matrix.RotationAxis(up, rotY);

            Vector4 result;

            result = Vector3.Transform(xAxis, deltaRM);
            xAxis = new Vector3(result.X, result.Y, result.Z);

            result = Vector3.Transform(yAxis, deltaRM);
            yAxis = new Vector3(result.X, result.Y, result.Z);

            result = Vector3.Transform(zAxis, deltaRM);
            zAxis = new Vector3(result.X, result.Y, result.Z);

            // recalcular las dependencias
            //

            rM *= deltaRM;

            forward = Vector3.Cross(xAxis, up);
            forward.Normalize();

            target = eye + xAxis;

            rotationChanged = true;
        }

        public void setCamera(Vector3 eye, Vector3 target)
        {
            this.eye = eye;
            this.target = target;

            zAxis = target - eye;
            zAxis.Normalize();

            xAxis = Vector3.Cross(up, zAxis);
            xAxis.Normalize();

            yAxis = Vector3.Cross(zAxis, xAxis);
            yAxis.Normalize();

            forward = Vector3.Cross(xAxis, up);
            forward.Normalize();

            tM = Matrix.Translation(eye);

            // \fixme actualizar rM

            rotationChanged = true;
            positionChanged = true;
        }

        /// <summary>
        /// Entrega la matriz de vista a D3D.
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

            tM = Matrix.Translation(eye);

            positionChanged = true;
        }

        /// <summary>
        /// Actualiza la matriz de vista, solo lo que sea necesario.
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
