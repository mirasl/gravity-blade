using Godot;
using System;

public class Enemy : Spatial
{
    [Export] public float Radius = 1.866f;
    [Export] public Type CurrentType = Type.Horizontal;

    public Vector3 FallDirection = Vector3.Down;

    public enum Type 
    {
        Horizontal,
        Vertical,
        Circle,
        None
    }

    // Material ringParticlesProcessMaterial;
    // Material centerParticlesProcessMaterial;
    Material sphereMaterial;
    GlobalColors globalColors;
    MeshInstance sphere;
    Spatial wings;
    Particles ringParticles;
    Spatial explosion;
    AnimationPlayer explosionAP;


    public override void _Ready()
    {
        // ringParticlesProcessMaterial = GetNode<Particles>(
        //         "KinematicBody/RingParticles").ProcessMaterial;
        // centerParticlesProcessMaterial = GetNode<Particles>(
        //         "KinematicBody/CenterParticles").ProcessMaterial;
        sphereMaterial = GetNode<MeshInstance>(
                "KinematicBody/Sphere").GetSurfaceMaterial(0);
        globalColors = GetNode<GlobalColors>("/root/GlobalColors");
        sphere = GetNode<MeshInstance>("KinematicBody/Sphere");
        wings = GetNode<Spatial>("KinematicBody/Wings");
        ringParticles = GetNode<Particles>("KinematicBody/RingParticles");
        explosion = GetNode<Spatial>("KinematicBody/Explosion");
        explosionAP = GetNode<AnimationPlayer>("KinematicBody/Explosion/AnimationPlayer");

        explosion.Hide();

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
        // sphereMaterial.Set("albedo_color", globalColors.text);
        LookAt(Translation + Vector3.Back, -FallDirection);
    }

    public void Explode(float angle)
    {
        ringParticles.Hide();
        sphere.Hide();
        wings.Hide();

        explosion.Show();
        explosion.Rotation = Vector3.Back * angle;
        explosionAP.Play("split");
    }
}
