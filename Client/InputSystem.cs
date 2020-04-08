using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;

namespace RunGun.Client
{
    public enum InputMode { KEYBOARD, CONTROLLER, TOUCH }

    public struct Keybinds
    {
        Keys Left;
        Keys Right;
        Keys Jump;
        Keys Shoot;
        Keys LookUp;
        Keys LookDown;

        public Keybinds? LoadFromFile() {
            return null;
        }
    }

    public class InputManager
    {
        InputMode inputMode;

        //public event Action OnStartMoveLeft;
        //public event Action OnStopMoveLeft;
        //public event Action OnStartMoveRight;
        //public event Action OnStopMoveRight;
        //public event Action OnStartJump;
       // public event Action OnStopJump;
        //public event Action OnStartLookUp;
       // public event Action OnStopLookUp;
       // public event Action OnStartLookDown;
        //public event Action OnStopLookDown;
        //public event Action OnStartShoot;
        //public event Action OnStopShoot;

        public Keybinds binds;

        public Keys moveLeftKey;
        public Keys moveRightKey;
        public Keys jumpKey;
        public Keys shootKey;
        public Keys lookDownKey;
        public Keys lookUpKey;

        bool movingLeft;
        bool movingRight;
        bool jumping;
        bool shooting;
        bool lookingDown;
        bool lookingUp;

        public InputManager(InputMode input) {
            inputMode = input;

            // keybind defaults:
            moveLeftKey = Keys.A;
            moveRightKey = Keys.D;
            lookUpKey = Keys.W;
            lookDownKey = Keys.S;
            jumpKey = Keys.Space;
            shootKey = Keys.LeftShift;
        }

        public bool IsUserMovingLeft() { return movingLeft; }
        public bool IsUserMovingRight() { return movingRight; }
        public bool IsUserJumping() { return jumping; }
        public bool IsUserShooting() { return shooting; }
        public bool IsUserLookingUp() { return lookingUp; }
        public bool IsUserLookingDown() { return lookingDown; }

        protected void UpdateEvents() {

        }

        protected virtual void UpdateKeyboard(float dt) {
            var kbState = Keyboard.GetState();

            movingLeft = kbState.IsKeyDown(moveLeftKey);
            movingRight = kbState.IsKeyDown(moveRightKey);
            jumping = kbState.IsKeyDown(jumpKey);
            shooting = kbState.IsKeyDown(shootKey);
            lookingUp = kbState.IsKeyDown(lookUpKey);
            lookingDown = kbState.IsKeyDown(lookDownKey);
        }

        protected virtual void UpdateTouch(float dt) {
            movingLeft = false;
            movingRight = false;

            foreach (TouchLocation touch in TouchPanel.GetState()) {

               //Console.WriteLine("TOUCH: {0} {1}", touch.Position, touch.Pressure);

                if (touch.Position.Y < 400) {
                    movingLeft = true; 
                }

                if (touch.Position.Y > 600) {
                    movingRight = true;
                }
            } 
        }

        protected virtual void UpdateController(float dt) {
            var gamepad = GamePad.GetState(PlayerIndex.One);

            var dpad = gamepad.DPad;

            var buttons = GamePad.GetState(PlayerIndex.One).Buttons;
            var sticks = GamePad.GetState(PlayerIndex.One).ThumbSticks;

            if (sticks.Left.X < 0 || dpad.Left == ButtonState.Pressed)
                movingLeft = true;

            if (sticks.Left.X > 0 || dpad.Right == ButtonState.Pressed)
                movingRight = true;

            if (sticks.Left.Y > 0 || dpad.Up == ButtonState.Pressed)
                lookingUp = true;

            if (sticks.Left.Y < 0 || dpad.Down == ButtonState.Pressed)
                lookingDown = true;

            if (buttons.A == ButtonState.Pressed)
                jumping = true;

            if (gamepad.Triggers.Right > 0.5)
                shooting = true;
        }

        public virtual void Update(float dt) {

            switch(inputMode) {
                case InputMode.KEYBOARD:
                    UpdateKeyboard(dt);
                    break;
                case InputMode.CONTROLLER:
                    UpdateController(dt);
                    break;
                case InputMode.TOUCH:
                    UpdateTouch(dt);
                    break;
            }
        }
    }
}
