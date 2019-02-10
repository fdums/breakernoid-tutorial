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
        Vector2 initialPaddlePos;

        SoundEffect ballBounceSFX;
        SoundEffect ballHitSFX;
        SoundEffect deathSFX;
        SoundEffect powerUpSFX;

        Random random = new Random();

        List<Block> blocks = new List<Block>();
        List<PowerUp> powerups = new List<PowerUp>();
        List<Ball> balls = new List<Ball>();

        double powerUpProb = 0.2;
        int[,] blockLayout = new int[,]{
           {5,5,5,5,5,5,5,5,5,5,5,5,5,5,5},
           {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
           {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
           {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
           {3,3,3,3,3,3,3,3,3,3,3,3,3,3,3},
           {4,4,4,4,4,4,4,4,4,4,4,4,4,4,4},
        };

        public bool ballCatchActive = false;

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
            powerUpSFX = Content.Load<SoundEffect>("powerup");
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
            float paddleXBefore = paddle.position.X;
            paddle.Update(deltaTime);
            float paddelXDiff = paddleXBefore - paddle.position.X;

            foreach (Ball b in balls)
            {
                if (b.caught)
                {
                    b.position.X -= paddelXDiff;
                }
                b.Update(deltaTime);
                CheckCollisions(b);
            }
            RemoveBalls();

            foreach (PowerUp p in powerups)
            {
                p.Update(deltaTime);
            }
     
            CheckForPowerups();
            RemovePowerUp();
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
            foreach (Ball ba in balls)
            {
                ba.Draw(spriteBatch);
            }

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

        protected void CheckCollisions(Ball currentBall)
        {
            float radius = currentBall.Width / 2;

            //collision with paddle
            if (currentBall.allowCollision == 0 &
                (currentBall.position.X > (paddle.position.X - radius - paddle.Width / 2)) &&
                (currentBall.position.X < (paddle.position.X + radius + paddle.Width / 2)) &&
                (currentBall.position.Y < paddle.position.Y) &&
                (currentBall.position.Y > (paddle.position.Y - radius - paddle.Height / 2)))
            {
                //collision with paddle
                float thirdOfPaddle = (paddle.Width + radius *2) / 3;
                float leftCornerPaddle = paddle.position.X - paddle.Width / 2 - radius;

                Vector2 reflectionVector;


                if (currentBall.position.X < (leftCornerPaddle + thirdOfPaddle))
                {
                    reflectionVector =  new Vector2(-0.196f, -0.981f);

                } else if (currentBall.position.X < (leftCornerPaddle + 2 * thirdOfPaddle))
                {
                    reflectionVector = new Vector2(0, -1);

                } else
                {
                    reflectionVector = new Vector2(0.196f, -0.981f);

                }

                if (ballCatchActive)
                {
                    currentBall.caught = true;

                    if (currentBall.position.X < paddle.Width/2)
                    {
                        currentBall.direction = new Vector2(-0.707f, -0.707f);
                    }
                    else
                    {
                        currentBall.direction = new Vector2(0.707f, -0.707f);
                    }

                }
                currentBall.direction = Vector2.Reflect(currentBall.direction, reflectionVector);
                currentBall.allowCollision = 20;
                ballBounceSFX.Play();
            }

            //collision with block
            Block collidedBlock = null;
            foreach (Block b in blocks)
            {
                if (currentBall.position.X > (b.position.X - radius - b.Width / 2) &&
                 (currentBall.position.X < (b.position.X + radius + b.Width / 2)) &&
                 (currentBall.position.Y < b.position.Y + radius + b.Height / 2) &&
                 (currentBall.position.Y > (b.position.Y - radius - b.Height / 2)))
                {
                    collidedBlock = b;
                    break;
                }
            }

            if (collidedBlock != null)
            {
                if ((currentBall.position.Y < (collidedBlock.position.Y - collidedBlock.Height / 2)) ||
                    (currentBall.position.Y > (collidedBlock.position.Y + collidedBlock.Height / 2)))
                {
                    currentBall.direction.Y = -currentBall.direction.Y;
                }
                else
                {
                    currentBall.direction.X = -currentBall.direction.X;
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
            if ( Math.Abs(currentBall.position.X - 32) < radius)
            {
                // left wall collision
                currentBall.direction.X = -currentBall.direction.X;
                ballBounceSFX.Play();
            } 
            else if ( Math.Abs(currentBall.position.X - 992) < radius)
            {
                //right wall collision
                currentBall.direction.X = -currentBall.direction.X;
                ballBounceSFX.Play();
            }
            else if (Math.Abs(currentBall.position.Y - 32) < radius)
            {
                // top wall collision
                currentBall.direction.Y = -currentBall.direction.Y;
                ballBounceSFX.Play();
            }
            else if (currentBall.position.Y > (768 + radius))
            {
                currentBall.shouldRemove = true;
                deathSFX.Play();
            }


        }

        protected void LoseLife()
        {
            paddle.position = initialPaddlePos;
            SpawnBall();
            ballCatchActive = false;
            paddle.ChangePaddleSize("paddle");

        }

        protected void SpawnPowerUp (Vector2 position)
        {
            PowerUpType type = (PowerUpType) random.Next(3);
            PowerUp tempPowerUp = new PowerUp(type, this);
            tempPowerUp.position = position;
            tempPowerUp.LoadContent();
            powerups.Add(tempPowerUp);

        }

        protected void RemovePowerUp ()
        {
            for (int i = powerups.Count - 1; i >= 0; i--)
            {
                if (powerups[i].shouldRemove)
                {
                    powerups.RemoveAt(i);
                }
            }
        }

        protected void CheckForPowerups ()
        {
            foreach (PowerUp p in powerups)
            {
                if (paddle.BoundingRect.Intersects(p.BoundingRect))
                {
                    ActivatePowerUp(p);
                }
            }

        }

        protected void ActivatePowerUp (PowerUp powerUp)
        {
            powerUp.shouldRemove = true;
            powerUpSFX.Play();

            switch (powerUp.type)
            {
                case PowerUpType.BallCatch:
                    ballCatchActive = true;
                    break;
                case PowerUpType.MultiBall:
                    paddle.ChangePaddleSize("paddle_long");
                    break;
                case PowerUpType.PaddleSize:
                    SpawnBall();
                    break;
            }

        }

        protected void SpawnBall ()
        {
            Ball tempBall = new Ball(this);
            tempBall.LoadContent();
            tempBall.position = paddle.position;
            tempBall.position.Y -= tempBall.Height + paddle.Height;

            balls.Add(tempBall);
        }

        protected void RemoveBalls()
        {
            for (int i = balls.Count - 1; i >= 0; i--)
            {
                if (balls[i].shouldRemove)
                {
                    balls.RemoveAt(i);
                }
            }

            if (balls.Count == 0)
            {
                LoseLife();
            }
        }

    }
}
