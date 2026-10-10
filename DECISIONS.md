# BROTE — Decisiones de diseño vigentes

Última actualización: 2026-10-04

## Proyecto

Ruta correcta:

D:/UnityGames/Brote/Brote

No trabajar sobre copias anidadas antiguas sin autorización.

---

## Dirección del juego

BROTE es un juego 2D de restauración ecológica ambientado en:

Acopalca, Huancayo, Junín.

Contexto ambiental:

subcuenca del río Shullcas.

Especie principal:

queñual / Polylepis.

---

## Dirección visual

Estilo:

- 2D cartoon pintado;
- altoandino;
- cálido;
- ligeramente estilizado;
- no pixel art duro.

La vista jugable se mantiene top-down / 3/4.

El paisaje lejano sirve para dar profundidad y contexto de Acopalca.

---

## Suyay

NO escalar directamente a Suyay para corregir composición de cámara.

Mantener diseño oficial actual.

Movimiento:

WASD / flechas.

Interacción:

E.

---

## Cámara

Orthographic Size actual objetivo:

4.3

framingOffset:

(0, 1.5)

No implementar parallax actualmente.

---

## Paisaje

Asset:

Acopalca_DistantLandscape

Transform confirmado:

Position = (0, 12.4, 0)

Scale = (2.4, 2.4, 1)

Sorting Layer:

Midground

Order:

0

El paisaje representa una zona NO JUGABLE.

---

## ScenicBoundary

ScenicBoundary está activo.

No permitir que Suyay camine dentro del paisaje lejano.

Los 3 PlantingSpots que interferían ya fueron reubicados.

PlantingSpot_08 puede cruzar ligeramente el límite con su trigger.

NO moverlo salvo que una prueba en Play demuestre que genera un
problema real.

---

## Ground

GroundTilemap:

40 x 30

No cambiar:

- PPU;
- Grid Cell Size;
- tilesets;
- escala.

La selección de tiles ya fue mejorada para reducir efecto de cuadrícula.

---

## Restauración

10 PlantingSpots.

Cada queñual desarrollado:

+10 restauración.

Máximo:

100%.

Etapas actuales:

Seedling
Young
Developed

Tiempos actuales:

Seedling -> Young = 5 s
Young -> Developed = 5 s

Conservar por ahora.

---

## Feedback deseado

Inspiración:

Forager.

Pero BROTE debe utilizar feedback:

- más natural;
- menos explosivo;
- sin screen shake fuerte;
- partículas de hojas;
- polvo;
- pop de escala;
- pequeño pulso natural.

Evitar spam cuando muchos árboles crezcan simultáneamente.

---

## UI

Indicador principal:

Salud del Valle.

No crear barras individuales sobre cada árbol.

Evitar exceso de números y popups.

---

## Prioridad actual

Primero terminar y pulir el loop existente.

NO añadir todavía:

- NPC complejos;
- crafting;
- sistema grande de upgrades;
- mapas adicionales;
- economía avanzada.

Una vez pulido el core loop se evaluarán esas mecánicas.

## Ola 4 autorizada — 2026-10-05

La solicitud del usuario autoriza exclusivamente la economía Minka, vivero clicker,
cuatro mejoras de vivero/suelo y expansión con los diez spots existentes.
Esta autorización actualiza las restricciones anteriores sobre economía, sin abrir
NPC, crafting, mapas nuevos,30spots, offline ni frameworks grandes.

- Minka: +1 por primer Developed;0.01/s por Developed, fracciones internas, enteros gastables.
- Producción10%/click, overflow conservado; capacidad llena pausa producción sin borrar progreso.
- Click10/15/20/25 costes1/2/4; riego0/2/5/10%/s costes2/4/7;
  capacidad5/8/12/20 costes2/4/7; suelo3/2/1 acciones costes2/5;
  expansión Initial/Expansion01/Expansion02 costes4/8.
- Grupos espaciales: Initial01/06/09/10 (oeste junto al vivero), Expansion01 04/05/08
  (centro/norte), Expansion02 02/03/07 (este/sur).01/06 PREPARED,09/10 AVAILABLE;
  seis restantes LOCKED. Layout JSON conserva posiciones reales; no se mueve ningún spot.
- El mapa abierto no ofrece un choke natural: bloqueo lógico y tinte, ningún gate físico.
  Sensación de mapa pequeño PLAY_REQUIRED / DESIGN_REVIEW.
- Objetivos fijos producir/plantar/desarrollar/preparar/mejorar/Expansion01, recompensas
  1/1/2/1/1/2 según las definiciones251–320 originalmente anexadas a STATUS.md,
  ahora en TASKS.md. Un solo indicador reutiliza la guía del tutorial.
- Save v2 extiende el manager existente. V1 válido migra sin premios retroactivos inmediatos,
  conserva árboles/inventario/salud/tutorial, abre las tres zonas para no encerrar
  progreso antiguo y deja vacíos AVAILABLE. Si todos los spots legacy están plantados,
  la secuencia introductoria se considera terminada sin premiarla de nuevo. La expansión legacy se registra como observada para que el objetivo6 no sea inalcanzable; solo se reclama al completar los pasos restantes.
  Copia local .v1.backup antes de migrar; sin offline, perfiles ni slots.
- Restauración sigue +10 y máximo100, crecimiento5+5s. Nueva partida suprime
  escrituras de la escena anterior durante recarga para no recrear su save eliminado.
