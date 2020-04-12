using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Core
{
	public interface IGameSystem
	{
		public void Update(float delta);
		public void Draw(SpriteBatch sb, GraphicsDevice gd);
	}
}
