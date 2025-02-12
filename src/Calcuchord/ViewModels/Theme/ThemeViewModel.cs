using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Material.Styles.Themes;
using Material.Styles.Themes.Base;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace Calcuchord {


    public class ThemeViewModel : ViewModelBase {

        #region Statics

        static ThemeViewModel _instance;
        public static ThemeViewModel Instance => _instance ??= new();

        #endregion

        #region Properties

        bool IsPretendMobile { get; set; }

        public bool IsDesktop =>
            !IsBrowser && !IsMobile;

        public bool IsBrowser =>
            OperatingSystem.IsBrowser();

        public bool IsMobile =>
            OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();

        public bool IsPhone =>
            IsMobile && !IsTablet;

        public bool IsTablet => false;

        public bool IsLandscape {
            get {

                if((IsDesktop && !IsPretendMobile) ||
                   MainView.Instance is not { } mv ||
                   TopLevel.GetTopLevel(mv) is not { } tl) {
                    return false;
                }

                double w = 0;
                double h = 0;
                if(tl.Screens is { } scr &&
                   scr.Primary is { } pscr) {
                    w = pscr.Bounds.Width / pscr.Scaling;
                    h = pscr.Bounds.Height / pscr.Scaling;
                } else {
                    w = tl.Bounds.Width / tl.RenderScaling;
                    h = tl.Bounds.Height / tl.RenderScaling;
                }

                return tl.Bounds.Width > tl.Bounds.Height;
            }
        }

        public Orientation Orientation =>
            IsLandscape ? Orientation.Horizontal : Orientation.Vertical;

        public bool IsDark {
            get => Prefs.Instance.IsThemeDark;
            private set {
                if(IsDark != value) {
                    Prefs.Instance.IsThemeDark = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsDark));
                }
            }
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
                    _fullPalette = new()
                    {
                        // LIGHT / DARK
                        {
                            PaletteColorType.None,
                            ("#00000000","#00000000")
                        },
                        {
                            PaletteColorType.HttTransparent,
                            ("#01000000","#01000000")
                        },
                        {
                            PaletteColorType.Bg,
                            ("#FFFFFF","#000000")
                        },
                        {
                            PaletteColorType.Fg,
                            ("#000000","#FFFFFF")
                        },
                        {
                            PaletteColorType.UserFretBg,
                            //("#EC3E7B","#EC3E7B")
                            ("#DC143C","#DC143C")
                        },
                        {
                            PaletteColorType.UserFretFg,
                            ("#FFFFFF","#FFFFFF")
                        },
                        {
                            PaletteColorType.RootFretBg,
                            ("#000000","#000000")
                        },
                        {
                            PaletteColorType.RootFretFg,
                            ("#FFFFFF","#FFFFFF")
                        },
                        {
                            PaletteColorType.Finger1Bg,
                            ("#77DD77","#77DD77")
                        },
                        {
                            PaletteColorType.Finger1Fg,
                            ("#FFFFFF","#FFFFFF")
                        },
                        {
                            PaletteColorType.Finger2Bg,
                            ("#21CEE1","#21CEE1")
                        },
                        {
                            PaletteColorType.Finger2Fg,
                            ("#FFFFFF","#FFFFFF")
                        },
                        {
                            PaletteColorType.Finger3Bg,
                            ("#A240E8","#A240E8")
                        },
                        {
                            PaletteColorType.Finger3Fg,
                            ("#FFFFFF","#FFFFFF")
                        },
                        {
                            PaletteColorType.Finger4Bg,
                            ("#FA8C00","#FA8C00")
                        },
                        {
                            PaletteColorType.Finger4Fg,
                            ("#FFFFFF","#FFFFFF")
                        },
                        {
                            PaletteColorType.NutBg,
                            ("#FFDEAD","#FFDEAD")
                        },
                        {
                            PaletteColorType.NutFg,
                            ("#000000","#000000")
                        },
                        {
                            PaletteColorType.PianoWhiteKeyBg,
                            ("#FFFFFF","#000000")
                        },
                        {
                            PaletteColorType.PianoWhiteKeyBg2,
                            ("#FFFFFF","#222222")
                        },
                        {
                            PaletteColorType.PianoWhiteKeyBg3,
                            ("#FFFFFF","#444444")
                        },
                        {
                            PaletteColorType.PianoWhiteKeyFg,
                            ("#000000","#FFFFFF")
                        },
                        {
                            PaletteColorType.PianoBlackKeyBg,
                            ("#000000","#FFFFFF")
                        },
                        {
                            PaletteColorType.PianoBlackKeyBg2,
                            ("#222222","#DDDDDD")
                        },
                        {
                            PaletteColorType.PianoBlackKeyBg3,
                            ("#444444","#DDDDDD")
                        },
                        {
                            PaletteColorType.PianoBlackKeyFg,
                            ("#000000","#FFFFFF")
                        },
                        {
                            PaletteColorType.DisabledAccentFg,
                            ("#333333","#DDDDDD")
                        },
                        {
                            PaletteColorType.UnknownFretBg,
                            ("#000000","#FFFFFF")
                        },
                        {
                            PaletteColorType.UnknownFretFg,
                            ("#FFFFFF","#000000")
                        },
                        {
                            PaletteColorType.MutedFretBg,
                            ("#696969","#DCDCDC")
                        },
                        {
                            PaletteColorType.MutedFretFg,
                            ("#FFFFFF","#000000")
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

        public bool IsDebug =>
#if DEBUG
            true;
#else
        false;
#endif

        public string Transparent { get; set; } = "#00000000";

        public string ThemeIcon =>
            IsDark ? "Lightbulb" : "LightbulbOutline";

        #endregion

        #region Events

        public event EventHandler OrientationChanged;

        #endregion

        #region Public Methods

        public void Init() {
            bool is_sys_theme_dark = Theme.BaseTheme == BaseThemeMode.Dark;
            bool is_dark = Prefs.Instance.IsInitialStartup ? is_sys_theme_dark : Prefs.Instance.IsThemeDark;
            SetTheme(is_dark);
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
                return;
            }

            IResourceDictionary res = ac.Resources;
            foreach(var kvp in P) {
                string brush_name = kvp.Key.ToString();
                IBrush brush = kvp.Value.ToAvBrush();
                if(res.ContainsKey(brush_name)) {
                    res[brush_name] = brush;
                } else {
                    res.Add(brush_name,brush);
                }

                string color_name = kvp.Key + "Color";
                Color color = kvp.Value.ToAvColor();
                if(res.ContainsKey(color_name)) {
                    res[color_name] = color;
                } else {
                    res.Add(color_name,color);
                }
            }
        }

        #endregion

        public ICommand ToggleThemeCommand => new MpCommand(
            () => {
                SetTheme(!IsDark);
                if(MainViewModel.Instance is { } mvm) {
                    mvm.ResetMatchSvg();
                }

                OnPropertyChanged(nameof(ThemeIcon));
            });

        public ICommand ToggleLandscapeCommand => new MpCommand(
            () => {
                IsPretendMobile = true;
                if(MainView.Instance is not { } mv ||
                   TopLevel.GetTopLevel(mv) is not { } tl) {
                    return;
                }

                bool was_landscape = IsLandscape;

                tl.EffectiveViewportChanged += TlOnEffectiveViewportChanged;

                void TlOnEffectiveViewportChanged(object sender,EffectiveViewportChangedEventArgs e) {
                    if(IsLandscape != was_landscape) {
                        tl.EffectiveViewportChanged -= TlOnEffectiveViewportChanged;
                        OnPropertyChanged(nameof(IsLandscape));
                        OnPropertyChanged(nameof(Orientation));
                        OrientationChanged?.Invoke(this,EventArgs.Empty);
                    }
                }

                double w = tl.Bounds.Width;
                double h = tl.Bounds.Height;
                tl.Width = h;
                tl.Height = w;


            });

    }
}