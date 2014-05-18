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

namespace AlumnoEjemplos.CEGA.Units
{
    class DetectorDeColisiones
    {
       public Player jugador {get;set;}
       public EnemigosAdmin enemigos {get;set;}
       public PlayScene escenario {get;set;}


        
   private static DetectorDeColisiones instance;

           public static DetectorDeColisiones Instance
   {
      get 
      {
         if (instance == null)
         {
             instance = new DetectorDeColisiones();
         }
         return instance;
      }
   }


        public bool ColisionDisparo(TgcRay disparo){
        Vector3 interseccion;

        foreach (Enemigo enemigo in enemigos.listaDeEnemigos())
        {
            if (TgcCollisionUtils.intersectRayAABB(disparo, enemigo.BoundigBox(), out interseccion))
            {
                //Restarle vida al enemigo
                return true;
            }
        }

           return false;
        }

        public bool ColisionConObjetos() {
      
            foreach (TgcMesh obstaculo in escenario.objetosConColision())
            {
                TgcCollisionUtils.BoxBoxResult result = TgcCollisionUtils.classifyBoxBox(jugador.boundingBoxJugador(), obstaculo.BoundingBox);
                if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                {
                    return true;
                }
            }

            return false;

        }


    }
}
