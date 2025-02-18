var isStorageLoaded = false;

function initStorage() {
    if (isStorageLoaded) {
        return;
    }
    try {
        localforage.config({
            //driver: localforage.WEBSQL, // Force WebSQL; same as using setDriver()
            name: 'myApp',
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
        const value = await localforage.getItem('prefs');
        // This code runs once the value has been loaded
        // from the offline store.
        return value;
    } catch (err) {
        // This code runs if there were any errors.
        console.log(err);
    }
    return '';
}

function writePrefsAsync(prefsJson) {
    initStorage();

    localforage.setItem('prefs', prefsJson).then(function (value) {
        // Do other things once the value has been saved.
        console.log('saved:')
        console.log(value);
    }).catch(function (err) {
        // This code runs if there were any errors
        console.log('save error:');
        console.log(err);
        alert(err);
    });
}