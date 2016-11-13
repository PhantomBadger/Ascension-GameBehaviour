using System;
using System.Collections.Generic;
using System.Text;
using General;
using Microsoft.Xna.Framework;

namespace Physics
{
    class PhysicsManager
    {
        struct CollisionPair
        {
            public RigidBody2D objectA;
            public RigidBody2D objectB;
            public Vector2 contactPoint;
            public Vector2 contactNormal;
            public Vector2 difference;
        }

        public List<RigidBody2D> RigidBodies { get; set; }

        /// <summary>
        /// Default Constructor for Physics Manager
        /// </summary>
        public PhysicsManager()
        {
            RigidBodies = new List<RigidBody2D>();
        }

        public void Step()
        {
            //Create a list of collision pairs
            List<CollisionPair> collisionsToResolve = new List<CollisionPair>();

            //Detect collisions to populate our pairs
            for (int i = 0; i < RigidBodies.Count; i++)
            {
                bool hasCol = false;
                for (int j = 0; j < RigidBodies.Count; j++)
                {
                    //Prevent it colliding with itself
                    if (i == j)
                    {
                        continue;
                    }

                    CollisionPair colPair;
                    if (TestAABBAABB(RigidBodies[i], RigidBodies[j], out colPair))
                    {
                        hasCol = true;
                        //Console.WriteLine($"Collision Detected {colPair.objectA} and {colPair.objectB}");
                        if (collisionsToResolve.FindIndex(f => ((f.objectA == colPair.objectA || f.objectA == colPair.objectB) && (f.objectB == colPair.objectA || f.objectB == colPair.objectB))) < 0)
                        {
                            collisionsToResolve.Add(colPair);
                        }
                    }
                }

                if (!hasCol)
                {
                    RigidBodies[i].ActiveDynamic = new Vector2(0);
                    RigidBodies[i].ActiveStatic = new Vector2(0);
                }
            }

            //Resolve collisions
            for (int i = 0; i < collisionsToResolve.Count; i++)
            {
                ResolveAABBAABB(collisionsToResolve[i]);
            }
        }

        private bool TestAABBAABB(RigidBody2D objectA, RigidBody2D objectB, out CollisionPair colPair)
        {
            colPair = new CollisionPair();

            //AABB Collision Detection
            if (objectA.Position.X + objectA.BoxCollider.X > objectB.Position.X &&
                objectA.Position.X < objectB.Position.X + objectB.BoxCollider.X &&
                objectA.Position.Y + objectA.BoxCollider.Y > objectB.Position.Y &&
                objectA.Position.Y < objectB.Position.Y + objectB.BoxCollider.Y)
            {
                //Collision detected!
                colPair.objectA = objectA;
                colPair.objectB = objectB;

                //Calculate middle of objects
                Vector2 objAMid = objectA.Position + (objectA.BoxCollider / 2);
                Vector2 objBMid = objectB.Position + (objectB.BoxCollider / 2);

                //Calculate the difference between the objects on the X and Y
                //Their point of intersection
                //
                float xColDistance, yColDistance;
                xColDistance = yColDistance = 0.0f;
                float dx = objBMid.X - objAMid.X;
                float dy = objBMid.Y - objAMid.Y;
                float pointX = (objectB.BoxCollider.X / 2) + (objectA.BoxCollider.X / 2) - Math.Abs(dx);
                float pointY = (objectB.BoxCollider.Y / 2) + (objectA.BoxCollider.Y / 2) - Math.Abs(dy);
                
                //Depending on which distance is shorter, add different values
                if (pointX < pointY)
                {
                    xColDistance = pointX * (Math.Sign(dx));
                    colPair.contactNormal = new Vector2(Math.Sign(dx), 0);
                    colPair.contactPoint = new Vector2(objectA.Position.X + (objectA.BoxCollider.X * Math.Sign(dx)), objectB.Position.Y);
                }
                else
                {
                    yColDistance = pointY * (Math.Sign(dy));
                    colPair.contactNormal = new Vector2(0, Math.Sign(dy));
                    colPair.contactPoint = new Vector2(objectB.Position.X, objectA.Position.Y + (objectA.BoxCollider.Y * Math.Sign(dy)));
                }

                //Add the difference to our Collision data so we can push them out
                colPair.difference = new Vector2(xColDistance, yColDistance);

                return true;
            }
            return false;
        }

        private void ResolveAABBAABB(CollisionPair colPair)
        {
            //Get relative velocity of objects
            Vector2 relativeVelocity = colPair.objectA.Velocity - colPair.objectB.Velocity;

            //Normalise contact normal
            Vector2 normalMagnitude = colPair.contactNormal;
            colPair.contactNormal.Normalize();

            //Console.WriteLine($"({colPair.contactNormal.X}, {colPair.contactNormal.Y})");

            //Get the velocity along the normal
            float velocityAlongContactNormal = Vector2.Dot(relativeVelocity, colPair.contactNormal);
            float coefficientOfRestitution = 0.2f;

            //Calc the inverse mass if the object isnt static
            float invMassA = colPair.objectA.IsStatic ? 0.0f : (1 / colPair.objectA.Mass);
            float invMassB = colPair.objectB.IsStatic ? 0.0f : (1 / colPair.objectB.Mass);

            //Calculate impulse
            float impulse = (-(1 + coefficientOfRestitution) * velocityAlongContactNormal) / (invMassA + invMassB);

            //Resolve the collision with a new velocity and position if they're not static
            if (!colPair.objectA.IsStatic)
            {
                //Create a new velocity
                Vector2 objectANewVelocity = colPair.objectA.Velocity + (colPair.contactNormal * (impulse / colPair.objectA.Mass));
                colPair.objectA.Position -= colPair.contactNormal;
                colPair.objectA.Velocity = objectANewVelocity;
            }

            if (!colPair.objectB.IsStatic)
            {
                //Create a new velocity
                Vector2 objectBNewVelocity = colPair.objectB.Velocity - (colPair.contactNormal * (impulse / colPair.objectB.Mass));
                colPair.objectB.Position += colPair.contactNormal;
                colPair.objectB.Velocity = objectBNewVelocity;
            }

            //Assign the current friction to both objects
            int frictionScale = 1;
            //Object A
            colPair.objectA.ActiveDynamic = (-(colPair.objectB.Friction.DynamicCoefficient / normalMagnitude.Length()) * (colPair.objectA.Velocity / colPair.objectA.Velocity.Length())) * frictionScale;
            colPair.objectA.ActiveStatic = (-(colPair.objectB.Friction.StaticCoefficient / normalMagnitude.Length()) * (new Vector2(colPair.contactNormal.Y, -colPair.contactNormal.X))) * frictionScale;

            //Object B
            colPair.objectB.ActiveDynamic = (-(colPair.objectA.Friction.DynamicCoefficient / normalMagnitude.Length()) * (colPair.objectB.Velocity / colPair.objectB.Velocity.Length())) * frictionScale;
            colPair.objectB.ActiveStatic = (-(colPair.objectA.Friction.StaticCoefficient / normalMagnitude.Length()) * (new Vector2(colPair.contactNormal.Y, -colPair.contactNormal.X))) * frictionScale;

        }

    }
}
