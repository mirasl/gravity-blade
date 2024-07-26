using Godot;
using System;

public class Pause : Control
{
    [Signal] delegate void Restart();
    [Signal] delegate void Quit();

    [Export] Vector2 circleSelectPosition1 = new Vector2();
    [Export] Vector2 circleSelectPosition2 = new Vector2();
    [Export] Vector2 circleSelectPosition3 = new Vector2();

    enum Selection
    {
        Resume,
        Restart,
        Quit
    }
    Selection currentSelection = Selection.Resume;

    public bool Pausable = true;

    protected AnimatedSprite circleSelect;
    protected GlobalColors globalColors;
    protected Control labels;
    protected ColorRect background;


    public override void _Ready()
    {
        circleSelect = GetNode<AnimatedSprite>("CircleSelect");
        globalColors = GetNode<GlobalColors>("/root/GlobalColors");
        labels = GetNode<Control>("Labels");
        background = GetNode<ColorRect>("Background");

        circleSelect.Position = circleSelectPosition1;

        Hide();
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("pause") && Pausable)
        {
            if (!GetTree().Paused)
            {
                GetTree().Paused = true;
                Show();
                circleSelect.Position = circleSelectPosition1;
                currentSelection = Selection.Resume;
            }
            else
            {
                GetTree().Paused = false;
                Hide();
            }
        }

        if (Input.IsActionJustPressed("up"))
        {
            if (currentSelection == Selection.Restart)
            {
                circleSelect.Position = circleSelectPosition1;
                currentSelection = Selection.Resume;
            }
            else if (currentSelection == Selection.Quit)
            {
                circleSelect.Position = circleSelectPosition2;
                currentSelection = Selection.Restart;
            }
        }
        else if (Input.IsActionJustPressed("down"))
        {
            if (currentSelection == Selection.Resume)
            {
                circleSelect.Position = circleSelectPosition2;
                currentSelection = Selection.Restart;
            }
            else if (currentSelection == Selection.Restart)
            {
                circleSelect.Position = circleSelectPosition3;
                currentSelection = Selection.Quit;
            }
        }

        if (Input.IsActionJustPressed("ui_accept") && GetTree().Paused)
        {
            GetTree().Paused = false;
            if (currentSelection == Selection.Resume)
            {
                Hide();
            }
            else if (currentSelection == Selection.Restart)
            {
                EmitSignal("Restart");
            }
            else if (currentSelection == Selection.Quit)
            {
                EmitSignal("Quit");
            }
        }

        labels.Modulate = globalColors.text;
        background.Modulate = globalColors.bg2;
        circleSelect.Modulate = globalColors.text;
    }
}
