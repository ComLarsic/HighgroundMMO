using System.Text.Json.Serialization;

namespace HGWorld;

/// <summary>
/// Keeps track of the updates to the world.
/// This is used to send updates to the clients.
/// </summary>
[Serializable]
public struct WorldUpdates
{
    /// <summary>
    /// An empty world updates object.
    /// </summary>
    public static WorldUpdates Empty => new()
    {
        AddedEntities = [],
        RemovedEntities = [],
        UpdatedEntities = []
    };

    /// <summary>
    /// The time since the last update.
    /// This is used to interpolate the position of entities.
    /// </summary>
    [JsonPropertyName("deltaTime")]
    public double DeltaTime { get; set; }

    /// <summary>
    /// The entities that were added to the world.
    /// </summary>
    [JsonPropertyName("addedEntities")]
    public List<Entity> AddedEntities { get; set; }

    /// <summary>
    /// The entities that were removed from the world.
    /// </summary>
    [JsonPropertyName("removedEntities")]
    public List<Guid> RemovedEntities { get; set; }

    /// <summary>
    /// The entities that were updated in the world.
    /// </summary>
    [JsonPropertyName("updatedEntities")]
    public List<Entity> UpdatedEntities { get; set; }

    /// <summary>
    /// Check if the world updates are empty.
    /// </summary>
    /// <returns></returns>
    public readonly bool IsEmpty => AddedEntities.Count == 0 && RemovedEntities.Count == 0 && UpdatedEntities.Count == 0;
}
