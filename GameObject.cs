using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BreakernoidsGL
{
    public class GameObject
    {
        protected string textureName = "";
        protected Texture2D texture;
        protected Game game;
        public Vector2 position = Vector2.Zero;

        public float Width
        {
            get { return texture.Width; }
        }

        public float Height
        {
            get { return texture.Height; }
        }

        public GameObject(Game myGame)
        {
            game = myGame;
        }

        public virtual void LoadContent()
        {
            if (textureName != "")
            {
                texture = game.Content.Load<Texture2D>(textureName);
            }
        }

        public virtual void Update(float deltaTime)
        {

        }

        public virtual void Draw(SpriteBatch batch)
        {
            if (texture != null)
            {
                Vector2 drawPosition = position;
                drawPosition.X -= texture.Width / 2;
                drawPosition.Y -= texture.Height / 2;
                batch.Draw(texture, drawPosition, Color.White);
         
            }
        }
    }
}
