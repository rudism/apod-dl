using System;
using System.IO;
using System.Text.RegularExpressions;
using CommandLine;

namespace apod_dl
{
    public class Program
    {
        public class Options
        {
            [Option('d', "date", HelpText = "First date to retrieve.", Default = "today")]
            public string Date { get; set; }

            [Option('c', "count", HelpText = "The total number of images to retrieve and retain.", Default = 1)]
            public int Count { get; set; }

            [Option('o', "outdir", Required = true, HelpText = "The directory to download the images to.")]
            public string OutDir { get; set; }

            [Option('f', "fillsize", HelpText = "Resize the image to fill these dimensions (eg. 1920x1080)")]
            public string FillSize { get; set; }

            [Option(Default = false, HelpText = "Suppress all output. Cannot be used with --debug.")]
            public bool Quiet { get; set; }

            [Option(Default = false, HelpText = "Output extra details. Cannot be used with --quiet.")]
            public bool Debug { get; set; }

            [Option(Default = false, HelpText = "Delete old images.")]
            public bool Delete { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(opts =>
            {
                if(opts.Debug && opts.Quiet)
                {
                    new Logger().Error("Cannot specify both --quiet and --debug.");
                }

                var logger = new Logger(
                    opts.Quiet ? Verbosity.Quiet :
                    opts.Debug ? Verbosity.Verbose :
                    Verbosity.Normal
                );

                var chronic = new Chronic.Parser();
                var date = chronic.Parse(opts.Date);

                if(date == null || date.Start == null)
                    logger.Error("Could not parse date.");

                int? width = null, height = null;
                if(opts.FillSize != null)
                {
                    var fill = Regex.Match(opts.FillSize, @"^(?<width>\d+)x(?<height>\d+)$");
                    if(!fill.Success) logger.Error("Could not parse fillsize.");

                    width = int.Parse(fill.Groups["width"].Value);
                    height = int.Parse(fill.Groups["height"].Value);
                }

                if(File.Exists(opts.OutDir))
                    logger.Error("Output directory cannot be a file.");

                if(Directory.Exists(opts.OutDir))
                    logger.Debug("Output directory already exists.");
                else
                {
                    try
                    {
                        logger.Debug("Output directory does not exist, creating...");
                        Directory.CreateDirectory(opts.OutDir);
                    }
                    catch(Exception ex)
                    {
                        logger.Error("Could not create output directory", ex);
                    }
                }

                logger.Log($"Retrieving {opts.Count} pictures, ending on {date.Start?.ToString("yyyy-MM-dd")}.");

                var downloader = new Downloader(logger, date.Start.Value, opts.Count, opts.OutDir, opts.Delete, width, height);

                downloader.Synchronize();
            });
        }
    }
}
