using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AppleUI
{
    public class TextInput
    {
        public StringBuilder Text { get; private set; }
        
        public int CursorPosition { get; private set; }
        public int SelectionBegin { get; private set; }

        public bool Selecting => SelectionBegin != -1;
        
        public bool AcceptingInput { get; set; }
        
        public TextInput(GameWindow window)
        {
            Text = new StringBuilder();
            window.TextInput += OnTextInput;

            (CursorPosition, SelectionBegin) = (0, -1);
        }

        private void DeleteSelection()
        {
            Text.Remove(Math.Min(CursorPosition, SelectionBegin), Math.Abs(CursorPosition - SelectionBegin));
                
            if (CursorPosition > SelectionBegin)
            {
                CursorPosition = SelectionBegin;
            }

            SelectionBegin = -1;
        }

        private void OnTextInput(object? sender, TextInputEventArgs args)
        {
            if (!AcceptingInput) return;
            
            KeyboardState keyboardState = Keyboard.GetState();
            bool shifted = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);

            if (!shifted) SelectionBegin = -1;
            else if (!Selecting) SelectionBegin = CursorPosition;
            
            switch (args.Key)
            {
                case Keys.Left when CursorPosition != 0:
                    CursorPosition--;
                    break;
                case Keys.Right when CursorPosition != Text.Length:
                    CursorPosition++;
                    break;
                case Keys.Back when CursorPosition != 0:
                    if (Selecting)
                    {
                        DeleteSelection();
                    }
                    else
                    {
                        Text.Remove(--CursorPosition, 1);
;                    }

                    break;
            }

            if (!char.IsControl(args.Character))
            {
                if (Selecting) DeleteSelection();
                
                Text.Insert(CursorPosition++, args.Character);
            }
        }

        private KeyboardState _prevKeyboardState;

        private Keys _heldKey;
        private TimeSpan _heldKeyDuration;
        
        private static readonly TimeSpan KeyRepeatDelay = TimeSpan.FromSeconds(0.5);
        private static readonly TimeSpan KeyRepeatOnHoldInterval = TimeSpan.FromSeconds(0.05);

        public void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            bool leftDown = keyboardState.IsKeyDown(Keys.Left);
            bool rightDown = keyboardState.IsKeyDown(Keys.Right);
            
            if (leftDown || rightDown)
            {
                if (leftDown && CursorPosition != 0) HandleCursorMovementKey(gameTime.ElapsedGameTime, Keys.Left, -1);
                if (rightDown && CursorPosition != Text.Length) HandleCursorMovementKey(gameTime.ElapsedGameTime, Keys.Right, 1);
            }
            else
            {
                _heldKey = Keys.None;
                _heldKeyDuration = TimeSpan.Zero;
            }
            
            _prevKeyboardState = keyboardState;
        }

        private void HandleCursorMovementKey(TimeSpan elapsedTime, Keys key, int movementDirection)
        {
            if (!_prevKeyboardState.IsKeyDown(key))
            {
                CursorPosition += movementDirection;
            }

            if (_heldKey != key)
            {
                _heldKey = key;
                _heldKeyDuration = TimeSpan.Zero;

                return;
            }

            if (_heldKeyDuration >= KeyRepeatDelay)
            {
                CursorPosition += movementDirection;
                _heldKeyDuration = KeyRepeatDelay - KeyRepeatOnHoldInterval;

                return;
            }

            _heldKeyDuration += elapsedTime;
        }
    }
}