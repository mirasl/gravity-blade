using Godot;
using System;

public class World : Spatial
{
    const float BASE_JUMP_DISTANCE = 140;
    const float JUMP_HEIGHT = 8.33f;

    Player player;
    PackedScene platformScene;


    public override void _Ready()
    {
        player = GetNode<Player>("Player");
        platformScene = GD.Load<PackedScene>("res://Platform.tscn");

        GD.Randomize();

        Spatial platform = GenerateRandomPlatform(new Vector3(0, -8.895f, 0), 0, 100);
        for (int i = 0; i < 10; i++)
        {
            // GD.Print(platform.Translation);
            platform = GenerateRandomPlatform(platform.Translation, platform.Rotation.z, 100);
        }
    }

    // returns newly generated platform
    private Spatial GenerateRandomPlatform(Vector3 currentPlatformTranslation, float 
            currentPlatformRotationZ, float speed)
    {
        float theta = (int)(GD.Randf()*8)*Mathf.Pi/4 - Mathf.Pi;
        float deltaH = -GD.Randf()*30;
        float distance = GetPlatformDistance(deltaH, speed);
        Vector3 fallDirection = Vector3.Down.Rotated(Vector3.Forward, -currentPlatformRotationZ);
        Vector3 axis = currentPlatformTranslation - fallDirection*JUMP_HEIGHT;
        
        Spatial platform = platformScene.Instance<Spatial>();

        platform.Rotation = new Vector3(0, 0, currentPlatformRotationZ + theta);
        platform.Translation = new Vector3(axis.x, axis.y, currentPlatformTranslation.z - distance);

        Vector3 rotationDirection = Vector3.Down.Rotated(Vector3.Forward, -currentPlatformRotationZ - theta);
        platform.Translation -= rotationDirection*(deltaH - JUMP_HEIGHT);

        AddChild(platform);

        return platform;
    }

    private float GetPlatformDistance(float deltaH, float speed)
    {
        return speed*GetJumpTime(deltaH) + BASE_JUMP_DISTANCE;
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
