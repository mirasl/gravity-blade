using Godot;
using System;

public class World : Spatial
{
    const float BASE_PLATFORM_DISTANCE = 140;
    const float BASE_ACCELERATOR_DISTANCE = 180;
    const float JUMP_HEIGHT = 8.33f;
    const float GENERATE_PLATFORM_DISTANCE = 1050; // distance from player to furthest platform at which we generate a new platform
    
    const int RING_INTERVAL = 3;
    const int DOT_INTERVAL = 2;
    
    const float MIN_DELTA_H = 10;
    const float MAX_DELTA_H = 30;

    const float RAMP_SPEED = 200;
    const float BIG_RAMP_SPEED = 400;
    
    const float RAMP_AXIS_OFFSET = 5f; // distance from the top of the player's arc to the axis after leaving a ramp
    const float BIG_RAMP_AXIS_OFFSET = 15f; // distance from the top of the player's arc to the axis after leaving a big ramp
    
    const float RAMP_DEPTH = 20; // distance the player slides down when they go on a ramp
    const float BIG_RAMP_DEPTH = 60; // distance the player slides down when they go on a big ramp
    
    const float RAMP_LENGTH = 120;
    const float BIG_RAMP_LENGTH = 208;
    
    const float RAMP_PROBABILITY = 0.25f;
    const float BIG_RAMP_PROBABILITY = 0.5f;
    

    float currentSpeed = 100;

    Platform lastGeneratedPlatform;

    Vector3 lastAxisPoint = Vector3.Zero;

    Player player;
    WorldEnvironment worldEnvironment;
    GlobalColors globalColors;
    DotsSpawner dotsSpawner;

    PackedScene platformScene;
    PackedScene rampScene;
    PackedScene bigRampScene;
    PackedScene tunnelRingScene;


    public override void _Ready()
    {
        player = GetNode<Player>("Player");
        worldEnvironment = GetNode<WorldEnvironment>("WorldEnvironment");
        globalColors = GetNode<GlobalColors>("/root/GlobalColors");
        dotsSpawner = GetNode<DotsSpawner>("DotsSpawner");

        platformScene = GD.Load<PackedScene>("res://Platform.tscn");
        rampScene = GD.Load<PackedScene>("res://Ramp.tscn");
        bigRampScene = GD.Load<PackedScene>("res://BigRamp.tscn");
        tunnelRingScene = GD.Load<PackedScene>("res://TunnelRing.tscn");

        GD.Randomize();

        // Start with platforms:
        Platform platform = GenerateRandomPlatform(new Vector3(0, -8.895f, 0), 0, false);
        while (lastAxisPoint.z < -GENERATE_PLATFORM_DISTANCE)
        {
            platform = GenerateRandomPlatform(platform.Translation, platform.Rotation.z, 
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
                    lastGeneratedPlatform.Rotation.z, lastGeneratedPlatform.IsAccelerator);
        }
    }

    // returns newly generated platform
    private Platform GenerateRandomPlatform(Vector3 currentPlatformTranslation, float 
            currentPlatformRotationZ, bool currentPlatformIsAccelerator)
    {
        // Random values:
        float theta = (int)(GD.Randf()*8)*Mathf.Pi/4 - Mathf.Pi;
        float deltaH = -GD.Randf()*(MAX_DELTA_H - MIN_DELTA_H) - MIN_DELTA_H;
        float rampSeed = GD.Randf();

        // Other values:
        float distance = GetPlatformDistance(deltaH, currentSpeed, currentPlatformIsAccelerator);
        Vector3 fallDirection = Vector3.Down.Rotated(Vector3.Forward, -currentPlatformRotationZ);
        Vector3 axis = currentPlatformTranslation - fallDirection*JUMP_HEIGHT;
        axis = new Vector3(axis.x, axis.y, currentPlatformTranslation.z - distance);
        
        // Assign platform to platform, ramp, bigRamp (based on rampSeed):
        Platform platform;
        if (rampSeed < BIG_RAMP_PROBABILITY) // BIG RAMP
        {
            platform = bigRampScene.Instance<Platform>();
            currentSpeed = BIG_RAMP_SPEED;
            // Subtract from axis in the fall direction (accounts for end ramp momentum):
            axis += fallDirection*BIG_RAMP_AXIS_OFFSET;
        }
        else if (rampSeed < BIG_RAMP_PROBABILITY + RAMP_PROBABILITY) // RAMP
        {
            platform = rampScene.Instance<Platform>();
            currentSpeed = RAMP_SPEED;
            // Subtract from axis in the fall direction (accounts for end ramp momentum):
            axis += fallDirection*RAMP_AXIS_OFFSET;
        }
        else // PLATFORM
        {
            platform = platformScene.Instance<Platform>();
            currentSpeed = 100;
        }
        // platform.IsAccelerator = GD.Randi() % 2 == 0;
        platform.IsAccelerator = true;


        AddTunnelRings(axis);
        AddDots(axis);
        lastAxisPoint = axis;
        
        Vector3 newFallDirection = Vector3.Down.Rotated(Vector3.Forward, 
                -currentPlatformRotationZ - theta);

        // RINGS FOLLOW RAMPS DOWNWARD:
        if (rampSeed < BIG_RAMP_PROBABILITY) // BIG RAMP
        {
            Vector3 newAxis = axis + (new Vector3(BIG_RAMP_DEPTH*newFallDirection.x, 
                    BIG_RAMP_DEPTH*newFallDirection.y, -BIG_RAMP_LENGTH));
            AddTunnelRings(newAxis);
            AddDots(newAxis);
            lastAxisPoint = newAxis;
        }
        else if (rampSeed < BIG_RAMP_PROBABILITY + RAMP_PROBABILITY) // RAMP
        {
            Vector3 newAxis = axis + (new Vector3(RAMP_DEPTH*newFallDirection.x, 
                    RAMP_DEPTH*newFallDirection.y, -RAMP_LENGTH));
            AddTunnelRings(newAxis);
            AddDots(newAxis);
            lastAxisPoint = newAxis;
        }

        platform.Rotation = new Vector3(0, 0, currentPlatformRotationZ + theta);
        platform.Translation = axis;
        platform.Translation -= newFallDirection*(deltaH - JUMP_HEIGHT);

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
        for (int i = 0; i < iterations; i += DOT_INTERVAL)
        {
            dotsSpawner.SpawnRandomDot(lastAxisPoint.MoveToward(newPoint, i));
        }
    }

    public void sig_GameOver()
    {
        /// !!! make an actual game over
        GetTree().ReloadCurrentScene();
    }
}
