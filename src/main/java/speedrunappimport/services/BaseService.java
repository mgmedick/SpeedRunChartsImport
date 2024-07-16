package speedrunappimport.services;

import org.springframework.beans.factory.annotation.Value;

import java.time.Instant;

public abstract class BaseService
{
	@Value("${settings.pullDelayMS}")
	private int pullDelayMS;

	@Value("${settings.errorPullDelayMS}")
	private int errorPullDelayMS;	

	@Value("${settings.maxMemorySizeBytes}")
	private long maxMemorySizeBytes;		

	@Value("${settings.maxPageLimit}")
	private int maxPageLimit;	
	
	@Value("${settings.maxPageLimitSM}")
	private int maxPageLimitSM;		

	@Value("${settings.maxRetryCount}")
	private int maxRetryCount;		

	@Value("${settings.sqlMinDateTime}")
	private Instant sqlMinDateTime;

	public int getPullDelayMS() {
		return pullDelayMS;
	}

	public int getErrorPullDelayMS() {
		return errorPullDelayMS;
	}
	
	public long getMaxMemorySizeBytes() {
		return maxMemorySizeBytes;
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

	
}


