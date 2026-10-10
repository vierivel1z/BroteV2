# Restoration Props integrados en Sector_01
26 props iniciales Deg_* en once agrupaciones; cero colliders y cero YSort2D nuevos. Default, orden -3 para iniciales y -2 para los diez Pending, detras de WorldObjects. PNG y meta de Restoration_Props intactos; PPU 256, nombres, pivotes e IDs conservados.
## Sprites iniciales
- Deg_Grass_01: 3
- Deg_Rock_01: 2
- Deg_Branch_01: 2
- Deg_Shrub_01: 2
- Deg_Rock_02: 3
- Deg_Grass_02: 2
- Deg_Grass_03: 4
- Deg_Branch_02: 2
- Deg_Rock_03: 2
- Deg_Patch_01: 2
- Deg_Shrub_02: 1
- Deg_Patch_02: 1

## Umbrales

| Objeto existente | Sprite | Escala local | Alto (u) |
|---|---|---:|---:|
| Pending_20_Shoots_A | Rec_Sprout_01 | 1.052 | 0.30 |
| Pending_20_Shoots_B | Rec_Sprout_02 | 0.883 | 0.40 |
| Pending_20_Shoots_C | Rec_Grass_01 | 1.146 | 0.47 |
| Pending_40_Shrub_A | Rec_Shrub_01 | 1.234 | 0.80 |
| Pending_40_Shrub_B | Rec_Grass_02 | 1.067 | 0.70 |
| Pending_40_Shrub_C | Rec_Plant_02 | 1.091 | 0.55 |
| Pending_50_Grass_A | Res_Grass_01 | 1.411 | 0.70 |
| Pending_50_Flowers_A | Res_Flowers_01 | 1.126 | 0.55 |
| Pending_50_Grass_B | Res_Shrub_02 | 1.440 | 0.90 |
| Pending_50_Flowers_B | Res_Flowers_02 | 1.138 | 0.60 |

Se conservaron los diez GameObjects, sus posiciones y las referencias de los tres grupos de SectorVisualState. SpriteRenderer reactivados. Visibilidad acumulativa prevista: 26 props al 0%, 29 al 20%, 32 al 40%, 36 al 50%+. No se cambia ni sustituye la logica de restauracion; los props secos permanecen mientras aparecen los verdes.

## Escala y composicion
Se normalizo la escala por categoria, sin alterar PPU ni escalas de sistemas existentes. Variacion de escala inicial de -8% a +8% respecto a esa base; Flip X selectivo y rotacion cero. Piedras alrededor de 0,44 unidades en su dimension mayor, brotes de 0,30/0,40 de alto, pastos de 0,47-0,74, arbustos de 0,66-0,90 y flores de 0,55/0,60. Todos se mantienen por debajo de Suyay (~1,41 de alto). No se considero necesario YSort2D para estos arbustos pequenos decorativos.

Se comprobaron bounds de los sprites con sus pivotes y flips: interior del perimetro, acceso y bounds del vivero, doce zonas limpias y ancho reservado de las curvas de Sector01VisualLayout. Las agrupaciones iniciales no pisan los bounds de los props futuros; permiten activacion acumulativa sin apilarlos. Los veintiseis elementos estan bajo Sector_01/DegradedProps. No se generaron Tile assets adicionales.

## Validacion y revision en Unity
Se preservaron 488 archivos existentes y todos los documentos de escena ajenos a Pending/Transform de Sector_01. Ground, Grid, restauracion, spots, vivero, jugador, camara y limites permanecen intactos. Referencias de sprite e IDs locales comprobados; cero colliders/YSort agregados. La imagen states-composition.png es una composicion diagnostica que muestra solo el cambio de props: no es Game View, no simula los cambios del terreno ni instancia quenuales.

Play no ejecutado en esta sesion. Abrir Acopalca desde disco y comprobar el estado inicial, luego plantar para observar 20/40/50%; verificar escala, acceso al vivero y lectura de spots. No hay sprites que asignar manualmente. Todavia falta el arte del camino; no se inicio el camino ni particulas. Los bordes no seamless del Ground siguen existiendo y esta decoracion solo los disimula parcialmente.
