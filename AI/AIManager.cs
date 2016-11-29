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
    class AIManager
    {
        //Generation Parameters
        private const int MaxPlatformNumbers = 5;
        private const int MinPlatformNumbers = 1;
        private const int MaxPlatformSpacePercentOfScreen = 30;
        private const int GenerationPosYOffset = 0;
        public const int PlatformYSize = 20;

        //Object Templates that are re-used when generating
        //Friction and Bounciness
        public struct PlatformComponents
        {
            public RigidBody2D.FrictionCoefficients FrictionCoefficients { get; set; }
            public float Bounciness { get; set; }
            public string TextureLeft { get; set; }
            public string TextureMid { get; set; }
            public string TextureRight { get; set; }
        }
        private PlatformComponents[] platformPrefabs;
        private Random rand;
        private int prevNumOfPlatforms = -1;

        /// <summary>
        /// Populate the platform templates
        /// </summary>
        public void Initialize()
        {
            platformPrefabs = new PlatformComponents[4];
            rand = new Random();

            //Create the Standard Platform Object Template
            platformPrefabs[0] = new PlatformComponents() { FrictionCoefficients = new RigidBody2D.FrictionCoefficients() { StaticCoefficient = 0.9f, DynamicCoefficient = 0.9f },
                                                            Bounciness = 0.1f,
                                                            TextureLeft = "grassLeft.png",
                                                            TextureMid = "grassMid.png",
                                                            TextureRight = "grassRight.png" };

            //Create the Ice Platform Object Template
            platformPrefabs[1] = new PlatformComponents() { FrictionCoefficients = new RigidBody2D.FrictionCoefficients() { StaticCoefficient = 0.4f, DynamicCoefficient = 0.4f },
                                                            Bounciness = 0.0f,
                                                            TextureLeft = "snowLeft.png",
                                                            TextureMid = "snowMid.png",
                                                            TextureRight = "snowRight.png" };

            //Create the Sticky Platform Object Template
            platformPrefabs[2] = new PlatformComponents() { FrictionCoefficients = new RigidBody2D.FrictionCoefficients() { StaticCoefficient = 1.5f, DynamicCoefficient = 1.5f },
                                                            Bounciness = 0.0f,
                                                            TextureLeft = "sandLeft.png",
                                                            TextureMid = "sandMid.png",
                                                            TextureRight = "sandRight.png" };

            //Create the Bouncy Platform Object Template
            platformPrefabs[3] = new PlatformComponents() { FrictionCoefficients = new RigidBody2D.FrictionCoefficients() { StaticCoefficient = 0.9f, DynamicCoefficient = 0.9f },
                                                            Bounciness = 0.8f,
                                                            TextureLeft = "planetLeft.png",
                                                            TextureMid = "planetMid.png",
                                                            TextureRight = "planetRight.png"};
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
            Platform[] platforms;

            float positionY = pos.Y;
            float screenWidth = xWidth;

            //Pick a number of platforms, as long as it wasnt one we just had
            int numOfPlatforms;
            while ((numOfPlatforms = rand.Next(MinPlatformNumbers, MaxPlatformNumbers)) == prevNumOfPlatforms) { }
            prevNumOfPlatforms = numOfPlatforms;
            platforms = new Platform[numOfPlatforms];

            //Get some size calculations
            //Total free space we'll have on the row
            float spaceSize = ((screenWidth / 100) * MaxPlatformSpacePercentOfScreen) / numOfPlatforms;
            float platformSize = (screenWidth - (spaceSize * numOfPlatforms)) / numOfPlatforms;

            for (int i = 0; i < numOfPlatforms; i++)
            {
                //Randomly decide what kind of platform to use
                int platformType = rand.Next(0, platformPrefabs.Length);

                platforms[i] = new Platform();
                platforms[i].IsStatic = true;
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
                platforms[i].Position = new Vector2(pos.X + ((i * (platformSize + spaceSize + 
                                                         (spaceSize / (numOfPlatforms > 1 ? numOfPlatforms - 1 : 1)))) + 
                                                    (numOfPlatforms == 1 ? spaceSize / 2 : 0)),
                                                    positionY);

                //Set the Scale
                platforms[i].Scale = new Vector2(0.2f, 0.2f);
            }

            //Return these generated platforms
            return platforms;
        }
    }
}
