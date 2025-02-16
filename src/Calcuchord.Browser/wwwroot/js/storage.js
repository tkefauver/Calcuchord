function readPrefs() {
    return localStorage.getItem('prefs');
}

function writePrefs(prefsJson) {
    localStorage.setItem('prefs', prefsJson);
}