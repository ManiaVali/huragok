using System.CommandLine;

namespace Huragok.Application.Commands.Query;

internal static class FieldArguments {
    internal static readonly Argument<string> FieldArgument =
        new(name: "field-path") {
            Arity = ArgumentArity.ExactlyOne,
            Description = "Full path to the field to be selected."
        };
}