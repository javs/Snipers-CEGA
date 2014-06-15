using AlumnoEjemplos.CEGA.Interfaces;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.Sound;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.CEGA.Scenes
{
    class VideoScene : IRenderable, IUpdatable
    {
        public bool Playing { get; set; }

        readonly float FPS;
        readonly int TotalFrames;

        int current_frame;
        TgcTexture[] frames;
        float time;
        TgcSprite sprite;

        TgcStaticSound sound;

        public VideoScene(string directory, string format, float fps, int total_frames)
        {
            FPS = fps;
            TotalFrames = total_frames;

            current_frame = 0;
            time = 0.0f;

            frames = new TgcTexture[TotalFrames];

            for (int i = 1; i <= TotalFrames; i++)
            {
                frames[i - 1] = TgcTexture.createTexture(directory + @"\" + i.ToString("D4") + "." + format);
            }

            sprite = new TgcSprite();
            sprite.Texture = frames[current_frame];

            Size screenSize = GuiController.Instance.Panel3d.Size;
            Size textureSize = sprite.Texture.Size;

            sprite.Scaling = new Vector2(
                (float)screenSize.Width / textureSize.Width,
                (float)screenSize.Height / textureSize.Height);

            sprite.Position = new Vector2(
                FastMath.Max(screenSize.Width / 2 - textureSize.Width * sprite.Scaling.X / 2, 0),
                FastMath.Max(screenSize.Height / 2 - textureSize.Height * sprite.Scaling.Y / 2, 0));

            sound = new TgcStaticSound();
            sound.loadSound(directory + @"\sound.wav");

            Playing = false;
        }

        public void Update(float elapsedTime)
        {
            if (!Playing)
                return;

            time += elapsedTime;
            
            int last_frame = current_frame;

            current_frame = (int)(time * FPS);

            // muy lento, compensar
            if (current_frame - last_frame > 2)
                current_frame = last_frame + 2;

            if (current_frame >= TotalFrames)
            {
                Playing = false;
                sound.stop();
            }
            else
                sprite.Texture = frames[current_frame];
        }

        public void Render(Snipers scene)
        {
        }

        public void RenderUI(Snipers scene)
        {
            if (Playing)
                sound.play();

            GuiController.Instance.D3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            GuiController.Instance.Drawer2D.beginDrawSprite();
            sprite.render();
            GuiController.Instance.Drawer2D.endDrawSprite();
        }

        public void Dispose()
        {
            sprite.dispose();

            foreach (var item in frames)
            {
                item.dispose();
            }
        }
    }
}
