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
        public List<MethodCallInfo> methodCalls = new List<MethodCallInfo>();
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

            // 메서드 호출 관계 분석 (간단 버전)
            methodCalls.Clear();
            string sourceCode = script.text;

            foreach (var method in publicMethods)
            {
                string methodName = method.Split('(')[0]; // e.g. MovePlayer(int) → MovePlayer

                int index = sourceCode.IndexOf(methodName + "(", StringComparison.Ordinal);
                if (index < 0) continue;

                int braceStart = sourceCode.IndexOf("{", index);
                if (braceStart < 0) continue;

                int braceDepth = 0;
                int braceEnd = -1;
                for (int i = braceStart; i < sourceCode.Length; i++)
                {
                    if (sourceCode[i] == '{') braceDepth++;
                    else if (sourceCode[i] == '}') braceDepth--;
                    if (braceDepth == 0) { braceEnd = i; break; }
                }

                if (braceEnd < 0) continue;
                string methodBody = sourceCode.Substring(braceStart, braceEnd - braceStart);

                foreach (var other in publicMethods)
                {
                    string otherName = other.Split('(')[0];
                    if (otherName == methodName) continue;
                    if (methodBody.Contains(otherName + "("))
                    {
                        methodCalls.Add(new MethodCallInfo
                        {
                            caller = methodName,
                            callee = otherName
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

    public class MethodCallInfo
    {
        public string caller;
        public string callee;
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

        // 메서드 종속관계도 그리기
        foreach (var scriptInfo in scripts)
        {
            DrawMethodDependencies(scriptInfo);
        }

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
        Rect dropArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "여기에 MonoScript를 드래그하세요", EditorStyles.helpBox);

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
        foreach (var existingScript in scripts)
        {
            if (existingScript.script == monoScript)
            {
                Debug.Log($"{monoScript.name}은 이미 추가되었습니다.");
                return;
            }
        }

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
        int methodCount = scriptInfo.publicMethods.Count;
        int referenceCount = scriptInfo.references.Count;
        float height = 60 + (methodCount * 16) + (referenceCount * 16) + 20;

        scriptInfo.rect = new Rect(scriptInfo.position.x, scriptInfo.position.y, 220, height);

        GUI.Box(scriptInfo.rect, "", EditorStyles.helpBox);

        float yOffset = 5;

        Rect headerRect = new Rect(scriptInfo.rect.x + 5, scriptInfo.rect.y + yOffset, scriptInfo.rect.width - 30, 20);
        EditorGUI.LabelField(headerRect, scriptInfo.className, EditorStyles.boldLabel);

        Rect removeRect = new Rect(scriptInfo.rect.x + scriptInfo.rect.width - 25, scriptInfo.rect.y + yOffset, 20, 18);
        if (GUI.Button(removeRect, "×", EditorStyles.miniButton))
        {
            scripts.Remove(scriptInfo);
            return;
        }

        yOffset += 25;
        EditorGUI.DrawRect(new Rect(scriptInfo.rect.x + 5, scriptInfo.rect.y + yOffset, scriptInfo.rect.width - 10, 1), Color.gray);
        yOffset += 5;

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
                ScriptInfo targetScript = scripts.FirstOrDefault(s => s.className == reference.targetType);
                if (targetScript != null)
                {
                    Vector2 endPos = new Vector2(targetScript.rect.x, targetScript.rect.y + targetScript.rect.height / 2);
                    Color lineColor = reference.referenceType == ReferenceType.Component ? Color.blue : Color.green;
                    DrawArrow(startPos, endPos, lineColor, reference.fieldName);
                }
            }
        }
    }

    void DrawMethodDependencies(ScriptInfo scriptInfo)
    {
        foreach (var call in scriptInfo.methodCalls)
        {
            Vector2 start = new Vector2(scriptInfo.rect.center.x, scriptInfo.rect.center.y);
            Vector2 end = start + new Vector2(100, UnityEngine.Random.Range(-40, 40));

            DrawArrow(start, end, Color.red, $"{call.caller} → {call.callee}");
        }
    }

    void DrawArrow(Vector2 start, Vector2 end, Color color, string label)
    {
        Handles.BeginGUI();
        Handles.color = color;

        Vector2 tangent = (end - start) * 0.5f;
        tangent.x = Mathf.Abs(tangent.x);
        Handles.DrawBezier(start, end, start + tangent, end - tangent, color, null, 2f);

        Vector2 direction = (end - start).normalized;
        Vector2 arrowHead1 = end - direction * 12 + Vector2.Perpendicular(direction) * 6;
        Vector2 arrowHead2 = end - direction * 12 - Vector2.Perpendicular(direction) * 6;

        Handles.DrawLine(end, arrowHead1);
        Handles.DrawLine(end, arrowHead2);

        Vector2 midPoint = (start + end) * 0.5f;
        Rect labelRect = new Rect(midPoint.x - 40, midPoint.y - 8, 80, 16);
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
