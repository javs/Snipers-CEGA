using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.CEGA
{
    /// <summary>
    /// Representa el escenario donde se juega, con terreno y objetos inanimados
    /// </summary>
    class PlayScene : IRenderObject
    {
        private TgcBox suelo;

        List<TgcMesh> otrosObjetos;

        public PlayScene()
        {
            string mediaDir = GuiController.Instance.AlumnoEjemplosMediaDir;

            TgcTexture pisoTexture = TgcTexture.createTexture(GuiController.Instance.D3dDevice,
                 mediaDir + "Textures\\Grass.jpg");

            suelo = TgcBox.fromSize(new Vector3(1100, 0, 1100), new Vector3(2600, 0, 2600), pisoTexture);

            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(mediaDir + "\\Pino-TgcScene.xml");

            TgcMesh pinoOriginal = scene.Meshes[0];

            //Crear varias instancias del modelo original, pero sin volver a cargar el modelo entero cada vez, hace 23*23 = 529 pinos
            int rows = 23;
            int cols = 23;
            float offset = 100;
            otrosObjetos = new List<TgcMesh>();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    //Crear instancia de modelo
                    TgcMesh instance = pinoOriginal.createMeshInstance(pinoOriginal.Name + i + "_" + j);

                    //Desplazarlo
                    instance.move(i * offset, 0, j * offset);
                    instance.AlphaBlendEnable = true;
                    instance.Scale = new Vector3(0.25f, 0.25f, 0.25f);

                    otrosObjetos.Add(instance);
                }
            }
        }

        public void render()
        {
            suelo.render();

            foreach (var mesh in otrosObjetos)
            {
                mesh.render();
            }
        }

        public void dispose()
        {
            suelo.dispose();

            foreach (var mesh in otrosObjetos)
            {
                mesh.dispose();
            }
        }

        public bool AlphaBlendEnable { get; set; }

    }
}
