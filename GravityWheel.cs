using Godot;
using System;

public class GravityWheel : Node2D
{
    public float GravityAngle = 0;

    protected Sprite gravityCircleArrow;


    public override void _Ready()
    {
        gravityCircleArrow = GetNode<Sprite>("GravityCircleArrow");
    }

    public override void _PhysicsProcess(float delta)
    {
        gravityCircleArrow.Rotation = GravityAngle;
    }
}
