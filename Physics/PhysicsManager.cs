using System;
using System.Collections.Generic;
using System.Text;
using General;
using Microsoft.Xna.Framework;

namespace Physics
{
    class PhysicsManager
    {
        public List<RigidBody2D> RigidBodies { get; private set; }

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
            List<RigidBody2D> frictionObjects = new List<RigidBody2D>();

            //Detect collisions to populate our pairs
            for (int i = 0; i < RigidBodies.Count; i++)
            {
                //Compare with all others
                for (int j = 0; j < RigidBodies.Count; j++)
                {
                    //Prevent it colliding with itself
                    if (i == j)
                    {
                        continue;
                    }

                    CollisionPair colPair;
                    if (TestAABBAABB_C(RigidBodies[i], RigidBodies[j], out colPair))
                    {
                        if (collisionsToResolve.FindIndex(f => (f.ObjectA == colPair.ObjectA && f.ObjectB == colPair.ObjectB) || (f.ObjectB == colPair.ObjectA && f.ObjectA == colPair.ObjectB)) < 0)
                        {
                            collisionsToResolve.Add(colPair);
                        }
                    }
                }
            }

            //Resolve collisions
            for (int i = 0; i < collisionsToResolve.Count; i++)
            {
                ResolveAABBAABB(collisionsToResolve[i]);

                collisionsToResolve[i].ObjectA.OnCollision(collisionsToResolve[i]);
                collisionsToResolve[i].ObjectB.OnCollision(collisionsToResolve[i]);
            }
        }

        /// <summary>
        /// AABB AABB Intersection Test, Does populate Collision Pair, used Internally
        /// </summary>
        /// <param name="objectA">AABB Object A</param>
        /// <param name="objectB">AABB Object B</param>
        /// <param name="colPair">Out Collision Pair Data</param>
        /// <returns>Boolean True or False</returns>
        private bool TestAABBAABB_C(RigidBody2D objectA, RigidBody2D objectB, out CollisionPair colPair)
        {
            colPair = new CollisionPair();

            //AABB Collision Detection
            if (objectA.Position.X + objectA.BoxCollider.X > objectB.Position.X &&
                objectA.Position.X < objectB.Position.X + objectB.BoxCollider.X &&
                objectA.Position.Y + objectA.BoxCollider.Y > objectB.Position.Y &&
                objectA.Position.Y < objectB.Position.Y + objectB.BoxCollider.Y)
            {
                //Collision detected!
                colPair.ObjectA = objectA;
                colPair.ObjectB = objectB;

                //Calculate middle of objects
                Vector2 objAMid = objectA.Position + (objectA.BoxCollider / 2);
                Vector2 objBMid = objectB.Position + (objectB.BoxCollider / 2);

                //Calculate the difference between the objects on the X and Y
                float xColDistance, yColDistance;
                xColDistance = yColDistance = 0.0f;
                float dx = objBMid.X - objAMid.X;
                float dy = objBMid.Y - objAMid.Y;

                //Get the depth of intsersection component
                float depthX = (objectB.BoxCollider.X / 2) + (objectA.BoxCollider.X / 2) - Math.Abs(dx);
                float depthY = (objectB.BoxCollider.Y / 2) + (objectA.BoxCollider.Y / 2) - Math.Abs(dy);

                //apply the displacement sign in order to give 'direction' (neg x/y for left/up, etc)
                xColDistance = depthX * (Math.Sign(dx));
                yColDistance = depthY * (Math.Sign(dy));

                //Depending on which distance is shorter, add different values
                //Ignore direction for these comparisons
                if (Math.Abs(xColDistance) < Math.Abs(yColDistance))
                {
                    colPair.ContactNormal = new Vector2(Math.Sign(dx), 0);
                    colPair.ContactPoint = new Vector2(objectA.Position.X + (objectA.BoxCollider.X * Math.Sign(dx)), objectB.Position.Y);
                }
                else
                {
                    colPair.ContactNormal = new Vector2(0, Math.Sign(dy));
                    colPair.ContactPoint = new Vector2(objectB.Position.X, objectA.Position.Y + (objectA.BoxCollider.Y * Math.Sign(dy)));
                }

                //Add the difference to our Collision data so we can push them out
                colPair.ContactPenetration = new Vector2(xColDistance, yColDistance);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Resolve the AABB Collision detailed in the Collision Pair Data
        /// </summary>
        /// <param name="colPair">Collision Information</param>
        private void ResolveAABBAABB(CollisionPair colPair)
        {
            //Get relative velocity of objects
            Vector2 relativeVelocity = colPair.ObjectA.Velocity - colPair.ObjectB.Velocity;

            //Normalise contact normal
            Vector2 normalMagnitude = colPair.ContactNormal;
            colPair.ContactNormal.Normalize();

            //Console.WriteLine($"Col {colPair.ObjectA.Tag} & {colPair.ObjectB.Tag} Pen Depth: {colPair.ContactPenetration.X}, {colPair.ContactPenetration.Y}");

            //Get the velocity along the normal
            float velocityAlongContactNormal = Vector2.Dot(relativeVelocity, colPair.ContactNormal);
            float coefficientOfRestitution = MathHelper.Clamp((colPair.ObjectA.Bounciness * colPair.ObjectB.Bounciness), 0, 1);

            //Calc the inverse mass if the object isnt static
            float invMassA = colPair.ObjectA.IsStatic ? 0.0f : (1 / colPair.ObjectA.Mass);
            float invMassB = colPair.ObjectB.IsStatic ? 0.0f : (1 / colPair.ObjectB.Mass);

            //Calculate impulse
            float impulse = (-(1 + coefficientOfRestitution) * velocityAlongContactNormal) / (invMassA + invMassB);

            float objAVelAcrossNormal = Vector2.Dot(colPair.ObjectA.Velocity, colPair.ContactNormal);
            float objBVelAcrossNormal = Vector2.Dot(colPair.ObjectB.Velocity, colPair.ContactNormal);
            float objAResolutionWeighting = (objAVelAcrossNormal + objBVelAcrossNormal) / objAVelAcrossNormal;
            float objBResolutionWeighting = (objAVelAcrossNormal + objBVelAcrossNormal) / objBVelAcrossNormal;

            //Resolve the collision with a new velocity and position if they're not static
            if (!colPair.ObjectA.IsStatic)
            {
                //Create a new velocity
                Vector2 objectANewVelocity = colPair.ObjectA.Velocity + (colPair.ContactNormal * (impulse / colPair.ObjectA.Mass));
                //Displace by the smallest pen depth, dont compare with signs
                colPair.ObjectA.Position -= Math.Abs(colPair.ContactPenetration.X) < Math.Abs(colPair.ContactPenetration.Y) ? new Vector2(colPair.ContactPenetration.X * objAResolutionWeighting, 0) : new Vector2(0, colPair.ContactPenetration.Y * objAResolutionWeighting);
                colPair.ObjectA.Velocity = objectANewVelocity;
            }

            if (!colPair.ObjectB.IsStatic)
            {
                //Create a new velocity
                Vector2 objectBNewVelocity = colPair.ObjectB.Velocity - (colPair.ContactNormal * (impulse / colPair.ObjectB.Mass));
                colPair.ObjectB.Position += Math.Abs(colPair.ContactPenetration.X) < Math.Abs(colPair.ContactPenetration.Y) ? new Vector2(colPair.ContactPenetration.X * objBResolutionWeighting, 0) : new Vector2(0, colPair.ContactPenetration.Y * objBResolutionWeighting);
                colPair.ObjectB.Velocity = objectBNewVelocity;
            }

            //Assign the current friction to both objects
            //We add together all the active friction components applied to each object

            //Object A
            colPair.ObjectA.ActiveDynamic += (-(colPair.ObjectB.Friction.DynamicCoefficient) * (colPair.ObjectA.Velocity));
            colPair.ObjectA.ActiveStatic += (-(colPair.ObjectB.Friction.StaticCoefficient) * (new Vector2(colPair.ContactNormal.Y, -colPair.ContactNormal.X)));

            //Object B
            colPair.ObjectB.ActiveDynamic += (-(colPair.ObjectA.Friction.DynamicCoefficient) * (colPair.ObjectB.Velocity));
            colPair.ObjectB.ActiveStatic += (-(colPair.ObjectA.Friction.StaticCoefficient) * (new Vector2(colPair.ContactNormal.Y, -colPair.ContactNormal.X)));
        }

        /// <summary>
        /// AABB Segment Intersection Test, Does not populate Collision Pair
        /// </summary>
        /// <param name="objectA">AABB Object</param>
        /// <param name="segment">Line Segment</param>
        /// <returns>Boolean true or false</returns>
        public bool TestAABBSegment(RigidBody2D objectA, Vector2 segmentStart, Vector2 segmentEnd)
        {
            //TODO!!!!!!!

        }

        /// <summary>
        /// AABB AABB Intersection Test, Does not populate Collision Pair
        /// </summary>
        /// <param name="objectA">AABB Object A</param>
        /// <param name="objectB">AABB Object B</param>
        /// <returns>Boolean true or false</returns>
        public bool TestAABBAABB(RigidBody2D objectA, RigidBody2D objectB)
        {
            //AABB Collision Detection
            return (objectA.Position.X + objectA.BoxCollider.X > objectB.Position.X &&
                objectA.Position.X < objectB.Position.X + objectB.BoxCollider.X &&
                objectA.Position.Y + objectA.BoxCollider.Y > objectB.Position.Y &&
                objectA.Position.Y < objectB.Position.Y + objectB.BoxCollider.Y);
        }
    }
}
