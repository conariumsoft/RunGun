using System;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Core.Physics
{
	public static class PhysicsProperties
	{
		public const float FRICTION = 2;
		public const float GRAVITY = 400; // pixels/second
		public const float TERMINAL_VELOCITY = 200; // pixels/second
		public const float PHYSICS_TIMESTEP = 1.0f / 100.0f;
		
	}
}
