using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using General;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Physics
{
    class RigidBody2D : GameObject
    {
        public Vector2 Velocity { get; set; }
        public Vector2 Acceleration { get; set; }
        public Vector2 Force { get; set; }
        public int Mass { get; set; }
        public bool IsStatic { get; set; }

        private SpriteFont debugFont;

        const float Drag = 0.01f;

        /// <summary>
        /// Constructor for the RigidBody2D Class with full information
        /// Anything affected by Physics should be, or inherit from, the RigidBody
        /// </summary>
        /// <param name="newTransform">The Transform of the Object</param>
        /// <param name="newScale">The Scale of the Object</param>
        /// <param name="newRotation">The Rotation of the Object</param>
        /// <param name="newMass">The Mass of the Object</param>
        /// <param name="isStatic">Whether the object is movable</param>
        public RigidBody2D(Vector2 newTransform, Vector2 newScale, Vector3 newRotation, int newMass, bool isStatic) : 
            base(newTransform, newScale, newRotation)
        {
            Mass = newMass;
            IsStatic = isStatic;
        }

        /// <summary>
        /// Default Constructor for the RigidBody2D Class
        /// Anything affected by Physics should be, or inherit from, the RigidBody
        /// </summary>
        public RigidBody2D() :
            base()
        {
            Mass = 1;
            IsStatic = false;
        }

        /// <summary>
        /// Load the Content Required for this RigidBody
        /// </summary>
        /// <param name="content"></param>
        public override void LoadContent(ContentManager content)
        {
            //Load Debug Font
            debugFont = content.Load<SpriteFont>("DebugFont");
        }

        /// <summary>
        /// Unload the Content Required for this RigidBody
        /// </summary>
        public override void UnloadContent()
        {
            //Do Nothing
        }

        /// <summary>
        /// Initialize Method called at the start
        /// </summary>
        public override void Initialize()
        {
            Velocity = new Vector2();
            Acceleration = new Vector2();
        }

        /// <summary>
        /// A Method called every frame for a RigidBody2D
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            //Calculate acceleration using F/m
            Acceleration = Force * (1 / Mass);

            //Calculate Velocity using V = at
            Velocity += (Acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds);

            //Add Air Resistance Drag to Velocity so there's a "Terminal Velocity"
            Velocity *= (1 - Drag);

            //Use Euler Integration to update Position
            Transform += (Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds);

            //Reset Force so it's not continually applied
            Force = new Vector2(0, 0);

            //throw new NotImplementedException();
        }

        /// <summary>
        /// Draw any RigidBody Debug Info (Force Arrows, etc)
        /// </summary>
        /// <param name="gameTime">Current Game Time</param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //Do Nothing
            spriteBatch.DrawString(debugFont, $"Acceleration: ({Acceleration.X}, {Acceleration.Y})", new Vector2(0, 0), Color.White);
            spriteBatch.DrawString(debugFont, $"Velocity: ({Velocity.X}, {Velocity.Y})", new Vector2(0, 20), Color.White);
        }
    }
}
