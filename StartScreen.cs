using Godot;
using System;

public class StartScreen : Control
{
    [Export] Vector2 circleSelectPosition1 = Vector2.Zero;
    [Export] Vector2 circleSelectPosition2 = Vector2.Zero;
    [Export] Vector2 circleSelectPosition3 = Vector2.Zero;

    bool hackerModeUnlocked = false;

    protected AnimatedSprite circleSelect;
    protected Label hackerMode;
    protected Godot.Node save;


    public override void _Ready()
    {
        circleSelect = GetNode<AnimatedSprite>("CircleSelect");
        hackerMode = GetNode<Label>("HackerMode");
        save = GetNode<Godot.Node>("/root/Save");

        hackerModeUnlocked = (float)save.Call("load_game") > 50000;
        if (!hackerModeUnlocked)
        {
            hackerMode.Modulate = new Color(1,1,1,0.2f);
        }
    }


}
