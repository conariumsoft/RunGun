using Microsoft.Xna.Framework;

namespace RunGun.Client.Rendering
{
	public class FrameCounter
	{
		static int framecounting = 1000;

		double framerate;
		double[] framesamples;
		int frames;
		double averageFramerate;

		public FrameCounter() {
			frames = 0;
			framerate = 0;
			averageFramerate = 0.1;
			framesamples = new double[framecounting];
		}

		public void Update(GameTime gameTime) {
			double dt = gameTime.ElapsedGameTime.TotalSeconds;

			// frame counter
			frames++;

			if (frames >= framecounting) {
				frames = 0;
			}

			framerate = 1 / dt;

			averageFramerate -= framesamples[frames];
			framesamples[frames] = dt;
			averageFramerate += framesamples[frames];
		}

		public double GetExactFramerate() {
			return framerate;
		}

		public double GetAverageFramerate() {
			return framecounting / averageFramerate;
		}
	}
}
