using Godot;
using System;

public class Platform : StaticBody
{
    [Signal] delegate void EndCombo();

    [Export] public bool IsAccelerator = false;
    [Export] public Type CurrentType = Type.Flat;
    [Export] private int extents = 65;

    public bool SuccessfullyLanded = false;
    public Player player;

    public enum Type
    {
        Flat,
        Ramp,
        BigRamp
    }


    // public override void _Ready()
    // {
    //     if (IsAccelerator)
    //     {
    //         // GetNode<MeshInstance>("TopMesh").GetSurfaceMaterial(0).Set("albedo_color", 
    //         //         Colors.Yellow);
    //     }
    // }

    public override void _Process(float delta)
    {
        if (player != null && player.GlobalTranslation.z < GlobalTranslation.z - extents)
        {
            if (!SuccessfullyLanded)
            {
                EmitSignal("EndCombo");
            }
            QueueFree();
        }
    }
}
