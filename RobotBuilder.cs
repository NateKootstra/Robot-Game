using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class RobotBuilder : Node2D
{
    TileMapLayer partsMap;
    TileMapLayer robotMap;
    TileMapLayer previewMap;

    private int selectedPart = -1;
    private int rotation = 0;
    private string selectedBot = "";

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

    public override void _Ready()
    {
        String[] robots = DirAccess.GetFilesAt("user://robots");


        // if (!robots.Contains(selectedBot + ".robot"))
        //     selectedBot = robots[0]

        partsMap = this.GetChild<TileMapLayer>(0);
        robotMap = this.GetParent().GetChild(2).GetChild<TileMapLayer>(0);
        previewMap = this.GetParent().GetChild(2).GetChild<TileMapLayer>(1);

        if (robots.Length == 0)
        {
            var robotCells = robotMap.GetUsedCells();
            for (int i = 0; i < robotCells.Count; i++)
                if (Math.Abs(robotCells[i].X) < 10 && Math.Abs(robotCells[i].Y) < 10 && !(robotMap.GetCellSourceId(robotCells[i]) == -1))
                {
                    if (robotMap.GetCellAtlasCoords(robotCells[i]) == Parts.partList[robotMap.GetCellSourceId(robotCells[i])].origin)
                    {
                        robot.Add(Parts.partList[robotMap.GetCellSourceId(robotCells[i])].Copy());
                        robot[^1].location = robotCells[i];
                        robot[^1].rotation = Parts.rotations.IndexOf(robotMap.GetCellAlternativeTile(robotCells[i]));
                    }
                }
        }
        UpdateRobot();
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
                else
                {
                    if (selectedPart == 0)
                        if (!(robotMap.GetCellSourceId(robotLocation) == -1))
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
                }
                UpdateRobot();
            }
            else if (robotLocation.X < -9 || robotLocation.Y < -9 || robotLocation.X > 8 || robotLocation.Y > 8)
            {
                selectedPart = partsMap.GetCellSourceId(partsMap.LocalToMap(partsMap.ToLocal(GetGlobalMousePosition())));
                if (selectedPart > 0)
                    Parts.partList[selectedPart].rotation = Parts.partList[selectedPart].directional ? rotation : 0;
            }
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