using Godot;
using System;

public class Platform : StaticBody
{
    [Export] public bool IsAccelerator = false;
    [Export] public Type CurrentType = Type.Flat;

    public enum Type
    {
        Flat,
        Ramp,
        BigRamp
    }


    public override void _Ready()
    {
        if (IsAccelerator)
        {
            // GetNode<MeshInstance>("TopMesh").GetSurfaceMaterial(0).Set("albedo_color", 
            //         Colors.Yellow);
        }
    }
}
