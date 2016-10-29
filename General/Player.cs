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

        private Texture2D playerTexture;

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
        /// Load the Content Required for the player, such as Sprites and Sounds
        /// </summary>
        /// <param name="content">Content Manager</param>
        public override void LoadContent(ContentManager content)
        {
            playerTexture = content.Load<Texture2D>("playerSprite.png");
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

            //Call RigidBody Update
            base.Update(gameTime);
        }

        /// <summary>
        /// Draw whats needed this frame
        /// </summary>
        /// <param name="gameTime">Current Game Time</param>
        /// <param name="spriteBatch">Sprite Batch</param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Rectangle drawRect = new Rectangle((int)Transform.X, (int)Transform.Y, playerTexture.Width, playerTexture.Height);
            spriteBatch.Draw(playerTexture, drawRect, Color.White);
            base.Draw(gameTime, spriteBatch);
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
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                //Move Down?
                Force = new Vector2(Force.X, PlayerSpeed);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                //Move Up?
                Force = new Vector2(Force.X, -PlayerSpeed);
            }
        }
    }
}
