using Microsoft.Xna.Framework.Graphics;

namespace RunGun.Core.Game
{
	public interface IGameController
	{
		GameWorld World { get;}

		void SpawnEntity(IEntity e);
		void RemoveEntity(short id);
	}

	public interface IUpdateableRG
	{
		void Update(float delta);
	}

	public interface IDrawableRG
	{
		void Draw(SpriteBatch sb);
	}
}
