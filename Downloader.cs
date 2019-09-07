using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace apod_dl
{
    public class Downloader
    {
        const string BASE_URL = "https://apod.nasa.gov";
        static Regex ImageRegex = new Regex(@"<a href=""(?<href>image/[^""]+)""", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex FileRegex = new Regex(@"/apod-(?<year>\d\d)(?<month>\d\d)(?<day>\d\d)\.jpg$", RegexOptions.Compiled);

        public DateTime Date { get; private set; }
        public int Count { get; private set; }
        public string OutDir { get; private set; }
        public Logger Logger { get; private set; }
        public int? Width { get; private set; }
        public int? Height { get; private set; }
        public bool Delete { get; private set; }

        public Downloader(Logger logger, DateTime date, int count, string outDir, bool delete, int? width, int? height)
        {
            Logger = logger;
            Date = date;
            Count = count;
            OutDir = outDir;
            Delete = delete;
            Width = width;
            Height = height;
        }

        public void Synchronize()
        {
            var curCount = Count;
            var newCount = 0;
            var curDay = Date;
            var maxLoops = 10;
            while((newCount = SynchronizeDays(curDay, curCount)) > 0)
            {
                curDay = curDay.AddDays(curCount * -1);
                curCount = newCount;
                if(--maxLoops <= 0)
                    Logger.Error("Too many missing images, bailing.");
            }
            if(newCount == 0 && Delete)
            {
                var deleted = 0;
                var oldest = curDay.AddDays(curCount * -1);
                Logger.Debug($"Deleting pictures from {oldest.ToString("yyyy-MM-dd")} and earlier...");
                foreach(var file in Directory.GetFiles(OutDir))
                {
                    var fmatch = FileRegex.Match(file);
                    if(fmatch.Success)
                    {
                        try
                        {
                            var year = int.Parse(fmatch.Groups["year"].Value);
                            var month = int.Parse(fmatch.Groups["month"].Value);
                            var day = int.Parse(fmatch.Groups["day"].Value);

                            year += year >= 95 ? 1900 : 2000;

                            var fdate = new DateTime(year, month, day);
                            if(fdate <= oldest)
                            {
                                Logger.Debug($"Deleting {file}.");
                                File.Delete(file);
                                deleted++;
                            }
                        }
                        catch(Exception ex)
                        {
                            Logger.Error($"Couldn't process {file}", ex, false);
                        }
                    }
                }
                if(deleted > 0)
                    Logger.Log($"Deleted {deleted} old pictures.");
            }
        }

        private int SynchronizeDays(DateTime ending, int count)
        {
            var days = Enumerable.Range(0, count).Select(i => ending.AddDays(i * -1)).ToArray();
            int missing = 0;

            foreach(var day in days)
            {
                var daystr = day.ToString("yyMMdd");
                var fname = $"apod-{daystr}.jpg";
                var path = Path.Combine(OutDir, fname);
                if(!File.Exists(path))
                {
                    var pageurl = $"{BASE_URL}/apod/ap{daystr}.html";
                    Logger.Debug($"Fetching page {pageurl}...");

                    using(var wc = new WebClient())
                    {
                        try
                        {
                            var content = wc.DownloadString(pageurl);
                            var img = ImageRegex.Match(content);
                            if(img.Success)
                            {
                                var imgurl = $"{BASE_URL}/{img.Groups["href"].Value}";
                                Logger.Debug($"Downloading image {imgurl}...");

                                var bytes = wc.DownloadData(imgurl);
                                Logger.Debug("Finished downloading image.");
                                Util.SaveImage(Logger, path, bytes, Width, Height);
                            }
                            else
                            {
                                missing++;
                                Logger.Log($"No image found for {day.ToString("yyyy-MM-dd")}.");
                            }
                        }
                        catch(Exception ex)
                        {
                            missing++;
                            Logger.Error($"Failed to fetch page {pageurl}", ex, false);
                        }
                    }
                }
                else
                    Logger.Log($"{fname} already exists, skipping.");
            }

            return missing;
        }
    }
}
