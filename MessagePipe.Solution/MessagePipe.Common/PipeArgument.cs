using System.Collections.Generic;
using System.Linq;

namespace MessagePipe.Common
{
    public static class PipeArgument
    {
        public class Builder
        {
            private readonly Dictionary<string, string> _arguments = new Dictionary<string, string>(8);
            
            public Builder ServerToClient(string value)
            {
                _arguments[Constants.PipeNameKeyS2C] = value;
                return this;
            }

            public Builder ClientToServer(string value)
            {
                _arguments[Constants.PipeNameKeyC2S] = value;
                return this;
            }

            public Builder OwnerProcess(int value)
            {
                _arguments[Constants.OwnerProcessId] = value.ToString();
                return this;
            }

            public string Build()
            {
                var ret = string.Join(" ", _arguments.Select(x => $"-{x.Key}={x.Value}"));
                _arguments.Clear();

                return ret;
            }
        }

        public class Parser
        {
            public Parser(string[] args)
            {
                foreach (var arg in args)
                {
                    if (!arg.StartsWith("-")) continue;
                
                    var pair = arg.Substring(1, arg.Length - 1).Split('=');
                    if (pair.Length != 2) continue;
                
                    _parsedValues[pair[0]] = pair[1];
                }
            }
            
            private readonly Dictionary<string, string> _parsedValues = new Dictionary<string, string>(8);
            
            public string GetString(string key)
            {
                return _parsedValues.TryGetValue(key, out var value) ? value : null;
            }

            public int GetInt(string key)
            {
                var stringValue = GetString(key);
                if (int.TryParse(stringValue, out var intValue)) return intValue;

                return 0;
            }
        }
    }
}