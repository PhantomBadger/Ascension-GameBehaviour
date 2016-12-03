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
        private bool onGround = false;
        private Queue<WaypointNode> currentPath;

        private const float MinDistance = 0.05f;

        /// <summary>
        /// Constructor for the Enemy Class with full info
        /// </summary>
        /// <param name="newRigidBody">RigidBody component of the Enemy</param>
        /// <param name="newSpeed">Enemy's default movement speed</param>
        /// <param name="newJumpSpeed">Enemy's default jump speed</param>
        public Enemy(RigidBody2D newRigidBody, float newSpeed, float newJumpSpeed) :
            base(newRigidBody.Position, newRigidBody.Scale, newRigidBody.Rotation, newRigidBody.Mass, newRigidBody.IsStatic, newRigidBody.Friction, newRigidBody.Bounciness)
        {
            EnemySpeed = newSpeed;
            JumpSpeed = newJumpSpeed;
            Tag = "Enemy";
            CurrentState = EnemyState.AtGoal;

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
            base.Initialize();
        }

        /// <summary>
        /// Update Method for Enemy, Called once a frame
        /// </summary>
        /// <param name="gameTime">Current Game Time</param>
        public override void Update(GameTime gameTime)
        {
            //Set onGround to false
            onGround = false;

            //React based on current state
            switch (CurrentState)
            {
                case EnemyState.AtGoal:
                    {
                        //Select a new goal and populate the path
                        currentPath = AI.AStarSearch(NextNode, AI.GetNewGoalNode());
                        CurrentState = EnemyState.SelectingNode;

                        //This is a good use of goto I swear! (http://stackoverflow.com/a/174223/4203525)
                        //goto case EnemyState.SelectingNode;
                        break;
                    }
                case EnemyState.SelectingNode:
                    {
                        //Select the next node in the list then begin travelling to it
                        PreviousNode = NextNode;
                        NextNode = currentPath.Dequeue();
                        CurrentState = EnemyState.TravellingToNode;

                        //goto case EnemyState.TravellingToNode;
                        break;
                    }
                case EnemyState.TravellingToNode:
                    {
                        //Travel to the next node
                        Vector2 dxy = NextNode.Position - Position;
                        if (dxy.Length() < MinDistance)
                        {
                            //If we have items left in the queue go to selecting node, if not we're at goal
                            CurrentState = currentPath.Count > 0 ? EnemyState.SelectingNode : EnemyState.AtGoal;
                        }

                        MoveTowards(NextNode.Position);

                        break;
                    }
            }

            //Call RigidBody Update
            base.Update(gameTime);


        }

        private void MoveTowards(Vector2 targetPosition)
        {
            Vector2 dxy = targetPosition - Position;

            //Move Vertically if it's above us or on a different platform
            if (dxy.Y > 0 || NextNode.ConnectedPlatform != PreviousNode.ConnectedPlatform)
            {
                Jump();
            }

            //If we're at a different X value to it, move in the right direction
            if (dxy.X >= MinDistance)
            {
                MoveRight();
            }
            else if (dxy.X <= -MinDistance)
            {
                MoveLeft();
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
            Force = new Vector2(Force.X, -JumpSpeed);
            onGround = false;
        }

        /// <summary>
        /// Draw whats needed this frame
        /// </summary>
        /// <param name="gameTime">Current Game Time</param>
        /// <param name="spriteBatch">Sprite Batch</param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Draw(enemyTexture, Position, null, Color.White, Rotation.Z, Vector2.Zero, Scale, SpriteEffects.None, 0.0f);
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
            }
            base.OnCollision(col);
        }
        
    }
}
