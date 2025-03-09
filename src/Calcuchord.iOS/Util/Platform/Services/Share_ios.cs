using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Threading;
using CoreGraphics;
using Foundation;
using Material.Styles.Controls;
using UIKit;

namespace Calcuchord.iOS;

public class Share_ios : Share_default {
    AppDelegate _appDelegate;
    public Share_ios(AppDelegate appDelegate) {
        _appDelegate = appDelegate;
    }
    protected override async Task<string> ShowSaveFilePickerAsync(string title,string[] extTypes) {
        await Task.Delay(1);
        string fileName = $"{title}.{extTypes[0]}";
        return Path.Combine(PlatformWrapper.Services.StorageHelper.StorageDir,fileName);
    }
    protected override void FinishShare(string filePath, string mimeType) {
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                if (_appDelegate is not {} ad || 
                    ad.Window is not { } w ||
                    w.RootViewController is not { } rvc)
                {
                    return;
                }

                var vc = rvc;
                while (vc.PresentedViewController != null)
                {
                    vc = vc.PresentedViewController;
                }

                // from https://stackoverflow.com/a/55092044/105028'
                var src = new TaskCompletionSource<bool>();
                NSObject[] items = [NSUrl.FromFilename(filePath), new NSString(Path.GetFileName(filePath))];
                var share_activity = new UIActivityViewController(items, null)
                {
                    CompletionWithItemsHandler = (_,_,_,_) => { src.TrySetResult(true); }
                };
                if (share_activity.PopoverPresentationController != null)
                {
                    share_activity.PopoverPresentationController.SourceView = vc.View;
                    //share_activity.PopoverPresentationController.SourceRect = new CGRect(vc.View.Frame.GetMidX(), vc.View.Frame.GetMidY(),0,0);
                    share_activity.PopoverPresentationController.SourceRect = new CGRect(0,0,1.0,1.0);
                }

                await vc.PresentViewControllerAsync(share_activity, true);
                await src.Task;
            }
            catch (Exception ex)
            {
                ex.Dump();
                SnackbarHost.Post(
                    ex.ToString(),
                    MainView.SnackbarHostName,
                    DispatcherPriority.Default);
            }
        });
    }
}