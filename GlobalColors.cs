using Godot;
using System;
using System.Collections.Generic;

public class GlobalColors : Node
{
    private static Dictionary<string, Color>[] colorPalettes = new Dictionary<string, Color>[]
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
            {"fg", Colors.White}
        },
        // MUMBAI (yellow, turquoise, blue)
        new Dictionary<string, Color> 
        {
            {"bg1", Colors.Yellow},
            {"bg2", Colors.Turquoise},
            {"fg", Colors.Blue}
        }
    };

    // public static Dictionary<string, Color> CurrentPalette {get; private set;} = 
    //         new Dictionary<string, Color>{};
    public Color bg1 = Colors.Black;
    public Color bg2 = Colors.Black;
    public Color fg = Colors.Black;

    private static int CurrentIndex = -1; // To ensure that ShiftPalette leads to new color

    Tween tween;


    public override void _Ready()
    {
        GD.Randomize();
        tween = GetNode<Tween>("Tween");

        ShiftPalette();
    }

    public override void _PhysicsProcess(float delta)
    {
        // GD.Print(bg1);
    }

    public void ShiftPalette()
    {
        int newIndex = (int)(GD.Randf()*colorPalettes.Length);
        while (newIndex == CurrentIndex)
        {
            newIndex = (int)(GD.Randf()*colorPalettes.Length);  
        }
        CurrentIndex = newIndex;
        Dictionary<string, Color> palette = colorPalettes[CurrentIndex];

        tween.InterpolateProperty(this, "bg1", bg1, palette["bg1"], 0.3f, Tween.TransitionType.Sine, 
                Tween.EaseType.Out);
        tween.InterpolateProperty(this, "bg2", bg2, palette["bg2"], 0.3f, Tween.TransitionType.Sine, 
                Tween.EaseType.Out);
        tween.InterpolateProperty(this, "fg", fg, palette["fg"], 0.3f, Tween.TransitionType.Sine, 
                Tween.EaseType.Out);
        tween.Start();

        // SceneTreeTween tween = GetTree().CreateTween();
        // CurrentPalette = (Dictionary<string, Color>)tween.InterpolateValue(CurrentPalette, colorPalettes[CurrentIndex], 0, 
        //         0.3f, Tween.TransitionType.Sine, Tween.EaseType.Out);
    }
}
