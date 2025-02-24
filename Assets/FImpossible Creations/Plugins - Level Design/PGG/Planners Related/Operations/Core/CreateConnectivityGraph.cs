using FIMSpace.Generating.Planning;
using UnityEngine;

public class CreateConnectivityGraph: BuildPlannerOperationBase
{
    public override void OnPrepareBuildPlan(BuildPlannerPreset planner, BuildPlannerOperationHelper helper)
    {
        UnityEngine.Debug.Log("On Prepare Build Plan Operation");
        planner.ConnectivityGraph.Clear();
    }
}
