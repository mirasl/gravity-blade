using Godot;
using System;

public class PlayerCamera : Camera
{
    float shakeStrength;
    float currentShakeTime;
    float totalShakeTime;


    public override void _Ready()
    {
        GD.Randomize();
    }

    public override void _PhysicsProcess(float delta)
    {
        currentShakeTime += delta;
        if (currentShakeTime < totalShakeTime)
        {
            HOffset = (0.5f - GD.Randf())*shakeStrength*2;
            VOffset = (0.5f - GD.Randf())*shakeStrength*2;    
        }
    }

    public void StartShake(float strength, float time)
    {
        shakeStrength = strength;
        totalShakeTime = time;
        currentShakeTime = 0;
    }
}
