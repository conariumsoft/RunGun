using Microsoft.Xna.Framework.Graphics;

namespace RunGun.Core.Game
{
	public interface IGameController
	{
		GameWorld World { get;}

		void SpawnEntity(IEntity e);
		void RemoveEntity(short id);
	}

	public interface IGameSystem
	{
		
	}



	public interface IUpdateComponent
	{
		void Update(float delta);
	}

	public interface IRenderComponent
	{
		void Draw(SpriteBatch sb);
	}
}
