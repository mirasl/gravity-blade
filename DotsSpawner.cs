using Godot;
using System;
using System.Collections.Generic;

public class DotsSpawner : Spatial
{
    const float MIN_RADIUS = 50;
    const float MAX_RADIUS = 100;
    const float MIN_SCALE = 0.25f;
    const float MAX_SCALE = 2f;

    [Export] Mesh dotMesh;
    [Export] string playerPath;

    MultiMeshInstance dotsInstantiator;
	List<Dot> dots = new List<Dot>();
	List<Dot> dotsQueuedForDestruction = new List<Dot>();

    Player player;
    GlobalColors globalColors;


	public class Dot : Reference
	{
		public Vector3 Translation;
		public float Scale;
		public int colorIndex;
	}


	public override void _ExitTree()
	{
		dots.Clear();
	}

    public override void _Ready()
    {
        player = GetParent().GetNode<Player>(playerPath);
        globalColors = GetNode<GlobalColors>("/root/GlobalColors");

        dotsInstantiator = new MultiMeshInstance();
        AddChild(dotsInstantiator);

		MultiMesh multimesh = new MultiMesh();
		dotsInstantiator.Multimesh = multimesh;
		multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3d;
		multimesh.ColorFormat = MultiMesh.ColorFormatEnum.Float;
		multimesh.Mesh = (Mesh)dotMesh.Duplicate();
    }

    public override void _Process(float delta)
    {
        // Destroy dots queued for destruction:
		for (int i = 0; i < dotsQueuedForDestruction.Count; i++)
		{
			Dot dot = dotsQueuedForDestruction[i];
			dots.Remove(dot);
			dotsInstantiator.Multimesh.InstanceCount = dots.Count;
		}
		dotsQueuedForDestruction.Clear();

		for (int i = dots.Count - 1; i >= 0; i--)
		{
			Dot dot = dots[i];

			// check to see if we should delete it:
			if (dot.Translation.z > player.Translation.z)
			{
				dotsQueuedForDestruction.Add(dot);
				continue;
			}

			// Draw the bullet:
			dotsInstantiator.Multimesh.InstanceCount = dots.Count;

			Transform dotTransform = new Transform(new Basis(Vector3.Zero).Scaled(dot.Scale * 
					Vector3.One), dot.Translation);

			dotsInstantiator.Multimesh.SetInstanceTransform(i, dotTransform);

			dotsInstantiator.Multimesh.SetInstanceColor(i, globalColors.GetColorFromIndex(
                    dot.colorIndex));
		}
    }

    public void SpawnRandomDot(Vector3 origin)
    {
        SpawnDot(
            GD.Randf()*(MAX_RADIUS-MIN_RADIUS) + MIN_RADIUS,
            GD.Randf()*Mathf.Pi*2,
            origin,
            GD.Randf()*(MAX_SCALE-MIN_SCALE) + MIN_SCALE,
            (int)(GD.Randf()*3)
        );
    }

    public void SpawnDot(
			float r,
            float theta,
            Vector3 origin,
			float scale, 
			int colorIndex)
	{
		Dot dot = new Dot();
		dot.Translation = PolarToRectangular(r, theta, origin);
		dot.Scale = scale;
		dot.colorIndex = colorIndex;
		
		dots.Add(dot);
	}

    public Vector3 PolarToRectangular(float r, float theta, Vector3 origin)
    {
        return new Vector3(r*Mathf.Cos(theta) + origin.x, r*Mathf.Sin(theta) + origin.y, origin.z);
    }
}
