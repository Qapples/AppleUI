using System;
using System.Diagnostics;
using AppleUI.Interfaces;
using AppleUI.Interfaces.Behavior;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using IUpdateable = AppleUI.Interfaces.IUpdateable;

namespace AppleUI.Elements
{
    internal sealed class BaseButton : IButton, ITransform, IUpdateable
    {
        public Panel? ParentPanel { get; set; }
        
        public Measurement Position { get; set; }
        
        /// <summary>
        /// This value is unused for BaseButton, but is required for ITransform. This value does serve a purpose for
        /// other buttons.
        /// </summary>
        public Vector2 Scale { get; set; }
        
        public Measurement ButtonSize { get; set; }

        public float Rotation { get; set; }

        public bool IsMouseHoveringOver { get; private set; }

        private MouseState _previousMouseState;

        public ButtonEvents ButtonEvents { get; private init; }

        public BaseButton(Panel? parentPanel, Measurement position, Measurement size, float rotation)
        {
            (ParentPanel, Position, ButtonSize, Rotation) = (parentPanel, position, size, rotation);
            
            Scale = Vector2.One;
            ButtonEvents = new ButtonEvents();
        }

        public void Update(Panel callingPanel, GameTime gameTime)
        {
            MouseState currentMouseState = Mouse.GetState();
            Vector2 panelPosition = callingPanel.RawPosition;
            
            //relative to callingPanel
            Vector2 relativeMousePos = currentMouseState.Position.ToVector2() - panelPosition;
            Vector2 relativeButtonPos = this.GetDrawPosition(callingPanel) - panelPosition;
            Vector2 buttonSizePixels = ButtonSize.GetRawPixelValue(callingPanel.RawSize);
            
            RotatableRectangle buttonRect = new(relativeButtonPos, buttonSizePixels, Rotation);
            bool buttonRectContainsMouse = buttonRect.Contains(relativeMousePos);

            if (buttonRectContainsMouse && !IsMouseHoveringOver)
            {
                IsMouseHoveringOver = true;
                ButtonEvents.InvokeOnHover(this, currentMouseState);
            }
            else if (!buttonRectContainsMouse)
            {
                if (IsMouseHoveringOver)
                {
                    ButtonEvents.InvokeOnMouseLeave(this, currentMouseState);
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
                    ButtonEvents.InvokeOnPress(this, currentMouseState);
                    wasOnPressedInvoked = true;
                }

                if (IsMouseHoveringOver && !wasOnReleaseInvoked && previousState == ButtonState.Pressed &&
                    currentState == ButtonState.Released)
                {
                    ButtonEvents.InvokeOnRelease(this, currentMouseState);
                    wasOnReleaseInvoked = true;
                }
            }
            
            //Debug.WriteLine($"{relativeMousePos} {buttonRect} {buttonRect.Contains(relativeMousePos)}");

            _previousMouseState = currentMouseState;
        }
        
        public Measurement GetCenterPositionPixels(Vector2 parentSizePixels)
        {
            Vector2 buttonSizePixels = ButtonSize.GetRawPixelValue(parentSizePixels);
            Vector2 buttonPositionPixels = Position.GetRawPixelValue(parentSizePixels);
            
            return new Measurement(buttonPositionPixels + buttonSizePixels * 0.5f, MeasurementType.Pixel);
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

        public object Clone() => new BaseButton(ParentPanel, Position, ButtonSize, Rotation)
        {
            ButtonEvents = this.ButtonEvents
        };
    }
}