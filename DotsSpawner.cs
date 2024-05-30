using Godot;
using System;
using System.Collections.Generic;

public class DotsSpawner : Spatial
{
    [Export] Mesh dotMesh;
    [Export] string playerPath;

    MultiMeshInstance dotsInstantiator;
	List<Dot> dots = new List<Dot>();
	List<Dot> dotsQueuedForDestruction = new List<Dot>();
    Player player;


	public class Dot : Reference
	{
		public Vector3 Translation;
		public float Scale;
		public Color Albedo;
	}


	public override void _ExitTree()
	{
		dots.Clear();
	}

    public override void _Ready()
    {
        player = GetParent().GetNode<Player>(playerPath);

        dotsInstantiator = new MultiMeshInstance();
        AddChild(dotsInstantiator);

		MultiMesh multimesh = new MultiMesh();
		dotsInstantiator.Multimesh = multimesh;
		multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3d;
		multimesh.ColorFormat = MultiMesh.ColorFormatEnum.Float;
		multimesh.Mesh = (Mesh)dotMesh.Duplicate();

        SpawnDot(new Vector3(0, 0, -100), 1, Colors.Black);
    }

    public override void _Process(float delta)
    {
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

			dotsInstantiator.Multimesh.SetInstanceColor(i, dot.Albedo);
		}
    }

    public void SpawnDot(
			Vector3 translation, 
			float scale, 
			Color albedo)
	{
		Dot dot = new Dot();
		dot.Translation = translation;
		dot.Scale = scale;
		dot.Albedo = albedo;
		
		dots.Add(dot);
	}
}
