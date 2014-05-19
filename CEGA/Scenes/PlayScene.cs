using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Terrain;
using AlumnoEjemplos.CEGA.Interfaces;

namespace AlumnoEjemplos.CEGA.Scenes
{
    /// <summary>
    /// Representa el escenario donde se juega, con terreno y objetos inanimados
    /// </summary>
    class PlayScene : IRenderable
    {
        private TgcBox suelo;
        private TgcSkyBox skyBox;

        List<TgcMesh> otrosObjetos;

        public PlayScene()
        {
            string mediaDir = GuiController.Instance.AlumnoEjemplosMediaDir;

            TgcTexture pisoTexture = TgcTexture.createTexture(GuiController.Instance.D3dDevice,
                 mediaDir + "Textures\\Grass.jpg");

            suelo = TgcBox.fromSize(new Vector3(1300, 0, 1300), new Vector3(2800, 0, 2800), pisoTexture);

            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(mediaDir + "\\Pino-TgcScene.xml");
            
            TgcMesh pinoOriginal = scene.Meshes[0];

            //Crear varias instancias del modelo original, pero sin volver a cargar el modelo entero cada vez, hace 23*23 = 529 pinos
            int rows = 23;
            int cols = 23;

            Random offsetRnd = new Random();

            int offset = offsetRnd.Next(100);

            otrosObjetos = new List<TgcMesh>();

            for (int i = 1; i <= rows; i++)
            {
                for (int j = 1; j <= cols; j++)
                {
                    //Randomeo el proximo offset, de esta forma nunca vamos a tener 2 escenarios iguales, si queremos evitar que se superpongan cosas hay que fijarse acá.
                    //Si les parece que quedan muy concentrados en el origen podemos separarlo en 2 For (o en 4) para que no se peguen tanto cuando i=1 y j=1.

                   offset = offsetRnd.Next(50,150);
                
                    //Me fijo que quede dentro de los limites del mapa

                   if (i * offset > 2600 || j * offset > 2600)
                       offset = offsetRnd.Next(10,100);

                    //Crear instancia de modelo
                    TgcMesh instance = pinoOriginal.createMeshInstance(pinoOriginal.Name + i + "_" + j);

                    //Desplazarlo
                    instance.move(i * offset, 0, j * offset);
                    instance.AlphaBlendEnable = true;
                    instance.Scale = new Vector3(0.25f, 0.25f, 0.25f);

                    otrosObjetos.Add(instance);
                }
            }

            //Sky Box

            string texturesPath = mediaDir + "Skyboxes\\SkyBox4\\";

            //Creo el SkyBox 
            skyBox = new TgcSkyBox();
            skyBox.Center = new Vector3(1300, 250, 1300);
            skyBox.Size = new Vector3(4000, 1500, 4000);

            //Cargo las texturas de las caras, (algunas no tienen "down" así que uso TOP, por que igual no se debería ver en nuestro caso)
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "top.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "top.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "left.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "right.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "front.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "back.jpg");

            //Actualizar todos los valores para crear el SkyBox
            skyBox.updateValues();
        }

        public void Render(Snipers scene)
        {
            suelo.render();
            skyBox.render();

            foreach (var mesh in otrosObjetos)
            {
                mesh.render();
            }
        }

        public void RenderUI(Snipers scene)
        {

        }

        public void Dispose()
        {
            skyBox.dispose();
            suelo.dispose();

            foreach (var mesh in otrosObjetos)
            {
                mesh.dispose();
            }
        }
        public TgcBoundingBox BoundingBoxTerreno()
        {
            return suelo.BoundingBox;
        }

        public List<TgcMesh> ObjetosConColision()
        {
            return otrosObjetos;
        }

    }
}
