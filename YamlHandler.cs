using System.Collections.Generic;
using System.IO;
using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using BepInEx;

namespace BossDirections;

public static class YamlLoader
{
    // T should match the root of your YAML (OfferingsYaml here).
    public static T LoadYamlFile<T>(string fileName) where T : class
    {
        string fullPath = Path.Combine(Paths.ConfigPath, fileName);
        if (!File.Exists(fullPath))
        {
            WriteConfigFileFromResource(fullPath);
            BossDirectionsPlugin.BossDirectionsLogger.LogError($"YAML file not found: {fullPath}, generating from resource.");
        }

        try
        {
            string yaml = File.ReadAllText(fullPath);
            return DeserializeFromString<T>(yaml);
        }
        catch (System.Exception ex)
        {
            BossDirectionsPlugin.BossDirectionsLogger.LogError($"Failed to parse YAML: {ex}");
            return null!;
        }
    }

    public static T DeserializeFromString<T>(string yaml) where T : class
    {
        if (string.IsNullOrWhiteSpace(yaml))
        {
            BossDirectionsPlugin.BossDirectionsLogger.LogError($"YAML file or content is empty");
        }

        IDeserializer deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();
        return deserializer.Deserialize<T>(yaml)!;
    }

    private static void WriteConfigFileFromResource(string configFilePath)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string resourceName = BossDirectionsPlugin.OfferingsFileName.Replace("Azumatt.", "");

        using Stream resourceStream = assembly.GetManifestResourceStream(resourceName);
        if (resourceStream == null)
        {
            throw new FileNotFoundException($"Resource '{resourceName}' not found in the assembly.");
        }

        using StreamReader reader = new StreamReader(resourceStream);
        string contents = reader.ReadToEnd();

        File.WriteAllText(configFilePath, contents);
    }
}

public class OfferingsYaml
{
    public List<Offering> offerings { get; set; } = null!;
}

public class Offering
{
    public string name { get; set; } = null!;
    public string location { get; set; } = null!;
    public bool addname { get; set; }
    public List<string> quotes { get; set; } = null!;
    public Dictionary<string, int> items { get; set; } = null!;
}