package speedrunappimport.repository;

import java.util.ArrayList;

import speedrunappimport.interfaces.repositories.*;
import speedrunappimport.model.Data.*;

public class GameRepository implements IGameRepository
{
	public boolean SaveGames(ArrayList<Game> games)
	{
		return true;
	}
}
