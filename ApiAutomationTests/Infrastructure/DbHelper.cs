using ApiAutomationTests.Tests;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiAutomationTests.Infrastructure;

public static class DbHelper
{
    private static string ConnectionString => GlobalDbSetup.DbContainer.GetConnectionString();

    /// <summary>
    /// Выполняет SQL-запрос без возврата данных (INSERT/UPDATE/DELETE).
    /// </summary>
    public static async Task<int> ExecuteAsync(string sql, Dictionary<string, object?>? parameters = null)
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        await using var cmd = CreateCommand(sql, conn, parameters);
        return await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Выполняет SQL-запрос и возвращает одиночное скалярное значение (например, RETURNING id или COUNT(*)).
    /// </summary>
    public static async Task<T?> ExecuteScalarAsync<T>(string sql, Dictionary<string, object?>? parameters = null)
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        await using var cmd = CreateCommand(sql, conn, parameters);

        var result = await cmd.ExecuteScalarAsync();
        if (result is null or DBNull) return default;

        return (T)Convert.ChangeType(result, typeof(T));
    }

    private static NpgsqlCommand CreateCommand(string sql, NpgsqlConnection conn, Dictionary<string, object?>? parameters)
    {
        var cmd = new NpgsqlCommand(sql, conn);
        if (parameters != null)
        {
            foreach (var (key, value) in parameters)
            {
                cmd.Parameters.AddWithValue(key, value ?? DBNull.Value);
            }
        }
        return cmd;
    }
}
