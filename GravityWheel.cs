using Godot;
using System;

public class GravityWheel : Node2D
{
    [Signal] delegate void Timeout();

    public float GravityAngle = 0;
    // public float TimeAngle = 0; // from 0 to 1

    bool overrideGravityAngle = false;

    protected Sprite gravityCircleArrow;
    protected ColorRect colorRect;
    protected Timer timer;
    protected Tween tween;


    public override void _Ready()
    {
        gravityCircleArrow = GetNode<Sprite>("GravityCircleArrow");
        colorRect = GetNode<ColorRect>("ColorRect");
        timer = GetNode<Timer>("Timer");
        tween = GetNode<Tween>("Tween");
    }

    public override void _PhysicsProcess(float delta)
    {
        if (!overrideGravityAngle)
        {
            gravityCircleArrow.Rotation = GravityAngle;
        }
        colorRect.Material.Set("shader_param/timeAngle", 
                (1-timer.TimeLeft/timer.WaitTime) * Mathf.Pi*2);
    }

    public void Start()
    {
        overrideGravityAngle = false;
        timer.Start();
        colorRect.Material.Set("shader_param/timeColorGreen", false);
    }

    public void sig_TimerTimeout()
    {
        EmitSignal("Timeout");
    }

    public void Lock()
    {
        overrideGravityAngle = true;
        colorRect.Material.Set("shader_param/timeColorGreen", true);
        tween.InterpolateProperty(gravityCircleArrow, "rotation", gravityCircleArrow.Rotation, 
                Mathf.Pi, 0.3f, Tween.TransitionType.Sine, Tween.EaseType.Out);
        tween.Start();
        timer.Stop();
    }
}
