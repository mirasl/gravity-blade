using Godot;
using System;

public class TunnelRing : MeshInstance
{
    float rotationSpeed;
    public Player player;
    
    Material surfaceMaterial;


    public override void _Ready()
    {
        surfaceMaterial = GetSurfaceMaterial(0);

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

        length = GD.Randf() * 0.6f + 0.2f;
        startingRotation = GD.Randf();
        rotationSpeed = GD.Randf() - 0.5f;

        Rotation = new Vector3(startingRotation * Mathf.Pi*2, Mathf.Pi/2, Mathf.Pi/2);
        surfaceMaterial.Set("shader_param/lineLength", length);
        surfaceMaterial.Set("shader_param/lineColor", color);
    }

    public override void _PhysicsProcess(float delta)
    {
        Rotation = new Vector3(Rotation.x + rotationSpeed * delta, Mathf.Pi/2, Mathf.Pi/2);
        if (player != null)
        {
            float diff = player.Translation.z - Translation.z;

            // Less trippy perspective:
            Visible = diff < 700;
            float coefficient = (1000 - diff)/1000;

            // Trippy constancy:
            // Visible = diff < 316;
            // float coefficient = 100/(diff-400) + 1.2f; //585

            Scale = new Vector3(6.687f*coefficient, 3.429f, 6.687f*coefficient);
            surfaceMaterial.Set("shader_param/alpha", coefficient*coefficient);
        }
    }
}