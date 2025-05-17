using Godot;
using System;
using System.Collections.Generic;

public partial class Robot : Node2D
{
    TileMapLayer map;

    Vector2 velocity = new(0, 0);
    double rotationalVelocity = 0;

    public static List<Double> CalculateDrive(List<List<double>> wheels, double mass)
    {
        double totalFx = 0.0;
        double totalFy = 0.0;
        double totalTau = 0.0;

        for (int i = 0; i < wheels.Count; i++)
        {
            List<Double> output = CalculateWheel(new Vector2((float)wheels[i][0], (float)wheels[i][1]), wheels[i][2], wheels[i][3]);
            totalFx += output[0];
            totalFy += output[1];
            totalTau += output[2];
        }

        double dx = totalFx / mass;
        double dy = totalFy / mass;
        double dTheta = totalTau / mass;

        return [dx, dy, dTheta];
    }

    public static List<Double> CalculateWheel(Vector2 location, double direction, double speed)
    {
        double θ = (Math.PI / 180) * direction;
        double fx = speed * Math.Sin(θ);
        double fy = -speed * Math.Cos(θ);
        double τ = location.X * fy - location.Y * fx;
        return [fx, fy, τ];
    }


    public override void _Ready()
    {
        map = this.GetChild<TileMapLayer>(0);
    }

    public override void _Process(double delta)
    {
        double speed = 0;

        List<List<double>> wheels = [];
        var swerveTiles = map.GetUsedCellsById(3);
        for (int i = 0; i < swerveTiles.Count; i++)
        {
            Vector2I atlasCoords = map.GetCellAtlasCoords(swerveTiles[i]);
            if (atlasCoords == new Vector2I(1, 1))
                wheels.Add([swerveTiles[i].X + 0.5, swerveTiles[i].Y + 0.5, 0, speed]);
        }
        List<double> displacement = CalculateDrive(wheels, 1);
        velocity = new((float)displacement[0], (float)displacement[1]);
        rotationalVelocity = displacement[2];
        this.Position += velocity;
        this.RotationDegrees += (float)rotationalVelocity;
    }
}