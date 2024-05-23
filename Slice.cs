using Godot;
using System;

public class Slice : Line2D
{
    [Signal] delegate void LineDrawn(bool gravity, float angle, float slope, Vector2[] points);

    [Export] bool Gravity = false;

    const float MAX_POINTS = 45;

    bool slicing = false;
    bool canSlice = true;

    protected Sprite arrow;
    protected Tween tween;
    protected Node2D pivot;
    protected CPUParticles2D arrowParticles;
    protected AnimationPlayer arrowAP;


    public override void _Ready()
    {
        tween = GetNode<Tween>("Tween");
        pivot = GetNode<Node2D>("Pivot");
        arrowParticles = GetNode<CPUParticles2D>("Pivot/CPUParticles2D");
        arrow = GetNode<Sprite>("Pivot/Arrow");
        arrowAP = GetNode<AnimationPlayer>("Pivot/Arrow/AnimationPlayer");

        arrowParticles.Emitting = false;
        // arrowParticles.Hide();
        arrow.Hide();
    }

    public override void _Process(float delta)
    {
        int mouseButton = Gravity ? 2 : 1;
        if (Input.IsMouseButtonPressed(mouseButton) && Points.Length < 35 && canSlice)
        {
            slicing = true;
            AddPoint(GetViewport().GetMousePosition() - new Vector2(960, 540));
        }
        else
        {
            if (slicing)
            {
                FinishSlice();
            }
        }

        if (Points.Length > 0 && !slicing)
        {
            RemovePoint(0);
            if (Points.Length == 0)
            {
                canSlice = true;
            }
        }
    }

    public async void FinishSlice()
    {
        slicing = false;
        canSlice = false;
        if (Gravity)
        {
            arrowParticles.Emitting = true;
            // arrowParticles.Show();
            arrow.Show();
            arrowAP.Play("point");
            float[] regressionInfo = RegressedSlopeAngle(Points);
            float angle = regressionInfo[0];
            float slope = regressionInfo[1];
            EmitSignal("LineDrawn", true, angle, slope, Points);

            tween.InterpolateProperty(pivot, "rotation", angle, 0, 0.3f, Tween.TransitionType.Sine, 
                    Tween.EaseType.Out);
            tween.Start();

            await ToSignal(tween, "tween_completed");

            arrowParticles.Emitting = false;
            // arrowParticles.Hide();
        }
        else
        {
            float[] regressionInfo = RegressedSlopeAngle(Points);
            float angle = regressionInfo[0];
            EmitSignal("LineDrawn", false, angle, 0, Points);
        }
    }

    public bool IsDrawnToRight(Vector2[] points)
    {
        return Points[Points.Length - 1].x > Points[0].x;
    }

    // Calculates slope of array of points with a linear regression:
    public float[] RegressedSlopeAngle(Vector2[] points)
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
        float slope = sumXYDiff / sumXDiffSquared;
        float angle = Mathf.Atan2(sumXDiffSquared, -sumXYDiff);

        // Scale angle to range from 0 to 2pi:
        if (IsDrawnToRight(points))
        {
            angle = (Mathf.Pi - angle)*-1;
        }
        // if (angle < 0)
        // {
        //     angle += Mathf.Pi*2;
        // }
        // GD.Print(angle);
        // angle = Mathf.Stepify(angle, Mathf.Pi*0.25f);
        return new float[]{angle, slope};
    }

    public void sig_ArrowAPFinished(string animName)
    {
        if (animName == "point")
        {
            arrow.Hide();
        }
    }
}
