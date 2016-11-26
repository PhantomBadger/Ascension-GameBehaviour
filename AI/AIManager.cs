using System;
using System.Collections.Generic;
using System.Text;
using General;
using Physics;
using MonoGame.Utilities;
using Microsoft.Xna.Framework;

namespace AI
{
    class AIManager
    {
        //Generation Parameters
        private const int MaxPlatformNumbers = 5;
        private const int MinPlatformNumbers = 1;
        private const int MaxPlatformSpacePercentOfScreen = 20;
        private const int GenerationPosYOffset = 0;
        private const int PlatformYSize = 20;

        //Object Templates that are re-used when generating
        //Friction and Bounciness
        public struct PlatformComponents
        {
            public RigidBody2D.FrictionCoefficients FrictionCoefficients { get; set; }
            public float Bounciness { get; set; }
        }
        private PlatformComponents[] platformPrefabs;
        private Random rand;

        /// <summary>
        /// Populate the platform templates
        /// </summary>
        public void Initialize()
        {
            platformPrefabs = new PlatformComponents[4];
            rand = new Random();

            //Create the Standard Platform Object Template
            platformPrefabs[0] = new PlatformComponents() { FrictionCoefficients = new RigidBody2D.FrictionCoefficients() { StaticCoefficient = 0.9f, DynamicCoefficient = 0.9f },
                                                            Bounciness = 0.1f };

            //Create the Ice Platform Object Template
            platformPrefabs[1] = new PlatformComponents() { FrictionCoefficients = new RigidBody2D.FrictionCoefficients() { StaticCoefficient = 0.4f, DynamicCoefficient = 0.4f },
                                                            Bounciness = 0.0f };

            //Create the Sticky Platform Object Template
            platformPrefabs[2] = new PlatformComponents() { FrictionCoefficients = new RigidBody2D.FrictionCoefficients() { StaticCoefficient = 1.5f, DynamicCoefficient = 1.5f },
                                                            Bounciness = 0.0f };

            //Create the Bouncy Platform Object Template
            platformPrefabs[3] = new PlatformComponents() { FrictionCoefficients = new RigidBody2D.FrictionCoefficients() { StaticCoefficient = 0.9f, DynamicCoefficient = 0.9f },
                                                            Bounciness = 0.8f };
        }
                
        /// <summary>
        /// Generate a set of platforms and return them
        /// </summary>
        /// <returns>Returns the platform instances to allow GameManager storage</returns>
        public Platform[] GeneratePlatforms(Camera camera)
        {
            //If we've not been initialised
            if (platformPrefabs == null)
            {
                //Then initialize so we have our platform prefabs
                Initialize();
            }

            //Get our list of platforms to return
            Platform[] platforms;

            float positionY = camera.Position.Y - GenerationPosYOffset - PlatformYSize;
            float screenWidth = camera.Viewport.X;

            //TODO: Weighted Decide how many platforms to make (depending on previous choice to prevent repetition)
            //      Instead of all random
            int numOfPlatforms = rand.Next(MinPlatformNumbers, MaxPlatformNumbers);
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

                //Calculate the size and positions of each platform
                platforms[i].Size = new Vector2(platformSize, PlatformYSize);
                platforms[i].BoxCollider = new Vector2(platformSize, PlatformYSize);

                //Automatically space out the platforms depending on how many there are
                    //The size of the platform, plus the size needed for a space,
                    //Then add a bit of etxra padding so it ends at the edge of the screen
                    //And if its only 1 platform, we add some extra space at the start so its centered
                platforms[i].Position = new Vector2((i * (platformSize + spaceSize + 
                                                         (spaceSize / (numOfPlatforms > 1 ? numOfPlatforms - 1 : 1)))) + 
                                                    (numOfPlatforms == 1 && i == 0 ? spaceSize / 2 : 0),
                                                    positionY);
            }

            //Return these generated platforms
            Console.WriteLine(platforms.Length);

            return platforms;
        }
    }
}
