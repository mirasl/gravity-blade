using Godot;
using System;

public class MouseLine : Line2D
{
    bool slicing = false;


    public override void _Ready()
    {
        
    }

    public override void _Process(float delta)
    {
        if (Input.IsMouseButtonPressed(1))
        {
            slicing = true;
            AddPoint(GetViewport().GetMousePosition());
        }
        else
        {
            if (slicing)
            {
                slicing = false;
                FinishSlice();
            }
        }

        if (Points.Length > 60)
        {
            RemovePoint(0);
        }
    }

    public async void FinishSlice()
    {

    }
}
