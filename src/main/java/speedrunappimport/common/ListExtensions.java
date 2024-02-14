package speedrunappimport.common;


import java.util.ArrayList;

public class ListExtensions
{
    public static <T> void clearMemory(ArrayList<T> list)
    {
        //int id = System.gc().getGeneration(list);
        list.clear();
        System.gc();
    }
}
