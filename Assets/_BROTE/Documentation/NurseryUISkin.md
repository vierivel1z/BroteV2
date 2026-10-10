# Nursery Artistic UI — Phases 1 + 2

El skin persistente está en Assets/_BROTE/Resources/UI/NurseryUISkin.asset.
El panel carga este asset automáticamente; no hay que modificar la escena.

## Arte conectado
Fuente de fase1: Assets/_BROTE/Art/UI/Nursery/NurseryUI_Atlas.png, Sprite Multiple
con recortes manuales existentes. Unity resolvió los ocho nombres exactos:
NurseryPanel_BG, Tab_Production_Active, Tab_Production_Inactive,
Tab_Upgrades_Active, Tab_Upgrades_Inactive, ProgressBar_Back, ProgressBar_Fill,
UpgradeCard_BG. Sin recorte automático, PNG derivados ni cambios de importador.
Las referencias se serializaron mediante AssetDatabase; no se dedujeron fileIDs.

## 9-slice y área útil
Los sprites originales tienen border=(0,0,0,0). Se conservan así.
UI Toolkit usa overrides del skin, en píxeles del sprite:
- panelSliceBorders=(180,100,180,100), escala0.3.
- cardSliceBorders=(90,70,90,70), escala0.5.
Orden: izquierda, abajo, derecha, arriba.
ApplyFrame prioriza sprite.border si posteriormente se configura.
Son valores conservadores iniciales; inspeccionar hojas, madera y textil en Play.
No se afirma aprobación estética de las franjas de borde con pruebas headless.
Padding panel58px lateral/38 vertical, compacto54/32; tarjetas48/38.
Nursery-shell contiene nursery-safe-area; todo el contenido vive en ella.

## Estructura
Sidebar derecho:390–430px con ventana>=1000px, margen24px; estrecho16px
y ancho adaptable. Alto86%,88% en ventanas muy bajas; centrado vertical.
Sin overlay oscuro sobre el mundo ni cambios al escalado del HUD.
Encabezado GESTIONAR VIVERO → recursos Minka/plántulas → tabs → contenido
→ cierre discreto. Producción inicial con scroll de seguridad para ventanas bajas.
Mejoras: scroll vertical, cinco instancias de NurseryUpgradeCard.uxml,
cada una con icono reservado, título/nivel/actual→siguiente/coste/compra.

Barra: production-progress-track → production-progress-interior
→ production-progress-clip → production-progress-fill.
Insets en píxeles UI: izquierda14,arriba8,derecha14,abajo8.
Clip overflow:hidden, left=0, width=progreso%; el fill conserva el ancho total
del interior y se recalcula solo cuando cambia la geometría. No se mueve
horizontalmente ni comprime su patrón con el progreso.

## Fase2 integrada — botones e iconos oficiales
Trece referencias exactas ya conectadas desde Icon_SoilPreparation.png (atlas
Sprite Multiple de fase2). Slots del Inspector de NurseryUISkin.asset:
- NurseryButton_Primary_Normal → primaryNormal
- NurseryButton_Primary_Pressed → primaryPressed
- NurseryButton_Primary_Disabled → primaryDisabled
- NurseryButton_Close → closeButtonBackground
- BuyButton_Available → buyAvailable
- BuyButton_Disabled → buyDisabled
- BuyButton_Max → buyMax
- Icon_Minka → iconMinka
- Icon_Seedling → iconSeedling (recurso del encabezado)
- Icon_Irrigation → iconIrrigation
- Icon_Capacity → iconCapacity
- Icon_SoilPreparation → iconSoilPreparation (Click Power provisional y Suelo)
- Icon_Expansion → iconExpansion

Pressed se aplica solo con puntero izquierdo presionado; Up/Cancel/Leave/
CaptureOut/cierre/deshabilitado/cambio a Mejoras limpian el estado.
Normal regresa al soltar. Producción conserva evento,feedback y lógica.
Compra sigue reglas existentes: available con Minka suficiente, disabled sin
Minka o durante pausa, max exclusivamente cuando no hay siguiente nivel.
El texto dinámico sigue en UI. Cierre32x32 en esquina superior derecha
INTERIOR de nursery-safe-area, sin botón textual adicional; tooltip y ESC.
Recursos compactos:icono28px+cantidad; capacidad actual/máxima.
Tasa Minka conservada en tooltip. Iconos de tarjeta36px.
Iconos y cierre usan background-size:contain, centrados y sin repetición.
El icono Click Power usa SoilPreparation por petición explícita.

Para reconectar fase1: menú BROTE/UI/Connect Nursery Phase 1 Atlas.
Busca nombres exactos y omite solamente asignaciones ausentes/duplicadas.

## Validación
Reconectar fase2: BROTE/UI/Connect Nursery Phase 2 Buttons and Icons.
Busca los nombres exactos en todos los PNG de Art/UI/Nursery; skips se reportan,
sin tocar PNG ni recortes/importadores. Evidencia: .brote-work/nursery-ui-phase-2/.
Consultar STATUS.md para resultados reales de compilación/tests.
PLAY_REQUIRED:1920x1080,1280x720,16:10,4:3,Free Aspect ancho/bajo,
clicks físicos,legibilidad,9-slice,alineación del track/fill y escala artística.
