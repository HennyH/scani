namespace Scani.Kiosk.Backends.GoogleSheet
{
    public class SheetWriteResult<T>
    {
        public T? Value { get; set; } = default;
        public ICollection<string> Errors { get; set; } = new List<string>();
        public ICollection<string> Warnings { get; set; } = new List<string>();
        public bool Ok => !Errors.Any();
    }
}
