using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Physics;
using MonoGame.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame;

namespace General
{
    class Player : RigidBody2D
    {
        public float PlayerSpeed { get; set; }
        public float JumpSpeed { get; set; }
        public Camera CameraRef { get; set; }

        private Texture2D playerTexture;
        private Texture2D offscreenTex;
        private bool onGround = false;

        private const float OffscreenIndicatorOffset = 10;

        /// <summary>
        /// Constructor for Player Class with full info
        /// </summary>
        /// <param name="newRigidBody">RigidBody component of the player</param>
        /// <param name="newSpeed">Player's default movement speed</param>
        /// /// <param name="newJumpSpeed">Player's default jump speed</param>
        public Player(RigidBody2D newRigidBody, float newSpeed, float newJumpSpeed) : 
            base(newRigidBody.Position, newRigidBody.Scale, newRigidBody.Rotation, newRigidBody.Mass, newRigidBody.IsStaticHorizontal, newRigidBody.Friction, newRigidBody.Bounciness)
        {
            PlayerSpeed = newSpeed;
            JumpSpeed = newJumpSpeed;
            Tag = "Player";
        }

        /// <summary>
        /// Load the Content Required for the player, such as Sprites and Sounds
        /// </summary>
        /// <param name="content">Content Manager</param>
        public override void LoadContent(ContentManager content)
        {
            playerTexture = content.Load<Texture2D>("alienGreen_front.png");
            offscreenTex = content.Load<Texture2D>("offscreenIndicator_player.png");
            base.LoadContent(content);
        }

        /// <summary>
        /// Unload the Content Required for the player, such as Sprites and Sounds
        /// </summary>
        public override void UnloadContent()
        {
            playerTexture.Dispose();
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
        /// Update Method for Player, Called once a frame
        /// </summary>
        /// <param name="gameTime">Current Game Time</param>
        public override void Update(GameTime gameTime)
        {
            //Handle User Input
            ControllerHandler();
            onGround = false;

            //Console.WriteLine($"Position {Position.X}, {Position.Y}");

            //Call RigidBody Update
            base.Update(gameTime);

            //Console.WriteLine($"Player Active Friction Dynamic ({ActiveDynamic.X}, {ActiveDynamic.Y}) Static ({ActiveStatic.X}, {ActiveStatic.Y})");
        }

        /// <summary>
        /// Draw whats needed this frame
        /// </summary>
        /// <param name="gameTime">Current Game Time</param>
        /// <param name="spriteBatch">Sprite Batch</param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (CameraRef.IsInViewport(this))
            {
                spriteBatch.Draw(playerTexture, Position, null, Color.White, Rotation.Z, Vector2.Zero, Scale, SpriteEffects.None, 0.0f);
            }
            else
            {
                //Draw an arrow pointing to our location
                Vector2 midpoint = CameraRef.GetViewportMid();
                Vector2 direction = midpoint - Position;

                Vector2? offscreenAnchor = null;
                if (//Top of Viewport
                    (offscreenAnchor = PhysicsManager.FindLineIntersection(midpoint, Position, CameraRef.Position, CameraRef.Position + new Vector2(CameraRef.Viewport.X, 0))).HasValue ||
                    //Bottom of Viewport
                    (offscreenAnchor = PhysicsManager.FindLineIntersection(midpoint, Position, CameraRef.Position + new Vector2(0, CameraRef.Viewport.Y), CameraRef.Position + CameraRef.Viewport)).HasValue ||
                    //Right of Viewport
                    (offscreenAnchor = PhysicsManager.FindLineIntersection(midpoint, Position, CameraRef.Position + new Vector2(CameraRef.Viewport.X, 0), CameraRef.Position + CameraRef.Viewport)).HasValue ||
                    //Left of Viewport
                    (offscreenAnchor = PhysicsManager.FindLineIntersection(midpoint, Position, CameraRef.Position, CameraRef.Position + new Vector2(0, CameraRef.Viewport.Y))).HasValue)
                {
                    //Draw a line at the direction angle 
                    Vector2 offscreenAngle = offscreenAnchor.Value - Position;
                    float offscreenRot = (float)Math.Atan2(offscreenAngle.Y, offscreenAngle.X);

                    offscreenAngle.Normalize();

                    //Draw indicator
                    spriteBatch.Draw(offscreenTex, offscreenAnchor.Value + (offscreenAngle * OffscreenIndicatorOffset), null, Color.White, offscreenRot, new Vector2(0, offscreenTex.Height / 2), new Vector2(1), Math.Abs(offscreenRot) > (Math.PI / 2) ? SpriteEffects.FlipVertically : SpriteEffects.None, 0.005f);
                }
            }
            base.Draw(gameTime, spriteBatch, graphicsDevice);
        }

        /// <summary>
        /// Handle User Input
        /// </summary>
        private void ControllerHandler()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                //Move Right
                Force = new Vector2(PlayerSpeed, Force.Y);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                //Move Left
                Force = new Vector2(-PlayerSpeed, Force.Y);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && onGround)
            {
                Force = new Vector2(Force.X, -JumpSpeed);
                onGround = false;
            }
        }

        /// <summary>
        /// On Collision event to allow jumping again
        /// </summary>
        /// <param name="col">Collision pair data</param>
        public override void OnCollision(CollisionPair col)
        {
            RigidBody2D collidedObject = col.ObjectA == (RigidBody2D)this ? col.ObjectB : col.ObjectA;
            if (col.ContactNormal.Y == 1)
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
