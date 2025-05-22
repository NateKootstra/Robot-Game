using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class Robots : Node
{
    public class Robot(List<Parts.Part> parts)
    {
        public List<Parts.Part> parts = parts;
        public Dictionary<string, double> memory = [];
    }

    public static List<Robot> list = [
        new Robot([])
    ];
}
