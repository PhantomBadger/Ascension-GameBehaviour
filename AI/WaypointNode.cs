using System;
using System.Collections.Generic;
using System.Text;
using MonoGame.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using General;

namespace AI
{
    class WaypointNode
    {
        public Vector2 Position { get; set; }
        public List<WaypointNode> ConnectedNodes { get; set; }
        public Platform ConnectedPlatform { get; set; }
        public float G { get; private set; }
        public float H { get; private set; }
        public WaypointNode ParentNode { get; set; }
        
        /// <summary>
        /// Default Constructor
        /// </summary>
        public WaypointNode()
        {
            G = 0;
            H = 0;
            Position = new Vector2();
            ConnectedPlatform = null;
            ConnectedNodes = new List<WaypointNode>();
        }

        /// <summary>
        /// Constructor with Position
        /// </summary>
        /// <param name="newPos">Position of Waypoint</param>
        public WaypointNode(Vector2 newPos)
        {
            Position = newPos;
            ConnectedPlatform = null;
            ConnectedNodes = new List<WaypointNode>();
        }

        public void CalculateH(Vector2 goalPos)
        {
            Vector2 dxy = goalPos - Position;
            float distance = dxy.Length();
            float h = distance;

            //We want high frictions for more mobility
            h -= (ConnectedPlatform.Friction.DynamicCoefficient * 250);
          
            //For now we ignore bounciness to simplify AI
            //And low bounciness for more stability
            h += (ConnectedPlatform.Bounciness * 100);

            H = h;
        }

        public void CalculateG()
        {
            //Heuristic Total from start to here
            G = ParentNode.G + ParentNode.H;
        }
    }
}
