using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Core.Game
{
	public interface IEntityCollidable
	{
		void OnEntityCollide(Vector2 sep, Vector2 normal, Entity victim);
	}

	public interface IUpdateableRG
	{
		void Update(float delta);
		void ServerUpdate(float delta);
		void ClientUpdate(float delta);
	}

	public interface IDrawableRG
	{
		void Draw();
	}
}
