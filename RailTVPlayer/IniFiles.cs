using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RailTVPlayer
{
    class IniFiles
    {
        public string iniFilePath;

        //声明API函数
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        //获取文件路径
        public IniFiles(string iniPath)
        {
            iniFilePath = iniPath;
        }

        //构造方法
        public IniFiles() { }

        //写入ini文件
        public void IniWriteValue(string Section,string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, iniFilePath);
        }

        //读取ini文件
        public string iniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(500);
            int i = GetPrivateProfileString(Section, Key, "", temp, 500, this.iniFilePath);
            return temp.ToString();
        }

        //验证文件是否存在
        public bool ExistIniFile()
        {
            return File.Exists(iniFilePath);
        }

    }
}
