using System.Data.SQLite;

namespace EnergyPerformance.Wrappers;
public class DatabaseMethodFactory
{
    public virtual bool FileExists(string file)
    {
        return File.Exists(file);
    }

    public virtual bool DirectoryExists(string directory)
    {
        return Directory.Exists(directory);
    }

    public virtual async Task<bool> DatabaseIsActive(SQLiteConnection conn)
    {
        string query = "SELECT COUNT(*) FROM energy_diary_log";
        SQLiteCommand cmd = new SQLiteCommand(query, conn);
        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        conn.Close();
        return count > 0;
    }
}

