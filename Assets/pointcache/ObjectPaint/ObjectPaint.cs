namespace pointcache.ObjectPaint {

#if UNITY_EDITOR
    using UnityEngine;
    using UnityEditor;
    using System.Collections.Generic;
    using UnityEditorInternal;

    //made by pointcache
    //start painting = "C"
    //stop = any other key without modifiers
    //or just use toggle button
    //remember rotation samples any last selected object, so just select and then paint and it will work.
    public class ObjectPaint : EditorWindow {

        bool m_paint;
        bool m_prevpaint;
        bool m_randomrotation;
        bool m_randomscale;
        bool m_rememberLastRotation;
        bool m_rememberScale;
        bool m_useMultiple;
        bool m_customMask;
        LayerMask m_mask;
        [SerializeField]
        float m_scaleModifier = 1f;
        [SerializeField]
        float m_minScale = 1f;
        [SerializeField]
        float m_maxScale = 1f;
        GameObject m_prefab;
        Vector3 m_lastRotation;
        Vector3 m_lastScale;
        GameObject m_lastSelected;
        [SerializeField]
        List<GameObject> m_multiple = new List<GameObject>();
        SerializedObject m_serObj;


        [MenuItem("Tools/ObjectPaint")]
        public static void ShowWindow() {
            EditorWindow.GetWindow(typeof(ObjectPaint));
        }

        void OnGUI() {
            GUI.color = Color.red;
            if (GUILayout.Button("Reset All")) {
                m_paint = false;

                m_randomrotation = false;
                m_randomscale = false;
                m_rememberLastRotation = false;
                m_rememberScale = false;
                m_useMultiple = false;
                m_customMask = false;

                m_scaleModifier = 1f;
                m_minScale = 1f;
                m_maxScale = 1f;
                m_prefab = null;


                m_multiple.Clear();

            }
            GUI.color = Color.white;
            GUILayout.Label("ObjectPaint");

            SerializedProperty prop = m_serObj.FindProperty("m_multiple");
            m_serObj.Update();
            EditorGUILayout.BeginHorizontal();
            m_prefab = EditorGUILayout.ObjectField(m_prefab, typeof(GameObject), false) as GameObject;
            if (GUILayout.Button("Get selected")) {
                if (Selection.gameObjects.Length < 2) {

                    if (PrefabUtility.GetPrefabType(Selection.activeGameObject) == UnityEditor.PrefabType.PrefabInstance) {
                        m_prefab = PrefabUtility.GetPrefabParent(Selection.activeGameObject) as GameObject;
                    }
                    else
                    if (PrefabUtility.GetPrefabType(Selection.activeGameObject) == UnityEditor.PrefabType.Prefab) {
                        m_prefab = Selection.activeGameObject;
                    }
                    else {
                        Debug.LogError("Not a prefab, or prefab instance, stop screwing around.");
                    }

                }
                else {
                    m_multiple.Clear();
                    foreach (var go in Selection.gameObjects) {
                        if (PrefabUtility.GetPrefabType(go) == UnityEditor.PrefabType.PrefabInstance) {
                            m_multiple.Add(PrefabUtility.GetPrefabParent(go) as GameObject);
                        }
                        else
                            if (PrefabUtility.GetPrefabType(go) == UnityEditor.PrefabType.Prefab) {
                            m_multiple.Add(go);
                        }
                        else {
                            Debug.LogError("Not a prefab, or prefab instance, stop screwing around.");
                        }
                    }
                }

            }
            EditorGUILayout.EndHorizontal();

            m_useMultiple = EditorGUILayout.BeginToggleGroup("Use Multiple objects", m_useMultiple);

            EditorGUILayout.PropertyField(prop, true);
            EditorGUILayout.EndToggleGroup();

            m_customMask = EditorGUILayout.BeginToggleGroup("Use Custom LayerMask", m_customMask);

            m_mask = LayerMaskField("LayerMask", m_mask);
            EditorGUILayout.EndToggleGroup();

            m_scaleModifier = EditorGUILayout.FloatField("Scale modifier", m_scaleModifier);

            m_randomscale = EditorGUILayout.BeginToggleGroup("Random Scale", m_randomscale);

            EditorGUILayout.BeginHorizontal();
            m_minScale = EditorGUILayout.FloatField("Min scale", m_minScale);
            if (GUILayout.Button("Get")) {
                if (Selection.activeGameObject) {
                    m_minScale = Selection.activeGameObject.transform.localScale.x;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            m_maxScale = EditorGUILayout.FloatField("Max scale", m_maxScale);
            if (GUILayout.Button("Get")) {
                if (Selection.activeGameObject) {
                    m_maxScale = Selection.activeGameObject.transform.localScale.x;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndToggleGroup();


            m_randomrotation = EditorGUILayout.Toggle("Random Rotation", m_randomrotation);


            EditorGUILayout.BeginHorizontal();
            m_rememberLastRotation = EditorGUILayout.Toggle("Remember Rotation", m_rememberLastRotation);
            if (GUILayout.Button("Reset Rotation")) {
                if (Selection.activeGameObject) {
                    if (PrefabUtility.GetPrefabType(Selection.activeGameObject) == UnityEditor.PrefabType.PrefabInstance) {
                        Selection.activeGameObject.transform.rotation = ((GameObject)PrefabUtility.GetPrefabParent(Selection.activeGameObject)).transform.rotation;
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            m_rememberScale = EditorGUILayout.Toggle("Remember Scale", m_rememberScale);
            if (GUILayout.Button("Reset Scale")) {
                if (Selection.activeGameObject) {
                    if (PrefabUtility.GetPrefabType(Selection.activeGameObject) == UnityEditor.PrefabType.PrefabInstance) {
                        Selection.activeGameObject.transform.localScale = ((GameObject)PrefabUtility.GetPrefabParent(Selection.activeGameObject)).transform.localScale;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            m_paint = EditorGUILayout.Toggle("Paint", m_paint);

            m_serObj.ApplyModifiedProperties();
        }

        void OnEnable() {
            m_serObj = new UnityEditor.SerializedObject(this);
            SceneView.onSceneGUIDelegate += Scene;
            Selection.selectionChanged += select;

            m_mask.value = EditorPrefs.GetInt("pointcache_objectpaint_mask");
            m_useMultiple = EditorPrefs.GetBool("pointcache_objectpaint_useMultiple");
            m_randomrotation = EditorPrefs.GetBool("pointcache_objectpaint_randomRotation");
            m_randomscale = EditorPrefs.GetBool("pointcache_objectpaint_randomScale");
            m_rememberLastRotation = EditorPrefs.GetBool("pointcache_objectpaint_rememberLastRotation");
            m_rememberScale = EditorPrefs.GetBool("pointcache_objectpaint_rememberScale");
            m_customMask = EditorPrefs.GetBool("pointcache_objectpaint_useCustomMask");
            m_minScale = EditorPrefs.GetFloat("pointcache_objectpaint_minScale");
            m_maxScale = EditorPrefs.GetFloat("pointcache_objectpaint_maxScale");
        }

        void select() {
            m_lastSelected = Selection.activeGameObject;
        }

        void OnDisable() {
            SceneView.onSceneGUIDelegate -= Scene;
            Selection.selectionChanged -= select;

            EditorPrefs.SetInt("pointcache_objectpaint_mask", m_mask.value);
            EditorPrefs.SetBool("pointcache_objectpaint_useMultiple", m_useMultiple);
            EditorPrefs.SetBool("pointcache_objectpaint_randomRotation", m_randomrotation);
            EditorPrefs.SetBool("pointcache_objectpaint_randomScale", m_randomscale);
            EditorPrefs.SetBool("pointcache_objectpaint_rememberLastRotation", m_rememberLastRotation);
            EditorPrefs.SetBool("pointcache_objectpaint_rememberScale", m_rememberScale);
            EditorPrefs.SetBool("pointcache_objectpaint_useCustomMask", m_customMask);
            EditorPrefs.SetFloat("pointcache_objectpaint_minScale", m_minScale);
            EditorPrefs.SetFloat("pointcache_objectpaint_maxScale", m_maxScale);

        }

        public void Update() {
            // This is necessary to make the framerate normal for the editor window.
            Repaint();

        }

        int getrandommultiple() {
            int random = Random.Range(0, m_multiple.Count);
            if (m_multiple[random] == null)
                return getrandommultiple();
            return random;
        }

        void Scene(SceneView sceneView) {

            string paintingnotification = EditorGUILayout.TextField("Painting");
            string stoppednotification = EditorGUILayout.TextField("Stopped painting");
            if (m_paint && m_paint != m_prevpaint) {
                if (m_lastSelected) {
                    m_lastRotation = m_lastSelected.transform.rotation.eulerAngles;
                    m_lastScale = m_lastSelected.transform.localScale;
                }
                sceneView.ShowNotification(new GUIContent(paintingnotification));
                m_prevpaint = m_paint;
            }
            else if (m_paint != m_prevpaint) {
                sceneView.RemoveNotification();
                sceneView.ShowNotification(new GUIContent(stoppednotification));
                m_prevpaint = m_paint;
            }

            if (Event.current.type == EventType.KeyDown && Event.current.modifiers == EventModifiers.None) {
                if (Event.current.keyCode != KeyCode.None)
                    m_paint = false;
                if (Event.current.keyCode == KeyCode.C)
                    m_paint = true;
            }

            if (!m_paint) {
                sceneView.RemoveNotification();
                return;
            }

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            if (Event.current != null) {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.modifiers == EventModifiers.None) {
                    Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    RaycastHit hit = new RaycastHit();
                    LayerMask temp = m_customMask ? m_mask.value : Physics.AllLayers;
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, temp, QueryTriggerInteraction.Ignore)) {
                        GameObject go = null;
                        if (!m_useMultiple) {
                            if (m_prefab == null)
                                return;
                            go = PrefabUtility.InstantiatePrefab(m_prefab) as GameObject;
                        }
                        else {
                            if (m_multiple == null && m_multiple.Count == 0)
                                return;
                            int random = Random.Range(0, m_multiple.Count);
                            go = PrefabUtility.InstantiatePrefab(m_multiple[random]) as GameObject;
                        }
                        Undo.RegisterCreatedObjectUndo(go, go.name);
                        go.transform.position = hit.point;
                        if (m_randomscale)
                            go.transform.localScale = MultiplyFloat(Vector3.one, Random.Range(m_minScale, m_maxScale));
                        else {
                            if (m_rememberScale)
                                go.transform.localScale = m_lastScale;
                            else
                                go.transform.localScale = MultiplyFloat(go.transform.localScale, m_scaleModifier);

                        }

                        if (m_randomrotation) {
                            Vector3 vec = go.transform.rotation.eulerAngles;
                            vec.y = Random.Range(0f, 360f);
                            go.transform.rotation = Quaternion.Euler(vec);
                        }
                        else if (m_rememberLastRotation) {
                            go.transform.rotation = Quaternion.Euler(m_lastRotation);
                        }
                        m_lastRotation = go.transform.rotation.eulerAngles;
                        m_lastScale = go.transform.localScale;
                        Event.current.Use();
                        Selection.activeGameObject = go;
                        m_lastSelected = go;
                        Debug.Log("Spawned " + go.name);
                    }
                }
            }
        }

        static Vector3 MultiplyFloat(Vector3 vec, float x) {
            vec.x *= x;
            vec.y *= x;
            vec.z *= x;
            return vec;
        }

        static List<int> m_layerNumbers = new List<int>();

        static LayerMask LayerMaskField(string label, LayerMask layerMask) {
            var layers = InternalEditorUtility.layers;

            m_layerNumbers.Clear();

            for (int i = 0; i < layers.Length; i++)
                m_layerNumbers.Add(LayerMask.NameToLayer(layers[i]));

            int maskWithoutEmpty = 0;
            for (int i = 0; i < m_layerNumbers.Count; i++) {
                if (((1 << m_layerNumbers[i]) & layerMask.value) > 0)
                    maskWithoutEmpty |= (1 << i);
            }

            maskWithoutEmpty = UnityEditor.EditorGUILayout.MaskField(label, maskWithoutEmpty, layers);

            int mask = 0;
            for (int i = 0; i < m_layerNumbers.Count; i++) {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << m_layerNumbers[i]);
            }
            layerMask.value = mask;

            return layerMask;
        }
    }

#endif 
}