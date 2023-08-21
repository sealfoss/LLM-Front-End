using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rotates agents towards a given target.
/// </summary>
public class FaceTargetNode : BehaviorNode
{
    /// <summary>
    /// Transform of game object to apply rotation to.
    /// </summary>
    private Transform mToRotate;

    /// <summary>
    /// Accessor for game object variable that is rotated by this node.
    /// </summary>
    public Transform ToRotate { get => mToRotate; set => mToRotate = value; }

    /// <summary>
    /// Whether to consider success even though object isn't yet in correct rotation.
    /// </summary>
    private bool mSucceedWhileRotating;

    /// <summary>
    /// Accessors for succeed while rotating switch.
    /// </summary>
    public bool SucceedWhileRotating { get => mSucceedWhileRotating; set => mSucceedWhileRotating = value; }

    /// <summary>
    /// Whether to immediately rotate all the way towards the target, or rotate over time.
    /// </summary>
    private bool mFaceImmediately;

    /// <summary>
    /// Accessor for immediate facing bool.
    /// </summary>
    public bool FaceImmediately { get => mFaceImmediately; set => mFaceImmediately = value; }

    /// <summary>
    /// Blackboard key of target to rotate towards.
    /// </summary>
    private string mTargetKey;

    /// <summary>
    /// Accessors for target key.
    /// </summary>
    public string TargetKey { get => mTargetKey; set => mTargetKey = value; }

    /// <summary>
    /// Target to rotate towards.
    /// </summary>
    private Vector3 mTarget;

    /// <summary>
    /// Accessors of rotation target.
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
    /// Key for rotation speed value.
    /// </summary>
    private string mRotateSpeedKey;

    /// <summary>
    /// Accessors for rotation speed key.
    /// </summary>
    public string RotateSpeedKey { get => mRotateSpeedKey; set => mRotateSpeedKey = value; }

    /// <summary>
    /// Speed of rotation towards target.
    /// </summary>
    private float mRotateSpeed;

    /// <summary>
    /// Acessors for rotation speed.
    /// </summary>
    public float RotateSpeed
    {
        get
        {
            float[] val = mTree.GetBlackboardValue<float[]>(mRotateSpeedKey);
            mRotateSpeed = val[0];
            return mRotateSpeed;
        }
        set
        {
            mRotateSpeed = value;
            float[] val = { mRotateSpeed };
            mTree.SetBlackboardValue(mRotateSpeedKey, val);
        }
    }

    /// <summary>
    /// Key for rotation threshold value in tree blackboard.
    /// </summary>
    private string mRotationThresholdKey;

    /// <summary>
    /// Accessors for rotation threshold key.
    /// </summary>
    public string RotationThresholdKey { get => mRotationThresholdKey; set => mRotationThresholdKey = value; }

    /// <summary>
    /// Angle in degrees between current rotation and target rotation to be considered within tolderance.
    /// </summary>
    private float mRotationThreshold;

    /// <summary>
    /// Accessors for rotation threshold.
    /// </summary>
    public float RotationThreshold
    {
        get
        {
            float[] val = mTree.GetBlackboardValue<float[]>(mRotationThresholdKey);
            mRotationThreshold = val[0];
            return mRotationThreshold;
        }
        set
        {
            mRotationThreshold = value;
            float[] val = { mRotationThreshold };
            mTree.SetBlackboardValue(mRotationThresholdKey, val);
        }
    }

    /// <summary>
    /// Executes the behavior inherent to the owning node.
    /// </summary>
    /// <returns>Returns true if behavior is successfully executed, false if otherwise.</returns>
    public override bool Run()
    {

        Vector3 dir = (Target - mToRotate.transform.position); //.normalized;
        dir.y = 0;
        dir.Normalize();
        bool goodRot = false;

        if (dir != Vector3.zero)
        {
            Quaternion look = Quaternion.LookRotation(dir);

            if (mFaceImmediately)
            {
                mToRotate.transform.rotation = look;
            }
            else
            {
                float rotSpeed = RotateSpeed * Time.deltaTime;
                Quaternion rot = Quaternion.Lerp(this.transform.rotation, look, rotSpeed);
                this.transform.rotation = rot;
                float angle = Quaternion.Angle(rot, look);
                if (angle <= RotationThreshold)
                {
                    mToRotate.rotation = look;
                }
            }

            goodRot = mToRotate.transform.rotation.Equals(look) || mToRotate.transform.rotation == look;
        }
        else
        {
            goodRot = true;
        }

        // Best to check this in both ways because unity 3d.
        return mSucceedWhileRotating || goodRot;
    }
}

