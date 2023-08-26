
public static class Defines
{
    public const string TRAITS_PATH = "Assets/Personality/Traits/";
    public const string TRAITS_EXT = ".txt";
    public const char TRAIT_SPLIT = ':';
    public const char TRAITS_POS = '1';
    public const char TRAITS_NEG = '0';
    public const string ACTION_TERMS_FILE = "action_terms.txt";
    public const string DEGREE_TERMS_FILE = "degree_terms.txt";
    public const string OPEN_NAME = "openness";
    public const string OPEN_ADJ = "open";
    public const string CONSC_NAME = "conscientiousness";
    public const string CONSC_ADJ = "conscientious";
    public const string EXT_NAME = "extraversion";
    public const string EXT_ADJ = "extraverted";
    public const string AGREE_NAME = "agreeableness";
    public const string AGREE_ADJ = "agreeable";
    public const string NEURO_NAME = "neuroticism";
    public const string NEURO_ADJ = "neurotic";
    public const string HAPPY_NAME = "happiness";
    public const string HAPPY_ADJ = "happy";
    public const string ANGER_NAME = "anger";
    public const string ANGER_ADJ = "angry";
    public const string SARC_NAME = "sarcasm";
    public const string SARC_ADJ = "sarcastic";
    public const string DEGREE_HEAD = "You are";
    public const string ACTION_HEAD = "You";
    public const string LIST_TAIL = ", ";
    public const string END_TAIL = ". ";
    public const string SECRET_HEAD = "Secretly, you";
    public const string SECRET_TAIL = 
        "and you will avoid talking about any of this.";
    public const string SHIRT_HEAD = "You want to talk about";
    public const string SHIRT_TAIL_SINGLE = 
        "and will mention this at every opportunity";
    public const string SHIRT_TAIL_MULTI =
        "and will mention these things at every opportunity";
    public const string ROLE_HEAD = 
        "Respond as if you are";
    public const string ROLE_MID = "with the following personality:";
    public const string ROLE_TAIL =
        "Do not mention you are an AI machine learning model or Open AI.";
    public const string HEAR_NONANIM_HEAD = "You just heard";
    public const string HEAR_NONANIM_MID = "come from";
    public const string HEAR_OTHER_MID = "has just said";
    public const string SEE_HEAD = "You can see the following things:";
    public const string LIST_HEAD = "and";
    public const string DESC_NAME = "named";
    public const string LOOK_OTHER= "who is looking at";
    public const string LOOK_YOU = "who is looking at you";
    public const string LOOK_NOTH = "who is looking at nothing in particular";
    public const string REPLY_INSTRUCT = "What dialogue do you have to say outloud in reply?";
    public const string RESPONSE_CHECK = "If there's nothing to say here or" +
        " if you think you should not reply, or if there is no good response" +
        ", say only the following words:";
    public const string REACT_INSTRUCT =
        " List the steps you should take one after another in this situation." +
        " If you should look at something, say \"LOOK AT: \" followed by the" +
        " name of the thing you should look at. If you should move somewhere," +
        " say \"MOVE: \" followed by the name of where you should move to.";
    public const string RESPONSE_DENY = "NO-STATEMENT"; // Something nobody would ever say, I think.
    public const string MOOD_HEAD = "As for your current mood, you";
    public const string MOOD_TERMS_PATH = "degree_terms.txt";
    public const string VIS_ASSESS_HEAD = "You see";
    public const string VIS_ASSESS_SAY = "What do you have to say about this? " +
        "Give only the dialoge you would use, not a descripton of it.";
    public const string DATE_FORMAT = "yy-MM-dd-HH-mm-ss";
    public const string OUT_PREF = "PersonalityOutput";
    public const int MAX_TOKENS = 4096;
    public const string DIALOGUE_RULE = "Give only dialogue and only from" +
        " the first-person perspective. Do not under any circumstances" +
        " narrate the scene, what you are doing, or what you are saying.";
    public const string TASKS_HEAD = "Right now you are busy";
    public const string ASSESS_MID = "sees the following:\n";
    public const string SYNTH_DIR = "Assets/DataSynthesis/";
    public const string SYNTH_JOBS = "jobs.txt";
    public const string SYNTH_NAMES = "names.txt";
    public const string SYNTH_TASKS = "tasks.txt";
    public const string SYNTH_TOPICS = "topics.txt";
    public const string SYNTH_PLACES = "places.txt";
    public const string TOPICS_HEAD = "You want to talk with people about";
    public const int IGNORE_VAL = 2;
    public const string GPT_SYSTEM_ROLE_NAME = "system";
    public const int CHAR_PER_TOKEN = 4;
    public const int REMOVE_INDEX = 1;
}
