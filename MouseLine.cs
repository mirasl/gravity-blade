using Godot;
using System;

public class MouseLine : Line2D
{
    bool slicing = false;


    public override void _Ready()
    {
        
    }

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
        GD.Print(RegressedSlope(Points));
    }

    // Calculates slope of array of points with a linear regression:
    public float RegressedSlope(Vector2[] points)
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
        return -sumXYDiff / sumXDiffSquared;
    }
}
