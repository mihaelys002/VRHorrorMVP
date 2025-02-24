using FIMSpace.Generating.Planning;
using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes.SpecificSolutions
{
    public class PR_SimpleDoorGenerator : PlannerRuleBase
    {


        public override string GetDisplayName(float maxWidth = 120) { return nameof(PR_SimpleDoorGenerator); }
        public override string GetNodeTooltipDescription { get { return nameof(PR_SimpleDoorGenerator); } }
        public override Color GetNodeColor() { return new Color(0.4f, 0.4f, 1.0f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(300, 240); } }

        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }





        public int WallHoleCommand;
        public int DirectionalDoorCommand;
        public int UniDirectionalDoorCommand;



        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            Debug.Log("Simple Door executed");
            var graph = ParentPlanner.ParentBuildPlanner.ConnectivityGraph;
            foreach(var corridor in graph.Corridors)
            {
                var direction = corridor.Towards_EndCell.PosXZ - corridor.From_StartCell.PosXZ ;
                switch(direction.sqrMagnitude)
                {
                    //rooms are  adjecent
                    case 1:

                        //No duplicates possible here
                        AddInstruction(corridor.FromPlanner, direction.V2toV3Int(), corridor.From_StartCell, UniDirectionalDoorCommand);
                        AddInstruction(corridor.ToPlanner, -direction.V2toV3Int(), corridor.Towards_EndCell, WallHoleCommand);
                        break;
                    //rooms are separated by corridor
                    default:

                        //check for duplicates
                        direction =  corridor.Path_StartCell.PosXZ - corridor.From_StartCell.PosXZ;

                        AddInstruction(corridor.FromPlanner, direction.V2toV3Int(), corridor.From_StartCell, DirectionalDoorCommand);
                        AddInstruction(corridor.PathPlanner, -direction.V2toV3Int(), corridor.Path_StartCell, WallHoleCommand);

                        //check for duplicates
                        direction = corridor.Towards_EndCell.PosXZ - corridor.Path_EndCell.PosXZ ;

                        AddInstruction(corridor.PathPlanner, direction.V2toV3Int(), corridor.Path_EndCell, WallHoleCommand);
                        AddInstruction(corridor.ToPlanner, -direction.V2toV3Int(), corridor.Towards_EndCell, DirectionalDoorCommand);

                        break;
                }
                Debug.Log("iteration in Simple Door");
            }
      
        }

        private static void AddInstruction(PlannerResult result, Vector3 direction, FieldCell cell, int instruction)
        {
            SpawnInstructionGuide instr = new SpawnInstructionGuide();
            instr.pos = cell.Pos;
            instr.HelperCellRef = cell;
            instr.rot = Quaternion.LookRotation(direction);
            instr.UseDirection = true;
            instr.Id = instruction;
            result.CellsInstructions.Add(instr);
        }
    }
}