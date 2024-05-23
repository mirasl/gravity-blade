using Godot;
using System;

public class Player : KinematicBody
{
    const float GRAVITY_MAGNITUDE = 0.4f;
    const float JUMPFORCE = 20f;
    const float ROTATION_SPEED = 0;//Mathf.Pi; // rad/s
    const float STRAFE_SPEED = 10;
    const float FORWARD_SPEED = 140;
    const float MOUSE_SENSITIVITY = 1.1f;
    const float BOOST_SPEED = 100;
    const float BRUSH_RADIUS_SQUARED = 400; // should be a bit larger than actual brush radius 
            //squared to account for spread out points on the line
    const float BRUSH_WIDTH = 20;
    const float ENEMY_RANGE = 300;

    public Vector3 Velocity = Vector3.Zero;
    private Vector3 fallDirection = Vector3.Down;
    private Vector2 lookDirection = Vector2.Zero;

    bool jumpButtonPressed = false;
    bool spinButtonPressed = false;
    bool shifting = false;
    bool snapped = false;

    protected GravityWheel gravityWheel;
    protected Tween tween;
    protected RayCast floorCast;
    protected Camera camera;
    protected PackedScene test;
    protected PackedScene enemyExplosionScene;
    // protected MouseLine mouseLine;


    public override void _Ready()
    {
        gravityWheel = GetNode<GravityWheel>("UI/GravityWheel");
        tween = GetNode<Tween>("Tween");
        floorCast = GetNode<RayCast>("FloorCast");
        camera = GetNode<Camera>("Camera");
        test = GD.Load<PackedScene>("res://Test.tscn");
        enemyExplosionScene = GD.Load<PackedScene>("res://EnemyExplosion.tscn");
        // mouseLine = GetNode<MouseLine>("SliceCanvas/MouseLine");

        gravityWheel.SetWheelVisibility(false);

        // Input.MouseMode = Input.MouseModeEnum.Captured;
        Input.MouseMode = Input.MouseModeEnum.Confined;
        Rotation = Vector3.Zero;
        UpdateFallDirection();

        Velocity.z = -FORWARD_SPEED;
    }

    public override void _PhysicsProcess(float delta)
    {
        UpdateFallDirection();
        HandleButtonRelease();

        if (IsOnFloor() && !snapped)
        {
            SnapToPlatform();
        }

        if (floorCast.IsColliding() && floorCast.GetCollider() is StaticBody && ((StaticBody)floorCast.GetCollider()).GetCollisionLayerBit(2))
        {
            GD.Print(((StaticBody)floorCast.GetCollider()).CollisionLayer);
            Velocity.z -= BOOST_SPEED*delta;
        }

        if (Input.IsActionJustPressed("jump") && OnFloor())
        {
            Jump();
        }
        else if (Input.IsActionPressed("jump") && !jumpButtonPressed)
        {
            Spin(delta);
        }

        float strafeAxis = (Input.GetActionStrength("left") - Input.GetActionStrength("right")) * 
                STRAFE_SPEED;
        Vector3 strafeDirection = fallDirection.Rotated(Vector3.Forward, Mathf.Pi*0.5f).Normalized();
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

        if (!shifting)
        {
            Velocity += new Vector3(fallDirection.x * GRAVITY_MAGNITUDE, fallDirection.y *
                    GRAVITY_MAGNITUDE, 0);
        }
        Velocity += inputVelocity;
        Velocity = MoveAndSlide(Velocity, -fallDirection);
        Velocity -= inputVelocity;
    }

    private bool OnFloor()
    {
        return floorCast.IsColliding(); 
    }

    private void Jump()
    {
        snapped = false;
        
        Velocity = new Vector3(-fallDirection.x * JUMPFORCE, -fallDirection.y * JUMPFORCE,
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
                // Velocity += new Vector3(fallDirection.x * oldXYSpeed, fallDirection.y *
                //         oldXYSpeed, 0);
            }
        }
    }

    private void UpdateFallDirection()
    {
        fallDirection = Vector3.Down.Rotated(Vector3.Back, Rotation.z);
    }

    private float GetMouseAngle()
    {
        Vector2 center = GetViewport().Size / 2;
        Vector2 relativeMousePos = GetViewport().GetMousePosition() - center;
        return -Mathf.Atan2(relativeMousePos.x, relativeMousePos.y);
    }

    private void SnapToPlatform()
    {
        snapped = true;

        Velocity.x = 0;
        Velocity.y = 0;

        Vector3 normal = GetSlideCollision(0).Normal;
        float angle = -Mathf.Atan2(normal.x, normal.y);

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

    public void sig_LineDrawn(bool gravity, float angle, float slope, Vector2[] points)
    {
        if (gravity)
        {
            RotateShift(angle);
        }

        SliceCollisionViaQuadrangulation(points, angle);
        // SliceCollisionViaCircles(points);
        // SliceCollisionViaRaycast(points);
    }

    private void SliceCollisionViaQuadrangulation(Vector2[] points, float angle)
    {
        GD.Print(angle);
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
            if (Mathf.Abs(enemy.Translation.z - Translation.z) > ENEMY_RANGE)
            {
                continue;
            }
            // Unproject enemy position and radius:
            Vector2 enemyPosition = camera.UnprojectPosition(enemy.Translation);
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
                enemy.QueueFree();
                EnemyExplosion enemyExplosion = enemyExplosionScene.Instance<EnemyExplosion>();
                enemyExplosion.Rotation = angle + Mathf.Pi*0.5f;
                enemyExplosion.Position = unrotatedEnemyPosition;
                GetParent().AddChild(enemyExplosion);
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
            GD.Print(from);
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
}
