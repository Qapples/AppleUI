using System;
using System.Collections.Generic;
using AppleSerialization.Info;
using Microsoft.Xna.Framework;

namespace AppleUI.Interfaces.Behavior
{
    public interface IElementBehaviorScript
    {
        bool Enabled { get; set; }
        
        /// <summary>
        /// This arguments object is automatically disposed when the element or its panel is disposed.
        /// </summary>
        Dictionary<string, object> Arguments { get; set; }
        
        IReadOnlyDictionary<string, Type> ArgumentDefinitions { get; } 
        
        void Init(IUserInterfaceElement element);

        void Update(IUserInterfaceElement element, GameTime gameTime);
    }

    public static class ElementBehaviorScriptExtensions
    {
        public static bool AreArgumentsValid(this IElementBehaviorScript script)
        {
            foreach (var (argName, argType) in script.ArgumentDefinitions)
            {
                if (!script.Arguments.TryGetValue(argName, out object? argObj)) return false;
                
                Type argObjType = argObj.GetType();
                if (argObjType == typeof(ValueInfo))
                {
                    ValueInfo argValue = (ValueInfo) argObj;
                    Type? argValueType = Type.GetType(argValue.ValueType);
                    
                    if (argValueType != argType) return false;
                }
                else if (argObjType != argType)
                {
                    return false;
                }
            }

            return true;
        }
    }
}