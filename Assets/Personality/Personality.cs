using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

/// <summary>
/// Manages text descriptions of a self and any "percieved" other describable
/// objects.
/// </summary>
public class Personality : MonoBehaviour, IDescribable
{
    [Header("Character Attributes")]

    [Tooltip("The name of this character.")]
    [SerializeField] private string mCharacterName = "Bilbo";

    [Tooltip("Who and what this character is. Short and sweet, use as little" +
        " punctuation as possible.")]
    [SerializeField] private string mBackStory = 
        "a simulated video game character";

    [Tooltip("Integer value, [0-4].\n\"Openness is an overarching concept or" +
        " philosophy that is characterized by an emphasis on transparency and " +
        "collaboration.\" -Wikipedia")]
    [MinMax(0, 4)]
    [SerializeField] private int mOpenness = 2;

    [Tooltip("Integer value, [0-4].\n\"...the personality trait of being " +
        "careful or diligent.\" -Wikipedia")]
    [MinMax(0, 4)]
    [SerializeField] private int mConscientiousness = 2;

    [Tooltip("Integer value, [0-4].\n\"Extraversion tends to be manifested in " +
        "outgoing, talkative, energetic behavior.\" -Wikipedia")]
    [MinMax(0, 4)]
    [SerializeField] private int mExtraversion = 2;

    [Tooltip("Integer value, [0-4].\n\"Agreeableness is a personality trait " +
        "that manifests as behavior that is perceived as kind, sympathetic, " +
        "cooperative, warm, frank, and considerate.\" -Wikipedia")]
    [MinMax(0, 4)]
    [SerializeField] private int mAgreeableness = 2;

    [Tooltip("Integer value, [0-4].\n\"...individuals with high scores for" +
        " neuroticism are more likely than average to be moody and to" +
        " experience such feelings as anxiety, worry, fear, anger," +
        " frustration, envy, jealousy, pessimism, guilt, depressed mood, and" +
        " loneliness.\" -Wikipedia")]
    [MinMax(0, 4)]
    [SerializeField] private int mNeuroticsm = 2;

    [Tooltip("Integer value, [0-4].\n\"Happiness is a positive and pleasant " +
        "emotion, ranging from contentment to intense joy.\" - Wikipedia")]
    [MinMax(0, 4)]
    [SerializeField] private int mHappiness = 2;

    [Tooltip("Integer value, [0-4].\n\"Anger, also known as wrath or rage, " +
        "is an intense emotional state involving a strong uncomfortable and " +
        "non-cooperative response to a perceived provocation, hurt or threat.\"" +
        " -Wikipedia")]
    [MinMax(0, 4)]
    [SerializeField] private int mAnger = 2;

    [Tooltip("Integer value, [0-4].\n\"Sarcasm is the caustic use of words, " +
        "often in a humorous way, to mock someone or something.\" -Wikipedia")]
    [MinMax(0, 4)]
    [SerializeField] private int mSarcasm = 2;

    [Tooltip("List of things this character avoids in conversation, in the" +
        " form, \"Secretly, you...\"")]
    [SerializeField] private string[] mSecrets;

    [Tooltip("List of things about themselves this character would love to" +
        " tell you about, in the form \"You want everyone to know...\"")]
    [SerializeField] private string[] mShirtSleeve;

    [Tooltip("What this character is doing right now. The tasks at hand.")]
    [SerializeField] private string[] mTasks;

    [Tooltip("Subjects this character wants to ask people and talk about.")]
    [SerializeField] private string[] mTopics;

    [Header("Model Paramters")]

    [Tooltip("\"The maximum number of tokens to generate in the completion.\" " +
        "-OpenAI API Documentation")]
    [SerializeField] private int mMaxTokens = Defines.MAX_TOKENS;

    [Tooltip("\"What sampling temperature to use, between 0 and 2. Higher" +
        " values like 0.8 will make the output more random, while lower values" +
        " like 0.2 will make it more focused and deterministic.\" " +
        "-OpenAI API Documenation")]
    [SerializeField] private float mTemperature = 0.7f;

    [Tooltip("\"Number between -2.0 and 2.0. Positive values penalize new " +
        "tokens based on whether they appear in the text so far, increasing the" +
        " model's likelihood to talk about new topics.\" " +
        "-OpenAI API Documenation")]
    [SerializeField] float mPresencePenalty = 1.0f;

    [Tooltip( "\"Number between - 2.0 and 2.0.Positive values penalize new " +
        "tokens based on their existing frequency in the text so far, " +
        "decreasing the model's likelihood to repeat the same line verbatim.\"" +
        " -OpenAI API Documenation")]
    [SerializeField] float mFrequencyPenalty = 1.0f;

    [Tooltip("Whehter to wait based on char length of last response.")]
    [SerializeField] bool mCalculateWait = false;

    [Tooltip("How many seconds per char the character should wait before" +
    " doing anything else after saying something.")]
    [SerializeField] float mWaitScalar = 0.1f;

    [Tooltip("Max time to wait before making a new gpt reply request.")]
    [SerializeField] private float mMaxReplyWait = 20.0f;

    [Header("Test Features")]

    [Tooltip("Test statement for the character to make manually.")]
    [SerializeField] private string mManualStatement = "";

    [Tooltip("Causes character to make a manually entered test statement.")]
    [SerializeField] private bool mMakeManualStatment = false;

    [Tooltip("Causes status statements to be printed to console.")]
    [SerializeField] private bool mVerbose = false;

    [Tooltip("Whether to immediatly bring character personality to life.")]
    [SerializeField] private bool mAutoActivate = false;

    /// <summary>
    /// Controller asset in scene that generates personality prompts for all 
    /// LLM personalities. 
    /// </summary>
    private PersonalityManager mPersonalityController;

    /// <summary>
    /// Personality descriptions based on parameters.
    /// </summary>
    private string[] mDescriptions;

    /// <summary>
    /// Single string representing all aspects of personality.
    /// </summary>
    private string mPersonalityPrompt;

    /// <summary>
    /// String builder used to build prompt strings efficiently.
    /// </summary>
    private StringBuilder mBuilder;
  
    /// <summary>
    /// Communication interface between the character and GPT.
    /// </summary>
    private GptCommunicator mGpt;

    /// <summary>
    /// Abstract hearing sense to generate prompts from heard "sounds".
    /// </summary>
    private Hearing mHearing;

    /// <summary>
    /// Function delegate to "say" statements to surrounding characters.
    /// </summary>
    /// <param name="speaker">
    /// The Character making the statement.
    /// </param>
    /// The statement being made.
    /// <param name="statment"></param>
    public delegate void SayToOthers(Personality speaker, string statment);

    /// <summary>
    /// Event fired to "say" statements to surrounding characters.
    /// </summary>
    public event SayToOthers onSayToOthers;

    /// <summary>
    /// What the character is "looking at" currently.
    /// </summary>
    private IDescribable mLookingAt;

    /// <summary>
    /// Vision abstraction to describe what the character
    /// can "see" in a written prompt.
    /// </summary>
    private Vision mVision;

    /// <summary>
    /// Text description of Character's role they are playing.
    /// </summary>
    private string mRole;

    /// <summary>
    /// List of GPT Messages sent and received on behalf of this Character.
    /// </summary>
    //private List<GptCommunicator.Message> mMessages;

    /// <summary>
    /// Description of Character's surroundings.
    /// </summary>
    private string mSurroundings;

    /// <summary>
    /// How long the character should wait before making another statment,
    /// based on the previous statement.
    /// </summary>
    private float mLastStatementWait;

    /// <summary>
    /// The last time a statement was made.
    /// </summary>
    private float mLastStatementTime;

    /// <summary>
    /// Short description of personality, for data collection purposes.
    /// </summary>
    private string mSummary;

    private GptCommunicator.MessageList mMessageList;

    /** Accessors/Setters **/
    public string CharacterName 
    { 
        get => mCharacterName; 
        set => mCharacterName = value; 
    }
    public string BackStory { get => mBackStory; set => mBackStory = value; }
    public float Temperature { get => mTemperature; }
    public float PresencePenalty { get => mPresencePenalty; }
    public float FrequencyPenalty { get => mFrequencyPenalty; }
    public int Openness { get => mOpenness; set => mOpenness = value; }
    public int Conscientiousness 
    { 
        get => mConscientiousness;
        set => mConscientiousness = value;
    }
    public int Extraversion 
    { 
        get => mExtraversion;
        set => mExtraversion = value;
    }
    public int Agreeableness 
    { 
        get => mAgreeableness;
        set => mAgreeableness = value;
    }
    public int Neroticsm { get => mNeuroticsm; set => mNeuroticsm = value; }
    public int Happiness { get => mHappiness; set => mHappiness = value; }
    public int Anger { get => mAnger; set => mAnger = value; }
    public int Sarcasm { get => mSarcasm; set => mSarcasm = value; }
    public string[] Secrets { get => mSecrets; set => mSecrets = value; }
    public string[] ShirtSleeve { get => mShirtSleeve; set => mShirtSleeve = value; }
    public IDescribable LookingAt
    {
        get => mLookingAt;
        set => mLookingAt = value;
    }
    public string[] Descriptions { get => mDescriptions; }
    //public List<GptCommunicator.Message> Messages { get => mMessages; }
    public bool Verbose { get => mVerbose; }
    public string Summary { get => mSummary; }
    public string Tasks { get => TasksDescription(); }
    public string[] TaskList { set => mTasks = value; }
    public string Role { get => BuildRole(); }
    public string Topics { get => TopicsDescription(); }
    public string[] TopicsList { set => mTopics = value; }
    public float Wait { get => mLastStatementWait; }
    public GptCommunicator.MessageList MessageList { get => mMessageList; }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        Init();
    }

    /// <summary>
    /// Start is called on the frame when a script is enabled just before any
    /// of the Update methods are called the first time.
    /// </summary>
    private void Start()
    {
        if (mAutoActivate)
            StartPersonality();
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    private void Update()
    {
        if (mMakeManualStatment)
            MakeManualStatement();
    }

    /// <summary>
    /// Starts the Personality.
    /// </summary>
    public void StartPersonality()
    {
        mBuilder.Clear();
        mMessageList = new GptCommunicator.MessageList();
        mDescriptions = mPersonalityController.GenerateNewPersonality(this);
        foreach (string aspect in mDescriptions)
            mBuilder.Append($"{aspect}");
        mPersonalityPrompt = mBuilder.ToString();
        BuildRole();
        MessageList.SetSystemMessage(mRole);
        mSummary = mPersonalityController.GenerateSummary(this);
        mVision.Reset();
        mHearing.Reset();
    }

    /// <summary>
    /// Initializes (or reinitializes) the Personality.
    /// </summary>
    public void Init()
    {
        mBuilder = new StringBuilder(mMaxTokens * 4);
        mPersonalityController = FindAnyObjectByType<PersonalityManager>();
        if (mPersonalityController == null)
            Debug.LogError($"ERROR: No PersonalityManager found in scene!");
        mDescriptions = mPersonalityController.GenerateNewPersonality(this);
        mVision = GetComponentInChildren<Vision>();
        if (mVision == null)
            Debug.LogError($"ERROR: Character {mCharacterName} has no vision" +
                $" component!");
        mGpt = FindFirstObjectByType<GptCommunicator>();
        if (mGpt == null)
            Debug.LogError($"ERROR: Gpt communicator not found in scene!");
        mHearing = gameObject.GetComponentInChildren<Hearing>();
    }

    /// <summary>
    /// Builds a role description string from parameter description strings.
    /// </summary>
    /// <returns>
    /// Role description.
    /// </returns>
    public string BuildRole()
    {
        mRole = $"{Defines.ROLE_HEAD} {BackStory} named " +
            $"{CharacterName} {Defines.ROLE_MID} {mPersonalityPrompt} " +
            $"{Tasks}{Topics}{Defines.ROLE_TAIL} {Defines.DIALOGUE_RULE}";
        mMessageList.SetSystemMessage(mRole);
        return mRole;
    }

    /// <summary>
    /// Builds a description of topics this personality wants to talk about.
    /// </summary>
    /// <returns>
    /// Description of topics this perosnality wants to talk about.
    /// </returns>
    private string TopicsDescription()
    {
        // Description of topics.
        string description = "";

        if (mTopics.Length > 0)
        {
            mBuilder.Clear();
            mBuilder.Append($"{Defines.TOPICS_HEAD} ");
            if (mTopics.Length > 1)
            {
                for (int i = 0; i < mTopics.Length; i++)
                {
                    mBuilder.Append(mTopics[i]);
                    if (i < mTopics.Length - 1)
                        mBuilder.Append(Defines.LIST_TAIL);
                    else
                        mBuilder.Append(Defines.END_TAIL);
                }
            }
            else
                mBuilder.Append($"{mTopics[0]}{Defines.END_TAIL}");
            description = mBuilder.ToString();
        }
        return description;
    }

    /// <summary>
    /// Generates a description of current tasks occupied by this character.
    /// </summary>
    /// <returns>
    /// Task desscription string.
    /// </returns>
    private string TasksDescription()
    {
        // Description of tasks.
        string description = "";

        if (mTasks.Length > 0)
        {
            mBuilder.Clear();
            mBuilder.Append($"{Defines.TASKS_HEAD} ");
            for (int i = 0; i < mTasks.Length; i++)
            {
                mBuilder.Append(mTasks[i]);
                if (i < mTasks.Length - 1)
                    mBuilder.Append(Defines.LIST_TAIL);
                else
                    mBuilder.Append(Defines.END_TAIL);
            }
            description = mBuilder.ToString();
        }
        return description;
    }

    /// <summary>
    /// Checks whether the character can see or hear anything.
    /// Only handles vision right now. TO DO: Handle hearing.
    /// </summary>
    public void AssessSurroundings()
    {
        // Text description of Character's environment.
        string assessment = DescribeVisualSurroundings();
        if(!assessment.Equals(string.Empty))
            mGpt.RequestVisualQueuePrompt(assessment, this, SayOutLoud);
        Debug.Log($"{mCharacterName} {Defines.ASSESS_MID}{assessment}");
    }

    /// <summary>
    /// Sets the system prompt for the message list.
    /// </summary>
    /// <param name="prompt"></param>
    public void SetSystemPrompt(string prompt)
    {
        // New system prompt message.
        GptCommunicator.Message system = new GptCommunicator.Message
        {
            role = "system",
            content = prompt
        };
        /*
        if (mMessages.Count > 0)
            mMessages.RemoveAt(0);
        mMessages.Insert(0, system);
        */
    }

    /// <summary>
    /// Checks verbosity.
    /// </summary>
    /// <returns>
    /// True if Character is verbose, false if set otherwise.
    /// </returns>
    public bool IsVerbose()
    {
        return mVerbose;
    }

    /// <summary>
    /// Character makes a preset statement for debugging.
    /// </summary>
    private void MakeManualStatement()
    {
        if (Verbose)
            Debug.Log($"{mCharacterName} is making the manual statement " +
                $"\"{mManualStatement}\".");
        if (mManualStatement != null && mManualStatement.Length > 0)
            onSayToOthers?.Invoke(this, mManualStatement);
        mMakeManualStatment = false;
    }

    /// <summary>
    /// Makes a statement out loud to surrounding Characters.
    /// </summary>
    /// <param name="statement">
    /// Statement being made.
    /// </param>
    private void SayOutLoud(string statement)
    {
        // Current game time.
        float time;

        // Difference between current time and the last dialogue request.
        float delta;

        // How long to wait before making another request.
        float waitSeconds;

        if (mCalculateWait)
        {
            time = Time.realtimeSinceStartup;
            delta = time - mLastStatementTime;
            waitSeconds = delta > mLastStatementWait ?
                0 : Mathf.Min(mMaxReplyWait, mLastStatementWait - delta);
            mLastStatementTime = time;
            mLastStatementWait = statement.Length * mWaitScalar;
        }
        else
            waitSeconds = 0;
        StartCoroutine(MakeStatement(statement, waitSeconds));
    }

    /// <summary>
    /// Makes a reply request from GPT.
    /// </summary>
    /// <param name="statement">
    /// The statement to reply to.
    /// </param>
    /// <param name="waitSeconds">
    /// How long to wait before making the request.
    /// </param>
    /// <returns></returns>
    IEnumerator MakeStatement(string statement, float waitSeconds)
    {
        yield return new WaitForSeconds(waitSeconds);
        onSayToOthers?.Invoke(this, statement);
        if(mVerbose)
            Debug.Log($"{mCharacterName} said \"{statement}\".");
    }

    /// <summary>
    /// Executes function calls based on text description of intended actions.
    /// </summary>
    /// <param name="instructions">
    /// Text description from which to generate fuction calls.
    /// </param>
    private void FollowInstructions(string instructions)
    {
        // To do.
    }

    /// <summary>
    /// Receives a statement made by another character,
    /// illicits a response if appropriate.
    /// </summary>
    /// <param name="speaker">
    /// Character making the statement.
    /// </param>
    /// <param name="statement">
    /// Statement being made by the spearker.
    /// </param>
    public void HearFromOther(IDescribable speaker, string statement)
    {
        string prompt = $"{speaker.DescribeSelfForOther(this)} " +
            $"{Defines.HEAR_OTHER_MID} \"{statement}\"{Defines.END_TAIL}";
        if (mVerbose)
            Debug.Log($"{mCharacterName} heard \"{statement}\".");
        mGpt.RequestConversationalReply(prompt, this, SayOutLoud);
    }

    /// <summary>
    /// Recieves a text descritption of a noise made in the vicinity of a
    /// Character, illicits a response if approrpriate.
    /// </summary>
    /// <param name="noiseMaker">
    /// Thing that made the noise.
    /// </param>
    /// <param name="noise">
    /// Text description of the noise.
    /// </param>
    public void HearFromNonAnimate(IDescribable noiseMaker, string noise)
    {
        mBuilder.Clear();
        mBuilder.Append($"{Defines.HEAR_NONANIM_HEAD} {noise} " +
            $"{Defines.HEAR_NONANIM_MID} {noiseMaker.DescribeSelfForOther(this)}" +
            $"{Defines.END_TAIL}");
        mBuilder.Append(DescribeVisualSurroundings());
        mGpt.RequestReactionInstructions(mBuilder.ToString(), this, FollowInstructions);
    }

    /// <summary>
    /// Generates a text description of the Character's visual surroundings.
    /// </summary>
    /// <returns></returns>
    public string DescribeVisualSurroundings()
    {
        // Things seen by Character.
        IDescribable[] seen = mVision.Seen;

        // Iterator variable.
        int i;

        // Description of visual surroundings.
        string description = string.Empty;

        if (seen.Length > 0)
        {
            mBuilder.Clear();
            mBuilder.Append($"{Defines.SEE_HEAD} ");
            for (i = 0; i < seen.Length; i++)
            {
                if (i < seen.Length - 1)
                    mBuilder.Append($"{seen[i].DescribeSelfForOther(this)}" +
                        $"{Defines.LIST_TAIL}");
                else
                    if(seen.Length > 1)
                        mBuilder.Append($"{Defines.LIST_HEAD} " +
                            $"{seen[i].DescribeSelfForOther(this)}{Defines.END_TAIL}");
                    else
                        mBuilder.Append($"{seen[i].DescribeSelfForOther(this)}" +
                            $"{Defines.END_TAIL}");
            }
            description = mBuilder.ToString();
        }
        mSurroundings = description;
        return description;
    }

    /// <summary>
    /// Generates a text description of this character for another character.
    /// </summary>
    /// <param name="caller">
    /// Character the text description is being generated for.
    /// </param>
    /// <returns>
    /// Character the text description os being generated of.
    /// </returns>
    public string DescribeSelfForOther(Personality caller)
    {
        mBuilder.Clear();
        mBuilder.Append($"{BackStory} {Defines.DESC_NAME} {CharacterName}");
        if (LookingAt != null)
            if ((object)LookingAt != caller)
                mBuilder.Append($" {Defines.LOOK_OTHER} {LookingAt}");
            else
                mBuilder.Append($" {Defines.LOOK_YOU}");
        else
            mBuilder.Append($" {Defines.LOOK_NOTH}");
        return mBuilder.ToString();
    }

    public string GetName()
    {
        return CharacterName;
    }
}
