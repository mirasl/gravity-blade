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
    protected CPUParticles2D arrowParticles;


    public override void _Ready()
    {
        gravityCircleArrow = GetNode<Sprite>("GravityCircleArrow");
        colorRect = GetNode<ColorRect>("ColorRect");
        timer = GetNode<Timer>("Timer");
        tween = GetNode<Tween>("Tween");
        arrowParticles = GetNode<CPUParticles2D>("GravityCircleArrow/CPUParticles2D");

        arrowParticles.Emitting = false;
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
        arrowParticles.Emitting = true;
        arrowParticles.SpeedScale = 6;
        overrideGravityAngle = false;
        timer.Start();
        colorRect.Material.Set("shader_param/timeColorGreen", false);
    }

    public void sig_TimerTimeout()
    {
        EmitSignal("Timeout");
    }

    public async void Lock()
    {
        arrowParticles.SpeedScale = 3;
        timer.Stop();
        overrideGravityAngle = true;
        colorRect.Material.Set("shader_param/timeColorGreen", true);

        tween.InterpolateProperty(gravityCircleArrow, "rotation", gravityCircleArrow.Rotation, 
                Mathf.Pi, 0.3f, Tween.TransitionType.Sine, Tween.EaseType.Out);
        tween.Start();

        await ToSignal(tween, "tween_completed");

        arrowParticles.Emitting = false;
    }

    public void SetWheelVisibility(bool visible)
    {
        colorRect.Visible = visible;
        float alpha = visible ? 1 : 0;
        gravityCircleArrow.SelfModulate = new Color(0, 0, 0, alpha);
    }
}
