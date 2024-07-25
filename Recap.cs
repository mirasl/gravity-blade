using Godot;
using System;

public class Recap : Control
{
    [Export] Vector2 circleSelectPosition1 = new Vector2();
    [Export] Vector2 circleSelectPosition2 = new Vector2();

    public bool Active = false;
    private bool hoveringRetry = true;

    protected AnimatedSprite circleSelect;
    protected GlobalColors globalColors;
    protected Control labels;
    protected AnimatedSprite frame;


    public override void _Ready()
    {
        circleSelect = GetNode<AnimatedSprite>("CircleSelect");
        globalColors = GetNode<GlobalColors>("/root/GlobalColors");
        labels = GetNode<Control>("Labels");
        frame = GetNode<AnimatedSprite>("Frame");
    }

    public override void _Process(float delta)
    {
        if (!Active)
        {
            Hide();
            return;
        }

        Show();
        circleSelect.Show();

        ProcessCircleSelectPosition();

        labels.Modulate = globalColors.text;
        frame.Modulate = globalColors.bg2;
    }

    private void ProcessCircleSelectPosition()
    {
        if (Input.IsActionJustPressed("right") && hoveringRetry)
        {
            hoveringRetry = false;
            circleSelect.Position = circleSelectPosition2;
        }
        else if (Input.IsActionJustPressed("left") && !hoveringRetry)
        {
            hoveringRetry = true;
            circleSelect.Position = circleSelectPosition1;
        }
    }
}
