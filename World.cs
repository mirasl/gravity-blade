using Godot;
using System;

public class World : Spatial
{
    [Export] bool HackerMode = false;

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

    const float ENEMY_HORIZONTAL_PROBABILITY = 0.2f;
    const float ENEMY_VERTICAL_PROBABILITY = 0.2f;
    const float ENEMY_CIRCLE_PROBABILITY = 0.2f;


    float currentSpeed = 100;
    public float HighScore = 0;
    bool gameOverCalled = false;
    bool quittingWhenTransitionFinished = false;

    Platform lastGeneratedPlatform;

    Vector3 lastAxisPoint = Vector3.Zero;

    protected Player player;
    protected WorldEnvironment worldEnvironment;
    protected GlobalColors globalColors;
    protected DotsSpawner dotsSpawner;
    protected Spatial enemies;
    protected UI ui;
    protected Spatial platforms;
    protected GravitySlashes gravitySlashes;
    protected Godot.Node save;
    protected Recap recap;
    protected Audio audio;
    protected Pause pause;
    protected AnimationPlayer transitionAP;

    protected PackedScene platformScene;
    protected PackedScene rampScene;
    protected PackedScene bigRampScene;
    protected PackedScene tunnelRingScene;
    protected PackedScene enemyScene;


    public override void _Ready()
    {
        player = GetNode<Player>("Player");
        worldEnvironment = GetNode<WorldEnvironment>("WorldEnvironment");
        globalColors = GetNode<GlobalColors>("/root/GlobalColors");
        dotsSpawner = GetNode<DotsSpawner>("DotsSpawner");
        enemies = GetNode<Spatial>("Enemies");
        ui = GetNode<UI>("UI");
        platforms = GetNode<Spatial>("Platforms");
        gravitySlashes = GetNode<GravitySlashes>("UI/GravitySlashes");
        save = GetNode<Godot.Node>("/root/Save");
        recap = GetNode<Recap>("Recap");
        audio = GetNode<Audio>("/root/Audio");
        pause = GetNode<Pause>("Pause");
        transitionAP = GetNode<AnimationPlayer>("Transition/AnimationPlayer");

        platformScene = GD.Load<PackedScene>("res://Platform.tscn");
        rampScene = GD.Load<PackedScene>("res://Ramp.tscn");
        bigRampScene = GD.Load<PackedScene>("res://BigRamp.tscn");
        tunnelRingScene = GD.Load<PackedScene>("res://TunnelRing.tscn");
        enemyScene = GD.Load<PackedScene>("res://Enemy.tscn");

        GD.Randomize();

        // Start with platforms:
        lastGeneratedPlatform = new Platform();
        Platform platform = GenerateRandomPlatform(new Vector3(0, -8.895f, 0), 0, false, false);
        while (lastAxisPoint.z > -GENERATE_PLATFORM_DISTANCE)
        {
            GenerateRandomPlatform(lastGeneratedPlatform.Translation, 
                    lastGeneratedPlatform.Rotation.z, lastGeneratedPlatform.IsAccelerator, false);
        }

        HackerMode = globalColors.HackerMode;

        if (HackerMode)
        {
            player.GetNode<MeshInstance>("DepthOverlay/HackerOutline").Show();
            player.GetNode<MeshInstance>("DepthOverlay/NormalOutline").Hide();
            dotsSpawner.dotMesh.SurfaceGetMaterial(0).Set("flags_transparent", false);
            // GD.Load<Resource>("res://RESOURCES/PlatformMaterial.material").Set("shader", 
            //         GD.Load<Resource>("res://RESOURCES/hackerModeShader.gdshader"));
            globalColors.HackerMode = true;
        }

        LoadHighScore();
        audio.StartGameplay();

        pause.Pausable = true;
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

        ui.DistanceScore = -player.Translation.z / 10;
        if (ui.DistanceScore < 0)
        {
            ui.DistanceScore = 0;
        }

        foreach (Enemy enemy in enemies.GetChildren())
        {
            enemy.FallDirection = player.FallDirection;
        }
    }

    // returns newly generated platform
    private Platform GenerateRandomPlatform(Vector3 currentPlatformTranslation, float 
            currentPlatformRotationZ, bool currentPlatformIsAccelerator, bool rampsEnabled = true)
    {
        // Random values:
        float theta = (int)(GD.Randf()*8)*Mathf.Pi/4 - Mathf.Pi;
        while (theta == 0)
        {
            theta = (int)(GD.Randf()*8)*Mathf.Pi/4 - Mathf.Pi;
        }
        float deltaH = -GD.Randf()*(MAX_DELTA_H - MIN_DELTA_H) - MIN_DELTA_H;
        float rampSeed = GD.Randf();
        float enemySeed = GD.Randf();

        // Other values:
        float distance = GetPlatformDistance(deltaH, currentSpeed, currentPlatformIsAccelerator);
        Vector3 fallDirection = Vector3.Down.Rotated(Vector3.Forward, -currentPlatformRotationZ);
        Vector3 axis = currentPlatformTranslation - fallDirection*JUMP_HEIGHT;
        axis = new Vector3(axis.x, axis.y, currentPlatformTranslation.z - distance);
        
        // Assign platform to platform, ramp, bigRamp (based on rampSeed):
        Platform platform;
        if (rampSeed < BIG_RAMP_PROBABILITY && rampsEnabled) // BIG RAMP
        {
            platform = bigRampScene.Instance<Platform>();
            platform.CurrentType = Platform.Type.BigRamp;
            currentSpeed = BIG_RAMP_SPEED;
            // Subtract from axis in the fall direction (accounts for end ramp momentum):
            axis += fallDirection*BIG_RAMP_AXIS_OFFSET;
        }
        else if (rampSeed < BIG_RAMP_PROBABILITY + RAMP_PROBABILITY && rampsEnabled) // RAMP
        {
            platform = rampScene.Instance<Platform>();
            platform.CurrentType = Platform.Type.Ramp;
            currentSpeed = RAMP_SPEED;
            // Subtract from axis in the fall direction (accounts for end ramp momentum):
            axis += fallDirection*RAMP_AXIS_OFFSET;
        }
        else // PLATFORM
        {
            platform = platformScene.Instance<Platform>();
            platform.CurrentType = Platform.Type.Flat;
            currentSpeed = 100;
        }
        platform.player = player;
        platform.Connect("EndCombo", this, "sig_EndCombo");
        // platform.IsAccelerator = GD.Randi() % 2 == 0;
        platform.IsAccelerator = true;


        AddTunnelRings(axis);
        AddDots(axis);
        lastAxisPoint = axis;
        
        Vector3 newFallDirection = Vector3.Down.Rotated(Vector3.Forward, 
                -currentPlatformRotationZ - theta);

        // RINGS FOLLOW RAMPS DOWNWARD:
        if (rampSeed < BIG_RAMP_PROBABILITY && rampsEnabled) // BIG RAMP
        {
            Vector3 newAxis = axis + (new Vector3(BIG_RAMP_DEPTH*newFallDirection.x, 
                    BIG_RAMP_DEPTH*newFallDirection.y, -BIG_RAMP_LENGTH));
            AddTunnelRings(newAxis);
            AddDots(newAxis);
            lastAxisPoint = newAxis;
        }
        else if (rampSeed < BIG_RAMP_PROBABILITY + RAMP_PROBABILITY && rampsEnabled) // RAMP
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

        // ENEMY:
        if (enemySeed < ENEMY_HORIZONTAL_PROBABILITY) // HORIZONTAL
        {
            Enemy enemy = enemyScene.Instance<Enemy>();
            enemy.CurrentType = Enemy.Type.Horizontal;
            enemies.AddChild(enemy);
            enemy.Rotation = platform.Rotation;
            enemy.Translation = platform.Translation - newFallDirection*15;
        }
        else if (enemySeed < ENEMY_HORIZONTAL_PROBABILITY + ENEMY_VERTICAL_PROBABILITY) // VERTICAL
        {
            Enemy enemy = enemyScene.Instance<Enemy>();
            enemy.CurrentType = Enemy.Type.Vertical;
            enemies.AddChild(enemy);
            enemy.Rotation = platform.Rotation;

            float distanceBack = 150;
            if (lastGeneratedPlatform.CurrentType == Platform.Type.BigRamp)
            {
                distanceBack = 350;
            }
            enemy.Translation = axis + Vector3.Back*distanceBack;
        }
        else if (enemySeed < ENEMY_HORIZONTAL_PROBABILITY + ENEMY_VERTICAL_PROBABILITY + 
                ENEMY_CIRCLE_PROBABILITY) // CIRCLE
        {
            Enemy enemy = enemyScene.Instance<Enemy>();
            enemy.CurrentType = Enemy.Type.Circle;
            enemies.AddChild(enemy);
            enemy.Rotation = platform.Rotation;

            // 50% chance of spawning between platforms
            if (GD.Randf() < 0.5f)
            {
                float distanceBack = 150;
                if (lastGeneratedPlatform.CurrentType == Platform.Type.BigRamp)
                {
                    distanceBack = 350;
                }
                enemy.Translation = axis + Vector3.Back*distanceBack;
            }
            else
            {
                enemy.Translation = platform.Translation - newFallDirection*15;
            }

        }

        platforms.AddChild(platform);

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

    public void SaveHighScore(float highScore)
    {
        save.Call("save_game", highScore);
    }

    public void LoadHighScore()
    {
        HighScore = (float)save.Call("load_game");
    }

    private void sig_AddScoreBonus(float value, string text)
    {
        ui.AddScoreBonus(value, text);
    }

    public void sig_GameOver()
    {
        if (gameOverCalled)
        {
            return;
        }
        gameOverCalled = true;
        pause.Pausable = false;
        if (ui.DisplayScore > HighScore)
        {
            HighScore = ui.DisplayScore;
            SaveHighScore(HighScore);
        }
        recap.SetActive(ui.DisplayScore, HighScore);
        player.Frozen = true;
        player.GetNode<PlayerCamera>("Camera").StartShake(1, 1);
        ui.Hide();
        /// !!! make an actual game over
        // GetTree().ReloadCurrentScene();
    }

    public void sig_Quit()
    {
        save.Call("save_game", HighScore);
        quittingWhenTransitionFinished = true;
        transitionAP.Play("TransitionOut");
    }

    public void sig_Retry()
    {
        save.Call("save_game", HighScore);
        quittingWhenTransitionFinished = false;
        transitionAP.Play("TransitionOut");
    }

    private void sig_TransitionAPFinished(string animName)
    {
        if (animName == "TransitionOut")
        {
            if (quittingWhenTransitionFinished)
            {
                GetTree().ChangeScene("res://StartScreen.tscn");
            }
            else
            {
                GetTree().ReloadCurrentScene();
            }
        }
    }

    private void sig_AddCombo()
    {
        ui.AddCombo();
        
        // hackily finds the platform that the player landed on to continue the combo:
        float maxPlatformZ = float.MinValue;
        Platform closestPlatform = null;
        foreach (Node platform in platforms.GetChildren())
        {
            if (platform is Platform && ((Platform)platform).Translation.z > maxPlatformZ)
            {
                maxPlatformZ = ((Platform)platform).Translation.z;
                closestPlatform = (Platform)platform;
            }
        }
        if (closestPlatform != null)
        {
            closestPlatform.SuccessfullyLanded = true;
        }
    }

    private void sig_EndCombo()
    {
        ui.EndCombo();
    }

    public void sig_PlayerUsedGravitySlash()
    {
        gravitySlashes.RemoveSlash();
    }

    public void sig_PlayerRestoredGravitySlash()
    {
        gravitySlashes.AddSlash();
    }
}
