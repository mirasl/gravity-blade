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

    protected AnimatedSprite circleSelect;


    public override void _Ready()
    {
        circleSelect = GetNode<AnimatedSprite>("CircleSelect");
        circleSelect.Position = circleSelectPosition1;

        Hide();
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("pause"))
        {
            if (!GetTree().Paused)
            {
                GetTree().Paused = true;
                Show();
                circleSelect.Position = circleSelectPosition1;
                currentSelection = Selection.Resume;
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
        if (Input.IsActionJustPressed("down"))
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
    }
}
