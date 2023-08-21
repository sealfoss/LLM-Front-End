using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Executes child node logic in sequence, stops at the first child that returns false.
/// Returns true if all children execute and return true.
/// </summary>
public class SequenceNode : BehaviorNode
{

    /// <summary>
    /// Executes the behavior inherent to the owning node.
    /// </summary>
    /// <returns>Returns true if behavior is successfully executed, false if otherwise.</returns>
    public override bool Run()
    {
        bool success = true;

        foreach (BehaviorNode child in base.mChildren)
        {
            if (!child.Run())
            {
                success = false;
                break;
            }
        }

        return success;
    }
}
