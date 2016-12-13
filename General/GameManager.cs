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
        const int PlatformDistance = 125;
        const int YPlatformBuffer = 500;
        public enum GameState { Countdown, Play, GameOver };

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        PhysicsManager physics;
        AIManager ai;
        PlatformGenerator platformGenerator;
        float nextPlatformY = 0;
        Player player;
        Enemy enemy;

        Camera camera;
        GameState currentGameState;
        const int countdownStart = 5;
        int countdownVal;
        float countdownTimer = 0;
        Vector2 camPosOnDebug;
        Texture2D[] countdownTextures = new Texture2D[6];
        Texture2D winTexture;
        Texture2D lossTexture;
        bool didPlayerWin = false;

        KeyboardState oldState;
        List<GameObject> gameObjects = new List<GameObject>();
        Platform[] previousPlatformRow;

        TimeSpan updateStep;

        const float shakeSpeed = 5.0f;
        const float shakeScale = 10.0f;

        public GameManager()
        {
            camPosOnDebug = new Vector2(0);
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            physics = new PhysicsManager();
            ai = new AIManager();
            platformGenerator = new PlatformGenerator();
            platformGenerator.AIReference = ai;

            //Run at a fixed step at 60 FPS
            IsFixedTimeStep = true;
            updateStep = TimeSpan.FromSeconds((1.0f / 60.0f));
            TargetElapsedTime = updateStep;

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
            platformGenerator.Initialize();

            //Init Keyboard State
            oldState = Keyboard.GetState();

            //Create Camera
            camera = new Camera();
            camera.Viewport = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            camera.Position = new Vector2(0);

            //Init Game State
            countdownVal = countdownStart;

            //Create the Player Component
            player = new Player(new RigidBody2D(new Vector2(0),
                                                new Vector2(0.3f, 0.3f),
                                                new Vector3(0, 0, 0),
                                                0.5f,
                                                false,
                                                new RigidBody2D.FrictionCoefficients() { StaticCoefficient = 0.0f, DynamicCoefficient = 0.0f },
                                                0.8f),
                                75.0f,
                                7500.0f);

            player.BoxCollider = new Vector2(31.5f, 45);
            player.Tag = "Player";
            player.CameraRef = camera;
            //Add to the collections
            physics.RigidBodies.Add(player);
            gameObjects.Add(player);

            //Create Enemy Component
            enemy = new Enemy(new RigidBody2D(new Vector2(0),
                                              new Vector2(0.3f, 0.3f),
                                              new Vector3(0, 0, 0),
                                              0.5f,
                                              false,
                                              new RigidBody2D.FrictionCoefficients() { StaticCoefficient = 0.0f, DynamicCoefficient = 0.0f },
                                              0.8f),
                                 90.0f,
                                 7700.0f,
                                 updateStep);
            enemy.AI = ai;
            enemy.BoxCollider = new Vector2(31.5f, 45);
            enemy.Tag = "Enemy";
            enemy.CameraRef = camera;
            physics.RigidBodies.Add(enemy);
            gameObjects.Add(enemy);
            platformGenerator.EnemyReference = enemy;

            //Create the Floor Object
            Platform ground = new Platform();
            ground.Mass = 200;
            ground.IsStaticHorizontal = true;
            ground.Position = new Vector2(0, camera.Viewport.Y - 20);
            ground.Scale = new Vector2(0.2f, 0.2f);
            ground.Size = new Vector2(camera.Viewport.X, 20);
            ground.BoxCollider = new Vector2(camera.Viewport.X, 20);
            ground.Friction = new RigidBody2D.FrictionCoefficients() { StaticCoefficient = 0.7f, DynamicCoefficient = 0.7f };
            ground.Bounciness = 0.0f;

            WaypointNode leftGround = new WaypointNode();
            leftGround.Position = new Vector2(ground.Position.X + PlatformGenerator.WaypointEdgeBuffer, ground.Position.Y - player.BoxCollider.Y);
            leftGround.ConnectedPlatform = ground;

            WaypointNode rightGround = new WaypointNode();
            rightGround.Position = new Vector2(ground.Position.X + ground.Size.X - PlatformGenerator.WaypointEdgeBuffer, ground.Position.Y - player.BoxCollider.Y);
            rightGround.ConnectedPlatform = ground;

            leftGround.ConnectedNodes.Add(rightGround);
            rightGround.ConnectedNodes.Add(leftGround);
            ground.ConnectedWaypoints.AddRange(new WaypointNode[] { leftGround, rightGround });
            ai.WaypointNetwork.AddRange(new WaypointNode[] { leftGround, rightGround });

            previousPlatformRow = new Platform[] { ground };

            //Set initial enemy & player positions
            player.Position = leftGround.Position;
            enemy.Position = new Vector2(rightGround.Position.X - enemy.BoxCollider.X, rightGround.Position.Y);
            enemy.NextNode = rightGround;
            enemy.PreviousNode = rightGround;

            //Create Background Object
            Background bg = new Background();
            bg.SceneCamera = camera;
            bg.TextureFile = "blue_grass.png";

            gameObjects.Add(ground);
            physics.RigidBodies.Add(ground);
            gameObjects.Add(bg);
            
            //Create the initial on-screen platforms
            List<Platform> platforms = new List<Platform>();
            for (int i = (int)camera.Viewport.Y - PlatformDistance; i > 0 - YPlatformBuffer; i -= PlatformDistance)
            {
                Platform[] platformRow = platformGenerator.GeneratePlatforms(new Vector2(0, i), camera.Viewport.X);
                platforms.AddRange(platformRow);

                WaypointNode[] genPlatforms = platformGenerator.GenerateWaypoints(platformRow, previousPlatformRow, enemy.BoxCollider.Y);
                previousPlatformRow = platformRow;
                ai.WaypointNetwork.AddRange(genPlatforms);
            }
            for (int i = 0; i < platforms.Count; i++)
            {
                platforms[i].Initialize();
                gameObjects.Add(platforms[i]);
                physics.RigidBodies.Add(platforms[i]);
            }

            nextPlatformY = camera.Position.Y - YPlatformBuffer;

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

            countdownTextures[0] = Content.Load<Texture2D>("textClimb.png");
            countdownTextures[1] = Content.Load<Texture2D>("number1.png");
            countdownTextures[2] = Content.Load<Texture2D>("number2.png");
            countdownTextures[3] = Content.Load<Texture2D>("number3.png");
            countdownTextures[4] = Content.Load<Texture2D>("number4.png");
            countdownTextures[5] = Content.Load<Texture2D>("number5.png");
            winTexture = Content.Load<Texture2D>("textYouWin.png");
            lossTexture = Content.Load<Texture2D>("textYouLose.png");

            ai.LoadContent(Content);
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

            if (IsActive)
            {
                if (currentGameState == GameState.Play)
                {
                    //Camera Logic - Move Upwards
                    camera.Update(gameTime);

                    if (!DebugMode)
                    {
                        //As platforms move off-screen, mark them for destructions
                        bool respawnPlatforms = false;
                        List<RigidBody2D> objectsToRemove = new List<RigidBody2D>();
                        for (int i = 0; i < physics.RigidBodies.Count; i++)
                        {
                            if (!camera.IsInViewport(physics.RigidBodies[i]) && physics.RigidBodies[i].Position.Y > (camera.Position + camera.Viewport).Y)
                            {
                                if (physics.RigidBodies[i].Tag == "Ground")
                                {
                                    respawnPlatforms = true;

                                    Platform plat = (Platform)physics.RigidBodies[i];
                                    //If dynamic allow Platform Gen to make a new one
                                    if (plat.PlatformType != Platform.PlatformTypes.Static)
                                    {
                                        platformGenerator.ContainsDynamic = false;
                                    }

                                    for (int j = 0; j < plat.ConnectedWaypoints.Count; j++)
                                    {
                                        ai.WaypointNetwork.Remove(plat.ConnectedWaypoints[j]);
                                    }
                                    gameObjects.Remove((GameObject)physics.RigidBodies[i]);
                                    objectsToRemove.Add(physics.RigidBodies[i]);
                                }
                                else if (physics.RigidBodies[i].Tag == "Player")
                                {
                                    currentGameState = GameState.GameOver;
                                    didPlayerWin = false;
                                }
                                else if (physics.RigidBodies[i].Tag == "Enemy")
                                {
                                    currentGameState = GameState.GameOver;
                                    didPlayerWin = true;
                                }

                            }
                        }

                        //Remove the platforms
                        for (int i = 0; i < objectsToRemove.Count; i++)
                        {
                            physics.RigidBodies.Remove(objectsToRemove[i]);
                        }

                        //If we destroyed some, repopulate some more at the top of the screen
                        if (respawnPlatforms)
                        {
                            Platform[] platformRow = platformGenerator.GeneratePlatforms(new Vector2(camera.Position.X, nextPlatformY), camera.Viewport.X);
                            nextPlatformY -= PlatformDistance;
                            for (int i = 0; i < platformRow.Length; i++)
                            {
                                platformRow[i].Initialize();
                                platformRow[i].LoadContent(Content);
                                gameObjects.Add(platformRow[i]);
                                physics.RigidBodies.Add(platformRow[i]);
                            }
                            WaypointNode[] genPlatforms = platformGenerator.GenerateWaypoints(platformRow, previousPlatformRow, player.BoxCollider.Y);
                            previousPlatformRow = platformRow;
                            ai.WaypointNetwork.AddRange(genPlatforms);
                        }
                    }

                    //Game Logic
                    ControllerHandler();

                    for (int i = 0; i < gameObjects.Count; i++)
                    {
                        gameObjects[i].Update(gameTime);
                    }

                }
                else if (currentGameState == GameState.Countdown)
                {
                    if ((countdownTimer += (float)gameTime.ElapsedGameTime.TotalSeconds) > 1)
                    {
                        countdownTimer = 0;
                        if (--countdownVal < 0)
                        {
                            currentGameState = GameState.Play;
                        }
                    }
                }
                else if (currentGameState == GameState.GameOver)
                {
                    //TODO
                    //Play again button
                }
                //Call Physics
                physics.Step();

                base.Update(gameTime);
            }
        }

        /// <summary>
        /// Control Handler for User Input
        /// </summary>
        private void ControllerHandler()
        {
            KeyboardState newState = Keyboard.GetState();

            //Enable Debug mode
            if (newState.IsKeyDown(Keys.P) && !oldState.IsKeyDown(Keys.P))
            {
                if (DebugMode)
                {
                    DebugMode = false;
                    camera.Position = camPosOnDebug;
                }
                else
                {
                    DebugMode = true;
                    camPosOnDebug = camera.Position;
                }
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

            ai.DebugDraw(gameTime, spriteBatch, GraphicsDevice);
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Draw(gameTime, spriteBatch, GraphicsDevice);
            }

            //Draw UI
            if (currentGameState == GameState.Countdown)
            {
                Vector2 midPoint = camera.GetViewportMid();
                float scale = MathHelper.Lerp(1.25f, 0.9f, countdownTimer);
                //Display the current number
                spriteBatch.Draw(countdownTextures[countdownVal],
                                 midPoint,
                                 null,
                                 Color.White,
                                 0.0f,
                                 countdownTextures[countdownVal].Bounds.Center.ToVector2(),
                                 scale,
                                 SpriteEffects.None,
                                 0.0f);
            }
            else if (currentGameState == GameState.GameOver)
            {
                string text = didPlayerWin ? "YOU WIN!" : "YOU LOSE!";
                Vector2 position = camera.GetViewportMid();
                float scale = 1.0f;
                //Y sin wave change
                float yChange = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * shakeSpeed) * shakeScale;
                position += new Vector2(0, yChange);

                //Display Game Over Message
                spriteBatch.Draw(didPlayerWin ? winTexture : lossTexture,
                                 position,
                                 null,
                                 Color.White,
                                 0.0f,
                                 didPlayerWin ? winTexture.Bounds.Center.ToVector2() : lossTexture.Bounds.Center.ToVector2(),
                                 scale,
                                 SpriteEffects.None,
                                 0.0f);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
