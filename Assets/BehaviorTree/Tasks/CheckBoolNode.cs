using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Returns the value of a bool variable stored in owning tree's blackboard.
/// </summary>
public class CheckBoolNode : BehaviorNode
{
    /// <summary>
    /// Key of bool variable to track.
    /// </summary>
    private string mKey;
    
    /// <summary>
    /// Accessors for key.
    /// </summary>
    public string Key { get => mKey; set => mKey = value; }

    /// <summary>
    /// Boolean representing true/false value of this node.
    /// </summary>
    private bool mVal;

    /// <summary>
    /// Accessors for boolean value, stores in tree blackboard.
    /// </summary>
    public bool Val
    {
        get
        {
            bool[] val = mTree.GetBlackboardValue<bool[]>(mKey);
            mVal = val[0];
            return mVal;
        }
        set
        {
            mVal = value;
            bool[] val = { mVal };
            mTree.SetBlackboardValue(mKey, val);
        }
    }

    /// <summary>
    /// Executes the behavior inherent to the owning node.
    /// </summary>
    /// <returns>Returns true if behavior is successfully executed, false if otherwise.</returns>
    public override bool Run()
    {
        return Val;
    }
}
