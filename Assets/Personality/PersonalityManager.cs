using UnityEngine;

/// <summary>
/// Manages personality strings for Characters in scene.
/// </summary>
public class PersonalityManager : MonoBehaviour
{
    /// <summary>
    /// Generates personality strings. 
    /// </summary>
    PersonalityGenerator mGenerator = new PersonalityGenerator();

    /// <summary>
    /// Generates a new personality description based on parameterized values.
    /// </summary>
    /// <param name="character">
    /// Character this personality is intended for.
    /// </param>
    /// <returns>
    /// Text description of a given parameterized personality.
    /// </returns>
    public string[] GenerateNewPersonality(Personality character)
    {
        // Integer values representing big five personality traits.
        int[] traits = 
        { 
            character.Openness, 
            character.Conscientiousness,
            character.Extraversion,
            character.Agreeableness, 
            character.Neroticsm
        };

        // Integer values representing defined moods.
        int[] moods =
        {
            character.Happiness,
            character.Anger,
            character.Sarcasm
        };

        // Secrets the personality wants to keep.
        string[] secrets = character.Secrets;

        // Things the personality wants to talk about.
        string[] shirtSleeve = character.ShirtSleeve;
        
        // Generated personality strings.
        string[] personality = 
            mGenerator.GeneratePersonalityDescriptions(
                traits, moods, secrets, shirtSleeve
            );

        return personality;
    }

    /// <summary>
    /// Generates a new description of a given trait based on a new value
    /// given for that trait. In the case a given character changes its 
    /// personality somehow.
    /// </summary>
    /// <param name="traitIndex">
    /// Index of trait in prompt generator trait array. Openness = 0, 
    /// Conscientiousness = 1, Extraversion = 2, Agreeableness = 3,
    /// Neruoticsm = 4, Happiness = 5, Anger = 6.
    /// </param>
    /// <param name="newValue">
    /// Value to generate trait description with.
    /// </param>
    /// <returns>
    /// Description of trait based on given value.
    /// </returns>
    public string GenerateNewTrait(string traitName, int newValue)
    {
        int traitIndex = mGenerator.TraitIndex(traitName);
        string description = 
            mGenerator.GenerateTraitDescription(newValue, traitIndex);
        return description;
    }

    /// <summary>
    /// Generates a new secrets description for a character.
    /// </summary>
    /// <param name="newSecrets">
    /// New array of secrets to generate a description of.
    /// </param>
    /// <returns>
    /// Text description of array of secrets.
    /// </returns>
    public string GenerateNewSecrets(string[] newSecrets)
    {
        return mGenerator.GenerateSecretsDescription(newSecrets);
    }

    /// <summary>
    /// Generates a text description from new list of shirt sleeve facts/ideas 
    /// for a personality.
    /// </summary>
    /// <param name="newShirtSleeve">
    /// New list of shirt sleeve facts.
    /// </param>
    /// <returns>
    /// Text description of shirt sleeve facts.
    /// </returns>
    public string GenerateNewShirtSleeve(string[] newShirtSleeve)
    {
        return mGenerator.GenerateShirtSleeveDescription(newShirtSleeve);
    }
}
