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
using System.IO;
using System.Xml.Serialization;

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

        double powerUpProb = 0.9;

        public bool ballCatchActive = false;

        Level level;
        int speedMult;

        int score = 0;
        int addLifeCounter = 20000;

        SpriteFont font;

        bool levelBreak;
        float breakTime;

        int levelCount = 1;

        int lifeCount = 3;

        bool gameOver;

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
            /*for (int i = 0; i < blockLayout.GetLength(0); i++)
             {
                 for (int j = 0; j < blockLayout.GetLength(1); j++)
                 {
                     BlockColor color = (BlockColor) blockLayout[i, j];
                     Block tempBlock = new Block(color, this);
                     tempBlock.LoadContent();
                     tempBlock.position = new Vector2(64 + j * 64, 100 + i * 32);
                     blocks.Add(tempBlock);
                 };
             };*/
            LoadLevel("Level1.xml");

            //soundeffects
            ballBounceSFX = Content.Load<SoundEffect>("ball_bounce");
            ballHitSFX = Content.Load<SoundEffect>("ball_hit");
            deathSFX = Content.Load<SoundEffect>("death");
            powerUpSFX = Content.Load<SoundEffect>("powerup");

            //font
            font = Content.Load<SpriteFont>("main_font");

            StartLevelBreak();
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
           if (!levelBreak && !gameOver)
            {
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

                if (blocks.Count == 0)
                {
                    AddScore("level");
                    NextLevel();
                    levelCount++;
                }
            }
            else if (!gameOver)
            {
                breakTime -= deltaTime;
                if (breakTime <= 0)
                {
                    levelBreak = false;
                    SpawnBall();
                }
            }

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

            spriteBatch.DrawString(font, String.Format("Score: {0:#,###0}", score),
                       new Vector2(40, 50), Color.White);

            string livesText = String.Format("Lives: {0}", lifeCount);
            Vector2 stringSize = font.MeasureString(livesText);
            Vector2 stringLoc = new Vector2(984, 50);
            stringLoc.X -= stringSize.X;
            spriteBatch.DrawString(font, livesText, stringLoc, Color.White);

            if (levelBreak)
            {
                string levelText = String.Format("Level {0}", levelCount);
                 stringSize = font.MeasureString(levelText);
                 stringLoc = new Vector2(1024 / 2, 768 / 2);
                stringLoc.X -= stringSize.X / 2;
                stringLoc.Y -= stringSize.Y / 2;
                spriteBatch.DrawString(font, levelText, stringLoc, Color.White);
            }

            if(gameOver)
            {
                string levelText = String.Format("Game Over");
                 stringSize = font.MeasureString(levelText);
                   stringLoc= new Vector2(1024 / 2, 768 / 2);
                  stringLoc.X -= stringSize.X / 2;
                  stringLoc.Y -= stringSize.Y / 2;
                spriteBatch.DrawString(font, levelText, stringLoc, Color.White);
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

                float dotResult = Vector2.Dot(currentBall.direction, Vector2.UnitX);
                if (dotResult > 0.9f)
                {
                    currentBall.direction = new Vector2(0.906f, -0.423f);
                }

                dotResult = Vector2.Dot(currentBall.direction, -Vector2.UnitY);
                if (dotResult > 0.9f)
                {
                    // We need to figure out if we're clockwise or counter-clockwise
                    Vector3 crossResult = Vector3.Cross(new Vector3(currentBall.direction, 0),
                                                        -Vector3.UnitY);
                    if (crossResult.Z < 0)
                    {
                        currentBall.direction = new Vector2(0.423f, -0.906f);
                    }
                    else
                    {
                        currentBall.direction = new Vector2(-0.423f, -0.906f);
                    }
                }


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

                    AddScore("block");

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
            ballCatchActive = false;
            paddle.ChangePaddleSize("paddle");
            if( lifeCount >0)
            {
                SpawnBall();
                lifeCount--;
            }
            else
            {
                gameOver = true;
            }

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

                    AddScore("powerup");
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
            tempBall.speed = level.ballSpeed + 100 * speedMult;
            balls.Add(tempBall);
        }

        protected void RemoveBalls ()
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

        protected void LoadLevel (string levelName)
        {
            using (FileStream fs = File.OpenRead("Levels/" + levelName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Level));
                level = (Level)serializer.Deserialize(fs);
            }


            // TODO: Generate blocks based on level.layout array
            for (int i = 0; i < level.layout.Length; i++)
            {
                for (int j = 0; j < level.layout[i].Length; j++)
                {
                   if (level.layout[i][j] == 9)
                    {

                    }
                    else
                    {
                        BlockColor color = (BlockColor)level.layout[i][j];
                        Block tempBlock = new Block(color, this);
                        tempBlock.LoadContent();
                        tempBlock.position = new Vector2(64 + j * 64, 100 + i * 32);
                        blocks.Add(tempBlock);
                    }
                    
                };
            };
        }

        protected void NextLevel ()
        {
            balls.Clear();
            powerups.Clear();
            paddle.position = initialPaddlePos;

            StartLevelBreak();

            ballCatchActive = false;
            paddle.ChangePaddleSize("paddle");

            if (level.nextLevel == "Level1.xml")
            {
                speedMult++;
            }
            LoadLevel(level.nextLevel);
        }

        protected void AddScore (string eventName)
        {
            int oldScore = score;
            switch (eventName)
            {
                case "block":
                    score += 100 + 100 * speedMult;
                    break;
                case "powerup":
                    score += 500 + 500 * speedMult;
                   break;
                case "level":
                    score += 5000 + 5000 * speedMult + 500 * (balls.Count - 1) * speedMult;
                    break;
            }

            addLifeCounter -= score - oldScore;

            while (addLifeCounter <= 0)
            {
                lifeCount++;
                powerUpSFX.Play();
            };

        }

        protected void StartLevelBreak ()
        {
            levelBreak = true;
            breakTime = 2.0f;
        }

    }
}
