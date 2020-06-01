using Microsoft.Xna.Framework;
using MonoGameControls;
using System;
using System.Windows;
using System.Windows.Input;

namespace Editor
{
    public static class WindowsDialogs
    {
        public static string? RunOpenFileSelectDialog() {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Map"; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "XML documents (.xml)|*.xml"; // Filter files by extension

            // Show open file dialog box
            bool? result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true) {
                // Open document
                string filename = dlg.FileName;
                return filename;
            }
            return null;
        }
        public static string? RunSaveFileSelectDialog() {
            // Configure save file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Map"; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "XML documents (.xml)|*.xml"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true) {
                // Save document
                string filename = dlg.FileName;
                return filename;
            }
            return null;
        }
    }

    public partial class MainWindow : Window {
        public IFormsEditor Editor { get; set; }

        public MainWindow() {
            Editor = MapEditor.Instance;
            InitializeComponent();
            MouseWheel += MainWindow_MouseWheel;
            MouseMove += MainWindow_MouseMove;
            
        }

        #region Actions
        private void File_Save_Action() {
            string filename = WindowsDialogs.RunSaveFileSelectDialog();
            if (filename == null) return;
            Editor.FileSave(filename);
        }
        private void File_New_Action() {
            Editor.FileNew();
        }
        private void File_Open_Action() {
            string filename = WindowsDialogs.RunOpenFileSelectDialog();
            if (filename == null) return;
            Editor.FileOpen(filename);
        }
        private void Edit_Undo_Action() { }
        private void Edit_Redo_Action() { }
        #endregion

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if (Keyboard.Modifiers == ModifierKeys.Control) {
                if (e.Key == Key.S)
                    File_Save_Action();
                if (e.Key == Key.N)
                    File_New_Action();
                if (e.Key == Key.O)
                    File_Open_Action();
            }
        }

        #region Main Window Events

        private void MainWindow_MouseMove(object sender, MouseEventArgs e) {
            //if (e.Handled == true) return;
            System.Windows.Point p = e.GetPosition(MGCC);
            Editor.OnMouseMove(new Vector2((float)p.X, (float)p.Y));
        }

        private void MainWindow_MouseWheel(object s, MouseWheelEventArgs e) {
            Editor.OnMouseWheel(e.Delta);
        }
        #endregion

        #region Dock Panel Tool Buttons
        private void ToolSelect_Insert(object s, RoutedEventArgs e) {
            Editor.InsertGeometry();
        }
        private void ToolSelect_Move(object s, RoutedEventArgs e) {

        }

        private void ToolSelect_Resize(object s, RoutedEventArgs e) {

        }
        private void ToolSelect_Color(object s, RoutedEventArgs e) {

        }
        #endregion

        #region Dock Standard Menus (File, Edit, Settings, etc)
        private void Dock_File_New(object sender, RoutedEventArgs e) {
            File_New_Action();
        }
        private void Dock_File_Open(object sender, RoutedEventArgs e) {
            File_Open_Action();
        }
        private void Dock_File_Save(object sender, RoutedEventArgs e) {
            File_Save_Action();

        }
        private void Dock_File_OpenLocation(object s, RoutedEventArgs e) {
            Editor.FileOpenFileLocation();
        }

        private void Dock_Insert_Click(object sender, RoutedEventArgs e) {

        }

        
        private void Dock_Edit_Undo(object s, RoutedEventArgs e)   { Editor.EditUndo();   }
        private void Dock_Edit_Redo(object s, RoutedEventArgs e)   { Editor.EditRedo();   }
        private void Dock_Edit_Cut(object s, RoutedEventArgs e)    { Editor.EditCut();    }
        private void Dock_Edit_Copy(object s, RoutedEventArgs e)   { Editor.EditCopy();   }
        private void Dock_Edit_Paste(object s, RoutedEventArgs e)  { Editor.EditPaste();  }
        private void Dock_Edit_Delete(object s, RoutedEventArgs e) { Editor.EditDelete(); }
        #endregion
    }
}
