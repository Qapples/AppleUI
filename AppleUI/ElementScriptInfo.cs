using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AppleUI
{
    public readonly struct ElementScriptInfo
    {
        public readonly string Name;
        public readonly Dictionary<string, object> Arguments;
        public readonly bool Enabled;
        
        [JsonConstructor]
        public ElementScriptInfo(string name, Dictionary<string, object> arguments, bool enabled)
        {
            Name = name;
            Arguments = arguments;
            Enabled = enabled;
        }
    }
}