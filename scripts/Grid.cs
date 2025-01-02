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
	Pool<Line2D> LinePool = new(()=>new Line2D());
	Dictionary<Vector2I, GridNode> PathLookup = new();
	// Called when the node enters the scene tree for the first time.
	public Line2D GetLine(){
		return LinePool.GetNew();
	}
	public void FreeLine(Line2D line){
		LinePool.Free(line);
	}
	public override void _Ready()
	{
		LinePool.NewFn = ()=>{
			var line = new Line2D();
			GetChild(1).AddChild(line);
			return line;
		};
		LinePool.InitFn = (Line2D l)=>{
			l.DefaultColor = Colors.White;
		};
		LinePool.FreeFn = (Line2D l)=>{
			l.Points = Array.Empty<Vector2>();
		};
		foreach (var t in GetChild(0).GetChildren())
		{
			if (t is Token token){
				token.SetGrid(this);
			}
		}
		AddWall(9, 1, 9, 7);
		AddWall(9, 7, 11, 7);
		AddWall(9, 1, 14, 1);
		AddWall(14, 1, 14, 7);
		AddWall(14, 7, 12, 7);
		AddWall(12, 7, 12, 8);
		AddWall(11, 7, 11, 8);
		AddWall(11, 8, 8, 8);
		AddWall(8, 8, 8, 17);
		AddWall(12, 8, 15, 8);
		AddWall(15, 8, 15, 17);
		AddWall(15, 17, 8, 17);
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
	static bool IsIntersecting(Vector2I a, Vector2I b, Vector2I c, Vector2I d)
	{
		// Shamelessly stolen from here: https://gamedev.stackexchange.com/questions/26004/how-to-detect-2d-line-on-line-collision
		float denominator = ((b.X - a.X) * (d.Y - c.Y)) - ((b.Y - a.Y) * (d.X - c.X));
		float numerator1 = ((a.Y - c.Y) * (d.X - c.X)) - ((a.X - c.X) * (d.Y - c.Y));
		float numerator2 = ((a.Y - c.Y) * (b.X - a.X)) - ((a.X - c.X) * (b.Y - a.Y));

		// Detect coincident lines (has a problem, read below)
		if (denominator == 0) return numerator1 == 0 && numerator2 == 0;
		
		float r = numerator1 / denominator;
		float s = numerator2 / denominator;

		return (r >= 0 && r <= 1) && (s >= 0 && s <= 1);
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
	static GridNode PopLeast(ref Dictionary<Vector2I, GridNode> dict){
		var least = new GridNode(new Vector2I(0,0), new Vector2I(0,0), Mathf.Inf);
		if (dict.Count == 0){
			GD.PrintErr("Tried to pop from empty dict!");
			return least;
		}
		foreach (var val in dict.Values)
		{
			if (val.Cost < least.Cost){
				least = val;
			}
		}
		dict.Remove(least.Position);
		return least;
	}
	static void PushLeast(ref Dictionary<Vector2I,GridNode> dict, GridNode node){
		if(dict.ContainsKey(node.Position)){
			var comp = dict[node.Position];
			if (comp.Cost < node.Cost){return;}
		}
		dict[node.Position] = node;
	}
	public void GenPaths(Vector2I start, float range, float rangeInc = 0){
		if(rangeInc == 0)rangeInc = range;
		PathLookup.Clear();
		Vector2I[] directions = {
			new(1,0),
			new(0,-1),
			new(-1,0),
			new(0,1),
		};
		Vector2I[] diagonals = {
			new(1,-1),
			new(-1,-1),
			new(-1,1),
			new(1,1),
		};
		Dictionary<Vector2I, GridNode> open = new()
		{
			{ start, new GridNode(start, start, 0) }
		};
		while (open.Count > 0){
			var node = PopLeast(ref open);
			if(node.Cost > range){break;}
			PushLeast(ref PathLookup, node);
			foreach (var dir in directions)
			{
				var pos = new Vector2I(dir.X + node.Position.X, dir.Y + node.Position.Y);
				var cost = node.Cost + 1; // Todo: implement difficult terrain
				var next = new GridNode(pos, node.Position, cost);
				PushLeast(ref open, next);
			}
			foreach (var dir in diagonals)
			{
				var pos = new Vector2I(dir.X + node.Position.X, dir.Y + node.Position.Y);
				var cost = node.Cost + 1.5f; // Todo: implement difficult terrain
				var next = new GridNode(pos, node.Position, cost);
				PushLeast(ref open, next);
			}
		}
		// GD.Print(PathLookup.Count);
	}
	public Vector2I[] GetPath(Vector2I dest){
		if(!PathLookup.ContainsKey(dest)){
			return Array.Empty<Vector2I>();
		}
		var temp = new List<Vector2I>();
		var node = PathLookup[dest];
		while(true){
			temp.Add(node.Position);
			if (node.Cost == 0) break;
			node = PathLookup[node.Parent];
		}
		return temp.ToArray();
	}
}
