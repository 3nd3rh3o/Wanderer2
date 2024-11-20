#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;


[CustomEditor(typeof(SolarSystemsData))]
public class SolarSystemsDataEditor : Editor
{
    public VisualTreeAsset m_InspectorXML;
    public override VisualElement CreateInspectorGUI()
    { 
        // Create a new VisualElement to be the root of our Inspector UI.
        VisualElement myInspector = new VisualElement();

        // Load the UXML file.
        m_InspectorXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/SolarSystemsDataEditor.uxml");

        // Instantiate the UXML.
        myInspector = m_InspectorXML.Instantiate();

        // Return the finished Inspector UI.
        return myInspector;
    }
}
#endif