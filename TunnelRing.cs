using Godot;
using System;

public class TunnelRing : MeshInstance
{
    float rotationSpeed;
    public Player player;
    int colorIndex;
    
    Material surfaceMaterial;
    GlobalColors globalColors;


    public override void _Ready()
    {
        globalColors = GetNode<GlobalColors>("/root/GlobalColors");

        surfaceMaterial = GetSurfaceMaterial(0);

        float rand = GD.Randf();

        if (rand > 0.3f)
        {
            return;
        } 

        float length;
        float startingRotation;

        if (rand < 0.05f)
        {
            colorIndex = 0;
        }
        else if (rand < 0.1f)
        {
            colorIndex = 1;
        }
        else
        {
            colorIndex = 2;
        }

        length = GD.Randf() * 0.6f + 0.2f;
        startingRotation = GD.Randf();
        rotationSpeed = GD.Randf() - 0.5f;

        Rotation = new Vector3(startingRotation * Mathf.Pi*2, Mathf.Pi/2, Mathf.Pi/2);
        surfaceMaterial.Set("shader_param/lineLength", length);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (Translation.z > player.Translation.z)
        {
            QueueFree();
        }

        Color color = globalColors.GetColorFromIndex(colorIndex);
        surfaceMaterial.Set("shader_param/lineColor", new Vector3(color.r, color.g, color.b));

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
