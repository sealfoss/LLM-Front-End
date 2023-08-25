using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// Generates text prompts describing a paramterized personality.
/// </summary>
public class PersonalityGenerator
{
    /// <summary>
    /// Aspect of personality by name and adjective, and other dscritpions.
    /// </summary>
    private class PersonalityAspect
    {
        /// <summary>
        /// Trait name.
        /// </summary>
        protected string mName;

        /// <summary>
        /// Adjective used to assign trait.
        /// </summary>
        protected string mAdjective;

        /** Accessors/Setters **/
        public string Name { get => mName; }
        public string Adjective { get => mAdjective; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Trait name.</param>
        /// <param name="adjective">Trait adjective.</param>
        public PersonalityAspect(string name, string adjective)
        {
            mName = name;
            mAdjective = adjective;
        }
    }

    /// <summary>
    /// Text prompts and fragments describing a given personality trait with
    /// methods to generate descriptive prompts based on a parameter value for
    /// the defined trait.
    /// </summary>
    private class Trait : PersonalityAspect
    {
        /// <summary>
        /// Things people with this personality trait will do.
        /// </summary>
        private string[] mManifestations;

        /// <summary>
        /// Things people with this personality trait will NOT do.
        /// </summary>
        private string[] mContradictions;

        /** Accessors/Setters **/
        public string[] Manifestations { get => mManifestations; }
        public string[] Contradictions { get => mContradictions; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Trait name.</param>
        /// <param name="adjective">Trait adjective.</param>
        public Trait(string name, string adjective) : base(name, adjective)
        {
            ReadTraits();
        }

        /// <summary>
        /// Reads in descriptions of a given trait.
        /// </summary>
        private void ReadTraits()
        {
            // List of read trait manifestations.
            List<string> pos = new List<string>();

            // List of read trait contradictions.
            List<string> neg = new List<string>();

            // Path to trait's descriptions file.
            string path =
                $"{Defines.TRAITS_PATH}/{mName}{Defines.TRAITS_EXT}";

            // Lines read from file.
            string[] lines = File.ReadAllLines(path);

            foreach (string line in lines)
                if (line[0] == Defines.TRAITS_POS)
                    pos.Add(line.Split(Defines.TRAIT_SPLIT)[1]);
                else if (line[0] == Defines.TRAITS_NEG)
                    neg.Add(line.Split(Defines.TRAIT_SPLIT)[1]);
            mManifestations = pos.ToArray();
            mContradictions = neg.ToArray();
        }
    }

    /// <summary>
    /// Initialization value for stringbuilder internal arrayl.
    /// </summary>
    private static readonly int mInitChars = 4096;

    /// <summary>
    /// Terms used to describe personality actions.
    /// </summary>
    private string[] mActionTerms;

    /// <summary>
    /// Terms used to describe degree of trait aspects.
    /// </summary>
    private string[] mDegreeTerms;

    /// <summary>
    /// Terms used to describe degree of mood aspects.
    /// </summary>
    private string[] mMoodTerms;

    /// <summary>
    /// Stringbuilder used to build prompt strings.
    /// </summary>
    private StringBuilder mBuilder;

    /// <summary>
    /// Array of read trait parameter strings.
    /// </summary>
    private Trait[] mTraits;

    /// <summary>
    /// Array of mood descriptions.
    /// </summary>
    private PersonalityAspect[] mMoods;

    /// <summary>
    /// Dictionary to query for index of a given trait from its name.
    /// </summary>
    private Dictionary<string, int> mAspectIndicies;

    /// <summary>
    /// Constructor.
    /// </summary>
    public PersonalityGenerator()
    {
        // Iterator. 
        int i;

        mAspectIndicies = new Dictionary<string, int>();
        Trait mOpenness = new Trait(Defines.OPEN_NAME, Defines.OPEN_ADJ);
        Trait mConscientiousness =
            new Trait(Defines.CONSC_NAME, Defines.CONSC_ADJ);
        Trait mExtraversion = new Trait(Defines.EXT_NAME, Defines.EXT_ADJ);
        Trait mAgreeableness =
            new Trait(Defines.AGREE_NAME, Defines.AGREE_ADJ);
        Trait mNeuroticism = new Trait(Defines.NEURO_NAME, Defines.NEURO_ADJ);
        Trait[] traits = { mOpenness, mConscientiousness, mExtraversion,
            mAgreeableness, mNeuroticism };
        mTraits = traits;
        PersonalityAspect mHappiness =
            new PersonalityAspect(Defines.HAPPY_NAME, Defines.HAPPY_ADJ);
        PersonalityAspect mAnger =
            new PersonalityAspect(Defines.ANGER_NAME, Defines.ANGER_ADJ);
        PersonalityAspect mSarcasm =
            new PersonalityAspect(Defines.SARC_NAME, Defines.SARC_ADJ);
        PersonalityAspect[] moods = { mHappiness, mAnger, mSarcasm };
        mMoods = moods;
        for (i = 0; i < mTraits.Length; i++)
            mAspectIndicies[mTraits[i].Name] = i;
        for (i = 0; i < mMoods.Length; i++)
            mAspectIndicies[mMoods[i].Name] = i;
        mActionTerms = File.ReadAllLines($"{Defines.TRAITS_PATH}" +
            $"{Defines.ACTION_TERMS_FILE}");
        mDegreeTerms = File.ReadAllLines($"{Defines.TRAITS_PATH}" +
            $"{Defines.DEGREE_TERMS_FILE}");
        mMoodTerms = File.ReadAllLines($"{Defines.TRAITS_PATH}" +
            $"{Defines.MOOD_TERMS_PATH}");
        mBuilder = new StringBuilder(mInitChars);
    }

    /// <summary>
    /// Generates a description string for a personality trait based on a
    /// given parameter value.
    /// </summary>
    /// <param name="traitValue">
    /// Value of of the personality trait.
    /// </param>
    /// <param name="traitIndex">
    /// Index of personality triat in mTraits member array.
    /// </param>
    /// <returns></returns>
    public string GenerateTraitDescription(int traitValue, int traitIndex)
    {
        // Trait to describe.
        Trait trait = mTraits[traitIndex];

        // String to end lines with.
        string tail;

        // Description of trait.
        string description = "";

        // Iterator. 
        int i;

        if (traitValue != 2)
        {
            mBuilder.Clear();
            mBuilder.Append($"{Defines.ACTION_HEAD}" +
                $" {mDegreeTerms[traitValue]}" +
                $" {trait.Adjective}{Defines.END_TAIL}");
            mBuilder.Append($"{Defines.ACTION_HEAD}" +
                $" {mActionTerms[traitValue]} ");
            for (i = 0; i < trait.Manifestations.Length; i++)
            {
                mBuilder.Append(trait.Manifestations[i]);
                tail = (i < trait.Manifestations.Length - 1) ?
                    Defines.LIST_TAIL : Defines.END_TAIL;
                mBuilder.Append(tail);
            }
            mBuilder.Append($"{Defines.ACTION_HEAD}" +
                $" {mActionTerms[mActionTerms.Length - traitValue - 1]} ");
            for (i = 0; i < trait.Contradictions.Length; i++)
            {
                mBuilder.Append(trait.Contradictions[i]);
                tail = (i < trait.Contradictions.Length - 1) ?
                    Defines.LIST_TAIL : Defines.END_TAIL;
                mBuilder.Append(tail);
            }
            description = mBuilder.ToString();
        }
        return description;
    }

    /// <summary>
    /// Generates a description string of secrets this perosnality would like 
    /// to keep to themselves.
    /// </summary>
    /// <param name="secrets">
    /// Array of secrets this personality should keep to themselves.
    /// </param>
    /// <returns></returns>
    public string GenerateSecretsDescription(string[] secrets)
    {
        // Description of secret.
        string description = "";

        // End of a given description or line.
        string tail;

        // Iterator variable.
        int i;

        if (secrets.Length > 0)
        {
            mBuilder.Clear();
            mBuilder.Append($"{Defines.SECRET_HEAD} ");
            for (i = 0; i < secrets.Length; i++)
            {
                mBuilder.Append(secrets[i]);
                tail = i < secrets.Length - 1 ?
                    Defines.LIST_TAIL : $", {Defines.SECRET_TAIL} ";
                mBuilder.Append(tail);
            }
            description = mBuilder.ToString();
        }
        return description;
    }

    /// <summary>
    /// Generates a text description of a character's "shirt sleeve", or the
    /// things that character wants to talk about or let you know.
    /// </summary>
    /// <param name="shirtSleeve">
    /// List of things the character wants to tell you about.
    /// </param>
    /// <returns>
    /// Text description of the character's shirt sleeve.
    /// </returns>
    public string GenerateShirtSleeveDescription(string[] shirtSleeve)
    {
        // Description of something character "wears on their shirt sleeve".
        string description = "";

        // End of a line or description.
        string tail;

        // Iterator variable.
        int i;

        if (shirtSleeve.Length > 0)
        {
            mBuilder.Clear();
            mBuilder.Append($"{Defines.SHIRT_HEAD} ");
            for (i = 0; i < shirtSleeve.Length; i++)
            {
                mBuilder.Append(shirtSleeve[i]);
                tail = i < shirtSleeve.Length - 1 ?
                    Defines.LIST_TAIL : $", {Defines.SHIRT_TAIL} ";
                mBuilder.Append(tail);
            }
            description = mBuilder.ToString();
        }
        return description;
    }

    /// <summary>
    /// Generates text descriptions of given moods.
    /// </summary>
    /// <param name="moodValues">
    /// Values for moods.
    /// </param>
    /// <returns>
    /// Text representation of personality mood(s).
    /// </returns>
    public string GenerateMoodDescriptions(int[] moodValues)
    {
        // Mood being described.
        PersonalityAspect mood;

        // Descriptions of read moods.
        string description;

        // End of line or description.
        string tail;

        // Iterator variable.
        int i;

        // Whether any mood descriptions have been used.
        bool useMood = false;

        mBuilder.Clear();
        mBuilder.Append($"{Defines.MOOD_HEAD} ");
        for (i = 0; i < moodValues.Length; i++)
        {
            if (moodValues[i] != 2)
            {
                useMood = true;
                mood = mMoods[i];
                description = $"{mMoodTerms[moodValues[i]]} {mood.Adjective}";
                mBuilder.Append(description);
                tail = i < moodValues.Length - 1 ?
                    Defines.LIST_TAIL : Defines.END_TAIL;
                mBuilder.Append(tail);
            }
        }
        description = useMood ? mBuilder.ToString() : "";
        return description;
    }

    /// <summary>
    /// Generates a summary of given personality values.
    /// </summary>
    /// <param name="traits">
    /// Values for personality traits.
    /// </param>
    /// <param name="moods">
    /// Values for personality moods.
    /// </param>
    /// <returns></returns>
    public string GenerateSummary(int[] traits, int[] moods)
    {
        // Mood being described.
        PersonalityAspect mood;

        // Trait to describe.
        Trait trait;

        // Value of personality aspect being recorded.
        int val;

        // Iterator. 
        int i;

        mBuilder.Clear();
        for (i=0; i < traits.Length; i++)
        {
            val = traits[i];
            if (val != 2)
            {
                trait = mTraits[i];
                mBuilder.Append($"{Defines.ACTION_HEAD}" +
                    $" {mDegreeTerms[val]} {trait.Adjective}.");
            }
        }
        for(i = 0; i < moods.Length; i++)
        {
            val = moods[i];
            if(val != 2)
            {
                mood = mMoods[i];
                mBuilder.Append($"{Defines.ACTION_HEAD}" +
                    $" {mDegreeTerms[val]} {mood.Adjective}.");
            }
        }
        return mBuilder.ToString();
    }

    /// <summary>
    /// Generates a personality description for a given character.
    /// </summary>
    /// <param name="traitValues">
    /// Openness, conscientiousness, extraversion, agreeableness, neuroticism,
    /// happiness, anger, sarcasm.
    /// </param>
    /// <param name="secrets">
    /// Array of topics this personality will avoid.
    /// </param>
    /// <param name="shirtSleeve">
    /// Array of topics this personality wants to talk about.
    /// </param>
    /// <returns>
    /// Text prompt representing a character's personality.
    /// </returns>
    public string[] GeneratePersonalityDescriptions(int[] traitValues,
        int[] moodValues, string[] secrets, string[] shirtSleeve)
    {
        // List of generated personality descriptions.
        List<string> personalityDescriptions = new List<string>();

        // Iterator variable.
        int i;

        for (i = 0; i < traitValues.Length; i++)
            personalityDescriptions.Add(
                GenerateTraitDescription(traitValues[i], i)
            );
        personalityDescriptions.Add(GenerateMoodDescriptions(moodValues));
        personalityDescriptions.Add(GenerateSecretsDescription(secrets));
        personalityDescriptions.Add(
            GenerateShirtSleeveDescription(shirtSleeve)
        );

        return personalityDescriptions.ToArray();
    }

    /// <summary>
    /// Returns the index of a given trait from its name.
    /// </summary>
    /// <param name="traitName">
    /// Name of trait you would like the index for.
    /// </param>
    /// <returns>
    /// Index of the named trait.
    /// </returns>
    public int TraitIndex(string traitName)
    {
        return mAspectIndicies[traitName];
    }
}
