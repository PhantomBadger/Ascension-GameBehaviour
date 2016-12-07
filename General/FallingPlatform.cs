using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using General;
using Microsoft.Xna.Framework;
using Physics;

namespace General
{
    class FallingPlatform : Platform
    {
        bool triggerFall = false;
        bool hasFallen = false;
        float fallCounter = 0;

        const float FallCounterMax = 2.5f;
        const float shakeSpeed = 50.0f;
        const float shakeScale = 1.25f;

        /// <summary>
        /// Update the Falling Platform Position
        /// </summary>
        /// <param name="gameTime">Current Game Time</param>
        public override void Update(GameTime gameTime)
        {
            //If we've triggered the fall
            if (triggerFall && !hasFallen)
            {
                //Increment the fall counter
                if ((fallCounter += (float)gameTime.ElapsedGameTime.TotalSeconds) > FallCounterMax)
                {
                    //Fall
                    Console.WriteLine("Fall");
                    hasFallen = true;
                    IsIgnoringGravity = false;
                    IsStaticHorizontal = false;
                }
                else
                {
                    Console.WriteLine("Shakeyshakeyshakey");

                    //Shake
                    float xChange = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * shakeSpeed) * shakeScale;
                    Position = new Vector2(Position.X + xChange, Position.Y);
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Collision Specific Events, Triggers the falling process when something lands ontop
        /// </summary>
        /// <param name="col">The Collision Pair Data</param>
        public override void OnCollision(CollisionPair col)
        {
            if (col.ContactNormal.Y == 1 && !triggerFall)
            {
                //If something has landed above us, trigger the fall
                triggerFall = true;
            }
            base.OnCollision(col);
        }
    }
}
