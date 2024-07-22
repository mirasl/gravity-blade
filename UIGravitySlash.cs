using Godot;
using System;

public class UIGravitySlash : AnimatedSprite
{
    public override void _Ready()
    {
        Play("default");
    }

    public void sig_AnimationFinished()
    {
        if (Animation == "enter")
        {
            Play("default");
        }
    }
}
