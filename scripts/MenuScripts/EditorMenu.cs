using Godot;
using System;

public partial class EditorMenu : Menu
{
    public string ProjectPath;
    public override void OnWake()
    {
        // Load project
        GD.Print($"Load project at path {ProjectPath}");
    }
}
