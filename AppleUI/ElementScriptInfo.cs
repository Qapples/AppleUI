using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AppleUI
{
    public readonly struct ElementScriptInfo
    {
        public readonly string Name;
        public readonly bool Enabled;
        public readonly Dictionary<string, object> Arguments;
        
        [JsonConstructor]
        public ElementScriptInfo(string name, bool enabled, Dictionary<string, object> arguments)
        {
            (Name, Enabled, Arguments) = (name, enabled, arguments);
        }
    }
}