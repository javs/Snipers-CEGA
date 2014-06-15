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
        Vector3 vectorNulo = new Vector3(0, 0, 0);
        Vector3 direccionAnterior = new Vector3(0, 0, 0);

        bool muriendo;

        public bool Muriendo
        {
            get { return muriendo; }
            set
            {
                muriendo = value;

                if (muriendo)
                    enemigo.playAnimation("Death", false);
            }
        }

        public bool TerminoDeMorir
        {
            get { return muriendo && !enemigo.IsAnimating; }
        }

        public bool colisionado { get; set; }
        
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
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "Run-TgcSkeletalAnim.xml",
                    GuiController.Instance.AlumnoEjemplosMediaDir + "Animations\\" + "Death-TgcSkeletalAnim.xml",
                });


            enemigo.playAnimation("Run", true);
            enemigo.Position = posicion;
            enemigo.Scale = new Vector3(0.12f, 0.12f, 0.12f);
            this.colisionado = false;
            
            //Inicializo HP
            hp = 100;

            //Creo el BB para la cabeza
            cabeza = new TgcBoundingSphere(new Vector3(enemigo.Position.X, enemigo.Position.Y + 5.2F,enemigo.Position.Z), 0.5F); //Debe haber alguna forma de sacar esta info del hueso directamente
            cabeza.setRenderColor(System.Drawing.Color.Red);

            //Modifico el BB del cuerpo
            enemigo.AutoUpdateBoundingBox = false;
            enemigo.BoundingBox.scaleTranslate(enemigo.Position, new Vector3(0.07f, 0.095f, 0.07f));

        }

        public void Update(float elapsedTime)
        {
            int velocidadEnemigo = randomEnemigo.Next(10, 15);
            float angulo;
            Enemigo otroEnemigo;

            Vector3 posicionPlayer = GuiController.Instance.CurrentCamera.getPosition();

            // vector con direccion al jugador
            Vector3 direccionPlayer = posicionPlayer - enemigo.Position;
            direccionPlayer.Y = 0;

            Vector3 direccionMovimiento = direccionPlayer;

            if (ColisionesAdmin.Instance.ColisionEnemigoConObjetos(this))
            {
                if (direccionAnterior == vectorNulo)
                {
                    angulo = (180 * (float)Math.Atan(direccionMovimiento.Z / direccionMovimiento.X)) / (float)Math.PI + 45;
                    angulo += 90;
                    if (angulo >= 360)
                        angulo -= 360;

                    if (angulo < 0)
                        angulo += 360;
                    direccionMovimiento = rotar90(angulo);
                    direccionAnterior = direccionMovimiento;
                }
            }
            else
            {
                direccionAnterior = vectorNulo;
            }

            if (this.colisionado == true)
            {
                if (direccionAnterior == vectorNulo)
                {
                    direccionMovimiento.X += randomEnemigo.Next(50, 100);
                    if (randomEnemigo.Next(0, 2) == 1)
                        direccionMovimiento.X *= -1;
                    direccionAnterior = direccionMovimiento;
                }

                if (!ColisionesAdmin.Instance.ColisionEnemigoConEnemigos(this, out otroEnemigo))
                {
                    this.colisionado = false;
                    direccionAnterior = vectorNulo;
                }

            }
            else
            {
                if (ColisionesAdmin.Instance.ColisionEnemigoConEnemigos(this, out otroEnemigo))
                    otroEnemigo.colisionado = true;
            }

            if (direccionAnterior != vectorNulo)
                direccionMovimiento = direccionAnterior;

            direccionMovimiento.Normalize();

            // rotar al enemigo para que mire al jugador
            enemigo.rotateY((float)Math.Atan2(direccionMovimiento.X, direccionMovimiento.Z) - enemigo.Rotation.Y + FastMath.PI);

            enemigo.updateAnimation();

            enemigo.move(direccionMovimiento * velocidadEnemigo * elapsedTime);
            cabeza.moveCenter(direccionMovimiento * velocidadEnemigo * elapsedTime);
            enemigo.BoundingBox.move(direccionMovimiento * velocidadEnemigo * elapsedTime);
        }

        private const float LADO_CUBO = 1.0f;
        private const float MEDIO_LADO_CUBO = LADO_CUBO * 0.5f;
        private float STEP_ANGULO = LADO_CUBO / 90;
        private Vector3 rotar90(float angulo)
        {
            float x = 0;
            float y = 0;
            float z = 0;

            if (angulo < 180)
            {
                if (angulo < 90)
                {
                    z = angulo * STEP_ANGULO;
                }
                else
                {
                    z = LADO_CUBO;
                    x = (angulo - 90) * STEP_ANGULO;
                }
                z = z - MEDIO_LADO_CUBO;
                x = MEDIO_LADO_CUBO - x;
            }
            else
            {
                if (angulo < 270)
                {
                    z = (angulo - 180) * STEP_ANGULO;
                }
                else
                {
                    z = LADO_CUBO;
                    x = (angulo - 270) * STEP_ANGULO;
                }
                z = MEDIO_LADO_CUBO - z;
                x = x - MEDIO_LADO_CUBO;
            }

            return new Vector3(x, y, z);

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

        public void Herir(float hpARestar)
        {
            hp -= hpARestar;
        }

        public bool Murio()
        {
            return (hp <= 0);
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
