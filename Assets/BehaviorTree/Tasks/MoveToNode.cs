using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moves an agent to a position.
/// </summary>
public abstract class MoveTo : BehaviorNode
{
    /// <summary>
    /// Blackboard key for vector representing target position to move towards.
    /// </summary>
    protected string mTargetKey;

    /// <summary>
    /// Accessors for target blackboard key.
    /// </summary>
    public string TargetKey { get => mTargetKey; set => mTargetKey = value; }

    /// <summary>
    /// Vector representing target position to agent should move towards.
    /// </summary>
    protected Vector3 mTarget;

    /// <summary>
    /// Accessors for target position.
    /// </summary>
    public Vector3 Target
    {
        get
        {
            Vector3[] val = mTree.GetBlackboardValue<Vector3[]>(mTargetKey);
            mTarget = val[0];
            return mTarget;
        }
        set
        {
            mTarget = value;
            Vector3[] val = { mTarget };
            mTree.SetBlackboardValue(mTargetKey, val);
        }
    }

    /// <summary>
    /// Blackboard key for float representing movement speed.
    /// </summary>
    protected string mSpeedKey;

    /// <summary>
    /// Accessors for agent speed blackboard key.
    /// </summary>
    public string SpeedKey { get => mSpeedKey; set => mSpeedKey = value; }

    /// <summary>
    /// Agent's speed of movement.
    /// </summary>
    protected float mSpeed;

    /// <summary>
    /// Accessors for movement speed.
    /// </summary>
    public float Speed
    {
        get
        {
            float[] val = mTree.GetBlackboardValue<float[]>(mSpeedKey);
            mSpeed = val[0];
            return mSpeed;
        }
        set
        {
            mSpeed = value;
            float[] val = { mSpeed };
            mTree.SetBlackboardValue(mSpeedKey, val);
        }
    }

    /// <summary>
    /// Blackboard key for float representing how close to the target agent 
    /// needs to be to consider itself as arribed.
    /// </summary>
    protected string mThresholdKey;

    /// <summary>
    /// Accessors for threshold blackboard key.
    /// </summary>
    public string ThresholdKey { get => mThresholdKey; set => mThresholdKey = value; }

    /// <summary>
    /// Distance to target required for agent to consider itself as arrived.
    /// </summary>
    protected float mThreshold;

    /// <summary>
    /// Accessors for distance threshold.
    /// </summary>
    public float Threshold
    {
        get
        {
            float[] val = mTree.GetBlackboardValue<float[]>(mThresholdKey);
            mThreshold = val[0];
            return mThreshold;
        }
        set
        {
            mThreshold = value;
            float[] val = { mThreshold };
            mTree.SetBlackboardValue(mThresholdKey, val);
        }
    }

    /// <summary>
    /// Single method to set all keys at once.
    /// </summary>
    /// <param name="targetKey">Key for target vector.</param>
    /// <param name="speedKey">Key for movement speed.</param>
    /// <param name="thresholdKey">Key for distance threshold.</param>
    public void SetKeys(string targetKey, string speedKey, string thresholdKey)
    {
        TargetKey = targetKey;
        SpeedKey = speedKey;
        ThresholdKey = thresholdKey;
    }

    /// <summary>
    /// Single method to set all values stored at keys in blackboard at once.
    /// </summary>
    /// <param name="target">Position the agent should move towards.</param>
    /// <param name="speed">How quickly the agent should move towards its target.</param>
    /// <param name="threshold">Proximity to target required before movement ends.</param>
    public void SetValues(Vector3 target, float speed, float threshold)
    {
        Speed = speed;
        Target = target;
        Threshold = threshold;
    }

    /// <summary>
    /// Checks if agent is within distance threshold to target position.
    /// </summary>
    /// <returns>Whether agent is within distance threshold to target.</returns>
    protected bool CheckIfWithinThreshold()
    {
        Vector3 current = mTree.gameObject.transform.position;
        Vector3 target = Target;
        float dist = Vector3.Magnitude(current - target);
        float threshold = Threshold;
        bool withinDistance = dist <= threshold;
        return withinDistance;
    }
}

/// <summary>
/// Moves agent towards target via vector linear interpolation.
/// </summary>
public class MoveToLerp : MoveTo
{

    /// <summary>
    /// Executes the behavior inherent to the owning node.
    /// </summary>
    /// <returns>Returns true if behavior is successfully executed, false if otherwise.</returns>
    public override bool Run()
    {
        bool success = CheckIfWithinThreshold();
        if (!success)
        {
            Vector3 target = Target;
            GameObject obj = mTree.gameObject;
            Vector3 current = obj.transform.position;
            float speed = Speed * Time.deltaTime;
            Vector3 newPosition = Vector3.Lerp(current, target, speed);
            obj.transform.position = newPosition;
        }
        return success;
    }
}

/// <summary>
/// Moves agent to target by calculating delta between current positon and target and
/// adding that vector scaled by speed and delta time to current position.
/// </summary>
public class MoveToSimple : MoveTo
{

    /// <summary>
    /// Executes the behavior inherent to the owning node.
    /// </summary>
    /// <returns>Returns true if behavior is successfully executed, false if otherwise.</returns>
    public override bool Run()
    {
        bool success = CheckIfWithinThreshold();
        if (!success)
        {
            Vector3 target = Target;
            GameObject obj = mTree.gameObject;
            Vector3 current = obj.transform.position;
            float speed = Speed * Time.deltaTime;
            Vector3 delta = (target - current).normalized * speed;
            Vector3 newPosition = current + delta;
            obj.transform.position = newPosition;
        }

        return success;
    }
}