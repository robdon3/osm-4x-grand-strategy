"""Seed-based procedural world: noise, height, biomes (deterministic)."""

from __future__ import annotations

import random
from dataclasses import dataclass
from enum import Enum, auto
from typing import Tuple

import numpy as np


class Biome(Enum):
    OCEAN = auto()
    BEACH = auto()
    PLAINS = auto()
    FOREST = auto()
    DESERT = auto()
    SAVANNA = auto()
    TUNDRA = auto()
    SNOW = auto()
    MOUNTAINS = auto()
    SWAMP = auto()


BIOME_COLORS: dict[Biome, Tuple[int, int, int]] = {
    Biome.OCEAN: (30, 70, 140),
    Biome.BEACH: (220, 205, 140),
    Biome.PLAINS: (115, 178, 76),
    Biome.FOREST: (38, 115, 46),
    Biome.DESERT: (217, 191, 102),
    Biome.SAVANNA: (178, 166, 76),
    Biome.TUNDRA: (140, 153, 140),
    Biome.SNOW: (235, 240, 245),
    Biome.MOUNTAINS: (115, 107, 102),
    Biome.SWAMP: (64, 102, 71),
}


@dataclass
class WorldConfig:
    seed: int = 42
    sea_level: float = 0.42
    height_amplitude: float = 24.0
    base_height: float = 4.0
    continental_scale: float = 0.003
    elevation_scale: float = 0.012
    detail_scale: float = 0.05
    temperature_scale: float = 0.004
    moisture_scale: float = 0.006
    chunk_size: int = 16
    cell_size: float = 1.0
    off_a: Tuple[float, float] = (0.0, 0.0)
    off_b: Tuple[float, float] = (0.0, 0.0)
    off_c: Tuple[float, float] = (0.0, 0.0)

    def apply_seed(self, seed: int) -> None:
        self.seed = seed
        rng = random.Random(seed)
        self.off_a = (rng.uniform(-1e5, 1e5), rng.uniform(-1e5, 1e5))
        self.off_b = (rng.uniform(-1e5, 1e5), rng.uniform(-1e5, 1e5))
        self.off_c = (rng.uniform(-1e5, 1e5), rng.uniform(-1e5, 1e5))


def _fade(t: np.ndarray) -> np.ndarray:
    return t * t * t * (t * (t * 6.0 - 15.0) + 10.0)


def _lerp(a: np.ndarray, b: np.ndarray, t: np.ndarray) -> np.ndarray:
    return a + t * (b - a)


def _grad(h: np.ndarray, x: np.ndarray, y: np.ndarray) -> np.ndarray:
    u = np.where(h & 1, x, -x)
    v = np.where(h & 2, y, -y)
    return u + v


class Noise:
    def __init__(self, seed: int) -> None:
        rng = np.random.default_rng(seed & 0xFFFFFFFF)
        p = np.arange(256, dtype=np.int32)
        rng.shuffle(p)
        self.perm = np.concatenate([p, p])

    def perlin2d(self, x: np.ndarray, y: np.ndarray) -> np.ndarray:
        xi = np.floor(x).astype(np.int32) & 255
        yi = np.floor(y).astype(np.int32) & 255
        xf = x - np.floor(x)
        yf = y - np.floor(y)
        u = _fade(xf)
        v = _fade(yf)
        perm = self.perm
        aa = perm[perm[xi] + yi]
        ab = perm[perm[xi] + yi + 1]
        ba = perm[perm[xi + 1] + yi]
        bb = perm[perm[xi + 1] + yi + 1]
        x1 = _lerp(_grad(aa, xf, yf), _grad(ba, xf - 1, yf), u)
        x2 = _lerp(_grad(ab, xf, yf - 1), _grad(bb, xf - 1, yf - 1), u)
        return (_lerp(x1, x2, v) + 1.0) * 0.5


class WorldSampler:
    def __init__(self, cfg: WorldConfig) -> None:
        self.cfg = cfg
        cfg.apply_seed(cfg.seed)
        self.noise = Noise(cfg.seed)

    def _sample(self, wx: np.ndarray, wz: np.ndarray, scale: float, off: Tuple[float, float]) -> np.ndarray:
        return self.noise.perlin2d((wx + off[0]) * scale, (wz + off[1]) * scale)

    def continental(self, wx: np.ndarray, wz: np.ndarray) -> np.ndarray:
        c = self.cfg
        n = self._sample(wx, wz, c.continental_scale, c.off_a)
        n2 = self._sample(wx, wz, c.continental_scale * 2.1, c.off_b)
        return np.clip(n * 0.7 + n2 * 0.3, 0.0, 1.0)

    def elevation(self, wx: np.ndarray, wz: np.ndarray) -> np.ndarray:
        c = self.cfg
        e1 = self._sample(wx, wz, c.elevation_scale, c.off_a)
        e2 = self._sample(wx, wz, c.elevation_scale * 2.3, c.off_b)
        detail = self._sample(wx, wz, c.detail_scale, c.off_c)
        return np.clip(e1 * 0.55 + e2 * 0.3 + detail * 0.15, 0.0, 1.0)

    def temperature(self, wx: np.ndarray, wz: np.ndarray) -> np.ndarray:
        c = self.cfg
        band = np.clip((wz + 2000.0) / 4000.0, 0.0, 1.0)
        n = self._sample(wx, wz, c.temperature_scale, c.off_b)
        return np.clip(band * 0.65 + n * 0.35, 0.0, 1.0)

    def moisture(self, wx: np.ndarray, wz: np.ndarray) -> np.ndarray:
        c = self.cfg
        return self._sample(wx, wz, c.moisture_scale, c.off_c)

    def surface_height(self, wx: np.ndarray, wz: np.ndarray) -> np.ndarray:
        c = self.cfg
        continental = self.continental(wx, wz)
        elev = self.elevation(wx, wz)
        ocean = continental < c.sea_level
        depth = (c.sea_level - continental) / max(c.sea_level, 1e-6)
        ocean_h = c.base_height * 0.25 - depth * 3.0
        land = (continental - c.sea_level) / max(1.0 - c.sea_level, 1e-6)
        land_h = c.base_height + land * c.height_amplitude * (0.35 + elev * 0.65)
        return np.where(ocean, ocean_h, land_h)

    def classify_biome_grid(self, wx: np.ndarray, wz: np.ndarray):
        c = self.cfg
        continental = self.continental(wx, wz)
        elev = self.elevation(wx, wz)
        temp = self.temperature(wx, wz)
        moist = self.moisture(wx, wz)
        height = self.surface_height(wx, wz)
        biome = np.full(wx.shape, Biome.PLAINS.value, dtype=np.int16)
        ocean = continental < c.sea_level
        beach = (~ocean) & (continental < c.sea_level + 0.03)
        mountains = (~ocean) & (elev > 0.78)
        cold = (~ocean) & (temp < 0.22)
        snow = cold & (moist > 0.45)
        tundra = cold & ~snow
        desert = (~ocean) & (temp > 0.7) & (moist < 0.3)
        savanna = (~ocean) & (temp > 0.55) & (moist < 0.45) & ~desert
        swamp = (~ocean) & (moist > 0.7) & (temp > 0.4) & (elev < 0.45)
        forest = (~ocean) & (moist > 0.5) & ~swamp & ~desert & ~savanna & ~cold & ~mountains
        biome[ocean] = Biome.OCEAN.value
        biome[beach] = Biome.BEACH.value
        biome[mountains] = Biome.MOUNTAINS.value
        biome[snow] = Biome.SNOW.value
        biome[tundra] = Biome.TUNDRA.value
        biome[desert] = Biome.DESERT.value
        biome[savanna] = Biome.SAVANNA.value
        biome[swamp] = Biome.SWAMP.value
        biome[forest] = Biome.FOREST.value
        return height, biome


def biome_color_lut() -> np.ndarray:
    max_id = max(b.value for b in Biome)
    lut = np.zeros((max_id + 1, 3), dtype=np.uint8)
    for b, rgb in BIOME_COLORS.items():
        lut[b.value] = rgb
    return lut
