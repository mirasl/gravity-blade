using Godot;
using System;

public class Audio : Node
{
    AudioStreamPlayer[] starts = new AudioStreamPlayer[20];
    AudioStreamPlayer[] loops = new AudioStreamPlayer[20];
    AudioStreamPlayer[] slices = new AudioStreamPlayer[20];

    int index = 0;
    bool queuedShift = false;
    float previousPlaybackPosition = 0;


    public override void _Ready()
    {
        for (int i = 0; i < 20; i++)
        {
            starts[i] = GetNode<AudioStreamPlayer>("Starts/" + (i + 1));
            loops[i] = GetNode<AudioStreamPlayer>("Loops/" + (i + 1));
            slices[i] = GetNode<AudioStreamPlayer>("Slices/" + (i + 1));
        }
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("ui_accept"))
        {
            QueueNextChord();
        }

        // Check if loop audio just finished a loop:
        if (loops[index].Playing)
        {
            if (loops[index].GetPlaybackPosition() < previousPlaybackPosition)
            {
                sig_LoopFinished();
            }
            previousPlaybackPosition = loops[index].GetPlaybackPosition();
        }
    }

    public void QueueNextChord()
    {
        // play the slice immediately:
        int nextIndex = index + 1;
        if (nextIndex >= 20)
        {
            nextIndex = 0;
        }
        slices[index].Stop();
        slices[nextIndex].Play();

        if (!starts[index].Playing && !loops[index].Playing)
        {
            NextChord();
        }
        else
        {
            queuedShift = true;
        }
    }

    private void NextChord()
    {
        starts[index].Stop();
        loops[index].Stop();
        // slices[index].Stop();

        index++;
        if (index >= 20)
        {
            index = 0;
        }

        starts[index].Play();
        // slices[index].Play();
    }

    private void sig_StartFinished()
    {
        if (queuedShift)
        {
            queuedShift = false;
            NextChord();
        }
        else
        {
            loops[index].Play();
        }
    }

    private void sig_LoopFinished()
    {
        GD.Print("hihi");
        if (queuedShift)
        {
            queuedShift = false;
            NextChord();
        }
    }
}
