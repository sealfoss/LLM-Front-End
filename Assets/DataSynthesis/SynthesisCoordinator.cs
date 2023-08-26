using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Coordinates data synthesis via conversation between two agents generating
/// responses from GPT.
/// </summary>
public class SynthesisCoordinator : MonoBehaviour
{
    [Tooltip("Whether to run data synthesis on Start().")]
    [SerializeField] private bool mRunSynthOnStart = false;

    [Tooltip("How many dialogue samples to run per aspect value level.")]
    [SerializeField] private int mSamplesPerAspect = 100;

    [Tooltip("Personality aspects to gather data on.")]
    [SerializeField] private string[] mValueKeys =
    {
        Defines.OPEN_NAME,
        Defines.CONSC_NAME,
        Defines.EXT_NAME,
        Defines.AGREE_NAME,
        Defines.NEURO_NAME,
        Defines.HAPPY_NAME,
        Defines.ANGER_NAME,
        Defines.SARC_NAME
    };

    [Tooltip("Values to use while synthesizing data on Openness.")]
    [SerializeField] private int[] mOpennessValues = { 0, 1, 3, 4 };

    [Tooltip("Values to use while synthesizing data on Conscientiousness.")]
    [SerializeField] private int[] mConscientiousnessValues = { 0, 1, 3, 4 };

    [Tooltip("Values to use while synthesizing data on Extraversion.")]
    [SerializeField] private int[] mExtraversionValues = { 0, 1, 3, 4 };

    [Tooltip("Values to use while synthesizing data on Agreeableness.")]
    [SerializeField] private int[] mAgreeablenessValues = { 0, 1, 3, 4 };

    [Tooltip("Values to use while synthesizing data on Neuroticsm.")]
    [SerializeField] private int[] mNeuroticsmValues = { 0, 1, 3, 4 };

    [Tooltip("Values to use while synthesizing data on Happiness.")]
    [SerializeField] private int[] mHappinessValues = { 0, 1, 3, 4 };

    [Tooltip("Values to use while synthesizing data on Anger.")]
    [SerializeField] private int[] mAngerValues = { 0, 1, 3, 4 };

    [Tooltip("Values to use while synthesizing data on Sarcasm.")]
    [SerializeField] private int[] mSarcasmValues = { 0, 1, 3, 4 };

    [Tooltip("Prefab instantiated to generate text respones.")]
    [SerializeField] private GameObject mCharacterPrefab;

    [Tooltip("Spawn location for character A.")]
    [SerializeField] private Transform mSpawnA;

    [Tooltip("Spawn locationf or character B.")]
    [SerializeField] private Transform mSpawnB;

    /// <summary>
    /// List of potential jobs for the personality to be described as having.
    /// </summary>
    private string[] mJobs;

    /// <summary>
    /// List of potential names for personalities to take.
    /// </summary>
    private string[] mNames;

    /// <summary>
    /// List of potential tasks for personalities to be occupied by.
    /// </summary>
    private string[] mTasks;

    /// <summary>
    /// List of potential topics for personalities to bring up in conversation.
    /// </summary>
    private string[] mTopics;

    /// <summary>
    /// List of potential places personalities can be described as being from.
    /// </summary>
    private string[] mPlaces;

    /// <summary>
    /// One of the character taking part in the synthesis conversation.
    /// </summary>
    private GameObject mCharacterA;

    /// <summary>
    /// The other character taking part in the synthesis conversation.
    /// </summary>
    private GameObject mCharacterB;

    /// <summary>
    /// Personality component of character A.
    /// </summary>
    private Personality mPersonalityA;

    /// <summary>
    /// Personality component of character B.
    /// </summary>
    private Personality mPersonalityB;

    /// <summary>
    /// Personality aspect values to iterate over during synthesis, by name.
    /// </summary>
    private Dictionary<string, int[]> mSynthValues;

    /// <summary>
    /// Current aspect name key in synth values being used in synthesis on
    /// character A.
    /// </summary>
    private int mCurrentKeyIndexA;

    /// <summary>
    /// Index of current value being used with current aspect used in data
    /// synthesis on character A.
    /// </summary>
    private int mCurrentValueIndexA;

    /// <summary>
    /// Current aspect name key in synth values being used in synthesis on
    /// character B.
    /// </summary>
    private int mCurrentKeyIndexB;

    /// <summary>
    /// Index of current value being used with current aspect used in data
    /// synthesis on character A.
    /// </summary>
    private int mCurrentValueIndexB;

    /// <summary>
    /// Compnent used to illicit responses from GPT.
    /// </summary>
    private GptCommunicator mCommunicator;

    /// <summary>
    /// Number of samples collected using character A in current configuration.
    /// </summary>
    private int mSamplesA;

    /// <summary>
    /// Number of samples collected using character B in current configuration.
    /// </summary>
    private int mSamplesB;

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        mCommunicator = FindFirstObjectByType<GptCommunicator>();
        mCommunicator.OnRecord += ConversationRecord;
        mCommunicator.OnDenial += ConversationDenial;
        mSynthValues = new Dictionary<string, int[]>();
        mSynthValues[Defines.OPEN_NAME] = mOpennessValues;
        mSynthValues[Defines.CONSC_NAME] = mConscientiousnessValues;
        mSynthValues[Defines.EXT_NAME] = mExtraversionValues;
        mSynthValues[Defines.AGREE_NAME] = mAgreeablenessValues;
        mSynthValues[Defines.NEURO_NAME] = mNeuroticsmValues;
        mSynthValues[Defines.HAPPY_NAME] = mHappinessValues;
        mSynthValues[Defines.ANGER_NAME] = mAngerValues;
        mSynthValues[Defines.SARC_NAME] = mSarcasmValues;
        mJobs = File.ReadAllLines($"{Defines.SYNTH_DIR}{Defines.SYNTH_JOBS}");
        mNames = 
            File.ReadAllLines($"{Defines.SYNTH_DIR}{Defines.SYNTH_NAMES}");
        mTasks = 
            File.ReadAllLines($"{Defines.SYNTH_DIR}{Defines.SYNTH_TASKS}");
        mTopics = 
            File.ReadAllLines($"{Defines.SYNTH_DIR}{Defines.SYNTH_TOPICS}");
        mPlaces =
            File.ReadAllLines($"{Defines.SYNTH_DIR}{Defines.SYNTH_PLACES}");
    }

    /// <summary>
    /// This function is called when the object becomes disabled and deactived.
    /// </summary>
    private void OnDisable()
    {
        mCommunicator.OnRecord -= ConversationRecord;
        mCommunicator.OnDenial -= ConversationDenial;
    }

    /// <summary>
    /// Start is called on the frame when a script is enabled just before any
    /// of the Update methods are called the first time.
    /// </summary>
    private void Start()
    {
        mCharacterA = 
            Instantiate(mCharacterPrefab, mSpawnA.position, mSpawnA.rotation);
        mCharacterA.name = "A";
        mPersonalityA = mCharacterA.GetComponent<Personality>();
        mCharacterB = 
            Instantiate(mCharacterPrefab, mSpawnB.position, mSpawnB.rotation);
        mCharacterB.name = "B";
        mPersonalityB = mCharacterB.GetComponent<Personality>();
        mCurrentKeyIndexA = 
            mCurrentValueIndexA = 
            mCurrentKeyIndexB = 
            mCurrentValueIndexB = 
            mSamplesA = 
            mSamplesB = 0;
        if (mRunSynthOnStart)
            RunSynth();
    }

    /// <summary>
    /// Increments indicies for values and aspect based on samples taken.
    /// </summary>
    /// <param name="samples">
    /// Current number of samples taken on character's current configuration.
    /// </param>
    /// <param name="keyIndex">
    /// Index of aspect in synth values in character's current configuration.
    /// </param>
    /// <param name="valueIndex">
    /// Index of value used for aspect in character's current configuration.
    /// </param>
    private void Increment
        (ref int samples, ref int keyIndex, ref int valueIndex)
    {
        samples++;
        if(samples >= mSamplesPerAspect)
        {
            valueIndex++;
            if(valueIndex >= mSynthValues[mValueKeys[keyIndex]].Length)
            {
                valueIndex = 0;
                keyIndex++;
            }
            RunSynth();
        }
    }

    /// <summary>
    /// Called when a new samples has been recorded.
    /// </summary>
    /// <param name="personality">
    /// Personality for which the recorded response was generated.
    /// </param>
    private void ConversationRecord(Personality personality)
    {
        if (personality == mPersonalityA)
            Increment(
                ref mSamplesA, ref mCurrentKeyIndexA, ref mCurrentValueIndexA
            );
        if (personality == mPersonalityB)
            Increment(
                ref mSamplesB, ref mCurrentKeyIndexB, ref mCurrentValueIndexB
            );
    }

    /// <summary>
    /// Called when a prompt is rejected by GPT.
    /// </summary>
    /// <param name="personality">
    /// Personality for which the response was rejected.
    /// </param>
    private void ConversationDenial(Personality personality)
    {
        RunSynth();
    }

    /// <summary>
    /// Runs data synthesis under the current synthesis configuration.
    /// </summary>
    private void RunSynth()
    {
        ResetSynth();
        mPersonalityA.StartPersonality();
        mPersonalityB.StartPersonality();
    }

    /// <summary>
    /// Resets personalities given the current synthesis configuration parameters.
    /// </summary>
    private void ResetSynth()
    {
        BuildRandomCharacter(mPersonalityA);
        BuildRandomCharacter(mPersonalityB);
        SetPersonalityValuesOneShot(mPersonalityA, mCurrentKeyIndexA, mCurrentValueIndexA);
        SetPersonalityValuesOneShot(mPersonalityB, mCurrentKeyIndexB, mCurrentValueIndexB);
        mPersonalityA.Init();
        mPersonalityB.Init();
    }

    /// <summary>
    /// Sets personality parameter given the current synthesis configuration.
    /// All other parameters are ignored.
    /// </summary>
    /// <param name="personality">
    /// Personality to set parameters on.
    /// </param>
    /// <param name="keyIndex">
    /// Index of aspect to set parameter on.
    /// </param>
    /// <param name="valueIndex">
    /// Value to set aspect to.
    /// </param>
    private void SetPersonalityValuesOneShot
        (Personality personality, int keyIndex, int valueIndex)
    {
        // Key of aspect being set.
        string key = mValueKeys[keyIndex];

        // Value to set aspect to.
        int value = mSynthValues[key][valueIndex];

        switch(key)
        {
            case Defines.OPEN_NAME:
                personality.Openness = value;
                personality.Conscientiousness = Defines.IGNORE_VAL;
                personality.Extraversion = Defines.IGNORE_VAL;
                personality.Agreeableness = Defines.IGNORE_VAL;
                personality.Neroticsm = Defines.IGNORE_VAL;
                personality.Happiness = Defines.IGNORE_VAL;
                personality.Anger = Defines.IGNORE_VAL;
                personality.Sarcasm = Defines.IGNORE_VAL;
                break;
            case Defines.CONSC_NAME:
                personality.Openness = Defines.IGNORE_VAL;
                personality.Conscientiousness = value;
                personality.Extraversion = Defines.IGNORE_VAL;
                personality.Agreeableness = Defines.IGNORE_VAL;
                personality.Neroticsm = Defines.IGNORE_VAL;
                personality.Happiness = Defines.IGNORE_VAL;
                personality.Anger = Defines.IGNORE_VAL;
                personality.Sarcasm = Defines.IGNORE_VAL;
                break;
            case Defines.EXT_NAME:
                personality.Openness = Defines.IGNORE_VAL;
                personality.Conscientiousness = Defines.IGNORE_VAL;
                personality.Extraversion = value;
                personality.Agreeableness = Defines.IGNORE_VAL;
                personality.Neroticsm = Defines.IGNORE_VAL;
                personality.Happiness = Defines.IGNORE_VAL;
                personality.Anger = Defines.IGNORE_VAL;
                personality.Sarcasm = Defines.IGNORE_VAL;
                break;
            case Defines.AGREE_NAME:
                personality.Openness = Defines.IGNORE_VAL;
                personality.Conscientiousness = Defines.IGNORE_VAL;
                personality.Extraversion = Defines.IGNORE_VAL;
                personality.Agreeableness = value;
                personality.Neroticsm = Defines.IGNORE_VAL;
                personality.Happiness = Defines.IGNORE_VAL;
                personality.Anger = Defines.IGNORE_VAL;
                personality.Sarcasm = Defines.IGNORE_VAL;
                break;
            case Defines.NEURO_NAME:
                personality.Openness = Defines.IGNORE_VAL;
                personality.Conscientiousness = Defines.IGNORE_VAL;
                personality.Extraversion = Defines.IGNORE_VAL;
                personality.Agreeableness = Defines.IGNORE_VAL;
                personality.Neroticsm = value;
                personality.Happiness = Defines.IGNORE_VAL;
                personality.Anger = Defines.IGNORE_VAL;
                personality.Sarcasm = Defines.IGNORE_VAL;
                break;
            case Defines.HAPPY_NAME:
                personality.Openness = Defines.IGNORE_VAL;
                personality.Conscientiousness = Defines.IGNORE_VAL;
                personality.Extraversion = Defines.IGNORE_VAL;
                personality.Agreeableness = Defines.IGNORE_VAL;
                personality.Neroticsm = Defines.IGNORE_VAL;
                personality.Happiness = value;
                personality.Anger = Defines.IGNORE_VAL;
                personality.Sarcasm = Defines.IGNORE_VAL;
                break;
            case Defines.ANGER_NAME:
                personality.Openness = Defines.IGNORE_VAL;
                personality.Conscientiousness = Defines.IGNORE_VAL;
                personality.Extraversion = Defines.IGNORE_VAL;
                personality.Agreeableness = Defines.IGNORE_VAL;
                personality.Neroticsm = Defines.IGNORE_VAL;
                personality.Happiness = Defines.IGNORE_VAL;
                personality.Anger = value;
                personality.Sarcasm = Defines.IGNORE_VAL;
                break;
            case Defines.SARC_NAME:
                personality.Openness = Defines.IGNORE_VAL;
                personality.Conscientiousness = Defines.IGNORE_VAL;
                personality.Extraversion = Defines.IGNORE_VAL;
                personality.Agreeableness = Defines.IGNORE_VAL;
                personality.Neroticsm = Defines.IGNORE_VAL;
                personality.Happiness = Defines.IGNORE_VAL;
                personality.Anger = Defines.IGNORE_VAL;
                personality.Sarcasm = value;
                break;
        }
    }

    /// <summary>
    /// Builds a backstory and other descriptions for a personality randomly.
    /// </summary>
    /// <param name="personality">
    /// Personality to build descriptions for.
    /// </param>
    private void BuildRandomCharacter(Personality personality)
    {
        // Personality's job.
        string job = mJobs[Random.Range(0, mJobs.Length)];

        // Personality's name.
        string name = mNames[Random.Range(0, mNames.Length)];

        // Personality's task at hand, what they're doing right now.
        string task = mTasks[Random.Range(0, mTasks.Length)];

        // Something this personality wants to talk about in conversation.
        string topic = mTopics[Random.Range(0, mTopics.Length)];

        // Where the personality is supposedly from.
        string place = mPlaces[Random.Range(0, mPlaces.Length)];

        // The personality's backstory.
        string backstory = $"a {job} from {place}";

        personality.BackStory = backstory;
        personality.CharacterName = name;
        personality.TaskList = new string[] { task };
        personality.TopicsList = new string[] { topic };
    }
}
