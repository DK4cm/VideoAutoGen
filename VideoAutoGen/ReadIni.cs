using System;
using System.Runtime.InteropServices;
using System.Text;

namespace VideoAutoGen
{
    public class ReadIni : IDisposable
    {
        private bool bDisposed;

        private string _FilePath = string.Empty;

        public string FilePath
        {
            get
            {
                if (this._FilePath == null)
                {
                    return string.Empty;
                }
                return this._FilePath;
            }
            set
            {
                if (this._FilePath != value)
                {
                    this._FilePath = value;
                }
            }
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public ReadIni(string path)
        {
            this._FilePath = path;
        }

        ~ReadIni()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool IsDisposing)
        {
            if (this.bDisposed)
            {
                return;
            }
            this.bDisposed = true;
        }

        public void setKeyValue(string IN_Section, string IN_Key, string IN_Value)
        {
            ReadIni.WritePrivateProfileString(IN_Section, IN_Key, IN_Value, this._FilePath);
        }

        public string getKeyValue(string IN_Section, string IN_Key)
        {
            StringBuilder stringBuilder = new StringBuilder(255);
            ReadIni.GetPrivateProfileString(IN_Section, IN_Key, "", stringBuilder, 255, this._FilePath);
            return stringBuilder.ToString();
        }

        public string getKeyValue(string Section, string Key, string DefaultValue)
        {
            string result;
            try
            {
                StringBuilder stringBuilder = new StringBuilder(255);
                ReadIni.GetPrivateProfileString(Section, Key, "", stringBuilder, 255, this._FilePath);
                result = ((stringBuilder.Length > 0) ? stringBuilder.ToString() : DefaultValue);
            }
            catch
            {
                result = string.Empty;
            }
            return result;
        }
    }
}
