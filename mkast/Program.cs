using System;
using xayrga.banana;
using xayrga.utils;
using Be.IO;
using System.IO;
using System.Security.Cryptography;

namespace mkast
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG 
            args = new string[]
            {
                "justice_civ_loop.wav",
                "justice.ast",
                "-format",
                "adpcm4",
            };
#endif
            cmdarg.cmdargs = args;
            Console.WriteLine("mkast adpcm4 ast renderer.");
            //util.consoleProgress("Test", 50, 100);
            var wavFile = cmdarg.assertArg(0, "WAV File");
            if (!File.Exists(wavFile))
                cmdarg.assert("WAV file didn't exist.");
            FileStream vWave = null;

            try { vWave = File.OpenRead(wavFile); }
            catch (Exception e) { cmdarg.assert($"Failed to open WAV file ({e.Message})"); }

            var binReader = new BinaryReader(vWave);
            var wav = PCM16WAV.readStream(binReader);

            if (wav == null)
                cmdarg.assert("WAV File in invalid.");
            //if (wav.sampleRate > 32000)
                //cmdarg.assert($"WAV file has invalid sample rate ({wav.sampleRate}/32000hz max)");

            if (!cmdarg.findDynamicFlagArgument("-noinfo"))
                Console.WriteLine($"WAV {wav.bitsPerSample}bit PCM\nChannels: {wav.channels}\nBlockSize: {wav.sampleCount} samples ({wav.sampleCount / wav.channels} samples/channel)");


            var outPath = cmdarg.assertArg(1, "Output File");
            var format = cmdarg.findDynamicStringArgument("-format", "adpcm4");


            switch (format)
            {
                case "adpcm4":
                    encode.encodeAPCM4(wav, outPath);
                    break;
                default:
                    cmdarg.assert($"Unknown format '{format}'");
                    break;
            }

            /*
            Console.Write(wav.buffer.Length);
            var testPCM = new short[wav.sampleCount];
            var tpcm_l = 0;
            for (int i = 0; i < wav.sampleCount; i++)
            {
                for (int j = 0; j < wav.channels; j++)
                {
                    var pootis = wav.buffer[i * wav.channels + j];
                    if (j==0)
                    {
                        testPCM[tpcm_l] = pootis;
                        tpcm_l++;
                    }
                }
            }


            var ww = File.OpenWrite("tpcm.pcm");
            var bb = new BinaryWriter(ww);
            for (int i = 0; i < testPCM.Length; i++) {
                bb.Write(testPCM[i]);

            }
            bb.Flush();
            bb.Close();
             */  




        }
    }
}
