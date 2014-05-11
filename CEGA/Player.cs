using Microsoft.DirectX;
using System;
using System.Drawing;
using TgcViewer;
using TgcViewer.Utils._2D;
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
        Boolean scope = false;
        float zoom = 1.0f;
        Matrix matrizSinZoom = GuiController.Instance.D3dDevice.Transform.Projection;
        Matrix matrizConZoom = GuiController.Instance.D3dDevice.Transform.Projection;

        TgcSprite scope_stencil;

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
        }

        public void update(float elapsedTime)
        {
            TgcFpsCamera camera = GuiController.Instance.FpsCamera;

            // Corremos con shift
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.LeftShift))
                camera.MovementSpeed = 200.0f;
            else if (GuiController.Instance.D3dInput.keyUp(Microsoft.DirectX.DirectInput.Key.LeftShift))
                camera.MovementSpeed = 100.0f;

            // Activa el scope
            if (GuiController.Instance.D3dInput.buttonPressed(TgcViewer.Utils.Input.TgcD3dInput.MouseButtons.BUTTON_RIGHT))
            {
                // Me fijo el estado del scope, cambio la matriz de proyeccion y la velocidad de rotación del mouse
                // (para disminuir la sens y que el mouse no vuele con el zoom)

                // TODO Alex: Si les parece podemos definir constantes zoomInicial = 1.2f, zoomWheel = 0.4f,
                // zoomMAX = 3 para que quede mejor el código. Por ahora lo dejo así

                scope = !scope;

                if (scope)
                {
                    zoom = 1.4f;
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
                    zoom += 0.8f;
                if (GuiController.Instance.D3dInput.WheelPos < 0)
                    zoom -= 0.8f;

                if (zoom > 3.8)
                    zoom = 3.8f;
                if (zoom < 1)
                    zoom = 1;
            }
            else
                updateRifle();

            matrizConZoom.M11 = matrizSinZoom.M11 * zoom;
            matrizConZoom.M22 = matrizSinZoom.M22 * zoom;

            GuiController.Instance.D3dDevice.Transform.Projection = matrizConZoom;
        }

        /// <summary>
        /// Actualiza la posicion del rifle, para que siga a la camara
        /// </summary>
        private void updateRifle()
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

        public void render()
        {
            if (scope)
            {
                GuiController.Instance.Drawer2D.beginDrawSprite();
                scope_stencil.render();
                GuiController.Instance.Drawer2D.endDrawSprite();
            }
            else
            {
                rifle.render();
            }
        }

        public void dispose()
        {
            rifle.dispose();
        }

        public bool AlphaBlendEnable { get; set; }
    }
}
