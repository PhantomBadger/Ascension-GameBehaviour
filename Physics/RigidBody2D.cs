using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using General;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Physics
{
    class RigidBody2D : GameObject
    {
        public struct FrictionCoefficients
        {
            public float StaticCoefficient;
            public float DynamicCoefficient;
        }

        public Vector2 Velocity { get; set; }
        public Vector2 Acceleration { get; set; }
        public Vector2 Force { get; set; }
        public Vector2 BoxCollider { get; set; }
        public int Mass { get; set; }
        public bool IsStatic { get; set; }
        public bool IsIgnoringGravity { get; set; }
        public FrictionCoefficients Friction { get; set; }
        public Vector2 ActiveStatic { get; set; }
        public Vector2 ActiveDynamic { get; set; }
        public int frictionCount = 0;

        private SpriteFont debugFont;

        const float AirResistance = 0.001f;
        const float MinPosChange = 0.1f;
        const float VelocityFloor = 0.005f;

        /// <summary>
        /// Constructor for the RigidBody2D Class with full information
        /// Anything affected by Physics should be, or inherit from, the RigidBody
        /// </summary>
        /// <param name="newTransform">The Transform of the Object</param>
        /// <param name="newScale">The Scale of the Object</param>
        /// <param name="newRotation">The Rotation of the Object</param>
        /// <param name="newMass">The Mass of the Object</param>
        /// <param name="isStatic">Whether the object is movable</param>
        public RigidBody2D(Vector2 newTransform, Vector2 newScale, Vector3 newRotation, int newMass, bool isStatic, FrictionCoefficients newFriction) : 
            base(newTransform, newScale, newRotation)
        {
            Mass = newMass;
            IsStatic = isStatic;
            IsIgnoringGravity = false;
            Friction = newFriction;
        }

        /// <summary>
        /// Default Constructor for the RigidBody2D Class
        /// Anything affected by Physics should be, or inherit from, the RigidBody
        /// </summary>
        public RigidBody2D() :
            base()
        {
            Mass = 1;
            IsStatic = false;
            IsIgnoringGravity = false;
            Friction = new FrictionCoefficients() { StaticCoefficient = 0.0f, DynamicCoefficient = 0.0f };
        }

        /// <summary>
        /// Load the Content Required for this RigidBody
        /// </summary>
        /// <param name="content"></param>
        public override void LoadContent(ContentManager content)
        {
            //Load Debug Font
            debugFont = content.Load<SpriteFont>("DebugFont");
        }

        /// <summary>
        /// Unload the Content Required for this RigidBody
        /// </summary>
        public override void UnloadContent()
        {
            //Do Nothing
        }

        /// <summary>
        /// Initialize Method called at the start
        /// </summary>
        public override void Initialize()
        {
            Velocity = new Vector2();
            Acceleration = new Vector2();
        }

        /// <summary>
        /// A Method called every frame for a RigidBody2D
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            //Calculate acceleration using F/m
            if (!IsIgnoringGravity)
            {
                //If we're not ignoring gravity, apply it as a constant force
                Acceleration = (Force + GameManager.Gravity) * (1 / Mass);
            }
            else
            {
                //If we are, ignore it
                Acceleration = Force * (1 / Mass);
            }

            if (!IsStatic)
            {
                //Calculate Velocity using V = at
                Velocity += (Acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds);

                //Add Air Resistance Drag to Velocity so there's a "Terminal Velocity"
                Velocity *= (1 - AirResistance);

                //Apply Friction
                if (Velocity.X == 0)
                {
                    Velocity = new Vector2(Math.Max(Velocity.X + ActiveStatic.X, 0), Velocity.Y + ActiveStatic.Y);
                }
                else
                {
                    Velocity = new Vector2(Math.Max(Velocity.X + ActiveDynamic.X, 0), Velocity.Y + ActiveDynamic.Y);
                }

                //Use Euler Integration to update Position
                //If the velocity is too low do not
                if (Math.Abs(Velocity.X) > MinPosChange || Math.Abs(Velocity.Y) > MinPosChange)
                {
                    Position += (Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds);
                }
            }

            //Reset Force so it's not continually applied
            Force = new Vector2(0, 0);

            //Floor Velocity to zero to prevent an incredibly small number - Prevents long decimal numbers when debugging
            Velocity = (Math.Abs(Velocity.X) < VelocityFloor ? new Vector2(0, Velocity.Y) : Velocity);
            Velocity = (Math.Abs(Velocity.Y) < VelocityFloor ? new Vector2(Velocity.X, 0) : Velocity);
        }

        /// <summary>
        /// Draw any RigidBody Debug Info (Force Arrows, etc)
        /// </summary>
        /// <param name="gameTime">Current Game Time</param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (GameManager.DebugMode)
            {
                //Draw Debug Lines
                Vector2 velocityLine = (Position + Velocity) - Position;
                float lineAngle = (float)Math.Atan2(velocityLine.Y, velocityLine.X);

                Texture2D velocityTex = new Texture2D(graphicsDevice, 1, 1);
                velocityTex.SetData(new Color[] { Color.DarkGray });

                spriteBatch.Draw(velocityTex,
                    new Rectangle((int)Position.X, (int)Position.Y, (int)velocityLine.Length(), 3),
                    null,
                    Color.White,
                    lineAngle,
                    new Vector2(0, 0),
                    SpriteEffects.None,
                    0);


                spriteBatch.DrawString(debugFont, $"({Velocity.X}, {Velocity.Y})", new Vector2(Position.X, Position.Y - 15), Color.White);
                //spriteBatch.DrawString(debugFont, $"Acceleration: ({Acceleration.X}, {Acceleration.Y})", new Vector2(0, 0), Color.White);
                //spriteBatch.DrawString(debugFont, $"Velocity: ({Velocity.X}, {Velocity.Y})", new Vector2(0, 20), Color.White);

                //Draw Debug Box Collider
                Texture2D colTex = new Texture2D(graphicsDevice, 1, 1);
                colTex.SetData(new Color[] { Color.Lime });

                //Top Line
                spriteBatch.Draw(colTex, new Rectangle((int)Position.X, (int)Position.Y, (int)BoxCollider.X, 3), Color.White);

                //Bottom Line
                spriteBatch.Draw(colTex, new Rectangle((int)Position.X, (int)(Position.Y + BoxCollider.Y), (int)BoxCollider.X, 3), Color.White);

                //Left Line
                spriteBatch.Draw(colTex, new Rectangle((int)Position.X, (int)Position.Y, 3, (int)BoxCollider.Y), Color.White);

                //Right Line
                spriteBatch.Draw(colTex, new Rectangle((int)(Position.X + BoxCollider.X), (int)Position.Y, 3, (int)BoxCollider.Y + 3), Color.White);
            }
        }

    }
}
