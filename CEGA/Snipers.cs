using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TgcViewer;
using TgcViewer.Example;
using TgcViewer.Utils;
using TgcViewer.Utils.Shaders;
using AlumnoEjemplos.CEGA.Units;
using AlumnoEjemplos.CEGA.Scenes;
using TgcViewer.Utils.TgcGeometry;
using System.Windows.Forms;

namespace AlumnoEjemplos.CEGA
{
    /// <summary>
    /// Ejemplo del alumno
    /// </summary>
    public class Snipers : TgcExample
    {
        /// <summary>
        /// Categoría a la que pertenece el ejemplo.
        /// Influye en donde se va a haber en el árbol de la derecha de la pantalla.
        /// </summary>
        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        /// <summary>
        /// Completar nombre del grupo en formato Grupo NN
        /// </summary>
        public override string getName()
        {
            return "CEGA";
        }

        /// <summary>
        /// Completar con la descripción del TP
        /// </summary>
        public override string getDescription()
        {
            return "TP Snipers";
        }

        PlayScene playScene;
        Player player;
        EnemigosAdmin enemigosAdmin;
        
        // Post-procesamiento
        //
        VertexBuffer vbPostProcessMesh;
        Effect postProcessEffect;
        Surface preDepthStencil;
        Texture preTargetTexture;

        int stabilize = 3;

        // hack: manejo de escenas
        VideoScene video;

        /// <summary>
        /// Define el estado de los efectos de post-procesamiento.
        /// </summary>
        public struct PostProcessEffects
        {
            public bool LensDistortion;
            public float LensRadius;
        };

        public PostProcessEffects PostProcessing;

        /// <summary>
        /// Método que se llama una sola vez,  al principio cuando se ejecuta el ejemplo.
        /// Escribir aquí todo el código de inicialización: cargar modelos, texturas, modifiers, uservars, etc.
        /// Borrar todo lo que no haga falta
        /// </summary>
        /// 
        public override void init()
        {
            Control panel = GuiController.Instance.Panel3d;

            // Corrige el near plane
            GuiController.Instance.D3dDevice.Transform.Projection =
                Matrix.PerspectiveFovLH(Geometry.DegreeToRadian(45.0f),
                (float)panel.Width / panel.Height, 0.5f, 10000.0f);

            playScene = new PlayScene();
            player = new Player();
            enemigosAdmin = new EnemigosAdmin(playScene);

            PostProcessing = new PostProcessEffects();
            PostProcessing.LensDistortion = false;
            PostProcessing.LensRadius = 0.3f;

            ColisionesAdmin.Instance.jugador = player;
            ColisionesAdmin.Instance.escenario = playScene;
            ColisionesAdmin.Instance.enemigos = enemigosAdmin;

            SetupPostProcessing();

            //Modifiers
            GuiController.Instance.Modifiers.addBoolean("showBB", "Mostrar BoundingBoxes", false);
            GuiController.Instance.Modifiers.addBoolean("showGrilla", "Mostrar Grilla", false);

            video = new VideoScene(GuiController.Instance.AlumnoEjemplosMediaDir + @"Video\CEGA",
                "jpg", 24.0f, 196);

            video.Playing = true;
        }

        private void SetupPostProcessing()
        {
            GuiController.Instance.CustomRenderEnabled = true;

            Device d3dDevice = GuiController.Instance.D3dDevice;

            // Define un quad que cubre toda la pantalla, para hacer post-procesamiento
            CustomVertex.PositionTextured[] screenVertices = new CustomVertex.PositionTextured[]
            {
                new CustomVertex.PositionTextured(-1, 1, 1, 0, 0), 
                new CustomVertex.PositionTextured( 1, 1, 1, 1, 0),
                new CustomVertex.PositionTextured(-1,-1, 1, 0, 1),
                new CustomVertex.PositionTextured( 1,-1, 1, 1, 1)
            };

            vbPostProcessMesh = new VertexBuffer(
                typeof(CustomVertex.PositionTextured), 4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
                CustomVertex.PositionTextured.Format, Pool.Default);

            vbPostProcessMesh.SetData(screenVertices, 0, LockFlags.None);

            // Textura donde se renderea la escena en el paso 1.
            preTargetTexture = new Texture(
                d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth,
                d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                Format.X8R8G8B8, Pool.Default);

            // Depth stencil usado en el pre-rendering. No tiene multisampling.
            preDepthStencil = d3dDevice.CreateDepthStencilSurface(
                d3dDevice.PresentationParameters.BackBufferWidth, d3dDevice.PresentationParameters.BackBufferHeight,
                DepthFormat.D24S8, MultiSampleType.None, 0, true);

            postProcessEffect = TgcShaders.loadEffect(
                GuiController.Instance.AlumnoEjemplosMediaDir + "Shaders\\PostProcess.fx");
        }


        /// <summary>
        /// Método que se llama cada vez que hay que refrescar la pantalla.
        /// Escribir aquí todo el código referido al renderizado.
        /// Borrar todo lo que no haga falta
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        /// 
        public override void render(float elapsedTime)
        {
            // El tiempo avanza durante la carga, estabilizarlo por tres frames.
            if (stabilize > 0)
            {
                stabilize--;
                return;
            }

            Device d3dDevice = GuiController.Instance.D3dDevice;

            // Render al pre target
            //
            Surface postTarget = d3dDevice.GetRenderTarget(0);
            Surface postDepthStencil = d3dDevice.DepthStencilSurface;
            Surface preTarget = preTargetTexture.GetSurfaceLevel(0);

            // hack: scene management
            if (video.Playing)
            {
                video.Update(elapsedTime);
            }
            else
            {
                playScene.Update(elapsedTime);
                player.Update(elapsedTime);
                enemigosAdmin.Update(elapsedTime);
            }

            d3dDevice.SetRenderTarget(0, preTarget);
            d3dDevice.DepthStencilSurface = preDepthStencil;
            SetupFog(d3dDevice, Color.Gray, -20.0f, 2600.0f);
            //SetupFog(d3dDevice, Color.Gray, 0.5f, 0.8f);

            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            
            RenderScene(d3dDevice);

            preTarget.Dispose();

            d3dDevice.RenderState.FogEnable = false;

            // para debugear:
            //TextureLoader.Save(GuiController.Instance.AlumnoEjemplosMediaDir + "Shaders\\render_target.bmp",
            //                      ImageFileFormat.Bmp, preTargetTexture);

            // Render al post target
            //
            d3dDevice.SetRenderTarget(0, postTarget);
            d3dDevice.DepthStencilSurface = postDepthStencil;

            PostProcess(d3dDevice);

            // Render de UI
            //
            RenderUI(d3dDevice);
        }

        void SetupFog(Device device, Color color, float start, float end)
        {
            device.RenderState.FogEnable = true;
            device.RenderState.FogColor = color;
            device.RenderState.FogTableMode = FogMode.Linear;
            device.RenderState.FogStart = start;
            device.RenderState.FogEnd = end;
        }

        private void RenderUI(Device d3dDevice)
        {
            playScene.RenderUI(this);
            player.RenderUI(this);

            if (video.Playing)
                video.RenderUI(this);
        }

        /// <summary>
        /// Aplica efectos de post-procesamiento
        /// </summary>
        /// <param name="d3dDevice"></param>
        private void PostProcess(Device d3dDevice)
        {
            d3dDevice.BeginScene();

            d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
            d3dDevice.SetStreamSource(0, vbPostProcessMesh, 0);

            // \TODO JJ: multiples efectos
            if (PostProcessing.LensDistortion)
            {
                postProcessEffect.Technique = "LensDistortion";
                postProcessEffect.SetValue("lens_radius", PostProcessing.LensRadius);

                Size screen = GuiController.Instance.Panel3d.Size;

                postProcessEffect.SetValue("screen_w", screen.Width);
                postProcessEffect.SetValue("screen_h", screen.Height);
                postProcessEffect.SetValue("screen_center_x", screen.Width / 2);
                postProcessEffect.SetValue("screen_center_y", screen.Height / 2);
            }
            else
                postProcessEffect.Technique = "NoEffect";

            postProcessEffect.SetValue("pre_render", preTargetTexture);

            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            postProcessEffect.Begin(FX.None);
            postProcessEffect.BeginPass(0);

            d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

            postProcessEffect.EndPass();
            postProcessEffect.End();

            d3dDevice.EndScene();
        }

        private void RenderScene(Device d3dDevice)
        {
            d3dDevice.BeginScene();

            GuiController.Instance.Text3d.drawText(
                "FPS: " + HighResolutionTimer.Instance.FramesPerSecond, 0, 0, Color.Yellow);

            GuiController.Instance.AxisLines.render();

            playScene.Render(this);
            enemigosAdmin.Render(this);
            player.Render(this);

            d3dDevice.EndScene();
        }

        public void GameOver()
        {
            GuiController.Instance.MainForm.Close();
        }

        /// <summary>
        /// Método que se llama cuando termina la ejecución del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void close()
        {
            playScene.Dispose();
            player.Dispose();
            vbPostProcessMesh.Dispose();
            preTargetTexture.Dispose();
            postProcessEffect.Dispose();
            preDepthStencil.Dispose();
            enemigosAdmin.Dispose();
        }

    }
}
