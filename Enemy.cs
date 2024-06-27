using Godot;
using System;

public class Enemy : Spatial
{
    [Export] public float Radius = 1.866f;
    [Export] public Type CurrentType = Type.Horizontal;

    public enum Type 
    {
        Horizontal,
        Vertical,
        Circle,
        None
    }


    public override void _Ready()
    {
        AnimationPlayer ap = GetNode<AnimationPlayer>("KinematicBody/AnimationPlayer");
        switch (CurrentType)
        {
            case (Type.Horizontal):
                ap.Play("horizontal");
                break;
            case (Type.Vertical):
                ap.Play("vertical");
                break;
            case (Type.Circle):
                ap.Play("circle");
                break;
            case (Type.None):
                ap.Stop();
                break;
        }
    }
}
