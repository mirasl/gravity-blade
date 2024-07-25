using Godot;
using System;
using System.Collections.Generic;

public class GlobalColors : Node
{
    public bool HackerMode = false;
    // bg2 is in the main background of the scene
    private static Dictionary<string, Color>[] colorPalettes = new Dictionary<string, Color>[]
    {
        // EVIL (dark blue, dark purple, bright red)
        new Dictionary<string, Color> 
        {
            {"bg1", new Color("3b0fa5")},
            {"bg2", new Color("0e1b49")},
            {"fg", new Color("c91731")}, // red
            {"text", new Color("ee5a6f")}, // lighter red
            {"enemy", new Color("ee5a6f")}, // lighter red (same as text)
            {"platform", new Color("6640c2")}, // lighter purple
        },
        // TRANS (light blue, pink, white)
        new Dictionary<string, Color> 
        {
            {"bg1", Colors.White},
            {"bg2", new Color("ed4cc5")}, // e99fde
            {"fg", new Color("39a8f0")},
            {"text", Colors.White}, 
            {"enemy", new Color("8ccffa")}, // lighter blue
            {"platform", new Color("2f6cb3")}, // darker blue
        },
        // AURORA (light blue, light green, white)
        new Dictionary<string, Color> 
        {
            {"bg1", new Color("19b387")}, // light green
            {"bg2", new Color("045aae")}, // blue
            {"fg", new Color("108f3e")}, // greener blue ; old 1a80a5   4cde6f
            {"text", new Color("4cde6f")},
            {"enemy", new Color("24b357")}, // lighter blue ; old  43abd1  49e3b7
            {"platform", new Color("1d42a1")},
        },
        // MUMBAI (yellow, turquoise, blue)
        new Dictionary<string, Color> 
        {
            {"bg1", new Color("52d998")},
            {"bg2", new Color("fdff9a")},
            {"fg", new Color("4082c0")}, // blue
            {"text", new Color("5f8fbb")},
            {"enemy", new Color("76aadb")}, // lighter blue
            {"platform", new Color("39a371")}, // dark turquoise
        }
    };

    // public static Dictionary<string, Color> CurrentPalette {get; private set;} = 
    //         new Dictionary<string, Color>{};
    public Color bg1 = Colors.Black;
    public Color bg2 = Colors.Black;
    public Color fg = Colors.Black;
    public Color text = Colors.Black;
    public Color enemy = Colors.Black;
    public Color platform = Colors.Black;

    private static int CurrentIndex = -1; // To ensure that ShiftPalette leads to new color

    Tween tween;
    Material bg1Material;
    Material bg2Material;
    Material fgMaterial;
    Material textMaterial;
    Material enemyMaterial;


    public override void _Ready()
    {
        GD.Randomize();
        tween = GetNode<Tween>("Tween");

        ShiftPalette();

        bg1Material = GD.Load<Material>("res://RESOURCES/ColorMaterials/bg1.material");
        bg2Material = GD.Load<Material>("res://RESOURCES/ColorMaterials/bg2.material");
        fgMaterial = GD.Load<Material>("res://RESOURCES/ColorMaterials/fg.material");
        textMaterial = GD.Load<Material>("res://RESOURCES/ColorMaterials/text.material");
        enemyMaterial = GD.Load<Material>("res://RESOURCES/ColorMaterials/enemy.material");
    }

    public override void _PhysicsProcess(float delta)
    {
        if (HackerMode)
        {
            bg2 = Colors.Black;
            text = new Color(0.2f, 0.6f, 0.2f);
        }
        // else
        // {
        //     // enemyMaterial.Set("flags_transparent", true);
        //     // fgMaterial.Set("flags_transparent", true);
        // }

        bg1Material.Set("albedo_color", bg1);
        bg2Material.Set("albedo_color", bg2);
        fgMaterial.Set("albedo_color", fg);
        textMaterial.Set("albedo_color", text);
        enemyMaterial.Set("albedo_color", enemy);
    }

    public Color GetColorFromIndex(int i)
    {
        switch (i)
        {
            case (0): return bg1;
            case (1): return bg2;
            case (2): return fg;
            case (3): return text;
        }
        GD.Print("ERROR! Invalid color index (GlobalColors.cs)");
        return Colors.Black;
    }

    public void ShiftPalette()
    {
        if (HackerMode)
        {
            return;
        }
        int newIndex = (int)(GD.Randf()*colorPalettes.Length);
        while (newIndex == CurrentIndex)
        {
            newIndex = (int)(GD.Randf()*colorPalettes.Length);  
        }
        CurrentIndex = newIndex;
        Dictionary<string, Color> palette = colorPalettes[CurrentIndex];

        tween.InterpolateProperty(this, "bg1", bg1, palette["bg1"], 0.3f, 
                Tween.TransitionType.Sine, Tween.EaseType.Out);
        tween.InterpolateProperty(this, "bg2", bg2, palette["bg2"], 0.3f, 
                Tween.TransitionType.Sine, Tween.EaseType.Out);
        tween.InterpolateProperty(this, "fg", fg, palette["fg"], 0.3f, 
                Tween.TransitionType.Sine, Tween.EaseType.Out);
        tween.InterpolateProperty(this, "text", text, palette["text"], 0.3f, 
                Tween.TransitionType.Sine, Tween.EaseType.Out);
        tween.InterpolateProperty(this, "enemy", text, palette["enemy"], 0.3f, 
                Tween.TransitionType.Sine, Tween.EaseType.Out);
        tween.InterpolateProperty(this, "platform", text, palette["platform"], 0.3f, 
                Tween.TransitionType.Sine, Tween.EaseType.Out);
        tween.Start();

        // SceneTreeTween tween = GetTree().CreateTween();
        // CurrentPalette = (Dictionary<string, Color>)tween.InterpolateValue(CurrentPalette, colorPalettes[CurrentIndex], 0, 
        //         0.3f, Tween.TransitionType.Sine, Tween.EaseType.Out);
    }
}
