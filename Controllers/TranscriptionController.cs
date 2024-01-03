using Google.Apis.Auth.OAuth2;
using Google.Protobuf;
using Google.Cloud.Speech.V1;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;

namespace TranscriptionAPI.Controllers
{
    [Route("api/transcribe")]
    [ApiController]
    public class TranscriptionController : ControllerBase
    {
        private async Task<string> TranscribeSpeechAsync(byte[] audioData)
        {
            try
            {
                string? environmentVariablePath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
                              
                GoogleCredential credentials = GoogleCredential.FromFile(environmentVariablePath);
                SpeechClientBuilder builder = new SpeechClientBuilder { CredentialsPath = environmentVariablePath };
                SpeechClient speech = await builder.BuildAsync();

                RecognitionConfig config = new RecognitionConfig
                {
                    Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                    SampleRateHertz = 16000,
                    LanguageCode = LanguageCodes.English.UnitedStates
                };

                RecognitionAudio audio = RecognitionAudio.FromBytes(audioData);
                RecognizeResponse response = await speech.RecognizeAsync(config, audio);

                string transcript = "";

                //foreach (var result in response.Results)
                //{
                //    foreach (var alternative in result.Alternatives)
                //    {
                //        transcript += alternative.Transcript + "\n";
                //    }
                //}

                if (response != null && response.Results != null && response.Results.Count > 0)
                {
                    foreach (var result in response.Results)
                    {
                        if (result.Alternatives != null && result.Alternatives.Any())
                        {
                            foreach (var alternative in result.Alternatives)
                            {
                                transcript += alternative.Transcript + "\n";
                            }
                        }
                        else
                        {
                            transcript += "No transcription available.";
                        }
                    }

                }

                return transcript;

            }
            catch (RpcException ex)
            {
                return "Error transcribing audio: " + ex.Status.Detail;
            }
            catch (Exception ex)
            {
                return "Error transcribing audio: " + ex.Message;
            }
       
        }

        [HttpPost]
        public async Task<IActionResult> PostTranscription()
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    await Request.Body.CopyToAsync(ms);
                    byte[] audioData = ms.ToArray();

                    if (audioData == null || audioData.Length == 0)
                    {
                        return BadRequest("No audio data received.");
                    }

                    string transcript = await TranscribeSpeechAsync(audioData);
                    return Ok(transcript);
                }

            }

            catch (Exception ex)
            {
                return BadRequest("Error transcribing audio: " + ex.Message);
            }
        }


    }
}
