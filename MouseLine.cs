using Godot;
using System;

public class MouseLine : Line2D
{
    [Signal] delegate void GravityLineDrawn(float angle);

    bool slicing = false;


    public override void _Process(float delta)
    {
        if (Input.IsMouseButtonPressed(1))
        {
            slicing = true;
            AddPoint(GetViewport().GetMousePosition());
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
        EmitSignal("GravityLineDrawn", RegressedSlopeAngle(Points));
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
        float angle = Mathf.Atan2(-sumXYDiff, sumXDiffSquared);

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
