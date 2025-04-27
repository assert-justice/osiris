using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Grid : Area2D
{
	[Export] PackedScene MatScene;
	[Export] Texture MatTexture;
	[Export] int Width = 21;
	[Export] int Height = 44;
	[Export] int CellWidth = 70;
	[Export] Color GridColor = Colors.White;
	[Export] Texture2D BaseTexture;
	[Export] bool BaseTextureTiles = false;
	[Export] bool ShowGrid = true;
	[Export] bool ShowWalls = true;
	List<(Vector2I, Vector2I)> Walls = new();
	// Dictionary<Vector2I, List<int>> WallCache = new();
	Pool<Line2D> LinePool;
	Pool<LightOccluder2D> OccluderPool;
	Dictionary<Vector2I, GridNode> PathLookup = [];
	Camera camera;
	bool IsDragging = false;
	public Line2D GetLine(){
		return LinePool.GetNew();
	}
	public void FreeLine(Line2D line){
		LinePool.Free(line);
	}
	public void Cleanup(){
		foreach (var item in GetNode("Mats").GetChildren())
		{
			item.QueueFree();
		}
		foreach (var item in GetNode("Tokens").GetChildren())
		{
			item.QueueFree();
		}
		OccluderPool.FreeAll();
		LinePool.FreeAll();
	}
	public override void _Ready()
	{
		var collider = GetNode<CollisionShape2D>("CollisionShape2D");
		var shape = new RectangleShape2D
		{
			Size = new Vector2(CellWidth * Width, CellWidth * Height)
		};
		collider.Position = new Vector2(CellWidth * Width/2,CellWidth * Height/2);
		collider.Shape = shape;
		camera = GetNode<Camera>("Camera2D");
		InputEvent += InputMethod;
		LinePool = new(() =>
		{
			var line = new Line2D();
			GetNode("Lines").AddChild(line);
			return line;
		})
		{
			InitFn = (Line2D l) =>{
				l.DefaultColor = Colors.White;
			},
			FreeFn = (Line2D l) =>{
				l.Points = [];
			}
		};
		OccluderPool = new(() => {
			var oc = new LightOccluder2D();
			GetNode("Occluders").AddChild(oc);
			return oc;
		}){
			InitFn = (LightOccluder2D oc)=>{
				oc.Visible = true;
			},
			FreeFn = (LightOccluder2D oc)=>{
				oc.Visible = false;
			}
		};
		Example();
	}
	void Example(){
		var mat = MatScene.Instantiate<Mat>();
		GetNode("Mats").AddChild(mat);
		mat.SetStats(CellWidth, Width, Height, (Texture2D)MatTexture);
	}
	void PlaceOccluder(int x0, int y0, int x1, int y1){
		Vector2[] vectors = {new(x0,y0), new(x1,y1)};
		var oc = OccluderPool.GetNew();
		oc.Occluder = new()
		{
			Polygon = vectors
		};
	}
	void PlaceMat(Image img, int x, int y, int w, int h){
		var tex = ImageTexture.CreateFromImage(img);
		var spr = new Sprite2D();
		GetNode("Mats").AddChild(spr);
		spr.Texture = tex;
	}
	void InputMethod(Node viewport, InputEvent @event, long shapeIdx){
		if(@event is InputEventMouseMotion mouseMotion){
			if(IsDragging){
				camera.Pan(-mouseMotion.Relative);
			}
		}
		if(@event is InputEventMouseButton mouseButt){
			if(mouseButt.ButtonIndex != MouseButton.Right) return;
			if (mouseButt.Pressed && !IsDragging){
				IsDragging = true;
			}
			if (!mouseButt.Pressed && IsDragging){
				IsDragging = false;
			}
		}
	}
	void AddWall(int x0, int y0, int x1, int y1){
		Walls.Add((new Vector2I(x0,y0), new Vector2I(x1,y1)));
	}
	static bool IsIntersecting(Vector2I wallStart, Vector2I wallEnd, Vector2I pathStart, Vector2I pathEnd)
	{
		Vector2 a = new(wallStart.X, wallStart.Y);
		Vector2 b = new(wallEnd.X, wallEnd.Y);
		Vector2 c = new(pathStart.X + 0.5f, pathStart.Y + 0.5f);
		Vector2 d = new(pathEnd.X + 0.5f, pathEnd.Y + 0.5f);

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

	public override void _Process(double delta)
	{
	}

	// public override void _Draw()
	// {
	// 	var width = Width * CellWidth;
	// 	var height = Height * CellWidth;
	// 	DrawTextureRect(BaseTexture, new Rect2(0, 0, width, height), BaseTextureTiles);
	// 	if(ShowGrid){
	// 		for (int y = 0; y < Height + 1; y++){
	// 			DrawLine(new Vector2(0, y * CellWidth), new Vector2(width, y * CellWidth), GridColor);
	// 		}
	// 		for (int x = 0; x < Width + 1; x++){
	// 			DrawLine(new Vector2(x * CellWidth,0), new Vector2(x * CellWidth,height), GridColor);
	// 		}
	// 	}
	// 	if(ShowWalls){
	// 		foreach ((var v0, var v1) in Walls)
	// 		{
	// 			var x0 = v0.X * CellWidth;
	// 			var y0 = v0.Y * CellWidth;
	// 			var x1 = v1.X * CellWidth;
	// 			var y1 = v1.Y * CellWidth;
	// 			DrawLine(new Vector2(x0,y0), new Vector2(x1,y1), Colors.Red);
	// 		}
	// 	}
	// }
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
				var deltaCost = dir.X == 0 || dir.Y == 0 ? 1.0f : 1.5f;
				var cost = node.Cost + deltaCost; // Todo: implement difficult terrain
				var next = new GridNode(pos, node.Position, cost);
				if(Walls.Any((w)=>{
					var (a,b) = w;
					return IsIntersecting(a, b, pos, node.Position);
				})){continue;}
				PushLeast(ref open, next);
			}
		}
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
