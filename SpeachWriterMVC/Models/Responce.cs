namespace SpeachWriterMVC.Models
{
    public class Responce
    {
        private DetectedLanguage detectedLanguage { get; set; }
        public Translation[] translations { get; set; }
    }
}
