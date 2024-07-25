using Godot;
using System;

public class Recap : Control
{
    [Signal] delegate void Retry();
    [Signal] delegate void Quit();

    [Export] Vector2 circleSelectPosition1 = new Vector2();
    [Export] Vector2 circleSelectPosition2 = new Vector2();

    private bool active = false;
    private bool hoveringRetry = true;

    protected AnimatedSprite circleSelect;
    protected GlobalColors globalColors;
    protected Control labels;
    protected AnimatedSprite frame;
    protected ColorRect background;
    protected AnimationPlayer backgroundAP;
    protected Label scoreNumber;
    protected Label highScoreNumber;


    public override void _Ready()
    {
        circleSelect = GetNode<AnimatedSprite>("CircleSelect");
        globalColors = GetNode<GlobalColors>("/root/GlobalColors");
        labels = GetNode<Control>("Labels");
        frame = GetNode<AnimatedSprite>("Frame");
        background = GetNode<ColorRect>("Background");
        backgroundAP = GetNode<AnimationPlayer>("Background/AnimationPlayer");
        scoreNumber = GetNode<Label>("Labels/ScoreNumber");
        highScoreNumber = GetNode<Label>("Labels/HighScoreNumber");
    }

    public override void _Process(float delta)
    {
        if (!active)
        {
            Hide();
            return;
        }

        Show();
        circleSelect.Show();

        labels.Modulate = globalColors.text;
        frame.Modulate = globalColors.bg2;
        background.Modulate = globalColors.bg2;

        ProcessCircleSelectPosition();

        if (Input.IsActionJustPressed("ui_accept"))
        {
            if (hoveringRetry)
            {
                EmitSignal("Retry");
            }
            else
            {
                EmitSignal("Quit");
            }
        }
    }

    public void SetActive(float score, float highScore)
    {
        scoreNumber.Text = score.ToString("000000");
        highScoreNumber.Text = highScore.ToString("000000");
        active = true;
        backgroundAP.Play("slide");
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
