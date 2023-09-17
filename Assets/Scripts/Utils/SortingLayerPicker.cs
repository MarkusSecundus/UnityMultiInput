
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Copied from <see href="https://discussions.unity.com/t/editor-script-for-setting-the-sorting-layer-of-an-object/101685/2"/>.
/// <para>
/// All credits go to the original authors (github user <c>nickgravelyn</c>  and unity forum user <c>ossobuko</c>)</para>
/// </summary>
public class SortingLayerAttribute : PropertyAttribute
{
    [CustomPropertyDrawer(typeof(SortingLayerAttribute))]
    public class SortingLayerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sortingLayers = SortingLayer.layers;
            var sortingLayerNames = sortingLayers.Select(l => l.name).ToArray();

            if (property.propertyType != SerializedPropertyType.String && property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.HelpBox(position, $"{property.name} is not a string or int but has [{nameof(SortingLayerAttribute)}].", MessageType.Error);
            }
            else if (sortingLayers == null || sortingLayers.Length == 0)
            {
                EditorGUI.HelpBox(position, "There is no Sorting Layers.", MessageType.Error);
            }
            else if (sortingLayers != null)
            {
                EditorGUI.BeginProperty(position, label, property);

                var oldLayerIndex = property.propertyType switch
                {
                    SerializedPropertyType.String => sortingLayers.FirstIndexed(l => l.name == property.stringValue)?.Index ?? -1,
                    SerializedPropertyType.Integer => sortingLayers.FirstIndexed(l => l.id == property.intValue)?.Index ?? -1,
                    _ => throw new System.ArgumentException()
                };

                // Show the popup for the names
                int newLayerIndex = EditorGUI.Popup(position, label.text, oldLayerIndex, sortingLayerNames);

                // If the index changes, look up the ID for the new index to store as the new ID
                if (newLayerIndex != oldLayerIndex)
                {
                    if(property.propertyType == SerializedPropertyType.String)
                        property.stringValue = sortingLayers[newLayerIndex].name;
                    else if(property.propertyType == SerializedPropertyType.Integer)
                        property.intValue = sortingLayers[newLayerIndex].id;
                }

                EditorGUI.EndProperty();
            }
        }
    }
    
}