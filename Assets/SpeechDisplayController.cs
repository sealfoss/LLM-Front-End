using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpeechDisplayController : MonoBehaviour
{
    [Tooltip("Check true if you want the text to always face main camera.")]
    [SerializeField] private bool mTowardsCamera = false;

    /// <summary>
    /// Text mesh pro component.
    /// </summary>
    private TextMeshPro mText;

    /// <summary>
    /// User's camera.
    /// </summary>
    private Camera mCam;

    private Character mCharacter;

    /// <summary>
    /// Called when object is enabled.
    /// </summary>
    private void OnEnable()
    {
        mText = this.GetComponent<TextMeshPro>();
        mCam = Camera.main;
        mCharacter = this.GetComponentInParent<Character>();
        mCharacter.onSayToOthers += SetText;
        SetText(null, "");
    }
    
    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    private void Update()
    {
        if (mTowardsCamera)
            OrientTowardsCamera();
    }

    /// <summary>
    /// Orients this object towards the user's camera.
    /// </summary>
    private void OrientTowardsCamera()
    {
        Vector3 dir = (mCam.transform.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(-dir, Vector3.up);
    }

    /// <summary>
    /// Sets the text displayed.
    /// </summary>
    /// <param name="text">
    /// Text to display.
    /// </param>
    public void SetText(Character character, string text)
    {
        if (character != null)
            mText.text = $"{character.CharacterName}:\n{text}";
        else
            mText.text = text;
    }
}
