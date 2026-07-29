using Cinemachine.Editor;
using TMPro;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    #region Serialized Propreties
    SerializedProperty sceneType;

    SerializedProperty overlayMenu;
    SerializedProperty playerControler;

    SerializedProperty winConditionCount;
    SerializedProperty winConditions;
    SerializedProperty taskItem;

    // audio
    SerializedProperty audioS;
    SerializedProperty beep;
    SerializedProperty last10secs;
    SerializedProperty boom;
    SerializedProperty winSFX;

    // bomb management
    SerializedProperty maxBombTime;
    SerializedProperty maxVolume;
    SerializedProperty currentBombFactor;
    SerializedProperty bombs;
    SerializedProperty minimalBombTime;

    // ending
    SerializedProperty deathScreen;
    SerializedProperty winScreen;
    SerializedProperty hasEnded;
    SerializedProperty notBegun;
    SerializedProperty won;

    // time measurement
    SerializedProperty timer;
    SerializedProperty timeText;

    // level specifics
    SerializedProperty musicManager;
    SerializedProperty hasNewSong;
    SerializedProperty newSong;
    SerializedProperty musicManagerFallback;
    SerializedProperty songVolume;

    // outline manager
    SerializedProperty som;
    SerializedProperty bombColor;
    SerializedProperty taskColor;
    SerializedProperty itemColor;
    SerializedProperty enableOutlines;
    #endregion

    bool debugMode;
    bool nullReferences;
    bool outlineManager;

    public void OnEnable()
    {
        // General Scene & Control
        sceneType = serializedObject.FindProperty("sceneType");
        overlayMenu = serializedObject.FindProperty("overlayMenu");
        playerControler = serializedObject.FindProperty("playerControler");
        winConditionCount = serializedObject.FindProperty("winConditionCount");
        winConditions = serializedObject.FindProperty("winConditions");
        taskItem = serializedObject.FindProperty("taskItem");

        // Audio
        audioS = serializedObject.FindProperty("audioS");
        beep = serializedObject.FindProperty("beep");
        last10secs = serializedObject.FindProperty("last10secs");
        boom = serializedObject.FindProperty("boom");
        winSFX = serializedObject.FindProperty("winSFX");

        // Bomb Management
        maxBombTime = serializedObject.FindProperty("maxBombTime");
        maxVolume = serializedObject.FindProperty("maxVolume");
        currentBombFactor = serializedObject.FindProperty("currentBombFactor");
        bombs = serializedObject.FindProperty("bombs");
        minimalBombTime = serializedObject.FindProperty("minimalBombTime");

        // Ending
        deathScreen = serializedObject.FindProperty("deathScreen");
        winScreen = serializedObject.FindProperty("winScreen");
        hasEnded = serializedObject.FindProperty("hasEnded");
        notBegun = serializedObject.FindProperty("notBegun");
        won = serializedObject.FindProperty("won");

        // Time Measurement
        timer = serializedObject.FindProperty("timer");
        timeText = serializedObject.FindProperty("timeText");

        // Level Specifics
        musicManager = serializedObject.FindProperty("musicManager");
        hasNewSong = serializedObject.FindProperty("hasNewSong");
        newSong = serializedObject.FindProperty("newSong");
        musicManagerFallback = serializedObject.FindProperty("musicManagerFallback");
        songVolume = serializedObject.FindProperty("songVolume");

        // Outline Manager
        bombColor = serializedObject.FindProperty("bombColor");
        taskColor = serializedObject.FindProperty("taskColor");
        itemColor = serializedObject.FindProperty("itemColor");
        enableOutlines = serializedObject.FindProperty("enableOutlines");
        som = serializedObject.FindProperty("som");
    }

    public override void OnInspectorGUI()
    {

        serializedObject.Update();

        GameManager _gm = (GameManager)target;

        // Draw out sceneType
        EditorGUILayout.PropertyField(sceneType);

        if (_gm.sceneType == GameManager.sceneTypes.Level)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Win Conditions", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(winConditionCount);
            EditorGUILayout.PropertyField(bombs);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Level Audio", EditorStyles.boldLabel);
            EditorGUILayout.Slider(maxVolume, 0f, 1f, new GUIContent("Max Bomb Volume"));

            if (debugMode)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Tasks", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(taskItem);
                    EditorGUILayout.PropertyField(winConditions, new GUIContent("Win Conditions Achieved"));

                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Bombs", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(maxBombTime);
                    EditorGUILayout.PropertyField(currentBombFactor);
                    EditorGUILayout.PropertyField(minimalBombTime);

                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Stage indicators", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(notBegun);
                    EditorGUILayout.PropertyField(hasEnded);
                    EditorGUILayout.PropertyField(won);

                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Other Stats", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(timer);

                }
            }
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(enableOutlines);
            if (_gm.enableOutlines)
            {
                EditorGUILayout.PropertyField(taskColor);
                EditorGUILayout.PropertyField(bombColor);
                EditorGUILayout.PropertyField(itemColor);
            }

        } 
        else if (_gm.sceneType == GameManager.sceneTypes.MainMenu)
        {
            // Nothing here yet...
        }
        else if (_gm.sceneType == GameManager.sceneTypes.Storyboard)
        {
            // Nothing here yet...
        }
        else
        {
            EditorGUILayout.HelpBox("Undefined sceneType for Unity editor", MessageType.Warning);
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Music", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(hasNewSong);
        if (_gm.hasNewSong)
        {
            EditorGUILayout.PropertyField(newSong);
            if (_gm.newSong == null)
            {
                EditorGUILayout.HelpBox("Cannot switch to a null song!", MessageType.Warning);
            }
            EditorGUILayout.Slider(songVolume, 0f, 1f, new GUIContent("Volume of new song"));
        }
        EditorGUILayout.PropertyField(musicManagerFallback);

        EditorGUILayout.Space(5);
        debugMode = EditorGUILayout.Toggle("Debug Mode", debugMode);
        nullReferences = EditorGUILayout.Foldout(nullReferences, "Null references", true);
        if (nullReferences)
        {
            if(_gm.sceneType == GameManager.sceneTypes.Level)
            {
                if (_gm.musicManager == null)
                {
                    EditorGUILayout.HelpBox("No music manager found!", MessageType.Warning);
                }

                if (_gm.playerControler == null)
                {
                    EditorGUILayout.HelpBox("No player found!", MessageType.Warning);
                }

                if (_gm.overlayMenu == null)
                {
                    EditorGUILayout.HelpBox("No overlay menu!", MessageType.Warning);
                }

                if (_gm.deathScreen == null)
                {
                    EditorGUILayout.HelpBox("No death screen!", MessageType.Warning);
                }

                if (_gm.winScreen == null)
                {
                    EditorGUILayout.HelpBox("No win screen!", MessageType.Warning);
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
