# ARCHITECTURE_GRAPH

> Сгенерировано: 2026-05-30T16:42:51
> Скрипт: script_graph.py v1.0.0
> Проект: `/home/vshan/Work/Companion/companion-git/Companion`

Файл сгенерирован автоматически. **Не редактировать руками.**
Перегенерация: `python <project>/Assets/Plugins/ClaudeTools/script_graph.py <project>`

Если дата выше старее последнего коммита кода — граф устарел, попроси перегенерировать.

## Сводка

- Классов: **20**
  - MonoBehaviour: 13
  - Plain: 7
- Интерфейсов: **0**
- Структур: **0**
- Сцен (.unity): **3**
- Префабов с пользовательскими скриптами: **7** из 7
- Полей `[Inject]`: **6**
- Синглтонов (`static Instance`): **1**
- Шейдеров: **0** (0 .shader, 0 .hlsl, 0 .compute, 0 .shadergraph)

## Структура проекта

Подсчёт файлов по top-level папкам `Assets/`. Колонки: cs, shader+hlsl, prefab, unity, asset, mat, всего.

- `Assets/!Companion/` — cs:19 sh:0 prefab:7 unity:3 asset:3 mat:0 всего:32
- `Assets/DefaultVolumeProfile.asset/` — cs:0 sh:0 prefab:0 unity:0 asset:1 mat:0 всего:1
- `Assets/Plugins/` — cs:256 sh:0 prefab:0 unity:0 asset:0 mat:0 всего:256
  - `Assets/Plugins/Zenject/` — cs:256 sh:0 prefab:0 unity:0 asset:0 mat:0 всего:256
- `Assets/UniversalRenderPipelineGlobalSettings.asset/` — cs:0 sh:0 prefab:0 unity:0 asset:1 mat:0 всего:1

## Центральные классы (топ-25)

Метрика: сумма входящих ссылок (`[Inject]` + `.Instance` + статический доступ + сцены + префабы).

- **EventManager** — 4 (static×4)
- **UIManager** — 3 (`[Inject]`×2, сцен×1)
- **TimersStorage** — 2 (`[Inject]`×2)
- **TimerService** — 1 (`[Inject]`×1)
- **CoroutineManager** — 1 (`[Inject]`×1)
- **MainSceneInstaller** — 1 (сцен×1)
- **UIWindowTimer** — 1 (префабов×1)
- **UIWindowMain** — 1 (префабов×1)
- **UIPopupTimerDone** — 1 (префабов×1)
- **UIPopupTimerCreate** — 1 (префабов×1)
- **TimerIndicatorBar** — 1 (префабов×1)
- **TimerIndicator** — 1 (префабов×1)
- **TimerButton** — 1 (префабов×1)

## Иерархия наследования

Только классы. Интерфейсы и базы помечены отдельно.

- *MonoBehaviour* (внешний)
  - AudioManager
  - BaseGameObject
    - UIManager
    - UIPopup
      - UIPopupTimerCreate
      - UIPopupTimerDone
    - UIWindow
      - UIWindowMain
      - UIWindowTimer
  - CoroutineManager
  - TimerButton
  - TimerIndicator
  - TimerIndicatorBar

- *MonoInstaller* (внешний)
  - MainSceneInstaller

- AppController [IInitializable]

- EventManager

- SaveData

- TimerData

- TimerService

- TimersStorage

## Менеджеры, контроллеры, системы, сервисы

Авто-выборка по суффиксам имени (`*Manager`, `*Controller`, `*System`, `*Service`). Часто отражают «ролевую» архитектуру проекта независимо от количества входящих ссылок.

### *Manager (4)
- **AudioManager** (`Assets/!Companion/Scripts/Core/AudioManager.cs`) — *без входящих ссылок*
- **CoroutineManager** (`Assets/!Companion/Scripts/Core/CoroutineManager.cs`) — inject×1
- **EventManager** (`Assets/!Companion/Scripts/Core/EventManager.cs`) — static×4
- **UIManager** (`Assets/!Companion/Scripts/UI/UIManager.cs`) — inject×2, сцен×1

### *Controller (1)
- **AppController** (`Assets/!Companion/Scripts/Core/AppController.cs`) — *без входящих ссылок*

### *Service (1)
- **TimerService** (`Assets/!Companion/Scripts/Core/TimerService.cs`) — inject×1

## Статические хабы

Классы к которым обращаются как `Class.Member` (не `.Instance`, не `[Inject]`). Типично — `EventManager`, статические утилиты, контейнеры данных, message-bus.

- **EventManager** ← 4: AudioManager, TimerIndicatorBar, TimerService, UIPopupTimerDone

## Карта DI (`[Inject]`)

Тип → классы которые его инжектят. Покрывает Zenject, VContainer, Reflex (общий атрибут).

- **TimersStorage** ← 2: UIPopupTimerCreate, UIWindowTimer
- **UIManager** ← 2: AppController, BaseGameObject
- **CoroutineManager** ← 1: BaseGameObject
- **TimerService** ← 1: UIWindowTimer

## Синглтоны и доступ через `.Instance`

### Объявленные `static Instance`:
- **UIManager** ← 0 потребителей: 

## Скрипты в сценах (.unity)

- `Assets/!Companion/Scenes/MainScene.unity` (3): MainSceneInstaller, SceneContext, UIManager

## Скрипты в префабах (.prefab)

Только префабы с привязанными пользовательскими скриптами.

- `Assets/!Companion/Prefabs/UI/TimerButton.prefab`: TimerButton
- `Assets/!Companion/Prefabs/UI/TimerIndicator.prefab`: TimerIndicator
- `Assets/!Companion/Prefabs/UI/TimerIndicatorBar.prefab`: TimerIndicatorBar
- `Assets/!Companion/Prefabs/UI/UIPopupTimerCreate.prefab`: UIPopupTimerCreate
- `Assets/!Companion/Prefabs/UI/UIPopupTimerDone.prefab`: UIPopupTimerDone
- `Assets/!Companion/Prefabs/UI/UIWindowMain.prefab`: UIWindowMain
- `Assets/!Companion/Prefabs/UI/UIWindowTimer.prefab`: UIWindowTimer

## Возможный мёртвый код

Классы имя которых не встречается ни в одном другом файле проекта.
Не учитывает: рефлексию, AssetBundle/Addressables-загрузку по строке, [MenuItem]-точки входа.
Editor/Tests папки исключены.

- SaveData — `Assets/!Companion/Scripts/Core/TimersStorage.cs`
