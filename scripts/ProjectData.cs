
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;

public struct MatData{
    public string TexPath;
    public int X;
    public int Y;
    public int Width;
    public int Height;
    public MatData(JsonObject node){
        TexPath = node["texture"].ToString();
        X = int.Parse(node["x"].ToString());
        Y = int.Parse(node["y"].ToString());
        Width = int.Parse(node["width"].ToString());
        Height = int.Parse(node["height"].ToString());
    }
}
public class MapData{
    public string Name;
    public int Width;
    public int Height;
    public int CellWidth;
    public List<MatData> Mats = new();
    public MapData(JsonObject node){
        Name = node["name"].ToString();
        Width = int.Parse(node["width"].ToString());
        Height = int.Parse(node["height"].ToString());
        CellWidth = int.Parse(node["cell_width"].ToString());
        foreach(var n in node["mats"].AsArray()){
            Mats.Add(new(n.AsObject()));
        }
    }
}

public class ProjectData{
    public string Name;
    public Dictionary<string, string> TextureLookup = new();
    public List<MapData> Maps = new();
    public ProjectData(string path){
        var node = JsonNode.Parse(File.ReadAllText(path));
        Name = node["name"].ToString();
        // TexturePath = Path.Join(path, node["texture_path"].ToString());
    }
}