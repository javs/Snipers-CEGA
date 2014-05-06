using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.CEGA
{
    /// <summary>
    /// Representa al jugador. Tiene control de la camara.
    /// </summary>
    class Player : IRenderObject, IUpdatable
    {
        TgcMesh rifle;
        Vector3 lookAtInicialDelRifle;
        Vector3 posicionAnterior;

        public Player()
        {
            TgcSceneLoader loaderSniper = new TgcSceneLoader();

            // Alex: Este modelo no carga bien, ya le pregunte al tutor para ver cual puede ser el problema
            TgcScene sniperRifle = loaderSniper.loadSceneFromFile(
                GuiController.Instance.AlumnoEjemplosMediaDir + "\\Sniper-TgcScene.xml");

            // De toda la escena solo nos interesa guardarnos el primer modelo (el único que hay en este caso).
            rifle = sniperRifle.Meshes[0];
            rifle.Position = new Vector3(125.0f, 5.0f, 125.0f);
            rifle.AlphaBlendEnable = true;
            rifle.Scale = new Vector3(0.01f, 0.01f, 0.01f);

            // Configuracion de la camara
            //
            TgcFpsCamera camera = GuiController.Instance.FpsCamera;

            //Camara en primera persona, tipo videojuego FPS
            //Solo puede haber una camara habilitada a la vez. Al habilitar la camara FPS se deshabilita la camara rotacional
            //Por default la camara FPS viene desactivada
            camera.Enable = true;
            //Configurar posicion y hacia donde se mira
            camera.setCamera(rifle.Position, new Vector3(0.0f, 0.0f, 0.0f));
            camera.MovementSpeed = 20.0f;

            // hacia donde mira el rifle, sin transformaciones
            lookAtInicialDelRifle = new Vector3(0.0f, 0.0f, -1.0f);
            lookAtInicialDelRifle.Normalize();

            rifle.AutoTransformEnable = false;
        }

        public void update(float elapsedTime)
        {
            // FpsCamera traslada el vector a la posicion de la camara. Eso complica los calculos, asique aca se substrae.
            Vector3 lookAt = GuiController.Instance.FpsCamera.LookAt - GuiController.Instance.FpsCamera.Position;

            lookAt.Y = 0; // la posicion vertical interfiere con el calculo del angulo, eliminarla
            lookAt.Normalize();

            // al normalizarlos, evita tener que dividir por el producto de sus modulos (es 1)
            float angle = FastMath.Acos(Vector3.Dot(lookAtInicialDelRifle, lookAt));
            
            // compensa los cuadrantes superiores ya que el acos tiene una imagen entre 0 y pi
            if (lookAt.X > 0.0f)
                angle = FastMath.TWO_PI - angle;

            // El orden y la separacion de las transformadas es muy importante.
            // 1. Escalar
            // 2. Alejar levemente del origen (efecto "en mis manos").
            // 3. Rotarlo (al aplicar primero 2, esto logra que el rifle rote sobre el eje del jugador, y no sobre si mismo).
            // 4. Moverlo a donde esta la camara.
            rifle.Transform =
                Matrix.Scaling(new Vector3(0.01f, 0.01f, 0.01f)) *
                Matrix.Translation(new Vector3(-0.5f, -1.0f, -2.0f)) *
                Matrix.RotationYawPitchRoll(angle, 0.0f, 0.0f) *
                Matrix.Translation(GuiController.Instance.FpsCamera.Position)
                ;

            //lookAtAnterior = lookAt;

            //Capturar Input teclado 
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.F))
            {
                //Tecla F apretada
            }

            //Capturar Input Mouse
            if (GuiController.Instance.D3dInput.buttonPressed(TgcViewer.Utils.Input.TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Boton izq apretado
            }
        }

        public void render()
        {
            rifle.render();
        }

        public void dispose()
        {
            rifle.dispose();
        }

        public bool AlphaBlendEnable { get; set; }
    }
}
