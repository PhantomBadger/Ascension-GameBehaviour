using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Physics;
using MonoGame.Utilities;
using Microsoft.Xna.Framework;

namespace General
{
    class Player : RigidBody2D
    {
        public float PlayerSpeed { get; set; }

        /// <summary>
        /// Constructor for Player Class with full info
        /// </summary>
        /// <param name="newRigidBody">RigidBody component of the player</param>
        /// <param name="newSpeed">Player's default movement speed</param>
        public Player(RigidBody2D newRigidBody, float newSpeed) : 
            base(newRigidBody.Transform, newRigidBody.Scale, newRigidBody.Rotation, newRigidBody.Mass, newRigidBody.IsStatic)
        {
            PlayerSpeed = newSpeed;
        }

        /// <summary>
        /// Default Constructor for Player Class with just required speed
        /// </summary>
        /// <param name="newSpeed">Speed for the player</param>
        public Player(float newSpeed) : 
            base ()
        {
            PlayerSpeed = newSpeed;
        }

        /// <summary>
        /// Update Method for Player, Called once a frame
        /// </summary>
        /// <param name="gameTime">Current Game Time</param>
        protected override void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }
    }
}
