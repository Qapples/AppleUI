using System;
using System.Diagnostics;
using AppleUI.Interfaces;
using AppleUI.Interfaces.Behavior;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using IUpdateable = AppleUI.Interfaces.IUpdateable;

namespace AppleUI.Elements
{
    public sealed class BaseButton : IButton, ITransform, IUpdateable
    {
        public Panel? ParentPanel { get; set; }
        
        public Measurement Position { get; set; }
        
        /// <summary>
        /// Represents the size of the button's area of intractability in pixels.
        /// </summary>
        public Vector2 Scale { get; set; }
        
        public float Rotation { get; set; }
        
        public bool IsMouseHoveringOver { get; private set; }

        private MouseState _previousMouseState;

        public event IButton.ButtonEventDelegate? OnHover;
        public event IButton.ButtonEventDelegate? OnMouseLeave;

        public event IButton.ButtonEventDelegate? OnPress;
        public event IButton.ButtonEventDelegate? OnRelease;

        public BaseButton(Panel? parentPanel, Measurement position, Vector2 size, float rotation)
        {
            (ParentPanel, Position, Scale, Rotation) = (parentPanel, position, size, rotation);
        }

        public void Update(Panel callingPanel, GameTime gameTime)
        {
            MouseState currentMouseState = Mouse.GetState();
            Vector2 panelPosition = callingPanel.RawPosition;
            
            //relative to callingPanel
            Vector2 relativeMousePos = currentMouseState.Position.ToVector2() - panelPosition;
            Vector2 relativeButtonPos = this.GetDrawPosition(callingPanel) - panelPosition;
            Rectangle buttonRect = new(relativeButtonPos.ToPoint(), Scale.ToPoint());
            bool buttonRectContainsMouse = buttonRect.Contains(relativeMousePos);

            if (buttonRectContainsMouse && !IsMouseHoveringOver)
            {
                IsMouseHoveringOver = true;
                OnHover?.Invoke(this, currentMouseState);
            }
            else if (!buttonRectContainsMouse)
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
            
            //Debug.WriteLine($"{relativeMousePos} {buttonRect} {buttonRect.Contains(relativeMousePos)}");

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

        public object Clone() => new BaseButton(ParentPanel, Position, Scale, Rotation)
        {
            OnHover = this.OnHover,
            OnPress = this.OnPress,
            OnRelease = this.OnRelease,
            OnMouseLeave = this.OnMouseLeave
        };
    }
}