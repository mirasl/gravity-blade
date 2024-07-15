using Godot;
using System;

public class PaintShot : Spatial
{
    // public float BaseHue;

    Material processMaterial;
    GlobalColors globalColors;
    AnimationPlayer animationPlayer;


    public override void _Ready()
    {
        GD.Randomize();
        globalColors = GetNode<GlobalColors>("/root/GlobalColors");
        processMaterial = GetNode<Particles>("Shot").ProcessMaterial;
        animationPlayer = GetNode<AnimationPlayer>("Shot/AnimationPlayer");
        // processMaterial.Set("hue_variation", BaseHue + (GD.Randf() - 0.5f)*0.4f);
        Rotation = new Vector3((0.5f + GD.Randf())*Mathf.Pi, GD.Randf()*Mathf.Pi*2, 
                (0.5f + GD.Randf())*Mathf.Pi);
    }

    public override void _Process(float delta)
    {
        Color color = globalColors.enemy;
        color.s = 1;
        color.a = animationPlayer.CurrentAnimationPosition / animationPlayer.CurrentAnimationLength;
        processMaterial.Set("color", color);
    }
}
