using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BreakernoidsGL
{
    public enum PowerUpType
    {
        BallCatch = 0,
        MultiBall,
        PaddleSize
    }

    public class PowerUp : GameObject
    {
        public float speed = 400;
        public bool offScreen = false;
        public PowerUpType type;

        public PowerUp(PowerUpType type, Game myGame) : base(myGame)
        {
            switch (type)
            {
                case PowerUpType.BallCatch:
                    textureName = "powerup_c";
                    this.type = type;
                    break;
                case PowerUpType.MultiBall:
                    textureName = "powerup_b";
                    this.type = type;
                    break;
                case PowerUpType.PaddleSize:
                    textureName = "powerup_p";
                    this.type = type;
                    break;
            }
        }

        public override void Update(float deltaTime)
        {
            position.Y +=  speed * deltaTime;
            if (position.Y > 768)
            {
                offScreen = true;
            }
            base.Update(deltaTime);
        }
    }
}
