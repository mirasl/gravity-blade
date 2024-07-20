using Godot;
using System;

public class GravitySlashes : Control
{
    private int slashes = 4;

    AnimatedSprite slash1;
    AnimatedSprite slash2;
    AnimatedSprite slash3;
    AnimatedSprite slash4;


    public override void _Ready()
    {
        slash1 = GetNode<AnimatedSprite>("1");
        slash2 = GetNode<AnimatedSprite>("2");
        slash3 = GetNode<AnimatedSprite>("3");
        slash4 = GetNode<AnimatedSprite>("4");
    }

    public void RemoveSlash()
    {
        if (slashes <= 0)
        {
            return;
        }
        slashes--;
        UpdateSlashes();
    }

    public void AddSlash()
    {
        if (slashes >= 4)
        {
            return;
        }
        slashes++;
        UpdateSlashes();
    }

    private void UpdateSlashes()
    {
        slash1.Visible = slashes >= 1;
        slash2.Visible = slashes >= 2;
        slash3.Visible = slashes >= 3;
        slash4.Visible = slashes >= 4;
    }
}
