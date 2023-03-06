using System;
using AppleUI.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using IUpdateable = AppleUI.Interfaces.IUpdateable;

namespace AppleUI.Elements
{
    public sealed class BaseButton : IButton, ITransform, IUpdateable
    {
        public Panel? ParentPanel { get; set; }
        
        public (Vector2 Value, PositionType Type) Position { get; set; }
        
        /// <summary>
        /// Represents the size of the button's area of intractability.
        /// </summary>
        public Vector2 Scale { get; set; }
        
        public float Rotation { get; set; }
        
        public bool IsMouseHoveringOver { get; private set; }

        private MouseState _previousMouseState;

        public event IButton.ButtonEventDelegate? OnHover;
        public event IButton.ButtonEventDelegate? OnMouseLeave;

        public event IButton.ButtonEventDelegate? OnPress;
        public event IButton.ButtonEventDelegate? OnRelease;

        public void Update(Panel callingPanel, GameTime gameTime)
        {
            MouseState currentMouseState = Mouse.GetState();
            
            //relative to callingPanel
            Vector2 relativeMousePos = currentMouseState.Position.ToVector2() - callingPanel.Position;
            Vector2 relativeButtonPos = this.GetDrawPosition(callingPanel) - callingPanel.Position;
            Rectangle buttonRect = new(relativeButtonPos.ToPoint(), Scale.ToPoint());

            if (buttonRect.Contains(relativeMousePos) && !IsMouseHoveringOver)
            {
                IsMouseHoveringOver = true;
                OnHover?.Invoke(this, currentMouseState);
            }
            else
            {
                if (IsMouseHoveringOver)
                {
                    OnMouseLeave?.Invoke(this, currentMouseState);
                }
                
                IsMouseHoveringOver = false;
            }

            //If *any* mouse button is pressed/clicked/released, then invoke the event. The event can distinguish what
            //button it is since the mouse state is passed to the event.

            bool wasOnPressedInvoked = false;
            bool wasOnReleaseInvoked = false;

            for (int i = 0; i < 5; i++)
            {
                ButtonState previousState = GetMouseButtonState(_previousMouseState, i);
                ButtonState currentState = GetMouseButtonState(currentMouseState, i);

                if (IsMouseHoveringOver && !wasOnPressedInvoked && previousState == ButtonState.Released &&
                    currentState == ButtonState.Pressed)
                {
                    OnPress?.Invoke(this, currentMouseState);
                    wasOnPressedInvoked = true;
                }

                if (IsMouseHoveringOver && !wasOnReleaseInvoked && previousState == ButtonState.Pressed &&
                    currentState == ButtonState.Released)
                {
                    OnRelease?.Invoke(this, currentMouseState);
                    wasOnReleaseInvoked = true;
                }
            }

            _previousMouseState = currentMouseState;
        }

        private static ButtonState GetMouseButtonState(MouseState mouseState, int index) => index switch
        {
            0 => mouseState.LeftButton,
            1 => mouseState.RightButton,
            2 => mouseState.MiddleButton,
            3 => mouseState.XButton1,
            4 => mouseState.XButton2,
            _ => throw new IndexOutOfRangeException("Index is out of range (range is [0, 4] )")
        };

        public object Clone() => new BaseButton
        {
            ParentPanel = ParentPanel,
            OnHover = this.OnHover,
            OnPress = this.OnPress,
            OnRelease = this.OnRelease,
            OnMouseLeave = this.OnMouseLeave
        };
    }
}