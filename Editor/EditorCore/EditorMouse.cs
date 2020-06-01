using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// mouse utility class for editor
namespace Editor.EditorCore
{
	public class EditorMouse
	{
		public bool IsLeftDown { get; private set; }
		public bool IsRightDown { get; private set; }

		private bool leftDownDebounce;
		private bool rightDownDebounce;

		IEditor ParentEditor;

		public EditorMouse(IEditor parentEd) {
			ParentEditor = parentEd;
		}

		public void Update(float dt) {
			// left mouse butn
			if (System.Windows.Input.Mouse.LeftButton == System.Windows.Input.MouseButtonState.Pressed) {
				IsLeftDown = true;
				if (leftDownDebounce == false) {
					leftDownDebounce = true;
					ParentEditor.OnLeftDown();
				}
			} else {
				IsLeftDown = false;
				if (leftDownDebounce == true) {
					leftDownDebounce = false;
					ParentEditor.OnLeftUp();
				}
			}
			//right mouse butn
			if (System.Windows.Input.Mouse.RightButton == System.Windows.Input.MouseButtonState.Pressed) {
				IsRightDown = true;
				if (rightDownDebounce == false) {
					rightDownDebounce = true;
					ParentEditor.OnRightDown();
				}
			} else {
				IsRightDown = false;
				if (rightDownDebounce == true) {
					rightDownDebounce = false;
					ParentEditor.OnRightUp();
				}
			}
		}
	}
}
