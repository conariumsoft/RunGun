using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RunGun.Client.Rendering;
using RunGun.Core;
using RunGun.Core.Rendering;
using RunGun.Core.World;
using System;
using System.Collections.Generic;
//using System.Windows.Input;
using Microsoft.Xna.Framework.Input;
using Editor.EditorCore;

namespace Editor
{

	public interface IEdTool
	{
		bool Active { get; set; }
		// TODO: keybind(s)
		// TODO:
		bool ActivationCheck();
		void OnActivate();
		void OnDeactivate();
		void Update(float dt);
		void Draw(SpriteBatch sb);
	}

	public class MoveTool : IEdTool
	{
		public bool Active { get; set; }

		public bool ActivationCheck() {
			if (Keyboard.GetState().IsKeyDown(Keys.M))
				return true;
			return false;
		}

		public void OnActivate() {

		}
		public void OnDeactivate() {

		}
		public void Update(float dt) { }
		public void Draw(SpriteBatch sb) { }
	}

	public interface IEdShortcut
	{

	}

	public class ToolManager
	{
		List<IEdTool> Toolset;
		IEdTool ActiveTool;

		public ToolManager() {
			Toolset = new List<IEdTool>();
		}

		public void AddTool(IEdTool tool) {
			Toolset.Add(tool);
		}

		private bool CheckForToolActivation() {
			foreach (IEdTool tool in Toolset) {
				if (tool.Active == false && tool.ActivationCheck() == true) {
					ResetTools();
					tool.Active = true;
					tool.OnActivate();
					ActiveTool = tool;
					return true;
				}
			}
			return false;
		}
		private void ResetTools() {
			foreach (IEdTool tool in Toolset) {
				if (tool.Active == true) {
					tool.Active = false;
					tool.OnDeactivate();
				}
			}
		}

		public void Update(float dt) {
			bool isNewActiveTool = CheckForToolActivation();

			ActiveTool.Update(dt);
		}

		public void Draw(SpriteBatch sb) {
			ActiveTool.Draw(sb);
		}
	}

	public interface IEditor
	{
		void OnLeftDown();
		void OnLeftUp();
		void OnRightDown();
		void OnRightUp();
	}
	public interface IFormsEditor
	{
		void FileSave(string file);
		void FileNew();
		void FileOpen(string file);
		void FileOpenFileLocation();

		void EditCut();
		void EditCopy();
		void EditPaste();
		void EditDelete();
		void EditUndo(int count = 1);
		void EditRedo(int count = 1);
		
		void InsertGeometry();
		void OnMouseWheel(int delta);
		void OnMouseMove(Vector2 vector);
	}
	public interface IViewModelEditor
	{
		void Initialize();
		void LoadContent();
		void Update(float delta);
		void Draw(GraphicsDevice gdev, SpriteBatch spriteBatch);
		void OnActivated();
		void OnDeactivated();
		void OnExiting();
		void OnViewportChange(int width, int height);
	}

	public class MapEditor : IEditor, IFormsEditor, IViewModelEditor
	{
		private static MapEditor inst;
		public static MapEditor Instance {
			get {
				if (inst==null) {
					inst = new MapEditor();
				}
				return inst;
			}
		} 

		//public List

		// TODO: Make mouse class?
		public EditorMouse EditorMouse { get; set; }
		public Camera Camera { get; set; }
		public Map Map { get; set; }
		public List<LevelGeometry> SelectedObjects { get; set; }
		public Vector2 MousePosition { get; set; }
		public Vector2 LastMousePosition { get; set; }

		bool draggingElement;
		Vector2 draggingOffst;
		float cameraZoomLerp = 1;


		private MapEditor() {
			EditorMouse = new EditorMouse(this);
			Camera = new Camera {
				Rotation = 0,
				Zoom = 1,
			};
			SelectedObjects = new List<LevelGeometry>();
			CreateBlankMap();
		}

		private void CreateBlankMap() {
			Map = new Map();
			Map.Metadata.Creator = "joshuu";
			Map.Geometry.Add(new LevelGeometry(new Vector2(0, 2), new Vector2(30, 30), new Color(0, 0, 0)));
		}

		public void EditDelete() {
			foreach (LevelGeometry geom in SelectedObjects) {
				Map.Geometry.Remove(geom);
			}
			SelectedObjects.Clear();
		}
		public void EditRedo(int count = 1) { }
		public void EditUndo(int count = 1) { }
		public void FileNew() {
			CreateBlankMap();
		}
		public void FileOpen(string file) {
			Map = Map.Load(file);
		}
		public void FileSave(string file) {
			Map.Save(file);
		}
		public void FileOpenFileLocation() {

		}

		public void EditCut() {

		}

		public void EditCopy() {

		}

		public void EditPaste() {

		}

		public void InsertGeometry() {
			Map.Geometry.Add(new LevelGeometry(new Vector2(0, 0), new Vector2(64, 16), Color.DarkKhaki));
		}

		public void Initialize() { }
		public void LoadContent() { }
		public void OnActivated() { }
		public void OnDeactivated() { }
		public void OnExiting() { }
		public void OnMouseMove(Vector2 mousePos) {
			MousePosition = mousePos;

		}
		public void OnMouseWheel(int delta) {
			//Camera.Zoom += (delta / 512.0f);
			cameraZoomLerp += (delta / 512.0f);
			// cap camera zoom
			cameraZoomLerp = Math.Min(cameraZoomLerp, 8);
			cameraZoomLerp = Math.Max(cameraZoomLerp, 0.5f);
		}

		public void OnViewportChange(int width, int height) {
			Camera.ViewportWidth = width;
			Camera.ViewportHeight = height;
		}

		private void KeyboardControls() {

			if (System.Windows.Input.Mouse.RightButton == System.Windows.Input.MouseButtonState.Pressed) {
				Camera.Position -= ((MousePosition - LastMousePosition) / Camera.Zoom);
			}
			LastMousePosition = MousePosition;
		}
	

		public void OnLeftDown() {
			Vector2 worldSpaceCursor = Camera.ScreenToWorldCoordinates(MousePosition);
			KeyboardState state = Keyboard.GetState();
			
			foreach (LevelGeometry geom in Map.Geometry) {
				Rectangle rect = new Rectangle(geom.Position.ToPoint(), geom.Size.ToPoint());
				if (rect.Contains(worldSpaceCursor)) {

					if (SelectedObjects.Contains(geom)== false) {
						SelectedObjects.Add(geom);
					}
					draggingElement = true;
					draggingOffst = new Vector2(worldSpaceCursor.X, worldSpaceCursor.Y);
				}
			}
		}

		public void OnLeftUp() {
			//KeyboardState state = Keyboard.GetState();
			//if (state.IsKeyDown(Keys.LeftShift)) return;
			draggingElement = false;
			//SelectedObjects.Clear();
		}

		public void OnRightDown() {
			SelectedObjects.Clear();
			draggingElement = false;
			//throw new NotImplementedException();
		}

		public void OnRightUp() {
			//throw new NotImplementedException();
		}

		private float Util_Lerp(float a, float b, float f) {
			return (a * (1.0f - f)) + (b * f);
		}

		public void Update(float delta) {

			Camera.Zoom = Util_Lerp(Camera.Zoom, cameraZoomLerp, 0.5f);

			var worldSpaceMouse = Camera.ScreenToWorldCoordinates(MousePosition);
			var lastWorldSpaceMouse = Camera.ScreenToWorldCoordinates(LastMousePosition);
			if (draggingElement) {
				foreach(LevelGeometry geom in SelectedObjects) {
					geom.Position += worldSpaceMouse - lastWorldSpaceMouse;
				}
			}

			KeyboardControls();
			EditorMouse.Update(delta);
		}

		Color bgLineColor = new Color(1.0f, 1.0f, 1.0f, 0.25f);
		private void DrawGrid(SpriteBatch sb) {
			int gridsize = 16;
			for (int x = -128; x < 128; x++) {
				ShapeRenderer.Line(sb, bgLineColor, new Vector2(x * gridsize, -4096), new Vector2(x * gridsize, 4096), 1/Camera.Zoom);
			}

			for (int y = -128; y < 128; y++) {
				ShapeRenderer.Line(sb, bgLineColor, new Vector2(-4096, y * gridsize), new Vector2(4096, y * gridsize), 1 / Camera.Zoom);
			}

		}

		public void Draw(GraphicsDevice gdev, SpriteBatch spriteBatch) {
			spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Camera.WorldSpaceMatrix);
			DrawGrid(spriteBatch);
			Map.Draw(spriteBatch);

			foreach (LevelGeometry geom in SelectedObjects) {
				ShapeRenderer.OutlineRect(spriteBatch, new Color(0.0f, 0.0f, 1.0f), geom.Position, geom.Size);
			}

			foreach (LevelGeometry geom in Map.Geometry) {
				Rectangle rect = new Rectangle(geom.Position.ToPoint(), geom.Size.ToPoint());
				if (rect.Contains(Camera.ScreenToWorldCoordinates(MousePosition))) {
					ShapeRenderer.OutlineRect(spriteBatch, new Color(0.5f, 0.5f, 1.0f), geom.Position, geom.Size);
				}
			}
			
			ShapeRenderer.Rect(spriteBatch, Color.Red, Camera.Position, new Vector2(3, 3));

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred);
			var value = Camera.ScreenToWorldCoordinates(MousePosition);
			string debugText = String.Format("mouz: {0} {1} {2} {3} {4} {5}",
				MousePosition.X, MousePosition.Y, value.X, value.Y, Camera.ViewportWidth, Camera.ViewportHeight
			);
			TextRenderer.Print(spriteBatch, debugText, Vector2.Zero, Color.White);
			spriteBatch.End();
		}

		
	}
}
