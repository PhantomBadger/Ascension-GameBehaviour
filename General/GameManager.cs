using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Physics;
using System;

namespace General
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class GameManager : Game
    {
        public static bool DebugMode = false;
        public static Vector2 Gravity = new Vector2(0, 98.0f);

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        PhysicsManager physics;
        Player player;

        List<GameObject> gameObjects = new List<GameObject>();

        public GameManager()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            physics = new PhysicsManager();

            //Run at a fixed step at 60 FPS
            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromMilliseconds((1.0f / 60.0f) * 1000);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //Create the Player Component
            player = new Player(new RigidBody2D(new Vector2(0, 0),
                                                new Vector2(1, 1),
                                                new Vector3(0, 0, 0),
                                                1,
                                                false,
                                                new RigidBody2D.FrictionCoefficients() { StaticCoefficient = 0.0f, DynamicCoefficient = 0.0f }),
                                50.0f,
                                196.0f);
            player.BoxCollider = new Vector2(32, 32);
            //Add to the collections
            physics.RigidBodies.Add(player);
            gameObjects.Add(player);

            //Create the Ground Object
            Platform ground = new Platform();
            ground.Mass = 200;
            ground.IsStatic = true;
            ground.Position = new Vector2(0, 300);
            ground.Size = new Vector2(500, 20);
            ground.BoxCollider = new Vector2(500, 20);
            ground.Friction = new RigidBody2D.FrictionCoefficients() { StaticCoefficient = 100.5f, DynamicCoefficient = 1.2f };

            gameObjects.Add(ground);
            physics.RigidBodies.Add(ground);

            //Create an additional platform
            Platform platform = new Platform();
            platform.Mass = 200;
            platform.IsStatic = true;
            platform.Position = new Vector2(100, 200);
            platform.Size = new Vector2(500, 20);
            platform.BoxCollider = new Vector2(500, 20);
            platform.Friction = new RigidBody2D.FrictionCoefficients() { StaticCoefficient = 0.5f, DynamicCoefficient = 0.2f };

            gameObjects.Add(platform);
            physics.RigidBodies.Add(platform);

            //Initialise all our game objects
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Initialize();
            }
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].LoadContent(Content);
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].UnloadContent();
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            //Game Logic
            ControllerHandler();

            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Update(gameTime);
            }

            //Call Physics
            physics.Step();

            base.Update(gameTime);
        }

        private void ControllerHandler()
        {
            //Enable Debug mode
            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                DebugMode = !DebugMode;
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Draw Code
            spriteBatch.Begin();
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Draw(gameTime, spriteBatch, GraphicsDevice);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
