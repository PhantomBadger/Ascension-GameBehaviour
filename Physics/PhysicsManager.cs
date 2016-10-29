using System;
using System.Collections.Generic;
using System.Text;
using General;

namespace Physics
{
    class PhysicsManager
    {
        public List<RigidBody2D> RigidBodies { get; set; }

        /// <summary>
        /// Default Constructor for Physics Manager
        /// </summary>
        public PhysicsManager()
        {
            RigidBodies = new List<RigidBody2D>();
        }

        //TODO:
        // Create Physics step
        // Handle basic collisions 
    }
}
