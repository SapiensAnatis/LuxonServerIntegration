using System.Globalization;
using System.Text.Json;
using Luxon.Ser;
using Microsoft.Extensions.Configuration;
using SharpYaml;
using SharpYaml.Events;
using SharpYaml.Model;
using SharpYaml.Serialization;

namespace Luxon.Server.Host.Configuration;

internal sealed class YamlConfigurationProvider(string filePath) : ConfigurationProvider
{
    public override void Load()
    {
        using var fs = File.OpenRead(filePath);
        using var sr = new StreamReader(fs);

        var stream = YamlStream.Load(sr);
        if (stream is not [{ } yamlDocument])
        {
            throw new InvalidOperationException("Expected to receive a single YamlDocument");
        }

        Visit(yamlDocument.Contents, new List<string>(), this.Data);
    }

    private static void Visit(
        YamlNode? node,
        List<string> pathSegments,
        IDictionary<string, string?> data
    )
    {
        switch (node)
        {
            case YamlMapping map:
            {
                foreach ((YamlElement key, YamlElement? value) in map)
                {
                    if (key is not YamlValue scalar)
                    {
                        throw new InvalidOperationException("Mapping key was not a scalar");
                    }

                    pathSegments.Add(scalar.Value);
                    Visit(value, pathSegments, data);
                    pathSegments.Pop();
                }

                break;
            }
            case YamlSequence sequence:
            {
                for (int i = 0; i < sequence.Count; i++)
                {
                    pathSegments.Add(i.ToString(CultureInfo.InvariantCulture));
                    Visit(sequence[i], pathSegments, data);
                    pathSegments.Pop();
                }

                break;
            }
            case null:
            {
                var path = ConfigurationPath.Combine(pathSegments);
                data.Add(path, null);
                break;
            }
            case YamlValue scalar:
            {
                var path = ConfigurationPath.Combine(pathSegments);
                data.Add(path, scalar.Value);
                break;
            }
        }
    }
}

/// <summary>
/// Extensions to make a List behave more like a stack. We don't use Stack directly because it needs to be
/// reversed before calling ConfigurationPath.Combine.
/// </summary>
internal static class ListStackExtensions
{
    extension<T>(List<T> list)
    {
        public void Pop()
        {
            list.RemoveAt(list.Count - 1);
        }
    }
}
