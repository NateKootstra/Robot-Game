using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

[GlobalClass]
public partial class Parts : Node
{
    public static readonly List<int> rotations = [
        0,
        (int)(TileSetAtlasSource.TransformTranspose + TileSetAtlasSource.TransformFlipH),
        (int)(TileSetAtlasSource.TransformFlipH + TileSetAtlasSource.TransformFlipV),
        (int)(TileSetAtlasSource.TransformTranspose + TileSetAtlasSource.TransformFlipV)
    ];

    // Bindings should look like these:
    // Trigger Speed     =  Number 0
    // Memory  AND       =  Number 0
    // Key    W   = Number 1 -> Trigger Speed     += Number 1
    // Key    S   = Number 1 -> Trigger Speed     -= Number 1
    // Key    H   = Number 0 -> Memory  AND       += Number 1
    // Key    J   = Number 0 -> Memory  AND       += Number 1
    // Memory AND < Number 2 -> Memory  AND       =  Number 0
    // Memory AND = Number 2 -> Memory  AND       =  Number 1
    // Memory AND = Key    R -> Trigger Direction =  Number 360
    public class Binding()
    {
        public bool comparison = true;
        public List<int> types = [2, 3, 0, 3];
        public List<string> inputs = ["W", "1", "Forward", "1"];
        public List<string> operands = ["=", "="];

        public void RunBinding(int robotIndex, int partIndex)
        {
            try
            {
                List<double> values = [];
                for (int i = 0; i < 4; i++)
                {
                    if (!(i == 2))
                        switch (types[i])
                        {
                            case 0:
                                values.Add(Robots.list[robotIndex].memory[inputs[i]]);
                                break;
                            case 1:
                                values.Add(Robots.list[robotIndex].parts[partIndex].triggers[inputs[i]].value);
                                break;
                            case 2:
                                if (Input.IsPhysicalKeyPressed(OS.FindKeycodeFromString(inputs[i])))
                                    values.Add(1d);
                                else
                                    values.Add(0d);
                                break;
                            case 3:
                                values.Add(double.Parse(inputs[i], System.Globalization.CultureInfo.InvariantCulture));
                                break;
                        }
                }
                if (comparison)
                {
                    bool comparisonResult = false;
                    switch (operands[0])
                    {
                        case "=":
                            comparisonResult = values[0] == values[1];
                            break;
                        case ">":
                            comparisonResult = values[0] > values[1];
                            break;
                        case "<":
                            comparisonResult = values[0] < values[1];
                            break;
                        case ">=":
                            comparisonResult = values[0] >= values[1];
                            break;
                        case "<=":
                            comparisonResult = values[0] <= values[1];
                            break;
                    }
                    if (!comparisonResult)
                        return;
                }
                switch (operands[1])
                {
                    case "=":
                        if (types[2] == 0)
                        {
                            if (Robots.list[robotIndex].memory.ContainsKey(inputs[2]))
                                Robots.list[robotIndex].memory[inputs[2]] = values[2];
                            else
                                Robots.list[robotIndex].memory.Add(inputs[2], values[2]);
                        }
                        else
                        {
                            if (Robots.list[robotIndex].parts[partIndex].triggers.ContainsKey(inputs[2]))
                                Robots.list[robotIndex].parts[partIndex].triggers[inputs[2]] = values[2];
                            else
                                Robots.list[robotIndex].parts[partIndex].triggers.Add(inputs[2], values[2]);
                        }
                        break;
                    case "+=":
                        if (types[2] == 0)
                        {
                            if (Robots.list[robotIndex].memory.ContainsKey(inputs[2]))
                                Robots.list[robotIndex].memory[inputs[2]] += values[2];
                            else
                                Robots.list[robotIndex].memory.Add(inputs[2], values[2]);
                        }
                        else
                        {
                            if (Robots.list[robotIndex].parts[partIndex].triggers.ContainsKey(inputs[2]))
                                Robots.list[robotIndex].parts[partIndex].triggers[inputs[2]] += values[2];
                            else
                                Robots.list[robotIndex].parts[partIndex].triggers.Add(inputs[2], values[2]);
                        }
                        break;
                    case "-=":
                        if (types[2] == 0)
                        {
                            if (Robots.list[robotIndex].memory.ContainsKey(inputs[2]))
                                Robots.list[robotIndex].memory[inputs[2]] -= values[2];
                            else
                                Robots.list[robotIndex].memory.Add(inputs[2], -values[2]);
                        }
                        else
                        {
                            if (Robots.list[robotIndex].parts[partIndex].triggers.ContainsKey(inputs[2]))
                                Robots.list[robotIndex].parts[partIndex].triggers[inputs[2]] -= values[2];
                            else
                                Robots.list[robotIndex].parts[partIndex].triggers.Add(inputs[2], -values[2]);
                        }
                        break;
                    case "*=":
                        if (types[2] == 0)
                        {
                            if (Robots.list[robotIndex].memory.ContainsKey(inputs[2]))
                                Robots.list[robotIndex].memory[inputs[2]] *= values[2];
                            else
                                Robots.list[robotIndex].memory.Add(inputs[2], 0);
                        }
                        else
                        {
                            if (Robots.list[robotIndex].parts[partIndex].triggers.ContainsKey(inputs[2]))
                                Robots.list[robotIndex].parts[partIndex].triggers[inputs[2]] *= values[2];
                            else
                                Robots.list[robotIndex].parts[partIndex].triggers.Add(inputs[2], 0);
                        }
                        break;
                    case "/=":
                        if (types[2] == 0)
                        {
                            if (Robots.list[robotIndex].memory.ContainsKey(inputs[2]))
                                Robots.list[robotIndex].memory[inputs[2]] /= values[2];
                            else
                                Robots.list[robotIndex].memory.Add(inputs[2], 0);
                        }
                        else
                        {
                            if (Robots.list[robotIndex].parts[partIndex].triggers.ContainsKey(inputs[2]))
                                Robots.list[robotIndex].parts[partIndex].triggers[inputs[2]] /= values[2];
                            else
                                Robots.list[robotIndex].parts[partIndex].triggers.Add(inputs[2], 0);
                        }
                        break;
                }
            }
            catch
            {
                return;
            }
        }
    }
    public class BooleanTrigger(bool value)
    {
        public bool value = value;
    }
    public class IntegerTrigger(int min, int max, int value)
    {
        public int min = min;
        public int max = max;
        public int value = value;
    }
    public class DoubleTrigger(double min, double max, double value)
    {
        public double min = min;
        public double max = max;
        public double value = value;
    }
    public class Part(int id, string name, bool directional, Vector2I origin, List<Vector2I> points, double weight, Dictionary<string, dynamic> triggers, string onTick)
    {
        public int id = id;
        public string name = name;
        public bool directional = directional;
        public Vector2I origin = origin;
        public List<Vector2I> points = points;
        public double weight = weight;
        public Dictionary<string, dynamic> triggers = triggers;
        public string onTick = onTick;

        public Vector2I location = new(-1, -1);
        public int rotation = 0;
        public int tickCooldown = 0;
        public List<Binding> bindings = [];

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

    public static readonly Dictionary<string, Action<Part, int, ArrayList>> tickActions = new(){
        {"GenericOnTick", delegate(Part self, int robotID, ArrayList args) {
            int cooldown = 60;

            self.tickCooldown = cooldown;
        }},
        {"SwerveModuleOnTick", delegate(Part self, int robotID, ArrayList args) {
            int cooldown = 1;

            self.tickCooldown = cooldown;
        }},
    };

    public static readonly List<Part> partList = [
        new Part(   0, "Delete",
                    false,
                    new(0, 0),
                    [new(0, 0)],
                    1,
                    [],
                    "GenericOnTick"),
        new Part(   1, "Bumper Corner",
                    true,
                    new(0, 0),
                    [new(0, 0)],
                    1,
                    [],
                    "GenericOnTick"),
        new Part(   2, "Bumper Side",
                    true,
                    new(0, 0),
                    [new(0, 0)],
                    1,
                    [],
                    "GenericOnTick"),
        new Part(   3, "Metal",
                    false,
                    new(0, 0),
                    [new(0, 0)],
                    1,
                    [],
                    "GenericOnTick"),
        new Part(   4, "Swerve Module",
                    true,
                    new(1, 1),
                    [new(0, 0), new(1, 0), new(0, 1), new(1, 1), new(2, 1), new(1, 2), new(2, 2)],
                    1,
                    new(){{ "Speed", new DoubleTrigger(-1, 1, 0) }, { "Dir", new DoubleTrigger(0, 360, 0) }},
                    "GenericOnTick"),
    ];
}