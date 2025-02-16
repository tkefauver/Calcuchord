var synth;
var volumeDb = -12;
var delayChordMs = 30;
var delayScaleMs = 300;

var isPlaying = false;
var needsStop = false;

const testNotes = [60, 64, 67];

function init() {
    if (synth) {
        return;
    }
    synth = new Tone.PolySynth().toDestination();
}

function stopPlayback() {
    Tone.Transport.stop();
    Tone.Transport.cancel(0);
    synth.stop();
    synth.releaseAll();
}

function playNotes(midiNotes, delayMs) {
    init();
    Tone.Master.volume.value = volumeDb;
    let cur_delay = 300 / 1000;
    let notes = midiNotes.map(midiNote => Tone.Midi(midiNote).toNote());
    const now = Tone.now();
    for (var i = 0; i < notes.length; i++) {
        synth.triggerAttackRelease(notes[i], "8n", now + cur_delay);
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
    playChord(testNotes);
}

function testScale() {
    playScale(testNotes);
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