using Godot;
using System;

public class UI : Control
{
    AnimatedSprite uiFrame;
    Label scoreText;
    Label score;
    Label subScore;
    GlobalColors globalColors;

    public override void _Ready()
    {
        uiFrame = GetNode<AnimatedSprite>("UIFrame");
        scoreText = GetNode<Label>("ScoreText");
        score = GetNode<Label>("Score");
        subScore = GetNode<Label>("SubScore");
        globalColors = GetNode<GlobalColors>("/root/GlobalColors");
    }

    public override void _Process(float delta)
    {
        Color fgColor = globalColors.text;//Brighten(globalColors.fg);
        uiFrame.Modulate = globalColors.bg2;
        scoreText.Modulate = fgColor;
        score.Modulate = fgColor;
        subScore.Modulate = fgColor;
    }

    private Color Brighten(Color color)
    {
        return color.LinearInterpolate(Colors.White, 0.4f);
    }
}
