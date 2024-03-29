﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Physics;
using AI;

namespace General
{
    public class Platform : RigidBody2D
    {
        public enum PlatformTypes { Static, DynamicMoving, DynamicFalling, DynamicSpring }

        public Vector2 Size { get; set; }
        public string TextureLeftFile { get; set; } = "stoneLeft.png";
        public string TextureMidFile { get; set; } = "stoneMid.png";
        public string TextureRightFile { get; set; } = "stoneRight.png";
        public List<WaypointNode> ConnectedWaypoints;
        public PlatformTypes PlatformType;

        private Texture2D textureLeft;
        private Texture2D textureMid;
        private Texture2D textureRight;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Platform()
        {
            PlatformType = PlatformTypes.Static;
            ConnectedWaypoints = new List<WaypointNode>();
            Size = new Vector2();
            Scale = new Vector2(1, 1);
            Friction = new FrictionCoefficients() { StaticCoefficient = 0.0f, DynamicCoefficient = 0.0f };
            Tag = "Ground";
        }

        /// <summary>
        /// Load in all the object textures
        /// </summary>
        /// <param name="content">Content Manager</param>
        public override void LoadContent(ContentManager content)
        {
            textureLeft = content.Load<Texture2D>(TextureLeftFile);
            textureMid = content.Load<Texture2D>(TextureMidFile);
            textureRight = content.Load<Texture2D>(TextureRightFile);

            //Adjust size to accurate display all platform tiles at least
            SizeSnap();
            base.LoadContent(content);
        }

        /// <summary>
        /// Unload all the object textures
        /// </summary>
        public override void UnloadContent()
        {
            textureLeft.Dispose();
            textureMid.Dispose();
            textureRight.Dispose();
            base.UnloadContent();
        }

        /// <summary>
        /// Draw the platform, using the left and right textures, then a tiling middle texture
        /// </summary>
        /// <param name="gameTime">Current Game Time</param>
        /// <param name="spriteBatch">Sprite Batch</param>
        /// <param name="graphicsDevice">Graphics Device</param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (textureLeft == null || textureMid == null || textureRight == null)
            {
                textureLeft = new Texture2D(graphicsDevice, 1, 1);
                textureLeft.SetData(new Color[] { Color.DarkGray });
                spriteBatch.Draw(textureLeft, new Rectangle(Position.ToPoint(), Size.ToPoint()), Color.DarkGray);
            }
            else
            {
                //If sprites are loaded, Use them to draw the platform
                float midCount = (Size.X - (textureLeft.Width * Scale.X) - (textureRight.Width * Scale.X)) / (textureMid.Width * Scale.X);
                float posOffset = 0;
                //Assume all tiles are the same size
                float texSize = textureMid.Width * Scale.X;

                spriteBatch.Draw(textureLeft, new Vector2(Position.X + posOffset, Position.Y), null, Color.White, Rotation.Z, Vector2.Zero, Scale, SpriteEffects.None, 0.1f);
                posOffset += texSize;
                for (int i = 0; i < midCount; i++, posOffset += texSize)
                {
                    spriteBatch.Draw(textureMid, new Vector2(Position.X + posOffset, Position.Y), null, Color.White, Rotation.Z, Vector2.Zero, Scale, SpriteEffects.None, 0.1f);
                }
                spriteBatch.Draw(textureRight, new Vector2(Position.X + posOffset, Position.Y), null, Color.White, Rotation.Z, Vector2.Zero, Scale, SpriteEffects.None, 0.1f);
            }

            base.Draw(gameTime, spriteBatch, graphicsDevice);
        }

        /// <summary>
        /// Snap the Platform's Size so that it fits a whole number of tile textures
        /// </summary>
        private void SizeSnap()
        {
            //Automatically snap to nearest size to allow all the tiles to fit comfortably
            float width = Size.X * Scale.X;
            float minPlatSize = (textureLeft.Width + textureRight.Width) * Scale.X;
            float midSize = textureMid.Width * Scale.X;
            float height = textureMid.Height * Scale.Y;

            //Snap X
            if (Size.X < minPlatSize)
            {
                Size = new Vector2(minPlatSize, height);
            }
            else if ((Size.X - minPlatSize) % midSize != 0)
            {
                float sizeMod = (Size.X - minPlatSize) % midSize;
                Size = sizeMod <= midSize / 2 ? new Vector2(Size.X - sizeMod, height) : new Vector2(Size.X + (midSize - sizeMod), height);
            }
            BoxCollider = Size;
        }
    }
}
