using Godot;
using System;

public class Player : KinematicBody
{
    [Signal] delegate void AddScoreBonus(float value, string text);
    [Signal] delegate void AddCombo();

    public const float GRAVITY_MAGNITUDE = 0.4f;
    public const float JUMPFORCE = 20f; // with gravity of 0.4, jump height is 8.33333
    const float ROTATION_SPEED = 0;//Mathf.Pi; // rad/s
    const float STRAFE_SPEED = 15;
    const float MOUSE_SENSITIVITY = 1.1f;
    const float BOOST_ACCELERATION = 200; // m/s^2
    const float AIR_RESISTANCE = 40; // m/s^2
    const float BRUSH_RADIUS_SQUARED = 400; // should be a bit larger than actual brush radius 
            //squared to account for spread out points on the line
    const float BRUSH_WIDTH = 20;
    const float ENEMY_RANGE = 1000;
    const float MIN_FORWARD_SPEED = 100;
    const float MAX_FORWARD_SPEED = 1000; // reasonable limit :(
    // const float MAX_FORWARD_SPEED = 99999999; // no limit :)
    const int MAX_SPEED_LINES_AMOUNT = 60;
    const float COYOTE_TIME = 0.2f;

    const float ENEMY_POINTS = 50;
    const float PERFECT_LANDING_POINTS = 100;
    const float GREAT_LANDING_POINTS = 40;
    const float GOOD_LANDING_POINTS = 20;


    public Vector3 Velocity = Vector3.Zero;
    public Vector3 FallDirection = Vector3.Down;
    private Vector2 lookDirection = Vector2.Zero;

    bool jumpButtonPressed = false;
    bool spinButtonPressed = false;
    bool shifting = false;
    bool snapped = false;
    bool onInlinePlatform = false;
    bool rushingDown = false;
    float currentCoyoteTime = 0;
    bool eligibleForLandingBonus = false;

    protected PackedScene test;
    protected PackedScene enemyExplosionScene;
    protected GravityWheel gravityWheel;
    protected Tween tween;
    protected RayCast floorCast;
    protected Camera camera;
    protected GlobalColors globalColors;
    protected Particles speedLines60;
    protected Particles speedLines30;
    protected Particles speedLines15;
    protected Particles speedLines5;
    // protected MouseLine mouseLine;


    public override void _Ready()
    {
        test = GD.Load<PackedScene>("res://Test.tscn");
        enemyExplosionScene = GD.Load<PackedScene>("res://EnemyExplosion.tscn");
        gravityWheel = GetNode<GravityWheel>("UI/GravityWheel");
        tween = GetNode<Tween>("Tween");
        floorCast = GetNode<RayCast>("FloorCast");
        camera = GetNode<Camera>("Camera");
        globalColors = GetNode<GlobalColors>("/root/GlobalColors");
        speedLines60 = GetNode<Particles>("Camera/SpeedLines60");
        speedLines30 = GetNode<Particles>("Camera/SpeedLines30");
        speedLines15 = GetNode<Particles>("Camera/SpeedLines15");
        speedLines5 = GetNode<Particles>("Camera/SpeedLines5");
        // mouseLine = GetNode<MouseLine>("SliceCanvas/MouseLine");

        gravityWheel.SetWheelVisibility(false);

        // Input.MouseMode = Input.MouseModeEnum.Captured;
        Input.MouseMode = Input.MouseModeEnum.Confined;
        Rotation = Vector3.Zero;
        UpdateFallDirection();

        Velocity.z = -MIN_FORWARD_SPEED;
    }

    public override void _PhysicsProcess(float delta)
    {
        UpdateFallDirection();
        HandleButtonRelease();

        onInlinePlatform = floorCast.IsColliding() && floorCast.GetCollider() is StaticBody && 
                ((StaticBody)floorCast.GetCollider()).GetCollisionLayerBit(4);

        if (onInlinePlatform)
        {
            snapped = true;
            StaticBody inlinePlatform = (StaticBody)floorCast.GetCollider();
            float startZ = inlinePlatform.GlobalTranslation.z;
            float endZ = startZ - 140;

            Rotation = new Vector3(Rotation.x, Rotation.y, -(Translation.z - startZ)/140 * Mathf.Pi);
            UpdateFallDirection();
        }

        // if (IsOnFloor() && (!snapped || onInlinePlatform))
        if (IsOnFloor() && !snapped)
        {
            SnapToPlatform();
        }
        else if (snapped && !IsOnFloor())
        {
            snapped = false;
        }

        if (!IsOnFloor())
        {
            Velocity.z += AIR_RESISTANCE*delta;
        }

        if (floorCast.IsColliding() && floorCast.GetCollider() is Platform && 
                ((Platform)floorCast.GetCollider()).IsAccelerator)
        {
            Velocity.z -= BOOST_ACCELERATION*delta;
        }

        if (OnFloor())
        {
            currentCoyoteTime = 0;
        }
        else
        {
            currentCoyoteTime += delta;
        }

        if (Input.IsActionJustPressed("jump") && (OnFloor() || currentCoyoteTime < COYOTE_TIME))
        {
            Jump();
        }
        else if (Input.IsActionPressed("jump") && !jumpButtonPressed)
        {
            Spin(delta);
        }

        float strafeAxis = (Input.GetActionStrength("left") - Input.GetActionStrength("right")) * 
                STRAFE_SPEED;
        Vector3 strafeDirection = FallDirection.Rotated(Vector3.Forward, Mathf.Pi*0.5f).Normalized();
        Vector3 inputVelocity = new Vector3(strafeDirection.x*strafeAxis, 
                strafeDirection.y*strafeAxis, 0);

        // SHIFT
        if (Input.IsActionJustPressed("shift"))
        {
            shifting = true;
            // camera.Rotation = new Vector3(0, 0, 0);
            Input.MouseMode = Input.MouseModeEnum.Confined;
            Engine.TimeScale = 0.2f;
            gravityWheel.SetWheelVisibility(true);
            gravityWheel.Start();
        }
        if (Input.IsActionJustReleased("shift") && shifting)
        {
            RotateShift(GetMouseAngle());
        }

        if (shifting)
        {
            gravityWheel.GravityAngle = GetMouseAngle() + Mathf.Pi;
        }

        rushingDown = Input.IsActionPressed("down") && !IsOnFloor() && !snapped;

        float gravityMagnitude = rushingDown ? 7*GRAVITY_MAGNITUDE : GRAVITY_MAGNITUDE;
        if (!shifting)
        {
            Velocity += new Vector3(FallDirection.x * gravityMagnitude, FallDirection.y *
                    gravityMagnitude, 0);
        }

        // Check bounds of speed
        Velocity.z = Mathf.Clamp(Velocity.z, -MAX_FORWARD_SPEED, -MIN_FORWARD_SPEED);

        // GD.Print(Velocity.z);
        Velocity += inputVelocity;
        if (snapped)
        {
            Velocity.z += Velocity.y*0.3f;

            Velocity = MoveAndSlideWithSnap(Velocity, FallDirection.Normalized()*5, -FallDirection);
        }
        else
        {
            Velocity = MoveAndSlide(Velocity, -FallDirection);
        }
        Velocity -= inputVelocity;

        SetSpeedLinesStrength((int)Mathf.Clamp((-Velocity.z - MIN_FORWARD_SPEED) / 40, 0, 4));
        SetSpeedLinesColor();
    }

    private bool OnFloor()
    {
        return floorCast.IsColliding(); 
    }

    private void Jump()
    {
        snapped = false;
        
        Velocity = new Vector3(-FallDirection.x * JUMPFORCE, -FallDirection.y * JUMPFORCE,
                Velocity.z);
        jumpButtonPressed = true;
    }

    private void Spin(float delta)
    {
        spinButtonPressed = true;
        Transform = Transform.Rotated(Vector3.Forward, ROTATION_SPEED * delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // if (@event is InputEventMouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        // {
        //     lookDirection = ((InputEventMouseMotion)@event).Relative * 0.001f;
        //     camera.Rotation = new Vector3(
        //             Mathf.Clamp(camera.Rotation.x - lookDirection.y * MOUSE_SENSITIVITY, -0.9f, 0.9f), 
        //             camera.Rotation.y - lookDirection.x * MOUSE_SENSITIVITY, 
        //             camera.Rotation.z
        //     );
        // }
    }

    private void HandleButtonRelease()
    {
        if (Input.IsActionJustReleased("jump"))
        {
            if (jumpButtonPressed)
            {
                jumpButtonPressed = false;
            }

            if (spinButtonPressed)
            {
                spinButtonPressed = false;
                // float oldXYSpeed = Mathf.Sqrt(Velocity.x*Velocity.x + Velocity.y*Velocity.y);
                // UpdateFallDirection();
                // Velocity += new Vector3(FallDirection.x * oldXYSpeed, FallDirection.y *
                //         oldXYSpeed, 0);
            }
        }
    }

    private void UpdateFallDirection()
    {
        FallDirection = Vector3.Down.Rotated(Vector3.Back, Rotation.z);
    }

    private float GetMouseAngle()
    {
        Vector2 center = GetViewport().Size / 2;
        Vector2 relativeMousePos = GetViewport().GetMousePosition() - center;
        return -Mathf.Atan2(relativeMousePos.x, relativeMousePos.y);
    }

    private void SnapToPlatform()
    {
        if (GetSlideCount() == 0)
        {
            return;
        }

        snapped = true;

        Velocity.x = 0;
        Velocity.y = 0;

        Vector3 normal = GetSlideCollision(0).Normal;
        float angle = -Mathf.Atan2(normal.x, normal.y);

        // ApplyLandingBonus(angle);

        if (eligibleForLandingBonus)
        {
            ApplyLandingBonus(angle);
            eligibleForLandingBonus = false;
        }

        // Eliminates unwanted barrel roll animation when normal points straight down:
        if (angle > 3.14 && Rotation.z < 0)
        {
            angle -= Mathf.Pi*2;
        }
        if (angle < -3.14 && Rotation.z > 0)
        {
            angle += Mathf.Pi*2;
        }

        tween.InterpolateProperty(this, "rotation", Rotation, new Vector3(Rotation.x, 
                Rotation.y, angle), 0.05f, Tween.TransitionType.Sine, 
                Tween.EaseType.Out);
        tween.Start();
        UpdateFallDirection();
    }

    private void ApplyLandingBonus(float angle)
    {
        float deltaTheta = Mathf.Abs(angle - Rotation.z); // introducing greek life to godot!
        while (deltaTheta > Mathf.Pi*0.5f)
        {
            deltaTheta = Mathf.Abs(deltaTheta - Mathf.Pi);
        }
        if (deltaTheta > Mathf.Pi*0.2f)
        {
            return;
        }

        if (deltaTheta < 0.01f)
        {
            EmitSignal("AddScoreBonus", PERFECT_LANDING_POINTS, "+ perfect landing ");
        }
        else if (deltaTheta < 0.05f)
        {
            EmitSignal("AddScoreBonus", GREAT_LANDING_POINTS, "+ great landing ");
        }
        else
        {
            EmitSignal("AddScoreBonus", GOOD_LANDING_POINTS, "+ good landing ");
        }
        EmitSignal("AddCombo");
        // float landingBonus = (Mathf.Pi*0.05f - deltaTheta) / (Mathf.Pi * 0.05f) * MAX_IMPERFECT_LANDING_POINTS;
        // EmitSignal("AddScoreBonus", (int)landingBonus, "+ landing ");
    }

    private void sig_GravityWheelTimeout()
    {
        RotateShift(GetMouseAngle());
    }

    public async void RotateShift(float angle)
    {
        Velocity.x = 0;
        Velocity.y = 0;
        Engine.TimeScale = 1f;
        // Input.MouseMode = Input.MouseModeEnum.Captured;
        gravityWheel.Lock();

        if (Mathf.Abs(angle) > Mathf.Pi/6)
        {
            globalColors.ShiftPalette();
            eligibleForLandingBonus = true;
        }

        tween.InterpolateProperty(this, "rotation", Rotation, new Vector3(Rotation.x, 
                Rotation.y, Rotation.z - angle), 0.3f, Tween.TransitionType.Sine, 
                Tween.EaseType.Out);
        // tween.InterpolateProperty(mouseLine, "rotation", mouseLine.Rotation, -angle, 0.3f, 
        //         Tween.TransitionType.Sine, Tween.EaseType.Out);
        tween.Start();

        await ToSignal(tween, "tween_completed");

        gravityWheel.SetWheelVisibility(false);
        shifting = false;
    }

    public void sig_LineDrawn(bool gravity, float angle, Vector2[] points)
    {
        if (gravity)
        {
            RotateShift(angle);
        }

        SliceCollisionViaQuadrangulation(points, angle);

        // foreach (Vector2 point in points)
        // {
        //     Vector2 translatedPoint = point + new Vector2(1920/2, 1080/2);
        //     Vector3 from = camera.ProjectRayOrigin(translatedPoint);
        //     Vector3 to = from + camera.ProjectRayNormal(translatedPoint) * 30;

        //     MeshInstance testInstance = test.Instance<MeshInstance>();
        //     testInstance.GlobalTranslation = to;
        //     GetParent().AddChild(testInstance);
        // }
        // SliceCollisionViaCircles(points);
        // SliceCollisionViaRaycast(points);
    }

    private void SliceCollisionViaQuadrangulation(Vector2[] points, float angle)
    {
        // Rotates entire points array by 90 degrees if the line of best fit is more upright:
        float stepifiedAngle = Mathf.Stepify(angle, Mathf.Pi/2);
        Vector2[] newPoints = points;
        bool upright = (stepifiedAngle == 0 || stepifiedAngle == Mathf.Pi|| 
                stepifiedAngle == -Mathf.Pi);

        for (int i = 0; i < newPoints.Length; i++)
        {
            newPoints[i] += new Vector2(960, 540);
            if (upright)
            {
                // Rotate entire points array by 90 degrees:
                newPoints[i] = new Vector2(newPoints[i].y, -newPoints[i].x);
            }
        }

        foreach (Enemy enemy in GetParent().GetNode<Spatial>("Enemies").GetChildren())
        {
            KinematicBody enemyKB = enemy.GetNode<KinematicBody>("KinematicBody");
            if (Mathf.Abs(enemyKB.GlobalTranslation.z - Translation.z) > ENEMY_RANGE)
            {
                continue;
            }
            // Unproject enemy position and radius:
            Vector2 enemyPosition = camera.UnprojectPosition(enemyKB.GlobalTranslation);
            float enemyRadius = Mathf.Abs(enemyPosition.x - camera.UnprojectPosition(
                    enemy.Translation + new Vector3(enemy.Radius, 0, 0)).x);
            Vector2 unrotatedEnemyPosition = enemyPosition;
            if (upright)
            {
                enemyPosition = new Vector2(enemyPosition.y, -enemyPosition.x);
            }

            for (int i = 0; i < newPoints.Length - 1; i++)
            {
                Vector2 thisPoint = newPoints[i];
                Vector2 nextPoint = newPoints[i + 1];

                // if enemy position is NOT between this point x and next point x:
                if (!(enemyPosition.x > thisPoint.x && enemyPosition.x < nextPoint.x) &&
                        !(enemyPosition.x < thisPoint.x && enemyPosition.x > nextPoint.x))
                {
                    continue;
                }
                // if enemy position is NOT within the correct y range:
                if (Mathf.Abs(enemyPosition.y - thisPoint.y) > BRUSH_WIDTH + enemyRadius)
                {
                    // GD.Print(enemyPosition.y - thisPoint.y);
                    continue;
                }
                EmitSignal("AddScoreBonus", ENEMY_POINTS, "+ Enemy ");
                enemy.Explode(angle);
                // EnemyExplosion enemyExplosion = enemyExplosionScene.Instance<EnemyExplosion>();
                // enemyExplosion.Rotation = angle + Mathf.Pi*0.5f;
                // enemyExplosion.Position = unrotatedEnemyPosition;
                // GetParent().AddChild(enemyExplosion);
                break;
            }
        }
    }

    private void SliceCollisionViaCircles(Vector2[] points)
    {
        foreach (Enemy enemy in GetParent().GetNode<Spatial>("Enemies").GetChildren())
        {
            // Unproject enemy position and radius:
            Vector2 enemyPosition = camera.UnprojectPosition(enemy.Translation);
            float enemyRadius = Mathf.Abs(enemyPosition.x - camera.UnprojectPosition(
                    enemy.Translation + new Vector3(enemy.Radius, 0, 0)).x);

            foreach (Vector2 point in points)
            {
                Vector2 translatedPoint = point + new Vector2(960, 540);
                if (enemyPosition.DistanceSquaredTo(translatedPoint) < BRUSH_RADIUS_SQUARED + 
                        enemyRadius*enemyRadius)
                {
                    enemy.QueueFree();
                }
            }
        }
    }

    private void SliceCollisionViaRaycast(Vector2[] points)
    {
        foreach (Vector2 point in points)
        {
            Vector2 translatedPoint = point + new Vector2(1920/2, 1080/2);
            Vector3 from = camera.ProjectRayOrigin(translatedPoint);
            Vector3 to = from + camera.ProjectRayNormal(translatedPoint) * 200;

            MeshInstance testInstance = test.Instance<MeshInstance>();
            GetParent().AddChild(testInstance);
            testInstance.GlobalTranslation = to;

            PhysicsDirectSpaceState spaceState = GetWorld().DirectSpaceState;
            Godot.Collections.Dictionary result = spaceState.IntersectRay(from, to, 
                    collisionMask:8);

            if (result.Count > 0)
            {
                ((Godot.Node)result["collider"]).QueueFree();
            }
        }
    }

    private void GenerateRandomPlatform(Vector3 currentPlatformTranslation, float speed)
    {
        float theta = (int)(GD.Randf()*8)*Mathf.Pi/4;
        float deltaH = -GD.Randf()*30;
        float distance = GetPlatformDistance(deltaH, speed);
        Vector3 axis = currentPlatformTranslation - FallDirection*8.33f;

    }

    private float GetPlatformDistance(float deltaH, float speed)
    {
        return speed*GetJumpTime(deltaH);
    }

    private float GetJumpTime(float deltaH)
    {
        float a = -GRAVITY_MAGNITUDE*0.5f;
        float b = JUMPFORCE;
        float c = -deltaH; // times delta???
        // quadratic formula:
        return (-20 - Mathf.Sqrt(b*b - 4*a*c)) / (2*a);
    }

    private void SetSpeedLinesStrength(int strength)
    {
        speedLines60.Emitting = strength == 4;
        speedLines30.Emitting = strength == 3;
        speedLines15.Emitting = strength == 2;
        speedLines5.Emitting = strength == 1;
    }

    private void SetSpeedLinesColor()
    {
        speedLines60.ProcessMaterial.Set("color", globalColors.bg1);
        speedLines30.ProcessMaterial.Set("color", globalColors.bg1);
        speedLines15.ProcessMaterial.Set("color", globalColors.bg1);
        speedLines5.ProcessMaterial.Set("color", globalColors.bg1);
    }
}
