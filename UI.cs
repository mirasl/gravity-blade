using Godot;
using System;
using System.Collections.Generic;

public class UI : Control
{
    const float SUB_SCORE_Y_INTERVAL = 84;
    const float SUB_SCORE_WAIT_TIME = 0.4f;

    float displayScore = 0;
    public float DistanceScore = 0;
    float addedScore = 0;
    List<SubScore> subScores = new List<SubScore>{};

    AnimatedSprite uiFrame;
    Label scoreText;
    Label scoreNumber;
    GlobalColors globalColors;
    Control subScoresControl;
    Timer subScoreTimer;
    AnimationPlayer subScoreAP;
    Control go;

    PackedScene subScoreScene;

    public override void _Ready()
    {
        uiFrame = GetNode<AnimatedSprite>("UIFrame");
        scoreText = GetNode<Label>("ScoreText");
        scoreNumber = GetNode<Label>("ScoreNumber");
        globalColors = GetNode<GlobalColors>("/root/GlobalColors");
        subScoresControl = GetNode<Control>("SubScores");
        subScoreTimer = GetNode<Timer>("SubScoreTimer");
        subScoreAP = GetNode<AnimationPlayer>("SubScores/AnimationPlayer");
        go = GetNode<Control>("Go");

        subScoreScene = GD.Load<PackedScene>("res://SubScore.tscn");

        AnimatedSprite goAS = GetNode<AnimatedSprite>("Go/AnimatedSprite");
        goAS.Frame = 0;
        goAS.Play("complete");
    }

    public override void _Process(float delta)
    {
        Color fgColor = globalColors.text;//Brighten(globalColors.fg);
        uiFrame.Modulate = globalColors.bg2;
        scoreText.Modulate = fgColor;
        scoreNumber.Modulate = fgColor;
        foreach (SubScore subScore in subScores)
        {
            subScore.Modulate = fgColor;
        }
        if (go.Visible)
        {
            go.Modulate = globalColors.fg;
        }
        // subScore.Modulate = fgColor;

        // if (Input.IsActionJustPressed("jump"))
        // {
        //     AddScoreBonus(50, "+ slide ");
        // }

        scoreNumber.Text = (DistanceScore + addedScore).ToString("000000");
    }

    private void sig_GoAnimationFinished(string animName)
    {
        if (animName == "go")
        {
            go.Hide();
        }
    }

    public void AddScoreBonus(float bonus, string text)
    {
        SubScore subScore = subScoreScene.Instance<SubScore>();
        subScore.Value = bonus;

        subScoresControl.AddChild(subScore);
        subScore.RectPosition = Vector2.Down * subScores.Count * SUB_SCORE_Y_INTERVAL;

        subScores.Add(subScore);
        
        subScore.Text = text + bonus;

        // ahh so professional, awaits are for losers :P
        SceneTreeTimer stt = GetTree().CreateTimer(SUB_SCORE_WAIT_TIME);
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
            addedScore += subScores[0].Value;
            subScores[0].QueueFree();
            subScores.RemoveAt(0);
            subScoreAP.Play("RESET");
            foreach (SubScore subScore in subScores)
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
