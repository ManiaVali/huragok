
using Huragok.Data.IntermediateFormats.Color;

namespace Huragok.Data.IntermediateFormats {
    /// <summary>
    /// Intermediate format representing a function curve as used by the Blam engine.
    /// </summary>
    internal sealed class IF_Function {
        /// <summary>
        /// Amount of graphs used in this function. Almost always 1.
        /// </summary>
        internal readonly int graphCount;
        /// <summary>
        /// Amount of colors used in this function.
        /// </summary>
        internal readonly int? colorCount;

        /// <summary>
        /// Master type of the function, such as Basic, Exponent, or Curve, etc.
        /// </summary>
        internal FunctionEditorMasterType masterType;
        /// <summary>
        /// The exponent of each graph in the function which is of master type exponent.
        /// </summary>
        internal List<float>? exponents;
        /// <summary>
        /// The minimum of each graph in the function which is of master type exponent.
        /// </summary>
        internal List<float>? mins;
        /// <summary>
        /// The maximum of each graph in the function which is of master type exponent.
        /// </summary>
        internal List<float>? maxes;
        /// <summary>
        /// The periodic function subtypes of every graph in the function which is of master type periodic.
        /// </summary>
        internal List<string>? periodicFuncTypes;
        /// <summary>
        /// The frequency of every graph in the function not of type basic.
        /// </summary>
        internal List<float>? frequencies;
        /// <summary>
        /// The phase of every graph in the function not of type basic.
        /// </summary>
        internal List<float>? phases;
        /// <summary>
        /// The colors used by the function.
        /// </summary>
        internal List<IF_Color>? colors;

        /// <summary>
        /// Constructs a new <see cref="IF_Function"/> from a function block.
        /// </summary>
        internal IF_Function(TagFieldCustomFunctionEditor func) {
            var editor = func.Value;

            this.graphCount = editor.GraphCount;
            var graphRange = Enumerable.Range(0, this.graphCount);

            this.colorCount = editor.ColorCount;
            var colorRange = Enumerable.Range(0, this.colorCount ?? 0);

            if (this.colorCount > 0) {
                this.colors = colorRange
                    .Select(editor.GetColor)
                    .Select(c => c.ReGamma())
                    .Select((c, i) => new IF_Color(
                        (int)Math.Round(Math.Clamp(c.Red, 0f, 1f) * 255),
                        (int)Math.Round(Math.Clamp(c.Green, 0f, 1f) * 255),
                        (int)Math.Round(Math.Clamp(c.Blue, 0f, 1f) * 255),
                        colormode: IF_ColorMode.PC))
                    .ToList();
            }

            this.masterType = editor.MasterType;
            switch (this.masterType) {
                case FunctionEditorMasterType.Curve:
                    throw new NotImplementedException($"Unimplemented function type: curve");

                case FunctionEditorMasterType.Exponent:
                    this.exponents = graphRange.Select(editor.GetExponent).ToList();
                    this.mins = graphRange.Select(editor.GetAmplitudeMin).ToList();
                    this.maxes = graphRange.Select(editor.GetAmplitudeMax).ToList();
                    break;

                case FunctionEditorMasterType.Periodic:
                    this.periodicFuncTypes = graphRange.Select(i => editor.GetPeriodicFunctionText(editor.GetFunctionIndex(i))).ToList();
                    this.frequencies = graphRange.Select(editor.GetFrequency).ToList();
                    this.phases = graphRange.Select(editor.GetPhase).ToList();
                    this.mins = graphRange.Select(editor.GetAmplitudeMin).ToList();
                    this.maxes = graphRange.Select(editor.GetAmplitudeMax).ToList();
                    break;

                case FunctionEditorMasterType.Transition:
                    throw new NotImplementedException($"Unimplemented function type: transition");
            }
        }
    }
}