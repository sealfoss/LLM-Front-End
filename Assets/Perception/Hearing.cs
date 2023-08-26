using UnityEngine;

/// <summary>
/// Coordinates a hearing "sense" for haracters.
/// </summary>
public class Hearing : MonoBehaviour
{
    // Hearing radius. If outside this radius, thing is not heard.
    public float mRadius = 10.0f;

    // Sphere collider used to detect entrance and exit of hearing radius.
    private SphereCollider mSphere;

    // Character hearing with this component.
    private Personality mOwner;

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        Init();
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
        // Other character, if any.
        Personality character = other.GetComponent<Personality>();

        if (character && character != mOwner)
            character.onSayToOthers += mOwner.HearFromOther;
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
        // Other character, if any.
        Personality character = other.GetComponent<Personality>();

        if (character && character != mOwner)
            character.onSayToOthers -= mOwner.HearFromOther;
    }

    /// <summary>
    /// Initializes hearing.
    /// </summary>
    public void Init()
    {
        mOwner = GetComponentInParent<Personality>();
        mSphere = GetComponent<SphereCollider>();
        mSphere.radius = mRadius;
        CheckEarshot();
    }


    /// <summary>
    /// Checks if any characters are within earshot.
    /// </summary>
    private void CheckEarshot()
    {
        // Colliders within hearing radius.
        Collider[] colliders = Physics.OverlapSphere(transform.position, mRadius);

        // Found Character owner of a given collider.
        Personality character;

        foreach(Collider collider in colliders)
        {
            character = collider.GetComponent<Personality>();
            if (character && character != mOwner)
                character.onSayToOthers += mOwner.HearFromOther;
        }
    }
}
