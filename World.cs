using Godot;
using System;

public class World : Spatial
{
    const float BASE_PLATFORM_DISTANCE = 140;
    const float BASE_ACCELERATOR_DISTANCE = 180;
    const float JUMP_HEIGHT = 8.33f;
    const int RING_INTERVAL = 3;

    public float ModulationAngle = 0; // from 0 to 1

    Player player;
    WorldEnvironment worldEnvironment;
    GlobalColors globalColors;
    PackedScene platformScene;
    PackedScene tunnelRingScene;

    Vector3 lastRingPoint = Vector3.Zero;


    public override void _Ready()
    {
        player = GetNode<Player>("Player");
        worldEnvironment = GetNode<WorldEnvironment>("WorldEnvironment");
        globalColors = GetNode<GlobalColors>("/root/GlobalColors");
        platformScene = GD.Load<PackedScene>("res://Platform.tscn");
        tunnelRingScene = GD.Load<PackedScene>("res://TunnelRing.tscn");

        GD.Randomize();

        Platform platform = GenerateRandomPlatform(new Vector3(0, -8.895f, 0), 0, 100, false);
        for (int i = 0; i < 10; i++)
        {
            // GD.Print(platform.Translation);
            platform = GenerateRandomPlatform(platform.Translation, platform.Rotation.z, 100, 
                    platform.IsAccelerator);
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        // GD.Print(ModulationAngle);
        
        // // Color primaryColor = (new Color(0.93f, 0.73f, 0.89f)).LinearInterpolate(
        // Color primaryColor = (new Color(1, 0.66f, 0.93f)).LinearInterpolate(
        //         new Color(0.15f, 0.08f, 0.27f), ModulationCoefficient);

        // Color secondaryColor = (new Color(1, 1, 1)).LinearInterpolate(
        //         new Color(0.21f, 0.16f, 0.57f), ModulationCoefficient);

        worldEnvironment.Environment.BackgroundSky.Set("sun_color", globalColors.bg1);
        worldEnvironment.Environment.BackgroundSky.Set("sky_top_color", globalColors.bg2);
    }

    // returns newly generated platform
    private Platform GenerateRandomPlatform(Vector3 currentPlatformTranslation, float 
            currentPlatformRotationZ, float speed, bool currentPlatformIsAccelerator)
    {
        // Random values:
        float theta = (int)(GD.Randf()*8)*Mathf.Pi/4 - Mathf.Pi;
        float deltaH = -GD.Randf()*30;

        // Other values:
        float distance = GetPlatformDistance(deltaH, speed, currentPlatformIsAccelerator);
        Vector3 fallDirection = Vector3.Down.Rotated(Vector3.Forward, -currentPlatformRotationZ);
        Vector3 axis = currentPlatformTranslation - fallDirection*JUMP_HEIGHT;
        axis = new Vector3(axis.x, axis.y, currentPlatformTranslation.z - distance);
        
        Platform platform = platformScene.Instance<Platform>();
        // platform.IsAccelerator = GD.Randi() % 2 == 0;
        platform.IsAccelerator = true;

        AddTunnelRings(axis);

        platform.Rotation = new Vector3(0, 0, currentPlatformRotationZ + theta);
        platform.Translation = axis;

        Vector3 rotationDirection = Vector3.Down.Rotated(Vector3.Forward, -currentPlatformRotationZ - theta);
        platform.Translation -= rotationDirection*(deltaH - JUMP_HEIGHT);

        AddChild(platform);

        return platform;
    }

    private float GetPlatformDistance(float deltaH, float speed, bool wasAccelerator)
    {
        float addend = wasAccelerator ? BASE_ACCELERATOR_DISTANCE : BASE_PLATFORM_DISTANCE;
        return speed*GetJumpTime(deltaH) + addend;
    }

    private float GetJumpTime(float deltaH)
    {
        float a = -Player.GRAVITY_MAGNITUDE*0.5f;
        float b = Player.JUMPFORCE;
        float c = -deltaH*60; // times delta???
        // quadratic formula:
        return (-20 - Mathf.Sqrt(b*b - 4*a*c)) / (2*a) / 60;
    }

    private void AddTunnelRings(Vector3 newPoint)
    {
        float iterations = lastRingPoint.DistanceTo(newPoint);
        for (int i = 0; i < iterations; i += RING_INTERVAL)
        {
            TunnelRing tunnelRing = tunnelRingScene.Instance<TunnelRing>();
            tunnelRing.Translation = lastRingPoint.MoveToward(newPoint, i);
            tunnelRing.player = player;
            AddChild(tunnelRing);
        }

        lastRingPoint = newPoint;
    }
}
