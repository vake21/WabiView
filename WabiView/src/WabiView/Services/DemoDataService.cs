using WabiView.Data;
using WabiView.Models;

namespace WabiView.Services;

/// <summary>
/// Seeds demo data for local preview without Bitcoin Core/Electrs.
/// </summary>
public static class DemoDataService
{
    public static async Task SeedDemoDataAsync(WabiViewDbContext db)
    {
        // Check if already seeded
        if (db.Coordinators.Any())
            return;

        // Add coordinators
        var kruw = new Coordinator
        {
            Name = "Kruw",
            Url = "https://coinjoin.kruw.io/",
            IsOnline = true,
            LastSeen = DateTime.UtcNow.AddMinutes(-2),
            LastChecked = DateTime.UtcNow,
            FeeRate = 0.003m,
            MinInputCount = 5
        };

        var openCoord = new Coordinator
        {
            Name = "OpenCoordinator",
            Url = "https://api.opencoordinator.org/",
            IsOnline = true,
            LastSeen = DateTime.UtcNow.AddMinutes(-1),
            LastChecked = DateTime.UtcNow,
            FeeRate = 0.005m,
            MinInputCount = 5
        };

        db.Coordinators.AddRange(kruw, openCoord);
        await db.SaveChangesAsync();

        // Add sample rounds
        var activeRound = new Round
        {
            RoundId = "demo-round-" + Guid.NewGuid().ToString()[..8],
            CoordinatorId = kruw.Id,
            Phase = RoundPhase.InputRegistration,
            InputCount = 12,
            CreatedAt = DateTime.UtcNow.AddMinutes(-3),
            UpdatedAt = DateTime.UtcNow
        };

        var completedRound = new Round
        {
            RoundId = "demo-round-" + Guid.NewGuid().ToString()[..8],
            CoordinatorId = openCoord.Id,
            Phase = RoundPhase.Ended,
            InputCount = 47,
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-45),
            EndedAt = DateTime.UtcNow.AddMinutes(-45),
            IsSuccessful = true,
            TxId = "a1b2c3d4e5f6789012345678901234567890abcdef1234567890abcdef123456"
        };

        db.Rounds.AddRange(activeRound, completedRound);
        await db.SaveChangesAsync();

        // Add sample coinjoins
        var coinjoins = new List<CoinjoinTransaction>
        {
            new()
            {
                TxId = "f4a2b8c1d9e7654321fedcba0987654321fedcba0987654321fedcba09876543",
                BlockHash = "00000000000000000001a2b3c4d5e6f7890abcdef1234567890abcdef1234567",
                BlockHeight = 831542,
                FirstSeen = DateTime.UtcNow.AddHours(-2),
                ConfirmedAt = DateTime.UtcNow.AddHours(-1.5),
                InputCount = 52,
                OutputCount = 104,
                TotalInputValue = 523400000,
                TotalOutputValue = 523100000,
                FeePaid = 300000,
                VSize = 12450,
                FeeRate = 24.1m,
                CoordinatorId = kruw.Id,
                Confirmations = 6
            },
            new()
            {
                TxId = "b7c8d9e0f1a2345678901234567890abcdef1234567890abcdef123456789012",
                BlockHash = "00000000000000000002b3c4d5e6f7890abcdef1234567890abcdef12345678",
                BlockHeight = 831540,
                FirstSeen = DateTime.UtcNow.AddHours(-4),
                ConfirmedAt = DateTime.UtcNow.AddHours(-3.5),
                InputCount = 38,
                OutputCount = 76,
                TotalInputValue = 412000000,
                TotalOutputValue = 411750000,
                FeePaid = 250000,
                VSize = 9800,
                FeeRate = 25.5m,
                CoordinatorId = openCoord.Id,
                Confirmations = 8
            },
            new()
            {
                TxId = "c9d0e1f2a3b4567890123456789012345678901234567890123456789abcdef0",
                BlockHash = "00000000000000000003c4d5e6f7890abcdef1234567890abcdef123456789",
                BlockHeight = 831538,
                FirstSeen = DateTime.UtcNow.AddHours(-6),
                ConfirmedAt = DateTime.UtcNow.AddHours(-5.5),
                InputCount = 65,
                OutputCount = 130,
                TotalInputValue = 892000000,
                TotalOutputValue = 891500000,
                FeePaid = 500000,
                VSize = 18200,
                FeeRate = 27.5m,
                CoordinatorId = kruw.Id,
                Confirmations = 10
            },
            new()
            {
                TxId = "d0e1f2a3b4c5678901234567890123456789012345678901234567890abcdef1",
                BlockHeight = null,
                FirstSeen = DateTime.UtcNow.AddMinutes(-15),
                InputCount = 41,
                OutputCount = 82,
                TotalInputValue = 315000000,
                TotalOutputValue = 314800000,
                FeePaid = 200000,
                VSize = 10500,
                FeeRate = 19.0m,
                CoordinatorId = openCoord.Id,
                Confirmations = 0
            },
            new()
            {
                TxId = "e1f2a3b4c5d6789012345678901234567890123456789012345678901abcdef2",
                BlockHeight = null,
                FirstSeen = DateTime.UtcNow.AddMinutes(-5),
                InputCount = 28,
                OutputCount = 56,
                TotalInputValue = 198000000,
                TotalOutputValue = 197850000,
                FeePaid = 150000,
                VSize = 7200,
                FeeRate = 20.8m,
                CoordinatorId = kruw.Id,
                Confirmations = 0
            },
            new()
            {
                TxId = "a2b3c4d5e6f70011223344556677889900aabbccddeeff00112233445566aa77",
                BlockHash = "00000000000000000004d5e6f7890abcdef1234567890abcdef1234567890123",
                BlockHeight = 831536,
                FirstSeen = DateTime.UtcNow.AddHours(-8),
                ConfirmedAt = DateTime.UtcNow.AddHours(-7.5),
                InputCount = 34,
                OutputCount = 68,
                TotalInputValue = 445000000,
                TotalOutputValue = 444700000,
                FeePaid = 300000,
                VSize = 11200,
                FeeRate = 26.8m,
                CoordinatorId = openCoord.Id,
                Confirmations = 14
            },
            new()
            {
                TxId = "b3c4d5e6f7a80011223344556677889900aabbccddeeff00112233445566bb88",
                BlockHash = "00000000000000000005e6f7890abcdef1234567890abcdef12345678901234a",
                BlockHeight = 831534,
                FirstSeen = DateTime.UtcNow.AddHours(-10),
                ConfirmedAt = DateTime.UtcNow.AddHours(-9.5),
                InputCount = 71,
                OutputCount = 142,
                TotalInputValue = 1023000000,
                TotalOutputValue = 1022400000,
                FeePaid = 600000,
                VSize = 21500,
                FeeRate = 27.9m,
                CoordinatorId = kruw.Id,
                Confirmations = 18
            },
            new()
            {
                TxId = "c4d5e6f7a8b90011223344556677889900aabbccddeeff00112233445566cc99",
                BlockHash = "00000000000000000006f7890abcdef1234567890abcdef123456789012345ab",
                BlockHeight = 831530,
                FirstSeen = DateTime.UtcNow.AddHours(-14),
                ConfirmedAt = DateTime.UtcNow.AddHours(-13.5),
                InputCount = 45,
                OutputCount = 90,
                TotalInputValue = 567000000,
                TotalOutputValue = 566650000,
                FeePaid = 350000,
                VSize = 13400,
                FeeRate = 26.1m,
                CoordinatorId = openCoord.Id,
                Confirmations = 25
            },
            new()
            {
                TxId = "d5e6f7a8b9c00011223344556677889900aabbccddeeff00112233445566dd00",
                BlockHash = "00000000000000000007890abcdef1234567890abcdef1234567890123456abc",
                BlockHeight = 831525,
                FirstSeen = DateTime.UtcNow.AddHours(-18),
                ConfirmedAt = DateTime.UtcNow.AddHours(-17.5),
                InputCount = 58,
                OutputCount = 116,
                TotalInputValue = 782000000,
                TotalOutputValue = 781550000,
                FeePaid = 450000,
                VSize = 16800,
                FeeRate = 26.8m,
                CoordinatorId = kruw.Id,
                Confirmations = 32
            },
            new()
            {
                TxId = "e6f7a8b9c0d10011223344556677889900aabbccddeeff00112233445566ee11",
                BlockHash = "00000000000000000008abcdef1234567890abcdef1234567890123456789abc",
                BlockHeight = 831520,
                FirstSeen = DateTime.UtcNow.AddHours(-22),
                ConfirmedAt = DateTime.UtcNow.AddHours(-21.5),
                InputCount = 39,
                OutputCount = 78,
                TotalInputValue = 401000000,
                TotalOutputValue = 400750000,
                FeePaid = 250000,
                VSize = 10100,
                FeeRate = 24.8m,
                CoordinatorId = openCoord.Id,
                Confirmations = 40
            },
            new()
            {
                TxId = "f7a8b9c0d1e20011223344556677889900aabbccddeeff00112233445566ff22",
                BlockHeight = null,
                FirstSeen = DateTime.UtcNow.AddMinutes(-8),
                InputCount = 33,
                OutputCount = 66,
                TotalInputValue = 287000000,
                TotalOutputValue = 286820000,
                FeePaid = 180000,
                VSize = 8600,
                FeeRate = 20.9m,
                CoordinatorId = kruw.Id,
                Confirmations = 0
            },
            new()
            {
                TxId = "a8b9c0d1e2f30011223344556677889900aabbccddeeff001122334455660033",
                BlockHeight = null,
                FirstSeen = DateTime.UtcNow.AddMinutes(-2),
                InputCount = 25,
                OutputCount = 50,
                TotalInputValue = 176000000,
                TotalOutputValue = 175860000,
                FeePaid = 140000,
                VSize = 6400,
                FeeRate = 21.9m,
                CoordinatorId = openCoord.Id,
                Confirmations = 0
            }
        };

        db.CoinjoinTransactions.AddRange(coinjoins);
        await db.SaveChangesAsync();
    }
}
