﻿using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;
using System.Drawing;

namespace AlumnoEjemplos.CEGA.Units
{
    /// <summary>
    /// Herramienta para crear y usar la Grilla Regular
    /// </summary>
    public class GrillaRegular
    {
        // Tamaños de celda de la grilla
        readonly float CELL_SIZE = 200;

        List<TgcMesh> modelos;
        TgcBoundingBox sceneBounds;
        List<TgcDebugBox> debugBoxes;
        GrillaRegularNode[, ,] grid;

        public GrillaRegular()
        {
        }

        /// <summary>
        /// Crear una nueva grilla
        /// </summary>
        /// <param name="modelos">Modelos a contemplar</param>
        /// <param name="sceneBounds">Límites del escenario</param>
        public void create(List<TgcMesh> modelos, TgcBoundingBox sceneBounds)
        {
            this.modelos = modelos;
            this.sceneBounds = sceneBounds;

            //build
            grid = buildGrid(modelos, sceneBounds, new Vector3(CELL_SIZE, CELL_SIZE, CELL_SIZE));

            foreach (TgcMesh mesh in modelos)
            {
                mesh.Enabled = false;
            }
        }

        /// <summary>
        /// Construye la grilla
        /// </summary>
        private GrillaRegularNode[, ,] buildGrid(List<TgcMesh> modelos, TgcBoundingBox sceneBounds, Vector3 cellDim)
        {
            Vector3 sceneSize = sceneBounds.calculateSize();

            int gx = (int)FastMath.Ceiling(sceneSize.X / cellDim.X) + 1;
            int gy = (int)FastMath.Ceiling(sceneSize.Y / cellDim.Y) + 1;
            int gz = (int)FastMath.Ceiling(sceneSize.Z / cellDim.Z) + 1;

            GrillaRegularNode[, ,] grid = new GrillaRegularNode[gx, gy, gz];

            //Construir grilla
            for (int x = 0; x < gx; x++)
            {
                for (int y = 0; y < gy; y++)
                {
                    for (int z = 0; z < gz; z++)
                    {
                        //Crear celda
                        GrillaRegularNode node = new GrillaRegularNode();

                        //Crear BoundingBox de celda
                        Vector3 pMin = new Vector3(sceneBounds.PMin.X + x * cellDim.X, sceneBounds.PMin.Y + y * cellDim.Y, sceneBounds.PMin.Z + z * cellDim.Z);
                        Vector3 pMax = Vector3.Add(pMin, cellDim);
                        node.BoundingBox = new TgcBoundingBox(pMin, pMax);

                        //Cargar modelos en celda
                        node.Models = new List<TgcMesh>();
                        addModelsToCell(node, modelos, new Vector3(x,y,z));

                        grid[x, y, z] = node;
                    }
                }
            }

            return grid;
        }

        /// <summary>
        /// Agregar modelos a una celda
        /// </summary>
        private void addModelsToCell(GrillaRegularNode node, List<TgcMesh> modelos, Vector3 posicion)
        {
            foreach (TgcMesh mesh in modelos)
            {
                if (TgcCollisionUtils.testAABBAABB(node.BoundingBox, mesh.BoundingBox))
                {
                    node.Models.Add(mesh);
                    string gid = posicion.X.ToString()
                        + "." + posicion.Y.ToString() 
                        + "." + posicion.Z.ToString();
                    if (mesh.UserProperties.ContainsKey("gid") == true)
                        mesh.UserProperties["gid"] = mesh.UserProperties["gid"] + "+" + gid;
                    else
                        mesh.UserProperties.Add("gid", gid);
                }
            }
        }

        /// <summary>
        /// Crear meshes debug
        /// </summary>
        public void createDebugMeshes()
        {
            debugBoxes = new List<TgcDebugBox>();

            for (int x = 0; x < grid.GetUpperBound(0); x++)
            {
                for (int y = 0; y < grid.GetUpperBound(1); y++)
                {
                    for (int z = 0; z < grid.GetUpperBound(2); z++)
                    {
                        GrillaRegularNode node = grid[x, y, z];
                        TgcDebugBox box = TgcDebugBox.fromExtremes(node.BoundingBox.PMin, node.BoundingBox.PMax, Color.Red);

                        debugBoxes.Add(box);
                    }
                }
            }
        }

        public List<TgcDebugBox> DebugGrillaBoxes()
        {
            return debugBoxes;
        }

        public List<GrillaRegularNode> NodosCercanos(TgcBoundingBox bb)
        {
            List<GrillaRegularNode> listaDeNodos = new List<GrillaRegularNode>();
            
            int min_x = (int)((bb.PMin.X - sceneBounds.PMin.X) / CELL_SIZE);
            int min_y = (int)((bb.PMin.Y - sceneBounds.PMin.Y) / CELL_SIZE);
            int min_z = (int)((bb.PMin.Z - sceneBounds.PMin.Z) / CELL_SIZE);

            int max_x = (int)((bb.PMax.X - sceneBounds.PMin.X) / CELL_SIZE);
            int max_y = (int)((bb.PMax.Y - sceneBounds.PMin.Y) / CELL_SIZE);
            int max_z = (int)((bb.PMax.Z - sceneBounds.PMin.Z) / CELL_SIZE);

            listaDeNodos.Add(grid[min_x, min_y, min_z]);

            if (min_x != max_x || min_y != max_y || min_z != max_z)
                listaDeNodos.Add(grid[max_x, max_y, max_z]);

            return listaDeNodos;
        }

        public void BorrarModelo(TgcMesh modelo)
        {
            string[] grillas;
            string[] posicion;
            int x;
            int y;
            int z;

            grillas = modelo.UserProperties["gid"].Split('+');

            foreach (string grilla in grillas)
            {
                posicion = grilla.Split('.');
                x = Convert.ToInt32(posicion[0]);
                y = Convert.ToInt32(posicion[1]);
                z = Convert.ToInt32(posicion[2]);

                GrillaRegularNode nodo = grid[x, y, z];

                for (int i = 0; i < nodo.Models.Count; i++)
                {
                    if (nodo.Models[i] == modelo)
                        nodo.Models.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Activar modelos dentro de celdas visibles
        /// </summary>
        public void UpdateVisibleMeshes(TgcFrustum frustum)
        {
            for (int x = 0; x < grid.GetUpperBound(0); x++)
            {
                for (int y = 0; y < grid.GetUpperBound(1); y++)
                {
                    for (int z = 0; z < grid.GetUpperBound(2); z++)
                    {
                        GrillaRegularNode node = grid[x, y, z];
                        TgcCollisionUtils.FrustumResult r = TgcCollisionUtils.classifyFrustumAABB(frustum, node.BoundingBox);

                        if (r != TgcCollisionUtils.FrustumResult.OUTSIDE)
                        {
                            node.activateCellMeshes();
                        }
                    }
                }
            }
        }


    }
}
