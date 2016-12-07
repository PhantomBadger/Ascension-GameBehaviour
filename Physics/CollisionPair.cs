using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Physics
{
    class CollisionPair
    {
        public RigidBody2D ObjectA { get; set; }
        public RigidBody2D ObjectB { get; set; }
        public Vector2 ContactPoint { get; set; }
        public Vector2 ContactNormal { get; set; }
        public Vector2 ContactPenetration { get; set; }
    }
}
