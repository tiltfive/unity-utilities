using System.Collections;
using System.Collections.Generic;
using TiltFive;
using UnityEngine;

public class GlassesDisconnectManager : SingletonComponent<GlassesDisconnectManager>
{
    [SerializeField] bool logging;

    public static event System.Action OnDisconnect;
    public static event System.Action OnReconnect;
    public static bool AnyDisconnected { get; private set; }

    public List<PlayerIndex> playersToWatch = new List<PlayerIndex>();
    public List<PlayerIndex> wandsToWatchRight = new List<PlayerIndex>();
    public List<PlayerIndex> wandsToWatchLeft = new List<PlayerIndex>();
    public List<PlayerIndex> disconnectedPlayers = new List<PlayerIndex>();
    public List<PlayerIndex> disconnectedWands = new List<PlayerIndex>();

    void Update()
    {
        // Detect new wands to watch
        foreach (PlayerIndex player in playersToWatch)
        {
            if (!wandsToWatchRight.Contains(player))
            {
                if (Wand.TryCheckConnected(out bool connected, player, ControllerIndex.Right) && connected)
                {
                    wandsToWatchRight.Add(player);
                    if (logging) Debug.Log($"GlassesDisconnectManager: Added right wand to watch (Player {player})");
                }
            }
            if (!wandsToWatchLeft.Contains(player))
            {
                if (Wand.TryCheckConnected(out bool connected, player, ControllerIndex.Left) && connected)
                {
                    wandsToWatchLeft.Add(player);
                    if (logging) Debug.Log($"GlassesDisconnectManager: Added left wand to watch (Player {player})");
                }
            }
        }

        // Detect player connection changes
        foreach (PlayerIndex player in playersToWatch)
        {
            if (Player.IsConnected(player))
            {
                if (disconnectedPlayers.Contains(player))
                {
                    disconnectedPlayers.Remove(player);
                    AlertReconnect();
                    if (logging) Debug.Log($"GlassesDisconnectManager: Player reconnected (Player {player})");
                }
            }
            else if (!disconnectedPlayers.Contains(player))
            {
                disconnectedPlayers.Add(player);
                AlertDisconnect();
                if (logging) Debug.Log($"GlassesDisconnectManager: Player disconnected (Player {player})");
            }
        }

        // Detect wand connection changes
        foreach (PlayerIndex player in wandsToWatchRight)
        {
            if (Wand.TryCheckConnected(out bool connected, player, ControllerIndex.Right) && connected)
            {
                if (disconnectedWands.Contains(player))
                {
                    disconnectedWands.Remove(player);
                    AlertReconnect();
                    if (logging) Debug.Log($"GlassesDisconnectManager: Right Wand reconnected (Player {player})");
                }
            }
            else if(!disconnectedWands.Contains(player))
            {
                disconnectedWands.Add(player);
                AlertDisconnect();
                if (logging) Debug.Log($"GlassesDisconnectManager: Right Wand disconnected (Player {player})");
            }
        }

        foreach (PlayerIndex player in wandsToWatchLeft)
        {
            if (Wand.TryCheckConnected(out bool connected, player, ControllerIndex.Left) && connected)
            {
                if (disconnectedWands.Contains(player))
                {
                    disconnectedWands.Remove(player);
                    AlertReconnect();
                    if (logging) Debug.Log($"GlassesDisconnectManager: Left Wand reconnected (Player {player})");
                }
            }
            else if (!disconnectedWands.Contains(player))
            {
                disconnectedWands.Add(player);
                AlertDisconnect();
                if (logging) Debug.Log($"GlassesDisconnectManager: Left Wand disconnected (Player {player})");
            }
        }
    }

    void AlertDisconnect()
    {
        if (disconnectedPlayers.Count + disconnectedWands.Count == 1)
        {
            AnyDisconnected = true;
            OnDisconnect?.Invoke();
        }
    }

    void AlertReconnect()
    {
        if (disconnectedPlayers.Count + disconnectedWands.Count == 0)
        {
            AnyDisconnected = false;
            OnReconnect?.Invoke();
        }
    }

    public void AddPlayerToWatch(PlayerIndex playerToWatch)
    {
        if (!playersToWatch.Contains(playerToWatch))
        {
            playersToWatch.Add(playerToWatch);
            if (logging) Debug.Log($"GlassesDisconnectManager: Added player to watch (Player {playerToWatch})");
        }
    }

    public void RemovePlayerToWatch(PlayerIndex playerToWatch)
    {
        if (playersToWatch.Contains(playerToWatch))
        {
            playersToWatch.Remove(playerToWatch);
            if (logging) Debug.Log($"GlassesDisconnectManager: Removed player to watch (Player {playerToWatch})");
        }
    }
}