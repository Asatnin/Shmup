using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace Shmup
{
    static class SoundClass
    {
        // контекст аудиоустройства
        static AudioContext context;

        // звуки
        static List<int> buffers;

        // источник для фоновой музыки
        static int sourceForLoopMusic;

        // источники для звуковых эффектов
        static int[] soundSources;

        public static void prepare()
        {
            foreach (string device in AudioContext.AvailableDevices)
            {
                Console.WriteLine(device);
            }
            Console.WriteLine(AudioContext.DefaultDevice);
            try
            {
                context = new AudioContext();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            buffers = new List<int>();
            buffers.Add(addSound("Sound/shipExplosion.wav"));
            buffers.Add(addSound("Sound/bulletExplosion.wav"));
            buffers.Add(addSound("Sound/loopMusic.wav"));
            buffers.Add(addSound("Sound/select.wav"));

            sourceForLoopMusic = AL.GenSource();
            AL.Source(sourceForLoopMusic, ALSourcei.Buffer, buffers[2]);
            AL.Source(sourceForLoopMusic, ALSourceb.Looping, true);

            soundSources = new int[16];
            for (int i = 0; i < soundSources.Length; i++)
                soundSources[i] = AL.GenSource();
        }

        static int addSound(string filename)
        {
            SoundData sound = new SoundData();
            sound.data = loadWave(File.Open(filename, FileMode.Open), out sound.channels,
                out sound.bits_per_sample, out sound.sample_rate);
            sound.buffer = AL.GenBuffer();

            AL.BufferData(sound.buffer, GetSoundFormat(sound.channels, sound.bits_per_sample),
                sound.data, sound.data.Length, sound.sample_rate);

            return sound.buffer;
        }

        public static void playShipExplosion()
        {
            for (int i = 0; i < soundSources.Length; i++)
                if (AL.GetSourceState(soundSources[i]) != ALSourceState.Playing)
                {
                    AL.Source(soundSources[i], ALSourcei.Buffer, buffers[0]);
                    AL.SourcePlay(soundSources[i]);
                    break;
                }
        }

        public static void playBulletExplosion()
        {
            for (int i = 0; i < soundSources.Length; i++)
                if (AL.GetSourceState(soundSources[i]) != ALSourceState.Playing)
                {
                    AL.Source(soundSources[i], ALSourcei.Buffer, buffers[1]);
                    AL.SourcePlay(soundSources[i]);
                    break;
                }
        }

        public static void playBonusSelect()
        {
            for (int i = 0; i < soundSources.Length; i++)
                if (AL.GetSourceState(soundSources[i]) != ALSourceState.Playing)
                {
                    AL.Source(soundSources[i], ALSourcei.Buffer, buffers[3]);
                    AL.SourcePlay(soundSources[i]);
                    break;
                }
        }

        public static void startLoopMusic()
        {
            AL.SourcePlay(sourceForLoopMusic);
        }

        public static void stopLoopMusic()
        {
            AL.SourceStop(sourceForLoopMusic);
        }

        public static void pauseAllSounds()
        {
            AL.SourcePause(sourceForLoopMusic);
            for (int i = 0; i < soundSources.Length; i++)
                if (AL.GetSourceState(soundSources[i]) == ALSourceState.Playing)
                    AL.SourcePause(soundSources[i]);
        }

        public static void resumeAllSounds()
        {
            AL.SourcePlay(sourceForLoopMusic);
            for (int i = 0; i < soundSources.Length; i++)
                if (AL.GetSourceState(soundSources[i]) == ALSourceState.Paused)
                    AL.SourcePlay(soundSources[i]);
        }

        public static void dispose()
        {
            AL.DeleteSource(sourceForLoopMusic);
            for (int i = 0; i < soundSources.Length; i++)
                AL.DeleteSource(soundSources[i]);
        }

        public static byte[] loadWave(Stream stream, out int channels, out int bits,
            out int rate)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                // RIFF header
                string signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    Console.WriteLine("Stream is not a wave file.");

                int riff_chunck_size = reader.ReadInt32();

                string format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    Console.WriteLine("Stream is not a wave file.");

                // WAVE HEADER
                string format_signature = new string(reader.ReadChars(4));
                if (format_signature != "fmt ")
                    Console.WriteLine("Specified wave file is not supported.");

                int format_chunk_size = reader.ReadInt32();
                int audio_format = reader.ReadInt16();
                int num_channels = reader.ReadInt16();
                int sample_rate = reader.ReadInt32();
                int byte_rate = reader.ReadInt32();
                int block_align = reader.ReadInt16();
                int bits_per_sample = reader.ReadInt16();

                string data_signature = new string(reader.ReadChars(4));
                if (data_signature != "data" && data_signature != "LIST")
                    Console.WriteLine("Specified wave file is not supported.");

                int data_chunk_size = reader.ReadInt32();

                channels = num_channels;
                bits = bits_per_sample;
                rate = sample_rate;

                return reader.ReadBytes((int)reader.BaseStream.Length);
            }
        }

        static ALFormat GetSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
        }
    }
}