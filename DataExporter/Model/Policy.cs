using System.ComponentModel.DataAnnotations.Schema;
namespace DataExporter.Model
{
    public class Policy
    {
        public int Id { get; set; }
        public string PolicyNumber { get; set; } = "unset";
        public decimal Premium { get; set; }
        public DateTime StartDate { get; set; }

        public List<Note> Notes { get; set; } = new List<Note>();
    }
}
