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
using System.Threading.Tasks;
using System;

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
            var document = Task.Run(async () => await context.OpenAsync(SpeedRunComLatestRunsUrl)).Result;
            var speedRunComIDs = document.QuerySelectorAll(".linked").Select(i => i.GetAttribute("data-target").Substring(i.GetAttribute("data-target").LastIndexOf('/') + 1)).ToList();
            var existingSpeedRunComIDs = _speedRunRepo.GetSpeedRunSpeedRunComIDs(i => speedRunComIDs.Contains(i.SpeedRunComID));
            var results = speedRunComIDs.Where(i => !existingSpeedRunComIDs.Any(g => g.SpeedRunComID == i)).ToList();

            return results;
        }

        public int? GetYouTubeViewCount(string videoLinkUrl)
        {
            int? viewCount = null;
            var requester = new DefaultHttpRequester("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36");
            var angleSharpConfig = Configuration.Default.With(requester).WithDefaultLoader();
            var context = BrowsingContext.New(angleSharpConfig);
            var document = Task.Run(async () => await context.OpenAsync(videoLinkUrl)).Result;
            var viewCountString = document.QuerySelectorAll("meta").Where(i => i.GetAttribute("itemprop") == "interactionCount").Select(i => i.GetAttribute("content")).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(viewCountString))
            {
                viewCount = Convert.ToInt32(viewCountString);
            }

            return viewCount;
        }
    }
}
