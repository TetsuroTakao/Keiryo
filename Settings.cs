using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace IngicateWpf
{
    public class Settings
    {
        public bool ModifyConfigure(string key, string value)
        {
            bool result = false;
            Configuration configFile = null;
            configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (configFile.AppSettings.Settings.AllKeys.Where(k => k == key).Count() > 0)
            {
                configFile.AppSettings.Settings[key].Value = value;
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
                result = true;
            }
            return result;
        }
        public string ReadConfigure(string key)
        {
            string result = ConfigurationManager.AppSettings[key];
            return result;
        }
        void SaveLog(string strCode, string strKeiryoKekka) 
        {
            ListViewItem lvwListItem = new ListViewItem();
            var intWeight = 0;
            //１０ｇ単位の計量結果をｋｇに換算（五捨六入）
            //ex)0557⇒5.57kg⇒小数点以下第一位を五捨六入して5kg
            intWeight = (int)Math.Round(double.Parse(strKeiryoKekka)- 0.1,MidpointRounding.AwayFromZero);
            if (intWeight == 0) intWeight = 1;
            lvwListItem.Text = strCode;
            lvwListItem.SubItems.Add(intWeight.ToString() + " kg");
            lvwListItem.SubItems.Add(DateTime.Now.ToString("yy/MM/dd HH:mm:ss"));
            var lvwKeiryoLog = new List<ListViewItem>();
            lvwKeiryoLog.Add(lvwListItem);
            //テキストファイルの保存
            //コード 7bytes
            //重量 4bytes
            //日付 8bytes（YYYYMMDD形式）
            var SAVE_DIR = "Y" + strCode + ".TXT";
            var file = new FileInfo(SAVE_DIR);
            if (!file.Directory.Exists) 
            {
                file.Directory.Create();
                if (!file.Exists) 
                {
                    file.Create();
                }
            }
            var text = strCode + intWeight.ToString("0000") + DateTime.Now.ToString("yyyyMMdd");
            using (var st = file.CreateText()) 
            {
                st.Write(text);
            }
        }
        class ListViewItem
        {
            public string Text { get; set; }
            public List<string> SubItems { get; set; } = new List<string>();
        }
        public string LoadComPortSettings() 
        {
            try
            {
                //'INI ファイルをプログラムと同じフォルダに置く場合
                //'ルートディレクトリかの判断
                //'iniファイルを読み込み（保存先フォルダの取得）
                var MYPATH = "";
                var SAVE_DIR = MYPATH + "KEIRYO_DATA";
                //'iniファイルを読み込み（COM_PORTの取得）
                var INIFILE_PATH = SAVE_DIR + @"\SETTINGS.INI";
                var file = new FileInfo(INIFILE_PATH);
                var COM_PORT = string.Empty;
                using (var t = file.OpenText()) 
                {
                    string r = t.ReadToEndAsync().Result;
                    foreach (var e in r.Split(@"\r\n")) 
                    {
                        if (e.StartsWith("COM_PORT")) COM_PORT = e.Replace("COM_PORT", "");
                    }
                }
                if (string.IsNullOrEmpty(COM_PORT)) 
                {
                    COM_PORT = "COM1";
                }
                return COM_PORT;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
