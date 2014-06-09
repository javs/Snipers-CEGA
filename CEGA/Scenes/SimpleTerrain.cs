using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Shaders;
using TgcViewer;

namespace AlumnoEjemplos.CEGA.Scenes
{
    /// <summary>
    /// Permite crear la malla de un terreno en base a una textura de Heightmap
    /// </summary>
    public class SimpleTerrain : IRenderObject
    {
        VertexBuffer vbTerrain;
        Texture terrainTexture;
        int totalVertices;

        CustomVertex.PositionTextured[] data;

        int[,] heightmapData;
        /// <summary>
        /// Valor de Y para cada par (X,Z) del Heightmap
        /// </summary>
        public int[,] HeightmapData
        {
            get { return heightmapData; }
        }

        private bool enabled;
        /// <summary>
        /// Indica si la malla esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        private Vector3 center;
        /// <summary>
        /// Centro del terreno
        /// </summary>
        public Vector3 Center
        {
            get { return center; }
        }

        private bool alphaBlendEnable;
        /// <summary>
        /// Habilita el renderizado con AlphaBlending para los modelos
        /// con textura o colores por vértice de canal Alpha.
        /// Por default está deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable
        {
            get { return alphaBlendEnable; }
            set { alphaBlendEnable = value; }
        }

        protected Effect effect;
        /// <summary>
        /// Shader del mesh
        /// </summary>
        public Effect Effect
        {
            get { return effect; }
            set { effect = value; }
        }

        protected string technique;
        /// <summary>
        /// Technique que se va a utilizar en el effect.
        /// Cada vez que se llama a render() se carga este Technique (pisando lo que el shader ya tenia seteado)
        /// </summary>
        public string Technique
        {
            get { return technique; }
            set { technique = value; }
        }

        private TgcBoundingBox boundingBox;

        public TgcBoundingBox BoundingBox
        {
            get { return boundingBox; }
        }


        public SimpleTerrain()
        {
            enabled = true;
            alphaBlendEnable = false;

            //BoundingBox
            boundingBox = new TgcBoundingBox();

            //Shader
            this.effect = GuiController.Instance.Shaders.VariosShader;
            this.technique = TgcShaders.T_POSITION_TEXTURED;
        }




        /// <summary>
        /// Crea la malla de un terreno en base a un Heightmap
        /// </summary>
        /// <param name="heightmapPath">Imagen de Heightmap</param>
        /// <param name="scaleXZ">Escala para los ejes X y Z</param>
        /// <param name="scaleY">Escala para el eje Y</param>
        /// <param name="center">Centro de la malla del terreno</param>
        public void loadHeightmap(string heightmapPath, float scaleXZ, float scaleY, Vector3 center)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            this.center = center;

            //Dispose de VertexBuffer anterior, si habia
            if (vbTerrain != null && !vbTerrain.Disposed)
            {
                vbTerrain.Dispose();
            }

            //cargar heightmap
            heightmapData = loadHeightMap(d3dDevice, heightmapPath);
            float width = (float)heightmapData.GetLength(0);
            float length = (float)heightmapData.GetLength(1);


            //Crear vertexBuffer
            totalVertices = 2 * 3 * (heightmapData.GetLength(0) - 1) * (heightmapData.GetLength(1) - 1);
            vbTerrain = new VertexBuffer(typeof(CustomVertex.PositionTextured), totalVertices, d3dDevice, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);

            //Cargar vertices
            int dataIdx = 0;
            data = new CustomVertex.PositionTextured[totalVertices];

            center.X = center.X * scaleXZ - (width / 2) * scaleXZ;
            center.Y = center.Y * scaleY;
            center.Z = center.Z * scaleXZ - (length / 2) * scaleXZ;

            for (int i = 0; i < width - 1; i += 1)
            {
                for (int j = 0; j < length - 1; j += 1)
                {

                    for (int ti = 0; ti < 1; ti++)
                    {
                        for (int tj = 0; tj < 1; tj++)
                        {
                            
                            if(  (ti == 0 && i == 149) || (tj == 0 &&  j ==149)   )
                            { 
                            }
                            else {

                            
                            // Vertices
                            //Vector3 v1 = new Vector3(center.X + i * scaleXZ, center.Y + heightmapData[i, j] * scaleY, center.Z + j * scaleXZ);
                            //Vector3 v2 = new Vector3(center.X + i * scaleXZ, center.Y + heightmapData[i, j + 1] * scaleY, center.Z + (j + 1) * scaleXZ);
                            //Vector3 v3 = new Vector3(center.X + (i + 1) * scaleXZ, center.Y + heightmapData[i + 1, j] * scaleY, center.Z + j * scaleXZ);
                            //Vector3 v4 = new Vector3(center.X + (i + 1) * scaleXZ, center.Y + heightmapData[i + 1, j + 1] * scaleY, center.Z + (j + 1) * scaleXZ);

                            //Vertices
                            Vector3 v1 = new Vector3(center.X + (ti + i) * scaleXZ, center.Y + heightmapData[(ti + i), (tj + j)] * scaleY, center.Z + (tj + j) * scaleXZ);
                            Vector3 v2 = new Vector3(center.X + (ti + i) * scaleXZ, center.Y + heightmapData[(ti + i), (tj + j) + 1] * scaleY, center.Z + ((tj + j) + 1) * scaleXZ);
                            Vector3 v3 = new Vector3(center.X + ((ti + i) + 1) * scaleXZ, center.Y + heightmapData[(ti + i) + 1, (tj + j)] * scaleY, center.Z + (tj + j) * scaleXZ);
                            Vector3 v4 = new Vector3(center.X + ((ti + i) + 1) * scaleXZ, center.Y + heightmapData[(ti + i) + 1, (tj + j) + 1] * scaleY, center.Z + ((tj + j) + 1) * scaleXZ);

                            ////Coordendas de textura
                            //Vector2 t1 = new Vector2(i / width, j / length);
                            //Vector2 t2 = new Vector2(i / width, (j + 1) / length);
                            //Vector2 t3 = new Vector2((i + 1) / width, j / length);
                            //Vector2 t4 = new Vector2((i + 1) / width, (j + 1) / length);

                            Vector2 t1 = new Vector2(ti / (width / 150), tj / (length / 150));
                            Vector2 t2 = new Vector2(ti / (width / 150), (tj + 1) / (length / 150));
                            Vector2 t3 = new Vector2((ti + 1) / (width / 150), tj / (length / 150));
                            Vector2 t4 = new Vector2((ti + 1) / (width / 150), (tj + 1) / (length / 150));


                            //Cargar triangulo 1
                            data[dataIdx] = new CustomVertex.PositionTextured(v1, t1.X, t1.Y);
                            data[dataIdx + 1] = new CustomVertex.PositionTextured(v2, t2.X, t2.Y);
                            data[dataIdx + 2] = new CustomVertex.PositionTextured(v4, t4.X, t4.Y);

                            //Cargar triangulo 2
                            data[dataIdx + 3] = new CustomVertex.PositionTextured(v1, t1.X, t1.Y);
                            data[dataIdx + 4] = new CustomVertex.PositionTextured(v4, t4.X, t4.Y);
                            data[dataIdx + 5] = new CustomVertex.PositionTextured(v3, t3.X, t3.Y);

                            dataIdx += 6;
                          } 
                        }
                    }
                }


            }

            Vector3 min;
            min.X = center.X + 200;
            min.Y = center.Y;
            min.Z = center.Z + 200;

            Vector3 max;
            max.X = center.X + (length - 10) * scaleXZ;
            max.Y = center.Y + 200;
            max.Z = center.Z + width * scaleXZ;

            this.boundingBox.setExtremes(min, max);

            vbTerrain.SetData(data, 0, LockFlags.None);
        }

        /// <summary>
        /// Carga la textura del terreno
        /// </summary>
        public void loadTexture(string path)
        {
            //Dispose textura anterior, si habia
            if (terrainTexture != null && !terrainTexture.Disposed)
            {
                terrainTexture.Dispose();
            }

            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Rotar e invertir textura
            Bitmap b = (Bitmap)Bitmap.FromFile(path);
            b.RotateFlip(RotateFlipType.Rotate90FlipX);
            terrainTexture = Texture.FromBitmap(d3dDevice, b, Usage.None, Pool.Managed);
        }

        public float outOfBound(float x, int max)
        {
            if (x <= 0)
                return 0;
            if (x >= max)
                return max;
            return x;
        }


        public float obtenerAltura(float x, float z, float ScaleXZ)
        {
            int tamanio = heightmapData.GetLength(1);
            float largo = ScaleXZ * tamanio;

            float pos_i = outOfBound(tamanio * (0.5f + x / largo), (tamanio - 2));
            float pos_j = outOfBound(tamanio * (0.5f + z / largo), (tamanio - 2));

            int pi = (int)pos_i;
            float fracc_i = pos_i - pi;
            int pj = (int)pos_j;
            float fracc_j = pos_j - pj;

            // Promedio ponderado entre los 2x2 puntos: 
            float H0 = data[((pj + 0) + (pi + 0) * tamanio)].Y;
            float H1 = data[((pj + 0) + (pi + 1) * tamanio)].Y;
            float H2 = data[((pj + 1) + (pi + 0) * tamanio)].Y;
            float H3 = data[((pj + 1) + (pi + 1) * tamanio)].Y;

            float H = (H0 * (1 - fracc_i) + H1 * fracc_i) * (1 - fracc_j) +
                      (H2 * (1 - fracc_i) + H3 * fracc_i) * fracc_j;
            return H;
        }


        /// <summary>
        /// Carga los valores del Heightmap en una matriz
        /// </summary>
        protected int[,] loadHeightMap(Device d3dDevice, string path)
        {
            Bitmap bitmap = (Bitmap)Bitmap.FromFile(path);
            int width = bitmap.Size.Width;
            int height = bitmap.Size.Height;
            int[,] heightmap = new int[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    //(j, i) invertido para primero barrer filas y despues columnas
                    Color pixel = bitmap.GetPixel(j, i);
                    float intensity = pixel.R * 0.299f + pixel.G * 0.587f + pixel.B * 0.114f;
                    heightmap[i, j] = (int)intensity;
                }

            }

            bitmap.Dispose();
            return heightmap;
        }


        /// <summary>
        /// Renderiza el terreno
        /// </summary>
        public void render()
        {
            if (!enabled)
                return;

            Device d3dDevice = GuiController.Instance.D3dDevice;
            TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;

            //Textura
            effect.SetValue("texDiffuseMap", terrainTexture);
            texturesManager.clear(1);

            GuiController.Instance.Shaders.setShaderMatrix(this.effect, Matrix.Identity);
            d3dDevice.VertexDeclaration = GuiController.Instance.Shaders.VdecPositionTextured;
            effect.Technique = this.technique;
            d3dDevice.SetStreamSource(0, vbTerrain, 0);

            //Render con shader
            effect.Begin(0);
            effect.BeginPass(0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, totalVertices / 3);
            effect.EndPass();
            effect.End();

        }

        public Vector3 Position
        {
            get { return center; }
        }

        /// <summary>
        /// Libera los recursos del Terreno
        /// </summary>
        public void dispose()
        {
            if (vbTerrain != null)
            {
                vbTerrain.Dispose();
            }

            if (terrainTexture != null)
            {
                terrainTexture.Dispose();
            }
            boundingBox.dispose();
        }

    }
}
