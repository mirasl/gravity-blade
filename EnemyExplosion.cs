using Godot;
using System;

public class EnemyExplosion : AnimatedSprite
{
    public override void _Ready()
    {
        Frame = 0;
        Playing = true;
    }

    public void sig_AnimationFinished()
    {
        QueueFree();
    }
}
