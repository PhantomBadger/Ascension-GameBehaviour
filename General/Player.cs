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

        private Texture2D playerTexture;
        private bool onGround = false;

        /// <summary>
        /// Constructor for Player Class with full info
        /// </summary>
        /// <param name="newRigidBody">RigidBody component of the player</param>
        /// <param name="newSpeed">Player's default movement speed</param>
        public Player(RigidBody2D newRigidBody, float newSpeed, float newJumpSpeed) : 
            base(newRigidBody.Position, newRigidBody.Scale, newRigidBody.Rotation, newRigidBody.Mass, newRigidBody.IsStatic, newRigidBody.Friction, newRigidBody.Bounciness)
        {
            PlayerSpeed = newSpeed;
            JumpSpeed = newJumpSpeed;
            Tag = "Player";
        }

        /// <summary>
        /// Default Constructor for Player Class with just required speed
        /// </summary>
        /// <param name="newSpeed">Speed for the player</param>
        public Player(float newSpeed) : 
            base ()
        {
            PlayerSpeed = newSpeed;
            Tag = "Player";
        }

        /// <summary>
        /// Load the Content Required for the player, such as Sprites and Sounds
        /// </summary>
        /// <param name="content">Content Manager</param>
        public override void LoadContent(ContentManager content)
        {
            playerTexture = content.Load<Texture2D>("alienGreen_front.png");
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

            //Console.WriteLine($"Position {Transform.X}, {Transform.Y}");

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
            spriteBatch.Draw(playerTexture, Position, null, Color.White, Rotation.Z, Vector2.Zero, Scale, SpriteEffects.None, 0.0f);
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

        public override void OnCollision(CollisionPair col)
        {
            RigidBody2D collidedObject = col.ObjectA == (RigidBody2D)this ? col.ObjectB : col.ObjectA;
            if (collidedObject.Tag == "Ground" && collidedObject.Position.Y >= Position.Y)
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
