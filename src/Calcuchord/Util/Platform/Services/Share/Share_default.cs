using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace Calcuchord {
    public class Share_default : IShareMidi,ISharePdf,IShareHtml {
        protected IStorageFolder LastFolder { get; set; }

        public virtual async Task ShareMidiAsync(IEnumerable<IEnumerable<int>> toneSets,bool isScale,string title) {
            if(PlatformWrapper.Services.MidiFileBuilder is not { } mfb) {
                return;
            }

            string fp = await ShowSaveFilePickerAsync(title,["mid","midi"]);
            if(fp is null) {
                return;
            }

            if(isScale) {
                await mfb.CreateMidiScaleAsync(toneSets,fp);
            } else {
                await mfb.CreateMidiChordAsync(toneSets,fp);
            }

            FinishShare(fp,"audio/midi");
        }

        public virtual async Task SharePdfAsync(byte[] pdfBytes,string title) {
            string fp = await ShowSaveFilePickerAsync(title,["pdf"]);
            if(fp is null) {
                return;
            }

            await File.WriteAllBytesAsync(fp,pdfBytes);

            FinishShare(fp,"application/pdf");
        }

        public virtual async Task ShareHtmlAsync(string html,string title) {
            string fp = await ShowSaveFilePickerAsync(title,["html"]);
            if(fp is null) {
                return;
            }

            await File.WriteAllTextAsync(fp,html);
            FinishShare(fp,"text/html");
        }

        protected virtual void FinishShare(string filePath,string mimeType) {
            PlatformWrapper.Services.UriNavigator.NavigateTo(Path.GetDirectoryName(filePath),filePath);
        }

        protected virtual async Task<string> ShowSaveFilePickerAsync(string title,string[] extTypes) {
            if(TopLevel.GetTopLevel(MainView.Instance) is not { } topLevel ||
               !topLevel.StorageProvider.CanSave) {
                return null;
            }

            IStorageFile file = null;

            await Dispatcher.UIThread.InvokeAsync(
                async () => {
                    if(LastFolder is null) {
                        // note supported on mobile or browser in av 11.2.4
                        // see https://docs.avaloniaui.net/docs/concepts/services/storage-provider/#platform-compatibility
                        LastFolder = await topLevel.StorageProvider.TryGetFolderFromPathAsync(
                            Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                    }

                    // Start async operation to open the dialog.
                    file = await topLevel.StorageProvider.SaveFilePickerAsync(
                        new FilePickerSaveOptions
                        {
                            Title = $"Save '{title}'",
                            DefaultExtension = extTypes.FirstOrDefault(),
                            FileTypeChoices = extTypes.Select(x => new FilePickerFileType(x)).ToList(),
                            SuggestedFileName = $"{title}.{extTypes.FirstOrDefault()}",
                            SuggestedStartLocation = LastFolder,
                        });
                });

            if(file?.Path.AbsolutePath is not { } fp) {
                return null;
            }

            // note supported on mobile or browser in av 11.2.4
            // see https://docs.avaloniaui.net/docs/concepts/services/storage-provider/#platform-compatibility
            string fp_dir = Path.GetDirectoryName(fp);
            LastFolder = await topLevel.StorageProvider.TryGetFolderFromPathAsync(fp_dir);

            return fp;
        }

    }
}