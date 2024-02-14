<script lang="ts">
  import { Gfx } from "$lib/gfx/gfx";
  import { Renderer } from "$lib/gfx/renderer";
  import { ceil, floor, round } from "mathjs";
  import { onMount } from "svelte";

  // The canvas element
  let canvas: HTMLCanvasElement;

  // The time of the last frame
  let lastFrameTime = 0;
  let fps = 0;

  // Handle the canvas resize event
  window.addEventListener("resize", () => {
    Gfx.resize();
    // Redraw frame for smooth resizing
    Renderer.render();
  });

  /// The render loop
  function render() {
    // Calculate the time since the last frame
    const now = performance.now();
    const dt = (now - lastFrameTime) / 1000;
    lastFrameTime = now;
    // Calculate the FPS
    fps = 1 / dt;

    Renderer.render();
    requestAnimationFrame(render);
  }

  onMount(async () => {
    // Initialize the graphics system
    Gfx.init(canvas);
    Gfx.resize();
    // Initialize the renderer
    await Renderer.init();

    // Start the render loop
    requestAnimationFrame(render);
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
    background-color: black;
    padding: 0.5rem;
  }
</style>
