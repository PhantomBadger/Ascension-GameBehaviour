using System;
using System.Collections.Generic;
using System.Text;
using General;
using Physics;
using MonoGame.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AI
{
    class Enemy : RigidBody2D
    {
        public enum EnemyState { TravellingToNode, SelectingNode, AtGoal }

        public float EnemySpeed { get; set; }
        public float JumpSpeed { get; set; }
        public AIManager AI { get; set; }
        public EnemyState CurrentState { get; set; }
        public WaypointNode PreviousNode { get; set; }
        public WaypointNode NextNode { get; set; }

        private Texture2D enemyTexture;
        private Texture2D waypointTex;
        private bool onGround = false;
        private Queue<WaypointNode> currentPath;
        private Platform currentPlatform;
        private TimeSpan gameUpdateStep;

        private const float MinDistance = 0.5f;
        private const float MinJumpHeightTrigger = 15.0f;

        /// <summary>
        /// Constructor for the Enemy Class with full info
        /// </summary>
        /// <param name="newRigidBody">RigidBody component of the Enemy</param>
        /// <param name="newSpeed">Enemy's default movement speed</param>
        /// <param name="newJumpSpeed">Enemy's default jump speed</param>
        public Enemy(RigidBody2D newRigidBody, float newSpeed, float newJumpSpeed, TimeSpan newUpdate) :
            base(newRigidBody.Position, newRigidBody.Scale, newRigidBody.Rotation, newRigidBody.Mass, newRigidBody.IsStatic, newRigidBody.Friction, newRigidBody.Bounciness)
        {
            EnemySpeed = newSpeed;
            JumpSpeed = newJumpSpeed;
            Tag = "Enemy";
            CurrentState = EnemyState.AtGoal;

            gameUpdateStep = newUpdate;
            currentPath = new Queue<WaypointNode>();
        }
        
        /// <summary>
        /// Load the Content Required for the Enemy, such as Sprites and Sounds
        /// </summary>
        /// <param name="content">Content Manager</param>
        public override void LoadContent(ContentManager content)
        {
            enemyTexture = content.Load<Texture2D>("alienPink_front.png");
            base.LoadContent(content);
        }

        /// <summary>
        /// Unload the Content Required for the Enemy, such as Sprites and Sounds
        /// </summary>
        public override void UnloadContent()
        {
            enemyTexture.Dispose();
            base.UnloadContent();
        }

        /// <summary>
        /// Init method called at the start
        /// </summary>
        public override void Initialize()
        {
            currentPath = new Queue<WaypointNode>();
            base.Initialize();
        }

        /// <summary>
        /// Update Method for Enemy, Called once a frame
        /// </summary>
        /// <param name="gameTime">Current Game Time</param>
        public override void Update(GameTime gameTime)
        {
            //React based on current state
            switch (CurrentState)
            {
                case EnemyState.AtGoal:
                    {
                        //Select a new goal and populate the path
                        WaypointNode goalNode = AI.GetNewGoalNode();
                        if (goalNode != null)
                        {
                            currentPath = AI.AStarSearch(NextNode, goalNode);
                        }
                        CurrentState = EnemyState.SelectingNode;

                        //This is a good use of goto I swear! (http://stackoverflow.com/a/174223/4203525)
                        //goto case EnemyState.SelectingNode;
                        break;
                    }
                case EnemyState.SelectingNode:
                    {
                        //Select the next node in the list then begin travelling to it
                        PreviousNode = NextNode;
                        if (currentPath != null && currentPath.Count > 0)
                        {
                            NextNode = currentPath.Dequeue();
                            CurrentState = EnemyState.TravellingToNode;
                        }
                        else
                        {
                            CurrentState = EnemyState.AtGoal;
                        }

                        //goto case EnemyState.TravellingToNode;
                        break;
                    }
                case EnemyState.TravellingToNode:
                    {
                        //Travel to the next node
                        Vector2 dxy = NextNode.Position - Position;
                        //Console.WriteLine($"Next Node: ({NextNode.Position.X}, {NextNode.Position.Y}) CurrentPos ({Position.X}, {Position.Y})");
                        Rectangle tempRec = new Rectangle(Position.ToPoint(), BoxCollider.ToPoint());
                        if (tempRec.Contains(NextNode.Position) && onGround)
                        {
                            //If we have items left in the queue go to selecting node, if not we're at goal
                            CurrentState = currentPath.Count > 0 ? EnemyState.SelectingNode : EnemyState.AtGoal;
                        }
                        else
                        {
                            MoveTowards(NextNode);
                        }

                        break;
                    }
            }

            //Set onGround to false
            onGround = false;

            //Call RigidBody Update
            base.Update(gameTime);
        }

        private void MoveTowards(WaypointNode targetNode)
        {
            //Determine if the next node is on a different platform than the one we're on
            //Or will it require jumping between platforms
            Vector2 deltaXY = targetNode.Position - (new Vector2(Position.X + (BoxCollider.X / 2), Position.Y));
            Vector2 nodeDeltaXY = targetNode.Position - PreviousNode.Position;

            //Are we on the same platform as our current target node
            if (onGround && targetNode.ConnectedPlatform == currentPlatform)
            {
                //Estimate a 'slow-down' location, where we hold the opposite direction to reduce velocity
                double numOfFramesToSlowdown = ((Math.Abs(Velocity.X) + EnemySpeed) * gameUpdateStep.TotalSeconds) / (EnemySpeed * (1/Mass));
                double distanceFromEndToSlowdown = numOfFramesToSlowdown * Math.Abs(Velocity.X);
                distanceFromEndToSlowdown /= (currentPlatform.Friction.DynamicCoefficient);
                distanceFromEndToSlowdown *= 10;

                Console.WriteLine($"slowdownDist {distanceFromEndToSlowdown}, curDist {deltaXY.X}");

                //If we're within that distance
                if (Math.Abs(deltaXY.X) < distanceFromEndToSlowdown)
                {
                    Console.WriteLine("Slowing Down");
                    //Move in Opposite direction for num of frames
                    if (deltaXY.X < 0)
                    {
                        MoveRight();
                    }
                    else
                    {
                        MoveLeft();
                    }
                }
                else
                {
                    //Move in direction
                    if (deltaXY.X < 0)
                    {
                        MoveLeft();
                    }
                    else
                    {
                        MoveRight();
                    }
                }
            }
            else
            {
                //Move Vertically if it's above us or on a different platform
                if ((Math.Abs(deltaXY.Y) > MinJumpHeightTrigger) ||
                    (Math.Abs(deltaXY.X) > MinDistance && NextNode.ConnectedPlatform != PreviousNode.ConnectedPlatform))
                {
                    Jump();
                }

                //If we're at a different X value to it, move in the right direction
                if (deltaXY.X >= MinDistance)
                {
                    MoveRight();
                }
                else if (deltaXY.X <= -MinDistance)
                {
                    MoveLeft();
                }
            }
        }

        /// <summary>
        /// Move to the Right
        /// </summary>
        private void MoveRight()
        {
            //Move Right
            Force = new Vector2(EnemySpeed, Force.Y);
        }

        /// <summary>
        /// Move to the Left
        /// </summary>
        private void MoveLeft()
        {
            //Move Left
            Force = new Vector2(-EnemySpeed, Force.Y);
        }

        /// <summary>
        /// Jump upwards
        /// </summary>
        private void Jump()
        {
            //Move Left
            if (onGround)
            {
                Force = new Vector2(Force.X, -JumpSpeed);
                onGround = false;
            }
        }

        /// <summary>
        /// Draw whats needed this frame
        /// </summary>
        /// <param name="gameTime">Current Game Time</param>
        /// <param name="spriteBatch">Sprite Batch</param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            //Create waypoint texture
            if (waypointTex == null)
            {
                waypointTex = new Texture2D(graphicsDevice, 1, 1);
                waypointTex.SetData(new Color[] { Color.White });
            }

            spriteBatch.Draw(enemyTexture, Position, null, Color.White, Rotation.Z, Vector2.Zero, Scale, SpriteEffects.None, 0.0f);

            if (GameManager.DebugMode && currentPath != null && currentPath.Count > 0)
            {
                //Draw his planned path in red
                WaypointNode prevNode = NextNode;
                spriteBatch.Draw(waypointTex, new Rectangle((int)prevNode.Position.X - 3, (int)prevNode.Position.Y - 3, 6, 6), Color.Yellow);
                foreach (WaypointNode node in currentPath)
                {
                    //Draw Waypoints
                    spriteBatch.Draw(waypointTex, new Rectangle((int)node.Position.X - 3, (int)node.Position.Y - 3, 6, 6), Color.Red);

                    Vector2 waypointLine = node.Position - prevNode.Position;
                    float lineAngle = (float)Math.Atan2(waypointLine.Y, waypointLine.X);

                    spriteBatch.Draw(waypointTex,
                        new Rectangle((int)prevNode.Position.X, (int)prevNode.Position.Y, (int)waypointLine.Length(), 2),
                        null,
                        prevNode == NextNode ? Color.Yellow : Color.Red,
                        lineAngle,
                        new Vector2(0, 0),
                        SpriteEffects.None,
                        0);

                    prevNode = node;
                }
            }

            base.Draw(gameTime, spriteBatch, graphicsDevice);
        }

        public override void OnCollision(CollisionPair col)
        {
            RigidBody2D collidedObject = col.ObjectA == (RigidBody2D)this ? col.ObjectB : col.ObjectA;
            if (collidedObject.Tag == "Ground" && collidedObject.Position.Y >= Position.Y && col.ContactNormal.Y != 0.0f)
            {
                //If we're with the ground, set the variable onGround to true
                //Also check if the collided object is below me
                //Console.WriteLine("On the ground");
                onGround = true;
                currentPlatform = (Platform)collidedObject;
            }
            base.OnCollision(col);
        }
        
    }
}
