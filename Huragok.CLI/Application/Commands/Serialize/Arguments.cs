
using System.CommandLine;
using Huragok.Data.Serialization;

namespace Huragok.Application.Commands.Serialize;

internal static class SerializeArguments {

    /// <summary>
    /// <para>Serialization language to be used. One of `json` or `yaml`.</para>
    /// <para>JSON is default.</para>
    /// </summary>
    internal static readonly Option<string> SerializerFormat =
        new(name: "--serialization-format", aliases: ["-s"]) {
            Arity = ArgumentArity.ExactlyOne,
            HelpName = "json or yaml",
            DefaultValueFactory = _ => SerializationFormat.JSON.ToString().ToLower(),
            Description = "Serialization language to be used."
        };

}