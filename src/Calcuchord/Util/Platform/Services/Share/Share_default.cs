using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using SkiaSharp;
using Svg.Skia;

namespace Calcuchord {
    public class Share_default : IShareMidi,ISharePdf {
        protected IStorageFolder LastFolder { get; set; }
        protected MidiFileBuilder Builder { get; } = new MidiFileBuilder();

        public async Task ShareMidiAsync(IEnumerable<IEnumerable<int>> toneSets,bool isScale,string title) {
            string fp = await ShowFileBrowserAsync(title,["mid","midi"]);
            if(fp is null) {
                return;
            }

            if(isScale) {
                await Builder.CreateMidiScaleAsync(toneSets,fp);
            } else {
                await Builder.CreateMidiChordAsync(toneSets,fp);
            }

            PlatformWrapper.Services.UriNavigator.NavigateTo(Path.GetDirectoryName(fp));

        }

        public async Task SharePdfAsync(SKSvg svg,string title) {
            string fp = await ShowFileBrowserAsync(title,["pdf"]);
            if(fp is null) {
                return;
            }

            svg.Picture.ToPdf(fp,ThemeViewModel.Instance.IsDark ? SKColors.Black : SKColors.White,1f,1f);
            PlatformWrapper.Services.UriNavigator.NavigateTo(Path.GetDirectoryName(fp));
        }

        protected async Task<string> ShowFileBrowserAsync(string title,string[] extTypes) {
            if(TopLevel.GetTopLevel(MainView.Instance) is not { } topLevel) {
                return null;
            }

            if(LastFolder is null) {
                LastFolder = await Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                    .ToFileOrFolderStorageItemAsync() as IStorageFolder;
            }

            // Start async operation to open the dialog.
            IStorageFile file = await topLevel.StorageProvider.SaveFilePickerAsync(
                new FilePickerSaveOptions
                {
                    Title = $"Save '{title}'",
                    DefaultExtension = extTypes.FirstOrDefault(),
                    FileTypeChoices = extTypes.Select(x => new FilePickerFileType(x)).ToList(),
                    SuggestedFileName = $"{title}.{extTypes.FirstOrDefault()}",
                    SuggestedStartLocation = LastFolder,
                });

            if(file is null ||
               file.Path.AbsoluteUri.ToPathFromUri() is not { } fp) {
                return null;
            }

            string fp_dir = Path.GetDirectoryName(fp);
            LastFolder = await fp_dir.ToFileOrFolderStorageItemAsync() as IStorageFolder;

            return fp;
            ;
        }

    }
}