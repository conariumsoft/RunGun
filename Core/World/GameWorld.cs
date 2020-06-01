using Microsoft.Xna.Framework;
using RunGun.Core;
using RunGun.Core.Game;
using RunGun.Core.Physics;
using System;
using System.Collections.Generic;
using System.Text;


namespace RunGun.Core
{
	public class GameWorld : IUpdateComponent
	{
		public List<LevelGeometry> levelGeometries;
		public List<IEntity> entities;

		public event Action<float, int> OnPhysicsStep;

		float physicsClock;
		public int physicsFrameIter;

		public GameWorld() {
			physicsClock = 0;
			physicsFrameIter = 0;

			levelGeometries = new List<LevelGeometry>();
			entities = new List<IEntity>();
		}

		public void AddEntity(IEntity e) {
			entities.Add(e);
		}
		public void RemoveEntity(IEntity e) {
			entities.Remove(e);
		}
		public void RemoveEntity(short entityID) {
			foreach(var entity in entities.ToArray()) {
				if (entity.EntityID == entityID) {
					entities.Remove(entity);
				}
			}
		}
		public bool HasEntity(short entityID) {
			foreach (var entity in entities) {
				if (entity.EntityID == entityID) {
					return true;
				}
			}
			return false;
		}
		public IEntity GetEntity(short entityID) {
			foreach (var entity in entities) {
				if (entity.EntityID == entityID) {
					return entity;
				}
			}
			throw new Exception("No entity with ID "+ entityID + " found!");
		}
		public List<IEntity> GetEntities() {
			return entities;
		}
		public void ProcessEntityPhysics(IPhysical entity, float step) {
			
			entity.IsFalling = true;

			if (entity is ICollidable collidableEntity) {
				for (int index = 0; index < levelGeometries.Count; index++) {
					LevelGeometry geom = levelGeometries[index];
					CollisionSolver.SolveEntityAgainstGeometry(collidableEntity, geom);
				}
			}

			entity.Physics(step);

			if (entity is IEntityCollidable ec) {
				for (int index = 0; index < entities.Count; index++) {
					ICollidable otherEntity = entities[index] as ICollidable;
					if (!ec.Equals(otherEntity) && ec.GetType() != otherEntity.GetType())
						CollisionSolver.SolveEntityAgainstEntity(ec, otherEntity);
				}
			} 
		}
		void Physics(float step) {
			physicsFrameIter++;

			OnPhysicsStep?.Invoke(step, physicsFrameIter);
			for (int index = 0; index < entities.Count; index++) {
				IEntity entity = entities[index];

				if (entity is IPhysical physicalEntity)
					ProcessEntityPhysics(physicalEntity, step);
			}
		}
		public void Update(float delta) {
			// NOTE: use numeric for loops inside performance critical code
			// also consider using arrays instead of lists
			// apparently there's quite a performance difference

			physicsClock += delta;

			if (physicsClock >= PhysicsProperties.PHYSICS_TIMESTEP) {
				physicsClock -= PhysicsProperties.PHYSICS_TIMESTEP;
				Physics(PhysicsProperties.PHYSICS_TIMESTEP);
			}

			for (int index = 0; index < entities.Count; index++) {
				IEntity entity = entities[index];
				entity.Update(delta);
			}
		}

		public void ServerUpdate(IGameController gc, float delta) {
			for (int index = 0; index < entities.Count; index++) {
				IEntity entity = entities[index];

				entity.ServerSideUpdate(gc, delta);
				if (entity.Remove == true) {
					gc.RemoveEntity(entity.EntityID);
					entities.RemoveAt(index);
					
				}
			}
		}

		public void ClientUpdate(IGameController gc, float delta) {
			for (int index = 0; index < entities.Count; index++) {
				IEntity entity = entities[index];
				entity.ClientSideUpdate(gc, delta);
			}
		}

		public void Draw() {
			
		}
	}
}
