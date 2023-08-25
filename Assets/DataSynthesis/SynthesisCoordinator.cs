using UnityEngine;
using System.IO;

public class SynthesisCoordinator : MonoBehaviour
{
    string[] mJobs;
    string[] mNames;
    string[] mTasks;
    string[] mTopics;

    private void OnEnable()
    {
        mJobs = File.ReadAllLines($"{Defines.SYNTH_DIR}{Defines.SYNTH_JOBS}");
        mNames = 
            File.ReadAllLines($"{Defines.SYNTH_DIR}{Defines.SYNTH_NAMES}");
        mTasks = 
            File.ReadAllLines($"{Defines.SYNTH_DIR}{Defines.SYNTH_TASKS}");
        mTopics = 
            File.ReadAllLines($"{Defines.SYNTH_DIR}{Defines.SYNTH_TOPICS}");
    }
}
