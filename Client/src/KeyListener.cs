using Microsoft.Xna.Framework.Input;
using System;


namespace RunGun.Client.Misc
{
    public class KeyListener
    {
        bool debounce;
        Keys key;
        Action kp;
        Action kr;

        public KeyListener(Keys keyToListen, Action onPress, Action onRelease) {
            key = keyToListen;
            kp = onPress;
            kr = onRelease;
        }

        public void Update() {
            if (Keyboard.GetState().IsKeyDown(key)) {
                if (debounce == false) {
                    debounce = true;
                    kp();
                }
            } else {
                if (debounce == true) {
                    debounce = false;
                    kr();
                }
            }
        }
    }
}
