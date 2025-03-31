var isStorageLoaded = false;

function initStorage() {
    if (isStorageLoaded) {
        return;
    }
    try {
        localforage.config({
            driver: localforage.INDEXEDDB,
            //driver: localforage.WEBSQL, // Force WebSQL; same as using setDriver()
            name: 'Calcuchord Storage',
            version: 1.0,
            size: 4980736, // Size of database, in bytes. WebSQL-only for now.
            storeName: 'keyvaluepairs', // Should be alphanumeric, with underscores.
            description: 'instrument tunings and bookmarks'
        })
        isStorageLoaded = true;
    } catch (err) {
        console.log(err);
    }

}

async function readPrefsAsync() {
    initStorage();
    try {
        return await localforage.getItem('prefs');
    } catch (err) {
        console.log(err);
    }
    return '';
}

function writePrefsAsync(prefsJson) {
    initStorage();

    localforage.setItem('prefs', prefsJson).then(function (value) {
        console.log('saved:')
    }).catch(function (err) {
        console.log('save error:');
        console.log(err);
        alert(err);
    });
}