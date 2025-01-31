using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using MonkeyPaste.Common;
using Newtonsoft.Json;
using ReactiveUI;

namespace Calcuchord {
    public class NewtonsoftJsonSuspensionDriver : ISuspensionDriver {
        readonly string _file;

        readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public NewtonsoftJsonSuspensionDriver(string file) {
            _file = file;
        }

        public IObservable<Unit> InvalidateState() {
            try {
                if(File.Exists(_file)) {
                    File.Delete(_file);
                }
            } catch(Exception ex) {
                ex.Dump();
            }

            return Observable.Return(Unit.Default);
        }

        public IObservable<object> LoadState() {
            string lines = string.Empty;
            try {
                if(!File.Exists(_file)) {
                    using(File.Create(_file)) {
                    }
                }

                lines = File.ReadAllText(_file);
            } catch(Exception ex) {
                ex.Dump();
            }

            object state = JsonConvert.DeserializeObject<object>(lines,_settings);
            return Observable.Return(state);
        }

        public IObservable<Unit> SaveState(object state) {
            string lines = JsonConvert.SerializeObject(state,_settings);
            try {
                File.WriteAllText(_file,lines);
            } catch(Exception ex) {
                ex.Dump();
            }

            return Observable.Return(Unit.Default);
        }
    }
}