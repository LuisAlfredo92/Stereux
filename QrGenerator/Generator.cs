using Net.Codecrete.QrCodeGenerator;
using System.Windows.Media;

namespace QrGenerator;

/// <summary>
/// The Qr codes generator.
/// </summary>
public static class Generator
{
    /// <summary>
    /// Generates the Qr to be showed in the Player.
    /// </summary>
    /// <param name="text">The text to be converted in Qr.</param>
    /// <returns>A DrawingImage that must be associated with the Image in the program.</returns>
    public static DrawingImage GenerateQr(string text)
        => QrCodeDrawing.CreateDrawing(QrCode.EncodeText(text, QrCode.Ecc.Low), 512);
}