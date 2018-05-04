using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VideoAutoGen
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.OutputEncoding = System.Text.Encoding.Default;
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string ffms2Dll = Path.Combine(baseDirectory, "bin", "ffms2", "ffms2.dll");
            string mkvtoolnix_old = Path.Combine(baseDirectory, "bin", "mkvtoolnix-o", "mkvmerge.exe");
            string mkvtoolnix = Path.Combine(baseDirectory, "bin", "mkvtoolnix", "mkvmerge.exe");
            string neroaacenc = Path.Combine(baseDirectory, "bin", "neroaacenc", "neroAacEnc.exe");
            //string x264 = Path.Combine(baseDirectory, "bin", "x264", "x264_64.exe");
            string x264 = Path.Combine(baseDirectory, "bin", "x264", "avs4x264mod.exe");
            string mp4box = Path.Combine(baseDirectory, "bin", "mp4box", "mp4box.exe");
            string ffmpeg = Path.Combine(baseDirectory, "bin", "ffmpeg", "ffmpeg.exe");
            string VSPipe = Path.Combine(baseDirectory, "bin", "VapourSynth64", "VSPipe.exe");
            string downMixPath = Path.Combine(baseDirectory, "bin", "DownMix.txt");
            string downMix;

            string iniFile = Path.Combine(baseDirectory, "VideoAutoGen.ini");
            ReadIni ini = new ReadIni(iniFile);

            string FileSourcePath = ini.getKeyValue("PathSetting", "FileSourcePath", "");
            string OutputPath = ini.getKeyValue("PathSetting", "OutputPath", "");

            string CopyListName = Path.Combine(ini.getKeyValue("ListSetting", "CopyListName", ""));

            using (StreamReader sr = new StreamReader(downMixPath))
            {
                downMix = sr.ReadToEnd();
            }

            if (!Directory.Exists(FileSourcePath) | !Directory.Exists(OutputPath))
            {
                Console.WriteLine("ini內FileSourcePath/OutputPath不存在");
                Console.ReadLine();
                return;
            }
            if (!File.Exists(CopyListName))
            {
                Console.WriteLine("ini內CopyListName不存在");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("複製路徑：{0}", FileSourcePath);
            Console.WriteLine("按enter開始處理檔案：");
            Console.ReadLine();
            autoProcess runProcess = new autoProcess(ffms2Dll, mkvtoolnix_old, mkvtoolnix, neroaacenc, x264, mp4box, OutputPath,ffmpeg,VSPipe, downMix);
            runProcess.LobbyMV(CopyListName, FileSourcePath);
            Console.ReadLine();
        } 
    }
}
