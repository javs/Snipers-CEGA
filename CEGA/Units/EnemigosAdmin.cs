using Microsoft.DirectX;
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
using AlumnoEjemplos.CEGA.Interfaces;
using AlumnoEjemplos.CEGA.Scenes;

namespace AlumnoEjemplos.CEGA.Units
{
    /// <summary>
    /// Representa al administrador de enemigos.
    /// </summary>
    class EnemigosAdmin : IRenderable, IUpdatable
    {

        Vector3 limiteTerrenoInferior;
        Vector3 limiteTerrenoSuperior;
        double spawnEnemigos;
        double velocidadSpawn;
        double aceleracionSpawn;
        Random randomEnemigosAdmin = new Random();

        List<Enemigo> listaEnemigos = new List<Enemigo>();

        uint idEnemigo = 0;

        public EnemigosAdmin(PlayScene playScene)
        {
            limiteTerrenoInferior = playScene.BoundingBoxTerreno().PMin;
            limiteTerrenoSuperior = playScene.BoundingBoxTerreno().PMax;

            this.Inicializar();

        }

        private void AgregarEnemigo()
        {
            Vector3 posicion = this.PosicionEnemigo();
            Enemigo enemigo = new Enemigo(posicion);
            enemigo.id = idEnemigo;
            idEnemigo++;
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
                float anguloRandom = ((float)randomEnemigosAdmin.Next(0, 100) / 50) * FastMath.PI;

                float posicionX = distanciaRandom * FastMath.Cos(anguloRandom);
                float posicionZ = distanciaRandom * FastMath.Sin(anguloRandom);

                posicion = new Vector3((int)(posicionPlayer.X + posicionX), 0, (int)(posicionPlayer.Z + posicionZ));

                posicionEnTerreno = true;
                posicionEnTerreno = posicionEnTerreno && posicion.X > limiteTerrenoInferior.X;
                posicionEnTerreno = posicionEnTerreno && posicion.Z > limiteTerrenoInferior.Z;
                posicionEnTerreno = posicionEnTerreno && posicion.X < limiteTerrenoSuperior.X;
                posicionEnTerreno = posicionEnTerreno && posicion.Z < limiteTerrenoSuperior.Z;

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

        public List<Enemigo> listaDeEnemigos() {
            return listaEnemigos;
        }

        public List<Enemigo> listaDeEnemigosOrdenadaPorDistancia() {

            List<Enemigo> listaEnemigosOrdenada = new List<Enemigo>();
            SortedDictionary<float,Enemigo> diccionario = new SortedDictionary<float,Enemigo>();
            Vector3 posicionPlayer = GuiController.Instance.CurrentCamera.getPosition();

            foreach (Enemigo enemigo in this.listaDeEnemigos())
            {
                float distancia = FastMath.Pow2(enemigo.Position().X - posicionPlayer.X) + FastMath.Pow2(enemigo.Position().Z - posicionPlayer.Z);
                diccionario.Add(distancia, enemigo);
            }

            foreach (var entrada in diccionario) 
            {
                listaEnemigosOrdenada.Add(entrada.Value);
            }

            return listaEnemigosOrdenada;
        }

        public void MatarEnemigo(uint id)
        {
            int i = 0;
            foreach (Enemigo enemigo in this.listaDeEnemigos())
            {
                if ( enemigo.id == id )
                {
                    enemigo.Morir();
                    this.listaDeEnemigos().RemoveAt(i);
                    return;
                }
                i++;
            }
        }

        public void Inicializar()
        {
            spawnEnemigos = 3;
            velocidadSpawn = 0.05;
            aceleracionSpawn = 0.001;
            int i;
            for (i = 0; i < (int)spawnEnemigos; i++)
            {
                this.AgregarEnemigo();
            }
        }

        public void Reset()
        {
            listaEnemigos.Clear();
            this.Inicializar();
        }
    }

}
