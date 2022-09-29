using Net.Codecrete.QrCodeGenerator;
using System.Windows.Media;

namespace QrGenerator
{
    public static class Generator
    {
        public static DrawingImage GenerateQr(string text) =>
            QrCodeDrawing.CreateDrawing(QrCode.EncodeText(text,
                    QrCode.Ecc.Low),
                512);
    }
}