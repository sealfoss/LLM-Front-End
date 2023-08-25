using UnityEngine;
using System.IO;

public class SynthesisCoordinator : MonoBehaviour
{
    [SerializeField] private GameObject mCharacterPrefab;
    [SerializeField] private Transform mSpawnA;
    [SerializeField] private Transform mSpawnB;
    private string[] mJobs;
    private string[] mNames;
    private string[] mTasks;
    private string[] mTopics;
    private string[] mPlaces;
    private GameObject mCharacterA;
    private GameObject mCharacterB;
    private Personality mPersonalityA;
    private Personality mPersonalityB;
    
    private void OnEnable()
    {
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

    private void Start()
    {
        mCharacterA = Instantiate(mCharacterPrefab, mSpawnA);
        mPersonalityA = mCharacterA.GetComponent<Personality>();
        mCharacterB = Instantiate(mCharacterPrefab, mSpawnB);
        mPersonalityB = mCharacterB.GetComponent<Personality>();
    }

    private void BuildRandomCharacter(Personality personality)
    {
        string job = mJobs[Random.Range(0, mJobs.Length)];
        string name = mNames[Random.Range(0, mNames.Length)];
        string task = mTasks[Random.Range(0, mTasks.Length)];
        string topic = mTopics[Random.Range(0, mTopics.Length)];
        string place = mPlaces[Random.Range(0, mPlaces.Length)];
        string backstory = $"a {job} from {place}";
    }
}
