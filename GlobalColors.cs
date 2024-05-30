using Godot;
using System;
using System.Collections.Generic;

public class GlobalColors : Node
{
    private Dictionary<string, Color>[] colorPalettes = new Dictionary<string, Color>[]
    {
        // EVIL (dark blue, dark purple, bright red)
        new Dictionary<string, Color> 
        {
            {"bg1", Colors.DarkBlue},
            {"bg2", Colors.Purple},
            {"fg", Colors.OrangeRed}
        },
        // TRANS (light blue, pink, white)
        new Dictionary<string, Color> 
        {
            {"bg1", Colors.White},
            {"bg2", Colors.LightPink},
            {"fg", Colors.LightSkyBlue}
        },
        // AURORA (light blue, light green, white)
        new Dictionary<string, Color> 
        {
            {"bg1", Colors.LightBlue},
            {"bg2", Colors.LightGreen},
            {"bg3", Colors.White}
        },
        // MUMBAI (yellow, turquoise, blue)
        new Dictionary<string, Color> 
        {
            {"bg1", Colors.Yellow},
            {"bg2", Colors.Turquoise},
            {"bg3", Colors.Blue}
        }
    };

    public Dictionary<string, Color> CurrentPalette {get; private set;} = 
            new Dictionary<string, Color>{};

    Tween tween;


    public override void _Ready()
    {
        tween = GetNode<Tween>("Tween");
        
        CurrentPalette = colorPalettes[(int)(GD.Randf()*4)];
    }

    public void ShiftPalette()
    {
        CurrentPalette = colorPalettes[(int)(GD.Randf()*4)];
    }
}
