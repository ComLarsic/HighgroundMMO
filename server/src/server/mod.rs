use actix::{Actor, Context, Handler, Recipient};
use hashbrown::HashMap;
use log::info;
use uuid::Uuid;

use self::{
    message::{
        ClientMessage, ClientMessageIncoming, ClientMessageOutgoing, Connect, MessageCallback,
    },
    room::RoomManager,
};
pub mod message;
pub mod room;

/// A session connected to the server
#[derive(Debug)]
pub struct Session {
    pub id: Uuid,
    pub recipient: Recipient<ClientMessageOutgoing>,
}

/// The main server module contains the actor that will handle the WebSocket connections.
pub struct GameServer {
    // The sessions in the server
    pub sessions: HashMap<Uuid, Session>,
    // The room manager
    pub room_manager: RoomManager,
}

impl GameServer {
    /// Create a new GameServer
    pub fn new() -> Self {
        GameServer {
            room_manager: RoomManager::new(),
            sessions: HashMap::new(),
        }
    }

    /// Add a session to the server
    pub fn add_session(&mut self, session: Uuid, addr: Recipient<ClientMessageOutgoing>) {
        self.sessions.insert(
            session,
            Session {
                id: session,
                recipient: addr,
            },
        );
    }

    /// Remove a session from the server
    pub fn remove_session(&mut self, session: Uuid) {
        self.sessions.remove(&session);
        if let Some(room) = self.room_manager.room_of_session(session) {
            let room = self.room_manager.room_mut(room).unwrap();
            room.remove_session(session);
        }
    }

    /// Send a message to a session
    pub fn send_message(&self, session: Uuid, message: ClientMessage) {
        let message_out = ClientMessageOutgoing(message);
        log::info!("Sessions: {:?}", self.sessions);
        if let Some(session) = self.sessions.get(&session) {
            session.recipient.do_send(message_out.clone());
        }
    }

    /// Send a message to all sessions
    pub fn send_message_all(&self, message: ClientMessage) {
        let message_out = ClientMessageOutgoing(message);
        for (_, session) in &self.sessions {
            session.recipient.do_send(message_out.clone());
        }
    }
}
