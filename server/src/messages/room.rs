use std::sync::{Arc, Mutex};

use actix::Context;
use serde::{Deserialize, Serialize};
use uuid::Uuid;

use crate::server::{
    message::{ClientMessage, ClientMessageIncoming},
    GameServer,
};

/// A room with players
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct RoomDescription {
    pub id: Uuid,
    pub name: String,
    pub capacity: usize,
    pub players: Vec<String>,
}

/// Sends a list of rooms
#[derive(Debug, Serialize, Deserialize)]
pub struct RoomListMessage {
    pub rooms: Vec<RoomDescription>,
}

/// The message to join a room
#[derive(Debug, Serialize, Deserialize)]
pub struct JoinRoomMessage {
    pub username: String,
    pub room: Uuid,
}

/// A message for failing to join a room
#[derive(Debug, Serialize, Deserialize)]
pub struct JoinRoomFailMessage {
    pub reason: String,
}

/// The message handler for responding with the list of rooms
pub fn list_rooms(incoming: ClientMessageIncoming, server: Arc<Mutex<GameServer>>) {
    let server = server.lock().unwrap();
    let rooms = server
        .room_manager
        .rooms()
        .iter()
        .map(|room| RoomDescription {
            id: room.id(),
            name: room.name.clone(),
            capacity: room.capacity(),
            players: room
                .sessions()
                .iter()
                .map(|session| server.sessions.get(session).unwrap().id.to_string())
                .collect(),
        })
        .collect();
    let response = ClientMessage::new(
        "get-room-list-response".to_string(),
        RoomListMessage { rooms },
    )
    .unwrap();
    server.send_message(incoming.session_id, response);
}

/// The message handler for joining a room
pub fn join_room(incoming: ClientMessageIncoming, server: Arc<Mutex<GameServer>>) {
    let join_room: JoinRoomMessage = serde_json::from_str(&incoming.message.content).unwrap();

    // Join the room
    let mut server = server.lock().unwrap();
    let Some(room) = server.room_manager.room_mut(join_room.room) else {
        let response = ClientMessage::new(
            "join-room-fail".to_string(),
            JoinRoomFailMessage {
                reason: "Room not found".to_string(),
            },
        )
        .unwrap();
        server.send_message(incoming.session_id, response);
        return;
    };

    // Handle the room being full
    if let Err(e) = room.add_session(incoming.session_id) {
        let response = ClientMessage::new(
            "join-room-fail".to_string(),
            JoinRoomFailMessage {
                reason: e.to_string(),
            },
        )
        .unwrap();
        server.send_message(incoming.session_id, response);
        return;
    }

    // Send the response message
    let response = ClientMessage::new("join-room-response".to_string(), {}).unwrap();
    server.send_message(incoming.session_id, response);
}
