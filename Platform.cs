using Godot;
using System;

public class Platform : StaticBody
{
    public bool IsAccelerator = false;


    public override void _Ready()
    {
        if (IsAccelerator)
        {
            // GetNode<MeshInstance>("TopMesh").GetSurfaceMaterial(0).Set("albedo_color", 
            //         Colors.Yellow);
        }
    }
}
