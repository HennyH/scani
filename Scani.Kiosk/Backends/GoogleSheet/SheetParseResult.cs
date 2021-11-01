namespace Scani.Kiosk.Backends.GoogleSheet
{
    public class SheetParseResult<T>
    {
        public ICollection<T> Values { get; set; } = new List<T>();
        public ICollection<string> Errors { get; set; } = new List<string>();
        public ICollection<string> Warnings { get; set; } = new List<string>();
    }
}
