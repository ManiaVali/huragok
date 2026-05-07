using System.Text.Json.Serialization;
using Bungie.Game;

namespace Huragok.Data.IntermediateFormats.Color {
    /// <summary>
    /// <para>Available modes which colors can be represented in.</para>
    /// <para><see cref="PC"/> represents Gamma, and <see cref="Xbox"/> is linear.</para>
    /// </summary>
    internal enum IF_ColorMode {
        PC,
        Xbox
    }

    /// <summary>
    /// A struct containing color information, including the RGB channels, hex code, and original color usage mode.
    /// </summary>
    internal readonly struct IF_Color {
        /// <summary>
        /// Value of the red channel, spanning from 0 to 255.
        /// </summary>
        public readonly int red;
        /// <summary>
        /// Value of the green channel, spanning from 0 to 255.
        /// </summary>
        public readonly int green;
        /// <summary>
        /// Value of the blue channel, spanning from 0 to 255.
        /// </summary>
        public readonly int blue;
        /// <summary>
        /// Optional value for the alpha channel, spanning from 0 to 255. If empty, color is assumed to be RGB only.
        /// </summary>
        public readonly int? alpha;
        /// <summary>
        /// Hexadecimal representation of this color. #RRGGBB
        /// </summary>
        public readonly string hexCode;
        /// <summary>
        /// The original <see cref="IF_ColorMode"/> of this color, in string format.
        /// </summary>
        internal readonly string colorMode;

        /// <summary>
        /// Constructs a new <see cref="IF_Color"/> from RGB components. 
        /// </summary>
        /// <param name="r">Red component. (0-255)</param>
        /// <param name="g">Green component. (0-255)</param>
        /// <param name="b">Blue component. (0-255)</param>
        /// <param name="a">Optional alpha component. (0-255)</param>
        /// <param name="colormode">The <see cref="IF_ColorMode"/> this color is in.</param>
        internal IF_Color(int r, int g, int b, int? a = null, IF_ColorMode colormode = IF_ColorMode.PC) {
            this.red = r;
            this.green = g;
            this.blue = b;
            this.alpha = a;

            this.hexCode = $"#{r:X2}{g:X2}{b:X2}";
            this.colorMode = colormode.ToString();
        }
    }
}