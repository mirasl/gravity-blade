using Godot;
using System;

public class World : Spatial
{
    const float PLATFORM_LENGTH = 87;
    Player player;
    PackedScene platformScene;
    PackedScene testScene;


    public override void _Ready()
    {
        player = GetNode<Player>("Player");
        platformScene = GD.Load<PackedScene>("res://Platform.tscn");
        testScene = GD.Load<PackedScene>("res://Test.tscn");

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
        GD.Print(fallDirection);
        Vector3 axis = currentPlatformTranslation - fallDirection*8.33f;
        
        Spatial platform = platformScene.Instance<Spatial>();
        MeshInstance test = testScene.Instance<MeshInstance>();

        platform.Rotation = new Vector3(0, 0, currentPlatformRotationZ + theta);
        platform.Translation = new Vector3(axis.x, axis.y, currentPlatformTranslation.z - distance);

        test.Translation = platform.Translation;
        AddChild(test);

        Vector3 rotationDirection = Vector3.Down.Rotated(Vector3.Forward, -theta);
        platform.Translation -= rotationDirection*(deltaH - 8.33f);
        GD.Print(theta * 180 / Mathf.Pi);
        // platform.Rotate(Vector3.Forward, currentPlatformRotationZ + theta);
        // GD.Print(platform.Translation);
        // platform.Translation = platform.Translation.MoveToward(), deltaH);
        // GD.Print(platform.Translation);
        // GD.Print();
        AddChild(platform);

        return platform;
    }

    private float GetPlatformDistance(float deltaH, float speed)
    {
        return speed*GetJumpTime(deltaH) + PLATFORM_LENGTH;
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
