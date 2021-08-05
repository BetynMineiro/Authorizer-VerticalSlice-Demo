using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Authorizer.Common
{
  
    public abstract class CommonOutPut
    {
        [JsonPropertyName("violations")]
        public IList<string> Violations { get; protected set; } = new List<string>();
    }
}
