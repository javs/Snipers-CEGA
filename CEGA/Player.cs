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
            camera.MovementSpeed = 50.0f;
        }

        public void update(float elapsedTime)
        {
            // Muevo el rifle con la camara
            rifle.Position = GuiController.Instance.FpsCamera.Position - new Vector3(0.5f, 1.0f, 2.0f);
            
            //Roto el sniper segun donde mira la camara ESTO NO FUNCIONA BIEN 
            /* Calculo el angulo entre la posicion anterior y la posicion actual, y roto el mesh segun ese angulo
             * Luego actualizo la posicion anterior
             */
             
            //float angle = FastMath.Acos(Vector3.Dot(Vector3.Normalize(lookAtAnterior), Vector3.Normalize(GuiController.Instance.FpsCamera.LookAt - GuiController.Instance.FpsCamera.Position)));

            //rifle.rotateY(angle);
            
            //lookAtAnterior = GuiController.Instance.FpsCamera.LookAt - GuiController.Instance.FpsCamera.Position;;

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
