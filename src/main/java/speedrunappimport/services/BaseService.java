package speedrunappimport.services;

import org.springframework.beans.factory.annotation.Value;

import java.time.Instant;

public abstract class BaseService
{
	@Value("${settings.pullDelayMS}")
	private int pullDelayMS;

	@Value("${settings.errorPullDelayMS}")
	private int errorPullDelayMS;	
	
	@Value("${settings.maxRecordCount}")
	private long maxRecordCount;		

	@Value("${settings.maxPageLimit}")
	private int maxPageLimit;	
	
	@Value("${settings.maxPageLimitSM}")
	private int maxPageLimitSM;		

	@Value("${settings.maxRetryCount}")
	private int maxRetryCount;		

	@Value("${settings.sqlMinDateTime}")
	private Instant sqlMinDateTime;

	@Value("${settings.hashKey}")
	private String hashKey;	

	@Value("${settings.youTubeAPIDailyRequestLimit}")
	private int youTubeAPIDailyRequestLimit;	

	@Value("${settings.youTubeAPIMaxBatchCount}")
	private int youTubeAPIMaxBatchCount;	

	@Value("${settings.youTubeAPIKey}")
	private String youTubeAPIKey;		

	public int getPullDelayMS() {
		return pullDelayMS;
	}

	public int getErrorPullDelayMS() {
		return errorPullDelayMS;
	}
	
	public long getMaxRecordCount() {
		return maxRecordCount;
	}

	public int getMaxPageLimit() {
		return maxPageLimit;
	}

	public int getMaxPageLimitSM() {
		return maxPageLimitSM;
	}

	public int getMaxRetryCount() {
		return maxRetryCount;
	}

	public Instant getSqlMinDateTime() {
		return sqlMinDateTime;
	}
	
	public String getHashKey() {
		return hashKey;
	}

	public int getYouTubeAPIDailyRequestLimit() {
		return youTubeAPIDailyRequestLimit;
	}	

	public int getYouTubeAPIMaxBatchCount() {
		return youTubeAPIMaxBatchCount;
	}		

	public String getYouTubeAPIKey() {
		return youTubeAPIKey;
	}	
}


