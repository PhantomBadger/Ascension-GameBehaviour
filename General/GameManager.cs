using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Physics;
using AI;
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
        const int InitPlatformDistance = 125;
        const int YPlatformBuffer = 500;
        public enum GameState { Countdown, Play, GameOver };

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        PhysicsManager physics;
        AIManager ai;
        Player player;
        Camera camera;
        GameState currentGameState;

        KeyboardState oldState;
        List<GameObject> gameObjects = new List<GameObject>();

        public GameManager()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            physics = new PhysicsManager();
            ai = new AIManager();

            //Run at a fixed step at 60 FPS
            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromMilliseconds((1.0f / 60.0f) * 1000);

            //Set screen size
            graphics.PreferredBackBufferHeight = 750;
            graphics.PreferredBackBufferWidth = 600;
            graphics.IsFullScreen = false;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //Init AI State
            ai.Initialize(GraphicsDevice);

            oldState = Keyboard.GetState();

            //Create Camera
            camera = new Camera();
            camera.Viewport = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            camera.Position = new Vector2(0);

            //Create the Player Component
            player = new Player(new RigidBody2D(new Vector2(0, 0),
                                                new Vector2(1, 1),
                                                new Vector3(0, 0, 0),
                                                0.5f,
                                                false,
                                                new RigidBody2D.FrictionCoefficients() { StaticCoefficient = 0.0f, DynamicCoefficient = 0.0f },
                                                0.8f),
                                50.0f,
                                7500.0f);
            player.BoxCollider = new Vector2(32, 32);
            player.Tag = "Player";
            //Add to the collections
            physics.RigidBodies.Add(player);
            gameObjects.Add(player);

            //Create the Floor Object
            Platform ground = new Platform();
            ground.Mass = 200;
            ground.IsStatic = true;
            ground.Position = new Vector2(0, camera.Viewport.Y - 20);
            ground.Size = new Vector2(camera.Viewport.X, 20);
            ground.BoxCollider = new Vector2(camera.Viewport.X, 20);
            ground.Friction = new RigidBody2D.FrictionCoefficients() { StaticCoefficient = 0.9f, DynamicCoefficient = 0.9f };
            ground.Bounciness = 0.0f;

            gameObjects.Add(ground);
            physics.RigidBodies.Add(ground);

            //Create the initial on-screen platforms
            List<Platform> platforms = new List<Platform>();
            for (int i = (int)camera.Viewport.Y - InitPlatformDistance; i > 0 - YPlatformBuffer; i -= InitPlatformDistance)
            {
                platforms.AddRange(ai.GeneratePlatforms(new Vector2(0, i), camera.Viewport.X));
            }
            for (int i = 0; i < platforms.Count; i++)
            {
                platforms[i].Initialize();
                gameObjects.Add(platforms[i]);
                physics.RigidBodies.Add(platforms[i]);
            }

            //Initialise all our game objects
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Initialize();
            }
            base.Initialize();

            //Set the initial game state
            currentGameState = GameState.Countdown;
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

            if (currentGameState == GameState.Play)
            {
                //Camera Logic - Move Upwards
                camera.Update(gameTime);

                //As platforms move off-screen, mark them for destructions
                bool respawnPlatforms = false;
                List<int> objectsToRemove = new List<int>();
                for (int i = 0; i < physics.RigidBodies.Count; i++)
                {
                    if (!camera.IsInViewport(physics.RigidBodies[i]) && physics.RigidBodies[i].Tag == "Ground")
                    {
                        respawnPlatforms = true;
                        gameObjects.Remove((GameObject)physics.RigidBodies[i]);
                        objectsToRemove.Add(i);
                    }
                }

                //Remove the platforms
                for (int i = 0; i < objectsToRemove.Count; i++)
                {
                    physics.RigidBodies.RemoveAt(objectsToRemove[i] - i);
                }

                //If we destroyed some, repopulate some more at the top of the screen
                if (respawnPlatforms)
                {
                    Platform[] platforms = ai.GeneratePlatforms(new Vector2(camera.Position.X, camera.Position.Y - YPlatformBuffer), camera.Viewport.X);
                    for (int i = 0; i < platforms.Length; i++)
                    {
                        platforms[i].Initialize();
                        platforms[i].LoadContent(Content);
                        gameObjects.Add(platforms[i]);
                        physics.RigidBodies.Add(platforms[i]);
                    }
                }
            }

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
            KeyboardState newState = Keyboard.GetState();

            //Enable Debug mode
            if (newState.IsKeyDown(Keys.P) && !oldState.IsKeyDown(Keys.P))
            {
                DebugMode = !DebugMode;
            }

            if (newState.IsKeyDown(Keys.U) && !oldState.IsKeyDown(Keys.U))
            {
                currentGameState = GameState.Play;
            }

            oldState = newState;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Draw Code
            spriteBatch.Begin(SpriteSortMode.BackToFront,
                              BlendState.AlphaBlend,
                              null,
                              null,
                              null,
                              null,
                              camera.GetTranslationMatrix());
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Draw(gameTime, spriteBatch, GraphicsDevice);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
