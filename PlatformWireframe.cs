using Godot;
using System;

public class PlatformWireframe : MeshInstance
{
    Material material;

    
    public override void _Ready()
    {
        material = GetSurfaceMaterial(0);
        Godot.Collections.Array array = Mesh.SurfaceGetArrays(0);
        ArrayMesh arrayMesh = new ArrayMesh();
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Lines, array);
        SetSurfaceMaterial(0, material);
    }
//     extends Spatial

// onready var mesh_node = get_node("Cube")
// onready var mesh_mat = mesh_node.get_surface_material(0)

// func _ready():
// 	make_wire();

// func make_wire():
// 	var array = mesh_node.mesh.surface_get_arrays(0)
// 	mesh_node.mesh = ArrayMesh.new()
// 	mesh_node.mesh.add_surface_from_arrays(Mesh.PRIMITIVE_LINES, array)
// 	mesh_node.set_surface_material(0, mesh_mat)
}
