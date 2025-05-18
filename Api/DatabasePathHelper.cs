namespace Api;

public class DatabasePathHelper
{
    public static string GetDatabasePath(string relativePath = "SQLLiteDatabase.db")
    {
        // In a Docker container, Directory.GetCurrentDirectory() will resolve to your working directory (set via WORKDIR)
        string currentDirectory = Directory.GetCurrentDirectory();
        // Compose the absolute path to your database file
        string fullPath = Path.Combine(currentDirectory, relativePath);
        return fullPath;
    }
}