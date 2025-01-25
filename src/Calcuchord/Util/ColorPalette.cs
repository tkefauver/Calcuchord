using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Material.Styles.Themes;
using Material.Styles.Themes.Base;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace Calcuchord {
    public enum PaletteColorType {
        None = 0,
        Bg,
        Fg,
        RootFretBg,
        RootFretFg,
        UserFretBg,
        UserFretFg,
        Finger1Fg,
        Finger1Bg,
        Finger2Fg,
        Finger2Bg,
        Finger3Fg,
        Finger3Bg,
        Finger4Fg,
        Finger4Bg,
        NutBg,
        NutFg,
        PianoWhiteKeyBg,
        PianoWhiteKeyFg,
        PianoBlackKeyBg,
        PianoBlackKeyFg,
        PianoMatch
    }

    public class ColorPalette : ViewModelBase {

        #region Statics

        static ColorPalette _instance;
        public static ColorPalette Instance => _instance ??= new();

        #endregion

        #region Properties

        public bool IsDark {
            get => Prefs.Instance.IsThemeDark;
            private set => Prefs.Instance.IsThemeDark = value;
        }

        CustomMaterialTheme _theme;

        CustomMaterialTheme Theme {
            get {
                if(_theme == null) {
                    if(Application.Current is not { } ac) {
                        return null;
                    }

                    _theme = ac.LocateMaterialTheme<CustomMaterialTheme>();
                }

                return _theme;
            }
        }

        Dictionary<PaletteColorType,(string light,string dark)> _fullPalette;
        Dictionary<PaletteColorType,string> _p;

        public Dictionary<PaletteColorType,string> P {
            get {
                if(_fullPalette == null) {
                    _fullPalette = new() {
                        // LIGHT / DARK
                        {
                            PaletteColorType.None,
                            ("#00000000","#00000000")
                        }, {
                            PaletteColorType.Bg,
                            ("#FFFFFF","#000000")
                        }, {
                            PaletteColorType.Fg,
                            ("#000000","#FFFFFF")
                        }, {
                            PaletteColorType.UserFretBg,
                            //("#EC3E7B","#EC3E7B")
                            ("#DC143C","#DC143C")
                        }, {
                            PaletteColorType.UserFretFg,
                            ("#FFFFFF","#FFFFFF")
                        }, {
                            PaletteColorType.RootFretBg,
                            ("#000000","#000000")
                        }, {
                            PaletteColorType.RootFretFg,
                            ("#FFFFFF","#FFFFFF")
                        }, {
                            PaletteColorType.Finger1Bg,
                            ("#77DD77","#77DD77")
                        }, {
                            PaletteColorType.Finger1Fg,
                            ("#FFFFFF","#FFFFFF")
                        }, {
                            PaletteColorType.Finger2Bg,
                            ("#21CEE1","#21CEE1")
                        }, {
                            PaletteColorType.Finger2Fg,
                            ("#FFFFFF","#FFFFFF")
                        }, {
                            PaletteColorType.Finger3Bg,
                            ("#A240E8","#A240E8")
                        }, {
                            PaletteColorType.Finger3Fg,
                            ("#FFFFFF","#FFFFFF")
                        }, {
                            PaletteColorType.Finger4Bg,
                            ("#FA8C00","#FA8C00")
                        }, {
                            PaletteColorType.Finger4Fg,
                            ("#FFFFFF","#FFFFFF")
                        }, {
                            PaletteColorType.NutBg,
                            ("#FAEBD7","#FAEBD7")
                        }, {
                            PaletteColorType.NutFg,
                            ("#000000","#000000")
                        }, {
                            PaletteColorType.PianoWhiteKeyBg,
                            ("#FFFFFF","#000000")
                        }, {
                            PaletteColorType.PianoWhiteKeyFg,
                            ("#000000","#FFFFFF")
                        }, {
                            PaletteColorType.PianoBlackKeyBg,
                            ("#000000","#FFFFFF")
                        }, {
                            PaletteColorType.PianoBlackKeyFg,
                            ("#000000","#FFFFFF")
                        }
                    };

                    if(IsDark) {
                        _p = _fullPalette.ToDictionary(x => x.Key,x => x.Value.dark);
                    } else {
                        _p = _fullPalette.ToDictionary(x => x.Key,x => x.Value.light);
                    }
                }

                return _p;
            }
        }

        public string Transparent { get; set; } = "#00000000";

        public string ThemeIcon =>
            IsDark ? "Lightbulb" : "LightbulbOutline";

        #endregion

        #region Public Methods

        public void Init() {
            SetTheme(Theme.BaseTheme == BaseThemeMode.Dark);
        }

        public void SetTheme(bool isDark) {
            IsDark = isDark;
            Theme.BaseTheme = IsDark ? BaseThemeMode.Dark : BaseThemeMode.Light;
            _fullPalette = null;
            _ = P;
            OnPropertyChanged(nameof(P));
            InitResoures();
        }

        #endregion

        #region Private Methods

        void InitResoures() {
            if(Application.Current is not { } ac) {
            }

            IResourceDictionary res = Application.Current.Resources;
            foreach(var kvp in P) {
                string brush_name = kvp.Key.ToString(); // + "Brush";
                IBrush brush = kvp.Value.ToAvBrush();
                if(res.ContainsKey(brush_name)) {
                    res[brush_name] = brush;
                } else {
                    res.Add(brush_name,brush);
                }
            }
        }

        #endregion

        public ICommand ToggleThemeCommand => new MpCommand(
            () => {
                SetTheme(!IsDark);
                OnPropertyChanged(nameof(ThemeIcon));
            });

    }
}