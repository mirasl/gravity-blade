using Godot;
using System;

public class Enemy : Spatial
{
    [Export] public float Radius = 1.866f;
    [Export] public Type CurrentType = Type.Horizontal;

    // const int FREEZE_FRAMES = 3;

    public Vector3 FallDirection = Vector3.Down;
    // int currentFreezeFrames = 0;
    public bool Dead = false;

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
    Particles explosionParticles;
    AnimationPlayer animationPlayer;

    PackedScene paintShotScene;


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
        explosionParticles = GetNode<Particles>("KinematicBody/Explosion/ExplosionParticles");
        animationPlayer = GetNode<AnimationPlayer>("KinematicBody/AnimationPlayer");

        paintShotScene = GD.Load<PackedScene>("res://PaintShot.tscn");

        sphere.Show();
        wings.Show();
        ringParticles.Show();
        explosion.Hide();
        explosionParticles.Emitting = false;

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

        // // run following code after other scripts:
        // await ToSignal(GetTree().CreateTimer(0), "timeout"); 

        // if (!globalColors.HackerMode)
        // {
        //     GetNode<MeshInstance>("KinematicBody/Wings/WingPivot/Wing/Grid").GetSurfaceMaterial(0).MakeUni
        //     GetNode<MeshInstance>("KinematicBody/Wings/WingPivot2/Wing/Grid");
            
        // }
    }

    public override void _Process(float delta)
    {
        // sphereMaterial.Set("albedo_color", globalColors.text);
        LookAt(Translation + Vector3.Back, -FallDirection);
        // if (currentFreezeFrames < FREEZE_FRAMES)
        // {
        //     GD.Print(currentFreezeFrames);
        //     currentFreezeFrames++;
        // }
        // else
        // {
        //     Engine.TimeScale = 1; // worst practice lolololol
        // }
    }

    public void Explode(float angle)
    {
        Dead = true;

        animationPlayer.Stop();

        // ringParticles.Hide();
        sphere.Hide();
        wings.Hide();

        explosion.Show();
        explosion.Rotation = Vector3.Back * angle;
        explosionAP.Play("split");
        explosionParticles.Emitting = true;
        
        // float baseHue = GD.Randf()*0.8f + 0.1f;
        for (int i = 0; i < 10; i++)
        {
            PaintShot ps = paintShotScene.Instance<PaintShot>();
            // ps.BaseHue = globalColors.enemy.h;
            explosion.AddChild(ps);
        }

        // Engine.TimeScale = 0.1f;
        // currentFreezeFrames = 0;

        // await ToSignal(GetTree().CreateTimer(0.05f), "timeout");

        // Engine.TimeScale = 1;
    }

    public void sig_ExplosionAnimationFinished(string animName)
    {
        if (animName == "split")
        {
            QueueFree();
        }
    }
}
