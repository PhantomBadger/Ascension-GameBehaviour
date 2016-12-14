using System;
using System.Collections.Generic;
using System.Text;
using General;
using Physics;
using MonoGame.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AI
{
    class PlatformGenerator
    {
        //Generation Parameters
        public const int PlatformYSize = 20;
        public const float WaypointEdgeBuffer = 20.0f;

        public Enemy EnemyReference { get; set; }
        public AIManager AIReference { get; set; }

        private Platform.PlatformTypes prevDynamicType = Platform.PlatformTypes.Static;

        private const int MaxPlatformNumbers = 5;
        private const int MinPlatformNumbers = 1;
        private const int MaxPlatformSpacePercentOfScreen = 30;
        private const int GenerationPosYOffset = 0;
        private const int MaxConnectionLength = 170;
        private const int DynamicPlatformChance = 5;

        //Object Templates that are re-used when generating
        //Friction and Bounciness
        public struct PlatformComponents
        {
            public RigidBody2D.FrictionCoefficients FrictionCoefficients { get; set; }
            public float Bounciness { get; set; }
            public string TextureLeft { get; set; }
            public string TextureMid { get; set; }
            public string TextureRight { get; set; }
            public Platform.PlatformTypes PlatformType { get; set; }
        }

        private PlatformComponents[] platformPrefabs;
        private PlatformComponents[] specialPlatformPrefabs;
        private Random rand;
        private int prevNumOfPlatforms = -1;
        private bool prevContainsDynamic = false;

        /// <summary>
        /// Populate the platform templates
        /// </summary>
        public void Initialize()
        {
            //Init the Static Platform Templates
            platformPrefabs = new PlatformComponents[3];
            rand = new Random();

            //Create the Ice Platform Object Template
            platformPrefabs[0] = new PlatformComponents()
            {
                FrictionCoefficients = new RigidBody2D.FrictionCoefficients() { StaticCoefficient = 0.4f, DynamicCoefficient = 0.4f },
                Bounciness = 0.0f,
                TextureLeft = "snowLeft.png",
                TextureMid = "snowMid.png",
                TextureRight = "snowRight.png",
                PlatformType = Platform.PlatformTypes.Static
            };

            //Create the Sticky Platform Object Template
            platformPrefabs[1] = new PlatformComponents()
            {
                FrictionCoefficients = new RigidBody2D.FrictionCoefficients() { StaticCoefficient = 1.5f, DynamicCoefficient = 1.5f },
                Bounciness = 0.0f,
                TextureLeft = "sandLeft.png",
                TextureMid = "sandMid.png",
                TextureRight = "sandRight.png",
                PlatformType = Platform.PlatformTypes.Static
            };

            //Create the Bouncy Platform Object Template
            platformPrefabs[2] = new PlatformComponents()
            {
                FrictionCoefficients = new RigidBody2D.FrictionCoefficients() { StaticCoefficient = 0.9f, DynamicCoefficient = 0.9f },
                Bounciness = 0.8f,
                TextureLeft = "planetLeft.png",
                TextureMid = "planetMid.png",
                TextureRight = "planetRight.png",
                PlatformType = Platform.PlatformTypes.Static
            };

            //Init the Dynamic Platform Templates
            specialPlatformPrefabs = new PlatformComponents[3];

            //Create the Spring Constrained Platform Object Template
            specialPlatformPrefabs[0] = new PlatformComponents()
            {
                FrictionCoefficients = new RigidBody2D.FrictionCoefficients() { StaticCoefficient = 0.9f, DynamicCoefficient = 0.9f },
                Bounciness = 0.1f,
                TextureLeft = "grassLeft.png",
                TextureMid = "grassMid.png",
                TextureRight = "grassRight.png",
                PlatformType = Platform.PlatformTypes.DynamicSpring
            };

            //Create the Falling Platform Object Template
            specialPlatformPrefabs[1] = new PlatformComponents()
            {
                FrictionCoefficients = new RigidBody2D.FrictionCoefficients() { StaticCoefficient = 0.9f, DynamicCoefficient = 0.9f },
                Bounciness = 0.1f,
                TextureLeft = "stoneLeft.png",
                TextureMid = "stoneMid.png",
                TextureRight = "stoneRight.png",
                PlatformType = Platform.PlatformTypes.DynamicFalling
            };

            //Create the Moving Platform Object Template
            specialPlatformPrefabs[2] = new PlatformComponents()
            {
                FrictionCoefficients = new RigidBody2D.FrictionCoefficients() { StaticCoefficient = 0.9f, DynamicCoefficient = 0.9f },
                Bounciness = 0.1f,
                TextureLeft = "stoneLeft.png",
                TextureMid = "stoneMid.png",
                TextureRight = "stoneRight.png",
                PlatformType = Platform.PlatformTypes.DynamicMoving
            };
        }

        /// <summary>
        /// Generates a series of platforms with varying frictions and bounciness
        /// </summary>
        /// <param name="pos">The X/Y Position to create the platforms at (top left)</param>
        /// <param name="xWidth">The X width the platforms must be constrained in</param>
        /// <returns>Array of generated platforms</returns>
        public Platform[] GeneratePlatforms(Vector2 pos, float xWidth)
        {
            //Get our list of platforms to return
            List<Platform> platforms;

            float positionY = pos.Y;
            float screenWidth = xWidth;

            //Pick a number of platforms, as long as it wasnt one we just had
            int numOfPlatforms;
            while ((numOfPlatforms = rand.Next(MinPlatformNumbers, MaxPlatformNumbers)) == prevNumOfPlatforms || prevNumOfPlatforms == 3 && numOfPlatforms == 2) { }
            prevNumOfPlatforms = numOfPlatforms;
            platforms = new List<Platform>();

            //Console.WriteLine(numOfPlatforms);

            //Will this row have a dynamic platform in?
            bool hasDynamicPlatform = rand.Next(1, 11) < DynamicPlatformChance;

            int dynamicPlatformType;
            while (specialPlatformPrefabs[dynamicPlatformType = rand.Next(0, specialPlatformPrefabs.Length)].PlatformType == prevDynamicType);

            //Decide which platform index it will replace, in the case of a moving platform, it replaces 2
            int dynamicPlatformIndex = specialPlatformPrefabs[dynamicPlatformType].PlatformType == Platform.PlatformTypes.DynamicMoving ?
                rand.Next(0, numOfPlatforms - 1) : rand.Next(0, numOfPlatforms);

            //Get some size calculations
            //Total free space we'll have on the row
            float spaceSize = ((screenWidth / 100) * MaxPlatformSpacePercentOfScreen) / numOfPlatforms;
            float platformSize = (screenWidth - (spaceSize * numOfPlatforms)) / numOfPlatforms;

            //Generate all the normal platforms
            for (int i = 0; i < numOfPlatforms; i++)
            {
                platforms.Add(new Platform());

                //Randomly decide what kind of platform to use
                int platformType = rand.Next(0, platformPrefabs.Length);

                platforms[i].IsStaticHorizontal = true;
                platforms[i].IsStaticVertical = true;
                platforms[i].IsIgnoringGravity = true;
                platforms[i].Friction = platformPrefabs[platformType].FrictionCoefficients;
                platforms[i].Bounciness = platformPrefabs[platformType].Bounciness;

                //Set the Texture
                platforms[i].TextureLeftFile = platformPrefabs[platformType].TextureLeft;
                platforms[i].TextureMidFile = platformPrefabs[platformType].TextureMid;
                platforms[i].TextureRightFile = platformPrefabs[platformType].TextureRight;

                //Calculate the size and positions of each platform
                platforms[i].Size = new Vector2(platformSize, PlatformYSize);
                platforms[i].BoxCollider = new Vector2(platformSize, PlatformYSize);

                //Automatically space out the platforms depending on how many there are
                //The size of the platform, plus the size needed for a space,
                //Then add a bit of etxra padding so it ends at the edge of the screen
                //And if its only 1 platform, we add some extra space at the start so its centered
                Vector2 newPos = new Vector2(pos.X + ((i * (platformSize + spaceSize +
                                                         (spaceSize / (numOfPlatforms > 1 ? numOfPlatforms - 1 : 1)))) +
                                                    (numOfPlatforms == 1 ? spaceSize / 2 : 0)),
                                                    positionY);
                platforms[i].Position = newPos;

                //Set the Scale
                platforms[i].Scale = new Vector2(0.2f, 0.2f);
            }

            //Replace the platform at the given index with our dynamic one
            if (hasDynamicPlatform && !prevContainsDynamic)
            {
                //If so, decide ahead of time which dynamic platform we'll include
                prevContainsDynamic = true;
                prevDynamicType = specialPlatformPrefabs[dynamicPlatformType].PlatformType;

                //Set Type Specific Values
                switch (specialPlatformPrefabs[dynamicPlatformType].PlatformType)
                {
                    case Platform.PlatformTypes.DynamicSpring:
                        {
                            //Spring
                            SpringPlatform springPlat = new SpringPlatform();

                            springPlat.IsStaticHorizontal = false;
                            springPlat.IsStaticVertical = false;
                            springPlat.IsIgnoringGravity = true;

                            //Set Textures
                            springPlat.TextureLeftFile = specialPlatformPrefabs[dynamicPlatformType].TextureLeft;
                            springPlat.TextureMidFile = specialPlatformPrefabs[dynamicPlatformType].TextureMid;
                            springPlat.TextureRightFile = specialPlatformPrefabs[dynamicPlatformType].TextureRight;

                            //Set Properties
                            springPlat.Friction = specialPlatformPrefabs[dynamicPlatformType].FrictionCoefficients;
                            springPlat.Bounciness = specialPlatformPrefabs[dynamicPlatformType].Bounciness;

                            //Set Type
                            springPlat.PlatformType = specialPlatformPrefabs[dynamicPlatformType].PlatformType;

                            //Set Positions and whatnot
                            springPlat.Position = platforms[dynamicPlatformIndex].Position;
                            springPlat.Rotation = platforms[dynamicPlatformIndex].Rotation;
                            springPlat.Scale = platforms[dynamicPlatformIndex].Scale;

                            //Set Scale
                            springPlat.Size = platforms[dynamicPlatformIndex].Size;
                            springPlat.BoxCollider = platforms[dynamicPlatformIndex].BoxCollider;

                            platforms[dynamicPlatformIndex] = (Platform)springPlat;
                            break;
                        }
                    case Platform.PlatformTypes.DynamicFalling:
                        {
                            //If it's a falling platform make sure it isnt in any middle platforms
                            //Or this could destroy the only path up to the next level
                            if (numOfPlatforms % 2 == 1 && (dynamicPlatformIndex != 0 && dynamicPlatformIndex != numOfPlatforms))
                            {
                                dynamicPlatformIndex--;
                            }

                            //If this is the only platform, dont change it or we could make the level impossible
                            if (numOfPlatforms == 1)
                            {
                                break;
                            }

                            //Falling
                            FallingPlatform fallingPlat = new FallingPlatform();

                            fallingPlat.IsStaticHorizontal = true;
                            fallingPlat.IsStaticVertical = true;
                            fallingPlat.IsIgnoringGravity = true;

                            //Set Textures
                            fallingPlat.TextureLeftFile = specialPlatformPrefabs[dynamicPlatformType].TextureLeft;
                            fallingPlat.TextureMidFile = specialPlatformPrefabs[dynamicPlatformType].TextureMid;
                            fallingPlat.TextureRightFile = specialPlatformPrefabs[dynamicPlatformType].TextureRight;

                            //Set Properties
                            fallingPlat.Friction = specialPlatformPrefabs[dynamicPlatformType].FrictionCoefficients;
                            fallingPlat.Bounciness = specialPlatformPrefabs[dynamicPlatformType].Bounciness;

                            //Set Type
                            fallingPlat.PlatformType = specialPlatformPrefabs[dynamicPlatformType].PlatformType;

                            //Set Positions and whatnot
                            fallingPlat.Position = platforms[dynamicPlatformIndex].Position;
                            fallingPlat.Rotation = platforms[dynamicPlatformIndex].Rotation;
                            fallingPlat.Scale = platforms[dynamicPlatformIndex].Scale;

                            //Set Scale
                            fallingPlat.Size = platforms[dynamicPlatformIndex].Size;
                            fallingPlat.BoxCollider = platforms[dynamicPlatformIndex].BoxCollider;

                            fallingPlat.EnemyReference = EnemyReference;
                            fallingPlat.AIReference = AIReference;

                            platforms[dynamicPlatformIndex] = (Platform)fallingPlat;
                            break;
                        }
                    case Platform.PlatformTypes.DynamicMoving:
                        {
                            //If this is the only platform, dont change it as it overcomplicates waypoints
                            if (numOfPlatforms <= 2)
                            {
                                break;
                            }

                            //Moving
                            MovingPlatform movPlat = new MovingPlatform();

                            movPlat.IsStaticHorizontal = true;
                            movPlat.IsStaticVertical = true;
                            movPlat.IsIgnoringGravity = true;

                            //Set Textures
                            movPlat.TextureLeftFile = specialPlatformPrefabs[dynamicPlatformType].TextureLeft;
                            movPlat.TextureMidFile = specialPlatformPrefabs[dynamicPlatformType].TextureMid;
                            movPlat.TextureRightFile = specialPlatformPrefabs[dynamicPlatformType].TextureRight;

                            //Set Properties
                            movPlat.Friction = specialPlatformPrefabs[dynamicPlatformType].FrictionCoefficients;
                            movPlat.Bounciness = specialPlatformPrefabs[dynamicPlatformType].Bounciness;

                            //Set Type
                            movPlat.PlatformType = specialPlatformPrefabs[dynamicPlatformType].PlatformType;

                            //Set Positions and whatnot
                            movPlat.Position = platforms[dynamicPlatformIndex].Position;
                            movPlat.Rotation = platforms[dynamicPlatformIndex].Rotation;
                            movPlat.Scale = platforms[dynamicPlatformIndex].Scale;

                            //Set Scale
                            movPlat.Size = platforms[dynamicPlatformIndex].Size;
                            movPlat.BoxCollider = platforms[dynamicPlatformIndex].BoxCollider;

                            //Left and Right Move Limits
                            movPlat.LeftPos = platforms[dynamicPlatformIndex].Position;
                            movPlat.RightPos = platforms[dynamicPlatformIndex + 1].Position;
                            platforms.RemoveAt(dynamicPlatformIndex + 1);

                            movPlat.EnemyReference = EnemyReference;

                            platforms[dynamicPlatformIndex] = (Platform)movPlat;
                            break;
                        }
                }
            }
            else if (prevContainsDynamic)
            {
                prevContainsDynamic = false;
            }

            //Return these generated platforms
            return platforms.ToArray();
        }

        /// <summary>
        /// Generate the Waypoints for the given platform row and connect them to the previous one at the given Y offset
        /// </summary>
        /// <param name="CurrentPlatformRow">An Array of the Current Plaform Row to generate Waypoints for</param>
        /// <param name="PreviousPlatformRow">An Array of the Previous Platform Row to connect the generated Waypoints to</param>
        /// <param name="waypointYOffset">The Y Offset to place the waypoints at</param>
        /// <returns>An array of WaypointNodes</returns>
        public WaypointNode[] GenerateWaypoints(Platform[] CurrentPlatformRow, Platform[] PreviousPlatformRow, float waypointYOffset)
        {
            //TODO:
            // Add safety incase either current row or previous row is null
            //Console.WriteLine("Waypoint");

            List<WaypointNode> waypointNodes = new List<WaypointNode>();

            float previousRowYDistance = PreviousPlatformRow[0].Position.Y - CurrentPlatformRow[0].Position.Y;

            //Add waypoints for the current platform row
            WaypointNode previousEdgeNode = new WaypointNode();
            for (int i = 0; i < CurrentPlatformRow.Length; i++)
            {
                //Generate a node at either side of each platform
                WaypointNode leftNode = new WaypointNode();
                leftNode.Position = new Vector2(CurrentPlatformRow[i].Position.X + WaypointEdgeBuffer,
                                                CurrentPlatformRow[i].Position.Y - waypointYOffset);
                leftNode.ConnectedPlatform = CurrentPlatformRow[i];

                WaypointNode rightNode = new WaypointNode();
                rightNode.Position = new Vector2(CurrentPlatformRow[i].Position.X + CurrentPlatformRow[i].Size.X - WaypointEdgeBuffer,
                                                 CurrentPlatformRow[i].Position.Y - waypointYOffset);
                rightNode.ConnectedPlatform = CurrentPlatformRow[i];

                //Connect them
                leftNode.ConnectedNodes.Add(rightNode);
                rightNode.ConnectedNodes.Add(leftNode);

                //Special case for a single platform on this row (Connect to the row below)
                if (CurrentPlatformRow.Length == 1)
                {
                    //Find closest waypoint on previous row and connect to it
                    WaypointNode closestToRightNode, closestToLeftNode;

                    //Find closest to Previous Edge Node
                    List<WaypointNode> sortedList = new List<WaypointNode>();
                    for (int j = 0; j < PreviousPlatformRow.Length; j++)
                    {
                        sortedList.AddRange(PreviousPlatformRow[j].ConnectedWaypoints);
                    }
                    //Sort by distance to right edge on X (to the right only)
                    sortedList.RemoveAll((n1) => n1.Position.X <= rightNode.Position.X);
                    if (sortedList.Count > 0)
                    {
                        sortedList.Sort((n1, n2) => (rightNode.Position.X - n1.Position.X).CompareTo(rightNode.Position.X - n2.Position.X));
                        closestToRightNode = sortedList[0];

                        if (Math.Abs((rightNode.Position - closestToRightNode.Position).Length()) < MaxConnectionLength)
                        {
                            rightNode.ConnectedNodes.Add(closestToRightNode);
                            closestToRightNode.ConnectedNodes.Add(rightNode);
                        }
                    }

                    //Find closest to left edge node
                    sortedList.Clear();
                    for (int j = 0; j < PreviousPlatformRow.Length; j++)
                    {
                        sortedList.AddRange(PreviousPlatformRow[j].ConnectedWaypoints);
                    }
                    //Sort by distance to previous edge on X (to the right only)
                    sortedList.RemoveAll((n1) => n1.Position.X >= leftNode.Position.X);
                    if (sortedList.Count > 0)
                    {
                        sortedList.Sort((n1, n2) => (n1.Position.X - leftNode.Position.X).CompareTo(n2.Position.X - leftNode.Position.X));
                        closestToLeftNode = sortedList[0];

                        if (Math.Abs((leftNode.Position - closestToLeftNode.Position).Length()) < MaxConnectionLength)
                        {
                            closestToLeftNode.ConnectedNodes.Add(leftNode);
                            leftNode.ConnectedNodes.Add(closestToLeftNode);
                        }
                    }
                }

                //Connect the platforms on each row together & to the row below it
                if (i > 0)
                {
                    previousEdgeNode.ConnectedNodes.Add(leftNode);
                    leftNode.ConnectedNodes.Add(previousEdgeNode);

                    //Add a waypoint in the middle of the two platforms on the row below to allow better jumping
                    Vector2 midPoint = (leftNode.Position + previousEdgeNode.Position) / 2;
                    midPoint = new Vector2(midPoint.X, midPoint.Y + previousRowYDistance);

                    //If it can find a platform at this point, put a waypoint there, if it can't then we asume it'll be able to
                    //connect via other means
                    try
                    {
                        Platform connectedPlatform = new List<Platform>(PreviousPlatformRow).Find((p) => midPoint.X >= p.Position.X && midPoint.X <= (p.Position.X + p.Size.X));

                        WaypointNode midNode = new WaypointNode();
                        midNode.Position = midPoint;
                        midNode.ConnectedPlatform = connectedPlatform;

                        //Connect to all nearby nodes
                        midNode.ConnectedNodes.AddRange(connectedPlatform.ConnectedWaypoints);
                        for (int j = 0; j < connectedPlatform.ConnectedWaypoints.Count; j++)
                        {
                            connectedPlatform.ConnectedWaypoints[j].ConnectedNodes.Add(midNode);
                        }
                        midNode.ConnectedNodes.AddRange(new WaypointNode[] { previousEdgeNode, leftNode });
                        previousEdgeNode.ConnectedNodes.Add(midNode);
                        leftNode.ConnectedNodes.Add(midNode);
                        connectedPlatform.ConnectedWaypoints.Add(midNode);

                        //Add to our list
                        waypointNodes.Add(midNode);
                    }
                    catch
                    {
                        //Only do this if it's the first or last platform gap
                        //There's some weird behaviour if it tries to do it with the middle gaps
                        //When encountering certain platform compositions
                        //This is a bit of a hacky workaround

                        //With 4 platforms on the current row and 2 on the previous, the waypoint lines overlap
                        //the platforms, and not amount of intersection checks I am running can seem to detect this
                        //and remove it
                        //I suspect either due to the Monogame axis being inversed, or the scale of the drawing making
                        //it appear to overlap when it doesnt
                        //So in order to avoid this:
                        /*
                                    *                 *
                            ____________            ____________
                            |__________|            |__________|

                                 *                        *
                        ___________                     ____________
                        ___________|                    |___________

                        It tries to connect the waypoints to the opposite top to allow you to jump up, however the waypoint line
                        intersects with the platform, or it's so close it would ruin the AI but appears to intersect
                        due to scale
                        */
                        //We do a hacky thing where it only makes paths up on the edges of the arena
                        if (i == 1 || i == CurrentPlatformRow.Length - 1)
                        {
                            //Find closest waypoint on previous row and connect to it
                            WaypointNode closestToPrevEdge, closestToLeftNode;

                            //Find closest to Previous Edge Node
                            List<WaypointNode> sortedList = new List<WaypointNode>();
                            for (int j = 0; j < PreviousPlatformRow.Length; j++)
                            {
                                sortedList.AddRange(PreviousPlatformRow[j].ConnectedWaypoints);
                            }
                            //Sort by distance to previous edge on X (to the right only)
                            sortedList.RemoveAll((n1) => n1.Position.X <= previousEdgeNode.ConnectedPlatform.Position.X + previousEdgeNode.ConnectedPlatform.BoxCollider.X);
                            if (sortedList.Count > 0)
                            {
                                sortedList.Sort((n1, n2) => (n1.Position.X - previousEdgeNode.Position.X).CompareTo(n2.Position.X - previousEdgeNode.Position.X));
                                closestToPrevEdge = sortedList[0];

                                previousEdgeNode.ConnectedNodes.Add(closestToPrevEdge);
                                closestToPrevEdge.ConnectedNodes.Add(previousEdgeNode);
                            }

                            //Find closest to left edge node
                            sortedList.Clear();
                            for (int j = 0; j < PreviousPlatformRow.Length; j++)
                            {
                                sortedList.AddRange(PreviousPlatformRow[j].ConnectedWaypoints);
                            }
                            //Sort by distance to previous edge on X (to the right only)
                            sortedList.RemoveAll((n1) => n1.Position.X >= leftNode.ConnectedPlatform.Position.X);
                            if (sortedList.Count > 0)
                            {
                                sortedList.Sort((n1, n2) => (leftNode.Position.X - n1.Position.X).CompareTo(leftNode.Position.X - n2.Position.X));
                                closestToLeftNode = sortedList[0];

                                //Connect to the visible waypoints on the area below
                                closestToLeftNode.ConnectedNodes.Add(leftNode);
                                leftNode.ConnectedNodes.Add(closestToLeftNode);
                            }
                        }
                    }
                }

                previousEdgeNode = rightNode;

                //Add to the waypoint nodes list & platform object
                waypointNodes.AddRange(new WaypointNode[] { leftNode, rightNode });
                CurrentPlatformRow[i].ConnectedWaypoints.AddRange(new WaypointNode[] { leftNode, rightNode });
            }

            return waypointNodes.ToArray();
        }
    }
}
