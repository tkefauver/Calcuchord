using System;
using System.IO;
using System.Threading.Tasks;

namespace Calcuchord;

public class PrefsIo_default : IPrefsIo {
    private const string PREFS_FILE_NAME = "appstate.json";
    private const string PREFS_BACKUP_FILE_NAME = "appstate.backup.json";

    private string _prefsBackupFilePath;

    private string _prefsFilePath;

    protected string PrefsBackupFilePath
    {
        get
        {
            if (_prefsBackupFilePath == null &&
                PlatformWrapper.Services is { } ps &&
                ps.StorageHelper is { } sh &&
                sh.StorageDir is { } sd)
                _prefsBackupFilePath = Path.Combine(sd, PREFS_BACKUP_FILE_NAME);

            return _prefsBackupFilePath;
        }
    }

    protected string PrefsFilePath
    {
        get
        {
            if (_prefsFilePath == null &&
                PlatformWrapper.Services is { } ps &&
                ps.StorageHelper is { } sh &&
                sh.StorageDir is { } sd)
                _prefsFilePath = Path.Combine(sd, PREFS_FILE_NAME);

            return _prefsFilePath;
        }
    }

    public virtual async Task<string> ReadPrefsAsync() {
        if (!File.Exists(PrefsFilePath))
            if (!File.Exists(PrefsBackupFilePath))
                return string.Empty;

        try {
            return await File.ReadAllTextAsync(PrefsFilePath);
        }
        catch (Exception e) {
            e.Dump();
            try {
                return await File.ReadAllTextAsync(PrefsBackupFilePath);
            }
            catch (Exception e2) {
                e2.Dump();
            }
        }

        return string.Empty;
    }


    public virtual async Task WritePrefsAsync(string prefsJson) {
        try {
            await File.WriteAllTextAsync(PrefsFilePath, prefsJson);
            await File.WriteAllTextAsync(PrefsBackupFilePath, prefsJson);
        }
        catch (Exception e) {
            e.Dump();
        }
    }
}