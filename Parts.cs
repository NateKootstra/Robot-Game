using Godot;
using System;
using System.Collections.Generic;


public partial class Parts : Node2D
{
    public readonly List<int> rotations = [
        0,
        (int)(TileSetAtlasSource.TransformTranspose + TileSetAtlasSource.TransformFlipH),
        (int)(TileSetAtlasSource.TransformFlipH + TileSetAtlasSource.TransformFlipV),
        (int)(TileSetAtlasSource.TransformTranspose + TileSetAtlasSource.TransformFlipV)
    ];
    TileMapLayer partsMap;
    TileMapLayer robotMap;
    TileMapLayer previewMap;

    private int selectedPart = -1;
    private int rotation = 0;
    public List<Part> robot = [];


    public class IntegerTrigger(int min, int max)
    {
        public int min = min;
        public int max = max;
    }
    public class DoubleTrigger(double min, double max)
    {
        public double min = min;
        public double max = max;
    }
    public class Part(int id, string name, Vector2I origin, List<Vector2I> points, double weight, Dictionary<string, bool> booleanTriggers, Dictionary<string, IntegerTrigger> integerTriggers, Dictionary<string, DoubleTrigger> doubleTriggers, Action<Part> onTick)
    {
        public int id = id;
        public string name = name;
        public Vector2I origin = origin;
        public List<Vector2I> points = points;
        public double weight = weight;
        public Dictionary<string, bool> booleanTriggers = booleanTriggers;
        public Dictionary<string, IntegerTrigger> integerTriggers = integerTriggers;
        public Dictionary<string, DoubleTrigger> doubleTriggers = doubleTriggers;
        public Action<Part> onTick = onTick;

        public Vector2I location = new(-1, -1);
        public int rotation = 0;
        public int tickCooldown = 0;

        public List<Vector2I> GetPoints()
        {
            Vector2I max = new(0, 0);
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].X > max.X)
                    max.X = points[i].X;
                if (points[i].Y > max.Y)
                    max.Y = points[i].Y;
            }
            Vector2 center = new(max.X / 2f, max.Y / 2f);
            List<Vector2I> newPoints = [];
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 newPoint = points[i];
                newPoint -= center;
                for (int i2 = 0; i2 < rotation; i2++)
                {

                    newPoint = new Vector2(-newPoint.Y, newPoint.X);
                }
                newPoint += center;
                newPoints.Add(new Vector2I((int)newPoint.X, (int)newPoint.Y));
            }
            return newPoints;
        }
        public Part Copy()
        {
            return (Part)MemberwiseClone();
        }
    }

    public static void GenericOnTick(int tickRate, Part self)
    {
        self.tickCooldown = tickRate - 1;
        GD.Print(self.name);
    }

    public static void SwerveModuleOnTick(int tickRate, Part self)
    {
        self.tickCooldown = tickRate - 1;
        GD.Print(self.name);
    }

    public readonly List<Part> partList = [
        new Part(   0, "Bumper Corner",
                    new(0, 0),
                    [new(0, 0)],
                    1,
                    [],
                    [],
                    [],
                    (part) => GenericOnTick(60, part)),
        new Part(   1, "Bumper Side",
                    new(0, 0),
                    [new(0, 0)],
                    1,
                    [],
                    [],
                    [],
                    (part) => GenericOnTick(60, part)),
        new Part(   2, "Metal",
                    new(0, 0),
                    [new(0, 0)],
                    1,
                    [],
                    [],
                    [],
                    (part) => GenericOnTick(60, part)),
        new Part(   3, "Swerve Module",
                    new(1, 1),
                    [new(0, 0), new(1, 0), new(0, 1), new(1, 1), new(2, 1), new(1, 2), new(2, 2)],
                    1,
                    [],
                    [],
                    new(){{ "Speed", new(-1, 1) }, { "Direction", new (0, 360) }},
                    (part) => SwerveModuleOnTick(1, part)),
    ];

    public void UpdateRobot()
    {
        robotMap.Clear();
        for (int i = 0; i < robot.Count; i++)
        {
            for (int i2 = 0; i2 < robot[i].GetPoints().Count; i2++)
            {
                robotMap.SetCell(robot[i].location + robot[i].GetPoints()[i2] - robot[i].origin, robot[i].id, robot[i].points[i2], rotations[robot[i].rotation]);
            }
        }
    }

    public override void _Ready()
    {
        partsMap = this.GetChild<TileMapLayer>(0);
        robotMap = this.GetParent().GetChild(0).GetChild<TileMapLayer>(0);
        previewMap = this.GetParent().GetChild(0).GetChild<TileMapLayer>(1);

        var robotCells = robotMap.GetUsedCells();
        for (int i = 0; i < robotCells.Count; i++)
            if (Math.Abs(robotCells[i].X) < 10 && Math.Abs(robotCells[i].Y) < 10 && !(robotMap.GetCellSourceId(robotCells[i]) == -1))
            {
                if (robotMap.GetCellAtlasCoords(robotCells[i]) == partList[robotMap.GetCellSourceId(robotCells[i])].origin)
                {
                    robot.Add(partList[robotMap.GetCellSourceId(robotCells[i])].Copy());
                    robot[^1].location = robotCells[i];
                    robot[^1].rotation = rotations.IndexOf(robotMap.GetCellAlternativeTile(robotCells[i]));
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
            if (!(selectedPart == -1))
                partList[selectedPart].rotation = rotation;
        }
        if (Input.IsActionJustPressed("LMB"))
        {
            Vector2I robotLocation = robotMap.LocalToMap(robotMap.ToLocal(GetGlobalMousePosition()));
            if (robotLocation.X >= -8 && robotLocation.Y >= -8 && robotLocation.X <= 7 && robotLocation.Y <= 7)
            {
                if (!(selectedPart == -1))
                {
                    bool valid = true;
                    for (int i = 0; i < partList[selectedPart].GetPoints().Count; i++)
                    {
                        Vector2I pointLocation = robotLocation + partList[selectedPart].GetPoints()[i] - partList[selectedPart].origin;
                        GD.Print(pointLocation);
                        if (pointLocation.X < -8 || pointLocation.Y < -8 || pointLocation.X > 7 || pointLocation.Y > 7)
                            valid = false;
                    }
                    if (valid)
                    {
                        for (int i = 0; i < partList[selectedPart].GetPoints().Count; i++)
                        {
                            Vector2I pointLocation = robotLocation + partList[selectedPart].GetPoints()[i] - partList[selectedPart].origin;
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
                        robot.Add(partList[selectedPart].Copy());
                        robot[^1].location = robotLocation;
                    }
                }
                else
                {
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
            else if (robotLocation.X < -8 || robotLocation.Y < -8 || robotLocation.X > 7 || robotLocation.Y > 7)
            {
                selectedPart = partsMap.GetCellSourceId(partsMap.LocalToMap(partsMap.ToLocal(GetGlobalMousePosition())));
                if (!(selectedPart == -1))
                    partList[selectedPart].rotation = rotation;
            }
        }
        previewMap.Clear();
        if (!(selectedPart == -1))
            for (int i = 0; i < partList[selectedPart].GetPoints().Count; i++)
            {
                Vector2I previewLocation = previewMap.LocalToMap(previewMap.ToLocal(GetGlobalMousePosition())) + partList[selectedPart].GetPoints()[i] - partList[selectedPart].origin;
                if (previewLocation.X < -8 || previewLocation.Y < -8 || previewLocation.X > 7 || previewLocation.Y > 7)
                    previewMap.Modulate = new Color(1, 0, 0, 0.5f);
                else
                    previewMap.Modulate = new Color(0, 1, 0, 0.5f);
                previewMap.SetCell(previewLocation, partList[selectedPart].id, partList[selectedPart].points[i], rotations[partList[selectedPart].rotation]);
            }
    }
}