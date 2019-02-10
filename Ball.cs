using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BreakernoidsGL
{

    public class Ball : GameObject
    {
        public float speed = 400;
        public Vector2 direction = new Vector2(0.707f, -0.707f);
        public bool caught = false;
        public int allowCollision = 0;
        public bool shouldRemove = false;

        public Ball(Game myGame) : base(myGame)
        {
            textureName = "ball";
        }


        public override void Update(float deltaTime)
        {
            if (!caught)
            {
                position += direction * speed * deltaTime;
                if(allowCollision > 0)
                {
                    allowCollision--;
                }
            }
            else
            {
                KeyboardState keyState = Keyboard.GetState();
                if (keyState.IsKeyDown(Keys.Space))
                {
                    caught = false;
                }
            }
            base.Update(deltaTime);
        }
    }

}

