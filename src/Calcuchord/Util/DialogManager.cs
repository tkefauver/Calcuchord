using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using DialogHostAvalonia;

namespace Calcuchord;

public static class DialogManager {
    public static async Task<object> ShowAsync(Control control, string hostName) {
        try {
            var result = await DialogHost.Show(control, hostName);
            return result;
        }
        catch (Exception ex) {
            ex.Dump();
        }

        return null;
    }

    public static void Close(string hostName) {
        try {
            DialogHost.Close(hostName);
        }
        catch (Exception ex) {
            ex.Dump();
        }
    }
}