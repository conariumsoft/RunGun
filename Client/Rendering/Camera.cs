using Microsoft.Xna.Framework;

namespace RunGun.Client.Rendering
{
	public class Camera
	{
		public Vector2 Position { get; set; }
		public float Rotation { get; set; }
		public float Zoom { get; set; }
		public int ViewportWidth { get; set; }
		public int ViewportHeight { get; set; }
		public Vector2 ViewportOffset { get; set; }

		public Camera() { }

		// grab center of screen
		public Vector2 ViewportCenter {
			get {
				return new Vector2(ViewportWidth * 0.5f, ViewportHeight * 0.5f);
			}
		}

		// scaled and zoomed in for UI drawing
		public Matrix ScreenSpaceMatrix {
			get {
				return Matrix.CreateTranslation(ViewportOffset.X, ViewportOffset.Y, 0) *
					Matrix.CreateScale(Zoom, Zoom, 0);
			}
		}

		// scaled and translated to camera origin, for game drawing
		public Matrix WorldSpaceMatrix {
			get {
				return Matrix.CreateTranslation(-(int)Position.X, -(int)Position.Y, 0) *
				//Matrix.CreateTranslation(ViewportOffset.X, ViewportOffset.Y, 0) *
				Matrix.CreateScale(Zoom, Zoom, 0) *
				Matrix.CreateTranslation(new Vector3(ViewportCenter, 0));
			}
		}
	}
}
