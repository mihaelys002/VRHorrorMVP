﻿using FIMSpace.Graph;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Checker
{

    public class PR_IsFieldFullyContainedBy : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Is Fully Contained By" : "Is Field Fully Contained By"; }
        public override string GetNodeTooltipDescription { get { return "Check if field is fully contained by other field volume"; } }
        public override Color GetNodeColor() { return new Color(0.07f, 0.66f, 0.56f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(_EditorFoldout ? 246 : 220, _EditorFoldout ? 126 : 102); } }
        public override bool IsFoldable { get { return true; } }

        [Port(EPortPinType.Input, 1)] public PGGPlannerPort CollisionWith;
        [Port(EPortPinType.Output, EPortValueDisplay.HideValue, 1)][Tooltip("If collision occured then true, if no then false")] public BoolPort IsFullyContained;
        [HideInInspector][Port(EPortPinType.Input, 1)][Tooltip("Using self if no input")] public PGGPlannerPort FirstColliderField;

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            FieldPlanner aPlanner = GetPlannerFromPort(FirstColliderField);
            List<FieldPlanner> bPlanners = GetPlannersFromPort(CollisionWith);

            if (aPlanner == null) return;

            var aChecker = aPlanner.LatestChecker;

            IsFullyContained.Value = false;

            for (int p = 0; p < bPlanners.Count; p++)
            {
                var planner = bPlanners[p];
                if (aChecker.ChildPositionsCount > planner.LatestChecker.ChildPositionsCount) continue; // Not enough cells to fully contain

                bool fully = true;
                for (int c = 0; c < aChecker.ChildPositionsCount; c++)
                {
                    if (planner.LatestChecker.ContainsWorld(aChecker.GetWorldPos(c)) == false) { fully = false; break; }
                }

                if ( fully)
                {
                    IsFullyContained.Value = true;
                    return;
                }
            }
        }

#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (!_EditorFoldout) return;

            if (_EditorFoldout)
            {
                FirstColliderField.AllowDragWire = true;
                baseSerializedObject.Update();
                if (sp == null) sp = baseSerializedObject.FindProperty("FirstColliderField");
                SerializedProperty spc = sp.Copy();
                EditorGUILayout.PropertyField(spc);
                baseSerializedObject.ApplyModifiedProperties();
            }
            else
            {
                FirstColliderField.AllowDragWire = false;
            }

        }
#endif

    }
}