﻿using Microsoft.DirectX;
using System;
using System.Drawing;
using TgcViewer;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Sound;
using AlumnoEjemplos.CEGA.Units;

namespace AlumnoEjemplos.CEGA
{
    /// <summary>
    /// Representa al jugador. Tiene control de la camara.
    /// </summary>
    class Player : IRenderable, IUpdatable
    {
        TgcMesh rifle;
        Matrix rifleBaseTransforms;

        Boolean scope = false;
        float zoom = 1.0f;

        Matrix matrizSinZoom = GuiController.Instance.D3dDevice.Transform.Projection;
        Matrix matrizConZoom = GuiController.Instance.D3dDevice.Transform.Projection;

        TgcStaticSound sound_Zoom;

        TgcSprite scope_stencil;

        FpsCamera camera;

        #region Constants
        const int zoomMaximo = 3;
        const float zoomWheel = 1.2F;
        const float zoomBase = 1.4F;

        const float RUNNING_SPEED = 50.0f;
        const float WALKING_SPEED = 25.0f;
        const float ROTATION_SPEED_NO_SCOPE = 2.5f;
        const float ROTATION_SPEED_SCOPE = 0.5f;
        #endregion

        public Player()
        {
            TgcSceneLoader loaderSniper = new TgcSceneLoader();

            string media = GuiController.Instance.AlumnoEjemplosMediaDir;

            TgcScene sniperRifle = loaderSniper.loadSceneFromFile(media + "Sniper-TgcScene.xml");

            rifle = sniperRifle.Meshes[0];
            rifle.AlphaBlendEnable = true;
            rifle.AutoTransformEnable = false;

            rifleBaseTransforms =
                Matrix.Scaling(new Vector3(0.01f, 0.01f, 0.01f)) *
                Matrix.RotationY(FastMath.PI - 0.03f) *
                Matrix.Translation(new Vector3(0.5f, -1.0f, 2.0f));

            // Configuracion de la camara
            //
            GuiController.Instance.CurrentCamera.Enable = false;
            camera = new FpsCamera();
            GuiController.Instance.CurrentCamera = camera;
            camera.MovementSpeed = WALKING_SPEED;
            camera.RotationSpeed = ROTATION_SPEED_NO_SCOPE;

            // Configuracion del stencil para el modo scope
            //
            scope_stencil = new TgcSprite();
            scope_stencil.Texture = TgcTexture.createTexture(media + @"Textures\scope_hi.png");

            Size screenSize = GuiController.Instance.Panel3d.Size;
            Size textureSize = scope_stencil.Texture.Size;

            scope_stencil.Scaling = new Vector2(
                (float)screenSize.Width / textureSize.Width,
                (float)screenSize.Height / textureSize.Height);

            scope_stencil.Position = new Vector2(
                FastMath.Max(screenSize.Width / 2 - textureSize.Width * scope_stencil.Scaling.X / 2, 0),
                FastMath.Max(screenSize.Height / 2 - textureSize.Height * scope_stencil.Scaling.Y / 2, 0));

            LoadSounds(media);
        }

        private void LoadSounds(string media)
        {
            sound_Zoom = new TgcStaticSound();
            sound_Zoom.loadSound(media + @"Sound\zoom.wav", -1000);

            TgcStaticSound sound_Walk = new TgcStaticSound();
            sound_Walk.loadSound(media + @"Sound\pl_dirt1.wav", -2000);

            camera.MovementSound = sound_Walk;
        }

        public void Update(float elapsedTime)
        {
            // correr
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.LeftShift))
                camera.MovementSpeed = RUNNING_SPEED;
            else if (GuiController.Instance.D3dInput.keyUp(Microsoft.DirectX.DirectInput.Key.LeftShift))
                camera.MovementSpeed = WALKING_SPEED;

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
                    camera.RotationSpeed = ROTATION_SPEED_SCOPE;
                }
                else
                {
                    zoom = 1;
                    camera.RotationSpeed = ROTATION_SPEED_NO_SCOPE;
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
            rifle.Transform =
                rifleBaseTransforms *
                camera.RotationMatrix *
                camera.TranslationMatrix;
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
            sound_Zoom.dispose();
            scope_stencil.dispose();
            camera.Dispose();
        }
    }
}
