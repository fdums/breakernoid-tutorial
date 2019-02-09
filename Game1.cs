using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace BreakernoidsGL
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        Texture2D bgTexture;
        Paddle paddle;
        Ball ball;
        Vector2 initialPaddlePos;
        Vector2 initialBallPos;
        Vector2 initialBallDirection;
        SoundEffect ballBounceSFX;
        SoundEffect ballHitSFX;
        SoundEffect deathSFX;
        Random random = new Random();

       List<Block> blocks = new List<Block>();
       List<PowerUp> powerups = new List<PowerUp>();

        double powerUpProb = 0.2;
        int allowCollision = 0;
        int[,] blockLayout = new int[,]{
           {5,5,5,5,5,5,5,5,5,5,5,5,5,5,5},
           {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
           {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
           {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
           {3,3,3,3,3,3,3,3,3,3,3,3,3,3,3},
           {4,4,4,4,4,4,4,4,4,4,4,4,4,4,4},
        };


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //set resolution
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            bgTexture = Content.Load<Texture2D>("bg");

            //initalizing Paddle
            paddle = new Paddle(this);
            paddle.LoadContent();
            initialPaddlePos = new Vector2(512, 740);
            paddle.position = initialPaddlePos;

            //initializing Ball
            ball = new Ball(this);
            ball.LoadContent();
            initialBallPos.X = initialPaddlePos.X;
            initialBallPos.Y = (paddle.position.Y - ball.Height - paddle.Height);
            initialBallDirection = ball.direction;
            ball.position = initialBallPos;

            //initializing Blocks
            for (int i = 0; i < blockLayout.GetLength(0); i++)
            {
                for (int j = 0; j < blockLayout.GetLength(1); j++)
                {
                    BlockColor color = (BlockColor) blockLayout[i, j];
                    Block tempBlock = new Block(color, this);
                    tempBlock.LoadContent();
                    tempBlock.position = new Vector2(64 + j * 64, 100 + i * 32);
                    blocks.Add(tempBlock);


                };
            };

            //soundeffects
            ballBounceSFX = Content.Load<SoundEffect>("ball_bounce");
            ballHitSFX = Content.Load<SoundEffect>("ball_hit");
            deathSFX = Content.Load<SoundEffect>("death");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            paddle.Update(deltaTime);
            ball.Update(deltaTime);
            foreach (PowerUp p in powerups)
            {
                p.Update(deltaTime);
            }
            CheckCollisions();
            removePowerUp();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Blue);


            // TODO: Add your drawing code here
            spriteBatch.Begin();
            // Draw all sprites here
            spriteBatch.Draw(bgTexture, new Vector2(0, 0), Color.White);
            paddle.Draw(spriteBatch);
            ball.Draw(spriteBatch);
            foreach (Block b in blocks)
            {
                b.Draw(spriteBatch);
            }
            foreach (PowerUp p in powerups)
            {
                p.Draw(spriteBatch);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        protected void CheckCollisions()
        {
            float radius = ball.Width / 2;

            //collision with paddle
            if ( allowCollision == 0 &
                (ball.position.X > (paddle.position.X - radius - paddle.Width / 2)) &&
                (ball.position.X < (paddle.position.X + radius + paddle.Width / 2)) &&
                (ball.position.Y < paddle.position.Y) &&
                (ball.position.Y > (paddle.position.Y - radius - paddle.Height / 2)))
            {
                //collision with paddle
                float thirdOfPaddle = (paddle.Width + radius *2) / 3;
                float leftCornerPaddle = paddle.position.X - paddle.Width / 2 - radius;

                Vector2 reflectionVector;


                if (ball.position.X < (leftCornerPaddle + thirdOfPaddle))
                {
                    reflectionVector =  new Vector2(-0.196f, -0.981f);

                } else if (ball.position.X < (leftCornerPaddle + 2 * thirdOfPaddle))
                {
                    reflectionVector = new Vector2(0, -1);

                } else
                {
                    reflectionVector = new Vector2(0.196f, -0.981f);

                }

                ball.direction = Vector2.Reflect(ball.direction, reflectionVector);
                allowCollision = 20;
                ballBounceSFX.Play();
            }
            else if(allowCollision > 0)
            {
                allowCollision--;
            }

            //collision with block
            Block collidedBlock = null;
            foreach (Block b in blocks)
            {
                if (ball.position.X > (b.position.X - radius - b.Width / 2) &&
                 (ball.position.X < (b.position.X + radius + b.Width / 2)) &&
                 (ball.position.Y < b.position.Y + radius + b.Height / 2) &&
                 (ball.position.Y > (b.position.Y - radius - b.Height / 2)))
                {
                    collidedBlock = b;
                    break;
                }
            }

            if (collidedBlock != null)
            {
                if ((ball.position.Y < (collidedBlock.position.Y - collidedBlock.Height / 2)) ||
                    (ball.position.Y > (collidedBlock.position.Y + collidedBlock.Height / 2)))
                {
                    ball.direction.Y = -ball.direction.Y;
                }
                else
                {
                    ball.direction.X = -ball.direction.X;
                }

                ballHitSFX.Play();

                if (collidedBlock.OnHit(collidedBlock))
                {
                    blocks.Remove(collidedBlock);

                    if (random.NextDouble() < powerUpProb)
                    {
                        SpawnPowerUp(collidedBlock.position);
                    }
                }

            }

            //collision with walls
            if ( Math.Abs(ball.position.X - 32) < radius)
            {
                // left wall collision
                ball.direction.X = -ball.direction.X;
                ballBounceSFX.Play();
            } 
            else if ( Math.Abs(ball.position.X - 992) < radius)
            {
                //right wall collision
                ball.direction.X = -ball.direction.X;
                ballBounceSFX.Play();
            }
            else if (Math.Abs(ball.position.Y - 32) < radius)
            {
                // top wall collision
                ball.direction.Y = -ball.direction.Y;
                ballBounceSFX.Play();
            }
            else if (ball.position.Y > (768 + radius))
            {
                LoseLife();
                deathSFX.Play();
            }


        }

        protected void LoseLife()
        {
            paddle.position = initialPaddlePos;
            ball.position = initialBallPos;
            ball.direction = initialBallDirection;
        }

        protected void SpawnPowerUp (Vector2 position)
        {
            PowerUpType type = (PowerUpType) random.Next(3);
            PowerUp tempPowerUp = new PowerUp(type, this);
            tempPowerUp.position = position;
            tempPowerUp.LoadContent();
            powerups.Add(tempPowerUp);

        }

        protected void removePowerUp ()
        {
            for (int i = powerups.Count - 1; i >= 0; i--)
            {
                if (powerups[i].offScreen)
                {
                    powerups.RemoveAt(i);
                }
            }
        }

    }
}
