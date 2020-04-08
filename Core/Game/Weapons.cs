using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Core.Game.Guns
{

	enum AccuracyStat
	{
		VERY_LOW,
		LOW,
		AVERAGE,
		HIGH,
		VERY_HIGH,
	}


	public interface IFirearm
	{
		void Draw(SpriteBatch sb);

	}

	public class BaseGun
	{
		// weapon characteristics
		public float damage; // self-explainatory
		public float penetrationPower; // % of damage retaned through 1 unit of wall (materials will differ in density)
		public float headshotMult; // headshot = damage * headshotMult
		public float rateOfFire; // seconds between each bullet.
		public float initialSpread; // # of degrees first shot can vary by.
		public float spreadGain; // # of degrees added to subsequent shots' spread
		public float maxSpread; // max # of degrees spread can be.
		public float recoil; // # of degrees to bounce up by per shot
		public float recoilRecovery; // time in seconds (should be very low?)

		// spread = clamp(initialSpread + (spreadGain * bulletsFired), maxSpread)
		// aimDirection += random(-spread, +spread)

		public BaseGun() {

		}

		public void PullTrigger() { }

		void Fire() { }

		struct menuStatistics
		{
			string damage;
			string ammoType;
			AccuracyStat accuracy;
			AccuracyStat penetration;
		}
	}


	class Rifle : BaseGun
	{

		public Rifle() : base() {
			damage = 10;
			penetrationPower = 1;
			headshotMult = 1.2f;
		}
	}
}
