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
        public Texture2D Texture { get; set; }

        public Platform()
        {
            Size = new Vector2();
            Friction = new FrictionCoefficients() { StaticCoefficient = 0.0f, DynamicCoefficient = 0.0f };
            Tag = "Ground";
        }

        public override void LoadContent(ContentManager content)
        {
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
            if (Texture == null)
            {
                Texture = new Texture2D(graphicsDevice, 1, 1);
                Texture.SetData(new Color[] { Color.DarkGray });
            }
            spriteBatch.Draw(Texture, new Rectangle(Position.ToPoint(), Size.ToPoint()), Color.DarkGray);

            base.Draw(gameTime, spriteBatch, graphicsDevice);
        }
    }
}
