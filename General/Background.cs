using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace General
{
    class Background : GameObject
    {
        public Camera SceneCamera { get; set; }
        public string TextureFile { get; set; }

        private Texture2D texture;

        public override void Initialize()
        {
            Scale = new Vector2(1, 1);
        }

        public override void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>(TextureFile);
        }

        public override void UnloadContent()
        {
            
        }

        public override void Update(GameTime gameTime)
        {
            //TODO:
            //Parallax with multiple images
            //For now just follow camera        
            Position = SceneCamera.Position;    
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Draw(texture, Position, null, Color.White, Rotation.Z, Vector2.Zero, Scale, SpriteEffects.None, 1.0f);
        }
    }
}
