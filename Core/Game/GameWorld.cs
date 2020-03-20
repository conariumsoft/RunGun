using Microsoft.Xna.Framework;
using RunGun.Core;
using RunGun.Core.Game;
using RunGun.Core.Physics;
using System;
using System.Collections.Generic;
using System.Text;


namespace RunGun.Core
{
	class GameWorld
	{
		public List<LevelGeometry> levelGeometries;
		public List<Entity> entities;

		public event Action<float, int> OnPhysicsStep;

		float physicsClock;
		public int physicsFrameIter;

		public GameWorld() {
			physicsClock = 0;
			physicsFrameIter = 0;

			levelGeometries = new List<LevelGeometry>();
			entities = new List<Entity>();
		}

		public void AddEntity(Entity e) {
			entities.Add(e);
		}
		public void RemoveEntity(Entity e) {
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
		public Entity GetEntity(short entityID) {
			foreach (var entity in entities) {
				if (entity.EntityID == entityID) {
					return entity;
				}
			}
			throw new Exception("No entity with ID "+ entityID + " found!");
		}
		public void ProcessEntityPhysics(Entity e, float step) {
			
			e.IsFalling = true;
			foreach (var geom in levelGeometries) {
				CollisionSolver.SolveEntityAgainstGeometry(e, geom);
			}
			e.Physics(step);

			if (e is IEntityCollidable ec) {
				foreach (var otherEnt in entities) {

				}
			} 
		}
		void Physics(float step) {
			physicsFrameIter++;

			OnPhysicsStep?.Invoke(step, physicsFrameIter);
			foreach (var entity in entities) {
				ProcessEntityPhysics(entity, step);
			}
		}
		public void Update(double delta) {

			foreach (var entity in entities) {
				entity.Update(delta);
			}

			physicsClock += (float)delta;

			while (physicsClock >= PhysicsProperties.PHYSICS_TIMESTEP) {
				physicsClock -= PhysicsProperties.PHYSICS_TIMESTEP;
				Physics(PhysicsProperties.PHYSICS_TIMESTEP);
			}
		}
	}
}
