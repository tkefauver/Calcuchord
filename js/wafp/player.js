var AudioContextFunc = window.AudioContext || window.webkitAudioContext;
var audioContext;
var player;
var instrument;

const defInstrumentName = '0240_Aspirin_sf2_file';
var instrumentName = '0240_Aspirin_sf2_file';
var volumeDb = 0.5;
var sustain = 1;
var delayChordMs = 30;
var delayScaleMs = 300;

var isPlaying = false;
var needsStop = false;

const testNoteNumbers = [60, 64, 67];

function initMidi() {
    audioContext = new AudioContextFunc();
    player = new WebAudioFontPlayer();
    setInstrumentAsync(instrumentName);
}

function stopPlayback() {
    player.cancelQueue(audioContext);
}

function playNotes(midiNotes, delayMs) {
    let cur_delay = 300 / 1000;
    const notes = midiNotes;//midiNotes.map(midiNote => Tone.Midi(midiNote).toNote());
    for (var i = 0; i < notes.length; i++) {
        player.queueWaveTable(
            audioContext,
            audioContext.destination,
            instrument,
            audioContext.currentTime + cur_delay,
            notes[i],
            sustain,
            volumeDb);

        cur_delay += (delayMs / 1000);
    }
}

function playChord(midiNotes) {
    playNotes(midiNotes, delayChordMs);
}

function playScale(midiNotes) {
    playNotes(midiNotes, delayScaleMs);
}

function testChord() {
    playChord(testNoteNumbers);
}

function testScale() {
    playScale(testNoteNumbers);
}

function setInstrumentAsync(instName) {
    //https://surikov.github.io/webaudiofontdata/sound/0240_SBLive_sf2.html
    instrumentName = instName;
    if (instrumentName === defInstrumentName) {
        return;
    }
    let inst_url = `wafp/${instName}.js`;
    let inst_var_name = `_tone_${instName}`;

    fetch(inst_url)
        .then(response => response.text())
        .then(text => {
            // Do something with the text
            //console.log(text);
            let script = document.createElement('script');
            script.textContent = text;
            document.head.appendChild(script);

            instrument = window[inst_var_name];
            player.adjustPreset(audioContext, instrument);
        })
        .catch(error => {
            // Handle any errors
            console.error('Error:', error);
        });


}

function testInstrument() {
    instrumentName = document.getElementById('instrumentInput').value;
    initMidi();
}

function testVolume() {
    volumeDb = document.getElementById('volumeInput').value;
    testChord();
}

function testChordDelay() {
    delayChordMs = document.getElementById('chordDelayInput').value;
    testChord();
}

function testScaleDelay() {
    delayScaleMs = document.getElementById('scaleDelayInput').value;
    testScale();
}