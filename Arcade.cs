using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Arcade : Node2D
{
    public Robots.Robot self = Robots.list[0];


    public void UpdateRobot()
    {
        self.map.Clear();
        for (int i = 0; i < self.parts.Count; i++)
        {
            for (int i2 = 0; i2 < self.parts[i].GetPoints().Count; i2++)
            {
                self.map.SetCell(self.parts[i].location + self.parts[i].GetPoints()[i2] - self.parts[i].origin, self.parts[i].id, self.parts[i].points[i2], Parts.rotations[self.parts[i].rotation]);
            }
        }
    }

    public override void _Ready()
    {
        self.bot = this;
        self.map = this.GetChild<TileMapLayer>(0);

        string selectedRobot = "";
        DirAccess userFolder = DirAccess.Open("user://");
        if (!userFolder.DirExists("robots"))
            userFolder.MakeDir("robots");
        bool isRobotSelected = false;
        try
        {
            Godot.FileAccess userDataRead = Godot.FileAccess.Open("user://user.dat", Godot.FileAccess.ModeFlags.Read);
            isRobotSelected = userDataRead.GetAsText().Contains("selectedRobot");
            userDataRead.Close();
        }
        catch { };
        if (!userFolder.GetFiles().Contains("user.dat") || !isRobotSelected)
        {
            DirAccess.CopyAbsolute("res://Kitbot.robot", "user://robots/Kitbot.robot");
            Godot.FileAccess userData = Godot.FileAccess.Open("user://user.dat", Godot.FileAccess.ModeFlags.Write);
            userData.StoreLine("selectedRobot=Kitbot");
            userData.Close();
        }
        Godot.FileAccess confirmedUserData = Godot.FileAccess.Open("user://user.dat", Godot.FileAccess.ModeFlags.Read);
        string[] points = confirmedUserData.GetAsText().Split(",\n");
        foreach (string point in points)
            if (point.Split("=")[0] == "selectedRobot")
                selectedRobot = point.Split("=")[1].Trim();
        confirmedUserData.Close();

        Godot.FileAccess botRead = Godot.FileAccess.Open("user://robots/" + selectedRobot + ".robot", Godot.FileAccess.ModeFlags.Read);
        string text = botRead.GetAsText().TrimPrefix("[").TrimSuffix("\n").TrimSuffix("]");
        botRead.Close();
        string[] partList = text.Split("},{\"id");
        for (int i = 0; i < partList.Length; i++)
        {
            if (!partList[i].StartsWith('{'))
                partList[i] = "{\"id" + partList[i];
            if (!partList[i].EndsWith('}'))
                partList[i] = partList[i] + "}";
        }
        self.parts = [];
        foreach (string partString in partList)
        {
            self.parts.Add(new Parts.Part(partString));
        }
        UpdateRobot();
    }

    public override void _Process(double delta)
    {
        PreParts();

        for (int i = 0; i < self.parts.Count; i++)
            foreach (Parts.Binding binding in self.parts[i].bindings)
                binding.RunBinding(0, i);
        for (int i = 0; i < self.parts.Count; i++)
            foreach (string key in self.parts[i].triggers.Keys)
            {

                self.parts[i].triggers[key][2] = self.parts[i].triggers[key][2] > self.parts[i].triggers[key][0] ? self.parts[i].triggers[key][2] : self.parts[i].triggers[key][0];
                self.parts[i].triggers[key][2] = self.parts[i].triggers[key][2] < self.parts[i].triggers[key][1] ? self.parts[i].triggers[key][2] : self.parts[i].triggers[key][1];
            }
        foreach (Parts.Part part in self.parts)
            Parts.tickActions[part.onTick](part, 0, []);

        PostParts();
    }
    public void PreParts()
    {
        Robots.list[0].privateMemory["totalFx"] = 0.0;
        Robots.list[0].privateMemory["totalFy"] = 0.0;
        Robots.list[0].privateMemory["totalTau"] = 0.0;
    }

    public void PostParts()
    {
        try
        {
            self.bot.Position += new Vector2((float)self.privateMemory["totalFx"] * 2, (float)self.privateMemory["totalFy"] * 2);
            foreach (Vector2I cell in self.map.GetUsedCells())
            {
                Vector2 coords = self.map.ToGlobal(self.map.MapToLocal(cell));
                if (coords.Y < 160)
                {
                    self.bot.Position -= new Vector2(0, coords.Y - 160);
                }
                else if (coords.Y > 1037.6f)
                {
                    self.bot.Position -= new Vector2(0, coords.Y - 1037.6f);
                }
                if (coords.X < 72)
                {
                    self.bot.Position -= new Vector2(coords.X - 72, 0);
                }
                else if (coords.X > 1846.6)
                {
                    self.bot.Position -= new Vector2(coords.X - 1846.6f, 0);
                }
            }
            self.bot.RotationDegrees += (float)self.privateMemory["totalTau"];
        }
        catch { }
    }
}