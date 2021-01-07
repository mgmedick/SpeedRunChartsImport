using Microsoft.Extensions.Caching.Memory;
using SpeedRunAppImport.Interfaces.Services;
using SpeedRunAppImport.Interfaces.Repositories;
using SpeedRunAppImport.Model.Entity;
using System.Collections.Generic;
using AngleSharp;
using AngleSharp.Io;
using AngleSharp.Html.Parser;
using System.Linq;
using System.IO;
using AngleSharp.Attributes;

namespace SpeedRunAppImport.Service
{
    public class ScrapeService : BaseService, IScrapeService
    {
        public ISpeedRunRepository _speedRunRepo { get; set; }
        public IConfiguration _config { get; set; }

        public ScrapeService(ISpeedRunRepository speedRunRepo)
        {
            _speedRunRepo = speedRunRepo;
        }

        public IEnumerable<string> GetLatestSpeedRunIDs()
        {
            var requester = new DefaultHttpRequester("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36");
            var angleSharpConfig = Configuration.Default.With(requester).WithDefaultLoader();
            var context = BrowsingContext.New(angleSharpConfig);
            var document = context.OpenAsync(SpeedRunComLatestRunsUrl).Result;
            var runIDs = document.QuerySelectorAll(".linked").Select(i => i.GetAttribute("data-target").Substring(i.GetAttribute("data-target").LastIndexOf('/') + 1)).ToList();
            var existingRunIDs = _speedRunRepo.GetSpeedRuns(i => runIDs.Contains(i.ID)).Select(i => i.ID).ToList();
            var results = runIDs.Where(i => !existingRunIDs.Contains(i)).ToList();

            return results;
        }
    }
}
