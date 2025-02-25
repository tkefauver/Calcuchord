using System;
using System.IO;
using System.Collections.Generic;
using CoreMidi;
using AudioToolbox;
using Foundation;
// using MonoMac.CoreMidi;
// using MonoMac.Foundation;

namespace Calcuchord.Mac {
    
public class MidiPlayer_mac : MidiPlayerBase {
    private MidiClient _midiClient;
    private MusicSequence _musicSequence;
    private MusicPlayer _musicPlayer;
    

    public MidiPlayer_mac() {
        InitializeMidi();
    }

    private void InitializeMidi()
    {
        // Create a MIDI client
        _midiClient = new MidiClient("MIDIClient");

        // Create a MusicSequence for playing MIDI files
        _musicSequence = new MusicSequence();
        _musicPlayer = new MusicPlayer();
    }

    void PlayNotes(IEnumerable<int> notes, int delayMs) {
        // Create a new MusicSequence
        MusicSequence sequence = new MusicSequence();

        // Create a new track in the sequence
        MusicTrack track = sequence.CreateTrack();

        // Set the tempo (beats per minute)
        double bpm = 120d / 60.0;
        track.AddExtendedTempoEvent(0, bpm);

        // Add notes to the track
        double dt = 0.0;
        double noteDuration = 1.0; // Duration of each note (in beats)

        foreach (int note in notes)
        {
            track.AddMidiNoteEvent(
                dt,
                new MidiNoteMessage(0,(byte)note,127,0,(float)noteDuration));
            dt += noteDuration + (double)delayMs;
        }

        string filePath = Path.Combine(PlatformWrapper.Services.StorageHelper.StorageDir, "output.mid");

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        // Save the sequence as a MIDI file
        NSUrl fileUrl = NSUrl.FromFilename(filePath);
        //sequence.Save(fileUrl, MusicSequenceFileTypeID.Midi);
        sequence.CreateFile(fileUrl, MusicSequenceFileTypeID.Midi);
        
        PlayMidiFile(filePath);
    }

    void PlayMidiFile(string midiFilePath)
    {
        try
        {
            // Load the MIDI file into the MusicSequence
            NSUrl midiFileUrl = NSUrl.FromFilename(midiFilePath);
            _musicSequence.LoadFile(midiFileUrl, MusicSequenceFileTypeID.Midi);

            // Set the MusicSequence to the MusicPlayer
            _musicPlayer.MusicSequence = _musicSequence;

            // Start playback
            _musicPlayer.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error playing MIDI file: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _musicPlayer?.Stop();
        _musicPlayer?.Dispose();
        _musicSequence?.Dispose();
        _midiClient?.Dispose();
    }

    public override bool CanPlay => true;

    public override void PlayChord(IEnumerable<Note> notes) {
        PlayNotes(GetMidiNotes(notes),1);
    }

    public override void PlayScale(IEnumerable<Note> notes) {
        PlayNotes(GetMidiNotes(notes),1);
    }
}
}