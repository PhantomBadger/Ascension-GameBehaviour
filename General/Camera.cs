using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Physics;

namespace General
{
    class Camera
    {
        public Vector2 Position { get; set; }
        public Vector2 Viewport { get; set; }

        private float moveRate = 0.0f;
        private float increaseCounter = 0.0f;

        private const float MaxMoveRate = 2.5f;
        private const float RateStep = 0.1f;
        private const float IncreaseRate = 7.5f;

        /// <summary>
        /// Get the middle of the screen
        /// </summary>
        /// <returns>Vector2 of the screen's center</returns>
        public Vector2 GetViewportMid()
        {
            return (Viewport / 2.0f);
        }

        /// <summary>
        /// Gets a Translation Matrix
        /// </summary>
        /// <returns>Returns a Translation Matrix with Translation, Rotation & Scale</returns>
        public Matrix GetTranslationMatrix()
        {
            return Matrix.CreateTranslation(new Vector3(-(int)Position.X, -(int)Position.Y, 0)) *
                    Matrix.CreateRotationZ(0) *
                    Matrix.CreateScale(new Vector3(1));
        }

        public void Update(GameTime gameTime)
        {
            if (!GameManager.DebugMode)
            {
                if (moveRate < MaxMoveRate && 
                    (increaseCounter += (float)gameTime.ElapsedGameTime.TotalSeconds) > IncreaseRate)
                {
                    increaseCounter = 0;
                    moveRate += RateStep;
                }

                //Auto Scroll Up
                Position -= new Vector2(0, moveRate);
            }
            else
            {
                //If in debug, allow manual control
                ControlHandler();
            }
        }

        private void ControlHandler()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                Position += new Vector2(moveRate * 10, 0);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                Position -= new Vector2(0, moveRate * 10);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                Position -= new Vector2(moveRate * 10, 0);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                Position += new Vector2(0, moveRate * 10);
            }
        }

        /// <summary>
        /// Is the given object in view of the camera
        /// </summary>
        /// <param name="rigidbody">The rigidbody object to compare</param>
        /// <returns>True or false of whether its visible or not</returns>
        public bool IsInViewport(RigidBody2D rigidbody)
        {
            //Use a simple AABB Detection with the current viewport and the 
            //rigidbody's box collider
            return (Position.X + Viewport.X > rigidbody.Position.X &&
                    Position.X - Viewport.X < rigidbody.Position.X + rigidbody.BoxCollider.X &&
                    Position.Y + Viewport.Y > rigidbody.Position.Y &&
                    Position.Y - Viewport.Y < rigidbody.Position.Y + rigidbody.BoxCollider.Y);
        }
    }
}
