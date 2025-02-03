using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Calcuchord {
    public abstract class ModelBase {

    }

    [JsonObject]
    public abstract class PrimaryModelBase : IPrimaryModel {
        public string Id { get; private set; }

        public void CreateId(string forcedId) {

            if(!string.IsNullOrEmpty(Id)) {
                // error
                Debugger.Break();
            }

            Id = forcedId ?? Guid.NewGuid().ToString();
        }
    }
}