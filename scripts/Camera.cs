using Godot;
using System;

public partial class Camera : Camera2D
{
	[Export] public float PanSpeed = 300;
	[Export] public float ZoomSpeed = 1;
	[Export] public float MaxZoom = 10;
	[Export] public float MinZoom = 0.1f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float dt = (float)delta;
		var move = Input.GetVector("pan_left", "pan_right", "pan_up", "pan_down");
		Position += move * dt * PanSpeed;
		var deltaZoom = Input.GetAxis("zoom_out", "zoom_in");
		if (Input.IsActionJustPressed("scroll_up")){deltaZoom = 1;}
		if (Input.IsActionJustPressed("scroll_down")){deltaZoom = -1;}
		deltaZoom *= ZoomSpeed * dt;
		Zoom += new Vector2(deltaZoom, deltaZoom);
	}
}