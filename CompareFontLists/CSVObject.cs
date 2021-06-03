namespace CompareFontLists
{
    public class CSVObject
    {
        public string fontName { get; set; }
        public string fileLocation { get; set; }
        public CSVObject(string location, string name)
        {
            fileLocation = location;
            fontName = name;
        }
    }
}
