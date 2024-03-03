use std::{
    env,
    sync::{Arc, Mutex},
};

use actix::{Actor, Addr};
use actix_web::{get, middleware::Logger, web, App, Error, HttpRequest, HttpResponse, HttpServer};
use actix_web_actors::ws;
use hgserver::{
    messages::room::{join_room, list_rooms},
    server::{message::MessageHandler, room::Room, GameServer},
    socket::WsSession,
};
use serde::{Deserialize, Serialize};
use uuid::Uuid;

/// The port the server gets hosted at
const PORT: u16 = 5189;
/// The default host
const HOST: &str = "0.0.0.0";

/// The usage text
const USAGE_TEXT: &str = r#"Usage: hgserver [options] [arguments]
Options:
    -h, --help          Show this help message and exit
    -c, --config <file> The configuration file to use (toml)
    -p, --port <port>   The port to host the server at
    -o, --host <host>   The host to bind the server to
    "#;

/// The server configuration
#[derive(Debug, Clone, Serialize, Deserialize)]
struct Config {
    host: String,
    port: u16,
}

#[actix_web::main]
async fn main() -> std::io::Result<()> {
    // Initialize the logger
    env_logger::init_from_env(env_logger::Env::new().default_filter_or("info"));

    // Read the server configuration
    let config = read_config();

    // Start the server
    let server = setup_game_server();
    let message_handler = setup_message_handler();
    log::info!("Hosting at: {}:{}", config.host, config.port);

    // Start the HTTP server
    HttpServer::new(move || {
        App::new()
            .app_data(web::Data::new(server.clone()))
            .app_data(web::Data::new(message_handler.clone()))
            .service(websocket_connect)
            .wrap(Logger::default())
    })
    .bind((config.host, config.port))?
    .run()
    .await
}

/// Setup the game server
fn setup_game_server() -> Arc<Mutex<GameServer>> {
    let mut server = GameServer::new();
    // Add rooms
    server.room_manager.add_room(Room::new("Heck", 20));
    server
        .room_manager
        .add_room(Room::new("SeaOfNightmares", 20));
    server
        .room_manager
        .add_room(Room::new("MushroomKingdom", 20));
    // Start the server and return the address
    Arc::new(Mutex::new(server))
}

/// Setup the message handler
fn setup_message_handler() -> Arc<MessageHandler> {
    let mut message_handler = MessageHandler::new();
    message_handler.add_callback("get-room-list", list_rooms);
    message_handler.add_callback("join-room", join_room);
    Arc::new(message_handler)
}

/// Connect to the websocket server
#[get("/ws")]
async fn websocket_connect(
    req: HttpRequest,
    stream: web::Payload,
    server: web::Data<Arc<Mutex<GameServer>>>,
    message_handler: web::Data<Arc<MessageHandler>>,
) -> Result<HttpResponse, Error> {
    let id = Uuid::new_v4();
    let resp = ws::start(
        WsSession {
            id,
            server: server.as_ref().clone(),
            message_handler: message_handler.as_ref().clone(),
        },
        &req,
        stream,
    );
    println!("{:?}", resp);
    resp
}

/// Read the server configuration from the command line
fn read_config() -> Config {
    let args: Vec<String> = env::args().collect();

    // Setup the default configuration
    let mut config = Config {
        port: PORT,
        host: HOST.to_string(),
    };
    let mut i = 1;
    while i < args.len() {
        match args[i].as_str() {
            "-h" | "--help" => {
                println!("{}", USAGE_TEXT);
                std::process::exit(0);
            }
            "-c" | "--config" => {
                i += 1;
                let file = std::fs::read_to_string(&args[i])
                    .expect(format!("Failed to read {}", &args[i]).as_str());
                config =
                    toml::from_str(&file).expect(format!("Failed to parse {}", &args[i]).as_str())
            }
            "-p" | "--port" => {
                i += 1;
                config.port = args[i].parse().unwrap();
            }
            "-o" | "--host" => {
                i += 1;
                config.host = args[i].clone();
            }
            _ => {
                println!("Unknown argument: {}\n{}", args[i], USAGE_TEXT);
                std::process::exit(1);
            }
        }
        i += 1;
    }
    config
}
