using UnityEngine;
using UnityEditor;
public class MinMaxAttribute : PropertyAttribute
{
    public float Min { get; private set; }
    public float Max { get; private set; }

    public MinMaxAttribute(float min, float max)
    {
        Min = min;
        Max = max;
    }
}

[CustomPropertyDrawer(typeof(MinMaxAttribute))]
public class MinMaxDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        MinMaxAttribute minMax = (MinMaxAttribute)attribute;

        if (property.propertyType == SerializedPropertyType.Float)
        {
            float newValue = EditorGUI.FloatField(position, label, property.floatValue);
            property.floatValue = Mathf.Clamp(newValue, minMax.Min, minMax.Max);
        }
        else if (property.propertyType == SerializedPropertyType.Integer)
        {
            int newValue = EditorGUI.IntField(position, label, property.intValue);
            property.intValue = Mathf.Clamp(newValue, (int)minMax.Min, (int)minMax.Max);
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Use MinMax with float or int.");
        }
    }
}
