using Godot;
using System;

public class World : Spatial
{
    const float BASE_PLATFORM_DISTANCE = 140;
    const float BASE_ACCELERATOR_DISTANCE = 180;
    const float JUMP_HEIGHT = 8.33f;
    const int RING_INTERVAL = 3;
    const float GENERATE_PLATFORM_DISTANCE = 1050; // distance from player to furthest platform at which we generate a new platform

    Platform lastGeneratedPlatform;

    Vector3 lastAxisPoint = Vector3.Zero;

    Player player;
    WorldEnvironment worldEnvironment;
    GlobalColors globalColors;
    DotsSpawner dotsSpawner;
    PackedScene platformScene;
    PackedScene tunnelRingScene;


    public override void _Ready()
    {
        player = GetNode<Player>("Player");
        worldEnvironment = GetNode<WorldEnvironment>("WorldEnvironment");
        globalColors = GetNode<GlobalColors>("/root/GlobalColors");
        dotsSpawner = GetNode<DotsSpawner>("DotsSpawner");
        platformScene = GD.Load<PackedScene>("res://Platform.tscn");
        tunnelRingScene = GD.Load<PackedScene>("res://TunnelRing.tscn");

        GD.Randomize();

        // Start with platforms:
        Platform platform = GenerateRandomPlatform(new Vector3(0, -8.895f, 0), 0, 100, false);
        while (lastAxisPoint.z < -GENERATE_PLATFORM_DISTANCE)
        {
            platform = GenerateRandomPlatform(platform.Translation, platform.Rotation.z, 100, 
                    platform.IsAccelerator);
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        worldEnvironment.Environment.BackgroundSky.Set("sun_color", globalColors.bg1);
        worldEnvironment.Environment.BackgroundSky.Set("sky_top_color", globalColors.bg2);

        if (player.Translation.z - lastGeneratedPlatform.Translation.z < GENERATE_PLATFORM_DISTANCE)
        {
            GenerateRandomPlatform(lastGeneratedPlatform.Translation, 
                    lastGeneratedPlatform.Rotation.z, 100, lastGeneratedPlatform.IsAccelerator);
        }
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
        AddDots(axis);
        lastAxisPoint = axis;

        platform.Rotation = new Vector3(0, 0, currentPlatformRotationZ + theta);
        platform.Translation = axis;

        Vector3 rotationDirection = Vector3.Down.Rotated(Vector3.Forward, 
                -currentPlatformRotationZ - theta);
        platform.Translation -= rotationDirection*(deltaH - JUMP_HEIGHT);

        AddChild(platform);

        lastGeneratedPlatform = platform;

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
        float iterations = lastAxisPoint.DistanceTo(newPoint);
        for (int i = 0; i < iterations; i += RING_INTERVAL)
        {
            TunnelRing tunnelRing = tunnelRingScene.Instance<TunnelRing>();
            tunnelRing.Translation = lastAxisPoint.MoveToward(newPoint, i);
            tunnelRing.player = player;
            tunnelRing.Connect("GameOver", this, "sig_GameOver");
            AddChild(tunnelRing);
        }
    }

    private void AddDots(Vector3 newPoint)
    {
        float iterations = lastAxisPoint.DistanceTo(newPoint);
        for (int i = 0; i < iterations; i++)
        {
            dotsSpawner.SpawnRandomDot(lastAxisPoint.MoveToward(newPoint, i));
        }
    }

    public void sig_GameOver()
    {
        GD.Print("GAME OVER");
        GetTree().Quit();
    }
}
