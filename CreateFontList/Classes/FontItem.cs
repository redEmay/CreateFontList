using System.Drawing;

namespace CreateFontList.Classes
{
    public class FontItem
    {
        private string directoryLocation { get; set; }
        private string fontName { get; set; }
        //private Bitmap bitMap { get; set; }
        private string bitMap { get; set; }


        public FontItem(string location, string name, string map)
        {
            directoryLocation = location;
            fontName = name;
            bitMap = map;
        }

        public string getDirectoryLocation() { return directoryLocation; }
        public string getFontName() { return fontName; }
        public string getBitMap() { return bitMap; }
    }
}
