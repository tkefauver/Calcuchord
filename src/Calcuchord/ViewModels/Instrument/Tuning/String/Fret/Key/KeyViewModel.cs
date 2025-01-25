using System.Linq;

namespace Calcuchord {
    public class KeyViewModel : FretViewModel {

        #region Private Variables

        #endregion

        #region Constants

        public const double WHITE_WIDTH_TO_HEIGHT_RATIO = 5.700677201;
        public const double WHITE_TO_BLACK_WIDTH_RATIO = 0.5;
        public const double WHITE_TO_BLACK_HEIGHT_RATIO = 0.633563;

        const double WHITE_KEY_WIDTH = 40;
        const double BLACK_KEY_WIDTH = WHITE_KEY_WIDTH * WHITE_TO_BLACK_WIDTH_RATIO;

        const double WHITE_KEY_HEIGHT = WHITE_KEY_WIDTH * WHITE_WIDTH_TO_HEIGHT_RATIO;
        const double BLACK_KEY_HEIGHT = WHITE_KEY_HEIGHT * WHITE_TO_BLACK_HEIGHT_RATIO;

        #endregion

        #region Statics

        public static double KeyboardWidth => WHITE_KEY_WIDTH * 14;
        public static double KeyboardHeight => WHITE_KEY_WIDTH * WHITE_WIDTH_TO_HEIGHT_RATIO;

        #endregion

        #region Interfaces

        #endregion

        #region Properties

        #region Members

        #endregion

        #region View Models

        #endregion

        #region Appearance

        #endregion

        #region Layout

        public double KeyWidth =>
            IsAltered ? BLACK_KEY_WIDTH : WHITE_KEY_WIDTH;

        public double KeyHeight =>
            IsAltered ? BLACK_KEY_HEIGHT : WHITE_KEY_HEIGHT;


        double? _keyX;

        public double KeyX {
            get {
                if(_keyX is not { } key_x) {
                    key_x = 0;
                    double white_x = 0;
                    foreach(FretViewModel fvm in Parent.Frets.OrderBy(x => x.FretNum)) {
                        if(fvm == this) {
                            if(IsAltered) {
                                key_x = white_x - (BLACK_KEY_WIDTH / 2d);
                            } else {
                                key_x = white_x;
                            }

                            break;
                        }

                        if(fvm.IsAltered) {
                            continue;
                        }

                        white_x += WHITE_KEY_WIDTH;
                    }

                    _keyX = key_x;
                }

                return _keyX.Value;
            }
        }

        #endregion

        #region State

        #endregion

        #region Model

        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors

        public KeyViewModel() {
        }

        public KeyViewModel(StringRowViewModel parent,InstrumentNote inn) : base(parent,inn) {
        }

        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        #endregion

    }
}