using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Menu : Node2D
{
    SceneTree tree;

    public override void _Ready()
    {
        tree = GetTree();
        for (int i = 0; i < GetChildren().Count; i++)
        {
            GD.Print(GetChild<Button>(i).Name);
            string scene = GetChild<Button>(i).Name;
            GetChild<Button>(i).ButtonUp += () => ChangeScene(scene);
        }

    }

    public void ChangeScene(string scene)
    {
        tree.ChangeSceneToFile("res://" + scene + ".tscn");
    }
}
