﻿using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcSkeletalAnimation;


namespace AlumnoEjemplos.CEGA
{
    /// <summary>
    /// Representa al administrador de enemigos.
    /// </summary>
    class EnemigosAdmin : IRenderable, IUpdatable
    {

        Vector3 limiteTerrenoInferior;
        Vector3 limiteTerrenoSuperior;
        double spawnEnemigos = 3;
        double velocidadSpawn = 0.05;
        double aceleracionSpawn = 0.001;
        Random randomEnemigosAdmin = new Random();

        List<Enemigo> listaEnemigos = new List<Enemigo>();

        public EnemigosAdmin(PlayScene playScene)
        {
            limiteTerrenoInferior = playScene.limitesTerreno().PMin;
            limiteTerrenoSuperior = playScene.limitesTerreno().PMax;

            int i;
            for(i = 0; i < (int)spawnEnemigos; i++)
            {
                this.AgregarEnemigo();
            }
        }

        private void AgregarEnemigo()
        {
            Vector3 posicion = this.PosicionEnemigo();
            Enemigo enemigo = new Enemigo(posicion);
            listaEnemigos.Add(enemigo);
        }

        private Vector3 PosicionEnemigo()
        {
            Vector3 posicion;
            bool posicionEnTerreno;

            do
            {
                // hacer un spawn random del enemigo dentro de una distancia del jugador
                Vector3 posicionPlayer = GuiController.Instance.CurrentCamera.getPosition();

                int distanciaRandom = randomEnemigosAdmin.Next(80, 150);
                float anguloRandom = (randomEnemigosAdmin.Next(0, 100) / 50) * FastMath.PI;

                double posicionX = distanciaRandom * Math.Cos(anguloRandom);
                double posicionZ = distanciaRandom * Math.Sin(anguloRandom);

                posicion = new Vector3((int)(posicionPlayer.X + posicionX), 0, (int)(posicionPlayer.Z + posicionZ));

                posicionEnTerreno = posicion.X > limiteTerrenoInferior.X;
                posicionEnTerreno = posicion.Z > limiteTerrenoInferior.Z;
                posicionEnTerreno = posicion.X < limiteTerrenoSuperior.X;
                posicionEnTerreno = posicion.Z < limiteTerrenoSuperior.Z;

            } while (posicionEnTerreno == false);

            return posicion;
        }

        public void Update(float elapsedTime)
        {
            int spawnEnemigosOld = (int)spawnEnemigos;
            velocidadSpawn += aceleracionSpawn * elapsedTime;
            spawnEnemigos += velocidadSpawn * elapsedTime;
            int restaSpawnEnemigos = (int)spawnEnemigos - spawnEnemigosOld;

            for (int i = 0; i < restaSpawnEnemigos; i++)
            {
                this.AgregarEnemigo();
            }

            foreach (Enemigo enemigo in listaEnemigos)
            {
                enemigo.Update(elapsedTime);
            }
        }
    
        public void Render(Snipers scene)
        {
            foreach (Enemigo enemigo in listaEnemigos)
            {
                enemigo.Render(scene);
            }
        }

        public void RenderUI(Snipers scene)
        {
        }

        public void Dispose()
        {
            foreach (Enemigo enemigo in listaEnemigos)
            {
                enemigo.Dispose();
            }
        }
    
    }

}
