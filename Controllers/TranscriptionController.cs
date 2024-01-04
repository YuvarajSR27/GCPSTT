using Google.Apis.Auth.OAuth2;
using Google.Protobuf;
using NAudio;
using Google.Cloud.Speech.V1;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Grpc.Auth;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Linq;

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
                //SpeechClientBuilder builder = new SpeechClientBuilder { CredentialsPath = environmentVariablePath };
                SpeechClientBuilder builder = new SpeechClientBuilder { ChannelCredentials = credentials.ToChannelCredentials() };
                SpeechClient speech = await builder.BuildAsync();

                RecognitionConfig config = new RecognitionConfig
                {
                    Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                    SampleRateHertz = 16000,
                    LanguageCode = LanguageCodes.English.UnitedStates,
                    AudioChannelCount = 1, // Set the number of channels in the audio (if applicable)
                    EnableSeparateRecognitionPerChannel = false, // Enable recognition for each audio channel separately
                    MaxAlternatives = 5, // Set the maximum number of recognition alternatives to return
                    ProfanityFilter = true, // Enable or disable profanity filtering
                    EnableWordTimeOffsets = true, // Enable word time offsets in the response
                    EnableWordConfidence = true, // Enable word confidence in the response
                    EnableAutomaticPunctuation = true, // Enable automatic punctuation detection
                    EnableSpokenPunctuation = true, // Enable spoken punctuation
                    EnableSpokenEmojis = true, // Enable spoken emojis
                    UseEnhanced = true// Use enhanced models for speech recognition
                };

                RecognitionAudio audio = RecognitionAudio.FromBytes(audioData);
                RecognizeResponse response = await speech.RecognizeAsync(config, audio);
               // Console.WriteLine($"Response received: {response}");

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
                //Console.WriteLine("gRPC Error: " + ex);
                return "Error transcribing audio: " + ex.Status.Detail;
            }
            catch (Exception ex)
            {
               // Console.WriteLine("Error: " + ex);
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
