using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace RunGun.Client
{
    public enum InputMode { KEYBOARD, CONTROLLER, TOUCH }

    public class InputManager
    {
        InputMode inputMode;

        public event Action OnStartMoveLeft;
        public event Action OnStopMoveLeft;
        public event Action OnStartMoveRight;
        public event Action OnStopMoveRight;
        public event Action OnStartJump;
        public event Action OnStopJump;
        public event Action OnStartLookUp;
        public event Action OnStopLookUp;
        public event Action OnStartLookDown;
        public event Action OnStopLookDown;
        public event Action OnStartShoot;
        public event Action OnStopShoot;

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

        private void UpdateKeyboard(float dt) {
            var kbState = Keyboard.GetState();

            bool newMovingLeft = kbState.IsKeyDown(moveLeftKey);
            bool newMovingRight = kbState.IsKeyDown(moveRightKey);
            bool newJumping = kbState.IsKeyDown(jumpKey);
            bool newShooting = kbState.IsKeyDown(shootKey);
            bool newLookingUp = kbState.IsKeyDown(lookUpKey);
            bool newLookingDown = kbState.IsKeyDown(lookDownKey);

            if (movingLeft != newMovingLeft) {
                if (newMovingLeft) OnStartMoveLeft?.Invoke(); else  OnStopMoveLeft?.Invoke();
                movingLeft = newMovingLeft;
            }
            if (movingRight != newMovingRight) {
                if (newMovingRight) OnStartMoveRight?.Invoke(); else OnStopMoveRight?.Invoke();
                movingRight = newMovingRight;
            }
            if (jumping != newJumping) {
                if (newJumping) OnStartJump?.Invoke(); else OnStopJump?.Invoke();
                jumping = newJumping;
            }
            if (shooting != newShooting) {
                if (newShooting) OnStartShoot?.Invoke(); else OnStopShoot?.Invoke();
                shooting = newShooting;
            }
            if (lookingUp != newLookingUp) {
                if (newLookingUp) OnStartLookUp?.Invoke(); else OnStopLookUp?.Invoke();
                lookingUp = newLookingUp;
            }
            if (lookingDown != newLookingDown) {
                if (newLookingDown) OnStartLookDown?.Invoke(); else OnStopLookDown?.Invoke();
                lookingDown = newLookingDown;
            }
        }

        private void UpdateTouch(float dt) {

           
        }

        private void UpdateController(float dt) {
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

        public void Update(float dt) {

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
