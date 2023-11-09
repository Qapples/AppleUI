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
                        Text.Remove(CursorPosition - 1, 1);
;                    }

                    break;
            }

            if (!char.IsControl(args.Character))
            {
                if (Selecting) DeleteSelection();
                
                Text.Insert(CursorPosition, args.Character);
            }
        }
    }
}