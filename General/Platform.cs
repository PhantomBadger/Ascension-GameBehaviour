using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Physics;

namespace General
{
    class Platform : RigidBody2D
    {
        public Vector2 Size { get; set; }
        public string TextureLeftFile { get; set; } = "stoneLeft.png";
        public string TextureMidFile { get; set; } = "stoneMid.png";
        public string TextureRightFile { get; set; } = "stoneRight.png";

        private Texture2D textureLeft;
        private Texture2D textureMid;
        private Texture2D textureRight;

        public Platform()
        {
            Size = new Vector2();
            Scale = new Vector2(1, 1);
            Friction = new FrictionCoefficients() { StaticCoefficient = 0.0f, DynamicCoefficient = 0.0f };
            Tag = "Ground";
        }

        public override void LoadContent(ContentManager content)
        {
            textureLeft = content.Load<Texture2D>(TextureLeftFile);
            textureMid = content.Load<Texture2D>(TextureMidFile);
            textureRight = content.Load<Texture2D>(TextureRightFile);

            //Adjust size to accurate display all platform tiles at least
            SizeSnap();
            base.LoadContent(content);
        }

        public override void UnloadContent()
        {
            //groundTexture.Dispose();
            base.UnloadContent();
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (textureLeft == null || textureMid == null || textureRight == null)
            {
                textureLeft = new Texture2D(graphicsDevice, 1, 1);
                textureLeft.SetData(new Color[] { Color.DarkGray });
                spriteBatch.Draw(textureLeft, new Rectangle(Position.ToPoint(), Size.ToPoint()), Color.DarkGray);
            }
            else
            {
                //If sprites are loaded, Use them to draw the platform
                float midCount = (Size.X - (textureLeft.Width * Scale.X) - (textureRight.Width * Scale.X)) / (textureMid.Width * Scale.X);
                float posOffset = 0;
                //Assume all tiles are the same size
                float texSize = textureMid.Width * Scale.X;

                spriteBatch.Draw(textureLeft, new Vector2(Position.X + posOffset, Position.Y), null, Color.White, Rotation.Z, Vector2.Zero, Scale, SpriteEffects.None, 0.0f);
                posOffset += texSize;
                for (int i = 0; i < midCount; i++, posOffset += texSize)
                {
                    spriteBatch.Draw(textureMid, new Vector2(Position.X + posOffset, Position.Y), null, Color.White, Rotation.Z, Vector2.Zero, Scale, SpriteEffects.None, 0.0f);
                }
                spriteBatch.Draw(textureRight, new Vector2(Position.X + posOffset, Position.Y), null, Color.White, Rotation.Z, Vector2.Zero, Scale, SpriteEffects.None, 0.0f);
            }

            base.Draw(gameTime, spriteBatch, graphicsDevice);
        }

        private void SizeSnap()
        {
            //Automatically snap to nearest size to allow all the tiles to fit comfortably
            float width = Size.X * Scale.X;
            float minPlatSize = (textureLeft.Width + textureRight.Width) * Scale.X;
            float midSize = textureMid.Width * Scale.X;
            float height = textureMid.Height * Scale.Y;

            //Snap X
            if (Size.X < minPlatSize)
            {
                Size = new Vector2(minPlatSize, height);
            }
            else if ((Size.X - minPlatSize) % midSize != 0)
            {
                float sizeMod = (Size.X - minPlatSize) % midSize;
                Size = sizeMod <= midSize / 2 ? new Vector2(Size.X - sizeMod, height) : new Vector2(Size.X + (midSize - sizeMod), height);
            }
            BoxCollider = Size;
        }
    }
}
