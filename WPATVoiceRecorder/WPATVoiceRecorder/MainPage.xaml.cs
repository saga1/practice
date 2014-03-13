using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework.Audio;

namespace WPATVoiceRecorder
{
    public partial class MainPage : PhoneApplicationPage
    {
        const String FILE_NAME = "WPATVoiceRecorder.wav";

        Microphone microphone;
        DynamicSoundEffectInstance playback;
        List<byte[]> bufferCollection = new List<byte[]>();

        enum STATE : int
        {
            INIT, RECORD, RECORDED, PLAY, STOP
        }
        STATE state = STATE.INIT;

        // コンストラクター
        public MainPage()
        {
            InitializeComponent();

            microphone = Microphone.Default;
            microphone.BufferReady += OnMicrophoneBufferReady;

            playback = new DynamicSoundEffectInstance(microphone.SampleRate, AudioChannels.Mono);
            playback.BufferNeeded += OnPlaybackBufferNeeded;
        }

        private void StateButton_Click(object sender, RoutedEventArgs e)
        {
            if (state == STATE.INIT)
            {
                StateButton.Content = "Stop";
                state = STATE.RECORD;

                if (microphone.State == MicrophoneState.Stopped)
                {
                    bufferCollection.Clear();
                    playback.Stop();
                    microphone.Start();
                }
            }
            else if (state == STATE.RECORD)
            {
                if (microphone.State == MicrophoneState.Started)
                {
                    StopRecording();
                }
            }
            else if (state == STATE.RECORDED)
            {
                StateButton.Content = "Stop";
                state = STATE.PLAY;

                playback.Stop();
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream stream = storage.OpenFile(FILE_NAME, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);
                        playback.SubmitBuffer(buffer);
                    }
                }

                playback.Play();
            }
            else if (state == STATE.PLAY)
            {
                StopPlaying();
            }
            else
            {
                throw new ArgumentException("invalid state");
            }
        }

        void StopRecording()
        {
            byte[] extraBuffer = new byte[microphone.GetSampleSizeInBytes(microphone.BufferDuration)];
            int extraBytes = microphone.GetData(extraBuffer);
            microphone.Stop();

            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = storage.CreateFile(FILE_NAME))
                {
                    foreach (byte[] buffer in bufferCollection)
                    {
                        stream.Write(buffer, 0, buffer.Length);
                    }
                    stream.Write(extraBuffer, 0, extraBytes);
                }
            }

            StateButton.Content = "Play";
            state = STATE.RECORDED;
        }

        void StopPlaying()
        {
            playback.Stop();

            StateButton.Content = "Record";
            state = STATE.INIT;
        }

        void OnMicrophoneBufferReady(object sender, EventArgs args)
        {
            byte[] buffer = new byte[microphone.GetSampleSizeInBytes(microphone.BufferDuration)];
            int size = microphone.GetData(buffer);
            bufferCollection.Add(buffer);
            if (bufferCollection.Count > 10)
            {
                StopRecording();
            }
        }

        void OnPlaybackBufferNeeded(object sender, EventArgs args)
        {
            if (playback.PendingBufferCount == 0)
            {
                StopPlaying();
            }
        }
    }
}