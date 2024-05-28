using Godot;
using System;

public class TunnelRing : MeshInstance
{
    float rotationSpeed;


    public override void _Ready()
    {
        float rand = GD.Randf();

        if (rand > 0.3f)
        {
            return;
        } 

        Vector3 color;
        float length;
        float startingRotation;

        if (rand < 0.1f)
        {
            color = new Vector3(1, 1, 1);
        }
        else if (rand < 0.2f)
        {
            color = new Vector3(0, 1, 1);
        }
        else
        {
            color = new Vector3(1, 0.3f, 1);
        }

        length = GD.Randf();
        startingRotation = GD.Randf();
        rotationSpeed = GD.Randf() - 0.5f;

        Rotation = new Vector3(startingRotation * Mathf.Pi*2, Mathf.Pi/2, Mathf.Pi/2);
        Material surfaceMaterial = GetSurfaceMaterial(0);
        surfaceMaterial.Set("shader_param/lineLength", length);
        surfaceMaterial.Set("shader_param/lineColor", color);
    }

    public override void _PhysicsProcess(float delta)
    {
        Rotation = new Vector3(Rotation.x + rotationSpeed * delta, Mathf.Pi/2, Mathf.Pi/2);
    }
}
