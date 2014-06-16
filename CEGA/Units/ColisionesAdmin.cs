using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using System.Collections.Generic;
using TgcViewer;
using TgcViewer.Example;
using TgcViewer.Utils;
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.Sound;
using AlumnoEjemplos.CEGA.Scenes;

namespace AlumnoEjemplos.CEGA.Units
{
    class ColisionesAdmin
    {

        public Player jugador { get; set; }
        public EnemigosAdmin enemigos { get; set; }
        public PlayScene escenario { get; set; }

        TgcStaticSound sound_Explosion = new TgcStaticSound();

        #region Constants
        const float damage_Body = 50;
        const float damage_Head = 100;
        #endregion

        private static ColisionesAdmin instance;

        public static ColisionesAdmin Instance
        {
            get 
            {
                if (instance == null)
                    instance = new ColisionesAdmin();
                return instance;
            }
        }

        public ColisionesAdmin()
        {
            sound_Explosion = new TgcStaticSound();
            sound_Explosion.loadSound(GuiController.Instance.AlumnoEjemplosMediaDir + @"Sound\explosion.wav", -1000);
        }

        public bool ColisionDisparo(TgcRay disparo)
        {
            Vector3 interseccion;
            float distancia;

            foreach (Enemigo enemigo in enemigos.ListaDeEnemigosOrdenadaPorDistancia())
            {
                
                if (TgcCollisionUtils.intersectRayAABB(disparo, enemigo.BoundingBoxEnemigo(), out interseccion))
                {
                    enemigo.Herir(damage_Body);

                    if (enemigo.Murio())
                    {
                        enemigo.Muriendo = true;
                        this.jugador.puntos += 10;
                    }

                    this.jugador.puntos += 1;
                    return true;
                }

                if (TgcCollisionUtils.intersectRaySphere(disparo,enemigo.BoundingBoxCabeza(), out distancia, out interseccion))
                {
                    enemigo.Herir(damage_Head);

                    if (enemigo.Murio())
                    {
                        enemigo.Muriendo = true;
                        this.jugador.puntos += 10;
                    }

                    this.jugador.puntos += 2;
                    return true;
                }
            }

            foreach (TgcMesh barril in escenario.BarrilesExplosivos())
            {

                if (TgcCollisionUtils.intersectRayAABB(disparo, barril.BoundingBox, out interseccion))
                {
                    //Se podria hacer un objeto barril, por ahora meto el código acá. -Alex
                    //Hasta aca ya sabemos que el disparo le dio al barril.
                    float d, xc, zc;
                    xc = barril.Position.X;
                    zc = barril.Position.Z;

                    var listaDeEnemigos = enemigos.ListaDeEnemigos();

                    foreach (var enemigo in listaDeEnemigos)
                    {
                        //Calculo la distancia hasta el barril
                        d = FastMath.Sqrt(FastMath.Pow2(enemigo.Position().X - xc) + FastMath.Pow2(enemigo.Position().Z - zc));

                        //Me fijo si cumple con el radio (si tenemos el objeto barril, cada barril puede tener su radio)
                        if (d <= 300) //Radio hardcodeado
                        {
                            enemigo.Herir(300 / (0.05F * d));

                            if (enemigo.Murio())
                            {
                                enemigo.Muriendo = true;
                                this.jugador.puntos += 10;
                            }
                        }
                    }

                    //Aparte, el barril exploto...
                    sound_Explosion.play();

                    escenario.BorrarObjeto(barril.Name);
                    return true;
                }
            }

            return false;
        }

        public bool ColisionConObjetos() 
        {
      
            foreach (TgcMesh obstaculo in escenario.ObjetosConColisionCerca(jugador.BoundingBoxJugador()))
            {
                if (TgcCollisionUtils.testSphereAABB(jugador.BoundingSphereJugador(), obstaculo.BoundingBox))
                    return true;
            }

            // limites del Heightmap, si llega a determinada altura, rebotar
            int cameraPosX = (int)(GuiController.Instance.CurrentCamera.getPosition().X + 598) / 26;
            int cameraPosZ = (int)(GuiController.Instance.CurrentCamera.getPosition().Z + 702) / 26;

            if (escenario.heightMap.HeightmapData[cameraPosX, cameraPosZ] > 11)
                return true;

            return false;

        }

        public bool ColisionConEnemigos()
        {
            foreach (Enemigo enemigo in enemigos.ListaDeEnemigos())
            {
                if (TgcCollisionUtils.testSphereAABB(jugador.BoundingSphereJugador(), enemigo.BoundingBoxEnemigo()))
                {
                    enemigos.Reset();
                    return true;
                }
                    
            }
            return false;
        }

        public bool ColisionEnemigoConObjetos(Enemigo enemigo)
        {
            foreach (TgcMesh obstaculo in escenario.ObjetosConColisionCerca(enemigo.BoundingBoxEnemigo()))
            {
                if (TgcCollisionUtils.testAABBAABB(enemigo.BoundingBoxEnemigo(), obstaculo.BoundingBox))
                    return true;
            }
            return false;
        }

        public bool ColisionEnemigoConEnemigos(Enemigo enemigo, out Enemigo enemigoColision)
        {
            foreach (Enemigo otroEnemigo in enemigos.ListaDeEnemigos())
            {
                if (enemigo == otroEnemigo)
                    continue;
                if (TgcCollisionUtils.testAABBAABB(enemigo.BoundingBoxEnemigo(), otroEnemigo.BoundingBoxEnemigo()))
                {
                    enemigoColision = otroEnemigo;
                    return true;
                }
            }
            enemigoColision = null;
            return false;
        }
		
    }
}
