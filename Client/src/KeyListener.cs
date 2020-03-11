using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunGun.Client.Misc
{
    public class KeyListener
    {
        bool debounce;
        Keys key;
        Func<int> kp;
        Func<int> kr;

        public KeyListener(Keys keyToListen, Func<int> onPress, Func<int> onRelease) {
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
