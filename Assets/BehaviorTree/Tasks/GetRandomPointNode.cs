using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gets the vector of a random position within some given range.
/// </summary>
public class GetRandomPointNode : BehaviorNode
{
    /// <summary>
    /// Blackboard key for float array representing min and max values on x, y, z.
    /// </summary>
    private string mMinMaxKey;

    /// <summary>
    /// Blackboard key used to store generated random position.
    /// </summary>
    private string mPointKey;

    /// <summary>
    /// Sets keys for multiple variables at once.
    /// </summary>
    /// <param name="minMaxKey">Key for min max values in blackboard.</param>
    /// <param name="pointKey">Key for vector point in blackboard.</param>
    public void SetKeys(string minMaxKey, string pointKey)
    {
        mMinMaxKey = minMaxKey;
        mPointKey = pointKey;
    }

    /// <summary>
    /// Sets min and max value float array.
    /// </summary>
    /// <param name="minMax"></param>
    public void SetMinMaxVals(float[] minMax)
    {
        mTree.SetBlackboardValue<float[]>(mMinMaxKey, minMax);
    }

    /// <summary>
    /// Gets the float array of min/max values.
    /// </summary>
    /// <returns>Float array containing min/max values.</returns>
    private float[] GetMinMaxValues()
    {
        float[] minMax = mTree.GetBlackboardValue<float[]>(mMinMaxKey);
        return minMax;
    }

    /// <summary>
    /// Sets the value for the vector stored in the blackboard at given key.
    /// </summary>
    /// <param name="point"></param>
    private void SetPointValue(Vector3 point)
    {
        Vector3[] value = { point };
        mTree.SetBlackboardValue(mPointKey, value);
    }

    /// <summary>
    /// Executes the behavior inherent to the owning node.
    /// </summary>
    /// <returns>Returns true if behavior is successfully executed, false if otherwise.</returns>
    public override bool Run()
    {
        float[] minMax = GetMinMaxValues();
        Vector3 point = new Vector3(
            Random.Range(minMax[0], minMax[1]),
            Random.Range(minMax[2], minMax[3]),
            Random.Range(minMax[4], minMax[5]));
        SetPointValue(point);
        return true;
    }
}
