using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
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

            foreach (Enemigo enemigo in enemigos.listaDeEnemigos())
            {
                if (TgcCollisionUtils.intersectRayAABB(disparo, enemigo.BoundingBoxEnemigo(), out interseccion))
                {
                    enemigo.Morir();
                    enemigos.listaDeEnemigos().RemoveAt(i);
                    return true;
                }
                i++;
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
