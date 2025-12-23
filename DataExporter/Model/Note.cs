namespace DataExporter.Model
{
    public class Note
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int PolicyId { get; set; }

        public Note() { }               // EF needs this
        public Note(string text) => Text = text ?? throw new ArgumentNullException(nameof(text));
        
    }
}
