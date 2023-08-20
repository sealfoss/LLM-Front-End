using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Detects describable objects able to be seen by this component.
/// </summary>
public class Vision : MonoBehaviour
{
    /// <summary>
    /// Type of vision volume to represent field of view.
    /// </summary>
    private enum VisionVolumeType
    {
        SIMPLE,
        ELIPSED
    }

    /// <summary>
    /// Layer index for visible objects; things this component can see.
    /// </summary>
    [SerializeField] private int mVisibleLayerIndex = 7;

    /// <summary>
    /// How far away things can be seen byt his vision component.
    /// </summary>
    [SerializeField] private float mNearClip = 0.1f;

    /// <summary>
    /// How close things can be seen by this vision component.
    /// </summary>
    [SerializeField] private float mFarClip = 20.0f;

    /// <summary>
    /// Vision horizontal field of view for this character.
    /// Bounded between [1-179] to prevent undefined behavior.
    /// </summary>
    [MinMax(1, 179)]
    [SerializeField] private int mFovHorizontal = 120;

    /// <summary>
    /// Vision vertical field of view for this character.
    /// Bounded between [1-179] to prevent undefined behavior.
    /// </summary>
    [MinMax(1, 179)]
    [SerializeField] private int mFovVertical = 120;

    /// <summary>
    /// Type of vision volume to use, simple or eilipsed.
    /// </summary>
    [SerializeField] private VisionVolumeType mVolumeType = 
        VisionVolumeType.SIMPLE;

    /// <summary>
    /// How many segments in the elipsed vision volume, if used.
    /// </summary>
    [SerializeField] private int mElipseSegments = 8;

    /// <summary>
    /// Layer mask used to discern visible objects from things that should be 
    /// ignored.
    /// </summary>
    private int mVisibleLayerMask;

    /// <summary>
    /// Horizontal field of view, in radians.
    /// </summary>
    private float mAlpha;

    /// <summary>
    /// Half the horizontal fov in radians.
    /// </summary>
    private float mBeta;

    /// <summary>
    /// Cosine of half the horizontal fov.
    /// </summary>
    private float mCosBeta;

    /// <summary>
    /// Sin of half the horizontal fov.
    /// </summary>
    private float mSinBeta;

    /// <summary>
    /// Vertical field of view, in radians.
    /// </summary>
    private float mPsi;

    /// <summary>
    /// Half the vertical fov in radians.
    /// </summary>
    private float mOmega;

    /// <summary>
    /// Cosine of half the vertical fov.
    /// </summary>
    private float mCosOmega;

    /// <summary>
    /// Sin of half the vertical fov.
    /// </summary>
    private float mSinOmega;

    /// <summary>
    /// Mesh used to detect objects entering and exiting vision field of view.
    /// </summary>
    private Mesh mVisionMesh;

    /// <summary>
    /// Index vector used to build triangles for vision mesh.
    /// </summary>
    private static readonly int[] mTriangles = 
    {
        0, 3, 1, 2, 1, 3,
        4, 5, 6, 6, 7, 4,
        0, 1, 5, 5, 4, 0,
        3, 6, 2, 7, 6, 3,
        0, 7, 3, 4, 7, 0,
        6, 5, 1, 1, 2, 6
    };

    /// <summary>
    /// Collider object built from mesh.
    /// </summary>
    private MeshCollider mCollider;

    /// <summary>
    /// Set of objects within field of view. As in, can be seen by this object.
    /// </summary>
    HashSet<GameObject> mWithinFov = new HashSet<GameObject>();

    /// <summary>
    /// Options for collision mesh used to define field of view.
    /// </summary>
    private const MeshColliderCookingOptions mCookingOptions =
        MeshColliderCookingOptions.UseFastMidphase & 
        MeshColliderCookingOptions.CookForFasterSimulation;

    /// <summary>
    /// Character owning this vision component.
    /// </summary>
    public Character mOwner;

    /// <summary>
    /// Describable objects visible to this component.
    /// </summary>
    private IDescribable[] mSeen;

    /// <summary>
    /// Accessor for mSeen.
    /// </summary>
    public IDescribable[] Seen { get => mSeen; }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        mOwner = GetComponentInParent<Character>();
        mVisibleLayerMask = 1 << mVisibleLayerIndex;
    }

    private void Start()
    {
        SetFov(mFovHorizontal);
    }

    /// <summary>
    /// When a GameObject collides with another GameObject, Unity calls 
    /// OnTriggerEnter.
    /// </summary>
    /// <param name="other">
    /// The other collider.
    /// </param>
    private void OnTriggerEnter(Collider other)
    {
        // Detected describable object entering the field of view.
        IDescribable describable = other.GetComponent<IDescribable>();

        if (describable != null)
        {
            mWithinFov.Add(other.gameObject);
            CheckSeen();
        }
    }

    /// <summary>
    /// OnTriggerExit is called when the Collider other has stopped touching 
    /// the trigger.
    /// </summary>
    /// <param name="other">
    /// The other collider.
    /// </param>
    private void OnTriggerExit(Collider other)
    {
        // Detected describable object exiting the field of view.
        IDescribable describable = other.GetComponent<IDescribable>();

        if (describable != null)
        {
            mWithinFov.Remove(other.gameObject);
            CheckSeen();
        }
    }

    /// <summary>
    /// Sets a new field of view, builds a new fov mesh, checks for new 
    /// detections.
    /// </summary>
    /// <param name="newFovDegrees">
    /// New field of view value in degrees.
    /// </param>
    public void SetFov(int newFovDegrees)
    {
        mFovHorizontal = newFovDegrees;
        GenerateTrigParams();
        mCollider = GetComponent<MeshCollider>();
        mVisionMesh = GenerateVisionVolume();
        Physics.BakeMesh(mVisionMesh.GetInstanceID(), false, mCookingOptions);
        mCollider.sharedMesh = mVisionMesh;
        mWithinFov.Clear();
        CheckFieldOfView();
    }

    /// <summary>
    /// Generates the appropriate vision volume mesh based on preference.
    /// </summary>
    /// <returns>
    /// Vision volume mesh.
    /// </returns>
    private Mesh GenerateVisionVolume()
    {
        // The generated vision volume to use.
        Mesh volume = null;

        switch (mVolumeType)
        {
            case VisionVolumeType.SIMPLE:
                volume = GenerateVisionVolumeSimple();
                break;
            case VisionVolumeType.ELIPSED:
                volume = GenerateVisionVolumeElipsed();
                break;
        }
        return volume;
    }

    /// <summary>
    /// Generates parametes used in trigometric operations.
    /// </summary>
    private void GenerateTrigParams()
    {
        mAlpha = ((float) mFovHorizontal) * Mathf.Deg2Rad;
        mBeta = mAlpha * 0.5f;
        mCosBeta = Mathf.Cos(mBeta);
        mSinBeta = Mathf.Sin(mBeta);
        mPsi = ((float)mFovVertical) * Mathf.Deg2Rad;
        mOmega = mPsi * 0.5f;
        mCosOmega = Mathf.Cos(mOmega);
        mSinOmega = Mathf.Sin(mOmega);
    }

    /// <summary>
    /// Generates mesh representing the volume of space things are visible within.
    /// </summary>
    /// <returns></returns>
    private Mesh GenerateVisionVolumeElipsed()
    {
        // The generated mesh.
        Mesh mesh = new Mesh();

        // Step size for segment angles.
        float angleStep = mAlpha / (float)mElipseSegments;

        // Relative x coordinate of mesh near clip plane. 
        float xNear = mSinBeta * mNearClip / mCosBeta;

        // Relative y coordinate of mesh near clip plane.
        float yNear = mSinOmega * mNearClip / mCosOmega;

        float yFar = mFarClip * mSinOmega;

        // Mesh is built in segments, gamma is angle of current segment.
        float gamma;

        // Sin of angle of current mesh segment.
        float sinGamma;

        // Cos of angle of current mesh segment.
        float cosGamma;

        // Far vertex x coordinate.
        float xFar;
        
        // Far vertex y coordinate.
        float zFar;

        // List of mesh triangles' vertex indicies.
        List<int> tris = new List<int>();

        // List of verticies making up mesh.
        List<Vector3> verts = new List<Vector3>();

        // This is very ugly and relies on a lot of magic numbers.
        // I apologize, but it was taking forever to get it to work and 
        // in the end I said screw it.
        verts.Add(new Vector3(xNear, yNear, mNearClip));
        verts.Add(new Vector3(-xNear, yNear, mNearClip));
        verts.Add(new Vector3(-xNear, -yNear, mNearClip));
        verts.Add(new Vector3(xNear, -yNear, mNearClip));
        tris.Add(0);
        tris.Add(3);
        tris.Add(1);
        tris.Add(1);
        tris.Add(3);
        tris.Add(2);
        int i;
        for(i = 0; i <= mElipseSegments; i++)
        {
            gamma = -mBeta + (i * angleStep);
            sinGamma = Mathf.Sin(gamma);
            cosGamma = Mathf.Cos(gamma);
            xFar = mFarClip * sinGamma;
            zFar = mFarClip * cosGamma;
            verts.Add(new Vector3(xFar, yFar, zFar));
            verts.Add(new Vector3(xFar, -yFar, zFar));
            if(i > 0)
            {
                if(i < mElipseSegments - 1)
                {
                    if(i <= mElipseSegments / 2)
                    {
                        tris.Add(verts.Count - 1);
                        tris.Add(0);
                        tris.Add(verts.Count - 3);
                        tris.Add(verts.Count - 2);
                        tris.Add(3);
                        tris.Add(verts.Count - 4);
                        tris.Add(verts.Count - 1);
                        tris.Add(verts.Count - 3);
                        tris.Add(verts.Count - 4);
                        tris.Add(verts.Count - 1);
                        tris.Add(verts.Count - 4);
                        tris.Add(verts.Count - 2);
                    }
                    else
                    {
                        tris.Add(verts.Count - 1);
                        tris.Add(1);
                        tris.Add(verts.Count - 3);
                        tris.Add(verts.Count - 2);
                        tris.Add(2);
                        tris.Add(verts.Count - 4);
                    }
                }
                else
                {
                    tris.Add(verts.Count - 2);
                    tris.Add(1);
                    tris.Add(2);
                    tris.Add(verts.Count - 1);
                    tris.Add(verts.Count - 2);
                    tris.Add(2);
                }
            }
            else
            {
                tris.Add(0);
                tris.Add(4);
                tris.Add(5);
                tris.Add(3);
                tris.Add(0);
                tris.Add(5);
            }
        }
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        return mesh;
    }    

    /// <summary>
    /// Generates a collider mesh to detect entrance and exit of field of view
    /// by other objects. Simple box.
    /// </summary>
    /// <returns>
    /// Gemerated mesh.
    /// </returns>
    private Mesh GenerateVisionVolumeSimple()
    {
        // Mesh to generate.
        Mesh mesh;

        // Relative x coordinate of mesh far clip plane.
        float xNear = mSinBeta * mNearClip / mCosBeta;

        // Relative x coordinate of mesh near clip plane. 
        float xFar = mSinBeta * mFarClip / mCosBeta;

        // Relative y coordinate of mesh far clip plane.
        float yNear = mSinOmega * mNearClip / mCosOmega;

        // Relative y coordinate of mesh far clip plane.
        float yFar = mSinOmega * mFarClip / mCosOmega;

        // Mesh vertex array.
        Vector3[] verts = new Vector3[8];

        verts[0] = new Vector3(xNear, yNear, mNearClip);
        verts[1] = new Vector3(-xNear, yNear, mNearClip);
        verts[2] = new Vector3(-xNear, -yNear, mNearClip);
        verts[3] = new Vector3(xNear, -yNear, mNearClip);
        verts[4] = new Vector3(xFar, yFar, mFarClip);
        verts[5] = new Vector3(-xFar, yFar, mFarClip);
        verts[6] = new Vector3(-xFar, -yFar, mFarClip);
        verts[7] = new Vector3(xFar, -yFar, mFarClip);
        mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = mTriangles;
        return mesh;
    }

    /// <summary>
    /// Deducts whether some other object can be seen by this object.
    /// </summary>
    /// <param name="other">
    /// Other object to check visibility of.
    /// </param>
    /// <returns>
    /// Whether other object is able to be seen by this vision component.
    /// </returns>
    public bool WithinFieldOfView(GameObject other)
    {
        // Whether the other object can be seen by this vision component.
        bool withinFov = false;

        // Vector delta between other object and this vision component.
        Vector3 delta = other.transform.position - transform.position;

        // Direction towards other object.
        Vector3 dir = delta.normalized;

        // Distance between the vision component and the other object.
        float dist = delta.magnitude;

        // Projection from direction towards other object to horizontal plane.
        Vector3 p;

        // Projection from direciton towards other object to vertical plane.
        Vector3 q;

        // Cosine of angle between p and forward.
        float cosP;

        // Cosine of angle between q and forward.
        float cosQ;

        if (dist <= mFarClip && dist >= mNearClip)
        {
            p = Vector3.ProjectOnPlane(dir, transform.up).normalized;
            q = Vector3.ProjectOnPlane(dir, transform.right).normalized;
            cosP = Vector3.Dot(p, transform.forward);
            cosQ = Vector3.Dot(q, transform.forward);
            withinFov = cosP >= mCosBeta && cosQ >= mCosOmega;
        }
        return withinFov;
    }

    /// <summary>
    /// Checks whether a given object is obscured by another from the point of 
    /// view of this vision component.
    /// </summary>
    /// <param name="other">
    /// Other object.
    /// </param>
    /// <returns>
    /// Whether the other object can be seen.
    /// </returns>
    public bool IsObscured(GameObject other)
    {
        // Vector delta between other object and this vision component.
        Vector3 delta = other.transform.position - transform.position;

        // Hit object in direction of other object according to raycast.
        RaycastHit hit;

        Physics.Raycast(
            transform.position, 
            delta.normalized, 
            out hit, 
            delta.magnitude,
            mVisibleLayerMask
        );
        return other != hit.collider.gameObject;
    }

    /// <summary>
    /// Checks which objects in field of view are visible to this vision 
    /// component.
    /// </summary>
    private void CheckSeen()
    {
        // List of seen describable objects.
        List<IDescribable> seen = new List<IDescribable>();

        // Seen describable object.
        IDescribable describable;

        foreach (GameObject obj in mWithinFov)
        {
            describable = obj.GetComponent<IDescribable>();
            if (describable != null && !IsObscured(obj))
                seen.Add(describable);
        }
        mSeen = seen.Count > 0 ? seen.ToArray() : new IDescribable[0];
        mOwner.AssessSurroundings();
    }

    /// <summary>
    /// Checks the defined field of view for any GameObjects.
    /// Does not check whether they are obscured.
    /// </summary>
    private void CheckFieldOfView()
    {
        // Colliders close enough to this vision component to be seen.
        Collider[] closeEnough = 
            Physics.OverlapSphere(transform.position, mFarClip);

        // Game object to check.
        GameObject obj;

        foreach (Collider collider in closeEnough)
        {
            obj = collider.gameObject;
            if (obj != mOwner.gameObject && obj.layer == mVisibleLayerIndex 
                && WithinFieldOfView(obj))
                    mWithinFov.Add(obj);
        }
        CheckSeen();
    }
}
