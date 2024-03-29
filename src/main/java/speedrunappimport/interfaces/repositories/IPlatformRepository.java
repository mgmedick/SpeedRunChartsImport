package speedrunappimport.interfaces.repositories;

import java.util.List;

import speedrunappimport.model.entity.*;

public interface IPlatformRepository {
    void SavePlatforms(List<Platform> platforms);
    List<Platform> GetAllPlatforms();
}


