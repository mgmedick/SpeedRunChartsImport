package speedrunappimport.repositories;

import java.time.Instant;
import org.springframework.beans.factory.annotation.Value;

public abstract class BaseRepository
{
	@Value("${settings.maxQueryLimit}")
	public int maxQueryLimit;	
	
	@Value("${settings.sqlMinDateTime}")
	public Instant sqlMinDateTime;
}
