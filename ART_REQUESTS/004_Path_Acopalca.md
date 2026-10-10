# 004 — Path_Acopalca

WAITING_ART. Overlay de tierra pisada para los corredores de Sector_01.

Estilo BROTE: cartoon pintado cálido altoandino, perspectiva top-down / 3/4,
compatible con Ground existente sin cambiar su PPU, escala o Grid.
Entregar PNG RGBA con transparencia real alrededor del camino; sin fondo de suelo.
Piezas: rectas verticales/horizontales, curvas, bordes irregulares, terminaciones,
cruces T/+ y uniones. Sin cuadrícula marcada ni sombra rígida.
Dimensiones aproximadas: piezas de 256 × 256 px, ancho útil de sendero 1–1.5 unidades;
confirmar encaje con el Ground antes de aprobar importación.
Destino: Assets/_BROTE/Art/Environment/Paths/Path_Acopalca.png.
Uso Unity: sprites overlay en GeneratedPath, Ground sorting por encima del suelo,
sin collider; PathDefinition almacena nodos/ancho y Painter valida clearance.
No reemplazar Ground ni pintar hasta recibir sprites aprobados. Los trazados de
Sector01VisualLayout son propuestas antiguas y requieren resolver conflictos del norte.
