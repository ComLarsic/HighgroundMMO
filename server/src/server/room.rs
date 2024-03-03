use hashbrown::HashMap;
use uuid::Uuid;

/// A room with players
#[derive(Debug, Clone)]
pub struct Room {
    id: Uuid,
    pub name: String,
    capacity: usize,
    sessions: Vec<Uuid>,
}

/// The room manager
#[derive(Debug)]
pub struct RoomManager {
    rooms: HashMap<Uuid, Room>,
}

impl RoomManager {
    /// Create a new RoomManager
    pub fn new() -> Self {
        RoomManager {
            rooms: HashMap::new(),
        }
    }

    /// Add a room to the manager
    pub fn add_room(&mut self, room: Room) {
        self.rooms.insert(room.id, room);
    }

    /// Remove a room from the manager
    pub fn remove_room(&mut self, room: Uuid) {
        self.rooms.remove(&room);
    }

    /// Get the rooms in the manager
    pub fn rooms(&self) -> Vec<&Room> {
        self.rooms.values().collect()
    }

    /// Get a room by id
    pub fn room(&self, id: Uuid) -> Option<&Room> {
        self.rooms.get(&id)
    }

    /// Get a mutable room by id
    pub fn room_mut(&mut self, id: Uuid) -> Option<&mut Room> {
        self.rooms.get_mut(&id)
    }

    /// Get the room a session is in
    pub fn room_of_session(&self, session: Uuid) -> Option<Uuid> {
        self.rooms
            .values()
            .find(|room| room.sessions.contains(&session))
            .map(|room| room.id())
    }
}

impl Room {
    /// Create a new room
    pub fn new(name: impl Into<String>, capacity: usize) -> Self {
        Room {
            id: Uuid::new_v4(),
            name: name.into(),
            capacity,
            sessions: Vec::new(),
        }
    }

    /// Add a session to the room
    pub fn add_session(&mut self, session: Uuid) -> anyhow::Result<()> {
        if self.sessions.len() >= self.capacity {
            return Err(anyhow::anyhow!("Room is full!"));
        }
        self.sessions.push(session);
        Ok(())
    }

    /// Remove a session from the room
    pub fn remove_session(&mut self, session: Uuid) {
        self.sessions.retain(|s| s != &session);
    }

    /// Get the sessions in the room
    pub fn sessions(&self) -> Vec<Uuid> {
        self.sessions.clone()
    }

    /// Get the capacity of the room
    pub fn capacity(&self) -> usize {
        self.capacity
    }

    /// Get the id of the room
    pub fn id(&self) -> Uuid {
        self.id
    }
}
