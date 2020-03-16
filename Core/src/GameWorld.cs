using Microsoft.Xna.Framework;
using RunGun.Core;
using RunGun.Core.Physics;
using System;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Core
{
	struct EntityGameState
	{
		public Vector2 position;
		public Vector2 velocity;
		public Vector2 nextPosition;
		public int step;
	}

	class GameWorld
	{
		Dictionary<Entity, Dictionary<int, EntityGameState>> bart;

		public EntityGameState? GetState(Entity e, int step) {
			var dict = bart[e];

			foreach (KeyValuePair<int, EntityGameState> kvp in dict) {
				if (kvp.Key == step) {
					return kvp.Value;
				}
			}
			return null;
		}

		public void SetState(Entity e, int step, EntityGameState newState) {
			bart[e][step] = newState;
		}

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
			bart = new Dictionary<Entity, Dictionary<int, EntityGameState>>();
		}

		public void AddEntity(Entity e) {
			entities.Add(e);
			bart.Add(e, new Dictionary<int, EntityGameState>());
		}
		public void RemoveEntity(Entity e) {
			entities.Remove(e);
		}
		public void RemoveEntity(int entityID) {
			foreach(var entity in entities.ToArray()) {
				if (entity.EntityID == entityID) {
					entities.Remove(entity);
				}
			}
		}
		public bool HasEntity(int entityID) {
			foreach (var entity in entities) {
				if (entity.EntityID == entityID) {
					return true;
				}
			}
			return false;
		}
		public Entity GetEntity(int networkID) {
			foreach (var entity in entities) {
				if (entity.EntityID == networkID) {
					return entity;
				}
			}
			throw new Exception("No entity with ID "+networkID+" found!");
		}

		public void ProcessEntityPhysics(PhysicalEntity e, float step, int physframe) {
			e.Physics(step);

			e.isFalling = true;
			foreach (var geom in levelGeometries) {
				CollisionSolver.SolveEntityAgainstGeometry(e, geom);
			}

			bart[e].Add(physicsFrameIter, new EntityGameState {
				position = e.position,
				velocity = e.velocity,
				nextPosition = e.nextPosition,
				step = physframe
			});
		}

		void Physics(float step) {
			physicsFrameIter++;

			foreach(var entity in entities) {
				if (entity is PhysicalEntity pe) {
					ProcessEntityPhysics(pe, step, physicsFrameIter);
				}
			}
			OnPhysicsStep?.Invoke(step, physicsFrameIter);
		}

		public void Update(double delta) {

			physicsClock += (float)delta;

			while (physicsClock > PhysicsProperties.PHYSICS_TIMESTEP) {
				physicsClock -= PhysicsProperties.PHYSICS_TIMESTEP;
				Physics(PhysicsProperties.PHYSICS_TIMESTEP);
			}

			foreach (var entity in entities) {
				entity.Update(delta);
			}
		}
	}
}
