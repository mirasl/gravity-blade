using Godot;
using System;

public class StartScreen : Control
{
    [Export] Vector2 circleSelectPosition1 = Vector2.Zero;
    [Export] Vector2 circleSelectPosition2 = Vector2.Zero;
    [Export] Vector2 circleSelectPosition3 = Vector2.Zero;

    bool hackerModeUnlocked = false;

    enum Selection
    {
        NormalMode,
        HackerMode,
        Quit
    }
    Selection currentSelection = Selection.NormalMode;

    protected AnimatedSprite circleSelect;
    protected Label hackerMode;
    protected Label hackerModePopup;
    protected Godot.Node save;
    protected GlobalColors globalColors;
    protected Control labels;
    protected ColorRect background;


    public override void _Ready()
    {
        circleSelect = GetNode<AnimatedSprite>("Labels/CircleSelect");
        hackerMode = GetNode<Label>("Labels/HackerMode");
        hackerModePopup = GetNode<Label>("Labels/HackerModePopup");
        save = GetNode<Godot.Node>("/root/Save");
        globalColors = GetNode<GlobalColors>("/root/GlobalColors");
        labels = GetNode<Control>("Labels");
        background = GetNode<ColorRect>("Background");

        hackerModeUnlocked = (float)save.Call("load_game") >= 50000;
        hackerModePopup.Hide();
        if (!hackerModeUnlocked)
        {
            hackerMode.Modulate = new Color(1,1,1,0.2f);
            hackerModePopup.Show();
        }

        circleSelect.Position = circleSelectPosition1;
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
                currentSelection = Selection.NormalMode;
            }
            else
            {
                GetTree().Paused = false;
                Hide();
            }
        }

        if (Input.IsActionJustPressed("up"))
        {
            if (currentSelection == Selection.HackerMode)
            {
                circleSelect.Position = circleSelectPosition1;
                currentSelection = Selection.NormalMode;
            }
            else if (currentSelection == Selection.Quit)
            {
                if (hackerModeUnlocked)
                {
                    circleSelect.Position = circleSelectPosition2;
                    currentSelection = Selection.HackerMode;
                }
                else
                {
                    circleSelect.Position = circleSelectPosition1;
                    currentSelection = Selection.NormalMode;
                }
            }
        }
        else if (Input.IsActionJustPressed("down"))
        {
            if (currentSelection == Selection.NormalMode)
            {
                if (hackerModeUnlocked)
                {
                    circleSelect.Position = circleSelectPosition2;
                    currentSelection = Selection.HackerMode;
                }
                else
                {
                    circleSelect.Position = circleSelectPosition3;
                    currentSelection = Selection.Quit;
                }
            }
            else if (currentSelection == Selection.HackerMode)
            {
                circleSelect.Position = circleSelectPosition3;
                currentSelection = Selection.Quit;
            }
        }

        if (Input.IsActionJustPressed("ui_accept"))
        {
            GetTree().Paused = false;
            if (currentSelection == Selection.NormalMode)
            {
                GetTree().ChangeScene("res://World.tscn");
                globalColors.HackerMode = false;
            }
            else if (currentSelection == Selection.HackerMode)
            {
                GetTree().ChangeScene("res://World.tscn");
                globalColors.HackerMode = true;
            }
            else if (currentSelection == Selection.Quit)
            {
                GetTree().Quit();
            }
        }

        labels.Modulate = globalColors.text;
        background.Modulate = globalColors.bg2;
        circleSelect.Modulate = globalColors.text;
    }
}
