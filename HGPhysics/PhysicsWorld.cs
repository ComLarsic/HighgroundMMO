using System.Numerics;

namespace HGPhysics;

/// <summary>
/// The physics world for the game.
/// This handles the physics of the game.
/// </summary>
public class PhyscisWorld
{
    /// <summary>
    /// The objects in the world.
    /// </summary>
    public List<PhysicsObject> Objects { get; } = [];

    /// <summary>
    /// Get the colliders in the world.
    /// </summary>
    public List<BoxCollider> Colliders { get; } = [];

    /// <summary>
    /// Add an object to the world.
    /// </summary>
    /// <param name="position">The position of the object.</param>
    /// <param name="velocity">The velocity of the object.</param>
    /// <param name="collider">The collider of the object.</param>
    /// <returns>The object that was added.</returns>
    public PhysicsObject AddObject(Vector2 position, PhysicsObjectType type, Vector2 velocity)
    {
        var obj = new PhysicsObject
        {
            Type = type,
            Position = position,
            Velocity = velocity,
        };
        Objects.Add(obj);
        return obj;
    }

    /// <summary>
    /// Remove an object from the world.
    /// </summary>
    /// <param name="obj">The object to remove.</param>
    /// <returns>Whether the object was removed.</returns>
    public bool RemoveObject(PhysicsObject obj)
        => Objects.Remove(obj);

    /// <summary>
    /// Update the world every game tick.
    /// </summary>
    /// <param name="deltaTime">The time since the last update.</param>
    /// <returns>The entities that have moved.</returns>
    public List<PhysicsObject> Update(double deltaTime)
    {
        const float gravity = 9.8f;

        var movedObjects = new List<PhysicsObject>();
        foreach (var obj in Objects)
        {
            // Only update dynamic objects.
            if (obj.Type == PhysicsObjectType.Static)
                continue;

            // Reset trackers
            obj.IsOnGround = false;

            // Add gravity to the object.
            obj.Velocity += new Vector2(0, gravity * obj.Mass * (float)deltaTime);

            // Calculate the new position of the object.
            var newPosition = obj.Position + obj.Velocity;

            // Check if the object is colliding with any other colliders.
            if (obj.ColliderId != null)
            {
                var collider = Colliders.First(c => c.Id == obj.ColliderId);

                // Check if the entity is colliding in the x direction.
                if (Colliders.Where(c => c.Id != obj.ColliderId).Any(c => c.IsCollidingWith(new BoxCollider
                {
                    Position = new Vector2(newPosition.X, obj.Position.Y),
                    Size = collider.Size
                })))
                {
                    obj.Velocity = new Vector2(0, obj.Velocity.Y);
                    newPosition = new Vector2(obj.Position.X, newPosition.Y);
                }
                // Check if the entity is colliding in the y direction.
                var newYCollider = new BoxCollider
                {
                    Position = new Vector2(obj.Position.X, newPosition.Y),
                    Size = collider.Size
                };
                if (Colliders.Where(c => c.Id != obj.ColliderId).Any(c => c.IsCollidingWith(newYCollider)))
                {
                    if (obj.Velocity.Y > 0)
                        obj.IsOnGround = true;
                    obj.Velocity = new Vector2(obj.Velocity.X, 0);
                    newPosition = new Vector2(newPosition.X, obj.Position.Y);
                }

                // Update the position of the collider.
                collider.Position = newPosition;
            }

            // Set the new position of the object.
            obj.Position = newPosition;

            // Add the object to the list of moved objects.
            movedObjects.Add(obj);
        }
        return movedObjects;
    }
}
