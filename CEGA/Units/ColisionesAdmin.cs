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
using AlumnoEjemplos.CEGA.Scenes;

namespace AlumnoEjemplos.CEGA.Units
{
    class ColisionesAdmin
    {

        public Player jugador {get;set;}
        public EnemigosAdmin enemigos {get;set;}
        public PlayScene escenario {get;set;}

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

        public bool ColisionDisparo(TgcRay disparo)
        {
            Vector3 interseccion;
            int i = 0;
            int j = 0;

            float distancia;

            foreach (Enemigo enemigo in enemigos.listaDeEnemigosOrdenadaPorDistancia())
            {
                
                if (TgcCollisionUtils.intersectRayAABB(disparo, enemigo.BoundingBoxEnemigo(), out interseccion))
                {
                    enemigo.Herir(damage_Body);

                    if (enemigo.Murio())
                    {
                        enemigos.MatarEnemigo(enemigo.id);
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
                        enemigos.MatarEnemigo(enemigo.id);
                        this.jugador.puntos += 10;
                    }

                    this.jugador.puntos += 2;
                    return true;
                }
                i++;
            }

            i = 0;

            foreach (TgcMesh barril in escenario.BarrilesExplosivos())
            {

                if (TgcCollisionUtils.intersectRayAABB(disparo, barril.BoundingBox, out interseccion))
                {
                    //Se podria hacer un objeto barril, por ahora meto el código acá. -Alex
                    //Hasta aca ya sabemos que el disparo le dio al barril.
                    float d, xc, zc;
                    xc = barril.Position.X;
                    zc = barril.Position.Z;

                    foreach (Enemigo enemigo in enemigos.listaDeEnemigos())
                    {
                        //Calculo la distancia hasta el barril
                        d = FastMath.Sqrt(FastMath.Pow2(enemigo.Position().X - xc) + FastMath.Pow2(enemigo.Position().Z - zc));
                        //Me fijo si cumple con el radio (si tenemos el objeto barril, cada barril puede tener su radio)

                        if (d <= 300) //Radio hardcodeado
                        {
                            enemigo.Herir(300/(0.5F*d));

                            //Me fijo si murio. Esta logica podría estar en el enemigo directamente, cuando lo hiero, pero hay que ver como lo sacamos de la lista.
                            if (enemigo.Murio())
                            {
                                enemigos.MatarEnemigo(enemigo.id);
                                this.jugador.puntos += 10;
                            }
                        }

                        j++;
                    }

                    enemigos.listaDeEnemigos().RemoveAll(e => e.Murio());
                    

                    //Aparte, el barril exploto...

                    escenario.BarrilesExplosivos().RemoveAt(i);
                    barril.dispose();
                    return true;
                }

                i++;
                j = 0;
            }

            return false;
        }

        public bool ColisionConObjetos() 
        {
      
            foreach (TgcMesh obstaculo in escenario.ObjetosConColision())
            {
                // usar el bounding box del arbol no esta bueno; ocupa mucho mas que el tronco
                if (TgcCollisionUtils.testSphereAABB(jugador.BoundingSphereJugador(), obstaculo.BoundingBox))
                    return true;
            }
            return false;

        }

        public bool ColisionConEnemigos()
        {
            foreach (Enemigo enemigo in enemigos.listaDeEnemigos())
            {
                if (TgcCollisionUtils.testSphereAABB(jugador.BoundingSphereJugador(), enemigo.BoundingBoxEnemigo()))
                {
                    enemigos.Reset();
                    return true;
                }
                    
            }
            return false;
        }

    }
}
