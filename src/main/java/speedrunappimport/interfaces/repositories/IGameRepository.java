package speedrunappimport.interfaces.repositories;

import java.util.ArrayList;
import speedrunappimport.model.Data.*;

public interface IGameRepository
{
	boolean SaveGames(ArrayList<Game> games);
}
