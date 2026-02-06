# Система инвентаря с подбором предметов

Проект Unity с реализацией системы инвентаря, подбором предметов и UI на базе UI Toolkit.

## Требования

- Unity 6.0.2 или выше
- Input System пакет (уже включен в проект)
- UI Toolkit (встроен в Unity)

## Структура проекта

### Data Layer
- `Scripts/Data/ItemData.cs` - класс данных предмета
- `Scripts/Data/InventorySlot.cs` - класс слота инвентаря
- `Resources/ItemsData.json` - JSON файл с данными предметов

### Logic Layer
- `Scripts/Player/PlayerController.cs` - контроллер персонажа (WASD движение)
- `Scripts/Player/ThirdPersonCamera.cs` - камера от третьего лица
- `Scripts/Items/CollectableItem.cs` - компонент предмета на сцене
- `Scripts/Items/ItemSpawner.cs` - спавнер предметов из JSON
- `Scripts/Inventory/Inventory.cs` - основная логика инвентаря
- `Scripts/Inventory/ItemPickup.cs` - логика подбора предметов

### UI Layer
- `Resources/InventoryWindow.uxml` - разметка UI Toolkit окна инвентаря
- `Resources/InventoryWindow.uss` - стили для окна инвентаря
- `Scripts/UI/InventoryUI.cs` - контроллер UI инвентаря
- `Scripts/UI/InventorySlotUI.cs` - UI элемент слота инвентаря

## Настройка сцены

### 1. Создание персонажа

1. Создайте пустой GameObject и назовите его "Player"
2. Добавьте компонент `CharacterController`
3. Добавьте компонент `PlayerController`
4. Добавьте компонент `Inventory`
5. Добавьте компонент `ItemPickup`
6. Установите тег "Player" для GameObject
7. Создайте примитив Capsule как дочерний объект Player (для визуализации)

### 2. Настройка камеры

1. Выберите Main Camera в сцене
2. Добавьте компонент `ThirdPersonCamera`
3. В инспекторе установите Target на Player
4. Настройте параметры:
   - Distance: 5
   - Height: 2
   - Rotation Speed: 2

### 3. Создание предметов

1. Создайте пустой GameObject и назовите его "ItemSpawner"
2. Добавьте компонент `ItemSpawner`
3. Настройте параметры:
   - Item Count: 6
   - Spawn Radius: 10
   - Min Distance: 2

### 4. Настройка UI

1. Создайте пустой GameObject и назовите его "InventoryUI"
2. Добавьте компонент `UIDocument`
3. Добавьте компонент `InventoryUI`
4. В инспекторе InventoryUI установите ссылки на UXML и USS файлы (опционально, загрузятся автоматически из Resources)

### 5. Настройка Input System

1. Убедитесь, что файл `Other/InputSystem_Actions.inputactions` присутствует в проекте
2. В настройках проекта (Edit > Project Settings > Player) убедитесь, что Active Input Handling установлен в "Input System Package (New)" или "Both"
3. Input Actions создаются программно в коде, поэтому дополнительная настройка не требуется

## Управление

- **WASD** - движение персонажа
- **Мышь** - вращение камеры
- **E** - подобрать предмет (когда рядом)
- **Tab** - открыть/закрыть инвентарь
- **ЛКМ + перетаскивание** - перемещение предметов в инвентаре
- **ПКМ** - выбросить предмет из инвентаря

## Формат JSON данных предметов

Файл `Resources/ItemsData.json` содержит массив предметов со следующими полями:

```json
{
  "items": [
    {
      "id": 1,
      "name": "Red Cube",
      "color": {"r": 1, "g": 0, "b": 0, "a": 1},
      "stackable": true,
      "maxStack": 10
    }
  ]
}
```

## Особенности реализации

- Разделение на слои: Data, Logic, UI
- Использование UI Toolkit для интерфейса
- Drag & Drop между ячейками инвентаря
- Поддержка стаков для stackable предметов
- Подсказка при приближении к предмету
- Выброс предметов обратно в мир

## Расширение функциональности

Для добавления новых предметов:
1. Отредактируйте `Resources/ItemsData.json`
2. Добавьте новый объект в массив items

Для изменения размера инвентаря:
1. Откройте `Scripts/Inventory/Inventory.cs`
2. Измените значение `slotCount` в инспекторе или в коде
3. Обновите UXML файл, добавив дополнительные слоты

## Известные ограничения

- Иконки предметов отображаются как цветные квадраты (цвет из данных предмета)
- Подсказка подбора использует UI Toolkit (может требовать настройки Canvas)
- Drag & Drop работает только в пределах окна инвентаря
