using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Terrain;
using AlumnoEjemplos.CEGA.Interfaces;
using TgcViewer.Utils.Shaders;
using Microsoft.DirectX.Direct3D;
using TgcViewer.Utils.Sound;
using AlumnoEjemplos.CEGA.Units;

namespace AlumnoEjemplos.CEGA.Scenes
{
    /// <summary>
    /// Representa el escenario donde se juega, con terreno y objetos inanimados
    /// </summary>
    class PlayScene : IRenderable, IUpdatable
    {
        private TgcBox suelo;
        private TgcSkyBox skyBox;
        public SimpleTerrain heightMap;

        List<TgcMesh> otrosObjetos;
        List<TgcMesh> barrilesExplosivos; //Creo una lista de barriles, para optimizar las comparaciones de las colisiones

        Effect treeWindEffect;

        const float WIND_SPEED = 0.02f;

        TgcStaticSound sound_WindLong;
        TgcStaticSound sound_WindMedium;

        float time = 0.0f;

        float wind_wave_1;
        float wind_wave_2;
        float wind_wave_3;

        QuadTree quadtree;

        private static PlayScene instance;
        public static PlayScene Instance
        {
            get
            {
                if (instance == null)
                    instance = new PlayScene();
                return instance;
            }
        }

         public PlayScene()
        {
            string mediaDir = GuiController.Instance.AlumnoEjemplosMediaDir;

            TgcTexture pisoTexture = TgcTexture.createTexture(GuiController.Instance.D3dDevice,
                 mediaDir + "Textures\\Grass.jpg");

            suelo = TgcBox.fromSize(new Vector3(1300, 0, 1300), new Vector3(2800, 0, 2800), pisoTexture);


            heightMap = new SimpleTerrain();

            heightMap.loadHeightmap(GuiController.Instance.AlumnoEjemplosMediaDir + "Heightmap\\" + "hmap4.jpg", 26, 0.4f, new Vector3(52, 0, 48));
            heightMap.loadTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "Textures\\" + "grass2.jpg");


            treeWindEffect = TgcShaders.loadEffect(
                GuiController.Instance.AlumnoEjemplosMediaDir + "Shaders\\TreeWind.fx");

            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(mediaDir + "\\Pino-TgcScene.xml");
            


            TgcMesh pinoOriginal = scene.Meshes[0];
            scene = loader.loadSceneFromFile(mediaDir + "\\BarrilPolvora-TgcScene.xml");

            TgcMesh barrilOriginal = scene.Meshes[0];

            //Crear varias instancias del modelo original, pero sin volver a cargar el modelo entero cada vez, hace 23*23 = 529 pinos
            int rows = 23;
            int cols = 23;

            Random RandomPlayScene = new Random(10);

            int offset = RandomPlayScene.Next(100);

            otrosObjetos = new List<TgcMesh>();
            barrilesExplosivos = new List<TgcMesh>();

            for (int i = 1; i <= rows; i++)
            {
                for (int j = 1; j <= cols; j++)
                {
                    //Randomeo el proximo offset, de esta forma nunca vamos a tener 2 escenarios iguales, si queremos evitar que se superpongan cosas hay que fijarse acá.
                    //Si les parece que quedan muy concentrados en el origen podemos separarlo en 2 For (o en 4) para que no se peguen tanto cuando i=1 y j=1.

                   offset = RandomPlayScene.Next(50,150);
                   int scale = RandomPlayScene.Next(10, 30);
                
                    //Me fijo que quede dentro de los limites del mapa

                   if (i * offset > 2600 || j * offset > 2600)
                       offset = RandomPlayScene.Next(10,100);

                    //Crear instancia de modelo
                   if (i == 23)
                   {
                       TgcMesh BarrilInstance = barrilOriginal.createMeshInstance(barrilOriginal.Name + i + j);
                       BarrilInstance.move(j * offset, 0, i * offset);
                       BarrilInstance.AlphaBlendEnable = true;
                       BarrilInstance.Scale = new Vector3(0.09f, 0.09f, 0.09f);
                       
                       otrosObjetos.Add(BarrilInstance);
                       barrilesExplosivos.Add(BarrilInstance);
                   }
                   
                        TgcMesh instance = pinoOriginal.createMeshInstance(pinoOriginal.Name + i + "_" + j);
                   
                   
                    //Desplazarlo
                    instance.move(i * offset, 0, j * offset);
                    instance.AlphaBlendEnable = true;

                    instance.Scale = new Vector3(0.05f * (scale) , 0.05f * (scale), 0.05f * (scale ));
                    
                    
                   
                    //Modifico el BB del arbol para que sea solo el tronco
                    instance.AutoUpdateBoundingBox = false;
                    instance.BoundingBox.scaleTranslate(instance.Position, new Vector3(0.0012f * instance.BoundingBox.calculateSize().X, 0.0016f * instance.BoundingBox.calculateSize().Y, 0.0012f * instance.BoundingBox.calculateSize().Z));
                    
                    
                    instance.Effect = treeWindEffect;
                    instance.Technique = "SimpleWind";


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

            sound_WindLong = new TgcStaticSound();
            sound_WindLong.loadSound(mediaDir + @"Sound\viento_largo.wav", -2000);

            sound_WindMedium = new TgcStaticSound();
            sound_WindMedium.loadSound(mediaDir + @"Sound\viento_medio.wav", -2000);

            //Crear Quadtree
            quadtree = new QuadTree();
            quadtree.create(otrosObjetos, heightMap.BoundingBox);
            quadtree.createDebugQuadtreeMeshes();
        }

        public void Render(Snipers scene)
        {
            //suelo.render();
            skyBox.render();
            heightMap.render();
            if ((bool)GuiController.Instance.Modifiers.getValue("showBB"))
                heightMap.BoundingBox.render();

            quadtree.findMeshes();
            if ((bool)GuiController.Instance.Modifiers.getValue("showQuadTree"))
            {
                foreach (TgcDebugBox debugBox in quadtree.DebugQuadtreeBoxes())
                {
                    debugBox.render();
                }
            }

            for (int i = 0; i < otrosObjetos.Count; i++)
            {
                TgcMesh mesh = otrosObjetos[i];

                if (mesh.Enabled)
                {
                    if (i % 3 == 0)
                        treeWindEffect.SetValue("wind_wave", wind_wave_1);
                    else if (i % 2 == 0)
                        treeWindEffect.SetValue("wind_wave", wind_wave_2);
                    else
                        treeWindEffect.SetValue("wind_wave", wind_wave_3);
                    mesh.render();
                    if ((bool)GuiController.Instance.Modifiers.getValue("showBB"))
                        mesh.BoundingBox.render();
                    mesh.Enabled = false;
                }
                
            }
        }

        public void RenderUI(Snipers scene)
        {

        }


        public void Dispose()
        {
            skyBox.dispose();
            //suelo.dispose();
            heightMap.dispose();

            foreach (var mesh in otrosObjetos)
            {
                mesh.dispose();
            }
        }
        public TgcBoundingBox BoundingBoxTerreno()
        {
            //return suelo.BoundingBox;
            return heightMap.BoundingBox;
        }

        public List<TgcMesh> ObjetosConColision()
        {
            return otrosObjetos;
        }

        public List<TgcMesh> BarrilesExplosivos()
        {
            return barrilesExplosivos;
        }

        public void Update(float elapsedTime)
        {
            time += elapsedTime;

            float x = time * WIND_SPEED;

            wind_wave_1 = WindCurve(x);
            wind_wave_2 = WindCurve(x + 1.0f);
            wind_wave_3 = WindCurve(x + 1.5f);

            if (FastMath.Abs(wind_wave_1) > 0.3f)
                sound_WindLong.play();
            else if (FastMath.Abs(wind_wave_1) > 0.13f)
                sound_WindMedium.play();
        }

        private float WindCurve(float x)
        {
            // Curva de viento para arboles, basada en el Crysis.
            return
                FastMath.Cos(x * FastMath.PI) *
                FastMath.Cos(x * 3.0f * FastMath.PI) *
                FastMath.Cos(x * 5.0f * FastMath.PI) *
                FastMath.Cos(x * 7.0f * FastMath.PI) +
                FastMath.Sin(x * 25.0f * FastMath.PI) * 0.1f;
        }
    }
}
