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

        public event EventHandler<TextChangedEventArgs>? OnTextChanged;
        public event EventHandler<CursorPositionChangedArgs>? OnCursorPositionChanged;
        
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

            int oldCursorPosition = CursorPosition;
            bool callEvents = false;

            if (args.Key == Keys.Back)
            {
                if (Selecting) DeleteSelection();
                else if (CursorPosition != 0) Text.Remove(--CursorPosition, 1);
                
                callEvents = true;
            }

            if (!char.IsControl(args.Character))
            {
                if (Selecting) DeleteSelection();

                Text.Insert(CursorPosition++, args.Character);

                callEvents = true;
            }

            if (callEvents)
            {
                OnTextChanged?.Invoke(this, new TextChangedEventArgs(CursorPosition));
                OnCursorPositionChanged?.Invoke(this, new CursorPositionChangedArgs(oldCursorPosition, CursorPosition));
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
            bool shiftDown = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);

            if (shiftDown && !Selecting) SelectionBegin = CursorPosition;

            if (leftDown || rightDown)
            {
                if (!shiftDown && Selecting)
                {
                    if (leftDown && CursorPosition > SelectionBegin || rightDown && CursorPosition < SelectionBegin)
                    {
                        int oldCursorPosition = CursorPosition;
                        
                        CursorPosition = SelectionBegin;
                        OnCursorPositionChanged?.Invoke(this,
                            new CursorPositionChangedArgs(oldCursorPosition, SelectionBegin));
                    }

                    SelectionBegin = -1;
                }
                else
                {
                    if (leftDown && CursorPosition != 0)
                    {
                        HandleCursorMovementKey(gameTime.ElapsedGameTime, Keys.Left, -1);
                    }

                    if (rightDown && CursorPosition != Text.Length)
                    {
                        HandleCursorMovementKey(gameTime.ElapsedGameTime, Keys.Right, 1);
                    }
                }
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
                OnCursorPositionChanged?.Invoke(this,
                    new CursorPositionChangedArgs(CursorPosition, CursorPosition += movementDirection));
            }

            if (_heldKey != key)
            {
                _heldKey = key;
                _heldKeyDuration = TimeSpan.Zero;

                return;
            }

            if (_heldKeyDuration >= KeyRepeatDelay)
            {
                _heldKeyDuration = KeyRepeatDelay - KeyRepeatOnHoldInterval;
                OnCursorPositionChanged?.Invoke(this,
                    new CursorPositionChangedArgs(CursorPosition, CursorPosition += movementDirection));

                return;
            }

            _heldKeyDuration += elapsedTime;
        }

        public readonly record struct TextChangedEventArgs(int CursorPosition);

        public readonly record struct CursorPositionChangedArgs(int OldPosition, int NewPosition);
    }
}