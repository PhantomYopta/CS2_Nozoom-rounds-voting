using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace NZ;

public class NZ : BasePlugin
{
    public override string ModuleName { get; }
    public override string ModuleVersion { get; }

    private UsersSettings?[] _users = new UsersSettings?[65];
    private Config _config;

    private bool _nzRound;
    private bool _isVoteSuccessful;
    
    private int _countRound = 0;
    private int _countVote = 0;

    public override void Load(bool hotReload)
    {
        _config = LoadConfig();
        RegisterListener<Listeners.OnClientConnected>((slot =>
        {
            _users[slot + 1] = new UsersSettings { IsVoted = false };
        }));
        RegisterListener<Listeners.OnClientDisconnectPost>((slot =>
        {
            if (_users[slot + 1]!.IsVoted) _countVote--;

            _users[slot + 1] = null;
        }));
        AddCommand("css_nz", "", ((player, info) =>
        {
            if (player == null) return;
            if (_isVoteSuccessful)
            {
                if (_nzRound)
                {
                    Server.PrintToChatAll("The NoZoom battle is already happening!");
                    return;
                }

                if (_countRound >= _config.NzRounds && !_nzRound)
                {
                    Server.PrintToChatAll($"You will be able to vote for NoZoom battle after {(_config.NzCooldownRounds + _config.NzRounds)-_countRound} rounds!");
                    return;
                }
                Server.PrintToChatAll("Enough votes have already been collected!");
            }
            if (_users[player.EntityIndex!.Value.Value]!.IsVoted)
            {
                Server.PrintToChatAll("You have already voted for NoZoom battle!");
                return;
            }

            _users[player.EntityIndex!.Value.Value]!.IsVoted = true;
            var successfulVoteCount = Utilities.GetPlayers().Count * _config.NzNeed;
            if ((int)successfulVoteCount == 0) successfulVoteCount = 1.0f;
            _countVote++;
            Server.PrintToChatAll($"{player.PlayerName} voted for NoZoom battle {_countVote}/{(int)successfulVoteCount}");
            if (_countVote == (int)successfulVoteCount)
            {
                _isVoteSuccessful = true;
                Server.PrintToChatAll("NoZoom battle will begin in the next round!");
            }
        }));

        RegisterEventHandler<EventRoundStart>(((@event, info) =>
        {
            if (_nzRound)
            {
                Server.PrintToChatAll("The sights are off this round!");
                if(_config.DisableDeagle)
                {
                    foreach (var player in Utilities.GetPlayers())
                    {
                        foreach (var weapon in player.PlayerPawn.Value.WeaponServices!.MyWeapons)
                        {
                            if (weapon.Value.DesignerName != "weapon_deagle") continue;

                            weapon.Value.Remove();
                            break;
                        }
                    }
                }
            }

            return HookResult.Continue;
        }));

        RegisterEventHandler<EventRoundEnd>(((@event, info) =>
        {
            if (!_isVoteSuccessful) return HookResult.Continue;
            
            if (_countRound >= _config.NzRounds && !_nzRound)
            {
                _countRound++;
                if (_countRound == _config.NzCooldownRounds+_config.NzRounds)
                {
                    _countVote = 0;
                    _countRound = 0;
                    _isVoteSuccessful = false;
                    foreach (var player in Utilities.GetPlayers())
                    {
                        _users[player.EntityIndex!.Value.Value]!.IsVoted = false;
                    }
                    Server.PrintToChatAll("The NoZoom battle is over!");
                }
                return HookResult.Continue;
            }

            if (!_nzRound)
            {
                _nzRound = true;
                _countRound = 0;
                return HookResult.Continue;
            }
            _countRound++;
            if (_countRound == _config.NzRounds)
            {
                _nzRound = false;
            }

            return HookResult.Continue;
        }));

        RegisterEventHandler<EventWeaponZoom>(((@event, info) =>
        {
            if (!_nzRound) return HookResult.Continue;
            var player = @event.Userid;
            var currentWeapon = player.PlayerPawn.Value.WeaponServices!.ActiveWeapon.Value.DesignerName;
            player.PlayerPawn.Value.WeaponServices!.ActiveWeapon.Value.Remove();
            player.GiveNamedItem(currentWeapon);
            return HookResult.Continue;
        }));
    }

    private Config LoadConfig()
    {
        var configPath = Path.Combine(ModuleDirectory, "settings.json");

        if (!File.Exists(configPath)) return CreateConfig(configPath);

        var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath))!;

        return config;
    }

    private Config CreateConfig(string configPath)
    {
        var config = new Config
        {
            NzNeed = 0.6f,
            NzRounds = 4,
            NzCooldownRounds = 4,
            DisableDeagle = false
        };

        File.WriteAllText(configPath,
            JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));

        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("[NoZoom] The configuration was successfully saved to a file: " + configPath);
        Console.ResetColor();

        return config;
    }
}

public class Config
{
    public float NzNeed { get; set; }
    public int NzRounds { get; set; }
    public int NzCooldownRounds { get; set; }
    public bool DisableDeagle { get; set; }
}

public class UsersSettings
{
    public bool IsVoted { get; set; }
}