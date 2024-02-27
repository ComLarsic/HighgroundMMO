local id

function Init(world)
    world:SpawnStaticEntity(-1000 / 2, 500, 1000, 32)

    local entity = world:SpawnDynamicEntity(0, 550, 96, 128)
    id = entity.Id
    entity.PhysicsObject.Velocity = Vector2(10, 0)
    entity.PhysicsObject.Mass = 0
end

function Update(world, delta)
    local entity = world:GetEntity(id)
    if entity.PhysicsObject.Position.X > 200 then
        entity.PhysicsObject.Velocity = Vector2(-10, 0)
    elseif entity.PhysicsObject.Position.X < -200 then
        entity.PhysicsObject.Velocity = Vector2(10, 0)
    end
end
