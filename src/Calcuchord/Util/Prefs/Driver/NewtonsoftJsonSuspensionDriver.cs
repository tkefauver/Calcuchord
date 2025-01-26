using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using Newtonsoft.Json;
using ReactiveUI;

namespace Calcuchord {
    public class NewtonsoftJsonSuspensionDriver : ISuspensionDriver {
        readonly string _file;

        readonly JsonSerializerSettings _settings = new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.All
        };

        public NewtonsoftJsonSuspensionDriver(string file) {
            _file = file;
        }

        public IObservable<Unit> InvalidateState() {
            if(File.Exists(_file)) {
                File.Delete(_file);
            }

            return Observable.Return(Unit.Default);
        }

        public IObservable<object> LoadState() {
            if(!File.Exists(_file)) {
                using(File.Create(_file)) {
                }
            }

            string lines = File.ReadAllText(_file);
            object state = JsonConvert.DeserializeObject<object>(lines,_settings);
            return Observable.Return(state);
        }

        public IObservable<Unit> SaveState(object state) {
            string lines = JsonConvert.SerializeObject(state,_settings);
            File.WriteAllText(_file,lines);
            return Observable.Return(Unit.Default);
        }
    }
}