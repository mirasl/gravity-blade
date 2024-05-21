using Godot;
using System;

public class Player : KinematicBody
{
    const float GRAVITY_MAGNITUDE = 0.4f;
    const float JUMPFORCE = 20f;
    const float ROTATION_SPEED = 0;//Mathf.Pi; // rad/s

    public Vector3 Velocity = Vector3.Zero;
    private Vector3 fallDirection;

    bool jumpButtonPressed = false;
    bool spinButtonPressed = false;


    public override void _Ready()
    {
        UpdateFallDirection();
    }

    public override void _PhysicsProcess(float delta)
    {
        HandleButtonRelease();
        UpdateFallDirection();

        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            Jump();
        }
        else if (Input.IsActionPressed("jump") && !jumpButtonPressed)
        {
            Spin(delta);
        }

        // SHIFT
        if (Input.IsActionJustPressed("shift"))
        {
            Engine.TimeScale = 0.2f;
        }
        if (Input.IsActionJustReleased("shift"))
        {
            Engine.TimeScale = 1f;
            // Rotation = new Vector3(Rotation.x, Rotation.y, GetMouseAngle());
            Transform = Transform.Rotated(Vector3.Forward, GetMouseAngle());
        }

        Velocity += new Vector3(fallDirection.x * GRAVITY_MAGNITUDE, fallDirection.y *
                GRAVITY_MAGNITUDE, 0);
        Velocity = MoveAndSlide(Velocity, -fallDirection);
    }

    private void Jump()
    {
        Velocity = new Vector3(-fallDirection.x * JUMPFORCE, -fallDirection.y * JUMPFORCE,
                Velocity.z);
        jumpButtonPressed = true;
    }

    private void Spin(float delta)
    {
        spinButtonPressed = true;
        Transform = Transform.Rotated(Vector3.Forward, ROTATION_SPEED * delta);
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
}
