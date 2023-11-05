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
            ElementTransform parentTransform = Parent.Transform;
            Vector2 parentOwnerRawPosition = Parent.Owner?.RawPosition ?? Vector2.Zero;
            Vector2 parentOwnerRawSize = Parent.Owner?.RawSize ?? Vector2.One;
            
            //relative to callingPanel
            Vector2 relativeMousePos = currentMouseState.Position.ToVector2() - parentOwnerRawPosition;
            Vector2 relativeButtonPos = parentTransform.Position.GetRawPixelValue(parentOwnerRawSize);
            Vector2 buttonSizePixels = Size.GetRawPixelValue(parentOwnerRawSize);
            
            RotatableRectangle buttonRect = new(relativeButtonPos, buttonSizePixels, parentTransform.Rotation);
            bool buttonRectContainsMouse = buttonRect.Contains(relativeMousePos);

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

        public Measurement GetCenterPositionPixels(Vector2 parentSizePixels)
        {
            ElementTransform parentTransform = Parent.Transform;
            
            Vector2 buttonHalfSizePixels = Size.GetRawPixelValue(parentSizePixels) * 0.5f;
            Vector2 parentPositionPixels = parentTransform.Position.GetRawPixelValue(parentSizePixels);
            Vector2 halfSizeRotated = Vector2.Transform(buttonHalfSizePixels,
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