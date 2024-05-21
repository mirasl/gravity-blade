using Godot;
using System;

public class Player : KinematicBody
{
    const float GRAVITY_MAGNITUDE = 0.4f;
    const float JUMPFORCE = 20f;
    const float ROTATION_SPEED = 0;//Mathf.Pi; // rad/s
    const float STRAFE_SPEED = 10;
    const float FORWARD_SPEED = 70;
    const float MOUSE_SENSITIVITY = 1.1f;

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


    public override void _Ready()
    {
        gravityWheel = GetNode<GravityWheel>("UI/GravityWheel");
        tween = GetNode<Tween>("Tween");
        floorCast = GetNode<RayCast>("FloorCast");
        camera = GetNode<Camera>("Camera");

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
        tween.Start();

        await ToSignal(tween, "tween_completed");

        gravityWheel.SetWheelVisibility(false);
        shifting = false;
    }

    public void sig_GravityLineDrawn(float angle)
    {
        RotateShift(GetMouseAngle());
    }
}
