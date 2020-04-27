using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace IncidentMonitorWPF
{
    public class DataAccessLayer
    {
        public string MajorVersion { get; set; }
        public string MinorVersion { get; set; }
        public LogModel LastLog { get; set; }
        public string CEDCaptureImagePath { get; set; }
        public string CEDCapturePath { get; set; }
        public string CEDLogsPath { get; set; }
        public string CEDSendIncidentsLogsPath { get; set; }
        public string ReportsPath { get; set; }
        public string ReportsVPath { get; set; }
        public DateTime TargetDate { get; set; }
        string dbPath { get; set; }
        string micsPath { get; set; }
        string targetFile { get; set; }
        string micsRegistDBFileFullPath { get; set; }
        public string TsvDivisionInfo { get; set; }
        public DataAccessLayer(string targetDate, bool useProductDirectory = true)
        {
            Nullable<DateTime> target = null;
            if (targetDate.Length == 10)
            {
                var year = targetDate.Split(new string[] { "/", ".", "-" }, StringSplitOptions.None).FirstOrDefault();
                var month = targetDate.Split(new string[] { "/", ".", "-" }, StringSplitOptions.None)[1];
                var day = targetDate.Split(new string[] { "/", ".", "-" }, StringSplitOptions.None).LastOrDefault();
                target = DateTime.Parse(year + "-" + month + "-" + day);
            }
            if (targetDate.Length == 8)
            {
                target = DateTime.Parse(targetDate.Substring(0, 4) + "-" + targetDate.Substring(4, 2) + "-" + targetDate.Substring(6, 2));
            }
            if (!target.HasValue)
            {
                TargetDate = DateTime.Now;
            }
            else
            {
                TargetDate = target.Value;
            }
            CEDCaptureImagePath = @"C:\CEDLogs\CEDManagementConsoleImage.png";
            CEDCapturePath = @"c:\CEDLogs\" + TargetDate.ToString("yyyyMMdd") + @"Capture.log";
            CEDSendIncidentsLogsPath = @"c:\CEDLogs\" + TargetDate.ToString("yyyyMMdd") + @"IncidntsSendLog.txt";
            CEDLogsPath = @"c:\CEDLogs\" + TargetDate.ToString("yyyyMMdd") + @"ErrorLog.txt";
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string divisionsHeaderfile = appPath + @"Assets\DepartmentDivisionsCode.txt";
            FileInfo f = new FileInfo(divisionsHeaderfile);
            DirectoryInfo d = new DirectoryInfo(f.DirectoryName);
            if (f.Exists)
            {
                TsvDivisionInfo = File.ReadAllText(f.FullName, Encoding.UTF8);
            }
            //MicsRegistLog = new List<MaintenanceDailyReport>();
            if (Regex.Match(targetDate, @"\d{8}").Success)
            {
                if (targetDate.Length == 10)
                {
                    TargetDate = new DateTime(int.Parse(targetDate.Substring(0, 4)), int.Parse(targetDate.Substring(5, 2)), int.Parse(targetDate.Substring(8, 2)));
                }
                else if (targetDate.Length == 8)
                {
                    TargetDate = new DateTime(int.Parse(targetDate.Substring(0, 4)), int.Parse(targetDate.Substring(4, 2)), int.Parse(targetDate.Substring(6, 2)));
                }
            }
            if (useProductDirectory)
            {
                micsPath = @"C:\inetpub\wwwroot\MitsuiwaMachineManager\Content\MICSStates";
                dbPath = @"C:\inetpub\wwwroot\MitsuiwaMachineManager\Content\DailyReports\DB";
                ReportsPath = @"C:\inetpub\wwwroot\MitsuiwaMachineManager\Content\DailyReports\Reports";
                ReportsVPath = @"../MitsuiwaMachineManager/Content/DailyReports/Reports";
            }
            else
            {
                DirectoryInfo dirctory = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
                micsPath = dirctory.FullName + @"\App_Data\MICSStates";
                dbPath = dirctory.FullName + @"\App_Data\DailyReports\DB";
                ReportsPath = dirctory.FullName + @"\App_Data\DailyReports\Reports";
            }
            if (!Directory.Exists(micsPath))
            {
                //Directory.CreateDirectory(micsPath);
            }
            if (!Directory.Exists(dbPath))
            {
                //Directory.CreateDirectory(dbPath);
            }
            if (string.IsNullOrEmpty(targetDate)) targetDate = DateTime.Now.ToString("yyyyMMdd");
             ReportsPath += targetDate + "Created"; 
            if (!Directory.Exists(ReportsPath))
            {
                //Directory.CreateDirectory(ReportsPath);
            }
            micsRegistDBFileFullPath = dbPath + @"\" + targetDate + "MicsRegistDB.txt";
        }
        public string SaveReportHTML(string html, string division)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(html))
            {
                FileInfo f = null;
                byte[] b = new byte[0];
                DateTime reportDate = DateTime.MinValue;
                if (!(new DirectoryInfo(ReportsPath).Exists)) { new DirectoryInfo(ReportsPath).Create(); }
                result = ReportsPath + @"\" + (string.IsNullOrEmpty(division) ? "0081A" : division.Split('：')[0]);
                foreach (string temp in html.Split(new string[] { "<html>", "</html>" }, StringSplitOptions.None))
                {
                    if (string.IsNullOrEmpty(temp)) continue;
                    if (!temp.Contains("***** 保守作業日報 *****")) continue;
                    html = "<html><head></head><body><center>" + temp + "</center></body></html>";
                    b = UTF8Encoding.UTF8.GetBytes(html);
                    f = new FileInfo(result + "_" + Regex.Match(new DirectoryInfo(ReportsPath).Name,@"\d+").Value + ".html");
                    using (FileStream st = f.Create())
                    {
                        st.Write(b, 0, b.Length);
                    }
                    break;
                }
            }
            return result;
        }
        public string[] GetReportFilesFullPath(ReportFileTypes type, string sort = "Desc")
        {
            string dateTimeShort8 = string.Empty;
            List<string> reports = new List<string>();
            SortedList<DateTime, List<string>> s = new SortedList<DateTime, List<string>>();
            DateTime tempDate = DateTime.MinValue;
            List<FileInfo> files = null;
            switch (type)
            {
                case ReportFileTypes.ReportsHTML:
                    if (type == ReportFileTypes.ReportsHTML)
                    {
                        foreach (DirectoryInfo d in new DirectoryInfo(ReportsPath).Parent.GetDirectories())
                        {
                            if (System.Text.RegularExpressions.Regex.Match(d.Name, @"\d").Success)
                            {
                                dateTimeShort8 = Regex.Match(d.Name, @"\d+").Value;
                            }
                            else
                            {
                                continue;
                            }
                            tempDate = new DateTime(int.Parse(dateTimeShort8.Substring(0, 4)), int.Parse(dateTimeShort8.Substring(4, 2)), int.Parse(dateTimeShort8.Substring(6, 2)));
                            files = d.GetFiles().Where(f => f.Extension == ".html").ToList();
                            reports = new List<string>();
                            foreach (FileInfo f in files)
                            {
                                reports.Add(f.FullName);
                            }
                            s.Add(tempDate, reports);
                        }
                    }
                    else
                    {
                        tempDate = new DateTime(int.Parse(dateTimeShort8.Substring(0, 4)), int.Parse(dateTimeShort8.Substring(4, 2)), int.Parse(dateTimeShort8.Substring(6, 2)));
                    }
                    break;
                case ReportFileTypes.ReportDB:
                    files = new DirectoryInfo(dbPath).GetFiles().Where(f => f.Name.EndsWith("ReportsDB.txt")).ToList();
                    foreach (FileInfo f in files)
                    {
                        dateTimeShort8 = f.Name.Substring(0, 8);
                        tempDate = new DateTime(int.Parse(dateTimeShort8.Substring(0, 4)), int.Parse(dateTimeShort8.Substring(4, 2)), int.Parse(dateTimeShort8.Substring(6, 2)));
                        s.Add(tempDate,new List<string>() { f.Name});
                    }
                    break;
                case ReportFileTypes.ReportsSummary:
                    files = new DirectoryInfo(dbPath).GetFiles().Where(f => f.Name.EndsWith("ReportsSummary.txt")).ToList();
                    foreach (FileInfo f in files)
                    {
                        dateTimeShort8 = f.Name.Substring(0, 8);
                        tempDate = new DateTime(int.Parse(dateTimeShort8.Substring(0, 4)), int.Parse(dateTimeShort8.Substring(4, 2)), int.Parse(dateTimeShort8.Substring(6, 2)));
                        s.Add(tempDate, new List<string>() { f.Name });
                    }
                    break;
            }
            reports = new List<string>();
            if (sort == "Desc")
            {
                foreach (var item in s.OrderByDescending(r => r.Key))
                {
                    reports.AddRange(item.Value);
                }
            }
            return reports.ToArray();
        }
        public enum ReportFileTypes
        {
            ReportDB,
            ReportsSummary,
            ReportsHTML
        }
        public string SaveExecLog(List<string> log,DateTime d)
        {
            string result = string.Empty;
            if (log.Count > 0)
            {
                byte[] b = new byte[0];
                string targetPath = dbPath + @"\" + d.ToString("yyyyMMdd") + "ResultLog.txt";
                FileInfo f = new FileInfo(targetPath);
                FileInfo backFile = new FileInfo(f.FullName.Replace(f.Extension, "_back.txt"));
                if (f.Exists)
                {
                    if (backFile.Exists) backFile.Delete();
                    f.CopyTo(f.FullName.Replace(f.Extension, "_back.txt"));
                }
                using (FileStream st = f.Create())
                {
                    foreach (string temp in log)
                    {
                        b = UTF8Encoding.UTF8.GetBytes(temp);
                        st.Write(b, 0, b.Length);
                    }
                }
            }
            return result;
        }
        public bool SendIncidents(string html)
        {
            bool result = false;
            DateTime n = DateTime.Now;
            string now = n.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            var sendData = new { RequestDateTime = now,HTML = html};
            string json = JsonConvert.SerializeObject(sendData, new IsoDateTimeConverter());
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            Uri requestURL = new Uri("https://dev.vantiq.com/api/v1/resources/custom/CRMSHTML");
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "gjTjGEyZ1DdJWU1uTHtJjjiuxh_OvgAjxkzcI-VRrS0=");
            var res = httpClient.PostAsync(requestURL, content).Result;
            return result;
        }
        public bool SendIncidents(IncidentModelStaging incident, string token)
        {
            bool result = false;
            List<IncidentModelStaging> incidents = new List<IncidentModelStaging>();
            //incident.RequiredLicense = GetLicensesRequrement(incident.MachineTypeName);
            incidents.Add(incident);
            string json = JsonConvert.SerializeObject(incidents, new IsoDateTimeConverter());
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            Uri requestURL = new Uri("https://dev.vantiq.com/api/v1/resources/custom/IncidentFullAttributes");
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var res = httpClient.PostAsync(requestURL, content).Result;
            var resstring = res.Content.ReadAsStringAsync().Result;
            if (res.StatusCode== HttpStatusCode.OK)
            {
                LastLog = new LogModel() { EventMessage = string.Format("RequiredLicense:{0}", incident.RequiredLicense),EventSource = incident.IncidentNo };
            }
            else
            {
                LastLog = new LogModel() { EventMessage = string.Format("RequiredLicense:{0}", incident.RequiredLicense), Error = new ErrorModel() { ErrorMessage = resstring }, EventSource = incident.IncidentNo };
            }
            return result;
        }
        //public bool SendIncidents(IncidentModel incident, string token)
        //{
        //    if (incident.IncidentNo != "190830010608") return false;
        //    bool result = false;
        //    List<IncidentModel>  incidents = new List<IncidentModel>();
        //    //incident.RequiredLicense = GetLicensesRequrement(incident.MachineTypeName);
        //    incidents.Add(incident);
        //    string json = JsonConvert.SerializeObject(incidents, new IsoDateTimeConverter());
        //    var content = new StringContent(json, Encoding.UTF8, "application/json");
        //    Uri requestURL = new Uri("https://dev.vantiq.com/api/v1/resources/custom/IncidentFullAttributes");
        //    HttpClient httpClient = new HttpClient();
        //    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //    var res = httpClient.PostAsync(requestURL, content).Result;
        //    return result;
        //}
        public bool SendIncidents(IncidentModel incident, string token, bool api)
        {
            bool result = false;
            List<IncidentModel> incidents = new List<IncidentModel>();
            incident.RequiredLicense = GetLicensesRequrement(incident.MachineTypeName);
            incidents.Add(incident);
            string json = JsonConvert.SerializeObject(incidents, new IsoDateTimeConverter());
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            Uri requestURL = new Uri("https://api.vantiq.com/api/v1/resources/custom/IncidentFullAttributes");
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var res = httpClient.PostAsync(requestURL, content).Result;
            var resstring = res.Content.ReadAsStringAsync().Result;
            if (res.StatusCode == HttpStatusCode.OK)
            {
                LastLog = new LogModel() { EventMessage = string.Format("RequiredLicense:{0}", incident.RequiredLicense), EventSource = incident.IncidentNo };
            }
            else
            {
                LastLog = new LogModel() { EventMessage = string.Format("RequiredLicense:{0}", incident.RequiredLicense), Error = new ErrorModel() { ErrorMessage = resstring }, EventSource = incident.IncidentNo };
            }
            return result;
        }
        public bool SaveIncidentsSendLog<T>(List<T> micsresult)
        {
            bool result = false;
            string jsonstring = string.Empty;
            string path = CEDSendIncidentsLogsPath;
            if(typeof(T).Name== "ClosedHTML") path = path.Replace("IncidntsSendLog", "CloseHTMLLog");
            if (typeof(T).Name == "MICSData") path = path.Replace("IncidntsSendLog", "MICSDataLog");
            if (typeof(T).Name == "HTMLModel") path =path.Replace("IncidntsSendLog", "CRMSHTMLLog");
            if(typeof(T).Name == "IncidentModel") path = path.Replace("IncidntsSendLog", "IncidntsAddNewLog");
            if (typeof(T).Name == "IncidentModelStaging") path = path.Replace("IncidntsSendLog", "IncidntsAddNewLog");
            if (typeof(T).Name == "LogModel") path = path.Replace("IncidntsSendLog", "MoniterLog");
            DirectoryInfo d = new DirectoryInfo(@"c:\CEDLogs\");
            if (!d.Exists) d.Create();
            FileInfo f = new FileInfo(path);
            if (!f.Exists)
            {
                FileStream fileStream = f.Create();
                fileStream.Close();
                fileStream.Dispose();
            }
            else
            {
                jsonstring = File.ReadAllText(f.FullName);
                if (!string.IsNullOrEmpty(jsonstring))
                {
                    micsresult.AddRange(JsonConvert.DeserializeObject<List<T>>(jsonstring));
                }
            }
            using (StreamWriter file = File.CreateText(f.FullName))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, micsresult);
            }
            result = true;
            return result;
        }
        public List<IncidentModel> ReadIncidentsSendLog(DateTime? TargetDate)
        {
            if (!TargetDate.HasValue) TargetDate = DateTime.Now;
            List<IncidentModel> result = new List<IncidentModel>();
            string jsonstring = string.Empty;
            string path = CEDSendIncidentsLogsPath;
            if (!string.IsNullOrEmpty(CEDLogsPath))
            {
                path = CEDLogsPath;
            }
            FileInfo f = new FileInfo(path);
            if (f.Exists)
            {
                jsonstring = File.ReadAllText(path);
                result = JsonConvert.DeserializeObject<List<IncidentModel>>(jsonstring);
            }
            return result;
        }
        public List<T> ReadIncidentsSendLog<T>()
        {
            List<T> logresult = new List<T>();
            string jsonstring = string.Empty;
            string path = CEDSendIncidentsLogsPath;
            if (typeof(T).Name == "MICSData") path = path.Replace("IncidntsSendLog", "MICSDataLog");
            string errorPath = CEDLogsPath;
            FileInfo f = new FileInfo(path);
            if (f.Exists)
            {
                try
                {
                    jsonstring = File.ReadAllText(f.FullName);
                    if (string.IsNullOrEmpty(jsonstring)) jsonstring = "[]";
                    logresult.AddRange(JsonConvert.DeserializeObject<List<T>>(jsonstring));
                    using (StreamWriter file = File.CreateText(f.FullName))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(file, logresult);
                    }
                }
                catch (Exception ex)
                {
                    ErrorModel error = new ErrorModel();
                    error.ErrorMessage = ex.Message;
                    error.ErrorOccours = DateTime.Now;
                    error.ErrorSource = "ReadIncidentsSendLog<T>()";
                    List<ErrorModel> errors = new List<ErrorModel>();
                    FileInfo errorfile = new FileInfo(errorPath);
                    if (errorfile.Exists)
                    {
                        jsonstring = File.ReadAllText(errorfile.FullName);
                        errors.AddRange(JsonConvert.DeserializeObject<List<ErrorModel>>(jsonstring));
                    }
                    errors.Add(error);
                    using (StreamWriter file = File.CreateText(errorfile.FullName))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(file, errors);
                    }
                }
            }
            else
            {
                using (StreamWriter file = File.CreateText(f.FullName))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, logresult);
                }
            }
            return logresult;
        }
        public bool SendCEUsers(List<VantiqCEUserModel> ceusers, string token,string vantiqCloudSubDomain)
        {
            bool result = false;
            string json = JsonConvert.SerializeObject(ceusers, new IsoDateTimeConverter());
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            string uri = "https://" + vantiqCloudSubDomain + ".vantiq.com/api/v1/resources/custom/ExcelCE";
            //string uri = "https://" + vantiqCloudSubDomain + ".vantiq.com/api/v1/resources/custom/CE";
            if (vantiqCloudSubDomain == "api") uri = "https://" + vantiqCloudSubDomain + ".vantiq.com/api/v1/resources/custom/CE";
            Uri requestURL = new Uri(uri);
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var res = httpClient.PostAsync(requestURL, content).Result;
            if (res.StatusCode != HttpStatusCode.OK)
            {
                string message = res.Content.ReadAsStringAsync().Result;
                int keyErrorRecord = Regex.Matches(message, "duplicate key error").Count;
                List<string> errorIDs = message.Split(new string[] { "duplicate key error" },StringSplitOptions.None).ToList();
                List<string> Ids = new List<string>();
                foreach (string errorID in errorIDs)
                {
                    var dupKey = Regex.Match(errorID, @"dup key: { : \\" + "\"" + @"\w*\d{6}").Value;
                    if (string.IsNullOrEmpty(dupKey)) continue;
                    Ids.Add(dupKey.Split('\"').LastOrDefault());
                }
                message = string.Format("CE情報{1}件を更新要求して{0}件が一意キー違反です。", Ids.Distinct().Count(), ceusers.Count);
                message += "ID=[" + string.Join(",", Ids) + "]";
                LastLog = new LogModel() { Error= new ErrorModel() { ErrorMessage= message, ErrorOccours= DateTime.Now, ErrorSource=GetType().Name }, EventMessage="Error type", EventOccours=DateTime.Now, EventSource= uri };
            }
            else
            {
                //LastLog = null;
                result = true;
            }
            return result;
        }
        public bool CreateCEFromExcel(string excelName)
        {
            bool result= false;
            FileInfo f = new FileInfo(excelName);
            if (excelName.EndsWith(".xlsx"))
            {
                if (excelName.Contains(@"\"))
                {
                    f = new FileInfo(excelName);
                }
                else
                {
                    f = new FileInfo(@".\" + excelName);
                }
            }
            else
            {
                f = new FileInfo(@".\ライセンス資格＆３６５アカウント.xlsx");
            }
            DirectoryInfo d = new DirectoryInfo(f.DirectoryName);
            if (d.Exists)
            {
                if (f.Exists)
                {

                }
            }
            else
            {
                return result;
            }
            return result;
        }
        public string GetLicensesRequrement(string key, string token, string subdomain)
        {
            string result = string.Empty;
            List<LicensesOfMachineKeysFull> results = new List<LicensesOfMachineKeysFull>();
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var master = "LicensesOfMachineKeys";
            Uri requestURL = new Uri("https://" + subdomain + ".vantiq.com/api/v1/resources/custom/" + master);
            var resultJson = string.Empty;
            try
            {
                HttpResponseMessage res = httpClient.GetAsync(requestURL).Result;
                resultJson = res.Content.ReadAsStringAsync().Result;
                var branch = (string.IsNullOrEmpty(MajorVersion) & string.IsNullOrEmpty(MinorVersion)) ? MajorVersion + "." + MinorVersion : "1.2";
                branch = "1.3";
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    results = JsonConvert.DeserializeObject<List<LicensesOfMachineKeysFull>>(resultJson);
                    foreach (var item in results) 
                    {
                        item.Code = item.Licenses;
                        item.Key = item.MachineKey;
                    }
                    if (branch == "1.3")
                    {
                        if (results.Where(r => !string.IsNullOrEmpty(r.Key)).Where(r => key.StartsWith(r.Key)).Count() > 0)
                        {
                            result = string.Join(",", results.Where(r => !string.IsNullOrEmpty(r.Key)).Where(r => key.StartsWith(r.Key)).Select(r => r.Code).ToList());
                        }
                        else 
                        {
                            result = "";
                        }
                    }//将来拡張（バージョンに対応）
                    //else
                    //{
                    //    if (results.Where(r => !string.IsNullOrEmpty(r.Key)).Where(r => key.StartsWith(r.Key)).Count() > 0)
                    //    {
                    //        result = string.Join(",", results);
                    //    }
                    //    else
                    //    {
                    //        result = "";
                    //    }
                    //    //result = (results.Where(r => key.StartsWith(r.Key)).Count() > 0 ? results.Where(r => key.StartsWith(r.Key)).FirstOrDefault().Code : "");
                    //}
                    LastLog = new LogModel() { EventMessage = string.Format("ライセンス情報取得時にVANTIQクラウドから[{0}]が返ってきました。トークン[{1}]URI[{2}]", resultJson, token, requestURL), Memo = results.Count.ToString() };
                }
                else
                {
                    LastLog = new LogModel() { EventMessage = string.Format("ライセンス情報取得時にVANTIQクラウドから[{0}]が返ってきました。トークン[{1}]URI[{2}]", resultJson, token, requestURL), Memo = results.Count.ToString() };
                }
            }
            catch (Exception ex)
            {
                LastLog = new LogModel() { EventMessage = string.Format("ライセンス情報取得時にVANTIQクラウドから[{0}]が返ってきました。トークン[{1}]URI[{2}]", ex.Message, token, requestURL), Memo = results.Count.ToString() };
            }
            return result;
        }
        public string GetLicensesRequrement(string MachineNumber)
        {
            List<string> results = new List<string>();
            string result = string.Empty;
            string licencefile = GetType().Module.FullyQualifiedName.Replace(GetType().Module.Name, "") + @"Assets\MachineLicenses.txt";
            FileInfo f = new FileInfo(licencefile);
            DirectoryInfo d = new DirectoryInfo(f.DirectoryName);
            if (f.Exists)
            {
                var tsvString = File.ReadAllText(f.FullName);
                foreach (string s in Regex.Split(tsvString, @"\r\n"))
                {
                    if(MachineNumber.StartsWith(Regex.Split(s, @"\t").FirstOrDefault()))
                    {
                        if(!string.IsNullOrEmpty(s))
                        {
                            results.Add(Regex.Split(s, @"\t").LastOrDefault());
                        }
                    }
                }
                result = string.Join(",", results);
            }
            else
            {
                return result;
            }
            return result;
        }
        public string GetDivisionsHeader(string DivisionCode)
        {
            List<string> results = new List<string>();
            string result = string.Empty;

            foreach (string s in Regex.Split(TsvDivisionInfo, @"\r\n"))
            {
                if (DivisionCode.StartsWith(Regex.Split(s, @"\t").FirstOrDefault()))
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        results.Add(Regex.Split(s, @"\t").LastOrDefault());
                    }
                }
            }
            result = string.Join(",", results);
            return result;
        }
        public async Task<List<OrganizationFull>> GetDivisionsInfo(string token, string subdomain)
        {
            List<OrganizationFull> results = new List<OrganizationFull>();
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            Uri requestURL = new Uri("https://" + subdomain + ".vantiq.com/api/v1/resources/custom/Division");
            try
            {
                HttpResponseMessage res = await httpClient.GetAsync(requestURL);
                var resultJson = await res.Content.ReadAsStringAsync();
                List<OrganizationInfo> resultSet = new List<OrganizationInfo>();
                if (res.IsSuccessStatusCode)
                {
                    resultSet = JsonConvert.DeserializeObject<List<OrganizationInfo>>(resultJson);
                    foreach (var r in resultSet)
                    {
                        if (!r.DivisionCode.StartsWith("0081A")) continue;
                        results.Add(new OrganizationFull() { DeleteCode = string.Empty, DivisionCode = r.DivisionCode, DepartmentName = r.DepartmentName, DivisionName = r.DivisionName, WebHook = r.WebHook, ResourceAddress = r.ResourceAddress, ChannelID = r.ChannelID, TeamID = r.TeamID });
                    }
                    LastLog = new LogModel() { EventMessage = string.Format("組織情報取得時にVANTIQクラウドから[{0}]が返ってきました。トークン[{1}]URI[{2}]", resultJson, token, requestURL), Memo = resultSet.Count.ToString() };
                }
                else
                {
                    LastLog = new LogModel() { EventMessage = string.Format("組織情報取得時にVANTIQクラウドから[{0}]が返ってきました。トークン[{1}]URI[{2}]", resultJson, token, requestURL) };
                }
            }
            catch (Exception ex)
            {
                LastLog = new LogModel() { EventMessage = string.Format("組織情報取得時に接続鰓0[{0}]が返ってきました。トークン[{1}]URI[{2}]", ex.Message, token, requestURL) };
            }
            return results;
        }
        /// <summary>
        /// 内閣府が公示する国民の休日を取得
        /// </summary>
        /// <param name="Year">yyyy指定年1月1日以降の休日を取得</param>
        /// <remarks>引数未指定の場合は本年1月1日以降の休日を返す</remarks>
        /// <returns>休日の日付型リスト</returns>
        public List<DateTime> GetHolidays(string Year="")
        {
            List<DateTime> results = new List<DateTime>();
            string holidaysfileUrl = @"https://www8.cao.go.jp/chosei/shukujitsu/syukujitsu.csv";
            HttpClient w = new HttpClient();
            var response = w.GetStringAsync(new Uri(holidaysfileUrl)).Result;
            if (string.IsNullOrEmpty(Year)|| Year.Length != 4) Year = DateTime.Now.Year.ToString();
            if (!string.IsNullOrEmpty(response)) 
            {
                foreach (string s in Regex.Split(response, @"\r\n"))
                {
                    if (int.TryParse(Regex.Split(s, @"/").FirstOrDefault(),out var i))
                    {
                        if (i < int.Parse(Year)) continue;
                        if (!string.IsNullOrEmpty(s)) results.Add(Convert.ToDateTime(Regex.Split(s, @",").FirstOrDefault()));
                    }
                }
            }
            return results;
        }
        public List<Tuple<DateTime, string>> GetHolidaysOnCloud(string token, string vantiqCloudSubDomain)
        {
            List<Tuple<DateTime, string>> result = new List<Tuple<DateTime, string>>();
            string uri = "https://" + vantiqCloudSubDomain + ".vantiq.com/api/v1/resources/custom/JapanHoliday";
            Uri requestURL = new Uri(uri);
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            try 
            {
                var res = httpClient.GetAsync(requestURL).Result;
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    List<Tuple<DateTime, string>> resultObject = new List<Tuple<DateTime, string>>();
                    string resultJson = res.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrEmpty(resultJson))
                    {
                        using (JsonTextReader reader = new JsonTextReader(new StringReader(resultJson)))
                        {
                            bool getValue = false;
                            DateTime dateTime = DateTime.MinValue;
                            while (reader.Read())
                            {
                                if (reader.Value != null)
                                {
                                    if (reader.TokenType == JsonToken.PropertyName)
                                    {
                                        if (DateTime.TryParse(reader.Value.ToString(), out dateTime))
                                        {
                                            getValue = true;
                                        }
                                    }
                                    if (reader.TokenType == JsonToken.String && getValue)
                                    {
                                        result.Add(new Tuple<DateTime, string>(dateTime, reader.Value.ToString()));
                                        getValue = false;
                                        dateTime = DateTime.MinValue;
                                    }
                                }
                            }
                        }
                    }
                    if (result.Count == 0) LastLog = new LogModel() { Error = new ErrorModel() { ErrorMessage = string.Format("VANTIQクラウド：{0}に接続し、問い合わせが成功しましたがデータは取得できませんでした。", requestURL) } };
                }
                else
                {
                    var resultJson = res.Content.ReadAsStringAsync().Result;
                    LastLog = new LogModel() { Error = (res.IsSuccessStatusCode ? null : new ErrorModel() { ErrorMessage = resultJson }), EventMessage = res.StatusCode.ToString(), EventSource= uri };
                }
            }
            catch (WebException e) 
            {
                LastLog = new LogModel() { Error = (e.Status == WebExceptionStatus.ProtocolError ? new ErrorModel() { ErrorMessage = e.Message } : new ErrorModel() { ErrorMessage= e.Status.ToString()}), EventMessage = token };
            }
            return result;
        }
        public bool SaveUploadLog(UploadModel log, string path)
        {
            bool result = false;
            string jsonstring = string.Empty;
            FileInfo f = new FileInfo(path);
            List<UploadModel> logs = new List<UploadModel>();
            logs.Add(log);
            if (!f.Directory.Exists) f.Directory.Create();
            if (f.Exists)jsonstring = File.ReadAllText(f.FullName);
            if (!string.IsNullOrEmpty(jsonstring))
            {
                logs.AddRange(JsonConvert.DeserializeObject<List<UploadModel>>(jsonstring));
            }
            using (StreamWriter file = File.CreateText(f.FullName))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, logs);
            }
            result = true;
            return result;
        }
        public bool ModifyUploadLog(string logName, string path)
        {
            bool result = false;
            string jsonstring = string.Empty;
            FileInfo f = new FileInfo(path);
            if (!f.Directory.Exists) f.Directory.Create();
            if (f.Exists) jsonstring = File.ReadAllText(f.FullName);
            if (!string.IsNullOrEmpty(jsonstring))
            {
                List<UploadModel> logs = JsonConvert.DeserializeObject<List<UploadModel>>(jsonstring);
                UploadModel log = logs.Where(l => l.UploadFileName == logName).FirstOrDefault();
                if (log == null) return result;
                if (logs.Where(l => l.UploadFileVersion > log.UploadFileVersion).Count() > 0)
                {
                    foreach (var u in logs)
                    {
                        if (u.UploadFileVersion > log.UploadFileVersion)
                        {
                            u.Rollback = true;
                        }
                    }
                    using (StreamWriter file = File.CreateText(f.FullName))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(file, logs);
                    }
                    result = true;
                }
            }
            return result;
        }
        public List<UploadModel> ReadUploadLog(string path)
        {
            string jsonstring = string.Empty;
            FileInfo f = new FileInfo(path);
            List<UploadModel> logs = new List<UploadModel>();
            if (!f.Directory.Exists) return logs;
            if (f.Exists) jsonstring = File.ReadAllText(f.FullName);
            if (!string.IsNullOrEmpty(jsonstring))
            {
                logs.AddRange(JsonConvert.DeserializeObject<List<UploadModel>>(jsonstring));
            }
            return logs;
        }
        public List<ErrorModel> ReadLog(string path)
        {
            string jsonstring = string.Empty;
            FileInfo f = new FileInfo(path);
            List<ErrorModel> logs = new List<ErrorModel>();
            if (!f.Directory.Exists) return logs;
            if (f.Exists) jsonstring = File.ReadAllText(f.FullName);
            if (!string.IsNullOrEmpty(jsonstring))
            {
                logs.AddRange(JsonConvert.DeserializeObject<List<ErrorModel>>(jsonstring));
            }
            return logs;
        }
        public bool SaveError(List<ErrorModel> logs, string path = "")
        {
            if (string.IsNullOrEmpty(path)) path = CEDLogsPath;
            bool result = false;
            string jsonstring = string.Empty;
            FileInfo f = new FileInfo(path);
            if (!f.Directory.Exists) f.Directory.Create();
            if (f.Exists) jsonstring = File.ReadAllText(f.FullName);
            if (!string.IsNullOrEmpty(jsonstring))
            {
                logs.AddRange(JsonConvert.DeserializeObject<List<ErrorModel>>(jsonstring));
            }
            using (StreamWriter file = File.CreateText(f.FullName))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, logs);
            }
            result = true;
            return result;
        }
        //public bool SaveCautureLog(LogModel logs, string path = "")
        //{
        //    if (string.IsNullOrEmpty(path)) path = CEDLogsPath;
        //    bool result = false;
        //    string jsonstring = string.Empty;
        //    FileInfo f = new FileInfo(path);
        //    if (!f.Directory.Exists) f.Directory.Create();
        //    if (f.Exists) jsonstring = File.ReadAllText(f.FullName);
        //    if (!string.IsNullOrEmpty(jsonstring))
        //    {
        //        logs.AddRange(JsonConvert.DeserializeObject<List<LogModel>>(jsonstring));
        //    }
        //    using (StreamWriter file = File.CreateText(f.FullName))
        //    {
        //        JsonSerializer serializer = new JsonSerializer();
        //        serializer.Serialize(file, logs);
        //    }
        //    result = true;
        //    return result;
        //}
        public bool SaveLog(LogModel log, string path = "")
        {
            if (string.IsNullOrEmpty(path)) path = CEDLogsPath;
            bool result = false;
            List<LogModel> logs = new List<LogModel>();
            logs.Add(log);
            string jsonstring = string.Empty;
            FileInfo f = new FileInfo(path);
            if (!f.Directory.Exists) f.Directory.Create();
            if (f.Exists) jsonstring = File.ReadAllText(f.FullName);
            if (!string.IsNullOrEmpty(jsonstring))
            {
                logs.AddRange(JsonConvert.DeserializeObject<List<LogModel>>(jsonstring));
            }
            using (StreamWriter file = File.CreateText(f.FullName))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, logs);
            }
            result = true;
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from">受電時間：VANTIQクラウドにIncidentタイプのデータが作られた時間で範囲指定したい場合は、「AcceptedDateTime」を「ars_createdAt」に変更</param>
        /// <param name="to"></param>
        /// <param name="division"></param>
        /// <returns></returns>
        public List<IncidentModelStaging> GetIncidents(DateTime from,DateTime to,string departmentName, string token)
        {
            List<IncidentModelStaging> result = new List<IncidentModelStaging>();
            string uri = "https://api.vantiq.com/api/v1/resources/custom/Incident?where={\"$and\":[{\"AcceptedDateTime\":{\"$gte\": \"" + from.ToString("yyyy'-'MM'-'dd'T00:00:00.000Z'") + "\"}},{\"AcceptedDateTime\":{\"$lte\":\"" + to.ToString("yyyy'-'MM'-'dd'T00:00:00.000Z'") + "\"}},{\"DepartmentName\":\"" + departmentName + "\"}]}";
            if (string.IsNullOrEmpty(departmentName))
            {
                uri = "https://api.vantiq.com/api/v1/resources/custom/Incident?where={\"$and\":[{\"AcceptedDateTime\":{\"$gte\": \"" + from.ToString("yyyy'-'MM'-'dd'T00:00:00.000Z'") + "\"}},{\"AcceptedDateTime\":{\"$lte\":\"" + to.ToString("yyyy'-'MM'-'dd'T00:00:00.000Z'") + "\"}}]}";
            }

            Uri requestURL = new Uri(uri);
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var res = httpClient.GetAsync(requestURL).Result;
            LastLog = new LogModel() { Error = (res.StatusCode == HttpStatusCode.OK ? null: new ErrorModel() { ErrorMessage = res.StatusCode.ToString() }), EventMessage = uri };
            var resultJson = res.Content.ReadAsStringAsync().Result;
            result = JsonConvert.DeserializeObject<List<IncidentModelStaging>>(resultJson);
            return result;

        }
        public void SaveBmpToFile(PngBitmapEncoder encoder)
        {
            FileInfo imageFile = new FileInfo(CEDCaptureImagePath);
            DirectoryInfo targetDirectory = imageFile.Directory;
            if (!targetDirectory.Exists) targetDirectory.Create();
            //if(!imageFile.Exists)
            using (FileStream fs = new FileStream(imageFile.FullName, FileMode.Create))
            {
                encoder.Save(fs);
            }
        }
    }
}
