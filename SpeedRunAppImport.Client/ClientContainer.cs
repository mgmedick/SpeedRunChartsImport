using System;

namespace SpeedRunAppImport.Client
{
    public class ClientContainer
    {
        public string UserAgent { get; private set; }
        public string AccessToken { internal get; set; }
        public int MaxCacheElements { get; private set; }
        public TimeSpan? Timeout { get; private set; }

        //Clients
        public CategoriesClient Categories { get; private set; }
        public CommonClient Common { get; private set; }
        public GamesClient Games { get; private set; }
        public GuestsClient Guests { get; private set; }
        public LeaderboardsClient Leaderboards { get; private set; }
        public LevelsClient Levels { get; private set; }
        //public NotificationsClient Notifications { get; private set; }
        public PlatformsClient Platforms { get; private set; }
        public RegionsClient Regions { get; private set; }
        public RunsClient Runs { get; private set; }
        //public SeriesClient Series { get; private set; }
        public UsersClient Users { get; private set; }
        public VariablesClient Variables { get; private set; }

        public ClientContainer(string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.190 Safari/537.36",
            string accessToken = null, int maxCacheElements = 0,
            TimeSpan? timeout = null)
        {
            UserAgent = userAgent;
            AccessToken = accessToken;
            MaxCacheElements = maxCacheElements;
            Timeout = timeout;

            Categories = new CategoriesClient(this);
            Common = new CommonClient(this);
            Games = new GamesClient(this);
            Guests = new GuestsClient(this);
            Leaderboards = new LeaderboardsClient(this);
            Levels = new LevelsClient(this);
            //Notifications = new NotificationsClient(this);
            Platforms = new PlatformsClient(this);
            Regions = new RegionsClient(this);
            Runs = new RunsClient(this);
            //Series = new SeriesClient(this);
            Users = new UsersClient(this);
            Variables = new VariablesClient(this);
        }
    }
}
