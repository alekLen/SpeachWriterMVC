
using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;

namespace SpeachWriterMVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AudioController : Controller
    {
        private void ConvertToPcm16Le(string inputFilePath, string outputFilePath)
        {
            using (var reader = new AudioFileReader(inputFilePath))
            {
                var format = new WaveFormat(16000, 16, 1);
                using (var resampler = new MediaFoundationResampler(reader, format))
                {
                    WaveFileWriter.CreateWaveFile(outputFilePath, resampler);
                }
            }
        }

        private bool IsWavFile(string filePath)
        {
            using (var reader = new BinaryReader(new FileStream(filePath, FileMode.Open)))
            {
                var riff = new string(reader.ReadChars(4));
                var chunkSize = reader.ReadInt32();
                var wave = new string(reader.ReadChars(4));

                return riff == "RIFF" && wave == "WAVE";
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadAudio()
        {
            var speechConfig = SpeechConfig.FromSubscription("ac8923a842584dbfa1ff8613106ee5dd", "eastus");
            speechConfig.SpeechRecognitionLanguage = "ru-RUS";

            try
            {
                var audio = Request.Form.Files["audio"];
                if (audio != null)
                {
                    var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    var filePath = Path.Combine(uploadsPath, "recording.wav");
                    var convertedFilePath = Path.Combine(uploadsPath, "recording_converted.wav");

                    try
                    {
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await audio.CopyToAsync(stream);
                        }

                        if (!System.IO.File.Exists(filePath))
                        {
                            throw new FileNotFoundException($"File {filePath} was not found after copying.");
                        }

                        var fileInfo = new FileInfo(filePath);
                        if (fileInfo.Length == 0)
                        {
                            throw new Exception($"File {filePath} is empty after copying.");
                        }

                        if (!IsWavFile(filePath))
                        {
                            throw new Exception($"File {filePath} is not a valid WAV file.");
                        }

                        // Конвертируем файл в требуемый формат
                        ConvertToPcm16Le(filePath, convertedFilePath);

                        try
                        {
                            using var audioConfig = AudioConfig.FromWavFileInput(convertedFilePath);
                            using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
                            var result = await speechRecognizer.RecognizeOnceAsync();

                            return Ok(result.Text);
                        }
                        catch (ApplicationException ex)
                        {
                            Console.WriteLine($"Error processing audio file: {ex.Message}");
                            return StatusCode(500, $"Internal server error during audio processing: {ex.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error writing file to {filePath} or processing audio: {ex.Message}");
                        return StatusCode(500, $"Internal server error: {ex.Message}");
                    }
                    finally
                    {
                        // Удаление файлов после обработки
                        try
                        {
                            if (filePath != null && System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                                Console.WriteLine($"Deleted file: {filePath}");
                            }

                            if (convertedFilePath != null && System.IO.File.Exists(convertedFilePath))
                            {
                                System.IO.File.Delete(convertedFilePath);
                                Console.WriteLine($"Deleted file: {convertedFilePath}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error deleting files: {ex.Message}");
                        }
                    }
                }

                return BadRequest("No audio file uploaded.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("upload")]
        public IActionResult UploadForm()
        {
            return View();
        }
    }
}
