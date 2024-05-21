using Godot;
using System;

public class Projectile : KinematicBody
{
    public Vector3 Velocity = Vector3.Zero;

    public override void _PhysicsProcess(float delta)
    {
        Velocity = MoveAndSlide(Velocity);
    }
}
