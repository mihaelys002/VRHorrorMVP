using FIMSpace.Generating.Planning;
using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes.SpecificSolutions
{
    public class PR_AssembleConnectivityGraph : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Assemble Connectivity Graph"; }
        public override string GetNodeTooltipDescription { get { return "Assemble Connectivity Graph"; } }
        public override Color GetNodeColor() { return new Color(0.4f, 0.4f, 1.0f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(300, 240); } }
        //public override bool DrawInputConnector { get { return true; } }
        //public override bool DrawOutputConnector { get { return false; } }

        //public override int OutputConnectorsCount { get { return 0; } }
        //public override int AllowedOutputConnectionIndex { get { return 0; } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }



        [Port(EPortPinType.Input)] public PGGPlannerPort CurrentA;
        [Tooltip("Current iteration Path connection target B Field.")]

        [Port(EPortPinType.Input)] public PGGPlannerPort CurrentB;

        [Port(EPortPinType.Input)] public PGGPlannerPort CurrentConnection;


        [Port(EPortPinType.Input)] public PGGCellPort From_StartCell;
        [Port(EPortPinType.Input)] public PGGCellPort Path_StartCell;
        [Port(EPortPinType.Input)] public PGGCellPort Path_EndCell;
        [Port(EPortPinType.Input)] public PGGCellPort Towards_EndCell;



        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {

            var graph = ParentPlanner.ParentBuildPlanner.ConnectivityGraph;

            CurrentA.TriggerReadPort(true);
            CurrentB.TriggerReadPort(true);

            From_StartCell.TriggerReadPort(true);
            Path_StartCell.TriggerReadPort(true);
            Path_EndCell.TriggerReadPort(true);
            Towards_EndCell.TriggerReadPort(true);

            CurrentConnection.TriggerReadPort(true);

            var roomA = graph.Rooms.Find(x => x.FieldPlanner == GetPlannerFromPort(CurrentA));
            var roomB = graph.Rooms.Find(x => x.FieldPlanner == GetPlannerFromPort(CurrentB));
            if (roomA == null)
                graph.Rooms.Add(roomA = new Room { FieldPlanner = GetPlannerFromPort(CurrentA) });
            if (roomB == null)
                graph.Rooms.Add(roomB = new Room { FieldPlanner = GetPlannerFromPort(CurrentB) });


            Corridor corridor = new();

            corridor.RoomA = roomA;
            corridor.RoomB = roomB;

            roomA.ConnectedCorridors.Add(corridor);
            roomB.ConnectedCorridors.Add(corridor);
            graph.Corridors.Add(corridor);

            corridor.From_StartCell = From_StartCell.GetInputCellValue;
            corridor.Path_StartCell = Path_StartCell.GetInputCellValue;
            corridor.Path_EndCell = Path_EndCell.GetInputCellValue;
            corridor.Towards_EndCell = Towards_EndCell.GetInputCellValue;

            corridor.FieldPlanner = GetPlannerFromPort(CurrentConnection);
            corridor.FromPlanner = From_StartCell.GetInputResultValue;
            corridor.PathPlanner = Path_StartCell.GetInputResultValue;
            corridor.ToPlanner = Towards_EndCell.GetInputResultValue;

            Debug.Log("Add here Corridor to ConnectivityGraph.");
        }
    }
}