﻿using FIMSpace.FEditor;
using FIMSpace.Generating;
using FIMSpace.Generating.Planner.Nodes;
using FIMSpace.Generating.Planning;
using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Generating.Planning.PlannerNodes.BuildSetup;
using FIMSpace.Generating.Planning.PlannerNodes.Field;
using FIMSpace.Generating.Planning.PlannerNodes.Field.Access;
using FIMSpace.Generating.Planning.PlannerNodes.Field.Transforming;
using FIMSpace.Generating.Planning.PlannerNodes.Math.Algebra;
using FIMSpace.Generating.Planning.PlannerNodes.Math.Values;
using FIMSpace.Generating.Planning.PlannerNodes.Math.Vectors;
using FIMSpace.Generating.Planning.PlannerNodes.Utilities;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{

    // Graph Drawer ---------------------

    public class PlannerGraphDrawer : FGraphDrawerBase
    {

        #region Preset file to modify setup

        public IPlanNodesContainer currentSetup = null;
        public override ScriptableObject DebugDrawPreset => currentSetup == null ? null : currentSetup.ScrObj;
        #endregion
        public bool DrawedInsideInspector = false;
        public Texture2D AltVignette = null;
        public double _LatestRefreshDisplayFlag = -2;
        FieldPlanner latestPlanner = null;
        PlannerFunctionNode latestFunction = null;
        IPlanNodesHandler latestCustomGraph = null;

        internal void SetCustomGraph(IPlanNodesHandler handler)
        {
            if (currentSetup == null) currentSetup = handler.NodesHandler_Container;
            latestCustomGraph = handler;

            if (handler != null)
                if (handler.NodesHandler_Container != null)
                    latestPlanner = handler.NodesHandler_Container.ScrObj as FieldPlanner;
        }

        public FieldPlanner.EViewGraph DisplayMode = FieldPlanner.EViewGraph.Procedures_Placement;

        //internal void SetDisplayMode(FieldPlanner.EViewGraph mode)
        //{
        //    if (mode == DisplayMode) return;
        //    DisplayMode = mode;
        //    RefreshNodes();
        //    RequestPlannerRefresh();
        //}

        public PlannerGraphDrawer(EditorWindow parent, IPlanNodesContainer preset) : base(parent)
        {
            currentSetup = preset;

            latestPlanner = null;
            latestFunction = null;
            latestCustomGraph = null;

            if (currentSetup is FieldPlanner) latestPlanner = currentSetup as FieldPlanner;
            else if (currentSetup is PlannerFunctionNode) latestFunction = currentSetup as PlannerFunctionNode;
            else latestCustomGraph = preset as IPlanNodesHandler;
        }

        public List<PGGPlanner_NodeBase> Nodes
        {
            get
            {
                if (DisplayMode == FieldPlanner.EViewGraph.Procedures_Placement) return currentSetup.Procedures;
                else if (DisplayMode == FieldPlanner.EViewGraph.PostProcedures_Cells) return currentSetup.PostProcedures;
                else
                {
                    if (latestCustomGraph == null) return null;
                    return latestCustomGraph.NodesHandler_Container.Procedures;
                }
            }
        }

        protected override int PresetNodesCount { get { if (Nodes == null) return 0; return Nodes.Count; } }
        public override ScriptableObject ProjectFilePreset => currentSetup == null ? null : currentSetup.ScrObj;
        //protected override Texture2D DefaultConnectionTex => PlannerGraphWindow.PlannerGraphStyles.TEX_Gradient1;
        public override Type GetBaseNodeType => typeof(PlannerRuleBase);


        protected override void OnAddNode(FGraph_NodeBase node)
        {
            PlannerRuleBase planner = node as PlannerRuleBase;

            if (planner)
            {
                planner.ParentPlanner = currentSetup as FieldPlanner;
                planner.ParentNodesContainer = planner.ParentPlanner;
            }

            base.OnAddNode(node);
            AutoConnectNode(node);
            RefreshNodes();
        }


        void AutoConnectNode(FGraph_NodeBase node)
        {
            if (node.DrawInputConnector == false) return;
            if (PGGPlanner_NodeBase.AutoSnap == false) return;

            FGraph_NodeBase lastConn = LastConnectedWithProceduresStart();

            if (lastConn != null)
            {
                lastConn.CreateConnectionWith(node, true, lastConn.HotOutputConnectionIndex);

                //if (PGGPlanner_NodeBase.AutoSnap)
                //{
                //    PGGPlanner_NodeBase nde = node as PGGPlanner_NodeBase;
                //    PGGPlanner_NodeBase lst = lastConn as PGGPlanner_NodeBase;
                //    if (nde && lst) lst.AlignViewedNodeWith(nde, true);
                //}

                lastConn._E_SetDirty();
                node._E_SetDirty();
            }
        }

        public override void OnGraphStructureChange()
        {
            base.OnGraphStructureChange();

            if (latestPlanner)
                if (latestPlanner.ParentBuildPlanner)
                {
                    latestPlanner.ParentBuildPlanner._Editor_GraphNodesChanged = true;
                }
        }

        List<FGraph_NodeBase> _loopStackOverflowPreventer = new List<FGraph_NodeBase>();
        FGraph_NodeBase LastConnectedWithProceduresStart()
        {
            FGraph_NodeBase sNode = FindNodeOfType(typeof(PE_Start));
            PE_Start startNode = null;
            if (sNode != null) startNode = sNode as PE_Start; else return null;

            if (sNode.FirstOutputConnection == null) return sNode;
            else
            {
                _loopStackOverflowPreventer.Clear();

                int limit = 0;
                while (sNode.FirstOutputConnection != null && sNode.FirstOutputConnection.DrawOutputConnector)
                {
                    if (_loopStackOverflowPreventer.Contains(sNode)) { Debug.Log("break"); break; }
                    _loopStackOverflowPreventer.Add(sNode);
                    limit += 1;
                    sNode = sNode.FirstOutputConnection;
                    if (limit > 1000) { Debug.Log("something wrong with node connections"); break; }
                }
            }

            return sNode;
        }


        //bool wasDrawed = false;
        bool reloadedOnReload = false;
        public override void DrawGraph()
        {
            if (AltVignette != null)
                if (BGVignetteStyle == null || BGVignetteStyle.normal.background != AltVignette)
                {
                    BGVignetteStyle = new GUIStyle();
                    BGVignetteStyle.normal.background = AltVignette;
                }


            if (latestPlanner)
            {
                latestPlanner.RefreshStartGraphNodes();

                if (latestPlanner.ProceduresBegin == null)
                {
                    PE_Start val = ScriptableObject.CreateInstance<PE_Start>();
                    val.NodePosition = new Vector2(690, 430);
                    latestPlanner.AddRuleToPlanner(val, false);
                    OnAddNode(val);
                }

                if (latestPlanner.PostProceduresBegin == null)
                {
                    PE_Start val = ScriptableObject.CreateInstance<PE_Start>();
                    val.NodePosition = new Vector2(690, 430);
                    latestPlanner.AddRuleToPlanner(val, true);
                    OnAddNode(val);
                    latestPlanner.RefreshStartGraphNodes();
                }
            }


            bool pre = EditorGUIUtility.wideMode;
            bool preh = EditorGUIUtility.hierarchyMode;

            EditorGUIUtility.wideMode = true;
            EditorGUIUtility.hierarchyMode = true;

            base.DrawGraph();

            EditorGUIUtility.wideMode = pre;
            EditorGUIUtility.hierarchyMode = preh;

            if (!reloadedOnReload)
            {
                reloadedOnReload = true;

                if (currentSetup != null)
                {
                    if (currentSetup is FieldPlanner)
                    {
                        FieldPlanner fp = currentSetup as FieldPlanner;
                        fp.RefreshOnReload();
                    }
                    else if (currentSetup is PlannerFunctionNode)
                    {
                        PlannerFunctionNode pf = currentSetup as PlannerFunctionNode;
                        pf.RefreshNodeParams();
                    }

                    //if (nodesToDraw.Count > 0)
                    //    if (wasDrawed == false)
                    //    {
                    //        wasDrawed = true;
                    //        ResetGraphPosition();
                    //    }
                }
            }
        }

        bool IsPostProc { get { return DisplayMode == FieldPlanner.EViewGraph.PostProcedures_Cells; } }

        public void RefreshTitle()
        {
            if (TopTitle != "") return;

            if (DisplayMode == FieldPlanner.EViewGraph.PostProcedures_Cells)
            {
                TopTitle = "Post Procedures (Cells Focus)";
            }
            else if (DisplayMode == FieldPlanner.EViewGraph.Procedures_Placement)
            {
                FieldPlanner planner = latestPlanner;
                if (planner)
                {
                    TopTitle = planner.name + "\n<size=10>Procedures (Layout Focus)</size>";
                }
                else
                    TopTitle = "Procedures (Layout Focus)";
            }
            else if (DisplayMode == FieldPlanner.EViewGraph.Procedures_CustomGraphs)
            {
                TopTitle = "Custom Graph" + "\n<size=10>" + CustomGraphName + "</size>";
            }
        }

        public string CustomGraphName = "";

        protected override void NodeModifyMenu(Event e, FGraph_NodeBase node)
        {
            GenericMenu menu = new GenericMenu();

            //if ((currentSetup is PlannerFunctionNode) == false)
            menu.AddItem(new GUIContent("REMOVE Node"), false, () =>
            {
                // Remove code
                ScheduleEditorEvent(() => RemoveNode(node));
            });

            //menu.AddItem(new GUIContent("Rename Node"), false, () =>
            //{
            //    string newName = FGenerators.RenamePopup(null, node.NameID);
            //    if (string.IsNullOrEmpty(newName) == false) { node.NameID = newName; /*SetDirty*/ }
            //});


            if (node is PlannerFunctionNode)
            {
                PlannerFunctionNode fNode = node as PlannerFunctionNode;

                if (fNode.ProjectFileParent)
                {
                    menu.AddItem(new GUIContent("Open Function Node Graph"), false, () =>
                    {
                        if (fNode.ProjectFileParent)
                            PlannerGraphWindow.Init(fNode.ProjectFileParent);
                    });
                }
            }

            if (!string.IsNullOrEmpty(node.GetNodeTooltipDescription))
                menu.AddItem(new GUIContent("Show node description"), false, () =>
                {
                    EditorUtility.DisplayDialog(node.GetDisplayName() + " Description", node.GetNodeTooltipDescription, "Ok");
                });

            if (!string.IsNullOrEmpty(node.GetNodeTooltipDescription))
                menu.AddItem(new GUIContent("Go to Planner Nodes List Doc (Google Drive)"), false, () =>
                {
                    Application.OpenURL("https://docs.google.com/document/d/11CG4T6aC6_ed7GUWO0c5zQCYALxwayXTCCeOy9lXKAk/edit?usp=drive_link");
                });


            menu.AddItem(GUIContent.none, false, () => { });


            menu.AddItem(new GUIContent(node._EditorCollapse ? "Reveal Node" : "Collapse Node"), false, () =>
            {
                node._EditorCollapse = !node._EditorCollapse;
            });


            menu.AddItem(GUIContent.none, false, () => { });



            menu.AddItem(new GUIContent("[Debugging] Display Node in the Inspector Window"), false, () =>
            {
                Selection.activeObject = node;
                //node.CheckForNulls();
                //node.CheckPortsForNullConnections();
            });

            menu.AddItem( new GUIContent( "[Debugging] Open Node Source Script File" ), false, () =>
            {
                MonoScript script = MonoScript.FromScriptableObject( node );
                AssetDatabase.OpenAsset( script );
            } );

            PlannerRuleBase plNode = node as PlannerRuleBase;
            if (plNode != null)
            {
                //    menu.AddItem(new GUIContent("[Debugging] Call Execution"), false, () =>
                //    {
                //        plNode.Execute(null, null);
                //    });

                menu.AddItem(new GUIContent("[Debugging] Refresh Ports (use only if there are port issues)"), false, () =>
                {
                    plNode.RemoveAllPortConnections();
                    plNode.CheckPortsForNullConnections();
                    plNode.RefreshPorts();
                });
            }

            menu.AddItem(new GUIContent("[Debugging] Switch Debug Variable"), node._EditorDebugMode, () =>
            {
                node._EditorDebugMode = !node._EditorDebugMode;
            });

            if (node is PlannerFunctionNode)
            {
                PlannerFunctionNode fNode = node as PlannerFunctionNode;
                if (fNode.ProjectFileParent)
                {
                    menu.AddItem(new GUIContent("[Debugging] Function Node - Rebuild Ports"), node._EditorDebugMode, () =>
                    {
                        fNode.RebuildPorts();
                    });
                }
            }


            menu.AddItem(GUIContent.none, false, () => { });

            menu.AddItem(new GUIContent("+ Create Custom Function (f) Node"), false, () =>
            {
                PlannerFunctionNode func = PlannerFunctionNode.CreateInstance<PlannerFunctionNode>();
                func.name = "FN_New Custom Function Node";
                AssetDatabase.CreateAsset(func, "Assets/FN_New Custom Function Node.asset");
                EditorGUIUtility.PingObject(func);
                AssetDatabase.OpenAsset(func);
            });


            DisplayMenuUnscaled(menu);
        }


        #region Searchable Dropdown Nodes List Modifications

        private static List<NodeRef> plannerNodes = new List<NodeRef>();
        public override List<NodeRef> GetNodesByNamespace()
        {
            plannerNodes.Clear();

            var assemblyNodes = base.GetNodesByNamespace();
            PlannerGraphWindow parent = Parent as PlannerGraphWindow;

            PlannerFunctionNode isFunction = currentSetup as PlannerFunctionNode;

            var functionNodes = AssetDatabase.FindAssets("t:PlannerFunctionNode");

            for (int i = 0; i < assemblyNodes.Count; i++)
            {
                var node = assemblyNodes[i];
                PGGPlanner_NodeBase plNode = node.node as PGGPlanner_NodeBase;

                // Select only wanted nodes
                if (isFunction != null) // Menu for function node graph
                {
                    if (plNode.NodeVisibility == PGGPlanner_NodeBase.EPlannerNodeVisibility.All || plNode.NodeVisibility == PGGPlanner_NodeBase.EPlannerNodeVisibility.JustFunctions)
                    {
                        plannerNodes.Add(node);
                    }
                }
                else // Planner Nodes
                {
                    if (plNode.NodeVisibility == PGGPlanner_NodeBase.EPlannerNodeVisibility.All || plNode.NodeVisibility == PGGPlanner_NodeBase.EPlannerNodeVisibility.JustPlanner)
                    {
                        plannerNodes.Add(node);
                    }
                }

            }

            for (int i = 0; i < functionNodes.Length; i++)
            {
                PlannerFunctionNode projectFunc = AssetDatabase.LoadAssetAtPath<PlannerFunctionNode>(AssetDatabase.GUIDToAssetPath(functionNodes[i]));

                if (projectFunc == isFunction) continue;
                if (projectFunc == null) continue;

                PlannerFunctionNode newFunc = PlannerFunctionNode.Instantiate(projectFunc);
                newFunc.ProjectFileParent = projectFunc;

                string nme = newFunc.DisplayName + " (f)";
                if (string.IsNullOrEmpty(nme)) nme = newFunc.name;

                if (string.IsNullOrEmpty(newFunc.CustomPath))
                {
                    nme = "Custom Function Nodes/" + nme;
                }
                else
                {
                    nme = newFunc.CustomPath + "/" + nme;
                }

                NodeRef r = new NodeRef() { name = nme, node = newFunc as FGraph_NodeBase };

                plannerNodes.Add(r);
            }


            return plannerNodes;
        }


        #endregion


        #region Adding / Removing Nodes implementation

        protected override void FillListWithNodesToDraw(List<FGraph_NodeBase> willBeDrawed)
        {
            if (Nodes == null) return;

            // Check for nulls
            for (int i = Nodes.Count - 1; i >= 0; i--)
            {
                if (Nodes[i] == null) Nodes.RemoveAt(i);
            }

            for (int i = 0; i < Nodes.Count; i++)
            {
                willBeDrawed.Add(Nodes[i]);
            }
        }

        bool IsFuncGraph
        {
            get
            {
                if (latestFunction) return true;
                return false;
            }
        }

        public override void AddNewNodeToPreset(FGraph_NodeBase node, bool moveToCursorPos = true)
        {
            if (IsFuncGraph)
            {
                bool containsAlready = false;

                if (node is PE_Start)
                    for (int i = 0; i < latestFunction.Procedures.Count; i++)
                    {
                        if (latestFunction.Procedures[i] is PE_Start) { containsAlready = true; break; }
                    }

                if (containsAlready)
                {
                    UnityEngine.Debug.Log("[PGG] Only one 'Procedures Start' is allowed inside one graph");
                    return;
                }
            }

            if (moveToCursorPos) node.NodePosition = GetNodeCreatePos();

            if (latestCustomGraph != null)
            {
                latestCustomGraph.NodesHandler_AddRule(node);
                //Nodes.Add(node as PlannerRuleBase);
                //base.AddNewNodeToPreset(node, moveToCursorPos);
                OnAddNode(node);
            }
            else
            if (latestPlanner == null) // Function
            {
                Nodes.Add(node as PlannerRuleBase);
                base.AddNewNodeToPreset(node, moveToCursorPos);
                OnAddNode(node);
            }
            else
            {
                latestPlanner.AddRuleToPlanner(node as PlannerRuleBase, IsPostProc);
                //base.AddNewNodeToPreset(node, moveToCursorPos);
                OnAddNode(node);
                latestPlanner.RefreshStartGraphNodes();
            }
            //PlannerFunctionNode func = node as PlannerFunctionNode;
            //if (func)
            //{
            //    UnityEngine.Debug.Log("function!");
            //}
        }

        protected override void RemoveNode(FGraph_NodeBase node)
        {
            Nodes.Remove(node as PlannerRuleBase);
            base.RemoveNode(node);

            if (latestFunction)
            {
                latestFunction.OnNodeRemove(node);
            }
            else if (latestPlanner)
            {
                latestPlanner.OnNodeRemove(node);
            }
            else if (latestCustomGraph != null)
            {
                latestCustomGraph.NodesHandler_OnNodeRemove(node);
            }
        }


        protected override void DoubleClickNode(Event e, FGraph_NodeBase node)
        {
            base.DoubleClickNode(e, node);

            if (node is PlannerFunctionNode)
            {
                PlannerFunctionNode fNode = node as PlannerFunctionNode;

                if (fNode.ProjectFileParent)
                    PlannerGraphWindow.Init(fNode.ProjectFileParent);
            }
        }

        protected override void DoubleMMBClick(Event e)
        {
            RequestPlannerRefresh();
        }

        protected override void MMBClick()
        {
            if (pressedKeys.Contains(KeyCode.R) || pressedKeys.Contains(KeyCode.N) || pressedKeys.Contains(KeyCode.C) || pressedKeys.Contains(KeyCode.LeftShift))
            {
                if (latestPlanner)
                {
                    if (latestPlanner.ParentBuildPlanner)
                    {
                        RequestPlannerRefresh();
                    }
                }
            }
        }

        public void RequestPlannerRefresh()
        {
            if (latestPlanner == null) return;
            if (latestPlanner.ParentBuildPlanner == null) return;

            latestPlanner.ParentBuildPlanner._Editor_GraphNodesChanged = true;
            latestPlanner.ParentBuildPlanner._Editor_GraphNodesChangedForced = true;
            repaintRequest = true;
            SceneView.RepaintAll();
            Event.current.Use();
        }

        protected override void LMBFreeClick()
        {
            if (pressedKeys.Count < 1) return;

            if (pressedKeys.Contains(KeyCode.Alpha1))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Value>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.Alpha2))
            {
                PR_Value val = ScriptableObject.CreateInstance<PR_Value>();
                val.InputType = EType.Vector3;
                AddNewNodeToPreset(val);
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.Alpha3))
            {
                PR_Value val = ScriptableObject.CreateInstance<PR_Value>();
                val._EditorFoldout = false;
                val.InputType = EType.Vector3;
                AddNewNodeToPreset(val);
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.S))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Subtract>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.A))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Add>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.G))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_GetLocalVariable>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.V) || pressedKeys.Contains(KeyCode.F) || pressedKeys.Contains(KeyCode.P))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_GetFieldPlannerVariable>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.B))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_GetBuildVariable>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.L))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Lerp>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.D))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Divide>());
                //AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_DiscardField>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.I))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_GetFieldSelector>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.R))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_SetFieldRotation>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.W))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_SetFieldPosition>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.X))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Rewire>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.C))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Comment>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.X))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Rewire>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.X))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Rewire>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.N))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Normalize>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.B))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Split>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.O))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_OneMinus>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.M))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Multiply>());
                LMBUnclickPrevent();
            }
            //else if (pressedKeys.Contains(KeyCode.X))
            //{
            //    AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Rewire>());
            //    LMBUnclickPrevent();
            //}
            //else if (pressedKeys.Contains(KeyCode.X))
            //{
            //    AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Rewire>());
            //    LMBUnclickPrevent();
            //}
            //else if (pressedKeys.Contains(KeyCode.X))
            //{
            //    AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Rewire>());
            //    LMBUnclickPrevent();
            //}
        }


        #endregion


        #region Override Connection draw style

        protected override Vector3 DrawConnectionWithTangents(Vector2 start, Vector2 end, bool outToInput = true, Color? color = null, float thickness = 5, bool getMiddlePoint = false, Texture2D lineTex = null, FGraph_NodeBase_Drawer.EConnectorsWireMode wireMode = FGraph_NodeBase_Drawer.EConnectorsWireMode.Left_Right, float oXTan = 0.8f, float iXTan = 0.8f, float oYTan = 0.6f, float iYTan = 0.6f, float tanMul = 1f)
        {
            if (wireMode == FGraph_NodeBase_Drawer.EConnectorsWireMode.Left_Right)
            {
                if (end.y < start.y)
                    return base.DrawConnectionWithTangents(start, end, outToInput, color, thickness * 0.6f, getMiddlePoint, null, wireMode, 0.8f, 0.8f, 0.6f, 0.6f, tanMul);
                else
                    return base.DrawConnectionWithTangents(start, end, outToInput, color, thickness * 0.6f, getMiddlePoint, null, wireMode, 0.9f, 0.5f, 1.05f, 0.6f, tanMul);
            }

            Vector2 a = start, b = end;
            Vector2 toTarget = b - a;
            if (toTarget.sqrMagnitude < 100) return start;

            if (end.y > start.y)
            {
                return base.DrawConnectionWithTangents(start, end, outToInput, color, thickness, getMiddlePoint, PlannerGraphWindow.PlannerGraphStyles.TEX_Gradient1, wireMode, 0.8f, 0.8f, 0.6f, 0.6f, tanMul);
            }
            else // Direct to above
            {
                if ((end.x > start.x) && _DrawConnection_input && _DrawConnection_output)
                {
                    Vector3 nend = _DrawConnection_input._E_LatestRect.center - new Vector2((_DrawConnection_input.NodeSize.x / 2f - 25), -_DrawConnection_input.NodeSize.y * 0.475f + 18);
                    base.DrawConnectionWithTangents(nend, end, outToInput, new Color(0.7f, 0.7f, 0.7f, 0.35f), thickness, getMiddlePoint, PlannerGraphWindow.PlannerGraphStyles.TEX_Gradient1, wireMode, 0.4f, 1.0f, -0.25f, -0.1f, tanMul);
                    return base.DrawConnectionWithTangents(start, nend, outToInput, color, thickness, getMiddlePoint, PlannerGraphWindow.PlannerGraphStyles.TEX_Gradient1, FGraph_NodeBase_Drawer.EConnectorsWireMode.Left_Right, 0.8f, 0.9f, 0.0f, -0.3f, tanMul);
                }
                else
                    return base.DrawConnectionWithTangents(start, end, outToInput, color, thickness, getMiddlePoint, PlannerGraphWindow.PlannerGraphStyles.TEX_Gradient1, wireMode, 0.8f, 0.8f, 0.6f, 0.6f, tanMul);
            }
        }


        #endregion


        #region Copy Paste Implementation

        //protected override FGraph_NodeBase ProcessPasteOf(Vector2 originPos, Vector2 mostTopL, FGraph_NodeBase node)
        //{
        //    if (node is PlannerFunctionNode)
        //    {
        //        PlannerFunctionNode coreFunc = node as PlannerFunctionNode;
        //        PlannerFunctionNode func = PlannerFunctionNode.Instantiate(coreFunc.ProjectFileParent);

        //        AddNewNodeToPreset(func, false);
        //        func.NodePosition = originPos + /* copied node position -> */ node.NodePosition - mostTopL;
        //        func.RefreshPorts();

        //        return func;
        //    }
        //    else
        //    {
        //        ScriptableObject inst = ScriptableObject.CreateInstance(node.GetType());
        //        FGraph_NodeBase nde = inst as FGraph_NodeBase;

        //        AddNewNodeToPreset(nde, false);
        //        nde.NodePosition = originPos + /* copied node position -> */ node.NodePosition - mostTopL;

        //        // TODO: Copy Parameters Values
        //        //if ( node.baseSerializedObject != null && nde.baseSerializedObject != null)
        //        //{
        //        //    var iterator = node.baseSerializedObject.GetIterator();
        //        //    iterator.Next(true);
        //        //    iterator.NextVisible(false);

        //        //    while (iterator.NextVisible(false))
        //        //    {
        //        //        UnityEngine.Debug.Log("V: " + iterator.displayName);
        //        //    }
        //        //}

        //        return nde;
        //    }
        //}

        #endregion


        protected override bool IsCursorInGraph()
        {
            if (objRefRect.Contains(eventMousePos)) return false;
            if (refrButtonRect.Contains(eventMousePos)) return false;
            return base.IsCursorInGraph();
        }

        protected override bool IsCursorInAdditionalActionArea()
        {
            if (refrButtonRect.Contains(eventMousePos)) return true;
            return false;
        }


        Event cursorOutOfGraphEventForward = null;
        Rect refrButtonRect = new Rect();
        Rect objRefRect = new Rect();
        public string TopTitle = "";


        protected override void DrawGraphOverlay()
        {
            float lOffset = 0f;
            bool refrButton = false;
            if (latestPlanner != null) if (latestPlanner.ParentBuildPlanner) refrButton = true;
            if (refrButton) lOffset = 28f;

            // Fowarding overlay input to function buttons
            if (cursorOutOfGraphEventForward != null)
            {
                Event.current = cursorOutOfGraphEventForward;
                cursorOutOfGraphEventForward = null;
            }

            //if (latestPlanner != null) lOffset = 28f;

            Rect up = new Rect(0f, TopMarginOffset - 3, 24, 20);
            Rect r = new Rect(up);

            r.position += new Vector2(10 + lOffset, 10);

            if (GUI.Button(r, new GUIContent(FGUI_Resources.TexTargetingIcon, "Reset view to center"), FGUI_Resources.ButtonStyle))
            {
                ResetGraphPosition();
                if (Event.current != null) Event.current.Use();
            }

            if (refrButton)
            {
                r = new Rect(up);
                r.position += new Vector2(10, 10);

                if (GUI.Button(r, new GUIContent(FGUI_Resources.Tex_Refresh, "Call 'Next Preview'"), FGUI_Resources.ButtonStyle))
                {
                    RequestPlannerRefresh();
                    if (Event.current != null) Event.current.Use();
                    //latestPlanner.ParentBuildPlanner._Editor_GraphNodesChanged = true;
                    //latestPlanner.ParentBuildPlanner._Editor_GraphNodesChangedForced = true;
                    //repaintRequest = true;
                    //SceneView.RepaintAll();
                }
            }

            #region Hourglass

            //if (latestPlanner)
            //{
            //    if (latestPlanner.ParentBuildPlanner)
            //    {
            //        if (latestPlanner.ParentBuildPlanner.IsGenerating)
            //        {
            //            r = new Rect(up);
            //            //float elaps = (float)EditorApplication.timeSinceStartup;
            //            r.size = new Vector2(22, 22);
            //            r.position += new Vector2(16,38);//ew Vector2(4+Mathf.Cos(elaps * 6f) * 3f, 30);
            //            Color preC = GUI.color;
            //            GUI.color = new Color(1f, 1f, 1f, 0.4f);
            //            GUI.DrawTexture(r, FGUI_Resources.TexWaitIcon);
            //            GUI.color = preC;
            //        }
            //    }
            //}

            #endregion

            r = new Rect(up);
            r.position += new Vector2(38 + lOffset, 10);

            if (!PGGPlanner_NodeBase.AutoSnap) GUI.backgroundColor = Color.gray;

            //if (GUI.Button(r, new GUIContent(FGUI_Resources.Tex_Drag, "Enable/Disable auto-snap node position on connection creation"), FGUI_Resources.ButtonStyle))
            if (GUI.Button(r, new GUIContent(FGUI_Resources.Tex_Drag, "Enable/Disable auto-trigger port connection creation with lastest trigger node"), FGUI_Resources.ButtonStyle))
            {
                PGGPlanner_NodeBase.AutoSnap = !PGGPlanner_NodeBase.AutoSnap;
                if (Event.current != null) Event.current.Use();
            }

            GUI.backgroundColor = Color.white;

            if (TopTitle != "")
            {
                float centerX = graphDisplayRect.width / 2f;
                Rect titleR = new Rect(centerX - 250f, TopMarginOffset - 3, 500f, 38f);
                GUI.color = new Color(0.9f, 0.9f, 0.9f, 0.2f);
                GUI.Label(titleR, TopTitle, FGUI_Resources.HeaderStyle);
                GUI.color = Color.white;
            }

            r.position -= new Vector2(60, 0);
            r.width += 64;
            r.height += 10;
            refrButtonRect = r;

            if (currentSetup is PlannerFunctionNode)
            {
                PlannerFunctionNode func = currentSetup as PlannerFunctionNode;
                r.position = new Vector2(graphDisplayRect.width - 160, r.position.y);
                r.width = 150;
                r.height = 18;
                EditorGUI.ObjectField(r, func, typeof(PlannerFunctionNode), false);
                r.width -= 20;
                r.height += 6;
                objRefRect = r;
            }
            else if (currentSetup is FieldPlanner)
            {
                if (DrawedInsideInspector == false)
                {
                    FieldPlanner func = currentSetup as FieldPlanner;
                    r.position = new Vector2(graphDisplayRect.width - 160, r.position.y);
                    r.width = 150;
                    r.height = 18;
                    EditorGUI.ObjectField(r, func, typeof(FieldPlanner), false);
                    r.width -= 20;
                    objRefRect = r;
                }
            }
            else
            {
                objRefRect = new Rect();
            }

        }


        public override FGraph_NodeBase_Drawer GetNodeDrawer(FGraph_NodeBase node)
        {
            if (node is PGGPlanner_ExecutionNode)
            {
                return new PlannerExecutionNode_Drawer(node);
            }
            else
                return new PlannerNode_Drawer(node);
        }

        public bool CheckDisplayRepaintRequest(double repaintID)
        {
            if (_LatestRefreshDisplayFlag != repaintID)
            {
                ForceGraphRepaint();
                _LatestRefreshDisplayFlag = repaintID;
                return true;
            }

            return false;
        }
    }


}