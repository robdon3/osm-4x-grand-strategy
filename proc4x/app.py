"""
Local procedural map viewer for macOS (pygame).

Controls:
  WASD / arrows  — pan
  Q E            — rotate view (cosmetic)
  Scroll / - =   — zoom
  R              — randomize seed
  [ ]            — decrease / increase seed by 1
  F              — toggle height shading
  ESC            — quit
"""

from __future__ import annotations

import sys
from typing import Dict, Tuple

import numpy as np
import pygame

from proc4x.world import Biome, WorldConfig, WorldSampler, biome_color_lut


ChunkKey = Tuple[int, int]


class ChunkCache:
    def __init__(self, sampler: WorldSampler, pixels_per_cell: int = 4) -> None:
        self.sampler = sampler
        self.ppc = max(1, pixels_per_cell)
        self.lut = biome_color_lut()
        self._cache: Dict[ChunkKey, pygame.Surface] = {}
        self.use_height_shade = True

    def clear(self) -> None:
        self._cache.clear()

    def get(self, cx: int, cz: int) -> pygame.Surface:
        key = (cx, cz)
        if key in self._cache:
            return self._cache[key]
        surf = self._build(cx, cz)
        self._cache[key] = surf
        # Soft cap so RAM stays friendly on 16GB Macs
        if len(self._cache) > 220:
            # drop arbitrary old entries
            for k in list(self._cache.keys())[:40]:
                del self._cache[k]
        return surf

    def _build(self, cx: int, cz: int) -> pygame.Surface:
        cfg = self.sampler.cfg
        n = cfg.chunk_size
        # world cell coords covering this chunk
        x0 = cx * n
        z0 = cz * n
        # sample at cell centers
        xs = np.arange(n, dtype=np.float64) + x0 + 0.5
        zs = np.arange(n, dtype=np.float64) + z0 + 0.5
        wx, wz = np.meshgrid(xs, zs)  # shape (n, n) — z rows, x cols

        height, biome = self.sampler.classify_biome_grid(wx, wz)
        colors = self.lut[biome]  # (n,n,3)

        if self.use_height_shade:
            # Shade by relative height (land only brighter on peaks)
            hmin, hmax = float(height.min()), float(height.max())
            if hmax > hmin:
                shade = 0.55 + 0.45 * (height - hmin) / (hmax - hmin)
            else:
                shade = np.ones_like(height)
            ocean = biome == Biome.OCEAN.value
            shade = np.where(ocean, 0.85 + 0.15 * shade, shade)
            colors = np.clip(colors.astype(np.float32) * shade[..., None], 0, 255).astype(np.uint8)

        # Upscale to pixels
        ppc = self.ppc
        pix = np.repeat(np.repeat(colors, ppc, axis=0), ppc, axis=1)
        # pygame wants (W,H) from array of (W,H,3) with make_surface — transpose
        # numpy is (row=z, col=x) = (H, W, 3); pygame surfarray wants (W, H, 3)
        arr = np.transpose(pix, (1, 0, 2))
        surf = pygame.surfarray.make_surface(arr)
        return surf


class App:
    def __init__(self, seed: int = 42) -> None:
        pygame.init()
        pygame.display.set_caption("Proc4x — Procedural Map (local)")
        self.w, self.h = 1280, 800
        self.screen = pygame.display.set_mode((self.w, self.h), pygame.RESIZABLE)
        self.clock = pygame.time.Clock()
        self.font = None
        self.font_sm = None
        try:
            # Avoid `import pygame.font` inside this method — it rebinds local name `pygame`.
            import importlib

            pfont = importlib.import_module("pygame.font")
            pfont.init()
            self.font = pfont.SysFont("Menlo", 16) or pfont.Font(None, 22)
            self.font_sm = pfont.SysFont("Menlo", 13) or pfont.Font(None, 18)
        except (pygame.error, NotImplementedError, AttributeError, ModuleNotFoundError, ImportError):
            self.font = self.font_sm = None

        self.cfg = WorldConfig(seed=seed)
        self.sampler = WorldSampler(self.cfg)
        self.cache = ChunkCache(self.sampler, pixels_per_cell=6)

        # camera in world cells (x, z)
        self.cam_x = 0.0
        self.cam_z = 0.0
        self.zoom = 1.0  # multiplies pixels-per-cell visually via scale
        self.load_radius = 4  # chunks around view

        self.running = True
        self._rebuild_view_params()

    def _rebuild_view_params(self) -> None:
        # effective pixels per world cell on screen
        self.ppc = max(1, int(round(self.cache.ppc * self.zoom)))

    def reseed(self, seed: int) -> None:
        self.cfg.apply_seed(seed)
        self.sampler = WorldSampler(self.cfg)
        self.cache = ChunkCache(self.sampler, pixels_per_cell=self.cache.ppc)
        self.cache.use_height_shade = True

    def handle_events(self) -> None:
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                self.running = False
            elif event.type == pygame.VIDEORESIZE:
                self.w, self.h = event.w, event.h
                self.screen = pygame.display.set_mode((self.w, self.h), pygame.RESIZABLE)
            elif event.type == pygame.KEYDOWN:
                if event.key == pygame.K_ESCAPE:
                    self.running = False
                elif event.key == pygame.K_r:
                    import random

                    self.reseed(random.randint(0, 2**31 - 1))
                elif event.key == pygame.K_LEFTBRACKET:
                    self.reseed(self.cfg.seed - 1)
                elif event.key == pygame.K_RIGHTBRACKET:
                    self.reseed(self.cfg.seed + 1)
                elif event.key == pygame.K_f:
                    self.cache.use_height_shade = not self.cache.use_height_shade
                    self.cache.clear()
                elif event.key in (pygame.K_EQUALS, pygame.K_PLUS, pygame.K_KP_PLUS):
                    self.zoom = min(4.0, self.zoom * 1.15)
                    self._rebuild_view_params()
                elif event.key in (pygame.K_MINUS, pygame.K_KP_MINUS):
                    self.zoom = max(0.25, self.zoom / 1.15)
                    self._rebuild_view_params()
            elif event.type == pygame.MOUSEWHEEL:
                if event.y > 0:
                    self.zoom = min(4.0, self.zoom * 1.12)
                else:
                    self.zoom = max(0.25, self.zoom / 1.12)
                self._rebuild_view_params()

    def update(self, dt: float) -> None:
        keys = pygame.key.get_pressed()
        # pan speed in cells/sec (faster when zoomed out)
        speed = 80.0 / max(self.zoom, 0.25)
        if keys[pygame.K_LSHIFT] or keys[pygame.K_RSHIFT]:
            speed *= 2.5
        dx = dz = 0.0
        if keys[pygame.K_a] or keys[pygame.K_LEFT]:
            dx -= 1
        if keys[pygame.K_d] or keys[pygame.K_RIGHT]:
            dx += 1
        if keys[pygame.K_w] or keys[pygame.K_UP]:
            dz -= 1
        if keys[pygame.K_s] or keys[pygame.K_DOWN]:
            dz += 1
        if dx or dz:
            length = math_hypot(dx, dz)
            self.cam_x += (dx / length) * speed * dt
            self.cam_z += (dz / length) * speed * dt

    def draw(self) -> None:
        self.screen.fill((12, 14, 20))
        cfg = self.cfg
        n = cfg.chunk_size
        # visible world rect in cells
        cells_w = self.w / (self.cache.ppc * self.zoom)
        cells_h = self.h / (self.cache.ppc * self.zoom)
        left = self.cam_x - cells_w * 0.5
        top = self.cam_z - cells_h * 0.5
        right = left + cells_w
        bottom = top + cells_h

        cx0 = int(np.floor(left / n)) - 1
        cz0 = int(np.floor(top / n)) - 1
        cx1 = int(np.floor(right / n)) + 1
        cz1 = int(np.floor(bottom / n)) + 1

        scale = self.cache.ppc * self.zoom
        for cz in range(cz0, cz1 + 1):
            for cx in range(cx0, cx1 + 1):
                surf = self.cache.get(cx, cz)
                # world cell origin of chunk
                wx = cx * n
                wz = cz * n
                sx = (wx - left) * scale
                sy = (wz - top) * scale
                size = int(n * scale)
                if size < 1:
                    continue
                if abs(scale - self.cache.ppc) > 0.01:
                    scaled = pygame.transform.scale(surf, (size, size))
                    self.screen.blit(scaled, (sx, sy))
                else:
                    self.screen.blit(surf, (sx, sy))

        # crosshair
        pygame.draw.line(self.screen, (255, 255, 255), (self.w // 2 - 8, self.h // 2), (self.w // 2 + 8, self.h // 2), 1)
        pygame.draw.line(self.screen, (255, 255, 255), (self.w // 2, self.h // 2 - 8), (self.w // 2, self.h // 2 + 8), 1)

        self._draw_hud()

    def _draw_text(self, text: str, pos: tuple[int, int], small: bool = False) -> int:
        """Draw text if fonts work; return vertical advance."""
        font = self.font_sm if small else self.font
        if font is None:
            # No font module — show a thin status bar only
            return 0
        surf = font.render(text, True, (230, 230, 235))
        bg = pygame.Surface((surf.get_width() + 10, surf.get_height() + 4), pygame.SRCALPHA)
        bg.fill((0, 0, 0, 140))
        self.screen.blit(bg, (pos[0] - 5, pos[1] - 2))
        self.screen.blit(surf, pos)
        return surf.get_height() + 6

    def _draw_hud(self) -> None:
        y = 8
        if self.font is not None:
            y += self._draw_text(
                f"Proc4x  seed={self.cfg.seed}  pos=({self.cam_x:.0f}, {self.cam_z:.0f})  zoom={self.zoom:.2f}",
                (11, y),
            )
            y += self._draw_text(
                "WASD pan  Shift fast  scroll zoom  R random seed  [ ] seed±1  F height shade  ESC quit",
                (11, y),
                small=True,
            )
        else:
            # Font-less fallback: colored bar encodes seed roughly
            pygame.draw.rect(self.screen, (0, 0, 0), (0, 0, self.w, 28))
            pygame.draw.rect(self.screen, (80, 160, 255), (8, 8, 12, 12))
            # seed as ticks
            for i in range(min(40, abs(self.cfg.seed) % 50 + 5)):
                pygame.draw.rect(self.screen, (200, 200, 200), (30 + i * 4, 10, 2, 8))

        # biome legend (color swatches always work)
        from proc4x.world import BIOME_COLORS

        legend_x = self.w - 160
        legend_y = 10
        if self.font_sm is not None:
            self._draw_text("Biomes", (legend_x, legend_y), small=True)
            legend_y += 18
        for biome, rgb in BIOME_COLORS.items():
            pygame.draw.rect(self.screen, rgb, (legend_x, legend_y, 14, 14))
            if self.font_sm is not None:
                label = self.font_sm.render(biome.name.title(), True, (200, 200, 200))
                self.screen.blit(label, (legend_x + 20, legend_y - 1))
            legend_y += 16

    def run(self) -> None:
        while self.running:
            dt = self.clock.tick(60) / 1000.0
            self.handle_events()
            self.update(dt)
            self.draw()
            pygame.display.flip()
        pygame.quit()


def math_hypot(a: float, b: float) -> float:
    return max((a * a + b * b) ** 0.5, 1e-9)


def main(argv: list[str] | None = None) -> int:
    argv = argv if argv is not None else sys.argv[1:]
    seed = 42
    if argv:
        try:
            seed = int(argv[0])
        except ValueError:
            print(f"Usage: python -m proc4x.app [seed]", file=sys.stderr)
            return 2
    App(seed=seed).run()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
