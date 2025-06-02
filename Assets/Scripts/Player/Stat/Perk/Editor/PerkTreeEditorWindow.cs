using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class PerkTreeEditorWindow : EditorWindow
{
    private PerkTreeData treeData;
    private readonly Dictionary<PerkData, Rect> nodeRects = new();

    private Vector2 scrollPos;
    private Vector2 dragOffset;
    private bool isDraggingCanvas = false;

    private const float NODE_WIDTH = 160f;
    private const float NODE_HEIGHT = 80f;
    private const float GRID_SIZE = 20f;

    private int? selectedNodeIndex = null;

    [MenuItem("Tools/Perk Tree Editor")]
    public static void OpenWindow()
    {
        GetWindow<PerkTreeEditorWindow>("Perk Tree Editor");
    }

    private void OnGUI()
    {
        HandleCanvasDragging();

        EditorGUILayout.Space();
        treeData = (PerkTreeData)EditorGUILayout.ObjectField("Perk Tree Data", treeData, typeof(PerkTreeData), false);

        if (treeData == null) return;
        treeData.nodes ??= new List<PerkNodeData>();

        EditorGUILayout.Space();
        DrawToolbar();
        DrawGrid();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, true, true);
        GUILayout.BeginHorizontal();
        GUILayout.Space(5000);
        GUILayout.BeginVertical();
        GUILayout.Space(2000);

        BeginWindows();
        nodeRects.Clear();

        for (int i = 0; i < treeData.nodes.Count; i++)
        {
            var node = treeData.nodes[i];
            Rect rect = new(node.position, new Vector2(NODE_WIDTH, NODE_HEIGHT));

            Color originalColor = GUI.backgroundColor;
            if (selectedNodeIndex == i) GUI.backgroundColor = Color.cyan;

            rect = GUI.Window(i, rect, id => DrawNodeWindow(id, node), node.perk != null ? node.perk.displayName : "(Empty)");
            node.position = SnapToGrid(rect.position);

            if (node.perk != null)
                nodeRects[node.perk] = rect;

            GUI.backgroundColor = originalColor;
        }

        EndWindows();
        DrawConnections();

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }

    private void HandleCanvasDragging()
    {
        Event e = Event.current;
        if (e.button == 2)
        {
            if (e.type == EventType.MouseDown)
            {
                isDraggingCanvas = true;
                dragOffset = e.mousePosition;
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && isDraggingCanvas)
            {
                Vector2 delta = dragOffset - e.mousePosition;
                scrollPos += delta;
                dragOffset = e.mousePosition;
                e.Use();
                Repaint();
            }
            else if (e.type == EventType.MouseUp)
            {
                isDraggingCanvas = false;
            }
        }
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("노드 추가", GUILayout.Height(30)))
        {
            treeData.nodes ??= new List<PerkNodeData>();

            var newNode = new PerkNodeData
            {
                position = new Vector2(200, 200),
                perk = null
            };

            Undo.RecordObject(treeData, "Add Perk Node");
            treeData.nodes.Add(newNode);
            EditorUtility.SetDirty(treeData);
            Repaint();
        }

        if (selectedNodeIndex != null)
        {
            if (GUILayout.Button("선택된 노드 삭제", GUILayout.Height(30)))
            {
                if (selectedNodeIndex.Value >= 0 && selectedNodeIndex.Value < treeData.nodes.Count)
                {
                    Undo.RecordObject(treeData, "Delete Perk Node");
                    treeData.nodes.RemoveAt(selectedNodeIndex.Value);
                    selectedNodeIndex = null;
                    EditorUtility.SetDirty(treeData);
                    Repaint();
                }
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawNodeWindow(int id, PerkNodeData node)
    {
        if (GUILayout.Button("선택", GUILayout.Height(20)))
        {
            selectedNodeIndex = id;
        }

        EditorGUI.BeginChangeCheck();
        node.perk = (PerkData)EditorGUILayout.ObjectField(node.perk, typeof(PerkData), false);
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(treeData);
        }

        GUI.DragWindow();
    }

    private void DrawConnections()
    {
        Handles.BeginGUI();
        HashSet<(PerkData from, PerkData to)> visited = new();

        foreach (var node in treeData.nodes)
        {
            if (node.perk == null || node.perk.prerequisites == null) continue;
            if (!nodeRects.TryGetValue(node.perk, out Rect fromRect)) continue;

            foreach (var prereq in node.perk.prerequisites)
            {
                if (prereq == null || !nodeRects.TryGetValue(prereq, out Rect toRect)) continue;
                if (!visited.Add((node.perk, prereq))) continue;
                if (visited.Contains((prereq, node.perk)))
                {
                    Debug.LogWarning($"[PerkTreeEditor] 순환 참조 감지: {node.perk.name} <-> {prereq.name}");
                    continue;
                }

                Vector3 start = new(fromRect.xMin, fromRect.center.y);
                Vector3 end = new(toRect.xMax, toRect.center.y);
                DrawArrow(end, start);
            }
        }

        Handles.EndGUI();
    }
    
    private void DrawArrow(Vector3 from, Vector3 to)
    {
        Vector3 direction = (to - from).normalized;
        Vector3 right = Quaternion.Euler(0, 0, 135) * direction;
        Vector3 left = Quaternion.Euler(0, 0, -135) * direction;

        Handles.color = Color.white;
        Handles.DrawLine(from, to);
        Handles.DrawLine(to, to + right * 6f);
        Handles.DrawLine(to, to + left * 6f);
    }

    private void DrawGrid()
    {
        Color originalColor = Handles.color;
        Handles.color = new Color(1f, 1f, 1f, 0.05f);

        float width = 5000;
        float height = 2000;

        for (float x = 0; x < width; x += GRID_SIZE)
        {
            Handles.DrawLine(new Vector3(x, 0, 0), new Vector3(x, height, 0));
        }

        for (float y = 0; y < height; y += GRID_SIZE)
        {
            Handles.DrawLine(new Vector3(0, y, 0), new Vector3(width, y, 0));
        }

        Handles.color = originalColor;
    }

    private Vector2 SnapToGrid(Vector2 pos)
    {
        pos.x = Mathf.Round(pos.x / GRID_SIZE) * GRID_SIZE;
        pos.y = Mathf.Round(pos.y / GRID_SIZE) * GRID_SIZE;
        return pos;
    }
}
