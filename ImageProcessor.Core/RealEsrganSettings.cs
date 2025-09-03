namespace ImageProcessor.Core
{
    public class RealEsrganSettings
    {
        public string CommandArguments { get; set; } = "-i {inputFile} -o {outputFile} -n {modelName} -s 4 -f png -m {modelsPath}";
    }
}