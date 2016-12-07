using System;
using System.Collections.Generic;
using System.Text;
using General;
using Physics;
using MonoGame.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Linq;

namespace AI
{
    class AIManager
    {
        public List<WaypointNode> WaypointNetwork { get; set; }

        private Texture2D waypointTex;
        private SpriteFont debugFont;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public AIManager()
        {
            WaypointNetwork = new List<WaypointNode>();
        }

        /// <summary>
        /// Load Default Sprite Fonts
        /// </summary>
        /// <param name="content">Content Manager</param>
        public void LoadContent(ContentManager content)
        {
            //Load Debug Font
            debugFont = content.Load<SpriteFont>("DebugFont");
        }

        /// <summary>
        /// Draw the waypoints and connections, used for Debugging
        /// </summary>
        /// <param name="gameTime">Current Game Time</param>
        /// <param name="spriteBatch">Sprite Batch</param>
        /// <param name="graphicsDevice">Graphics Device</param>
        public void DebugDraw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            //Create waypoint texture
            if (waypointTex == null)
            {
                waypointTex = new Texture2D(graphicsDevice, 1, 1);
                waypointTex.SetData(new Color[] { Color.SkyBlue });
            }

            if (GameManager.DebugMode)
            {
                for (int i = 0; i < WaypointNetwork.Count; i++)
                {
                    //Draw Waypoints
                    spriteBatch.Draw(waypointTex, new Rectangle((int)WaypointNetwork[i].Position.X - 3, (int)WaypointNetwork[i].Position.Y - 3, 6, 6), Color.White);
                    spriteBatch.DrawString(debugFont,
                        $"[{WaypointNetwork[i].G}, {WaypointNetwork[i].H}]",
                        new Vector2(WaypointNetwork[i].Position.X, WaypointNetwork[i].Position.Y - 15),
                        Color.Blue,
                        0,
                        Vector2.Zero,
                        0.75f,
                        SpriteEffects.None,
                        0);
                        
                    //Draw Waypoint connectors
                    for (int j = 0; j < WaypointNetwork[i].ConnectedNodes.Count; j++)
                    {
                        Vector2 waypointLine = WaypointNetwork[i].ConnectedNodes[j].Position - WaypointNetwork[i].Position;
                        float lineAngle = (float)Math.Atan2(waypointLine.Y, waypointLine.X);

                        spriteBatch.Draw(waypointTex,
                            new Rectangle((int)WaypointNetwork[i].Position.X, (int)WaypointNetwork[i].Position.Y, (int)waypointLine.Length(), 1),
                            null,
                            Color.White,
                            lineAngle,
                            new Vector2(0, 0),
                            SpriteEffects.None,
                            0.1f);
                    }
                }
            }
        }

        /// <summary>
        /// Selects the highest node to be the new Goal node from the Waypoint network
        /// </summary>
        public WaypointNode GetNewGoalNode()
        {
            //Find the highest waypoint(s) and select one
            if (WaypointNetwork != null && WaypointNetwork.Count > 0)
            {
                WaypointNetwork.Sort((n1, n2) => n1.Position.Y.CompareTo(n2.Position.Y));
                Console.WriteLine($"New Goal Waypoint at ({WaypointNetwork[0].Position.X}, {WaypointNetwork[0].Position.Y})");
                return WaypointNetwork[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Uses A* to compute the shortest path from the start node to the goal node
        /// </summary>
        /// <returns>An ordered list of the path taken to reach the goal node</returns>
        public Queue<WaypointNode> AStarSearch(WaypointNode startNode, WaypointNode endNode)
        {
            List<WaypointNode> openList = new List<WaypointNode>();
            List<WaypointNode> closedList = new List<WaypointNode>();

            //Make sure we have a goal
            if (endNode == null)
            {
                //Error - No Goal
                return null;
            }

            //Add the start node to the open list
            openList.Add(startNode);
            WaypointNode currentNode = startNode;
            while (openList.Count > 0)
            {
                //Sort based on lowest heuristic
                openList.Sort((n1, n2) => (n1.H + n1.G).CompareTo((n2.H + n2.G)));

                //Get the next node
                currentNode = openList[0];
                closedList.Add(currentNode);
                openList.RemoveAt(0);

                //Check if we're at the goal node
                if (currentNode == endNode)
                {
                    //Break out of search and trace back
                    break;
                }

                //For each connected node
                for (int i = 0; i < currentNode.ConnectedNodes.Count; i++)
                {
                    //If it's on the closed list
                    if (closedList.Contains(currentNode.ConnectedNodes[i]))
                    {
                        //Ignore it
                        continue;
                    }
                    //If it's already in the open list, check to see if it's a better path
                    //and reassign the parent node
                    //If not, add it and assign parent node
                    else if (openList.Contains(currentNode.ConnectedNodes[i]))
                    {
                        float curF = currentNode.ConnectedNodes[i].ParentNode.G + currentNode.ConnectedNodes[i].ParentNode.H;
                        //If our F is less than it's current F, then we swap the parents over
                        if (currentNode.G + currentNode.H < curF)
                        {
                            int index = openList.FindIndex((n1) => n1 == currentNode.ConnectedNodes[i]);
                            openList[index].ParentNode = currentNode;
                            openList[index].CalculateG();
                        }
                        //Otherwise we ignore it
                    }
                    else
                    {
                        //Calculate Heuristics and add to open list
                        currentNode.ConnectedNodes[i].ParentNode = currentNode;
                        currentNode.ConnectedNodes[i].CalculateG();
                        currentNode.ConnectedNodes[i].CalculateH(endNode.Position);

                        openList.Add(currentNode.ConnectedNodes[i]);
                    }
                }       
            }

            //If we found the end node
            if (endNode.ParentNode != null)
            {
                Queue<WaypointNode> path = new Queue<WaypointNode>();
                //Trace back through the end node to the start node
                WaypointNode pathNode = endNode;
                while (pathNode != startNode)
                {
                    path.Enqueue(pathNode);
                    pathNode = pathNode.ParentNode;
                }
                //Reverse path & return
                path = new Queue<WaypointNode>(path.Reverse());
                return path;
            }
            else
            {
                //Couldnt find a path
                return null;
            }
        }
    }
}
