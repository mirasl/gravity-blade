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

    Material ringParticlesProcessMaterial;
    Material centerParticlesProcessMaterial;
    Material sphereMaterial;
    GlobalColors globalColors;


    public override void _Ready()
    {
        ringParticlesProcessMaterial = GetNode<Particles>(
                "KinematicBody/RingParticles").ProcessMaterial;
        centerParticlesProcessMaterial = GetNode<Particles>(
                "KinematicBody/CenterParticles").ProcessMaterial;
        sphereMaterial = GetNode<MeshInstance>(
                "KinematicBody/Sphere").GetSurfaceMaterial(0);
        globalColors = GetNode<GlobalColors>("/root/GlobalColors");

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

    public override void _Process(float delta)
    {
        ringParticlesProcessMaterial.Set("color", globalColors.text);
        centerParticlesProcessMaterial.Set("color", globalColors.text);
        Color darkenedColor = globalColors.text;
        // darkenedColor.v = 0.835f;
        sphereMaterial.Set("albedo_color", darkenedColor);
    }
}
