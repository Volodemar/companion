# ARCHITECTURE_GRAPH

> Сгенерировано: 2026-05-30T15:39:09
> Скрипт: script_graph.py v1.0.0
> Проект: `/home/vshan/Work/Companion/companion-git/Companion`

Файл сгенерирован автоматически. **Не редактировать руками.**
Перегенерация: `python <project>/Assets/Plugins/ClaudeTools/script_graph.py <project>`

Если дата выше старее последнего коммита кода — граф устарел, попроси перегенерировать.

## Сводка

- Классов: **8**
  - MonoBehaviour: 6
  - Plain: 2
- Интерфейсов: **0**
- Структур: **0**
- Сцен (.unity): **3**
- Префабов с пользовательскими скриптами: **1** из 1
- Полей `[Inject]`: **3**
- Синглтонов (`static Instance`): **1**
- Шейдеров: **0** (0 .shader, 0 .hlsl, 0 .compute, 0 .shadergraph)

## Структура проекта

Подсчёт файлов по top-level папкам `Assets/`. Колонки: cs, shader+hlsl, prefab, unity, asset, mat, всего.

- `Assets/!Companion/` — cs:8 sh:0 prefab:1 unity:3 asset:3 mat:0 всего:15
- `Assets/DefaultVolumeProfile.asset/` — cs:0 sh:0 prefab:0 unity:0 asset:1 mat:0 всего:1
- `Assets/Plugins/` — cs:256 sh:0 prefab:0 unity:0 asset:0 mat:0 всего:256
  - `Assets/Plugins/Zenject/` — cs:256 sh:0 prefab:0 unity:0 asset:0 mat:0 всего:256
- `Assets/UniversalRenderPipelineGlobalSettings.asset/` — cs:0 sh:0 prefab:0 unity:0 asset:1 mat:0 всего:1

## Центральные классы (топ-25)

Метрика: сумма входящих ссылок (`[Inject]` + `.Instance` + статический доступ + сцены + префабы).

- **UIManager** — 3 (`[Inject]`×2, сцен×1)
- **CoroutineManager** — 1 (`[Inject]`×1)
- **MainSceneInstaller** — 1 (сцен×1)
- **UIWindowMain** — 1 (префабов×1)

## Иерархия наследования

Только классы. Интерфейсы и базы помечены отдельно.

- *MonoBehaviour* (внешний)
  - BaseGameObject
    - UIManager
    - UIPopup
    - UIWindow
      - UIWindowMain
  - CoroutineManager

- *MonoInstaller* (внешний)
  - MainSceneInstaller

- AppController [IInitializable]

## Менеджеры, контроллеры, системы, сервисы

Авто-выборка по суффиксам имени (`*Manager`, `*Controller`, `*System`, `*Service`). Часто отражают «ролевую» архитектуру проекта независимо от количества входящих ссылок.

### *Manager (2)
- **CoroutineManager** (`Assets/!Companion/Scripts/Core/CoroutineManager.cs`) — inject×1
- **UIManager** (`Assets/!Companion/Scripts/UI/UIManager.cs`) — inject×2, сцен×1

### *Controller (1)
- **AppController** (`Assets/!Companion/Scripts/Core/AppController.cs`) — *без входящих ссылок*

## Карта DI (`[Inject]`)

Тип → классы которые его инжектят. Покрывает Zenject, VContainer, Reflex (общий атрибут).

- **UIManager** ← 2: AppController, BaseGameObject
- **CoroutineManager** ← 1: BaseGameObject

## Синглтоны и доступ через `.Instance`

### Объявленные `static Instance`:
- **UIManager** ← 0 потребителей: 

## Скрипты в сценах (.unity)

- `Assets/!Companion/Scenes/MainScene.unity` (3): MainSceneInstaller, SceneContext, UIManager

## Скрипты в префабах (.prefab)

Только префабы с привязанными пользовательскими скриптами.

- `Assets/!Companion/Prefabs/UI/UIWindowMain.prefab`: UIWindowMain

## Возможный мёртвый код

Классы имя которых не встречается ни в одном другом файле проекта.
Не учитывает: рефлексию, AssetBundle/Addressables-загрузку по строке, [MenuItem]-точки входа.
Editor/Tests папки исключены.

Не найдено.
