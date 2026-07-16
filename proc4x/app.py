"""
Proc4x — 12×6 procedural 4X skirmish (local Mac / pygame).

You get a capital, territory, and one army. Move, capture tiles, fight the enemy.

Controls:
  WASD / arrows     — move cursor
  Click / Enter     — select unit or move to tile
  E                 — end turn (enemy moves)
  R                 — new random map + new game
  [ ]               — seed ±1 (new game)
  F                 — height shading
  ESC               — quit
"""

from __future__ import annotations

import random
import sys
from typing import Optional, Set, Tuple

import numpy as np
import pygame

from proc4x.game import GameState, Owner, Unit
from proc4x.world import BIOME_COLORS, Biome, WorldConfig, WorldSampler, biome_color_lut

GRID_W = 12
GRID_H = 6
CELL_WORLD_STEP = 48.0


class Board:
    def __init__(self, seed: int = 42, height_shade: bool = True) -> None:
        self.seed = seed
        self.height_shade = height_shade
        self.cfg = WorldConfig(seed=seed)
        self.cfg.continental_scale = 0.02
        self.cfg.elevation_scale = 0.05
        self.cfg.detail_scale = 0.12
        self.cfg.temperature_scale = 0.03
        self.cfg.moisture_scale = 0.04
        self.cfg.sea_level = 0.38
        self.sampler = WorldSampler(self.cfg)
        self.lut = biome_color_lut()
        self.height: np.ndarray
        self.biome: np.ndarray
        self.colors: np.ndarray
        self.regenerate()

    def regenerate(self) -> None:
        self.cfg.apply_seed(self.seed)
        self.sampler = WorldSampler(self.cfg)
        xs = (np.arange(GRID_W, dtype=np.float64) + 0.5) * CELL_WORLD_STEP
        zs = (np.arange(GRID_H, dtype=np.float64) + 0.5) * CELL_WORLD_STEP
        wx, wz = np.meshgrid(xs, zs)
        self.height, self.biome = self.sampler.classify_biome_grid(wx, wz)
        self._rebuild_colors()

    def _rebuild_colors(self) -> None:
        colors = self.lut[self.biome].astype(np.float32)
        if self.height_shade:
            hmin, hmax = float(self.height.min()), float(self.height.max())
            if hmax > hmin:
                shade = 0.55 + 0.45 * (self.height - hmin) / (hmax - hmin)
            else:
                shade = np.ones_like(self.height)
            ocean = self.biome == Biome.OCEAN.value
            shade = np.where(ocean, 0.8 + 0.2 * shade, shade)
            colors = colors * shade[..., None]
        self.colors = np.clip(colors, 0, 255).astype(np.uint8)

    def reseed(self, seed: int) -> None:
        self.seed = seed
        self.regenerate()

    def toggle_shade(self) -> None:
        self.height_shade = not self.height_shade
        self._rebuild_colors()


class App:
    def __init__(self, seed: int = 42) -> None:
        pygame.init()
        pygame.display.set_caption("Proc4x — 12×6 Skirmish")
        self.margin = 56
        self.side_pad = 24
        cell = 72
        self.w = self.side_pad * 2 + cell * GRID_W
        self.h = self.margin + self.side_pad + 28 + cell * GRID_H
        self.screen = pygame.display.set_mode((self.w, self.h), pygame.RESIZABLE)
        self.clock = pygame.time.Clock()

        self.font = None
        self.font_sm = None
        try:
            import importlib

            pfont = importlib.import_module("pygame.font")
            pfont.init()
            self.font = pfont.SysFont("Menlo", 17) or pfont.Font(None, 22)
            self.font_sm = pfont.SysFont("Menlo", 13) or pfont.Font(None, 18)
        except (pygame.error, NotImplementedError, AttributeError, ModuleNotFoundError, ImportError):
            self.font = self.font_sm = None

        self.board = Board(seed=seed)
        self.game = GameState.new(self.board.biome.copy(), self.board.height.copy())
        self.cursor = [GRID_W // 2, GRID_H // 2]
        self.selected_unit: Optional[Unit] = None
        self.reachable: Set[Tuple[int, int]] = set()
        self.running = True
        self._snap_cursor_to_player()

    def _snap_cursor_to_player(self) -> None:
        units = self.game.player_units()
        if units:
            self.cursor = [units[0].x, units[0].y]
            self.selected_unit = units[0]
            self.reachable = self.game.reachable(units[0])

    def _new_game(self, seed: int) -> None:
        self.board.reseed(seed)
        self.game = GameState.new(self.board.biome.copy(), self.board.height.copy())
        self.selected_unit = None
        self.reachable = set()
        self._snap_cursor_to_player()

    def _layout(self) -> Tuple[int, int, int]:
        avail_w = max(1, self.w - self.side_pad * 2)
        avail_h = max(1, self.h - self.margin - self.side_pad - 24)
        cell = max(8, min(avail_w // GRID_W, avail_h // GRID_H))
        grid_px_w = cell * GRID_W
        grid_px_h = cell * GRID_H
        ox = (self.w - grid_px_w) // 2
        oy = self.margin + max(0, (avail_h - grid_px_h) // 2)
        return ox, oy, cell

    def _select_or_act(self, gx: int, gy: int) -> None:
        if self.game.winner is not None:
            return
        unit_here = self.game.unit_at(gx, gy)

        # Select own unit
        if unit_here and unit_here.owner == Owner.PLAYER and self.game.active == Owner.PLAYER:
            self.selected_unit = unit_here
            self.reachable = self.game.reachable(unit_here)
            self.game.message = f"Army selected at ({gx},{gy}). Moves: {unit_here.moves_left}. Click a green tile."
            return

        # Move selected unit
        if self.selected_unit and self.selected_unit.owner == Owner.PLAYER:
            if self.game.try_move(self.selected_unit, gx, gy):
                self.reachable = self.game.reachable(self.selected_unit)
                if self.selected_unit.moves_left <= 0:
                    self.reachable = set()
            return

        self.game.message = "Select your blue army first."

    def handle_events(self) -> None:
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                self.running = False
            elif event.type == pygame.VIDEORESIZE:
                self.w, self.h = max(event.w, 200), max(event.h, 120)
                self.screen = pygame.display.set_mode((self.w, self.h), pygame.RESIZABLE)
            elif event.type == pygame.KEYDOWN:
                if event.key == pygame.K_ESCAPE:
                    self.running = False
                elif event.key in (pygame.K_LEFT, pygame.K_a):
                    self.cursor[0] = (self.cursor[0] - 1) % GRID_W
                elif event.key in (pygame.K_RIGHT, pygame.K_d):
                    self.cursor[0] = (self.cursor[0] + 1) % GRID_W
                elif event.key in (pygame.K_UP, pygame.K_w):
                    self.cursor[1] = (self.cursor[1] - 1) % GRID_H
                elif event.key in (pygame.K_DOWN, pygame.K_s):
                    self.cursor[1] = (self.cursor[1] + 1) % GRID_H
                elif event.key in (pygame.K_RETURN, pygame.K_SPACE):
                    self._select_or_act(self.cursor[0], self.cursor[1])
                elif event.key == pygame.K_e:
                    if self.game.winner is None:
                        self.game.end_turn()
                        self.selected_unit = None
                        self.reachable = set()
                        self._snap_cursor_to_player()
                elif event.key == pygame.K_r:
                    self._new_game(random.randint(0, 2**31 - 1))
                elif event.key == pygame.K_LEFTBRACKET:
                    self._new_game(self.board.seed - 1)
                elif event.key == pygame.K_RIGHTBRACKET:
                    self._new_game(self.board.seed + 1)
                elif event.key == pygame.K_f:
                    self.board.toggle_shade()
            elif event.type == pygame.MOUSEBUTTONDOWN and event.button == 1:
                ox, oy, cell = self._layout()
                mx, my = event.pos
                gx = (mx - ox) // cell
                gy = (my - oy) // cell
                if 0 <= gx < GRID_W and 0 <= gy < GRID_H:
                    self.cursor = [int(gx), int(gy)]
                    self._select_or_act(int(gx), int(gy))

    def draw(self) -> None:
        self.screen.fill((18, 20, 28))
        ox, oy, cell = self._layout()

        pygame.draw.rect(
            self.screen,
            (8, 10, 14),
            (ox - 2, oy - 2, cell * GRID_W + 4, cell * GRID_H + 4),
            border_radius=4,
        )

        for gy in range(GRID_H):
            for gx in range(GRID_W):
                rgb = self.board.colors[gy, gx]
                color = [int(rgb[0]), int(rgb[1]), int(rgb[2])]
                owner = Owner(int(self.game.ownership[gy, gx]))
                # Ownership tint
                if owner == Owner.PLAYER:
                    color = self._blend(color, (50, 120, 255), 0.35)
                elif owner == Owner.ENEMY:
                    color = self._blend(color, (220, 50, 50), 0.35)
                rect = pygame.Rect(ox + gx * cell, oy + gy * cell, cell, cell)
                pygame.draw.rect(self.screen, color, rect)

                # Reachable highlight
                if (gx, gy) in self.reachable:
                    overlay = pygame.Surface((cell, cell), pygame.SRCALPHA)
                    overlay.fill((80, 255, 120, 70))
                    self.screen.blit(overlay, rect.topleft)

                pygame.draw.rect(self.screen, (0, 0, 0), rect, 1)

                # Capital star: tile with unit that is "home" — mark densest / first owned with crown via unit start
                # Draw small ownership pip
                if owner != Owner.NEUTRAL:
                    pip = (60, 140, 255) if owner == Owner.PLAYER else (240, 70, 70)
                    pygame.draw.circle(
                        self.screen,
                        pip,
                        (rect.centerx, rect.y + max(4, cell // 8)),
                        max(2, cell // 14),
                    )

        # Units
        for u in self.game.units:
            if u.hp <= 0:
                continue
            self._draw_unit(u, ox, oy, cell)

        # Cursor
        cx, cy = self.cursor
        crect = pygame.Rect(ox + cx * cell, oy + cy * cell, cell, cell)
        pygame.draw.rect(self.screen, (255, 255, 255), crect, max(2, cell // 24))

        # Selected unit ring
        if self.selected_unit and self.selected_unit.hp > 0:
            sx, sy = self.selected_unit.x, self.selected_unit.y
            srect = pygame.Rect(ox + sx * cell, oy + sy * cell, cell, cell)
            pygame.draw.rect(self.screen, (255, 220, 60), srect, max(3, cell // 18))

        self._draw_hud(ox, oy, cell)

    def _draw_unit(self, u: Unit, ox: int, oy: int, cell: int) -> None:
        cx = ox + u.x * cell + cell // 2
        cy = oy + u.y * cell + cell // 2
        r = max(6, cell // 4)
        fill = (70, 150, 255) if u.owner == Owner.PLAYER else (230, 70, 70)
        border = (255, 255, 255)
        pygame.draw.circle(self.screen, fill, (cx, cy), r)
        pygame.draw.circle(self.screen, border, (cx, cy), r, 2)
        # Simple "army" mark
        pygame.draw.line(self.screen, border, (cx, cy - r // 2), (cx, cy + r // 2), 2)
        pygame.draw.line(self.screen, border, (cx - r // 2, cy), (cx + r // 2, cy), 2)

    @staticmethod
    def _blend(base, tint, a: float):
        return tuple(int(base[i] * (1 - a) + tint[i] * a) for i in range(3))

    def _draw_hud(self, ox: int, oy: int, cell: int) -> None:
        g = self.game
        cx, cy = self.cursor
        biome_id = int(self.board.biome[cy, cx])
        try:
            biome_name = Biome(biome_id).name.title()
        except ValueError:
            biome_name = "?"
        owner = Owner(int(g.ownership[cy, cx]))
        owner_s = {Owner.NEUTRAL: "Neutral", Owner.PLAYER: "Yours", Owner.ENEMY: "Enemy"}[owner]

        line1 = (
            f"Proc4x  seed={self.board.seed}  Turn {g.turn}  "
            f"cell=({cx},{cy}) {biome_name}  owner={owner_s}"
        )
        line2 = g.message
        line3 = "WASD move cursor  Enter/click select·move  E end turn  R new map  ESC quit"
        if g.winner == Owner.PLAYER:
            line2 = "YOU WIN — press R for a new map"
        elif g.winner == Owner.ENEMY:
            line2 = "YOU LOSE — press R for a new map"

        if self.font is not None:
            self._text(line1, (12, 8), small=False)
            self._text(line2, (12, 28), small=True)
            self._text(line3, (12, 44), small=True)
        else:
            pygame.draw.rect(self.screen, (0, 0, 0), (0, 0, self.w, 48))
            # Color code status
            col = (80, 200, 80) if g.winner == Owner.PLAYER else (
                (200, 60, 60) if g.winner == Owner.ENEMY else (80, 160, 255)
            )
            pygame.draw.rect(self.screen, col, (8, 8, 20, 20))
            for i in range(min(g.turn, 30)):
                pygame.draw.rect(self.screen, (200, 200, 200), (36 + i * 6, 12, 4, 12))

        # Side legend
        legend_x = self.w - 128
        legend_y = 8
        items = [
            ((70, 150, 255), "You / army"),
            ((230, 70, 70), "Enemy"),
            ((80, 255, 120), "Move range"),
            ((50, 120, 255), "Your land"),
            ((220, 50, 50), "Enemy land"),
        ]
        for rgb, label in items:
            pygame.draw.rect(self.screen, rgb, (legend_x, legend_y, 12, 12))
            if self.font_sm is not None:
                self.screen.blit(
                    self.font_sm.render(label, True, (200, 200, 200)),
                    (legend_x + 16, legend_y - 1),
                )
            legend_y += 15

        if self.font_sm is not None:
            self._text(f"{GRID_W}×{GRID_H}  |  destroy enemy army to win", (ox, oy + cell * GRID_H + 6), small=True)

    def _text(self, text: str, pos: Tuple[int, int], small: bool = False) -> None:
        font = self.font_sm if small else self.font
        if font is None:
            return
        surf = font.render(text, True, (230, 230, 235))
        bg = pygame.Surface((surf.get_width() + 8, surf.get_height() + 4), pygame.SRCALPHA)
        bg.fill((0, 0, 0, 150))
        self.screen.blit(bg, (pos[0] - 4, pos[1] - 2))
        self.screen.blit(surf, pos)

    def run(self) -> None:
        while self.running:
            self.clock.tick(60)
            self.handle_events()
            self.draw()
            pygame.display.flip()
        pygame.quit()


def main(argv: list[str] | None = None) -> int:
    argv = argv if argv is not None else sys.argv[1:]
    seed = 42
    if argv:
        try:
            seed = int(argv[0])
        except ValueError:
            print("Usage: python -m proc4x.app [seed]", file=sys.stderr)
            return 2
    App(seed=seed).run()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
