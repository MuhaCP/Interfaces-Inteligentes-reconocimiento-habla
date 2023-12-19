using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HuggingFace.API.Examples {
    public class SpeechRecognitionExample : MonoBehaviour {

        private AudioClip clip;
        private byte[] bytes;
        private bool recording;

        private void Update() {
            if (recording && Microphone.GetPosition(null) >= clip.samples) {
                StopRecording();
            }
            if (Input.GetKeyDown(KeyCode.Space)) {
                if (recording) {
                    StopRecording();
                } else {
                    StartRecording();
                }
            }
        }

        private void StartRecording() {
            Debug.Log("Start recording");
            clip = Microphone.Start(null, false, 10, 44100);
            recording = true;
        }


        private void StopRecording() {
            Debug.Log("Stop recording");
            var position = Microphone.GetPosition(null);
            Microphone.End(null);
            var samples = new float[position * clip.channels];
            clip.GetData(samples, 0);
            bytes = EncodeAsWAV(samples, clip.frequency, clip.channels);
            recording = false;
            SendRecording();
        }

        private void SendRecording() {
            HuggingFaceAPI.AutomaticSpeechRecognition(bytes, response => {
                Debug.Log(response);
                if (response.Contains("jump") || response.Contains("Salta") || response.Contains("salta") || response.Contains("Jump")) {
                Debug.Log("Jump");
                GetComponent<Rigidbody>().AddForce(Vector3.up * 10, ForceMode.Impulse);
            } else if (response.Contains("move") || response.Contains("Move") || response.Contains("Mueve")) {
                Debug.Log("Move");
                GetComponent<Rigidbody>().AddForce(Vector3.right * 10, ForceMode.Impulse);
            } else {
                Debug.Log("No action");
            }
            }, error => {
                Debug.LogError(error);
            });
        }

        private byte[] EncodeAsWAV(float[] samples, int frequency, int channels) {
            using (var memoryStream = new MemoryStream(44 + samples.Length * 2)) {
                using (var writer = new BinaryWriter(memoryStream)) {
                    writer.Write("RIFF".ToCharArray());
                    writer.Write(36 + samples.Length * 2);
                    writer.Write("WAVE".ToCharArray());
                    writer.Write("fmt ".ToCharArray());
                    writer.Write(16);
                    writer.Write((ushort)1);
                    writer.Write((ushort)channels);
                    writer.Write(frequency);
                    writer.Write(frequency * channels * 2);
                    writer.Write((ushort)(channels * 2));
                    writer.Write((ushort)16);
                    writer.Write("data".ToCharArray());
                    writer.Write(samples.Length * 2);

                    foreach (var sample in samples) {
                        writer.Write((short)(sample * short.MaxValue));
                    }
                }
                return memoryStream.ToArray();
            }
        }
    }
}