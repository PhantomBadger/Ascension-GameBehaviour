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
    public class Enemy : RigidBody2D
    {
        public enum EnemyState { TravellingToNode, SelectingNode, AtGoal }
        public enum TravellingToNodeState { SamePlatform, DifferentPlatformHorizontal, DifferentPlatformVertical, Lost }

        public float EnemySpeed { get; set; }
        public float JumpSpeed { get; set; }
        public AIManager AI { get; set; }
        public EnemyState CurrentState { get; set; }
        public TravellingToNodeState CurrentMovingState { get; set; }
        public WaypointNode PreviousNode { get; set; }
        public WaypointNode NextNode { get; set; }
        public bool OnGround { get; set; } = false;

        private Texture2D enemyTexture;
        private Texture2D waypointTex;
        private SpriteFont debugFont;
        private Queue<WaypointNode> currentPath;
        private Platform currentPlatform;
        private TimeSpan gameUpdateStep;
        private bool hasJumped = false;

        private const float MinDistance = 0.05f;
        private const float MinNodeJumpX = 30f;
        private const float MaxNodeJumpX = 70f;

        /// <summary>
        /// Constructor for the Enemy Class with full info
        /// </summary>
        /// <param name="newRigidBody">RigidBody component of the Enemy</param>
        /// <param name="newSpeed">Enemy's default movement speed</param>
        /// <param name="newJumpSpeed">Enemy's default jump speed</param>
        public Enemy(RigidBody2D newRigidBody, float newSpeed, float newJumpSpeed, TimeSpan newUpdate) :
            base(newRigidBody.Position, newRigidBody.Scale, newRigidBody.Rotation, newRigidBody.Mass, newRigidBody.IsStaticHorizontal, newRigidBody.Friction, newRigidBody.Bounciness)
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
            debugFont = content.Load<SpriteFont>("DebugFont");
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

                        break;
                    }
                case EnemyState.SelectingNode:
                    {
                        //Select the next node in the list then begin travelling to it
                        PreviousNode = NextNode;
                        if (currentPath != null && currentPath.Count > 0)
                        {
                            NextNode = currentPath.Dequeue();
                            CurrentMovingState = SelectTravellingState(PreviousNode, NextNode);
                            CurrentState = EnemyState.TravellingToNode;
                        }
                        else
                        {
                            CurrentState = EnemyState.AtGoal;
                        }

                        break;
                    }
                case EnemyState.TravellingToNode:
                    {
                        //Travel to the next node
                        Vector2 dxy = NextNode.Position - Position;
                        //Console.WriteLine($"Next Node: ({NextNode.Position.X}, {NextNode.Position.Y}) CurrentPos ({Position.X}, {Position.Y})");
                        Rectangle tempRec = new Rectangle(Position.ToPoint() + new Point((int)BoxCollider.X / 4, 0), (BoxCollider / 2).ToPoint());
                        if (tempRec.Contains(NextNode.Position) && OnGround)
                        {
                            //Console.WriteLine("AT GOAL");
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

            //Set OnGround to false
            OnGround = false;

            //Call RigidBody Update
            base.Update(gameTime);
        }

        /// <summary>
        /// Select the next TravellingToNode State based on the node positions
        /// </summary>
        /// <param name="currentNode">The Node the character is at</param>
        /// <param name="nextNode">The next Node the character is going to</param>
        /// <returns>The TravellingToNode State</returns>
        private TravellingToNodeState SelectTravellingState(WaypointNode currentNode, WaypointNode nextNode)
        {
            //Reset jumped flag
            hasJumped = false;

            if (currentNode.ConnectedPlatform == nextNode.ConnectedPlatform)
            {
                return TravellingToNodeState.SamePlatform;
            }
            else if (Math.Abs(nextNode.Position.Y - currentNode.Position.Y) > MinDistance)
            {
                return TravellingToNodeState.DifferentPlatformVertical;
            }
            else
            {
                return TravellingToNodeState.DifferentPlatformHorizontal;
            }
        }

        /// <summary>
        /// Move Towards the next node based on the TravellingToNode State 
        /// </summary>
        /// <param name="targetNode">The character's target node</param>
        private void MoveTowards(WaypointNode targetNode)
        {
            //Get distance data
            Vector2 deltaXY = targetNode.Position - (new Vector2(Position.X + (BoxCollider.X / 2), Position.Y));
            Vector2 nodeDeltaXY = targetNode.Position - PreviousNode.Position;

            switch (CurrentMovingState)
            {
                case TravellingToNodeState.SamePlatform:
                    {
                        //Higher leniency means more velocity at the target node
                        //Peek ahead to see whether we can afford to keep some momentum
                        //Or whether we need to slow down completely
                        WaypointNode peekNode = currentPath.Peek();
                        float leniency = 1.5f;
                        if (peekNode != null)
                        {
                            Vector2 peekDeltaXY = peekNode.Position - targetNode.Position;
                            if ((peekDeltaXY.X > 0 && deltaXY.X > 0) ||
                                (peekDeltaXY.X < 0 && deltaXY.X < 0))
                            {
                                leniency = 4.0f;
                            }
                        }

                        //Move in a straight line towards the target node, slowing down when near it
                        double oppAcc = CalculateAcceleration(new Vector2(-Force.X, Force.Y)).X;
                        double timeToSlowdown = -Velocity.X / (oppAcc * leniency);
                        double distanceRequired = Math.Abs(Velocity.X * timeToSlowdown);

                        //Console.WriteLine($"slowdownDist {distanceRequired}, curDist {deltaXY.X}");

                        //If we're within that distance
                        if (Math.Abs(deltaXY.X) < distanceRequired)
                        {
                            //Console.WriteLine("Slowing Down");
                            //Slowdown to a 0 vel
                            SlowdownHorizontal(deltaXY.X);
                        }
                        else
                        {
                            //Move in direction
                            MoveHorizontal(deltaXY.X);
                        }

                        //Check if we're lost
                        if (currentPlatform != targetNode.ConnectedPlatform)
                        {
                            CurrentMovingState = TravellingToNodeState.Lost;
                        }
                        break;
                    }

                case TravellingToNodeState.DifferentPlatformHorizontal:
                    {
                        //If we're below the target node, then we've fallen and are lost
                        if ((currentPlatform.Position.Y < targetNode.ConnectedPlatform.Position.Y) ||
                            (hasJumped && OnGround && currentPlatform != targetNode.ConnectedPlatform))
                        {
                            CurrentMovingState = TravellingToNodeState.Lost;
                            break;
                        }
                        else if (hasJumped && OnGround && currentPlatform == targetNode.ConnectedPlatform)
                        {
                            //Change to Same-Platform traversal
                            WaypointNode tempNode = new WaypointNode(Position);
                            PreviousNode = tempNode;
                            CurrentMovingState = TravellingToNodeState.SamePlatform;
                            break;
                        }

                        //Jump so we're in the air
                        Jump();

                        //Move in direction
                        MoveHorizontal(deltaXY.X);
                        break;
                    }
                case TravellingToNodeState.DifferentPlatformVertical:
                    {
                        //If we've landed on the same platform as the node, switch to same platform
                        if (currentPlatform == targetNode.ConnectedPlatform)
                        {
                            CurrentState = EnemyState.SelectingNode;
                            CurrentMovingState = TravellingToNodeState.SamePlatform;
                            break;
                        }
                        else if ((currentPlatform != targetNode.ConnectedPlatform &&
                                 currentPlatform != PreviousNode.ConnectedPlatform) ||
                                 (hasJumped && OnGround && currentPlatform != targetNode.ConnectedPlatform))
                        {
                            //Lost
                            CurrentMovingState = TravellingToNodeState.Lost;
                            break;
                        }

                        //If we're in jumping range - We only wont be if we're on a moving platform
                        if ((deltaXY.X > MinNodeJumpX && deltaXY.X < MaxNodeJumpX && nodeDeltaXY.X > 0) ||
                            (deltaXY.X < -MinNodeJumpX && deltaXY.X > -MaxNodeJumpX && nodeDeltaXY.X <= 0))
                        {
                            //Jump to get airborne
                            Jump();

                            //Once we're near the target Y, then move across the X
                            if (deltaXY.Y > -MinDistance)
                            {
                                //Move in direction
                                MoveHorizontal(deltaXY.X);
                            }
                            else
                            {
                                //Correct velocity to 0
                                SlowdownHorizontal(deltaXY.X);
                            }
                        }
                        else
                        {
                            MoveHorizontal(deltaXY.X + (MinNodeJumpX * Math.Sign(nodeDeltaXY.X)));
                        }
                        break;
                    }
                case TravellingToNodeState.Lost:
                    {
                        //We're not where we should be...
                        //Find a new path
                        NextNode = MakeTempNode(false);
                        CurrentState = EnemyState.AtGoal;
                        break;
                    }
            }
        }

        /// <summary>
        /// Utility method to automatically slow down the character on the Horizontal plan by moving in the opposite direction of it's velocity
        /// </summary>
        /// <param name="dx">The distance to the target</param>
        private void SlowdownHorizontal(float dx)
        {
            //Move in Opposite direction for num of frames
            if ((Velocity.X < 0 && dx < 0) || (Velocity.X >= 0 && dx >= 0))
            {
                //We're moving in the direction of the target, 
                //slow down with opposite delta x
                MoveHorizontal(-dx);
            }
            else if ((Velocity.X < 0 && dx >= 0) || (Velocity.X >= 0 && dx < 0))
            {
                //We're moving away from the target
                //Slow down with normal delta x
                MoveHorizontal(dx);
            }
        }

        /// <summary>
        /// Utility method for moving in a horizontal dir
        /// </summary>
        /// <param name="dx">The x distance</param>
        private void MoveHorizontal(float dx)
        {
            //Move in direction
            if (dx < 0)
            {
                MoveLeft();
            }
            else
            {
                MoveRight();
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
            if (OnGround)
            {
                Force = new Vector2(Force.X, -JumpSpeed);
                OnGround = false;
                hasJumped = true;
            }
        }

        /// <summary>
        /// Creates a temp node connected to the local
        /// </summary>
        /// <returns></returns>
        private WaypointNode MakeTempNode(bool stillOnCurrentPath)
        {
            WaypointNode tempNode = new WaypointNode(Position);

            if (!stillOnCurrentPath)
            {
                tempNode.ConnectedNodes = currentPlatform.ConnectedWaypoints;
            }
            else
            {
                tempNode.ConnectedNodes.Add(currentPath.Peek());
            }

            return tempNode;
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
                spriteBatch.DrawString(debugFont, CurrentMovingState.ToString(), new Vector2(Position.X, Position.Y - 25), Color.Red);

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

        /// <summary>
        /// On Collision Trigger Event
        /// </summary>
        /// <param name="col">Collision Data</param>
        public override void OnCollision(CollisionPair col)
        {
            RigidBody2D collidedObject = col.ObjectA == (RigidBody2D)this ? col.ObjectB : col.ObjectA;
            if (collidedObject.Tag == "Ground" && collidedObject.Position.Y >= Position.Y && col.ContactNormal.Y != 0.0f)
            {
                //If we're with the ground, set the variable OnGround to true
                //Also check if the collided object is below me
                //Console.WriteLine("On the ground");
                OnGround = true;
                currentPlatform = (Platform)collidedObject;
            }
            base.OnCollision(col);
        }

        /// <summary>
        /// Generates a new path, creating a temp node at the current location
        /// </summary>
        public void InvalidatePath()
        {
            Console.WriteLine("INVALIDATED");
            PreviousNode = MakeTempNode(true);
            NextNode = currentPath.Peek();
            CurrentState = EnemyState.AtGoal;
        }
        
    }
}
