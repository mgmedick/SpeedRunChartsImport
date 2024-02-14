package speedrunappimport.services;

import java.time.Instant;

import org.springframework.beans.factory.annotation.Value;

public abstract class BaseService
{
	@Value("${settings.pullDelayMS}")
	public int pullDelayMS;

	@Value("${settings.errorPullDelayMS}")
	public int errorPullDelayMS;	

	@Value("${settings.maxMemorySizeBytes}")
	public long maxMemorySizeBytes;		

	@Value("${settings.maxPageLimit}")
	public int maxPageLimit;	
	
	@Value("${settings.maxPageLimitSM}")
	public int maxPageLimitSM;		

	@Value("${settings.maxRetryCount}")
	public int maxRetryCount;		

	@Value("${settings.sqlMinDateTime}")
	public Instant sqlMinDateTime;
}
