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
        public float Mass { get; set; }
        public bool IsStaticHorizontal { get; set; }
        public bool IsStaticVertical { get; set; }
        public bool IsIgnoringGravity { get; set; }
        public FrictionCoefficients Friction { get; set; }
        public Vector2 ActiveStatic { get; set; }
        public Vector2 ActiveDynamic { get; set; }
        public float Bounciness { get; set; }

        private SpriteFont debugFont;
        private Texture2D velocityTex;
        private Texture2D colTex;

        const float AirResistance = 0.001f;
        const float MinPosChange = 0.1f;
        const float VelocityFloor = 0.05f;

        /// <summary>
        /// Constructor for the RigidBody2D Class with full information
        /// Anything affected by Physics should be, or inherit from, the RigidBody
        /// </summary>
        /// <param name="newTransform">The Transform of the Object</param>
        /// <param name="newScale">The Scale of the Object</param>
        /// <param name="newRotation">The Rotation of the Object</param>
        /// <param name="newMass">The Mass of the Object</param>
        /// <param name="isStatic">Whether the object is movable</param>
        public RigidBody2D(Vector2 newTransform, Vector2 newScale, Vector3 newRotation, float newMass, bool isStatic, FrictionCoefficients newFriction, float newBounciness) : 
            base(newTransform, newScale, newRotation)
        {
            Mass = newMass;
            IsStaticHorizontal = isStatic;
            IsStaticVertical = isStatic;
            IsIgnoringGravity = false;
            Friction = newFriction;
            Bounciness = newBounciness;
        }

        /// <summary>
        /// Default Constructor for the RigidBody2D Class
        /// Anything affected by Physics should be, or inherit from, the RigidBody
        /// </summary>
        public RigidBody2D() :
            base()
        {
            Mass = 1;
            IsStaticHorizontal = false;
            IsStaticVertical = false;
            IsIgnoringGravity = false;
            Friction = new FrictionCoefficients() { StaticCoefficient = 0.0f, DynamicCoefficient = 0.0f };
            Bounciness = 0.0f;
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
            Acceleration = CalculateAcceleration(Force);

            //Calculate Velocity using V = at
            Velocity += (Acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds);

            //Add Air Resistance Drag to Velocity so there's a "Terminal Velocity"
            Velocity *= (1 - AirResistance);

            //Use Euler Integration to update Position
            //If the velocity is too low do not
            if (Math.Abs(Velocity.X) > MinPosChange || Math.Abs(Velocity.Y) > MinPosChange)
            {
                Position += (Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds);
            }

            //Reset Force so it's not continually applied
            Force = new Vector2(0);

            //Reset Friction
            ActiveStatic = new Vector2(0);
            ActiveDynamic = new Vector2(0);

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
            //Spawn debug textures
            if (velocityTex == null)
            {
                velocityTex = new Texture2D(graphicsDevice, 1, 1);
                velocityTex.SetData(new Color[] { Color.DarkGray });
            }
            if (colTex == null)
            {
                colTex = new Texture2D(graphicsDevice, 1, 1);
                colTex.SetData(new Color[] { Color.Lime });
            }

            if (GameManager.DebugMode)
            {
                //Draw Debug Lines
                Vector2 velocityLine = (Position + Velocity) - Position;
                float lineAngle = (float)Math.Atan2(velocityLine.Y, velocityLine.X);

                spriteBatch.Draw(velocityTex,
                    new Rectangle((int)Position.X, (int)Position.Y, (int)velocityLine.Length(), 3),
                    null,
                    Color.White,
                    lineAngle,
                    new Vector2(0, 0),
                    SpriteEffects.None,
                    0);
                    
                float frictionAngle = (float)Math.Atan2(ActiveDynamic.Y, ActiveDynamic.X);

                spriteBatch.Draw(velocityTex,
                    new Rectangle((int)Position.X, (int)Position.Y, (int)ActiveDynamic.Length(), 3),
                    null,
                    Color.Red,
                    frictionAngle,
                    new Vector2(0,0),
                    SpriteEffects.None,
                    0);


                spriteBatch.DrawString(debugFont, $"({Velocity.X}, {Velocity.Y})", new Vector2(Position.X, Position.Y - 15), Color.Red);
                //spriteBatch.DrawString(debugFont, $"Acceleration: ({Acceleration.X}, {Acceleration.Y})", new Vector2(0, 0), Color.White);
                //spriteBatch.DrawString(debugFont, $"Velocity: ({Velocity.X}, {Velocity.Y})", new Vector2(0, 20), Color.White);

                //Draw Debug Box Collider

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

        /// <summary>
        /// Handle specific actions relative to collisions
        /// </summary>
        /// <param name="col"></param>
        public virtual void OnCollision(CollisionPair col)
        {
            //Do Nothing
        }

        /// <summary>
        /// Utility Method to calculate the acceleration of this object given a Force
        /// </summary>
        /// <param name="Force">Force to Apply</param>
        /// <returns>Calculated Acceleration as a Vector2</returns>
        protected Vector2 CalculateAcceleration(Vector2 Force)
        {
            //Apply the Friction to the current Force
            if (Velocity.X == 0)
            {
                //Static Friction
                Force += ActiveStatic;
            }
            else
            {
                //Dynamic Friction
                Force += ActiveDynamic;
            }

            //Calculate acceleration using F/m
            if (!IsIgnoringGravity)
            {
                //If we're not ignoring gravity, apply it as a constant force
                return (Force + GameManager.Gravity) * (1 / Mass);
            }
            else
            {
                //If we are, ignore it
                return Force * (1 / Mass);
            }
        }

    }
}
