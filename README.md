# MethodSpace

Проект подготовлен для запуска через Visual Studio.

## Что нужно

- Visual Studio 2022
- .NET Framework 4.8 Developer Pack

## Как запустить

1. Откройте файл `MethodSpace.sln`.
2. Дождитесь восстановления NuGet-пакетов, если Visual Studio это предложит.
3. Выберите проект `MethodSpace` как startup project.
4. Нажмите `F5`.

## Как работает база

- В `App.config` уже настроено подключение к SQL Server `MAKSIK\\SQLEXPRESS`, база `CollegeMethodService`.
- Если на другом компьютере этой базы нет, приложение автоматически запускается в локальном режиме и не падает.

## Демо-вход без базы

- Администратор: `admin@methodspace.local` / `admin123`
- Методист: `methodist@methodspace.local` / `method123`
- Преподаватель: `teacher@methodspace.local` / `teacher123`

## Примечание

- В архив включена папка `packages`, поэтому проект можно открыть даже без ручной настройки зависимостей.
