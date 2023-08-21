using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Has an agent wait for a specified period of time before continuing on to new behavior.
/// </summary>
public class WaitNode : BehaviorNode
{
    /// <summary>
    /// Time elapsed since wait was called.
    /// </summary>
    private float mElapsed;

    /// <summary>
    /// Key used to store wait time in blackboard.
    /// </summary>
    private string mWaitTimeKey;

    /// <summary>
    /// Accessors for wait time key.
    /// </summary>
    public string WaitTimeKey { get => mWaitTimeKey; set => mWaitTimeKey = value; }

    /// <summary>
    /// How long the agent should wait for.
    /// </summary>
    private float mWaitTime;

    /// <summary>
    /// Accessors for wait time.
    /// </summary>
    public float WaitTime
    {
        get
        {
            float[] val = mTree.GetBlackboardValue<float[]>(mWaitTimeKey);
            mWaitTime = val[0];
            return mWaitTime;
        }
        set
        {
            mWaitTime = value;
            float[] val = { mWaitTime };
            mTree.SetBlackboardValue<float[]>(mWaitTimeKey, val);
        }
    }

    /// <summary>
    /// Executes the behavior inherent to the owning node.
    /// </summary>
    /// <returns>Returns true if behavior is successfully executed, false if otherwise.</returns>
    public override bool Run()
    {
        bool success = false;
        mElapsed += Time.deltaTime;

        if(mElapsed >= WaitTime)
        {
            mElapsed = 0;
            success = true;
        }

        return success;
    }
}
