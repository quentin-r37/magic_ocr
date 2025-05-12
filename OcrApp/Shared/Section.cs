namespace OcrApp.Shared;

public class Section
{
    public string Title { get; set; } = "";
    public int Level { get; set; } = 1;
    public string Type { get; set; } = "section";
    public int Page { get; set; } = 1;
    public int DocIndex { get; set; } = 0;
}

