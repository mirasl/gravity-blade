using Godot;
using System;

public class Audio : Node
{
    AudioStreamPlayer[] starts = new AudioStreamPlayer[20];
    AudioStreamPlayer[] loops = new AudioStreamPlayer[20];
    AudioStreamPlayer[] slices = new AudioStreamPlayer[20];

    int index = 0;


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
            NextChord();
        }
    }

    public void NextChord()
    {
        starts[index].Stop();
        loops[index].Stop();
        slices[index].Stop();

        index++;
        if (index >= 20)
        {
            index = 0;
        }

        starts[index].Play();
        slices[index].Play();

    }

    private void sig_StartFinished()
    {
        loops[index].Play();
    }
}
