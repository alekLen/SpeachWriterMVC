using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using SpeachWriterMVC.Models;

namespace SpeachWriterMVC.Controllers
{
    public class SpeachController : Controller
    {
        async public Task<IActionResult> ToWrite()
        {
            var speechConfig = SpeechConfig.FromSubscription("ac8923a842584dbfa1ff8613106ee5dd", "eastus");
            speechConfig.SpeechRecognitionLanguage = "ru-RUS";

            using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
           
            var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
            MyText model=new MyText();
           model.text= OutputSpeechRecognitionResult(speechRecognitionResult);
            return View(model);
        }

        static string OutputSpeechRecognitionResult(SpeechRecognitionResult speechRecognitionResult)
        {

            switch (speechRecognitionResult.Reason)
            {
                case ResultReason.RecognizedSpeech:
                    return speechRecognitionResult.Text;
                    break;
                case ResultReason.NoMatch:
                    return "Речь не распознана.";
                    break;
                case ResultReason.Canceled:
                    var cancellation = CancellationDetails.FromResult(speechRecognitionResult);
                    return cancellation.ErrorDetails.ToString();
                   
            }
            return "Ошибка";
        }
    }
}
