using Godot;
using System;

public class GravityWheel : Node2D
{
    [Signal] delegate void Timeout();

    public float GravityAngle = 0;
    // public float TimeAngle = 0; // from 0 to 1

    protected Sprite gravityCircleArrow;
    protected ColorRect colorRect;
    protected Timer timer;


    public override void _Ready()
    {
        gravityCircleArrow = GetNode<Sprite>("GravityCircleArrow");
        colorRect = GetNode<ColorRect>("ColorRect");
        timer = GetNode<Timer>("Timer");

        timer.Start();
    }

    public override void _PhysicsProcess(float delta)
    {
        gravityCircleArrow.Rotation = GravityAngle;
        colorRect.Material.Set("shader_param/timeAngle", 
                (1-timer.TimeLeft/timer.WaitTime) * Mathf.Pi*2);
    }

    public void Start()
    {
        timer.Start();
        colorRect.Material.Set("shader_param/timeColorGreen", false);
    }

    public void sig_TimerTimeout()
    {
        EmitSignal("Timeout");
    }

    public void Lock()
    {
        colorRect.Material.Set("shader_param/timeColorGreen", true);
        timer.Stop();
    }
}
