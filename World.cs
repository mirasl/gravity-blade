using Godot;
using System;

public class World : Spatial
{
    const float BASE_PLATFORM_DISTANCE = 140;
    const float BASE_ACCELERATOR_DISTANCE = 140;
    const float JUMP_HEIGHT = 8.33f;

    Player player;
    PackedScene platformScene;


    public override void _Ready()
    {
        player = GetNode<Player>("Player");
        platformScene = GD.Load<PackedScene>("res://Platform.tscn");

        GD.Randomize();

        Platform platform = GenerateRandomPlatform(new Vector3(0, -8.895f, 0), 0, 100, false);
        for (int i = 0; i < 10; i++)
        {
            // GD.Print(platform.Translation);
            platform = GenerateRandomPlatform(platform.Translation, platform.Rotation.z, 100, 
                    platform.IsAccelerator);
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
        
        Platform platform = platformScene.Instance<Platform>();
        platform.IsAccelerator = GD.Randi() % 2 == 0;

        platform.Rotation = new Vector3(0, 0, currentPlatformRotationZ + theta);
        platform.Translation = new Vector3(axis.x, axis.y, currentPlatformTranslation.z - distance);

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
}
