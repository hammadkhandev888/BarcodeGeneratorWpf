namespace BarcodeGenerator.Helpers
{
    public static class ZplAlignmentHelper
    {
        public static string GetZplAlignment(BarcodeGenerator.Models.LabelTextAlignment alignment)
        {
            return alignment switch
            {
                BarcodeGenerator.Models.LabelTextAlignment.Left => "L",
                BarcodeGenerator.Models.LabelTextAlignment.Center => "C",
                BarcodeGenerator.Models.LabelTextAlignment.Right => "R",
                _ => "C"
            };
        }

        public static int CalculateZplTextX(int labelWidthDots, int barcodeWidthDots, 
            BarcodeGenerator.Models.LabelTextAlignment alignment)
        {
            return alignment switch
            {
                BarcodeGenerator.Models.LabelTextAlignment.Left => 10,
                BarcodeGenerator.Models.LabelTextAlignment.Right => labelWidthDots - barcodeWidthDots - 10,
                BarcodeGenerator.Models.LabelTextAlignment.Center => (labelWidthDots - barcodeWidthDots) / 2,
                _ => (labelWidthDots - barcodeWidthDots) / 2
            };
        }
    }
}
