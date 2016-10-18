using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using General;

namespace Physics
{
    class RigidBody2D : GameObject
    {
        public Vector2 Velocity { get; set; }
        public Vector2 Acceleration { get; set; }
        public int Mass { get; set; }
        public bool IsStatic { get; set; }

        /// <summary>
        /// Constructor for the RigidBody2D Class with full information
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
            Velocity = new Vector2();
            Acceleration = new Vector2();
        }

        /// <summary>
        /// Default Constructor for the RigidBody2D Class
        /// </summary>
        public RigidBody2D() :
            base()
        {
            Mass = 1;
            IsStatic = false;
            Velocity = new Vector2();
            Acceleration = new Vector2();
        }

        /// <summary>
        /// A Method called every frame for a RigidBody2D
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }
    }
}
