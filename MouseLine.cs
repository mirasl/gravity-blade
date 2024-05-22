using Godot;
using System;

public class MouseLine : Line2D
{
    [Signal] delegate void GravityLineDrawn(float angle);

    [Export] bool Gravity = false;

    bool slicing = false;

    protected Sprite arrow;
    protected Tween tween;
    protected Node2D pivot;
    protected CPUParticles2D arrowParticles;


    public override void _Ready()
    {
        tween = GetNode<Tween>("Tween");
        pivot = GetNode<Node2D>("Pivot");
        arrowParticles = GetNode<CPUParticles2D>("Pivot/CPUParticles2D");
        arrow = GetNode<Sprite>("Pivot/Arrow");

        arrowParticles.Emitting = false;
        arrow.Hide();
    }

    public override void _Process(float delta)
    {
        int mouseButton = Gravity ? 2 : 1;
        if (Input.IsMouseButtonPressed(mouseButton))
        {
            if (Gravity)
            {
                arrowParticles.Emitting = true;
            }
            slicing = true;
            AddPoint(GetViewport().GetMousePosition() - new Vector2(960, 540));
        }
        else
        {
            if (slicing)
            {
                slicing = false;
                FinishSlice();
            }
        }

        if (Points.Length > 0 && !slicing)
        {
            RemovePoint(0);
        }
    }

    public void FinishSlice()
    {
        if (Gravity)
        {
            arrowParticles.Emitting = false;
            EmitSignal("GravityLineDrawn", RegressedSlopeAngle(Points));
        }
    }

    public bool IsDrawnToRight(Vector2[] points)
    {
        return Points[Points.Length - 1].x > Points[0].x;
    }

    // Calculates slope of array of points with a linear regression:
    public float RegressedSlopeAngle(Vector2[] points)
    {
        // Calculate means:
        float xMean = 0;
        float yMean = 0;
        foreach (Vector2 point in points)
        {
            xMean += point.x;
            yMean += point.y;
        }
        xMean /= points.Length;
        yMean /= points.Length;

        // Calculate xi - xMean and yi - yMean
        float sumXYDiff = 0;
        float sumXDiffSquared = 0;
        foreach (Vector2 point in points)
        {
            sumXYDiff += (point.x - xMean) * (point.y - yMean);
            sumXDiffSquared += (point.x - xMean) * (point.x - xMean);
        }
        float angle = Mathf.Atan2(-sumXYDiff * 9, sumXDiffSquared * 16);

        // Scale angle to range from 0 to 2pi:
        if (!IsDrawnToRight(points))
        {
            angle += Mathf.Pi;
        }
        if (angle < 0)
        {
            angle += Mathf.Pi*2;
        }
        return angle;
    }
}
