namespace Huragok.Data.RuntimeFormats;

/// <summary>
/// <para>Available modes which colors can be represented in.</para>
/// <para><see cref="PC"/> represents Gamma, and <see cref="Xbox"/> is linear.</para>
/// </summary>
internal enum ColorMode {
    PC,
    Xbox
}

/// <summary>
/// A struct containing color information, including the RGB channels, hex code, and original color usage mode.
/// </summary>
internal readonly struct BlamColor {
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
    /// The original <see cref="ColorMode"/> of this color, in string format.
    /// </summary>
    internal readonly string colorMode;

    /// <summary>
    /// Constructs a new <see cref="BlamColor"/> from RGB components. 
    /// </summary>
    /// <param name="r">Red component. (0-255)</param>
    /// <param name="g">Green component. (0-255)</param>
    /// <param name="b">Blue component. (0-255)</param>
    /// <param name="a">Optional alpha component. (0-255)</param>
    /// <param name="colormode">The <see cref="ColorMode"/> this color is in.</param>
    internal BlamColor(int r, int g, int b, int? a = null, ColorMode colormode = ColorMode.PC) {
        this.red = r;
        this.green = g;
        this.blue = b;
        this.alpha = a;

        this.hexCode = $"#{r:X2}{g:X2}{b:X2}";
        this.colorMode = colormode.ToString();
    }
}