"""Minimal 4X loop on a 12×6 board: capital, ownership, unit, turns."""

from __future__ import annotations

from dataclasses import dataclass, field
from enum import IntEnum
from typing import List, Optional, Set, Tuple

import numpy as np

from proc4x.world import Biome

Coord = Tuple[int, int]  # (x, y)


class Owner(IntEnum):
    NEUTRAL = 0
    PLAYER = 1
    ENEMY = 2


OWNER_TINT = {
    Owner.NEUTRAL: None,
    Owner.PLAYER: (40, 120, 255),
    Owner.ENEMY: (220, 60, 60),
}


@dataclass
class Unit:
    owner: Owner
    x: int
    y: int
    moves_left: int = 1
    max_moves: int = 1
    hp: int = 10

    @property
    def pos(self) -> Coord:
        return (self.x, self.y)

    def refresh(self) -> None:
        self.moves_left = self.max_moves


@dataclass
class GameState:
    """Turn-based skirmish on a fixed grid over procedural biomes."""

    grid_w: int
    grid_h: int
    biome: np.ndarray  # (H, W) int
    height: np.ndarray
    ownership: np.ndarray  # (H, W) int Owner
    units: List[Unit] = field(default_factory=list)
    turn: int = 1
    active: Owner = Owner.PLAYER
    message: str = "Your turn — select your army (blue), then a land tile to move."
    winner: Optional[Owner] = None

    @classmethod
    def new(cls, biome: np.ndarray, height: np.ndarray) -> "GameState":
        h, w = biome.shape
        ownership = np.zeros((h, w), dtype=np.int8)
        land = [ (x, y) for y in range(h) for x in range(w) if biome[y, x] != Biome.OCEAN.value ]
        if len(land) < 2:
            # force two land corners if map is all water (rare)
            land = [(0, 0), (w - 1, h - 1)]
            biome[0, 0] = Biome.PLAINS.value
            biome[h - 1, w - 1] = Biome.PLAINS.value

        # Place capitals far apart on land
        player_cap = land[0]
        enemy_cap = land[0]
        best = -1
        for a in land:
            for b in land:
                d = abs(a[0] - b[0]) + abs(a[1] - b[1])
                if d > best:
                    best = d
                    player_cap, enemy_cap = a, b

        ownership[player_cap[1], player_cap[0]] = Owner.PLAYER
        ownership[enemy_cap[1], enemy_cap[0]] = Owner.ENEMY

        # Starting territory: capital + 4-neighbors on land
        def claim(cap: Coord, owner: Owner) -> None:
            cx, cy = cap
            for dx, dy in ((0, 0), (1, 0), (-1, 0), (0, 1), (0, -1)):
                nx, ny = cx + dx, cy + dy
                if 0 <= nx < w and 0 <= ny < h and biome[ny, nx] != Biome.OCEAN.value:
                    ownership[ny, nx] = owner

        claim(player_cap, Owner.PLAYER)
        claim(enemy_cap, Owner.ENEMY)

        units = [
            Unit(Owner.PLAYER, player_cap[0], player_cap[1], moves_left=1, max_moves=1),
            Unit(Owner.ENEMY, enemy_cap[0], enemy_cap[1], moves_left=1, max_moves=1),
        ]
        return cls(
            grid_w=w,
            grid_h=h,
            biome=biome,
            height=height,
            ownership=ownership,
            units=units,
            message="Your turn — click your army, then a reachable land tile. E = end turn.",
        )

    def is_land(self, x: int, y: int) -> bool:
        return self.biome[y, x] != Biome.OCEAN.value

    def in_bounds(self, x: int, y: int) -> bool:
        return 0 <= x < self.grid_w and 0 <= y < self.grid_h

    def unit_at(self, x: int, y: int) -> Optional[Unit]:
        for u in self.units:
            if u.x == x and u.y == y and u.hp > 0:
                return u
        return None

    def player_units(self) -> List[Unit]:
        return [u for u in self.units if u.owner == Owner.PLAYER and u.hp > 0]

    def enemy_units(self) -> List[Unit]:
        return [u for u in self.units if u.owner == Owner.ENEMY and u.hp > 0]

    def reachable(self, unit: Unit) -> Set[Coord]:
        """Orthogonal steps up to moves_left; land only; can enter enemy tile to attack."""
        if unit.moves_left <= 0:
            return set()
        out: Set[Coord] = set()
        # BFS limited by moves
        from collections import deque

        q = deque([(unit.x, unit.y, 0)])
        seen = {(unit.x, unit.y)}
        while q:
            x, y, d = q.popleft()
            if d > 0:
                out.add((x, y))
            if d >= unit.moves_left:
                continue
            for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                nx, ny = x + dx, y + dy
                if not self.in_bounds(nx, ny) or (nx, ny) in seen:
                    continue
                if not self.is_land(nx, ny):
                    continue
                other = self.unit_at(nx, ny)
                if other and other.owner == unit.owner:
                    continue
                # Can step onto empty or enemy (attack ends move)
                seen.add((nx, ny))
                q.append((nx, ny, d + 1))
        return out

    def try_move(self, unit: Unit, tx: int, ty: int) -> bool:
        if self.winner is not None:
            return False
        if unit.owner != self.active:
            self.message = "Not that side's turn."
            return False
        if unit.moves_left <= 0:
            self.message = "No moves left — press E to end turn."
            return False
        if (tx, ty) not in self.reachable(unit):
            self.message = "Can't reach that tile."
            return False

        target = self.unit_at(tx, ty)
        if target and target.owner != unit.owner:
            # Simple combat: attacker wins, both lose HP flavor
            target.hp = 0
            unit.hp = max(1, unit.hp - 3)
            self.units = [u for u in self.units if u.hp > 0]
            unit.x, unit.y = tx, ty
            unit.moves_left = 0
            self.ownership[ty, tx] = unit.owner
            self.message = f"Combat! You captured ({tx},{ty})."
            self._check_winner()
            return True

        unit.x, unit.y = tx, ty
        unit.moves_left -= 1
        # Claim tile
        self.ownership[ty, tx] = unit.owner
        self.message = f"Moved to ({tx},{ty}). Moves left: {unit.moves_left}."
        self._check_winner()
        return True

    def end_turn(self) -> None:
        if self.winner is not None:
            return
        if self.active == Owner.PLAYER:
            self.active = Owner.ENEMY
            self.message = "Enemy turn…"
            self._enemy_ai()
            self.active = Owner.PLAYER
            self.turn += 1
            for u in self.units:
                u.refresh()
            if self.winner is None:
                self.message = f"Turn {self.turn} — your move. Select army, then tile. E = end turn."
        else:
            self.active = Owner.PLAYER
            for u in self.units:
                if u.owner == Owner.PLAYER:
                    u.refresh()

    def _enemy_ai(self) -> None:
        """Greedy: move each enemy unit toward nearest player unit or unowned land."""
        for u in list(self.enemy_units()):
            u.refresh()
            if u.moves_left <= 0:
                continue
            targets = self.player_units()
            if targets:
                goal = min(targets, key=lambda p: abs(p.x - u.x) + abs(p.y - u.y))
                gx, gy = goal.x, goal.y
            else:
                gx, gy = u.x, u.y

            best: Optional[Coord] = None
            best_score = 10**9
            for cx, cy in self.reachable(u):
                score = abs(cx - gx) + abs(cy - gy)
                # Prefer attacking player
                other = self.unit_at(cx, cy)
                if other and other.owner == Owner.PLAYER:
                    score -= 50
                if score < best_score:
                    best_score = score
                    best = (cx, cy)
            if best:
                self.try_move(u, best[0], best[1])
        self._check_winner()

    def _check_winner(self) -> None:
        pu = self.player_units()
        eu = self.enemy_units()
        if not eu and pu:
            self.winner = Owner.PLAYER
            self.message = "You win — enemy army destroyed!"
        elif not pu and eu:
            self.winner = Owner.ENEMY
            self.message = "Defeat — your army was destroyed."
        elif not pu and not eu:
            self.winner = Owner.NEUTRAL
            self.message = "Draw — no armies left."

    def capital_of(self, owner: Owner) -> Optional[Coord]:
        # First tile still owned that had a unit start — approximate: densest owned cluster center
        # Simpler: tile where we had unit at start is tracked via highest concentration
        ys, xs = np.where(self.ownership == owner)
        if len(xs) == 0:
            return None
        # Return centroid-ish owned tile
        return (int(xs[0]), int(ys[0]))
