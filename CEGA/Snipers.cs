using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;

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

        /// <summary>
        /// Método que se llama una sola vez,  al principio cuando se ejecuta el ejemplo.
        /// Escribir aquí todo el código de inicialización: cargar modelos, texturas, modifiers, uservars, etc.
        /// Borrar todo lo que no haga falta
        /// </summary>
        /// 

        TgcBox suelo;
        List<TgcMesh> meshes;
        TgcMesh pinoOriginal;
        TgcMesh sniperRifleMesh;

        Vector3 lookAtAnterior;


        public override void init()
        {
            //GuiController.Instance: acceso principal a todas las herramientas del Framework

            //Device de DirectX para crear primitivas
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Carpeta de archivos Media del alumno
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;

            //////ESCENARIO///////

            TgcTexture pisoTexture = TgcTexture.createTexture(d3dDevice, alumnoMediaFolder + "Textures\\Grass.jpg");
            suelo = TgcBox.fromSize(new Vector3(1100, 0, 1100), new Vector3(2600, 0, 2600), pisoTexture);

            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(alumnoMediaFolder + "\\Pino-TgcScene.xml");

            pinoOriginal = scene.Meshes[0];

            //Crear varias instancias del modelo original, pero sin volver a cargar el modelo entero cada vez, hago 23*23 = 529 pinos
            int rows = 23;
            int cols = 23;
            float offset = 100;
            meshes = new List<TgcMesh>();
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

                    meshes.Add(instance);
                }
            }


            ////////SNIPER////////

            //Sniper

            TgcSceneLoader loaderSniper = new TgcSceneLoader();
            TgcScene sniperRifle = loaderSniper.loadSceneFromFile(alumnoMediaFolder + "\\Sniper-TgcScene.xml"); //Este modelo no carga bien, ya le pregunte al tutor para ver cual puede ser el problema

            //De toda la escena solo nos interesa guardarnos el primer modelo (el único que hay en este caso).
            sniperRifleMesh = sniperRifle.Meshes[0];
            sniperRifleMesh.Position = new Vector3(125, 5, 125);
            sniperRifleMesh.AlphaBlendEnable = true;
            sniperRifleMesh.Scale = new Vector3(0.01F, 0.01F, 0.01F);





            ///////////////USER VARS//////////////////


            ///////////////MODIFIERS//////////////////

            
            ///////////////CONFIGURAR CAMARA PRIMERA PERSONA//////////////////
            //Camara en primera persona, tipo videojuego FPS
            //Solo puede haber una camara habilitada a la vez. Al habilitar la camara FPS se deshabilita la camara rotacional
            //Por default la camara FPS viene desactivada
            GuiController.Instance.FpsCamera.Enable = true;
            //Configurar posicion y hacia donde se mira
            GuiController.Instance.FpsCamera.setCamera(sniperRifleMesh.Position, new Vector3(0, 0, 0));
            //Velocidad
            GuiController.Instance.FpsCamera.Velocity = new Vector3(10, 10, 10);


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
            //Device de DirectX para renderizar
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Muevo el sniper con la camara
            sniperRifleMesh.Position = GuiController.Instance.FpsCamera.Position - new Vector3(0.5F,1,1.5F);

            //Roto el sniper segun donde mira la camara ESTO NO FUNCIONA BIEN 
            /* Calculo el angulo entre la posicion anterior y la posicion actual, y roto el mesh segun ese angulo
             * Luego actualizo la posicion anterior
             
            float angle = FastMath.Acos(Vector3.Dot(Vector3.Normalize(lookAtAnterior), Vector3.Normalize(GuiController.Instance.FpsCamera.LookAt - GuiController.Instance.FpsCamera.Position)));

            sniperRifleMesh.rotateY(angle);

            lookAtAnterior = GuiController.Instance.FpsCamera.LookAt - GuiController.Instance.FpsCamera.Position;*/

           ///////////////INPUT//////////////////

            //Capturar Input teclado 
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.F))
            {
                //Tecla F apretada
            }

            //Capturar Input Mouse
            if (GuiController.Instance.D3dInput.buttonPressed(TgcViewer.Utils.Input.TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Boton izq apretado
            }


            /////////RENDERS/////////

            //Renderizar suelo
            suelo.render();

            //Renderizo el Sniper
            sniperRifleMesh.render();
            //Renderizar instancias
            foreach (TgcMesh mesh in meshes)
            {
                mesh.render();
            }

        }



        /// <summary>
        /// Método que se llama cuando termina la ejecución del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void close()
        {

        }

    }
}
