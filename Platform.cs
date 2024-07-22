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

    protected MeshInstance hackerMesh;
    protected MeshInstance normalMesh;
    protected GlobalColors globalColors;


    // public override void _Ready()
    // {
    //     if (IsAccelerator)
    //     {
    //         // GetNode<MeshInstance>("TopMesh").GetSurfaceMaterial(0).Set("albedo_color", 
    //         //         Colors.Yellow);
    //     }
    // }

    public override async void _Ready()
    {
        hackerMesh = GetNode<MeshInstance>("HackerMesh");
        normalMesh = GetNode<MeshInstance>("NormalMesh");
        globalColors = GetNode<GlobalColors>("/root/GlobalColors");

        // delays following code to ensure that globalColors' hackerMode has been properly set:
        await ToSignal(GetTree().CreateTimer(0), "timeout");

        hackerMesh.Visible = globalColors.HackerMode;
        normalMesh.Visible = !globalColors.HackerMode;
    }

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
