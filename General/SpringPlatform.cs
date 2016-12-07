using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Physics;

namespace General
{
    class SpringPlatform : Platform
    {

        public Vector2 ConstrainedPosition { get; set; }     
        public float Stiffness { get; set; }
        public float Dampen { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public SpringPlatform()
        {
            Stiffness = 100.0f;
            Dampen = 0.2f;
        }

        /// <summary>
        /// Overridden Initialize to Init ConstrainedPosition
        /// </summary>
        public override void Initialize()
        {
            ConstrainedPosition = Position;
            base.Initialize();
        }

        /// <summary>
        /// Overridden Update Method to handle spring constraints
        /// </summary>
        /// <param name="gameTime">Current Game Time</param>
        public override void Update(GameTime gameTime)
        {
            //Use Hooke's Law to calculate force
            Vector2 dxy = ConstrainedPosition - Position;
            float length = Math.Abs(dxy.Length());

            //Hooke's Law
            //F = -k(l - l0)
            //F = Spring Force
            //l = stretched length
            //k = stiffness
            //l0 = rest length
            float springForce;
            springForce = ((-Stiffness) * (length)) * Dampen;

            if (dxy != new Vector2(0))
            {
               // dxy.Normalize();
            }

            Force = (dxy  * -1) * springForce;

            base.Update(gameTime);
        }
    }
}
