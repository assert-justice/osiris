
using System.IO;
using System.Text.Json.Nodes;
using Godot;

public partial class GameBrowserMenu: Menu{
    string listPath;
    VBoxContainer listContainer;
    public override void _Ready()
    {
        base._Ready();
        listContainer = GetNode<VBoxContainer>("VBox/ScrollContainer/VBoxContainer");
        var userPath = ProjectSettings.GlobalizePath("user://");
        listPath = Path.Join(userPath, "game_list.json");
        GetNode<Button>("VBox/HBox2/Back").ButtonDown += ()=>{
			menuSystem.PopMenu();
		};
        GetNode<Button>("VBox/HBox/Create").ButtonDown += ()=>{
			menuSystem.PushMenu("NewProject");
		};
        GetNode<Button>("VBox/HBox/Import").ButtonDown += ()=>{
			var dialogue = GetNode<FileDialog>("ImportProjectDialogue");
            dialogue.Visible = true;
            dialogue.Confirmed += () => {
                ImportGame(dialogue.CurrentPath);
            };
		};
        GetNode<Button>("VBox/HBox/ClearList").ButtonDown += ()=>{
			if(!File.Exists(listPath)) return;
            File.Delete(listPath);
            RefreshList();
		};
    }
    public override void OnWake()
    {
        RefreshList();
    }
    void RefreshList(){
        foreach (var child in listContainer.GetChildren())
        {
            child.QueueFree();
        }
        bool empty = true;
        if(File.Exists(listPath)){
            var node = JsonNode.Parse(File.ReadAllText(listPath));
            var list = node.AsObject()["games"].AsArray();
            if(list.Count > 0){
                empty = false;
                foreach(var item in list){
                    var game = item.AsObject();
                    var name = game["name"].ToString();
                    var path = game["path"].ToString();
                    var box = new HBoxContainer();
                    var nameLabel = new Label{
                        Text = name,
                    };
                    var pathLabel = new Label{
                        Text = $"Path: {path}",
                    };
                    var loadButton = new Button{
                        Text = "Load",
                    };
                    loadButton.ButtonDown += ()=>{
                        LoadGame(name);
                    };
                    var deleteButton = new Button{
                        Text = "Delete",
                    };
                    deleteButton.ButtonDown += ()=>{
                        DeleteGame(name);
                    };
                    box.AddChild(nameLabel);
                    box.AddChild(pathLabel);
                    box.AddChild(loadButton);
                    box.AddChild(deleteButton);
                    listContainer.AddChild(box);
                }
            }
        }
        if(empty){
            var label = new Label
            {
                Text = "Empty"
            };
            listContainer.AddChild(label);
        }
    }
    void LoadGame(string name){
        GD.Print($"Load {name}");
    }
    void DeleteGame(string name){
        var confirm = GetNode<ConfirmationDialog>("DeleteConfirmation");
        confirm.Visible = true;
        confirm.Confirmed += ()=>{
            var node = JsonNode.Parse(File.ReadAllText(listPath));
            var list = node.AsObject()["games"].AsArray();
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i].AsObject();
                if(item["name"].ToString() == name){
                    list.RemoveAt(i);
                    break;
                }
            }
            File.WriteAllText(listPath, node.ToString());
            RefreshList();
        };
    }
    void ImportGame(string path){
        GD.Print($"Import project at {path}");
        var node = JsonNode.Parse(File.ReadAllText(path)).AsObject();
        // Todo: Validate fields
        var name = node["name"].ToString();
        var listNode = JsonNode.Parse(File.ReadAllText(listPath));
        var list = listNode.AsObject()["games"].AsArray();
        var entryNode = JsonNode.Parse("{}").AsObject();
        entryNode["name"] = name;
        entryNode["path"] = path;
        list.Add(entryNode);
        File.WriteAllText(listPath, listNode.ToString());
        RefreshList();
    }
}