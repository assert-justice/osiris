using System.IO;
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
                GD.PrintErr("File path does not exist!");
                acceptDialog.Visible = true;
                acceptDialog.Title = "File path does not exist!";
                return;
            }
            // Validate name
            var name = projectName.Text;
            if(name.Length == 0){
                GD.PrintErr("Input valid project name!");
                acceptDialog.Visible = true;
                acceptDialog.Title = "Input valid project name!";
                return;
            }
            path = Path.Join(path, name);
            if(Directory.Exists(path)){
                GD.PrintErr("Directory already exists!");
                acceptDialog.Visible = true;
                acceptDialog.Title = "Directory already exists!";
                return;
            }
            // Add to saved games
            // Create project files and directories
            Directory.CreateDirectory(path);
            var fname = Path.Join(path, "manifest.json");
            // File.Create(Path.Join(path, "manifest.json"));
            var f = File.Open(fname, FileMode.OpenOrCreate);
            f.Write("{}".ToAsciiBuffer());
            f.Close();
        };
    }
}
