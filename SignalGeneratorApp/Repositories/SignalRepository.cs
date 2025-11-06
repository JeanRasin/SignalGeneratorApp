using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using SignalGeneratorApp.Models;
using SignalGeneratorApp.Models.Enums;

namespace SignalGeneratorApp.Repositories;

/// <summary>
/// Реализация репозитория сигналов с использованием SQLite.
/// </summary>
public class SignalRepository : ISignalRepository
{

    #region SQL

    /// <summary>
    /// SQL команда для создания таблицы Signals (Сигналы)
    /// </summary>
    private const string CreateSignalsTableSql = """
        CREATE TABLE IF NOT EXISTS Signals (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Type INTEGER NOT NULL,
            Amplitude REAL NOT NULL,
            Frequency REAL NOT NULL,
            Phase REAL,
            TimeInterval REAL NOT NULL,
            CreatedAt TEXT NOT NULL
        )
        """;

    /// <summary>
    /// SQL команда для создания таблицы SignalPoints (Точки сигнала)
    /// Содержит дискретные точки сигналов с временными метками и значениями
    /// Связана с таблицей Signals через внешний ключ SignalId с каскадным удалением
    /// </summary>
    private const string CreatePointsTableSql = """
        CREATE TABLE IF NOT EXISTS SignalPoints (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            SignalId INTEGER NOT NULL,
            Time REAL NOT NULL,
            Value REAL NOT NULL,
            FOREIGN KEY (SignalId) REFERENCES Signals(Id) ON DELETE CASCADE
        )
        """;

    /// <summary>
    /// SQL команда для вставки нового сигнала в таблицу Signals
    /// Возвращает идентификатор созданной записи через last_insert_rowid()
    /// </summary>
    private const string InsertSignalSql = """
        INSERT INTO Signals (Type, Amplitude, Frequency, Phase, TimeInterval, CreatedAt)
        VALUES (@type, @amplitude, @frequency, @phase, @timeInterval, @createdAt);
        SELECT last_insert_rowid();
        """;

    /// <summary>
    /// SQL команда для вставки точек сигнала в таблицу SignalPoints
    /// Использует пакетную вставку для улучшения производительности
    /// </summary>
    private const string InsertPointSql = "INSERT INTO SignalPoints (SignalId, Time, Value) VALUES (@signalId, @time, @value)";

    /// <summary>
    /// SQL команда для загрузки всех сигналов с подсчетом количества точек для каждого сигнала
    /// Использует LEFT JOIN для включения сигналов без точек и GROUP BY для агрегации
    /// Сортировка по дате создания в порядке убывания (новые первыми)
    /// </summary>
    private const string LoadSignalsSql = """
        SELECT 
            s.Id, 
            s.Type, 
            s.Amplitude, 
            s.Frequency, 
            s.Phase, 
            s.TimeInterval, 
            s.CreatedAt,
            COUNT(p.Id) AS PointCount
        FROM Signals s
        LEFT JOIN SignalPoints p ON s.Id = p.SignalId
        GROUP BY s.Id, s.Type, s.Amplitude, s.Frequency, s.Phase, s.TimeInterval, s.CreatedAt
        ORDER BY s.CreatedAt DESC
        """;

    /// <summary>
    /// SQL команда для загрузки всех точек конкретного сигнала
    /// Точки сортируются по времени для правильного порядка воспроизведения/отображения
    /// </summary>
    private const string LoadSignalPointsSql = "SELECT Time, Value FROM SignalPoints WHERE SignalId = @signalId ORDER BY Time";

    /// <summary>
    /// SQL команда для удаления сигнала по идентификатору
    /// Благодаря ON DELETE CASCADE в внешнем ключе, автоматически удаляются связанные точки
    /// </summary>
    private const string DeleteSignalSql = "DELETE FROM Signals WHERE Id = @id";

    #endregion

    private readonly string _connectionString;

    public SignalRepository(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(_connectionString))
            throw new InvalidOperationException("Строка подключения 'DefaultConnection' не найдена или пуста в конфигурации.");

        InitializeDatabase();
    }

    /// <summary>
    /// Инициализирует базу данных и создает таблицы при необходимости
    /// </summary>
    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        ExecuteCommand(connection, CreateSignalsTableSql);
        ExecuteCommand(connection, CreatePointsTableSql);
    }

    /// <summary>
    /// Выполняет SQL команду без возврата результата
    /// </summary>
    private static void ExecuteCommand(SqliteConnection connection, string commandText)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = commandText;
        cmd.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public async Task SaveSignalAsync(Signal signal, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        await using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = transaction;
            cmd.CommandText = InsertSignalSql;
            cmd.Parameters.AddWithValue("@type", (int)signal.Type);
            cmd.Parameters.AddWithValue("@amplitude", signal.Amplitude);
            cmd.Parameters.AddWithValue("@frequency", signal.Frequency);
            cmd.Parameters.AddWithValue("@phase", signal.Phase);
            cmd.Parameters.AddWithValue("@timeInterval", signal.TimeInterval);
            cmd.Parameters.AddWithValue("@createdAt", signal.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            signal.Id = Convert.ToInt64(result);
        }

        await using (var pointCmd = connection.CreateCommand())
        {
            pointCmd.Transaction = transaction;
            pointCmd.CommandText = InsertPointSql;
            pointCmd.Parameters.Add("@signalId", SqliteType.Integer);
            pointCmd.Parameters.Add("@time", SqliteType.Real);
            pointCmd.Parameters.Add("@value", SqliteType.Real);

            foreach (var point in signal.Points)
            {
                pointCmd.Parameters["@signalId"].Value = signal.Id;
                pointCmd.Parameters["@time"].Value = point.Time;
                pointCmd.Parameters["@value"].Value = point.Value;
                await pointCmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        await transaction.CommitAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<Signal>> LoadSignalsAsync(CancellationToken cancellationToken = default)
    {
        var signals = new List<Signal>();
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = LoadSignalsSql;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var signal = new Signal
            {
                Id = reader.GetInt32(0),
                Type = (SignalTypeEnum)reader.GetInt32(1),
                Amplitude = reader.GetDouble(2),
                Frequency = reader.GetDouble(3),
                Phase = reader.GetDouble(4),
                TimeInterval = reader.GetDouble(5),
                CreatedAt = DateTime.Parse(reader.GetString(6)),
                PointCount = reader.GetInt32(7)
            };

            signals.Add(signal);
        }

        return signals;
    }

    /// <inheritdoc/>
    public async Task<List<SignalPoint>> LoadSignalPointsAsync(long id, CancellationToken cancellationToken = default)
    {
        var points = new List<SignalPoint>();
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = LoadSignalPointsSql;
        cmd.Parameters.AddWithValue("@signalId", id);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            points.Add(new SignalPoint(Time: reader.GetDouble(0), Value: reader.GetDouble(1)));
        }

        return points;
    }

    /// <inheritdoc/>
    public async Task DeleteSignalAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = DeleteSignalSql;
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}