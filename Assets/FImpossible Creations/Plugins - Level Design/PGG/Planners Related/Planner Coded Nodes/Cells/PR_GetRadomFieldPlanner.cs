using FIMSpace.Graph;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Access
{

    public class PR_GetRadomFieldPlanner : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Get Random Field Instance" : "Get Random Field Instance"; }
        public override string GetNodeTooltipDescription { get { return "Getting random Field Setup instance"; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.75f, 0.25f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(228, _EditorFoldout ? 118 * 4 / 3f : 98 * 4 / 3f); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        public override bool IsFoldable { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }

        [Port(EPortPinType.Input, EPortValueDisplay.Default)] public PGGPlannerPort InstancesOf;
        public bool AcceptDiscarded = false;
        [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGPlannerPort ChoosedInstance;

        public override void OnStartReadingNode()
        {
            if (CurrentExecutingPlanner == null) return;

            var planners = GetPlannersFromPort(InstancesOf);
            if (planners == null) return;


            List<FieldPlanner> list = new();
            foreach (var planner in planners)
            {
                list.AddRange(planner.GetDuplicatesPlannersList());
                list.Add(planner);
            }
            if (!AcceptDiscarded)
                list.RemoveAll(x => x.Discarded);


            ChoosedInstance.Output_Provide_Planner(list.GetRandomElement());
        }
    }
}