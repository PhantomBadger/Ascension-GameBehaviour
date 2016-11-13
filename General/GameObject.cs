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
    abstract class GameObject
    {
        public Vector2 Position { get; set; }
        public Vector2 Scale { get; set; }
        public Vector3 Rotation { get; set; }

        /// <summary>
        /// Constructor for the GameObject Class
        /// </summary>
        /// <param name="newTransform"></param>
        /// <param name="newScale"></param>
        /// <param name="newRotation"></param>
        public GameObject(Vector2 newTransform, Vector2 newScale, Vector3 newRotation)
        {
            Position = newTransform;
            Scale = newScale;
            Rotation = newRotation;
        }

        /// <summary>
        /// Default Constructor for the GameObject Class
        /// </summary>
        public GameObject()
        {
            Position = new Vector2();
            Scale = new Vector2();
            Rotation = new Vector3();
        }

        /// <summary>
        /// Initialize Method Stub for all GameObjects
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Update Method Stub for all GameObjects
        /// </summary>
        /// <param name="gameTime">Current Game Time</param>
        public abstract void Update(GameTime gameTime);

        /// <summary>
        /// Draw Method Stub for all GameObjects
        /// </summary>
        /// <param name="gameTime">Current Game Time</param>
        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice);

        /// <summary>
        /// Load Content Stub for all GameObjects
        /// </summary>
        /// <param name="content">Content Manager</param>
        public abstract void LoadContent(ContentManager content);

        /// <summary>
        /// Unload Content Stub for all GameObjects
        /// </summary>
        public abstract void UnloadContent();
    }
}
