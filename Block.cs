using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BreakernoidsGL
{
    public enum BlockColor
    {
        Red = 0,
        Yellow,
        Blue,
        Green,
        Purple,
        GreyHi,
        Grey
    }

    public class Block: GameObject
    {
        BlockColor color;

          public Block(BlockColor color, Game myGame) : base(myGame)
            {
            this.color = color;
            switch (color)
            {
                case BlockColor.Red:
                    textureName = "block_red";
                    break;
                case BlockColor.Yellow:
                    textureName = "block_yellow";
                    break;
                case BlockColor.Blue:
                    textureName = "block_blue";
                    break;
                case BlockColor.Green:
                    textureName = "block_green";
                    break;
                case BlockColor.Purple:
                    textureName = "block_purple";
                    break;
                case BlockColor.GreyHi:
                    textureName = "block_grey_hi";
                    break;
                case BlockColor.Grey:
                    textureName = "block_grey";
                    break;
            };

            }

        public bool OnHit (Block block)
        {
            if (block.color == BlockColor.GreyHi)
            {
                block.color = BlockColor.Grey;
                block.textureName = "block_grey";
                block.LoadContent();
                return false;
            }
            return true;
        }
    }
}
