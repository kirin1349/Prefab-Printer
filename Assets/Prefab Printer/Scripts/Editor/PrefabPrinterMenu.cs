using UnityEditor;

public class PrefabPrinterMenu
{
    [MenuItem("Tools/Prefab Printer")]
    static void OpenPrefabPrinter()
    {
        PrefabPrinterWindow window = PrefabPrinterWindow.GetWindow<PrefabPrinterWindow>("Prefab Printer", true);
        window.ShowTab();
    }
}
