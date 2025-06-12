using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class Parts : Node
{
    public static readonly List<int> rotations = [
        0,
        (int)(TileSetAtlasSource.TransformTranspose + TileSetAtlasSource.TransformFlipH),
        (int)(TileSetAtlasSource.TransformFlipH + TileSetAtlasSource.TransformFlipV),
        (int)(TileSetAtlasSource.TransformTranspose + TileSetAtlasSource.TransformFlipV)
    ];

    public class Binding()
    {
        public static readonly List<string> typeOptions = [
            "Memory",
            "Trigger",
            "Number",
            "Key"
        ];

        public bool comparison = false;
        public List<int> types = [2, 2, 0, 2];
        public List<string> inputs = ["", "", "", ""];
        public List<string> operands = ["==", "="];

        public void RunBinding(int robotIndex, int partIndex)
        {
            try
            {
                List<double> values = [];
                for (int i = 0; i < 4; i++)
                {
                    if (comparison || (!comparison && (i > 1)))
                    {
                        if (!(i == 2))
                            switch (types[i])
                            {
                                case 0:
                                    values.Add(Robots.list[robotIndex].memory[inputs[i]]);
                                    break;
                                case 1:
                                    values.Add(Robots.list[robotIndex].parts[partIndex].triggers[inputs[i]][2]);
                                    break;
                                case 2:
                                    values.Add(double.Parse(inputs[i], System.Globalization.CultureInfo.InvariantCulture));
                                    break;
                                case 3:
                                    if (Input.IsPhysicalKeyPressed(OS.FindKeycodeFromString(inputs[i])))
                                        values.Add(1d);
                                    else
                                        values.Add(0d);
                                    break;
                            }
                    }
                    else
                        values.Add(-1);
                }
                if (comparison)
                {
                    bool comparisonResult = false;
                    switch (operands[0])
                    {
                        case "==":
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
                            if (Robots.list[robotIndex].parts[partIndex].triggers.TryGetValue(inputs[2], out List<double> value))
                                value[2] = values[2];
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
                            if (Robots.list[robotIndex].parts[partIndex].triggers.TryGetValue(inputs[2], out List<double> value))
                                value[2] += values[2];
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
                            if (Robots.list[robotIndex].parts[partIndex].triggers.TryGetValue(inputs[2], out List<double> value))
                                value[2] -= values[2];
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
                            if (Robots.list[robotIndex].parts[partIndex].triggers.TryGetValue(inputs[2], out List<double> value))
                                value[2] *= values[2];
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
                            if (Robots.list[robotIndex].parts[partIndex].triggers.TryGetValue(inputs[2], out List<double> value))
                                value[2] /= values[2];
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
    public class Part(int id, string name, int priority, bool directional, Vector2I origin, List<Vector2I> points, double weight, Dictionary<string, List<double>> triggers, string onTick)
    {
        public int id = id;
        public string name = name;
        public int priority = priority;
        public bool directional = directional;
        public Vector2I origin = origin;
        public List<Vector2I> points = points;
        public double weight = weight;
        public Dictionary<string, List<double>> triggers = triggers;
        public string onTick = onTick;

        public Vector2I location = new(-1, -1);
        public int rotation = 0;
        public List<int> linkingGroups;
        public List<Binding> bindings;

        public int tickCooldown = 0;

        public Part(string json) : this(
            (int)Part.FromJSON(json)[0],
            partList[(int)Part.FromJSON(json)[0]].name,
            partList[(int)Part.FromJSON(json)[0]].priority,
            partList[(int)Part.FromJSON(json)[0]].directional,
            partList[(int)Part.FromJSON(json)[0]].origin,
            partList[(int)Part.FromJSON(json)[0]].points,
            partList[(int)Part.FromJSON(json)[0]].weight,
            partList[(int)Part.FromJSON(json)[0]].triggers,
            partList[(int)Part.FromJSON(json)[0]].onTick
            )
        {
            location = (Vector2I)Part.FromJSON(json)[1];
            rotation = (int)Part.FromJSON(json)[2];
            linkingGroups = (List<int>)Part.FromJSON(json)[3];
            bindings = (List<Binding>)Part.FromJSON(json)[4];
        }
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
            Part clone = (Part)MemberwiseClone();
            clone.linkingGroups = [];
            clone.bindings = [];
            return clone;
        }

        public Dictionary<string, dynamic> GetJSON()
        {
            List<Dictionary<string, dynamic>> preparedBindings = [];
            foreach (Binding binding in bindings)
            {
                preparedBindings.Add(new()
                {
                    { "comparison", binding.comparison },
                    { "types", binding.types },
                    { "inputs", binding.inputs },
                    { "operands", binding.operands }
                });
            }

            return new(){
                { "id", id },
                { "location", new List<int>(){location.X, location.Y} },
                { "rotation", rotation },
                { "linkingGroups", linkingGroups },
                { "bindings", preparedBindings }
            };
        }

        private static ArrayList FromJSON(string json)
        {
            ArrayList newPart = [];
            var data = Json.ParseString(json);
            newPart.Add((int)data.AsGodotDictionary()["id"]);
            newPart.Add(new Vector2I(data.AsGodotDictionary()["location"].AsInt32Array()[0], data.AsGodotDictionary()["location"].AsInt32Array()[1]));
            newPart.Add((int)data.AsGodotDictionary()["rotation"]);
            newPart.Add(data.AsGodotDictionary()["linkingGroups"].AsInt32Array().ToList());
            List<Binding> bindings = [];
            foreach (var bindingDict in data.AsGodotDictionary()["bindings"].AsGodotArray())
            {
                if (bindingDict.AsGodotDictionary()["types"].AsInt32Array().Length > 0)
                {
                    bindings.Add(new());
                    bindings[^1].comparison = (Boolean)bindingDict.AsGodotDictionary()["comparison"];
                    bindings[^1].types = [.. bindingDict.AsGodotDictionary()["types"].AsInt32Array()];
                    bindings[^1].inputs = [.. bindingDict.AsGodotDictionary()["inputs"].AsStringArray()];
                    bindings[^1].operands = [.. bindingDict.AsGodotDictionary()["operands"].AsStringArray()];
                }
            }
            newPart.Add(bindings);
            return newPart;
        }
    }


    public static readonly Dictionary<string, Action<Part, int, ArrayList>> tickActions = new(){
        {"GenericOnTick", delegate(Part self, int robotID, ArrayList args) {
            int cooldown = 1;

            self.tickCooldown = cooldown;
        }},
        {"SwerveModuleOnTick", delegate(Part self, int robotID, ArrayList args) {
            int cooldown = 1;

            double θ = (Math.PI / 180) * self.triggers["Dir"][2];
            double fx = self.triggers["Speed"][2] * Math.Sin(θ);
            double fy = -self.triggers["Speed"][2] * Math.Cos(θ);
            Robots.list[robotID].privateMemory["totalFx"] += fx;
            Robots.list[robotID].privateMemory["totalFy"] += fy;
            Robots.list[robotID].privateMemory["totalTau"] += ((float)self.location.X + 0.5) * fy - ((float)self.location.Y + 0.5) * fx;

            self.tickCooldown = cooldown;
        }},
    };

    public static readonly List<Part> partList = [
        new Part(   0, "Delete",
                    -1,
                    false,
                    new(0, 0),
                    [new(0, 0)],
                    1,
                    [],
                    "GenericOnTick"),
        new Part(   1, "Bumper Corner",
                    -1,
                    true,
                    new(0, 0),
                    [new(0, 0)],
                    1,
                    [],
                    "GenericOnTick"),
        new Part(   2, "Bumper Side",
                    -1,
                    true,
                    new(0, 0),
                    [new(0, 0)],
                    1,
                    [],
                    "GenericOnTick"),
        new Part(   3, "Metal",
                    -1,
                    false,
                    new(0, 0),
                    [new(0, 0)],
                    1,
                    [],
                    "GenericOnTick"),
        new Part(   4, "Swerve Module",
                    2,
                    true,
                    new(1, 1),
                    [new(0, 0), new(1, 0), new(0, 1), new(1, 1), new(2, 1), new(1, 2), new(2, 2)],
                    1,
                    new(){{ "Speed", [-1, 1, 0] }, { "Dir", [0, 360, 0]}},
                    "SwerveModuleOnTick"),
    ];
}