using System;
using System.IO;
using System.Text;

namespace VideoAutoGen
{
    class autoProcess
    {
        private string ffms2Dll;
        private string mkvtoolnix_o;
        private string mkvtoolnix_n;
        private string neroaacenc;
        private string x264;
        private string mp4box;
        private string OutputPath;
        private string ffmpeg;
        private string VSPipe;
        private string downMix;

        public autoProcess(string ffms2DllI, string mkvtoolnix_oI, string mkvtoolnix_nI,
            string neroaacencI, string x264I, string mp4boxI, string OutputPathI, string ffmpegI, string VSPipeI, string downMixI)
        {
            ffms2Dll = ffms2DllI;
            mkvtoolnix_o = mkvtoolnix_oI;
            mkvtoolnix_n = mkvtoolnix_nI;
            neroaacenc = neroaacencI;
            x264 = x264I;
            mp4box = mp4boxI;
            OutputPath = OutputPathI;
            ffmpeg = ffmpegI;
            VSPipe = VSPipeI;
            downMix = downMixI;
        }

        public void LobbyMV(string LobbyMVList, string LobbyMVPath)
        {
            string outputname = DateTime.Now.ToString("yyyyMMddHmmss");
            string outputMVFolder = Path.Combine(OutputPath, outputname);
            Directory.CreateDirectory(Path.Combine(OutputPath, outputMVFolder));
            string outputBatName = Path.Combine(OutputPath, outputname + ".bat");
            string outputlogName = Path.Combine(OutputPath, outputname + ".log");
            string outputMP4Name = Path.Combine(OutputPath, outputname + ".mp4");
            string outputMKVName = Path.Combine(OutputPath, outputname + ".mkv");
            string outputMKVName_final = Path.Combine(OutputPath, outputname + "-final.mkv");
            string finalMP4List = Path.Combine(outputMVFolder, "final.txt");

            //從檔案列表讀取所有檔案名稱
            string[] filelist = File.ReadAllLines(LobbyMVList);
            //取得資料夾所有檔案名稱
            DirectoryInfo di = new DirectoryInfo(LobbyMVPath);
            FileInfo[] MVFile = di.GetFiles("*", SearchOption.AllDirectories);
            //對比檔案後輸出要複製檔案列表
            string[] outputList = getFileList(filelist, MVFile);
            //複製並編號所有檔案
            int count = 1;
            string destName;

            using (StreamWriter sw = new StreamWriter(outputlogName, false, Encoding.UTF8))
            {
                foreach (string name in outputList)
                {
                    Console.WriteLine("{0}<={1}", count.ToString("000"), name);
                    sw.WriteLine("{0}<={1}", count.ToString("000"), name);
                    destName = Path.Combine(outputMVFolder, count.ToString("000") + name.Substring(name.LastIndexOf(".")));
                    File.Copy(name, destName);

                    count++;
                }
                sw.Flush();
            }
            Console.WriteLine("複製檔案完成");

            //取得所有需要處理的檔案
            string[] fileMV = Directory.GetFiles(outputMVFolder);

            //各自建立畫面及聲音AVS檔案
            foreach (string filename in fileMV)
            {
                string avsVideoFile = filename + ".video.avs";
                string avsAudioFile = filename + ".audio.avs";
                using (StreamWriter sw = new StreamWriter(avsVideoFile))
                {
                    sw.WriteLine("LoadPlugin(\"{0}\")", ffms2Dll);
                    frameRate fr = getFrame(filename);
                    sw.WriteLine("Final = FFVideoSource(\"{0}\",track=-1,fpsnum={1},fpsden={2},width=1280,height=720,resizer=\"BICUBIC\",threads=1).ChangeFPS(25)", filename, fr.FrameRate_Num, fr.FrameRate_Den);
                    sw.WriteLine();
                    sw.WriteLine("Return Final");
                    sw.Flush();
                }
                using (StreamWriter sw = new StreamWriter(avsAudioFile))
                {
                    MediaInfoWrapper.MediaInfo MI = new MediaInfoWrapper.MediaInfo(filename);
                    int audioCount = MI.Audio.Count;
                    sw.WriteLine("LoadPlugin(\"{0}\")", ffms2Dll);
                    if (audioCount == 2)
                    {
                        sw.WriteLine("Final=DownMix(FFAudioSource (\"{0}\",track=2))", filename);
                    }
                    else
                    {
                        sw.WriteLine("Final=DownMix(FFAudioSource (\"{0}\",track=-1))", filename);
                    }
                    sw.WriteLine();
                    sw.WriteLine("Return Final");
                    sw.WriteLine(downMix);
                    sw.Flush();
                }
            }
            Console.WriteLine("完成生成各自AVS");

            //產生編碼用BAT編碼並合併成MP4檔案
            using (StreamWriter sw = new StreamWriter(outputBatName))
            {
                foreach (string filename in fileMV)
                {
                    string avsVideoFile = filename + ".video.avs";
                    string avsAudioFile = filename + ".audio.avs";
                    //sw.WriteLine("\"{0}\" --profile baseline --level 3.1 --preset veryfast --tune film --crf 22.0 --output \"{1}.264\" \"{1}\"", x264, avsVideoFile);
                    sw.WriteLine("\"{0}\" --profile main --level 3.1 --preset medium --tune film --crf 22.0 --output \"{1}.264\" \"{1}\"", x264, avsVideoFile);
                    sw.WriteLine("\"{0}\" -i \"{1}\" -c:a pcm_s16le -f wav -| \"{2}\" -ignorelength -lc -br 256000 -if - -of \"{1}.mp4\"", ffmpeg, avsAudioFile, neroaacenc);
                    sw.WriteLine("\"{0}\" -add \"{1}.264#trackID=1:fps=25.0:name=\" -add \"{2}.mp4#trackID=1:name=\" -tmp \"{3}\" -new \"{1}.mp4\"", mp4box, avsVideoFile, avsAudioFile, OutputPath.Replace("\\", "\\\\"));
                }
                sw.Flush();
            }

            //產生avslist.txt
            using (StreamWriter sw = new StreamWriter(finalMP4List))
            {
                foreach (string filename in fileMV)
                {
                    sw.WriteLine("file '" + filename + ".video.avs.mp4'");
                }
                sw.Flush();
            }

            //合併所有mp4檔案(ffmpeg)(ffmpeg -f concat -i filelist.txt -c copy output)
            using (StreamWriter sw = new StreamWriter(outputBatName,true))
            {
                if (fileMV.Length == 1) //特殊例子，當得一個檔案時直接複製文件就得，否則用ffmpeg concat單一檔案會爛畫面
                {
                    sw.WriteLine("copy \"{0}\" \"{1}\"", fileMV[0].ToString() + ".video.avs.mp4", outputMP4Name);
                }
                else //用concat將所有avslist.txt內的檔案合併
                {
                    sw.WriteLine("\"{0}\" -f concat -safe 0 -i \"{1}\" -c copy \"{2}\"", ffmpeg, finalMP4List, outputMP4Name);
                    string inputMP4Name = outputMP4Name.Replace(".mp4","tmp.mp4");
                    sw.WriteLine("move \"{0}\" \"{1}\"", outputMP4Name, inputMP4Name); //改名
                    sw.WriteLine("\"{0}\" -i \"{1}\" -c:v libx264 -profile:v main -level:v 3.1 -preset:v medium -tune:v film -crf:v 22.0 -c:a aac -b:a 256k \"{2}\"", ffmpeg, inputMP4Name, outputMP4Name); //用ffmpeg再轉換一次，避免因為合併聲畫出現問題(電腦播放沒有問題，但盒子播放會卡)。
                    sw.WriteLine("Del \"{0}\"", inputMP4Name); //刪除臨時mp4
                }
                sw.WriteLine("\"{0}\" -o \"{1}\"  \"--forced-track\" \"0:no\" \"--forced-track\" \"1:no\" \"-a\" \"1\" \"-d\" \"0\" \"-S\" \"-T\" \"--no-global-tags\" \"--no-chapters\" \"{2}\" \"--track-order\" \"0:0,0:1\"", mkvtoolnix_n,outputMKVName.Replace("\\","\\\\"),outputMP4Name.Replace("\\", "\\\\"));
                sw.WriteLine("\"{0}\" -o \"{1}\"  --default-track 1:yes --display-dimensions 1:1280x720 --default-track 2:yes -a 2 -d 1 -S \"{2}\" --track-order 0:1,0:2", mkvtoolnix_o, outputMKVName_final, outputMKVName);
                sw.WriteLine("del \"{0}\"", outputMKVName);//刪除舊版本MKV
                sw.WriteLine("pause");
                sw.Flush();
            }

            Console.WriteLine("完成");
        }

        private string[] getFileList(string[] inputList, FileInfo[] inputfile)
        {
            string[] tmpList = new string[inputList.Length];
            int count = 0;
            LevenshteinDistance ld = new LevenshteinDistance();
            foreach (string name in inputList)
            {
                if (name.Trim().Equals("")) { continue; }   //空白就跳過
                decimal largestValue = 0;
                decimal currentValue = 0;
                foreach (FileInfo fi in inputfile)
                {
                    //if (!fi.Extension.Equals(".mp4")) { continue; } //mp4 file only
                    currentValue = ld.LevenshteinDistancePercent(name.ToUpper(), fi.Name.ToUpper());
                    //Console.WriteLine("{0},{1},{2}",name,fi.Name,currentValue);
                    if (currentValue > largestValue)
                    {
                        largestValue = currentValue;
                        tmpList[count] = fi.FullName;
                    }
                }
                count++;
            }
            string[] returnList = new string[count];
            Array.Copy(tmpList, returnList, count);
            return returnList;
        }

        private frameRate getFrame(string fileName)
        {
            frameRate fr = new frameRate();
            MediaInfoWrapper.MediaInfo MI = new MediaInfoWrapper.MediaInfo(fileName);
            string frameRate = MI.Video[0].FrameRate;
            switch (frameRate)
            {
                case "24.000":
                    fr.FrameRate_Num = 24;
                    fr.FrameRate_Den = 1;
                    break;
                case "25.000":
                    fr.FrameRate_Num = 25;
                    fr.FrameRate_Den = 1;
                    break;
                case "30.000":
                    fr.FrameRate_Num = 25;
                    fr.FrameRate_Den = 1;
                    break;
                case "50.000":
                    fr.FrameRate_Num = 50;
                    fr.FrameRate_Den = 1;
                    break;
                case "60.000":
                    fr.FrameRate_Num = 50;
                    fr.FrameRate_Den = 1;
                    break;
                case "29.970":
                    fr.FrameRate_Num = 30000;
                    fr.FrameRate_Den = 1001;
                    break;
                case "23.976":
                    fr.FrameRate_Num = 24000;
                    fr.FrameRate_Den = 1001;
                    break;
                default:
                    fr.FrameRate_Num = 23;
                    fr.FrameRate_Den = 1;
                    break;
            }
            return fr;
        }

        private struct frameRate
        {
            public float FrameRate_Num;
            public float FrameRate_Den;
        }


    }
}
