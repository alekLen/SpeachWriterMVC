using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using SpeachWriterMVC.Models;

namespace SpeachWriterMVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpeachController : Controller
    {
        [HttpPost("upload")]
        async public Task<IActionResult> ToWrite()
        {
            MyText model = new MyText();

            //try
            //{
            //    var speechConfig = SpeechConfig.FromSubscription("ac8923a842584dbfa1ff8613106ee5dd", "eastus");
            //    speechConfig.SpeechRecognitionLanguage = "ru-RUS";

            //    using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            //    using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

            //    var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
            //    model.text = OutputSpeechRecognitionResult(speechRecognitionResult);
            //}
            //catch(Exception ex) { model.text = ex.Message; }

            var speechConfig = SpeechConfig.FromSubscription("ac8923a842584dbfa1ff8613106ee5dd", "eastus");
             speechConfig.SpeechRecognitionLanguage = "ru-RUS";
            try {  
            var audio = Request.Form.Files["audio"];
            if (audio != null)
            {
                var filePath = Path.Combine("Uploads", "recording.wav");

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await audio.CopyToAsync(stream);
                }

            }
            using var audioConfig = AudioConfig.FromWavFileInput("recording.wav");
            using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
            var result = await speechRecognizer.RecognizeOnceAsync();
            model.text=result.Text;
             }
             catch(Exception ex) { model.text = ex.Message; }
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
