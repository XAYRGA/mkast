using Be.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using wsystool;
using xayrga.banana;
using xayrga.utils;

namespace mkast
{
    public static partial class encode
    {
        private const int STRM_HEAD = 0x5354524D;
        private const int STRM_LENGTH = 0x00002760;
        private const int BLCK_HEAD = 0x424C434B;
        public static void encodeAPCM4(PCM16WAV wav,string fileOutput)
        {
            int[] last = new int[6];
            int[] penult = new int[6];

            FileStream fOutput = null;
            try
            {
                fOutput = File.Open(fileOutput, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            } catch (Exception E) { cmdarg.assert($"Failed to create {fileOutput} for writing. ({E.Message})"); }

            if (fOutput == null)
                cmdarg.assert("How did you get in here? File handle was null.");

            BeBinaryWriter bw = new BeBinaryWriter(fOutput); // create writer.


            var isLoop = false;
            if (wav.sampler.loops != null)
                isLoop = true;     

            bw.Write(STRM_HEAD);
            bw.Write((int)0x00000000); // Temporary length. 
            bw.Write((ushort)0x0); // ADPCM format. 
            bw.Write((ushort)wav.bitsPerSample);
            bw.Write((ushort)wav.channels);
            bw.Write(isLoop ? (ushort)0xFFFF : (ushort)0x0000);
            bw.Write(wav.sampleRate);
            //Console.WriteLine(wav.sampleCount);
            bw.Write(wav.sampleCount);
            bw.Write(isLoop ? wav.sampler.loops[0].dwStart : 0);
            bw.Write(isLoop ? wav.sampler.loops[0].dwEnd   : 0);
            bw.Write(STRM_LENGTH);
            bw.Write(0);
            bw.Write(0x7F000000); // wat. 
            for (int i = 0; i < 0x14; i++)
                bw.Write((byte)0x00);
            bw.Flush();

            Dictionary<int, short[]> wavTracks = new Dictionary<int, short[]>();          

            for (int i=0; i < wav.channels; i++)
            {
                wavTracks[i] = new short[wav.sampleCount];
            }

  


            var tpcm_l = 0;
            for (int i = 0; i < wav.sampleCount; i++)
            {
                for (int j = 0; j < wav.channels; j++)
                {
                    var pootis = wav.buffer[i * wav.channels + j];
                    wavTracks[j][tpcm_l] = pootis;
                }
                tpcm_l++;
            }
    
 
            var frames_per_block = (0x2760 / 0x9); // 9 byte frames. 
            var remaining_frames = wav.sampleCount / 16;
            var blocks_total = ((wav.sampleCount / 16) / frames_per_block) + 1;

            var bufferRemaining = (wav.sampleCount / 16) * 9;
            Console.WriteLine(bufferRemaining);

          
            var total_sample_offset = 0; 
            
            for (int blk = 0; blk < blocks_total; blk++)
            {
                util.consoleProgress("Rendering ", blk + 1, blocks_total, true);
                bw.Write(BLCK_HEAD);
                bw.Write(bufferRemaining >= 0x2760 ? 0x2760 : bufferRemaining);
                
                for (int i=0; i < last.Length; i++)
                {
                    bw.Write((short)last[i]);
                    bw.Write((short)penult[i]); 
                }
               
                for (int chn = 0; chn < wav.channels; chn++)
                {
                    for (int frm = 0; frm < frames_per_block; frm++)
                    {

                        short[] wavIn = new short[16];
                        byte[] adpcmOut = new byte[9];
                        var wavFP = wavTracks[chn];
                        var hist0 = last[chn];
                        var hist1 = penult[chn];
                        var sampleOffset = total_sample_offset + frm * 16;
                        //Console.WriteLine(sampleOffset);
                        for (int k = 0; k < 16; k++)
                        {
                            if ((sampleOffset + k) >= wavFP.Length)
                                break;
                            wavIn[k] = wavFP[sampleOffset + k];
                        }
                        bananapeel.Pcm16toAdpcm4(wavIn, adpcmOut, ref hist0, ref hist1);
                        last[chn] = hist0;
                        penult[chn] = hist1;
                        for (int k = 0; k < 9; k++)
                        {
                            bw.Write(adpcmOut[k]);
                        }
 
                    }
                }
                total_sample_offset += frames_per_block * 16; 


                bufferRemaining -= bufferRemaining >= 0x2760 ? 0x2760 : bufferRemaining;
            }
            var bop = bw.BaseStream.Position;
            bw.BaseStream.Position = 4;
            bw.Write((int)(bop - 0x40));

            bw.Flush();
            bw.Close();       
        }
    }
}
