use std::sync::{Arc, Mutex};

use actix::{Context, Message};
use hashbrown::HashMap;
use serde::{Deserialize, Serialize};
use uuid::Uuid;

use super::{GameServer, Session};

/// A message to connect to the server
#[derive(Debug, Message)]
#[rtype(result = "()")]
pub struct Connect(pub Session);

/// A message send to the server
#[derive(Debug, Clone, Message, Serialize, Deserialize)]
#[rtype(result = "()")]
pub struct ClientMessageIncoming {
    pub session_id: Uuid,
    pub message: ClientMessage,
}

/// A message send to the client
#[derive(Debug, Clone, Message, Serialize, Deserialize)]
#[rtype(result = "()")]
pub struct ClientMessageOutgoing(pub ClientMessage);

/// A message from/to the client. The string is usually a JSON object.
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ClientMessage {
    #[serde(alias = "type")]
    #[serde(rename = "type")]
    pub message_type: String,
    pub content: String,
}

impl ClientMessage {
    /// Create a new ClientMessage
    pub fn new(message_type: String, content: impl Serialize) -> Option<Self> {
        let content_str = serde_json::to_string(&content).ok()?;
        Some(ClientMessage {
            message_type,
            content: content_str,
        })
    }
}

/// The message handler
pub struct MessageHandler {
    callbacks: HashMap<String, Vec<MessageCallbackSendSync>>,
}

impl MessageHandler {
    /// Create a new MessageHandler
    pub fn new() -> Self {
        MessageHandler {
            callbacks: HashMap::new(),
        }
    }

    /// Add a message callback
    pub fn add_callback(
        &mut self,
        message_type: impl Into<String>,
        callback: impl 'static + MessageCallback,
    ) {
        let message_type = message_type.into();
        self.callbacks
            .entry(message_type)
            .or_insert_with(Vec::new)
            .push(MessageCallbackSendSync(Box::new(callback)));
    }

    /// Handle a message
    pub fn handle_message(&self, message: ClientMessageIncoming, server: Arc<Mutex<GameServer>>) {
        if let Some(callbacks) = self.callbacks.get(&message.message.message_type) {
            for callback in callbacks {
                callback.0(message.clone(), server.clone());
            }
        }
    }
}

/// A message callback that can be sent between threads
pub struct MessageCallbackSendSync(pub Box<dyn MessageCallback>);

/// A message callback
pub trait MessageCallback: Fn(ClientMessageIncoming, Arc<Mutex<GameServer>>) -> () {
    /// Clone the message callback
    fn clone_box(&self) -> Box<dyn MessageCallback>;
}

impl<T> MessageCallback for T
where
    T: Fn(ClientMessageIncoming, Arc<Mutex<GameServer>>) -> () + 'static + Clone,
{
    fn clone_box(&self) -> Box<dyn MessageCallback> {
        let f = (*self).clone();
        Box::new(f)
    }
}

unsafe impl Send for MessageCallbackSendSync {}
unsafe impl Sync for MessageCallbackSendSync {}
