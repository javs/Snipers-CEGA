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


        public TgcMesh pastoOriginal;
        int scale;

        List<TgcMesh> otrosObjetos;

        Effect treeWindEffect;

        const float WIND_SPEED = 0.02f;

        TgcStaticSound sound_WindLong;
        TgcStaticSound sound_WindMedium;
        TgcStaticSound sound_music;

        float time = 0.0f;

        float wind_wave_1;
        float wind_wave_2;
        float wind_wave_3;

        GrillaRegular grilla;

        public PlayScene()
        {

            string mediaDir = GuiController.Instance.AlumnoEjemplosMediaDir;

            TgcTexture pisoTexture = TgcTexture.createTexture(GuiController.Instance.D3dDevice,
                 mediaDir + "CEGA\\Textures\\Grass.jpg");

            suelo = TgcBox.fromSize(new Vector3(1300, 0, 1300), new Vector3(2800, 0, 2800), pisoTexture);


            heightMap = new SimpleTerrain();

            heightMap.loadHeightmap(GuiController.Instance.AlumnoEjemplosMediaDir + "CEGA\\Heightmap\\" + "hmap4.jpg", 26, 0.4f, new Vector3(52, 0, 48));
            heightMap.loadTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "CEGA\\Textures\\" + "Pasto2.jpg");


            treeWindEffect = TgcShaders.loadEffect(
                GuiController.Instance.AlumnoEjemplosMediaDir + "CEGA\\Shaders\\TreeWind.fx");

            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(mediaDir + "CEGA\\Pino-TgcScene.xml");
            TgcMesh pinoOriginal = scene.Meshes[0];

            scene = loader.loadSceneFromFile(mediaDir + "CEGA\\BarrilPolvora-TgcScene.xml");
            TgcMesh barrilOriginal = scene.Meshes[0];

            scene = loader.loadSceneFromFile(mediaDir + "CEGA\\Pasto3-TgcScene.xml");
            pastoOriginal = scene.Meshes[0];

            //Crear varias instancias del modelo original, pero sin volver a cargar el modelo entero cada vez, hace 23*23 = 529 pinos
            int rows = 23;
            int cols = 23;

            Random RandomPlayScene = new Random(10);

            int offset = RandomPlayScene.Next(100);

            otrosObjetos = new List<TgcMesh>();

            for (int i = 1; i <= rows; i++)
            {
                for (int j = 1; j <= cols; j++)
                {
                    //Randomeo el proximo offset, de esta forma nunca vamos a tener 2 escenarios iguales, si queremos evitar que se superpongan cosas hay que fijarse acá.
                    //Si les parece que quedan muy concentrados en el origen podemos separarlo en 2 For (o en 4) para que no se peguen tanto cuando i=1 y j=1.

                    offset = RandomPlayScene.Next(50, 150);
                    scale = RandomPlayScene.Next(10, 30);

                    //Me fijo que quede dentro de los limites del mapa

                    if (i * offset > 2600 || j * offset > 2600)
                        offset = RandomPlayScene.Next(10, 100);

                    //Crear instancia de modelo 
                    //  Barriles
                    if (i == 23)
                    {
                        TgcMesh BarrilInstance = barrilOriginal.createMeshInstance(barrilOriginal.Name + i + j);
                        BarrilInstance.move(j * offset, 0, i * offset);
                        BarrilInstance.AlphaBlendEnable = true;
                        BarrilInstance.Scale = new Vector3(0.09f, 0.09f, 0.09f);

                        // gana algunos fps
                        BarrilInstance.AutoTransformEnable = false;
                        BarrilInstance.Transform =
                            Matrix.Scaling(BarrilInstance.Scale) *
                            Matrix.Translation(BarrilInstance.Position);

                        BarrilInstance.UserProperties = new Dictionary<string, string>();
                        BarrilInstance.UserProperties["colisionable"] = "";

                        otrosObjetos.Add(BarrilInstance);
                    }

                    //  Pinos
                    //

                    TgcMesh instance = pinoOriginal.createMeshInstance(pinoOriginal.Name + i + "_" + j);
                    
                    instance.AlphaBlendEnable = true;
                    instance.Position = new Vector3(i * offset, 0, j * offset);
                    instance.Scale = new Vector3(0.05f * (scale), 0.05f * (scale), 0.05f * (scale));

                    // gana algunos fps
                    instance.AutoTransformEnable = false;
                    instance.Transform =
                        Matrix.Scaling(instance.Scale) *
                        Matrix.Translation(instance.Position);

                    instance.UserProperties = new Dictionary<string, string>();
                    instance.UserProperties["colisionable"] = "";

                    //Modifico el BB del arbol para que sea solo el tronco
                    instance.AutoUpdateBoundingBox = false;
                    instance.BoundingBox.scaleTranslate(instance.Position, new Vector3(0.0012f * instance.BoundingBox.calculateSize().X, 0.0016f * instance.BoundingBox.calculateSize().Y, 0.0012f * instance.BoundingBox.calculateSize().Z));
                    //Effecto de Viento (Shader);
                    instance.Effect = treeWindEffect;
                    instance.Technique = "SimpleWindTree";
                    //Agrego a la coleccion
                    otrosObjetos.Add(instance);

                    //  Bancos de Pasto

                    if ( j == 22)
                    generarBancoPasto(otrosObjetos, new Vector3(j * offset, 0, i * offset), RandomPlayScene);

                }

            }

            //Sky Box

            string texturesPath = mediaDir + "CEGA\\Skyboxes\\SkyBox4\\";

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
            sound_WindLong.loadSound(mediaDir + @"CEGA\Sound\viento_largo.wav", -2000);

            sound_WindMedium = new TgcStaticSound();
            sound_WindMedium.loadSound(mediaDir + @"CEGA\Sound\viento_medio.wav", -2000);

            sound_music = new TgcStaticSound();
            sound_music.loadSound(mediaDir + @"CEGA\Sound\rabbia.wav", -2000);
            sound_music.play(true);

            //Crear Grilla
            grilla = new GrillaRegular();
            grilla.create(otrosObjetos, heightMap.BoundingBox);
            grilla.createDebugMeshes();
        }

        public void Render(Snipers scene)
        {
            //suelo.render();
            skyBox.render();
            heightMap.render();

            if ((bool)GuiController.Instance.Modifiers.getValue("showBB"))
                heightMap.BoundingBox.render();

            this.UpdateVisibleMeshes();
            if ((bool)GuiController.Instance.Modifiers.getValue("showGrilla"))
            {
                foreach (TgcDebugBox debugBox in grilla.DebugGrillaBoxes())
                {
                    debugBox.render();
                }
            }

            for (int i = 0; i < otrosObjetos.Count; i++)
            {
                TgcMesh mesh = otrosObjetos[i];

                if (mesh.Enabled)
                {
                    // Tres curvas distintas en diferentes etapas, para mayor realismo
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

        public void UpdateVisibleMeshes()
        {
            this.grilla.UpdateVisibleMeshes(GuiController.Instance.Frustum);
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
            List<TgcMesh> listaObjetos = new List<TgcMesh>();
            foreach (TgcMesh objeto in otrosObjetos)
            {
                if (objeto.UserProperties.ContainsKey("colisionable"))
                    listaObjetos.Add(objeto);
            }

            return listaObjetos;
        }

        public List<TgcMesh> ObjetosConColisionCerca(TgcBoundingBox bb)
        {
            List<TgcMesh> listaObjetos = new List<TgcMesh>();

            foreach (GrillaRegularNode nodo in this.grilla.NodosCercanos(bb))
            {
                foreach (TgcMesh objeto in nodo.Models)
                {
                    if (objeto.UserProperties.ContainsKey("colisionable"))
                        listaObjetos.Add(objeto);
                }
            }

            return listaObjetos;
        }

        private void generarBancoPasto(List<TgcMesh> objs, Vector3 pos , Random rdm)
        {
            for (int q = 0; q < 1; q++)
            {
                for (int w = 0; w < 1; w++)
                {
                    scale = rdm.Next(10, 20);
                    
                    TgcMesh pastoBanco = pastoOriginal.createMeshInstance(pastoOriginal.Name + q + w + pos.X + pos.Z);
                    pastoBanco.AlphaBlendEnable = true;
                    pastoBanco.Scale = new Vector3(0.025f * scale, 0.025f * scale, 0.025f * scale);
                    pastoBanco.move(pos.X + q*2.6f, 0, pos.Z + w *2.6f);
                    pastoBanco.UserProperties = new Dictionary<string, string>();
                    pastoBanco.Effect = treeWindEffect;
                    pastoBanco.Technique = "SimpleWindGrass";

                    // gana algunos fps
                    pastoBanco.AutoTransformEnable = false;
                    pastoBanco.Transform =
                        Matrix.Scaling(pastoBanco.Scale) *
                        Matrix.Translation(pastoBanco.Position);

                    objs.Add(pastoBanco);

                }

            }

        }


        public GrillaRegular Grilla()
        {
            return this.grilla;
        }

        public List<TgcMesh> BarrilesExplosivos()
        {
            List<TgcMesh> barrilesExplosivos = new List<TgcMesh>();

            foreach (TgcMesh objeto in this.ObjetosConColision())
            {
                if (objeto.Name.StartsWith("Barril"))
                    barrilesExplosivos.Add(objeto);
            }

            return barrilesExplosivos;
        }

        public void BorrarObjeto(string nombre)
        {
            for (int i = 0; i < otrosObjetos.Count; i++)
            {
                if (otrosObjetos[i].Name == nombre)
                {
                    otrosObjetos[i].dispose();
                    grilla.BorrarModelo(otrosObjetos[i]);
                    otrosObjetos.RemoveAt(i);
                    break;
                }
            }
        }

        public void Update(float elapsedTime)
        {
            time += elapsedTime;

            float x = time * WIND_SPEED;

            wind_wave_1 = WindCurve(x);
            wind_wave_2 = WindCurve(x + 1.0f);
            wind_wave_3 = WindCurve(x + 3.5f);

            if (FastMath.Abs(wind_wave_1) > 0.3f)
                sound_WindLong.play();
            else if (FastMath.Abs(wind_wave_1) > 0.13f)
                sound_WindMedium.play();
        }

        private float WindCurve(float x)
        {
            // Curva de viento para arboles, basada en el libro de GPU Gems.
            return
                FastMath.Cos(x * FastMath.PI) *
                FastMath.Cos(x * 3.0f * FastMath.PI) *
                FastMath.Cos(x * 5.0f * FastMath.PI) *
                FastMath.Cos(x * 7.0f * FastMath.PI) +
                FastMath.Sin(x * 25.0f * FastMath.PI) * 0.1f;
        }
    }
}
