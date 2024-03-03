use std::sync::{Arc, Mutex};

use actix::{Actor, Addr, AsyncContext, Handler, Recipient, StreamHandler};
use actix_web_actors::ws;
use uuid::Uuid;

use crate::server::{
    message::{
        ClientMessage, ClientMessageIncoming, ClientMessageOutgoing, Connect, MessageHandler,
    },
    GameServer, Session,
};

/// The socket module contains the actor that will handle the WebSocket connections.
#[derive(Clone)]
pub struct WsSession {
    pub id: Uuid,
    pub server: Arc<Mutex<GameServer>>,
    pub message_handler: Arc<MessageHandler>,
}

impl Actor for WsSession {
    type Context = ws::WebsocketContext<Self>;

    fn started(&mut self, ctx: &mut Self::Context) {
        let recipient: Recipient<ClientMessageOutgoing> = ctx.address().recipient();
        self.server.lock().unwrap().add_session(self.id, recipient);
    }
}

impl WsSession {
    /// Create a new WsSession
    pub fn new(
        id: Uuid,
        server: Arc<Mutex<GameServer>>,
        message_handler: Arc<MessageHandler>,
    ) -> Self {
        WsSession {
            id,
            server,
            message_handler,
        }
    }
}

impl Handler<ClientMessageOutgoing> for WsSession {
    type Result = ();

    fn handle(&mut self, msg: ClientMessageOutgoing, ctx: &mut Self::Context) {
        let message_str = serde_json::to_string(&msg.0).unwrap();
        ctx.text(message_str);
    }
}

impl StreamHandler<Result<ws::Message, ws::ProtocolError>> for WsSession {
    fn handle(&mut self, msg: Result<ws::Message, ws::ProtocolError>, ctx: &mut Self::Context) {
        match msg {
            Ok(ws::Message::Ping(msg)) => ctx.pong(&msg),
            Ok(ws::Message::Pong(_)) => (),
            Ok(ws::Message::Text(text)) => {
                let message_str = text.to_string();
                let Ok(message) = serde_json::from_str::<ClientMessage>(&message_str) else {
                    log::error!("Invalid message: {}", message_str);
                    return;
                };
                log::info!("Received message: {:?}", message);
                self.message_handler.handle_message(
                    ClientMessageIncoming {
                        session_id: self.id,
                        message,
                    },
                    self.server.clone(),
                );
            }
            Ok(ws::Message::Binary(bin)) => ctx.binary(bin),
            Ok(ws::Message::Close(reason)) => {
                self.server.lock().unwrap().remove_session(self.id);
                ctx.close(reason);
            }
            _ => (),
        }
    }
}
