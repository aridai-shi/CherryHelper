﻿// Celeste.ChooChooBlock
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.CherryHelper
{

    [CustomEntity("CherryHelper/ChooChoo")]
    public class ChooChooBlock : Solid
    {


        private class ChooChooBlockPathRenderer : Entity
        {
            public ChooChooBlock ChooChooBlock;

            private MTexture cog;

            private Vector2 sparkAdd;

            private Vector2 from;

            private Vector2 to;

            private float sparkDirFromA;

            private float sparkDirFromB;

            private float sparkDirToA;

            private float sparkDirToB;

            public ChooChooBlockPathRenderer(ChooChooBlock ChooChooey, Vector2 from, Vector2 to)
            {
                this.from = from;
                ChooChooBlock = ChooChooey;
                this.to = to;
                Depth = 5000;
                sparkAdd = (from - to).SafeNormalize(5f).Perpendicular();
                float num = (from - to).Angle();
                sparkDirFromA = num + (float)Math.PI / 8f;
                sparkDirFromB = num - (float)Math.PI / 8f;
                sparkDirToA = num + (float)Math.PI - (float)Math.PI / 8f;
                sparkDirToB = num + (float)Math.PI + (float)Math.PI / 8f;
                cog = GFX.Game["objects/choochooblock/cog"];
            }


            public override void Render()
            {
                DrawCogs(Vector2.UnitY, Color.Black);
                DrawCogs(Vector2.Zero);
                if (ChooChooBlock.drawBlackBorder)
                {
                    Draw.Rect(new Rectangle((int)(ChooChooBlock.X + ChooChooBlock.Shake.X - 1f), (int)(ChooChooBlock.Y + ChooChooBlock.Shake.Y - 1f), (int)ChooChooBlock.Width + 2, (int)ChooChooBlock.Height + 2), Color.Black);
                }
            }

            private void DrawCogs(Vector2 offset, Color? colorOverride = null)
            {
                Vector2 vector = (to - from).SafeNormalize();
                Vector2 value = vector.Perpendicular() * 3f;
                Vector2 value2 = -vector.Perpendicular() * 4f;
                float rotation = ChooChooBlock.percent * (float)Math.PI * 2f;
                Draw.Line(from + value + offset, to + value + offset, colorOverride.HasValue ? colorOverride.Value : ropeColor);
                Draw.Line(from + value2 + offset, to + value2 + offset, colorOverride.HasValue ? colorOverride.Value : ropeColor);
                for (float num = 4f - ChooChooBlock.percent * (float)Math.PI * 8f % 4f; num < (to - from).Length(); num += 4f)
                {
                    Vector2 value3 = from + value + vector.Perpendicular() + vector * num;
                    Vector2 value4 = to + value2 - vector * num;
                    Draw.Line(value3 + offset, value3 + vector * 2f + offset, colorOverride.HasValue ? colorOverride.Value : ropeLightColor);
                    Draw.Line(value4 + offset, value4 - vector * 2f + offset, colorOverride.HasValue ? colorOverride.Value : ropeLightColor);
                }
                cog.DrawCentered(from + offset, colorOverride.HasValue ? colorOverride.Value : Color.White, 1f, rotation);
                cog.DrawCentered(to + offset, colorOverride.HasValue ? colorOverride.Value : Color.White, 1f, rotation);
            }
        }

        public static ParticleType P_Scrape;


        private MTexture[,] edges = new MTexture[3, 3];

        private Sprite streetlight;

        private BloomPoint bloom;

        private ChooChooBlockPathRenderer pathRenderer;

        private List<MTexture> innerCogs;

        private MTexture temp = new MTexture();

        private bool drawBlackBorder;

        private Vector2 start;

        private Vector2[] targetNodes;

        private float percent = 0f;
        private int index;

        private static readonly Color ropeColor = Calc.HexToColor("663931");

        private static readonly Color ropeLightColor = Calc.HexToColor("9b6157");

        private SoundSource sfx = new SoundSource();

        public ChooChooBlock(Vector2 position, int width, int height, Vector2[] targetNodes)
            : base(position, width, height, safe: false)
        {
            Depth = -9999;
            index = 0;
            start = Position;
            this.targetNodes = targetNodes;
            Add(new Coroutine(Sequence()));
            Add(new LightOcclude());
            string path;
            string id;
            string key;
            path = "objects/choochooblock/light";
            id = "objects/choochooblock/block";
            key = "objects/choochooblock/innercog";
            drawBlackBorder = true;
            innerCogs = GFX.Game.GetAtlasSubtextures(key);
            Add(streetlight = new Sprite(GFX.Game, path));
            streetlight.Add("frames", "", 1f);
            streetlight.Play("frames");
            streetlight.Active = false;
            streetlight.SetAnimationFrame(0);
            streetlight.Position = new Vector2(Width / 2f - streetlight.Width / 2f, Height / 2f - streetlight.Height / 2f);
            Add(bloom = new BloomPoint(1f, 6f));
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    edges[i, j] = GFX.Game[id].GetSubtexture(i * 8, j * 8, 8, 8);
                }
            }
            SurfaceSoundIndex = 7;
            sfx.Position = new Vector2(Width, Height) / 2f;
            Add(sfx);
        }

        public ChooChooBlock(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Height, data.NodesWithPosition(offset))
        {
        }

        public void CreateSparks(Vector2 from, Vector2 to)
        {

            Vector2 sparkAdd = (from - to).SafeNormalize(5f).Perpendicular();
            float num = (from - to).Angle();
            Single sparkDirFromA = num + (float)Math.PI / 8f;
            Single sparkDirFromB = num - (float)Math.PI / 8f;
            Single sparkDirToA = num + (float)Math.PI - (float)Math.PI / 8f;
            Single sparkDirToB = num + (float)Math.PI + (float)Math.PI / 8f;
            SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, from + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromA);
            SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, from - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromB);
            SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, to + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToA);
            SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, to - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToB);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Vector2 lineOffset = new Vector2(Width, Height) / 2f;
            for (int i = 0; i < targetNodes.Length - 1; i++)
            {
                scene.Add(pathRenderer = new ChooChooBlockPathRenderer(this, targetNodes[i] + lineOffset, targetNodes[i + 1] + lineOffset));
            }
            scene.Add(pathRenderer = new ChooChooBlockPathRenderer(this, targetNodes[targetNodes.Length - 1] + lineOffset, targetNodes[0] + lineOffset));
        }

        public override void Removed(Scene scene)
        {
            scene.Remove(pathRenderer);
            pathRenderer = null;
            base.Removed(scene);
        }

        public override void Update()
        {
            base.Update();
            bloom.Visible = true;
            bloom.Y = streetlight.Position.Y + 24f;
            switch (streetlight.CurrentAnimationFrame)
            {
                case 0:
                    bloom.Visible = false;
                    break;
                case 1:
                    bloom.X = streetlight.X + 10f;
                    break;
                case 2:
                    bloom.X = streetlight.X + 23f;
                    break;
            }

        }

        public override void Render()
        {
            Vector2 position = Position;
            Position += Shake;
            Draw.Rect(X + 1f, Y + 1f, Width - 2f, Height - 2f, Color.Black);
            int num = 1;
            float num2 = 0f;
            int count = innerCogs.Count;
            for (int i = 4; (float)i <= Height - 4f; i += 8)
            {
                int num3 = num;
                for (int j = 4; (float)j <= Width - 4f; j += 8)
                {
                    int index = (int)(mod((num2 + (float)num * percent * (float)Math.PI * 4f) / ((float)Math.PI / 2f), 1f) * (float)count);
                    MTexture mTexture = innerCogs[index];
                    Rectangle rectangle = new Rectangle(0, 0, mTexture.Width, mTexture.Height);
                    Vector2 zero = Vector2.Zero;
                    if (j <= 4)
                    {
                        zero.X = 2f;
                        rectangle.X = 2;
                        rectangle.Width -= 2;
                    }
                    else if ((float)j >= Width - 4f)
                    {
                        zero.X = -2f;
                        rectangle.Width -= 2;
                    }
                    if (i <= 4)
                    {
                        zero.Y = 2f;
                        rectangle.Y = 2;
                        rectangle.Height -= 2;
                    }
                    else if ((float)i >= Height - 4f)
                    {
                        zero.Y = -2f;
                        rectangle.Height -= 2;
                    }
                    mTexture = mTexture.GetSubtexture(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, temp);
                    mTexture.DrawCentered(Position + new Vector2(j, i) + zero, Color.White * ((num < 0) ? 0.5f : 1f));
                    num = -num;
                    num2 += (float)Math.PI / 3f;
                }
                if (num3 == num)
                {
                    num = -num;
                }
            }
            for (int k = 0; (float)k < Width / 8f; k++)
            {
                for (int l = 0; (float)l < Height / 8f; l++)
                {
                    int num4 = (k != 0) ? (((float)k != Width / 8f - 1f) ? 1 : 2) : 0;
                    int num5 = (l != 0) ? (((float)l != Height / 8f - 1f) ? 1 : 2) : 0;
                    if (num4 != 1 || num5 != 1)
                    {
                        edges[num4, num5].Draw(new Vector2(X + (float)(k * 8), Y + (float)(l * 8)));
                    }
                }
            }
            base.Render();
            Position = position;
        }

        private void ScrapeParticlesCheck(Vector2 to)
        {
            if (!Scene.OnInterval(0.03f))
            {
                return;
            }
            bool flag = to.Y != ExactPosition.Y;
            bool flag2 = to.X != ExactPosition.X;
            if (flag && !flag2)
            {
                int num = Math.Sign(to.Y - ExactPosition.Y);
                Vector2 value = (num != 1) ? TopLeft : BottomLeft;
                int num2 = 4;
                if (num == 1)
                {
                    num2 = Math.Min((int)Height - 12, 20);
                }
                int num3 = (int)Height;
                if (num == -1)
                {
                    num3 = Math.Max(16, (int)Height - 16);
                }
                if (Scene.CollideCheck<Solid>(value + new Vector2(-2f, num * -2)))
                {
                    for (int i = num2; i < num3; i += 8)
                    {
                        SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, TopLeft + new Vector2(0f, (float)i + (float)num * 2f), (num == 1) ? (-(float)Math.PI / 4f) : ((float)Math.PI / 4f));
                    }
                }
                if (Scene.CollideCheck<Solid>(value + new Vector2(Width + 2f, num * -2)))
                {
                    for (int j = num2; j < num3; j += 8)
                    {
                        SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, TopRight + new Vector2(-1f, (float)j + (float)num * 2f), (num == 1) ? ((float)Math.PI * -3f / 4f) : ((float)Math.PI * 3f / 4f));
                    }
                }
            }
            else
            {
                if (!flag2 || flag)
                {
                    return;
                }
                int num4 = Math.Sign(to.X - ExactPosition.X);
                Vector2 value2 = (num4 != 1) ? TopLeft : TopRight;
                int num5 = 4;
                if (num4 == 1)
                {
                    num5 = Math.Min((int)Width - 12, 20);
                }
                int num6 = (int)Width;
                if (num4 == -1)
                {
                    num6 = Math.Max(16, (int)Width - 16);
                }
                if (Scene.CollideCheck<Solid>(value2 + new Vector2(num4 * -2, -2f)))
                {
                    for (int k = num5; k < num6; k += 8)
                    {
                        SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, TopLeft + new Vector2((float)k + (float)num4 * 2f, -1f), (num4 == 1) ? ((float)Math.PI * 3f / 4f) : ((float)Math.PI / 4f));
                    }
                }
                if (Scene.CollideCheck<Solid>(value2 + new Vector2(num4 * -2, Height + 2f)))
                {
                    for (int l = num5; l < num6; l += 8)
                    {
                        SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, BottomLeft + new Vector2((float)l + (float)num4 * 2f, 0f), (num4 == 1) ? ((float)Math.PI * -3f / 4f) : (-(float)Math.PI / 4f));
                    }
                }
            }
        }
        private IEnumerator Sequence()
        {
            while (true)
            {
                if (!HasPlayerRider())
                {
                    yield return null;
                    continue;
                }
                Vector2 start = targetNodes[index];
                Vector2 target;
                if (index < targetNodes.Length - 1)
                {
                    target = targetNodes[index + 1];
                }
                else
                {
                    target = targetNodes[0];
                }
                sfx.Play("event:/cherryhelper/train_mover");
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                StartShaking(0.1f);
                yield return 0.1f;
                StopPlayerRunIntoAnimation = false;
                bool frametoggle = false;
                float at2 = 0f;
                while (at2 < 1f)
                {
                    yield return null;
                    at2 = Calc.Approach(at2, 1f, 4f * Engine.DeltaTime);
                    percent = Ease.SineIn(at2);
                    Vector2 to = Vector2.Lerp(start, target, percent);
                    ScrapeParticlesCheck(to);
                    Vector2 lineOffset = new Vector2(Width, Height) / 2f;
                    if (Scene.OnInterval(0.1f))
                    {
                        CreateSparks(start + lineOffset, to + lineOffset);
                    }
                    if (Engine.FrameCounter % 10 == 0)
                    {
                        frametoggle = !frametoggle;
                    }
                    if (frametoggle)
                    {
                        streetlight.SetAnimationFrame(1);
                    }
                    else
                    {
                        streetlight.SetAnimationFrame(2);
                    }
                    MoveTo(to);
                }
                StartShaking(0.2f);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                SceneAs<Level>().Shake();
                StopPlayerRunIntoAnimation = true;
                yield return 0.5f;
                StopPlayerRunIntoAnimation = false;
                streetlight.SetAnimationFrame(0);
                if (index < targetNodes.Length - 1)
                {
                    index++;
                }
                else
                {
                    index = 0;
                }


            }
        }

        private float mod(float x, float m)
        {
            return (x % m + m) % m;
        }
    }
}