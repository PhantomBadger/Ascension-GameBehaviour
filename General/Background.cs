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

        /// <summary>
        /// Initialize Method to Initialize the Scale
        /// </summary>
        public override void Initialize()
        {
            Scale = new Vector2(1, 1);
        }

        /// <summary>
        /// Load in the Specified Texture
        /// </summary>
        /// <param name="content">Content Manager</param>
        public override void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>(TextureFile);
        }

        /// <summary>
        /// Unload the Texture
        /// </summary>
        public override void UnloadContent()
        {
            texture.Dispose();
        }

        /// <summary>
        /// Update the Background Image, Move it to the Camera's Position
        /// </summary>
        /// <param name="gameTime">Current Game Time</param>
        public override void Update(GameTime gameTime)
        {
            //TODO:
            //Parallax with multiple images
            //For now just follow camera        
            Position = SceneCamera.Position;    
        }

        /// <summary>
        /// Draw the Texture for the Background
        /// </summary>
        /// <param name="gameTime">Current Game Time</param>
        /// <param name="spriteBatch">Sprite Batch</param>
        /// <param name="graphicsDevice">Graphics Device</param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Draw(texture, Position, null, Color.White, Rotation.Z, Vector2.Zero, Scale, SpriteEffects.None, 1.0f);
        }
    }
}
