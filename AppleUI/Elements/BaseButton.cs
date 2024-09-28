using System;
using AppleUI.Interfaces.Behavior;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AppleUI.Elements
{
    public sealed class BaseButton : IButtonBehavior, ICloneable
    {
        public UserInterfaceElement Parent { get; internal set; }
        
        public Measurement Size { get; set; }

        public bool IsMouseHoveringOver { get; private set; }

        public ButtonEvents ButtonEvents { get; private set; }
        
        private MouseState _previousMouseState;

        public BaseButton(UserInterfaceElement parent, Measurement size)
        {
            (Parent, Size) = (parent, size);
   
            ButtonEvents = new ButtonEvents();
        }

        public void Update(GameTime gameTime)
        {
            MouseState currentMouseState = Mouse.GetState();
            
            Vector2 mousePos = currentMouseState.Position.ToVector2();
            Vector2 buttonSizePixels = Size.GetRawPixelValue(Parent.OwnerRawSize);
            
            RotatableRectangle buttonRect = new(Parent.RawPosition, buttonSizePixels, Parent.Transform.Rotation);
            bool buttonRectContainsMouse = buttonRect.Contains(mousePos);

            if (buttonRectContainsMouse && !IsMouseHoveringOver)
            {
                IsMouseHoveringOver = true;
                ButtonEvents.InvokeOnHover(Parent, currentMouseState);
            }
            else if (!buttonRectContainsMouse)
            {
                if (IsMouseHoveringOver)
                {
                    ButtonEvents.InvokeOnMouseLeave(Parent, currentMouseState);
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
                    ButtonEvents.InvokeOnPress(Parent, currentMouseState);
                    wasOnPressedInvoked = true;
                }

                if (IsMouseHoveringOver && !wasOnReleaseInvoked && previousState == ButtonState.Pressed &&
                    currentState == ButtonState.Released)
                {
                    ButtonEvents.InvokeOnRelease(Parent, currentMouseState);
                    wasOnReleaseInvoked = true;
                }
            }
            
            //Debug.WriteLine($"{relativeMousePos} {buttonRect} {buttonRect.Contains(relativeMousePos)}");

            _previousMouseState = currentMouseState;
        }

        public Measurement GetCenterPositionPixels()
        {
            ElementTransform parentTransform = Parent.Transform;
            
            Vector2 parentPositionPixels =
                parentTransform.GetDrawPosition(Parent.OwnerRawPosition, Parent.OwnerRawSize, Parent.RawSize);
            Vector2 halfSizeRotated = Vector2.Transform(Parent.RawSize * 0.5f,
                Quaternion.CreateFromYawPitchRoll(0f, 0f, parentTransform.Rotation));
            
            return new Measurement(parentPositionPixels + halfSizeRotated, MeasurementType.Pixel);
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

        public object Clone() => new BaseButton(Parent, Size) { ButtonEvents = this.ButtonEvents };
    }
}