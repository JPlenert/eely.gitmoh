using System.Linq;
using YamlDotNet.Core.Tokens;
using YamlDotNet.RepresentationModel;

namespace eely.gitmoh
{
    internal static class YamlExtentions
    {
        public static bool TryGetMappingStringValue(this YamlMappingNode node, string key, out string value)
        {
            value = node.GetMappingStringValueOrDefault(key);
            return value == null;
        }

        public static string GetMappingStringValueOrDefault(this YamlMappingNode node, string key)
        {
            YamlNode map = node.Children.FirstOrDefault(x => x.Key is YamlScalarNode a && a.Value == key).Value;
            if (map is YamlScalarNode b)
                 return b.Value;
            return null;
        }

        public static YamlMappingNode GetMappingOrDefault(this YamlMappingNode node, string key)
        {
            YamlNode map = node.Children.FirstOrDefault(x => x.Key is YamlScalarNode a && a.Value == key).Value;
            if (map is YamlMappingNode b)
                return b;
            return null;
        }


    }
}
