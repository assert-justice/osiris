using System.IO;
using System.Text.Json.Nodes;
using Godot;

public partial class NewProjectMenu : Menu
{
    TextEdit projectName;
    TextEdit projectPath;
    FileDialog fileDialog;
    AcceptDialog acceptDialog;
    public override void _Ready()
    {
        base._Ready();
        projectName = GetNode<TextEdit>("NameEdit");
        projectPath = GetNode<TextEdit>("PathEdit");
        fileDialog = GetNode<FileDialog>("CreateProjectDialogue");
        fileDialog.Confirmed += ()=>{
            projectPath.Text = fileDialog.CurrentPath;
        };
        acceptDialog = GetNode<AcceptDialog>("AcceptDialog");
        GetNode<Button>("Cancel").ButtonDown += ()=>{menuSystem.PopMenu();};
        GetNode<Button>("SetPath").ButtonDown += ()=>{fileDialog.Visible = true;};
        GetNode<Button>("Confirm").ButtonDown += ()=>{
            // Validate path
            var path = projectPath.Text;
            if(!Directory.Exists(path)){
                ShowError("File path does not exist!");
                return;
            }
            // Validate name
            var name = projectName.Text;
            if(name.Length == 0){
                ShowError("Input valid project name!");
                return;
            }
            path = Path.Join(path, name);
            if(Directory.Exists(path)){
                ShowError("Directory already exists!");
                return;
            }
            // Add to saved games
            var userPath = ProjectSettings.GlobalizePath("user://");
            userPath = Path.GetFullPath(userPath);
            // GD.Print(userPath);
            var listPath = Path.Join(userPath, "game_list.json");
            JsonNode node;
            if(File.Exists(listPath)){
                var listFile = File.ReadAllText(listPath);
                node = JsonNode.Parse(listFile);
            }
            else{
                node = JsonNode.Parse("{}");
                var obj = node.AsObject();
                obj.Add("games", JsonNode.Parse("[]"));
            }
            var gameObj = JsonNode.Parse("{}").AsObject();
            gameObj["name"] = name;
            gameObj["path"] = Path.Join(path, "manifest.json");
            var gameList = node.AsObject()["games"].AsArray();
            gameList.Add(gameObj);
            GD.Print(node.ToJsonString());
            File.WriteAllText(listPath, node.ToJsonString());
            // Create project files and directories
            Directory.CreateDirectory(path);
            var fname = Path.Join(path, "manifest.json");
            var newNode = JsonNode.Parse("{}").AsObject();
            newNode["name"] = name;
            newNode["version"] = "0.1.0";
            newNode["author"] = "author";
            newNode["map_path"] = "maps";
            newNode["texture_path"] = "textures";
            File.WriteAllText(fname, newNode.ToJsonString());
            menuSystem.PopMenu();
        };
    }
    void ShowError(string message){
        GD.PrintErr(message);
        acceptDialog.Visible = true;
        acceptDialog.Title = message;
    }
}
