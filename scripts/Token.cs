using Godot;
using System;
using System.Linq;

public partial class Token : Area2D
{
	[Export] public Texture2D Texture;
	[Export] public int Speed = 5;
	public Vector2I IPosition = new();
	public Vector2I DragStart = new();
	Grid grid;
	public int CellWidth = 70;
	public int Width = 1;
	bool IsDragging = false;
	Line2D line;
	public override void _Ready()
	{
		InputEvent += InputMethod;
	}
	public void SetGrid(Grid grid){
		this.grid = grid;
		CellWidth = grid.CellWidth;
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

	public override void _Process(double delta)
	{
		if(IsDragging){
			var mPos = GetGlobalMousePosition();
			Position = mPos;
			var x = (int)Position.X / CellWidth;
			var y = (int)Position.Y / CellWidth;
			IPosition = new(x,y);
			var points = grid.GetPath(IPosition)
				.Select(p => new Vector2(p.X * CellWidth + CellWidth/2, p.Y * CellWidth + CellWidth/2))
				.ToArray();
			if(points.Count() > 0) line.Points = points;
		}
	}
	void InputMethod(Node viewport, InputEvent @event, long shapeIdx){
		if(@event is InputEventMouseButton mousButt){
			if(mousButt.ButtonIndex != MouseButton.Left) return;
			if (mousButt.Pressed && !IsDragging){
				IsDragging = true;
				line = grid.GetLine();
				var x = (int)Position.X / CellWidth;
				var y = (int)Position.Y / CellWidth;
				DragStart = new(x,y);
				grid.GenPaths(DragStart, Speed);
			}
			if (!mousButt.Pressed && IsDragging){
				IsDragging = false;
				grid.FreeLine(line);
				line = null;
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
