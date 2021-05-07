using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using static Auxiliary.Upload.UploadTask;

namespace Auxiliary.Upload
{
    class AliPanUpload
    {
        private string configFile { set; get; }
        private int exitCode;
        /// <summary>
        /// 初始化AliPan Upload
        /// </summary>
        public AliPanUpload()
        { }

        /// <summary>
        /// 上传到AliPan
        /// </summary>
        /// <param name="uploadInfo">传入上传信息</param>
        public void doUpload(UploadInfo uploadInfo)
        {
            Process proc = new Process();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                proc.StartInfo.FileName = @"python"; //windows下的程序位置
            }
            else
            {
                proc.StartInfo.FileName = "python";
            }
            DateTime showTime = DateTime.Now;
            proc.StartInfo.Arguments = $"aliyundrive-uploader\\main.py  \"{uploadInfo.srcFile}\" \"{Uploader.aliPanPath + uploadInfo.remotePath}\"";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            //proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;
            //proc.StartInfo.CreateNoWindow = true; // 不显示窗口。
            proc.EnableRaisingEvents = true;
            //proc.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            proc.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                string stringResults = e.Data;
                if (stringResults == "" || stringResults == null) return;
                uploadInfo.status["AliPan"].comments = stringResults;
                InfoLog.InfoPrintf($"AliPan: {stringResults}", InfoLog.InfoClass.上传必要提示);

            };  // 捕捉的信息
            proc.Start();
            proc.BeginOutputReadLine();   // 开始异步读取
            proc.Exited += Process_Exited;
            proc.WaitForExit();
            proc.Close();
            GC.Collect();
            if (exitCode != 0)
                throw new UploadFailure("fail to upload");
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            Process P = (Process)sender;
            exitCode = P.ExitCode;
        }
    }
}
