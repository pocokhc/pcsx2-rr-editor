using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pcsx2_rr_editor.src.pcsx2_rr
{
    //--------------------------------------
    // pcsx2_rr の内容を移植
    // ヘッダー情報
    //--------------------------------------
    public class MovieInfo
    {
        //FILE* File;
        //public uint FrameNum=0;  //u32 FrameNum;
        public uint FrameMax = 0;  //u32 FrameMax;
        public uint Rerecs = 0;    //u32 Rerecs;
        //public bool Paused=false;    //bool Paused;
        //public bool Replay = false;    //bool Replay;
        //public bool ReadOnly = false;  //bool ReadOnly;
        //public string Filename="";    //char Filename[256];
    }

    //--------------------------------------
    // pcsx2_rr の内容を移植
    // キー情報
    //
    // extern u8 g_PadData[2][20];
    //--------------------------------------
    public class PadData
    {
        //public byte[,] key = new byte[2, 20];
        public byte[] key = new byte[6];        //キー情報
        public int frame;                       //フレーム情報

        //--- PadDataコピー用
        public void set(PadData p)
        {
            frame = p.frame;
            for(int i = 0; i < p.key.Length; i++)
            {
                key[i] = p.key[i];
            }
        }
        //--- 表示用
        public override string ToString()
        {
            return frame + " : " + string.Join(",", key);
        }

        //-----------
        // set key
        //-----------
        public void setKey(string button, bool fpushed)
        {
            byte[] keybit = getKeyBit(button);

            if (fpushed)
            {
                //押した状態にする
                key[0] = (byte)~((byte)~key[0] | (byte)~keybit[0]);
                key[1] = (byte)~((byte)~key[1] | (byte)~keybit[1]);
            }
            else
            {
                //押してない状態にする
                key[0] = (byte)(key[0] | (byte)~keybit[0]);
                key[1] = (byte)(key[1] | (byte)~keybit[1]);
            }
        }

        //-----------
        // is key
        //-----------
        public bool isPushKey(string button)
        {
            byte[] keybit = getKeyBit(button);

            bool f1 = ((byte)~key[0] & (byte)~keybit[0]) != 0;    //押されているかどうか
            bool f2 = ((byte)~key[1] & (byte)~keybit[1]) != 0;

            return ( f1 || f2);
        }
            
        //----------------
        // keyとbitの対応
        //----------------
        private byte[] getKeyBit(string button)
        {
            byte[] key = null;

            if (button == "↑")      key = new byte[2] { Convert.ToByte("11101111", 2), Convert.ToByte("11111111", 2) };
            else if (button == "←") key = new byte[2] { Convert.ToByte("01111111", 2), Convert.ToByte("11111111", 2) };
            else if(button == "→")  key = new byte[2] { Convert.ToByte("11011111", 2), Convert.ToByte("11111111", 2) };
            else if(button == "↓")  key = new byte[2] { Convert.ToByte("10111111", 2), Convert.ToByte("11111111", 2) };

            else if(button == "start")  key = new byte[2] { Convert.ToByte("11110111", 2), Convert.ToByte("11111111", 2) };
            else if(button == "select") key = new byte[2] { Convert.ToByte("11111110", 2), Convert.ToByte("11111111", 2) };

            else if(button == "×") key = new byte[2] { Convert.ToByte("11111111", 2), Convert.ToByte("10111111", 2) };
            else if(button == "○") key = new byte[2] { Convert.ToByte("11111111", 2), Convert.ToByte("11011111", 2) };
            else if(button == "□") key = new byte[2] { Convert.ToByte("11111111", 2), Convert.ToByte("01111111", 2) };
            else if(button == "△") key = new byte[2] { Convert.ToByte("11111111", 2), Convert.ToByte("11101111", 2) };

            else if(button == "L1") key = new byte[2] { Convert.ToByte("11111111", 2), Convert.ToByte("11111011", 2) };
            else if(button == "L2") key = new byte[2] { Convert.ToByte("11111111", 2), Convert.ToByte("11111110", 2) };
            else if(button == "R1") key = new byte[2] { Convert.ToByte("11111111", 2), Convert.ToByte("11110111", 2) };
            else if(button == "R2") key = new byte[2] { Convert.ToByte("11111111", 2), Convert.ToByte("11111101", 2) };
            else
            {
                key = new byte[2] { Convert.ToByte("11111111", 2), Convert.ToByte("11111111", 2) };
            }
            return key;
        }
    }

    //--------------------------------------
    // セーブ、ロードをするクラス
    //--------------------------------------
    public class TAS
    {
        public MovieInfo info = new MovieInfo();
        public List<PadData> keys = new List<PadData>();
        public string file = "";

        //--------------------------------------
        // keyをフレームにソート
        //--------------------------------------
        public void sort()
        {
            keys.Sort((a, b) => a.frame - b.frame);
        }

        //--------------------------------------
        // pcsx2_rr の内容を移植
        // fread(&g_Movie.FrameMax, 4, 1, g_Movie.File);
        // fread(&g_Movie.Rerecs, 4, 1, g_Movie.File);
        // fread(g_PadData[0]+2, 6, 1, g_Movie.File);
        //--------------------------------------
        public void load(string file)
        {
            this.file = file;
            FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            BinaryReader binRead = new BinaryReader(fs);
            try
            {
                info.FrameMax = binRead.ReadUInt32();
                info.Rerecs = binRead.ReadUInt32();

                keys.Clear();
                for (int i=0;i<info.FrameMax;i++)
                {
                    PadData key = new PadData();
                    key.key = binRead.ReadBytes(6);
                    key.frame = i;
                    keys.Add(key);
                }
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("end of stream");
            }
            finally
            {
                binRead.Close();
                fs.Close();
            }

        }

        //--------------------------------------
        // 保存
        //--------------------------------------
        public void save(string file)
        {
            FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);
            
            try
            {
                bw.Write(info.FrameMax);
                bw.Write(info.Rerecs);

                foreach (PadData key in keys)
                {
                    bw.Write(key.key);
                }
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("end of stream");
            }
            finally
            {
                bw.Close();
                fs.Close();
            }
        }
        
    }
}
