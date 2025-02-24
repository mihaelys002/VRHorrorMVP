using FIMSpace.Generating;
using FIMSpace.Generating.Planning;
using Mirror.BouncyCastle.Asn1.Cmp;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ConnectivityGraph
{
    // List of all rooms in the map.
    public List<Room> Rooms { get; } = new List<Room>();

    // List of corridors connecting the rooms.
    public List<Corridor> Corridors { get; } = new List<Corridor>();

    internal void Clear()
    {
        Rooms.Clear();
        Corridors.Clear();
    }
}

public class Room
{
    // Unique identifier for the room.
    public int Id { get; set; }

    // A name or description for the room.
    public string Name { get; set; }

    public FieldPlanner FieldPlanner { get; set; }

    // Corridors connected to this room.
    public List<Corridor> ConnectedCorridors { get; } = new List<Corridor>();

    // Points of interest in this room (could be key locations, special items, etc.).
    public List<PointOfInterest> PointsOfInterest { get; } = new List<PointOfInterest>();
}



public class Corridor
{
    // Unique identifier for the corridor.
    public int Id { get; set; }

    public FieldPlanner FieldPlanner { get; set; }

    // The two rooms this corridor connects.
    public Room RoomA { get; set; }
    public Room RoomB { get; set; }

    public FieldCell From_StartCell;
    public FieldCell Path_StartCell;
    public FieldCell Path_EndCell;
    public FieldCell Towards_EndCell;

    public bool IsZeroDistance = false;
    public Blocker Blocker1 { get; set; }
    public Blocker Blocker2 { get; set; }

    public PlannerResult FromPlanner;
    public PlannerResult ToPlanner;
    public PlannerResult PathPlanner;


}

public abstract class PointOfInterest
{
    // Unique identifier for the point of interest.
    public int Id { get; set; }

    // The cell this point of interest is in.
    public PlannerCell Cell { get; set; }
}

public class Key : PointOfInterest
{
    public Blocker Door { get; set; }
}

public class Loot : PointOfInterest
{
    public int Value { get; set; }
}
public class Turret : PointOfInterest { }

public class EnemySpawn : PointOfInterest { }


public class Blocker
{
    public BlockerType BlockerType { get; set; }
}



public enum BlockerType
{
    None,
    KeyedDoor,
    MalfuctioningDoor,
    BrokenDoor,
    CaveIn,
    Barrickade
}

