using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace General
{
    abstract class GameObject
    {
        public Vector2 Transform { get; set; }
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
            Transform = newTransform;
            Scale = newScale;
            Rotation = newRotation;
        }

        /// <summary>
        /// Default Constructor for the GameObject Class
        /// </summary>
        public GameObject()
        {
            Transform = new Vector2();
            Scale = new Vector2();
            Rotation = new Vector3();
        }

        /// <summary>
        /// Update Method Stub for all GameObjects
        /// </summary>
        /// <param name="gameTime">Current Game Time</param>
        protected abstract void Update(GameTime gameTime);
    }
}
