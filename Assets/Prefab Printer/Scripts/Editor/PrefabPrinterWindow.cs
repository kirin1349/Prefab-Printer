using UnityEditor;
using UnityEngine;

public class PrefabPrinterWindow : EditorWindow
{
    private bool m_inited = false;

    private PrefabPrinter m_printer = null;

    private Transform m_root = null;
    private Camera m_camera = null;
    private Vector2Int m_canvasSize = Vector2Int.zero;
    private Vector2Int m_printSize = Vector2Int.zero;
    private int m_frameTotal = 0;
    private float m_duration = 0f;
    private bool m_useSimulate = true;
    private bool m_ignoreZero = true;
    private bool m_additiveMode = true;
    private string m_outputFolder = string.Empty;
    private string m_outputNameFormat = string.Empty;
    private PrefabPrinterTextureTypes m_outputTextureType = PrefabPrinterTextureTypes.PNG;
    private TextureFormat m_outputTextureFormat = TextureFormat.ARGB32;
    private bool m_outputCreateFolder = false;

    private string m_outputPath = string.Empty;

    private string m_message = string.Empty;
    private MessageType m_messageType = MessageType.None;

    void OnEnable()
    {
        if (!m_inited)
        {
            m_inited = true;
            m_root = GameObject.Find("Print Root")?.transform;
            m_camera = m_root?.GetComponentInChildren<Camera>();
            if (m_camera == null) m_camera = Camera.main;
            m_canvasSize = new Vector2Int(750, 1334);
            m_printSize = new Vector2Int(100, 100);
            m_frameTotal = 10;
            m_useSimulate = true;
            m_ignoreZero = true;
            m_additiveMode = true;
            m_outputFolder = "Output";
            m_outputNameFormat = "{0}_{1}";
            m_outputCreateFolder = true;
        }
    }

    void OnGUI()
    {
        m_message = string.Empty;
        m_messageType = MessageType.None;

        m_root = EditorGUILayout.ObjectField(new GUIContent("Root"), m_root, typeof(Transform), true) as Transform;
        m_camera = EditorGUILayout.ObjectField(new GUIContent("Camera"), m_camera, typeof(Transform), true) as Camera;
        m_canvasSize = EditorGUILayout.Vector2IntField(new GUIContent("Canvas Size"), m_canvasSize);
        m_printSize = EditorGUILayout.Vector2IntField(new GUIContent("Print Size"), m_printSize);
        m_frameTotal = EditorGUILayout.IntField(new GUIContent("Frame Total"), m_frameTotal);
        m_duration = EditorGUILayout.FloatField(new GUIContent("Duration (Optional)"), m_duration);
        m_useSimulate = EditorGUILayout.Toggle(new GUIContent("Simulate (Particle Only)"), m_useSimulate);
        m_ignoreZero = EditorGUILayout.Toggle(new GUIContent("Ignore 0s"), m_ignoreZero);
        m_additiveMode = EditorGUILayout.Toggle(new GUIContent("Additive Mode"), m_additiveMode);
        m_outputFolder = EditorGUILayout.TextField(new GUIContent("Output Folder"), m_outputFolder);
        m_outputNameFormat = EditorGUILayout.TextField(new GUIContent("Output Name Format"), m_outputNameFormat);
        m_outputTextureType = (PrefabPrinterTextureTypes)EditorGUILayout.EnumPopup(new GUIContent("Output Texture Type"), m_outputTextureType);
        m_outputTextureFormat = (TextureFormat)EditorGUILayout.EnumPopup(new GUIContent("Output Texture Format"), m_outputTextureFormat);
        m_outputCreateFolder = EditorGUILayout.Toggle(new GUIContent("Create Folder"), m_outputCreateFolder);

        m_outputPath = System.IO.Path.Combine(Application.dataPath, m_outputFolder);

        GUILayout.Space(10f);
        GUILayout.Box(new GUIContent(string.Format("Output Path: {0}", m_outputPath)), GUILayout.ExpandWidth(true));
        GUILayout.Space(10f);

        if (GUILayout.Button("Print", GUILayout.Height(45f)))
        {
            onPrint();
        }

        if (!string.IsNullOrEmpty(m_message))
        {
            EditorGUILayout.HelpBox(m_message, m_messageType);
        }
    }

    void Update()
    {
        if (m_printer != null)
        {
            m_printer.update(Time.deltaTime);
        }
    }

    protected void onPrint()
    {
        if (m_root == null)
        {
            m_message = "missing root node.";
            m_messageType = MessageType.Error;
            return;
        }
        if (m_camera == null)
        {
            m_message = "missing camera.";
            m_messageType = MessageType.Error;
            return;
        }

        if (m_root.childCount <= 0)
        {
            m_message = "no object.";
            m_messageType = MessageType.Error;
            return;
        }

        if (m_printer == null)
        {
            m_printer = new PrefabPrinter();
        }
        m_printer.setCamera(m_camera);
        m_printer.setCanvasSize(m_canvasSize);
        m_printer.setPrintSize(m_printSize);
        m_printer.setFrameTotal(m_frameTotal);
        m_printer.setDuration(m_duration);
        m_printer.setUseSimulate(m_useSimulate);
        m_printer.setIgnoreZero(m_ignoreZero);
        m_printer.setAdditiveMode(m_additiveMode);
        m_printer.setOutputFolder(m_outputPath);
        m_printer.setOutputNameFormat(m_outputNameFormat);
        m_printer.setOutputTextureType(m_outputTextureType);
        m_printer.setOutputTextureFormat(m_outputTextureFormat);
        m_printer.setOutputCreateFolder(m_outputCreateFolder);
        int count = m_root.childCount;
        for (int i = 0; i < count; i++)
        {
            m_printer.addObject(m_root.GetChild(i).gameObject);
        }
        m_printer.start();
    }

    void OnDestroy()
    {
        if (m_printer != null)
        {
            m_printer.dispose();
            m_printer = null;
        }
    }
}
