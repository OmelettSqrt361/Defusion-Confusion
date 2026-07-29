using Cinemachine.Editor;
using TMPro;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Task))]
public class TaskEditor : Editor
{
    #region Serialized Propreties
    // Task Type
    SerializedProperty taskTypeProp;

    // Object Interaction
    SerializedProperty player;
    SerializedProperty animator;
    SerializedProperty closestInteractable;

    // Bomb Things
    SerializedProperty isBomb;
    SerializedProperty b;

    // Task Menu Interaction
    SerializedProperty gm;
    SerializedProperty mainCam;
    SerializedProperty taskCam;
    SerializedProperty zoomCam;
    SerializedProperty zoomButtons;
    SerializedProperty taskMenu;
    SerializedProperty isRunning;
    SerializedProperty isZoomed;

    // Tool Usage
    SerializedProperty toolNames;
    SerializedProperty toolsToActivate;

    // Deactivation & Audio
    SerializedProperty noninteractable;
    SerializedProperty audiosS;
    SerializedProperty hasAudio;
    SerializedProperty clip;
    SerializedProperty noZoomingOut;
    #endregion

    bool debugMode;
    bool nullReferences;
    bool hasTools;

    private void OnEnable()
    {
        // Task Type
        taskTypeProp = serializedObject.FindProperty("taskType");

        // Object Interaction
        player = serializedObject.FindProperty("player");
        animator = serializedObject.FindProperty("animator");
        closestInteractable = serializedObject.FindProperty("closestInteractable");

        // Bomb Things
        isBomb = serializedObject.FindProperty("isBomb");
        b = serializedObject.FindProperty("b");

        // Task Menu Interaction
        gm = serializedObject.FindProperty("gm");
        mainCam = serializedObject.FindProperty("mainCam");
        taskCam = serializedObject.FindProperty("taskCam");
        zoomCam = serializedObject.FindProperty("zoomCam");
        zoomButtons = serializedObject.FindProperty("zoomButtons");
        taskMenu = serializedObject.FindProperty("taskMenu");
        isRunning = serializedObject.FindProperty("isRunning");
        isZoomed = serializedObject.FindProperty("isZoomed");

        // Tool Usage
        toolNames = serializedObject.FindProperty("toolNames");
        toolsToActivate = serializedObject.FindProperty("toolsToActivate");

        // Deactivation & Audio
        noninteractable = serializedObject.FindProperty("noninteractable");
        audiosS = serializedObject.FindProperty("audiosS");
        hasAudio = serializedObject.FindProperty("hasAudio");
        clip = serializedObject.FindProperty("clip");
        noZoomingOut = serializedObject.FindProperty("noZoomingOut");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Task _task = (Task)target;

        EditorGUILayout.PropertyField(taskTypeProp);
        debugMode = EditorGUILayout.Toggle("Debug Mode", debugMode);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Task Menu Attributes", EditorStyles.boldLabel);
        if (_task.taskType != Task.taskTypes.door)
        {
            EditorGUILayout.PropertyField(taskMenu);
            if (_task.taskMenu == null)
            {
                EditorGUILayout.HelpBox("No Task Menu found!", MessageType.Warning);
            }
        }
        EditorGUILayout.PropertyField(noninteractable);
        if (debugMode)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(closestInteractable);
                EditorGUILayout.PropertyField(isRunning);
                EditorGUILayout.PropertyField(isZoomed);
                EditorGUILayout.PropertyField(noZoomingOut);

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Camera stuff", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(zoomButtons);
                EditorGUILayout.PropertyField(mainCam);
                EditorGUILayout.PropertyField(taskCam);
                EditorGUILayout.PropertyField(zoomCam);
            }
        }

        if (_task.taskType != Task.taskTypes.door)
        {
            EditorGUILayout.PropertyField(hasAudio, new GUIContent("Has On-Open Audio"));
            if (_task.hasAudio)
            {
                EditorGUILayout.PropertyField(clip);
                if (clip == null)
                {
                    EditorGUILayout.HelpBox("No sound clip provided!", MessageType.Warning);
                }
            }
        }

        if(_task.taskType != Task.taskTypes.text && _task.taskType != Task.taskTypes.box && _task.taskType != Task.taskTypes.door)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);
            hasTools = EditorGUILayout.Toggle("Has Tools?", hasTools);
            if (hasTools)
            {
                DrawToolsTable();
            }
        }

        if (_task.taskType == Task.taskTypes.bomb)
        {
            EditorGUILayout.LabelField("Debug Task Events", EditorStyles.boldLabel);
            if (GUILayout.Button("Defuse Bomb"))
            {
                _task.Defuse();
            }
        }

        nullReferences = EditorGUILayout.Foldout(nullReferences, "Null references", true);
        if (nullReferences)
        {
            if(player == null)
            {
                EditorGUILayout.HelpBox("No player found!", MessageType.Warning);
            }
            if (mainCam == null)
            {
                EditorGUILayout.HelpBox("No mainCam found!", MessageType.Warning);
            }
            if (taskCam == null)
            {
                EditorGUILayout.HelpBox("No taskCam found!", MessageType.Warning);
            }
            if(_task.hasAudio && audiosS == null)
            {
                EditorGUILayout.HelpBox("No Audio Source found, while has audio is enabled!", MessageType.Warning);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawToolsTable()
    {
        EditorGUILayout.LabelField("Tool Configurations", EditorStyles.boldLabel);

        // Keep array sizes synchronized
        if (toolsToActivate.arraySize != toolNames.arraySize)
        {
            toolsToActivate.arraySize = toolNames.arraySize;
        }

        // 1. Draw Table Header
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        EditorGUILayout.LabelField("Attribute", EditorStyles.boldLabel, GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.40f));
        EditorGUILayout.LabelField("GameObject", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("", GUILayout.Width(30)); // Header spacer for minus button
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(2);

        int indexToRemove = -1;

        // 2. Draw Rows Side-by-Side
        for (int i = 0; i < toolNames.arraySize; i++)
        {
            SerializedProperty nameProp = toolNames.GetArrayElementAtIndex(i);
            SerializedProperty objProp = toolsToActivate.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginHorizontal();

            // Attribute column
            EditorGUILayout.PropertyField(nameProp, GUIContent.none, GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.40f));

            // GameObject column
            EditorGUILayout.PropertyField(objProp, GUIContent.none);

            // Minus Button (-) for each row
            if (GUILayout.Button("-", GUILayout.Width(25), GUILayout.Height(18)))
            {
                indexToRemove = i;
            }

            EditorGUILayout.EndHorizontal();
        }

        // 3. Remove item safely if minus button was clicked
        if (indexToRemove >= 0 && indexToRemove < toolNames.arraySize)
        {
            // Safe delete for Object Reference property
            SerializedProperty objProp = toolsToActivate.GetArrayElementAtIndex(indexToRemove);
            if (objProp.objectReferenceValue != null)
            {
                objProp.objectReferenceValue = null; // Clear reference first
            }
            toolsToActivate.DeleteArrayElementAtIndex(indexToRemove);

            // Safe delete for String property
            toolNames.DeleteArrayElementAtIndex(indexToRemove);
        }

        EditorGUILayout.Space(5);

        // 4. Add Button (+) at the bottom
        if (GUILayout.Button("+ Add Tool Attribute", GUILayout.Height(24)))
        {
            toolNames.arraySize++;
            toolsToActivate.arraySize++;
        }
    }
}
