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
using TgcViewer.Utils.TgcSkeletalAnimation;
using AlumnoEjemplos.CEGA.Interfaces;

namespace AlumnoEjemplos.CEGA.Units
{

    /// <summary>
    /// Representa a un enemigo.
    /// </summary>
    class Enemigo : IRenderable, IUpdatable
    {
        TgcSkeletalMesh enemigo;
        List<string> meshEnemigos = new List<string>();
        Random randomEnemigo = new Random();
        TgcBoundingSphere cabeza;
        float hp;

        public uint id { get; set; }
        
        public Enemigo(Vector3 posicion) 
        {
            //Cargar enemigo
            TgcSkeletalLoader skeletalLoader = new TgcSkeletalLoader();

          
            meshEnemigos.Add("BasicHuman-TgcSkeletalMesh.xml");
            meshEnemigos.Add("CombineSoldier-TgcSkeletalMesh.xml");
            meshEnemigos.Add("CS_Gign-TgcSkeletalMesh.xml");
            meshEnemigos.Add("CS_Arctic-TgcSkeletalMesh.xml");
            meshEnemigos.Add("Pilot-TgcSkeletalMesh.xml");
            meshEnemigos.Add("Quake2Scout-TgcSkeletalMesh.xml");
            meshEnemigos.Add("WomanJeans-TgcSkeletalMesh.xml");

            enemigo = skeletalLoader.loadMeshAndAnimationsFromFile(
                GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\" + meshEnemigos[randomEnemigo.Next(0,6)],
                new string[] {
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "Walk-TgcSkeletalAnim.xml",
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "StandBy-TgcSkeletalAnim.xml",
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "Run-TgcSkeletalAnim.xml"
                });


            enemigo.playAnimation("Run", true);
            enemigo.Position = posicion;
            enemigo.Scale = new Vector3(0.12f, 0.12f, 0.12f);
        
            //Inicializo HP
            hp = 100;

            //Creo el BB para la cabeza
            cabeza = new TgcBoundingSphere(new Vector3(enemigo.Position.X,enemigo.Position.Y + 5.2F,enemigo.Position.Z), 0.5F); //Debe haber alguna forma de sacar esta info del hueso directamente
            cabeza.setRenderColor(System.Drawing.Color.Red);

            //Modifico el BB del cuerpo
            enemigo.AutoUpdateBoundingBox = false;
            enemigo.BoundingBox.scaleTranslate(enemigo.Position, new Vector3(0.07f, 0.095f, 0.07f));
            
        }

        public void Update(float elapsedTime)
        {
            int velocidadEnemigo = randomEnemigo.Next(10, 15);

            Vector3 posicionPlayer = GuiController.Instance.CurrentCamera.getPosition();

            // vector con direccion al jugador
            Vector3 direccionPlayer = posicionPlayer - enemigo.Position;

            // forzar la posicion 'Y' para que se mantenaga en el piso
            direccionPlayer.Y = 0;

            // distancia al jugador
            float distanciaPlayer = direccionPlayer.Length();

            direccionPlayer.Normalize();
            enemigo.move(direccionPlayer * velocidadEnemigo * elapsedTime);

            // rotar al enemigo para que mire al jugador
            enemigo.rotateY((float)Math.Atan2(direccionPlayer.X, direccionPlayer.Z) - enemigo.Rotation.Y + FastMath.PI);

            enemigo.updateAnimation();

            cabeza.moveCenter(direccionPlayer * velocidadEnemigo * elapsedTime);
            enemigo.BoundingBox.move(direccionPlayer * velocidadEnemigo * elapsedTime);
        }

        public void Render(Snipers scene)
        {
            enemigo.render();

            if ((bool)GuiController.Instance.Modifiers.getValue("showBB"))
            {
                enemigo.BoundingBox.render();
                cabeza.render();
            }
        }

        public void RenderUI(Snipers scene)
        {
        }

        public void Herir(float hpARestar) {
            hp -= hpARestar;
        }

        public bool Murio()
        {
            return (hp <= 0);
        }

        public void Morir()
        {
            enemigo.dispose();
        }

        public void Dispose()
        {
            enemigo.dispose();
            cabeza.dispose();
        }

        public Vector3 Position()
        {
            return enemigo.Position;
        }

        public TgcBoundingBox BoundingBoxEnemigo()
        {
            return enemigo.BoundingBox;
        }

        public TgcBoundingSphere BoundingBoxCabeza()
        {
            return cabeza;
        }

    }
}
