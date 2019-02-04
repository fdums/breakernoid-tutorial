using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BreakernoidsGL
{
    public class Block: GameObject
    {

          public Block(Game myGame) : base(myGame)
            {
                textureName = "block_red";
            }
        }
}
