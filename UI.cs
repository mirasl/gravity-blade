using Godot;
using System;
using System.Collections.Generic;

public class UI : Control
{
    const float SUB_SCORE_Y_INTERVAL = 84;

    float displayScore = 0;
    float playerDistance = 0;
    float addedScore = 0;
    List<Label> subScores = new List<Label>{};

    AnimatedSprite uiFrame;
    Label scoreText;
    Label scoreNumber;
    // Label subScore;
    GlobalColors globalColors;
    Control subScoresControl;
    Timer subScoreTimer;
    AnimationPlayer subScoreAP;

    PackedScene subScoreScene;

    public override void _Ready()
    {
        uiFrame = GetNode<AnimatedSprite>("UIFrame");
        scoreText = GetNode<Label>("ScoreText");
        scoreNumber = GetNode<Label>("ScoreNumber");
        // subScore = GetNode<Label>("SubScore");
        globalColors = GetNode<GlobalColors>("/root/GlobalColors");
        subScoresControl = GetNode<Control>("SubScores");
        subScoreTimer = GetNode<Timer>("SubScoreTimer");
        subScoreAP = GetNode<AnimationPlayer>("SubScores/AnimationPlayer");

        subScoreScene = GD.Load<PackedScene>("res://SubScore.tscn");
    }

    public override void _Process(float delta)
    {
        Color fgColor = globalColors.text;//Brighten(globalColors.fg);
        uiFrame.Modulate = globalColors.bg2;
        scoreText.Modulate = fgColor;
        scoreNumber.Modulate = fgColor;
        foreach (Label subScore in subScores)
        {
            subScore.Modulate = fgColor;
        }
        // subScore.Modulate = fgColor;

        if (Input.IsActionJustPressed("jump"))
        {
            AddScoreBonus(50, "+ slide ");
        }

        displayScore = playerDistance + addedScore;

        scoreNumber.Text = displayScore.ToString("000000");
    }

    public void AddScoreBonus(float bonus, string text)
    {
        Label subScore = subScoreScene.Instance<Label>();

        subScoresControl.AddChild(subScore);
        subScore.RectPosition = Vector2.Down * subScores.Count * SUB_SCORE_Y_INTERVAL;

        subScores.Add(subScore);
        
        subScore.Text = text + bonus;

        SceneTreeTimer stt = GetTree().CreateTimer(1);
        stt.Connect("timeout", this, "sig_SubScoreTimerTimeout");

        // await ToSignal(GetTree().CreateTimer(1), "timeout");

        // sig_SubScoreTimerTimeout();

        // if (true || subScores.Count == 1)
        // {
        //     subScoreTimer.Start();
        // }
    }

    public void sig_SubScoreTimerTimeout()
    {
        if (subScoreAP.IsPlaying())
        {
            // Force it to finish
            sig_SubScoreAPAnimationFinished("pop");
        }
        subScoreAP.Play("pop");
        // if (subScores.Count > 1)
        // {
        //     subScoreTimer.Start();
        // }
    }

    public void sig_SubScoreAPAnimationFinished(string animName)
    {
        if (animName == "pop")
        {
            subScores[0].QueueFree();
            subScores.RemoveAt(0);
            subScoreAP.Play("RESET");
            foreach (Label subScore in subScores)
            {
                subScore.RectPosition += Vector2.Up * SUB_SCORE_Y_INTERVAL;
            }
        }
    }
    private Color Brighten(Color color)
    {
        return color.LinearInterpolate(Colors.White, 0.4f);
    }
}
