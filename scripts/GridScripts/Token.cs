using Godot;
using System;
using System.Linq;

public partial class Token : Mat
{
	protected override void GrabMenus()
	{
		// TODO: implement this
	}
	// [Export] public Texture2D Texture;
	// [Export] public int Speed = 5;
	// public Vector2I IPosition = new();
	// public Vector2I DragStart = new();
	// Grid grid;
	// public int CellWidth = 70;
	// public int Width = 1;
	// public int Height = 1;
	// bool IsDragging = false;
	// Line2D line;
	// public override void _Ready()
	// {
	// 	InputEvent += InputMethod;
	// }
	// public void SetData(Grid grid, int width, int height, Texture texture){
	// 	this.grid = grid;
	// 	CellWidth = grid.CellWidth;
	// 	Width = width;
	// 	Height = height;
	// 	var collider = GetNode<CollisionShape2D>("CollisionShape2D");
	// 	var shape = new RectangleShape2D();
	// 	shape.Size = new Vector2(CellWidth * width, CellWidth * height);
	// 	collider.Shape = shape;
	// 	var spr = GetNode<Sprite2D>("Sprite2D");
	// 	spr.Texture = (Texture2D)texture;
	// }
	// public override void _Process(double delta)
	// {
	// 	if(IsDragging){
	// 		var mPos = GetGlobalMousePosition();
	// 		Position = mPos;
	// 		var x = (int)Position.X / CellWidth;
	// 		var y = (int)Position.Y / CellWidth;
	// 		IPosition = new(x,y);
	// 		var points = grid.GetPath(IPosition)
	// 			.Select(p => new Vector2(p.X * CellWidth + CellWidth/2, p.Y * CellWidth + CellWidth/2))
	// 			.ToArray();
	// 		if(points.Count() > 0) line.Points = points;
	// 	}
	// }
	// void InputMethod(Node viewport, InputEvent @event, long shapeIdx){
	// 	if(@event is InputEventMouseButton mousButt){
	// 		if(mousButt.ButtonIndex != MouseButton.Left) return;
	// 		if (mousButt.Pressed && !IsDragging){
	// 			IsDragging = true;
	// 			line = grid.GetLine();
	// 			var x = (int)Position.X / CellWidth;
	// 			var y = (int)Position.Y / CellWidth;
	// 			DragStart = new(x,y);
	// 			grid.GenPaths(DragStart, Speed);
	// 		}
	// 		if (!mousButt.Pressed && IsDragging){
	// 			IsDragging = false;
	// 			grid.FreeLine(line);
	// 			line = null;
	// 			// Snap to grid
	// 			var x = (int)Position.X / CellWidth;
	// 			var y = (int)Position.Y / CellWidth;
	// 			x = x * CellWidth + CellWidth/2;
	// 			y = y * CellWidth + CellWidth/2;
	// 			Position = new Vector2(x,y);
	// 		}
	// 	}
	// }
}
