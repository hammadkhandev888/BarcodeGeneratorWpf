using System;
using System.IO;
using BarcodeGenerator.Models;
using BarcodeGenerator.Helpers;

namespace BarcodeGenerator.Tests
{
    public class ZplAlignmentTest
    {
        public static void RunTest()
        {
            Console.WriteLine("=== ZPL ALIGNMENT TEST ===\n");
            
            var barcodeData = new BarcodeData
            {
                Value = "TEST123456",
                Data = "ROW-FREEZER-60F-SOEJE-W",
                Description = "SS 2DOOR UPRIGHT FRE",
                Copies = 1
            };

            var labelSettings = new LabelSettings
            {
                LabelWidth = 100.0,
                LabelHeight = 50.0,
                BarcodeWidth = 80.0,
                BarcodeHeight = 20.0,
                LabelFontSize = 15,
                DescriptionFontSize = 18,
                LabelTextAlignment = LabelTextAlignment.Center,
                DescriptionTextAlignment = LabelTextAlignment.Center
            };

            int printerDpi = 203;
            
            int labelWidthDots = ZplCommandGenerator.ConvertMmToDots(labelSettings.LabelWidth, printerDpi);
            int labelHeightDots = ZplCommandGenerator.ConvertMmToDots(labelSettings.LabelHeight, printerDpi);
            int barcodeWidthDots = ZplCommandGenerator.ConvertMmToDots(labelSettings.BarcodeWidth, printerDpi);
            int barcodeHeightDots = ZplCommandGenerator.ConvertMmToDots(labelSettings.BarcodeHeight, printerDpi);

            Console.WriteLine("LABEL DIMENSIONS:");
            Console.WriteLine($"  Label Width: {labelSettings.LabelWidth}mm = {labelWidthDots} dots");
            Console.WriteLine($"  Label Height: {labelSettings.LabelHeight}mm = {labelHeightDots} dots");
            Console.WriteLine($"  Barcode Width: {labelSettings.BarcodeWidth}mm = {barcodeWidthDots} dots");
            Console.WriteLine($"  Barcode Height: {labelSettings.BarcodeHeight}mm = {barcodeHeightDots} dots\n");

            var barcodePosition = ZplCommandGenerator.CalculateBarcodePosition(labelSettings, printerDpi);
            Console.WriteLine("BARCODE POSITION:");
            Console.WriteLine($"  X: {barcodePosition.X} dots");
            Console.WriteLine($"  Y: {barcodePosition.Y} dots");
            Console.WriteLine($"  Calculation: X = ({labelWidthDots} - {barcodeWidthDots}) / 2 = {barcodePosition.X}\n");

            string labelAlignment = ZplAlignmentHelper.GetZplAlignment(labelSettings.LabelTextAlignment);
            int labelTextX = ZplAlignmentHelper.CalculateZplTextX(labelWidthDots, barcodeWidthDots, labelSettings.LabelTextAlignment);
            
            Console.WriteLine("LABEL TEXT POSITION:");
            Console.WriteLine($"  Alignment: {labelSettings.LabelTextAlignment} -> ZPL: {labelAlignment}");
            Console.WriteLine($"  X: {labelTextX} dots");
            Console.WriteLine($"  Calculation: X = ({labelWidthDots} - {barcodeWidthDots}) / 2 = {labelTextX}");
            Console.WriteLine($"  Block Width: {barcodeWidthDots} dots");
            Console.WriteLine($"  Text: \"{barcodeData.Data}\"\n");

            string descAlignment = ZplAlignmentHelper.GetZplAlignment(labelSettings.DescriptionTextAlignment);
            int descTextX = ZplAlignmentHelper.CalculateZplTextX(labelWidthDots, barcodeWidthDots, labelSettings.DescriptionTextAlignment);
            
            Console.WriteLine("DESCRIPTION TEXT POSITION:");
            Console.WriteLine($"  Alignment: {labelSettings.DescriptionTextAlignment} -> ZPL: {descAlignment}");
            Console.WriteLine($"  X: {descTextX} dots");
            Console.WriteLine($"  Calculation: X = ({labelWidthDots} - {barcodeWidthDots}) / 2 = {descTextX}");
            Console.WriteLine($"  Block Width: {barcodeWidthDots} dots");
            Console.WriteLine($"  Text: \"{barcodeData.Description}\"\n");

            Console.WriteLine("VERIFICATION:");
            Console.WriteLine($"  ✓ Barcode X position: {barcodePosition.X}");
            Console.WriteLine($"  ✓ Label text X position: {labelTextX}");
            Console.WriteLine($"  ✓ Description text X position: {descTextX}");
            
            if (barcodePosition.X == labelTextX && labelTextX == descTextX)
            {
                Console.WriteLine("  ✅ ALL ELEMENTS ALIGNED AT SAME X POSITION - PERFECTLY CENTERED!\n");
            }
            else
            {
                Console.WriteLine("  ⚠️ WARNING: Elements have different X positions!\n");
            }

            string zplCommand = ZplCommandGenerator.GenerateLabelZpl(barcodeData, labelSettings, printerDpi);
            
            Console.WriteLine("GENERATED ZPL COMMAND:");
            Console.WriteLine("====================");
            Console.WriteLine(zplCommand);
            Console.WriteLine("====================\n");

            string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test_label.zpl");
            File.WriteAllText(outputPath, zplCommand);
            Console.WriteLine($"ZPL saved to: {outputPath}");
            
            Console.WriteLine("\nKEY ZPL COMMANDS TO VERIFY:");
            Console.WriteLine($"  ^PW{labelWidthDots}  <- Print Width");
            Console.WriteLine($"  ^FO{barcodePosition.X},{barcodePosition.Y}  <- Barcode Origin");
            Console.WriteLine($"  ^FO{labelTextX},<Y>  <- Label Text Origin");
            Console.WriteLine($"  ^FB{barcodeWidthDots},2,0,{labelAlignment},0  <- Label Text Block (Centered)");
            Console.WriteLine($"  ^FO{descTextX},<Y>  <- Description Origin");
            Console.WriteLine($"  ^FB{barcodeWidthDots},2,0,{descAlignment},0  <- Description Block (Centered)");
            
            Console.WriteLine("\n=== TEST COMPLETE ===");
        }
    }
}
