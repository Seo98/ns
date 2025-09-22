using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

public class ManualClassVisualizer : EditorWindow
{
    private Vector2 scrollPos;
    private List<ScriptInfo> scripts = new List<ScriptInfo>();
    private Vector2 dragOffset;
    private ScriptInfo draggingScript;

    [System.Serializable]
    public class ScriptInfo
    {
        public MonoScript script;
        public string className;
        public List<string> publicMethods = new List<string>();
        public List<ReferenceInfo> references = new List<ReferenceInfo>();
        public Vector2 position;
        public Rect rect;

        public ScriptInfo(MonoScript monoScript)
        {
            script = monoScript;
            className = script.name;
            position = new Vector2(UnityEngine.Random.Range(100, 500), UnityEngine.Random.Range(100, 400));
            AnalyzeScript();
        }

        public void AnalyzeScript()
        {
            Type scriptType = script.GetClass();
            if (scriptType == null) return;

            // Public 메서드 분석
            publicMethods.Clear();
            MethodInfo[] methods = scriptType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (MethodInfo method in methods)
            {
                if (!method.Name.StartsWith("get_") && !method.Name.StartsWith("set_") &&
                    method.Name != "Start" && method.Name != "Update" && method.Name != "Awake" &&
                    method.Name != "OnEnable" && method.Name != "OnDisable" && method.Name != "OnDestroy")
                {
                    var parameters = method.GetParameters();
                    string[] paramNames = new string[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        paramNames[i] = parameters[i].ParameterType.Name;
                    }
                    string paramStr = string.Join(", ", paramNames);
                    publicMethods.Add($"{method.Name}({paramStr})");
                }
            }

            // 참조 관계 분석
            references.Clear();
            FieldInfo[] fields = scriptType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                bool isSerializeField = field.IsPublic ||
                    System.Attribute.IsDefined(field, typeof(SerializeField));

                if (isSerializeField)
                {
                    if (field.FieldType.IsSubclassOf(typeof(MonoBehaviour)))
                    {
                        references.Add(new ReferenceInfo
                        {
                            fieldName = field.Name,
                            targetType = field.FieldType.Name,
                            referenceType = ReferenceType.Component
                        });
                    }
                    else if (field.FieldType.IsSubclassOf(typeof(ScriptableObject)))
                    {
                        references.Add(new ReferenceInfo
                        {
                            fieldName = field.Name,
                            targetType = field.FieldType.Name,
                            referenceType = ReferenceType.ScriptableObject
                        });
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class ReferenceInfo
    {
        public string fieldName;
        public string targetType;
        public ReferenceType referenceType;
    }

    public enum ReferenceType
    {
        Component,
        ScriptableObject,
        Interface
    }

    [MenuItem("Tools/Manual Class Visualizer")]
    static void Init()
    {
        ManualClassVisualizer window = GetWindow<ManualClassVisualizer>("Manual Class Visualizer");
        window.Show();
    }

    void OnGUI()
    {
        DrawHeader();
        DrawDropArea();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        // 참조선 먼저 그리기
        DrawReferences();

        // 스크립트 박스 그리기
        foreach (var scriptInfo in scripts)
        {
            DrawScriptBox(scriptInfo);
        }

        EditorGUILayout.EndScrollView();

        HandleEvents();
    }

    void DrawHeader()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Clear All", GUILayout.Width(80)))
        {
            scripts.Clear();
        }

        if (GUILayout.Button("Auto Layout", GUILayout.Width(100)))
        {
            AutoLayoutScripts();
        }

        if (GUILayout.Button("Refresh Analysis", GUILayout.Width(120)))
        {
            foreach (var scriptInfo in scripts)
            {
                scriptInfo.AnalyzeScript();
            }
        }

        EditorGUILayout.LabelField($"스크립트: {scripts.Count}개", EditorStyles.miniLabel);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);
    }

    void DrawDropArea()
    {
        // 드래그 앤 드롭 영역
        Rect dropArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "여기에 MonoScript를 드래그하세요", EditorStyles.helpBox);

        // 드래그 앤 드롭 처리
        Event evt = Event.current;
        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    break;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is MonoScript monoScript)
                        {
                            AddScript(monoScript);
                        }
                    }
                }
                break;
        }
    }

    void AddScript(MonoScript monoScript)
    {
        // 중복 확인
        foreach (var existingScript in scripts)
        {
            if (existingScript.script == monoScript)
            {
                Debug.Log($"{monoScript.name}은 이미 추가되었습니다.");
                return;
            }
        }

        // 유효한 MonoBehaviour인지 확인
        Type scriptType = monoScript.GetClass();
        if (scriptType != null && scriptType.IsSubclassOf(typeof(MonoBehaviour)))
        {
            scripts.Add(new ScriptInfo(monoScript));
            Debug.Log($"{monoScript.name} 추가됨");
        }
        else
        {
            Debug.Log($"{monoScript.name}은 MonoBehaviour가 아닙니다.");
        }
    }

    void DrawScriptBox(ScriptInfo scriptInfo)
    {
        // 박스 크기 계산
        int methodCount = scriptInfo.publicMethods.Count;
        int referenceCount = scriptInfo.references.Count;
        float height = 60 + (methodCount * 16) + (referenceCount * 16) + 20;

        scriptInfo.rect = new Rect(scriptInfo.position.x, scriptInfo.position.y, 220, height);

        // 배경 - window 스타일 대신 helpBox 사용
        GUI.Box(scriptInfo.rect, "", EditorStyles.helpBox);

        float yOffset = 5;

        // 헤더 (클래스명 + 제거 버튼)
        Rect headerRect = new Rect(scriptInfo.rect.x + 5, scriptInfo.rect.y + yOffset, scriptInfo.rect.width - 30, 20);
        EditorGUI.LabelField(headerRect, scriptInfo.className, EditorStyles.boldLabel);

        // 제거 버튼
        Rect removeRect = new Rect(scriptInfo.rect.x + scriptInfo.rect.width - 25, scriptInfo.rect.y + yOffset, 20, 18);
        if (GUI.Button(removeRect, "×", EditorStyles.miniButton))
        {
            scripts.Remove(scriptInfo);
            return;
        }

        yOffset += 25;

        // 구분선
        EditorGUI.DrawRect(new Rect(scriptInfo.rect.x + 5, scriptInfo.rect.y + yOffset, scriptInfo.rect.width - 10, 1), Color.gray);
        yOffset += 5;

        // 참조 섹션
        if (referenceCount > 0)
        {
            EditorGUI.LabelField(new Rect(scriptInfo.rect.x + 8, scriptInfo.rect.y + yOffset, 100, 15), "References:", EditorStyles.miniLabel);
            yOffset += 18;

            foreach (var reference in scriptInfo.references)
            {
                Color refColor = reference.referenceType == ReferenceType.Component ? Color.cyan : Color.yellow;
                GUI.color = refColor;
                string refText = $"→ {reference.fieldName}: {reference.targetType}";
                EditorGUI.LabelField(new Rect(scriptInfo.rect.x + 15, scriptInfo.rect.y + yOffset, scriptInfo.rect.width - 20, 15),
                                    refText, EditorStyles.miniLabel);
                GUI.color = Color.white;
                yOffset += 16;
            }

            yOffset += 5;
        }

        // 메서드 섹션
        if (methodCount > 0)
        {
            EditorGUI.LabelField(new Rect(scriptInfo.rect.x + 8, scriptInfo.rect.y + yOffset, 100, 15), "Public Methods:", EditorStyles.miniLabel);
            yOffset += 18;

            foreach (string method in scriptInfo.publicMethods)
            {
                EditorGUI.LabelField(new Rect(scriptInfo.rect.x + 15, scriptInfo.rect.y + yOffset, scriptInfo.rect.width - 20, 15),
                                    $"• {method}", EditorStyles.miniLabel);
                yOffset += 16;
            }
        }
    }

    void DrawReferences()
    {
        foreach (var scriptInfo in scripts)
        {
            Vector2 startPos = new Vector2(scriptInfo.rect.x + scriptInfo.rect.width, scriptInfo.rect.y + scriptInfo.rect.height / 2);

            foreach (var reference in scriptInfo.references)
            {
                // 참조 대상 스크립트 찾기
                ScriptInfo targetScript = null;
                foreach (var script in scripts)
                {
                    if (script.className == reference.targetType)
                    {
                        targetScript = script;
                        break;
                    }
                }

                if (targetScript != null)
                {
                    Vector2 endPos = new Vector2(targetScript.rect.x, targetScript.rect.y + targetScript.rect.height / 2);

                    // 참조 타입에 따른 색상
                    Color lineColor = reference.referenceType == ReferenceType.Component ? Color.blue : Color.green;
                    DrawArrow(startPos, endPos, lineColor, reference.fieldName);
                }
            }
        }
    }

    void DrawArrow(Vector2 start, Vector2 end, Color color, string label)
    {
        Handles.BeginGUI();
        Handles.color = color;

        // 곡선 그리기
        Vector2 tangent = (end - start) * 0.5f;
        tangent.x = Mathf.Abs(tangent.x);
        Handles.DrawBezier(start, end, start + tangent, end - tangent, color, null, 2f);

        // 화살표 머리
        Vector2 direction = (end - start).normalized;
        Vector2 arrowHead1 = end - direction * 12 + Vector2.Perpendicular(direction) * 6;
        Vector2 arrowHead2 = end - direction * 12 - Vector2.Perpendicular(direction) * 6;

        Handles.DrawLine(end, arrowHead1);
        Handles.DrawLine(end, arrowHead2);

        // 레이블
        Vector2 midPoint = (start + end) * 0.5f;
        Rect labelRect = new Rect(midPoint.x - 30, midPoint.y - 8, 60, 16);
        GUI.Label(labelRect, label, EditorStyles.miniLabel);

        Handles.EndGUI();
    }

    void HandleEvents()
    {
        Event e = Event.current;

        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    foreach (var scriptInfo in scripts)
                    {
                        if (scriptInfo.rect.Contains(e.mousePosition))
                        {
                            draggingScript = scriptInfo;
                            dragOffset = e.mousePosition - scriptInfo.position;
                            e.Use();
                            break;
                        }
                    }
                }
                break;

            case EventType.MouseDrag:
                if (draggingScript != null)
                {
                    draggingScript.position = e.mousePosition - dragOffset;
                    Repaint();
                    e.Use();
                }
                break;

            case EventType.MouseUp:
                if (e.button == 0)
                {
                    draggingScript = null;
                }
                break;
        }
    }

    void AutoLayoutScripts()
    {
        if (scripts.Count == 0) return;

        // 간단한 그리드 레이아웃
        int columns = Mathf.CeilToInt(Mathf.Sqrt(scripts.Count));
        int rows = Mathf.CeilToInt((float)scripts.Count / columns);

        float spacing = 250f;
        Vector2 startPos = new Vector2(50, 50);

        for (int i = 0; i < scripts.Count; i++)
        {
            int row = i / columns;
            int col = i % columns;

            scripts[i].position = startPos + new Vector2(col * spacing, row * (spacing * 0.8f));
        }

        Repaint();
    }
}