using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.IO;
using System;

/// <summary>
/// Communicates with GPT and illicits text responses from given prompts.
/// </summary>
public class GptCommunicator : MonoBehaviour
{
    /// <summary>
    /// Message structure for prompts sent to GPT.
    /// </summary>
    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    /// <summary>
    /// Request structure for prompts sent to GPT.
    /// </summary>
    [System.Serializable]
    public class RequestBody
    {
        public string model;
        public Message[] messages;
        public double temperature;
        public double presence_penalty;
        public double frequency_penalty;
    }

    /// <summary>
    /// Structure of choice made when multiple outputs are generatd.
    /// </summary>
    [System.Serializable]
    public class Choice
    {
        public int index;
        public Message message;
        public string finish_reason;
    }

    /// <summary>
    /// Structure measuring GPT usage.
    /// </summary>
    [System.Serializable]
    public class Usage
    {
        public int prompt_tokens;
        public int completion_tokens;
        public int total_tokens;
    }

    /// <summary>
    /// Structure of responses received by GPT.
    /// </summary>
    [System.Serializable]
    public class Response
    {
        public string id;
        public string object_name;
        public int created;
        public string model;
        public Choice[] choices;
        public Usage usage;
    }

    /// <summary>
    /// Queue node for messages to be sent to GPT.
    /// </summary>
    private class RequestNode
    {
        /// <summary>
        /// String to prompt GPT with.
        /// </summary>
        public string mRequestPrompt;

        /// <summary>
        /// Original prompt, before formatting for GPT.
        /// </summary>
        public string mOriginalPrompt;

        /// <summary>
        /// Personality for whom the response is being requested.
        /// </summary>
        public Personality mCaller;

        /// <summary>
        /// Callback function to fire upon reciept of a response.
        /// </summary>
        public ResponseReceived mCallback;
    }

    /// <summary>
    /// List of messages sent to GPT and associated meta data.
    /// </summary>
    public class MessageList
    {
        /// <summary>
        /// List of messages associated w/ their token count.
        /// </summary>
        List< Tuple<Message, int> > mMessages = 
            new List< Tuple<Message, int> >();

        /// <summary>
        /// Current amount of tokens used by this message list.
        /// </summary>
        int mCurrentTokens = 0;

        /// <summary>
        /// Adds a message to the list.
        /// </summary>
        /// <param name="message">
        /// Message to add.
        /// </param>
        public void AddMessage(Message message)
        {
            // Token count of message content.
            int tokens = message.content.Length / Defines.CHAR_PER_TOKEN;

            // Pair of message and its token count.
            Tuple<Message, int> pair = 
                new Tuple<Message, int>(message, tokens);

            mCurrentTokens += tokens;
            mMessages.Add(pair);
            CheckTokenCount();
        }

        /// <summary>
        /// Checks if messages need to be removed to keep tokens under max.
        /// </summary>
        private void CheckTokenCount()
        {
            // Amount of tokens removed if over token count.
            int removed = mMessages[Defines.REMOVE_INDEX].Item2;

            if (mCurrentTokens >= Defines.MAX_TOKENS)
            {
                mMessages.RemoveAt(Defines.REMOVE_INDEX);
                mCurrentTokens -= removed;
            }
        }

        /// <summary>
        /// Sets the system message for GPT prompts.
        /// </summary>
        /// <param name="message">
        /// Message to use in the system role.
        /// </param>
        public void SetSystemMessage(string message)
        {
            // Message - token count pair.
            Tuple<Message, int> pair;

            // Token size of message, approximate.
            int tokenCount = message.Length / Defines.CHAR_PER_TOKEN;

            // New system message.
            Message system = new Message
            {
                role = Defines.GPT_SYSTEM_ROLE_NAME,
                content = message
            };

            pair = new Tuple<Message, int>(system, tokenCount);
            if (mMessages.Count > 0)
            {
                mCurrentTokens -= mMessages[0].Item2;
                mMessages.RemoveAt(0);
            }
            mMessages.Insert(0, pair);
            mCurrentTokens += pair.Item2;
            CheckTokenCount();
        }
    }

    [Tooltip("Enter your OpenAI API key here.")]
    [SerializeField] private string mApiKey = "YOUR API KEY HERE";

    [Tooltip("Open AI competions URL. Probably shouldn't be changed.")]
    [SerializeField] private string mUrl = 
        "https://api.openai.com/v1/chat/completions";

    [Tooltip("Open AI model to generate responses from.")]
    [SerializeField] private string mModel = "gpt-4";

    [Tooltip("How often in seconds to make a request of the Open AI API.")]
    [SerializeField] float mRateLimit = 3.0f;

    [Tooltip("Turns GPT response requests on/off for debugging.")]
    [SerializeField] bool mSendRequests = true;

    [Tooltip("Records output for analysis or training.")]
    [SerializeField] private bool mRecordOutput = false;

    [Tooltip("Whether to print status messages to console.")]
    [SerializeField] private bool mVerbose = false;

    /// <summary>
    /// Current filename to record output to.
    /// </summary>
    private string mOutFile;

    /// <summary>
    /// Current path to record output to.
    /// </summary>
    private string mOutPath;

    /// <summary>
    /// Writes output to file.
    /// </summary>
    private StreamWriter mWriter;

    /// <summary>
    /// Builds strings for output.
    /// </summary>
    private StringBuilder mBuilder;

    /// <summary>
    /// Time of last request made of Open AI API.
    /// </summary>
    private float mLastRequestTime = 0;

    /// <summary>
    /// Delegate for callbacks to be executed upon receipt of a response from 
    /// Open AI API.
    /// </summary>
    /// <param name="received">
    /// Text response generated by Open AI model.
    /// </param>
    public delegate void ResponseReceived(string received);

    /// <summary>
    /// Delegate for functions called when denial string is received.
    /// </summary>
    public delegate void Denial(Personality personality);

    /// <summary>
    /// Event fired when denial string is received.
    /// </summary>
    public event Denial OnDenial;

    /// <summary>
    /// Delegate for functions called when data is recorded.
    /// </summary>
    public delegate void Record(Personality personality);

    /// <summary>
    /// Event fired when data is recorded.
    /// </summary>
    public event Record OnRecord;

    /// <summary>
    /// Queue of requests to be made to GPT.
    /// </summary>
    private Queue<RequestNode> mRequestQueue;

    /// <summary>
    /// Called on the frame when a script is enabled just before any of the
    /// Update methods are called the first time.
    /// </summary>
    private void OnEnable()
    {
        mRequestQueue = new Queue<RequestNode>();
        if (mRecordOutput)
        {
            mBuilder = new StringBuilder(Defines.MAX_TOKENS);
            mOutFile = $"{Defines.OUT_PREF}_" +
                $"{DateTime.Now.ToString(Defines.DATE_FORMAT)}.txt";
            mOutPath = Path.Combine(Application.persistentDataPath, mOutFile);
            mWriter = new StreamWriter(mOutPath, append: true);
            Debug.Log("Opened file for recording data at: " + mOutPath);
        }
    }

    /// <summary>
    /// Called when disabled and deactivated.
    /// </summary>
    private void OnDisable()
    {
        if (mWriter != null)
            mWriter.Close();
    }

    /// <summary>
    /// Requests a statement to be made by Character from GPT in reply to 
    /// an ongoing conversation.
    /// </summary>
    /// <param name="prompt">
    /// Conversation GPT should generate a reply to.
    /// </param>
    /// <param name="callback">
    /// Method to execute once reply has been received from GPT.
    /// </param>
    public void RequestConversationalReply
        (string prompt, Personality caller, ResponseReceived callback)
    {
        string replyPrompt = $"{prompt} {Defines.REPLY_INSTRUCT}" +
            $"{Defines.RESPONSE_CHECK}{Defines.RESPONSE_DENY}";
        //StartCoroutine(PromptGpt(replyPrompt, caller, callback, prompt));
        RequestNode node = new RequestNode
        {
            mRequestPrompt = replyPrompt,
            mOriginalPrompt = prompt,
            mCallback = callback,
            mCaller = caller
        };
        lock(mRequestQueue)
            mRequestQueue.Enqueue(node);
        StartCoroutine(ProcessNextNode());
    }

    /// <summary>
    ///  Processes next request node through queue.
    /// </summary>
    IEnumerator ProcessNextNode()
    {
        RequestNode node;
        lock (mRequestQueue)
            node = mRequestQueue.Count > 0 ? mRequestQueue.Dequeue() : null;
        if (node != null)
        {
            yield return StartCoroutine(
                PromptGpt(
                    node.mRequestPrompt,
                    node.mCaller,
                    node.mCallback,
                    node.mOriginalPrompt)
            );
            StartCoroutine(ProcessNextNode());
        }
    }

    /// <summary>
    /// Requests a statement to be made by Character from GPT in reply to 
    /// what is currently "seeing".
    /// </summary>
    /// <param name="prompt">
    /// Conversation GPT should generate a reply to.
    /// </param>
    /// <param name="callback">
    /// Method to execute once reply has been received from GPT.
    /// </param>
    public void RequestVisualQueuePrompt
        (string prompt, Personality caller, ResponseReceived callback)
    {
        string replyPrompt = $"{Defines.VIS_ASSESS_HEAD} {prompt}" +
            $" {Defines.VIS_ASSESS_SAY} {Defines.RESPONSE_CHECK}" +
            $"{Defines.RESPONSE_DENY}";
        //StartCoroutine(PromptGpt(replyPrompt, caller, callback, prompt));
        RequestNode node = new RequestNode
        {
            mRequestPrompt = replyPrompt,
            mOriginalPrompt = prompt,
            mCallback = callback,
            mCaller = caller
        };
        lock (mRequestQueue)
            mRequestQueue.Enqueue(node);
        StartCoroutine(ProcessNextNode());
    }

    /// <summary>
    /// Requests instructions on what to do from GPT based on a text 
    /// description of a Character's current state in the game.
    /// </summary>
    /// <param name="prompt">
    /// Description of character's current state.
    /// </param>
    /// <param name="callback">
    /// Method to execute upon receipt of instructions from GPT.
    /// </param>
    public void RequestReactionInstructions
        (string prompt, Personality caller, ResponseReceived callback)
    {
        string spokenReplyPrompt = $"{prompt} {Defines.REACT_INSTRUCT}";
        StartCoroutine(PromptGpt(spokenReplyPrompt, caller, callback, prompt));
    }

    /// <summary>
    /// Prompts Open AI GPT model.
    /// </summary>
    /// <param name="prompt">
    /// Text prompt for GPT.
    /// </param>
    /// <param name="callback">
    /// Method to execute upon receipt of response from GPT.
    /// </param>
    /// <returns></returns>
    IEnumerator PromptGpt(
        string prompt, Personality caller, 
        ResponseReceived callback, string original)
    {
        if (mVerbose)
            Debug.Log($"GptCommunicator: Sending reply request for prompt:\n{prompt} with" +
                $" key {mApiKey}.");
        if (mSendRequests)
        {
            // Current game time.
            float time = Time.realtimeSinceStartup;

            // Difference between current time and the last  request.
            float delta = time - mLastRequestTime;

            // How long to wait before making another request.
            float waitSeconds = mLastRequestTime == 0 || delta >= mRateLimit ?
                0 : mRateLimit - delta;
            if (mVerbose)
                Debug.Log($"GptCommunicator: Waiting {waitSeconds} seconds before sending the next request...");
            yield return new WaitForSeconds(waitSeconds);
            if (mVerbose)
                Debug.Log("GptCommunicator: Wait ended.");
            UnityWebRequest www = new UnityWebRequest(mUrl, "POST");
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", $"Bearer {mApiKey}");
            Message message = new Message
            {
                role = "user",
                content = prompt
            };
            caller.Messages.Add(message);
            RequestBody body = new RequestBody
            {
                model = mModel,
                messages = caller.Messages.ToArray(),
                temperature = caller.Temperature,
                presence_penalty = caller.PresencePenalty,
                frequency_penalty = caller.FrequencyPenalty
            };
            string bodyJson = JsonUtility.ToJson(body);
            byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(bodyJson);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
            www.downloadHandler = dH;
            if (mVerbose)
                Debug.Log($"GptCommunicator:  Request sent...");
            yield return www.SendWebRequest();
            if (mVerbose)
                Debug.Log("GptCommunicator:  Response recieved.");
            if (www.result == UnityWebRequest.Result.Success)
            {
                Response response =
                    JsonUtility.FromJson<Response>(www.downloadHandler.text);
                
                // TO DO: You need to figure out how to turn this into a wait time
                // and have it sit here after receiving the response based on that number.
                int tokens = response.usage.total_tokens;

                string responseText = response.choices[0].message.content;
                if (!responseText.Contains(Defines.RESPONSE_DENY))
                {
                    caller.Messages.Add(
                        new Message { role = "assistant", content = responseText }
                    );
                    callback?.Invoke(responseText.Replace("\"", string.Empty));

                    if (mRecordOutput)
                        RecordOutput(caller, original, responseText);
                }
                else
                {
                    Debug.Log($"GptCommunicator: Got denial string \"{Defines.RESPONSE_DENY}\"" +
                        $" from prompt:\n\"{prompt}\"");
                    OnDenial?.Invoke(caller);
                }
            }
            else
            {
                Debug.Log($"GptCommunicator: Requester Error: {www.error}");
            }
            mLastRequestTime = Time.realtimeSinceStartup;
        }
    }

    /// <summary>
    /// Records input to gpt and output from gpt to file.
    /// </summary>
    /// <param name="personality">
    /// Personality being served by gpt.
    /// </param>
    /// <param name="prompt">
    /// Dialogue prompt being sent to gpt.
    /// </param>
    /// <param name="output">
    /// Output received from GPT.
    /// </param>
    private void RecordOutput(Personality personality, string prompt, string output)
    {
        // First charcter of prompt, capitalized.
        string promptUpper = $"{prompt[0]}".ToUpper();

        // Prompt formatted with first character capitalized.
        string promptFormatted = $"{promptUpper[0]}{prompt[1..]}";

        // String to record to file.
        string record = $"You are {personality.BackStory}." +
            $" {personality.Summary}\n{promptFormatted} What is your" +
            $" response?\n{output}";

        mWriter.WriteLine(record);
        OnRecord?.Invoke(personality);
    }
}
