using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using AI;

namespace General
{
    class MovingPlatform : Platform
    {
        public float MoveVelocity { get; set; }
        public Vector2 LeftPos { get; set; }
        public Vector2 RightPos { get; set; }
        public Enemy EnemyReference { get; set; }

        bool isMovingRight = true;
        Vector2 prevPos;

        const float MinDistance = 0.5f;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public MovingPlatform()
        {
            MoveVelocity = 50.0f;
            LeftPos = new Vector2(0);
            RightPos = new Vector2(0);
        }

        /// <summary>
        /// Overridden Initialise
        /// </summary>
        public override void Initialize()
        {
            prevPos = Position;
            base.Initialize();
        }

        /// <summary>
        /// Overridden Update Method to Move the Platform
        /// </summary>
        /// <param name="gameTime">Current Game Time</param>
        public override void Update(GameTime gameTime)
        {
            //Init Vel
            if (Velocity == new Vector2(0))
            {
                Velocity = new Vector2(isMovingRight ? MoveVelocity : -MoveVelocity, 0.0f);
            }

            //Direction Change
            Vector2 dist = isMovingRight ? RightPos - Position : LeftPos - Position;
            if (Math.Abs(dist.X) < MinDistance && Math.Abs(dist.Y) < MinDistance)
            {
                isMovingRight = !isMovingRight;
                Velocity = new Vector2(isMovingRight ? MoveVelocity : -MoveVelocity, 0.0f);

                if (EnemyReference != null && EnemyReference.OnGround)
                {
                    EnemyReference.InvalidatePath();
                }
            }

            //Update Waypoints
            Vector2 posDxy = Position - prevPos;
            for (int i = 0; i < ConnectedWaypoints.Count; i++)
            {
                ConnectedWaypoints[i].Position += posDxy;
            }
            prevPos = Position;

            base.Update(gameTime);
        }
    }
}
