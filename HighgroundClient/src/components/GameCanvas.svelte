<script lang="ts">
  import { InputManager } from "$lib/game/input_manager";
  import { World } from "$lib/game/world";
  import { Camera } from "$lib/gfx/camera";
  import { Gfx } from "$lib/gfx/gfx";
  import { Renderer } from "$lib/gfx/renderer";
  import { Player } from "$lib/networking/player";
  import { ceil, floor, round } from "mathjs";
  import { onDestroy, onMount } from "svelte";

  // The canvas element
  let canvas: HTMLCanvasElement;

  // The time of the last frame
  let lastFrameTime = 0;
  let fps = 0;

  // The timestep for client-side calculations
  const timestep = 1 / 60;

  // The render loop
  let renderLoopId: number;

  // Handle the canvas resize event
  window.addEventListener("resize", () => {
    Gfx.resize();
    // Redraw frame for smooth resizing
    Renderer.render();
  });

  // On unmount, cancel the render loop
  onDestroy(() => {
    cancelAnimationFrame(renderLoopId);
  });

  /// The update loop
  async function update(dt: number): Promise<void> {
    // Update the input on the server
    InputManager.updateServer();

    // Toggle rendering the server world
    if (InputManager.isKeyPressed("F2")) {
      Renderer.renderServerWorld = !Renderer.renderServerWorld;
    }
    // Toggle rendering the collision boxes
    if (InputManager.isKeyPressed("F3")) {
      Renderer.renderCollisionBoxes = !Renderer.renderCollisionBoxes;
    }

    // Update the world
    World.processBackloggedMessages();
    World.update();
    // Update the camera
    Camera.focusOn(
      {
        X: Player.physicsData.position.X,
        Y: Player.physicsData.position.Y - 96,
      },
      dt
    );

    // Update the input manager
    InputManager.update();
  }

  /// The render loop
  async function render(): Promise<void> {
    // Render the frame
    Renderer.render();
  }

  /// The mainloop loop
  let sinceLastUpdate = 0;
  async function mainloop() {
    // Calculate the time since the last frame
    const now = performance.now();
    const dt = (now - lastFrameTime) / 1000;
    lastFrameTime = now;
    // Calculate the FPS
    fps = 1 / dt;

    // Update the game
    await update(dt);
    // Render the game
    await render();

    renderLoopId = requestAnimationFrame(mainloop);
    sinceLastUpdate += dt;
  }

  onMount(async () => {
    // Initialize the input system
    InputManager.init();

    // Initialize the graphics system
    Gfx.init(canvas);
    Gfx.resize();
    // Initialize the renderer
    await Renderer.init();

    // Start the render loop
    renderLoopId = requestAnimationFrame(mainloop);
  });
</script>

<p class="fps-label">{floor(fps)}fps</p>

<canvas id="game-canvas" bind:this={canvas}></canvas>

<style>
  canvas {
    margin: 0;
    padding: 0;
    width: 100%;
    height: 100%;
    border: 1px solid black;
  }

  .fps-label {
    position: absolute;
    top: 0;
    left: 0;
    color: white;
    padding: 0.5rem;
  }
</style>
