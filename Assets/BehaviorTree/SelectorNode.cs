using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Executes child behaviors one at a time, stops at the first to succeed.
/// Returns true should any executed child behaviors returns true.
/// </summary>
public class SelectorNode : BehaviorNode
{

    /// <summary>
    /// Executes the behavior inherent to the owning node.
    /// </summary>
    /// <returns>Returns true if behavior is successfully executed, false if otherwise.</returns>
    public override bool Run()
    {
        bool success = false;

        foreach(BehaviorNode child in base.mChildren)
        {
            if (child.Run())
            {
                success = true;
                break;
            }
        }

        return success;
    }
}
