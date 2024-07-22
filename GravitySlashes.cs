using Godot;
using System;

public class GravitySlashes : Control
{
    private int slashes = 4;

    AnimatedSprite slash1;
    AnimatedSprite slash2;
    AnimatedSprite slash3;
    AnimatedSprite slash4;
    GlobalColors globalColors;


    public override void _Ready()
    {
        slash1 = GetNode<AnimatedSprite>("1");
        slash2 = GetNode<AnimatedSprite>("2");
        slash3 = GetNode<AnimatedSprite>("3");
        slash4 = GetNode<AnimatedSprite>("4");
        globalColors = GetNode<GlobalColors>("/root/GlobalColors");
    }

    public override void _Process(float delta)
    {
        Modulate = globalColors.text;
    }

    public void RemoveSlash()
    {
        if (slashes <= 0)
        {
            return;
        }
        GetLastSlash().Play("exit");
        slashes--;
    }

    public void AddSlash()
    {
        if (slashes >= 4)
        {
            return;
        }
        slashes++;
        GetLastSlash().Play("enter");
        // UpdateSlashes();
    }

    // private void UpdateSlashes()
    // {
    //     slash1.Visible = slashes >= 1;
    //     slash2.Visible = slashes >= 2;
    //     slash3.Visible = slashes >= 3;
    //     slash4.Visible = slashes >= 4;
    // }

    private AnimatedSprite GetLastSlash()
    {
        switch (slashes)
        {
            case (1): return slash1;
            case (2): return slash2;
            case (3): return slash3;
            case (4): return slash4;
        }
        return null;
    }
}
