using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

public partial class RobotBuilder : Node2D
{
    TileMapLayer partsMap;
    TileMapLayer robotMap;
    TileMapLayer previewMap;

    private int selectedPart = -1;
    private int rotation = 0;
    private string selectedRobot = "";
    private Parts.Part viewedPart;
    private bool canLoadBindings = true;

    Node2D UI;
    OptionButton robotDropdown;

    Node2D configurePart;
    Node2D linkingGroups;
    ScrollContainer bindings;


    public List<Parts.Part> robot = [];


    public void UpdateRobot()
    {
        robotMap.Clear();
        for (int i = 0; i < robot.Count; i++)
        {
            for (int i2 = 0; i2 < robot[i].GetPoints().Count; i2++)
            {
                robotMap.SetCell(robot[i].location + robot[i].GetPoints()[i2] - robot[i].origin, robot[i].id, robot[i].points[i2], Parts.rotations[robot[i].rotation]);
            }
        }
    }

    public void UpdateLinkingGroup(int index)
    {
        if (!viewedPart.linkingGroups.Remove(index))
            viewedPart.linkingGroups.Add(index);
        linkingGroups.GetChild<Button>(index).GetChild<Sprite2D>(0).Frame = viewedPart.linkingGroups.Contains(index) ? 1 : 0;
    }

    public void LoadBindings()
    {
        if (bindings.GetChildren().Count > 1)
            bindings.GetChild<VBoxContainer>(1).QueueFree();
        VBoxContainer newBindings = (VBoxContainer)bindings.GetChild<VBoxContainer>(0).Duplicate();
        newBindings.Show();
        int i = 0;
        foreach (Parts.Binding binding in viewedPart.bindings)
        {
            Node2D bindingNode = (Node2D)newBindings.GetChild<Node2D>(0).Duplicate();

            bindingNode.Show();
            bindingNode.Position += new Vector2(0, i * 150);
            Button delete = bindingNode.GetChild<Button>(0);
            CheckButton comparison = bindingNode.GetChild<CheckButton>(1);
            Node2D conditional = bindingNode.GetChild<Node2D>(2);
            Node2D statement = bindingNode.GetChild<Node2D>(3);
            comparison.ButtonPressed = binding.comparison;
            conditional.GetChild<Button>(0).Text = Parts.Binding.typeOptions[binding.types[0]];
            conditional.GetChild<LineEdit>(1).Text = binding.inputs[0];
            conditional.GetChild<Button>(2).Text = binding.operands[0];
            conditional.GetChild<Button>(3).Text = Parts.Binding.typeOptions[binding.types[1]];
            conditional.GetChild<LineEdit>(4).Text = binding.inputs[1];
            if (!binding.comparison)
                conditional.Hide();
            statement.GetChild<Button>(0).Text = Parts.Binding.typeOptions[binding.types[2]];
            statement.GetChild<LineEdit>(1).Text = binding.inputs[2];
            statement.GetChild<Button>(2).Text = binding.operands[1];
            statement.GetChild<Button>(3).Text = Parts.Binding.typeOptions[binding.types[3]];
            statement.GetChild<LineEdit>(4).Text = binding.inputs[3];

            int index = i;
            delete.Pressed += () => RemoveBinding(index);
            comparison.Pressed += () => ToggleConditional(index);
            conditional.GetChild<Button>(0).Pressed += () => IncrementType(index, 0);
            conditional.GetChild<Button>(2).Pressed += () => IncrementOperand1(index);
            conditional.GetChild<Button>(3).Pressed += () => IncrementType(index, 1);
            statement.GetChild<Button>(0).Pressed += () => IncrementType(index, 2);
            statement.GetChild<Button>(2).Pressed += () => IncrementOperand2(index);
            statement.GetChild<Button>(3).Pressed += () => IncrementType(index, 3);
            conditional.GetChild<LineEdit>(1).FocusExited += () => SetInput(index, 0, conditional.GetChild<LineEdit>(1).Text);
            conditional.GetChild<LineEdit>(4).FocusExited += () => SetInput(index, 1, conditional.GetChild<LineEdit>(4).Text);
            statement.GetChild<LineEdit>(1).FocusExited += () => SetInput(index, 2, statement.GetChild<LineEdit>(1).Text);
            statement.GetChild<LineEdit>(4).FocusExited += () => SetInput(index, 3, statement.GetChild<LineEdit>(4).Text);

            newBindings.AddChild(bindingNode);

            i++;
        }
        newBindings.GetChild<Node2D>(1).Position += new Vector2(0, i * 150);
        newBindings.GetChild<Node2D>(1).GetChild<Button>(0).Pressed += AddBinding;
        newBindings.CustomMinimumSize = new Vector2(1000, 150 + i * 150);
        bindings.AddChild(newBindings);
        canLoadBindings = true;
    }

    public void AddBinding()
    {
        viewedPart.bindings.Add(new Parts.Binding());
        LoadBindings();
    }

    public void RemoveBinding(int index)
    {
        viewedPart.bindings.RemoveAt(index);
        LoadBindings();
    }

    public void ToggleConditional(int index)
    {
        viewedPart.bindings[index].comparison = !viewedPart.bindings[index].comparison;
        LoadBindings();
    }

    public void IncrementType(int index, int type)
    {
        viewedPart.bindings[index].types[type] += 1;
        viewedPart.bindings[index].types[type] = viewedPart.bindings[index].types[type] > 3 ? 0 : viewedPart.bindings[index].types[type];
        if (type == 2 && viewedPart.bindings[index].types[type] > 1)
            viewedPart.bindings[index].types[type] = 0;
        LoadBindings();
    }

    public void IncrementOperand1(int index)
    {
        if (viewedPart.bindings[index].operands[0] == "==")
            viewedPart.bindings[index].operands[0] = ">";
        else if (viewedPart.bindings[index].operands[0] == ">")
            viewedPart.bindings[index].operands[0] = "<";
        else if (viewedPart.bindings[index].operands[0] == "<")
            viewedPart.bindings[index].operands[0] = ">=";
        else if (viewedPart.bindings[index].operands[0] == ">=")
            viewedPart.bindings[index].operands[0] = "<=";
        else if (viewedPart.bindings[index].operands[0] == "<=")
            viewedPart.bindings[index].operands[0] = "==";
        LoadBindings();
    }

    public void IncrementOperand2(int index)
    {
        if (viewedPart.bindings[index].operands[1] == "=")
            viewedPart.bindings[index].operands[1] = "+=";
        else if (viewedPart.bindings[index].operands[1] == "+=")
            viewedPart.bindings[index].operands[1] = "-=";
        else if (viewedPart.bindings[index].operands[1] == "-=")
            viewedPart.bindings[index].operands[1] = "*=";
        else if (viewedPart.bindings[index].operands[1] == "*=")
            viewedPart.bindings[index].operands[1] = "/=";
        else if (viewedPart.bindings[index].operands[1] == "/=")
            viewedPart.bindings[index].operands[1] = "=";
        LoadBindings();
    }

    public void SetInput(int index, int input, string value)
    {
        if (canLoadBindings)
        {
            canLoadBindings = false;
            viewedPart.bindings[index].inputs[input] = value;
            LoadBindings();
        }
    }

    public void Save()
    {
        List<Dictionary<string, dynamic>> data = [];
        foreach (Parts.Part part in robot)
        {
            data.Add(part.GetJSON());
        }
        Godot.FileAccess bot = Godot.FileAccess.Open("user://robots/" + robotDropdown.Text + ".robot", Godot.FileAccess.ModeFlags.Write);
        bot.StoreLine(JsonSerializer.Serialize(data));
        bot.Close();
    }

    public void Load()
    {
        Godot.FileAccess botRead = Godot.FileAccess.Open("user://robots/" + robotDropdown.Text + ".robot", Godot.FileAccess.ModeFlags.Read);
        string text = botRead.GetAsText().TrimPrefix("[").TrimSuffix("\n").TrimSuffix("]");
        botRead.Close();
        string[] partList = text.Split("},{");
        for (int i = 0; i < partList.Length; i++)
        {
            if (!partList[i].StartsWith('{'))
                partList[i] = "{" + partList[i];
            if (!partList[i].EndsWith('}'))
                partList[i] = partList[i] + "}";
        }
        robot = [];
        foreach (string partString in partList)
        {
            robot.Add(new Parts.Part(partString));
        }
        configurePart.Hide();
        UpdateRobot();
    }

    public void Rename()
    {
        if (!(UI.GetChild<LineEdit>(1).Text == "New Robot"))
            DirAccess.Open("user://robots").Rename(robotDropdown.Text + ".robot", UI.GetChild<LineEdit>(1).Text + ".robot");
        Godot.FileAccess userData = Godot.FileAccess.Open("user://user.dat", Godot.FileAccess.ModeFlags.Read);
        string[] userDataArray = userData.GetAsText().Split(",\n");
        for (int i = 0; i < userDataArray.Length; i++)
        {
            if (userDataArray[i].StartsWith("selectedRobot"))
                userDataArray[i] = "selectedRobot=" + UI.GetChild<LineEdit>(1).Text;
        }
        userData.Close();
        Godot.FileAccess userDataWrite = Godot.FileAccess.Open("user://user.dat", Godot.FileAccess.ModeFlags.Write);
        userDataWrite.StoreLine(userDataArray.Join(",\n"));
        userDataWrite.Close();
        LoadSelectedRobot();
    }

    public void Delete()
    {
        DirAccess.Open("user://robots").Remove(robotDropdown.Text + ".robot");
        Godot.FileAccess userData = Godot.FileAccess.Open("user://user.dat", Godot.FileAccess.ModeFlags.Read);
        string[] userDataArray = userData.GetAsText().Split(",\n");
        List<string> userDataList = [];
        for (int i = 0; i < userDataArray.Length; i++)
        {
            if (!userDataArray[i].StartsWith("selectedRobot"))
                userDataList.Add(userDataArray[i]);
        }
        userData.Close();
        Godot.FileAccess userDataWrite = Godot.FileAccess.Open("user://user.dat", Godot.FileAccess.ModeFlags.Write);
        userDataWrite.StoreLine(userDataList.ToArray().Join(",\n"));
        userDataWrite.Close();
        LoadSelectedRobot();
    }

    public void Select()
    {
        if (robotDropdown.Text == "New Robot")
        {
            int number = 1;
            bool run = true;
            while (run)
            {
                run = false;
                for (int i = 0; i < robotDropdown.ItemCount; i++)
                    if (robotDropdown.GetItemText(i) == ("Untitled " + number.ToString()))
                        run = true;
                if (run)
                    number += 1;
            }
            robotDropdown.AddItem("Untitled " + number.ToString());
            robotDropdown.Select(robotDropdown.ItemCount - 1);
            Save();
            Godot.FileAccess userData = Godot.FileAccess.Open("user://user.dat", Godot.FileAccess.ModeFlags.Read);
            string[] userDataArray = userData.GetAsText().Split(",\n");
            for (int i = 0; i < userDataArray.Length; i++)
            {
                if (userDataArray[i].StartsWith("selectedRobot"))
                    userDataArray[i] = "selectedRobot=" + "Untitled " + number.ToString();
            }
            userData.Close();
            Godot.FileAccess userDataWrite = Godot.FileAccess.Open("user://user.dat", Godot.FileAccess.ModeFlags.Write);
            userDataWrite.StoreLine(userDataArray.Join(",\n"));
            userDataWrite.Close();
            LoadSelectedRobot();
        }
    }

    public void LoadSelectedRobot()
    {
        DirAccess userFolder = DirAccess.Open("user://");
        if (!userFolder.GetFiles().Contains("user.dat"))
        {
            DirAccess.CopyAbsolute("res://Kitbot.robot", "user://robots/Kitbot.robot");
            Godot.FileAccess userData = Godot.FileAccess.Open("user://user.dat", Godot.FileAccess.ModeFlags.Write);
            userData.StoreLine("selectedRobot=Kitbot");
            userData.Close();
        }
        DirAccess robotFolder = DirAccess.Open("user://robots");
        robotDropdown.Clear();
        foreach (string file in robotFolder.GetFiles())
            robotDropdown.AddItem(file.TrimSuffix(".robot"));
        robotDropdown.AddItem("New Robot");

        Godot.FileAccess userDataRead = Godot.FileAccess.Open("user://user.dat", Godot.FileAccess.ModeFlags.Read);
        if (!userDataRead.GetAsText().Contains("selectedRobot"))
        {
            Godot.FileAccess userData = Godot.FileAccess.Open("user://user.dat", Godot.FileAccess.ModeFlags.Write);
            userData.StoreLine("selectedRobot=" + robotDropdown.Text);
            userData.Close();
        }
        userDataRead.Close();
        Godot.FileAccess confirmedUserData = Godot.FileAccess.Open("user://user.dat", Godot.FileAccess.ModeFlags.Read);
        string[] points = confirmedUserData.GetAsText().Split(",\n");
        foreach (string point in points)
            if (point.Split("=")[0] == "selectedRobot")
                selectedRobot = point.Split("=")[1].Trim();
        confirmedUserData.Close();
        GD.Print("Robot: " + selectedRobot);
        for (int i = 0; i < robotDropdown.ItemCount; i++)
            if (robotDropdown.GetItemText(i) == selectedRobot)
                robotDropdown.Select(i);

        Load();
    }

    public override void _Ready()
    {
        partsMap = this.GetChild<TileMapLayer>(0);
        robotMap = this.GetParent().GetChild(2).GetChild<TileMapLayer>(0);
        previewMap = this.GetParent().GetChild(2).GetChild<TileMapLayer>(1);

        configurePart = GetParent().GetChild<Node2D>(3);
        linkingGroups = configurePart.GetChild<Node2D>(3);
        bindings = configurePart.GetChild<ScrollContainer>(4);

        UI = GetParent().GetChild<Node2D>(4);
        robotDropdown = UI.GetChild<OptionButton>(0);

        for (int i = 0; i < linkingGroups.GetChildren().Count; i++)
        {
            int index = i;
            linkingGroups.GetChild<Button>(i).Pressed += () => UpdateLinkingGroup(index);
        }

        DirAccess userFolder = DirAccess.Open("user://");
        if (!userFolder.DirExists("robots"))
            userFolder.MakeDir("robots");

        LoadSelectedRobot();

        robotDropdown.ItemSelected += (long index) => Select();
        UI.GetChild<Button>(2).Pressed += () => Save();
        UI.GetChild<Button>(3).Pressed += () => Load();
        UI.GetChild<Button>(4).Pressed += () => Rename();
        UI.GetChild<Button>(5).Pressed += () => Delete();
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("RotatePart"))
        {
            rotation += 1;
            if (rotation > 3)
                rotation = 0;
            if (selectedPart > 0)
                Parts.partList[selectedPart].rotation = Parts.partList[selectedPart].directional ? rotation : 0;
        }
        if (Input.IsActionJustPressed("LMB"))
        {
            Vector2I robotLocation = robotMap.LocalToMap(robotMap.ToLocal(GetGlobalMousePosition()));
            if (robotLocation.X >= -8 && robotLocation.Y >= -8 && robotLocation.X <= 7 && robotLocation.Y <= 7)
            {
                if (selectedPart > 0)
                {
                    bool valid = true;
                    for (int i = 0; i < Parts.partList[selectedPart].GetPoints().Count; i++)
                    {
                        Vector2I pointLocation = robotLocation + Parts.partList[selectedPart].GetPoints()[i] - Parts.partList[selectedPart].origin;
                        if (pointLocation.X < -8 || pointLocation.Y < -8 || pointLocation.X > 7 || pointLocation.Y > 7)
                            valid = false;
                    }
                    if (valid)
                    {
                        for (int i = 0; i < Parts.partList[selectedPart].GetPoints().Count; i++)
                        {
                            Vector2I pointLocation = robotLocation + Parts.partList[selectedPart].GetPoints()[i] - Parts.partList[selectedPart].origin;
                            for (int i2 = 0; i2 < robot.Count; i2++)
                            {
                                bool marked = false;
                                for (int i3 = 0; i3 < robot[i2].GetPoints().Count; i3++)
                                    if ((robot[i2].location + robot[i2].GetPoints()[i3] - robot[i2].origin) == pointLocation)
                                        marked = true;
                                if (marked)
                                    robot.RemoveAt(i2);
                            }
                        }
                        robot.Add(Parts.partList[selectedPart].Copy());
                        robot[^1].location = robotLocation;
                    }
                }
                if (!(robotMap.GetCellSourceId(robotLocation) == -1))
                {
                    if (selectedPart == 0)
                    {
                        for (int i = 0; i < robot.Count; i++)
                        {
                            bool marked = false;
                            for (int i2 = 0; i2 < robot[i].GetPoints().Count; i2++)
                                if ((robot[i].location + robot[i].GetPoints()[i2] - robot[i].origin) == robotLocation)
                                    marked = true;
                            if (marked)
                                robot.RemoveAt(i);
                        }
                    }
                    else if (selectedPart == -1)
                    {
                        for (int i = 0; i < robot.Count; i++)
                        {
                            bool marked = false;
                            for (int i2 = 0; i2 < robot[i].GetPoints().Count; i2++)
                                if ((robot[i].location + robot[i].GetPoints()[i2] - robot[i].origin) == robotLocation)
                                    marked = true;
                            if (marked)
                                viewedPart = robot[i];
                        }
                        configurePart.GetChild<RichTextLabel>(1).Text = viewedPart.name;
                        configurePart.GetChild<RichTextLabel>(2).Text = "[right]" + (viewedPart.location + new Vector2I(8, 8)).ToString();
                        for (int i = 0; i < linkingGroups.GetChildren().Count; i++)
                        {
                            linkingGroups.GetChild<Button>(i).GetChild<Sprite2D>(0).Frame = viewedPart.linkingGroups.Contains(i) ? 1 : 0;
                        }
                        LoadBindings();
                        configurePart.Show();
                    }
                }
                else if (selectedPart == -1)
                {
                    configurePart.Hide();
                }
            }
            else if (robotLocation.X >= -9 && robotLocation.Y >= -9 && robotLocation.X <= 8 && robotLocation.Y <= 8)
                configurePart.Hide();
            else if ((robotLocation.X < -9 || robotLocation.Y < -9 || robotLocation.X > 8 || robotLocation.Y > 8) && !configurePart.Visible)
            {
                selectedPart = partsMap.GetCellSourceId(partsMap.LocalToMap(partsMap.ToLocal(GetGlobalMousePosition())));
                if (selectedPart > 0)
                    Parts.partList[selectedPart].rotation = Parts.partList[selectedPart].directional ? rotation : 0;
            }
            UpdateRobot();
        }
        previewMap.Clear();
        if (selectedPart > -1)
            for (int i = 0; i < Parts.partList[selectedPart].GetPoints().Count; i++)
            {
                Vector2I previewLocation = previewMap.LocalToMap(previewMap.ToLocal(GetGlobalMousePosition())) + Parts.partList[selectedPart].GetPoints()[i] - Parts.partList[selectedPart].origin;
                if (previewLocation.X < -8 || previewLocation.Y < -8 || previewLocation.X > 7 || previewLocation.Y > 7)
                    previewMap.Modulate = new Color(1, 0, 0, 0.5f);
                else
                    previewMap.Modulate = new Color(0, 1, 0, 0.5f);
                previewMap.SetCell(previewLocation, Parts.partList[selectedPart].id, Parts.partList[selectedPart].points[i], Parts.rotations[Parts.partList[selectedPart].rotation]);
            }
    }
}