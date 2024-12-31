using Godot;
using System;
using System.Collections.Generic;

public partial class Grid : Node2D
{
	[Export] public int Width = 25;
	[Export] public int Height = 25;
	[Export] public int CellWidth = 70;
	[Export] public Color GridColor = Colors.White;
	[Export] public Texture2D BaseTexture;
	[Export] public bool BaseTextureTiles = false;
	[Export] public bool ShowGrid = true;
	[Export] public bool ShowWalls = true;
	List<(Vector2I, Vector2I)> Walls = new();
	Dictionary<Vector2I, List<int>> WallCache = new();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddWall(9, 1, 9, 7);
		AddWall(9, 7, 11, 7);
		AddWall(9, 1, 14, 1);
		AddWall(14, 1, 14, 7);
		AddWall(14, 7, 12, 7);
		foreach ((var v0, var v1) in Walls)
		{
			var x0 = v0.X * CellWidth;
			var y0 = v0.Y * CellWidth;
			var x1 = v1.X * CellWidth;
			var y1 = v1.Y * CellWidth;
			Vector2[] vectors = {new Vector2(x0,y0), new Vector2(x1,y1)};
			var occluder = new LightOccluder2D();
			occluder.Occluder = new();
			occluder.Occluder.Polygon = vectors;
			AddChild(occluder);
		}
	}
	void AddWall(int x0, int y0, int x1, int y1){
		Walls.Add((new Vector2I(x0,y0), new Vector2I(x1,y1)));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Draw()
	{
		var width = Width * CellWidth;
		var height = Height * CellWidth;
		DrawTextureRect(BaseTexture, new Rect2(0, 0, width, height), BaseTextureTiles);
		if(ShowGrid){
			for (int y = 0; y < Height + 1; y++){
				DrawLine(new Vector2(0, y * CellWidth), new Vector2(width, y * CellWidth), GridColor);
			}
			for (int x = 0; x < Width + 1; x++){
				DrawLine(new Vector2(x * CellWidth,0), new Vector2(x * CellWidth,height), GridColor);
			}
		}
		if(ShowWalls){
			foreach ((var v0, var v1) in Walls)
			{
				var x0 = v0.X * CellWidth;
				var y0 = v0.Y * CellWidth;
				var x1 = v1.X * CellWidth;
				var y1 = v1.Y * CellWidth;
				DrawLine(new Vector2(x0,y0), new Vector2(x1,y1), Colors.Red);
			}
		}
	}
}
