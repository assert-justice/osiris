using Godot;
using System;

public partial class Token : Area2D
{
	[Export] public Texture2D Texture;
	[Export] public int Speed = 5;
	Grid grid;
	public int CellWidth = 70;
	public int Width = 1;
	bool IsDragging = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		grid = GetParent<Grid>();
		CellWidth = grid.CellWidth;
		InputEvent += InputMethod;
		var collider = GetChild<CollisionShape2D>(1);
		var shape = new RectangleShape2D();
		shape.Size = new Vector2(CellWidth,CellWidth);
		collider.Shape = shape;
	}
	public override void _Draw()
	{
		var width = CellWidth * Width;
		DrawTextureRect(Texture, new Rect2(-width/2.0f,-width/2.0f,width,width), false);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!IsDragging)return;
		var mPos = GetGlobalMousePosition();
		Position = mPos;
	}
	void InputMethod(Node viewport, InputEvent @event, long shapeIdx){
		if(@event is InputEventMouseButton mousButt){
			if (mousButt.Pressed && !IsDragging){
				IsDragging = true;
			}
			if (!mousButt.Pressed && IsDragging){
				IsDragging = false;
				// Snap to grid
				var x = (int)Position.X / CellWidth;
				var y = (int)Position.Y / CellWidth;
				x = x * CellWidth + CellWidth/2;
				y = y * CellWidth + CellWidth/2;
				Position = new Vector2(x,y);
			}
		}
	}
}
