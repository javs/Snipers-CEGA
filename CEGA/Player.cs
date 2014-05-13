using Microsoft.DirectX;
using System;
using System.Drawing;
using TgcViewer;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Sound;

namespace AlumnoEjemplos.CEGA
{
    /// <summary>
    /// Representa al jugador. Tiene control de la camara.
    /// </summary>
    class Player : IRenderable, IUpdatable
    {
        TgcMesh rifle;
        Vector3 lookAtInicialDelRifle;
        Vector3 posicionAnteriorCamara;
        Boolean scope = false;
        float zoom = 1.0f;
        Matrix matrizSinZoom = GuiController.Instance.D3dDevice.Transform.Projection;
        Matrix matrizConZoom = GuiController.Instance.D3dDevice.Transform.Projection;

        TgcStaticSound sound_Zoom;
        TgcStaticSound sound_Walk;

        TgcSprite scope_stencil;

        //Constantes
        const int zoomMaximo = 3;
        const float zoomWheel = 1.2F;
        const float zoomBase = 1.4F;

        public Player()
        {
            TgcSceneLoader loaderSniper = new TgcSceneLoader();

            string media = GuiController.Instance.AlumnoEjemplosMediaDir + "\\";

            TgcScene sniperRifle = loaderSniper.loadSceneFromFile(media + "Sniper-TgcScene.xml");

            // De toda la escena solo nos interesa guardarnos el primer modelo (el único que hay en este caso).
            rifle = sniperRifle.Meshes[0];
            rifle.Position = new Vector3(125.0f, 5.0f, 125.0f);
            rifle.AlphaBlendEnable = true;
            rifle.Scale = new Vector3(0.01f, 0.01f, 0.01f);
            rifle.AutoTransformEnable = false;

            // Configuracion de la camara
            //
            TgcFpsCamera camera = GuiController.Instance.FpsCamera;

            //Camara en primera persona, tipo videojuego FPS
            //Solo puede haber una camara habilitada a la vez. Al habilitar la camara FPS se deshabilita la camara rotacional
            //Por default la camara FPS viene desactivada
            camera.Enable = true;
            //Configurar posicion y hacia donde se mira
            camera.setCamera(rifle.Position, new Vector3(0.0f, 0.0f, 0.0f));
            camera.MovementSpeed = 100.0f;

            //Inicializo la pos de la camara
            posicionAnteriorCamara = camera.Position;

            // hacia donde mira el rifle, sin transformaciones
            lookAtInicialDelRifle = new Vector3(0.0f, 0.0f, -1.0f);
            lookAtInicialDelRifle.Normalize();

            scope_stencil = new TgcSprite();
            scope_stencil.Texture = TgcTexture.createTexture(media + "Textures\\scope_hi.png");

            // Centrado en el medio de la pantalla
            Size screenSize = GuiController.Instance.Panel3d.Size;
            Size textureSize = scope_stencil.Texture.Size;

            scope_stencil.Scaling = new Vector2(
                (float)screenSize.Width / textureSize.Width,
                (float)screenSize.Height / textureSize.Height);

            scope_stencil.Position = new Vector2(
                FastMath.Max(screenSize.Width / 2 - textureSize.Width * scope_stencil.Scaling.X / 2, 0),
                FastMath.Max(screenSize.Height / 2 - textureSize.Height * scope_stencil.Scaling.Y / 2, 0));

            //Instancio los sonidos

            sound_Zoom = new TgcStaticSound();
            sound_Walk = new TgcStaticSound();

            sound_Zoom.loadSound(media + "Sound\\zoom.wav");
            sound_Walk.loadSound(media + "Sound\\pl_dirt1.wav");
        }

        public void Update(float elapsedTime)
        {
            TgcFpsCamera camera = GuiController.Instance.FpsCamera;

            // Corremos con shift
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.LeftShift))
                camera.MovementSpeed = 200.0f;
            else if (GuiController.Instance.D3dInput.keyUp(Microsoft.DirectX.DirectInput.Key.LeftShift))
                camera.MovementSpeed = 100.0f;


            // Sonido al caminar

            if (posicionAnteriorCamara.X != camera.Position.X || posicionAnteriorCamara.Z != camera.Position.Z)
            {
                sound_Walk.play();
                posicionAnteriorCamara = camera.Position;
            }
            // Activa el scope
            if (GuiController.Instance.D3dInput.buttonPressed(TgcViewer.Utils.Input.TgcD3dInput.MouseButtons.BUTTON_RIGHT))
            {
                // Me fijo el estado del scope, cambio la matriz de proyeccion y la velocidad de rotación del mouse
                // (para disminuir la sens y que el mouse no vuele con el zoom)

                //Reproduzco el sonido del zoom
                sound_Zoom.play();

                scope = !scope;
                
                if (scope)
                {
                    zoom = zoomBase;
                    camera.RotationSpeed = .4F;
                }
                else
                {
                    zoom = 1;
                    camera.RotationSpeed = 1;
                }
            }

            // Zoom con la rueda del mouse, SOLO si el scope esta activado
            if (scope)
            {
                if (GuiController.Instance.D3dInput.WheelPos > 0)
                    zoom += zoomWheel;
                if (GuiController.Instance.D3dInput.WheelPos < 0)
                    zoom -= zoomWheel;

                if (GuiController.Instance.D3dInput.WheelPos != 0 && zoom <= (zoomBase + zoomMaximo*zoomWheel) && zoom >= 1)
                    sound_Zoom.play();


                if (zoom > zoomBase + (zoomWheel*zoomMaximo))
                    zoom = zoomBase + (zoomWheel*zoomMaximo);
                if (zoom < zoomBase)
                    zoom = zoomBase;
            }
            else
                UpdateRifle();

            matrizConZoom.M11 = matrizSinZoom.M11 * zoom;
            matrizConZoom.M22 = matrizSinZoom.M22 * zoom;

            GuiController.Instance.D3dDevice.Transform.Projection = matrizConZoom;
        }

        /// <summary>
        /// Actualiza la posicion del rifle, para que siga a la camara
        /// </summary>
        private void UpdateRifle()
        {
            TgcFpsCamera camera = GuiController.Instance.FpsCamera;

            // FpsCamera traslada el vector a la posicion de la camara. Eso complica los calculos, asique aca se substrae.
            Vector3 lookAt = camera.LookAt - camera.Position;

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
        }

        public void Render(Snipers scene)
        {
            if (!scope)
                rifle.render();

            scene.PostProcessing.LensDistortion = scope;
        }

        public void RenderUI(Snipers scene)
        {
            if (scope)
            {
                GuiController.Instance.Drawer2D.beginDrawSprite();
                scope_stencil.render();
                GuiController.Instance.Drawer2D.endDrawSprite();
            }
        }

        public void Dispose()
        {
            rifle.dispose();
        }
    }
}
